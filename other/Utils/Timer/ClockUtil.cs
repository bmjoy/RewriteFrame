using System.Diagnostics;

namespace Utils.Timer
{
    public class ClockUtil
    {
		/// <summary>
		/// 单例
		/// </summary>
        private static ClockUtil s_Instance;
		/// <summary>
		/// 秒表
		/// </summary>
        private static Stopwatch s_Stopwatch;
		/// <summary>
		/// 是否开始
		/// </summary>
        private bool m_IsStart = false;

        private ClockUtil()
        {

        }
		/// <summary>
		/// 单例
		/// </summary>
		/// <returns></returns>
        public static ClockUtil Instance()
        {
			if (s_Instance == null)
			{
				s_Instance = new ClockUtil();
				s_Stopwatch = new Stopwatch();
				return s_Instance;
			}

			return s_Instance;
		}
		/// <summary>
		/// 秒表开始
		/// </summary>
		public void Start()
		{
			if (m_IsStart == false)
			{
				m_IsStart = true;
				s_Stopwatch.Start();
			}
		}
		/// <summary>
		/// 毫秒
		/// </summary>
		/// <returns></returns>
		public long GetMillisecond()
        {
            return s_Stopwatch.ElapsedMilliseconds;
        }
    }
}
