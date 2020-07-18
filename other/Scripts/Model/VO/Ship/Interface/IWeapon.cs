using Eternity.FlatBuffer;

public interface IWeapon : IShipItemBase
{
	/// <summary>
	/// 位置
	/// </summary>
	/// <returns></returns>
	int GetPos();

	/// <summary>
	/// 等级
	/// </summary>
	/// <returns></returns>
	int GetLv();

	/// <summary>
	/// 基础数据
	/// </summary>
	/// <returns></returns>
	Item GetBaseConfig();

	/// <summary>
	/// 武器数据
	/// </summary>
	/// <returns></returns>
	Weapon GetConfig();

	/// <summary>
	/// 通用mod包
	/// </summary>
	/// <returns></returns>
	IModContainer GetGeneralModContainer();

	/// <summary>
	/// 专属mod包
	/// </summary>
	/// <returns></returns>
	IModContainer GetExclusivelyModContainer();
}