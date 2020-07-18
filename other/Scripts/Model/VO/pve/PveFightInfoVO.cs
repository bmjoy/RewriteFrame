public class PveFightInfoVO
{
    /// <summary>
    /// 玩家ID
    /// </summary>
    public ulong PlayerID;

    /// <summary>
    /// 玩家名字
    /// </summary>
    public string PlayerName;

    /// <summary>
    /// 伤害输出量
    /// </summary>
    public uint DpsCount;
	/// <summary>
	/// 伤害输出百分比
	/// </summary>
	public float DpsPercent;

    /// <summary>
    /// 伤害量
    /// </summary>
    public uint HurtCount;
	/// <summary>
	/// 伤害百分比
	/// </summary>
	public float HurtPercent;

    /// <summary>
    /// 击杀数
    /// </summary>
    public uint KillCount;

    /// <summary>
    /// 死亡数
    /// </summary>
    public uint DeathCount;
}
