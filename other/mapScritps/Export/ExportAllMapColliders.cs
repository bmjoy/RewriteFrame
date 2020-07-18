#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Map
{
    /// <summary>
    /// 导出所有Map碰撞信息
    /// </summary>
    public class ExportAllMapColliders:ExportBase
    {
        private List<string> m_MapPaths;

        protected override void DoBegin()
        {
            base.DoBegin();

            m_MapPaths = ExportBase.GetMapScenePath();
        }

        protected override void DoEnd()
        {
            base.DoEnd();
            m_MapPaths = null;
            if (Application.isBatchMode)
            {
                EditorApplication.Exit(0);
            }
        }

        protected override IEnumerator DoExport()
        {
            if(m_MapPaths != null && m_MapPaths.Count>0)
            {
                Debug.LogError("需要导出的地图数量:"+ m_MapPaths.Count);
                for(int iMap = 0;iMap<m_MapPaths.Count;iMap++)
                {
                    string mapPath = m_MapPaths[iMap];
                    Debug.Log("正在导出:"+ mapPath);
                    Scene iterScene = EditorSceneManager.OpenScene(mapPath, OpenSceneMode.Single);
                    GameObject[] rootGameObjects = iterScene.GetRootGameObjects();
                    for (int iRootGameObject = 0; iRootGameObject < rootGameObjects.Length; iRootGameObject++)
                    {
                        GameObject iterGameObject = rootGameObjects[iRootGameObject];
                        Map iterMap = iterGameObject.GetComponent<Map>();
                        if (iterMap)
                        {
                            ExporterHandle handle = new ExportMapColliders().BeginExport(iterMap);
                            while (!handle.IsDone)
                            {
                                yield return null;
                            }
                        }
                    }
                    Debug.Log("导出成功:" + mapPath);
                }
            }
        }
    }
}
#endif
