#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Map
{
	public class ExporterAll
	{
		private IEnumerator m_ExportEnumerator;

		public void BeginExport()
		{
			m_ExportEnumerator = Export_Co();
			EditorApplication.update += OnUpdate;
		}


		private void EndExport()
		{
			EditorApplication.update -= OnUpdate;

			EditorUtility.DisplayDialog("MapEditor", "导出完成", "OK");
		}

		private void OnUpdate()
		{
			bool moveNext;
			try
			{
				moveNext = m_ExportEnumerator.MoveNext();
			}
			catch (Exception)
			{
				moveNext = false;
			}

			if (!moveNext)
			{
				EndExport();
			}
		}


		private IEnumerator Export_Co()
		{
			yield return null;

            ExportParameter exportParameter = MapEditorUtility.CreateExportParameter();
            exportParameter.ThrowExceptionAtAbort = true;
			exportParameter.ExportedUnitAddressableKeys = new HashSet<string>();

			string[] sceneGuids = AssetDatabase.FindAssets("t:Scene");
			for (int iScene = 0; iScene < sceneGuids.Length; iScene++)
			{
				Scene iterScene = EditorSceneManager.OpenScene(AssetDatabase.GUIDToAssetPath(sceneGuids[iScene]), OpenSceneMode.Single);
				GameObject[] rootGameObjects = iterScene.GetRootGameObjects();
				for (int iRootGameObject = 0; iRootGameObject < rootGameObjects.Length; iRootGameObject++)
				{
					GameObject iterGameObject = rootGameObjects[iRootGameObject];
					Map iterMap = iterGameObject.GetComponent<Map>();
					if (iterMap)
					{
						//ExporterHandle handle = new Exporter().BeginExport(iterMap, MapEditorUtility.GetOrCreateMapEditorSetting().ExportAllMapSetting, exportParameter);

						ExporterHandle handle = new Exporter().BeginExport(iterMap, MapEditorUtility.GetOrCreateMapEditorSetting().ExportAllMapSetting, exportParameter, iterMap.GetAreaSpawnerList(), true);
						while (!handle.IsDone)
						{
							yield return null;
						}
					}
				}
			}
		}
	}
}
#endif