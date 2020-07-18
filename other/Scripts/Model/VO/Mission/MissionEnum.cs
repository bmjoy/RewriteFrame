/// <summary>
/// 任务类型
/// </summary>
public enum MissionType
{
	None,
	/// <summary>
	/// 主线
	/// </summary>
	Main = 1,
	/// <summary>
	/// 支线
	/// </summary>
	Branch = 2,
	/// <summary>
	/// 日常
	/// </summary>
	Daily = 3,
	/// <summary>
	/// 副本
	/// </summary>
	Dungeon = 4,
	/// <summary>
	/// 世界
	/// </summary>
	World = 5,
	/// <summary>
	/// 子任务
	/// </summary>
	Sub = 6
}

/// <summary>
/// 任务目标类型
/// </summary>
public enum MissionTargetType
{
	None,
	/// <summary>
	/// 杀怪
	/// </summary>
	Kill = 1,
	/// <summary>
	/// 对话
	/// </summary>
	Chat = 2,
	/// <summary>
	/// 上交道具
	/// </summary>
	CollectItem = 3,
	/// <summary>
	/// 拥有道具
	/// </summary>
	HaveItem = 4,
	/// <summary>
	/// 探索区域
	/// </summary>
	Search = 5,
	/// <summary>
	/// 护送npc
	/// </summary>
	Escort = 6,
	/// <summary>
	/// 装配武器
	/// </summary>
	UseEquip = 7,
	/// <summary>
	/// npc 死亡
	/// </summary>
	NPDDied = 8,
	/// <summary>
	/// 时间到达
	/// </summary>
	TimeOut = 9
}

/// <summary>
/// 任务状态
/// </summary>
public enum MissionState
{
	None,
	/// <summary>
	/// 可接
	/// </summary>
	CanAccept,
	/// <summary>
	/// 已接/进行中
	/// </summary>
	Going,
	/// <summary>
	/// 任务完成 未提交
	/// </summary>
	Finished,
	/// <summary>
	/// 失败
	/// </summary>
	Failed
}
public enum MissionIconID
{
	Error = 0,
	/// <summary>
	/// 主线icon
	/// </summary>
	MajorIconId = 40000,
	/// <summary>
	/// 日常icon
	/// </summary>
	DayDailyIconId = 40001,
	/// <summary>
	/// 支线icon
	/// </summary>
	ExtensionIconId = 40002,
	/// <summary>
	/// 世界icon
	/// </summary>
	WorldTaskIconId = 40003,
	/// <summary>
	/// 副本icon
	/// </summary>
	DuplicateTaskIconId = 40004

}