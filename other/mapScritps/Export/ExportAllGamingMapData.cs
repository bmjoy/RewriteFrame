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
    /// 导出所有GamingMap数据
    /// </summary>
    public class ExportAllGamingMapData: ExportBase
    {
        private List<string> m_GamingMapPaths;

        protected override void DoBegin()
        {
            base.DoBegin();
            m_GamingMapPaths = ExportBase.GetGamingMapScenePath();
        }

        protected override void DoEnd()
        {
            base.DoEnd();
            m_GamingMapPaths = null;
            if (Application.isBatchMode)
            {
                EditorApplication.Exit(0);
            }
        }

        protected override  IEnumerator DoExport()
        {
            if(m_GamingMapPaths != null && m_GamingMapPaths.Count>0)
            {
                for(int iMap = 0;iMap< m_GamingMapPaths.Count;iMap++)
                {
                    string mapPath = m_GamingMapPaths[iMap];
                    Scene iterScene = EditorSceneManager.OpenScene(mapPath, OpenSceneMode.Single);
                    
                    GameObject[] rootGameObjects = iterScene.GetRootGameObjects();
                    for (int iRootGameObject = 0; iRootGameObject < rootGameObjects.Length; iRootGameObject++)
                    {
                        GameObject iterGameObject = rootGameObjects[iRootGameObject];
                        GamingMap iterMap = iterGameObject.GetComponent<GamingMap>();
                        if (iterMap)
                        {
                            Scene scene = iterMap.GetMapScene();
                            if (!scene.isLoaded)
                            {
                                iterMap.OpenMapScene();
                                scene = iterMap.GetMapScene();
                            }
                            while(!scene.isLoaded)
                            {
                                yield return null;
                            }
                            Map map = null;
                            GameObject[] rootObjs = scene.GetRootGameObjects();
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
                            while(map == null)
                            {
                                yield return null;
                            }
                            LeapOverview leapOverView = iterMap.GetComponentInChildren<LeapOverview>();
                            List<ulong> areaIds = null;
                            if (leapOverView != null)
                            {
                                areaIds = new List<ulong>();
                                List<Leap> allLeaps = leapOverView.m_LeapList;
                                if (allLeaps != null && allLeaps.Count > 0)
                                {
                                    for (int iLeap = 0; iLeap < allLeaps.Count; iLeap++)
                                    {
                                        if (allLeaps[iLeap].m_IsExportToJson)
                                        {
                                            areaIds.Add(allLeaps[iLeap].m_LeapId);
                                        }
                                    }
                                }
                            }
                            ExporterHandle handle = new ExportGamingMapData().BeginExport(map,iterMap, areaIds);
                            //ExporterHandle handle = iterMap.BeginExportGamingMap(iterMap, map);
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
}
#endif
