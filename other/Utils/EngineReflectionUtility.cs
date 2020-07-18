using System;
using System.Reflection;
using UnityEngine;

namespace Leyoutech.Utility
{
	public static class EngineReflectionUtility
	{
		public static class GUILayoutUtility
		{
			private static MethodInfo GET_TOP_LEVEL_METHODINFO;

			public static object GetTopLevel()
			{
				if (GET_TOP_LEVEL_METHODINFO == null)
				{
					GET_TOP_LEVEL_METHODINFO = typeof(UnityEngine.GUILayoutUtility)
						.GetProperty("topLevel", BindingFlags.Static | BindingFlags.NonPublic)
						.GetMethod;
				}

				return GET_TOP_LEVEL_METHODINFO.Invoke(null, null);
			}
		}

		public static class GUILayoutGroup
		{
			private static MethodInfo GET_LAST_METHODINFO;

			public static Rect GetLast(object guiLayoutGroup)
			{
				if (GET_LAST_METHODINFO == null)
				{
					GET_LAST_METHODINFO = Type.GetType("UnityEngine.GUILayoutGroup, UnityEngine.IMGUIModule")
						.GetMethod("GetLast", BindingFlags.Public | BindingFlags.Instance);
				}
				
				return (Rect)GET_LAST_METHODINFO.Invoke(guiLayoutGroup, null);
			}
		}
	}
}