using Eternity.Share.Config.SkillDatas;
using Eternity.Share.Config.TimeLine.Datas.Attributes;
using EternityEditor.Battle.Timeline;
using LeyoutechEditor.Core.EGUI;
using UnityEditor;
using UnityEngine;

namespace EternityEditor.Battle.Skill
{
    public class SkillEndStageContentTab : IContentTabPage
    {
        private SkillEndStageData m_EndStageData = null;

        private TrackGroupDrawer groupDrawer = null;
        private TrackGroupDrawer breakGroupDrawer = null;

        private SkillEditorWindow m_Window = null;
        public SkillEndStageContentTab(SkillEditorWindow win)
        {
            m_Window = win;
        }

        public void OnGUI(Rect rect)
        {
            if (m_EndStageData == null)
            {
                EditorGUI.HelpBox(rect, "Data is Null", MessageType.Error);
                return;
            }
            float deltaHeight = 0.0f;
            Rect isScaleTimeRect = new Rect(rect.x, rect.y + deltaHeight, rect.width, EditorGUIUtility.singleLineHeight);
            m_EndStageData.IsScaleByTime = EditorGUI.Toggle(isScaleTimeRect, "Is Scale By Time", m_EndStageData.IsScaleByTime);

            deltaHeight += isScaleTimeRect.height;

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
            m_EndStageData = (SkillEndStageData)data;
            if(m_EndStageData !=null)
            {
                groupDrawer = new TrackGroupDrawer(GetActionIndex, OnRepaint, ActionCategory.Skill, IsShowDesc);
                groupDrawer.SetData("Group", m_EndStageData.Group);

                breakGroupDrawer = new TrackGroupDrawer(GetActionIndex, OnRepaint, ActionCategory.Skill, IsShowDesc);
                breakGroupDrawer.SetData("Break Group", m_EndStageData.BreakGroup);
            }
            else
            {
                groupDrawer = null;
                breakGroupDrawer = null;
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
