namespace Leyoutech.Core.Pool
{
    /// <summary>
    /// 对象池接口
    /// </summary>
    public interface IObjectPoolItem
    {
        /// <summary>
        /// 新元素被创建时调用
        /// </summary>
        void OnNew();

        /// <summary>
        /// 元素回收时被调用
        /// </summary>
        void OnRelease();
    }
}
