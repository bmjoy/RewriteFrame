#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
using System;
using System.Runtime.InteropServices;

namespace Win32API
{
	public static class User32
	{
		private const string USER32 = "user32.dll";

		[DllImport(USER32)]
		public static extern IntPtr SetWindowsHookEx(HookType code, HookProc func, IntPtr hInstance, int threadID);

		[DllImport(USER32)]
		public static extern int UnhookWindowsHookEx(IntPtr hhook);

		[DllImport(USER32)]
		public static extern int CallNextHookEx(IntPtr hhook, int code, IntPtr wParam, IntPtr lParam);

		[DllImport(USER32)]
		public static extern bool SystemParametersInfo(SystemParametersInfoAction uiAction, uint uiParam, IntPtr pvParam, uint fWinIni);
	}
}
#endif