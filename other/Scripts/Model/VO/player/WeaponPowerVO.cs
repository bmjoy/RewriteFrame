public class WeaponPowerVO
{
	/// <summary>
	/// 武器的物品UID
	/// </summary>
	public ulong WeaponUID;

	/// <summary>
	/// 当前能量
	/// </summary>
	public int CurrentValue;

	/// <summary>
	/// 能量上限
	/// </summary>
	public int MaxValue;

	/// <summary>
	/// 安全阀值（激光类武器过热后，必须降到此值以下才能继续使用）
	/// </summary>
	public int SafeValue;

	/// <summary>
	/// 是否处于强制冷却中（激光武器过热后，为true）
	/// </summary>
	public bool ForceCooldown;

}