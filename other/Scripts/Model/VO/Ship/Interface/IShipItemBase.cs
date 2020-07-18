public interface IShipItemBase
{
	/// <summary>
	/// UID
	/// </summary>
	/// <returns></returns>
	ulong GetUID();

	/// <summary>
	/// TID
	/// </summary>
	/// <returns></returns>
	uint GetTID();

	/// <summary>
	/// 引用的道具uid
	/// </summary>
	/// <returns></returns>
	ulong GetReference();
}