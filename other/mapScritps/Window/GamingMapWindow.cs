#if UNITY_EDITOR
using Leyoutech.Utility;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Map
{
	[ExecuteInEditMode]
	public class GamingMapWindow : EditorWindow
	{
		private static GamingMapWindow sm_MapWin;
		private int m_SelectIndex = -1;
		/// <summary>
		/// GamingMap存储路径
		/// </summary>
		private string m_GamingMapPath;
		/// <summary>
		/// GamingMap模板
		/// </summary>
		private string m_GamingMapTemplete;
		/// <summary>
		/// GamingMapArea模板
		/// </summary>
		private string m_GamingAreaTemplete;
		/// <summary>
		/// 配置表路径
		/// </summary>
		private string m_ConfigPath;
		/// <summary>
		/// json保存路径
		/// </summary>
		private string m_JsonPath;

		/// <summary>
		/// Creature模板路径
		/// </summary>
		private string m_CreatureTempletePath;
		/// <summary>
		/// Location模板路径
		/// </summary>
		private string m_LocationTempletePath;

		/// <summary>
		/// 模板路径
		/// </summary>
		private string m_MapTempletePath;
		/// <summary>
		/// 地图保存路径
		/// </summary>
		private string m_MapSavePath;

		/// <summary>
		/// AreaSpawner模板路径
		/// </summary>
		private string m_AreaSpawnerTempletePath;

        /// <summary>
        /// collider保存路径
        /// </summary>
        private string m_ColliderSavePath;

        /// <summary>
        /// Collider备份路径
        /// </summary>
        private string m_ColliderSavePathCopy;
        /// <summary>
        /// 跃迁模板路径
        /// </summary>
        private string m_LeapTempletePath;

        /// <summary>
        /// 触发器模板路径
        /// </summary>
        private string m_TriggerTempletePath;
        /// <summary>
        /// 最大战舰路径
        /// </summary>
        private string m_MaxShipPath;
        /// <summary>
        /// 星图保存路径
        /// </summary>
        private string m_StarMapPath;

        /// <summary>
        /// 战舰属性存放路径
        /// </summary>
        private string m_SpacecraftPropertyPath;
        /// <summary>
        /// 寻宝预设保存路径
        /// </summary>
        private string m_TreasurePrefabSavePath;

        /// <summary>
        /// 挖矿预设保存路径
        /// </summary>
        private string m_MineralPrefabSavePath;

        /// <summary>
        /// 地图功能列表
        /// </summary>
        private GUIContent[] m_GuiContentArray;

		private GamingMapEditorSetting m_GamingSetting;
		private MapEditorSetting m_MapSetting;

		private GUIStyle m_GuiStyle;
		public enum MapFunctionType
		{
			CreateGamingMap,
			OpenGamingMap,
			CreateMap
		}

		[MenuItem("Custom/Map/地图编辑器")]
		public static void OpenMapWindow()
		{
			sm_MapWin = GetWindow<GamingMapWindow>();
			sm_MapWin.Show();
		}

		private void OnEnable()
		{
			Init();
		}
		/// <summary>
		/// 初始化
		/// </summary>
		public void Init()
		{
            m_GamingSetting = GamingMapEditorUtility.GetGamingMapEditorSetting();
            m_MapSetting = MapEditorUtility.GetMapEditorSetting();
            m_SelectIndex = -1;
			m_GuiStyle = new GUIStyle();
			m_GuiStyle.fontSize = 20;
			m_GuiStyle.fontStyle = FontStyle.BoldAndItalic;
			m_GuiContentArray = new GUIContent[] { new GUIContent("创建地图配置"), new GUIContent("打开地图配置列表"),new GUIContent("创建地图") };

			m_GamingMapPath = m_GamingSetting.m_GamingMapPath;
			m_GamingMapTemplete = m_GamingSetting.m_GamingMapTemplete;
			m_GamingAreaTemplete = m_GamingSetting.m_GamingAreaTemplete;
			m_ConfigPath = m_GamingSetting.m_ConfigPath;
			m_JsonPath = m_GamingSetting.m_JsonPath;
			m_CreatureTempletePath = m_GamingSetting.m_CreatureTempletePath;
			m_LocationTempletePath = m_GamingSetting.m_LocationTempletePath;
            m_LeapTempletePath = m_GamingSetting.m_LeapTempletePath;
            m_TriggerTempletePath = m_GamingSetting.m_TriggerTempletePath;
            m_MaxShipPath = m_GamingSetting.m_MaxShipPath;
            m_StarMapPath = m_GamingSetting.m_StarMapPath;
            m_SpacecraftPropertyPath = m_GamingSetting.m_SpacecraftPropertyPath;

            m_MapTempletePath = m_MapSetting.m_MapTempletePath;
            m_TreasurePrefabSavePath = m_MapSetting.m_TreasurePrefabSavePath;
            m_MapSavePath = m_MapSetting.m_MapSavePath;
			m_AreaSpawnerTempletePath = m_MapSetting.m_AreaSpawnerTempletePath;
            m_ColliderSavePath = m_MapSetting.m_ColliderSavePath;
            m_ColliderSavePathCopy = m_MapSetting.m_ColliderSavePathCopy;
            m_MineralPrefabSavePath = m_MapSetting.m_MineralPrefabSavePath;
            /*m_GamingMapPath = PlayerPrefsUtility.GetString(Constants.GAMINGMAP_SAVE_PATH);
			m_GamingMapTemplete = PlayerPrefsUtility.GetString(Constants.GAMINGMAP_TEMPLETE_PATH);
			m_GamingAreaTemplete = PlayerPrefsUtility.GetString(Constants.GAMINGAREA_TEMPLETE_PATH);
			m_ConfigPath = PlayerPrefsUtility.GetString(Constants.CONFIG_PATH);
			m_JsonPath = PlayerPrefsUtility.GetString(Constants.SAVEJSON_PATH);

			m_CreatureTempletePath = PlayerPrefsUtility.GetString(Constants.CREATURE_TEMPLETE_PATH);
			m_LocationTempletePath = PlayerPrefsUtility.GetString(Constants.LOCATION_TEMPLETE_PATH);

			m_MapTempletePath = PlayerPrefsUtility.GetString(Constants.MAP_TEMPLETE_PATH);
			m_MapSavePath = PlayerPrefsUtility.GetString(Constants.MAP_SAVE_PATH);
			m_AreaSpawnerTempletePath = PlayerPrefsUtility.GetString(Constants.AREASPAWNER_TEMPLETE_PATH);*/
        }

		private void OnGUI()
		{
            m_GamingSetting = GamingMapEditorUtility.GetGamingMapEditorSetting();
            m_MapSetting = MapEditorUtility.GetMapEditorSetting();
            GUILayout.BeginVertical();
			m_SelectIndex = GUILayout.SelectionGrid(m_SelectIndex, m_GuiContentArray,2,GUILayout.Height(100));
			OnClickGrid(m_SelectIndex);
			GUILayout.BeginVertical("box");
			EditorGUILayout.LabelField("GamingMap设置:", m_GuiStyle);
			GUILayout.Space(5);
			GUILayout.BeginHorizontal("box");
			
			EditorGUILayout.LabelField("GamingMap保存路径:", m_GamingMapPath);
			//GUILayout.FlexibleSpace();
			if (GUILayout.Button("选择"))
			{
				m_GamingMapPath = MapEditorUtility.OpenFloderPanel();
				m_GamingMapPath = MapEditorUtility.GetRelativePath(m_GamingMapPath);
				m_GamingSetting.m_GamingMapPath = m_GamingMapPath;
				//PlayerPrefsUtility.SetString(Constants.GAMINGMAP_SAVE_PATH, m_GamingMapPath);
			}
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal("box");
			EditorGUILayout.LabelField("GamingMap模板:", m_GamingMapTemplete);
			if (GUILayout.Button("选择"))
			{
				m_GamingMapTemplete = MapEditorUtility.OpenFilePanel("", "unity");
				m_GamingMapTemplete = MapEditorUtility.GetRelativePath(m_GamingMapTemplete);
				m_GamingSetting.m_GamingMapTemplete = m_GamingMapTemplete;
				//PlayerPrefsUtility.SetString(Constants.GAMINGMAP_TEMPLETE_PATH, m_GamingMapTemplete);
			}
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal("box");
			EditorGUILayout.LabelField("GamingMapArea模板",m_GamingAreaTemplete);
			if(GUILayout.Button("选择"))
			{
				m_GamingAreaTemplete = MapEditorUtility.OpenFilePanel("", "prefab");
				m_GamingAreaTemplete = MapEditorUtility.GetRelativePath(m_GamingAreaTemplete);
				m_GamingSetting.m_GamingAreaTemplete = m_GamingAreaTemplete;
				//PlayerPrefsUtility.SetString(Constants.GAMINGAREA_TEMPLETE_PATH, m_GamingAreaTemplete);
			}
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal("box");
			EditorGUILayout.LabelField("配置表路径", m_ConfigPath);
			if (GUILayout.Button("选择"))
			{
				m_ConfigPath = MapEditorUtility.OpenFloderPanel("");
				m_ConfigPath = MapEditorUtility.GetRelativePath(m_ConfigPath);
				m_GamingSetting.m_ConfigPath = m_ConfigPath;
				//PlayerPrefsUtility.SetString(Constants.CONFIG_PATH, m_ConfigPath);
			}
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal("box");
			EditorGUILayout.LabelField("Json保存路径", m_JsonPath);
			if (GUILayout.Button("选择"))
			{
				m_JsonPath = MapEditorUtility.OpenFloderPanel("");
				m_JsonPath = MapEditorUtility.GetRelativePath(m_JsonPath);
				m_GamingSetting.m_JsonPath = m_JsonPath;
				//PlayerPrefsUtility.SetString(Constants.SAVEJSON_PATH, m_JsonPath);
			}
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal("box");
			EditorGUILayout.LabelField("Creature模板", m_CreatureTempletePath);
			if (GUILayout.Button("选择"))
			{
				m_CreatureTempletePath = MapEditorUtility.OpenFilePanel("","prefab");
				m_CreatureTempletePath = MapEditorUtility.GetRelativePath(m_CreatureTempletePath);
				m_GamingSetting.m_CreatureTempletePath = m_CreatureTempletePath;
				//PlayerPrefsUtility.SetString(Constants.CREATURE_TEMPLETE_PATH, m_CreatureTempletePath);
			}
			GUILayout.EndHorizontal();


			GUILayout.BeginHorizontal("box");
			EditorGUILayout.LabelField("Location模板", m_LocationTempletePath);
			if (GUILayout.Button("选择"))
			{
				m_LocationTempletePath = MapEditorUtility.OpenFilePanel("", "prefab");
				m_LocationTempletePath = MapEditorUtility.GetRelativePath(m_LocationTempletePath);
				m_GamingSetting.m_LocationTempletePath = m_LocationTempletePath;
			}
			GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal("box");
            EditorGUILayout.LabelField("Leap模板", m_LeapTempletePath);
            if (GUILayout.Button("选择"))
            {
                m_LeapTempletePath = MapEditorUtility.OpenFilePanel("", "prefab");
                m_LeapTempletePath = MapEditorUtility.GetRelativePath(m_LeapTempletePath);
                m_GamingSetting.m_LeapTempletePath = m_LeapTempletePath;
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal("box");
            EditorGUILayout.LabelField("Trigger模板", m_TriggerTempletePath);
            if (GUILayout.Button("选择"))
            {
                m_TriggerTempletePath = MapEditorUtility.OpenFilePanel("", "prefab");
                m_TriggerTempletePath = MapEditorUtility.GetRelativePath(m_TriggerTempletePath);
                m_GamingSetting.m_TriggerTempletePath = m_TriggerTempletePath;
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal("box");
            EditorGUILayout.LabelField("最大战舰", m_MaxShipPath);
            if (GUILayout.Button("选择"))
            {
                m_MaxShipPath = MapEditorUtility.OpenFilePanel("", "prefab");
                m_MaxShipPath = MapEditorUtility.GetRelativePath(m_MaxShipPath);
                m_GamingSetting.m_MaxShipPath = m_MaxShipPath;
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal("box");
            EditorGUILayout.LabelField("星图保存路径", m_StarMapPath);
            if (GUILayout.Button("选择"))
            {
                m_StarMapPath = MapEditorUtility.OpenFloderPanel("");
                m_StarMapPath = MapEditorUtility.GetRelativePath(m_StarMapPath);
                m_GamingSetting.m_StarMapPath = m_StarMapPath;
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal("box");
            EditorGUILayout.LabelField("战舰属性保存路径", m_SpacecraftPropertyPath);
            if (GUILayout.Button("选择"))
            {
                m_SpacecraftPropertyPath = MapEditorUtility.OpenFloderPanel("");
                m_SpacecraftPropertyPath = MapEditorUtility.GetRelativePath(m_SpacecraftPropertyPath);
                m_GamingSetting.m_SpacecraftPropertyPath = m_SpacecraftPropertyPath;
            }
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
			GUILayout.Space(20);
			//Map的相关配置
			GUILayout.BeginVertical("box");
			EditorGUILayout.LabelField("Map设置:", m_GuiStyle);
			GUILayout.Space(5);
			GUILayout.BeginHorizontal("box");
			EditorGUILayout.LabelField("地图模板路径", m_MapTempletePath);
			if (GUILayout.Button("选择"))
			{
				m_MapTempletePath = MapEditorUtility.OpenFilePanel("", "unity");
				m_MapTempletePath = MapEditorUtility.GetRelativePath(m_MapTempletePath);
				m_MapSetting.m_MapTempletePath = m_MapTempletePath;
				//PlayerPrefsUtility.SetString(Constants.MAP_TEMPLETE_PATH, m_MapTempletePath);
			}
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal("box");
			EditorGUILayout.LabelField("地图保存路径:", m_MapSavePath);
			if (GUILayout.Button("选择"))
			{
				m_MapSavePath = MapEditorUtility.OpenFloderPanel();
				m_MapSavePath = MapEditorUtility.GetRelativePath(m_MapSavePath);
				m_MapSetting.m_MapSavePath = m_MapSavePath;
				//PlayerPrefsUtility.SetString(Constants.MAP_SAVE_PATH, m_MapSavePath);
			}
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal("box");
			EditorGUILayout.LabelField("AreaSpawner模板路径:", m_AreaSpawnerTempletePath);
			if (GUILayout.Button("选择"))
			{
				m_AreaSpawnerTempletePath = MapEditorUtility.OpenFilePanel("", "prefab");
				m_AreaSpawnerTempletePath = MapEditorUtility.GetRelativePath(m_AreaSpawnerTempletePath);
				m_MapSetting.m_AreaSpawnerTempletePath = m_AreaSpawnerTempletePath;
			}
			GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal("box");
            EditorGUILayout.LabelField("碰撞体保存路径:", m_ColliderSavePath);
            if (GUILayout.Button("选择"))
            {
                m_ColliderSavePath = MapEditorUtility.OpenFloderPanel("");
                m_ColliderSavePath = MapEditorUtility.GetRelativePath(m_ColliderSavePath);
                m_MapSetting.m_ColliderSavePath = m_ColliderSavePath;
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal("box");
            EditorGUILayout.LabelField("碰撞体备份保存路径:", m_ColliderSavePathCopy);
            if (GUILayout.Button("选择"))
            {
                m_ColliderSavePathCopy = MapEditorUtility.OpenFloderPanel("");
                m_ColliderSavePathCopy = MapEditorUtility.GetRelativePath(m_ColliderSavePathCopy);
                m_MapSetting.m_ColliderSavePathCopy = m_ColliderSavePathCopy;
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal("box");
            EditorGUILayout.LabelField("寻宝预设保存路径:", m_TreasurePrefabSavePath);
            if (GUILayout.Button("选择"))
            {
                m_TreasurePrefabSavePath = MapEditorUtility.OpenFloderPanel("");
                m_TreasurePrefabSavePath = MapEditorUtility.GetRelativePath(m_TreasurePrefabSavePath);
                m_MapSetting.m_TreasurePrefabSavePath = m_TreasurePrefabSavePath;
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal("box");
            EditorGUILayout.LabelField("挖矿预设保存路径:", m_MineralPrefabSavePath);
            if (GUILayout.Button("选择"))
            {
                m_MineralPrefabSavePath = MapEditorUtility.OpenFloderPanel("");
                m_MineralPrefabSavePath = MapEditorUtility.GetRelativePath(m_MineralPrefabSavePath);
                m_MapSetting.m_MineralPrefabSavePath = m_MineralPrefabSavePath;
            }
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
			GUILayout.EndVertical();
            EditorUtility.SetDirty(m_MapSetting);
            EditorUtility.SetDirty(m_GamingSetting);
        }

		/// <summary>
		/// Grid点击事件
		/// </summary>
		/// <param name="gridIndex"></param>
		private void OnClickGrid(int gridIndex)
		{
			if(gridIndex<0)
			{
				return;
			}
			m_SelectIndex = -1;
			MapFunctionType funType = (MapFunctionType)gridIndex;
			switch(funType)
			{
				case MapFunctionType.CreateGamingMap:
					CreateGamingMapWindow.OpenGamingMap();
					break;
				case MapFunctionType.OpenGamingMap:
					OpenGamingMapWindow.OpenGamingMap();
					break;
				case MapFunctionType.CreateMap:
					CreateMapWindow.OpenCreateMap();
					break;
			}
		}
        
    }
}
#endif
