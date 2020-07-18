using System.Collections.Generic;


namespace Leyoutech.Core.Timer
{
    /// <summary>
    /// 添加定时任务后返回数据结构，可用于删除定时任务
    /// </summary>
    public class TimerTaskInfo
    {
        internal int Index { get; set; }//唯一ID
        internal int WheelIndex { get; set; }//时间轮层级
        internal int WheelSlotIndex { get; set; }//时间轮位置
        internal int TaskListIndex { get; set; }//任务中索引

        internal TimerTaskInfo()
        {
        }

        internal void OnClear()
        {
            Index = -1;
            WheelIndex = -1;
            WheelSlotIndex = -1;
            TaskListIndex = -1;
        }

        internal bool IsClear()
        {
            return Index == -1;
        }

        public override string ToString()
        {
            return string.Format("TimerTaskInfo:{{index = {0},wheelIndex = {1},wheelSlotIndex = {2},taskListIndex = {3}, }}", Index, WheelIndex, WheelSlotIndex, TaskListIndex);
        }
    }
    /// <summary>
    /// 使用TimerWheel来实现多级时间轮
    /// </summary>
    internal sealed class HierarchicalTimerWheel
    {
        private TimerWheel[] m_WheelArr = new TimerWheel[4];
        private int m_TaskIndex = 0;
        private Dictionary<int, TimerTaskInfo> m_TaskInfoDic = new Dictionary<int, TimerTaskInfo>();
        private List<TimerTask> m_IdleTimerTaskList = new List<TimerTask>();

        private float m_LapseTime = 0; //seconds
        internal HierarchicalTimerWheel()
        {
            m_WheelArr[0] = new TimerWheel(0, 50, 20);
            m_WheelArr[1] = new TimerWheel(1, 1000, 60);
            m_WheelArr[2] = new TimerWheel(2, 60000, 60);
            m_WheelArr[3] = new TimerWheel(3, 3600000, 24);//4层时间轮，以每秒20帧速率进行运行
            for (int i = 0; i < m_WheelArr.Length; i++)
            {
                m_WheelArr[i].wheelTriggerEvent = OnTimerWheelTrigger;
                m_WheelArr[i].wheelOutEvent = OnTimerWheelOut;
            }
        }
        /// <summary>
        /// 时间流逝
        /// </summary>
        /// <param name="deltaTime"></param>
        internal void OnUpdate(float deltaTime)
        {
            m_LapseTime += deltaTime;
            int lTime = (int)(m_LapseTime * 1000);
            int turnNum = lTime / m_WheelArr[0].TickInMS;
            if (m_WheelArr[0] != null && m_TaskInfoDic.Count > 0)
            {
                if (turnNum > 0)
                {
                    m_WheelArr[0].DoTimerTurn(turnNum);
                }
            }
            m_LapseTime -= turnNum * m_WheelArr[0].TickInMS * 0.001f;
        }
        /// <summary>
        /// 添加定时任务
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        internal TimerTaskInfo AddTimerTask(TimerTask task)
        {
            TimerTaskInfo taskInfo = new TimerTaskInfo();
            if (AddTimerTask(task, taskInfo))
            {
                task.OnTaskStart();
                return taskInfo;
            }
            else
            {
                return null;
            }
        }
        /// <summary>
        /// 添加定时任务
        /// </summary>
        /// <param name="task"></param>
        /// <param name="taskInfo"></param>
        /// <returns></returns>
        private bool AddTimerTask(TimerTask task, TimerTaskInfo taskInfo)
        {
            if (!task.IsValidTask())
            {
                return false;
            }

            for (int i = 0; i < m_WheelArr.Length; i++)
            {
                int slotIndex = -1;
                int taskListIndex = -1;
                if (m_WheelArr[i].AddTimerTask(task, ref slotIndex, ref taskListIndex))
                {
                    taskInfo.WheelSlotIndex = slotIndex;
                    taskInfo.TaskListIndex = taskListIndex;
                    taskInfo.WheelIndex = i;
                    break;
                }
            }
            if (taskInfo.WheelIndex < 0 || taskInfo.WheelSlotIndex < 0 || taskInfo.TaskListIndex < 0)
            {
                return false;
            }
            m_TaskIndex++;
            task.Index = m_TaskIndex;
            taskInfo.Index = m_TaskIndex;
            m_TaskInfoDic.Add(taskInfo.Index, taskInfo);
            return true;
        }
        /// <summary>
        ///删除定时任务
        /// </summary>
        /// <param name="taskInfo"></param>
        /// <returns></returns>
        internal bool RemoveTimerTask(TimerTaskInfo taskInfo)
        {
            if (taskInfo == null || taskInfo.WheelIndex < 0 || taskInfo.WheelSlotIndex < 0 || taskInfo.TaskListIndex < 0)
            {
                return false;
            }
            if (!m_TaskInfoDic.ContainsKey(taskInfo.Index))
            {
                return false;
            }

            if (taskInfo.WheelIndex < 0 || taskInfo.WheelIndex >= m_WheelArr.Length || m_WheelArr[taskInfo.WheelIndex] == null)
            {
                return false;
            }

            m_TaskInfoDic.Remove(taskInfo.Index);

            int wheelIndex = taskInfo.WheelIndex;
            int wheelSlotIndex = taskInfo.WheelSlotIndex;
            int taskListIndex = taskInfo.TaskListIndex;
            taskInfo.OnClear();

            return m_WheelArr[wheelIndex].RemoveTimerTask(wheelSlotIndex, taskListIndex);
        }

        private void OnTimerWheelOut(int index)
        {
            if (index >= 0 && index < m_WheelArr.Length - 1)
            {
                m_WheelArr[index + 1].DoTimerTurn(1);
            }
        }
        /// <summary>
        /// 指定
        /// </summary>
        /// <param name="index"></param>
        /// <param name="taskList"></param>
        private void OnTimerWheelTrigger(int index, List<TimerTask> taskList)
        {
            for (int i = 0; i < taskList.Count; i++)
            {
                TimerTask task = taskList[i];
                if (task == null)
                {
                    continue;
                }
                TimerTaskInfo taskInfo = null;
                if (!m_TaskInfoDic.TryGetValue(task.Index, out taskInfo))
                {
                    continue;
                }
                if (task.RemainingWheelInMS == 0)
                {
                    task.OnTrigger();
                }

                if (taskInfo.IsClear())
                {
                    RecycleTimerTask(task);
                }
                else
                {
                    m_TaskInfoDic.Remove(task.Index);
                    taskInfo.OnClear();
                    if (task.IsValidTask())
                    {
                        AddTimerTask(task, taskInfo);
                    }
                    else
                    {
                        RecycleTimerTask(task);
                    }
                }
            }
        }
        /// <summary>
        /// 获取空闲的任务数据，将会被重复使用
        /// </summary>
        /// <returns></returns>
        internal TimerTask GetIdleTimerTask()
        {
            TimerTask task = null;
            if (m_IdleTimerTaskList.Count == 0)
            {
                task = new TimerTask();
            }
            else
            {
                task = m_IdleTimerTaskList[0];
                m_IdleTimerTaskList.RemoveAt(0);
            }
            return task;
        }
        /// <summary>
        /// 回收数据
        /// </summary>
        /// <param name="task"></param>
        private void RecycleTimerTask(TimerTask task)
        {
            if (task != null)
            {
                task.OnClear();
                m_IdleTimerTaskList.Add(task);
            }
        }

        internal void Clear()
        {
            if (m_TaskInfoDic.Count > 0)
            {
                List<int> keys = new List<int>(m_TaskInfoDic.Keys);
                for (int i = 0, imax = keys.Count; i < imax; ++i)
                {
                    RemoveTimerTask(m_TaskInfoDic[keys[i]]);
                }
            }    
        }
    }
}


