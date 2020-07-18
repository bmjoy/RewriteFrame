namespace Leyoutech.Core.Timeline
{
    /// <summary>
    /// 持续一段时间的Action基类
    /// </summary>
    public abstract class ADurationActionItem : AActionItem
    {
        /// <summary>
        /// Action持续时长
        /// </summary>
        public float Duration { get; set; }

        /// <summary>
        /// 结束时间
        /// </summary>
        public float EndTime { get => FireTime + Duration; }

        /// <summary>
        /// 进入
        /// </summary>
        public abstract void DoEnter();

        /// <summary>
        /// 循环
        /// </summary>
        /// <param name="deltaTime"></param>
        public abstract void DoUpdate(float deltaTime);


        /// <summary>
        /// 退出
        /// </summary>
        public abstract void DoExit();


        /// <summary>
        /// 暂停
        /// </summary>
        public virtual void DoPause()
        {

        }

        /// <summary>
        /// 恢复
        /// </summary>
        public virtual void DoResume()
        {

        }
    }
}
