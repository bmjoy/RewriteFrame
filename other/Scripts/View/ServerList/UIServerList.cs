using Leyoutech.Core.Loader.Config;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIServerList : UIListPart
{
    /// <summary>
    /// list预设地址
    /// </summary>
    private const string ASSET_ADDRESS = AssetAddressKey.PRELOADUI_COMMONLISTPART_SERVER;
    /// <summary>
	/// 服务器Proxy
	/// </summary>
	private ServerListProxy m_ServerListProxy;
    /// <summary>
	/// 已经存储的当前服务器id
	/// </summary>
	private string m_SelectedServerID = "";
    /// <summary>
	/// 当前toggle标记
	/// </summary>
	private int m_CurrentTierIndex;
    /// <summary>
    /// 当前服务器数据
    /// </summary>
    private ServerInfoVO m_CurrentData;
    private ServerListPanel m_Parent => OwnerView as ServerListPanel;


    public override void OnShow(object msg)
    {
        base.OnShow(msg);

        LoadViewPart(ASSET_ADDRESS, OwnerView.ListBox);   
        
        m_ServerListProxy = (ServerListProxy)GameFacade.Instance.RetrieveProxy(ProxyName.ServerListProxy);

        State.GetAction(UIAction.Common_Select).Callback += SpaceOnClick;
    }

    public override void OnHide()
    {
        State.GetAction(UIAction.Common_Select).Callback -= SpaceOnClick;

        GameFacade.Instance.SendNotification(NotificationName.MSG_LOGINPANEL_ACTIVE);

        base.OnHide();
    }

    protected override string GetCellTemplate()
    {
        return AssetAddressKey.PRELOADUIELEMENT_SERVERELEMENT_LIST;
    }

    protected override void OnViewPartLoaded()
    {
        base.OnViewPartLoaded();
        OwnerView.PageBox.gameObject.SetActive(false);
        RefreshViewByLabel();       
    }

    protected override void OnCellRenderer(int groupIndex, int cellIndex, object cellData, RectTransform cellView, bool selected)
    {
        Toggle toggle = cellView.GetComponent<Toggle>();
        Animator m_Animator = cellView.GetComponent<Animator>();
        if (m_Animator)
        {
            m_Animator.SetBool("IsOn", selected);
        }

        ServerInfoVO serverData = (ServerInfoVO)cellData;
        cellView.Find("Content/ServerField").GetComponent<TMP_Text>().text = serverData.Name;
        cellView.Find("Content/StateField").GetComponent<TMP_Text>().text = serverData.State.ToString();

        if (selected)
        {
            SelectToggle(cellIndex, selected);            
        }
    }

    protected override void OnItemDoubleClick(int groupIndex, int cellIndex, object cellData)
    {
        UIManager.Instance.ClosePanel(UIPanel.ServerListPanel);
        GameFacade.Instance.SendNotification(NotificationName.MSG_LOGINPANEL_ACTIVE);
    }

    /// <summary>
    /// 刷新界面
    /// </summary>
    public void RefreshViewByLabel()
    {
        m_SelectedServerID = m_ServerListProxy.GetSelectedServer()?.Gid ?? m_ServerListProxy.GetLastLoginServer()?.Gid ?? "";
        List<object> datas = new List<object>();        
        int index = 0;
        for (int i = 0; i < m_ServerListProxy.GetServerList().Count; i++)
        {
            datas.Add(m_ServerListProxy.GetServerList()[i]);
            if (m_ServerListProxy.GetServerList()[i].Gid == m_SelectedServerID)
            {
                m_Parent.m_CurrentServerNameText.text = m_ServerListProxy.GetServerList()[i].Name;
                m_Parent.m_CurrentServerStateText.text = m_ServerListProxy.GetServerList()[i].State.ToString();
                index = i;
            }
        }       
        ClearData();
        SetSortEnabled(false);
        AddDatas(null, datas);
        SetPageAndSelection(0, m_ServerListProxy.GetServerList()[index]);
    }

    /// <summary>
	/// 选择toggle事件
	/// </summary>
	/// <param name="index">索引</param>
	/// <param name="select">是否选中</param>
	private void SelectToggle(int index, bool select)
    {
        if (select)
        {
            m_CurrentTierIndex = index;
            m_CurrentData = m_ServerListProxy.GetServerList()[m_CurrentTierIndex];
            m_ServerListProxy.SetSelectedServer(m_CurrentData.Gid);
            m_Parent.m_CurrentServerNameText.text = m_CurrentData.Name;
            m_Parent.m_CurrentServerStateText.text = m_CurrentData.State.ToString();
        }
    }

    /// <summary>
	/// space键按下
	/// </summary>
	/// <param name="obj"></param>
	private void SpaceOnClick(HotkeyCallback callbackContext)
    {
        if (callbackContext.performed)
        {
            //if (callbackContext.isFromKeyboardMouse || callbackContext.isFromUI)
            //{
                UIManager.Instance.ClosePanel(UIPanel.ServerListPanel);
                GameFacade.Instance.SendNotification(NotificationName.MSG_LOGINPANEL_ACTIVE);
            //}
        }
    }
}
