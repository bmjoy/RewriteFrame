using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace Leyoutech.Core.Loader
{
    /// <summary>
    /// 对AB资源加载操作类
    ///每个bundle 一个操作类
    /// </summary>
    public class AssetBundleAsyncOperation : AAssetAsyncOperation
    {
        private AssetBundleCreateRequest m_asyncOperation = null;         //AB加载结果


        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="assetPath">资源路径</param>
        /// <param name="assetRoot">资源磁盘根路径</param>
        public AssetBundleAsyncOperation(string assetPath, string assetRoot) : base(assetPath, assetRoot)
        {
        }

        /// <summary>
        /// 在Update中检查AB是否完成加载
        /// </summary>
        public override void DoUpdate()
        {
            if (Status == AssetAsyncOperationStatus.Loading)
            {
                if (m_asyncOperation.isDone)
                {
                    Status = AssetAsyncOperationStatus.Loaded;
                }
            }
        }

        /// <summary>
        /// 从加载到的Operation中得到AB的引用
        /// </summary>
        /// <returns></returns>
        public override UnityObject GetAsset()
        {
            if (Status == AssetAsyncOperationStatus.Loaded)
            {
                return m_asyncOperation.assetBundle;
            }
            return null;
        }

        /// <summary>
        /// 当前资源加载的进度
        /// </summary>
        /// <returns></returns>
        public override float Progress()
        {
            if (Status == AssetAsyncOperationStatus.Loaded)
            {
                return 1;
            }
            else if (Status == AssetAsyncOperationStatus.Loading)
            {
                return m_asyncOperation.progress;
            }
            else
            {
                return 0;
            }
        }


        /// <summary>
        ///  创建AB的加载Operation
        /// </summary>
        protected override void CreateAsyncOperation()
        {
            m_asyncOperation = AssetBundle.LoadFromFileAsync(AssetRootPath+AssetPath);
        }
    }
}
