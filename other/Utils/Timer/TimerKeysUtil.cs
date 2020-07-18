namespace Utils.Timer
{
    public enum TimerName
    {
        TIMER_NAME_BEGIN,
        TIMER_KEY_SYNC_SERVER,
        TIMER_KEY_CHANGE_HERO_STATE,
        TIMER_KEY_ON_HUMAN_INPUT_SAMPLE,
        TIMER_DELAY_FREEZE_ROTATION,
    }


    public static class TimerKeysUtil
    {

        /// <summary>
        /// 时间key 同步服务器
        /// </summary>
        public const string TIMER_KEY_SYNC_SERVER = "TIMER_KEY_SYNC_SERVER";
		/// <summary>
		/// 时间Key 玩家状态改变
		/// </summary>
        public const string TIMER_KEY_CHANGE_HERO_STATE = "TIMER_KEY_HERO_STATE_CHANGE";
        /// <summary>
        /// 时间Key 玩家输入采样
        /// </summary>
        public const string TIMER_KEY_ON_HUMAN_INPUT_SAMPLE = "TIMER_KEY_ON_HUMAN_INPUT_SAMPLE";
    }
}
