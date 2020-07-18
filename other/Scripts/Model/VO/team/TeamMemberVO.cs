public class TeamMemberVO
{
    /// <summary>
    /// 玩家UID
    /// </summary>
    public ulong UID;

	/// <summary>
	/// EnerityId
	/// </summary>
	public ulong EnerityId;

	/// <summary>
	/// 玩家TID
	/// </summary>
	public int TID;

    /// <summary>
    /// 玩家名称
    /// </summary>
    public string Name;

	/// <summary>
	/// 是否静音
	/// </summary>
	public bool IsMute;

    /// <summary>
    /// 玩家等级
    /// </summary>
    public int Level;

	/// <summary>
	/// 玩家段位等级
	/// </summary>
	public int DanLevel;

	/// <summary>
	/// 是否为队长
	/// </summary>
	public bool IsLeader;

	/// <summary>
	/// 是否在线
	/// </summary>
	public bool IsOnline;

    /// <summary>
    /// 玩家入队时间
    /// </summary>
    public ulong JoinTime;

	/// <summary>
	/// 当前血量
	/// </summary>
	public ulong HP;

	/// <summary>
	/// 当前护盾
	/// </summary>
	public ulong Defense;

	/// <summary>
	/// 最大血量
	/// </summary>
	public ulong MaxHP;

	/// <summary>
	/// 最大护盾
	/// </summary>
	public ulong MaxDefense;

	/// <summary>
	/// 是否死亡
	/// </summary>
	public bool IsDead;

    /// <summary>
    /// 区域ID
    /// </summary>
    public int AreaId;
    /// <summary>
    /// MapID
    /// </summary>
    public ulong MapID;
}