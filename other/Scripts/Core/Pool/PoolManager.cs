using Leyoutech.Core.Loader;
using Leyoutech.Core.Timer;
using Leyoutech.Core.Util;
using System.Collections.Generic;
using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace Leyoutech.Core.Pool
{
    public delegate void OnPoolComplete(string spawnName, string assetPath);

    /// <summary>
    /// 对于需要Pool异步加载的资源，可以通过PoolData指定对应池的属性，
    /// 等到资源加载完成将会使用指定的属性设置缓存池
    /// </summary>
    public class PoolData
    {
        public string SpawnName { get; set; }
        public string AssetPath { get; set; }
        public bool IsAutoClean { get; set; } = true;

        public int PreloadTotalAmount { get; set; } = 0;
        public int PreloadOnceAmount { get; set; } = 1;
        public OnPoolComplete CompleteCallback { get; set; } = null;

        public bool IsCull { get; set; } = false;
        public int CullOnceAmount { get; set; } = 0;
        public int CullDelayTime { get; set; } = 30;

        public int LimitMaxAmount { get; set; } = 0;
        public int LimitMinAmount { get; set; } = 0;

        internal AssetLoaderHandle LoaderHandle { get; set; } = null;
    }
    /// <summary>
    /// 缓存池管理器
    /// </summary>
    public class PoolManager : Util.Singleton<PoolManager>
    {
        private Transform m_CachedTransform = null;
        private Dictionary<string, SpawnPool> m_SpawnDic = new Dictionary<string, SpawnPool>();

        private float m_CullTimeInterval = 60f;
        private TimerTaskInfo m_CullTimerTask = null;

        private List<PoolData> m_PoolDatas = new List<PoolData>();

        protected override void DoInit()
        {
            m_CachedTransform = DontDestroyHandler.CreateTransform("PoolManager");
            m_CullTimerTask = TimerManager.GetInstance().AddIntervalTimer(m_CullTimeInterval, OnCullTimerUpdate);
        }
        
        /// <summary>
        /// 判断是否存在指定的分组
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool HasSpawnPool(string name)=> m_SpawnDic.ContainsKey(name);

        /// <summary>
        ///获取指定的分组，如果不存在可以指定isCreateIfNot为true进行创建
        /// </summary>
        /// <param name="name"></param>
        /// <param name="isCreateIfNot"></param>
        /// <returns></returns>
        public SpawnPool GetSpawnPool(string name,bool isCreateIfNot = false)
        {
            if (m_SpawnDic.TryGetValue(name, out SpawnPool pool))
            {
                return pool;
            }

            if(isCreateIfNot)
            {
                return CreateSpawnPool(name);
            }
            return null;
        }
        /// <summary>
        /// 创建指定名称的分组
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public SpawnPool CreateSpawnPool(string name)
        {
            if (!m_SpawnDic.TryGetValue(name, out SpawnPool pool))
            {
                pool = new SpawnPool();
                pool.InitSpawn(name, m_CachedTransform);

                m_SpawnDic.Add(name, pool);
            }
            return pool;
        }
        /// <summary>
        /// 删除指定的分组，对应分组中所有的缓存池都将被删除
        /// </summary>
        /// <param name="name"></param>
        public void DeleteSpawnPool(string name)
        {
            if (m_SpawnDic.TryGetValue(name, out SpawnPool spawn))
            {
                spawn.DestroySpawn();
                m_SpawnDic.Remove(name);
            }
        }

        /// <summary>
        /// 使用PoolData进行资源加载，资源加载完成后创建对应的缓存池
        /// </summary>
        /// <param name="poolData"></param>
        public void LoadAssetToCreateGameObjectPool(PoolData poolData)
        {
            SpawnPool spawnPool = GetSpawnPool(poolData.SpawnName);
            if(spawnPool == null)
            {
                CreateSpawnPool(poolData.SpawnName);
            }else
            {
                if(spawnPool.HasGameObjectPool(poolData.AssetPath))
                {
                    Debug.LogWarning("PoolManager::LoadAssetToCreateGameObjectPool->GameObjectPool has been created!");
                    return;
                }
            }

            for(int i =0;i< m_PoolDatas.Count;i++)
            {
                PoolData pData = m_PoolDatas[i];
                if(pData.SpawnName == poolData.SpawnName && pData.AssetPath == poolData.AssetPath)
                {
                    Debug.LogError("PoolManager::CreateGameObjectPool->pool data has been added");
                    return;
                }
            }

            AssetLoaderHandle assetHandle = Loader.AssetManager.GetInstance().LoadAssetAsync(poolData.AssetPath, OnLoadComplete, AssetLoaderPriority.Default,null, poolData);
            poolData.LoaderHandle = assetHandle;
            m_PoolDatas.Add(poolData);
        }
        /// <summary>
        /// 加载完成的回调
        /// </summary>
        /// <param name="assetPath"></param>
        /// <param name="uObj"></param>
        /// <param name="userData"></param>
        private void OnLoadComplete(string assetPath,UnityObject uObj,System.Object userData)
        {
            PoolData poolData = userData as PoolData;

            if(!m_PoolDatas.Contains(poolData))
            {
                return;
            }

            m_PoolDatas.Remove(poolData);
            
            if(uObj is GameObject templateGO)
            {
                SpawnPool spawnPool = GetSpawnPool(poolData.SpawnName);
                if(spawnPool!=null)
                {
                    GameObjectPool objPool = spawnPool.CreateGameObjectPool(poolData.AssetPath, templateGO);
                    objPool.IsAutoClean = poolData.IsAutoClean;
                    objPool.PreloadTotalAmount = poolData.PreloadTotalAmount;
                    objPool.PreloadOnceAmount = poolData.PreloadOnceAmount;
                    objPool.completeCallback = poolData.CompleteCallback;
                    objPool.IsCull = poolData.IsCull;
                    objPool.CullOnceAmount = poolData.CullOnceAmount;
                    objPool.CullDelayTime = poolData.CullDelayTime;
                    objPool.LimitMaxAmount = poolData.LimitMaxAmount;
                    objPool.LimitMinAmount = poolData.LimitMinAmount;
                }
            }
        }
        /// <summary>
        /// 对各个缓存池的分组进行裁剪
        /// </summary>
        /// <param name="obj"></param>
        private void OnCullTimerUpdate(System.Object obj)
        {
            foreach(var kvp in m_SpawnDic)
            {
                kvp.Value.CullSpawn(m_CullTimeInterval);
            }
        }
        /// <summary>
        /// 重置所有的缓存池
        /// </summary>
        public override void DoReset()
        {
            foreach (var kvp in m_SpawnDic)
            {
                kvp.Value.DestroySpawn();
            }
            m_SpawnDic.Clear();
        }
        /// <summary>
        /// 销毁缓存池管理器
        /// </summary>
        public override void DoDispose()
        {
            DoReset();
            if(m_CullTimerTask != null)
            {
                TimerManager.GetInstance().RemoveTimer(m_CullTimerTask);
            }
            m_CullTimerTask = null;
            m_SpawnDic = null;
        }
       
    }
}
