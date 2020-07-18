using Eternity.Share.Config.SkillDatas.AttrEffects;
using LeyoutechEditor.EGUI.FieldDrawer;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace EternityEditor.Battle
{
    [TargetFieldType(typeof(List<AttrEffectData>))]
    public class AttrEffectDataListDrawer : AListFieldDrawer
    {
        public AttrEffectDataListDrawer(FieldInfo fieldInfo) : base(fieldInfo)
        {
        }

        protected override void DrawElement(Rect rect, IList list, int index)
        {
            AttrEffectData valueData = list[index] as AttrEffectData;
            Rect fieldRect = new Rect(rect.x, rect.y, rect.width , EditorGUIUtility.singleLineHeight);
            valueData.Function = EditorGUI.TextField(fieldRect, "Function", valueData.Function);
            fieldRect.y += fieldRect.height;
            valueData.Attribute=EditorGUI.TextField(fieldRect, "Attribute", valueData.Attribute);
            fieldRect.y += fieldRect.height;
            valueData.Value=EditorGUI.FloatField(fieldRect, "Value", valueData.Value);
            fieldRect.y += fieldRect.height;
            valueData.PipeLv=EditorGUI.IntField(fieldRect, "PipeLv", valueData.PipeLv);
        }

        protected override Type GetDataType()
        {
            return typeof(AttrEffectData);
        }

        protected override float GetElementHeight()
        {
            return EditorGUIUtility.singleLineHeight * 5;
        }

        protected override object GetNewData()
        {
            return new AttrEffectData();
        }

        protected override IList GetNewList()
        {
            return new List<AttrEffectData>();
        }
    }
}
