#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Map
{
    [CustomEditor(typeof(FixedStarElement))]
    public class FixedStarElementInspector : Editor
    {
        private FixedStarElement m_Target;

        private StarMapVO m_StarMapVo;

        private void OnEnable()
        {
            m_Target = target as FixedStarElement;
            m_StarMapVo = ConfigVO<StarMapVO>.Instance.GetData(m_Target.m_FixedStarid);
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.LabelField("恒星Id:", m_Target.m_FixedStarid.ToString());
            if(m_StarMapVo != null)
            {
                EditorGUILayout.LabelField("恒星名称:", m_StarMapVo.Name);
                EditorGUILayout.LabelField("恒星资源:",m_StarMapVo.AssetName);
            }
            EditorGUILayout.Vector2Field("坐标:", m_Target.GetPosition());
            
        }
    }
}

#endif