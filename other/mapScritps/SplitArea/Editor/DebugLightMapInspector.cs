#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DebugLightMap))]
public class DebugLightMapInspector : Editor
{
    private DebugLightMap m_Target;
    private void OnEnable()
    {
        m_Target = target as DebugLightMap;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if(GUILayout.Button("确认"))
        {
            m_Target.Set();
        }
    }
}
#endif