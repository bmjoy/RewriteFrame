using System.Linq;
using UnityObject = UnityEngine.Object;

namespace Leyoutech.Core.Loader
{
    /// <summary>
    /// 一次加载任务的状态句柄
    /// </summary>
    public sealed class AssetLoaderHandle
    {
        /// <summary>
        /// 唯一索引ID
        /// </summary>
        private long m_UniqueID;

        /// <summary>
        ///Path模式  此次任务路径/地址集合
        /// </summary>
        private string[] m_PathOrAddresses;

        /// <summary>
        /// 加载的所有资源Obj保存集合
        /// </summary>
        private UnityObject[] m_UObjs;

        /// <summary>
        /// 加载资源每个资源Obj 的进度
        /// </summary>
        private float[] m_Progresses;

        /// <summary>
        /// 获取唯一索引ID
        /// </summary>
        public long UniqueID { get => m_UniqueID; }

        /// <summary>
        /// 获取路径集合
        /// </summary>
        public string[] PathOrAddresses { get => m_PathOrAddresses; }

        /// <summary>
        /// 获取首个地址
        /// </summary>
        public string PathOrAddress { get => m_PathOrAddresses.Length>0?m_PathOrAddresses[0]:null; }

        /// <summary>
        /// 获取所有资源Obj 保存集合
        /// </summary>
        public UnityObject[] AssetObjects { get => m_UObjs; }

        /// <summary>
        /// 获取首个资源Obj 
        /// </summary>
        public UnityObject AssetObject { get => m_UObjs.Length > 0 ? m_UObjs[0] : null; }

        /// <summary>
        /// 获取所有资源进度集合
        /// </summary>
        public float[] AssetProgresses { get => m_Progresses; }

        /// <summary>
        /// 获取首个进度值
        /// </summary>
        public float AssetProgress { get => m_Progresses.Length > 0 ? m_Progresses[0] : 0.0f; }

        /// <summary>
        /// 加载状态
        /// </summary>
        internal AssetLoaderState state = AssetLoaderState.None;


        /// <summary>
        /// 平均进度
        /// </summary>
        public float TotalProgress
        {
            get
            {
                if (m_Progresses == null) return 0.0f;

                return m_Progresses.Sum((v) => v) / m_Progresses.Length;
            }
        }

        /// <summary>
        /// 加载状态
        /// </summary>
        public AssetLoaderState State { get => state; set => state = value; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="id">唯一标识ID</param>
        /// <param name="paths">资源路径集合</param>
        internal AssetLoaderHandle(long id, string[] paths)
        {
            m_UniqueID = id;
            m_PathOrAddresses = paths;

            m_UObjs = new UnityObject[paths.Length];
            m_Progresses = new float[paths.Length];
        }


        /// <summary>
        /// index 资源加载完成了
        /// </summary>
        /// <param name="index"></param>
        /// <param name="uObj"></param>
        internal void SetObject(int index,UnityObject uObj)
        {
            m_UObjs[index] = uObj;
        }

        /// <summary>
        /// 获取index 的Obj
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        internal UnityObject GetObject(int index)
        {
            return m_UObjs[index];
        }


        /// <summary>
        /// 设置第index 进度
        /// </summary>
        /// <param name="index"></param>
        /// <param name="progress"></param>
        internal void SetProgress(int index,float progress)
        {
            m_Progresses[index] = progress;
        }

        /// <summary>
        /// 获取index 的进度
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        internal float GetProgress(int index)
        {
            return m_Progresses[index];
        }

        /// <summary>
        /// 取消加载,销毁已经实例化的Obj
        /// </summary>
        /// <param name="destroyIfLoaded">s是否直接销毁</param>
        internal void CancelLoader(bool destroyIfLoaded)
        {
            State = AssetLoaderState.Cancel;

            if(destroyIfLoaded)
            {
                for(int i =0;i<m_UObjs.Length;++i)
                {
                    UnityObject uObj = m_UObjs[i];
                    if(uObj !=null)
                    {
                        UnityObject.Destroy(uObj);
                        m_UObjs[i] = null;
                    }
                }
            }
            m_UniqueID = -1;
            m_PathOrAddresses = null;
            m_UObjs = null;
            m_Progresses = null;
        }
    }
}
