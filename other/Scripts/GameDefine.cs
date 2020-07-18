using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

[InitializeOnLoad]
#endif
public class GameDefine
{
	#region DefineSymbols
#if UNITY_EDITOR
	const string kClientMode = "Loveasy/Everlasting/DefineSettings/ClientMode";
	const string kDebugMode = "Loveasy/Everlasting/DefineSettings/DebugMode";
	const string kTMPMode = "Loveasy/Everlasting/DefineSettings/CurvedUI Enable TMP";

	private static List<string> GetDefinesList(BuildTargetGroup group)
	{
		return new List<string>(PlayerSettings.GetScriptingDefineSymbolsForGroup(group).Split(';'));
	}

	public static void SetEnabled(string symbol, bool enable)
	{
		List<string> defines = GetDefinesList(EditorUserBuildSettings.selectedBuildTargetGroup);
		if (enable)
		{
			if (!defines.Contains(symbol))
			{
				defines.Add(symbol);
			}
		}
		else
		{
			while (defines.Contains(symbol))
			{
				defines.Remove(symbol);
			}
		}

		string definesString = string.Join(";", defines.ToArray());
		PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, definesString);
	}

	private static void ToogleEnabled(string symbol)
	{
		List<string> defines = GetDefinesList(EditorUserBuildSettings.selectedBuildTargetGroup);
		if (defines.Contains(symbol))
		{
			defines.Remove(symbol);
		}
		else
		{
			defines.Add(symbol);
		}

		string definesString = string.Join(";", defines.ToArray());
		PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, definesString);
	}

	[MenuItem(kClientMode)]
	public static void ToggleClientMode()
	{
		ToogleEnabled("CLIENT");
	}

	[MenuItem(kClientMode, true)]
	public static bool ToggleClientModeValidate()
	{
		List<string> defines = GetDefinesList(EditorUserBuildSettings.selectedBuildTargetGroup);
		bool check = defines.Contains("CLIENT");
		Menu.SetChecked(kClientMode, check);
		return true;
	}

	[MenuItem(kDebugMode)]
	public static void ToggleDebugMode()
	{
		ToogleEnabled("BEHAVIAC_RELEASE");
	}

	[MenuItem(kDebugMode, true)]
	public static bool ToggleDebugModeValidate()
	{
		List<string> defines = GetDefinesList(EditorUserBuildSettings.selectedBuildTargetGroup);
		bool check = !defines.Contains("BEHAVIAC_RELEASE");
		Menu.SetChecked(kDebugMode, check);
		return true;
	}

	[MenuItem(kTMPMode)]
	public static void ToggleTMPMode()
	{
		ToogleEnabled("CURVEDUI_TMP");
	}

	[MenuItem(kTMPMode, true)]
	public static bool ToggleTMPModeValidate()
	{
		List<string> defines = GetDefinesList(EditorUserBuildSettings.selectedBuildTargetGroup);
		bool check = defines.Contains("CURVEDUI_TMP");
		Menu.SetChecked(kTMPMode, check);
		return true;
	}
#endif
	#endregion DefineSymbols

	#region AssetBundles

	public static string AssetBundleNameSettings = "/SharedLibrary/Loveasy/Editor/AssetBundleNameSettings.asset";
	public static string FlatBuffersPath = "/SharedLibrary/Database/FlatBuffers/data";
	public static string UIAudioPath = "artworks/sound/ui";

	public const string FlatBufferBundle = "data/flatbuffers.bundle";
	public const string UIBundle = "artworks/ui/";
	public const string IconBundle = "artworks/ui/sprites/icon.bundle";
	public const string CursorBundle = "artworks/ui/cursor.bundle";
	public const string CharacterBundle = "artworks/character/";
	public const string CommonMaterialBundle = "artworks/common/materials/other.bundle";
	public const string ModPolarityBundle = "artworks/ui/sprites/mod.bundle";
	public const string LoadingBundle = "artworks/ui/sprites/backs.bundle";
	//public const string UniverseNebulaBundle = "artworks/universe/nebula.bundle";
	//public const string UniversePlanetBundle = "artworks/universe/planet.bundle";
	//public const string UniverseSatelliteBundle = "artworks/universe/satellite.bundle";
	//public const string UniverseSkyboxBundle = "artworks/universe/skybox.bundle";


	public readonly static string[] PreloadAssetBundles = {
		"shader/loveasy.bundle",
        //"shader/tmp.bundle",
        "shader/thirdparty.bundle",
        //"shader/spacetools.bundle",
    };

	#endregion AssetBundles


	#region "本机记录"

	public const string AgreementKey = "lastAgreementKeyd";

	public const string LastLoginUser = "lastLoginUserIDd";

	public const string LastLoginServer = "lastLoginServerd";

	#endregion


}

public enum eSceneType
{
	/// <summary>
	/// 城市
	/// </summary>
	CITY = 1,
	/// <summary>
	/// 宇宙
	/// </summary>
	UNIVERSE = 2,
	/// <summary>
	/// 空间站
	/// </summary>
	SPACE_STATION = 3,
	/// <summary>
	/// 大气层内
	/// </summary>
	ATMOSPHERE = 4,
}

public enum eMapType
{
	/// <summary>
	/// 城市
	/// </summary>
	CITY = 1,
	/// <summary>
	/// 空间站
	/// </summary>
	SPACE_STATION = 2,
	/// <summary>
	/// 近地表
	/// </summary>
	ATMOSPHERE = 3,
	/// <summary>
	/// 深空
	/// </summary>
	UNIVERSE = 4,
	/// <summary>
	/// 副本
	/// </summary>
	DUNGEON = 6,
}

public enum eColliderType
{
	NONE = 0,
	/// <summary>
	/// 盒碰撞体
	/// </summary>
	BOX = 1,
	/// <summary>
	/// 球碰撞体
	/// </summary>
	SPHERE,
	/// <summary>
	/// 胶囊碰撞体
	/// </summary>
	CAPSULE,
}

public enum WeaponType
{
	None = 0,
	Turret = 1,
	Missile = 2,
	UAV = 3,
	Howitzer = 4,
	Laser = 5,
	Guard = 6,
	Support = 7
}

public enum TipsType
{
	None = 0,

	//蓝图
	Blueprint_Ship,
	Blueprint_Keel,
	Blueprint_Module,
	Blueprint_Weapon,

	//道具
	Item_Keel,
	Item_Module,
	Item_Weapon,
	Item_WeaponMod,
	Item_Material,
	Item_Painting,

	//船
	Ship_Attribute,
	Ship_Equip,
	Ship_Skill,

	//one skill
	Skill,
}

public enum TipsDataType
{
	None,
	Blueprint,
	Material,
	Ship,
	Weapon,
	Mod,


}

public enum CameraEventType
{
	PostProcessingStack,
	PanelFrontOfCamera,
}

/// <summary>
/// 字条所属类型
/// </summary>
public enum SetType
{
	Control = 1,
	Game = 2,
	Video = 3,
	Audio = 5
}

/// <summary>
/// 选项类型
/// </summary>
public enum OptionType
{
	DropDown = 1,
	Toggle = 2,
	Slider = 3,
	Button = 4,
	Text = 5
}

/// <summary>
/// 背包新分类页签
/// </summary>
public enum PackagePageType
{
    /// <summary>
    /// 消耗品
    /// </summary>
    Consumables,
    /// <summary>
    /// 材料
    /// </summary>
    Material,
    /// <summary>
    /// 武器
    /// </summary>
    Weapons,
    /// <summary>
    /// 转化炉
    /// </summary>
    Converters,
    /// <summary>
    /// 装备
    /// </summary>
    Devices,
    /// <summary>
    /// 芯片
    /// </summary>
    Chips 
}

/// <summary>
/// 社交类型
/// </summary>
public enum SocialType
{
	/// <summary>
	/// 邮件
	/// </summary>
	Mail,
	/// <summary>
	/// 日志
	/// </summary>
	Log
}

/// <summary>
/// 好友排序类型
/// </summary>
public enum SortType
{
	/// <summary>
	/// 首字母正序
	/// </summary>
	Positive,
	/// <summary>
	/// 首字母倒序
	/// </summary>
	Reverse,
	/// <summary>
	/// 添加时间
	/// </summary>
	AddTime
}

/// <summary>
/// 生产类型
/// </summary>
public enum ProduceType
{
	/// <summary>
	/// 重型武器
	/// </summary>
	HeavyWeapon,

	/// <summary>
	/// 轻武器
	/// </summary>
	LightWeapon,

	/// <summary>
	/// 转化炉
	/// </summary>
	Reformer,

	/// <summary>
	/// 芯片
	/// </summary>
	Chip,

	/// <summary>
	/// 装置
	/// </summary>
	Device,

	/// <summary>
	/// 材料
	/// </summary>
	Material,

	/// <summary>
	/// 战舰
	/// </summary>
	Ship,
}

/// <summary>
/// 采集掉落物状态
/// </summary>
public enum DropItemState
{
	None,
	/// <summary>`
	/// 出生
	/// </summary>
	Born,
	/// <summary>
	/// 停止
	/// </summary>
	Stay,
	/// <summary>
	/// 采集
	/// </summary>
	Gather

}


