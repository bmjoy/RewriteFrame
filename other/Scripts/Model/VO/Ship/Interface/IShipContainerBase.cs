public interface IShipContainerBase : IShipItemBase
{
	/// <summary>
	/// 当前最大容量
	/// </summary>
	/// <returns></returns>
	uint GetCurrentSizeMax();
}