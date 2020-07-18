#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Map
{
	/// <summary>
	/// GamingMapEditor工具类
	/// </summary>
	public class GamingMapEditorUtility
	{
		#region GamingMapEditorSetting操作相关
		/// <summary>
		/// GamingMap相关配置
		/// </summary>
		private static GamingMapEditorSetting m_GamingMapEditorSetting = null;

		/// <summary>
		/// 获取、创建GamingMap配置
		/// </summary>
		/// <returns></returns>
		public static GamingMapEditorSetting GetOrCreateGamingMapEditorSetting()
		{
			if (GetGamingMapEditorSetting() == null)
			{
				CreateMapEditorSetting();
			}
			return m_GamingMapEditorSetting;
		}

		/// <summary>
		/// 获取GamingMap配置
		/// </summary>
		/// <returns></returns>
		public static GamingMapEditorSetting GetGamingMapEditorSetting()
		{
			if (m_GamingMapEditorSetting)
			{
				return m_GamingMapEditorSetting;
			}

			if (!LoadGamingMapEditorSetting(EditorPrefs.GetString(Constants.GAMINGMAP_EDITOR_SETTING_PATH_PREFSKEY)))
			{
				string[] assets = AssetDatabase.FindAssets("t:GamingMapEditorSetting");
				if (assets.Length > 0
					&& LoadGamingMapEditorSetting(AssetDatabase.GUIDToAssetPath(assets[0])))
				{
					EditorPrefs.SetString(Constants.GAMINGMAP_EDITOR_SETTING_PATH_PREFSKEY
						, assets[0]);
				}
			}
			return m_GamingMapEditorSetting;
		}

		/// <summary>
		/// 载入GamingMapEdtior配置
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		private static bool LoadGamingMapEditorSetting(string path)
		{
			UnityEngine.Object settingObject = AssetDatabase.LoadAssetAtPath(path, typeof(GamingMapEditorSetting));
			if (settingObject
				&& settingObject is GamingMapEditorSetting)
			{
				m_GamingMapEditorSetting = settingObject as GamingMapEditorSetting;
				return true;
			}
			else
			{
				return false;
			}
		}

		/// <summary>
		/// 创建GamingMap配置
		/// </summary>
		/// <param name="directory"></param>
		/// <returns></returns>
		public static GamingMapEditorSetting CreateGamingMapEditorSetting(string directory)
		{
			string path = string.Format("{0}/{1}.asset", directory, Constants.GAMINGMAP_EDITOR_SETTING_DEFAULT_FILENAME);
			AssetDatabase.CreateAsset(new GamingMapEditorSetting(), path);
			EditorPrefs.SetString(Constants.GAMINGMAP_EDITOR_SETTING_PATH_PREFSKEY, path);
			return GetGamingMapEditorSetting();
		}

		/// <summary>
		/// 创建MapEditorSetting
		/// </summary>
		[MenuItem("Custom/Map/Create GamingMap Editor Setting")]
		private static void CreateMapEditorSetting()
		{
			GamingMapEditorSetting setting = GetGamingMapEditorSetting();
			if (!setting)
			{
				string path = AssetDatabase.GetAssetOrScenePath(Selection.activeObject);
				string directory;
				if (Directory.Exists(path))
				{
					directory = path;
				}
				else
				{
					if (File.Exists(path))
					{
						directory = path.Substring(0, path.LastIndexOf('/'));
					}

					else
					{
						directory = "Assets";
					}
				}

				setting = CreateGamingMapEditorSetting(directory);
			}

			if (setting)
			{
				Selection.activeObject = setting;
			}
		}
		#endregion

		#region GamingMapEditorSetting数据相关

		/// <summary>
		/// 获取npc模板路径
		/// </summary>
		/// <returns></returns>
		public static string GetCreatureTempletePath()
		{
			if(m_GamingMapEditorSetting != null)
			{
				return m_GamingMapEditorSetting.m_CreatureTempletePath;
			}
			return "";
		}
		/// <summary>
		/// 获取Location模板路径
		/// </summary>
		/// <returns></returns>
		public static string GetLocationTempletePath()
		{
			if(m_GamingMapEditorSetting != null)
			{
				return m_GamingMapEditorSetting.m_LocationTempletePath;
			}
			return "";
		}

        /// <summary>
        /// 获取Trigger模板路径
        /// </summary>
        /// <returns></returns>
        public static string GetTriggerTempletePath()
        {
            if(m_GamingMapEditorSetting != null)
            {
                return m_GamingMapEditorSetting.m_TriggerTempletePath;
            }
            return "";
        }
        /// <summary>
        /// 获取Leap模板路径
        /// </summary>
        /// <returns></returns>
        public static string GetLeapTempletePath()
        {
            if(m_GamingMapEditorSetting != null)
            {
                return m_GamingMapEditorSetting.m_LeapTempletePath;
            }
            return "";
        }

        /// <summary>
        /// 获取最大战舰路径
        /// </summary>
        /// <returns></returns>
        public static string GetMaxShipPath()
        {
            if(m_GamingMapEditorSetting != null)
            {
                return m_GamingMapEditorSetting.m_MaxShipPath;
            }
            return "";
        }
		#endregion
	}
}
#endif