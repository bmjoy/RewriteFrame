#if UNITY_EDITOR
using Map;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Map
{
    [ExecuteInEditMode]
    public class PlanetElement : MonoBehaviour
    {
        public EditorGamingMap m_StarData;
        /// <summary>
        /// 资源
        /// </summary>
        public string m_Res;

        public PlanetContainer m_Container;

        public GameObject m_ResObj;
        /// <summary>
        /// 资源的大小
        /// </summary>
        public Vector2 m_ResScale;
        private RectTransform m_RectTrans;

        public void Init(EditorGamingMap gamingData, EditorPlanet planet)
        {
            if (gamingData == null)
            {
                return;
            }
            m_StarData = gamingData;
            gameObject.name = gamingData.gamingmapName;
            if (m_RectTrans == null)
            {
                m_RectTrans = GetComponent<RectTransform>();
            }
            if (planet != null)
            {
                m_Res = planet.gamingmapRes;
                m_RectTrans.anchoredPosition = planet.position.ToVector2();
                m_ResScale = planet.scale.ToVector2();
                m_RectTrans.sizeDelta = m_ResScale;
                m_ResObj = StarMapEditorRoot.FindResAsset(m_Res);
            }
        }

        public Vector2 GetScale()
        {
            return m_ResScale;
        }

        public Vector2 GetPosition()
        {
            if(m_Container == null)
            {
                return Vector2.zero;
            }
            if(m_RectTrans == null)
            {
                m_RectTrans = GetComponent<RectTransform>();
            }
            if(m_RectTrans != null)
            {
                return m_RectTrans.anchoredPosition;
            }
            else
            {
                Vector3 position = transform.position - m_Container.m_FixedStarPanel.m_Root.GetRootPos();
                return new Vector2(position.x, position.y);
            }
        }

        public IEnumerator DoEditorUpdate(PlanetContainer container, float offestY)
        {
            m_Container = container;
            if(m_RectTrans == null)
            {
                m_RectTrans = GetComponent<RectTransform>();
            }
            if(m_RectTrans != null)
            {
                Vector2 anchorPos = m_RectTrans.anchoredPosition;
                m_RectTrans.anchoredPosition3D = new Vector3(anchorPos.x, offestY,0);
                m_RectTrans.sizeDelta = m_ResScale;
            }
            else
            {
                Vector3 position = transform.localPosition;
                if (position.y != 0)
                {
                    transform.localPosition = new Vector3(position.x, offestY, position.z);
                }
            }
            if(m_ResObj != null)
            {
                m_Res = m_ResObj.name;
            }
            else
            {
                m_Res = "";
            }
            //int childCount = transform.childCount;
            //if(m_ResObj != null)
            //{
            //    m_Res = m_ResObj.name;
            //    Transform trans = null;
            //    if(childCount>0)
            //    {
            //        trans = transform.GetChild(0);
            //        if(trans.name != m_Res)
            //        {
            //            GameObject.DestroyImmediate(trans.gameObject);
            //            childCount = transform.childCount;
            //        }
            //    }
            //    if(childCount <=0)
            //    {
            //        trans = GameObject.Instantiate(m_ResObj,transform).transform;
            //        trans.gameObject.name = m_Res;
            //    }
            //    trans.localPosition = Vector3.zero;
            //    trans.localScale = m_ResScale;
            //}
            //else
            //{
            //    m_Res = "";
            //    if (childCount > 0)
            //    {
            //        for(int iChild = childCount-1; iChild>=0;iChild--)
            //        {
            //            GameObject.DestroyImmediate(transform.GetChild(iChild).gameObject);
            //        }
            //    }
            //}
            //transform.localScale = Vector3.one;
            yield return null;
        }
    }
}

#endif