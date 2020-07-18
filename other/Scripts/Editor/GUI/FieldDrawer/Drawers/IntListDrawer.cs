using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace LeyoutechEditor.EGUI.FieldDrawer
{
    [TargetFieldType(typeof(List<int>))]
    public class IntListDrawer : AListFieldDrawer
    {
        public IntListDrawer(FieldInfo fieldInfo) : base(fieldInfo)
        {
        }

        protected override void DrawElement(Rect rect, IList list, int index)
        {
            list[index] = EditorGUI.IntField(new Rect(rect.x, rect.y, rect.width, rect.height), (int)list[index]);
        }

        protected override Type GetDataType()
        {
            return typeof(int);
        }

        protected override float GetElementHeight()
        {
            return EditorGUIUtility.singleLineHeight;
        }

        protected override object GetNewData()
        {
            return 0;
        }

        protected override IList GetNewList()
        {
            return new List<int>();
        }
    }
}
