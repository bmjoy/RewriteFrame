#if UNITY_EDITOR
using EditorExtend;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Map
{
    [ExecuteInEditMode]
    public class LightmapSetting : MonoBehaviour
    {
        [System.Serializable]
        public class LightInfo
        {
            public Renderer m_Render;
            public int m_BakedLightmapIndex;
            public Vector4 m_BakedLightmapScaleOffset;

            public int m_RealLightmapIndex;
            public Vector4 m_RealLightmapScaleOffest;

            public int[] m_ChildIndex = null;

            public void Init(Renderer render)
            {
                m_Render = render;
                m_BakedLightmapIndex = render.lightmapIndex;
                m_BakedLightmapScaleOffset = render.lightmapScaleOffset;

                m_RealLightmapIndex = render.realtimeLightmapIndex;
                m_RealLightmapScaleOffest = render.realtimeLightmapScaleOffset;
            }
        }

        [ReadOnly]
        public LightInfo[] m_LightInfos;

        void Awake()
        {
            if (Application.isPlaying)
            {
                Object.DestroyImmediate(this);
                return;
            }
            ApplyLightInfo();
        }


        public void ApplyLightInfo()
        {
            if (m_LightInfos != null && m_LightInfos.Length > 0)
            {
                for (int iInfo = 0; iInfo < m_LightInfos.Length; iInfo++)
                {
                    LightInfo info = m_LightInfos[iInfo];
                    if (info.m_Render != null)
                    {
                        info.m_Render.lightmapIndex = info.m_BakedLightmapIndex;
                        info.m_Render.lightmapScaleOffset = info.m_BakedLightmapScaleOffset;

                        info.m_Render.realtimeLightmapIndex = info.m_RealLightmapIndex;
                        info.m_Render.realtimeLightmapScaleOffset = info.m_RealLightmapScaleOffest;
                    }
                }
            }
        }

        public void SetLightInfo(LightInfo[] infos)
        {
            if (infos == null || infos.Length <= 0)
            {
                return;
            }
            m_LightInfos = infos;
            ApplyLightInfo();
        }

        public void Clear()
        {
            m_LightInfos = null;
        }

    }
}
#endif
