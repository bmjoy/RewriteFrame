using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace EditorExtend
{
    public static class ToolbarExtend
    {
        private static Type TOOLBAR_TYPE = typeof(Editor).Assembly.GetType("UnityEditor.Toolbar");
        private static Type GUIVIEW_TYPE = typeof(Editor).Assembly.GetType("UnityEditor.GUIView");
        private static PropertyInfo VISUALTREE_PROPERTYINFO = GUIVIEW_TYPE.GetProperty("visualTree"
            , BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        private static FieldInfo ONGUI_HANDLER_FIELDINFO = typeof(IMGUIContainer).GetField("m_OnGUIHandler"
            , BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        private static FieldInfo TOOL_ICONS_FIELDINFO = TOOLBAR_TYPE.GetField("s_ShownToolIcons"
            , BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);

        public static Action OnLeftToolbarGUI;
        public static Action OnRightToolbarGUI;

        private static ScriptableObject ms_CurrentToolbar;
        private static int ms_ToolIconCount;
        private static GUIStyle ms_CommandStyle;
        private static GUIStyle ms_CommandButtonStyle;

        static ToolbarExtend()
        {
            EditorApplication.update -= OnUpdate;
            EditorApplication.update += OnUpdate;
        }

        public static GUIStyle GetCommandButtonStyle()
        {
            return ms_CommandButtonStyle;
        }

        private static void OnUpdate()
        {
            if (ms_CurrentToolbar != null)
            {
                return;
            }

            UnityEngine.Object[] toolbars = Resources.FindObjectsOfTypeAll(TOOLBAR_TYPE);
            ms_CurrentToolbar = toolbars.Length > 0 ? (ScriptableObject)toolbars[0] : null;
            if (ms_CurrentToolbar != null)
            {

                // ms_ToolIconCount = TOOL_ICONS_FIELDINFO != null ? ((Array)TOOL_ICONS_FIELDINFO.GetValue(null)).Length : 6;

                VisualElement visualTree = (VisualElement)VISUALTREE_PROPERTYINFO.GetValue(ms_CurrentToolbar, null);
                IMGUIContainer container = (IMGUIContainer)visualTree[0];
                Action onGUIHandler = (Action)ONGUI_HANDLER_FIELDINFO.GetValue(container);
                onGUIHandler -= OnGUI;
                onGUIHandler += OnGUI;
                ONGUI_HANDLER_FIELDINFO.SetValue(container, onGUIHandler);
            }
        }

        private static void OnGUI()
        {
            if (ms_CommandStyle == null)
            {
                ms_CommandStyle = new GUIStyle("CommandLeft");

                ms_CommandButtonStyle = new GUIStyle("Command")
                {
                    fontSize = 12,
                    alignment = TextAnchor.MiddleCenter,
                    imagePosition = ImagePosition.ImageAbove,
                    fontStyle = FontStyle.Bold
                };
            }

            float screenWidth = EditorGUIUtility.currentViewWidth;
            // Following calculations match code reflected from Toolbar.OldOnGUI()
            float playButtonsPosition = (screenWidth - 100) / 2 - 20;

            Rect leftRect = new Rect(0, 0, screenWidth, Screen.height);
            leftRect.xMin += 10; // Spacing left
            leftRect.xMin += 32 * 20; // Tool buttons
            leftRect.xMin += 20; // Spacing between tools and pivot
            leftRect.xMin += 64 * 2; // Pivot buttons
            leftRect.xMax = playButtonsPosition;

            Rect rightRect = new Rect(0, 0, screenWidth, Screen.height);
            rightRect.xMin = playButtonsPosition;
            rightRect.xMin += ms_CommandStyle.fixedWidth * 3; // Play buttons
            rightRect.xMax = screenWidth;
            rightRect.xMax -= 10; // Spacing right
            rightRect.xMax -= 80; // Layout
            rightRect.xMax -= 10; // Spacing between layout and layers
            rightRect.xMax -= 80; // Layers
            rightRect.xMax -= 20; // Spacing between layers and account
            rightRect.xMax -= 80; // Account
            rightRect.xMax -= 10; // Spacing between account and cloud
            rightRect.xMax -= 32; // Cloud
            rightRect.xMax -= 10; // Spacing between cloud and collab
            rightRect.xMax -= 78; // Colab

            // Add spacing around existing controls
            leftRect.xMin += 10;
            leftRect.xMax -= 10;
            rightRect.xMin += 10;
            rightRect.xMax -= 10;

            // Add top and bottom margins
            leftRect.y = 5;
            leftRect.height = 24;
            rightRect.y = 5;
            rightRect.height = 24;

            if (leftRect.width > 0)
            {
                GUILayout.BeginArea(leftRect);
                GUILayout.BeginHorizontal();
                OnLeftToolbarGUI?.Invoke();
                GUILayout.EndHorizontal();
                GUILayout.EndArea();
            }

            if (rightRect.width > 0)
            {
                GUILayout.BeginArea(rightRect);
                GUILayout.BeginHorizontal();
                OnRightToolbarGUI?.Invoke();
                GUILayout.EndHorizontal();
                GUILayout.EndArea();
            }
        }
    }
}