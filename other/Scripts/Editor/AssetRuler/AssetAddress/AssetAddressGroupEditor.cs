using UnityEditor;
using UnityEngine;

namespace LeyoutechEditor.Core.AssetRuler.AssetAddress
{
    [CustomEditor(typeof(AssetAddressGroup))]
    public class AssetAddressGroupEditor :AssetGroupEditor
    {
        private SerializedProperty m_IsGenAddress;
        private SerializedProperty m_IsMain;
        private SerializedProperty m_IsPreload;

        protected override void OnEnable()
        {
            base.OnEnable();
            m_IsGenAddress = serializedObject.FindProperty("m_IsGenAddress");
            m_IsMain = serializedObject.FindProperty("m_IsMain");
            m_IsPreload = serializedObject.FindProperty("m_IsPreload");
        }

        public override void OnInspectorGUI()
        {
            GUIStyle titleStyle = new GUIStyle();
            titleStyle.fontSize = 13;
            titleStyle.normal.textColor = new Color(255f / 255f, 0 / 255f, 0 / 255f, 255f / 255f);
            UnityEditor.EditorGUILayout.LabelField("--------------------------资源组配置---------------------", titleStyle);

            serializedObject.Update();

            DrawBaseInfo(titleStyle);
            EditorGUILayout.PropertyField(m_IsGenAddress);
            EditorGUILayout.PropertyField(m_IsMain);
            EditorGUILayout.PropertyField(m_IsPreload);


            GUILayout.Space(5f);
            //绘制搜索路径条件列表
            DrawAssetSearcher();

            GUILayout.Space(5f);
            //过滤条件列表--对搜索路径二次过滤
            DrawFilterOperations();

            serializedObject.ApplyModifiedProperties();
        }
    }
}
