using Eternity.Share.Config.Flyer.Datas;
using Eternity.Share.Config.TimeLine.Datas.Attributes;
using EternityEditor.Battle.Timeline;
using LeyoutechEditor.Core.EGUI;
using UnityEditor;
using UnityEngine;

namespace EternityEditor.Battle.Flyer
{
    public class FlyerEndStageContentTab : IContentTabPage
    {
        private FlyerEndStageData m_EndStageData = null;

        private TrackGroupDrawer groupDrawer = null;

        private FlyerEditorWindow m_Window = null;
        public FlyerEndStageContentTab(FlyerEditorWindow win)
        {
            m_Window = win;
        }

        public void SetData(object data)
        {
            m_EndStageData = (FlyerEndStageData)data;

            if (m_EndStageData != null)
            {
                groupDrawer = new TrackGroupDrawer(GetActionIndex, OnRepaint,ActionCategory.Flyer, IsShowDesc);
                groupDrawer.SetData("Group", m_EndStageData.Group);
            }
            else
            {
                groupDrawer = null;
            }
        }

        public void OnGUI(Rect rect)
        {
            if (m_EndStageData == null)
            {
                EditorGUI.HelpBox(rect, "Data is Null", MessageType.Error);
                return;
            }
            EditorGUIUtil.DrawAreaLine(rect, Color.gray);
            groupDrawer.OnGUI(rect);
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
