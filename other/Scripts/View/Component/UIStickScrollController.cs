using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(ScrollRect))]
public class UIStickScrollController : MonoBehaviour
{
    public enum ScrollStick { LeftStick, RightStick };

    /// <summary>
    /// 相关的ScrollRect
    /// </summary>
    private ScrollRect m_Scroller;
    /// <summary>
    /// 相关的RectTransform
    /// </summary>
    private RectTransform m_ScrollerRecttransform;
    /// <summary>
    /// 相关的Canvas相机
    /// </summary>
    private Camera m_Camera;
    /// <summary>
    /// 是否已启用
    /// </summary>
    private bool m_Enabled = false;
    /// <summary>
    /// 是否有焦点
    /// </summary>
    private bool m_Focused = false;
    /// <summary>
    /// 上一次右摇杆的输入值
    /// </summary>
    private float m_RightStickValue = 0.0f;

    /// <summary>
    /// 右摇杆滚动灵敏度
    /// </summary>
    [SerializeField]
    public ScrollStick Stick = ScrollStick.LeftStick;
    /// <summary>
    /// 左摇杆滚动灵敏度
    /// </summary>
    [SerializeField]
    public float LeftStickScrollSensitivity = 0.2f;
    /// <summary>
    /// 右摇杆滚动灵敏度
    /// </summary>
    [SerializeField]
    public float RightStickScrollSensitivity = 15f;

    /// <summary>
    /// 设置焦点
    /// </summary>
    /// <param name="focused"></param>
    public void SetFocused(bool focused)
    {
        if (m_Focused != focused)
        {
            m_Focused = focused;
            ResetScrollMethod();
        }
    }

    private void OnEnable()
    {
        m_Scroller = GetComponent<ScrollRect>();
        m_ScrollerRecttransform = GetComponent<RectTransform>();
        //m_Camera = GetComponentInParent<Canvas>().worldCamera;

        m_Enabled = true;
        ResetScrollMethod();
    }

    private void OnDisable()
    {
        m_Scroller = null;
        m_ScrollerRecttransform = null;
        m_Camera = null;

        m_Enabled = false;
        ResetScrollMethod();
    }

    /// <summary>
    /// 重置滚动方法
    /// </summary>
    /// <param name="bind"></param>
    private void ResetScrollMethod()
    {
        if (InputManager.Instance == null)
            return;

        InputManager.Instance.OnInputDeviceChanged -= OnInputDeviceChanged;
        InputManager.Instance.OnNavigateModeChanged -= OnNavigateModeChanged;

        if (m_Focused && m_Enabled)
        {
            InputManager.Instance.OnInputDeviceChanged += OnInputDeviceChanged;
            InputManager.Instance.OnNavigateModeChanged += OnNavigateModeChanged;
        }

        RestScrollEvent();
    }

    /// <summary>
    /// 输入设备改变时
    /// </summary>
    /// <param name="device"></param>
    private void OnInputDeviceChanged(InputManager.GameInputDevice device)
    {
        RestScrollEvent();
    }

    /// <summary>
    /// 输入方式改变时
    /// </summary>
    /// <param name="isNavigate"></param>
    private void OnNavigateModeChanged(bool isNavigate)
    {
        RestScrollEvent();
    }

    /// <summary>
    /// 重置滚动相关事件处理
    /// </summary>
    private void RestScrollEvent()
    {
        if (HotkeyManager.Instance == null)
            return;
        if (InputManager.Instance == null)
            return;

        bool focused = m_Enabled && m_Focused;
        bool navigateMode = InputManager.Instance.GetNavigateMode();
        InputManager.GameInputDevice device = InputManager.Instance.CurrentInputDevice;

        string hotkeyID = "scroll_" + GetInstanceID();

        HotkeyManager.Instance.Unregister(hotkeyID);

        UIManager.Instance.OnUpdate -= UpdateScroll;

        if (focused && !navigateMode && device != InputManager.GameInputDevice.KeyboardAndMouse)
        {
            HotkeyManager.Instance.Register(hotkeyID, HotKeyID.UGUI_Stick2, UpdateScroll);

            if (Stick == ScrollStick.LeftStick || Stick == ScrollStick.RightStick)
                UIManager.Instance.OnUpdate += UpdateScroll;
        }
    }

    /// <summary>
    /// 响应右摇杆
    /// </summary>
    /// <param name="callback"></param>
    private void UpdateScroll(HotkeyCallback callback)
    {
        m_RightStickValue = callback.ReadValue<Vector2>().y;
    }

    /// <summary>
    /// 计算滚动量
    /// </summary>
    private void UpdateScroll()
    {
        if (!m_ScrollerRecttransform)
            return;

        if (m_Camera == null)
        {
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas)
                m_Camera = canvas.worldCamera;
            else
                return;
        }

        Vector2 point = Vector2.zero;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(m_ScrollerRecttransform, InputManager.Instance.GetCurrentVirtualCursorPos(), m_Camera, out point))
        {
            Rect rect = m_ScrollerRecttransform.rect;

            if (Stick == ScrollStick.LeftStick)
            {
                if (point.y < rect.yMin)
                {
                    float contentHeight = m_Scroller.content.rect.size.y;
                    float scrollOffset = Mathf.Abs(point.y - rect.yMin);

                    float scrollDelay = 0;
                    if (contentHeight > 0)
                        scrollDelay = LeftStickScrollSensitivity * scrollOffset / contentHeight * (Screen.height / 1080.0f);

                    m_Scroller.verticalNormalizedPosition = Mathf.Clamp01(m_Scroller.verticalNormalizedPosition - scrollDelay);
                }
                else if (point.y > rect.yMax)
                {
                    float contentHeight = m_Scroller.content.rect.size.y;
                    float scrollOffset = Mathf.Abs(point.y - rect.yMax);

                    float scrollDelay = 0;
                    if (contentHeight > 0)
                        scrollDelay = LeftStickScrollSensitivity * scrollOffset / contentHeight * (Screen.height / 1080.0f);

                    m_Scroller.verticalNormalizedPosition = Mathf.Clamp01(m_Scroller.verticalNormalizedPosition + scrollDelay);
                }
            }
            else if (Stick == ScrollStick.RightStick)
            {
                if (m_ScrollerRecttransform.rect.Contains(point))
                {
                    float contentHeight = m_Scroller.content.rect.size.y;

                    float scrollDelay = 0;
                    if (contentHeight > 0)
                        scrollDelay = RightStickScrollSensitivity * m_RightStickValue / contentHeight * (Screen.height / 1080.0f);

                    m_Scroller.verticalNormalizedPosition = Mathf.Clamp01(m_Scroller.verticalNormalizedPosition + scrollDelay);
                }
            }
        }
    }
}