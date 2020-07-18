#if UNITY_EDITOR
using EditorExtend;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Map
{
    /// <summary>
    /// 恒星容器
    /// </summary>
    [ExecuteInEditMode]
    public class PlanetContainer : MonoBehaviour
    {
        [ReadOnly]
        public int m_StarMapId;
        public FixedStarPanel m_FixedStarPanel;

        [ReadOnly]
        public List<PlanetElement> m_PlanetElements = new List<PlanetElement>();

        public void Init(int  starMapId,FixedStarPanel panel,List<EditorGamingMap> gamingMaps)
        {
            m_StarMapId = starMapId;
            m_FixedStarPanel = panel;
            if (gamingMaps != null && gamingMaps.Count > 0)
            {
                for (int iGaming = 0; iGaming < gamingMaps.Count; iGaming++)
                {
                    EditorGamingMap editorData = gamingMaps[iGaming];
                    if (editorData == null)
                    {
                        continue;
                    }
                    CreateElement(editorData);
                }
            }
        }
        
        private void CreateElement(EditorGamingMap editorData)
        {
            GameObject planetObj = Instantiate(m_FixedStarPanel.m_PlanetElementPrefab);
            planetObj.SetActive(true);
            planetObj.transform.SetParent(transform);
            
            PlanetElement planetElement = planetObj.GetComponent<PlanetElement>();
            if (planetElement != null)
            {
                EditorPlanet planet = m_FixedStarPanel.m_Root.GetPreviewPlanet(editorData.gamingmapId);
                if (planet != null)
                {
                    planetElement.Init(editorData, planet);
                    //planetObj.transform.localPosition = Vector3.zero;
                }
                else
                {
                    planetElement.Init(editorData, null);
                    //planetObj.transform.localPosition = Vector3.zero;
                    Debug.LogError("PlanetContainer "+m_StarMapId+"新增了:" + editorData.gamingmapId);
                }
                m_PlanetElements.Add(planetElement);
            }
        }

        public void UpdateElements(List<EditorGamingMap> gamingMaps)
        {
            if(gamingMaps == null || gamingMaps.Count<=0)
            {
                for (int iElement = m_PlanetElements.Count - 1; iElement >= 0; iElement--)
                {
                    PlanetElement planetElement = m_PlanetElements[iElement];
                    if(planetElement != null)
                    {
                        GameObject.DestroyImmediate(planetElement.gameObject);
                        EditorGamingMap gamingMap = planetElement.m_StarData;
                        if(gamingMap != null)
                        {
                            Debug.LogError("PlanetContainer " + m_StarMapId + " 删除了" + gamingMap.gamingmapId);
                        }
                    }
                }
                return;
            }

            for (int iGaming = 0; iGaming < gamingMaps.Count; iGaming++)
            {
                EditorGamingMap editorData = gamingMaps[iGaming];
                if (editorData == null)
                {
                    continue;
                }
                PlanetElement element = GetElement(editorData.gamingmapId);
                if(element == null)
                {
                    CreateElement(editorData);
                    if (editorData != null)
                    {
                        Debug.LogError("PlanetContainer " + m_StarMapId + "新增了" + editorData.gamingmapId);
                    }
                }
                else
                {
                    element.Init(editorData,null);
                }
            }

            if(m_PlanetElements != null && m_PlanetElements.Count>0)
            {
                for(int iElement = m_PlanetElements.Count-1; iElement>=0;iElement--)
                {
                    PlanetElement planetElement = m_PlanetElements[iElement];
                    if(planetElement == null)
                    {
                        continue;
                    }
                    EditorGamingMap editorGamingMap = planetElement.m_StarData;
                    if(editorGamingMap != null)
                    {
                        bool exist = ExistsGamingMap(gamingMaps, editorGamingMap.gamingmapId);
                        if(!exist)
                        {
                            m_PlanetElements.Remove(planetElement);
                            GameObject.DestroyImmediate(planetElement.gameObject);
                            EditorGamingMap gamingMap = planetElement.m_StarData;
                            if (gamingMap != null)
                            {
                                Debug.LogError("PlanetContainer " + m_StarMapId + "删除了" + gamingMap.gamingmapId);
                            }
                        }
                    }
                }
            }
        }

        private bool ExistsGamingMap(List<EditorGamingMap> gamingMaps,uint gamingMapId)
        {
            if(gamingMaps == null || gamingMaps.Count<=0)
            {
                return false;
            }
            for(int iGaming = 0;iGaming<gamingMaps.Count;iGaming++)
            {
                if(gamingMaps[iGaming].gamingmapId == gamingMapId)
                {
                    return true;
                }
            }
            return false;
        }

        public PlanetElement GetElement(uint gamingMapId)
        {
            if(m_PlanetElements != null && m_PlanetElements.Count>0)
            {
                for(int iElement = 0;iElement<m_PlanetElements.Count;iElement++)
                {
                    if(m_PlanetElements[iElement].m_StarData.gamingmapId == gamingMapId)
                    {
                        return m_PlanetElements[iElement];
                    }
                }
            }
            return null;
        }

        public IEnumerator DoEditorUpdate(FixedStarPanel panel)
        {
            m_FixedStarPanel = panel;
            //Vector3 position = transform.localPosition;
            //transform.localPosition = new Vector3(position.x,panel.m_ContainerY,position.z);
            transform.localPosition = Vector3.zero;
            yield return null;
            m_PlanetElements.Clear();
            PlanetElement[] elements = GetComponentsInChildren<PlanetElement>(true);
            if(elements != null && elements.Length>0)
            {
                for(int iElement = 0;iElement<elements.Length;iElement++)
                {
                    m_PlanetElements.Add(elements[iElement]);
                }
            }
            yield return null;
            if(m_PlanetElements != null && m_PlanetElements.Count>0)
            {
                for(int iPlanet = 0;iPlanet<m_PlanetElements.Count;iPlanet++)
                {
                    PlanetElement element = m_PlanetElements[iPlanet];
                    if(element == null || element.gameObject == null)
                    {
                        continue;
                    }
                    IEnumerator elementEnumera = element.DoEditorUpdate(this, panel.m_ContainerY);
                    while(elementEnumera.MoveNext())
                    {
                        yield return null;
                    }
                }
            }
        }
    }
}

#endif