/*===============================
 * Author: [Allen]
 * Purpose: 目标选择顶部角色信息
 * Time: 2019/4/1 16:24:03
================================*/

/// <summary>
/// 目标选择顶部角色信息
/// </summary>
public class SelectTargetTopRoleInfoVO
{
	/// <summary>
	///  1 NPC  2 玩家
	/// </summary>
	public int IsNpc;

	/// <summary>
	/// //1人形态，2 船形态
	/// </summary>
	public int IsHuman;

	/// <summary>
	/// 玩家ID
	/// </summary>
	public ulong ID;

	/// <summary>
	/// 玩家名称
	/// </summary>
	public string Name;

	/// <summary>
	/// 玩家等级
	/// </summary>
	public int Level;

	/// <summary>
	/// 血量
	/// </summary>
	public long Hp { get; set; }

	/// <summary>
	/// 血量上限
	/// </summary>
	public long HpMax;

	/// <summary>
	/// Mp值
	/// </summary>
	public long Mp;

	/// <summary>
	/// MP上限
	/// </summary>
	public long MpMax;
}