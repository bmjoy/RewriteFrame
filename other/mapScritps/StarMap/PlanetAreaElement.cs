#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Map
{
    [ExecuteInEditMode]
    public class PlanetAreaElement : MonoBehaviour
    {
        public string m_Res;
        public EditorArea m_AreaData;
        public PlanetAreaContainer m_Container;
        public GameObject m_Resobj;
        private RectTransform rectTrans;
        /// <summary>
        /// 用来画线的
        /// </summary>
        private List<TrailRenderer> m_Trails = new List<TrailRenderer>();

        private Text m_Tip;
        public void Init(EditorArea area,EditorStarMapArea starMapArea)
        {
            if(area == null)
            {
                return;
            }
            if (rectTrans == null)
            {
                rectTrans = GetComponent<RectTransform>();
            }

            if (starMapArea != null)
            {
                m_Res = starMapArea.area_res;
                rectTrans.anchoredPosition = starMapArea.position.ToVector2();
                m_Resobj = StarMapEditorRoot.FindResAsset(m_Res);
            }
            m_AreaData = area;
            gameObject.name = area.areaName;
            if (m_Tip == null)
            {
                Transform tipTrans = transform.Find("tip");
                if (tipTrans != null)
                {
                    m_Tip = tipTrans.gameObject.GetComponent<Text>();
                }
            }
            
            if (m_Tip != null)
            {
                m_Tip.text = "";
                EditorLeap[] leaps = area.leapList;
                if(leaps != null && leaps.Length>0)
                {
                    EditorLeap leap = leaps[0];
                    if(leap.leapType == (int)LeapType.Main)
                    {
                        m_Tip.text = "主";
                    }
                    else if(leap.leapType == (int)LeapType.Child)
                    {
                        m_Tip.text = "副";
                    }
                }
               
            }
        }

        

        public Vector2 GetPosition()
        {
            if(m_Container == null)
            {
                return Vector2.zero;
            }
            if(rectTrans == null)
            {
                rectTrans = GetComponent<RectTransform>();
            }
            if(rectTrans != null)
            {
                return rectTrans.anchoredPosition;
            }
            else
            {
                Vector3 position = transform.position - m_Container.m_Panel.m_Root.GetRootPos();
                return new Vector2(position.x, position.y);
            }

        }
        public IEnumerator DoEditorUpdate(PlanetAreaContainer container)
        {
            m_Container = container;
            if(m_Resobj != null)
            {
                m_Res = m_Resobj.name;
            }
            else
            {
                m_Res = "";
            }
            transform.localScale = Vector3.one;
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

            if (m_AreaData != null)
            {
                ulong[] childArea = m_AreaData.childrenAreaList;
                if (childArea != null && childArea.Length > 0)
                {
                    TrailRenderer trailRender = null;
                    for (int iChild = 0;iChild<childArea.Length;iChild++)
                    {

                        if(m_Trails.Count>iChild)
                        {
                            trailRender = m_Trails[iChild];
                        }
                        else
                        {
                            GameObject trailObj = new GameObject("Trail_"+iChild);
                            trailObj.transform.SetParent(transform);
                            trailRender = trailObj.AddComponent<TrailRenderer>();
                        }
                        
                        PlanetAreaElement targetElement = m_Container.GetElement(childArea[iChild]);
                        if(targetElement != null)
                        {
                            trailRender.Clear();
                            trailRender.startWidth = 0.3f;
                            trailRender.AddPosition(transform.position);
                            trailRender.AddPosition(targetElement.transform.position);
                            trailRender.transform.position = targetElement.transform.position;
                        }
                    }
                }
            }
        }
    }
}
#endif
