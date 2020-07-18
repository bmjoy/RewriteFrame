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
    [TargetFieldType(typeof(List<CdType>))]
    public class CDTypeListDrawer : AListFieldDrawer
    {
        public CDTypeListDrawer(FieldInfo fieldInfo) : base(fieldInfo)
        {
        }

        protected override void DrawElement(Rect rect, IList list, int index)
        {
            list[index] = EditorGUI.EnumPopup(rect,(CdType)list[index]);
        }

        protected override Type GetDataType()
        {
            return typeof(CdType);
        }

        protected override float GetElementHeight()
        {
            return EditorGUIUtility.singleLineHeight;
        }

        protected override object GetNewData()
        {
            return CdType.Skill;
        }

        protected override IList GetNewList()
        {
            return new List<CdType>();
        }
    }
}
