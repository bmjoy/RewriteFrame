using UnityEngine;

namespace Leyoutech.Core.Timer
{
    /// <summary>
    /// 添加的定时任务的数据
    /// </summary>
    internal class TimerTask
    {
        internal int Index { get; set; }//唯一标识
        internal int IntervalInMS { get; set; }//触发间隔，以毫秒为单位
        internal int RemainingWheelInMS{ get; set; }//剩余时间，以毫秒为单位

        private int m_TotalInMS = 0;
        private TimerCallback m_OnStartEvent = null;
        private TimerCallback m_OnIntervalEvent = null;
        private TimerCallback m_OnEndEvent = null;
        private object m_UserData = null;

        private int m_LeftInMS = 0;

        internal TimerTask()
        {

        }
        /// <summary>
        /// 重置数据
        /// </summary>
        /// <param name="intervalInSec">触发间隔，以秒为单位</param>
        /// <param name="totalInSec">总时长，秒为单位</param>
        /// <param name="startCallback">开始触发时回调</param>
        /// <param name="intervalCallback">每次触发时回调</param>
        /// <param name="endCallback">定时结束时回调</param>
        /// <param name="callbackData">携带的格外参数</param>
        internal void OnReused(float intervalInSec,
                                                float totalInSec,
                                                TimerCallback startCallback,
                                                TimerCallback intervalCallback,
                                                TimerCallback endCallback,
                                                object callbackData)
        {
            this.IntervalInMS = Mathf.CeilToInt(intervalInSec * 1000);
            if (totalInSec <= 0)
            {
                m_TotalInMS = 0;
            }
            else
            {
                this.m_TotalInMS = Mathf.CeilToInt(totalInSec * 1000);
            }
            m_OnStartEvent = startCallback;
            m_OnIntervalEvent = intervalCallback;
            m_OnEndEvent = endCallback;
            m_UserData = callbackData;

            RemainingWheelInMS = IntervalInMS;
            m_LeftInMS = m_TotalInMS;
        }
        /// <summary>
        /// 判断定时任务是否还有效
        /// </summary>
        /// <returns></returns>
        internal bool IsValidTask()
        {
            if (IntervalInMS <= 0)
            {
                return false;
            }
            if (m_TotalInMS == 0)
            {
                return true;
            }
            else if (m_TotalInMS > 0)
            {
                return m_LeftInMS > 0;
            }
            return false;
        }
        /// <summary>
        /// 定时任务开始触发
        /// </summary>
        internal void OnTaskStart()
        {
            if (m_OnStartEvent != null)
            {
                m_OnStartEvent(m_UserData);
            }
        }
        /// <summary>
        /// 时间轮触发任务，进行时间的处理
        /// </summary>
        internal void OnTrigger()
        {
            if (m_TotalInMS > 0)
            {
                m_LeftInMS -= IntervalInMS;
            }

            if (m_OnIntervalEvent != null)
            {
                m_OnIntervalEvent(m_UserData);
            }

            if (m_TotalInMS == 0 || m_LeftInMS > 0)
            {
                if (m_TotalInMS == 0 || m_LeftInMS >= IntervalInMS)
                {
                    RemainingWheelInMS = IntervalInMS;
                }
                else
                {
                    RemainingWheelInMS = m_LeftInMS;
                }
            }
            else
            {
                if (m_OnEndEvent != null)
                {
                    m_OnEndEvent(m_UserData);
                }
            }
        }
        /// <summary>
        /// 清理数据
        /// </summary>
        internal void OnClear()
        {
            IntervalInMS = 0; ;
            m_TotalInMS = 0;
            RemainingWheelInMS = 0;
            m_LeftInMS = 0;
            m_OnStartEvent = null;
            m_OnIntervalEvent = null;
            m_OnEndEvent = null;
            m_UserData = null;
        }
    }
}
