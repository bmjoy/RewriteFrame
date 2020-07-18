public interface IWeaponContainer : IShipContainerBase
{
	/// <summary>
	/// 获取武器
	/// </summary>
	/// <returns></returns>
	IWeapon[] GetWeapons();
}
