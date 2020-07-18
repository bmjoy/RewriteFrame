#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Profiling;

namespace Leyoutech.Utility
{
	public static class DebugUtility
	{
		public const string LOG_VERBOSE_EDITOR_CONDITIONAL = "LVE_E";
		public const string LOG_EDITOR_CONDITIONAL = "LLE_E";
		public const string LOG_WARNING_EDITOR_CONDITIONAL = "LWE_E";
		public const string LOG_ERROR_EDITOR_CONDITIONAL = "LEE_E";
		public const string LOG_ASSERT_EDITOR_CONDITIONAL = "LAE_E";

		public const string LOG_VERBOSE_BUILT_CONDITIONAL = "LVB_E";
		public const string LOG_BUILT_CONDITIONAL = "LLB_E";
		public const string LOG_WARNING_BUILT_CONDITIONAL = "LWB_E";
		public const string LOG_ERROR_BUILT_CONDITIONAL = "LEB_E";
		public const string LOG_ASSERT_BUILT_CONDITIONAL = "LAB_E";

#if UNITY_EDITOR
		public const string LOG_VERBOSE_CONDITIONAL = LOG_VERBOSE_EDITOR_CONDITIONAL;
		public const string LOG_CONDITIONAL = LOG_EDITOR_CONDITIONAL;
		public const string LOG_WARNING_CONDITIONAL = LOG_WARNING_EDITOR_CONDITIONAL;
		public const string LOG_ERROR_CONDITIONAL = LOG_ERROR_EDITOR_CONDITIONAL;
		public const string LOG_ASSERT_CONDITIONAL = LOG_ASSERT_EDITOR_CONDITIONAL;
#else
		public const string LOG_VERBOSE_CONDITIONAL = LOG_VERBOSE_BUILT_CONDITIONAL;
		public const string LOG_CONDITIONAL = LOG_BUILT_CONDITIONAL;
		public const string LOG_WARNING_CONDITIONAL = LOG_WARNING_BUILT_CONDITIONAL;
		public const string LOG_ERROR_CONDITIONAL = LOG_ERROR_BUILT_CONDITIONAL;
		public const string LOG_ASSERT_CONDITIONAL = LOG_ASSERT_BUILT_CONDITIONAL;
#endif

		private const string DEFAULT_TAG = "Default";
		/// <summary>
		/// Log的分隔符
		/// {LOG_SPLIT1}Tag{LOG_SPLIT2}Text
		/// </summary>
		private const string LOG_SPLIT1 = "🐷";
		/// <summary>
		/// <see cref="LOG_SPLIT1"/>
		/// </summary>
		private const string LOG_SPLIT2 = " | ";

		private const double BYTES_TO_MBYTES = 1.0 / 1024.0 / 1024.0;

		/// <summary>
		/// 相当于<see cref="Time.realtimeSinceStartup"/>
		/// 但是编辑器下也能使用
		/// </summary>
		private static System.Diagnostics.Stopwatch ms_Stopwatch;
		/// <summary>
		/// 只能在这个类中使用
		/// </summary>
		private static System.Text.StringBuilder ms_StringBuilderCache;

		static DebugUtility()
		{
			ms_Stopwatch = new System.Diagnostics.Stopwatch();
			ms_Stopwatch.Start();
			ms_StringBuilderCache = new System.Text.StringBuilder();
		}

		#region Time
		/// <summary>
		/// <see cref="Time.realtimeSinceStartup"/>
		/// </summary>
		public static long GetMillisecondsSinceStartup()
		{
			return ms_Stopwatch.ElapsedMilliseconds;
		}

		/// <summary>
		/// <see cref="Time.realtimeSinceStartup"/>
		/// </summary>
		public static long GetTicksSinceStartup()
		{
			return ms_Stopwatch.ElapsedTicks;
		}

		/// <summary>
		/// Tick to FPS
		/// </summary>
		/// <param name="ticks"></param>
		/// <returns></returns>
		public static float ConvertTicksToFPS(long ticks)
		{
			return 10000000.0f / ticks;
		}

		/// <summary>
		/// Convert to *h *m *s
		/// </summary>
		/// <param name="milliseconds"></param>
		/// <returns></returns>
		public static string FormatMilliseconds(long milliseconds)
		{
			long totalS = (long)(milliseconds / 1000.0);
			long h = (long)(totalS / 3600.0);
			long m = (long)((totalS - h * 3600) / 60.0);
			long s = totalS - h * 3600 - m * 60;
			return $"({h}h {m}m {s}s)";
		}
		#endregion

		#region Log
		public static string FormatPathToHyperLink(string path)
		{
			return $"<a path=\"{path}\">{path}</a>";
		}

		public static string FormatDirectoryToHyperLink(string directory)
		{
			return $"<a directory=\"{directory}\">{directory}</a>";
		}

		public static string FormatLog(string tag, string message)
		{
			return string.Format("{0}{1}{2}{3}", LOG_SPLIT1, tag, LOG_SPLIT2, message);
		}

		public static void ParserLog(string log, out string tag, out string message)
		{
			if (log.StartsWith(LOG_SPLIT1))
			{
				int split2Index = log.IndexOf(LOG_SPLIT2);
				if (split2Index > 0)
				{
					tag = log.Substring(LOG_SPLIT1.Length, split2Index - LOG_SPLIT1.Length);
					message = log.Substring(split2Index + 1);
				}
				else
				{
					tag = DEFAULT_TAG;
					message = log;
				}
			}
			else
			{
				tag = DEFAULT_TAG;
				message = log;
			}
		}

		[System.Diagnostics.Conditional(LOG_VERBOSE_CONDITIONAL)]
		public static void LogVerbose(string tag, string message)
		{
			string text = FormatLog(tag, message);
			Debug.Log(text);
			LogToSystemConsole(LogType.Log, text);
		}

		[System.Diagnostics.Conditional(LOG_VERBOSE_CONDITIONAL)]
		public static void LogVerbose(string tag, string message, UnityEngine.Object context)
		{
			string text = FormatLog(tag, message);
			Debug.Log(text, context);
			LogToSystemConsole(LogType.Log, text);
		}

		[System.Diagnostics.Conditional(LOG_CONDITIONAL)]
		public static void Log(string tag, string message)
		{
			string text = FormatLog(tag, message);
			Debug.Log(text);
			LogToSystemConsole(LogType.Log, text);
		}

		[System.Diagnostics.Conditional(LOG_CONDITIONAL)]
		public static void Log(string tag, string message, UnityEngine.Object context)
		{
			string text = FormatLog(tag, message);
			Debug.Log(text, context);
			LogToSystemConsole(LogType.Log, text);
		}

		[System.Diagnostics.Conditional(LOG_CONDITIONAL)]
		public static void LogFormat(string tag, string message, params object[] args)
		{
			message = string.Format(message, args);
			string text = FormatLog(tag, message);
			Debug.Log(text);
			LogToSystemConsole(LogType.Log, text);
		}

		[System.Diagnostics.Conditional(LOG_WARNING_CONDITIONAL)]
		public static void LogWarning(string tag, string message)
		{
			string text = FormatLog(tag, message);
			Debug.LogWarning(text);
			LogToSystemConsole(LogType.Warning, text);
		}

		[System.Diagnostics.Conditional(LOG_WARNING_CONDITIONAL)]
		public static void LogWarning(string tag, string message, UnityEngine.Object context)
		{
			string text = FormatLog(tag, message);
			Debug.LogWarning(text, context);
			LogToSystemConsole(LogType.Warning, text);
		}

		[System.Diagnostics.Conditional(LOG_ERROR_CONDITIONAL)]
		public static void LogError(string tag, string message)
		{
			string text = FormatLog(tag, message);
			Debug.LogError(text);
			LogToSystemConsole(LogType.Error, text);
		}

		[System.Diagnostics.Conditional(LOG_ERROR_CONDITIONAL)]
		public static void LogError(string tag, string message, UnityEngine.Object context)
		{
			string text = FormatLog(tag, message);
			Debug.LogError(text, context);
			LogToSystemConsole(LogType.Error, text);
		}

		[System.Diagnostics.Conditional(LOG_CONDITIONAL)]
		public static void LogErrorFormat(string tag, string message, params object[] args)
		{
			message = string.Format(message, args);
			string text = FormatLog(tag, message);
			Debug.LogError(text);
			LogToSystemConsole(LogType.Error, text);
		}

		/// <summary>
		/// <see cref="Assert"/>和<see cref="AssertMust"/>的区别是
		/// <see cref="Assert"/> 会在正式打包时被编译器忽略
		/// <see cref="AssertMust"/> 永远都会生效
		/// </summary>
		[System.Diagnostics.Conditional(LOG_ASSERT_CONDITIONAL)]
		public static void Assert(bool condition, string message, bool displayDialog = true)
		{
			AssertMust(condition, message, displayDialog);
		}

		public static bool AssertMust(bool condition, string message, bool displayDialog = true)
		{
			if (!condition)
			{
				Debug.Assert(condition, message);
				LogToSystemConsole(LogType.Assert, message);
#if UNITY_EDITOR
				if (displayDialog)
				{
					EditorUtility.DisplayDialog("Assert Failed", message, "OK");
				}
				Debug.Break();
#endif
			}
			return condition;
		}

		/// <summary>
		/// 不一定执行，如果一定要执行，请用<see cref="AssertMust"/>
		/// </summary>
		[System.Diagnostics.Conditional(LOG_ASSERT_CONDITIONAL)]
		public static void Assert(bool condition, string message, UnityEngine.Object context, bool displayDialog = true)
		{
			AssertMust(condition, message, context, displayDialog);
		}

		public static bool AssertMust(bool condition, string message, UnityEngine.Object context, bool displayDialog = true)
		{
			if (!condition)
			{
				Debug.Assert(condition, message, context);
				LogToSystemConsole(LogType.Assert, message);
#if UNITY_EDITOR
				if (displayDialog)
				{
					EditorUtility.DisplayDialog("Assert Failed", message, "OK");
				}
				Debug.Break();
#endif
			}
			return condition;
		}

		/// <summary>
		/// 不一定执行，如果一定要执行，请用<see cref="AssertMust"/>
		/// </summary>
		[System.Diagnostics.Conditional(LOG_ASSERT_CONDITIONAL)]
		public static void Assert(bool condition, string tag, string message, bool displayDialog = true)
		{
			if (condition)
			{
				return;
			}

			Assert(condition, FormatLog(tag, message), displayDialog);
		}

		public static bool AssertMust(bool condition, string tag, string message, bool displayDialog = true)
		{
			if (condition)
			{
				return true;
			}

			return AssertMust(condition, FormatLog(tag, message), displayDialog);
		}

		/// <summary>
		/// 不一定执行，如果一定要执行，请用<see cref="AssertMust"/>
		/// </summary>
		[System.Diagnostics.Conditional(LOG_ASSERT_CONDITIONAL)]
		public static void Assert(bool condition, string tag, string message, UnityEngine.Object context, bool displayDialog = true)
		{
			if (condition)
			{
				return;
			}

			Assert(condition, FormatLog(tag, message), context, displayDialog);
		}

		public static bool AssertMust(bool condition, string tag, string message, UnityEngine.Object context, bool displayDialog = true)
		{
			if (condition)
			{
				return true;
			}

			return AssertMust(condition, FormatLog(tag, message), context, displayDialog);
		}

		/// <summary>
		/// 生成内存Log
		/// </summary>
		/// <param name="forceCollection">true:强制收集，可能会导致卡顿</param>
		public static string GenerateMemoryLog(string message, bool forceCollection)
		{
			return ms_StringBuilderCache.Clear()
				.Append($"TotalMemory: {System.GC.GetTotalMemory(forceCollection) * BYTES_TO_MBYTES:F2}MB, ")
				.Append($"MaxUsed: {Profiler.maxUsedMemory * BYTES_TO_MBYTES:F2}MB, ")
				.Append($"Graphics: {Profiler.GetAllocatedMemoryForGraphicsDriver() * BYTES_TO_MBYTES:F2}MB, ")
				.Append($"MonoHeap: {Profiler.GetMonoHeapSizeLong() * BYTES_TO_MBYTES:F2}MB, ")
				.Append($"MonoUsed: {Profiler.GetMonoUsedSizeLong() * BYTES_TO_MBYTES:F2}MB, ")
				.Append($"TempAlloc: {Profiler.GetTempAllocatorSize() * BYTES_TO_MBYTES:F2}MB, ")
				.Append($"TotalAlloc: {Profiler.GetTotalAllocatedMemoryLong() * BYTES_TO_MBYTES:F2}MB, ")
				.Append($"TotalUnusedReserved: {Profiler.GetTotalUnusedReservedMemoryLong() * BYTES_TO_MBYTES:F2}MB.")
				.Append(" ").Append(message)
				.ToString();
		}

		/// <param name="forceCollection">true:强制收集，可能会导致卡顿</param>
		public static void LogMemory(string tag, string message, bool forceCollection)
		{
			Log(tag, GenerateMemoryLog(message, forceCollection));
		}

		/// <param name="forceCollection">true:强制收集，可能会导致卡顿</param>
		public static void LogVerboseMemory(string tag, string message, bool forceCollection)
		{
			LogVerbose(tag, GenerateMemoryLog(message, forceCollection));
		}

		/// <summary>
		/// <see cref="s_EnableLogToSystemConsole"/>
		/// 只在编辑器下生效
		/// </summary>
		[System.Diagnostics.Conditional("UNITY_EDITOR_WIN"), System.Diagnostics.Conditional("UNITY_STANDALONE_WIN")]
		public static void LogToSystemConsole(LogType logType, string text)
		{
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
			LogPro.OutputHelper.GetInstance().WriteLog(logType, text);
#endif
		}
		#endregion
	}
}