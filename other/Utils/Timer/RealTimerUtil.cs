using System;

namespace Utils.Timer
{
    public class RealTimerUtil : TimerBaseUtil<RealTimerUtil>
    {
		/// <summary>
		/// 获取当前时间（单位为秒）。
		/// </summary>
		/// <returns>当前时间。</returns>
		protected override double GetNow()
        {
            return ClockUtil.Instance().GetMillisecond() * 0.001;
        }

        private void Update()
        {
            TimerUpdate();
        }
    }
}
