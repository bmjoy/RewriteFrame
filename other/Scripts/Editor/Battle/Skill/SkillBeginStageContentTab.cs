using Eternity.Share.Config.SkillDatas;
using Eternity.Share.Config.TimeLine.Datas.Attributes;
using EternityEditor.Battle.Timeline;
using LeyoutechEditor.Core.EGUI;
using UnityEditor;
using UnityEngine;

namespace EternityEditor.Battle.Skill
{
    public class SkillBeginStageContentTab : IContentTabPage
    {
        private SkillEditorWindow m_Window = null;

        private SkillBeginStageData m_BeginStageData = null;

        private TrackGroupDrawer groupDrawer = null;
        private TrackGroupDrawer breakGroupDrawer = null;
        
        public SkillBeginStageContentTab(SkillEditorWindow win)
        {
            m_Window = win;
        }

        public void SetData(object data)
        {
            m_BeginStageData = (SkillBeginStageData)data;

            if (m_BeginStageData != null)
            {
                groupDrawer = new TrackGroupDrawer(GetActionIndex,OnRepaint, ActionCategory.Skill, IsShowDesc);
                groupDrawer.SetData("Group", m_BeginStageData.Group);

                breakGroupDrawer = new TrackGroupDrawer(GetActionIndex, OnRepaint, ActionCategory.Skill, IsShowDesc);
                breakGroupDrawer.SetData("Break Group", m_BeginStageData.BreakGroup);
            }
            else
            {
                groupDrawer = null;
                breakGroupDrawer = null;
            }
        }

        public void OnGUI(Rect rect)
        {
            if (m_BeginStageData == null)
            {
                EditorGUI.HelpBox(rect, "Data is Null", MessageType.Error);
                return;
            }

            float deltaHeight = 0.0f;
            Rect isScaleTimeRect = new Rect(rect.x, rect.y+deltaHeight, rect.width, EditorGUIUtility.singleLineHeight);
            m_BeginStageData.IsScaleByTime = EditorGUI.Toggle(isScaleTimeRect, "Is Scale By Time", m_BeginStageData.IsScaleByTime);
            deltaHeight += isScaleTimeRect.height;

            //进度条
            Rect ProgressBarRect = new Rect(isScaleTimeRect.x, isScaleTimeRect.y + isScaleTimeRect.height, isScaleTimeRect.width, EditorGUIUtility.singleLineHeight * 3);
            GUI.BeginGroup(ProgressBarRect);
            Rect PbRect = new Rect(0,0, ProgressBarRect.width, EditorGUIUtility.singleLineHeight);
            EditorGUI.LabelField(PbRect, "进度条信息:");
            PbRect.x += 20;
            PbRect.y += EditorGUIUtility.singleLineHeight;
            m_BeginStageData.ProgressBarData.IsShow = EditorGUI.Toggle(PbRect, "是否显示", m_BeginStageData.ProgressBarData.IsShow);
            PbRect.y += EditorGUIUtility.singleLineHeight;
            m_BeginStageData.ProgressBarData.StyleIndex = EditorGUI.IntField(PbRect, "样式索引", m_BeginStageData.ProgressBarData.StyleIndex);
            GUI.EndGroup();



            deltaHeight += ProgressBarRect.height;
            Rect groupRect = new Rect(rect.x,rect.y+deltaHeight,rect.width,(rect.height -deltaHeight)*.6f);
            EditorGUIUtil.DrawAreaLine(groupRect, Color.gray);
            groupDrawer.OnGUI(groupRect);

            deltaHeight += groupRect.height;

            Rect breakGroupRect = new Rect(rect.x, rect.y+deltaHeight, rect.width,rect.height - deltaHeight);
            EditorGUIUtil.DrawAreaLine(breakGroupRect, Color.gray);
            breakGroupDrawer.OnGUI(breakGroupRect);
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
