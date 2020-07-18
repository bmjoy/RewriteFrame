namespace Leyoutech.Core.Timeline
{
    /// <summary>
    /// 瞬间触发的Action基类
    /// </summary>
    public abstract class AEventActionItem: AActionItem
    {
        /// <summary>
        /// 触发
        /// </summary>
        public abstract void Trigger();
    }
}
