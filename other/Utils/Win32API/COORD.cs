#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
using System.Runtime.InteropServices;

namespace Win32API
{
	[StructLayout(LayoutKind.Sequential)]
	public struct COORD
	{
		public short X;
		public short Y;
	}
}
#endif