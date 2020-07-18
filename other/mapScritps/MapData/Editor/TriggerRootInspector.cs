#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Map
{
    [CustomEditor(typeof(TriggerRoot))]
    public class TriggerRootInspector : Editor
    {
        private TriggerRoot m_Target = null;
        private bool m_ShowTrigger;
        private void OnEnable()
        {
            m_Target = target as TriggerRoot;
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.indentLevel = 0;
            m_ShowTrigger = EditorGUILayout.Foldout(m_ShowTrigger, "Trigger列表");
            if (m_ShowTrigger)
            {
                EditorGUI.indentLevel = 1;
                if (m_Target.m_TriggerCache != null && m_Target.m_TriggerCache.Length > 0)
                {
                    for (int iTrigger = 0; iTrigger < m_Target.m_TriggerCache.Length; iTrigger++)
                    {
                        EditorGUILayout.ObjectField(m_Target.m_TriggerCache[iTrigger], typeof(Trigger), true);
                    }
                }

            }
            EditorGUI.indentLevel = 0;
        }
    }
}
#endif