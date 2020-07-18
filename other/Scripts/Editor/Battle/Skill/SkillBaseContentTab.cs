using Eternity.Share.Config.SkillDatas;
using LeyoutechEditor.EGUI.FieldDrawer;
using UnityEditor;
using UnityEngine;

namespace EternityEditor.Battle.Skill
{
    public class SkillBaseContentTab : IContentTabPage
    {
        private SkillEditorWindow m_Window = null;
        public SkillBaseContentTab(SkillEditorWindow win)
        {
            m_Window = win;
        }

        private SkillBaseData m_BaseData = null;
        private ObjectDrawer m_DataDrawer = null;
        public void SetData(object data)
        {
            m_BaseData = (SkillBaseData)data;
            m_DataDrawer = new ObjectDrawer("Base Data", m_BaseData);
        }

        private Vector2 scrollPos = Vector2.zero;
        public void OnGUI(Rect rect)
        {
            if(m_BaseData == null)
            {
                EditorGUI.HelpBox(rect, "Data is Null", MessageType.Error);
                return;
            }

            GUILayout.BeginArea(rect);
            {
                scrollPos = EditorGUILayout.BeginScrollView(scrollPos,GUILayout.Width(rect.width));
                {
                    m_DataDrawer.OnGUI(IsShowDesc());
                }
                EditorGUILayout.EndScrollView();

            }
            GUILayout.EndArea();
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
