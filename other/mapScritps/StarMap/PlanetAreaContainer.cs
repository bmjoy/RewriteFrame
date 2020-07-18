#if UNITY_EDITOR
using EditorExtend;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Map
{
    [ExecuteInEditMode]
    public class PlanetAreaContainer : MonoBehaviour
    {
        [ReadOnly]
        [Tooltip("恒星Id")]
        public uint m_GamingMapId;
        [ReadOnly]
        public PlanetPanel m_Panel;
        [ReadOnly]
        public List<PlanetAreaElement> m_PlanetAreas = new List<PlanetAreaElement>();

        /// <summary>
        /// 实际地图宽度
        /// </summary>
        [Range(0, 100000)]
        [Tooltip("实际场景地图宽度")]
        public float m_ActMapWidth = 100000;
        /// <summary>
        /// 实际地图高度
        /// </summary>
        [Range(0, 100000)]
        [Tooltip("实际场景地图高度")]
        public float m_ActMapHeight = 100000;

        /// <summary>
        /// 映射到地图的图片宽度
        /// </summary>
        //[Range(0,5000)]
        //public float m_MiniMapWidth = 1920;

        /// <summary>
        /// 映射到地图的图片高度
        /// </summary>
        //[Range(0,5000)]
        // public float m_MiniMapHeight = 1080;

        /// <summary>
        /// 映射到地图的图片大小
        /// </summary>

        [Range(500,5000)]
        [Tooltip("映射ui地图大小")]
        public float m_MiniMapSize=2000;

        /// <summary>
        /// 偏移宽度
        /// </summary>
        //[Range(-5000,5000)]
        //public float m_OffestX;
        /// <summary>
        /// 偏移高度
        /// </summary>
        //[Range(-5000,5000)]
        //public float m_OffestY;

        /// <summary>
        /// 迷你地图
        /// </summary>
        public RectTransform m_MiniMap;

        /// <summary>
        /// 恒星资源
        /// </summary>
        [Tooltip("恒星资源")]
        public GameObject m_FixedStarObj;

        [ReadOnly]
        [Tooltip("恒星资源名称")]
        public string m_FixedStarRes;

        [Tooltip("恒星资源大小")]
        public Vector2 m_FixedStarScale;

        [Tooltip("恒星资源位置")]
        public Vector2 m_FixedStarPos;

        [EditorExtend.Button("刷新地图", "RefreshMap", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)]
        public bool m_RefreshMapTag;

        private Image m_FixedStarContainer;

        private void RefreshMap()
        {
            float widthRate = m_MiniMapSize / m_ActMapWidth;
            float heightRate = m_MiniMapSize / m_ActMapHeight;
            if(m_PlanetAreas != null && m_PlanetAreas.Count>0)
            {
                for(int iArea = 0;iArea<m_PlanetAreas.Count;iArea++)
                {
                    PlanetAreaElement element = m_PlanetAreas[iArea];
                    EditorArea areaData = element.m_AreaData;
                    if(areaData == null)
                    {
                        continue;
                    }
                    EditorPosition editorPosition = areaData.position;
                    element.GetComponent<RectTransform>().anchoredPosition = new Vector2(editorPosition.x * widthRate, editorPosition.z * heightRate);
                    //element.transform.position = new Vector3(editorPosition.x*widthRate- m_MiniMapWidth/2,editorPosition.z*heightRate - m_MiniMapHeight/2,0);
                }
            }

            if(m_MiniMap == null)
            {
                Transform miniMap = transform.Find("map");
                RectTransform miniMapRect = null;
                if (miniMap == null)
                {
                    GameObject imageObj = new GameObject("map");
                    imageObj.transform.SetParent(transform);
                    Image image = imageObj.AddComponent<Image>();
                    image.color = new Color(1,0,0,0.5f);
                    miniMapRect = image.rectTransform;
                }
                else
                {
                    miniMapRect = miniMap.GetComponent<RectTransform>();
                }
                m_MiniMap = miniMapRect;
            }
            m_MiniMap.SetAsFirstSibling();
            m_MiniMap.anchoredPosition3D = Vector3.zero;
            m_MiniMap.sizeDelta = m_MiniMapSize * Vector2.one;
        }

        public string GetStarRes()
        {
            if(string.IsNullOrEmpty(m_FixedStarRes) && m_FixedStarObj != null)
            {
                m_FixedStarRes = m_FixedStarObj.name;
            }
            return m_FixedStarRes;
        }

        public void UpdateElements(EditorArea[] areas)
        {
            if(areas == null || areas.Length<=0)
            {
                if (m_PlanetAreas != null && m_PlanetAreas.Count > 0)
                {
                    for(int iPlanet = m_PlanetAreas.Count -1; iPlanet>=0;iPlanet--)
                    {
                        PlanetAreaElement areaElement = m_PlanetAreas[iPlanet];
                        if(areaElement != null)
                        {
                            if(areaElement.m_AreaData != null)
                            {
                                Debug.LogError("PlanetAreaContainer " + m_GamingMapId + " 删除了 " + areaElement.m_AreaData.areaId);
                            }
                            m_PlanetAreas.Remove(areaElement);
                            GameObject.DestroyImmediate(areaElement.gameObject);
                        }
                    }
                }
                return;
            }

            for(int iArea = 0;iArea< areas.Length;iArea++)
            {
                EditorArea editorArea = areas[iArea];
                PlanetAreaElement element = GetElement(editorArea.areaId);
                if(element == null)
                {
                    CreateElement(editorArea,null);
                    Debug.LogError("PlanetAreaContainer " + m_GamingMapId + " 新增了 " + editorArea.areaId);
                }
                else
                {
                    element.Init(editorArea,null);
                }
            }

            if (m_PlanetAreas != null && m_PlanetAreas.Count > 0)
            {
                for(int iPlanet = m_PlanetAreas.Count -1;iPlanet>=0;iPlanet--)
                {
                    PlanetAreaElement areaElement = m_PlanetAreas[iPlanet];
                    EditorArea editorArea = GetArea(areaElement.m_AreaData.areaId, areas);
                    if(editorArea == null)
                    {
                        if (areaElement.m_AreaData != null)
                        {
                            Debug.LogError("PlanetAreaContainer " + m_GamingMapId + " 删除了 " + areaElement.m_AreaData.areaId);
                        }
                        m_PlanetAreas.Remove(areaElement);
                        GameObject.DestroyImmediate(areaElement.gameObject);
                    }
                }
            }
        }
        
        private EditorArea GetArea(ulong areaId,EditorArea[] areas)
        {
            if(areas == null || areas.Length<=0)
            {
                return null;
            }
            for(int iArea = 0;iArea<areas.Length;iArea++)
            {
                if(areas[iArea].areaId == areaId)
                {
                    return areas[iArea];
                }
            }
            return null;
        }

        public void Init(PlanetPanel panel, EditorArea[] areas,uint mapId)
        {
            m_Panel = panel;
            m_GamingMapId = mapId;
            m_PlanetAreas.Clear();
            if (areas != null && areas.Length > 0)
            {
                EditorPlanet planet = m_Panel.m_Root.GetPreviewPlanet(mapId);
                if(planet != null)
                {
                    m_MiniMapSize = planet.minimapSize;
                    m_FixedStarRes = planet.bgmapRes;
                    m_FixedStarScale = planet.bgmapScale.ToVector2();
                    if(m_FixedStarContainer != null)
                    {
                        m_FixedStarContainer.rectTransform.sizeDelta = m_FixedStarScale;
                    }
                    
                    m_FixedStarPos = planet.bgmapPos.ToVector2();
                    m_FixedStarObj = StarMapEditorRoot.FindResAsset(m_FixedStarRes);
                }
                for (int iArea = 0; iArea < areas.Length; iArea++)
                {
                    EditorArea editorArea = areas[iArea];
                    if (editorArea == null)
                    {
                        continue;
                    }
                    CreateElement(editorArea, planet);
                }
            }
            RefreshMap();
        }

        private void CreateElement(EditorArea editorArea,EditorPlanet planet)
        {
            GameObject planetArea = GameObject.Instantiate(m_Panel.m_PlanetAreaElementPrefab);
            planetArea.SetActive(true);
            planetArea.transform.SetParent(transform);
            planetArea.transform.localPosition = Vector3.zero;
            PlanetAreaElement planetElement = planetArea.GetComponent<PlanetAreaElement>();
            if (planetElement != null)
            {
                EditorStarMapArea starMapArea = m_Panel.m_Root.GetPreviewArea(planet, editorArea.areaId);
                planetElement.Init(editorArea, starMapArea);
                m_PlanetAreas.Add(planetElement);
            }
        }

        public PlanetAreaElement GetElement(ulong areaId)
        {
            if(m_PlanetAreas != null && m_PlanetAreas.Count>0)
            {
                for(int iPlanet = 0;iPlanet<m_PlanetAreas.Count;iPlanet++)
                {
                    PlanetAreaElement element = m_PlanetAreas[iPlanet];
                    if(element.m_AreaData.areaId == areaId)
                    {
                        return element;
                    }
                }
            }
            return null;
        }

        public IEnumerator DoEditorUpdate(PlanetPanel panel)
        {
            m_Panel = panel;
            transform.localPosition = Vector3.zero;
            yield return null;
            m_PlanetAreas.Clear();
            PlanetAreaElement[] elements = GetComponentsInChildren<PlanetAreaElement>(true);
            if(elements != null && elements.Length>0)
            {
                for(int iElement = 0;iElement<elements.Length;iElement++)
                {
                    m_PlanetAreas.Add(elements[iElement]);
                }
            }
            yield return null;
            if(m_PlanetAreas != null && m_PlanetAreas.Count>0)
            {
                for(int iPlanet = 0;iPlanet<m_PlanetAreas.Count;iPlanet++)
                {
                    PlanetAreaElement element = m_PlanetAreas[iPlanet];
                    IEnumerator elementEnumera = element.DoEditorUpdate(this);
                    while(element !=null && element.gameObject != null && elementEnumera.MoveNext())
                    {
                        yield return null;
                    }
                }
            }
            yield return null;
            if(m_MiniMap != null)
            {
                m_MiniMap.localScale = Vector3.one;
            }
            yield return null;
            Transform containerTrans = transform.Find("FixedStarContainer");
           
            if(containerTrans == null)
            {
                GameObject fixedContainer = new GameObject("FixedStarContainer");
                fixedContainer.transform.SetParent(transform);
                Image image = fixedContainer.AddComponent<Image>();
                m_FixedStarContainer = image;
            }
            else
            {
                m_FixedStarContainer = containerTrans.GetOrAddComponent<Image>(true);
            }
            m_FixedStarContainer.rectTransform.anchoredPosition3D = m_FixedStarPos;
            m_FixedStarContainer.rectTransform.sizeDelta = m_FixedStarScale;
            m_FixedStarContainer.transform.localScale = Vector3.one;
            if (m_FixedStarContainer != null)
            {
                if (m_MiniMap != null)
                {
                    int miniIndex = m_MiniMap.GetSiblingIndex();
                    m_FixedStarContainer.transform.SetSiblingIndex(miniIndex + 1);
                }
                else
                {
                    m_FixedStarContainer.transform.SetSiblingIndex(0);
                }
            }
            yield return null;
            if(m_FixedStarObj != null)
            {
                m_FixedStarRes = m_FixedStarObj.name;
            }
            else
            {
                m_FixedStarRes = "";
            }

            //if(m_FixedStarContainer != null)
            //{
            //    int childCount = m_FixedStarContainer.transform.childCount;
            //    if(m_FixedStarObj != null)
            //    {
            //        GameObject fixedStarObj = null;
            //        if (childCount > 0)
            //        {
            //            fixedStarObj = m_FixedStarContainer.GetChild(0).gameObject;
            //            if(fixedStarObj.name != m_FixedStarObj.name)
            //            {
            //                GameObject.DestroyImmediate(fixedStarObj);
            //                childCount = m_FixedStarContainer.transform.childCount;
            //            }
            //        }
            //        if(childCount <=0)
            //        {
            //            fixedStarObj = GameObject.Instantiate(m_FixedStarObj, m_FixedStarContainer);
            //            fixedStarObj.name = m_FixedStarObj.name;
            //        }
            //        fixedStarObj.transform.localPosition = Vector3.zero; ;
            //        fixedStarObj.transform.localScale = Vector3.one;
            //    }
            //    else
            //    {
            //        for (int iChild = childCount - 1; iChild >= 0; iChild--)
            //        {
            //            GameObject.DestroyImmediate(m_FixedStarContainer.GetChild(iChild).gameObject);
            //        }
            //    }
                
            //}
        }
        
    }
}

#endif