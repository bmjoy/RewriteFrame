using PureMVC.Interfaces;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EscWatchBase : UIPanelBase
{   
    /// <summary>
    /// 选项根节点
    /// </summary>
    public Transform m_Root;
    /// <summary>
    /// 标题文字
    /// </summary>
    public TMP_Text m_Title;
    /// <summary>
    /// 标题表字段
    /// </summary>
    public string m_TitleString;
    /// <summary>
    /// 列表组合
    /// </summary>
    public Toggle[] m_Toggles;
    /// <summary>
    /// 当前选中索引
    /// </summary>
    public int m_Index = 0;
    /// <summary>
    /// 选中切换动画
    /// </summary>
    public UIAnimationEvent m_WatchController;
    /// <summary>
    /// 进入退出动画
    /// </summary>
    public UIAnimationEvent m_AnimatorController;
    /// <summary>
    /// 选项文字集合
    /// </summary>
    public string[] m_ToggleString;
    /// <summary>
    /// 显示图标
    /// </summary>
    public uint[] m_Icons;
    /// <summary>
    /// 热键挂点
    /// </summary>
    public Transform m_HotkeyRoot;    
    private float m_FovOld;
    public EscWatchBase(UIPanel panelName, string assetAddress, PanelType panelType) : base(panelName, assetAddress, panelType) { }

    public override void Initialize()
    {
        base.Initialize();
        m_Root = FindComponent<Transform>("Content");
        m_Title = FindComponent<TMP_Text>("Title/Label_Title");
        m_WatchController = GetTransform().GetComponent<UIAnimationEvent>();
        m_AnimatorController = m_Root.GetComponent<UIAnimationEvent>();
        m_HotkeyRoot = FindComponent<Transform>("Control/Content/View_Bottom/List");        
    }
    public override void OnGotFocus()
    {
        base.OnGotFocus();
        m_FovOld = CameraManager.GetInstance().GetUICameraComponent().GetCamera().fieldOfView;
        CameraManager.GetInstance().GetUICameraComponent().GetCamera().fieldOfView = 30;
        AddHotKey(HotKeyID.NavLeft, OnLeftClick);
        AddHotKey(HotKeyID.NavRight, OnRightClick);
        AddHotKey(HotKeyID.FuncA, OpenPanel, m_HotkeyRoot, TableUtil.GetLanguageString("esc_hotkey_1002"));
        AddHotKey(HotKeyID.FuncB, ClosePanel, m_HotkeyRoot, TableUtil.GetLanguageString("esc_hotkey_1003"));
    }
    public override void OnLostFocus()
    {
        base.OnLostFocus();
        CameraManager.GetInstance().GetUICameraComponent().GetCamera().fieldOfView = m_FovOld;
    }

    public override void OnShow(object msg)
    {
        base.OnShow(msg);
        ResetToggle();
        if (msg != null)
        {
            m_Toggles[(int)msg].isOn = true;
            m_Toggles[(int)msg].GetComponent<Animator>().SetBool("IsOn", true);
        }

    }
    public override NotificationName[] ListNotificationInterests()
    {
        return new NotificationName[]
        {
            NotificationName.MSG_ESC_ANIMATOR_END,
        };
    }
    public override void HandleNotification(INotification notification)
    {
        switch (notification.Name)
        {
            case NotificationName.MSG_ESC_ANIMATOR_END:
                foreach (Toggle toggle in m_Toggles)
                {
                    Animator animator = toggle.GetComponent<Animator>();
                    animator.SetBool("Normal", !toggle.isOn);
                    animator.SetBool("IsOn", toggle.isOn);
                }
                break;
        }
    }
    protected virtual void ResetToggle()
    {        
        m_Toggles = m_Root.GetComponentsInChildren<Toggle>();
        m_Toggles[0].isOn = true;
        m_Toggles[0].GetComponent<Animator>().SetBool("IsOn", true);
        for (int i = 0; i < m_Toggles.Length; i++)
        {
            Toggle toggle = m_Toggles[i];            
            toggle.group = m_Root.GetComponent<ToggleGroup>();
            int index = i;                  
            toggle.onValueChanged.RemoveAllListeners();
            toggle.onValueChanged.AddListener((select) =>
            {                
                ToggleOnClick(select, toggle, index);
                m_Index = index;
            });
        }

       
    }
    /// <summary>
    /// 左按下
    /// </summary>
    public void OnLeftClick(HotkeyCallback callback)
    {
        if (!callback.performed)
            return;
        if (m_Index == 0)
        {
            m_Index = m_Toggles.Length - 1;
        }
        else
        {
            m_Index = m_Index - 1;
        }
        m_Toggles[m_Index].isOn = true;
    }
    /// <summary>
    /// 右按下
    /// </summary>
    private void OnRightClick(HotkeyCallback callback)
    {
        if (!callback.performed)
            return;
        if (m_Index == m_Toggles.Length - 1)
        {
            m_Index = 0;
        }
        else
        {
            m_Index = m_Index + 1;
        }
        m_Toggles[m_Index].isOn = true;
    }
    /// <summary>
    /// 打开子界面
    /// </summary>
    /// <param name="callbackContext"></param>
    private void OpenPanel(HotkeyCallback callbackContext)
    {
        if (callbackContext.performed)
        {
            Open();
        }
    }
    /// <summary>
    /// 关闭界面
    /// </summary>
    /// <param name="callbackContext"></param>
    private void ClosePanel(HotkeyCallback callbackContext)
    {
        if (callbackContext.performed)
        {
            Close();
        }
    }
    /// <summary>
    /// 进去切出动画播放结束
    /// </summary>
    /// <param name="key"></param>
    protected virtual void OnAnimationEvent(string key)
    {
        m_AnimatorController.OnAnimationEvent -= OnAnimationEvent;
        UIManager.Instance.ClosePanel(this);
    }
    /// <summary>
    /// 打开
    /// </summary>
    protected virtual void Open()
    {

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
    protected void ToggleOnClick(bool select, Toggle toggle, int index)
    {
        if (select)
        {
            int num = index + 1;
            m_WatchController.Animator.SetInteger("State",num);           
        }
    }

    public override void OnHide(object msg)
    {
        base.OnHide(msg);
        foreach (Toggle toggle in m_Toggles)
        {
            toggle.onValueChanged.RemoveAllListeners();
            toggle.isOn = false;
        }
        m_Index = 0;
    }
}
