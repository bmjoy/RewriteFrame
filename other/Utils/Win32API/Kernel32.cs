#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
using System;
using System.Runtime.InteropServices;

namespace Win32API
{
	public static class Kernel32
	{
		public const int STD_INPUT_HANDLE = -10;
		public const int STD_OUTPUT_HANDLE = -11;

		private const string KERNEL32 = "kernel32.dll";

		[DllImport(KERNEL32)]
		public static extern uint GetCurrentThreadId();

		[DllImport(KERNEL32, SetLastError = true)]
		public static extern bool AttachConsole(int dwProcessId);

		[DllImport(KERNEL32, SetLastError = true)]
		public static extern bool AllocConsole();

		[DllImport(KERNEL32, SetLastError = true)]
		public static extern bool FreeConsole();

		[DllImport(KERNEL32, EntryPoint = "GetStdHandle", SetLastError = true, CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
		public static extern IntPtr GetStdHandle(StandardDevice standardDevice);

		[DllImport(KERNEL32, SetLastError = true)]
		public static extern bool GetConsoleScreenBufferInfo(IntPtr hConsoleOutput, out CONSOLE_SCREEN_BUFFER_INFO lpConsoleScreenBufferInfo);

		[DllImport(KERNEL32, SetLastError = true)]
		public static extern bool SetConsoleTextAttribute(IntPtr hConsoleOutput, short attributes);

		[DllImport(KERNEL32, SetLastError = true)]
		public static extern bool SetConsoleOutputCP(uint codePage);
	}
}
#endif