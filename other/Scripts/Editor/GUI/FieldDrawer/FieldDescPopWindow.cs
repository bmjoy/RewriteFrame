using LeyoutechEditor.Core.Window;
using UnityEditor;
using UnityEngine;

namespace LeyoutechEditor.EGUI.FieldDrawer
{
    public class FieldDescPopWindow : DotPopupWindow
    {
        public static void ShowWin(Rect position,string fieldName,string fieldDesc)
        {
            var win = GetPopupWindow<FieldDescPopWindow>();
            win.m_FieldName = fieldName;
            win.m_FieldDesc = fieldDesc;
            win.Show<FieldDescPopWindow>(new Rect(position.x-100, position.y+position.height, 200, 100), true, true);
        }

        private string m_FieldName = "";
        private string m_FieldDesc = "";

        private GUIStyle m_BoldLabelCenterStyle = null;
        private GUIStyle m_LabelWrapStyle = null;
        protected override void OnGUI()
        {
            base.OnGUI();

            if(m_BoldLabelCenterStyle == null)
            {
                m_BoldLabelCenterStyle = new GUIStyle(EditorStyles.boldLabel);
                m_BoldLabelCenterStyle.alignment = TextAnchor.MiddleCenter;
                m_BoldLabelCenterStyle.fontSize = 15;
            }
            if(m_LabelWrapStyle == null)
            {
                m_LabelWrapStyle = new GUIStyle(EditorStyles.label);
                m_LabelWrapStyle.wordWrap = true;
            }

            EditorGUILayout.LabelField(m_FieldName, m_BoldLabelCenterStyle);
            EditorGUILayout.Space();
            EditorGUILayout.LabelField(m_FieldDesc, m_LabelWrapStyle, GUILayout.ExpandHeight(true));

        }
    }
}
