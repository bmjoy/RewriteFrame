using System;
using System.Collections;
using System.Reflection;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using SystemObject = System.Object;

namespace LeyoutechEditor.EGUI.FieldDrawer
{
    public abstract class AListFieldDrawer : AFieldDrawer
    {
        private ReorderableList m_RList = null;
        private IList m_ValueList = null;

        protected AListFieldDrawer(FieldInfo fieldInfo) : base(fieldInfo)
        {
        }

        protected abstract Type GetDataType();
        protected abstract float GetElementHeight();
        protected abstract void DrawElement(Rect rect, IList list, int index);
        protected abstract SystemObject GetNewData();
        protected abstract IList GetNewList();

        public override void SetData(object data)
        {
            base.SetData(data);
            m_ValueList = (IList)m_FieldInfo.GetValue(data);
            if (m_ValueList != null)
            {
                m_RList = new ReorderableList(m_ValueList, GetDataType(), true, true, true, true);
                m_RList.elementHeight = GetElementHeight();
                m_RList.drawHeaderCallback = (rect) =>
                {
                    EditorGUI.LabelField(rect, m_NameContent, EditorStyles.boldLabel);
                };
                m_RList.drawElementCallback = (rect, index, isActive, isFocused) =>
                {
                    EditorGUI.LabelField(new Rect(rect.x, rect.y, 30, rect.height), "" + index);
                    Rect fieldRect = new Rect(rect.x + 30, rect.y, rect.width - 30, rect.height);
                    DrawElement(fieldRect, m_ValueList, index);
                };
                m_RList.onAddCallback = (list) =>
                {
                    list.list.Add(GetNewData());
                };
            }
            else
            {
                m_RList = null;
            }
        }

        protected override void OnDraw(bool isReadonly, bool isShowDesc)
        {
            if (m_ValueList == null)
            {
                EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                {
                    EditorGUILayout.LabelField(m_FieldInfo.Name, "Data is null");
                    if (GUILayout.Button("New", GUILayout.Width(40)))
                    {
                        m_FieldInfo.SetValue(data, GetNewList());
                        SetData(data);
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                EditorGUILayout.BeginVertical();
                {
                    m_RList.DoLayoutList();
                }
                EditorGUILayout.EndVertical();
            }
        }

    }
}
