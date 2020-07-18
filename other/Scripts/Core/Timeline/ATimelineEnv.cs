using Leyoutech.Core.Context;

namespace Assets.Scripts.Core.Timeline
{
    /// <summary>
    /// 时间轴环境
    /// </summary>
    public abstract class ATimelineEnv
    {

        /// <summary>
        /// 上下文
        /// </summary>
        protected IContext m_Context;


        /// <summary>
        /// 设置环境
        /// </summary>
        /// <param name="context"></param>
        public virtual void SetEnv(IContext context)
        {
            m_Context = context;
        }


        /// <summary>
        /// 获取上下文
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetContext<T>() where T:IContext
        {
            return (T)m_Context;
        }


        /// <summary>
        /// 循环
        /// </summary>
        /// <param name="deltaTime"></param>
        public abstract void DoUpdate(float deltaTime);


        /// <summary>
        /// 销毁
        /// </summary>
        public virtual void DoDestroy()
        {
            m_Context = null;
        }
    }
}
