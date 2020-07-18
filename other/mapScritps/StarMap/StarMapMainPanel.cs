#if UNITY_EDITOR
using EditorExtend;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Map
{
    [ExecuteInEditMode]
    public class StarMapMainPanel : MonoBehaviour
    {
        #region 属性
        /// <summary>
        /// 星图配置数据
        /// </summary>
        private List<StarMapVO> m_StarMapVoList;
        [ReadOnly]
        public List<FixedStarElement> m_FixedStarElements = new List<FixedStarElement>();
        [ReadOnly]
        public StarMapEditorRoot m_Root;
        private RectTransform m_RectTrans;

        /// <summary>
        /// 用来画线的
        /// </summary>
        private List<TrailRenderer> m_Trails = new List<TrailRenderer>();
        #endregion

        #region 挂载
        /// <summary>
        /// 需要生成星图的预设
        /// </summary>
        public GameObject m_StarMapPrefab;
        /// <summary>
        /// 存放星图的容器
        /// </summary>
        public Transform m_StarContainer;
        #endregion

        #region 方法

        public FixedStarElement GetElement(int starId)
        {
            if(m_FixedStarElements != null && m_FixedStarElements.Count>0)
            {
                for(int iStar = 0;iStar< m_FixedStarElements.Count;iStar++)
                {
                    if(m_FixedStarElements[iStar].m_FixedStarid == starId)
                    {
                        return m_FixedStarElements[iStar];
                    }
                }
            }
            return null;
        }

        public void Clear()
        {
            ClearStarMap();
        }
        private void ClearStarMap()
        {
            if (m_FixedStarElements != null && m_FixedStarElements.Count > 0)
            {
                for(int iStar = 0;iStar<m_FixedStarElements.Count;iStar++)
                {
                    FixedStarElement starElement = m_FixedStarElements[iStar];
                    if(starElement == null)
                    {
                        continue;
                    }
                    GameObject.DestroyImmediate(starElement.gameObject);
                }
                m_FixedStarElements.Clear();
            }

            if (m_StarContainer != null)
            {
                int childCount = m_StarContainer.childCount;
                if (childCount > 0)
                {
                    for (int iChild = childCount - 1; iChild >= 0; iChild--)
                    {
                        Transform childTrans = m_StarContainer.GetChild(iChild);
                        if (childTrans == null)
                        {
                            continue;
                        }

                        GameObject.DestroyImmediate(childTrans.gameObject);
                    }
                }
            }

        }

        /// <summary>
        /// 判断excel表中是否有新增或者删除 有的话刷新对应的
        /// </summary>
        private void Check()
        {
            if(m_StarMapVoList == null || m_StarMapVoList.Count<=0)
            {
                return;
            }
            ///判断是否有新增的 有的话添加
            for(int iStar = 0;iStar<m_StarMapVoList.Count;iStar++)
            {
                StarMapVO starMapVo = m_StarMapVoList[iStar];
                if(starMapVo == null)
                {
                    continue;
                }
                bool exist= ExistElement(starMapVo.FixedStarid);
                if(!exist)
                {
                    CreateFixedStarElement(starMapVo);
                }
            }
            //判断是否有删除的 有的话删除
            if(m_FixedStarElements != null && m_FixedStarElements.Count>0)
            {
                for(int iStar = m_FixedStarElements.Count-1; iStar>=0;iStar--)
                {
                    FixedStarElement element = m_FixedStarElements[iStar];
                    if(element == null || element.gameObject == null)
                    {
                        continue;
                    }
                    int fixedStarId = element.m_FixedStarid;
                    StarMapVO vo = ConfigVO<StarMapVO>.Instance.GetData(fixedStarId);
                    if(vo == null || vo.ID<=0)
                    {
                        GameObject.DestroyImmediate(element.gameObject);
                        m_FixedStarElements.Remove(element);
                    }
                }
            }
        }

        private bool ExistElement(int fixedStarId)
        {
            if(m_FixedStarElements != null && m_FixedStarElements.Count>0)
            {
                for(int iFixed = 0;iFixed<m_FixedStarElements.Count;iFixed++)
                {
                    FixedStarElement element = m_FixedStarElements[iFixed];
                    if(element == null || element.gameObject == null)
                    {
                        continue;
                    }
                    if(element.m_FixedStarid == fixedStarId)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        

        private void CreateFixedStarElement(StarMapVO starVo)
        {
            GameObject starObj = GameObject.Instantiate(m_StarMapPrefab);
            starObj.SetActive(true);
            starObj.transform.SetParent(m_StarContainer);
            if(m_Root.m_PreviewStarMap != null)
            {
                //starObj.transform.position =m_Root.GetPreviewStarMapPos(starVo.FixedStarid);
                RectTransform starRect = starObj.GetComponent<RectTransform>();
                if(starRect == null)
                {
                    starRect = starObj.AddComponent<RectTransform>();
                }
                starRect.anchoredPosition = m_Root.GetPreviewStarMapPos(starVo.FixedStarid);
            }
            else
            {
                starObj.transform.localPosition = Vector3.zero;
            }
            
            starObj.transform.localRotation = Quaternion.identity;
            starObj.transform.localScale = Vector3.one;
            FixedStarElement fixedElement = starObj.GetComponent<FixedStarElement>();
            if (fixedElement != null)
            {
                fixedElement.Init(this,starVo);
                m_FixedStarElements.Add(fixedElement);
            }
        }

        public void InitStarMap(StarMapEditorRoot root, bool needReset = false)
        {
            m_Root = root;
            m_StarMapVoList = ConfigVO<StarMapVO>.Instance.GetList();
            if(!needReset)
            {
                Check();
                return;
            }
            ClearStarMap();
            if (m_StarMapVoList != null && m_StarMapVoList.Count > 0)
            {
                for (int iStar = 0; iStar < m_StarMapVoList.Count; iStar++)
                {
                    StarMapVO starVo = m_StarMapVoList[iStar];
                    if (starVo == null)
                    {
                        continue;
                    }
                    CreateFixedStarElement(starVo);
                }
            }
        }

        public IEnumerator DoEditorUpdate(StarMapEditorRoot root)
        {
            m_Root = root;
            if(m_RectTrans != null)
            {
                m_RectTrans = GetComponent<RectTransform>();
            }
            if(m_RectTrans != null)
            {
                m_RectTrans.anchoredPosition = Vector2.zero;
            }
            yield return null;
            m_FixedStarElements.Clear();
            FixedStarElement[] elements = GetComponentsInChildren<FixedStarElement>(true);
            if(elements != null && elements.Length>0)
            {
                for(int iElement = 0;iElement<elements.Length;iElement++)
                {
                    m_FixedStarElements.Add(elements[iElement]);
                }
            }
            yield return null;
            if(m_FixedStarElements != null && m_FixedStarElements.Count>0)
            {
                for(int iStarElement = 0;iStarElement<m_FixedStarElements.Count;iStarElement++)
                {
                    FixedStarElement element = m_FixedStarElements[iStarElement];
                    if(element != null)
                    {
                        IEnumerator elementEnumer = element.DoEditorUpdate(this);
                        while(elementEnumer.MoveNext())
                        {
                            yield return null;
                        }
                    }
                }
            }
            yield return null;


            m_Trails.Clear();
            TrailRenderer[] trails = GetComponentsInChildren<TrailRenderer>();
            if (trails != null && trails.Length > 0)
            {
                for (int iTrail = 0; iTrail < trails.Length; iTrail++)
                {
                    m_Trails.Add(trails[iTrail]);
                    //trails[iTrail].Clear();
                }
            }
            yield return null;
            if(m_StarMapVoList != null && m_StarMapVoList.Count>0)
            {
                int trailIndex = 0;
                TrailRenderer trailRender = null;
                for (int iStar = 0;iStar<m_StarMapVoList.Count;iStar++)
                {
                    StarMapVO starMapVo = m_StarMapVoList[iStar];
                    if(starMapVo == null)
                    {
                        continue;
                    }
                    FixedStarElement beginElement = GetElement(starMapVo.FixedStarid);
                    if(beginElement == null)
                    {
                        continue;
                    }
                    int[] relationIds = starMapVo.Relation_Id;
                    if(relationIds != null && relationIds.Length>0)
                    {
                        for(int iRelation = 0;iRelation<relationIds.Length;iRelation++)
                        {
                            FixedStarElement endElement = GetElement(relationIds[iRelation]);
                            if(endElement == null)
                            {
                                continue;
                            }
                            if (m_Trails.Count> trailIndex)
                            {
                                trailRender = m_Trails[trailIndex];
                            }
                            else
                            {
                                GameObject trailObj = new GameObject("Trail_" + trailIndex);
                                trailRender = trailObj.AddComponent<TrailRenderer>();
                            }
                            trailRender.transform.SetParent(beginElement.transform);
                            trailIndex++;
                            trailRender.Clear();
                            trailRender.startWidth = 0.3f;
                            trailRender.AddPosition(beginElement.transform.position);
                            trailRender.AddPosition(endElement.transform.position);
                            trailRender.transform.position = endElement.transform.position;
                        }
                    }
                }
            }
        }
        #endregion
    }
}

#endif