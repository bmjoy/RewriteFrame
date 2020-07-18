using Eternity.FlatBuffer;

public interface IMod : IShipItemBase
{
	/// <summary>
	/// 基础数据
	/// </summary>
	/// <returns></returns>
	Item GetBaseConfig();

	/// <summary>
	/// mod数据
	/// </summary>
	/// <returns></returns>
	Mod GetConfig();

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
}