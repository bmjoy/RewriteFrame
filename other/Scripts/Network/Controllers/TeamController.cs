using Assets.Scripts.Define;
using Assets.Scripts.Lib.Net;
using Assets.Scripts.Proto;
using Game.Frame.Net;
using System.Collections.Generic;
using UnityEngine;
public class TeamController : AbsRpcController
{
	/// <summary>
	/// 队伍proxy
	/// </summary>
	private TeamProxy m_TeamProxy;

	public TeamController() : base()
    {  
		NetworkManager.Instance.ListenGameServer(KS2C_Protocol.s2c_team_member_leave, OnMemberLeave, typeof(S2C_TEAM_MEMBER_LEAVE));//离队
		NetworkManager.Instance.ListenGameServer(KS2C_Protocol.s2c_team_bechairman, OnLeaderChange, typeof(S2C_TEAM_BECHAIRMAN));//转让队长
		NetworkManager.Instance.ListenGameServer(KS2C_Protocol.s2c_team_member_forceleave, OnForcedToleaveTeam, typeof(S2C_TEAM_MEMBER_FORCELEAVE));//队长踢人
		NetworkManager.Instance.ListenGameServer(KS2C_Protocol.s2c_team_member_list, OnGetTeamList, typeof(S2C_TEAM_MEMBER_LIST));//好友列表
	    NetworkManager.Instance.ListenGameServer(KS2C_Protocol.s2c_team_member_added, OnMemberAdded, typeof(S2C_TEAM_MEMBER_ADDED));//添加成员
		NetworkManager.Instance.ListenGameServer(KS2C_Protocol.s2c_team_dissove, OnTeamDissove, typeof(S2C_TEAM_DISSOVE));//队伍解散
		NetworkManager.Instance.ListenGameServer(KS2C_Protocol.s2c_team_member_change, OnMemberChanged, typeof(S2C_TEAM_MEMBER_CHANGE));//成员改变
		NetworkManager.Instance.ListenGameServer(KS2C_Protocol.s2c_team_create, OnTeamCreate, typeof(S2C_TEAM_CREATE));//创建队伍
		NetworkManager.Instance.ListenGameServer(KS2C_Protocol.s2c_notify_memberbattleinfo_list, MemberBattleInfo, typeof(S2C_NOTIFY_MEMBERBATTLEINFO_LIST));//同步战斗信息
	}

	/// <summary>
	/// 获取TeamProxy
	/// </summary>
	/// <returns>TeamProxy</returns>
	private TeamProxy GetTeamProxy()
    {
		return m_TeamProxy == null ? m_TeamProxy = GameFacade.Instance.RetrieveProxy(ProxyName.TeamProxy) as TeamProxy : m_TeamProxy;
    }

	/// <summary>
	/// 获取SeverListProxy
	/// </summary>
	/// <returns>TeamProxy</returns>
	private ServerListProxy GetSeverListProxy()
	{
		return GameFacade.Instance.RetrieveProxy(ProxyName.ServerListProxy) as ServerListProxy;
	}
	#region S2C

	/// <summary>
	/// 创建队员数据
	/// </summary>
	/// <param name="msg">协议数据</param>
	/// <returns>队员数据</returns>
	private TeamMemberVO CreateMember(TeamMember msg)
    {
        TeamMemberVO member = new TeamMemberVO();
        member.UID = msg.id;
		member.TID = msg.tempid;
		member.Name = msg.name;
		Debug.Log(msg.position+"新成员名字===" + msg.name + "状态=="+msg.isOnline+"TID=="+ msg.tempid + "死亡=="+ msg.alive);
        Debug.Log(msg.areaid + "区域===地图" + msg.mapid+"===等级"+msg.level+"段位==="+msg.dan_level);
        member.Level = msg.level;
        member.DanLevel = msg.dan_level;
        member.IsOnline = msg.isOnline == 1;
        member.IsLeader = msg.position == 1;
        member.JoinTime = msg.jointime;
		member.IsDead = msg.alive == 0;
        member.AreaId = msg.areaid;
        member.MapID = msg.mapid;
        return member;
    }

	/// <summary>
	/// 队员离开
	/// </summary>
	/// <param name="buf">协议内容</param>
	private void OnMemberLeave(KProtoBuf buf)
    {
        S2C_TEAM_MEMBER_LEAVE msg = buf as S2C_TEAM_MEMBER_LEAVE;

		if (msg.id == GetSeverListProxy().GetCurrentCharacterVO().UId)
		{
			Debug.Log("自己被踢出222");
			TeamProxy teamProxy = GetTeamProxy();
			teamProxy.SetCaptainID(0);
			teamProxy.UpdateMember(null);
			GameFacade.Instance.SendNotification(NotificationName.MSG_TEAM_MEMBER_UPDATE);
		}
		else
		{
			GetTeamProxy().DelMember(msg.id);
		}
	}

    /// <summary>
    /// 自已被离队
    /// </summary>
    /// <param name="buf">协议内容</param>
    private void OnForcedToleaveTeam(KProtoBuf buf)
    {
        S2C_TEAM_MEMBER_FORCELEAVE msg = buf as S2C_TEAM_MEMBER_FORCELEAVE;
		Debug.Log("自己被踢出");
        TeamProxy teamProxy = GetTeamProxy();
        teamProxy.SetCaptainID(0);
		teamProxy.UpdateMember(null);
		GameFacade.Instance.SendNotification(NotificationName.MSG_TEAM_MEMBER_UPDATE);
	}

	/// <summary>
	/// 获取队伍列表
	/// </summary>
	/// <param name="buf">协议内容</param>
	private void OnGetTeamList(KProtoBuf buf)
	{
		S2C_TEAM_MEMBER_LIST msg = buf as S2C_TEAM_MEMBER_LIST;
		TeamProxy teamProxy = GetTeamProxy();
		for (int i = 0; i < msg.members.Count; i++)
		{
			TeamMemberVO data = CreateMember(msg.members[i]);
			teamProxy.AddMember(data);
		}
		GameFacade.Instance.SendNotification(NotificationName.MSG_TEAM_MEMBER_UPDATE);

	}

	/// <summary>
	/// 队长已切换
	/// </summary>
	/// <param name="buf">协议内容</param>
	private void OnLeaderChange(KProtoBuf buf)
    {
        S2C_TEAM_BECHAIRMAN msg = buf as S2C_TEAM_BECHAIRMAN;

        GetTeamProxy().SwitchingCaptain(msg.chair_man_id);
	}
	

	/// <summary>
	/// 同步队伍战斗信息
	/// </summary>
	/// <param name="buf">协议内容</param>
	private void MemberBattleInfo(KProtoBuf buf)
	{
		S2C_NOTIFY_MEMBERBATTLEINFO_LIST msg = buf as S2C_NOTIFY_MEMBERBATTLEINFO_LIST;
		if (GetTeamProxy().GetMember(msg.members.id) != null)
		{
			GetTeamProxy().GetMember(msg.members.id).Name = msg.members.name;
			GetTeamProxy().GetMember(msg.members.id).HP = msg.members.blood;
			GetTeamProxy().GetMember(msg.members.id).MaxHP = msg.members.bloodMax;
			GetTeamProxy().GetMember(msg.members.id).Defense = msg.members.defenseMax;
			GetTeamProxy().GetMember(msg.members.id).MaxDefense = msg.members.defense;
			GetTeamProxy().GetMember(msg.members.id).IsDead = msg.members.isAlive == 0 ? true : false;
			//if (GetTeamProxy().GetMember(msg.members.id).HP > 0)
			//{
			//	GetTeamProxy().GetMember(msg.members.id).IsDead = false;
			//}
			//else
			//{
			//	GetTeamProxy().GetMember(msg.members.id).IsDead = true;
			//}
			//Debug.LogError(msg.members.name + "GetTeamProxy().GetMember(msg.members.id).IsDead"+ msg.members.isAlive);
			GameFacade.Instance.SendNotification(NotificationName.MSG_TEAM_BATTLE_UPDATE, msg.members.id);
			//Debug.LogError(GetTeamProxy().GetMember(msg.members.id).HP + "收到血量+=====" + GetTeamProxy().GetMember(msg.members.id).HP);
		}
		else
		{
			Debug.Log(msg.members.id+"队伍不包括这个人" + msg.members.name);
		}
		//GetTeamProxy().SwitchingCaptain(msg.chair_man_id);
	}
	#region 

	/// <summary>
	/// 队员状态改变
	/// </summary>
	/// <param name="buf">协议内容</param>
	private void OnMemberChanged(KProtoBuf buf)
	{
		S2C_TEAM_MEMBER_CHANGE msg = buf as S2C_TEAM_MEMBER_CHANGE;

		GetTeamProxy().SetMember(msg.members.id, msg.members.level, msg.members.isOnline != 0, true,
            msg.members.position == 1, msg.members.areaid, msg.members.mapid,msg.members.dan_level);
		//GameFacade.Instance.SendNotification(NotificationName.MSG_TEAM_MEMBER_UPDATE);
	}

	/// <summary>
	/// 队伍已创建
	/// </summary>
	/// <param name="buf">协议内容</param>
	private void OnTeamCreate(KProtoBuf buf)
	{
		S2C_TEAM_CREATE msg = buf as S2C_TEAM_CREATE;
	}

	/// <summary>
	/// 队伍已解散
	/// </summary>
	/// <param name="buf">协议内容</param>
	private void OnTeamDissove(KProtoBuf buf)
	{
		S2C_TEAM_DISSOVE msg = buf as S2C_TEAM_DISSOVE;

		TeamProxy teamProxy = GameFacade.Instance.RetrieveProxy(ProxyName.TeamProxy) as TeamProxy;
		teamProxy.SetCaptainID(0);
		teamProxy.UpdateMember(null);
		GameFacade.Instance.SendNotification(NotificationName.MSG_TEAM_MEMBER_UPDATE);

	}

	/// <summary>
	/// 队员加入
	/// </summary>
	/// <param name="buf">协议内容</param>
	private void OnMemberAdded(KProtoBuf buf)
	{
		S2C_TEAM_MEMBER_ADDED msg = buf as S2C_TEAM_MEMBER_ADDED;
		TeamProxy teamProx = GameFacade.Instance.RetrieveProxy(ProxyName.TeamProxy) as TeamProxy;

		teamProx.AddMember(CreateMember(msg.members));
		teamProx.UpdateLeaderID();
		GameFacade.Instance.SendNotification(NotificationName.MSG_TEAM_MEMBER_ADDED);
		GameFacade.Instance.SendNotification(NotificationName.MSG_TEAM_MEMBER_UPDATE);
	}

	/// <summary>
	/// 处理队伍错误
	/// </summary>
	/// <param name="buf">协议内容</param>
	private void OnErrorCode(KProtoBuf buf)
    {
        //S2C_ERROR_CODE msg = buf as S2C_ERROR_CODE;

        //CfgLanguageProxy language = GameFacade.Instance.RetrieveProxy(ProxyName.CfgLanguageProxy) as CfgLanguageProxy;

        //string str = string.Empty;
        //switch ((KErrorCode)msg.error)
        //{
        //    case KErrorCode.errSendTeamOnLineMsg:
        //        //玩家组队不在线
        //        str = language.GetLocalization(100031);
        //        break;
        //    case KErrorCode.errSendHasTeamMsg:
        //        //玩家已经有队伍
        //        str = language.GetLocalization(100032);
        //        break;
        //    case KErrorCode.errSendTeamInviteLimited:
        //        //玩家达到最大邀请个数
        //        str = language.GetLocalization(100032);
        //        break;
        //    case KErrorCode.errSendTeamInvited:
        //        //玩家已經邀请过
        //        str = language.GetLocalization(100033);
        //        break;
        //    case KErrorCode.errInviteTeamWithNoShip:
        //        //组队邀请者没有出站船
        //        str = language.GetLocalization(100034);
        //        break;
        //    case KErrorCode.errInvitedTeamWithNoShip:
        //        //组队被邀请者没有出站船
        //        str = language.GetLocalization(100035);
        //        break;
        //    case KErrorCode.errTeamNumberMaxLimited:
        //        //队伍人数达到最大上限
        //        str = language.GetLocalization(100036);
        //        break;
        //    case KErrorCode.errTeamPriForLeaderAsMatch:
        //        //匹配禁止野队队长所有权限
        //        str = language.GetLocalization(100037);
        //        break;
        //    case KErrorCode.errTeamInviteInSence:
        //        //副本野队不许组队
        //        str = language.GetLocalization(100038);
        //        break;
        //}

        //if (string.IsNullOrEmpty(str))
        //{
        //    MsgTeamErrorInfo message = MessageSingleton.Get<MsgTeamErrorInfo>();
        //    message.m_Error = str;

        //    GameFacade.Instance.SendNotification(NotificationName.TeamErrorInfo, message);
        //}
    }
	#endregion

	#endregion

	#region C2S

	/// <summary>
	/// 邀请玩家组队
	/// </summary>
	/// <param name="playerID">玩家ID</param>
	public void Invite(ulong playerID)
    {
		Debug.Log("邀请======>"+playerID);
        CfgLanguageProxy languageProxy = GameFacade.Instance.RetrieveProxy(ProxyName.CfgLanguageProxy) as CfgLanguageProxy;
        TeamMemberVO member = GetTeamProxy().GetMember(playerID);
        if (member != null)
        {
			//已在队伍里
			Debug.Log("已在队伍里");
            return;
        }

        C2S_TEAM_INVITE msg = new C2S_TEAM_INVITE();
        msg.protocolID = (ushort)KC2S_Protocol.c2s_team_invite;
        msg.playerID = playerID;
        NetworkManager.Instance.SendToGameServer(msg);
    }

    /// <summary>
    /// 队长转让
    /// </summary>
    /// <param name="playerID">玩家ID</param>
    public void SwitchingCaptain(ulong playerID)
    {
		Debug.Log("转让======>" + playerID);
		C2S_TEAMLEADER_CHANGE msg = new C2S_TEAMLEADER_CHANGE();
        msg.protocolID = (ushort)KC2S_Protocol.c2s_teamleader_change;
        msg.change_to_playerid = playerID;
        NetworkManager.Instance.SendToGameServer(msg);
    }

    /// <summary>
    /// 踢出队员
    /// </summary>
    /// <param name="playerID">玩家ID</param>
    public void RemoveTeammates(ulong playerID)
    {
		Debug.Log("踢出======>" + playerID);
		C2S_TEAM_LEAVE_BYLEADR msg = new C2S_TEAM_LEAVE_BYLEADR();
        msg.protocolID = (ushort)KC2S_Protocol.c2s_team_leave_byleadr;
        msg.remove_playerid = playerID;
        NetworkManager.Instance.SendToGameServer(msg);
    }

    /// <summary>
    /// 离开队伍
    /// </summary>
    public void LeaveTeam()
    {
		Debug.Log("离开======>");
		C2S_TEAM_LEAVE msg = new C2S_TEAM_LEAVE();
        msg.protocolID = (ushort)KC2S_Protocol.c2s_team_leave;
		NetworkManager.Instance.SendToGameServer(msg);
		TeamProxy teamProxy = GetTeamProxy();
        teamProxy.SetCaptainID(0);
		teamProxy.UpdateMember(null);
    }

	/// <summary>
	/// 请求队伍列表
	/// </summary>
	public void GetTeamList()
	{
		C2S_TEAMLIST_REQUEST msg = new C2S_TEAMLIST_REQUEST();
		msg.protocolID = (ushort)KC2S_Protocol.c2s_teamlist_request;
		NetworkManager.Instance.SendToGameServer(msg);
	}
	#endregion
}
