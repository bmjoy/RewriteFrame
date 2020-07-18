using LeyoutechEditor.Core.EGUI;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using System;
using SystemObject = System.Object;

namespace LeyoutechEditor.EGUI.FieldDrawer
{
    [TargetFieldType(typeof(object))]
    public class DefalutClassDrawer : AFieldDrawer
    {
        private FieldData[] m_FieldDatas = null;
        private bool m_IsFoldout = false;
        private SystemObject m_ValueObject = null;
        public DefalutClassDrawer(FieldInfo fieldInfo) : base(fieldInfo)
        {
            m_FieldDatas = FieldDrawerUtil.GetTypeFieldDrawer(fieldInfo.FieldType);
        }

        public override void SetData(object data)
        {
            base.SetData(data);
            m_ValueObject = m_FieldInfo.GetValue(data);

            if(m_ValueObject!=null)
            {
                foreach(var fd in m_FieldDatas)
                {
                    if(fd.Drawer!=null)
                    {
                        fd.Drawer.SetData(m_ValueObject);
                    }
                }
            }

        }

        protected override void OnDraw(bool isReadonly, bool isShowDesc)
        {
            EditorGUILayout.BeginHorizontal();
            {
                m_IsFoldout = EditorGUILayout.Foldout(m_IsFoldout, m_NameContent, true);
                OnDrawAskOperation();
            }
            EditorGUILayout.EndHorizontal();

            if(m_IsFoldout)
            {
                EditorGUIUtil.BeginIndent();
                {
                    if (m_ValueObject == null)
                    {
                        EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                        {
                            EditorGUILayout.LabelField("Data is null");
                            if (GUILayout.Button("New", GUILayout.Width(40)))
                            {
                                m_ValueObject = Activator.CreateInstance(m_FieldInfo.FieldType);
                                m_FieldInfo.SetValue(data, m_ValueObject);

                                SetData(data);
                            }
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                    else
                    {
                        EditorGUILayout.BeginVertical();
                        {
                            foreach (var fieldData in m_FieldDatas)
                            {
                                if (fieldData.Drawer == null)
                                {
                                    EditorGUILayout.LabelField(fieldData.Name, "Drawer is null");
                                }
                                else
                                {
                                    fieldData.Drawer.DrawField(isShowDesc);
                                }
                            }
                        }
                        EditorGUILayout.EndVertical();
                    }
                    
                }
                EditorGUIUtil.EndIndent();

            }
        }
    }
}
