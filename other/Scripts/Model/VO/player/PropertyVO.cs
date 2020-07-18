using Assets.Scripts.Define;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAccountVO
{
	/// <summary>
	/// 玩家ID
	/// </summary>
	public ulong Id;

	/// <summary>
	/// 性别
	/// </summary>
	public int Gender;

	/// <summary>
	/// 服务器ID
	/// </summary>
	public int GroupID;

	/// <summary>
	/// 帐号名称
	/// </summary>
	public string AccountName;

	/// <summary>
	/// 玩家名称
	/// </summary>
	public string Nickname;

	/// <summary>
	/// 创建时间
	/// </summary>
	public int CreateTime;

	/// <summary>
	/// 最后一次登录时间
	/// </summary>
	public int LastLoginTime;

	/// <summary>
	/// 最后一次保存时间
	/// </summary>
	public int LastSaveTime;

	/// <summary>
	/// 总计游戏时间
	/// </summary>
	public int TotalGameTime;

	/// <summary>
	/// 服务器时间
	/// </summary>
	public int ServerTime;

	/// <summary>
	/// VIP经验
	/// </summary>
	public uint VipExp;

	/// <summary>
	/// VIP等级
	/// </summary>
	public uint VipLevel;

	/// <summary>
	/// VIP结束时间
	/// </summary>
	public uint VipEndTime;

	/// <summary>
	/// 阵营ID
	/// </summary>
	public uint CampID;

	/// <summary>
	/// 当前玩家经验
	/// </summary>
	public ulong PlayerExp;

	/// <summary>
	/// 当前玩家等级
	/// </summary>
	public uint PlayerLevel;

	/// <summary>
	/// 当前段位经验
	/// </summary>
	public ulong DanExp;

	/// <summary>
	/// 当前段位等级
	/// </summary>
	public uint DanLevel;

	/// <summary>
	/// 能量类型
	/// </summary>
	public KPowerType PowerType;

	/// <summary>
	/// key: 对方阵营, value: 当前单位对key的阵营的声望值
	/// 对应表camp_list.xlsx
	/// </summary>
	public Dictionary<int, int> CampPrestigeMap = new Dictionary<int, int>();
}
