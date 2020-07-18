using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Leyoutech.Utility
{
	public static class GUIUtility
	{
#if UNITY_EDITOR
		public static bool EGL_DelayedIntField(out int newValue, string label, int value, params GUILayoutOption[] options)
		{
			newValue = EditorGUILayout.DelayedIntField(label, value, options);
			return value != newValue;
		}

		public static bool EGL_Toggle(out bool newValue, string label, bool value, params GUILayoutOption[] options)
		{
			newValue = EditorGUILayout.Toggle(label, value, options);
			return value != newValue;
		}

		public static bool EGL_DelayedFloatField(out float newValue, string label, float value, params GUILayoutOption[] options)
		{
			newValue = EditorGUILayout.DelayedFloatField(label, value, options);
			return value != newValue;
		}

		public static bool EGL_Vector3Field(out Vector3 newValue, string label, Vector3 value, params GUILayoutOption[] options)
		{
			newValue = EditorGUILayout.Vector3Field(label, value, options);
			return value != newValue;
		}

		public static bool EnumPopup(out Enum newValue, string label, Enum value, params GUILayoutOption[] options)
		{
			newValue = EditorGUILayout.EnumPopup(label, value, options);
			return value != newValue;
		}
#endif
	}
}