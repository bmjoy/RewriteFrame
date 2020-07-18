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
    /// 导出所有空间寻路
    /// </summary>
    public class ExportAll3DNavMesh : ExportBase
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
                for(int iMap = 0;iMap< m_MapPaths.Count;iMap++)
                {
                    string mapPath = m_MapPaths[iMap];
                    Scene iterScene = EditorSceneManager.OpenScene(mapPath, OpenSceneMode.Single);
                    while(!iterScene.isLoaded)
                    {
                        yield return null;
                    }

                    Map map = null;
                    GameObject[] rootObjs = iterScene.GetRootGameObjects();
                    if (rootObjs != null && rootObjs.Length > 0)
                    {
                        for (int iRoot = 0; iRoot < rootObjs.Length; iRoot++)
                        {
                            GameObject obj = rootObjs[iRoot];
                            map = obj.GetComponent<Map>();
                            if (map != null)
                            {
                                break;
                            }
                        }
                    }
                    while (map == null)
                    {
                        yield return null;
                    }

                    ExporterHandle exportHandler = new Export3DNavMesh().BeginExport(map, RootScenceRecast.ROOT_SCENCE_CUT_SIZE.RSCS_2000);
                    while(!exportHandler.IsDone)
                    {
                        yield return null;
                    }
                }
            }
        }
    }
}
#endif
