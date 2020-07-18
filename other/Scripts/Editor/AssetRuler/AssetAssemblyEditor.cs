using LeyoutechEditor.Core.EGUI;
using LeyoutechEditor.Core.Util;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace LeyoutechEditor.Core.AssetRuler
{
    public class AssetAssemblyEditor : Editor
    {
        private SerializedProperty m_AssetAssemblyType = null;            // 资源集合类型
        private SerializedProperty m_AssetGroups = null;                         // 资源组列表
		/// <summary>
		/// 用于标识测试group名的前缀字符串
		/// </summary>
		private SerializedProperty m_TestGroupPrefix = null;

		private ReorderableList groupList = null;
        protected virtual void OnEnable()
        {
            m_AssetAssemblyType = serializedObject.FindProperty("m_AssetAssemblyType");
            m_AssetGroups = serializedObject.FindProperty("m_AssetGroups");
			m_TestGroupPrefix = serializedObject.FindProperty("TestGroupPrefix");

			groupList = new ReorderableList(serializedObject, m_AssetGroups, false, true, false, false);

            //自定义列表名称
            groupList.drawHeaderCallback = (rect) =>
            {
                EditorGUI.LabelField(rect, "Asset Group List:");
            };

            //自定义绘制列表元素
            groupList.drawElementCallback = (rect, index, isActive, isFocused) =>
            {
                EditorGUIUtil.BeginLabelWidth(40);
                {
                    EditorGUI.PropertyField(rect, m_AssetGroups.GetArrayElementAtIndex(index), new GUIContent("" + index));
                }
                EditorGUIUtil.EndLableWidth();
            };
        }

        protected override bool ShouldHideOpenButton()
        {
            return true;
        }

        //绘制Group 列表 ，在OnInspectorGUI 内执行
        protected void DrawGroup()
        {
            EditorGUILayout.PropertyField(m_AssetAssemblyType);

			m_TestGroupPrefix.stringValue = EditorGUILayout.TextField("忽略以此字符串开头的Group", m_TestGroupPrefix.stringValue);

            groupList.DoLayoutList(); //自动布局绘制列表

			serializedObject.ApplyModifiedProperties();
		}

        /// <summary>
        ///自动查找 Group 配置插入到assetGroups 执行显示
        /// </summary>
        protected void DrawOperation()
        {
            if (GUILayout.Button("自动查找 Group 配置", GUILayout.Height(40)))
            {
                AutoFindGroup();
            }
        }

        //自动查找 Group 配置插入到assetGroups 执行显示
        public void AutoFindGroup()
        {
            string[] assetPaths = AssetDatabaseUtil.FindAssets<AssetGroup>();
            List<AssetGroup> groupList = new List<AssetGroup>();
            foreach (var assetPath in assetPaths)
            {
                AssetGroup group = AssetDatabase.LoadAssetAtPath<AssetGroup>(assetPath);
                if (group != null && group.m_AssetAssemblyType == (AssetAssemblyType)m_AssetAssemblyType.intValue) //资源集合类型一致
                {
					if (group.name.IndexOf(m_TestGroupPrefix.stringValue) == 0)
						continue;

                    groupList.Add(group);
                }
            }
            m_AssetGroups.ClearArray();
            for (int i = 0; i < groupList.Count; ++i)
            {
                m_AssetGroups.InsertArrayElementAtIndex(i);//插入
                m_AssetGroups.GetArrayElementAtIndex(i).objectReferenceValue = groupList[i]; //引用赋值
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
