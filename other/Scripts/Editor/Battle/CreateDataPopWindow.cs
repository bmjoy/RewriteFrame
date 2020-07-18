using LeyoutechEditor.Core.EGUI;
using LeyoutechEditor.Core.Window;
using System;
using UnityEditor;
using UnityEngine;

namespace EternityEditor.Battle
{
    public class CreateDataPopWindow : DotPopupWindow
    {
        public static void ShowWin(Rect position, Action<int> createCallback, Func<int, bool> checkCallback)
        {
            var win = GetPopupWindow<CreateDataPopWindow>();
            win.Show<CreateDataPopWindow>(position, true, false);
            win.createCallback = createCallback;
            win.checkCallback = checkCallback;
        }

        private Action<int> createCallback = null;
        private Func<int, bool> checkCallback = null;
        private int id = 0;

        private string errorMsg = "";

        private GUIStyle boldCenterStyle = null;

        protected override void OnGUI()
        {
            if (boldCenterStyle == null)
            {
                boldCenterStyle = new GUIStyle(EditorStyles.boldLabel);
                boldCenterStyle.alignment = TextAnchor.MiddleCenter;
                boldCenterStyle.fontSize = 16;
            }

            base.OnGUI();
            EditorGUILayout.BeginVertical();
            {
                EditorGUILayout.LabelField("New Data Creater", boldCenterStyle);
                EditorGUILayout.Space();
                EditorGUIUtil.BeginLabelWidth(60);
                {
                    id = EditorGUILayout.IntField("Input ID:", id);
                }
                EditorGUIUtil.EndLableWidth();
                GUILayout.FlexibleSpace();
                if (!string.IsNullOrEmpty(errorMsg))
                {
                    EditorGUILayout.HelpBox(errorMsg, MessageType.Error);
                }
                EditorGUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button("Create"))
                    {
                        if (checkCallback != null)
                        {
                            if (checkCallback(id))
                            {
                                createCallback?.Invoke(id);
                                createCallback = null;
                                Close();
                            }
                            else
                            {
                                errorMsg = "Error ID";
                            }
                        }
                        else
                        {
                            createCallback?.Invoke(id);
                            Close();
                        }
                    }
                    if (GUILayout.Button("Cancel"))
                    {
                        Close();
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();

        }
    }
}
