using Leyoutech.Core.Loader.Config;
using PureMVC.Interfaces;
using PureMVC.Patterns.Facade;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using static ConfirmPanel;

public class UIMailList : UIListPart
{
    /// <summary>
    /// 新邮件列表
    /// </summary>
    private List<string> m_NewIds = new List<string>();
    /// <summary>
    /// 邮件Proxy
    /// </summary>
    private MailProxy m_MailProxy;
    /// <summary>
    /// CfgEternityProxy
    /// </summary>
    private CfgEternityProxy m_CfgEternityProxy;
    /// <summary>
    /// 日志Proxy
    /// </summary>
    private LogProxy m_LogProxy;
    /// <summary>
    /// 邮件选中的数据
    /// </summary>
    private MailDataVO m_LastMailSelectVo = null;
    /// <summary>
    /// 日志选中数据
    /// </summary>
    private LogDataVO m_LastLogSelectVo = null;
    /// <summary>
    /// 当前选中页
    /// </summary>
    private int m_LastPageIndex = 0;
    public override void OnShow(object msg)
    {
        base.OnShow(msg);
        OwnerView.State.SetActionCompareEnabled(false);
        if (m_MailProxy == null)
        {
            m_MailProxy = GameFacade.Instance.RetrieveProxy(ProxyName.MailProxy) as MailProxy;
        }
        if (m_CfgEternityProxy == null)
        {
            m_CfgEternityProxy = GameFacade.Instance.RetrieveProxy(ProxyName.CfgEternityProxy) as CfgEternityProxy;
        }
        if (m_LogProxy == null)
        {
            m_LogProxy = GameFacade.Instance.RetrieveProxy(ProxyName.LogProxy) as LogProxy;
        }

        OwnerView.State.GetAction(UIAction.Mail_Retrieve).Callback -= GetAccessory;
        OwnerView.State.GetAction(UIAction.Mail_Retrieve).Callback += GetAccessory;
        OwnerView.State.GetAction(UIAction.Mail_Empty).Callback -= DeleteAll;
        OwnerView.State.GetAction(UIAction.Mail_Empty).Callback += DeleteAll;
        OwnerView.State.GetAction(UIAction.Mail_Delete).Callback -= DeleteMail;
        OwnerView.State.GetAction(UIAction.Mail_Delete).Callback += DeleteMail;
        OwnerView.State.GetAction(UIAction.Mail_Mark).Callback -= SetMarkMail;
        OwnerView.State.GetAction(UIAction.Mail_Mark).Callback += SetMarkMail;
        State.OnSelectionChanged += OnSelectionChanged;
    }
    protected override void OnViewPartLoaded()
    {
        base.OnViewPartLoaded();
        UpdateList(OwnerView.State.GetPageIndex());

    }
    protected override void OnViewPartUnload()
    {
        base.OnViewPartUnload();
    }
    protected override string GetCellTemplate()
    {
        int pageIndex = OwnerView.State.GetPageIndex();
        switch (pageIndex)
        {
            case 0:
                return AssetAddressKey.PRELOADUIELEMENT_MAILCELLELEMENT; ;
            case 1:
                return AssetAddressKey.PRELOADUIELEMENT_LOGELEMENT;
        }
        return null;
    }

    protected override string GetCellPlaceholderTemplate()
    {
        return AssetAddressKey.PRELOADUIELEMENT_PACKAGEELEMENT_EMPTY;
    }

    protected override string GetHeadTemplate()
    {
        return AssetAddressKey.PRELOADUIELEMENT_PACKAGETITLEELEMENT;
    }
    private void OnSelectionChanged(object obj)
    {
        if (State.GetTipData() == null)
        {
            OwnerView.State.GetAction(UIAction.Mail_Delete).Enabled = false;
            OwnerView.State.GetAction(UIAction.Mail_Mark).Enabled = false;
            OwnerView.State.GetAction(UIAction.Mail_Retrieve).Enabled = false;            
        }
    }
    protected override void OnPageIndexChanged(int oldIndex, int newIndex)
    {
        OwnerView.State.GetAction(UIAction.Mail_Empty).Enabled = true;
        m_LastPageIndex = OwnerView.State.GetPageIndex();
        if (GetTransform() == null)
        {
            return;
        }
        UpdateList(newIndex);
    }
    private void UpdateList(int index)
    {
        if (index == 0)
        {
            RenderMailList();
        }
        else
        {
            RenderLogList();
        }
    }
    protected override void OnCellRenderer(int groupIndex, int cellIndex, object cellData, RectTransform cellView, bool selected)
    {
        Animator m_Animator = cellView.GetComponent<Animator>();
        if (m_Animator)
        {
            m_Animator.SetBool("IsOn", selected);
        }
        if (OwnerView.State.GetPageIndex() == 0)
        {
            MailDataVO m_MailVo = (MailDataVO)cellData;
            if (selected)
            {
                if (m_LastMailSelectVo != m_MailVo)
                {
                    m_LastMailSelectVo = m_MailVo;

                    NetworkManager.Instance.GetMailController().C_to_S_GetMailContent(m_LastMailSelectVo.Id);
                    if (m_LastMailSelectVo.IsNew == 1)
                    {
                        RecordNewMail(m_LastMailSelectVo.Id);
                        m_MailProxy.ClearMailNew(m_LastMailSelectVo.Id);
                    }
                }
                object data = OwnerView.State.GetTipData();
                OwnerView.State.SetTipData(null);
                OwnerView.State.SetTipData(data);
                SetMailState();
            }
            MailTemplate m_MailTemplate = cellView.GetOrAddComponent<MailTemplate>();
            m_MailTemplate.Init(m_MailVo, RecordNewMail);
        }
        else
        {
            LogDataVO m_LogVo = (LogDataVO)cellData;
            LogTemplate m_LogTemplate = cellView.GetOrAddComponent<LogTemplate>();
            m_LogTemplate.Init(m_LogVo);
            if (selected)
            {
                m_LastLogSelectVo = m_LogVo;
                SetLogState();
            }
        }

    }

    protected override void OnCellPlaceholderRenderer(int groupIndex, int cellIndex, RectTransform cellView, bool selected)
    {
        Animator m_Animator = cellView.GetComponent<Animator>();
        if (m_Animator)
        {
            m_Animator.SetBool("IsOn", selected);
        }
        OwnerView.State.GetAction(UIAction.Mail_Delete).Enabled = false;
        OwnerView.State.GetAction(UIAction.Mail_Empty).Enabled = false;
        OwnerView.State.GetAction(UIAction.Mail_Mark).Enabled = false;
        OwnerView.State.GetAction(UIAction.Mail_Retrieve).Enabled = false;
    }

    /// <summary>
    /// 填充邮件数据
    /// </summary>
    private void RenderMailList()
    {
        if (GetTransform() == null)
        {
            return;
        }
        if (OwnerView.State.GetPageIndex() == 0)
        {
            MailDataVO[] mailDataVOs = m_MailProxy.GetMails().Values.ToArray();
            int totalCount = (int)m_CfgEternityProxy.GetGamingConfig(1).Value.Mail.Value.MaxNum;
            OwnerView.State.SetPageLabel(0, string.Format("{0}/{1}", mailDataVOs.Length, totalCount));
            Array.Sort(mailDataVOs, (MailDataVO x, MailDataVO y) =>
            {
                if (x.SendTime.CompareTo(y.SendTime) != 0)
                {
                    return x.SendTime.CompareTo(y.SendTime) * -1;
                }
                else
                {
                    return 0;
                }
            });
            ClearData();
            AddDatas(null, mailDataVOs);
        }
    }
    /// <summary>
    /// 填充日志数据
    /// </summary>
    private void RenderLogList()
    {
        if (GetTransform() == null)
        {
            return;
        }
        if (OwnerView.State.GetPageIndex() == 1)
        {
            LogDataVO[] m_logDataVOs = m_LogProxy.GetLogs().Values.ToArray();
            int totalCount = m_CfgEternityProxy.GetGamingConfig(1).Value.Log.Value.MaxNum;
            OwnerView.State.SetPageLabel(1, string.Format("{0}/{1}", m_logDataVOs.Length, totalCount));
            Array.Sort(m_logDataVOs, (LogDataVO x, LogDataVO y) =>
            {
                if (x.ReveiveTime.CompareTo(y.ReveiveTime) != 0)
                {
                    return x.ReveiveTime.CompareTo(y.ReveiveTime) * -1;
                }
                else
                {
                    return 0;
                }
            });
            ClearData();
            AddDatas(null, m_logDataVOs);
        }
    }
    /// <summary>
    /// 邮件选中按键状态
    /// </summary>
    private void SetMailState()
    {
        if (m_LastMailSelectVo.Got == 0 && m_LastMailSelectVo.HasAccessory == 1)
        {
            OwnerView.State.GetAction(UIAction.Mail_Retrieve).Enabled = true;
        }
        else
        {
            OwnerView.State.GetAction(UIAction.Mail_Retrieve).Enabled = false;
        }
        OwnerView.State.GetAction(UIAction.Mail_Empty).Enabled = true;
        OwnerView.State.GetAction(UIAction.Mail_Mark).Enabled = true;
        if (m_LastMailSelectVo.IsMark == 1)
        {
            OwnerView.State.GetAction(UIAction.Mail_Mark).State = 1;
            OwnerView.State.GetAction(UIAction.Mail_Delete).Enabled = false;
        }
        else
        {
            OwnerView.State.GetAction(UIAction.Mail_Mark).State = 0;
            if (m_LastMailSelectVo.HasAccessory == 1 && m_LastMailSelectVo.Got == 1)
            {
                OwnerView.State.GetAction(UIAction.Mail_Delete).Enabled = true;
            }
            else if (m_LastMailSelectVo.HasAccessory == 0)
            {
                OwnerView.State.GetAction(UIAction.Mail_Delete).Enabled = true;
            }
            else
            {
                OwnerView.State.GetAction(UIAction.Mail_Delete).Enabled = false;
            }
        }
    }
    /// <summary>
    /// 日志选中按键状态
    /// </summary>
    private void SetLogState()
    {
        OwnerView.State.GetAction(UIAction.Mail_Empty).Enabled = true;
        OwnerView.State.GetAction(UIAction.Mail_Delete).Enabled = true;
    }
    public override NotificationName[] ListNotificationInterests()
    {
        return new NotificationName[]
       {
            NotificationName.MSG_EMAIL_RENDERLIST,
            NotificationName.MSG_EMAIL_DELETE,
            NotificationName.MSG_PACKAGE_NOTENOUGH,
            NotificationName.MSG_LOG_RENDERLIST
       };
    }
    public override void HandleNotification(INotification notification)
    {
        switch ((NotificationName)notification.Name)
        {
            case NotificationName.MSG_EMAIL_RENDERLIST:
            case NotificationName.MSG_EMAIL_DELETE:
                {
                    RenderMailList();
                }
                break;
            case NotificationName.MSG_PACKAGE_NOTENOUGH:
                {
                    OpenPackageError();
                }
                break;
            case NotificationName.MSG_LOG_RENDERLIST:
                {
                    RenderLogList();
                }
                break;
            default:
                break;
        }
    }
    /// <summary>
    /// 领取附件
    /// </summary>
    /// <param name="callbackContext"></param>
    private void GetAccessory(HotkeyCallback callbackContext)
    {
        if (OwnerView.State.GetTipData() == null)
            return;

        if (callbackContext.performed)
        {
            NetworkManager.Instance.GetMailController().C_to_S_GetMail_Accessory(m_LastMailSelectVo.Id, 0);
        }
    }
    /// <summary>
    /// 设为标记
    /// </summary>
    /// <param name="callbackContext"></param>
    private void SetMarkMail(HotkeyCallback callbackContext)
    {
        if (callbackContext.performed)
        {
            NetworkManager.Instance.GetMailController().C_to_S_MarkMail(m_LastMailSelectVo.Id);
        }
    }
    /// <summary>
    /// 删除普通邮件(删除日志)
    /// </summary>
    /// <param name="callbackContext"></param>
    private void DeleteMail(HotkeyCallback callbackContext)
    {
        if (callbackContext.started)
        {

        }
        if (callbackContext.performed)
        {
            if (m_LastPageIndex == 0)
            {
                NetworkManager.Instance.GetMailController().C_to_S_GetMail_Delete(m_LastMailSelectVo.Id);
            }
            else
            {
                NetworkManager.Instance.GetLogController().C_to_S_GetLogDelete(m_LastLogSelectVo.Id);
            }
        }
    }
    /// <summary>
    /// 删除所有已读无附件未标记邮件(删除所有日志)
    /// </summary>
    /// <param name="callbackContext"></param>
    private void DeleteAll(HotkeyCallback callbackContext)
    {
        if (callbackContext.started)
        {

        }
        if (callbackContext.performed)
        {
            WwiseUtil.PlaySound((int)WwiseMusic.Music_deleteMessages_Over, false, null);
            if (m_LastPageIndex == 0)
            {
                NetworkManager.Instance.GetMailController().C_to_S_DeleAllOpened();
            }
            else
            {
                NetworkManager.Instance.GetLogController().C_to_S_DeleAllLog();
            }
        }
    }
    /// <summary>
    /// 背包格子不足提示
    /// </summary>
    private void OpenPackageError()
    {
        OpenParameter openParameter = new OpenParameter();
        openParameter.Title = TableUtil.GetLanguageString("common_hotkey_id_1001");
        openParameter.Content = TableUtil.GetLanguageString("mailbox_text_id_1017");
        openParameter.backgroundColor = BackgroundColor.Normal;
        HotKeyButton HotKeyConfirm = new HotKeyButton();
        HotKeyConfirm.actionName = HotKeyID.FuncA;
        HotKeyConfirm.showText = TableUtil.GetLanguageString("common_hotkey_id_1001");
        HotKeyConfirm.onEvent = ConfirmPanelClose;
        openParameter.HotkeyArray = new HotKeyButton[] { HotKeyConfirm };
        UIManager.Instance.OpenPanel(UIPanel.ConfirmPanel, openParameter);
    }
    /// <summary>
	/// 关闭确认弹窗
	/// </summary>
	/// <param name="notification"></param>
	/// <param name="obj"></param>
	private void ConfirmPanelClose(HotkeyCallback callback)
    {
        if (callback.performed)
        {
            UIManager.Instance.ClosePanel(UIPanel.ConfirmPanel);
        }
    }
    public override void OnHide()
    {
        m_LastMailSelectVo = null;
        m_LastLogSelectVo = null;
        if (m_NewIds != null && m_NewIds.Count > 0)
        {
            NetworkManager.Instance.GetMailController().C_to_S_ClearNew(m_NewIds);
        }
        m_NewIds.Clear();
        OwnerView.State.GetAction(UIAction.Mail_Retrieve).Callback -= GetAccessory;
        OwnerView.State.GetAction(UIAction.Mail_Empty).Callback -= DeleteAll;
        OwnerView.State.GetAction(UIAction.Mail_Delete).Callback -= DeleteMail;
        OwnerView.State.GetAction(UIAction.Mail_Mark).Callback -= SetMarkMail;
        State.OnSelectionChanged += OnSelectionChanged;

        base.OnHide();
    }
    /// <summary>
    /// 记录新邮件
    /// </summary>
    private void RecordNewMail(string id)
    {
        if (m_NewIds.Contains(id))
        {
            return;
        }
        m_NewIds.Add(id);
    }
}
