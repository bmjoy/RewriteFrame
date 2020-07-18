using System.Reflection;
using UnityEditor;

namespace LeyoutechEditor.EGUI.FieldDrawer
{
    [TargetFieldType(typeof(int))]
    public class IntDrawer : AFieldDrawer
    {
        public IntDrawer(FieldInfo fieldInfo) : base(fieldInfo)
        {
        }

        protected override void OnDraw(bool isReadonly, bool isShowDesc)
        {
            int value = (int)m_FieldInfo.GetValue(data);

            EditorGUILayout.BeginHorizontal();
            {
                EditorGUI.BeginDisabledGroup(isReadonly);
                {
                    EditorGUI.BeginChangeCheck();
                    {
                        value = EditorGUILayout.IntField(m_NameContent, value);
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
