using UnityEditor;
using UnityEngine;

namespace LeyoutechEditor.Core.AssetRuler.AssetAddress
{
    [CustomEditor(typeof(AssetAddressAssembly))]
    public class AssetAddressAssemblyEditor : AssetAssemblyEditor
    {
        public override void OnInspectorGUI()
        {
            GUIStyle titleStyle = new GUIStyle();
            titleStyle.fontSize = 13;
            titleStyle.normal.textColor = new Color(255f / 255f, 0 / 255f, 0 / 255f, 255f / 255f);
            EditorGUILayout.LabelField("--------------------------资源组合配置---------------------", titleStyle);
            GUILayout.Space(5f);


            serializedObject.Update();

            //绘制Group 列表
            DrawGroup();
            //执行自行寻找group配置
            DrawOperation();

            serializedObject.ApplyModifiedProperties();
        }
    }
}
