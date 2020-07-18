
namespace Leyoutech.Core.Util
{
    /// <summary>
    /// 单例类型
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class Singleton <T> where T :Singleton<T>, new()
    {
        private static T m_Instance = null;
        public static T GetInstance()
        {
            if(m_Instance == null)
            {
                m_Instance = new T();
                m_Instance.DoInit();
            }
            return m_Instance;
        }

        /// <summary>
        /// 初始化时会被调用
        /// </summary>
        protected virtual void DoInit()
        {
        }

        /// <summary>
        /// 可以通过实现此函数来重置数据
        /// </summary>
        public virtual void DoReset()
        {

        }
        /// <summary>
        /// 销毁单例
        /// </summary>
        public virtual void DoDispose()
        {
            m_Instance = null;
        }
    }
}

