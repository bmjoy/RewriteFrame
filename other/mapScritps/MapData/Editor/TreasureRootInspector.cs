#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Map
{
    [CustomEditor(typeof(TreasureRoot))]
    public class TreasureRootInspector : Editor
    {
        private TreasureRoot m_Target = null;
        private bool m_ShowTreasure;
        /// <summary>
        /// 信号量标记
        /// </summary>
        private List<SemaphoreMark> marks = null;
        private void OnEnable()
        {
            m_Target = target as TreasureRoot;
            GamingMapArea gamingArea = m_Target.m_GamingMapArea;
            
            if(gamingArea != null)
            {
                Area[] areas = FindObjectsOfType<Area>();
                if (areas != null && areas.Length > 0)
                {
                    for(int iArea = 0;iArea<areas.Length;iArea++)
                    {
                        Area area = areas[iArea];
                        if(area.Uid == gamingArea.m_AreaId)
                        {
                            marks = area.GetSemaphoreMarks();
                        }
                    }
                }
            }
            
        }

        public override void OnInspectorGUI()
        {
            //显示map中对应Area的信号量
            if(marks != null && marks.Count>0)
            {
                if(GUILayout.Button("添加"))
                {
                    TreasureInfoWindow.OpenWindow(m_Target);
                }
            }

            if(GUILayout.Button("同步"))
            {
                //去Map层 找对应的物体同步数据
                m_Target.SyncInfo();
            }

            EditorGUI.indentLevel = 0;
            m_ShowTreasure = EditorGUILayout.Foldout(m_ShowTreasure, "Treasure列表");
            if (m_ShowTreasure)
            {
                EditorGUI.indentLevel = 1;
                if (m_Target.m_TreasureCache != null && m_Target.m_TreasureCache.Count > 0)
                {
                    for (int iTreasure = 0; iTreasure < m_Target.m_TreasureCache.Count; iTreasure++)
                    {
                        EditorGUILayout.ObjectField(m_Target.m_TreasureCache[iTreasure], typeof(Treasure), true);
                    }
                }

            }
            EditorGUI.indentLevel = 0;
        }
    }
}
#endif
