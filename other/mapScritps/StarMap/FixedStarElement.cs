#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Map
{
    [ExecuteInEditMode]
    public class FixedStarElement : MonoBehaviour
    {
        public int m_FixedStarid;
        public StarMapMainPanel m_StarMainPanel;

        private RectTransform m_RectTrans;
        public void Init(StarMapMainPanel panel,StarMapVO starVo)
        {
            m_StarMainPanel = panel;
            m_FixedStarid = starVo.FixedStarid;
            if (starVo != null)
            {
                gameObject.name = starVo.Name;
            }
        }
        
        public Vector2 GetPosition()
        {
            if(m_StarMainPanel == null)
            {
                return Vector2.zero;
            }
            if(m_RectTrans == null)
            {
                m_RectTrans = GetComponent<RectTransform>();
            }
            return m_RectTrans.anchoredPosition;
        }
        

        public IEnumerator DoEditorUpdate(StarMapMainPanel panel)
        {
            m_StarMainPanel = panel;
            if(m_RectTrans == null)
            {
                m_RectTrans = GetComponent<RectTransform>();
            }
            if(m_RectTrans != null)
            {
                Vector3 position = m_RectTrans.anchoredPosition3D;
                m_RectTrans.anchoredPosition3D = new Vector3(position.x,position.y,0);
            }
            
            yield return null;
        }
    }
}
#endif
