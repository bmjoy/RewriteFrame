using Eternity.FlatBuffer;
/// <summary>
/// 任务目标数据
/// </summary>
public class MissionTargetVO
{
	/// <summary>
	/// tid
	/// </summary>
	public uint Tid;

	/// <summary>
	/// uid
	/// </summary>
	public ulong Uid;

	/// <summary>
	/// 任务目标数据
	/// </summary>
	public MissionTarget MissionTargetConfig;

	/// <summary>
	/// 任务类型
	/// </summary>
	public MissionTargetType MissionTargetType;

	/// <summary>
	/// 是否是正常判断条件
	/// true 是完成判断条件 -》完成
	/// false 是失败判断条件 -》失败
	/// </summary>
	public bool DoneToFinish;

	/// <summary>
	/// 是否在hud中显示
	/// </summary>
	public bool HudVisible;

	/// <summary>
	/// 任务值
	/// 不作为任务完成与否的判定条件
	/// </summary>
	public long Value;

	/// <summary>
	/// 任务状态
	/// 唯一的子任务判定完成度标准
	/// </summary>
	public MissionState TargetState;

	/// <summary>
	/// arg4 关联的另一个target
	/// 多用于展示某些 负面target条件
	/// </summary>
	public MissionTargetVO Relation;
}