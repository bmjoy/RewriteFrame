using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(TMPro.TMP_InputField))]
public class UIXboxVirtualKeyboard : MonoBehaviour, IPointerUpHandler, ISelectHandler, IDeselectHandler
{
    /// <summary>
    /// 是否处于选中状态
    /// </summary>
    private bool m_Selected;

    /// <summary>
    /// 输入字段
    /// </summary>
    private TMPro.TMP_InputField m_InputField;

    /// <summary>
    /// 虚拟键盘指针
    /// </summary>
    private System.IntPtr m_VirtualKeyboardPtr = System.IntPtr.Zero;

    /// <summary>
    /// 虚拟键盘标题
    /// </summary>
    public string VirtualKeyboardTitle = string.Empty;
    /// <summary>
    /// 虚拟键盘描述
    /// </summary>
    public string VirtualKeyboardDescription = string.Empty;


    private void Awake()
    {
        m_InputField = GetComponent<TMPro.TMP_InputField>();
    }

    /// <summary>
    /// 选中时
    /// </summary>
    /// <param name="eventData">事件数据</param>
    public void OnSelect(BaseEventData eventData)
    {
#if UNITY_EDITOR
#elif UNITY_XBOXONE
        m_Selected = true;

        TextSystems.TextSystemsManager.OnVirtualKeyboardInput -= OnVirtualKeyboardInput;

        if (m_VirtualKeyboardPtr != System.IntPtr.Zero)
            TextSystems.TextSystemsManager.CancelVirtualKeyboard(m_VirtualKeyboardPtr);

        TextSystems.TextSystemsManager.OnVirtualKeyboardInput += OnVirtualKeyboardInput;

        m_VirtualKeyboardPtr = TextSystems.TextSystemsManager.VirtualKeyboardGetTextAsync(m_InputField.text, VirtualKeyboardTitle, VirtualKeyboardDescription, 1);
#endif
    }

    /// <summary>
    /// 取消选中时
    /// </summary>
    /// <param name="eventData">事件数据</param>
    public void OnDeselect(BaseEventData eventData)
    {
#if UNITY_EDITOR
#elif UNITY_XBOXONE
        m_Selected = false;

        TextSystems.TextSystemsManager.OnVirtualKeyboardInput -= OnVirtualKeyboardInput;

        if (m_VirtualKeyboardPtr != System.IntPtr.Zero)
        {
            TextSystems.TextSystemsManager.CancelVirtualKeyboard(m_VirtualKeyboardPtr);

            m_VirtualKeyboardPtr = System.IntPtr.Zero;
        }
#endif
    }

    /// <summary>
    /// 点击时
    /// </summary>
    /// <param name="eventData">事件数据</param>
    public void OnPointerUp(PointerEventData eventData)
    {
#if UNITY_EDITOR
#elif UNITY_XBOXONE
        if (!m_Selected)
            return;

        if (m_VirtualKeyboardPtr != System.IntPtr.Zero)
            return;

        TextSystems.TextSystemsManager.OnVirtualKeyboardInput -= OnVirtualKeyboardInput;

        if (m_VirtualKeyboardPtr != System.IntPtr.Zero)
            TextSystems.TextSystemsManager.CancelVirtualKeyboard(m_VirtualKeyboardPtr);

        TextSystems.TextSystemsManager.OnVirtualKeyboardInput += OnVirtualKeyboardInput;

        m_VirtualKeyboardPtr = TextSystems.TextSystemsManager.VirtualKeyboardGetTextAsync(m_InputField.text, VirtualKeyboardTitle, VirtualKeyboardDescription, 1);
#endif
    }


#if UNITY_EDITOR
#elif UNITY_XBOXONE
    /// <summary>
    /// 收到Xbox虚拟键盘输入时
    /// </summary>
    /// <param name="result">状态</param>
    /// <param name="resultCode">状态码</param>
    /// <param name="resultData">返回数据</param>
    private void OnVirtualKeyboardInput(UnityPlugin.AsyncStatus result, int resultCode, string resultData)
    {
        if (result == UnityPlugin.AsyncStatus.Completed)
            m_InputField.text = resultData;

        TextSystems.TextSystemsManager.OnVirtualKeyboardInput -= OnVirtualKeyboardInput;

        m_VirtualKeyboardPtr = System.IntPtr.Zero;
    }
#endif

}
