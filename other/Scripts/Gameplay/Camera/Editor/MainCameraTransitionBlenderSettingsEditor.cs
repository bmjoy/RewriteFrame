using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using static MainCameraComponent;

[CustomEditor(typeof(MainCameraTransitionBlenderSettings))]
public class MainCameraTransitionBlenderSettingsEditor : Editor
{
	private ReorderableList m_BlendList;
	private string[] m_CMTypes;

	[EditorExtend.CTMenuItem("Camera/Create Main Camera Transition Blender Settings")]
	public static void CreateMainCameraTransitionBlenderSettings()
	{
		AssetDatabase.CreateAsset(CreateInstance(typeof(MainCameraTransitionBlenderSettings))
			, "Assets/Artwork/Camera/Camera_Main_Transition_Blends.asset");
	}

	public override void OnInspectorGUI()
	{
		serializedObject.Update();

		if (m_BlendList == null)
		{
			SetupBlendList();
		}

		m_BlendList.DoLayoutList();
		serializedObject.ApplyModifiedProperties();
		EditorUtility.SetDirty(target);
	}

	private void SetupBlendList()
	{
		m_CMTypes = new string[(int)CMType.Count + 2];
		m_CMTypes[0] = MainCameraTransitionBlenderSettings.ANY_CAMERA_LABEL;
		m_CMTypes[m_CMTypes.Length - 1] = CMType.Notset.ToString();
		for (int iCM = 0; iCM < (int)CMType.Count; iCM++)
		{
			m_CMTypes[iCM + 1] = ((CMType)iCM).ToString();
		}
		m_BlendList = new ReorderableList(serializedObject
			, serializedObject.FindProperty("m_CustomBlends")
			, true
			, true
			, true
			, true);

		float vSpace = 2;
		float hSpace = 3;
		float floatFieldWidth = EditorGUIUtility.singleLineHeight * 2.5f;

		m_BlendList.drawHeaderCallback = (Rect rect) =>
		{
			rect.width = (rect.width - EditorGUIUtility.singleLineHeight + 3 * hSpace) * 0.25f;

			Vector2 pos = rect.position;
			pos.x += EditorGUIUtility.singleLineHeight;
			rect.position = pos;
			EditorGUI.LabelField(rect, "From");

			pos.x += rect.width + hSpace;
			rect.position = pos;
			EditorGUI.LabelField(rect, "To");

			pos.x += rect.width + hSpace;
			rect.position = pos;
			EditorGUI.LabelField(rect, "BlendHint");

			pos.x += rect.width + hSpace;
			rect.position = pos;
			EditorGUI.LabelField(rect, "InheritPosition");
		};

		m_BlendList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
		{
			SerializedProperty element
				= m_BlendList.serializedProperty.GetArrayElementAtIndex(index);

			rect.y += vSpace;
			rect.height = EditorGUIUtility.singleLineHeight;
			rect.width = (rect.width - EditorGUIUtility.singleLineHeight + 3 * hSpace) * 0.25f;

			Vector2 pos = rect.position;
			SerializedProperty fromProp = element.FindPropertyRelative("From");
			int current = GetCMTypeIndex(fromProp.stringValue);
			int sel = EditorGUI.Popup(rect, current, m_CMTypes);
			fromProp.stringValue = m_CMTypes[sel];

			pos.x += rect.width + hSpace;
			rect.position = pos;
			SerializedProperty toProp = element.FindPropertyRelative("To");
			current = GetCMTypeIndex(toProp.stringValue);
			sel = EditorGUI.Popup(rect, current, m_CMTypes);
			toProp.stringValue = m_CMTypes[sel];

			pos.x += rect.width + hSpace;
			rect.position = pos;
			SerializedProperty blendProp = element.FindPropertyRelative("BlendHint");
			EditorGUI.PropertyField(rect, blendProp, GUIContent.none);

			pos.x += rect.width + hSpace;
			rect.position = pos;
			SerializedProperty inheritProp = element.FindPropertyRelative("InheritPosition");
			EditorGUI.PropertyField(rect, inheritProp, GUIContent.none);
		};

		m_BlendList.onAddCallback = (ReorderableList l) =>
		{
			++l.serializedProperty.arraySize;
		};
	}

	private int GetCMTypeIndex(string stringValue)
	{
		for (int iCM = 0; iCM < m_CMTypes.Length; iCM++)
		{
			if (stringValue == m_CMTypes[iCM])
			{
				return iCM;
			}
		}
		return 0;
	}
}