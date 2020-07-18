public class WorldMissionVO
{
    public enum WorldStatus
    {
        /// <summary>
        /// 不开放
        /// </summary>
        NotOpen = 0,

        /// <summary>
        /// 可激活
        /// </summary>
        CanActivate = 1,

        /// <summary>
        /// 被激活 召集玩家中
        /// </summary>
        Convene = 2,

        /// <summary>
        /// 战斗中
        /// </summary>
        InWar = 3,

        /// <summary>
        /// 战斗结束CD了
        /// </summary>
        Cooling = 4,
    }

    /// <summary>
    /// UID
    /// </summary>
    public ulong UID;

    /// <summary>
    /// 战场ID
    /// </summary>
    public int BattleId;

    /// <summary>
    /// 任务ID
    /// </summary>
    public int MissionId;

    /// <summary>
    /// 状态
    /// </summary>
    public WorldStatus Status;

    /// <summary>
    /// CD
    /// </summary>
    public uint CD;
}