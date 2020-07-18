using System;
using System.Reflection;
using UnityEditor;

namespace LeyoutechEditor.EGUI.FieldDrawer
{
    [TargetFieldType(typeof(Enum))]
    public class EnumDrawer : AFieldDrawer
    {
        public EnumDrawer(FieldInfo fieldInfo) : base(fieldInfo)
        {
        }

        protected override void OnDraw(bool isReadonly, bool isShowDesc)
        {
            object value = (Enum)m_FieldInfo.GetValue(data);

            EditorGUILayout.BeginHorizontal();
            {
                EditorGUI.BeginDisabledGroup(isReadonly);
                {
                    EditorGUI.BeginChangeCheck();
                    {
                        value = EditorGUILayout.EnumPopup(m_NameContent, (Enum)value);
                    }
                    if (EditorGUI.EndChangeCheck())
                    {
                        m_FieldInfo.SetValue(data, value);
                    }
                }
                EditorGUI.EndDisabledGroup();

                OnDrawAskOperation();
            }
            EditorGUILayout.EndHorizontal();
        }
    }
}
