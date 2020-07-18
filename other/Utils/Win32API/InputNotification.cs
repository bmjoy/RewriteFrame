#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
namespace Win32RawInput
{
	/// <summary>
	/// <see cref="https://docs.microsoft.com/en-us/windows/desktop/inputdev/keyboard-input-notifications"/>
	/// </summary>
	public enum InputNotification 
    {
        KeyDown = 0x0100,
        KeyUp = 0x0101,
        SysKeyDown = 0x0104,
        SysKeyUp = 0x0105
    }
}
#endif