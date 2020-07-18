#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Map
{
    [CustomEditor(typeof(PlanetElement))]
    public class PlanetElementInspector : Editor
    {
        private PlanetElement m_Target;
        private EditorGamingMap m_StarData;
        private void OnEnable()
        {
            m_Target = target as PlanetElement;
            m_StarData = m_Target.m_StarData;
        }

        public override void OnInspectorGUI()
        {
            if(m_StarData != null)
            {
                EditorGUILayout.LabelField("行星Id:", m_StarData.gamingmapId.ToString());
                EditorGUILayout.LabelField("行星名称:", m_StarData.gamingmapName);
            }
            m_Target.m_ResObj = (GameObject)EditorGUILayout.ObjectField("资源:", m_Target.m_ResObj, typeof(GameObject), false);
            m_Target.m_ResScale = EditorGUILayout.Vector2Field("资源scale:", m_Target.m_ResScale);
            EditorGUILayout.Vector2Field("坐标:", m_Target.GetPosition());
        }
    }
}

#endif