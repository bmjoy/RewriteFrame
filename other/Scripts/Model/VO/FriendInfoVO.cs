/// <summary>
/// 好友信息
/// </summary>
public class FriendInfoVO
{
	/// <summary>
	/// 玩家ID
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
	/// 名称
	/// </summary>
	public string Name;

	/// <summary>
	/// 备注
	/// </summary>
	public string SName;

	/// <summary>
	/// 等级
	/// </summary>
	public int Level;

	/// <summary>
	/// 玩家段位等级
	/// </summary>
	public int DanLevel;

	/// <summary>
	/// 状态
	/// </summary>
	public FriendState Status;

	/// <summary>
	/// 好友类型
	/// </summary>
	public FriendType Type;

	/// <summary>
	/// 添加时间
	/// </summary>
	public ulong AddTime;

	/// <summary>
	/// 上次下线时间
	/// </summary>
	public ulong OfflineTime;


	/// <summary>
	/// 好友状态
	/// </summary>
	public enum FriendState
	{
		/// <summary>
		/// 离线
		/// </summary>
		LEAVE = 0,

		/// <summary>
		/// 在线
		/// </summary>
		ONLINE = 1,

		/// <summary>
		/// 繁忙
		/// </summary>
		BUSY = 2
	}

	/// <summary>
	/// 好友类型
	/// </summary>
	public enum FriendType
	{
		/// <summary>
		/// 好友
		/// </summary>
		Friend = 0,

		/// <summary>
		/// 拉黑
		/// </summary>
		Black = 1,

		/// <summary>
		/// 陌生
		/// </summary>
		Strange = 2
	}
}

