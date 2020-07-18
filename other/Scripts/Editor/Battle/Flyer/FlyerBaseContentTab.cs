using Eternity.Share.Config.Flyer.Datas;
using LeyoutechEditor.EGUI.FieldDrawer;
using UnityEditor;
using UnityEngine;

namespace EternityEditor.Battle.Flyer
{
    public class FlyerBaseContentTab : IContentTabPage
    {
        private FlyerEditorWindow m_Window = null;
        public FlyerBaseContentTab(FlyerEditorWindow win)
        {
            m_Window = win;
        }

        private FlyerBaseData m_BaseData = null;
        public void SetData(object data)
        {
            m_BaseData = (FlyerBaseData)data;
            m_DataDrawer = new ObjectDrawer("Base Data", m_BaseData);
        }

        private ObjectDrawer m_DataDrawer = null;
        private Vector2 scrollPos = Vector2.zero;
        public void OnGUI(Rect rect)
        {
            if (m_BaseData == null)
            {
                EditorGUI.HelpBox(rect, "Data is Null", MessageType.Error);
                return;
            }

            GUILayout.BeginArea(rect);
            {
                scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Width(rect.width));
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
