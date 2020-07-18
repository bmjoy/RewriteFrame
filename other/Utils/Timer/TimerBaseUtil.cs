using System;
using System.Collections.Generic;
using UnityEngine;

namespace Utils.Timer
{
	public abstract class TimerBaseUtil<T> : Singleton<T> where T : MonoBehaviour
    {
		/// <summary>
		/// 恒值
		/// </summary>
		public const int EVERLASTING = -1;

        private uint m_TimerId = 0;

		/// <summary>
		/// 时间数据
		/// </summary>
        public class TimerData
        {
            /// <summary>
            /// 名字唯一标识
            /// </summary>
            public uint TimerId;
			/// <summary>
			/// 间隔时间
			/// </summary>
			public float Interval;
			/// <summary>
			/// 更新次数
			/// </summary>
			public int OnUpdateTimes;
			/// <summary>
			/// 更新回调
			/// </summary>
            public Action<int> OnUpdate;
			/// <summary>
			/// 下次更新时间
			/// </summary>
            public double NextUpdateTime;
			/// <summary>
			/// 剩余时间
			/// </summary>
            public int RemainingTimes;
			/// <summary>
			/// 更新是否恒定
			/// </summary>
			/// <returns></returns>
            public bool IsEverlasting()
            {
                return OnUpdateTimes == EVERLASTING;
            }
			/// <summary>
			/// 是否可用
			/// </summary>
			/// <returns></returns>
            public bool IsAvailable()
            {
                return IsEverlasting() || RemainingTimes > 0;
            }
        }

        private uint GetTimerId()
        {
            return ++m_TimerId;
        }

        /// <summary>
        /// TimerData比较器 用于SortedSet排序
        /// </summary>
        private class TimerDataComparer : IComparer<TimerData>
        {
            public int Compare(TimerData x, TimerData y)
            {
                if (x.TimerId > y.TimerId)
                {
                    return 1;
                }
                else if (x.TimerId < y.TimerId)
                {
                    return -1;
                }

                return 0;
            }
        }

        /// <summary>
        /// 遍历Timer标识
        /// </summary>
        private bool m_IsDispatching = false;

        /// <summary>
        /// 遍历的Timer集合
        /// </summary>
        private SortedSet<TimerData> m_TimerDatas = new SortedSet<TimerData>(new TimerDataComparer());

        /// <summary>
        /// 将要添加的Timer列表
        /// </summary>
        private List<TimerData> m_WatieAddTimerDatas = new List<TimerData>();

        /// <summary>
        /// 将要删除Timer列表
        /// </summary>
        private List<TimerData> m_WatieRemoveTimerDatas = new List<TimerData>();

        /// <summary>
        /// 将要删除Tiner集合
        /// </summary>
        private HashSet<uint> m_WatieRemoveTimerIds = new HashSet<uint>();

        /// <summary>
        /// 获取Timer
        /// </summary>
        /// <param name="timerId"></param>
        /// <returns></returns>
        public TimerData GetTimer(uint timerId)
        {
            foreach (var timer in m_TimerDatas)
            {
                if (timer.TimerId == timerId)
                {
                    return timer;
                }
            }

            return null;
        }

        /// <summary>
        /// 刷新下次更新时间
        /// </summary>
        public void RefreshNextUpdateTime(uint timerId)
        {
            TimerData timerData = GetTimer(timerId);
            if (timerData != null)
            {
                timerData.NextUpdateTime = GetNow() + timerData.Interval;
            }
        }

        /// <summary>
        /// 注册定时器
        /// </summary>
        /// <param name="key"></param>
        /// <param name="interval">间隔时间</param>
        /// <param name="onUpdateTimes">更新次数</param>
        /// <param name="onUpdate">更新回调</param>
        public uint Register(float interval, int onUpdateTimes, Action<int> onUpdate)
        {
            return Register(0, interval, onUpdateTimes, onUpdate);
        }

        /// <summary>
        /// 注册定时器
        /// </summary>
        /// <param name="key"></param>
        /// <param name="delay">开始延迟时间</param>
        /// <param name="interval">间隔时间</param>
        /// <param name="onUpdateTimes">更新次数</param>
        /// <param name="onUpdate">更新回调</param>
        public uint Register(float delay, float interval, int onUpdateTimes, Action<int> onUpdate)
        {
            uint timerId = GetTimerId();

            TimerData timerData = new TimerData()
            {
                TimerId = timerId,
                Interval = interval,
                OnUpdateTimes = onUpdateTimes,
                OnUpdate = onUpdate,
                NextUpdateTime = GetNow() + delay + interval,
                RemainingTimes = onUpdateTimes,
            };

            if (m_IsDispatching)
            {
                m_WatieAddTimerDatas.Add(timerData);
            }
            else
            {
                /// 删除待删除列表里的
                if (m_WatieRemoveTimerIds.Contains(timerId))
                {
                    m_WatieRemoveTimerIds.Remove(timerId);
                    foreach (var item in m_TimerDatas)
                    {
                        if (item.TimerId == timerId)
                        {
                            m_TimerDatas.Remove(item);
                            break;
                        }
                    }
                }
                m_TimerDatas.Add(timerData);
            }

            return timerId;
        }

        /// <summary>
        /// 反注册定时器
        /// </summary>
        /// <param name="key">key</param>
        public void Unregister(uint timerId)
        {
            if (m_IsDispatching)
            {
                m_WatieRemoveTimerIds.Add(timerId);
            }
            else
            {
                foreach (var item in m_TimerDatas)
                {
                    if (item.TimerId == timerId)
                    {
                        m_TimerDatas.Remove(item);
                        break;
                    }
                }

            }
        }

        /// <summary>
        /// 删除成员
        /// </summary>
        /// <param name="key">key</param>
        public void Clear(string key)
        {
            m_TimerDatas.Clear();
            m_WatieAddTimerDatas.Clear();
            m_WatieRemoveTimerDatas.Clear();
            m_WatieRemoveTimerIds.Clear();
        }

        /// <summary>
		/// 子类调用
		/// </summary>
		protected virtual void TimerUpdate()
        {
            double now = GetNow();

            foreach (var timerData in m_WatieAddTimerDatas)
            {
                m_TimerDatas.Add(timerData);
            }

            m_IsDispatching = true;

            foreach (var timerData in m_TimerDatas)
            {
                if (m_WatieRemoveTimerIds.Contains(timerData.TimerId))
                {
                    m_WatieRemoveTimerDatas.Add(timerData);
                }
                else
                {
                    if (now >= timerData.NextUpdateTime && timerData.IsAvailable())
                    {
                        timerData.NextUpdateTime += timerData.Interval;
                        if (!timerData.IsEverlasting())
                        {
                            timerData.RemainingTimes--;
                        }

                        timerData.OnUpdate?.Invoke(timerData.OnUpdateTimes - timerData.RemainingTimes);
                    }
                }

                if (timerData.RemainingTimes == 0)
                {
                    m_WatieRemoveTimerDatas.Add(timerData);
                }
            }

            m_IsDispatching = false;

            foreach (var timerData in m_WatieRemoveTimerDatas)
            {
                m_TimerDatas.Remove(timerData);
            }

            m_WatieRemoveTimerDatas.Clear();
        }

        /// <summary>
        /// 获取当前时间（单位为秒）
        /// </summary>
        /// <returns>当前时间</returns>
        protected abstract double GetNow();

    }
}
