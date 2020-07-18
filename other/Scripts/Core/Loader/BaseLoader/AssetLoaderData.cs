using Leyoutech.Core.Pool;
using Priority_Queue;
using SystemObject = System.Object;
using UnityObject = UnityEngine.Object;

namespace Leyoutech.Core.Loader
{
    /// <summary>
    /// 一次加载任务的数据
    /// </summary>
    public class AssetLoaderData : StablePriorityQueueNode, IObjectPoolItem
    {
       /// <summary>
       /// 唯一索引ID
       /// </summary>
        public long m_UniqueID = -1;

        /// <summary>
        ///Path集合
        /// </summary>
        public string[] m_PathOrAddresses;

        /// <summary>
        /// Address集合
        /// </summary>
        public string[] m_AssetPaths;

        /// <summary>
        /// 是否需要实例化
        /// </summary>
        public bool m_IsInstance = false;

        /// <summary>
        /// 携带参数
        /// </summary>
        public SystemObject m_UserData;


        #region 回调委托
        public OnAssetLoadComplete CompleteCallback;                      //单个资源加载完成
        public OnAssetLoadProgress ProgressCallback;                          //单个资源加载完成进度回调
        public OnBatchAssetLoadComplete BatchCompleteCallback; //全部完成回调
        public OnBatchAssetsLoadProgress BatchProgressCallback;   //全部完成进度
        #endregion

        /// <summary>
        /// 资源加载情况，对应true = 加载完成
        /// </summary>
        private bool[] m_AssetLoadStates;


        /// <summary>
        /// 初始化
        /// </summary>
        internal void InitData()
        {
            m_AssetLoadStates = new bool[m_AssetPaths.Length];
        }


        /// <summary>
        /// 获取index 的资源加载状态
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        internal bool GetLoadState(int index) => m_AssetLoadStates[index];


        /// <summary>
        /// 设置index 的资源加载状态 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        internal bool SetLoadState(int index) => m_AssetLoadStates[index] = true;



        /// <summary>
        /// index 加载完成，执行单个资源回调函数，跟回调进度（100%）
        /// </summary>
        /// <param name="index"></param>
        /// <param name="uObj"></param>
        internal void InvokeComplete(int index,UnityObject uObj)
        {
			string pathOrAddress = m_PathOrAddresses[index];
			Utility.DebugUtility.LogVerboseMemory("Asset", $"Complete({pathOrAddress})", false);
			ProgressCallback?.Invoke(pathOrAddress, 1.0f, m_UserData);
            CompleteCallback?.Invoke(pathOrAddress, uObj, m_UserData);
        }


        /// <summary>
        /// index加载进度更新，执行回调
        /// </summary>
        /// <param name="index"></param>
        /// <param name="progress"></param>
        internal void InvokeProgress(int index, float progress) => ProgressCallback?.Invoke(m_PathOrAddresses[index], progress, m_UserData);


        /// <summary>
        /// 全部完成，执行回调
        /// </summary>
        /// <param name="uObjs"></param>
        internal void InvokeBatchComplete(UnityObject[] uObjs) => BatchCompleteCallback?.Invoke(m_PathOrAddresses, uObjs, m_UserData);


        /// <summary>
        /// 全部加载进度回调
        /// </summary>
        /// <param name="progresses"></param>
        internal void InvokeBatchProgress(float[] progresses) => BatchProgressCallback?.Invoke(m_PathOrAddresses, progresses, m_UserData);


        /// <summary>
        /// 取消加载任务
        /// </summary>
        internal void CancelLoader()
        {
            CompleteCallback = null;
            ProgressCallback = null;
            BatchCompleteCallback = null;
            BatchProgressCallback = null;
            m_UserData = null;
        }

        /// <summary>
        /// 创建
        /// </summary>
        public void OnNew() { }

        /// <summary>
        /// 释放
        /// </summary>
        public void OnRelease()
        {
            CancelLoader();
            m_UniqueID = -1;
            m_AssetPaths = null;
            m_IsInstance = false;
            m_AssetLoadStates = null;
        }
    }
}
