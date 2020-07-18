using LeyoutechEditor.Core.Util;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace LeyoutechEditor.Core.EGUI
{
    public static class EditorGUIUtil
    {
        /// <summary>
        /// 定义颜色
        /// </summary>
        public static Color BorderColor
        {
            get
            {
                return EditorGUIUtility.isProSkin ? new Color(0.13f, 0.13f, 0.13f) : new Color(0.51f, 0.51f, 0.51f);
            }
        }
        /// <summary>
        /// 背景色
        /// </summary>
        public static Color BackgroundColor
        {
            get
            {
                return EditorGUIUtility.isProSkin ? new Color(0.18f, 0.18f, 0.18f) : new Color(0.83f, 0.83f, 0.83f);
            }
        }

        /// <summary>
        /// 文件夹图标
        /// </summary>
        private static Texture2D folderIcon = null;
        public static Texture2D FolderIcon
        {
            get
            {
                if (folderIcon == null)
                {
                    folderIcon = EditorGUIUtility.FindTexture("Folder Icon");
                }
                return folderIcon;
            }
        }

        /// <summary>
        /// 绘制区域外框
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="color"></param>
        public static void DrawAreaLine(Rect rect, Color color)
        {
            Handles.color = color;

            var points = new Vector3[] {
                new Vector3(rect.x, rect.y, 0),
                new Vector3(rect.x + rect.width, rect.y, 0),
                new Vector3(rect.x + rect.width, rect.y + rect.height, 0),
                new Vector3(rect.x, rect.y + rect.height, 0),
            };

            var indexies = new int[] {
                0, 1, 1, 2, 2, 3, 3, 0,
            };

            Handles.DrawLines(points, indexies);
        }

        /// <summary>
        /// 设定Label的宽度
        /// </summary>
        private static Stack<float> labelWidthStack = new Stack<float>();
        public static void BeginLabelWidth(float labelWidth)
        {
            labelWidthStack.Push(EditorGUIUtility.labelWidth);
            EditorGUIUtility.labelWidth = labelWidth;
        }

        public static void EndLableWidth()
        {
            if (labelWidthStack.Count > 0)
                EditorGUIUtility.labelWidth = labelWidthStack.Pop();
        }

        /// <summary>
        /// 设置GUI的颜色
        /// </summary>
        private static Stack<Color> guiColorStack = new Stack<Color>();
        public static void BeginGUIColor(Color color)
        {
            guiColorStack.Push(GUI.color);
            GUI.color = color;
        }
        public static void EndGUIColor()
        {
            if (guiColorStack.Count > 0)
                GUI.color = guiColorStack.Pop();
        }

        /// <summary>
        /// 开始设定GUI的背景色
        /// </summary>
        private static Stack<Color> guiBgColorStack = new Stack<Color>();
        public static void BeginGUIBackgroundColor(Color color)
        {
            guiBgColorStack.Push(GUI.backgroundColor);
            GUI.backgroundColor= color;
        }
        public static void EndGUIBackgroundColor()
        {
            if (guiBgColorStack.Count > 0)
                GUI.backgroundColor = guiBgColorStack.Pop();
        }

        /// <summary>
        /// 开始GUI的缩进
        /// </summary>
        public static void BeginIndent()
        {
            EditorGUI.indentLevel++;
        }

        public static void EndIndent()
        {
            EditorGUI.indentLevel--;
        }

        /// <summary>
        /// 绘制文件夹选择条目
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="label">条目标题</param>
        /// <param name="assetFolder">文件夹路径</param>
        /// <param name="isReadonly">是否是只读</param>
        /// <returns></returns>
        public static string DrawAssetFolderSelection(Rect rect, string label, string assetFolder, bool isReadonly = true)
        {
            string folder = assetFolder;

            //标题，文件夹路径
            EditorGUI.BeginDisabledGroup(isReadonly);
            {
                folder = EditorGUI.TextField(new Rect(rect.x, rect.y, rect.width - 40, rect.height), label, assetFolder);
            }
            EditorGUI.EndDisabledGroup();

            //文件夹打开按钮
            if (GUI.Button(new Rect(rect.x+rect.width - 40, rect.y, 20, rect.height), new GUIContent(EditorGUIUtil.FolderIcon)))
            {
                string folderPath = EditorUtility.OpenFolderPanel("folder", folder, "");//显示“打开文件夹”对话框，返回选择的路径名 参数:标题 ,文件夹,defaultName
                if (!string.IsNullOrEmpty(folderPath))
                {
                    folder = PathUtil.GetAssetPath(folderPath);
                }
            }

            //文件夹路径清空按钮
            if (GUI.Button(new Rect(rect.x + rect.width - 20, rect.y, 20, rect.height), "\u2716"))
            {
                folder = "";
            }
            return folder;
        }
    }

    
}
