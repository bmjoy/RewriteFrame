using UnityEngine;
/// <summary>
/// 行星item
/// </summary>
public class StarPlantElement : StarmapPointElementBase
{
	/// <summary>
	/// 行星模型显示
	/// </summary>
	private Starmap3DViewer m_3DViewer;
	/// <summary>
	/// 行星数据
	/// </summary>
	private EditorPlanet m_Data;

	/// <summary>
	/// 设置行星数据
	/// </summary>
	/// <param name="data"></param>
	/// <param name="isCurrentPoint"></param>
	public void SetData(EditorPlanet data, bool isCurrentPoint, bool selected = false)
	{
		Initialize();
		m_Data = data;

		SetName($"gamingmap_name_{m_Data.gamingmapId}");
		SetLocationVisible(isCurrentPoint);
		SetSelectState(selected);
		SetModel(data.gamingmapRes);
	}
	/// <summary>
	/// 设置行星模型数据
	/// </summary>
	/// <param name="path"></param>
	private void SetModel(string path)
	{
		m_3DViewer = GetRawImage().GetOrAddComponent<Starmap3DViewer>();
		SetToRaw(m_Data.scale.ToVector2());
		m_3DViewer.SetModel(path, Vector3.zero, Vector3.zero, Vector3.one);
	}
	/// <summary>
	/// 获取当前行星数据
	/// </summary>
	/// <returns></returns>
	public EditorPlanet GetData()
	{
		return m_Data;
	}

	public override void Destroy()
	{
		m_3DViewer.SetModel(null);
		Destroy(m_3DViewer);
		base.Destroy();
	}

}