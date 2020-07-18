using UnityObject = UnityEngine.Object;

namespace Leyoutech.Core.Loader
{
    /// <summary>
    /// Database 的异步操作类
    /// 每个资源地址一个操作类
    /// </summary>
    public class AssetDatabaseAsyncOperation : AAssetAsyncOperation
    {
        public AssetDatabaseAsyncOperation(string assetPath) : base(assetPath, "")
        {
        }

        /// <summary>
        /// 创建异步操作
        /// </summary>
        protected override void CreateAsyncOperation()
        {
            
        }

        /// <summary>
        /// 刷新
        /// </summary>
        public override void DoUpdate()
        {
            if(Status == AssetAsyncOperationStatus.Loading)         //Database 状态直接切换
            {
                Status = AssetAsyncOperationStatus.Loaded;
            }
        }

        /// <summary>
        /// 获取加载完成的Obj
        /// </summary>
        public override UnityObject GetAsset()
        {
            if(Status == AssetAsyncOperationStatus.Loaded)
            {
#if UNITY_EDITOR
                return UnityEditor.AssetDatabase.LoadAssetAtPath(AssetPath, typeof(UnityObject));
#endif
            }
            return null;
        }



        /// <summary>
        ///获取进度
        /// </summary>
        /// <returns></returns>
        public override float Progress()
        {
            if(Status == AssetAsyncOperationStatus.Loaded)
            {
                return 1.0f;
            }
            return 0.0f;
        }
    }
}
