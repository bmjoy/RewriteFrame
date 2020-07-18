using Eternity.Share.Config.Attributes;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace LeyoutechEditor.EGUI.FieldDrawer
{
    [TargetFieldType(typeof(string))]
    public class StringDrawer : AFieldDrawer
    {
        private bool isMultilineText = false;
        private int multilineHeight = 0;
        public StringDrawer(FieldInfo fieldInfo) : base(fieldInfo)
        {
            FieldMultilineText attr = fieldInfo.GetCustomAttribute<FieldMultilineText>();
            if(attr!=null)
            {
                isMultilineText = true;
                multilineHeight = attr.Height;
            }
        }

        protected override void OnDraw(bool isReadonly, bool isShowDesc)
        {
            string value = (string)m_FieldInfo.GetValue(data);

            EditorGUILayout.BeginHorizontal();
            {
                EditorGUI.BeginDisabledGroup(isReadonly);
                {
                    EditorGUI.BeginChangeCheck();
                    {
                        if (isMultilineText)
                        {
                            EditorGUILayout.BeginHorizontal();
                            {
                                EditorGUILayout.LabelField(m_NameContent);
                                value = EditorGUILayout.TextArea(value, GUILayout.Height(multilineHeight));
                            }
                            EditorGUILayout.EndHorizontal();

                        }
                        else
                        {
                            value = EditorGUILayout.TextField(m_NameContent, value);
                        }
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
