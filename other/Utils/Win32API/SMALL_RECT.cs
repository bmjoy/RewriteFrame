#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
using System.Runtime.InteropServices;

namespace Win32API
{
	[StructLayout(LayoutKind.Sequential)]
	public struct SMALL_RECT
	{
		public short Left;
		public short Top;
		public short Right;
		public short Bottom;
	}
}
#endif