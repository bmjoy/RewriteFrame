#if UNITY_EDITOR
using Leyoutech.Utility;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Map
{
    [ExecuteInEditMode]
    public class RecastRootMark : IRootMark
    {
        private IEnumerator m_DoUpdateEnumerator;

        private void OnEnable()
        {
            if(Application.isPlaying)
            {
                Object.DestroyImmediate(this);
                return;
            }
            EditorApplication.update += OnUpdate;
            m_DoUpdateEnumerator = DoUpdate();
        }

        private void OnDisable()
        {
            EditorApplication.update -= OnUpdate;
            m_DoUpdateEnumerator = null;
        }

        void OnUpdate()
        {
            if(m_DoUpdateEnumerator != null)
            {
               if( !m_DoUpdateEnumerator.MoveNext())
                {
                    m_DoUpdateEnumerator = null;
                }
            }
        }

        IEnumerator DoUpdate()
        {
            while(true)
            {
                Transform areaTrans = transform.parent;
                while (areaTrans != null && areaTrans.GetComponent<Area>() == null)
                {
                    areaTrans = areaTrans.parent;
                }
                yield return null;
                if(areaTrans != null)
                {
                    Area area = areaTrans.GetComponent<Area>();
                    if (area != null)
                    {
                        m_AreaId = area.Uid;

                        m_RelativeAreaPos = area.transform.InverseTransformPoint(transform.position);
                        m_RelativeAreaRot = Quaternion.Inverse(areaTrans.rotation) * transform.rotation;
                        m_RelativeAreaScale = ObjectUtility.CalculateLossyScale(transform, area.transform);
                        gameObject.name = string.Format("{0}_Recast区域", areaTrans.name);
                    }
                }
                
                yield return null;
            }
            
        }
    }

}
#endif