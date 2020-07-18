using Eternity.Share.Config.TimeLine.Datas.Summons;
using LeyoutechEditor.Core.EGUI;
using LeyoutechEditor.EGUI.FieldDrawer;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace EternityEditor.Battle
{
    [TargetFieldType(typeof(List<SummonData>))]
    public class SummonDataListDrawer : AListFieldDrawer
    {
        public SummonDataListDrawer(FieldInfo fieldInfo) : base(fieldInfo)
        {
        }

        protected override void DrawElement(Rect rect, IList list, int index)
        {
            SummonData valueData = list[index] as SummonData;
            Rect fieldRect = new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight);
            valueData.SummonID = EditorGUI.IntField(fieldRect, "SummonID", valueData.SummonID);
            fieldRect.y += fieldRect.height;
            valueData.MaxCount = EditorGUI.IntField(fieldRect, "MaxCount", valueData.MaxCount);
            fieldRect.y += fieldRect.height;
            valueData.LifeTime = EditorGUI.FloatField(fieldRect, "LifeTime", valueData.LifeTime);
            fieldRect.y += fieldRect.height;
            valueData.IsDeadWithSummoner = EditorGUI.Toggle(fieldRect, "IsDeadWithSummoner", valueData.IsDeadWithSummoner);
            fieldRect.y += fieldRect.height;
            valueData.IsFllowTarget = EditorGUI.Toggle(fieldRect, "IsFllowTarget", valueData.IsFllowTarget);
            fieldRect.y += fieldRect.height;
            EditorGUI.LabelField(fieldRect, "OffsetPosition");
            fieldRect.y += fieldRect.height;
            EditorGUIUtil.BeginLabelWidth(30);
            {
                Rect itemRect = fieldRect;
                itemRect.x += 12;
                itemRect.width = (itemRect.width - 12) * 0.33f;
                valueData.OffsetPosition.X = EditorGUI.FloatField(itemRect, "x", valueData.OffsetPosition.X);
                itemRect.x += itemRect.width;
                valueData.OffsetPosition.Y = EditorGUI.FloatField(itemRect, "y", valueData.OffsetPosition.Y);
                itemRect.x += itemRect.width;
                valueData.OffsetPosition.Z = EditorGUI.FloatField(itemRect, "z", valueData.OffsetPosition.Z);
            }
            EditorGUIUtil.EndLableWidth();
            fieldRect.y += fieldRect.height;
            valueData.PositionType = (SummonEntityPositionType)EditorGUI.EnumPopup(fieldRect, "PositionType", valueData.PositionType);
            if(valueData.PositionType!= SummonEntityPositionType.None)
            {
                fieldRect.y += fieldRect.height;
                valueData.MinDistance = EditorGUI.FloatField(fieldRect, "NegativeDistance", valueData.MinDistance);
                fieldRect.y += fieldRect.height;
                valueData.MaxDistance = EditorGUI.FloatField(fieldRect, "PositiveDistance", valueData.MaxDistance);
            }
        }

        protected override Type GetDataType()
        {
            return typeof(SummonData);
        }

        protected override float GetElementHeight()
        {
            return EditorGUIUtility.singleLineHeight * 9;
        }

        protected override object GetNewData()
        {
            return new SummonData();
        }

        protected override IList GetNewList()
        {
            return new List<SummonData>();
        }
    }
}
