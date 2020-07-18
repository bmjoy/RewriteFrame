using LeyoutechEditor.Core.Util;
using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using SystemObject = System.Object;

namespace LeyoutechEditor.Core.EGUI
{
    public static class EditorGUILayoutUtil
    {
        /// <summary>
        /// 绘制文件夹选择
        /// </summary>
        /// <param name="label"></param>
        /// <param name="diskFolder"></param>
        /// <param name="isReadonly"></param>
        /// <returns></returns>
        public static string DrawDiskFolderSelection(string label, string diskFolder, bool isReadonly = true)
        {
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUI.BeginDisabledGroup(isReadonly);
                {
                    EditorGUILayout.TextField(label, diskFolder);
                }
                EditorGUI.EndDisabledGroup();

                if (GUILayout.Button(new GUIContent(EditorGUIUtil.FolderIcon), GUILayout.Width(20), GUILayout.Height(20)))
                {
                    diskFolder = EditorUtility.OpenFolderPanel("folder", diskFolder, "");
                }
                if (GUILayout.Button("\u2716", GUILayout.Width(20), GUILayout.Height(20)))
                {
                    diskFolder = "";
                }
            }
            EditorGUILayout.EndHorizontal();

            return diskFolder;
        }
    }
}
