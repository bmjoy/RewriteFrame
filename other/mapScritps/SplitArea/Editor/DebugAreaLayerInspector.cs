#if true
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Map
{
    [CustomEditor(typeof(DebugAreaLayer))]
    public class DebugAreaLayerInspector : Editor
    {
        private DebugAreaLayer m_Target;
        
        private void OnEnable()
        {
            m_Target = target as DebugAreaLayer;
            if(m_Target.m_AreaLayerInfos!=null && m_Target.m_AreaLayerInfos.Length>0)
            {
                m_Target.m_ShowLayer = new bool[m_Target.m_AreaLayerInfos.Length];
                m_Target.m_ShowUnitAB = new bool[m_Target.m_AreaLayerInfos.Length];
            }
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if(m_Target.m_ShowLayer == null)
            {
                return;
            }
            GUILayout.Label("Show Layer:");
            for(int iLayer =0;iLayer< m_Target.m_ShowLayer.Length;iLayer++)
            {
                m_Target.m_ShowLayer[iLayer] = EditorGUILayout.Toggle(iLayer.ToString(), m_Target.m_ShowLayer[iLayer]);
            }
            GUILayout.Label("Show Unit AABB:");
            for (int iLayer = 0; iLayer < m_Target.m_ShowUnitAB.Length; iLayer++)
            {
                m_Target.m_ShowUnitAB[iLayer] = EditorGUILayout.Toggle(iLayer.ToString(), m_Target.m_ShowUnitAB[iLayer]);
            }
        }

        private void OnSceneGUI()
        {
            m_Target.ShowSceneUI();
        }
    }
}
#endif
