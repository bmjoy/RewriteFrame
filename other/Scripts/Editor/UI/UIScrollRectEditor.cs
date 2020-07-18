using UnityEditor;
using UnityEditor.UI;

[CustomEditor(typeof(UIScrollRect), true)]
[CanEditMultipleObjects]
public class UIScrollRectEditor : ScrollRectEditor
{
    SerializedProperty m_NeedHead;
    SerializedProperty m_ContentPadding;
    SerializedProperty m_ContentSpacing;
    SerializedProperty m_HeadHeight;
    SerializedProperty m_ListColumns;
    SerializedProperty m_ListPadding;
    SerializedProperty m_CellSize;
    SerializedProperty m_CellSpace;

    protected override void OnEnable()
    {
        base.OnEnable();

        m_NeedHead = serializedObject.FindProperty("m_NeedHead");
        m_ContentPadding = serializedObject.FindProperty("m_ContentPadding");
        m_ContentSpacing = serializedObject.FindProperty("m_ContentSpacing");
        m_HeadHeight = serializedObject.FindProperty("m_HeadHeight");
        m_ListColumns = serializedObject.FindProperty("m_ListColumns");
        m_ListPadding = serializedObject.FindProperty("m_ListPadding");
        m_CellSize = serializedObject.FindProperty("m_CellSize");
        m_CellSpace = serializedObject.FindProperty("m_CellSpace");
    }

    private bool m_ContentShow = false;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        serializedObject.Update();

        EditorGUILayout.Space();
        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(m_NeedHead);
        EditorGUILayout.PropertyField(m_HeadHeight);
        EditorGUILayout.Space();
        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(m_ListColumns);
        EditorGUILayout.PropertyField(m_CellSize);
        EditorGUILayout.PropertyField(m_CellSpace);
        EditorGUILayout.PropertyField(m_ListPadding,true);
        EditorGUILayout.Space();
        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(m_ContentSpacing);
        EditorGUILayout.PropertyField(m_ContentPadding, true);
        EditorGUILayout.EndFoldoutHeaderGroup();

        serializedObject.ApplyModifiedProperties();
    }
}