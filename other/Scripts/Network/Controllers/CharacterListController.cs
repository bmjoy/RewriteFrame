using Assets.Scripts.Define;
using Assets.Scripts.Lib.Net;
using Assets.Scripts.Proto;
using Game.Frame.Net;
using System;
using System.Collections.Generic;
using UnityEngine;
using static CharacterRolePanel;

public class CharacterListController : BaseNetController
{
	private ServerListProxy m_ServerListProxy;
	private ServerListProxy GetServerListProxy()
	{
		if (m_ServerListProxy == null)
		{
			m_ServerListProxy = GameFacade.Instance.RetrieveProxy(ProxyName.ServerListProxy) as ServerListProxy;
		}
		return m_ServerListProxy;
	}



	/// <summary>
	/// 角色ID列表
	/// </summary>
	private List<ulong> m_RoleIDList = new List<ulong>();

	/// <summary>
	/// ..........
	/// </summary>
	public bool m_IsPlayerEnterScene = false;

	public CharacterListController()
	{
		//Socket事件
		NetworkManager.Instance._GetSocketClient().OnConnected += OnConnected;
		NetworkManager.Instance._GetSocketClient().OnDisconnected += OnDisConnect;

		//角色创建
		NetworkManager.Instance.ListenGateway(KG2C_Protocol.g2c_query_role_id_list_respond, OnQueryRoleIdList, typeof(KG2C_QueryIDListRespond));
		NetworkManager.Instance.ListenGateway(KG2C_Protocol.g2c_sync_role_info, OnSyncRoleBaseInfo, typeof(KG2C_SyncRoleInfo));
		NetworkManager.Instance.ListenGateway(KG2C_Protocol.g2c_role_name_respond, OnRollRoleNameResponse, typeof(KG2C_RoleNameRespond));
		NetworkManager.Instance.ListenGateway(KG2C_Protocol.g2c_create_role_respond, OnCreateRoleRespond, typeof(KG2C_CreateRoleRespond));
		NetworkManager.Instance.ListenGateway(KG2C_Protocol.g2c_delete_role_respond, OnDeleteRoleRespond, typeof(KG2C_DeleteRoleRespond));
	}


	#region 监视Socket状态

	/// <summary>
	/// 连接服务器成功
	/// </summary>
	/// <param name="objs"></param>
	/// <returns></returns>
	private void OnConnected()
	{
		//roleIDList.Clear();
		//DataManager.GetProxy<CharacterListProxy>().Clear();
	}

	/// <summary>
	/// 服务器连接断开
	/// </summary>
	/// <param name="objs"></param>
	/// <returns></returns>
	private void OnDisConnect()
	{
		//roleIDList.Clear();
		//DataManager.GetProxy<CharacterListProxy>().Clear();
	}

	#endregion


	#region "角色创建"


	/// <summary>
	/// 收到帐号的角色ID列表
	/// </summary>
	/// <param name="proto"></param>
	private void OnQueryRoleIdList(KProtoBuf proto)
	{
		KG2C_QueryIDListRespond roleIDRespond = proto as KG2C_QueryIDListRespond;

		string serverId = GetServerListProxy().GetLastLoginServer()?.Gid ?? string.Empty;
		GetServerListProxy().ClearServerCharacters(serverId);

		m_RoleIDList.Clear();
		for (int i = 0; i < roleIDRespond.roleIdList.Count; i++)
		{
			m_RoleIDList.Add(roleIDRespond.roleIdList[i]);
			//Debug.LogError("Re add role id "+ roleIDRespond.roleIdList[i]);
		}

		if (m_RoleIDList.Count <= 0)
		{
			GetServerListProxy().SendNotification(NotificationName.MSG_CHARACTER_LIST_GETED);
		}
	}

	/// <summary>
	/// 收到帐号里的角色信息列表
	/// </summary>
	/// <param name="proto"></param>
	private void OnSyncRoleBaseInfo(KProtoBuf proto)
	{
		string serverId = GetServerListProxy().GetLastLoginServer()?.Gid ?? string.Empty;

		KG2C_SyncRoleInfo msg = proto as KG2C_SyncRoleInfo;

		ulong roleID = msg.uRoleID;
		if (m_RoleIDList.Contains(roleID))
		{
			m_RoleIDList.Remove(roleID);

			ulong id = msg.uRoleID;
			string name = msg.szRoleName;
			int tid = msg.nMainHeroTemplateID;
			ushort level = msg.uLevel;
			int loginTime = msg.nLastLoginTime;
			float deleteTime = msg.nDelCountDownTime > 0 ? Time.time + msg.nDelCountDownTime : 0;
			byte[] extData = msg.exData;
            ushort rank = 1;

            GetServerListProxy().AddCharacter(false, serverId, id, tid, name, level, (ulong)loginTime, 0, rank);
			//Debug.LogError("Re addRole");
		}

		if (m_RoleIDList.Count > 0)
		{
			//继续等角色信息..
			return;
		}

		if (m_IsPlayerEnterScene)
		{
			//OnLoginGameRequest(PlayerManager.GetInstance().MajorPlayer.PlayerID);
			throw new Exception(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>");
		}
		else
		{
			//Debug.LogError("Re addRoledddddddd");
			GetServerListProxy().SendNotification(NotificationName.MSG_CHARACTER_LIST_GETED);
		}

	}

	/// <summary>
	/// 随机一个角色名
	/// </summary>
	/// <param name="eGender"></param>
	public void RollRoleName(int eGender)
	{
		KC2G_Role_Name_Request request = SingleInstanceCache.GetInstanceByType<KC2G_Role_Name_Request>();
		request.byProtocol = (byte)KC2G_Protocol.c2g_role_name_request;
		request.eGender = (byte)eGender;
		NetworkManager.Instance.SendToGatewayServer(request);
	}

	/// <summary>
	/// 随机角色名返回
	/// </summary>
	/// <param name="buf"></param>
	private void OnRollRoleNameResponse(KProtoBuf buf)
	{
		KG2C_RoleNameRespond respond = buf as KG2C_RoleNameRespond;
		string roleName = respond.szRoleName.Replace("\0", "");
		if (roleName.Length <= 0)
		{
			GameFacade.Instance.SendNotification(NotificationName.MSG_CHARACTER_ROLLNAME_FAIL);
			return;
		}
		GameFacade.Instance.SendNotification(NotificationName.MSG_CHARACTER_ROLLNAME_SUCC, roleName);
	}

	/// <summary>
	/// 创建角色请求
	/// </summary>
	/// <param name="nickname"></param>
	/// <param name="gender"></param>
	/// <param name="nMainHeroTemplateID"></param>
	public void CreateRole(string nickname, int nMainHeroTemplateID)
	{
		KC2G_CreateRoleRequest request = SingleInstanceCache.GetInstanceByType<KC2G_CreateRoleRequest>();
		request.byProtocol = (byte)KC2G_Protocol.c2g_create_role_request;
		request.szRoleName = nickname;
		request.nMainHeroTemplateID = nMainHeroTemplateID;
		NetworkManager.Instance.SendToGatewayServer(request);

		Debug.Log("请求创建角色:" + nickname);
	}

	/// <summary>
	/// 创建角色返回
	/// </summary>
	private void OnCreateRoleRespond(KProtoBuf proto)
	{
		KG2C_CreateRoleRespond respond = proto as KG2C_CreateRoleRespond;
		KCreateRoleRespondCode code = (KCreateRoleRespondCode)(respond.code);

		if (code == KCreateRoleRespondCode.eCreateRoleSucceed)
		{
			ServerInfoVO server = GetServerListProxy().GetSelectedServer() ?? GetServerListProxy().GetLastLoginServer();
			GetServerListProxy().AddCharacter(true, server.Gid,
												   respond.uRoleID,
												   respond.nMainHeroTemplateID,
												   respond.szRoleName,
												   respond.uLevel, 0, 0,1);
			Debug.Log("创建角色成功  GID:" + server.Gid + "/ id" + +server.Id + " / " + respond.szRoleName);
			GetServerListProxy().GetCurrentCharacterVO().UId = respond.uRoleID;
			GetServerListProxy().GetCurrentCharacterVO().Name = respond.szRoleName;
			GetServerListProxy().GetCurrentCharacterVO().Level = respond.uLevel;
            GetServerListProxy().GetCurrentCharacterVO().DanLevel = respond.uLevel;
            PlayerPrefs.SetString(LAST_LOGIN_ROLE, respond.uRoleID.ToString());
			GameFacade.Instance.SendNotification(NotificationName.MSG_CHARACTER_CREATE_SUCCESS, respond.uRoleID);
		}
		else
		{
			Debug.Log("创建角色失败=" + respond.code);

			//EventDispatcher.GameWorld.DispatchEvent(ControllerCommand.CREATE_ROLE_FAILD);
			GameFacade.Instance.SendNotification(NotificationName.MSG_CHARACTER_CREATE_FAIL, (int)(proto as KG2C_CreateRoleRespond).code);

			string errTex = "";
			if (code == KCreateRoleRespondCode.eCreateRoleSucceed)
			{
				errTex = "NICKNAME_CREATE_SUCCESS";
			}
			else if (code == KCreateRoleRespondCode.eCreateRoleNameAlreadyExist)
			{
				errTex = "NICKNAME_USED";
			}
			else if (code == KCreateRoleRespondCode.eCreateRoleInvalidRoleName)
			{
				errTex = "NICKNAME_INVALID";
			}
			else if (code == KCreateRoleRespondCode.eCreateRoleNameTooLong)
			{
				errTex = "NICKNAME_TOOLONG";
			}
			else if (code == KCreateRoleRespondCode.eCreateRoleNameTooShort)
			{
				errTex = "NICKNAME_TOOSHORT";
			}
			else
			{
				errTex = "NICKNAME_NOTCREATE";
			}
			Debug.LogWarning(errTex);
		}
	}

	/// <summary>
	/// 删除角色
	/// </summary>
	/// <param name="uRoleID"></param>
	public void DeleteRole(ulong uRoleID)
	{
		KC2G_DeleteRoleRequest request = SingleInstanceCache.GetInstanceByType<KC2G_DeleteRoleRequest>();
		request.byProtocol = (byte)KC2G_Protocol.c2g_delete_role_request;
		request.uRoleID = uRoleID;
		NetworkManager.Instance.SendToGatewayServer(request);
	}

	/// <summary>
	/// 删除角色返回
	/// </summary>
	/// <param name="proto"></param>
	private void OnDeleteRoleRespond(KProtoBuf proto)
	{
		KG2C_DeleteRoleRespond respond = proto as KG2C_DeleteRoleRespond;
		KDeleteRoleRespondCode code = (KDeleteRoleRespondCode)(respond.code);

		//code目前只返回eDeleteRoleSucceed或eDeleteRoleUnknownError
		if (code == KDeleteRoleRespondCode.eDeleteRoleSucceed)
		{
			GetServerListProxy().RemoveCharacter(respond.uRoleID);
			GameFacade.Instance.SendNotification(NotificationName.MSG_CHARACTER_DEL_SUCC);

			/*
			var roleInfo = proxy.GetCharacter(respond.uRoleID);
			if (roleInfo != null)
			{
				roleInfo.DeleteTime = respond.nDelCountDownTime + Time.time;
				//EventDispatcher.GameWorld.DispatchEvent(ControllerCommand.UPDATE_LOGIN_VIEW);
			}
			else
			{
				Debug.Log("OnDeleteRoleRespond: roleInfo(" + respond.uRoleID + ") is null !");
			}
			*/
		}
		else
		{
			GameFacade.Instance.SendNotification(NotificationName.MSG_CHARACTER_DEL_FAIL);
			Debug.Log("删除角色失败");
		}
	}

	#endregion

}
