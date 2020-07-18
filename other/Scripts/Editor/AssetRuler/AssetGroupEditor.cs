using LeyoutechEditor.Core.EGUI;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace LeyoutechEditor.Core.AssetRuler
{
    public class AssetGroupEditor : Editor
    {
        SerializedProperty m_IsEnable;
        SerializedProperty m_GroupName;
        SerializedProperty m_AssetAssemblyType;
        SerializedProperty m_AssetSearchers;
        SerializedProperty m_FilterOperations;

        private ReorderableList filterOperationRList;
        private ReorderableList assetSearcherRList;
        protected virtual void OnEnable()
        {
            m_IsEnable = serializedObject.FindProperty("m_IsEnable");
            m_GroupName = serializedObject.FindProperty("m_GroupName");
            m_AssetAssemblyType = serializedObject.FindProperty("m_AssetAssemblyType");
            m_AssetSearchers = serializedObject.FindProperty("m_AssetSearchers");
            m_FilterOperations = serializedObject.FindProperty("m_FilterOperations");

            #region--------------------------------------------- 搜索路径条件列表------------------------------------------------

            assetSearcherRList = new ReorderableList(serializedObject, m_AssetSearchers, false, true, true, true);
            //自定义列表名称
            assetSearcherRList.drawHeaderCallback = (rect) =>
            {
                EditorGUI.LabelField(rect, "资源路径内容搜索列表");
            };

            //自定义绘制列表元素
            assetSearcherRList.drawElementCallback = (curRect, index, isActive, isFocused) =>
            {
                SerializedProperty property = m_AssetSearchers.GetArrayElementAtIndex(index);

                //文件夹路径
                SerializedProperty folder = property.FindPropertyRelative("m_Folder");
                curRect.height = EditorGUIUtility.singleLineHeight;
                folder.stringValue = EditorGUIUtil.DrawAssetFolderSelection(curRect, "m_Folder", folder.stringValue);

                //是否包括子文件夹
                SerializedProperty includeSubfolder = property.FindPropertyRelative("m_IncludeSubfolder");
                curRect.y += curRect.height;
                EditorGUI.PropertyField(curRect, includeSubfolder);

                //文件名过滤器正则表达式
                SerializedProperty fileNameFilterRegex = property.FindPropertyRelative("m_FileNameFilterRegex");
                curRect.y += curRect.height;
                EditorGUI.PropertyField(curRect, fileNameFilterRegex);
            };

            //设置元素高度
            assetSearcherRList.elementHeightCallback = (index) =>
            {
                return EditorGUIUtility.singleLineHeight * 3 + 10;
            };

            //当添加新元素时的回调函数，自定义新元素的值
            assetSearcherRList.onAddCallback = (ReorderableList list) =>
            {
                if (list.serializedProperty != null)
                {
                    list.serializedProperty.arraySize++;
                    list.index = list.serializedProperty.arraySize - 1;
                    //设置默认
                    SerializedProperty item = list.serializedProperty.GetArrayElementAtIndex(list.index);
                    SerializedProperty folder = item.FindPropertyRelative("m_Folder");
                    folder.stringValue = ""; 
                    SerializedProperty includeSubfolder = item.FindPropertyRelative("m_IncludeSubfolder");
                    includeSubfolder.boolValue = true;
                    SerializedProperty fileNameFilterRegex = item.FindPropertyRelative("m_FileNameFilterRegex");
                    fileNameFilterRegex.stringValue = "";
                }
                else
                {
                    ReorderableList.defaultBehaviours.DoAddButton(list);
                }
            };

            //当删除元素时候的回调函数，实现删除元素时，有提示框跳出
            assetSearcherRList.onRemoveCallback = (ReorderableList list) =>
            {
                if (EditorUtility.DisplayDialog("提示！", "移除这个元素吗?", "移除", "取消"))
                {
                    ReorderableList.defaultBehaviours.DoRemoveButton(list);
                }
            };

            #endregion---------------------------------------end------ 搜索路径条件列表--------------------------------------




            #region--------------------------------------------- 过滤条件列表--对搜索路径二次过滤-------------------------
            filterOperationRList = new ReorderableList(serializedObject,m_FilterOperations, true, true, true, true);
            //自定义列表名称
            filterOperationRList.drawHeaderCallback = (rect) =>
            {
                EditorGUI.LabelField(rect, "二次过滤条件列表");
            };
            //自定义绘制列表元素
            filterOperationRList.drawElementCallback = (rect, index, isActive, isFocused) =>
            {
                EditorGUIUtil.BeginLabelWidth(40);
                {
                    EditorGUI.PropertyField(rect, m_FilterOperations.GetArrayElementAtIndex(index),new GUIContent(""+index));  
                }
                EditorGUIUtil.EndLableWidth();
            };

            //添加元素
            filterOperationRList.onAddCallback += (ReorderableList list) =>
            {
                m_FilterOperations.InsertArrayElementAtIndex(m_FilterOperations.arraySize);
            };

            //当删除元素时候的回调函数，实现删除元素时，有提示框跳出
            filterOperationRList.onRemoveCallback = (ReorderableList list) =>
            {
                if (EditorUtility.DisplayDialog("提示！", "移除这个元素吗?", "移除", "取消"))
                {
                    ReorderableList.defaultBehaviours.DoRemoveButton(list);
                }
            };

            #endregion---------------------------------------end------ 对搜索路径二次过滤--------------------------------------

        }

        //绘制基础信息
        protected void DrawBaseInfo(GUIStyle titleStyle)
        {
            EditorGUILayout.PropertyField(m_IsEnable);
            EditorGUILayout.PropertyField(m_GroupName);

            GUILayout.Space(5f);
            titleStyle.normal.textColor = new Color(0 / 255f, 255 / 255f, 0 / 255f, 255f / 255f);
            UnityEditor.EditorGUILayout.LabelField("注意：<集合类型> 必须跟assetaddress_assembly 类型一致 ", titleStyle);
            EditorGUILayout.PropertyField(m_AssetAssemblyType);
        }

        /// <summary>
        /// 绘制资源路径搜索列表
        /// </summary>
        protected void DrawAssetSearcher()
        {
            assetSearcherRList.DoLayoutList();
        }

        /// <summary>
        /// 绘制二次过滤列表
        /// </summary>
        protected void DrawFilterOperations()
        {
            filterOperationRList.DoLayoutList();
        }
    }
}
