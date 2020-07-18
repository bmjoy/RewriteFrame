using UnityObject = UnityEngine.Object;

namespace Leyoutech.Core.Loader
{
    /// <summary>
    /// 资源异步操作状态
    /// </summary>
    public enum AssetAsyncOperationStatus
    {
        None,                               //无效
        Loading,                          //加载中
        Loaded,                           //加载完成
    }

    /// <summary>
    /// 异步操作类
    /// </summary>
    public abstract class AAssetAsyncOperation
    {
        /// <summary>
        /// 资源路径
        /// </summary>
        public string AssetPath { get; set; }

        /// <summary>
        /// 存储Root根路径
        /// </summary>
        protected string AssetRootPath { get; set; }

        /// <summary>
        /// 异步操作状态
        /// </summary>
        public AssetAsyncOperationStatus Status { get; set; } = AssetAsyncOperationStatus.None;
        
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="assetPath">资源路径</param>
        /// <param name="assetRoot">存储Root根路径</param>
        public AAssetAsyncOperation(string assetPath,string assetRoot)
        {
            this.AssetPath = assetPath;
            AssetRootPath = assetRoot;
        }

        /// <summary>
        /// 开始异步
        /// </summary>
        public void StartAsync()
        {
            Status = AssetAsyncOperationStatus.Loading;
            CreateAsyncOperation();
        }
        
        /// <summary>
        /// 创建异步操作
        /// </summary>
        protected abstract void CreateAsyncOperation();

        /// <summary>
        /// 刷新
        /// </summary>
        public abstract void DoUpdate();

        /// <summary>
        /// 获取加载完成的Obj
        /// </summary>
        /// <returns></returns>
        public abstract UnityObject GetAsset();

        /// <summary>
        ///获取进度
        /// </summary>
        /// <returns></returns>
        public abstract float Progress();
    }
}
