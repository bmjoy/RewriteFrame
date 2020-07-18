using Leyoutech.Core.Loader.Config;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIAgreementList : UIListPart
{
    /// <summary>
    /// list预设地址
    /// </summary>
    private const string ASSET_ADDRESS = AssetAddressKey.PRELOADUI_COMMONLISTPART_AGREEMENT;
    /// <summary>
	/// 页面 ScrollRect 组件
	/// </summary>
	private UIScrollRect m_TextsScrollRect;
    /// <summary>
	/// 协议内容root
	/// </summary>
	private Transform m_ContentRoot;
    public override void OnShow(object msg)
    {
        base.OnShow(msg);
        LoadViewPart(ASSET_ADDRESS, OwnerView.ListBox);
        OwnerView.AddHotKey(HotKeyID.NavUp, UpOnClick);
        OwnerView.AddHotKey(HotKeyID.NavDown, DownOnClick);
        State.GetAction(UIAction.Agreement_Agree).Callback += AgreeAgreement;
        State.GetAction(UIAction.Agreement_Reject).Callback += RejectAgreement;
        State.GetAction(UIAction.Agreement_Agree).Enabled = false;
    }
    protected override void OnViewPartLoaded()
    {
        base.OnViewPartLoaded();
        List<object> list = new List<object> { 1 };
        OwnerView.PageBox.gameObject.SetActive(false);
        SetSortEnabled(false);
        ClearData();
        AddDatas(null, list);
        m_TextsScrollRect = FindComponent<UIScrollRect>(OwnerView.ListBox.GetChild(0), "Content/Scroller");
        m_ContentRoot = FindComponent<Transform>(OwnerView.ListBox.GetChild(0), "Content/Scroller/Viewport/Content");
        m_TextsScrollRect.verticalScrollbar.onValueChanged.AddListener((value) =>
        {
            if (value < 0.02f || value > 1)
            {
                State.GetAction(UIAction.Agreement_Agree).Enabled = true;
            }
        });
    }
    protected override string GetCellTemplate()
    {
        return AssetAddressKey.PRELOADUIELEMENT_AGREEMENTELEMENT_LIST;
    }
    protected override void OnCellRenderer(int groupIndex, int cellIndex, object cellData, RectTransform cellView, bool selected)
    {

    }
    /// <summary>
	/// 上键按下滑动
	/// </summary>
	/// <param name="callbackContext"></param>
	private void UpOnClick(HotkeyCallback callbackContext)
    {
        if (callbackContext.performed)
        {
            m_ContentRoot.localPosition = new Vector3(m_ContentRoot.localPosition.x,
            m_ContentRoot.localPosition.y - 60,
            m_ContentRoot.localPosition.z);
        }
    }

    /// <summary>
    /// 下键按下滑动
    /// </summary>
    /// <param name="callbackContext"></param>
    private void DownOnClick(HotkeyCallback callbackContext)
    {
        if (callbackContext.performed)
        {
            m_ContentRoot.localPosition = new Vector3(m_ContentRoot.localPosition.x,
             m_ContentRoot.localPosition.y + 60,
             m_ContentRoot.localPosition.z);
        }
    }
    /// <summary>
	/// 同意协议
	/// </summary>
	/// <param name="callbackContext"></param>
	private void AgreeAgreement(HotkeyCallback callbackContext)
    {
        if (callbackContext.performed)
        {
            PlayerPrefs.SetString(GameConstant.FIRSTLOGIN, "1");
            UIManager.Instance.ClosePanel(UIPanel.AgreementPanel);
            UIManager.Instance.OpenPanel(UIPanel.ServerListPanel);
        }
    }

    /// <summary>
    /// 拒绝协议
    /// </summary>
    /// <param name="callbackContext"></param>
    private void RejectAgreement(HotkeyCallback callbackContext)
    {
        if (callbackContext.performed)
        {
            PlayerPrefs.SetString(GameConstant.FIRSTLOGIN, "0");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif        
        }
    }
    public override void OnHide()
    {
        State.GetAction(UIAction.Agreement_Agree).Callback -= AgreeAgreement;
        State.GetAction(UIAction.Agreement_Reject).Callback -= RejectAgreement;
        m_TextsScrollRect.verticalScrollbar.onValueChanged.RemoveAllListeners();
        base.OnHide();
    }
}
