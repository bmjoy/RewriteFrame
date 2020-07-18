using PureMVC.Patterns.Proxy;

/// <summary>
/// 处理验证登陆相关
/// </summary>
public class LoginProxy : Proxy
{
	/// <summary>
	/// AuthServer地址
	/// </summary>
	private string m_MicroServerURL_Auth;
    /// <summary>
    /// 获取服务器列表的地址
    /// </summary>
    private string m_MicroServerURL_GetServerList;
    /// <summary>
    /// 当前尝试次数
    /// </summary>
    private int m_CurrentRetryCount;
	/// <summary>
	/// ServerListProxy
	/// </summary>
	private ServerListProxy m_ServerListProxy;

#if MICROSOFT_OFFICE
    private const string MICRO_SERVER_AUTH_URL = "http://10.53.2.103:3001/";
    private const string MICRO_SERVER_GETSERVERLIST_URL = "http://10.53.2.103:3002/";
#else
    private const string MICRO_SERVER_AUTH_URL = "http://10.53.2.83:3001/";
    private const string MICRO_SERVER_GETSERVERLIST_URL = "http://10.53.2.83:3002/";
#endif

    public LoginProxy() : base(ProxyName.LoginProxy)
    {
        m_MicroServerURL_Auth = SettingINI.Setting.GetValue(SettingINI.Setting.CombineKey(SettingINI.Constants.GROUP_SERVER
            , SettingINI.Constants.KEY_MICRO_SERVER_URL_AUTH), MICRO_SERVER_AUTH_URL);
        m_MicroServerURL_GetServerList = SettingINI.Setting.GetValue(SettingINI.Setting.CombineKey(SettingINI.Constants.GROUP_SERVER
            , SettingINI.Constants.KEY_MICRO_SERVER_URL_GETSERVERLIST), MICRO_SERVER_GETSERVERLIST_URL);
    }

    /// <summary>
    /// 加载ServerList
    /// </summary>
    public void LoadServerList()
	{
		GetServerListProxy().LoadServerList();
	}

	/// <summary>
	/// 请求认证账号
	/// </summary>
	public void Login()
	{
		GetServerListProxy().Login();
	}

	public string GetMicroServerURL_Auth()
	{
		return m_MicroServerURL_Auth;
	}

    public string GetMicroServerURL_GetServerList()
    {
        return m_MicroServerURL_GetServerList;
    }

    #region 快捷
    /// <summary>
    /// 是否有最后一次登陆的服务器信息
    /// </summary>
    /// <returns></returns>
    public bool HasLastLoginServer()
	{
		return GetServerListProxy().GetLastLoginServer() != null;
	}

	public void SetLastLoginServer(string serverGid)
	{
		GetServerListProxy().SetLastLoginServer(serverGid);
	}
	#endregion
	
	#region Proxy
	/// <summary>
	/// 获取ServerListProxy
	/// </summary>
	/// <returns></returns>
	private ServerListProxy GetServerListProxy()
	{
		if (m_ServerListProxy == null)
		{
			m_ServerListProxy = (ServerListProxy)Facade.RetrieveProxy(ProxyName.ServerListProxy);
		}
		return m_ServerListProxy;
	}
	#endregion
}