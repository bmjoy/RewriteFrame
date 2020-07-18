#if UNITY_EDITOR
using Leyoutech.Utility;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Map
{
	[ExecuteInEditMode]
	public class CreateMapWindow : EditorWindow
	{
		/// <summary>
		/// 地图ID
		/// </summary>
		private string m_MapId;
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

		private static CreateMapWindow sm_CreateWin;

		private MapEditorSetting m_MapSetting;

		//[MenuItem("Custom/Map/创建地图")]
		public static void OpenCreateMap()
		{
			sm_CreateWin = GetWindow<CreateMapWindow>();
			sm_CreateWin.Show();
		}

		private void OnEnable()
		{
			m_MapSetting = MapEditorUtility.GetMapEditorSetting();
			m_MapTempletePath = m_MapSetting.m_MapTempletePath;
			m_MapSavePath = m_MapSetting.m_MapSavePath;
			m_AreaSpawnerTempletePath = m_MapSetting.m_AreaSpawnerTempletePath;
		}

		private void OnGUI()
		{
			GUILayout.BeginVertical("box");
			GUILayout.BeginHorizontal();
			m_MapId = EditorGUILayout.TextField("地图ID:", m_MapId);
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("地图模板路径:",m_MapTempletePath);
			if (GUILayout.Button("选择"))
			{
				m_MapTempletePath = MapEditorUtility.OpenFilePanel("", "unity");
				m_MapTempletePath = MapEditorUtility.GetRelativePath(m_MapTempletePath);
				m_MapSetting.m_MapTempletePath = m_MapTempletePath;
			}
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("地图保存路径:", m_MapSavePath);
			if (GUILayout.Button("选择"))
			{
				m_MapSavePath = MapEditorUtility.OpenFloderPanel();
				m_MapSavePath = MapEditorUtility.GetRelativePath(m_MapSavePath);
				m_MapSetting.m_MapSavePath = m_MapSavePath;
			}
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("AreaSpawner模板路径:", m_AreaSpawnerTempletePath);
			if (GUILayout.Button("选择"))
			{
				m_AreaSpawnerTempletePath = MapEditorUtility.OpenFilePanel("","prefab");
				m_AreaSpawnerTempletePath = MapEditorUtility.GetRelativePath(m_MapSavePath);
				m_MapSetting.m_AreaSpawnerTempletePath = m_AreaSpawnerTempletePath;
			}
			GUILayout.EndHorizontal();

			if (GUILayout.Button("创建"))
			{
				CreateMap();
			}
			
			GUILayout.EndVertical();
		}

		private void CreateMap()
		{
			if (string.IsNullOrEmpty(m_MapId))
			{
				EditorUtility.DisplayDialog("提示", "请填写地图Id", "确定");
				return;
			}
			uint mapId;
			if (!uint.TryParse(m_MapId, out mapId))
			{
				EditorUtility.DisplayDialog("提示", "地图Id格式不对", "确定");
				return;
			}
			if (string.IsNullOrEmpty(m_MapTempletePath) || !File.Exists(m_MapTempletePath))
			{
				EditorUtility.DisplayDialog("提示", "不存在该路径,请选择地图模板路径", "确定");
				return;
			}

			if (string.IsNullOrEmpty(m_MapSavePath) || !Directory.Exists(m_MapSavePath))
			{
				EditorUtility.DisplayDialog("提示", "不存在该路径,请选择地图保存路径", "确定");
				return;
			}

			if(string.IsNullOrEmpty(m_AreaSpawnerTempletePath) || !File.Exists(m_AreaSpawnerTempletePath))
			{
				EditorUtility.DisplayDialog("提示", "不存在该路径,请选择AreaSpawner模板路径", "确定");
				return;
			}
			string mapPath = string.Format("{0}/Map_{1}.unity", m_MapSavePath, mapId);
			if (File.Exists(mapPath))
			{
				EditorUtility.DisplayDialog("提示", "该地图已存在", "确定");
				return;
			}
			SaveOpenScene();
			

			FileUtil.CopyFileOrDirectory(m_MapTempletePath, mapPath);
			AssetDatabase.ImportAsset(mapPath);
			Scene exportScene = EditorSceneManager.OpenScene(mapPath, OpenSceneMode.Additive);
			GameObject mapObj = new GameObject(string.Format("Map_{0}", mapId));
			Map map = mapObj.AddComponent<Map>();
			map.Uid = mapId;
			SceneManager.MoveGameObjectToScene(mapObj, exportScene);
			SceneManager.SetActiveScene(exportScene);
			EditorSceneManager.SaveScene(exportScene);
			EditorSceneManager.CloseScene(exportScene, true);
			
			EditorSceneManager.OpenScene(mapPath, OpenSceneMode.Single);
			sm_CreateWin.Close();
		}
		

		/// <summary>
		/// 保存当前打开的场景
		/// </summary>
		private void SaveOpenScene()
		{
			for (int iScene = 0; iScene < EditorSceneManager.sceneCount; iScene++)
			{
				Scene scene = EditorSceneManager.GetSceneAt(iScene);
				EditorSceneManager.SaveScene(scene);
			}
		}
	}
}

#endif