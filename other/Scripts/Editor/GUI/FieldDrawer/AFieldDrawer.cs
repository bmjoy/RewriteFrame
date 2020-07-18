using Eternity.Share.Config.Attributes;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using SystemObject = System.Object;

namespace LeyoutechEditor.EGUI.FieldDrawer
{
    public abstract class AFieldDrawer
    {
        protected FieldInfo m_FieldInfo = null;

        private FieldDesc m_DescAttr = null;
        private bool m_IsReadonly = false;

        protected GUIContent m_NameContent = null;
        protected AFieldDrawer(FieldInfo fieldInfo)
        {
            this.m_FieldInfo = fieldInfo;

            m_DescAttr = this.m_FieldInfo.GetCustomAttribute<FieldDesc>();
            m_IsReadonly = this.m_FieldInfo.GetCustomAttribute<FieldReadonly>() != null;

            if(m_DescAttr!=null)
            {
                m_NameContent = new GUIContent(fieldInfo.Name, m_DescAttr.BriefDesc);
            }else
            {
                m_NameContent = new GUIContent(fieldInfo.Name);
            }
        }

        protected SystemObject data;
        public virtual void SetData(SystemObject data)
        {
            this.data = data;
        }

        public void DrawField(bool isShowDesc)
        {
            bool showDesc = m_DescAttr != null && isShowDesc;
            
            EditorGUILayout.BeginVertical();
            {
                if(showDesc)
                {
                    EditorGUILayout.LabelField(m_DescAttr.DetailDesc, EditorStyles.helpBox);
                }

                OnDraw(m_IsReadonly,isShowDesc);
            }
            EditorGUILayout.EndVertical();
        }

        protected abstract void OnDraw(bool isReadonly,bool isShowDesc);

        protected void OnDrawAskOperation()
        {
            if (m_DescAttr != null)
            {
                GUIContent askBtn = new GUIContent("?");
                Rect rect = GUILayoutUtility.GetRect(askBtn, EditorStyles.miniButton, GUILayout.Width(16), GUILayout.Height(16));
                if (GUI.Button(rect, askBtn, EditorStyles.miniButton))
                {
                    Rect position = GUIUtility.GUIToScreenRect(rect);
                    FieldDescPopWindow.ShowWin(position, m_FieldInfo.Name, m_DescAttr.DetailDesc);
                }
            }
        }
    }
}
