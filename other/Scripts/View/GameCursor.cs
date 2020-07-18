using Leyoutech.Core.Loader.Config;
using UnityEngine;

public class GameCursor : UIPanelBase
{
    private const string ASSET_ADDRESS = AssetAddressKey.PRELOADUI_GAMECURSOR;

    /// <summary>
    /// 根节点
    /// </summary>
    private RectTransform m_Content;
    /// <summary>
    /// 鼠标光标
    /// </summary>
    private Animator m_MouseCursor;
    /// <summary>
    /// 手柄光标
    /// </summary>
    private Animator m_GamepadCursor;

    /// <summary>
    /// 光标是否可见
    /// </summary>
    private bool m_CursorVisible = false;
    /// <summary>
    /// 是否为鼠标模式
    /// </summary>
    private bool m_IsMouseMode = false;

    public GameCursor() : base(UIPanel.GameCursor, ASSET_ADDRESS, PanelType.Cursor)
    {
        CanReceiveFocus = false;
    }

    public override void OnShow(object msg)
    {
        base.OnShow(msg);

        m_Content = FindComponent<RectTransform>("Content");
        m_MouseCursor = FindComponent<Animator>(m_Content, "MouseCursor");
        m_GamepadCursor = FindComponent<Animator>(m_Content, "JoystickCursor");

        InputManager.Instance.OnInputActionMapChanged += OnInputMapChanged;
        InputManager.Instance.OnInputDeviceChanged += OnInputDeviceChanged;
        InputManager.Instance.OnNavigateModeChanged += OnNavigateModeChanged;

        UpdateCursorStyle();

        StartUpdate();
    }

    public override void OnHide(object msg)
    {
        InputManager.Instance.OnInputActionMapChanged -= OnInputMapChanged;
        InputManager.Instance.OnInputDeviceChanged -= OnInputDeviceChanged;
        InputManager.Instance.OnNavigateModeChanged -= OnNavigateModeChanged;

        base.OnHide(msg);
    }

    /// <summary>
    /// 输入表改变时
    /// </summary>
    /// <param name="oldMap"></param>
    /// <param name="newMap"></param>
    private void OnInputMapChanged(HotKeyMapID newMap)
    {
        UpdateCursorStyle();
    }

    /// <summary>
    /// 输入设备改变时
    /// </summary>
    /// <param name="device"></param>
    private void OnInputDeviceChanged(InputManager.GameInputDevice device)
    {
        UpdateCursorStyle();
    }

    /// <summary>
    /// 导航模式改变时
    /// </summary>
    /// <param name="navigateMode"></param>
    private void OnNavigateModeChanged(bool navigateMode)
    {
        UpdateCursorStyle();
    }

    /// <summary>
    /// 更新光标样式
    /// </summary>
    /// <param name="input">InputActionMapID</param>
    /// <returns>bool</returns>
    private void UpdateCursorStyle()
    {
        bool cursorVisible = InputManager.Instance.CurrentInputMap == HotKeyMapID.Chat || (InputManager.Instance.CurrentInputMap == HotKeyMapID.UI && !InputManager.Instance.GetNavigateMode());
        bool isMouseMode = InputManager.Instance.CurrentInputDevice == InputManager.GameInputDevice.KeyboardAndMouse;

        if (cursorVisible != m_CursorVisible || isMouseMode != m_IsMouseMode)
        {
            m_CursorVisible = cursorVisible;
            m_IsMouseMode = isMouseMode;

            m_MouseCursor.gameObject.SetActive(m_CursorVisible && m_IsMouseMode);
            m_GamepadCursor.gameObject.SetActive(m_CursorVisible && !m_IsMouseMode);
        }
    }

    /// <summary>
    /// 每帧更新
    /// </summary>
    protected override void Update()
    {
        if (!m_CursorVisible)
            return;

        Vector2 cursorPosition = InputManager.Instance.GetCurrentVirtualCursorPos();

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(GetTransform().GetComponent<RectTransform>(), cursorPosition, Camera, out Vector2 localPosition))
        {
            m_Content.anchoredPosition = localPosition;

            bool hited = UIManager.Instance.HitTest(cursorPosition);
            if (m_IsMouseMode)
                m_MouseCursor.SetBool("Interactive", hited);
            else
                m_GamepadCursor.SetBool("Interactive", hited);
        }
    }
}