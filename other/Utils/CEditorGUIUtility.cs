#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Leyoutech.Utility
{
	public static class CEditorGUIUtility
	{
		/// <summary>
		/// 项目中路径，格式：
		///		Assets/……
		///	未来会支持Packages目录
		/// </summary>
		public static string AssetFolderField(string folder, string display, string defaultFoler, string defaultFolderName)
		{
			EditorGUILayout.BeginHorizontal();
			folder = EditorGUILayout.TextField(display, folder);
			if (GUILayout.Button("……", GUILayout.Width(24)))
			{
				folder = EditorUtility.OpenFolderPanel("Choose " + display, defaultFoler, defaultFolderName);
				folder = folder.Replace(Application.dataPath, "Assets/");
			}
			EditorGUILayout.EndHorizontal();

			return folder;
		}

		public static string AssetFolderField(string folder, string display)
		{
			return AssetFolderField(folder, display, Application.dataPath, "");
		}

		/// <summary>
		/// 项目中文件，格式：
		///		Assets/……
		///	未来会支持Packages目录
		/// </summary>
		public static string AssetFileField(string file, string display, string directory, string extension)
		{
			EditorGUILayout.BeginHorizontal();
			file = EditorGUILayout.TextField(display, file);
			if (GUILayout.Button("……", GUILayout.Width(24)))
			{
				file = EditorUtility.OpenFilePanel("Choose " + display, directory, extension);
				file = file.Replace(Application.dataPath, "Assets");
			}
			EditorGUILayout.EndHorizontal();

			return file;
		}

		public static string AssetFileField(string file, string display, string extension)
		{
			return AssetFileField(file, display, Application.dataPath, extension);
		}
	}
}
#endif