using Leyoutech.Core.Loader.Config;
using PureMVC.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static FriendInfoVO;

public class UISocialList : UIListPart
{
    /// <summary>
    /// 标题栏资源路径
    /// </summary>
    private const string TITLE_ADDRESS = AssetAddressKey.PRELOADUIELEMENT_PACKAGETITLEELEMENT;
    /// <summary>
	/// 好友proxy
	/// </summary>
	private FriendProxy m_FriendProxy;

    /// <summary>
    /// 队伍proxy
    /// </summary>
    private TeamProxy m_TeamProxy;

    /// <summary>
    /// 公用表
    /// </summary>
    private CfgEternityProxy m_CfgEternityProxy;

    /// <summary>
    /// 公用表
    /// </summary>
    private ServerListProxy m_ServerListProxy;

    /// <summary>
	/// 当前的选项
	/// </summary>
    private SocialType m_SocialType;

    /// <summary>
    /// 社交子类型按钮
    /// </summary>
    private SocialSubClass m_SocialSubClass;

    /// <summary>
	/// 当前选择的Uid
	/// </summary>
	private ulong m_SelectVoUid;

    /// <summary>
    /// 上次选择的Uid 
    /// </summary>
    private ulong m_OldVoUid;
    /// <summary>
	/// 是否选中第一个
	/// </summary>
	private bool m_FirstSelect;

    /// <summary>
    /// 本页选中第几个
    /// </summary>
    private int m_SelectIndex;

    /// <summary>
    /// 第几页
    /// </summary>
    private int m_PageIndex;

    /// <summary>
    /// 左侧标签选中第几个
    /// </summary>
    private int m_LeftSelectIndex;
    /// <summary>
    /// 视图打开时调用
    /// </summary>
    /// <param name="owner">父视图</param>
    public override void OnShow(object msg)
    {
        base.OnShow(msg);
        m_FriendProxy = GameFacade.Instance.RetrieveProxy(ProxyName.FriendProxy) as FriendProxy;
        m_TeamProxy = GameFacade.Instance.RetrieveProxy(ProxyName.TeamProxy) as TeamProxy;
        m_CfgEternityProxy = GameFacade.Instance.RetrieveProxy(ProxyName.CfgEternityProxy) as CfgEternityProxy;
        m_ServerListProxy = GameFacade.Instance.RetrieveProxy(ProxyName.ServerListProxy) as ServerListProxy;
        m_OldVoUid = 2;
        m_SelectVoUid = 0;
        State.OnSelectionChanged -= OnSelectionDataChanged;
        State.OnSelectionChanged += OnSelectionDataChanged;
        State.SetActionCompareEnabled(false);
    }

    

    /// <summary>
    /// 视图关闭时调用
    /// </summary>
    /// <param name="owner">父视图</param>
    public override void OnHide()
    {
        base.OnHide();
        State.OnSelectionChanged -= OnSelectionDataChanged;
    }

    protected override void OnViewPartLoaded()
    {
        base.OnViewPartLoaded();
        NetworkManager.Instance.GetFriendController().RequestFriendList();
        NetworkManager.Instance.GetFriendController().RequestRecentList();
        NetworkManager.Instance.GetTeamController().GetTeamList();//获取组队列表

        State.GetAction(UIAction.Social_PrivateChat).Callback += OnChatDown;
        State.GetAction(UIAction.Team_Promote).Callback += OnSwitchDown;
        State.GetAction(UIAction.Team_Invite).Callback += OnSwitchDown;
        State.GetAction(UIAction.Social_Inspect_AddFriend).Callback += OnAddFriendDown;
        State.GetAction(UIAction.Social_AddBlacklist).Callback += OnBlockDown;
        State.GetAction(UIAction.Social_Delete_RemoveBlacklist).Callback += OnLeaveDown;
        State.GetAction(UIAction.Team_Kick_Leave).Callback += OnLeaveDown;
        State.GetAction(UIAction.Team_Mute_Unmute_MicOn_MicOff).Callback += OnMuteDown;
    }
    protected override void OnViewPartUnload()
    {
        base.OnViewPartUnload();
        State.GetAction(UIAction.Social_PrivateChat).Callback -= OnChatDown;
        State.GetAction(UIAction.Team_Promote).Callback -= OnSwitchDown;
        State.GetAction(UIAction.Team_Invite).Callback -= OnSwitchDown;
        State.GetAction(UIAction.Social_Inspect_AddFriend).Callback -= OnAddFriendDown;
        State.GetAction(UIAction.Social_AddBlacklist).Callback -= OnBlockDown;
        State.GetAction(UIAction.Social_Delete_RemoveBlacklist).Callback -= OnLeaveDown;
        State.GetAction(UIAction.Team_Kick_Leave).Callback -= OnLeaveDown;
        State.GetAction(UIAction.Team_Mute_Unmute_MicOn_MicOff).Callback -= OnMuteDown;
    }
    protected override string GetHeadTemplate()
    {
        return TITLE_ADDRESS;
    }
    protected override string GetCellTemplate()
    {
        int pageIndex = State.GetPageIndex();
        UIViewListLayout style = State.GetPageLayoutStyle(State.GetPageIndex());

        if (style == UIViewListLayout.Row)
        {
            return AssetAddressKey.PRELOADUIELEMENT_SOCIALELEMENT;
        }
        else if (style == UIViewListLayout.Grid)
        {
            return AssetAddressKey.PRELOADUIELEMENT_SOCIALELEMENT;
        }
        return null;
    }
    protected override string GetCellPlaceholderTemplate()
    {
        return AssetAddressKey.PRELOADUIELEMENT_PACKAGEELEMENT_EMPTY;
    }
    public override NotificationName[] ListNotificationInterests()
    {
        return new NotificationName[]
        {
            NotificationName.MSG_FRIEND_LIST_CHANGED,
            NotificationName.MSG_TEAM_MEMBER_UPDATE,
            NotificationName.MSG_FRIEND_SHIPDATA_CHANGE,
        };
    }

    public override void HandleNotification(INotification notification)
    {
        switch ((NotificationName)notification.Name)
        {
            case NotificationName.MSG_FRIEND_LIST_CHANGED://好友列表更新
                 FillPanel();
                break;
            case NotificationName.MSG_TEAM_MEMBER_UPDATE://team成员更新
                 FillPanel();
                //Debug.Log("队伍更新");
                break;
            case NotificationName.MSG_FRIEND_SHIPDATA_CHANGE:
                 GetShipData((ulong)notification.Body);
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// 页签变化时
    /// </summary>
    /// <param name="oldIndex">老页签</param>
    /// <param name="newIndex">新页签</param>
    protected override void OnPageIndexChanged(int oldIndex, int newIndex)
    {
        FillPanel();
    }

    /// <summary>
    /// 过滤索引改变时
    /// </summary>
    /// <param name="oldIndex"></param>
    /// <param name="newIndex"></param>
    protected override void OnFilterIndexChanged(int oldIndex, int newIndex)
    {
        FillPanel();
    }

    /// <summary>
    /// 选中事件
    /// </summary>
    /// <param name="obj">数据</param>
    private void OnSelectionDataChanged(object obj)
    {
        SetHotKeyEnabled(UIAction.Social_AddBlacklist, false, true);
        SetHotKeyEnabled(UIAction.Social_Delete_RemoveBlacklist, false, true);
        SetHotKeyEnabled(UIAction.Team_Kick_Leave, false, true);
        if (obj is TeamMemberVO)
        {
            m_SelectVoUid = (obj as TeamMemberVO).UID;
            if (m_OldVoUid != m_SelectVoUid)
            {
                m_OldVoUid = m_SelectVoUid;
            }
        }
        else if (obj is FriendInfoVO)
        {
            m_SelectVoUid = (obj as FriendInfoVO).UID;
            if (m_OldVoUid != m_SelectVoUid)
            {
                m_OldVoUid = m_SelectVoUid;
            }
        }
        else
        {
            m_OldVoUid = m_SelectVoUid = 0;
        }
        RefreshHotKey();

    }

    protected override void OnCellRenderer(int groupIndex, int cellIndex, object cellData, RectTransform cellView, bool selected)
    {
        UIViewListLayout style = State.GetPageLayoutStyle(State.GetPageIndex());

        Animator animator = cellView.GetComponent<Animator>();
        RectTransform content = FindComponent<RectTransform>(cellView, "Content");
        if (animator)
            animator.SetBool("IsOn", selected);

        int page = groupIndex;
        int index = cellIndex;
        ulong infoVOId = 0;
        SocialElement m_SocialElement = cellView.GetComponent<SocialElement>();
        if (m_SocialElement == null)
        {
            m_SocialElement = cellView.gameObject.AddComponent<SocialElement>();
            m_SocialElement.Initialize();
        }

        switch (m_SocialType)
        {
            case SocialType.Team:
                TeamMemberVO teamMemberVO = cellData as TeamMemberVO;
                m_SocialElement.SetTeamData(teamMemberVO, m_SocialType, new Vector2Int(groupIndex, cellIndex));
                infoVOId = teamMemberVO.UID;
                break;
            case SocialType.Friend:
                FriendInfoVO friendInfoVO = cellData as FriendInfoVO;
                m_SocialElement.SetFriendData(friendInfoVO, m_SocialType);
                infoVOId = friendInfoVO.UID;
                break;
            case SocialType.Ship:
                FriendInfoVO Ship = cellData as FriendInfoVO;
                m_SocialElement.SetFriendData(Ship, m_SocialType);
                infoVOId = Ship.UID;
                break;
            case SocialType.Other:
                FriendInfoVO Other = cellData as FriendInfoVO;
                m_SocialElement.SetFriendData(Other, m_SocialType);
                infoVOId = Other.UID;
                break;
            default:
                break;
        }   

    }

    /// <summary>
    /// 刷新热键
    /// </summary>
    public void RefreshHotKey()
    {
        if (m_SelectVoUid > 0)
        {
            SetHotKeyEnabled(UIAction.Social_AddBlacklist, true, true);
            SetHotKeyEnabled(UIAction.Social_Delete_RemoveBlacklist, true, true);
            SetHotKeyEnabled(UIAction.Team_Kick_Leave, true, true);
            SetHotKeyEnabled(UIAction.Social_Inspect_AddFriend, true);
            SetHotKeyEnabled(UIAction.Social_PrivateChat, true);
            SetHotKeyEnabled(UIAction.Team_Promote, true);
            SetHotKeyEnabled(UIAction.Team_Invite, true);
            State.GetAction(UIAction.Social_Inspect_AddFriend).Visible = true;
        }
        SetHotKeyEnabled(UIAction.Team_Mute_Unmute_MicOn_MicOff, true);
        switch (m_SocialType)
        {
            case SocialType.Team:
                SetHotKeyDescription(UIAction.Social_Inspect_AddFriend, 1);
                SetHotKeyDescription(UIAction.Team_Promote, 0);
                SetHotKeyDescription(UIAction.Team_Invite, 0);
                if (m_TeamProxy.GetMember(m_SelectVoUid) != null)
                {
                    if (!m_TeamProxy.GetMember(m_SelectVoUid).IsOnline)
                    {
                        SetHotKeyEnabled(UIAction.Social_Inspect_AddFriend, false);
                        SetHotKeyEnabled(UIAction.Social_PrivateChat, false);
                        SetHotKeyEnabled(UIAction.Team_Promote, false);
                        SetHotKeyEnabled(UIAction.Team_Invite, false);
                        SetHotKeyEnabled(UIAction.Team_Mute_Unmute_MicOn_MicOff, false);
                    }
                }
                if (IsCaptainBySelf())//自己是队长
                {
                    if (m_SelectVoUid == m_ServerListProxy.GetCurrentCharacterVO().UId)//选自己
                    {
                        SetHotKeyDescription(UIAction.Social_Delete_RemoveBlacklist, 1, true);
                        SetHotKeyDescription(UIAction.Team_Kick_Leave, 1, true);
                        if (m_TeamProxy.GetMember(m_SelectVoUid) != null)
                        {
                            if (m_TeamProxy.GetMember(m_SelectVoUid).IsMute)
                                SetHotKeyDescription(UIAction.Team_Mute_Unmute_MicOn_MicOff, 3);
                            else
                                SetHotKeyDescription(UIAction.Team_Mute_Unmute_MicOn_MicOff, 2);
                        }
                        SetHotKeyEnabled(UIAction.Social_Inspect_AddFriend, false);
                        SetHotKeyEnabled(UIAction.Social_AddBlacklist, false, true);
                        SetHotKeyEnabled(UIAction.Social_PrivateChat, false);
                        SetHotKeyEnabled(UIAction.Team_Promote, false);
                        SetHotKeyEnabled(UIAction.Team_Invite, false);
                        SetHotKeyDescription(UIAction.Social_Inspect_AddFriend, 0);
                    }
                    else
                    {
                        SetHotKeyDescription(UIAction.Social_Delete_RemoveBlacklist, 0, true);
                        SetHotKeyDescription(UIAction.Team_Kick_Leave, 0, true);
                        if (m_TeamProxy.GetMember(m_SelectVoUid) != null)
                        {
                            if (m_TeamProxy.GetMember(m_SelectVoUid).IsMute)
                            {
                                SetHotKeyDescription(UIAction.Team_Mute_Unmute_MicOn_MicOff, 1);
                            }

                        }
                        SetHotKeyDescription(UIAction.Team_Mute_Unmute_MicOn_MicOff, 0);
                        if (m_FriendProxy.GetFriend(m_SelectVoUid) != null)//好友
                        {
                            SetHotKeyEnabled(UIAction.Social_Inspect_AddFriend, false);
                            SetHotKeyDescription(UIAction.Social_Inspect_AddFriend, 0);
                        }
                        else
                        {
                            SetHotKeyDescription(UIAction.Social_Inspect_AddFriend, 1);
                        }
                        if (m_FriendProxy.GetBlack(m_SelectVoUid) != null)
                        {
                            SetHotKeyEnabled(UIAction.Social_AddBlacklist, false, true);
                        }
                    }

                }
                else
                {
                    SetHotKeyEnabled(UIAction.Team_Promote, false);
                    SetHotKeyEnabled(UIAction.Team_Invite, false);
                    if (m_SelectVoUid == m_ServerListProxy.GetCurrentCharacterVO().UId)//选自己
                    {
                        SetHotKeyDescription(UIAction.Social_Delete_RemoveBlacklist, 1, true);
                        SetHotKeyDescription(UIAction.Team_Kick_Leave, 1, true);
                        SetHotKeyEnabled(UIAction.Social_Inspect_AddFriend, false);
                        SetHotKeyEnabled(UIAction.Social_AddBlacklist, false, true);
                        SetHotKeyEnabled(UIAction.Social_PrivateChat, false);
                        SetHotKeyDescription(UIAction.Social_Inspect_AddFriend, 0);
                        if (m_TeamProxy.GetMember(m_SelectVoUid) != null)
                        {
                            if (m_TeamProxy.GetMember(m_SelectVoUid).IsMute)
                                SetHotKeyDescription(UIAction.Team_Mute_Unmute_MicOn_MicOff, 3);
                            else
                                SetHotKeyDescription(UIAction.Team_Mute_Unmute_MicOn_MicOff, 2);
                        }

                    }
                    else
                    {
                        SetHotKeyDescription(UIAction.Team_Mute_Unmute_MicOn_MicOff, 0);
                        SetHotKeyDescription(UIAction.Social_Delete_RemoveBlacklist, 0, true);
                        SetHotKeyDescription(UIAction.Team_Kick_Leave, 0, true);
                        SetHotKeyEnabled(UIAction.Social_Delete_RemoveBlacklist, false, true);
                        SetHotKeyEnabled(UIAction.Team_Kick_Leave, false, true);
                        if (m_FriendProxy.GetFriend(m_SelectVoUid) != null)//好友
                        {
                            SetHotKeyDescription(UIAction.Social_Inspect_AddFriend, 0);
                            SetHotKeyEnabled(UIAction.Social_Inspect_AddFriend, false);
                        }
                        else
                        {
                            SetHotKeyDescription(UIAction.Social_Inspect_AddFriend, 1);
                        }
                        if (m_FriendProxy.GetBlack(m_SelectVoUid) != null)
                        {
                            SetHotKeyEnabled(UIAction.Social_AddBlacklist, false, true);
                        }
                    }
                }
                if (m_TeamProxy.GetMember(m_ServerListProxy.GetCurrentCharacterVO().UId) == null)
                {
                    SetHotKeyEnabled(UIAction.Social_Delete_RemoveBlacklist, false,true);
                    SetHotKeyEnabled(UIAction.Team_Kick_Leave, false, true);
                    SetHotKeyEnabled(UIAction.Team_Mute_Unmute_MicOn_MicOff, false);
                    SetHotKeyEnabled(UIAction.Social_AddBlacklist, false, true);
                    if (m_SelectVoUid == m_ServerListProxy.GetCurrentCharacterVO().UId)
                    {
                        SetHotKeyDescription(UIAction.Team_Mute_Unmute_MicOn_MicOff, 2);
                    }
                    else
                    {
                        SetHotKeyDescription(UIAction.Team_Mute_Unmute_MicOn_MicOff, 0);
                    }
                }
                if (m_SelectVoUid <= 0)
                {
                    SetHotKeyEnabled(UIAction.Team_Mute_Unmute_MicOn_MicOff, false);
                }
                break;
            case SocialType.Friend:
                SetHotKeyDescription(UIAction.Social_Inspect_AddFriend, 1);
                State.GetAction(UIAction.Social_Inspect_AddFriend).Visible = false;
                SetHotKeyDescription(UIAction.Team_Mute_Unmute_MicOn_MicOff, 0);
                SetHotKeyDescription(UIAction.Social_Delete_RemoveBlacklist, 0, true);
                SetHotKeyDescription(UIAction.Team_Kick_Leave, 0, true);
                SetHotKeyDescription(UIAction.Team_Promote, 0);
                SetHotKeyDescription(UIAction.Team_Invite, 0);
                if (m_FriendProxy.GetBlack(m_SelectVoUid) != null)
                {
                    SetHotKeyDescription(UIAction.Social_Inspect_AddFriend, 1);
                    SetHotKeyEnabled(UIAction.Social_AddBlacklist, false, true);
                    SetHotKeyDescription(UIAction.Social_Delete_RemoveBlacklist, 1, true);
                    SetHotKeyDescription(UIAction.Team_Kick_Leave, 1, true);
                    SetHotKeyEnabled(UIAction.Social_Inspect_AddFriend, false);
                }
                else
                {
                    SetHotKeyDescription(UIAction.Social_Delete_RemoveBlacklist, 0, true);
                    SetHotKeyDescription(UIAction.Team_Kick_Leave, 0, true);
                }
                if (m_FriendProxy.GetFriend(m_SelectVoUid) != null)
                {
                    SetHotKeyEnabled(UIAction.Social_Inspect_AddFriend, false);

                    if (m_FriendProxy.GetFriend(m_SelectVoUid).Status == FriendInfoVO.FriendState.LEAVE)
                    {
                        SetHotKeyEnabled(UIAction.Social_PrivateChat, false);
                        SetHotKeyEnabled(UIAction.Team_Promote, false);
                        SetHotKeyEnabled(UIAction.Team_Invite, false);
                    }
                }
                if (m_TeamProxy.GetMember(m_SelectVoUid) != null)
                {
                    SetHotKeyEnabled(UIAction.Team_Promote, false);
                    SetHotKeyEnabled(UIAction.Team_Invite, false);
                }
              
                break;
            case SocialType.Ship:
                break;
            case SocialType.Other:
                SetHotKeyDescription(UIAction.Social_Inspect_AddFriend, 1);
                SetHotKeyDescription(UIAction.Team_Mute_Unmute_MicOn_MicOff, 0);
                SetHotKeyDescription(UIAction.Social_Delete_RemoveBlacklist, 0, true);
                SetHotKeyDescription(UIAction.Team_Kick_Leave, 0, true);
                SetHotKeyDescription(UIAction.Team_Promote, 0);
                SetHotKeyDescription(UIAction.Team_Invite, 0);
                SetHotKeyEnabled(UIAction.Social_Delete_RemoveBlacklist, false, true);
                SetHotKeyEnabled(UIAction.Team_Kick_Leave, false, true);
                if (m_FriendProxy.GetBlack(m_SelectVoUid) != null)
                {
                    SetHotKeyEnabled(UIAction.Social_AddBlacklist, false, true);
                    if (m_FriendProxy.GetBlack(m_SelectVoUid).Status == FriendInfoVO.FriendState.LEAVE)
                    {
                        SetHotKeyEnabled(UIAction.Team_Invite, false);
                    }
                }
                if (m_FriendProxy.GetFriend(m_SelectVoUid) != null)
                {
                    SetHotKeyDescription(UIAction.Social_Inspect_AddFriend, 0);
                    SetHotKeyEnabled(UIAction.Social_Inspect_AddFriend, false);
                    State.GetAction(UIAction.Social_Inspect_AddFriend).Visible = false;
                    if (m_FriendProxy.GetFriend(m_SelectVoUid).Status == FriendInfoVO.FriendState.LEAVE)
                    {
                        SetHotKeyEnabled(UIAction.Social_PrivateChat, false);
                        SetHotKeyEnabled(UIAction.Team_Promote, false);
                        SetHotKeyEnabled(UIAction.Team_Invite, false);
                    }
                }
                else
                {
                    if (m_FriendProxy.GetRecent(m_SelectVoUid)!=null)
                    {
                        if (m_FriendProxy.GetRecent(m_SelectVoUid).Status == FriendInfoVO.FriendState.LEAVE)
                            State.GetAction(UIAction.Social_Inspect_AddFriend).Visible = false;
                    }
                    SetHotKeyDescription(UIAction.Social_Inspect_AddFriend, 1);
                }
                if (m_TeamProxy.GetMember(m_SelectVoUid) != null)
                {
                    SetHotKeyEnabled(UIAction.Team_Promote, false);
                    SetHotKeyEnabled(UIAction.Team_Invite, false);
                }
                break;
            default:
                break;
        }
        if (m_SelectVoUid <= 0)
        {
            SetHotKeyEnabled(UIAction.Social_AddBlacklist, false,true);
            SetHotKeyEnabled(UIAction.Social_Delete_RemoveBlacklist, false, true);
            SetHotKeyEnabled(UIAction.Team_Kick_Leave, false, true);
            SetHotKeyEnabled(UIAction.Social_Inspect_AddFriend, false);
            State.GetAction(UIAction.Social_Inspect_AddFriend).Visible = false;
            SetHotKeyEnabled(UIAction.Social_PrivateChat, false);
            SetHotKeyEnabled(UIAction.Team_Promote, false);
            SetHotKeyEnabled(UIAction.Team_Invite, false);
        }
    }

    /// <summary>
    /// 设置热键可见性
    /// </summary>
    /// <param name="hotKeyID">hotKeyID</param>
    /// <param name="enable">可见性</param>
    public void SetHotKeyEnabled(string hotKeyID, bool enable, bool isHold = false)
    {
        State.GetAction(hotKeyID).Enabled = enable;
    }

    /// <summary>
    /// 设置热键可见性
    /// </summary>
    /// <param name="hotKeyID">hotKeyID</param>
    /// <param name="style">风格样式</param>
    public void SetHotKeyDescription(string hotKeyID, int style = 0, bool isHold = false)
    {
        State.GetAction(hotKeyID).State = style;

        if (hotKeyID == UIAction.Social_Inspect_AddFriend)
        {
            if (style == 0)
                State.GetAction(hotKeyID).Visible = false;
            else
                State.GetAction(hotKeyID).Visible = true;
        }
    }

    #region 按键

    /// <summary>
    /// X 键按下 好友筛选获取队伍静音 
    /// </summary>
    /// <param name="callbackContext">参数</param>
    public void OnMuteDown(HotkeyCallback callbackContext)
    {
        if (callbackContext.performed)
        {
            if (m_SocialType == SocialType.Team)
            {
                Debug.Log(111111);
            }
            else
            {
                //禁言关麦
            }
        }
    }

    /// <summary>
    /// F 键按下 转让队长或好友邀请组队
    /// </summary>
    /// <param name="callbackContext">参数</param>
    public void OnSwitchDown(HotkeyCallback callbackContext)
    {
        if (callbackContext.performed)
        {
            if (m_SelectVoUid > 0)
            {
                //转让队长
                if (m_SocialType == SocialType.Team)
                {
                    NetworkManager.Instance.GetTeamController().SwitchingCaptain(m_SelectVoUid);
                }
                else//组队邀请
                {
                    if (m_TeamProxy.GetMember(m_SelectVoUid) == null)
                    {
                        NetworkManager.Instance.GetTeamController().Invite(m_SelectVoUid);
                    }
                }
            }
        }
    }
    /// <summary>
    /// V 键按下 离开 或删除 或移除黑名单
    /// </summary>
    /// <param name="callbackContext">参数</param>
    public void OnLeaveDown(HotkeyCallback callbackContext)
    {
        if (callbackContext.performed)
        {
            WwiseUtil.PlaySound((int)WwiseMusic.Music_SocialLift_Over, false, null);
            if (m_SelectVoUid > 0)
            {
                if (m_SocialType != SocialType.Team)
                {
                    if (m_FriendProxy.GetFriend(m_SelectVoUid) != null)
                    {
                        NetworkManager.Instance.GetFriendController().RequestDeleteFromFriendList(m_SelectVoUid);
                    }
                    else if (m_FriendProxy.GetBlack(m_SelectVoUid) != null)
                    {
                        NetworkManager.Instance.GetFriendController().RequestDeleteFromBlackList(m_SelectVoUid);
                    }
                }
                else
                {
                    //离开队伍
                    if (m_SelectVoUid == m_ServerListProxy.GetCurrentCharacterVO().UId)
                    {
                        NetworkManager.Instance.GetTeamController().LeaveTeam();
                        Debug.Log("发送离开队伍");
                        Debug.Log(m_TeamProxy.GetMembersList().Count);
                    }
                    else
                    {
                        NetworkManager.Instance.GetTeamController().RemoveTeammates(m_SelectVoUid);
                        Debug.Log("发送离开队伍踢人");
                    }
                }

            }
        }
    }

    /// <summary>
    /// `键按下 拉黑
    /// </summary>
    /// <param name="callbackContext">参数</param>
    public void OnBlockDown(HotkeyCallback callbackContext)
    {
        if (callbackContext.performed)
        {
            //add black
            if (m_SelectVoUid > 0)
            {
                NetworkManager.Instance.GetFriendController().RequestAddToBlackList(m_SelectVoUid);
                Debug.Log("发送add black========>>>>>" + m_SelectVoUid);
            }

        }
    }

    /// <summary>
    /// Tab 键按下 查看详情 添加好友
    /// </summary>
    /// <param name="callbackContext">参数</param>
    public void OnAddFriendDown(HotkeyCallback callbackContext)
    {
        if (callbackContext.performed)
        {
            //好友列表里
            if (m_SelectVoUid > 0)
            {
                if (m_FriendProxy.GetFriend(m_SelectVoUid) == null)
                {
                    //add Friend
                    NetworkManager.Instance.GetFriendController().RequestAddToFriendList(m_SelectVoUid);
                    Debug.Log("发送add Friend========>>>>>" + m_SelectVoUid);
                }
                else
                {
                    //查看详情
                   // NetworkManager.Instance.GetFriendController().RequestPlayerInfo(m_SelectVoUid);
                   // Debug.Log("发送查看详情========>>>>>" + m_SelectVoUid);
                }
            }

        }
    }
    /// <summary>
    /// 空格 键按下 私聊
    /// </summary>
    /// <param name="callbackContext">参数</param>
    public void OnChatDown(HotkeyCallback callbackContext)
    {
        if (callbackContext.performed)
        {

        }
    }

    #endregion

    /// <summary>
    /// 填充面板
    /// </summary>
    private void FillPanel()
    {
        //Debug.Log("刷新界面.................." + m_TeamProxy.GetMembersList().Count);
        int pageIndex = State.GetPageIndex();
        int filterIndex = State.GetPageCategoryIndex(State.GetPageIndex());
        m_SocialSubClass = (SocialSubClass)filterIndex;
        m_SocialType = (SocialType)pageIndex;
        List<FriendInfoVO> list = new List<FriendInfoVO>();
        List<FriendInfoVO> volistNearby = new List<FriendInfoVO>();
        ClearData();
        UIViewCategory category = State.GetPageCategoryData();

        switch (m_SocialType)
        {
            case SocialType.Team:
                m_TeamProxy.SortMembers();
                AddTeamDataToView(m_TeamProxy.GetMembersList());
               // SetRightLabelCapacity(0, m_TeamProxy.GetMembersList().Count, m_TeamProxy.MEMBERCOUNTLIMIT);
                break;
            case SocialType.Friend:
                if (category.IsAll)
                {
                    list.Clear();
                    UIViewPage page = State.GetPage();
                    for (int i = 0; i < page.Categorys.Length; i++)
                    {
                        if (page.Categorys[i].IsAll)
                            continue;
                        category = page.Categorys[i];
                        if ((SocialSubClass)category.Arguments[0] == SocialSubClass.First)
                        {
                            for (int j = 0; j < m_FriendProxy.GetFriendList().Count; j++)
                            {
                                if (m_FriendProxy.GetFriendList()[j].Status == FriendState.ONLINE)
                                {
                                    list.Add(m_FriendProxy.GetFriendList()[j]);
                                }
                            }
                            AddFriendDataToView(list, category.Label);
                            list.Clear();
                        }
                        else if ((SocialSubClass)category.Arguments[0] == SocialSubClass.Second)
                        {
                            for (int k = 0; k < m_FriendProxy.GetFriendList().Count; k++)
                            {
                                if (m_FriendProxy.GetFriendList()[k].Status == FriendState.LEAVE)
                                {
                                    list.Add(m_FriendProxy.GetFriendList()[k]);
                                }
                            }
                            AddFriendDataToView(list, category.Label);
                            list.Clear();
                        }
                    }
                }
                else
                {
                    list.Clear();
                    switch ((SocialSubClass)category.Arguments[0])
                    {
                        case SocialSubClass.First:
                            for (int i = 0; i < m_FriendProxy.GetFriendList().Count; i++)
                            {
                                if (m_FriendProxy.GetFriendList()[i].Status == FriendState.ONLINE)
                                {
                                    list.Add(m_FriendProxy.GetFriendList()[i]);
                                }
                            }
                            break;
                        case SocialSubClass.Second:
                            list.Clear();
                            for (int i = 0; i < m_FriendProxy.GetFriendList().Count; i++)
                            {
                                if (m_FriendProxy.GetFriendList()[i].Status == FriendState.LEAVE)
                                {
                                    list.Add(m_FriendProxy.GetFriendList()[i]);
                                }
                            }
                            break;
                        case SocialSubClass.Blacklist:
                            for (int i = 0; i < m_FriendProxy.GetBlackList().Count; i++)
                            {
                                list.Add(m_FriendProxy.GetBlackList()[i]);
                            }
                            break;
                        default:
                            break;
                    }
                    AddFriendDataToView(list, category.Label);
                }
                break;
            case SocialType.Ship:
                list.Clear();
                //SetRightLabelCapacity(2, 0, 0);
                //m_IsOpenTips = false;
                List<FriendInfoVO> listShip = new List<FriendInfoVO>();
                list = listShip;
                AddFriendDataToView(list, null);
               // UIManager.Instance.ClosePanel(PanelName.TipsFriendPanel);
               // SetActiveNullTips(true);
                break;
            case SocialType.Other:
                if (category.IsAll)
                {
                    list.Clear();
                    UIViewPage page = State.GetPage();
                    for (int i = 0; i < page.Categorys.Length; i++)
                    {
                        if (page.Categorys[i].IsAll)
                            continue;
                        category = page.Categorys[i];
                        if ((SocialSubClass)category.Arguments[0] == SocialSubClass.First)
                        {
                            volistNearby = m_FriendProxy.GetNearbyList();
                            for (int j = 0; j < volistNearby.Count; j++)
                            {
                                list.Add(volistNearby[j]);
                            }
                        }
                        else if ((SocialSubClass)category.Arguments[0] == SocialSubClass.Second)
                        {
                            for (int k = 0; k < m_FriendProxy.GetRecentList().Count; k++)
                            {
                                list.Add(m_FriendProxy.GetRecentList()[k]);
                            }
                        }
                        AddFriendDataToView(list, category.Label);
                        list.Clear();
                    }
                }
                else
                {
                    list.Clear();
                    switch ((SocialSubClass)category.Arguments[0])
                    {
                        case SocialSubClass.First:
                            volistNearby = m_FriendProxy.GetNearbyList();
                            for (int i = 0; i < volistNearby.Count; i++)
                            {
                                list.Add(volistNearby[i]);
                            }
                            break;
                        case SocialSubClass.Second:
                            list.Clear();
                            for (int i = 0; i < m_FriendProxy.GetRecentList().Count; i++)
                            {
                                list.Add(m_FriendProxy.GetRecentList()[i]);
                            }
                            break;
                        default:
                            break;
                    }
                    AddFriendDataToView(list, category.Label);

                }
                break;
            default:
                break;
        }
        //State.SetPageLabel(string.Format(GetLocalization("package_title_1008"), 0, 100));
        RefreshHotKey();
    }

    /// <summary>
	/// 界面添加好友数据
	/// </summary>
	/// <param name="title">标题</param>
	/// <param name="list">数据</param>
	/// <param name=""></param>
    public void AddFriendDataToView(List<FriendInfoVO> list, string title = null)
    {
        if (title == null)
        {
            AddDatas(null, list.ToArray());
        }
        else
        {
            AddDatas(TableUtil.GetLanguageString(title), list.ToArray());
        }
      
    }

    /// <summary>
	/// 界面添加队伍数据
	/// </summary>
	/// <param name="title">标题</param>
	/// <param name="list">数据</param>
	/// <param name=""></param>
    public void AddTeamDataToView(List<TeamMemberVO> list, string title = null)
    {
        List<object> listRef = new List<object>();
        if (list.Count == 0)//没有队伍
        {
            //Debug.Log("没有队伍啦");
            TeamMemberVO vo = new TeamMemberVO();
            vo.UID = m_ServerListProxy.GetCurrentCharacterVO().UId;
            vo.Level = m_ServerListProxy.GetCurrentCharacterVO().Level;
            vo.DanLevel= m_ServerListProxy.GetCurrentCharacterVO().DanLevel;
            vo.Name = m_ServerListProxy.GetCurrentCharacterVO().Name;
            vo.TID = m_ServerListProxy.GetCurrentCharacterVO().Tid;
            listRef.Add(vo);
            for (int i = 0; i < 4; i++)
            {
                TeamMemberVO teamMemberVO = new TeamMemberVO();
                teamMemberVO.UID = 0;
                listRef.Add(teamMemberVO);
            }
        }
        else
        {
            for (int i = 0; i < list.Count; i++)
            {
                listRef.Add((object)list[i]);
            }
            if (listRef.Count < 5)
            {
                while (listRef.Count < 5)
                {
                    TeamMemberVO teamMemberVO = new TeamMemberVO();
                    teamMemberVO.UID = 0;
                    listRef.Add(teamMemberVO);
                }
            }
        }
        if (title == null)
        {
            AddDatas(null, listRef);
        }
        else
        {
            AddDatas(title, listRef);
        }
    }

    /// <summary>
	/// 自己是否为队长
	/// </summary>
	/// <returns></returns>
	public bool IsCaptainBySelf()
    {
        if (m_TeamProxy.GetMember(m_ServerListProxy.GetCurrentCharacterVO().UId) == null)
        {
            return false;
        }
        return m_TeamProxy.GetMember(m_ServerListProxy.GetCurrentCharacterVO().UId).IsLeader;
    }

    /// <summary>
	/// 获取其他玩家的信息
	/// </summary>
	/// <param name="id"></param>
	private void GetShipData(ulong id)
    {
        //Debug.LogError("收到玩家信息"+id);
        ShipItemsProxy shipItemsProxy = GameFacade.Instance.RetrieveProxy(ProxyName.ShipItemsProxy) as ShipItemsProxy;
        // 飞船的
        IShip ship = shipItemsProxy.GetCurrentWarShip(id);
        if (ship != null)
        {
            FriendInfoVO vO = m_FriendProxy.GetFriend(id);
            if (vO != null)
            {
                MsgPlayerInfo msgPlayerInfo = new MsgPlayerInfo();
                msgPlayerInfo.Name = vO.Name;
                msgPlayerInfo.Tid = vO.TID;
                msgPlayerInfo.UID = vO.UID;
                msgPlayerInfo.Ship = ship;
                m_LeftSelectIndex = (int)m_SocialSubClass;

                //OwnerView.OpenChildPanel(UIPanel.EscWatchPanel, msgPlayerInfo);
            }
        }
        else
        {
            Debug.Log("船的信息为空");
        }
    }

    #region 枚举
    /// <summary>
    /// 社交类型
    /// </summary>
    public enum SocialType
    {
        /// <summary>
        /// 队伍
        /// </summary>
        Team,
        /// <summary>
        /// 好友
        /// </summary>
        Friend,
        /// <summary>
        /// 舰队
        /// </summary>
        Ship,
        /// <summary>
        /// 其他
        /// </summary>
        Other
    }
    /// <summary>
    /// 社交子类型按钮类型
    /// </summary>
    public enum SocialSubClass
    {
        /// <summary>
        /// 全部
        /// </summary>
        All,
        /// <summary>
        /// 首位(在线，附近的人)
        /// </summary>
        First,
        /// <summary>
        /// 离线，最近玩过的
        /// </summary>
        Second,
        /// <summary>
        /// 黑名单
        /// </summary>
        Blacklist
    }
    #endregion
}
