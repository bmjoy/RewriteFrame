/*===============================
 * Author: [Allen]
 * Purpose: AssetAddressLabelOperationEditor
 * Time: 2019/10/31 11:55:44
================================*/

using LeyoutechEditor.Core.EGUI;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace LeyoutechEditor.Core.AssetRuler.AssetAddress
{
    [CustomEditor(typeof(AssetAddressLabelOperation))]
    public class AssetAddressLabelOperationEditor : Editor
    {
        SerializedProperty m_labels;

        private ReorderableList m_labelsR;

        protected virtual void OnEnable()
        {
            m_labels = serializedObject.FindProperty("m_labels");

            m_labelsR = new ReorderableList(serializedObject, m_labels, false, true, true, true);
            //自定义列表名称
            m_labelsR.drawHeaderCallback = (rect) =>
            {
                EditorGUI.LabelField(rect, "可配置标签列表");
            };


            //自定义绘制列表元素
            m_labelsR.drawElementCallback = (rect, index, isActive, isFocused) =>
            {
                EditorGUIUtil.BeginLabelWidth(40);
                {
                    EditorGUI.PropertyField(rect, m_labels.GetArrayElementAtIndex(index), new GUIContent("" + index));
                }
                EditorGUIUtil.EndLableWidth();
            };

            //添加元素
            m_labelsR.onAddCallback += (ReorderableList list) =>
            {
                m_labels.InsertArrayElementAtIndex(m_labels.arraySize);
            };

            //当删除元素时候的回调函数，实现删除元素时，有提示框跳出
            m_labelsR.onRemoveCallback = (ReorderableList list) =>
            {
                if (EditorUtility.DisplayDialog("提示！", "移除这个元素吗?", "移除", "取消"))
                {
                    ReorderableList.defaultBehaviours.DoRemoveButton(list);
                }
            };

        }

        public override void OnInspectorGUI()
        {
            GUIStyle titleStyle = new GUIStyle();
            titleStyle.fontSize = 13;
            titleStyle.normal.textColor = new Color(255f / 255f, 0 / 255f, 0 / 255f, 255f / 255f);
            UnityEditor.EditorGUILayout.LabelField("--------------------------标签配置--------------------", titleStyle);
            GUILayout.Space(5f);

            serializedObject.Update();
            m_labelsR.DoLayoutList();

            GUILayout.Space(5f);
            serializedObject.ApplyModifiedProperties();
        }
    }
}
