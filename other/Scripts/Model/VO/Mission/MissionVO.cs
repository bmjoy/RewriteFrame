using Eternity.FlatBuffer;
using System.Collections.Generic;
/// <summary>
/// 任务数据结构
/// </summary>
public class MissionVO
{
	/// <summary>
	/// tid
	/// </summary>
	public uint Tid { get; set; }

	/// <summary>
	/// uid
	/// </summary>
	public ulong Uid { get; set; }

	/// <summary>
	/// 通用配置数据
	/// </summary>
	public MissionData MissionConfig { get; set; }

	/// <summary>
	/// 主线 支线任务的配置, 接交任务npc 章节 对话 奖励 等相关数据
	/// </summary>
	public MissionMain? MissionMainConfig { get; set; }

	/// <summary>
	/// 任务目标
	/// </summary>
	public SortedDictionary<uint, SortedDictionary<uint, MissionTargetVO>> MissionGroupTargetList { get; } = new SortedDictionary<uint, SortedDictionary<uint, MissionTargetVO>>();

	/// <summary>
	/// 任务类型
	/// </summary>
	public MissionType MissionType { get; set; }

	/// <summary>
	/// 任务状态
	/// </summary>
	public MissionState MissionState { get; set; }
}