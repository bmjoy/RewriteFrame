using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using SystemObject = System.Object;

namespace Leyoutech.Core.Loader
{
    /// <summary>
    /// 对于场景的加载器
    /// </summary>
    public class SceneAssetLoader
    {
        private AssetLoaderMode m_LoaderMode;
        private AAssetLoader m_AssetLoader;

        private List<SceneLoaderHandle> m_LoadedSceneHandles = new List<SceneLoaderHandle>();//加载完成的场景

        private List<SceneLoadData> m_LoadingSceneDatas = new List<SceneLoadData>();//正在加载的场景的数据
        private List<SceneUnloadData> m_UnloadingSceneDatas = new List<SceneUnloadData>();//卸载中的场景的数据

        public SceneAssetLoader(AssetLoaderMode loaderMode, AAssetLoader assetLoader)
        {
            this.m_LoaderMode = loaderMode;
            this.m_AssetLoader = assetLoader;
        }
        /// <summary>
        /// 加载场景
        /// </summary>
        /// <param name="pathOrAddress">地址</param>
        /// <param name="completeCallback">完成后回调</param>
        /// <param name="progressCallback">加载进度回调</param>
        /// <param name="loadMode">场景加载模式</param>
        /// <param name="activateOnLoad">是否立即激活</param>
        /// <param name="userData">自定义数据</param>
        /// <returns></returns>
        public SceneLoaderHandle LoadSceneAsync(string pathOrAddress,
            OnSceneLoadComplete completeCallback,
            OnSceneLoadProgress progressCallback,
            LoadSceneMode loadMode = LoadSceneMode.Single,
            bool activateOnLoad = true,
            SystemObject userData = null)
        {
            bool isSceneLoaded = m_LoadedSceneHandles.Any((sHandle) =>
            {
                return sHandle.PathOrAddress == pathOrAddress;
            });
            if (isSceneLoaded)
            {
                Debug.LogError($"SceneAssetLoader::LoadSceneAsync->Scene has been loaded.pathOrAddress={pathOrAddress}");
                return null;
            }
            bool isSceneLoading = m_LoadingSceneDatas.Any((loadData) =>
            {
                return loadData.PathOrAddress == pathOrAddress;
            });
            if (isSceneLoading)
            {
                Debug.LogError($"SceneAssetLoader::LoadSceneAsync->Scene is in loading.pathOrAddress={pathOrAddress}");
                return null;
            }

            string assetPath = m_AssetLoader.GetAssetPath(pathOrAddress);
            string sceneName = Path.GetFileNameWithoutExtension(assetPath);

            SceneLoadData loaderData = new SceneLoadData();
            loaderData.PathOrAddress = pathOrAddress;
            loaderData.AssetPath = assetPath;
            loaderData.CompleteCallback = completeCallback;
            loaderData.ProgressCallback = progressCallback;
            loaderData.LoadMode = loadMode;
            loaderData.ActivateOnLoad = activateOnLoad;
            loaderData.UserData = userData;
            
            SceneLoaderHandle handle = new SceneLoaderHandle();
            handle.PathOrAddress = pathOrAddress;
            handle.AssetPath = assetPath;
            handle.SceneName = sceneName;
            loaderData.SceneHandle = handle;

            m_LoadingSceneDatas.Add(loaderData);

            return handle;
        }
        /// <summary>
        /// 卸载场景
        /// </summary>
        /// <param name="pathOrAddress">资源地址</param>
        /// <param name="completeCallback">完成后回调</param>
        /// <param name="progressCallback">卸载进度 回调 </param>
        /// <param name="userData">自定义数据</param>
        public void UnloadSceneAsync(string pathOrAddress,
            OnSceneUnloadComplete completeCallback,
            OnSceneUnloadProgress progressCallback,
            SystemObject userData = null)
        {
            bool isSceneLoaded = m_LoadedSceneHandles.Any((sHandle) =>
            {
                return sHandle.PathOrAddress == pathOrAddress;
            });
            if (!isSceneLoaded)
            {
                Debug.LogError($"SceneAssetLoader::UnloadSceneAsync->Scene not found.pathOrAddress={pathOrAddress}");
                return;
            }

            SceneUnloadData unloadData = new SceneUnloadData();
            unloadData.PathOrAddress = pathOrAddress;
            unloadData.CompleteCallback = completeCallback;
            unloadData.ProgressCallback = progressCallback;
            unloadData.UserData = userData;

            m_UnloadingSceneDatas.Add(unloadData);
        }
        /// <summary>
        /// 在Update中检查场景加载与卸载
        /// </summary>
        /// <param name="deltaTime"></param>
        internal void DoUpdate(float deltaTime)
        {
            if (m_LoadingSceneDatas.Count > 0)
            {
                SceneLoadData loadData = m_LoadingSceneDatas[0];
                if (IsSceneLoadComplete(loadData))
                {
                    m_LoadingSceneDatas.RemoveAt(0);
                    SceneLoadComplete(loadData);
                } else if (!loadData.IsLoading())
                {
                    SceneLoadStart(loadData);
                } else
                {
                    SceneLoadProgress(loadData);
                }
            }
            //处理场景的卸载
            if (m_UnloadingSceneDatas.Count > 0)
            {
                SceneUnloadData unloadData = m_UnloadingSceneDatas[0];
                if (unloadData.IsDone())
                {
                    m_UnloadingSceneDatas.RemoveAt(0);
                    SceneUnloadComplete(unloadData);
                } else if (unloadData.IsUnloading())
                {
                    unloadData.ProgressCallback?.Invoke(unloadData.PathOrAddress, unloadData.Progress(), unloadData.UserData);
                } else
                {
                    SceneUnloadStart(unloadData);
                }
            }
        }
        /// <summary>
        /// 判断是否加载完成
        /// </summary>
        /// <param name="loadData"></param>
        /// <returns></returns>
        private bool IsSceneLoadComplete(SceneLoadData loadData)
        {
            if (m_LoaderMode == AssetLoaderMode.AssetDatabase)
            {
                return loadData.IsOperationDone();
            }
            else if (m_LoaderMode == AssetLoaderMode.AssetBundle)
            {
                return loadData.IsOperationDone() && loadData.IsAssetLoaderDone();
            }
            Debug.LogError($"SceneAssetLoader::IsSceneLoadComplete->Unvalid AssetLoaderMode.loaderMode = {m_LoaderMode}");
            return false;
        }
        /// <summary>
        /// 开始加载场景
        /// </summary>
        /// <param name="loadData"></param>
        private void SceneLoadStart(SceneLoadData loadData)
        {
            if (m_LoaderMode == AssetLoaderMode.AssetDatabase)
            {
#if UNITY_EDITOR
                loadData.AsyncOperation = UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode(loadData.AssetPath, new LoadSceneParameters(loadData.LoadMode));
#endif
            }
            else if (m_LoaderMode == AssetLoaderMode.AssetBundle)
            {
                loadData.LoaderHandle = m_AssetLoader.LoadOrInstanceBatchAssetAsync(new string[] { loadData.PathOrAddress }, false, AssetLoaderPriority.High, null, null, null, null, null);
            }
        }
        /// <summary>
        /// 场景加载进度 处理
        /// </summary>
        /// <param name="loadData"></param>
        private void SceneLoadProgress(SceneLoadData loadData)
        {
            if (m_LoaderMode == AssetLoaderMode.AssetBundle && loadData.IsAssetLoaderDone() && loadData.AsyncOperation == null)
            {
                string sceneName = Path.GetFileNameWithoutExtension(loadData.AssetPath);
                loadData.AsyncOperation = SceneManager.LoadSceneAsync(sceneName, loadData.LoadMode);
            }

            float progress = loadData.Progress();
            float oldProgress = loadData.SceneHandle.Progress;
            if (progress != oldProgress)
            {
                loadData.SceneHandle.Progress = progress;

                loadData.ProgressCallback?.Invoke(loadData.PathOrAddress, progress, loadData.UserData);
            }
        }
        /// <summary>
        /// 场景加载结束
        /// </summary>
        /// <param name="loadData"></param>
        private void SceneLoadComplete(SceneLoadData loadData)
        {
            if (loadData.LoadMode == LoadSceneMode.Single)
            {
                foreach (var handle in m_LoadedSceneHandles)
                {
                    m_AssetLoader.UnloadAsset(handle.PathOrAddress);
                }
                m_LoadedSceneHandles.Clear();
            }

            SceneLoaderHandle loaderHandle = loadData.SceneHandle;
            Scene scene = SceneManager.GetSceneByName(loaderHandle.SceneName);
            loaderHandle.SetScene(scene);

            m_LoadedSceneHandles.Add(loaderHandle);
            loadData.CompleteCallback?.Invoke(loadData.PathOrAddress, scene, loadData.UserData);
        }
        /// <summary>
        /// 场景卸载结束
        /// </summary>
        /// <param name="unloadData"></param>
        private void SceneUnloadComplete(SceneUnloadData unloadData)
        {
            SceneLoaderHandle loaderHandle = null;
            foreach(var handle in m_LoadedSceneHandles)
            {
                if(handle.PathOrAddress == unloadData.PathOrAddress)
                {
                    loaderHandle = handle;
                    break;
                }
            }
            m_LoadedSceneHandles.Remove(loaderHandle);

            m_AssetLoader.UnloadAsset(loaderHandle.PathOrAddress);

            unloadData.CompleteCallback?.Invoke(unloadData.PathOrAddress, unloadData.UserData);
        }
        /// <summary>
        /// 开始卸载场景
        /// </summary>
        /// <param name="unloadData"></param>
        private void SceneUnloadStart(SceneUnloadData unloadData)
        {
            SceneLoaderHandle loaderHandle = null;
            foreach (var handle in m_LoadedSceneHandles)
            {
                if (handle.PathOrAddress == unloadData.PathOrAddress)
                {
                    loaderHandle = handle;
                    break;
                }
            }
            unloadData.AsyncOperation = SceneManager.UnloadSceneAsync(loaderHandle.SceneName);
        }
    }
}
