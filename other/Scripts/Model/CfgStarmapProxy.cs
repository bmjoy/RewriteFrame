using FlatBuffers;
using PureMVC.Patterns.Proxy;
using UnityEngine;

/// <summary>
/// 星图配置
/// </summary>
public class CfgStarmapProxy : Proxy
{
	private const string ASSET_ADDRESS = "Assets/Config/data/starmap_data.json";

	private EditorStarMap m_Config;

	public CfgStarmapProxy() : base(ProxyName.CfgStarmapProxy)
	{
		DataManager.Instance.LoadDataToProxy(ASSET_ADDRESS, this);
	}

	public override void InitData(ByteBuffer byteBuffer)
	{
		m_Config = JsonUtility.FromJson<EditorStarMap>(System.Text.Encoding.UTF8.GetString(byteBuffer.ToSizedArray()));
		m_Config.Init();
	}

	/// <summary>
	/// 获取一个星系数据
	/// </summary>
	/// <param name="starTid"></param>
	/// <returns></returns>
	public EditorFixedStar GetFixedStarByTid(int starTid)
	{
		if (m_Config.FixedStarsDic.TryGetValue(starTid, out EditorFixedStar value))
		{
			return value;
		}
		return null;
	}

	/// <summary>
	/// 获取一个星域数据
	/// </summary>
	/// <param name="starTid"></param>
	/// <param name="PlanetTid"></param>
	/// <returns></returns>
	public EditorPlanet GetPlanet(int starTid, uint PlanetTid)
	{
		EditorFixedStar star = GetFixedStarByTid(starTid);
		if (star != null && star.PlanetDic.TryGetValue(PlanetTid, out EditorPlanet value))
		{
			return value;
		}
		return null;
	}

	/// <summary>
	/// 获取恒星
	/// </summary>
	/// <param name="index"></param>
	/// <returns></returns>
	public EditorFixedStar GetFixedStarByIndex(int index)
	{
		return m_Config.fixedStars[index];
	}

	/// <summary>
	/// 恒星数量
	/// </summary>
	/// <returns></returns>
	public int GetFixedStarCount()
	{
		return m_Config.fixedStars.Length;
	}
}