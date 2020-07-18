#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Map
{
    /// <summary>
    /// 导出所有地图碰撞信息
    /// </summary>
    public class ExportMapColliders : ExportBase
    {
        private Map m_Map;
        private int m_AutoId;
        public override ExporterHandle BeginExport(params object[] exportPara)
        {
            if (exportPara == null || exportPara.Length < 1)
            {
                Debug.LogError("ExportMapColliders 参数异常");
                return null;
            }
            m_Map = exportPara[0] as Map;
            Physics.autoSyncTransforms = true;
            return base.BeginExport(exportPara);
        }
        

        protected override void DoEnd()
        {
            base.DoEnd();
            m_Map = null;
        }

        public IEnumerator GenerateAreaDecorate(EditorCollider outData, List<EditorDecorate> editorDecorateList)
        {
            if (m_Map.m_AreaSpawnerCache != null && m_Map.m_AreaSpawnerCache.Count > 0)
            {
                EditorColliderArea[] areaList = new EditorColliderArea[m_Map.m_AreaSpawnerCache.Count];
                outData.areaList = areaList;
                List<GameObject> prefabList = new List<GameObject>();
                for (int iArea = 0; iArea < m_Map.m_AreaSpawnerCache.Count; iArea++)
                {

                    editorDecorateList.Clear();
                    EditorColliderArea editorArea = new EditorColliderArea();
                    areaList[iArea] = editorArea;
                    AreaSpawner areaSpawner = m_Map.m_AreaSpawnerCache[iArea];
                    Area area = areaSpawner.GetArea();

                    string areaName = areaSpawner.GetAreaScenePath();
                    while (!string.IsNullOrEmpty(areaName) && area == null)
                    {
                        area = areaSpawner.GetArea();
                        yield return null;
                    }

                    editorArea.areaId = areaSpawner.GetAreaId();
                    if (area != null)
                    {
                        EditorGamingMapData.CorrectAreaColliderCenter(area);
                        yield return null;
                        yield return null;
                        prefabList.Clear();
                        MapEditorUtility.GetAllPrefab(area.transform, prefabList);
                        if (prefabList != null && prefabList.Count > 0)
                        {
                            for (int iUnit = 0; iUnit < prefabList.Count; iUnit++)
                            {
                                GameObject unit = prefabList[iUnit];
                                List<Transform> colliderRoots = MapEditorUtility.FindChilds<Transform>(unit.transform, "Collider");
                                if(colliderRoots == null || colliderRoots.Count<=0)
                                {
                                    continue;
                                }
                                for(int iRoot = 0; iRoot < colliderRoots.Count; iRoot++)
                                {
                                    Transform colliderRoot = colliderRoots[iRoot];
                                    if (colliderRoot != null)
                                    {
                                        Collider[] colliders = colliderRoot.GetComponentsInChildren<Collider>();
                                        if (colliders != null && colliders.Length > 0)
                                        {
                                            for (int iCollider = 0; iCollider < colliders.Length; iCollider++)
                                            {
                                                EditorUtility.DisplayProgressBar("GenerateAreaDecorate", string.Format("{0} {1}", area.name, colliders[iCollider].gameObject.name), (iArea + 1) * 1.0f / m_Map.m_AreaSpawnerCache.Count);
                                                Quaternion rot = colliders[iCollider].transform.rotation;
                                                colliders[iCollider].transform.rotation = Quaternion.identity;
                                                yield return null;
                                                EditorDecorate decorate = EditorGamingMapData.SaveColliderData(colliders[iCollider], rot, true);
                                                yield return null;
                                                colliders[iCollider].transform.rotation = rot;
                                                if (decorate != null)
                                                {
                                                    decorate.id = m_AutoId++;
                                                    editorDecorateList.Add(decorate);
                                                }
                                                CheckColliderLayer(colliders[iCollider]);
                                            }
                                        }
                                    }
                                }
                               
                            }
                        }
                        editorArea.decorateList = editorDecorateList.ToArray();
                    }
                    else
                    {
                        Debug.LogError(string.Format("Area {0} 未加载进来", areaSpawner.GetAreaId()));
                    }
                    yield return null;
                    EditorUtility.ClearProgressBar();
                }
            }
        }

        private void CheckColliderLayer(Collider collider)
        {
            if (collider.isTrigger)
            {
                return;
            }

            string layer = LayerMask.LayerToName(collider.gameObject.layer);
            if (!layer.Equals("SceneOnly"))
            {
                Debug.LogError(string.Format("{0}层不对，需要改成SceneOnly", EditorGamingMapData.GetParentName(collider.gameObject)));
            }
        }

        private IEnumerator GenerateResidentDecorate(List<EditorDecorate> editorDecorateList)
        {
            DontDestroyAtExport residentRoot = UnityEngine.Object.FindObjectOfType<DontDestroyAtExport>();
            if (residentRoot != null)
            {
                List<Transform> colliderRoots = MapEditorUtility.FindChilds<Transform>(residentRoot.transform,"Collider");
                if(colliderRoots == null || colliderRoots.Count<=0)
                {
                    yield break;
                }

                for(int iRoot =0;iRoot<colliderRoots.Count;iRoot++)
                {
                    Transform colliderRoot = colliderRoots[iRoot];
                    if (colliderRoot != null)
                    {
                        Collider[] colliders = colliderRoot.GetComponentsInChildren<Collider>();
                        if (colliders != null && colliders.Length > 0)
                        {
                            for (int iCollider = 0; iCollider < colliders.Length; iCollider++)
                            {
                                EditorGamingMapData.CorrectCollider(colliders[iCollider]);
                                yield return null;
                                yield return null;
                                Quaternion rot = colliders[iCollider].transform.rotation;
                                colliders[iCollider].transform.rotation = Quaternion.identity;
                                yield return null;
                                EditorDecorate decorate = EditorGamingMapData.SaveColliderData(colliders[iCollider], rot);
                                yield return null;
                                colliders[iCollider].transform.rotation = rot;
                                if (decorate != null)
                                {
                                    decorate.id = m_AutoId++;
                                    editorDecorateList.Add(decorate);
                                }
                                CheckColliderLayer(colliders[iCollider]);
                            }

                        }
                    }
                }
            }
            yield return null;
        }

        protected override IEnumerator DoExport()
        {
            List<AreaSpawner> areaSpawners = m_Map.GetAreaSpawnerList();
            if(areaSpawners!= null && areaSpawners.Count>0)
            {
                for(int iArea = 0;iArea<areaSpawners.Count;iArea++)
                {
                    AreaSpawner areaSpawner = areaSpawners[iArea];
                    IEnumerator areaUpdateEnum = areaSpawner.DoUpdate(m_Map,false);
                    while(areaUpdateEnum.MoveNext())
                    {
                        yield return null;
                    }
                }
            }
            IEnumerator loadAreaEnumerator = m_Map.LoadAllArea();
            while (loadAreaEnumerator.MoveNext())
            {
                yield return null;
            }
            EditorCollider outData = new EditorCollider();
            List<EditorDecorate> editorDecorateList = new List<EditorDecorate>();

            m_AutoId = 1;
            IEnumerator residentColliderEnumerator = GenerateAreaDecorate(outData, editorDecorateList);
            while (residentColliderEnumerator.MoveNext())
            {
                yield return null;
            }

            //常驻collider导出
            editorDecorateList.Clear();

            IEnumerator areaColliderEnumerator = GenerateResidentDecorate(editorDecorateList);
            while (areaColliderEnumerator.MoveNext())
            {
                yield return null;
            }

            outData.mapID = m_Map.Uid;
            outData.commondecorateList = editorDecorateList.ToArray();
            EditorGamingMapData.SaveColliderToJson(outData);

            yield return null;
            Area[] areaArray = UnityEngine.Object.FindObjectsOfType<Area>();
            if (areaArray != null && areaArray.Length > 0)
            {
                for (int iArea = areaArray.Length - 1; iArea >= 0; iArea--)
                {
                    GameObject.DestroyImmediate(areaArray[iArea].gameObject);
                    if (iArea % 10 == 0)
                    {
                        yield return null;
                    }
                }
            }
        }
    }
}

#endif