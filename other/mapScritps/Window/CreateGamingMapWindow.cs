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
	public class CreateGamingMapWindow : EditorWindow
	{
		private static CreateGamingMapWindow sm_MapWin;
		/// <summary>
		/// 关联地图Scene
		/// </summary>
		private string m_RelationScene;
		/// <summary>
		/// 关联Scene列表
		/// </summary>
		private List<string> m_RelationSceneList;
		/// <summary>
		/// 选择 收起
		/// </summary>
		private bool m_HasSelect = false;
		private Vector2 m_VerticalPos;

		/// <summary>
		/// GamingMap Id
		/// </summary>
		private string m_GamingMapId;
		/// <summary>
		/// GamingMap名字
		/// </summary>
		private string m_GamingMapName;
		/// <summary>
		/// GamingMap类型
		/// </summary>
		private GamingMapType m_GamingMapType;
		/// <summary>
		/// 选择的类型
		/// </summary>
		private int m_SelectTypeIndex = 0;

		public static void OpenGamingMap()
		{
			sm_MapWin = GetWindow<CreateGamingMapWindow>();
			sm_MapWin.position = new Rect(600, 300, 400, 400);
			sm_MapWin.minSize = new Vector2(700, 400);
			sm_MapWin.Show();
		}

		private void OnEnable()
		{
			m_SelectTypeIndex = 0;
			if (m_RelationSceneList == null)
			{
				m_RelationSceneList = new List<string>();
			}
			m_RelationSceneList.Clear();
			string mapScenePath = "";
			MapEditorSetting mapSetting = MapEditorUtility.GetMapEditorSetting();
			if(mapSetting != null)
			{
				mapScenePath = mapSetting.m_MapSavePath;
			}
			string[] mapPathArray = AssetDatabase.FindAssets("t:Scene", new string[] { mapScenePath });
			if (mapPathArray != null && mapPathArray.Length > 0)
			{
				for (int iMap = 0; iMap < mapPathArray.Length; iMap++)
				{
					string assetPath = AssetDatabase.GUIDToAssetPath(mapPathArray[iMap]);
                    string[] pathSplit =  assetPath.Split('/');
                    if(pathSplit != null && pathSplit.Length>0)
                    {
                        if(pathSplit[pathSplit.Length -1].Contains("Map_"))
                        {
                            m_RelationSceneList.Add(assetPath);
                        }
                    }

                }
			}
		}

		private void OnGUI()
		{
			GUILayout.BeginHorizontal("box");
			EditorGUILayout.LabelField("关联地图Scene:", m_RelationScene);
			if(m_HasSelect)
			{
				if(GUILayout.Button("收起",GUILayout.Width(100)))
				{
					m_HasSelect = false;
				}
				GUILayout.EndHorizontal();
				m_VerticalPos = GUILayout.BeginScrollView(m_VerticalPos);
				GUILayout.BeginVertical("box");
				if (m_RelationSceneList != null && m_RelationSceneList.Count > 0)
				{
					for(int iRelation = 0;iRelation<m_RelationSceneList.Count;iRelation++)
					{
						string[] splitPath = m_RelationSceneList[iRelation].Split('/');
						if (splitPath != null && splitPath.Length > 0)
						{
							string showPath = splitPath[splitPath.Length - 1];
							if (!string.IsNullOrEmpty(showPath))
							{
								showPath = showPath.Replace(".unity", "");
								if (GUILayout.Button(showPath))
								{
									m_RelationScene = m_RelationSceneList[iRelation];
									m_HasSelect = false;
								}
							}
						}
					}
				}
				else
				{
					GUILayout.Label("未找到关联的Scene");
				}
				GUILayout.EndVertical();
				GUILayout.EndScrollView();
			}
			else
			{
				if (GUILayout.Button("选择", GUILayout.Width(100)))
				{
					m_HasSelect = true;
				}
				GUILayout.EndHorizontal();
			}
			GUILayout.BeginVertical("box");
		    m_GamingMapId = EditorGUILayout.TextField("GamingMap Id:", m_GamingMapId);
			m_GamingMapName = EditorGUILayout.TextField("名称:",m_GamingMapName);
			//m_GamingMapType = (GamingMapType)EditorGUILayout.EnumPopup("类型:", m_GamingMapType);
			m_SelectTypeIndex = EditorGUILayout.Popup(new GUIContent("类型:"),m_SelectTypeIndex, EditorGamingMapData.GAMINGMAPTYPE_NAME);
			m_GamingMapType = (GamingMapType)m_SelectTypeIndex;
			if(GUILayout.Button("创建"))
			{
				if (m_RelationScene == null)
				{
					EditorUtility.DisplayDialog("提示", "请选择关联地图Scene", "确定");
					return;
				}

				uint mapId;
				if(!uint.TryParse(m_GamingMapId, out mapId))
				{
					EditorUtility.DisplayDialog("提示", "GamingMap id格式错误", "确定");
					return;
				}
				if(string.IsNullOrEmpty(m_GamingMapName))
				{
					EditorUtility.DisplayDialog("提示", "名称不能为空", "确定");
					return;
				}
				string mapPath = "";
				GamingMapEditorSetting gamingSetting = GamingMapEditorUtility.GetGamingMapEditorSetting();
				if(gamingSetting != null)
				{
					mapPath = string.Format("{0}/GamingMap_{1}.unity",MapEditorUtility.GetFullPath(gamingSetting.m_GamingMapPath), mapId);
				}
				if (File.Exists(mapPath))
				{
					EditorUtility.DisplayDialog("提示", "该GamingMap已存在", "确定");
					return;
				}
				List<Scene> openScenes = new List<Scene>();
				for (int iScene = 0; iScene < EditorSceneManager.sceneCount; iScene++)
				{
					Scene sceneTmp = EditorSceneManager.GetSceneAt(iScene);
					openScenes.Add(sceneTmp);
				}

				SaveOpenScene();
				//打开对应的map 找到mapid 创建Gamingmap 初始化GamingMap和Area
				Scene scene = EditorSceneManager.OpenScene(m_RelationScene,OpenSceneMode.Additive);
				Map map = null;
				GameObject[] objs = scene.GetRootGameObjects();
				if(objs != null && objs.Length>0)
				{
					for(int iObj = 0;iObj<objs.Length;iObj++)
					{
						map = objs[iObj].GetComponent<Map>();
						if(map != null)
						{
							break;
						}
					}
				}
				
				//string gamingPath = PlayerPrefsUtility.GetString(Constants.GAMINGMAP_TEMPLETE_PATH);
				string gamingPath = "";
				if(gamingSetting != null)
				{
					gamingPath = MapEditorUtility.GetFullPath(gamingSetting.m_GamingMapTemplete);
					gamingPath += "/";
				}
				FileUtil.CopyFileOrDirectory(gamingPath, mapPath);
				AssetDatabase.ImportAsset(mapPath);
				Scene exportScene = EditorSceneManager.OpenScene(mapPath, OpenSceneMode.Additive);
				GameObject[] exportObjs = exportScene.GetRootGameObjects();
				if(exportObjs != null && exportObjs.Length>0)
				{
					for(int iExport =0;iExport<exportObjs.Length;iExport++)
					{
						GamingMap gamingMap = exportObjs[iExport].GetComponent<GamingMap>();
						if(gamingMap != null)
						{
							gamingMap.Init(mapId, map.Uid, m_GamingMapName, m_GamingMapType, map.GetAreaSpawnerList());
							break;
						}
					}
				}
				SceneManager.SetActiveScene(exportScene);
				EditorSceneManager.SaveScene(exportScene);
				EditorSceneManager.CloseScene(scene, true);

				if(openScenes != null && openScenes.Count>0)
				{
					for(int iOpen = 0;iOpen<openScenes.Count;iOpen++)
					{
						Scene openScene = openScenes[iOpen];
						EditorSceneManager.SaveScene(openScene);
						EditorSceneManager.CloseScene(openScene, true);
					}
				}
            }
			GUILayout.EndVertical();
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
				EditorSceneManager.CloseScene(scene, true);
			}
		}
	}
}

#endif