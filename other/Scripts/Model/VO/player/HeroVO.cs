using Assets.Scripts.Define;
using System.Collections.Generic;

public class HeroVO
{
	/// <summary>
	/// 基本属性列表
	/// </summary>
	public PlayerAccountVO Properties = new PlayerAccountVO();

	/// <summary>
	/// 扩展属性列表
	/// </summary>
	public Dictionary<AircraftAttributeType, long> Attributes = new Dictionary<AircraftAttributeType, long>();

	/// <summary>
	/// Buff列表
	/// </summary>
	public Dictionary<uint, BuffVO> Buffs = new Dictionary<uint, BuffVO>();

	/// <summary>
	/// 当前档位
	/// </summary>
	public int Gear;

	/// <summary>
	/// 最高档位
	/// </summary>
	public int GearForwardCount;

	/// <summary>
	/// 最小档位
	/// </summary>
	public int GearFallbackCount;

	/// <summary>
	/// 当前姿态索引
	/// </summary>
	public int PoseIndex = 0;

	/// <summary>
	/// 姿态总个数
	/// </summary>
	public int PoseCount = 1;
}