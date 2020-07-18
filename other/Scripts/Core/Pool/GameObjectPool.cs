using Leyoutech.Core.Timer;
using Leyoutech.Core.Util;
using Leyoutech.Utility;
using System;
using System.Collections.Generic;
using UnityEngine;
using SystemObject = System.Object;
using UnityObject = UnityEngine.Object;

namespace Leyoutech.Core.Pool
{
    /// <summary>
    /// 用于创建缓存池的模板的类型
    /// </summary>
    public enum PoolTemplateType
    {
        Prefab,//使用Prefab做为缓存池模型
        PrefabInstance,//使用Prefab实例化后的对象做为模板
        RuntimeInstance,//运行时创建的对象做模板
    }
    /// <summary>
    ///游戏中GameObject或者Prefab的缓存池实现
    /// </summary>
    public class GameObjectPool
    {
        private const string LOG_TAG = "GameObjectPool";

        private SpawnPool m_SpawnPool = null;//缓存池所属的分组
        private string m_AssetPath = null;//资源的唯一标识，一般使用资源路径做唯一标识
        private PoolTemplateType m_TemplateType = PoolTemplateType.Prefab;//缓存池类型
        private GameObject m_InstanceOrPrefabTemplate = null;//缓存池模板，可以是具体的GameObject的实例，也可以是Prefab
        private ChangableQueue<GameObject> m_UnusedItemQueue = new ChangableQueue<GameObject>();//未被使用的对象

        private List<WeakReference<GameObject>> m_UsedItemList = new List<WeakReference<GameObject>>();//对正在使用的对象的引用

        public OnPoolComplete completeCallback = null;//缓存池初始化完成后回调

        public bool IsAutoClean { get; set; }=false;//是否自动清理

        public int PreloadTotalAmount { get; set; }=0;//预加载的总数量
        public int PreloadOnceAmount { get; set; }=1;//一次预加载的数量

        public bool IsCull { get; set; } = false;//是否对缓存池进行裁剪
        public int CullOnceAmount { get; set; } = 0;//一次裁剪的数量
        public int CullDelayTime { get; set; } = 30;//裁剪周期

        public int LimitMaxAmount { get; set; } = 0;//缓存池存储的最大数量，如果超出数量则获取失败
        public int LimitMinAmount { get; set; } = 0;//缓存池中存留的最小的数量，如果数量小于等于此值将不会再裁剪

        private float m_PreCullTime = 0;//上一次裁剪时间记录
        private float m_CurTime = 0;//当前时间记录

        private TimerTaskInfo m_PreloadTimerTask = null;//使用定时器来对缓存池进行预加载

        internal GameObjectPool()
        {
        }
        /// <summary>
        /// 初始化缓存池
        /// </summary>
        /// <param name="pool">所有的分组</param>
        /// <param name="aPath">唯一标识，一般以资源路径为值</param>
        /// <param name="templateGObj">模板</param>
        /// <param name="templateType">模板类型</param>
        internal void InitPool(SpawnPool pool, string aPath, GameObject templateGObj, PoolTemplateType templateType)
        {
            m_SpawnPool = pool;
            m_AssetPath = aPath;

            m_InstanceOrPrefabTemplate = templateGObj;
            this.m_TemplateType = templateType;

            if(templateType!= PoolTemplateType.Prefab)
            {
                m_InstanceOrPrefabTemplate.SetActive(false);
                m_InstanceOrPrefabTemplate.transform.SetParent(pool.CachedTransform, false);
            }

            m_PreloadTimerTask = TimerManager.GetInstance().AddIntervalTimer(0.05f, OnPreloadTimerUpdate);
        }

        #region Preload
        /// <summary>
        /// 使用Timer的Tick进行预加载
        /// </summary>
        /// <param name="obj"></param>
        private void OnPreloadTimerUpdate(SystemObject obj)
        {
            int curAmount = m_UsedItemList.Count + m_UnusedItemQueue.Count;
            if (curAmount >= PreloadTotalAmount)
            {
                OnPoolComplete();
            }
            else
            {
                int poa = PreloadOnceAmount;
                if (poa == 0)
                {
                    poa = PreloadTotalAmount;
                }
                else
                {
                    poa = Mathf.Min(PreloadOnceAmount, PreloadTotalAmount - curAmount);
                }
                for (int i = 0; i < poa; ++i)
                {
                    GameObject instance = CreateNewItem();
                    instance.transform.SetParent(m_SpawnPool.CachedTransform, false);
                    instance.SetActive(false);
                    m_UnusedItemQueue.Enqueue(instance);
                }
            }
        }
        /// <summary>
        /// 使用定时器进行预加载结束
        /// </summary>
        private void OnPoolComplete()
        {
            completeCallback?.Invoke(m_SpawnPool.PoolName, m_AssetPath);
            completeCallback = null;

            if(m_PreloadTimerTask!=null)
            {
                TimerManager.GetInstance().RemoveTimer(m_PreloadTimerTask);
                m_PreloadTimerTask = null;
            }
        }

        #endregion

        #region GetItem
        /// <summary>
        /// 从缓存池中得到一个GameObject对象
        /// </summary>
        /// <param name="isAutoSetActive">是否激获取到的GameObject,默认为true</param>
        /// <returns></returns>
        public GameObject GetPoolItem(bool isAutoSetActive = true)
        {
            if (LimitMaxAmount != 0 && m_UsedItemList.Count > LimitMaxAmount)
            {
                DebugUtility.LogWarning(LOG_TAG, "GameObjectPool::GetItem->Large than Max Amount");
                return null;
            }

            GameObject item = null;
            if (m_UnusedItemQueue.Count > 0)
            {
                item = m_UnusedItemQueue.Dequeue();
            }
            else
            {
                item = CreateNewItem();
            }

            if (item != null)
            {
                GameObjectPoolItem poolItem = item.GetComponent<GameObjectPoolItem>();
                if (poolItem != null)
                {
                    poolItem.DoSpawned();
                }
            }

            if (isAutoSetActive)
            {
                item.SetActive(true);
            }

            m_UsedItemList.Add(new WeakReference<GameObject>(item));
            return item;
        }

        /// <summary>
        /// 从缓存池中得到指定类型的组件
        /// </summary>
        /// <typeparam name="T">继承于MonoBehaviour的组件</typeparam>
        /// <param name="isAutoActive">是否激获取到的GameObject,默认为true</param>
        /// <returns></returns>
        public T GetComponentItem<T>(bool isAutoActive = true,bool isAddIfNotFind = false) where T:MonoBehaviour
        {
            if(m_InstanceOrPrefabTemplate.GetComponent<T> ()==null && !isAddIfNotFind)
            {
                return null;
            }

            GameObject gObj = GetPoolItem(isAutoActive);
            T component = null;
            if(gObj!=null)
            {
                component = gObj.GetComponent<T>();
                if(component == null)
                {
                    component = gObj.AddComponent<T>();
                    GameObjectPoolItem poolItem = component as GameObjectPoolItem;
                    if(poolItem!=null)
                    {
                        poolItem.SpawnName = m_SpawnPool.PoolName;
                        poolItem.AssetPath = m_AssetPath;
                        poolItem.DoSpawned();
                    }
                }
            }

            return component;
        }
        /// <summary>
        /// 根据模板创建一个新对象
        /// </summary>
        /// <returns></returns>
        private GameObject CreateNewItem()
        {
            GameObject item = null;
            if(m_TemplateType == PoolTemplateType.RuntimeInstance)
            {
                item = GameObject.Instantiate(m_InstanceOrPrefabTemplate);
            }
            else
            {
                item = (GameObject)Loader.AssetManager.GetInstance().InstantiateAsset(m_AssetPath, m_InstanceOrPrefabTemplate);
            }

            if (item != null)
            {
                GameObjectPoolItem poolItem = item.GetComponent<GameObjectPoolItem>();
                if (poolItem != null)
                {
                    poolItem.AssetPath = m_AssetPath;
                    poolItem.SpawnName = m_SpawnPool.PoolName;
                }
            }
            return item;
        }
        #endregion

        #region Release Item
        /// <summary>
        /// 回收GameObject，如果此GameObject不带有GameObjectPoolItem组件，则无法回收到池中，将会直接删除
        /// </summary>
        /// <param name="item"></param>
        public void ReleasePoolItem(GameObject item)
        {
            if(item == null)
            {
                DebugUtility.LogError(LOG_TAG, "GameObjectPool::ReleaseItem->Item is Null");
                return;
            }

            GameObjectPoolItem pItem = item.GetComponent<GameObjectPoolItem>();
            if(pItem!=null)
            {
                pItem.DoDespawned();
            }

            item.transform.SetParent(m_SpawnPool.CachedTransform, false);
            item.SetActive(false);
            m_UnusedItemQueue.Enqueue(item);

            for (int i = m_UsedItemList.Count - 1; i >= 0; i--)
            {
                if(m_UsedItemList[i].TryGetTarget(out GameObject target))
                {
                    if(!UnityObjectExtension.IsNull(target))
                    {
                        if(target!=item)
                        {
                            continue;
                        }else
                        {
                            m_UsedItemList.RemoveAt(i);
                            break;
                        }
                    }
                }

                m_UsedItemList.RemoveAt(i);
            }
        }

		public void RemoveItemFromUnusedList(GameObject item)
		{
			if (m_UnusedItemQueue.Contains(item))
				m_UnusedItemQueue.Remove(item);
		}
        #endregion

        /// <summary>
        /// 裁剪缓存池
        /// </summary>
        /// <param name="deltaTime"></param>
        internal void CullPool(float deltaTime)
        {
            for (int i = m_UsedItemList.Count - 1; i >= 0; i--)
            {
                if (m_UsedItemList[i].TryGetTarget(out GameObject target))
                {
                    if (!UnityObjectExtension.IsNull(target))
                    {
                        continue;
                    }
                }
                m_UsedItemList.RemoveAt(i);
            }

            if (!IsCull)
            {
                return;
            }

            m_CurTime += deltaTime;
            if(m_CurTime - m_PreCullTime < CullDelayTime)
            {
                return;
            }

            int cullAmout = 0;
            if (m_UsedItemList.Count + m_UnusedItemQueue.Count <= LimitMinAmount)
            {
                cullAmout = 0;
            }
            else
            {
                cullAmout = m_UsedItemList.Count + m_UnusedItemQueue.Count - LimitMinAmount;
                if (cullAmout > m_UnusedItemQueue.Count)
                {
                    cullAmout = m_UnusedItemQueue.Count;
                }
            }

            if (CullOnceAmount > 0 && CullOnceAmount < cullAmout)
            {
                cullAmout = CullOnceAmount;
            }

            for (int i = 0; i < cullAmout && m_UnusedItemQueue.Count>0; i++)
            {
                UnityObject.Destroy(m_UnusedItemQueue.Dequeue());
            }

            m_PreCullTime = m_CurTime;
        }
        /// <summary>
        /// 销毁缓存池
        /// </summary>
        internal void DestroyPool()
        {
            completeCallback = null;
            if (m_PreloadTimerTask != null)
            {
                TimerManager.GetInstance().RemoveTimer(m_PreloadTimerTask);
                m_PreloadTimerTask = null;
            }

            m_UsedItemList.Clear();

            for (int i = m_UnusedItemQueue.Count - 1; i >= 0; i--)
            {
                UnityObject.Destroy(m_UnusedItemQueue.Dequeue());
            }
            m_UnusedItemQueue.Clear();

            if(m_TemplateType == PoolTemplateType.PrefabInstance || m_TemplateType == PoolTemplateType.RuntimeInstance)
            {
                UnityObject.Destroy(m_InstanceOrPrefabTemplate);
            }

            m_AssetPath = null;
            m_SpawnPool = null;
            m_InstanceOrPrefabTemplate = null;
            IsAutoClean = false;
        }
    }
}
