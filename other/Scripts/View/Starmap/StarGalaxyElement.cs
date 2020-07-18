using UnityEngine;

/// <summary>
/// 星系item
/// </summary>
public class StarGalaxyElement : StarmapPointElementBase
{
	/// <summary>
	/// 星系模型显示图
	/// </summary>
	private Starmap3DViewer m_3DViewer;
	/// <summary>
	/// 星系数据
	/// </summary>
	private EditorFixedStar m_Data;

	/// <summary>
	/// 设置星系数据
	/// </summary>
	/// <param name="data"></param>
	/// <param name="isCurrentPoint"></param>
	public void SetData(EditorFixedStar data, bool isCurrentPoint, bool selected = false)
	{
		Initialize();
		m_Data = data;

		SetName($"starmap_name_{data.fixedStarId}");
		SetLocationVisible(isCurrentPoint);
		SetSelectState(selected);
		SetModel(data.fixedStarRes);
	}
	/// <summary>
	/// 设置星系模型
	/// </summary>
	/// <param name="path"></param>
	private void SetModel(string path)
	{
		m_3DViewer = GetRawImage().GetOrAddComponent<Starmap3DViewer>();
		SetToRaw(new Vector2(25, 25));
		m_3DViewer.SetModel(path, Vector3.zero, Vector3.zero, Vector3.one);
	}
	/// <summary>
	/// 获取星系数据
	/// </summary>
	/// <returns></returns>
	public EditorFixedStar GetData()
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