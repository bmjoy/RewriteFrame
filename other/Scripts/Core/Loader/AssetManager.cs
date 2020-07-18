using Leyoutech.Core.Util;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using SystemObject = System.Object;
using UnityObject = UnityEngine.Object;

namespace Leyoutech.Core.Loader
{
    public class AssetManager : Util.Singleton<AssetManager>
    {
		public const string LOG_TAG = "Asset";

        private AAssetLoader m_AssetLoader = null;                                //资源加载器
        private SceneAssetLoader m_SceneLoader = null;                       //场景资源加载器
        private bool m_IsInit = false;                                                            //是否成功初始化


        /// <summary>
        /// 同时加载的最大数量
        /// </summary>
        public int MaxLoadingCount
        {
            get
            {
                return  m_AssetLoader.MaxLoadingCount;
            }
            set
            {
                m_AssetLoader.MaxLoadingCount = value;
            }
        }
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="loaderMode">加载方式</param>
        /// <param name="pathMode">路径方式</param>
        /// <param name="maxLoadingCount">同时加载的最大数量</param>
        /// <param name="assetRootDir">资源存放根路径</param>
        /// <param name="initCallback">回调</param>
        public void InitLoader(AssetLoaderMode loaderMode, 
            AssetPathMode pathMode, 
            int maxLoadingCount, 
            string assetRootDir, 
            Action<bool> initCallback)
        {
			Leyoutech.Utility.DebugUtility.Log(LOG_TAG, $"Begin init loader AssetLoaderMode({loaderMode}) AssetPathMode({pathMode}) MaxLoadingCount({maxLoadingCount}) AssetRootDir({assetRootDir})");
            			
            if(loaderMode == AssetLoaderMode.AssetBundle)
            {
                m_AssetLoader = new AssetBundleLoader();
            }
            else if (loaderMode == AssetLoaderMode.AssetDatabase)
			{
				m_AssetLoader = new AssetDatabaseLoader();
#if !UNITY_EDITOR
				Debug.LogError("AssetManager::InitLoader->AssetLoaderMode(AssetDatabase) can be used in Editor");
#endif
			}
			m_AssetLoader?.Initialize((isSuccess) =>
            {
                m_IsInit = isSuccess;
                if(isSuccess)
                {
                    m_SceneLoader = new SceneAssetLoader(loaderMode, m_AssetLoader);
                }

				Leyoutech.Utility.DebugUtility.Log(LOG_TAG, "End init loader");
				initCallback?.Invoke(isSuccess);
			},pathMode, maxLoadingCount ,assetRootDir);
        }

        /// <summary>
        /// 异步加载单个文件
        /// </summary>
        /// <param name="pathOrAddress">路径/地址</param>
        /// <param name="complete">单个资源加载完成委托</param>
        /// <param name="priority">加载优先级</param>
        /// <param name="progress">单个资源加载进度委托</param>
        /// <param name="userData">携带参数</param>
        /// <returns></returns>
        public AssetLoaderHandle LoadAssetAsync(
            string pathOrAddress,
            OnAssetLoadComplete complete, 
            AssetLoaderPriority priority = AssetLoaderPriority.Default,  
            OnAssetLoadProgress progress = null,
            SystemObject userData = null)
        {
            if(m_IsInit)
            {
                return m_AssetLoader.LoadOrInstanceBatchAssetAsync(new string[] { pathOrAddress }, false, priority, complete, progress, null, null, userData);
            }
            else
            {
                Debug.LogError("AssetManager::LoadAssetAsync->init is failed");
                return null;
            }
        }

        /// <summary>
        /// 一组资源加载
        /// </summary>
        /// <param name="pathOrAddresses">一组 路径/地址</param>
        /// <param name="complete">单个资源加载完成委托</param>
        /// <param name="batchComplete">一组资源加载完成委托</param>
        /// <param name="priority">加载优先级</param>
        /// <param name="progress">单个资源加载进度委托</param>
        /// <param name="batchProgress">一组资源加载进度委托</param>
        /// <param name="userData">携带参数</param>
        /// <returns></returns>
        public AssetLoaderHandle LoadBatchAssetAsync(
            string[] pathOrAddresses,
            OnAssetLoadComplete complete,
            OnBatchAssetLoadComplete batchComplete,
            AssetLoaderPriority priority = AssetLoaderPriority.Default,
            OnAssetLoadProgress progress = null,
            OnBatchAssetsLoadProgress batchProgress = null,
            SystemObject userData = null)
        {
            if (m_IsInit)
            {
                return m_AssetLoader.LoadOrInstanceBatchAssetAsync(pathOrAddresses, false, priority, complete, progress, batchComplete, batchProgress, userData);
            }
            else
            {
                Debug.LogError("AssetManager::LoadAssetAsync->init is failed");
                return null;
            }
        }


        /// <summary>
        /// 实例化单个资源
        /// </summary>
        /// <param name="pathOrAddress">路径/地址</param>
        /// <param name="complete">单个资源加载完成委托</param>
        /// <param name="priority">加载优先级</param>
        /// <param name="progress">单个资源加载进度委托</param>
        /// <param name="userData">携带参数</param>
        /// <returns></returns>
        public AssetLoaderHandle InstanceAssetAsync(
            string pathOrAddress,
            OnAssetLoadComplete complete,
            AssetLoaderPriority priority = AssetLoaderPriority.Default,
            OnAssetLoadProgress progress = null,
            SystemObject userData = null)
        {
            if (m_IsInit)
            {
                return m_AssetLoader.LoadOrInstanceBatchAssetAsync(new string[] { pathOrAddress }, true, priority, complete, progress, null, null, userData);
            }
            else
            {
                Debug.LogError("AssetManager::LoadAssetAsync->init is failed");
                return null;
            }
        }


        /// <summary>
        ///实例化一组资源
        /// </summary>
        /// <param name="pathOrAddresses">一组 路径/地址</param>
        /// <param name="complete">单个资源加载完成委托</param>
        /// <param name="batchComplete">一组资源加载完成委托</param>
        /// <param name="priority">加载优先级</param>
        /// <param name="progress">单个资源加载进度委托</param>
        /// <param name="batchProgress">一组资源加载进度委托</param>
        /// <param name="userData">携带参数</param>
        /// <returns></returns>
        public AssetLoaderHandle InstanceBatchAssetAsync(
            string[] pathOrAddresses,
            OnAssetLoadComplete complete,
            OnBatchAssetLoadComplete batchComplete,
            AssetLoaderPriority priority = AssetLoaderPriority.Default,
            OnAssetLoadProgress progress = null,
            OnBatchAssetsLoadProgress batchProgress = null,
            SystemObject userData = null)
        {
            if (m_IsInit)
            {
                return m_AssetLoader.LoadOrInstanceBatchAssetAsync(pathOrAddresses, true, priority, complete, progress, batchComplete, batchProgress, userData);
            }
            else
            {
                Debug.LogError("AssetManager::LoadAssetAsync->init is failed");
                return null;
            }
        }

        /// <summary>
        /// 实例化一个资源
        /// </summary>
        /// <param name="pathOrAddress">路径/地址</param>
        /// <param name="asset">资源/模板UnityObject</param>
        /// <returns></returns>
        public UnityObject InstantiateAsset(string pathOrAddress,UnityObject asset)
        {
            if (m_IsInit)
            {
                if(string.IsNullOrEmpty(pathOrAddress) || asset == null)
                {
                    Debug.LogError($"AssetManager::InstantiateAsset->asset is null or asset is null.assetPath = {(pathOrAddress ?? "")}");
                    return null;
                }
                return m_AssetLoader?.InstantiateAsset(pathOrAddress, asset);
            }
            else
            {
                Debug.LogError("AssetManager::InstantiateAsset->init is failed");
                return null;
            }
        }

        /// <summary>
        /// 加载场景
        /// </summary>
        /// <param name="pathOrAddress">路径/地址</param>
        /// <param name="completeCallback">场景加载完成委托</param>
        /// <param name="progressCallback">场景加载进度委托</param>
        /// <param name="loadMode">加载方式</param>
        /// <param name="activateOnLoad">是否激活场景，（没有到这个参数呀）</param>
        /// <param name="userData">携带参数</param>
        /// <returns></returns>
        public SceneLoaderHandle LoadSceneAsync(string pathOrAddress,
            OnSceneLoadComplete completeCallback,
            OnSceneLoadProgress progressCallback,
            LoadSceneMode loadMode = LoadSceneMode.Single,
            bool activateOnLoad = true,
            SystemObject userData = null)
        {
            if(m_SceneLoader == null)
            {
                Debug.LogError("AssetManager::LoadSceneAsync->sceneLoader has not been inited");
                return null;
            }
            return m_SceneLoader.LoadSceneAsync(pathOrAddress, completeCallback, progressCallback, loadMode, activateOnLoad, userData);
        }


        /// <summary>
        /// 卸载场景
        /// </summary>
        /// <param name="pathOrAddress">路径/地址</param>
        /// <param name="completeCallback">完成委托</param>
        /// <param name="progressCallback">进度委托</param>
        /// <param name="userData">携带参数</param>
        public void UnloadSceneAsync(string pathOrAddress,
            OnSceneUnloadComplete completeCallback,
            OnSceneUnloadProgress progressCallback,
            SystemObject userData = null)
        {
            if (m_SceneLoader == null)
            {
                Debug.LogError("AssetManager::LoadSceneAsync->sceneLoader has not been inited");
                return;
            }
            m_SceneLoader.UnloadSceneAsync(pathOrAddress, completeCallback, progressCallback, userData);
        }

        /// <summary>
        /// 卸载闲置资源
        /// </summary>
        /// <param name="callback"></param>
        public void UnloadUnusedAsset(Action callback = null)
        {
            if (m_IsInit)
            {
                m_AssetLoader?.UnloadUnusedAssets(callback);
            }
            else
            {
                Debug.LogError("AssetManager::InstantiateAsset->init is failed");
            }
        }

        /// <summary>
        /// 卸载一个加载任务
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="destroyIfLoaded"></param>
        public void UnloadAssetLoader(AssetLoaderHandle handle, bool destroyIfLoaded = false)
        {
            if (m_IsInit)
            {
               m_AssetLoader?.UnloadAssetLoader(handle, destroyIfLoaded);
            }
            else
            {
                Debug.LogError("AssetManager::InstantiateAsset->init is failed");
            }
        }
        
        /// <summary>
        /// 通过 label 标签，获取一组资源地址
        /// </summary>
        /// <param name="label"></param>
        /// <returns></returns>
        public string[] GetAssetPathOrAddressByLabel(string label)
        {
            if(m_IsInit && m_AssetLoader!=null)
            {
                return m_AssetLoader.GetAssetPathOrAddressByLabel(label);
            }
            return null;
        }

        /// <summary>
        /// Update
        /// </summary>
        /// <param name="deltaTime"></param>
        public void DoUpdate(float deltaTime)
        {
            m_AssetLoader?.DoUpdate(deltaTime);
            if(m_IsInit)
            {
                m_SceneLoader?.DoUpdate(deltaTime);
            }
        }

		/// <summary>
		/// Is Init
		/// </summary>
		/// <returns></returns>
		public bool IsInit()
		{
			return m_IsInit;
		}
    }
}
