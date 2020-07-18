#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Map
{
    [CustomEditor(typeof(MineralRoot))]
    public class MineralRootInspector : Editor
    {
        private MineralRoot m_Target = null;
        private bool m_ShowTreasure;
        private List<SemaphoreMark> marks = null;
        private void OnEnable()
        {
            m_Target = target as MineralRoot;
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
                            marks = area.GetMineralSemaphoreMarks();
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
                    MineralInfoWindow.OpenWindow(m_Target);
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
                if (m_Target.m_MineralCache != null && m_Target.m_MineralCache.Count > 0)
                {
                    for (int iMineral = 0; iMineral < m_Target.m_MineralCache.Count; iMineral++)
                    {
                        EditorGUILayout.ObjectField(m_Target.m_MineralCache[iMineral], typeof(Treasure), true);
                    }
                }

            }
            EditorGUI.indentLevel = 0;
        }
    }
}
#endif
