using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIEscSecondPart : BaseViewPart
{

    private const string ASSET_ADDRESS = Leyoutech.Core.Loader.Config.AssetAddressKey.PRELOADUIELEMENT_ESCSECONDELEMENT_CONTENT;
    /// <summary>
    /// 列表资源
    /// </summary>
    public Toggle[] m_Toggles;
    /// <summary>
    /// 根节点
    /// </summary>
    private Transform m_Root;
    /// <summary>
    /// 进入退出动画
    /// </summary>
    private UIAnimationEvent m_AnimatorController;
    /// <summary>
    /// 手表转圈动画
    /// </summary>
    public UIAnimationEvent m_WatchController;
    /// <summary>
    /// 当前选中索引
    /// </summary>
    private int m_Index;
    /// <summary>
    /// 选项信息集合
    /// </summary>
    private List<EscChooseInfo> m_ChooseInfos;
    /// <summary>
    ///当前界面名称
    /// </summary>
    private UIPanel m_PanelName = UIPanel.None;
    /// <summary>
    /// 事件触发器
    /// </summary>
    private EventTrigger m_Trigger;
    /// <summary>
    /// 指针是否在上面
    /// </summary>
    private bool m_IsPointerOver;

    public override void OnShow(object msg)
    {
        base.OnShow(msg);
        if (msg != null)
        {
            m_Index = (int)msg;
        }
        LoadViewPart(ASSET_ADDRESS, OwnerView.OtherBox);
        State.GetAction(UIAction.Common_Confirm).Callback += OpenNextPanel;
        State.GetAction(UIAction.Common_Back).Callback += ClosePanel;
    }

    protected override void OnViewPartLoaded()
    {
        m_Root = FindComponent<Transform>("Content");
        m_AnimatorController = m_Root.GetComponent<UIAnimationEvent>();
        m_Toggles = m_Root.GetComponentsInChildren<Toggle>();
        m_WatchController = GetTransform().GetComponent<UIAnimationEvent>();

        m_Trigger = FindComponent<EventTrigger>("FocousRange");
        m_Trigger.triggers.Clear();
        AddTrigger(m_Trigger, EventTriggerType.PointerEnter, OnFocusRangeEnter);
        AddTrigger(m_Trigger, EventTriggerType.PointerExit, OnFocusRangeExit);

        m_ChooseInfos = GetEscChooseInfos();
        m_PanelName = GetPanelName();
        for (int i = 0; i < m_Toggles.Length; i++)
        {
            Toggle toggle = m_Toggles[i];
            TMP_Text label = toggle.transform.Find("Content/Content/Label_Name").GetComponent<TMP_Text>();
            label.text = GetLocalization(m_ChooseInfos[i].Name);
            Image icon = toggle.transform.Find("Content/Content/Image_Icon").GetComponent<Image>();
            UIUtil.SetIconImage(icon, m_ChooseInfos[i].IconId);
            toggle.group = m_Root.GetComponent<ToggleGroup>();
            int index = i;
            toggle.onValueChanged.RemoveAllListeners();
            toggle.onValueChanged.AddListener((select) =>
            {                
                ToggleOnClick(select, toggle, index);
            });
            UIEventListener.UIEventListener.AttachListener(m_Toggles[index].gameObject).onClick += OnClick;
        }
        State.OnPageIndexChanged -= OnPageChanged;
        State.OnPageIndexChanged += OnPageChanged;
        State.SetPageIndex(m_Index);
        OnPageChanged(State.GetPageIndex(), State.GetPageIndex());
        ToggleOnClick(true, m_Toggles[m_Index], m_Index);
    }

    private void AddTrigger(EventTrigger trigger, EventTriggerType t, UnityAction<BaseEventData> callback)
    {
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = t;
        entry.callback.AddListener(callback);
        trigger.triggers.Add(entry);
    }

    protected override void OnViewPartUnload()
    {
        m_Trigger.triggers.Clear();

        State.OnPageIndexChanged -= OnPageChanged;
        foreach (Toggle toggle in m_Toggles)
        {
            toggle.onValueChanged.RemoveAllListeners();
            toggle.isOn = false;
            UIEventListener.UIEventListener.AttachListener(toggle.gameObject).onClick -= OnClick;
        }
    }

    /// <summary>
    /// 指针进入时
    /// </summary>
    private void OnFocusRangeEnter(BaseEventData e)
    {
        m_IsPointerOver = true;
    }
    /// <summary>
    /// 指针退出时
    /// </summary>
    private void OnFocusRangeExit(BaseEventData e)
    {
        m_IsPointerOver = false;
    }

    protected virtual List<EscChooseInfo> GetEscChooseInfos()
    {
        return null;
    }
    protected virtual UIPanel GetPanelName()
    {
        return UIPanel.None;
    }    
    /// <summary>
    /// 当前页面改变时
    /// </summary>
    /// <param name="oldIndex">变化前的索引号</param>
    /// <param name="newIndex">变化后的索引号</param>
    private void OnPageChanged(int oldIndex, int newIndex)
    {
        SetHotKeyEnable(newIndex);
        m_Toggles[newIndex].isOn = true;
        m_Index = newIndex;        
    }
    /// <summary>
    /// 鼠标点击时
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="objs"></param>
    private void OnClick(GameObject sender, params object[] objs)
    {
        Open();
    }
    protected virtual void ToggleOnClick(bool select, Toggle toggle, int index)
    {
        Animator animator = toggle.GetComponent<Animator>();
        animator.SetBool("Normal", !toggle.isOn);
        animator.SetBool("IsOn", toggle.isOn);
        if (select)
        {
            int num = index + 1;
            m_WatchController.Animator.SetInteger("State", num);
            foreach (var item in m_Toggles)
            {
                item.transform.Find("Content").GetComponent<Image>().raycastTarget = false;
            }
            m_Toggles[index].transform.Find("Content").GetComponent<Image>().raycastTarget = true;
        }
    }
    /// <summary>
    /// 打开子界面
    /// </summary>
    /// <param name="callbackContext"></param>
    private void OpenNextPanel(HotkeyCallback callbackContext)
    {
        if (callbackContext.performed)
        {
            if (m_IsPointerOver 
                || InputManager.Instance.GetNavigateMode() 
                || InputManager.Instance.CurrentInputDevice == InputManager.GameInputDevice.KeyboardAndMouse)
                Open();
        }
    }
    /// <summary>
    /// 关闭界面
    /// </summary>
    /// <param name="callbackContext"></param>
    private void ClosePanel(HotkeyCallback callbackContext)
    {
        Close();
    }
    /// <summary>
    /// 界面关闭动画播放结束
    /// </summary>
    /// <param name="key"></param>
    protected virtual void OnAnimationEvent(string key)
    {
        m_AnimatorController.OnAnimationEvent -= OnAnimationEvent;
        UIManager.Instance.ClosePanel(m_PanelName);
        UIManager.Instance.OpenPanel(UIPanel.EscWatchPanel, m_PanelName);
    }

    /// <summary>
    /// 关闭
    /// </summary>
    protected virtual void Close()
    {
        m_AnimatorController.Animator.SetTrigger("Close");
        m_AnimatorController.OnAnimationEvent -= OnAnimationEvent;
        m_AnimatorController.OnAnimationEvent += OnAnimationEvent;
    }

    protected virtual void Open()
    {

    }
    protected virtual void SetHotKeyEnable(int index)
    {

    }
    public override void OnHide()
    {

        State.GetAction(UIAction.Common_Confirm).Callback -= OpenNextPanel;
        State.GetAction(UIAction.Common_Back).Callback -= ClosePanel;
        base.OnHide();
    }
    /// <summary>
    /// 选项信息
    /// </summary>
    public class EscChooseInfo
    {
        /// <summary>
        /// 名字
        /// </summary>
        public string Name;
        /// <summary>
        /// 图标Id
        /// </summary>
        public uint IconId;
    }
}
