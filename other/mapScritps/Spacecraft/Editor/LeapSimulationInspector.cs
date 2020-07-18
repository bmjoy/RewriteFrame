#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Map
{
    [CustomEditor(typeof(LeapSimulation))]
    public class LeapSimulationInspector : Editor
    {
        private LeapSimulation m_Target;

        private bool m_LeapObjsFold;
        private bool m_SelectLeapFold;
        private List<bool> m_Toggles = new List<bool>();
        private void OnEnable()
        {
            m_Target = target as LeapSimulation;
            m_Target.RefreshMapLeaps();
            MapLeap[] leapObjs = m_Target.m_Leaps;
            if(leapObjs != null && leapObjs.Length>0)
            {
                for(int iLeap = 0;iLeap<leapObjs.Length;iLeap++)
                {
                    m_Toggles.Add(false);
                }
            }
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.indentLevel = 0;
            m_LeapObjsFold = EditorGUILayout.Foldout(m_LeapObjsFold, "跃迁点列表");
            if(m_LeapObjsFold)
            {
                EditorGUI.indentLevel = 1;
                MapLeap[] leapObjs = m_Target.m_Leaps;
                if(leapObjs != null && leapObjs.Length>0)
                {
                    for(int iLeap = 0;iLeap<leapObjs.Length;iLeap++)
                    {
                        MapLeap mapLeap = leapObjs[iLeap];
                        if (GUILayout.Button(mapLeap.gameObject.name))
                        {
                            Selection.activeObject = mapLeap;
                        }
                    }
                }
            }

            EditorGUI.indentLevel = 0;
            m_SelectLeapFold = EditorGUILayout.Foldout(m_SelectLeapFold,"选择跃迁点");
            if(m_SelectLeapFold)
            {
                EditorGUI.indentLevel = 1;
                MapLeap[] leapObjs = m_Target.m_Leaps;
                if (leapObjs != null && leapObjs.Length > 0)
                {
                    for (int iLeap = 0; iLeap < leapObjs.Length; iLeap++)
                    {
                        MapLeap mapLeap = leapObjs[iLeap];
                        m_Toggles[iLeap] = EditorGUILayout.Toggle(mapLeap.m_LeapName, m_Toggles[iLeap]);
                        if(m_Toggles[iLeap])
                        {
                            for (int iToggle = 0;iToggle<m_Toggles.Count;iToggle++)
                            {
                                m_Toggles[iToggle] = iToggle == iLeap ? true : false;
                            }
                        }
                    }
                }
            }
            EditorGUI.indentLevel = 0;
            if(GUILayout.Button("跃迁"))
            {
                Leap();
            }
            if(GUILayout.Button("刷新"))
            {
                OnEnable();
            }
        }

        /// <summary>
        /// 跃迁
        /// </summary>
        private void Leap()
        {
            int toggleIndex = m_Toggles.IndexOf(true);
            if (toggleIndex >= 0)
            {
                MapLeap[] leapObjs = m_Target.m_Leaps;
                if (leapObjs != null && leapObjs.Length > toggleIndex)
                {
                    //leapObjs[toggleIndex]
                }
            }
        }
    }
}
#endif
