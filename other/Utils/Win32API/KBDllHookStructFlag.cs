#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
using System;

namespace Win32API
{
	/// <summary>
	/// <see cref="https://docs.microsoft.com/en-us/windows/desktop/api/winuser/ns-winuser-tagkbdllhookstruct"/> flags
	/// </summary>
	[Flags]
	public enum KBDllHookStructFlag : uint
	{
		LLKHF_EXTENDED = 0x01,
		LLKHF_INJECTED = 0x10,
		LLKHF_ALTDOWN = 0x20,
		LLKHF_UP = 0x80,
	}
}
#endif