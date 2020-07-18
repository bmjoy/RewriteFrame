using Leyoutech.Core.Pool;
using System;
using System.Collections.Generic;
using SystemObject = System.Object;
using UnityObject = UnityEngine.Object;

namespace Leyoutech.Core.Loader
{
    internal class BridgeData : IObjectPoolItem
    {
        public AssetLoaderHandle LoaderHandle { get; set; }                                            //加载句柄
        public OnAssetLoadComplete Complete { get; set; }                                 //单个资源加载完成委托
        public OnBatchAssetLoadComplete BatchComplete { get; set; }             //一组资源加载完成
        public SystemObject UserData { get; set; }                                                   //携带的参数

        /// <summary>
        /// 对象创建时调用
        /// </summary>
        public void OnNew()
        {
        }

        /// <summary>
        /// 对象回收时调用
        /// </summary>
        public void OnRelease()
        {
            LoaderHandle = null;
            Complete = null;
            BatchComplete = null;
            UserData = null;
        }
    }

    /// <summary>
    /// 资源加载中间件类
    /// </summary>
    public class AssetLoaderBridge : IDisposable
    {
        private static ObjectPool<BridgeData> sm_BrigeDataPool = new ObjectPool<BridgeData>(10); //桥梁数据对象池

        private bool m_IsDisposed = false;                                                                                                        //是否已经执行释放操作了
        private List<BridgeData> m_BridgeDatas = new List<BridgeData>();                                              //桥梁数据列表
        private AssetLoaderPriority m_LoaderPriority = AssetLoaderPriority.Default;                              //加载优先级

        public AssetLoaderBridge()
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="priority">设置加载优先级</param>
        public AssetLoaderBridge(AssetLoaderPriority priority)
        {
            m_LoaderPriority = priority;
        }

        /// <summary>
        /// 释放
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// 析构
        /// </summary>
        ~AssetLoaderBridge()
        {
            Dispose(false);
        }

        /// <summary>
        /// 释放操作
        /// </summary>
        /// <param name="isDisposing">是否在在执行是否操作中了</param>
        private void Dispose(bool isDisposing)
        {
            if (m_IsDisposed) return;
            if(isDisposing)
            {
                if(m_BridgeDatas.Count>0)
                {
                    for(int i = m_BridgeDatas.Count-1;i>=0;--i)
                    {
                        CancelLoader(m_BridgeDatas[i]);
                    }
                }
            }
            m_IsDisposed = true;
        }

        /// <summary>
        /// 取消加载
        /// </summary>
        /// <param name="bridgeData"></param>
        private void CancelLoader(BridgeData bridgeData)
        {
            AssetLoaderHandle handle = bridgeData.LoaderHandle;
            if (bridgeData.Complete != null)
            {
                AssetManager.GetInstance().UnloadAssetLoader(handle, false);
            }
            else
            {
                AssetManager.GetInstance().UnloadAssetLoader(handle, true);
            }
            m_BridgeDatas.Remove(bridgeData);
            sm_BrigeDataPool.Release(bridgeData);
        }


        /// <summary>
        /// 加载单个资源
        /// </summary>
        /// <param name="pathOrAddress">路径/地址</param>
        /// <param name="complete">单个资源加载完成委托</param>
        /// <param name="userData">携带参数</param>
        public void LoadAssetAsync(string pathOrAddress,OnAssetLoadComplete complete,SystemObject userData = null)
        {
            LoadBatchAssetAsync(new string[] { pathOrAddress }, complete, null,userData);
        }

        /// <summary>
        /// 实例化单个资源
        /// </summary>
        /// <param name="pathOrAddress"> 路径/地址</param>
        /// <param name="complete">单个资源加载完成委托</param>
        /// <param name="userData">携带参数</param>
        public void InstanceAssetAsync(string pathOrAddress, OnAssetLoadComplete complete, SystemObject userData = null)
        {
            InstanceBatchAssetAsync(new string[] { pathOrAddress }, complete, null, userData);
        }

        /// <summary>
        /// 单个资源加载被取消
        /// </summary>
        /// <param name="complete"></param>
        public void CancelLoadAsset(OnAssetLoadComplete complete)
        {
            CancelLoadAsset(complete, null);
        }

       /// <summary>
       /// 一组资源加载被取消
       /// </summary>
       /// <param name="batchComplete"></param>
        public void CancelLoadAsset(OnBatchAssetLoadComplete batchComplete)
        {
            CancelLoadAsset(null, batchComplete);
        }


        /// <summary>
        /// 加载被取消的函数
        /// </summary>
        /// <param name="complete"></param>
        /// <param name="batchComplete"></param>
        public void CancelLoadAsset(OnAssetLoadComplete complete, OnBatchAssetLoadComplete batchComplete)
        {
            if(complete == null && batchComplete == null)
            {
                return;
            }

            for(int i = m_BridgeDatas.Count;i>=0;--i)
            {
                BridgeData bridgeData = m_BridgeDatas[i];
                bool isSame = true;
                if(complete!=null)
                {
                    isSame = bridgeData.Complete == complete;
                }
                if(isSame && batchComplete!=null)
                {
                    isSame = bridgeData.BatchComplete == batchComplete;
                }
                if(isSame)
                {
                    CancelLoader(bridgeData);
                }
            }
        }

        /// <summary>
        /// 加载一组资源
        /// </summary>
        /// <param name="pathOrAddresses">路径/地址</param>
        /// <param name="complete">单个资源加载完成委托</param>
        /// <param name="batchComplete"> 一组资源加载完成委托</param>
        /// <param name="userData">携带参数</param>
        public void LoadBatchAssetAsync(string[] pathOrAddresses,OnAssetLoadComplete complete,OnBatchAssetLoadComplete batchComplete, SystemObject userData = null)
        {
            BridgeData brigeData = sm_BrigeDataPool.Get();
            brigeData.Complete = complete;
            brigeData.BatchComplete = batchComplete;
            brigeData.UserData = userData;

            AssetLoaderHandle handle = AssetManager.GetInstance().LoadBatchAssetAsync(pathOrAddresses, AssetLoadComplete, BatchAssetLoadComplete,
                m_LoaderPriority, null,null, brigeData);

            brigeData.LoaderHandle = handle;

            m_BridgeDatas.Add(brigeData);
        }


        /// <summary>
        /// 实例化一组资源
        /// </summary>
        /// <param name="pathOrAddresses">路径/地址</param>
        /// <param name="complete">单个资源加载完成委托</param>
        /// <param name="batchComplete"> 一组资源加载完成委托</param>
        /// <param name="userData">携带参数</param>
        public void InstanceBatchAssetAsync(string[] pathOrAddresses, OnAssetLoadComplete complete, OnBatchAssetLoadComplete batchComplete, SystemObject userData = null)
        {
            BridgeData brigeData = sm_BrigeDataPool.Get();
            brigeData.Complete = complete;
            brigeData.BatchComplete = batchComplete;
            brigeData.UserData = userData;

            AssetLoaderHandle handle = AssetManager.GetInstance().InstanceBatchAssetAsync(pathOrAddresses, AssetLoadComplete, BatchAssetLoadComplete,
                m_LoaderPriority, null, null, brigeData);

            brigeData.LoaderHandle = handle;

            m_BridgeDatas.Add(brigeData);
        }

        /// <summary>
        /// 辅助的 单个资源加载完成委托
        /// </summary>
        /// <param name="pathOrAddress"></param>
        /// <param name="uObj"></param>
        /// <param name="userData"></param>
        private void AssetLoadComplete(string pathOrAddress, UnityObject uObj, SystemObject userData)
        {
            BridgeData brigeData = userData as BridgeData;
            brigeData.Complete?.Invoke(pathOrAddress, uObj, brigeData.UserData);
        }

        /// <summary>
        /// 辅助的 一组资源加载完成委托
        /// </summary>
        /// <param name="pathOrAddresses"></param>
        /// <param name="uObjs"></param>
        /// <param name="userData"></param>
        private void BatchAssetLoadComplete(string[] pathOrAddresses, UnityObject[] uObjs, SystemObject userData)
        {
            BridgeData bridgeData = userData as BridgeData;
            bridgeData.BatchComplete?.Invoke(pathOrAddresses, uObjs, bridgeData.UserData);

            m_BridgeDatas.Remove(bridgeData);
            sm_BrigeDataPool.Release(bridgeData);
        }
    }
}
