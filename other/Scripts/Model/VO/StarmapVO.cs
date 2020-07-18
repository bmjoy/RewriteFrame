using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class EditorPosition2D
{
	public float x;
	public float y;

	public EditorPosition2D(Vector3 pos)
	{
		x = pos.x;
		y = pos.y;
	}

	public EditorPosition2D(Vector2 pos)
	{
		x = pos.x;
		y = pos.y;
	}

	public Vector3 ToVector3()
	{
		return new Vector3(x, y, 0);
	}

	public Vector2 ToVector2()
	{
		return new Vector2(x, y);
	}
}

[Serializable]
public class EditorStarMap
{
	public EditorFixedStar[] fixedStars;
	public Dictionary<int, EditorFixedStar> FixedStarsDic;

	public void Init()
	{
		if (FixedStarsDic != null)
		{
			return;
		}
		FixedStarsDic = new Dictionary<int, EditorFixedStar>();
		for (int i = 0; i < fixedStars.Length; i++)
		{
			fixedStars[i].Init();
			FixedStarsDic.Add(fixedStars[i].fixedStarId, fixedStars[i]);
		}
	}
}

/// <summary>
/// 星系
/// </summary>
[Serializable]
public class EditorFixedStar
{
	public int fixedStarId;
	public string fixedStarName;
	public string fixedStarRes;
	public int[] relations;
    /// <summary>
    /// 泰坦区域所属GamingMapID
    /// </summary>
    public uint ttGamingMapId;
    /// <summary>
    /// 泰坦区域id
    /// </summary>
    public ulong ttGamingAreaId;
    public EditorPosition2D position;
	public EditorPlanet[] planetList;
	public Dictionary<uint, EditorPlanet> PlanetDic;

	public void Init()
	{
		if (PlanetDic != null)
		{
			return;
		}
		PlanetDic = new Dictionary<uint, EditorPlanet>();
		for (int i = 0; i < planetList.Length; i++)
		{
			planetList[i].Init(fixedStarId);
			PlanetDic.Add(planetList[i].gamingmapId, planetList[i]);
		}
	}
}

/// <summary>
/// 星域
/// </summary>
[Serializable]
public class EditorPlanet
{
	public int fixedStarId;
	public uint gamingmapId;
	public string gamingmapName;
	public string gamingmapRes;
	public EditorPosition2D position;
	public EditorPosition2D scale;
	public float minimapSize;
	public string bgmapRes;
	public EditorPosition2D bgmapPos;
	public EditorPosition2D bgmapScale;
	public EditorStarMapArea[] arealist;
	public Dictionary<ulong, EditorStarMapArea> AreaDic;

	public void Init(int starID)
	{
		fixedStarId = starID;
		if (AreaDic != null)
		{
			return;
		}
		AreaDic = new Dictionary<ulong, EditorStarMapArea>();
		for (int i = 0; i < arealist.Length; i++)
		{
			AreaDic.Add(arealist[i].areaId, arealist[i]);
		}
	}
}

/// <summary>
/// 星球
/// </summary>
[Serializable]
public class EditorStarMapArea
{
	public ulong areaId;
	public string areaName;
	public string area_res;
	public int area_leap_type;
	public int areaType;
	/// <summary>
	/// 当为主跃迁点时 即leap_type == 1 此字段生效 返回main_leapid == 自己的leap_id对应的区域列表
	/// </summary>
	public ulong[] childrenAreaList;
	public EditorPosition2D position;
}