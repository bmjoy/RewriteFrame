using UnityEngine;
using UnityEngine.SceneManagement;
using SystemObject = System.Object;

namespace Leyoutech.Core.Loader
{
    /// <summary>
    /// 加载场景时存储的数据结构
    /// </summary>
    public class SceneLoadData
    {
        //由于场景的加载分两步执行，第一：先加载场景的AB及其依赖的AB，第二：对场景进行初始化。
        //此表示第一步占总体进度的比例
        private static float DEPEND_ASSET_PROGRESS_RATE = 0.9f;

        public string PathOrAddress { get; set; }
        public string AssetPath { get; set; }
        public LoadSceneMode LoadMode { get; set; } = LoadSceneMode.Single;
        public bool ActivateOnLoad { get; set; } = true;
        public OnSceneLoadComplete CompleteCallback { get; set; }
        public OnSceneLoadProgress ProgressCallback { get; set; }
        public SystemObject UserData { get; set; }

        public SceneLoaderHandle SceneHandle { get; set; }
        public AssetLoaderHandle LoaderHandle { get; set; } = null;
        public AsyncOperation AsyncOperation { get; set; } = null;

        public bool IsLoading() => LoaderHandle != null || AsyncOperation != null;

        public bool IsAssetLoaderDone()
        {
            if (LoaderHandle == null) return false;
            return LoaderHandle.State == AssetLoaderState.Complete;
        }

        public bool IsOperationDone()
        {
            if (AsyncOperation == null) return false;
            return AsyncOperation.isDone;
        }
        /// <summary>
        /// 获取场景的加载进度
        /// </summary>
        /// <returns></returns>
        public float Progress()
        {
            float progress = 0.0f;
            if (LoaderHandle != null)
            {
                progress = LoaderHandle.TotalProgress * DEPEND_ASSET_PROGRESS_RATE;
                if (AsyncOperation != null)
                {
                    progress += AsyncOperation.progress * (1 - DEPEND_ASSET_PROGRESS_RATE);
                }
            }
            else
            {
                if (AsyncOperation != null)
                {
                    progress = AsyncOperation.progress;
                }
            }
            return progress;
        }
    }
    /// <summary>
    /// 场景卸载数据结构
    /// </summary>
    public class SceneUnloadData
    {
        public string PathOrAddress { get; set; }
        public OnSceneUnloadComplete CompleteCallback { get; set; }
        public OnSceneUnloadProgress ProgressCallback { get; set; }
        public SystemObject UserData { get; set; }
        
        public AsyncOperation AsyncOperation { get; set; } = null;

        public bool IsDone()
        {
            if (AsyncOperation != null)
            {
                return AsyncOperation.isDone;
            }
            return false;
        }

        public bool IsUnloading()
        {
            return AsyncOperation != null;
        }

        public float Progress()
        {
            if (AsyncOperation != null)
            {
                return AsyncOperation.progress;
            }
            return 0.0f;
        }
    }

}
