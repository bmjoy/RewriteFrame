using Assets.Scripts.Define;
using Leyoutech.Core.Loader.Config;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static InputManager;

public class UIReviveList : UIListPart
{
    /// <summary>
    /// 复活list资源地址
    /// </summary>
    private const string ASSET_ADDRESS = AssetAddressKey.PRELOADUI_COMMONLISTPART_DEATH;
    /// <summary>
    /// 请求是否已经发送
    /// </summary>
    private bool m_SelectionSended = true;
    /// <summary>
    /// 倒计时的结束时间点
    /// </summary>
    private float m_CountdownEndTime = 0;
    /// 获取距离Proxy
    /// </summary>
    private TaskTrackingProxy m_TaskTrackingProxy;
    /// <summary>
    /// 复活选项文字id
    /// </summary>
    private string[] m_TogglesString;
    /// <summary>
    /// 选中标记
    /// </summary>
    private int m_Index = 0;
    /// <summary>
    /// 复活人物信息
    /// </summary>
    private ShowRelviePanelNotify reliveInfo;
    private RevivePanel m_Parent => OwnerView as RevivePanel;

    public override void OnShow(object msg)
    {
        base.OnShow(msg);
        LoadViewPart(ASSET_ADDRESS, OwnerView.ListBox);
        if (m_TaskTrackingProxy == null)
        {
            m_TaskTrackingProxy = GameFacade.Instance.RetrieveProxy(ProxyName.TaskTrackingProxy) as TaskTrackingProxy;
        }
        reliveInfo = (ShowRelviePanelNotify)msg;
        m_CountdownEndTime = reliveInfo.Countdown / 1000.0f + Time.time;
        m_SelectionSended = false;      
        m_Parent.m_KillerName.text = string.Format(TableUtil.GetLanguageString("revive_text_id_1002"), reliveInfo.KillerName);
        State.GetAction(UIAction.Common_Select).Callback += OnHotkeyCallback;
        OwnerView.PageBox.gameObject.SetActive(false);
        OwnerView.HotkeyBox.gameObject.SetActive(false);
        OwnerView.ListBox.gameObject.SetActive(false);
        State.GetAction(UIAction.Common_Select).Enabled = false;
        UIManager.Instance.OnUpdate += Update;
    }

    protected override string GetCellTemplate()
    {
        return AssetAddressKey.PRELOADUIELEMENT_DEATHELEMENT_LIST;
    }

    protected override void OnViewPartLoaded()
    {
        base.OnViewPartLoaded();
        if (reliveInfo.IsShowHallRelive)
        {
            m_TogglesString = new string[2] { "revive_text_id_1004", "revive_text_id_1003" };
        }
        else
        {
            m_TogglesString = new string[1] { "revive_text_id_1004" };
        }
        SetSortEnabled(false);
        ClearData();
        AddDatas(null, m_TogglesString);        
    }
    protected override void OnCellRenderer(int groupIndex, int cellIndex, object cellData, RectTransform cellView, bool selected)
    {
        Animator m_Animator = cellView.GetComponent<Animator>();
        if (m_Animator)
        {
            m_Animator.SetBool("IsOn", selected);
        }
        string id = (string)cellData;
        TMP_Text label = FindComponent<TMP_Text>(cellView.transform, "Label_Name");
        label.text = string.Format(TableUtil.GetLanguageString(id), GetDistanceText(id == "revive_text_id_1003"));
        if (selected)
        {
            m_Index = cellIndex;
        }
        UIEventListener.UIEventListener.AttachListener(cellView.gameObject).onClick = OnClick;
    }
    /// <summary>
    /// 倒计时刷新
    /// </summary>
    private void Update()
    {
        bool countdowning = m_CountdownEndTime > Time.time;
        if (countdowning)
        {
            float needTime = m_CountdownEndTime - Time.time;
            int needSeconds = countdowning ? Mathf.FloorToInt(needTime) : 0;
            int needMilliSeconds = countdowning ? Mathf.FloorToInt((needTime - needSeconds) * 100) : 0;

            m_Parent.m_CountdownBox.gameObject.SetActive(true);
            m_Parent.m_CountdownSecond.text = FormatTime(needSeconds);
            m_Parent.m_CountdownMillisecond.text = FormatTime(needMilliSeconds);
            State.GetAction(UIAction.Common_Select).Enabled = false;
        }
        else
        {
            m_Parent.m_CountdownBox.gameObject.SetActive(false);
            OwnerView.ListBox.gameObject.SetActive(true);
            OwnerView.HotkeyBox.gameObject.SetActive(true);
            State.GetAction(UIAction.Common_Select).Enabled = true;
            UIManager.Instance.OnUpdate -= Update;            
        }
    }
    public override void OnHide()
    {
        m_CountdownEndTime = 0;
        m_SelectionSended = false;
        UIManager.Instance.OnUpdate -= Update;
        State.GetAction(UIAction.Common_Select).Callback -= OnHotkeyCallback;
        base.OnHide();
    }
    /// <summary>
    /// 热键回调
    /// </summary>
    /// <param name="callback">HotkeyCallback</param>
    private void OnHotkeyCallback(HotkeyCallback callback)
    {
        if (callback.performed)
        {
            Debug.Log(11111111111);
            SendToServer();
        }

    }
    /// <summary>
    /// 鼠标点击时
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="objs"></param>
    private void OnClick(GameObject sender, params object[] objs)
    {            
        SendToServer();
    }
    /// <summary>
    /// 发送到服务器
    /// </summary>
    private void SendToServer()
    {
        if (m_SelectionSended)
            return;
        switch (m_Index)
        {
            case 0:
                NetworkManager.Instance.GetReliveController().RequestRelive(PlayerReliveType.relive_pos);
                break;
            case 1:
                NetworkManager.Instance.GetReliveController().RequestRelive(PlayerReliveType.relive_hall);
                break;
        }
        m_SelectionSended = true;

        UIManager.Instance.ClosePanel(UIPanel.RevivePanel);        
    }
    /// <summary>
    /// 格式化时间
    /// </summary>
    /// <param name="time"></param>
    /// <returns></returns>
    private string FormatTime(int time)
    {
        if (time <= 0)
            return "<color=#808080>00</color>";
        else if (time < 10)
            return "<color=#808080>0</color>" + time.ToString();
        else
            return time.ToString();
    }

    /// <summary>
    /// 获取距离文本
    /// </summary>
    /// <param name="relifeToHuman"></param>
    /// <returns></returns>
    private string GetDistanceText(bool relifeToHuman)
    {
        float km = m_TaskTrackingProxy.MeasureRelifeDistance(relifeToHuman) / 1000;
        if (km > 0 && km < 1000)
            return Mathf.Floor(km).ToString();
        if (km >= 1000 && km < 1000000)
            return Mathf.Floor(km / 1000) + "K";
        else
            return Mathf.Floor(km / 1000000) + "M";
    }
}
