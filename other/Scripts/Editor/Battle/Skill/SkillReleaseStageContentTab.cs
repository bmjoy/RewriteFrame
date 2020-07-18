
using Eternity.Share.Config.SkillDatas;
using Eternity.Share.Config.TimeLine.Datas.Attributes;
using EternityEditor.Battle.Timeline;
using LeyoutechEditor.Core.EGUI;
using UnityEditor;
using UnityEngine;

namespace EternityEditor.Battle.Skill
{
    public class SkillReleaseStageContentTab : IContentTabPage
    {
        private static readonly int CHILD_OPERATION_WIDTH = 60;

        private SkillReleaseStageData m_ReleaseStageData = null;

        private TrackGroupDrawer groupDrawer = null;
        private TrackGroupDrawer breakGroupDrawer = null;

        private int m_SelectedChildIndex = -1;
        public int SelectedChildIndex
        {
            get
            {
                return m_SelectedChildIndex;
            }
            set
            {
                if(m_SelectedChildIndex!=value)
                {
                    m_SelectedChildIndex = value;
                    if(m_SelectedChildIndex>=0 && m_SelectedChildIndex<m_ReleaseStageData.Childs.Count)
                    {
                        SkillReleaseChildData childData = m_ReleaseStageData.Childs[m_SelectedChildIndex];
                        groupDrawer = new TrackGroupDrawer(GetActionIndex, OnRepaint, ActionCategory.Skill, IsShowDesc);
                        groupDrawer.SetData("Group", childData.Group);

                        breakGroupDrawer = new TrackGroupDrawer(GetActionIndex, OnRepaint, ActionCategory.Skill, IsShowDesc);
                        breakGroupDrawer.SetData("Break Group", childData.BreakGroup);
                    }else
                    {
                        m_SelectedChildIndex = -1;
                        groupDrawer = null;
                        breakGroupDrawer = null;
                    }
                }
            }
        }

        private SkillEditorWindow m_Window = null;
        public SkillReleaseStageContentTab(SkillEditorWindow win)
        {
            m_Window = win;
        }

        public void OnGUI(Rect rect)
        {
            if (m_ReleaseStageData == null)
            {
                EditorGUI.HelpBox(rect, "Data is Null", MessageType.Error);
                return;
            }

            float deltaHeight = 0.0f;
            Rect fieldRect = new Rect(rect.x, rect.y + deltaHeight, rect.width, EditorGUIUtility.singleLineHeight);
            m_ReleaseStageData.IsScaleByTime = EditorGUI.Toggle(fieldRect, "IsScaleByTime", m_ReleaseStageData.IsScaleByTime);

            fieldRect.y += fieldRect.height;
            m_ReleaseStageData.LoopCount = EditorGUI.IntField(fieldRect, "LoopCount", m_ReleaseStageData.LoopCount);
            fieldRect.y += fieldRect.height;
            m_ReleaseStageData.IsDynamicLoopCount = EditorGUI.Toggle(fieldRect, "IsDynamicLoopCount", m_ReleaseStageData.IsDynamicLoopCount);

            fieldRect.y += fieldRect.height+10;
            EditorGUI.LabelField(fieldRect,"Child Datas:");

            deltaHeight += EditorGUIUtility.singleLineHeight * 5+10;

            Rect childRect = new Rect(rect.x, rect.y + deltaHeight, rect.width, rect.height - deltaHeight);
            EditorGUIUtil.DrawAreaLine(childRect, Color.gray);

            Rect childOperationRect = new Rect(rect.x,rect.y+deltaHeight,rect.width, 25);
            using (new GUILayout.AreaScope(childOperationRect))
            {
                using (new GUILayout.HorizontalScope(GUILayout.Height(25)))
                {
                    if (GUILayout.Button("New", GUILayout.Width(CHILD_OPERATION_WIDTH)))
                    {
                        SkillReleaseChildData childData = new SkillReleaseChildData();
                        m_ReleaseStageData.Childs.Add(childData);

                        SelectedChildIndex = m_ReleaseStageData.Childs.Count - 1;
                    }
                    if (GUILayout.Button("Delete", GUILayout.Width(CHILD_OPERATION_WIDTH)))
                    {
                        if(m_ReleaseStageData.Childs.Count>1)
                        {
                            m_ReleaseStageData.Childs.RemoveAt(SelectedChildIndex);
                            int prevChildIndex = SelectedChildIndex - 1;
                            if(prevChildIndex<0)
                            {
                                prevChildIndex = 0;
                            }
                            SelectedChildIndex = prevChildIndex;
                        }
                    }

                    GUILayout.FlexibleSpace();

                    if (GUILayout.Button("Prev", GUILayout.Width(CHILD_OPERATION_WIDTH)))
                    {
                        int prevChildIndex = SelectedChildIndex - 1;
                        if (prevChildIndex >=0)
                        {
                            SelectedChildIndex = prevChildIndex;
                        }
                    }

                    GUILayout.Label("" + (SelectedChildIndex+1)+"/"+ m_ReleaseStageData.Childs.Count);

                    if (GUILayout.Button("Next", GUILayout.Width(CHILD_OPERATION_WIDTH)))
                    {
                        int nextChildIndex = SelectedChildIndex + 1;
                        if (nextChildIndex < m_ReleaseStageData.Childs.Count)
                        {
                            SelectedChildIndex = nextChildIndex;
                        }
                    }
                }
            }
            deltaHeight += childOperationRect.height;

            Rect groupRect = new Rect(rect.x, rect.y + deltaHeight, rect.width, rect.height - deltaHeight);
            DrawGroup(groupRect);
        }

        private void DrawGroup(Rect rect)
        {
            if(m_ReleaseStageData.Childs.Count == 0)
            {
                EditorGUI.LabelField(rect, "Child Data is Empty.Add a new child by btn(new)");
                return;
            }

            SkillReleaseChildData childData = m_ReleaseStageData.Childs[SelectedChildIndex];

            float deltaHeight = 0.0f;

            Rect fieldRect = new Rect(rect.x, rect.y + deltaHeight, rect.width, EditorGUIUtility.singleLineHeight);
            childData.ConditionType = (SkillReleaseStageCondtionType)EditorGUI.EnumPopup(fieldRect, "ConditionType", childData.ConditionType);

            SkillReleaseTimeLengthCondition condition = childData.TimeLengthCondition;
            fieldRect.y += fieldRect.height;
            EditorGUI.LabelField(fieldRect, "TimeLengthCondition");

            fieldRect.y += fieldRect.height;
            Rect conditionRect = fieldRect;
            conditionRect.x += 12;
            condition.MinTime = EditorGUI.FloatField(conditionRect, "MinTime",condition.MinTime);

            conditionRect.y += conditionRect.height;
            condition.MaxTime = EditorGUI.FloatField(conditionRect, "MaxTime",condition.MaxTime);

            deltaHeight += EditorGUIUtility.singleLineHeight * 4;

            Rect groupRect = new Rect(rect.x, rect.y + deltaHeight, rect.width, (rect.height - deltaHeight) * .6f);
            EditorGUIUtil.DrawAreaLine(groupRect, Color.gray);
            groupDrawer.OnGUI(groupRect);

            deltaHeight += groupRect.height;

            Rect breakGroupRect = new Rect(rect.x, rect.y + deltaHeight, rect.width, rect.height - deltaHeight);
            EditorGUIUtil.DrawAreaLine(breakGroupRect, Color.gray);
            breakGroupDrawer.OnGUI(breakGroupRect);
        }

        public void SetData(object data)
        {
            m_ReleaseStageData = (SkillReleaseStageData)data;
            SelectedChildIndex = -1;
            if (m_ReleaseStageData!=null && m_ReleaseStageData.Childs.Count > 0)
            {
                SelectedChildIndex = 0;
            }
        }

        public int GetActionIndex()
        {
            return m_Window.GetActionIndex();
        }

        public void OnRepaint()
        {
            m_Window.Repaint();
        }

        public bool IsShowDesc()
        {
            return m_Window.IsShowDesc();
        }
    }
}
