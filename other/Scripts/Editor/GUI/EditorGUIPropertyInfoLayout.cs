using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using SystemObject = System.Object;

namespace LeyoutechEditor.Core.EGUI
{
    public static class EditorGUIPropertyInfoLayout
    {
        public static void PropertyInfoIntPopField(SystemObject target, PropertyInfo pInfo, int[] popValues)
        {
            string label = pInfo.Name;
            if (!pInfo.CanRead)
            {
                UnityEditor.EditorGUILayout.LabelField(label, "PropertyInfo can't read!");
                return;
            }
            SystemObject value = pInfo.GetValue(target);
            if (value == null)
            {
                UnityEditor.EditorGUILayout.LabelField(label, "Value is Null!");
                return;
            }
            using (new EditorGUI.DisabledScope(!pInfo.CanWrite))
            {
                string[] displayStrs = new string[popValues.Length];
                for (int i = 0; i < popValues.Length; i++)
                {
                    displayStrs[i] = popValues[i].ToString();
                }

                value = UnityEditor.EditorGUILayout.IntPopup(label, (int)value, displayStrs, popValues);
            }
            if (value != null && pInfo.CanWrite)
            {
                pInfo.SetValue(target, value);
            }
        }

        public static void PropertyInfoIntField(SystemObject target, PropertyInfo pInfo)
        {
            string label = pInfo.Name;
            if (!pInfo.CanRead)
            {
                UnityEditor.EditorGUILayout.LabelField(label, "PropertyInfo can't read!");
                return;
            }
            SystemObject value = pInfo.GetValue(target);
            if (value == null)
            {
                UnityEditor.EditorGUILayout.LabelField(label, "Value is Null!");
                return;
            }
            using (new EditorGUI.DisabledScope(!pInfo.CanWrite))
            {
                value = UnityEditor.EditorGUILayout.IntField(label, (int)value);
            }
            if (value != null && pInfo.CanWrite)
            {
                pInfo.SetValue(target, value);
            }
        }

        public static void PropertyInfoFloatField(SystemObject target, PropertyInfo pInfo)
        {
            string label = pInfo.Name;
            if (!pInfo.CanRead)
            {
                UnityEditor.EditorGUILayout.LabelField(label, "PropertyInfo can't read!");
                return;
            }
            SystemObject value = pInfo.GetValue(target);
            if (value == null)
            {
                UnityEditor.EditorGUILayout.LabelField(label, "Value is Null!");
                return;
            }
            using (new EditorGUI.DisabledScope(!pInfo.CanWrite))
            {
                value = UnityEditor.EditorGUILayout.FloatField(label, (float)value);
            }
            if (value != null && pInfo.CanWrite)
            {
                pInfo.SetValue(target, value);
            }
        }

        public static void PropertyInfoBoolField(SystemObject target, PropertyInfo pInfo)
        {
            string label = pInfo.Name;
            if (!pInfo.CanRead)
            {
                UnityEditor.EditorGUILayout.LabelField(label, "PropertyInfo can't read!");
                return;
            }
            SystemObject value = pInfo.GetValue(target);
            if (value == null)
            {
                UnityEditor.EditorGUILayout.LabelField(label, "Value is Null!");
                return;
            }
            using (new EditorGUI.DisabledScope(!pInfo.CanWrite))
            {
                value = UnityEditor.EditorGUILayout.Toggle(label, (bool)value);
            }
            if (value != null && pInfo.CanWrite)
            {
                pInfo.SetValue(target, value);
            }
        }

        public static void PropertyInfoStringField(SystemObject target, PropertyInfo pInfo)
        {
            string label = pInfo.Name;
            if (!pInfo.CanRead)
            {
                UnityEditor.EditorGUILayout.LabelField(label, "PropertyInfo can't read!");
                return;
            }
            SystemObject value = pInfo.GetValue(target);
            if (value == null)
            {
                UnityEditor.EditorGUILayout.LabelField(label, "Value is Null!");
                return;
            }
            using (new EditorGUI.DisabledScope(!pInfo.CanWrite))
            {
                value = UnityEditor.EditorGUILayout.TextArea(label, (string)value);
            }
            if (value != null && pInfo.CanWrite)
            {
                pInfo.SetValue(target, value);
            }
        }

        public static void PropertyInfoVector3Field(SystemObject target, PropertyInfo pInfo)
        {
            string label = pInfo.Name;
            if (!pInfo.CanRead)
            {
                UnityEditor.EditorGUILayout.LabelField(label, "PropertyInfo can't read!");
                return;
            }
            SystemObject value = pInfo.GetValue(target);
            if (value == null)
            {
                UnityEditor.EditorGUILayout.LabelField(label, "Value is Null!");
                return;
            }
            using (new EditorGUI.DisabledScope(!pInfo.CanWrite))
            {
                value = UnityEditor.EditorGUILayout.Vector3Field(label, (Vector3)value);
            }
            if (value != null && pInfo.CanWrite)
            {
                pInfo.SetValue(target, value);
            }
        }

        public static void PropertyInfoEnumField(SystemObject target, PropertyInfo pInfo)
        {
            string label = pInfo.Name;
            if (!pInfo.CanRead)
            {
                UnityEditor.EditorGUILayout.LabelField(label, "PropertyInfo can't read!");
                return;
            }
            SystemObject value = pInfo.GetValue(target);
            if (value == null)
            {
                UnityEditor.EditorGUILayout.LabelField(label, "Value is Null!");
                return;
            }
            using (new EditorGUI.DisabledScope(!pInfo.CanWrite))
            {
                value = UnityEditor.EditorGUILayout.EnumPopup(label, (Enum)value);
                value = (int)value;
            }
            if (value != null && pInfo.CanWrite)
            {
                pInfo.SetValue(target, value);
            }
        }
    }
}
