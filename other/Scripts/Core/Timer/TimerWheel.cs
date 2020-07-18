using System.Collections.Generic;

namespace Leyoutech.Core.Timer
{
    internal delegate void OnTimerWheelTrigger(int index, List<TimerTask> taskList);
    internal delegate void OnTimerWheelOut(int index);
    /// <summary>
    /// 时间轮计时器实现
    /// </summary>
    internal sealed class TimerWheel
    {
        private int m_Index = 0;//索引
        private int m_TickInMS = 0;//时间间隔
        private int m_SlotSize = 0;//时间刻度数量

        private int m_CurrentSlotIndex = 0;//当下执行的刻度
        private List<TimerTask>[] m_SlotArr = null;//对应刻度上任务
        private List<TimerTask> m_WillTriggerTaskList = new List<TimerTask>();//将会执行的任务列表
        internal OnTimerWheelTrigger wheelTriggerEvent = null;//触发回调
        internal OnTimerWheelOut wheelOutEvent = null;//执行完成一周后回调

        internal int TickInMS
        {
            get { return m_TickInMS; }
        }
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="index">索引</param>
        /// <param name="tickInMS">时间间隔</param>
        /// <param name="slotSize">时间刻度量</param>
        internal TimerWheel(int index, int tickInMS, int slotSize)
        {
            this.m_Index = index;
            this.m_TickInMS = tickInMS;
            this.m_SlotSize = slotSize;

            m_SlotArr = new List<TimerTask>[slotSize];
        }
        /// <summary>
        /// 添加定时任务，如果能添加到当前时间轮上则返回true，否则为False
        /// </summary>
        /// <param name="task">定时任务</param>
        /// <param name="slotIndex">任务当前位置</param>
        /// <param name="taskListIndex">在列表中位置</param>
        /// <returns></returns>
        internal bool AddTimerTask(TimerTask task, ref int slotIndex, ref int taskListIndex)
        {
            if (task.RemainingWheelInMS >= m_SlotSize * m_TickInMS)
            {
                slotIndex = -1;
                taskListIndex = -1;
                return false;
            }

            int targetSlot = task.RemainingWheelInMS / m_TickInMS;
            if (targetSlot == 0)
            {
                targetSlot = 1;
                task.RemainingWheelInMS = 0;
            }
            else
            {
                task.RemainingWheelInMS = task.RemainingWheelInMS % m_TickInMS;
            }

            slotIndex = m_CurrentSlotIndex + targetSlot;
            slotIndex = slotIndex % m_SlotSize;
            if (m_SlotArr[slotIndex] == null)
            {
                m_SlotArr[slotIndex] = new List<TimerTask>();
            }
            taskListIndex = m_SlotArr[slotIndex].Count;
            m_SlotArr[slotIndex].Add(task);

            return true;
        }
        /// <summary>
        /// 删除指定刻度，指定位置上的定时任务
        /// </summary>
        /// <param name="slotIndex"></param>
        /// <param name="taskListIndex"></param>
        /// <returns></returns>
        internal bool RemoveTimerTask(int slotIndex, int taskListIndex)
        {
            List<TimerTask> taskList = m_SlotArr[slotIndex];
            if (taskList != null && taskListIndex >= 0 && taskListIndex < taskList.Count)
            {
                taskList.RemoveAt(taskListIndex);
                return true;
            }
            return false;
        }
        /// <summary>
        /// 推动时间轮执行
        /// </summary>
        /// <param name="turnNum">执行的刻度量</param>
        internal void DoTimerTurn(int turnNum)
        {
            for (int i = 0; i < turnNum; i++)
            {
                m_CurrentSlotIndex++;
                if (m_CurrentSlotIndex == m_SlotSize)
                {
                    m_CurrentSlotIndex = 0;
                    if (wheelOutEvent != null)
                    {
                        wheelOutEvent(m_Index);
                    }
                }
                if (m_SlotArr[m_CurrentSlotIndex] != null)
                {
                    m_WillTriggerTaskList.AddRange(m_SlotArr[m_CurrentSlotIndex]);
                    m_SlotArr[m_CurrentSlotIndex].Clear();
                }
            }

            if (m_WillTriggerTaskList.Count > 0)
            {
                if (wheelTriggerEvent != null)
                {
                    wheelTriggerEvent(m_Index, m_WillTriggerTaskList);
                }
                m_WillTriggerTaskList.Clear();
            }
        }
    }
}