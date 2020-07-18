#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Map
{
    [ExecuteInEditMode]
    public class FixedStarPanel : MonoBehaviour
    {
        #region 属性
        /// <summary>
        /// 存放行星的容器
        /// </summary>
        public List<PlanetContainer> m_PlanetContains = new List<PlanetContainer>();
        /// <summary>
        /// 星图配置数据
        /// </summary>
        private List<StarMapVO> m_StarMapVoList;

        public StarMapEditorRoot m_Root;
        /// <summary>
        /// 容器中的y值
        /// </summary>
        public float m_ContainerY;
        private RectTransform m_RectTrans;
        #endregion

        #region 挂载
        /// <summary>
        /// 行星预设
        /// </summary>
        public GameObject m_PlanetElementPrefab;
        #endregion

        #region 方法

        public void Open(PlanetContainer container)
        {
            if(m_PlanetContains != null && m_PlanetContains.Count>0)
            {
                for(int iPlanet = 0;iPlanet<m_PlanetContains.Count;iPlanet++)
                {
                    m_PlanetContains[iPlanet].gameObject.SetActive(m_PlanetContains[iPlanet] == container);
                }
            }
        }

        public void Clear()
        {
            int childCount = transform.childCount;
            if (childCount > 0)
            {
                for (int iChild = childCount - 1; iChild >= 0; iChild--)
                {
                    GameObject.DestroyImmediate(transform.GetChild(iChild).gameObject);
                }
            }
            m_PlanetContains.Clear();
        }

        /// <summary>
        /// 判断excel表中是否有新增或者删除 有的话刷新对应的
        /// </summary>
        private void Check()
        {
            if (m_StarMapVoList == null || m_StarMapVoList.Count <= 0)
            {
                return;
            }
            ///判断是否有新增的 有的话添加
            for (int iStar = 0; iStar < m_StarMapVoList.Count; iStar++)
            {
                StarMapVO starMapVo = m_StarMapVoList[iStar];
                if (starMapVo == null)
                {
                    continue;
                }
                PlanetContainer container = ExistElement(starMapVo.FixedStarid);
                if (container == null)
                {
                    CreateFixedStarElement(starMapVo);
                }
                else
                {
                    List<EditorGamingMap> gamingMaps = new List<EditorGamingMap>();
                    GetGamingMap(starMapVo.FixedStarid, gamingMaps);
                    container.UpdateElements(gamingMaps);
                }
            }
            //判断是否有删除的 有的话删除
            if (m_PlanetContains != null && m_PlanetContains.Count > 0)
            {
                for (int iStar = m_PlanetContains.Count - 1; iStar >= 0; iStar--)
                {
                    PlanetContainer element = m_PlanetContains[iStar];
                    if (element == null || element.gameObject == null)
                    {
                        continue;
                    }
                    int fixedStarId = element.m_StarMapId;
                    StarMapVO vo = ConfigVO<StarMapVO>.Instance.GetData(fixedStarId);
                    if (vo == null)
                    {
                        GameObject.DestroyImmediate(element.gameObject);
                        m_PlanetContains.Remove(element);
                    }
                }
            }
        }

        private void CreateFixedStarElement(StarMapVO starVo)
        {
            GameObject container = new GameObject(starVo.Name);
            container.transform.SetParent(transform);
            container.transform.localPosition = Vector3.zero;
            container.transform.localScale = Vector3.one;
            container.SetActive(false);
            PlanetContainer planetContainer = container.AddComponent<PlanetContainer>();
            m_PlanetContains.Add(planetContainer);
            List<EditorGamingMap> gamingMaps = new List<EditorGamingMap>();
            GetGamingMap(starVo.FixedStarid, gamingMaps);
            planetContainer.Init(starVo.FixedStarid, this, gamingMaps);
        }

        private PlanetContainer ExistElement(int starId)
        {
            if (m_PlanetContains != null && m_PlanetContains.Count > 0)
            {
                for (int iFixed = 0; iFixed < m_PlanetContains.Count; iFixed++)
                {
                    PlanetContainer element = m_PlanetContains[iFixed];
                    if (element == null || element.gameObject == null)
                    {
                        continue;
                    }
                    if (element.m_StarMapId == starId)
                    {
                        return element;
                    }
                }
            }
            return null;
        }

        public void InitFixedStar(StarMapEditorRoot root,bool needReset = false)
        {
            m_Root = root;
            m_StarMapVoList = ConfigVO<StarMapVO>.Instance.GetList();
            if(!needReset)
            {
                Check();
                return;
            }
            Clear();
            if (m_StarMapVoList != null && m_StarMapVoList.Count > 0)
            {
                for (int iStar = 0; iStar < m_StarMapVoList.Count; iStar++)
                {
                    StarMapVO starVo = m_StarMapVoList[iStar];
                    CreateFixedStarElement(starVo);
                }
            }
        }

        private void GetGamingMap(int starId, List<EditorGamingMap> gamingMapList)
        {
            gamingMapList.Clear();
            List<EditorGamingMap> m_AllGamingMaps = m_Root.m_AllGamingMaps;
            if (m_AllGamingMaps != null && m_AllGamingMaps.Count > 0)
            {
                for (int iAllGaming = 0; iAllGaming < m_AllGamingMaps.Count; iAllGaming++)
                {
                    EditorGamingMap editorData = m_AllGamingMaps[iAllGaming];
                    if (editorData == null)
                    {
                        continue;
                    }
                    if (editorData.belongFixedStar == starId)
                    {
                        gamingMapList.Add(editorData);
                    }
                }
            }
        }

        public PlanetContainer GetElement(int fixedStarId)
        {
            if(m_PlanetContains != null && m_PlanetContains.Count>0)
            {
                for (int iContain = 0; iContain < m_PlanetContains.Count; iContain++)
                {
                    if (m_PlanetContains[iContain].m_StarMapId == fixedStarId)
                    {
                        return m_PlanetContains[iContain];
                     }
                }
            }
            return null;
        }

        public void ShowContainer(int starMapId)
        {
            if(m_PlanetContains != null && m_PlanetContains.Count>0)
            {
                for(int iContain = 0;iContain<m_PlanetContains.Count;iContain++)
                {
                    PlanetContainer container = m_PlanetContains[iContain];
                    if(container.m_StarMapId == starMapId)
                    {
                        container.gameObject.SetActive(true);
                        Selection.activeGameObject = container.gameObject;
                    }
                    else
                    {
                        container.gameObject.SetActive(false);
                    }
                    //container.gameObject.SetActive(container.m_StarMapId == starMapId);
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
            m_PlanetContains.Clear();
            PlanetContainer[] containers = GetComponentsInChildren<PlanetContainer>(true);
            if(containers != null && containers.Length>0)
            {
                for(int iContainer = 0;iContainer<containers.Length;iContainer++)
                {
                    m_PlanetContains.Add(containers[iContainer]);
                }
            }
            yield return null;
            if(m_PlanetContains != null && m_PlanetContains.Count>0)
            {
                for(int iPlanet = 0;iPlanet<m_PlanetContains.Count;iPlanet++)
                {
                    PlanetContainer container = m_PlanetContains[iPlanet];
                    if(container == null || container.gameObject ==null)
                    {
                        continue;
                    }
                    IEnumerator containerEnumer = container.DoEditorUpdate(this);
                    while(container != null && container.gameObject != null&& containerEnumer.MoveNext())
                    {
                        yield return null;
                    }
                    
                }
            }
        }
        #endregion
    }
}

#endif