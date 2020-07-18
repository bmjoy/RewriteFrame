using LeyoutechEditor.Core.EGUI;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace LeyoutechEditor.Core.AssetRuler
{
    [CustomEditor(typeof(AssetGroupFilterOperation))]
    public class AssetGroupFilterOperationEditor : Editor
    {
        private SerializedProperty m_FilterComposeType;
        private SerializedProperty m_AssetFilters;

        private SerializedProperty m_OperationComposeType;
        private SerializedProperty m_AssetOperations;

        private ReorderableList m_AssetFilterRList;
        private ReorderableList m_AssetOperationRList;

        private void OnEnable()
        {
            m_FilterComposeType = serializedObject.FindProperty("m_FilterComposeType");
            m_AssetFilters = serializedObject.FindProperty("m_AssetFilters");
            m_OperationComposeType = serializedObject.FindProperty("m_OperationComposeType");
            m_AssetOperations = serializedObject.FindProperty("m_AssetOperations");


            #region--------------------------------------------- 二级筛选条件列表------------------------------------------------

            m_AssetFilterRList = new ReorderableList(serializedObject, m_AssetFilters, true, true, true, true);
            //自定义列表名称
            m_AssetFilterRList.drawHeaderCallback = (rect) =>
            {
                EditorGUI.LabelField(rect, "二级删选允许条件列表");
            };

            //自定义绘制列表元素
            m_AssetFilterRList.drawElementCallback = (rect, index, isActive, isFocused) =>
            {
                EditorGUIUtil.BeginLabelWidth(40);
                {
                    EditorGUI.PropertyField(rect, m_AssetFilters.GetArrayElementAtIndex(index), new GUIContent("" + index));
                }
                EditorGUIUtil.EndLableWidth();
            };

            //添加元素
            m_AssetFilterRList.onAddCallback += (ReorderableList list) =>
            {
                m_AssetFilters.InsertArrayElementAtIndex(m_AssetFilters.arraySize);
            };

            //当删除元素时候的回调函数，实现删除元素时，有提示框跳出
            m_AssetFilterRList.onRemoveCallback = (ReorderableList list) =>
            {
                if (EditorUtility.DisplayDialog("提示！", "移除这个元素吗?", "移除", "取消"))
                {
                    ReorderableList.defaultBehaviours.DoRemoveButton(list);
                }
            };

            #endregion---------------------------------------end------ 二级筛选条件列表--------------------------------------



            #region--------------------------------------------规则列表------------------------------------------------

            m_AssetOperationRList = new ReorderableList(serializedObject, m_AssetOperations, true, true, true, true);
            //自定义列表名称
            m_AssetOperationRList.drawHeaderCallback = (rect) =>
            {
                EditorGUI.LabelField(rect, "规则列表");
            };

            //自定义绘制列表元素
            m_AssetOperationRList.drawElementCallback = (rect, index, isActive, isFocused) =>
            {
                EditorGUIUtil.BeginLabelWidth(40);
                {
                    EditorGUI.PropertyField(rect, m_AssetOperations.GetArrayElementAtIndex(index), new GUIContent("" + index));
                }
                EditorGUIUtil.EndLableWidth();
            };

            //添加元素
            m_AssetOperationRList.onAddCallback += (list) =>
            {
                m_AssetOperations.InsertArrayElementAtIndex(m_AssetOperations.arraySize);
            };

            //当删除元素时候的回调函数，实现删除元素时，有提示框跳出
            m_AssetOperationRList.onRemoveCallback = (ReorderableList list) =>
            {
                if (EditorUtility.DisplayDialog("提示！", "移除这个元素吗?", "移除", "取消"))
                {
                    ReorderableList.defaultBehaviours.DoRemoveButton(list);
                }
            };

            #endregion---------------------------------------end------ 规则列表--------------------------------------
        }

        public override void OnInspectorGUI()
        {
            GUIStyle titleStyle = new GUIStyle();
            titleStyle.fontSize = 13;
            titleStyle.normal.textColor = new Color(255f / 255f, 0 / 255f, 0 / 255f, 255f / 255f);
            UnityEditor.EditorGUILayout.LabelField("--------------------------二级筛选配置---------------------", titleStyle);
            GUILayout.Space(5f);

            serializedObject.Update();

            //二级筛选条件列表
            EditorGUILayout.PropertyField(m_FilterComposeType);
            m_AssetFilterRList.DoLayoutList();

            EditorGUILayout.Space();
            titleStyle.normal.textColor = new Color(0 / 255f, 255 / 255f, 0 / 255f, 255f / 255f);
            UnityEditor.EditorGUILayout.LabelField("<规则条件类型>没特殊需求，选择 <全部>", titleStyle);

            //规则列表
            EditorGUILayout.PropertyField(m_OperationComposeType);
            m_AssetOperationRList.DoLayoutList();

            serializedObject.ApplyModifiedProperties();
        }
    }
}
