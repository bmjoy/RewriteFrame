#if UNITY_EDITOR
using EditorExtend;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Map
{
    [ExecuteInEditMode]
    public class PlanetPanel : MonoBehaviour
    {
        [ReadOnly]
        public StarMapEditorRoot m_Root;
        [ReadOnly]
        public List<PlanetAreaContainer> m_PlanetAreaContainers = new List<PlanetAreaContainer>();
        public GameObject m_PlanetAreaElementPrefab;
        private RectTransform m_RectTrans;

        public void Open(PlanetAreaContainer areaContainer)
        {
            if(areaContainer == null)
            {
                return;
            }
            for(int iPlanet = 0;iPlanet<m_PlanetAreaContainers.Count;iPlanet++)
            {
                PlanetAreaContainer container = m_PlanetAreaContainers[iPlanet];
                container.gameObject.SetActive(container == areaContainer);
            }
        }

        public PlanetAreaContainer ExistElement(uint starId)
        {
            if (m_PlanetAreaContainers != null && m_PlanetAreaContainers.Count > 0)
            {
                for (int iFixed = 0; iFixed < m_PlanetAreaContainers.Count; iFixed++)
                {
                    PlanetAreaContainer element = m_PlanetAreaContainers[iFixed];
                    if (element == null || element.gameObject == null)
                    {
                        continue;
                    }
                    if (element.m_GamingMapId == starId)
                    {
                        return element;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// 判断excel表中是否有新增或者删除 有的话刷新对应的
        /// </summary>
        private void Check()
        {
            List<EditorGamingMap> allGamingMaps = m_Root.m_AllGamingMaps;
            if(allGamingMaps == null || allGamingMaps.Count<=0)
            {
                for (int iStar = m_PlanetAreaContainers.Count - 1; iStar >= 0; iStar--)
                {
                    PlanetAreaContainer element = m_PlanetAreaContainers[iStar];
                    if (element == null || element.gameObject == null)
                    {
                        continue;
                    }
                    GameObject.DestroyImmediate(element.gameObject);
                    m_PlanetAreaContainers.Remove(element);
                    Debug.LogError("PlanetPanel 删除了:"+ element.m_GamingMapId);
                }
                return;
            }

            ///判断是否有新增的 有的话添加
            for (int iStar = 0; iStar < allGamingMaps.Count; iStar++)
            {
                EditorGamingMap starMapVo = allGamingMaps[iStar];
                if (starMapVo == null)
                {
                    continue;
                }
                PlanetAreaContainer container = ExistElement(starMapVo.gamingmapId);
                if (container == null)
                {
                    CreatePlanetArea(starMapVo);
                    Debug.LogError("PlanetPanel 新增了:" + starMapVo.gamingmapId);
                }
                else
                {
                    container.UpdateElements(starMapVo.areaList);
                }
            }
            //判断是否有删除的 有的话删除
            if (m_PlanetAreaContainers != null && m_PlanetAreaContainers.Count > 0)
            {
                for (int iStar = m_PlanetAreaContainers.Count - 1; iStar >= 0; iStar--)
                {
                    PlanetAreaContainer element = m_PlanetAreaContainers[iStar];
                    if (element == null || element.gameObject == null)
                    {
                        continue;
                    }
                    uint fixedStarId = element.m_GamingMapId;
                    EditorGamingMap vo = GetMapData(fixedStarId);
                    if (vo == null)
                    {
                        GameObject.DestroyImmediate(element.gameObject);
                        m_PlanetAreaContainers.Remove(element);
                        Debug.LogError("PlanetPanel 删除了:" + element.m_GamingMapId);
                    }
                }
            }
        }

        private EditorGamingMap GetMapData(uint mapId)
        {
            List<EditorGamingMap> gamings = m_Root.m_AllGamingMaps;
            if(gamings == null || gamings.Count<=0)
            {
                return null;
            }
            for(int iGaming = 0;iGaming<gamings.Count;iGaming++)
            {
                if(gamings[iGaming].gamingmapId == mapId)
                {
                    return gamings[iGaming];
                }
            }
            return null;
        }
        public void Init(StarMapEditorRoot root,bool needReset = false)
        {
            m_Root = root;
            if(!needReset)
            {
                Check();
                return;
            }
            int childCount = transform.childCount;
            if (childCount > 0)
            {
                for (int iChild = childCount - 1; iChild >= 0; iChild--)
                {
                    GameObject.DestroyImmediate(transform.GetChild(iChild).gameObject);
                }
            }

            m_PlanetAreaContainers.Clear();

            List<EditorGamingMap> allGamingMaps = m_Root.m_AllGamingMaps;
            if(allGamingMaps != null && allGamingMaps.Count>0)
            {
                for (int iAllGaming = 0; iAllGaming < allGamingMaps.Count; iAllGaming++)
                {
                    EditorGamingMap editorData = allGamingMaps[iAllGaming];
                    if (editorData == null)
                    {
                        continue;
                    }
                    CreatePlanetArea(editorData);
                }
            }
        }

        private void CreatePlanetArea(EditorGamingMap editorData)
        {
            GameObject planetContainer = new GameObject(editorData.gamingmapName);
            planetContainer.SetActive(false);
            planetContainer.transform.SetParent(transform);
            planetContainer.transform.localPosition = Vector3.zero;
            planetContainer.transform.localScale = Vector3.one;
            PlanetAreaContainer planetAreaConatiner = planetContainer.AddComponent<PlanetAreaContainer>();
            m_PlanetAreaContainers.Add(planetAreaConatiner);
            EditorArea[] areas = editorData.areaList;
            planetAreaConatiner.Init(this, areas, editorData.gamingmapId);
        }

        public PlanetAreaElement GetElement(uint mapId,ulong areaId)
        {
            if(m_PlanetAreaContainers != null && m_PlanetAreaContainers.Count>0)
            {
                for(int iArea = 0;iArea<m_PlanetAreaContainers.Count;iArea++)
                {
                    PlanetAreaContainer container = m_PlanetAreaContainers[iArea];
                    if(container.m_GamingMapId == mapId)
                    {
                        return container.GetElement(areaId);
                    }

                }
            }
            return null;
        }

        public void ShowContainer(uint gamingMapId)
        {
            if(m_PlanetAreaContainers != null && m_PlanetAreaContainers.Count>0)
            {
                for(int iContainer = 0;iContainer<m_PlanetAreaContainers.Count;iContainer++)
                {
                    PlanetAreaContainer container = m_PlanetAreaContainers[iContainer];
                    if(container.m_GamingMapId == gamingMapId)
                    {
                        container.gameObject.SetActive(true);
                        Selection.activeGameObject = container.gameObject;
                    }
                    else
                    {
                        container.gameObject.SetActive(false);
                    }
                    //container.gameObject.SetActive(container.m_GamingMapId == gamingMapId);
                }
            }
        }

        public IEnumerator DoEditorUpdate(StarMapEditorRoot root)
        {
            m_Root = root;
            if(m_RectTrans == null)
            {
                m_RectTrans = GetComponent<RectTransform>();
            }
            if(m_RectTrans != null)
            {
                m_RectTrans.anchoredPosition = Vector2.zero;
            }
            yield return null;
            m_PlanetAreaContainers.Clear();
            PlanetAreaContainer[] containers = GetComponentsInChildren<PlanetAreaContainer>(true);
            if(containers!= null&& containers.Length>0)
            {
                for(int iContainer = 0;iContainer<containers.Length;iContainer++)
                {
                    m_PlanetAreaContainers.Add(containers[iContainer]);
                }
            }
            yield return null;
            if(m_PlanetAreaContainers != null && m_PlanetAreaContainers.Count>0)
            {
                for(int iPlanet = 0;iPlanet<m_PlanetAreaContainers.Count;iPlanet++)
                {
                    PlanetAreaContainer container = m_PlanetAreaContainers[iPlanet];
                    if(container == null || container.gameObject == null)
                    {
                        continue;
                    }
                    IEnumerator containerEnumera = container.DoEditorUpdate(this);
                    while(container != null && containerEnumera.MoveNext())
                    {
                        yield return null;
                    }
                }
            }
        }
    }
}

#endif