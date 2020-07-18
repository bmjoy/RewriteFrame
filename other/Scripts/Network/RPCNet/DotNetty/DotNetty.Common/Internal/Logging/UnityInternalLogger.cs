using Leyoutech.Utility;
using System;
using System.Collections.Generic;

namespace DotNetty.Common.Internal.Logging
{
	public class UnityInternalLogger : IInternalLogger
	{
		private static UnityInternalLogger ms_Instance;

		public static UnityInternalLogger GetInstance(string name)
		{
			if (ms_Instance == null)
			{
				ms_Instance = new UnityInternalLogger("DNet");
			}
			return ms_Instance;
		}

		string IInternalLogger.Name { get { return m_Name; } }
		bool IInternalLogger.TraceEnabled { get { return m_TraceEnabled; } }
		bool IInternalLogger.DebugEnabled { get { return m_DebugEnabled; } }
		bool IInternalLogger.InfoEnabled { get { return m_InfoEnabled; } }
		bool IInternalLogger.WarnEnabled { get { return m_WarnEnabled; } }
		bool IInternalLogger.ErrorEnabled { get { return m_ErrorEnabled; } }

		private string m_Name;
		private bool m_TraceEnabled;
		private bool m_DebugEnabled;
		private bool m_InfoEnabled;
		private bool m_WarnEnabled;
		private bool m_ErrorEnabled;

		public UnityInternalLogger(string name)
		{
			m_Name = name;

			m_TraceEnabled = false;
			m_DebugEnabled = false;
			m_InfoEnabled = false;
			m_WarnEnabled = false;
			m_ErrorEnabled = false;

			EnableVerboseLog();
			EnableLog();
			EnableWarningLog();
			EnableErrorLog();
		}

		public bool IsEnabled(InternalLogLevel level)
		{
			switch (level)
			{
				case InternalLogLevel.DEBUG:
					return m_DebugEnabled;
				case InternalLogLevel.TRACE:
					return m_TraceEnabled;
				case InternalLogLevel.INFO:
					return m_InfoEnabled;
				case InternalLogLevel.WARN:
					return m_WarnEnabled;
				case InternalLogLevel.ERROR:
					return m_ErrorEnabled;
				default:
					return false;
			}
		}

		public void Log(InternalLogLevel level, string msg)
		{
			switch (level)
			{
				case InternalLogLevel.TRACE:
				case InternalLogLevel.DEBUG:
					DebugUtility.LogVerbose(m_Name, msg);
					break;
				case InternalLogLevel.INFO:
					DebugUtility.Log(m_Name, msg);
					break;
				case InternalLogLevel.WARN:
					DebugUtility.LogWarning(m_Name, msg);
					break;
				case InternalLogLevel.ERROR:
					DebugUtility.LogError(m_Name, msg);
					break;
			}
		}

		public void Log(InternalLogLevel level, string format, object arg)
		{
			switch (level)
			{
				case InternalLogLevel.TRACE:
				case InternalLogLevel.DEBUG:
					DebugUtility.LogVerbose(m_Name, MyStringFormat(format, arg));
					break;
				case InternalLogLevel.INFO:
					DebugUtility.Log(m_Name, MyStringFormat(format, arg));
					break;
				case InternalLogLevel.WARN:
					DebugUtility.LogWarning(m_Name, MyStringFormat(format, arg));
					break;
				case InternalLogLevel.ERROR:
					DebugUtility.LogError(m_Name, MyStringFormat(format, arg));
					break;
			}
		}

		public void Log(InternalLogLevel level, string format, object argA, object argB)
		{
			switch (level)
			{
				case InternalLogLevel.TRACE:
				case InternalLogLevel.DEBUG:
					DebugUtility.LogVerbose(m_Name, MyStringFormat(format, argA, argB));
					break;
				case InternalLogLevel.INFO:
					DebugUtility.Log(m_Name, MyStringFormat(format, argA, argB));
					break;
				case InternalLogLevel.WARN:
					DebugUtility.LogWarning(m_Name, MyStringFormat(format, argA, argB));
					break;
				case InternalLogLevel.ERROR:
					DebugUtility.LogError(m_Name, MyStringFormat(format, argA, argB));
					break;
			}
		}

		public void Log(InternalLogLevel level, string format, params object[] arguments)
		{
			switch (level)
			{
				case InternalLogLevel.TRACE:
				case InternalLogLevel.DEBUG:
					DebugUtility.LogVerbose(m_Name, MyStringFormat(format, arguments));
					break;
				case InternalLogLevel.INFO:
					DebugUtility.Log(m_Name, MyStringFormat(format, arguments));
					break;
				case InternalLogLevel.WARN:
					DebugUtility.LogWarning(m_Name, MyStringFormat(format, arguments));
					break;
				case InternalLogLevel.ERROR:
					DebugUtility.LogError(m_Name, MyStringFormat(format, arguments));
					break;
			}
		}

		public void Log(InternalLogLevel level, string msg, Exception t)
		{
			switch (level)
			{
				case InternalLogLevel.TRACE:
				case InternalLogLevel.DEBUG:
					DebugUtility.LogVerbose(m_Name, $"{msg}\n{t.ToString()}");
					break;
				case InternalLogLevel.INFO:
					DebugUtility.Log(m_Name, $"{msg}\n{t.ToString()}");
					break;
				case InternalLogLevel.WARN:
					DebugUtility.LogWarning(m_Name, $"{msg}\n{t.ToString()}");
					break;
				case InternalLogLevel.ERROR:
					DebugUtility.LogError(m_Name, $"{msg}\n{t.ToString()}");
					break;
			}
		}

		public void Log(InternalLogLevel level, Exception t)
		{
			switch (level)
			{
				case InternalLogLevel.TRACE:
				case InternalLogLevel.DEBUG:
					DebugUtility.LogVerbose(m_Name, t.ToString());
					break;
				case InternalLogLevel.INFO:
					DebugUtility.Log(m_Name, t.ToString());
					break;
				case InternalLogLevel.WARN:
					DebugUtility.LogWarning(m_Name, t.ToString());
					break;
				case InternalLogLevel.ERROR:
					DebugUtility.LogError(m_Name, t.ToString());
					break;
			}
		}

		public void Trace(string msg)
		{
			DebugUtility.LogVerbose(m_Name, msg);
		}

		public void Trace(string format, object arg)
		{
			DebugUtility.LogVerbose(m_Name, MyStringFormat(format, arg));
		}

		public void Trace(string format, object argA, object argB)
		{
			DebugUtility.LogVerbose(m_Name, MyStringFormat(format, argA, argB));
		}

		public void Trace(string format, params object[] arguments)
		{
			DebugUtility.LogVerbose(m_Name, MyStringFormat(format, arguments));
		}

		public void Trace(string msg, Exception t)
		{
			DebugUtility.LogVerbose(m_Name, $"{msg}\n{t.ToString()}");
		}

		public void Trace(Exception t)
		{
			DebugUtility.LogVerbose(m_Name, t.ToString());
		}

		public void Debug(string msg)
		{
			DebugUtility.LogVerbose(m_Name, msg);
		}

		public void Debug(string format, object arg)
		{
			DebugUtility.LogVerbose(m_Name, MyStringFormat(format, arg));
		}

		public void Debug(string format, object argA, object argB)
		{
			DebugUtility.LogVerbose(m_Name, MyStringFormat(format, argA, argB));
		}

		public void Debug(string format, params object[] arguments)
		{
			DebugUtility.LogVerbose(m_Name, MyStringFormat(format, arguments));
		}

		public void Debug(string msg, Exception t)
		{
			DebugUtility.LogVerbose(m_Name, $"{msg}\n{t.ToString()}");
		}

		public void Debug(Exception t)
		{
			DebugUtility.LogVerbose(m_Name, t.ToString());
		}

		public void Info(string msg)
		{
			DebugUtility.Log(m_Name, msg);
		}

		public void Info(string format, object arg)
		{
			DebugUtility.Log(m_Name, MyStringFormat(format, arg));
		}

		public void Info(string format, object argA, object argB)
		{
			DebugUtility.Log(m_Name, MyStringFormat(format, argA, argB));
		}

		public void Info(string format, params object[] arguments)
		{
			DebugUtility.Log(m_Name, MyStringFormat(format, arguments));
		}

		public void Info(string msg, Exception t)
		{
			DebugUtility.Log(m_Name, $"{msg}\n{t.ToString()}");
		}

		public void Info(Exception t)
		{
			DebugUtility.Log(m_Name, t.ToString());
		}

		public void Warn(string msg)
		{
			DebugUtility.LogWarning(m_Name, msg);
		}

		public void Warn(string format, object arg)
		{
			DebugUtility.LogWarning(m_Name, MyStringFormat(format, arg));
		}

		public void Warn(string format, object argA, object argB)
		{
			DebugUtility.LogWarning(m_Name, MyStringFormat(format, argA, argB));
		}

		public void Warn(string format, params object[] arguments)
		{
			DebugUtility.LogWarning(m_Name, MyStringFormat(format, arguments));
		}

		public void Warn(string msg, Exception t)
		{
			DebugUtility.LogWarning(m_Name, $"{msg}\n{t.ToString()}");
		}

		public void Warn(Exception t)
		{
			DebugUtility.LogWarning(m_Name, t.ToString());
		}

		public void Error(string msg)
		{
			DebugUtility.LogError(m_Name, msg);
		}

		public void Error(string format, object arg)
		{
			DebugUtility.LogError(m_Name, MyStringFormat(format, arg));
		}

		public void Error(string format, object argA, object argB)
		{
			DebugUtility.LogError(m_Name, MyStringFormat(format, argA, argB));
		}

		public void Error(string format, params object[] arguments)
		{
			DebugUtility.LogError(m_Name, MyStringFormat(format, arguments));
		}

		public void Error(string msg, Exception t)
		{
			DebugUtility.LogError(m_Name, $"{msg}\n{t.ToString()}");
		}

		public void Error(Exception t)
		{
			DebugUtility.LogError(m_Name, t.ToString());
		}

		[System.Diagnostics.Conditional(DebugUtility.LOG_VERBOSE_CONDITIONAL)]
		private void EnableVerboseLog()
		{
			m_TraceEnabled = true;
			m_DebugEnabled = true;
		}

		[System.Diagnostics.Conditional(DebugUtility.LOG_CONDITIONAL)]
		private void EnableLog()
		{
			m_InfoEnabled = true;
		}

		[System.Diagnostics.Conditional(DebugUtility.LOG_WARNING_CONDITIONAL)]
		private void EnableWarningLog()
		{
			m_WarnEnabled = true;
		}

		[System.Diagnostics.Conditional(DebugUtility.LOG_ERROR_CONDITIONAL)]
		private void EnableErrorLog()
		{
			m_ErrorEnabled = true;
		}

		private string MyStringFormat(string format, object arg)
		{
			int index = format.IndexOf("{}");
			return index >= 0
				? format = format.Remove(index, 2).Insert(index, arg.ToString())
				: $"format ({arg})";
		}

		private string MyStringFormat(string format, object argA, object argB)
		{
			return MyStringFormat(MyStringFormat(format, argA), argB);
		}

		private string MyStringFormat(string format, params object[] arguments)
		{
			for (int iArgument = 0; iArgument < arguments.Length; iArgument++)
			{
				format = MyStringFormat(format, arguments[iArgument]);
			}
			return format;
		}
	}
}
