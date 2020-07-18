using Eternity.FlatBuffer;

public interface IEquipment : IShipItemBase
{
	/// <summary>
	/// 基础数据
	/// </summary>
	/// <returns></returns>
	Item GetBaseConfig();

	/// <summary>
	/// 装备数据
	/// </summary>
	/// <returns></returns>
	Equip GetConfig();

	/// <summary>
	/// 等级
	/// </summary>
	/// <returns></returns>
	int GetLv();

	/// <summary>
	/// 位置
	/// </summary>
	/// <returns></returns>
	int GetPos();
}