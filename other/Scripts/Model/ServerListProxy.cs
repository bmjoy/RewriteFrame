/**********************
 *	ServerListProxy   *
 *	Gaoyu			  *
 *	2019-3-21		  *
 **********************/
using Assets.Scripts.Lib.Net;
using PureMVC.Patterns.Proxy;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using static ServerInfoVO;
public class ServerListProxy : Proxy
{
	/// <summary>
	/// 保存最后登录服务器 Key值
	/// </summary>
	private const string LAST_LOGIN_SERVER = "LAST_LOGIN_SERVER";

	/// <summary>
	/// 最大尝试次数
	/// 用于grpc相关的请求
	/// </summary>
	private const int RETRY_MAX_COUNT = 3;

	/// <summary>
	/// 角色Proxy
	/// </summary>
	private CfgEternityProxy m_CfgEternityProxy;

	/// <summary>
	/// 登陆Proxy
	/// </summary>
	private LoginProxy m_LoginProxy;

	/// <summary>
	/// 服务器信息列表
	/// </summary>
	private Dictionary<string, ServerInfoVO> m_ServerInfoVOlist;

	/// <summary>
	/// 选择的服务器ID
	/// </summary>
	private string m_SelectedServerID = string.Empty;
	
	/// <summary>
	/// 性别 是否是男的
	/// </summary>
	private bool m_IsMale;

	/// <summary>
	/// 皮肤
	/// </summary>
	private int m_SkinIndex;

	/// <summary>
	/// 角色列表中选中的vo
	/// 用于展示npc形象
	/// </summary>
	private CharacterVO m_CurrentCharacterVO;

	/// <summary>
	/// 当前创角选角状态
	/// </summary>
	private CharacterPanelState m_state;
	
	/// <summary>
	/// 当前尝试次数
	/// </summary>
	private int m_CurrentRetryCount;

	public ServerListProxy() : base(ProxyName.ServerListProxy)
	{
		m_ServerInfoVOlist = new Dictionary<string, ServerInfoVO>();
	}

	#region LastServer
	/// <summary>
	/// 设置最后登陆的服务器Gid
	/// </summary>
	/// <param name="serverGid">服务器Gid</param>
	public void SetLastLoginServer(string serverGid)
	{
		if (!string.IsNullOrEmpty(serverGid))
		{
			PlayerPrefs.SetString(LAST_LOGIN_SERVER, serverGid);
		}
	}

	/// <summary>
	/// 获取最后登陆的服务器
	/// </summary>
	/// <returns></returns>
	public ServerInfoVO GetLastLoginServer()
	{
		return GetServerBy(PlayerPrefs.GetString(LAST_LOGIN_SERVER, string.Empty));
	}

	#endregion

	#region SelectedServer
	/// <summary>
	/// 选择服务器
	/// </summary>
	/// <param name="serverGid">服务器Gid</param>
	public void SetSelectedServer(string serverGid)
	{
		m_SelectedServerID = serverGid;
	}

	/// <summary>
	/// 获取选择服务器信息
	/// </summary>
	/// <returns></returns>
	public ServerInfoVO GetSelectedServer()
	{
		return GetServerBy(m_SelectedServerID);
	}
	#endregion

	/// <summary>
	/// 添加一个服务器
	/// </summary>
	/// <param name="id">服务器id</param>
	/// <param name="gid">服务器Gid</param>
	/// <param name="name">服务器名字</param>
	/// <param name="state">状态</param>
	/// <param name="ip">IP地址</param>
	/// <param name="port">端口</param>
	/// <param name="openTime">开服时间</param>
	public void AddServer(int id, string gid, string name, int state, string ip, int port, string openTime)
	{
		ServerInfoVO server = new ServerInfoVO();
		server.Id = id;
		server.Gid = gid;
		server.Name = name;
		server.State = state;

        //为了适应外网联调，服务器添加特殊的IP地址
        //如果IP地址中带有：号，则进行解析做为新地址和端口使用
        int indexOfColon = ip.IndexOf(':');
        if (indexOfColon > 0)
        {
            server.Ip = ip.Substring(0, indexOfColon);
            server.Port = int.Parse(ip.Substring(indexOfColon + 1));
        }
        else
        {
            server.Ip = ip;
            server.Port = port;
        }

        server.OpenTime = openTime;
		if (!m_ServerInfoVOlist.ContainsKey(gid))
		{
			m_ServerInfoVOlist.Add(gid, server);
		}
	}

	/// <summary>
	/// 获取服务器列表
	/// </summary>
	/// <returns></returns>
	public List<ServerInfoVO> GetServerList()
	{
		return m_ServerInfoVOlist.Values.ToList();
	}

	/// <summary>
	/// 排序
	/// </summary>
	public void Sort()
	{
		m_ServerInfoVOlist.OrderBy(m => m.Value.Id);
	}

	/// <summary>
	/// 按ID获取服务器
	/// </summary>
	/// <param name="serverID">服务器id</param>
	/// <returns></returns>
	public ServerInfoVO GetServerBy(string serverID)
	{
		if (m_ServerInfoVOlist.ContainsKey(serverID))
		{
			return m_ServerInfoVOlist[serverID];
		}
		return null;
	}

	/// <summary>
	/// 清空本地存的所有服务器上的角色
	/// </summary>
	public void ClearAllServerCharacters()
	{
		foreach (KeyValuePair<string, ServerInfoVO> item in m_ServerInfoVOlist)
		{
			item.Value.CharacterList = null;
		}
	}

	/// <summary>
	/// 清空本地存的所有服务器上的角色
	/// </summary>
	public void ClearServerCharacters(string serverID)
	{
		ServerInfoVO serverInfo = GetServerBy(serverID);
		if (serverInfo != null)
		{
			serverInfo.CharacterList = null;
		}
	}

	/// <summary>
	/// 从本地缓存数据服务器中删除一个角色数据
	/// </summary>
	/// <param name="uid">实例id</param>
	public void RemoveCharacter(ulong uid)
	{
        if (PlayerPrefs.HasKey(uid.ToString()))
        {
            PlayerPrefs.DeleteKey(uid.ToString());
        }
        ServerInfoVO serverInfo = GetLastLoginServer();
		for (int i = 0; i < serverInfo.CharacterList.Count; i++)
		{
			if (serverInfo.CharacterList[i].UId == uid)
			{
				serverInfo.CharacterList.RemoveAt(i);
				return;
			}
		}
	}

	#region  AddCharacter
	/// <summary>
	/// 添加角色到角色总列表
	/// </summary>
	/// <param name="headAdd">添加到列表头部</param>
	/// <param name="serverGid">服务器gid</param>
	/// <param name="id">角色uid</param>
	/// <param name="tid"></param>
	/// <param name="name"></param>
	/// <param name="lv"></param>
	/// <param name="offtime">上次登陆时间</param>
	/// <param name="pos">位置</param>
	public void AddCharacter(bool headAdd, string serverGid, ulong id, int tid, string name, int lv, ulong offtime, int pos,int rank)
	{
		ServerInfoVO serverInfo = GetServerBy(serverGid);
		if (serverInfo == null)
		{
			return;
		}
		if (serverInfo.CharacterList == null)
		{
			serverInfo.CharacterList = new List<CharacterVO>();
		}

		CharacterVO player = new CharacterVO();
		player.UId = id;
		player.Name = name;
		player.Tid = tid;
		player.Level = lv;
        player.DanLevel = rank;
		player.LastLoginTime = (int)offtime;
		if (headAdd)
		{
			serverInfo.CharacterList.Insert(0, player);
		}
		else
		{
			serverInfo.CharacterList.Add(player);
		}
	}
	#endregion

	#region  LoginProxy Step1 获取服务器列表
	
	/// <summary>
	/// 获取服务器列表
	/// </summary>
	public void LoadServerList()
	{
		m_CurrentRetryCount = RETRY_MAX_COUNT;
		CoroutineHelper.GetInstance().StartCoroutine(LoadServerList_Co());
	}

	private System.Collections.IEnumerator LoadServerList_Co()
	{
		if (SettingINI.Setting.TryGetValue(SettingINI.Setting.CombineKey(SettingINI.Constants.GROUP_SERVER
			, SettingINI.Constants.KEY_DEFAULT_SERVER_LIST), out string defaultServerList))
		{
			Leyoutech.Utility.DebugUtility.Log(KConstants.LOG_TAG, "Load defaultServerList from settingINI: \n" + defaultServerList);
			ServerListData serverListData = JsonUtility.FromJson<ServerListData>(defaultServerList);
			for (int iServer = 0; iServer < serverListData.server_list.Length; iServer++)
			{
				ServerListData.Server_listItem iterServer = serverListData.server_list[iServer];
				AddServer(int.Parse(iterServer.id)
					, iterServer.gid
					, iterServer.name
					, iterServer.status
					, iterServer.ip
					, iterServer.port
					, iterServer.open_time);
			}
			Sort();
			SendNotification(NotificationName.MSG_GRPC_SERVERLIST_BACK);
		}
		else
		{
			do
			{
				string url = GetLoginProxy().GetMicroServerURL_GetServerList() + "getServerList";
				Leyoutech.Utility.DebugUtility.Log(KConstants.LOG_TAG, "Request " + url);
				UnityWebRequest www = UnityWebRequest.Get(url);
				yield return www.SendWebRequest();

				if (www.isNetworkError || www.isHttpError)
				{
					Leyoutech.Utility.DebugUtility.LogError(KConstants.LOG_TAG, "Request getServerList Error: \n" + www.error);
					SendNotification(NotificationName.MSG_GRPC_SERVERLIST_ERR);
				}
				else
				{
					Leyoutech.Utility.DebugUtility.Log(KConstants.LOG_TAG, "Request getServerList Success: \n" + www.downloadHandler.text);
					ServerListData serverListData = JsonUtility.FromJson<ServerListData>(www.downloadHandler.text);
					for (int iServer = 0; iServer < serverListData.server_list.Length; iServer++)
					{
						ServerListData.Server_listItem iterServer = serverListData.server_list[iServer];
						AddServer(int.Parse(iterServer.id)
							, iterServer.gid
							, iterServer.name
							, iterServer.status
							, iterServer.ip
							, iterServer.port
							, iterServer.open_time);
					}
					Sort();
					SendNotification(NotificationName.MSG_GRPC_SERVERLIST_BACK);
					break;
				}
			}
			while (--m_CurrentRetryCount > 0);
		}
	}
	#endregion

	#region Step2 认证账号
	/// <summary>
	/// 请求认证账号
	/// </summary>
	public void Login()
	{
		m_CurrentRetryCount = RETRY_MAX_COUNT;
		CoroutineHelper.GetInstance().StartCoroutine(Login_Co());
	}

	private System.Collections.IEnumerator Login_Co()
	{
		do
		{
			string url = string.Format("{0}authorize?username=\"{1}\"&password=\"{2}\""
				, GetLoginProxy().GetMicroServerURL_Auth()
				, NetworkManager.Instance.GetLoginController().Account
				, NetworkManager.Instance.GetLoginController().Password);
			Leyoutech.Utility.DebugUtility.Log(KConstants.LOG_TAG, "Request " + url);
			UnityWebRequest www = UnityWebRequest.Get(url);
			yield return www.SendWebRequest();

			if (www.isNetworkError || www.isHttpError)
			{
				Leyoutech.Utility.DebugUtility.LogError(KConstants.LOG_TAG, "Request authorize Error: \n" + www.error);
				SendNotification(NotificationName.MSG_GRPC_AUTH_ERR);
			}
			else
			{
				Leyoutech.Utility.DebugUtility.Log(KConstants.LOG_TAG, "Request authorize Success: \n" + www.downloadHandler.text);
				TokenData tokenData = JsonUtility.FromJson<TokenData>(www.downloadHandler.text);
				ClearAllServerCharacters();
				NetworkManager.Instance.GetLoginController().AccountToken = tokenData.success.token_id;

				SendNotification(NotificationName.MSG_GRPC_AUTH_BACK);

				// HACK 现在不需要从微服获取角色列表
				SendNotification(NotificationName.MSG_GRPC_GETROLES_BACK);

				ServerInfoVO server = GetSelectedServer() ?? GetLastLoginServer();
				if (server != null)
				{
					NetworkManager.Instance.GetLoginController().Connect(server.Ip, server.Port);
				}
				break;
			}
		}
		while (--m_CurrentRetryCount > 0);
	}
	#endregion

	#region Proxy

	/// <summary>
	/// CfgPlayerProxy
	/// </summary>
	/// <returns></returns>
	private LoginProxy GetLoginProxy()
	{
		if (m_LoginProxy == null) 
		{
			m_LoginProxy = (LoginProxy)Facade.RetrieveProxy(ProxyName.LoginProxy);
		}
		return m_LoginProxy;
	}

	/// <summary>
	/// CfgPlayerProxy
	/// </summary>
	/// <returns></returns>
	private CfgEternityProxy GetCfgEternityProxy()
	{
		if (m_CfgEternityProxy == null)
		{
			m_CfgEternityProxy = (CfgEternityProxy)Facade.RetrieveProxy(ProxyName.CfgEternityProxy);
		}
		return m_CfgEternityProxy;
	}
	#endregion

	#region CharacterCreateProxy
	/// <summary>
	/// 设置当前创角选角状态
	/// </summary>
	/// <param name="state"></param>
	public void SetCurrentState(CharacterPanelState state)
	{
		m_state = state;
		MsgCharacterPanelState stateMsg = MessageSingleton.Get<MsgCharacterPanelState>();
		stateMsg.State = m_state;
		SendNotification(NotificationName.MSG_CHARACTER_CREATE_STATE_CHANGE, stateMsg);
	}

	/// <summary>
	/// 创角时 改变性别
	/// </summary>
	/// <param name="isMale"></param>
	public void SetChangeGender(bool isMale)
	{
		m_IsMale = isMale;
		SetCurrentCharacter();
	}

	/// <summary>
	/// 创角时 改变skin
	/// </summary>
	/// <param name="index"></param>
	public void SetChangeSkinIndex(int index)
	{
		int max = PlayerListCount();
		if (index > max)
		{
			index = max;
		}
		m_SkinIndex = index;
		SetCurrentCharacter();
	}

	/// <summary>
	/// 设置皮肤index
	/// </summary>
	/// <param name="value">数值</param>
	public void SetSkinIndex(int value)
	{
		m_SkinIndex = value;
	}

	/// <summary>
	/// 设置当前创角时 角色信息
	/// </summary>
	public void SetCurrentCharacter()
	{
		if (m_CurrentCharacterVO == null)
		{
			m_CurrentCharacterVO = new CharacterVO();
		}
		if (m_IsMale)
		{
			m_CurrentCharacterVO.Tid = (int)GetCfgEternityProxy().GetMalePlayerByIndex(m_SkinIndex).Id;
		}
		else
		{
			m_CurrentCharacterVO.Tid = (int)GetCfgEternityProxy().GetFamalePlayerByIndex(m_SkinIndex).Id;
		}
		SendNotification(NotificationName.MSG_CHARACTER_CREATE_CURRENT_CHARACTERVO_CHANGE);
		
	}

	/// <summary>
	/// 根据性别返回skin模板数量
	/// </summary>
	/// <returns></returns>
	public int PlayerListCount()
	{
		if (m_IsMale)
		{
			return GetCfgEternityProxy().GetMalePlayerLength();
		}
		return GetCfgEternityProxy().GetFamalePlayerLength();
	}

	/// <summary>
	/// 设置当前选中的角色列表中的角色
	/// </summary>
	/// <param name="characterVO"></param>
	public void SetCurrentCharacterVO(CharacterVO characterVO)
	{
		m_CurrentCharacterVO = new CharacterVO();
		if (characterVO != null)
		{
			m_CurrentCharacterVO.Tid = characterVO.Tid;
			m_CurrentCharacterVO.UId = characterVO.UId;
			m_CurrentCharacterVO.Level = characterVO.Level;
            m_CurrentCharacterVO.DanLevel = characterVO.DanLevel;
            m_CurrentCharacterVO.LastLoginTime = characterVO.LastLoginTime;
			m_CurrentCharacterVO.Name = characterVO.Name;
		}
		else
		{
			m_CurrentCharacterVO.Tid = (int)GetCfgEternityProxy().GetMalePlayerByIndex(0).Id;
		}
		SendNotification(NotificationName.MSG_CHARACTER_CREATE_CURRENT_CHARACTERVO_CHANGE);
	}

	/// <summary>
	/// 获取角色列表中当前选中的角色
	/// </summary>
	/// <returns></returns>
	public CharacterVO GetCurrentCharacterVO()
	{
		if (m_CurrentCharacterVO == null)
		{
			m_CurrentCharacterVO = new CharacterVO();
			m_CurrentCharacterVO.Tid = (int)GetCfgEternityProxy().GetMalePlayerByIndex(0).Id;
		}
		return m_CurrentCharacterVO;
	}

	/// <summary>
	/// 角色登陆
	/// </summary>
	public void CharacterLogin()
	{
		WwiseUtil.PlaySound((int)1001, false, null);
		NetworkManager.Instance.GetLoginController().CharacterLogin(m_CurrentCharacterVO.UId);
	}

	/// <summary>
	/// 角色随机起名
	/// </summary>
	public void RandomName()
	{
		NetworkManager.Instance.GetCharacterListController().RollRoleName(m_CfgEternityProxy.GetGenderByTid(GetCurrentCharacterVO().Tid));
	}

	/// <summary>
	/// 申请创角
	/// </summary>
	/// <param name="name">名字</param>
	public void CreateCharacter(string name)
	{
		NetworkManager.Instance.GetCharacterListController().CreateRole(name, GetCurrentCharacterVO().Tid);
	}

	/// <summary>
	/// 删除角色
	/// </summary>
	public void DelCharacter()
	{
		NetworkManager.Instance.GetCharacterListController().DeleteRole(m_CurrentCharacterVO.UId);             
	}


	#endregion

	/// <summary>
	/// 标记当前操作状态
	/// </summary>
	public enum CharacterPanelState
	{
		/// <summary>
		/// 选择角色列表状态
		/// </summary>
		RoleList,

		/// <summary>
		/// 创建角色状态
		/// </summary>
		CreatRole
	}

	[Serializable]
	public class ServerListData
	{
		public Server_listItem[] server_list;

		[Serializable]
		public class Server_listItem
		{
			public string id;
			public string gid;
			public string ip;
			public int port;
			public string name;
			public string open_flag;
			public int status;
			public string open_time;
		}
	}

	[Serializable]
	public class TokenData
	{
		public Success success;
		public Error error;

		[Serializable]
		public class Success
		{
			public string token_id;
		}

		[Serializable]
		public class Error
		{
			public string code;
		}
	}
}
