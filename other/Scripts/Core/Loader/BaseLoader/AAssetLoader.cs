using Leyoutech.Core.Generic;
using Leyoutech.Core.Loader.Config;
using Leyoutech.Core.Pool;
using Priority_Queue;
using System;
using System.Collections.Generic;
using UnityEngine;
using SystemObject = System.Object;
using UnityObject = UnityEngine.Object;

namespace Leyoutech.Core.Loader
{
    public abstract class AAssetLoader
    {
        /// <summary>
        /// 唯一ID计数器
        /// </summary>
        private UniqueIDCreator m_IdCreator = new UniqueIDCreator();

        /// <summary>
        /// 加载任务数据池
        /// </summary>
        protected ObjectPool<AssetLoaderData> m_LoaderDataPool = new ObjectPool<AssetLoaderData>(10);

        /// <summary>
        /// 等待加载队列容器（稳定的 快速优先级排序队列）
        /// </summary>
        protected StablePriorityQueue<AssetLoaderData> m_LoaderDataWaitingQueue = new StablePriorityQueue<AssetLoaderData>(10);

        /// <summary>
        /// 在执行加载过程的加载任务列表
        /// </summary>
        protected List<AssetLoaderData> m_LoaderDataLoadingList = new List<AssetLoaderData>();

        /// <summary>
        /// 加载任务状态句柄，跟加载任务状态结构数据 的保存容器 《唯一ID，状态句柄》
        /// </summary>
        protected Dictionary<long, AssetLoaderHandle> m_LoaderHandleDic = new Dictionary<long, AssetLoaderHandle>();

        /// <summary>
        /// 加载实施操作列表
        /// </summary>
        protected List<AAssetAsyncOperation> m_LoadingAsyncOperationList = new List<AAssetAsyncOperation>();

        #region init Loader
        private bool m_IsInitFinished = false;                                                            //是否初始化完成
        private bool m_IsInitSuccess = false;                                                             //是否初始化成功

        /// <summary>
        /// //路径模式
        /// </summary>
        protected AssetPathMode m_PathMode = AssetPathMode.Address;

        /// <summary>
        /// 资源配置
        /// </summary>
        protected AssetAddressConfig m_AssetAddressConfig = null; 

        /// <summary>
        ///初始化完成回调
        /// </summary>
        protected Action<bool> m_InitCallback = null;

        /// <summary>
        /// 同时加载的最大加载数量（真正执行加载操作的数量--> Operation数量）
        /// </summary>
        private int m_MaxLoadingCount = 5;


        /// <summary>
        ///  同时加载的最大加载数量
        /// </summary>
        public int MaxLoadingCount { get => m_MaxLoadingCount; internal set => m_MaxLoadingCount = value; }

        /// <summary>
        /// 无用资源操作Operation
        /// </summary>
        private AsyncOperation m_UnloadUnusedAssetOperation = null;

        /// <summary>
        /// 无用资源释放完毕回调
        /// </summary>
        private Action m_UnloadUnusedAssetCallback = null;


        /// <summary>
        /// 获取路径
        /// </summary>
        /// <param name="pathOrAddress"></param>
        /// <returns></returns>
        internal string GetAssetPath(string pathOrAddress)
        {
            if(m_PathMode == AssetPathMode.Address)
            {
                return m_AssetAddressConfig.GetAssetPathByAddress(pathOrAddress);
            }
            return pathOrAddress;
        }


        /// <summary>
        /// 获取label 标签的所有路径集合
        /// </summary>
        /// <param name="label"></param>
        /// <returns></returns>
        public string[] GetAssetPathOrAddressByLabel(string label)
        {
            if(m_IsInitSuccess && m_AssetAddressConfig!=null)
            {
                if(m_PathMode == AssetPathMode.Address)
                {
                    return m_AssetAddressConfig.GetAssetAddressByLabel(label);
                }
                return m_AssetAddressConfig.GetAssetPathByLabel(label);
            }
            return null;
        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="initCallback">初始化完成回调</param>
        /// <param name="pathMode">地址模式</param>
        /// <param name="maxLoadingCount">同时加载的最大加载数量</param>
        /// <param name="assetRootDir">资源读取Root地址</param>
        public void Initialize(Action<bool> initCallback,AssetPathMode pathMode,int maxLoadingCount,string assetRootDir)
        {
            this.m_InitCallback = initCallback;
            this.m_MaxLoadingCount = maxLoadingCount;
            this.m_PathMode = pathMode;

            InnerInitialize(assetRootDir);
        }

        /// <summary>
        /// 读取资源配置文件
        /// </summary>
        /// <param name="assetRootDir"></param>
        protected abstract void InnerInitialize(string assetRootDir);

        /// <summary>
        /// 刷新初始化完成/成功情况
        /// </summary>
        /// <param name="isSuccess">成功结果</param>
        /// <returns></returns>
        protected abstract bool UpdateInitialize(out bool isSuccess);
        #endregion

        /// <summary>
        /// 加载或者初始化一批资源（异步的）
        /// </summary>
        /// <param name="pathOrAddresses">路径/地址集合</param>
        /// <param name="isInstance">是否执行实例化</param>
        /// <param name="priority"> 加载优先级</param>
        /// <param name="complete">单个资源加载完成委托</param>
        /// <param name="progress">单个资源加载进度委托</param>
        /// <param name="batchComplete"> 一组资源加载完成委托</param>
        /// <param name="batchProgress">一组资源加载进度委托</param>
        /// <param name="userData">携带参数</param>
        /// <returns></returns>
        public AssetLoaderHandle LoadOrInstanceBatchAssetAsync(string[] pathOrAddresses,
            bool isInstance,
            AssetLoaderPriority priority,
            OnAssetLoadComplete complete,
            OnAssetLoadProgress progress,
            OnBatchAssetLoadComplete batchComplete,
            OnBatchAssetsLoadProgress batchProgress,
            SystemObject userData)
        {
            long uniqueID = m_IdCreator.Next();//给这次任务数据AssetLoaderData，任务状态 AssetLoaderHandle 分配一个唯一标识ID

            if (pathOrAddresses == null || pathOrAddresses.Length == 0)
            {
                Debug.LogError($"AAssetLoader::LoadOrInstanceBatchAssetAsync->pathOrAddresses is Null");
                return null;
            }

            AssetLoaderData loaderData = m_LoaderDataPool.Get();//对象池获取一个数据结构
            loaderData.m_PathOrAddresses = pathOrAddresses;

            if (m_PathMode == AssetPathMode.Address)
            {
                loaderData.m_AssetPaths = m_AssetAddressConfig.GetAssetPathByAddress(pathOrAddresses);
                if (loaderData.m_AssetPaths == null)
                {
                    m_LoaderDataPool.Release(loaderData);
                    Debug.LogError($"AAssetLoader::GetLoaderData->asset not found.address = {string.Join(",", pathOrAddresses)}");
                    return null;
                }
            }
            else
            {
                loaderData.m_AssetPaths = pathOrAddresses;
            }
            loaderData.InitData();

            loaderData.m_UniqueID = uniqueID;                                                                                                                                 //唯一标记ID
            loaderData.m_IsInstance = isInstance;                                                                                                                              //是否初始化
            loaderData.CompleteCallback = complete;                                                                                                                 //单资源完成
            loaderData.ProgressCallback = progress;                                                                                                                    // 单资源进度
            loaderData.BatchCompleteCallback = batchComplete;                                                                                           //全部完成
            loaderData.BatchProgressCallback = batchProgress;                                                                                               //全部进度
            loaderData.m_UserData = userData;                                                                                                                                //携带参数

            if (m_LoaderDataWaitingQueue.Count >= m_LoaderDataWaitingQueue.MaxSize)                                                       //容量不够，扩容 2倍
            {
                m_LoaderDataWaitingQueue.Resize(m_LoaderDataWaitingQueue.MaxSize * 2);
            }
            m_LoaderDataWaitingQueue.Enqueue(loaderData, (float)priority);                                                                       //插入等待容器，并优先级排序，

            AssetLoaderHandle handle = new AssetLoaderHandle(uniqueID, pathOrAddresses);                                 //加载状态句柄
            handle.State = AssetLoaderState.Waiting;
            m_LoaderHandleDic.Add(uniqueID, handle);

            return handle;
        }
        
        /// <summary>
        /// 刷新
        /// </summary>
        /// <param name="deltaTime"></param>
        public void DoUpdate(float deltaTime)
        {
            if(!m_IsInitFinished)
            {
                m_IsInitFinished = UpdateInitialize(out m_IsInitSuccess);
                if(m_IsInitFinished)
                {
                    if(!m_IsInitSuccess)
                    {
                        Debug.LogError("AssetLoader::DoUpdate->init failed");
                    }

                    m_InitCallback?.Invoke(m_IsInitSuccess);
                    m_InitCallback = null;
                }
                return;
            }
            if(!m_IsInitSuccess)
            {
                return;
            }

            //刷新等待列表
            UpdateWaitingLoaderData();

            //刷新 操作状态机 Operetion 状态
            UpdateAsyncOperation();

            //刷新加载中列表的资源的  加载结果
            UpdateLoadingLoaderData();

            //检查无用资源执行GC
            CheckUnloadUnusedAction();
        }

        /// <summary>
        /// 更新等待列表
        /// 到开始加载队列
        /// </summary>
        private void UpdateWaitingLoaderData()
        {
            while (m_LoaderDataWaitingQueue.Count > 0 && m_LoadingAsyncOperationList.Count < MaxLoadingCount) //Operation数量 < 5
            {
                AssetLoaderData loaderData = m_LoaderDataWaitingQueue.Dequeue();                                                      //出列，移除头部元素
                m_LoaderHandleDic[loaderData.m_UniqueID].State = AssetLoaderState.Loading;                                            //更新对应句柄，加载状态为 加载中ing

                m_LoaderDataLoadingList.Add(loaderData);
                StartLoaderDataLoading(loaderData);
            }
        }

        /// <summary>
        /// 刷新 操作状态机 Operetion 状态
        /// </summary>
        private void UpdateAsyncOperation()
        {
            if(m_LoadingAsyncOperationList.Count>0)
            {
                int index = 0;
                while(index<m_LoadingAsyncOperationList.Count&&index< MaxLoadingCount)
                {
                    AAssetAsyncOperation operation = m_LoadingAsyncOperationList[index];
                    operation.DoUpdate();

                    if (operation.Status == AssetAsyncOperationStatus.None)
                    {
                        operation.StartAsync();//operation 状态，修改为加载中
                    }
                    else if (operation.Status == AssetAsyncOperationStatus.Loaded)
                    {
                        //加载操作完成，移除位置，等待列表补位
                        m_LoadingAsyncOperationList.RemoveAt(index);
                        OnAsyncOperationLoaded(operation);
                        continue;
                    }

                    ++index;
                }
            }
        }

        /// <summary>
        /// 一个加载操作，完成了
        /// </summary>
        /// <param name="operation"></param>
        protected virtual void OnAsyncOperationLoaded(AAssetAsyncOperation operation)
        {

        }

        /// <summary>
        /// 刷新加载中列表的资源的  加载结果
        /// </summary>
        private void UpdateLoadingLoaderData()
        {
            if(m_LoaderDataLoadingList.Count>0)
            {
                for(int i = m_LoaderDataLoadingList.Count-1;i>=0;--i)
                {
                    AssetLoaderData loaderData = m_LoaderDataLoadingList[i];
                    if (UpdateLoadingLoaderData(loaderData))
                    {
                        //加载完成
                        m_LoaderDataLoadingList.RemoveAt(i);
                        m_LoaderHandleDic.Remove(loaderData.m_UniqueID);
                        m_LoaderDataPool.Release(loaderData);
                    }
                }
            }
        }

        /// <summary>
        /// 更新加载状态，进度等，并反馈此次加载任务的完成结果
        /// </summary>
        /// <param name="loaderData">加载任务数据</param>
        /// <returns></returns>
        protected abstract bool UpdateLoadingLoaderData(AssetLoaderData loaderData);


        /// <summary>
        /// 设置加载操作数据,创建 Operation
        /// </summary>
        /// <param name="loaderData"></param>
        protected abstract void StartLoaderDataLoading(AssetLoaderData loaderData);

        #region unload Asset


        /// <summary>
        /// 
        /// 警告：在AssetBundle中，会强制性的卸载掉使用的资源，请确保指向的资源没有其它使用者。
        /// </summary>
        /// <param name="pathOrAddress"></param>
        public virtual void UnloadAsset(string pathOrAddress)
        {

        }


        /// <summary>
        /// 卸载一个加载任务
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="destroyIfLoaded"></param>

        public void UnloadAssetLoader(AssetLoaderHandle handle,bool destroyIfLoaded)
        {
            if(m_LoaderHandleDic.ContainsKey(handle.UniqueID))
            {
                m_LoaderHandleDic.Remove(handle.UniqueID);
            }
            else
            {
                return;
            }
            
            //等待队列取消
            foreach(var data in m_LoaderDataWaitingQueue)
            {
                if(data.m_UniqueID == handle.UniqueID)
                {
                    handle.CancelLoader(data.m_IsInstance && destroyIfLoaded);
                    m_LoaderDataWaitingQueue.Remove(data);
                    m_LoaderDataPool.Release(data);
                    return;
                }
            }

            //加载中列表取消
            foreach(var data in m_LoaderDataLoadingList)
            {
                if(data.m_UniqueID == handle.UniqueID)
                {
                    handle.CancelLoader(data.m_IsInstance && destroyIfLoaded);
                    data.CancelLoader();

                    //加载数据取消
                    UnloadLoadingAssetLoader(data);
                    return;
                }
            }
        }

        /// <summary>
        /// 卸载，加载中“加载数据”执行删除
        /// </summary>
        /// <param name="loaderData"></param>
        protected abstract void UnloadLoadingAssetLoader(AssetLoaderData loaderData);


        /// <summary>
        ///检查无用资源执行GC
        /// </summary>
        private void CheckUnloadUnusedAction()
        {
            if(m_UnloadUnusedAssetOperation!=null)
            {
                if(m_UnloadUnusedAssetOperation.isDone)
                {
                    GC.Collect();
                    InnerUnloadUnusedAssets();                      //内部卸载无用资源函数

                    m_UnloadUnusedAssetOperation = null;
                    m_UnloadUnusedAssetCallback?.Invoke();    //回调
                    m_UnloadUnusedAssetCallback = null;
                }
            }
        }

        /// <summary>
        /// 卸载闲置资源
        /// </summary>
        /// <param name="callback"></param>

        public void UnloadUnusedAssets(Action callback)
        {
            if(m_UnloadUnusedAssetOperation == null)
            {
                m_UnloadUnusedAssetCallback = callback;

                GC.Collect();
                m_UnloadUnusedAssetOperation = Resources.UnloadUnusedAssets();
                GC.Collect();
            }
        }

        /// <summary>
        /// 内部卸载无用资源函数
        /// </summary>
        protected virtual void InnerUnloadUnusedAssets() { }
        #endregion


        /// <summary>
        /// 实例化资源
        /// </summary>
        /// <param name="assetPath"></param>
        /// <param name="asset"></param>
        /// <returns></returns>
        public virtual UnityObject InstantiateAsset(string assetPath, UnityObject asset)
        {
            return UnityObject.Instantiate(asset);
        }
    }
}
