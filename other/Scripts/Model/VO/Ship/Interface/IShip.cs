using Eternity.FlatBuffer;
using Eternity.Runtime.Item;

public interface IShip : IShipItemBase
{
	/// <summary>
	/// 基础数据
	/// </summary>
	/// <returns></returns>
	Item GetBaseConfig();

	/// <summary>
	/// 船数据
	/// </summary>
	/// <returns></returns>
	Warship GetConfig();

	/// <summary>
	/// 等级
	/// </summary>
	/// <returns></returns>
	int GetLv();

	/// <summary>
	/// 经验
	/// </summary>
	/// <returns></returns>
	int GetExp();
    /// <summary>
    /// 生成时间
    /// </summary>
    /// <returns></returns>
    ulong GetCreatTime();
    /// <summary>
    /// 类型
    /// </summary>
    /// <returns></returns>
    WarshipL1 GetWarShipType();
    /// <summary>
    /// 通过uid查找船上的一个部件
    /// </summary>
    /// <param name="uid"></param>
    /// <returns></returns>
    IShipItemBase GetItem(ulong uid);

	/// <summary>
	/// 武器包
	/// </summary>
	/// <returns></returns>
	IWeaponContainer GetWeaponContainer();

	/// <summary>
	/// 转化炉包
	/// </summary>
	/// <returns></returns>
	IReformerContainer GetReformerContainer();

	/// <summary>
	/// 装备包
	/// </summary>
	/// <returns></returns>
	IEquipmentContainer GetEquipmentContainer();

	/// <summary>
	/// 船上的通用mod包
	/// </summary>
	/// <returns></returns>
	IModContainer GetGeneralModContainer();

	/// <summary>
	/// 船上的专属mod包
	/// </summary>
	/// <returns></returns>
	IModContainer GetExclusivelyModContainer();

	/// <summary>
	/// 船本身的技能包
	/// </summary>
	/// <returns></returns>
	ISkillContainer GetSkillContainer();
}