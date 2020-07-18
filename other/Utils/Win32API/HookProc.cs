#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
using System;

namespace Win32API
{
	public delegate int HookProc(int code, IntPtr wParam, IntPtr lParam);
}
#endif