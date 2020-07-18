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
    /// <summary>
    /// 导出所有NavMesh数据
    /// </summary>
    public class ExportAllNavMesh : ExportBase
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

        protected override  IEnumerator DoExport()
        {
            if(m_MapPaths != null && m_MapPaths.Count>0)
            {
                for(int iMap = 0;iMap< m_MapPaths.Count;iMap++)
                {
                    string mapPath = m_MapPaths[iMap];
                    Scene iterScene = EditorSceneManager.OpenScene(mapPath, OpenSceneMode.Single);
                    while(!iterScene.isLoaded)
                    {
                        yield return null;
                    }
                    try
                    {
                        ExportScene.BakeNavMeshFromNav();
                    }
                    catch(Exception e)
                    {
                        Debug.LogError("导出NavMesh失败:"+ mapPath);
                    }
                    yield return null;
                }
            }
        }
    }
}
#endif
