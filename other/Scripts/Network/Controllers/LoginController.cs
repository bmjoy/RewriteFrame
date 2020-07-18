using System;
using System.Collections.Generic;
using Assets.Scripts.Define;
using Assets.Scripts.Lib.Net;
using Assets.Scripts.Proto;
using Game.Frame.Net;
using Map;
using UnityEngine;

public class LoginController : BaseNetController
{
	/// <summary>
	/// 连接超时时长
	/// </summary>
	private readonly int CONNECT_TIME_OUT = 3;

	/// <summary>
	/// 连接失败后多久尝试重连
	/// </summary>
	private readonly int RECONNECT_DELAY = 5;

	/// <summary>
	/// 连接最大多尝试次数
	/// </summary>
	private readonly int CONNECT_MAX_TRY_COUNT = 3;

	/// <summary>
	/// 网关IP
	/// </summary>
	private string GatewayIP = "127.0.0.1";

	/// <summary>
	/// 网关端口
	/// </summary>
	private int GatewayPort = 37240;

	/// <summary>
	/// 帐号
	/// </summary>
	public string Account = "";

	/// <summary>
	/// 密码
	/// </summary>
	public string Password = "";

	/// <summary>
	/// 帐号Token
	/// </summary>
	public string AccountToken { get; set; } = "";

	/// <summary>
	/// 已经发过退出消息
	/// </summary>
	private bool ExitMsgSended = false;

	/// <summary>
	/// 客户端的IP地址
	/// </summary>
	private string clientIP;

	public LoginController()
	{
		//Socket事件
		NetworkManager.Instance._GetSocketClient().OnConnected += OnConnected;
		NetworkManager.Instance._GetSocketClient().OnConnectFailed += OnConnectFailed;
		NetworkManager.Instance._GetSocketClient().OnDisconnected += OnDisconnect;
		pingTimeout = (int)ClientPingInterval.PingIntervalTime*1000;
		//网关握手
		NetworkManager.Instance.ListenGateway(KG2C_Protocol.g2c_handshake_respond, OnHandShakeResponse, typeof(KG2C_HandshakeRespond));

		//Ping
		NetworkManager.Instance.ListenGateway(KG2C_Protocol.g2c_ping_respond, OnPing, typeof(KG2C_PingRespond));

		//帐号登录
		NetworkManager.Instance.ListenGateway(KG2C_Protocol.g2c_account_verify_respond, OnAccountVerifyResponse, typeof(KG2C_AccountVerifyRespond));
		NetworkManager.Instance.ListenGateway(KG2C_Protocol.g2c_error_info_notify, OnAccountErrorNotify, typeof(KG2C_Error_Info_Notify));
		NetworkManager.Instance.ListenGateway(KG2C_Protocol.g2c_account_locked_notify, OnAccountLockedNotify, typeof(KG2C_AccountLockedNotify));
		NetworkManager.Instance.ListenGateway(KG2C_Protocol.g2c_kick_account_notify, OnAccountKickNotify, typeof(KG2C_KickAccountNotify));

		//角色登录
		NetworkManager.Instance.ListenGateway(KG2C_Protocol.g2c_sync_login_key, OnCharacterLoginResponse, typeof(KG2C_SyncLoginKey));

		//GameServer
		NetworkManager.Instance.ListenGateway(KG2C_Protocol.g2c_player_swtich_gs, OnPlayerSwitchGS, typeof(KG2C_PlayerSwitchGS));

		//exit()
		NetworkManager.Instance.ListenGateway(KG2C_Protocol.g2c_exit_game_respond, OnExitCurrentServerResponse, typeof(KG2C_EXIT_GAME_RESPOND));

		//跨服
		NetworkManager.Instance.ListenGameServer(KS2C_Protocol.s2c_switch_zs_respond, OnSwitchToZoneServer, typeof(S2C_SWITCH_ZS_RESPOND));
		NetworkManager.Instance.ListenGameServer(KS2C_Protocol.s2c_switch_gc_respond, OnSwitchToGameCenter, typeof(S2C_SWITCH_GC_RESPOND));
	}


	/// <summary>
	/// 玩家自已的IP
	/// </summary>
	public string ClientIP
	{
		get
		{
			return clientIP;
		}
	}


	#region "SOCKET连接" 

	private enum ConnectState { NONE, CONNECTING, CONNECT_SUCCESS, CONNECT_FAIL, CONNECT_BREAK, CONNECT_DELAY, CONNECT_EXIT }

	/// <summary>
	/// 连接状态
	/// </summary>
	private ConnectState connectState;

	/// <summary>
	/// 连接状态的开始时间
	/// </summary>
	private float connectStateBeginTime = 0;

	/// <summary>
	/// 连接已尝试次数
	/// </summary>
	private int connectTryCount = 0;

	/// <summary>
	/// 是否正在连接
	/// </summary>
	/// <returns></returns>
	public bool IsConnecting()
	{
		return connectState == ConnectState.CONNECTING || connectState == ConnectState.CONNECT_DELAY;
	}

	/// <summary>
	/// 是否已连接
	/// </summary>
	/// <returns></returns>
	public bool IsConnected()
	{
		return NetworkManager.Instance._GetSocketClient() != null && NetworkManager.Instance._GetSocketClient().IsConnected();
	}

	/// <summary>
	/// 改变连接状态
	/// </summary>
	/// <param name="state"></param>
	private void ChangeConnState(ConnectState state)
	{
		connectState = state;
		connectStateBeginTime = Time.realtimeSinceStartup;
	}

	/// <summary>
	/// 开始连接服务器
	/// </summary>
	/// <param name="host"></param>
	/// <param name="port"></param>
	public void Connect(string host, int port)
	{
		Debug.Log("开始连接服务器: " + host + ":" + port + "  -  " + Account + "/" + AccountToken);

		GatewayIP = host;
		GatewayPort = port;

		GameFacade.Instance.SendNotification(NotificationName.SocketConnectStart);

		ChangeConnState(ConnectState.CONNECTING);
		NetworkManager.Instance._GetSocketClient().Connect(host, port);
	}

	/// <summary>
	/// 重连服务器
	/// </summary>
	public void Reconnect()
	{
		Connect(GatewayIP, GatewayPort);
	}

	/// <summary>
	/// 重连服务器
	/// </summary>
	/// <param name="host"></param>
	/// <param name="port"></param>
	public void Reconnect(string host, int port)
	{
		Connect(host, port);
	}

	/// <summary>
	/// 关闭服务器连接
	/// </summary>
	public void Close(bool needSendExitMSG = false)
	{
		Debug.Log("关闭服务器连接");

		ChangeConnState(ConnectState.CONNECT_EXIT);

		if (needSendExitMSG && !ExitMsgSended)
		{
			var msg = new C2S_QUIT_MESSAGE();
			msg.protocolID = (ushort)KC2S_Protocol.c2s_quit_message;

			NetworkManager.Instance.SendToGameServer(msg);

			ExitMsgSended = true;

			//Debug.LogError("QuitGame");
		}

		NetworkManager.Instance._GetSocketClient().Close();
	}

	/// <summary>
	/// 更新
	/// </summary>
	public void _DoUpdate()
	{
		if (connectState == ConnectState.CONNECTING)
		{
			if (!IsConnected() && Time.realtimeSinceStartup - connectStateBeginTime > CONNECT_TIME_OUT)
			{
				OnConnectFailed();
			}
		}
		else if (connectState == ConnectState.CONNECT_SUCCESS)
		{
			PingGateway();
		}
		else if (connectState == ConnectState.CONNECT_DELAY)
		{
			if (Time.realtimeSinceStartup - connectStateBeginTime > RECONNECT_DELAY)
			{
				GameFacade.Instance.SendNotification(NotificationName.SocketConnectRestart);

				ChangeConnState(ConnectState.CONNECTING);
				NetworkManager.Instance._GetSocketClient().Connect(GatewayIP, GatewayPort);
				Debug.Log("开始重连服务器");
			}
		}
	}

	/// <summary>
	/// 连接服务器成功
	/// </summary>
	/// <param name="objs"></param>
	/// <returns></returns>
	private void OnConnected()
	{
		ExitMsgSended = false;

		Debug.Log("服务器连接成功");
		GameFacade.Instance.SendNotification(NotificationName.MSG_LOGINWAITSHOW, false);

        HandShakeRequest();

        connectTryCount = 0;
        ChangeConnState(ConnectState.CONNECT_SUCCESS);

        GameFacade.Instance.SendNotification(NotificationName.SocketConnected);

		ServerListProxy proxy = GameFacade.Instance.RetrieveProxy(ProxyName.ServerListProxy) as ServerListProxy;
		if (proxy.GetSelectedServer() != null)
		{
			proxy.SetLastLoginServer(proxy.GetSelectedServer().Gid);
		}
	}

	/// <summary>
	/// 连接服务器失败
	/// </summary>
	/// <param name="objs"></param>
	/// <returns></returns>
	private void OnConnectFailed()
	{
		ExitMsgSended = false;

		if (connectState == ConnectState.CONNECT_EXIT)
		{
			return;
		}

		NetworkManager.Instance._GetSocketClient().Close();

		connectTryCount++;
		if (connectTryCount >= CONNECT_MAX_TRY_COUNT)
		{
			Debug.Log("连接服务器失败!");
			GameFacade.Instance.SendNotification(NotificationName.MSG_LOGINWAITSHOW, false);
			connectTryCount = 0;

			ChangeConnState(ConnectState.CONNECT_FAIL);
			GameFacade.Instance.SendNotification(NotificationName.SOCKETCONNECTFAIL);
		}
		else
		{
			Debug.Log("连接服务器超时, " + RECONNECT_DELAY + "秒后重试!");

			ChangeConnState(ConnectState.CONNECT_DELAY);
		}
	}

	/// <summary>
	/// 服务器连接断开
	/// </summary>
	/// <param name="objs"></param>
	/// <returns></returns>
	private void OnDisconnect()
	{
		ExitMsgSended = false;

		NetworkManager.Instance._SetHandshake(false);

		if (connectState == ConnectState.CONNECT_EXIT)
		{
			return;
		}

		Debug.Log("服务器连接断开!");

		ChangeConnState(ConnectState.CONNECT_BREAK);
		GameFacade.Instance.SendNotification(NotificationName.SocketConnectBreak);
	}

	#endregion

	#region "帐号登录"

	public uint uPlayerIndex { get; set; } = 0;

	private KCenterType centerType = KCenterType.kctGameCenter;

	/// <summary>
	/// 发送握手消息
	/// </summary>
	private void HandShakeRequest()
	{
		KC2G_HandshakeRequest handSkakeRequest = SingleInstanceCache.GetInstanceByType<KC2G_HandshakeRequest>();

		handSkakeRequest.byProtocol = (byte)KC2G_Protocol.c2g_handshake_request;
		handSkakeRequest.nProtocolVersion = (int)KProcotolVersion.eProcotolVersion;
		handSkakeRequest.uNetType = (byte)0;
		handSkakeRequest.szAccount = Account;
		handSkakeRequest.szToken = AccountToken;
		handSkakeRequest.uOldPlayerIndex = uPlayerIndex;
		handSkakeRequest.uRoleID = 0;// PlayerManager.GetInstance().MajorPlayer.PlayerID;

		NetworkManager.Instance.SendToGatewayServer(handSkakeRequest);

		NetworkManager.Instance._ResetTokenAndIndex(AccountToken);

		Debug.LogFormat("发送网关握手: account={0}, roleID={1}", handSkakeRequest.szAccount, handSkakeRequest.uRoleID);
	}

	/// <summary>
	/// 握手消息返回
	/// </summary>
	/// <param name="buf"></param>
	public void OnHandShakeResponse(KProtoBuf buf)
	{
		KG2C_HandshakeRespond respond = buf as KG2C_HandshakeRespond;
		KGateWayHandShakeCode code = (KGateWayHandShakeCode)(respond.code);

		clientIP = respond.szClientIP;
		centerType = (KCenterType)respond.nTag;
		Debug.Log("OnHandShakeRespond centerType:" + centerType + ", result:" + code);


		if (respond.bReconnect > 0)
		{
			if (code == KGateWayHandShakeCode.ghcHandshakeSucceed)
			{
				Debug.Log("重连成功!");
				uPlayerIndex = respond.uPlayerIndex;
				NetworkManager.Instance._SetHandshake(true);
			}
			else
			{
				Debug.Log("重连失败!");
				uPlayerIndex = 0;
				HandShakeRequest();
			}
		}
		else
		{
			if (code == KGateWayHandShakeCode.ghcHandshakeSucceed)
			{
				Debug.Log("网关握手成功!");
				uPlayerIndex = respond.uPlayerIndex;
				AccountVerify();
			}
			else
			{
				Debug.LogError("网关握手失败！ RetCode:" + code);
				///
			}
		}
	}


	/// <summary>
	/// 发送帐号验证请求
	/// </summary>
	private void AccountVerify()
	{
		KC2G_AccountVerifyRequest request = SingleInstanceCache.GetInstanceByType<KC2G_AccountVerifyRequest>();
		request.byProtocol = (byte)KC2G_Protocol.c2g_account_verify_request;
		request.account = Account;
		request.nGroupID = (int)KConstDefine.cdDefaultGroupID;
		request.password = AccountToken;
		request.pf = "?";// ApplicationManager.channel;
		request.nTag = (int)centerType;
		request.roleIdList = new List<ulong>();

		// ServerListProxy serverListProxy = DataManager.GetProxy<ServerListProxy>();
		// string serverId = serverListProxy.LastLoginServer != null ? serverListProxy.LastLoginServer.gid : string.Empty;
		// CharacterListProxy characterListProxy = DataManager.GetProxy<CharacterListProxy>();
		// Character[] characters = characterListProxy.GetAll(serverId);
		// for (int i = 0; i < characters.Length; i++)
		// {
		//     request.roleIdList.Add(characters[i].Id);
		//     //Debug.LogError("send grpc role id: " + characters[i].Id);
		// }

		NetworkManager.Instance.SendToGatewayServer(request);

		Debug.LogFormat("Account Verify Request: account={0}, groupId={1}", request.account, request.nGroupID);
	}

	/// <summary>
	/// 帐号请求结果
	/// </summary>
	/// <param name="buf"></param>
	public void OnAccountVerifyResponse(KProtoBuf buf)
	{
		KG2C_AccountVerifyRespond respond = buf as KG2C_AccountVerifyRespond;

		bool bLimitPlay = (int)respond.limitPlayEnable == 1;
		KLimitPlayTimeType eLimitPlayTimeFlag = (KLimitPlayTimeType)respond.limitPlayTimeFlag;

		int nOnlineSetting = (int)respond.limitOnlineSetting;
		int nOfflineSetting = (int)respond.limitOfflineSetting;
		int nLimitOnlineSecond = (int)respond.limitPlayOnlineSeconds;
		int nLimitOfflineSeconds = (int)respond.limitPlayOfflineSeconds;

		//AntiAddictionLogic.GetInstance().OnAccountVerifyRespond(bLimitPlay, eLimitPlayTimeFlag, nOnlineSetting, nOfflineSetting, nLimitOnlineSecond, nLimitOfflineSeconds);
		if (respond.code == 1)
		{
			//验证成功.
			if (respond.nTag == (int)KCenterType.kctZoneServer)
			{
				EnterZoneServer();//跨服要进行特殊处理.
			}
		}
		else
		{
			Debug.LogError("验证账号错误, code:" + respond.code);
		}
	}

	/// <summary>
	/// 处理登录错误
	/// </summary>
	/// <param name="proto"></param>
	private void OnAccountErrorNotify(KProtoBuf proto)
	{
		KG2C_Error_Info_Notify errorInfo = proto as KG2C_Error_Info_Notify;
		if (errorInfo.code == (byte)KGATEWAY_ERROR_CODE.gecInvalidAccountInTestMode)
		{
			Debug.LogError("无效的测试用户");
		}
		else if (errorInfo.code == (byte)KGATEWAY_ERROR_CODE.gecAccountInfoIsNotExist)
		{
			Debug.LogError("验证已过期，请重新登录");
		}
		else if (errorInfo.code == (byte)KGATEWAY_ERROR_CODE.gecPasswdInfoIsNotExist)
		{
			Debug.LogError("密码错误");
		}
		else if (errorInfo.code == (byte)KGATEWAY_ERROR_CODE.gecTokenIsNotMatch)
		{
			Debug.LogError("QQ登录失败");
		}
		else
		{
			Debug.LogError("未知错误");
		}
	}

	/// <summary>
	/// 帐号被锁定
	/// </summary>
	/// <param name="buf"></param>
	private void OnAccountLockedNotify(KProtoBuf buf)
	{
		KG2C_AccountLockedNotify respond = buf as KG2C_AccountLockedNotify;
		/*
        ApplicationManager.OnAccountLocked((int)respond.nothing);
        */
	}

	/// <summary>
	/// 帐号被踢
	/// </summary>
	/// <param name="buf"></param>
	private void OnAccountKickNotify(KProtoBuf buf)
	{
		KG2C_KickAccountNotify respond = buf as KG2C_KickAccountNotify;
		Debug.Log("玩家被踢, tag:" + respond.nothing);
		/*
        ApplicationManager.OnKickOut((int)KKickPlayerType.kptByRelogin);
        */
	}

	#endregion

	#region "角色登录"

	/// <summary>
	/// 请求登录角色
	/// </summary>
	/// <param name="uRoleID"></param>
	public void CharacterLogin(ulong uRoleID)
	{
		KC2G_RoleLoginRequest request = SingleInstanceCache.GetInstanceByType<KC2G_RoleLoginRequest>();

		request.byProtocol = (byte)KC2G_Protocol.c2g_role_login_request;
		request.uRoleID = uRoleID;
		NetworkManager.Instance.SendToGatewayServer(request);

		//SelectRoleInfo sRoleInfo = GetRoleData(uRoleID);
		// 服务器没有任何在登录后同步这两个字段的消息...所以在本地设置下
		//MajorPlayer majorPlayer = PlayerManager.GetInstance().MajorPlayer;
		//majorPlayer.BodyAvatarID = sRoleInfo.appearanceInfo.bodyAvatarID;
		//majorPlayer.WeaponAvatarID = sRoleInfo.appearanceInfo.weaponAvatarID;
	}

	/// <summary>
	/// 请求登录角色返回
	/// </summary>
	/// <param name="proto"></param>
	private void OnCharacterLoginResponse(KProtoBuf proto)
	{
		KG2C_SyncLoginKey respond = proto as KG2C_SyncLoginKey;
		KGameLoginRespondCode code = (KGameLoginRespondCode)(respond.code);

		if (code == KGameLoginRespondCode.eGameLoginSucceed)
		{
			Debug.Log("角色登录成功：" + code);

			WwiseUtil.PlaySound((int)WwiseMusic.Music_CreateRoleToGame, false, null);
			NetworkManager.Instance._SetHandshake(true);
			GSHandShakeRequest(respond.uRoleID, respond.guid);
		}
		else
		{
			Debug.Log("角色登录失败：" + code);
		}
	}

	#endregion

	#region "游戏服务器"

	/// <summary>
	/// 和游戏服务器握手
	/// </summary>
	/// <param name="playerID"></param>
	/// <param name="guid"></param>
	private void GSHandShakeRequest(ulong playerID, byte[] guid)
	{
		C2S_HANDSHAKE_REQUEST request = new C2S_HANDSHAKE_REQUEST();
		request.protocolID = (byte)KC2S_Protocol.c2s_handshake_request;
		request.procotolversion = (byte)KProcotolVersion.eProcotolVersion;
		request.uRoleID = playerID;
		request.guid = guid;

		NetworkManager.Instance.SendToGameServer(request);
	}

	/// <summary>
	/// 切换游戏服务器
	/// </summary>
	/// <param name="proto"></param>
	private void OnPlayerSwitchGS(KProtoBuf proto)
	{
		KG2C_PlayerSwitchGS respond = proto as KG2C_PlayerSwitchGS;
		ulong uPlayerID = respond.uPlayerID;
		byte[] guid = respond.guid;
		int fromGSIndex = respond.fromGSIndex;
		int toGSIndex = respond.toGSIndex;

		Debug.Log("切换GS, toGSIndex:" + toGSIndex);

		/*
        MajorPlayer player = PlayerManager.GetInstance().MajorPlayer;
        if (player == null)
        {
            Debug.LogError("OnSwitchGS error, MajorPlayer is null");
            return;
        }
        if (player.PlayerID != uPlayerID)
        {
            Debug.LogError("OnSwitchGS error, PlayerID error");
            return;
        }
        */

		GSHandShakeRequest(uPlayerID, guid);
	}


	#endregion

	#region "跨服功能"

	/// <summary>
	/// 当前正在处理跨服的角色ID.
	/// </summary>
	private ulong switchZoneServerRoleID = 0;

	/// <summary>
	/// 要去的服务器IP
	/// </summary>
	private string switchZoneServerIP;

	/// <summary>
	/// 要去的服务器Port
	/// </summary>
	private ushort switchZoneServerPort;

	/// <summary>
	/// 进入区域服务器
	/// </summary>
	private void EnterZoneServer()
	{
		KC2G_ENTER_ZS request = new KC2G_ENTER_ZS();
		request.byProtocol = (byte)KC2G_Protocol.c2g_enter_zs;
		request.uRoleID = switchZoneServerRoleID;
		NetworkManager.Instance.SendToGatewayServer(request);
	}

	/// <summary>
	/// 切换到区域服务器
	/// </summary>
	/// <param name="respond"></param>
	public void OnSwitchToZoneServer(KProtoBuf buf)
	{
		S2C_SWITCH_ZS_RESPOND respond = buf as S2C_SWITCH_ZS_RESPOND;

		Debug.Log(string.Format("OnSwitchZSRespond, nResult:{0}, gateway {1}:{2}", respond.nResult, respond.szGateWayIP.TrimEnd('\0'), (int)respond.uGateWayPort));
		if (respond.nResult > 0)
		{
			switchZoneServerRoleID = respond.roleID;
			switchZoneServerIP = respond.szGateWayIP.TrimEnd('\0');
			switchZoneServerPort = respond.uGateWayPort;

			ExitCurrentServer(1);
		}
	}

	/// <summary>
	/// 返回到中心服务器
	/// </summary>
	/// <param name="respond"></param>
	public void OnSwitchToGameCenter(KProtoBuf buf)
	{
		S2C_SWITCH_GC_RESPOND respond = buf as S2C_SWITCH_GC_RESPOND;

		Debug.Log(string.Format("OnSwitchGCRespond, nResult:{0}, gateway {1}:{2}", respond.nResult, switchZoneServerIP, (int)switchZoneServerPort));
		if (respond.nResult > 0)
		{
			switchZoneServerRoleID = respond.roleID;
			switchZoneServerIP = null;
			switchZoneServerPort = 0;

			ExitCurrentServer(2);
		}
	}

	/// <summary>
	/// 请求退出当前服务器
	/// </summary>
	/// <param name="nCode"></param>
	public void ExitCurrentServer(int nCode)
	{
		Debug.Log("Send Exit Game, code:" + nCode + "   : connnect ? " + IsConnected());
		if (IsConnected())
		{
			KC2G_EXIT_GAME exitGameRequest = SingleInstanceCache.GetInstanceByType<KC2G_EXIT_GAME>();
			exitGameRequest.byProtocol = (byte)KC2G_Protocol.c2g_exit_game;
			exitGameRequest.nExitCode = nCode;
			NetworkManager.Instance.SendToGatewayServer(exitGameRequest);
		}
	}

	/// <summary>
	/// 退出当前服务器返回
	/// </summary>
	/// <param name="proto"></param>
	private void OnExitCurrentServerResponse(KProtoBuf proto)
	{

		KG2C_EXIT_GAME_RESPOND respond = proto as KG2C_EXIT_GAME_RESPOND;
		if (respond.exitCode == 1)
		{
			//连接到ZS 的gateway
			//回到选角
			//UIManager.Instance.OpenPanel(PanelName.LoadingSecondPanel);
			//MapManager.GetInstance().TryCloseMap(OnMapClosed);
			//GameplayManager.Instance.Clear();
		}
		else if (respond.exitCode == 2)
		{
			//连接到GC 的gateway,跨服返回原GC.  //回到登陆
			NetworkManager.Instance._GetSocketClient().Close();
			ChangeConnState(ConnectState.CONNECTING);
		}

		Debug.Log("respond.exitCode=" + respond.exitCode +
			"   GatewayIP=" + GatewayIP +
			"   GatewayPort=" + GatewayPort);
	}

	private void OnMapClosed()
	{
		UIManager.Instance.ClosePanel(UIPanel.LoadingSecondPanel);
        UIManager.Instance.CloseAllHud();
        UIManager.Instance.OpenPanel(UIPanel.CharacterPanel);

	}

	#endregion

	#region "心跳"

	/// <summary>
	/// 间隔多长时间ping一次
	/// </summary>
	private int pingTimeout = 2000;

	/// <summary>
	/// 最后一次ping是什么时间
	/// </summary>
	private long lastPingTime = 0;
    /// <summary>
	/// 时间戳延迟
	/// </summary>
	private long m_TimeStampDelay;
    /// <summary>
    /// Ping服务器
    /// </summary>
    /// <param name="objs"></param>
    /// <returns></returns>
    private void PingGateway(params object[] objs)
	{
		if (!IsConnected()) { return; }

		long currentTime = (int)(Time.realtimeSinceStartup * 1000);
        if(lastPingTime!= 0 && currentTime - lastPingTime < pingTimeout)
		{
			return;
		}
		lastPingTime = currentTime;

		KC2G_PingSignal ping = new KC2G_PingSignal();
		ping.byProtocol = (byte)KC2G_Protocol.c2g_ping_signal;
		ping.dwTime = (uint)currentTime;

		NetworkManager.Instance.SendToGatewayServer(ping);

		//Debug.Log("Ping服务器!"+DateTime.Now);
	}

	private void OnPing(KProtoBuf buf)
	{
        KG2C_PingRespond msg = buf as KG2C_PingRespond;
        m_TimeStampDelay = (long)msg.time * 1000 - (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000;
    }

    /// <summary>
	/// 时间戳
	/// </summary>
	/// <returns></returns>
	public ulong TimeStamp()
    {
        return (ulong)((DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000 + m_TimeStampDelay);
    }
    #endregion
}