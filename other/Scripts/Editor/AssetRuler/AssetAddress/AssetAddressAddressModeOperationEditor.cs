/*===============================
 * Author: [Allen]
 * Purpose: AssetAddressAddressModeOperation窗口
 * Time: 2019/10/30 16:52:18
================================*/
using UnityEditor;
using UnityEngine;

namespace LeyoutechEditor.Core.AssetRuler.AssetAddress
{
    [CustomEditor(typeof(AssetAddressAddressModeOperation))]
    public class AssetAddressAddressModeOperationEditor : Editor
    {
        SerializedProperty m_AddressMode;
        SerializedProperty m_FileNameFormat;

        protected virtual void OnEnable()
        {
            m_AddressMode = serializedObject.FindProperty("m_AddressMode");
            m_FileNameFormat = serializedObject.FindProperty("m_FileNameFormat");
        }

        public override void OnInspectorGUI()
        {
            GUIStyle titleStyle = new GUIStyle();
            titleStyle.fontSize = 13;
            titleStyle.normal.textColor = new Color(255f / 255f, 0 / 255f, 0 / 255f, 255f / 255f);
            UnityEditor.EditorGUILayout.LabelField("--------------------------地址模式配置---------------------", titleStyle);
            GUILayout.Space(5f);

            serializedObject.Update();

            EditorGUILayout.PropertyField(m_AddressMode);

            if (m_AddressMode.enumValueIndex == (int)AssetAddressMode.FileFormatName)
            {
                EditorGUILayout.PropertyField(m_FileNameFormat);
            }

            GUILayout.Space(5f);
            serializedObject.ApplyModifiedProperties();
        }
    }
}
