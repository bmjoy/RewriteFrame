using System.Reflection;
using UnityEditor;

namespace LeyoutechEditor.EGUI.FieldDrawer
{
    [TargetFieldType(typeof(bool))]
    public class BoolDrawer : AFieldDrawer
    {
        public BoolDrawer(FieldInfo fieldInfo) : base(fieldInfo)
        {
        }

        protected override void OnDraw(bool isReadonly, bool isShowDesc)
        {
            bool value = (bool)m_FieldInfo.GetValue(data);

            EditorGUILayout.BeginHorizontal();
            {
                EditorGUI.BeginDisabledGroup(isReadonly);
                {
                    EditorGUI.BeginChangeCheck();
                    {
                        value = EditorGUILayout.Toggle(m_NameContent, value);
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
