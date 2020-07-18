#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
namespace Win32API
{
	public enum SystemParametersInfoAction : uint
	{
		SPI_GETMOUSESPEED = 0x0070,
	}
}
#endif