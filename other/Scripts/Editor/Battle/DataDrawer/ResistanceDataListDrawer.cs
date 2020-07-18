using Eternity.Share.Config.SkillDatas.Resistances;
using LeyoutechEditor.EGUI.FieldDrawer;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace EternityEditor.Battle
{
    [TargetFieldType(typeof(List<ResistanceData>))]
    public class ResistanceDataListDrawer : AListFieldDrawer
    {
        public ResistanceDataListDrawer(FieldInfo fieldInfo) : base(fieldInfo)
        {
        }

        protected override void DrawElement(Rect rect, IList list, int index)
        {
            ResistanceData rData = list[index] as ResistanceData;
            Rect fieldRect = new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight);
            rData.Resistance = (ResistanceType)EditorGUI.EnumPopup(fieldRect, "Resistance", rData.Resistance);
            fieldRect.y += fieldRect.height;
            rData.IsValid = EditorGUI.Toggle(fieldRect, "IsValid", rData.IsValid);
        }

        protected override Type GetDataType()
        {
            return typeof(ResistanceData);
        }

        protected override float GetElementHeight()
        {
            return EditorGUIUtility.singleLineHeight * 3;
        }

        protected override object GetNewData()
        {
            return new ResistanceData();
        }

        protected override IList GetNewList()
        {
            return new List<ResistanceData>();
        }
    }
}
