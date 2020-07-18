/*===============================
 * Author: [Allen]
 * Purpose: AssetAddressPackModeOperationEditor
 * Time: 2019/10/30 20:40:06
================================*/

using UnityEditor;
using UnityEngine;

namespace LeyoutechEditor.Core.AssetRuler.AssetAddress
{
    [CustomEditor(typeof(AssetAddressPackModeOperation))]
    public class AssetAddressPackModeOperationEditor : Editor
    {
        SerializedProperty m_PackMode;
        SerializedProperty m_BundNameMode;
        SerializedProperty m_HasType;



        SerializedProperty m_PackCount;
        SerializedProperty m_PackName;


        protected virtual void OnEnable()
        {
            m_PackMode = serializedObject.FindProperty("m_PackMode");
            m_BundNameMode = serializedObject.FindProperty("m_BundNameMode");
            m_HasType = serializedObject.FindProperty("m_HasType");
            m_PackCount = serializedObject.FindProperty("m_PackCount");
            m_PackName = serializedObject.FindProperty("m_PackName");
        }

        public override void OnInspectorGUI()
        {
            GUIStyle titleStyle = new GUIStyle();
            titleStyle.fontSize = 13;
            titleStyle.normal.textColor = new Color(255f / 255f, 0 / 255f, 0 / 255f, 255f / 255f);
            UnityEditor.EditorGUILayout.LabelField("--------------------------AB 设置规则---------------------", titleStyle);
            GUILayout.Space(5f);

            serializedObject.Update();

            EditorGUILayout.PropertyField(m_PackMode);


            if (m_PackMode.enumValueIndex == (int)AssetBundlePackMode.GroupByCount)
            {
                EditorGUILayout.PropertyField(m_PackCount);
            }

            if (m_PackMode.enumValueIndex == (int)AssetBundlePackMode.TogetherWithFolderSuperaddAssignName||
                m_PackMode.enumValueIndex == (int)AssetBundlePackMode.TogetherWithLotOfFolderAssignName)
            {
                EditorGUILayout.PropertyField(m_PackName);
            }


            EditorGUILayout.PropertyField(m_BundNameMode);

            if (m_BundNameMode.enumValueIndex == (int)AssetBundleNameMode.PathMd5)
            {
                GUILayout.Space(10f);
                titleStyle.normal.textColor = new Color(0 / 255f, 255 / 255f, 0 / 255f, 255f / 255f);
                UnityEditor.EditorGUILayout.LabelField("提示：Pack Name 是 AB 的文件名， Hash 是对于此AB 的 Set Name 属性", titleStyle);
                EditorGUILayout.PropertyField(m_HasType);
            }

            GUILayout.Space(5f);
            serializedObject.ApplyModifiedProperties();
        }
    }
}
