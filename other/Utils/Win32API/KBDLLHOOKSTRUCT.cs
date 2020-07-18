#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
using System;
using System.Runtime.InteropServices;

namespace Win32API
{
	/// <summary>
	/// <see cref="https://docs.microsoft.com/en-us/windows/desktop/api/winuser/ns-winuser-tagkbdllhookstruct"/>
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	public struct KBDLLHOOKSTRUCT
	{
		public uint vkCode;
		public uint scanCode;
		public KBDllHookStructFlag flags;
		public uint time;
		public UIntPtr dwExtraInfo;

		public static KBDLLHOOKSTRUCT CreateFromPtr(IntPtr ptr)
		{
			return (KBDLLHOOKSTRUCT)Marshal.PtrToStructure(ptr, typeof(KBDLLHOOKSTRUCT));
		}
	}
}
#endif