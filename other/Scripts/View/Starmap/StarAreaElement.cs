using UnityEngine.Events;
/// <summary>
/// 星域item
/// </summary>
public class StarAreaElement : StarmapPointElementBase
{
	/// <summary>
	/// 当前星域数据
	/// </summary>
	private EditorStarMapArea m_Data;

	protected override void Initialize()
	{
		base.Initialize();
		SetToIcon();
	}

	/// <summary>
	/// 设置当前星域数据
	/// </summary>
	/// <param name="gamingmapId"></param>
	/// <param name="data"></param>
	/// <param name="isCurrentPoint"></param>
	public void SetData(uint gamingmapId, EditorStarMapArea data, bool isCurrentPoint)
	{
		Initialize();
		m_Data = data;

		SetName($"area_name_{gamingmapId}_{m_Data.areaId}");
		SetLocationVisible(isCurrentPoint);
		SetSelectState(isCurrentPoint);
	}

	/// <summary>
	/// 获取当前星域数据
	/// </summary>
	/// <returns></returns>
	public EditorStarMapArea GetData()
	{
		return m_Data;
	}
}