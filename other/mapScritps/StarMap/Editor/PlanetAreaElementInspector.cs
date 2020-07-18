#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Map
{
    [CustomEditor(typeof(PlanetAreaElement))]
    public class PlanetAreaElementInspector : Editor
    {
        private PlanetAreaElement m_Target;
        private EditorArea m_AreaData;
        private void OnEnable()
        {
            m_Target = target as PlanetAreaElement;
            m_AreaData = m_Target.m_AreaData;
        }

        public override void OnInspectorGUI()
        {
            if(m_AreaData != null)
            {
                EditorGUILayout.LabelField("区域Id:", m_AreaData.areaId.ToString());
                EditorGUILayout.LabelField("区域名字:",m_AreaData.areaName);
            }
            m_Target.m_Resobj = (GameObject)EditorGUILayout.ObjectField("资源:", m_Target.m_Resobj, typeof(GameObject), false);
            EditorGUILayout.Vector2Field("坐标:", m_Target.GetPosition());
        }
    }
}

#endif