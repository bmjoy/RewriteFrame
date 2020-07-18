using Eternity.Share.Config.SkillDatas.Cds;
using LeyoutechEditor.EGUI.FieldDrawer;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace EternityEditor.Battle
{
    [TargetFieldType(typeof(List<CdData>))]
    public class CDDataListDrawer : AListFieldDrawer
    {
        public CDDataListDrawer(FieldInfo fieldInfo) : base(fieldInfo)
        {
        }

        protected override void DrawElement(Rect rect, IList list, int index)
        {
            Rect drawRect = rect;
            drawRect.height = EditorGUIUtility.singleLineHeight;

            CdData cdData = list[index] as CdData;

            cdData.CdType = (CdType)EditorGUI.EnumPopup(drawRect, "Type", cdData.CdType);
            drawRect.y += drawRect.height;
            cdData.CdTime = EditorGUI.FloatField(drawRect, "Time", cdData.CdTime);

        }

        protected override Type GetDataType()
        {
            return typeof(CdData);
        }

        protected override float GetElementHeight()
        {
            return EditorGUIUtility.singleLineHeight * 2;
        }

        protected override object GetNewData()
        {
            return new CdData();
        }

        protected override IList GetNewList()
        {
            return new List<CdData>();
        }
    }
}
