using Eternity.FlatBuffer;

public interface IReformer : IShipItemBase
{
	/// <summary>
	/// 基础数据
	/// </summary>
	/// <returns></returns>
	Item GetBaseConfig();

	/// <summary>
	/// 转化炉数据
	/// </summary>
	/// <returns></returns>
	Weapon GetConfig();

	/// <summary>
	/// lv
	/// </summary>
	/// <returns></returns>
	int GetLv();
}