using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace LeyoutechEditor.EGUI.FieldDrawer
{
    [TargetFieldType(typeof(List<float>))]
    public class FloatListDrawer : AListFieldDrawer
    {
        public FloatListDrawer(FieldInfo fieldInfo) : base(fieldInfo)
        {
        }

        protected override void DrawElement(Rect rect, IList list, int index)
        {
            list[index] = EditorGUI.FloatField(new Rect(rect.x, rect.y, rect.width, rect.height), (float)list[index]);
        }

        protected override Type GetDataType()
        {
            return typeof(float);
        }

        protected override float GetElementHeight()
        {
            return EditorGUIUtility.singleLineHeight;
        }

        protected override object GetNewData()
        {
            return 0.0f;
        }

        protected override IList GetNewList()
        {
            return new List<float>();
        }
    }
}
