using Leyoutech.Core.Util;

namespace Leyoutech.Core.Timer
{
    public delegate void TimerCallback(object obj);
    /// <summary>
    /// 多层时间轮管理器
    /// </summary>
    public class TimerManager : Util.Singleton<TimerManager>
    {
        private HierarchicalTimerWheel m_HTimerWheel = null;
        private bool m_IsPause = false;

        protected override void DoInit()
        {
            m_HTimerWheel = new HierarchicalTimerWheel();
        }

        public override void DoReset()
        {
            m_HTimerWheel?.Clear();
            m_IsPause = false;
        }
        /// <summary>
        /// 通过Update来推动时间轮
        /// </summary>
        /// <param name="deltaTime">流逝的时长</param>
        public void DoUpdate(float deltaTime)
        {
            if (!m_IsPause && m_HTimerWheel != null)
            {
                m_HTimerWheel.OnUpdate(deltaTime);
            }
        }
        /// <summary>
        /// 暂停时间轮，暂停后所有的添加的任务都将不再执行
        /// </summary>
        public void Pause()
        {
            m_IsPause = true;
        }
        /// <summary>
        /// 恢复时间轮的运行
        /// </summary>
        public void Resume()
        {
            m_IsPause = false;
        }
        /// <summary>
        /// 添加定时任务
        /// </summary>
        /// <param name="intervalInSec">触发时间间隔</param>
        /// <param name="totalInSec">总时长</param>
        /// <param name="startCallback">开始触发时回调</param>
        /// <param name="intervalCallback">每次时间间隔的回调</param>
        /// <param name="endCallback">定时结束时回调</param>
        /// <param name="callbackData">格外的参数</param>
        /// <returns></returns>
        public TimerTaskInfo AddTimer(float intervalInSec,
                                                float totalInSec,
                                                TimerCallback startCallback,
                                                TimerCallback intervalCallback,
                                                TimerCallback endCallback,
                                                object callbackData)
        {
            if (m_HTimerWheel == null) return null;

            TimerTask task = m_HTimerWheel.GetIdleTimerTask();
            task.OnReused(intervalInSec, totalInSec, startCallback, intervalCallback, endCallback, callbackData);
            return m_HTimerWheel.AddTimerTask(task);
        }
        /// <summary>
        /// 添加定时任务
        /// </summary>
        /// <param name="intervalInSec">触发时间间隔</param>
        /// <param name="totalInSec">总时长</param>
        /// <param name="intervalCallback">每次时间间隔的回调</param>
        /// <param name="endCallback">定时结束时回调</param>
        /// <param name="callbackData">格外的参数</param>
        /// <returns></returns>
        public TimerTaskInfo AddTimer(float intervalInSec,
                                                float totalInSec,
                                                TimerCallback intervalCallback,
                                                TimerCallback endCallback,
                                                object callbackData = null)
        {
            return AddTimer(intervalInSec, totalInSec, null, intervalCallback, endCallback, callbackData);
        }
        /// <summary>
        /// 添加按固定频率触发的定时任务，此方法添加的任务为永久性任务
        /// </summary>
        /// <param name="intervalInSec">触发时间间隔</param>
        /// <param name="intervalCallback">每次时间间隔的回调</param>
        /// <param name="callbackData">格外的参数</param>
        /// <returns></returns>
        public TimerTaskInfo AddIntervalTimer(float intervalInSec, TimerCallback intervalCallback, object callbackData = null)
        {
            return AddTimer(intervalInSec, 0f, null, intervalCallback, null, callbackData);
        }
        /// <summary>
        /// 添加一次性触发定时任务
        /// </summary>
        /// <param name="totalInSec">总时长</param>
        /// <param name="endCallback">定时结束时回调</param>
        /// <param name="callbackData">格外的参数</param>
        /// <returns></returns>
        public TimerTaskInfo AddEndTimer(float totalInSec, TimerCallback endCallback,object callbackData = null)
        {
            return AddTimer(totalInSec, totalInSec, null, null, endCallback, callbackData);
        }
       /// <summary>
       /// 删除指定的定时任务
       /// </summary>
       /// <param name="taskInfo"></param>
       /// <returns></returns>
        public bool RemoveTimer(TimerTaskInfo taskInfo)
        {
            if (m_HTimerWheel != null)
            {
                return m_HTimerWheel.RemoveTimerTask(taskInfo);
            }
            return false;
        }
        /// <summary>
        /// 释放
        /// </summary>
        public override void DoDispose()
        {
            DoReset();
            m_HTimerWheel = null;
        }
    }
}
