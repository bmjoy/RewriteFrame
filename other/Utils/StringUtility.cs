using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Leyoutech.Utility
{
	public static class StringUtility
	{
		/// <summary>
		/// Cache下来避免每次使用时new一个
		/// 只能在主线程中使用
		/// 不能在协程中跨过yield return去使用，For Error Example：
		/// <code>
		/// StringBuilder stringBuilder = AllocStringBuilderCache();
		/// yield return null;
		/// stringBuilder.Append("");
		/// </code>
		/// 
		/// 使用方法：
		///		<see cref="AllocStringBuilderCache"/>
		///		<see cref="ReleaseStringBuilderCache"/>
		///		<see cref="ReleaseStringBuilderCacheAndReturnString"/>
		/// </summary>
		private static StringBuilder ms_StringBuilderCache;
		private static System.Security.Cryptography.MD5 ms_DefaultMd5;
		private static StringBuilder ms_HTML_StringBuilder;
		private static object ms_HTML_Lock = new object();

		static StringUtility()
		{
			ms_StringBuilderCache = new StringBuilder();
		}

		/// <summary>
		/// 例：
		///		E:\qwe\sdf\wts.gwe 返回wts
		/// </summary>
		/// <param name="path">完整路径</param>
		/// <returns>文件名(不包括扩展名)</returns>
		public static string SubFileNameFromPath(string path)
		{
			int index = path.LastIndexOfAny(new char[] { '\\', '/' });
			if (index > 0)
			{
				path = path.Substring(index + 1);
			}
			index = path.LastIndexOf('.');
			if (index > 0)
			{
				path = path.Substring(0, index);
			}
			return path;
		}

		/// <summary>
		/// <see cref="ms_StringBuilderCache"/>返回一个空的StringBuilder
		/// </summary>
		public static StringBuilder AllocStringBuilderCache()
		{
#if UNITY_EDITOR
			DebugUtility.Assert(ms_StringBuilderCache.Length == 0
				, "AllocStringBuilderCache时ms_StringBuilderCache不为空，是不是上次AllocStringBuilderCache后没调用ReleaseStringBuilderCache");
			ms_StringBuilderCache.Clear();
#endif
			return ms_StringBuilderCache;
		}

		/// <summary>
		/// 和<see cref="AllocStringBuilderCache"/>的区别是这个方法返回的StringBuilder不一定是空的
		/// </summary>
		public static StringBuilder GetStringBuilderCache()
		{
			return ms_StringBuilderCache;
		}

		/// <summary>
		/// <see cref="ms_StringBuilderCache"/>
		/// </summary>
		public static void ReleaseStringBuilderCache()
		{
			ms_StringBuilderCache.Clear();
		}

		/// <summary>
		/// <see cref="ms_StringBuilderCache"/>
		/// </summary>
		public static string ReleaseStringBuilderCacheAndReturnString()
		{
			string str = ms_StringBuilderCache.ToString();
			ms_StringBuilderCache.Clear();
			return str;
		}

		/// <summary>
		/// 计算一个字符串的MD5
		/// </summary>
		public static string CalculateMD5Hash(string input)
		{
			if (ms_DefaultMd5 == null)
			{
				ms_DefaultMd5 = System.Security.Cryptography.MD5.Create();
			}

			byte[] inputBytes = Encoding.ASCII.GetBytes(input);
			byte[] hashBytes = ms_DefaultMd5.ComputeHash(inputBytes);

			StringBuilder stringBuilder = AllocStringBuilderCache();
			for (int iByte = 0; iByte < hashBytes.Length; iByte++)
			{
				stringBuilder.Append(hashBytes[iByte].ToString("X2"));
			}
			return ReleaseStringBuilderCacheAndReturnString();
		}

		/// <summary>
		/// 把一个字符串转换为变量名
		/// </summary>
		public static string FormatToVariableName(string value, char replace = '_')
		{
			string variableName = string.Empty;
			for (int iChar = 0; iChar < value.Length; iChar++)
			{
				char iterChar = value[iChar];
				if (iterChar == '_'
					|| char.IsLetterOrDigit(iterChar))
				{
					variableName += iterChar;
				}
				else
				{
					variableName += replace;
				}
			}
			if (char.IsNumber(variableName[0]))
			{
				variableName = replace + variableName;
			}
			return variableName;
		}

		public static string ConvertToDex(byte[] buffer)
		{
			return ConvertToDex(buffer, 0, buffer.Length);
		}

		public static string ConvertToDex(byte[] buffer, int length)
		{
			return ConvertToDex(buffer, 0, length);
		}

		public static string ConvertToDex(byte[] buffer, int offset, int length)
		{
			StringBuilder stringBuilder = AllocStringBuilderCache();
			for (int iByte = offset; iByte < length; iByte++)
			{
				stringBuilder.AppendFormat(buffer[iByte].ToString("X2"));
			}
			return ReleaseStringBuilderCacheAndReturnString();
		}

		public static string Format(string format, Vector3 vec3)
		{
			return string.Format(format, vec3.x, vec3.y, vec3.z);
		}

		public static void SplitToFloatArray(List<float> result, string str, char separator)
		{
			string[] strs = str.Split(separator);
			for (int iStr = 0; iStr < strs.Length; iStr++)
			{
				if (float.TryParse(strs[iStr], out float value))
				{
					result.Add(value);
				}
			}
		}

		public static string FormatToFileName(string value, char sign = '_')
		{
			char[] chars = Path.GetInvalidFileNameChars();
			for (int iChar = 0; iChar < chars.Length; iChar++)
			{
				value = value.Replace(chars[iChar], sign);
			}
			return value;
		}

		#region HTML
		public static void HTML_SetStringBuilder(StringBuilder html)
		{
			lock (ms_HTML_Lock)
			{
				DebugUtility.Assert(ms_HTML_StringBuilder == null || html == null, "Utility", "ms_HTML_StringBuilder == null || html == null");
				ms_HTML_StringBuilder = html;
			}
		}

		public static void HTML_Line(string line = "<br>", string spanStyle = null)
		{
			ms_HTML_StringBuilder.Append("<div style=\"clear: both;\">");
			if (spanStyle != null)
			{
				ms_HTML_StringBuilder.Append($"<span style=\"{spanStyle}\">");
			}
			ms_HTML_StringBuilder.Append(line);
			if (spanStyle != null)
			{
				ms_HTML_StringBuilder.Append("</span>");
			}
			ms_HTML_StringBuilder.Append("</div>");
		}

		public static void HTML_BeginTable(int width
			, string tableStyle = "border-collapse: collapse; border: 1px solid rgb(102, 102, 102);")
		{
			ms_HTML_StringBuilder.Append($"<table style=\"{tableStyle}\" width=\"{width}\">").Append("<tbody>");
		}

		public static void HTML_BeginTr()
		{
			ms_HTML_StringBuilder.Append("<tr>");
		}

		public static void HTML_BeginTd(int width
			, string valign = "top"
			, string style = "border: 1px solid rgb(102, 102, 102); padding: 3px 8px;")
		{
			ms_HTML_StringBuilder.Append($"<td valign=\"{valign}\" style=\"{style}\" width=\"{width}\">");
		}

		public static void HTML_EngTd()
		{
			ms_HTML_StringBuilder.Append("</td>");
		}

		public static void HTML_Td(int width
			, string body
			, string spanStyle = null
			, string valign = "top"
			, string tdStyle = "border: 1px solid rgb(102, 102, 102); padding: 3px 8px;")
		{
			HTML_BeginTd(width, valign, tdStyle);
			HTML_Line(body, spanStyle);
			HTML_EngTd();
		}

		public static void HTML_EndTr()
		{
			ms_HTML_StringBuilder.Append("</tr>");
		}

		public static void HTML_EndTable()
		{
			ms_HTML_StringBuilder.Append("</tbody></table>");
		}
		#endregion

        public static string ConvertToDisplay(Vector3 vec3, string format = "F2")
        {
            return $"({vec3.x.ToString(format)}, {vec3.y.ToString(format)}, {vec3.z.ToString(format)})";
        }

		/// <summary>
		/// 解析属性，例：
		///		当attributeText = "ABC()"时：
		///			attributeName = "ABC"
		///			args = null
		///		当attributeText = "ABC(D,E)"时：
		///			attributeName = "ABC"
		///			args = string[2] {"D", "E"}
		/// </summary>
		public static bool TryParseAttribute(string attributeText, out string attributeName, out string[] args)
		{
			attributeName = string.Empty;
			args = null;

			Match match = Regex.Match(attributeText, @"(\w+)\s*\((.*)\)");
			if (!match.Success)
			{
				return false;
			}

			attributeName = match.Groups[1].Value;
			string argText = match.Groups[2].Value.Trim();

			if (!string.IsNullOrEmpty(argText))
			{
				args = argText.Split(',');

				for (var iArg = 0; iArg < args.Length; ++iArg)
				{
					args[iArg] = args[iArg].Trim();
				}
			}
			return true;
		}
	}
}