using PureMVC.Patterns.Proxy;
using System;
using System.Collections.Generic;
using UnityEngine;

public class TeamProxy : Proxy
{
	/// <summary>
	/// 本地存储KEY
	/// </summary>
    private const string HISTORY_KEY = "HistoryTeamMember_";

	/// <summary>
	/// 最近组队：显示最多30个玩家
	/// </summary>
	public int HISTORY_COUNT = 50;

	/// <summary>
	/// 队伍成员人数上限
	/// </summary>
    public int MEMBERCOUNTLIMIT = 5;

	/// <summary>
	/// 队长
	/// </summary>
	private ulong m_CurrentCaptainID;

    /// <summary>
    /// 成员列表
    /// </summary>
    private List<TeamMemberVO> m_Members = new List<TeamMemberVO>();

	public TeamProxy() : base(ProxyName.TeamProxy) { }

	/// <summary>
	/// 主角自己是否在队伍中
	/// </summary>
	/// <returns></returns>
	public bool IsInTeam()
	{
		return m_Members != null && m_Members.Count > 0;
	}

	/// <summary>
	/// 获取队长ID
	/// </summary>
	/// <returns></returns>
	public void SetCaptainID(ulong uid)
	{
		m_CurrentCaptainID = uid;
	}

	/// <summary>
	/// 查找成员
	/// </summary>
	/// <param name="playerID">队员ID</param>
	/// <returns>队员数据</returns>
	public TeamMemberVO GetMember(ulong playerID)
	{
		for (int i = 0; i < m_Members.Count; i++)
		{
			if (m_Members[i].UID == playerID)
			{
				return m_Members[i];
			}
		}
		return null;
	}

	/// <summary>
	/// 成员列表
	/// </summary>
	/// <returns>队伍成员列表</returns>
	public List<TeamMemberVO> GetMembersList()
    {
        return m_Members;
    }

	/// <summary>
	/// 更新成员列表
	/// </summary>
	/// <param name="members">新的队员列表</param>
    public void UpdateMember(List<TeamMemberVO> members)
	{		
        m_Members.Clear();
		m_CurrentCaptainID = 0;
		if (members != null)
        {
            for (int i = 0; i < members.Count; i++)
            {
                TeamMemberVO member = members[i];

                m_Members.Add(member);
            }
        }
		UpdateLeaderID();
		GameFacade.Instance.SendNotification(NotificationName.MSG_TEAM_MEMBER_UPDATE);
    }

	/// <summary>
	/// 更新队长ID
	/// </summary>
	public void UpdateLeaderID()
	{
		ulong old = m_CurrentCaptainID;
		for (int t = 0; t < m_Members.Count; t++)
		{
			TeamMemberVO member = m_Members[t];
			if(member.IsLeader)
			{
				m_CurrentCaptainID = member.UID;
				break;
			}
		}
	}

    /// <summary>
    /// 添加成员
    /// </summary>
    /// <param name="member">新的队伍成员</param>
    public void AddMember(TeamMemberVO member)
    {
        for (int i = m_Members.Count - 1; i >= 0; i--)
        {
            if (m_Members[i].UID == member.UID)
            {
				member.Name = m_Members[i].Name;
				member.HP = m_Members[i].HP;
				member.MaxHP = m_Members[i].MaxHP;
				member.Defense = m_Members[i].Defense;
				member.MaxDefense = m_Members[i].MaxDefense;
				//member.IsDead = m_Members[i].IsDead ;

				m_Members.RemoveAt(i);
			}
        }

		m_Members.Add(member);

	}

	/// <summary>
	/// 删除成员
	/// </summary>
	/// <param name="playerID">队员ID</param>
	public void DelMember(ulong playerID)
    {
        TeamMemberVO member = null;
        for (int i = m_Members.Count - 1; i >= 0; i--)
        {
            if (m_Members[i].UID == playerID)
            {
                member = m_Members[i];
				m_Members.RemoveAt(i);
				
			}
        }

		GameFacade.Instance.SendNotification(NotificationName.MSG_TEAM_MEMBER_UPDATE);
    }

    /// <summary>
    /// 改变成员
    /// </summary>
    /// <param name="id">队员ID</param>
    /// <param name="level">等级</param>
    /// <param name="isOnline">是否在线</param>
    /// <param name="isReady">是否准备好了</param>
    /// <param name="isLeader">是否为队长</param>
    public void SetMember(ulong id, int level, bool isOnline, bool isReady, bool isLeader,int areaId,ulong mapId, int danLevel)
    {
        TeamMemberVO member = GetMember(id);
        if (member != null)
        {
            member.Level = level;
            member.DanLevel = danLevel;
            member.IsOnline = isOnline;
            member.IsLeader = isLeader;
            member.AreaId = areaId;
            member.MapID = mapId;
            GameFacade.Instance.SendNotification(NotificationName.MSG_TEAM_MEMBER_UPDATE, member);
            GameFacade.Instance.SendNotification(NotificationName.MSG_TEAM_MEMBER_UPDATEAREA, member);
        }
	}

	/// <summary>
	/// 切换队长
	/// </summary>
	public void SwitchingCaptain(ulong CurrentID)
	{
        TeamMemberVO OldCaptain = null;

        if (m_CurrentCaptainID > 0)
		{
			if (GetMember(m_CurrentCaptainID) != null)
			{
                OldCaptain = GetMember(m_CurrentCaptainID);
				OldCaptain.IsLeader = false;
			}
		}

		if (CurrentID > 0)
		{
			for (int i = 0; i < m_Members.Count; i++)
			{
				m_Members[i].IsLeader = false;
			}
		
			TeamMemberVO CurrCaptain = GetMember(CurrentID);
            if (CurrCaptain != null)
            {
                CurrCaptain.IsLeader = true;
                m_CurrentCaptainID = CurrentID;
            }
            else
            {
                if (OldCaptain!=null)
                    OldCaptain.IsLeader = true;
            }
			
		}

		GameFacade.Instance.SendNotification(NotificationName.MSG_TEAM_MEMBER_UPDATE);
	}

	/// <summary>
	/// 排序成员
	/// </summary>
	public void SortMembers()
    {
        m_Members.Sort(
            (TeamMemberVO vo1, TeamMemberVO vo2) => 
            {
                if(vo1.IsLeader && !vo2.IsLeader)
                {
                    return -1;
                }
                else if(!vo1.IsLeader && vo2.IsLeader)
                {
                    return 1;
                }
                else if (vo1.JoinTime < vo2.JoinTime)
                {
                    return -1;
                }
                else if (vo1.JoinTime > vo2.JoinTime)
                {
                    return 1;
                }
                return 0;
            }
        );
    }


    #region 邀请

    /// <summary>
    /// 邀请
    /// </summary>
    /// <param name="playerID"></param>
    public void Invite(ulong playerID)
    {
		GameFacade.Instance.SendNotification(NotificationName.CMD_TEAM_INVITE, playerID);
    }

    /// <summary>
    /// 添加邀请信息
    /// </summary>
    /// <param name="info"></param>
    public void AddInviteInfo(TeamInviteVO info)
    {
        if (IsInTeam())
            return;
		
        string strr = "sssssssss not found";
        string ContentText = string.Format(strr, info.FromPlayerName);
        float ScreenWidth = Screen.width;
        float ScreenHeight = Screen.height;
        Vector3 viewPos = Vector3.zero;
        viewPos.y = -(ScreenHeight * 0.5f - 200f); 

    }

	/// <summary>
	/// 同意时
	/// </summary>
	/// <param name="param"></param>
    private void OnOKClick(object param)
    {
		GameFacade.Instance.SendNotification(NotificationName.CMD_TEAM_INVITE_REPLY, new object[] { param, true });
    }

	/// <summary>
	/// 拒绝时
	/// </summary>
	/// <param name="param"></param>
    private void OnNoClick(object param)
    {
		GameFacade.Instance.SendNotification(NotificationName.CMD_TEAM_INVITE_REPLY, new object[] { param, false });
    }

    /// <summary>
    /// 回复邀请
    /// </summary>
    /// <param name="playerID"></param>
    /// <param name="accept"></param>
    public void InviteReply(ulong playerID, bool accept)
    {
		GameFacade.Instance.SendNotification(NotificationName.CMD_TEAM_INVITE_REPLY, new object[] { playerID, accept });
    }

    /// <summary>
    /// 收到回复邀请
    /// </summary>
    public void OnInviteReply(ulong playerID,string playerName,bool accept)
    {
		GameFacade.Instance.SendNotification(NotificationName.MSG_TEAM_INVITE_REPLY, new object[] { playerID, playerName, accept });
    }

	#endregion

	public void OnError(int code, params string[] arguments)
    {
        Debug.LogWarningFormat("TEAM_ERROR : {0}", code);

        /*
        TEAM_ERROR_INFO_1 = 您不是队长
        TEAM_ERROR_INFO_2 = 您已经有队伍了
        TEAM_ERROR_INFO_3 =< color =#fcc32c>{0}</color>已经有队伍了
        TEAM_ERROR_INFO_4 =< color =#fcc32c>{0}</color>已离线
        TEAM_ERROR_INFO_5 = 队伍不存在
        TEAM_ERROR_INFO_6 = 加入队伍失败,队伍人数已满!
        TEAM_ERROR_INFO_7 = 加入队伍失败,该队伍需要邀请才能加入!
        TEAM_ERROR_INFO_8 = 加入队伍失败,等级低于队长 < color =#ff0000>5</color>级以上
        TEAM_ERROR_INFO_9 = 加入队伍失败,等级高于队长 < color =#ff0000>5</color>级以上
        TEAM_ERROR_INFO_10 =< color =#fcc32c>{0}</color>拒绝了你的组队邀请
        TEAM_ERROR_INFO_11 =< color =#fcc32c>{0}</color>拒绝了你的组队申请
        TEAM_ERROR_INFO_12 = 您邀请的玩家在副本中,无法接受邀请
        TEAM_ERROR_INFO_13 =< color =#fcc32c>{0}</color>的等级太低，无法邀请
        TEAM_ERROR_INFO_14 =< color =#fcc32c>{0}</color>的等级太高，无法邀请
        TEAM_ERROR_INFO_20 =< color =#ff0000>您已经在副本中</color>
        TEAM_ERROR_INFO_21 = 您正在参加活动
        TEAM_ERROR_INFO_22 =< color =#fcc32c>{0}</color>所处的状态无法进入副本
        TEAM_ERROR_INFO_23 =< color =#fcc32c>{0}</color>正在参加活动
        TEAM_ERROR_INFO_24 =< color =#fcc32c>{0}</color>等级小于<color=#ff0000>{1}</color>级，无法进入组队战役
        TEAM_ERROR_INFO_30 =< color =#fcc32c>{0}</color>不是本军盟成员，不能进入同一军盟副本
        TEAM_ERROR_INFO_31 =< color =#fcc32c>{0}</color>等级小于<color=#ff0000>{1}</color>级，无法进入军盟副本
        TEAM_ERROR_INFO_32 =< color =#fcc32c>{0}</color>不满足进入军盟24小时以上，无法进入军盟副本
        TEAM_ERROR_INFO_40 = 队员：{ 0} 拒绝了进入讨伐群雄副本
        */
    }
}
