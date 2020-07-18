using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering;

namespace Leyoutech.Utility
{
	public static class UnityEngineReflectionUtility
	{
		public static class GUILayoutUtility
		{
			private static FieldInfo CURRENT_FIELDINFO;

			public static object Current()
			{
				if (CURRENT_FIELDINFO == null)
				{
					CURRENT_FIELDINFO = typeof(UnityEngine.GUILayoutUtility).GetField("current", BindingFlags.Static | BindingFlags.NonPublic);
				}

				return CURRENT_FIELDINFO.GetValue(null);
			}

			public static object TopLevel()
			{
				return LayoutCache.TopLevel(Current());
			}

			public static class LayoutCache
			{
				private static Type LAYOUT_CAHCE_TYPE;
				private static FieldInfo TOPLEVEL_FIELDINFO;

				static LayoutCache()
				{
					LAYOUT_CAHCE_TYPE = typeof(UnityEngine.GUILayoutUtility).Assembly.GetType("UnityEngine.GUILayoutUtility+LayoutCache");
				}

				public static object TopLevel(object layoutCache)
				{
					if (TOPLEVEL_FIELDINFO == null)
					{
						TOPLEVEL_FIELDINFO = LAYOUT_CAHCE_TYPE.GetField("topLevel", BindingFlags.Instance | BindingFlags.NonPublic);
					}

					return TOPLEVEL_FIELDINFO.GetValue(layoutCache);
				}
			}
		}

		public static class GUILayoutGroup
		{
			private static Type GUI_LAYOUT_GROUP_TYPE;
			private static MethodInfo GET_LAST_METHODINFO;

			static GUILayoutGroup()
			{
				GUI_LAYOUT_GROUP_TYPE = typeof(UnityEngine.GUILayoutUtility).Assembly.GetType("UnityEngine.GUILayoutGroup");
			}

			public static Rect GetLast(object guiLayoutGroup)
			{
				if (GET_LAST_METHODINFO == null)
				{
					GET_LAST_METHODINFO = GUI_LAYOUT_GROUP_TYPE.GetMethod("GetLast", BindingFlags.Instance | BindingFlags.Public);
				}

				return (Rect)GET_LAST_METHODINFO.Invoke(guiLayoutGroup, null);
			}
		}

		public static class ShaderKeyword
		{
			/// <summary>
			/// Keep in sync with kMaxShaderKeywords in ShaderKeywordSet.h
			/// vaild index [0, MAX_SHADER_KEYWORDS)
			/// </summary>
			public const int MAX_SHADER_KEYWORDS = 256;

			private static MethodInfo GET_SHADER_KEYWORD_NAME_METHODINFO;
			private static MethodInfo GET_SHADER_KEYWORD_TYPE_METHODINFO;
			private static FieldInfo KEYWORD_INDEX_FIELDINFO;

			static ShaderKeyword()
			{
				GET_SHADER_KEYWORD_NAME_METHODINFO = typeof(UnityEngine.Rendering.ShaderKeyword).GetMethod("GetShaderKeywordName", BindingFlags.Static | BindingFlags.NonPublic);
				GET_SHADER_KEYWORD_TYPE_METHODINFO = typeof(UnityEngine.Rendering.ShaderKeyword).GetMethod("GetShaderKeywordType", BindingFlags.Static | BindingFlags.NonPublic);
				KEYWORD_INDEX_FIELDINFO = typeof(UnityEngine.Rendering.ShaderKeyword).GetField("m_KeywordIndex", BindingFlags.Instance | BindingFlags.NonPublic);
			}

			public static string GetShaderKeywordName(int keywordIndex)
			{
				object result = GET_SHADER_KEYWORD_NAME_METHODINFO.Invoke(null, new object[] { keywordIndex });
				return result == null
					? null
					: (string)result;
			}

			public static ShaderKeywordType GetShaderKeywordType(int keywordIndex)
			{
				object result = GET_SHADER_KEYWORD_TYPE_METHODINFO.Invoke(null, new object[] { keywordIndex });
				return result == null
					? ShaderKeywordType.None
					: (ShaderKeywordType)result;
			}

			public static int GetShaderKeywordIndex(UnityEngine.Rendering.ShaderKeyword shaderKeyword)
			{
				return (int)KEYWORD_INDEX_FIELDINFO.GetValue(shaderKeyword);
			}

			public static void SetShaderKeywordIndex(UnityEngine.Rendering.ShaderKeyword shaderKeyword, int keywordIndex)
			{
				KEYWORD_INDEX_FIELDINFO.SetValue(shaderKeyword, keywordIndex);
			}
		}

		public static class Material
		{
			private static PropertyInfo RAW_RENDER_QUEUE_PROPERTYINFO;

			static Material()
			{
				RAW_RENDER_QUEUE_PROPERTYINFO = typeof(UnityEngine.Material).GetProperty("rawRenderQueue", BindingFlags.Instance | BindingFlags.NonPublic);
			}

			public static int RawRenderQueue(UnityEngine.Material material)
			{
				return (int)RAW_RENDER_QUEUE_PROPERTYINFO.GetValue(material);
			}
		}
	}
}