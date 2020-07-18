#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
namespace Win32API
{
	public enum StandardDevice : int
	{
		STD_INPUT_HANDLE = -10,
		STD_OUTPUT_HANDLE = -11,
		STD_ERROR_HANDLE = -12,
	}
}
#endif