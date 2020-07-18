using Leyoutech.Core.Loader.Config;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIEscList : UIListPart
{
    /// <summary>
    /// 选项集合
    /// </summary>
    private string[] m_ToggleString;
    /// <summary>
    /// list预设地址
    /// </summary>
    private const string ASSET_ADDRESS = AssetAddressKey.PRELOADUI_COMMONLISTPART_ESCFIRST;
    /// <summary>
    /// 内容容器
    /// </summary>
    private Transform m_Position;
    /// <summary>
    /// 当前选中索引
    /// </summary>
    private int m_Index = 0;
    /// <summary>
    /// 选项展示集合
    /// </summary>
    private Toggle[] m_Element;
    /// <summary>
    /// 打开的子界面
    /// </summary>
    private UIPanel m_ChildPanel;
    public override void OnShow(object msg)
    {
        base.OnShow(msg);
        if (msg != null)
        {
            m_ChildPanel = (UIPanel)msg;
        }

        m_ToggleString = new string[]
       {
            TableUtil.GetLanguageString("esc_title_1002"),
            TableUtil.GetLanguageString("esc_title_1003"),
            TableUtil.GetLanguageString("esc_title_1004"),
            TableUtil.GetLanguageString("esc_title_1005"),
            TableUtil.GetLanguageString("esc_title_1006"),
            TableUtil.GetLanguageString("esc_title_1007"),
       };
        LoadViewPart(ASSET_ADDRESS, OwnerView.ListBox);
        State.GetAction(UIAction.Esc_Quit).Callback += ExitGame;
        State.GetAction(UIAction.Common_Confirm).Callback += OpenChildPanel;

        State.OnSelectionChanged += OnSelectionChanged;

        switch (m_ChildPanel)
        {
            case UIPanel.EscRolePanel:
                SetPageAndSelection(0, m_ToggleString[0]);
                break;
            case UIPanel.EscWarShipPanel:
                SetPageAndSelection(0, m_ToggleString[1]);
                break;
            case UIPanel.EscPermitPanel:
                SetPageAndSelection(0, m_ToggleString[2]);
                break;
            case UIPanel.EscSettingPanel:
                SetPageAndSelection(0, m_ToggleString[5]);
                break;
        }
    }
    public override void OnHide()
    {
        State.GetAction(UIAction.Esc_Quit).Callback -= ExitGame;
        State.GetAction(UIAction.Common_Confirm).Callback -= OpenChildPanel;

        State.OnSelectionChanged -= OnSelectionChanged;
        m_Index = 0;
        base.OnHide();
    }

    private void OnSelectionChanged(object obj)
    {
       
    }

    protected override string GetCellTemplate()
    {
        return AssetAddressKey.PRELOADUIELEMENT_ESCFIRSTELEMENT_LIST;
    }

    protected override void OnViewPartLoaded()
    {
        base.OnViewPartLoaded();
        ClearData();
        AddDatas(null, m_ToggleString);
        m_Position = OwnerView.ListBox.GetChild(0).Find("Content/Position");
        m_Element = m_Position.GetComponentsInChildren<Toggle>(true);
        EscRoleElement m_EscRole = m_Element[0].GetOrAddComponent<EscRoleElement>();
        m_EscRole.Init();
        EscWarShipElement m_EscWarship = m_Element[1].GetOrAddComponent<EscWarShipElement>();
        m_EscWarship.Init();
        EscPermitElement m_Permit = m_Element[2].GetOrAddComponent<EscPermitElement>();
        m_Permit.Init();
        EscStarMapElement m_EscStarMap = m_Element[3].GetOrAddComponent<EscStarMapElement>();
        m_EscStarMap.Init();
        EscShopElement m_EscShop = m_Element[4].GetOrAddComponent<EscShopElement>();
        m_EscShop.Init();
        EscSettingElement m_EscSetting = m_Element[5].GetOrAddComponent<EscSettingElement>();
        m_EscSetting.Init();
    }


    protected override void OnCellRenderer(int groupIndex, int cellIndex, object cellData, RectTransform cellView, bool selected)
    {

        Animator m_Animator = cellView.GetComponent<Animator>();
        if (m_Animator)
        {
            m_Animator.SetBool("IsOn", selected);
            m_Animator.SetTrigger("Normal");
        }
        TMP_Text label = cellView.Find("Background/Label").GetComponent<TMP_Text>();
        label.text = (string)cellData;
        if (selected)
        {
            m_Index = cellIndex;
            ShowChoose(cellIndex);
            State.GetAction(UIAction.Common_Confirm).Enabled = cellIndex == 0;
        }
    }
    /// <summary>
    /// 显示选项内容
    /// </summary>
    /// <param name="index"></param>
    private void ShowChoose(int index)
    {       
        for (int i = 0; i < m_Element.Length; i++)
        {
            if (i == index)
            {
                m_Element[i].gameObject.SetActive(true);
                m_Element[i].isOn = true;
                Animator animator = m_Element[i].GetComponent<Animator>();
                animator.SetBool("Normal", !m_Element[i].isOn);
                animator.SetBool("IsOn", m_Element[i].isOn);                          
            }
            else
            {               
                m_Element[i].gameObject.SetActive(false);
                m_Element[i].isOn = false;
            }
        }
    }
    /// <summary>
    /// 退出游戏
    /// </summary>
    /// <param name="callbackContext"></param>
    private void ExitGame(HotkeyCallback callbackContext)
    {
        if (callbackContext.performed)
        {
            NetworkManager.Instance.GetLoginController().ExitCurrentServer(3);
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
    private void OpenChildPanel(HotkeyCallback callbackContext)
    {
        if (callbackContext.performed)
        {
            switch (m_Index)
            {
                case 0: UIManager.Instance.OpenPanel(UIPanel.EscRolePanel); break;
                case 1: UIManager.Instance.OpenPanel(UIPanel.EscWarShipPanel); break;
                case 2: UIManager.Instance.OpenPanel(UIPanel.EscPermitPanel); break;
                case 3: break;
                case 4: break;
                case 5: UIManager.Instance.OpenPanel(UIPanel.EscSettingPanel); break;
            }
            UIManager.Instance.ClosePanel(UIPanel.EscWatchPanel);
        }
    }
}
