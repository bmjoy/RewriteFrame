#if UNITY_EDITOR
using Leyoutech.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Map
{
	[ExecuteInEditMode]
	public class OpenGamingMapWindow : EditorWindow
	{
		private static OpenGamingMapWindow sm_OpenMapWin;
		/// <summary>
		/// GamingMap保存路径
		/// </summary>
		private string m_GamingMapPath;
		/// <summary>
		/// GamingMap列表
		/// </summary>
		private List<string> m_GamingMapList;

		private Vector2 assetScrollPos;
		/// <summary>
		/// 水平方向显示按钮数量
		/// </summary>
		private const int HORIZONTAL_COUNT = 4;

		public static void OpenGamingMap()
		{
			sm_OpenMapWin = GetWindow<OpenGamingMapWindow>();
			sm_OpenMapWin.Show();
		}

		private void OnEnable()
		{
			if (m_GamingMapList == null)
			{
				m_GamingMapList = new List<string>();
			}
			m_GamingMapList.Clear();
			GamingMapEditorSetting gamingSetting = GamingMapEditorUtility.GetGamingMapEditorSetting();
			if (gamingSetting != null)
			{
				m_GamingMapPath = gamingSetting.m_GamingMapPath;
			}
			string []GamingList =  AssetDatabase.FindAssets("t:Scene", new string[] { m_GamingMapPath });
			if(GamingList != null && GamingList.Length>0)
			{
				for(int iGame = 0;iGame<GamingList.Length;iGame++)
				{
					string gamingPath = GamingList[iGame];
					string assetPath = AssetDatabase.GUIDToAssetPath(GamingList[iGame]);
					if (assetPath.Contains("Area"))
					{
						continue;
					}
					m_GamingMapList.Add(assetPath);
				}
			}
		}
		private void OnGUI()
		{
			if (m_GamingMapList != null && m_GamingMapList.Count > 0)
			{
				assetScrollPos = GUILayout.BeginScrollView(assetScrollPos);
				GUILayout.BeginVertical("box");
				for (int iGaming = 0; iGaming < m_GamingMapList.Count; iGaming++)
				{
					/*if (iGaming % HORIZONTAL_COUNT == 0)
					{
						GUILayout.BeginHorizontal();
					}*/
					string[] splitPath = m_GamingMapList[iGaming].Split('/');
					if (splitPath != null && splitPath.Length > 0)
					{
						string showPath = splitPath[splitPath.Length - 1];
						if (!string.IsNullOrEmpty(showPath))
						{
							showPath = showPath.Replace(".unity", "");
							if (GUILayout.Button(showPath))
							{
								bool confirm = EditorUtility.DisplayDialog("提示", "是否保存当前场景", "确定", "取消");
								if (confirm)
								{
									SaveOpenScene();
								}
								EditorSceneManager.OpenScene(m_GamingMapList[iGaming], OpenSceneMode.Single);
								sm_OpenMapWin.Close();
							}
						}
					}
					/*if ((iGaming + 1) % HORIZONTAL_COUNT == 0)
					{
						GUILayout.EndHorizontal();
					}*/

				}
				GUILayout.EndVertical();
				GUILayout.EndScrollView();
			}


			if (GUILayout.Button("刷新"))
			{
				OnEnable();
			}

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
