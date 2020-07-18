using Eternity.Share.Config.Attributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace LeyoutechEditor.EGUI.FieldDrawer
{
    [TargetFieldType(typeof(List<string>))]
    public class StringListDrawer : AListFieldDrawer
    {
        private bool m_IsMultilineText = false;
        private int m_MultilineHeight = 0;
        public StringListDrawer(FieldInfo fieldInfo) : base(fieldInfo)
        {
            FieldMultilineText attr = fieldInfo.GetCustomAttribute<FieldMultilineText>();
            if (attr != null)
            {
                m_IsMultilineText = true;
                m_MultilineHeight = attr.Height;
            }
        }

        protected override void DrawElement(Rect rect, IList list, int index)
        {
            if (m_IsMultilineText)
            {
                list[index] = EditorGUI.TextArea(new Rect(rect.x, rect.y, rect.width, rect.height), (string)list[index]);
            }
            else
            {
                list[index] = EditorGUI.TextField(new Rect(rect.x + 40, rect.y, rect.width - 40, rect.height), (string)list[index]);
            }
        }

        protected override Type GetDataType()
        {
            return typeof(string);
        }

        protected override float GetElementHeight()
        {
            if (m_IsMultilineText)
            {
                return m_MultilineHeight;
            }
            else
            {
                return EditorGUIUtility.singleLineHeight;
            }
        }

        protected override object GetNewData()
        {
            return "";
        }

        protected override IList GetNewList()
        {
            return new List<string>();
        }
    }
}
