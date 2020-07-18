using PureMVC.Patterns.Proxy;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public class FriendProxy : Proxy
{
	/// <summary>
	/// 好友数据
	/// </summary>
    private List<FriendInfoVO> m_FriendList = new List<FriendInfoVO>();

	/// <summary>
	/// 黑名单数据
	/// </summary>
    private List<FriendInfoVO> m_BlackList = new List<FriendInfoVO>();

	/// <summary>
	/// 附近玩家数据
	/// </summary>
	private List<FriendInfoVO> m_NearbyList = new List<FriendInfoVO>();

	/// <summary>
	/// 近期聊过的玩家数据
	/// </summary>
	private List<FriendInfoVO> m_RecentList = new List<FriendInfoVO>();

	/// <summary>
	/// 邀请好友数据
	/// </summary>
	private List<FriendInviteInfoVO> m_FriendInviteList = new List<FriendInviteInfoVO>();

	//状态
	public FriendInfoVO.FriendState m_Status;

	/// <summary>
	/// 好友列表数量
	/// </summary>
    public int m_FirendListSize = 200;

	/// <summary>
	/// 黑名单列表
	/// </summary>
	public int m_BlackListSize = 200;

	/// <summary>
	/// GameplayProxy
	/// </summary>
	private GameplayProxy m_GameplayProxy;

	/// <summary>
	/// ServerListProxy
	/// </summary>
	private ServerListProxy m_ServerListProxy;

	public FriendProxy() : base(ProxyName.FriendProxy) { }

	/// <summary>
	/// 清理
	/// </summary>
	public void Clear()
    {
        m_FriendList.Clear();
        m_BlackList.Clear();
        m_FriendInviteList.Clear();
    }

	/// <summary>
	///  赋值好友列表数量
	/// </summary>
	/// <param name="size"></param>
	public void SetFirendListSize(int size)
	{
		m_FirendListSize = size;
	}

	/// <summary>
	/// 赋值 邮件列表
	/// </summary>
	/// <param name="size"></param>
	public void SetBlackListSize(int size)
	{
		m_BlackListSize = size;
	}
	#region 好友列表

	/// <summary>
	/// 查找好友
	/// </summary>
	/// <param name="uid">好友ID</param>
	/// <returns>好友信息</returns>
	public FriendInfoVO GetFriend(ulong uid)
    {
        for (int i = 0; i < m_FriendList.Count; i++)
        {
            if (m_FriendList[i].UID == uid)
            {
                return m_FriendList[i];
            }
        }
        return null;
    }

	/// <summary>
	/// 查找附近的人
	/// </summary>
	/// <param name="uid">附近的人ID</param>
	/// <returns>附近的人信息</returns>
	public FriendInfoVO GetNearby(ulong uid)
	{
		for (int i = 0; i < m_NearbyList.Count; i++)
		{
			if (m_NearbyList[i].UID == uid)
			{
				return m_NearbyList[i];
			}
		}
		return null;
	}
	/// <summary>
	/// 查找最近玩过得人
	/// </summary>
	/// <param name="uid">最近玩过得人ID</param>
	/// <returns>最近玩过得人信息</returns>
	public FriendInfoVO GetRecent(ulong uid)
	{
		for (int i = 0; i < m_RecentList.Count; i++)
		{
			if (m_RecentList[i].UID == uid)
			{
				return m_RecentList[i];
			}
		}
		return null;
	}


	/// <summary>
	/// 获取全部好友列表
	/// </summary>
	/// <returns>好友列表</returns>
	public List<FriendInfoVO> GetFriendList()
    {
		return m_FriendList;
    }
	/// <summary>
	/// 获取黑名单列表
	/// </summary>
	/// <returns></returns>
	public List<FriendInfoVO> GetBlackList()
    {
        return m_BlackList;
    }

	/// <summary>
	/// 获取附近玩家列表
	/// </summary>
	/// <returns></returns>
	public List<FriendInfoVO> GetNearbyList()
	{
		AddNearbyListData();
		return m_NearbyList;
	}

	/// <summary>
	/// 获取最近聊过玩家列表
	/// </summary>
	/// <returns></returns>
	public List<FriendInfoVO> GetRecentList()
	{
		return m_RecentList;
	}

	/// <summary>
	/// 获取全部好友数组
	/// </summary>
	/// <returns>好友列表</returns>
	public FriendInfoVO[] GetFriends()
    {        
        return m_FriendList.ToArray();
    }
    /// <summary>
    /// 根据排序类型获取好友数据
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public FriendInfoVO[] GetSortFriends(int sortType)
    {
        FriendInfoVO[] friends = GetFriends();
        System.Array.Sort(friends,
                (FriendInfoVO a, FriendInfoVO b) =>
                {
                    switch (sortType)
                    {
                        case (int)SortType.Positive:
                            {
                                if (a.SName.CompareTo(b.SName) != 0)
                                {
                                    return a.SName.CompareTo(b.SName);
                                }
                                else if (a.AddTime.CompareTo(b.AddTime) != 0)
                                {
                                    return a.AddTime.CompareTo(b.AddTime);
                                }
                                else
                                {
                                    return 0;
                                }

                            }
                        case (int)SortType.Reverse:
                            {
                                if (a.SName.CompareTo(b.SName) != 0)
                                {
                                    return a.SName.CompareTo(b.SName) * -1;
                                }
                                else if (a.AddTime.CompareTo(b.AddTime) != 0)
                                {
                                    return a.AddTime.CompareTo(b.AddTime);
                                }
                                else
                                {
                                    return 0;
                                }
                            }
                        case (int)SortType.AddTime:
                            {
                                if (a.AddTime.CompareTo(b.AddTime) != 0)
                                {
                                    return a.AddTime.CompareTo(b.AddTime);
                                }
                                else if (a.SName.CompareTo(b.SName) != 0)
                                {
                                    return a.SName.CompareTo(b.SName);
                                }
                                else
                                {
                                    return 0;
                                }
                            }
                        default:
                            return 0;
                    }
                }
                );
        return friends;
    }

	/// <summary>
	/// 好友按照是否在线和首字母排序
	/// </summary>
	public void SortFriendsByOnLine()
	{
		m_FriendList.OrderBy(o => (int)o.Status).ThenBy(o => o.Name).ThenBy(o => o.UID).ToList();
	}

	/// <summary>
	/// 添加好友
	/// </summary>
	/// <param name="info">好友数据</param>
    public void AddFriend(FriendInfoVO info)
    {
        if (info.UID != 0 && info.UID != GetServerListProxy().GetCurrentCharacterVO().UId)
        {
			for (int i = 0; i < m_FriendList.Count; i++)
			{
				if (m_FriendList[i].UID == info.UID)
				{
					m_FriendList.RemoveAt(i);
				}
			}
			m_FriendList.Add(info);
		}
       
		//Debug.Log(m_FriendList.Count);
    }

	/// <summary>
	/// 添加最近玩过的
	/// </summary>
	/// <param name="info">数据</param>
	public void AddRecent(FriendInfoVO info)
	{
		if (info.UID == 0)
		{
			return;
		}
		for (int i = 0; i < m_RecentList.Count; i++)
		{
			if (m_RecentList[i].UID == info.UID)
			{
				m_RecentList.RemoveAt(i);
			}
		}
		if (GetBlack(info.UID) == null)
		{
			m_RecentList.Add(info);
		}
	}

	/// <summary>
	/// 删除好友
	/// </summary>
	/// <param name="uid">好友ID</param>
	/// <returns>好友数据</returns>
	public FriendInfoVO DelFriend(ulong uid)
    {
        for (int i = 0; i < m_FriendList.Count; i++)
        {
            if (m_FriendList[i].UID == uid)
            {
				FriendInfoVO result = m_FriendList[i];
                m_FriendList.RemoveAt(i);
                return result;
            }
        }
        return null;
    }

	/// <summary>
	/// 改变成员
	/// </summary>
	public void SetMember(ulong id, bool isOnline)
	{
		FriendInfoVO member = GetFriend(id);
		if (member != null)
		{
			member.Status = isOnline? FriendInfoVO.FriendState.ONLINE: FriendInfoVO.FriendState.LEAVE;
			GameFacade.Instance.SendNotification(NotificationName.MSG_FRIEND_LIST_CHANGED, member);
		}
	}
	#endregion

	#region 黑名单列表
	/// <summary>
	/// 查找黑名单数据
	/// </summary>
	/// <param name="uid">好友ID</param>
	/// <returns>好友数据</returns>
	public FriendInfoVO GetBlack(ulong uid)
    {
        for (int i = 0; i < m_BlackList.Count; i++)
        {
            if (m_BlackList[i].UID == uid)
            {
                return m_BlackList[i];
            }
        }
        return null;
    }
	/// <summary>
	/// 黑名单数组
	/// </summary>
	/// <returns>黑名单数组</returns>
    public FriendInfoVO[] GetBlacks()
    {
        return m_BlackList.ToArray();
    }
	/// <summary>
	/// 黑名单列表
	/// </summary>
	/// <param name="info">黑名单</param>
    public void AddBlack(FriendInfoVO info)
    {
		if (info.UID != 0)
		{
			for (int i = 0; i < m_BlackList.Count; i++)
			{
				if (m_BlackList[i].UID == info.UID)
				{
					m_BlackList.RemoveAt(i);
				}
			}
			m_BlackList.Add(info);
		}
    }
	/// <summary>
	/// 删除某一黑名单
	/// </summary>
	/// <param name="uid">uid</param>
	/// <returns>黑名单</returns>
    public FriendInfoVO DelBlack(ulong uid)
    {
        for (int i = 0; i < m_BlackList.Count; i++)
        {
            if (m_BlackList[i].UID == uid)
            {
				FriendInfoVO result = m_BlackList[i];
                m_BlackList.RemoveAt(i);
                return result;
            }
        }
        return null;
    }
	#endregion

	#region 好友请求列表
	/// <summary>
	/// 获取好友请求数组
	/// </summary>
	/// <returns>好友请求数据</returns>
	public FriendInviteInfoVO[] GetFriendReqs()
    {    
        return m_FriendInviteList.ToArray();
    }
	/// <summary>
	/// 添加好友请求
	/// </summary>
	/// <param name="info">好友请求信息</param>
	public void AddFriendInvite(FriendInviteInfoVO info)
    {
        m_FriendInviteList.Add(info);
    }
	/// <summary>
	/// 删除好友请求
	/// </summary>
	/// <param name="uid">好友UID</param>
	/// <returns>好友请求数据</returns>
    public FriendInviteInfoVO DelFriendInvite(ulong uid)
    {
        for (int i = 0; i < m_FriendInviteList.Count; i++)
        {
            if (m_FriendInviteList[i].UID == uid)
            {
				FriendInviteInfoVO result = m_FriendInviteList[i];
                m_FriendInviteList.RemoveAt(i);
                return result;
            }
        }
        return null;
    }

	#endregion
	/// <summary>
	/// 获取proxy
	/// </summary>
	/// <returns></returns>
	public GameplayProxy GetGameplayProxy()
	{
		return m_GameplayProxy == null ? m_GameplayProxy = (GameplayProxy)Facade.RetrieveProxy(ProxyName.GameplayProxy) : m_GameplayProxy;
	}
	public ServerListProxy GetServerListProxy()
	{
		return m_ServerListProxy == null ? m_ServerListProxy = (ServerListProxy)Facade.RetrieveProxy(ProxyName.ServerListProxy) : m_ServerListProxy;
	}
	/// <summary>
	/// 添加数据到附近玩家列表
	/// </summary>
	public void  AddNearbyListData()
	{
		List<BaseEntity> list = GetGameplayProxy().GetPlayEntitiesExcludeSelf();
		m_NearbyList.Clear();
		for (int i = 0; i < list.Count; i++)
		{
			FriendInfoVO friendInfoVO = new FriendInfoVO();
			friendInfoVO.UID = list[i].GetPlayerId();
			Debug.Log("附近的人UID-------->"+ friendInfoVO.UID);
			if (friendInfoVO.UID== GetServerListProxy().GetCurrentCharacterVO().UId)
			{
				return;
			}
			friendInfoVO.Name = list[i].GetName();
			friendInfoVO.Level = (int)list[i].GetLevel();
			friendInfoVO.TID = (int)list[i].GetTemplateID();
			//Debug.Log("附近的人TID-------->" + friendInfoVO.TID);
			if (GetBlack(friendInfoVO.UID) == null)
			{
				m_NearbyList.Add(friendInfoVO);
			}
		}
	}

	/// <summary>
	/// 是否在同一地图
	/// </summary>
	/// <param name="id"></param>
	/// <returns></returns>
	public bool NearbyList(ulong id)
	{
		List<BaseEntity> list = GetGameplayProxy().GetPlayEntitiesExcludeSelf();
		
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i].GetPlayerId()==id)
			{
				return true;
			}
		}
		return false;
	}
}
