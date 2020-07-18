using System.Reflection;
using UnityEditor;

namespace LeyoutechEditor.EGUI.FieldDrawer
{
    [TargetFieldType(typeof(float))]
    public class FloatDrawer : AFieldDrawer
    {
        public FloatDrawer(FieldInfo fieldInfo) : base(fieldInfo)
        {
        }

        protected override void OnDraw(bool isReadonly, bool isShowDesc)
        {
            float value = (float)m_FieldInfo.GetValue(data);

            EditorGUILayout.BeginHorizontal();
            {
                EditorGUI.BeginDisabledGroup(isReadonly);
                {
                    EditorGUI.BeginChangeCheck();
                    {
                        value = EditorGUILayout.FloatField(m_NameContent, value);
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
