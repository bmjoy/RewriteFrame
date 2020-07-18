using Eternity.Share.Config.Flyer.Datas;
using Eternity.Share.Config.TimeLine.Datas.Attributes;
using EternityEditor.Battle.Timeline;
using LeyoutechEditor.Core.EGUI;
using UnityEditor;
using UnityEngine;

namespace EternityEditor.Battle.Flyer
{
    public class FlyerFlyingStageContentTab : IContentTabPage
    {
        private FlyerFlyingStageData m_FlyingStageData = null;

        private TrackGroupDrawer groupDrawer = null;

        private FlyerEditorWindow m_Window = null;
        public FlyerFlyingStageContentTab(FlyerEditorWindow win)
        {
            m_Window = win;
        }

        public void SetData(object data)
        {
            m_FlyingStageData = (FlyerFlyingStageData)data;

            if (m_FlyingStageData != null)
            {
                groupDrawer = new TrackGroupDrawer(GetActionIndex, OnRepaint, ActionCategory.Flyer, IsShowDesc);
                groupDrawer.SetData("Group", m_FlyingStageData.Group);
            }
            else
            {
                groupDrawer = null;
            }
        }

        public void OnGUI(Rect rect)
        {
            if (m_FlyingStageData == null)
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
