using Assets.Scripts.Lib.Net;
using Assets.Scripts.Proto;
using UnityEngine;
namespace Game.Frame.Net
{
    public class FriendController : AbsRpcController
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public FriendController() : base()
        {
			// ListenGameServer(KS2C_Protocol.s2c_reset_friend, OnResetFriend, typeof(S2C_RESET_FRIEND));
			FriendProxy proxy = GameFacade.Instance.RetrieveProxy(ProxyName.FriendProxy) as FriendProxy;
			proxy.Clear();
			NetworkManager.Instance.ListenGameServer(KS2C_Protocol.s2c_add_friend_back, OnAddFriendBack, typeof(S2C_ADD_FRIEND_BACK));
            NetworkManager.Instance.ListenGameServer(KS2C_Protocol.s2c_del_friend_back, OnDelFriendback, typeof(S2C_DEL_FRIEND_BACK));
            NetworkManager.Instance.ListenGameServer(KS2C_Protocol.s2c_add_black_back, OnAddBlackBack, typeof(S2C_ADD_BLACK_BACK));
            NetworkManager.Instance.ListenGameServer(KS2C_Protocol.s2c_del_black_back, OnDelBlackBack, typeof(S2C_DEL_BLACK_BACK));
			NetworkManager.Instance.ListenGameServer(KS2C_Protocol.s2c_sync_friendlist, OnFriendList, typeof(S2C_SYNC_FRIENDLIST));
			NetworkManager.Instance.ListenGameServer(KS2C_Protocol.s2c_sync_togetherlist, OnTogetherList, typeof(S2C_SYNC_TOGETHERLIST));
			NetworkManager.Instance.ListenGameServer(KS2C_Protocol.s2c_sync_friend, OnSyncFriend, typeof(S2C_SYNC_FRIEND));

			NetworkManager.Instance.ListenGameServer(KS2C_Protocol.s2c_sync_playerinfo, OnSyncPlayerInfoBack, typeof(S2C_SYNC_PLAYERINFO));

			NetworkManager.Instance.ListenGameServer(KS2C_Protocol.s2c_set_status_back, OnSetStatusBack, typeof(S2C_SET_STATUS_BACK));
			NetworkManager.Instance.ListenGameServer(KS2C_Protocol.s2c_sync_friend_invite, OnSyncFriendInvite, typeof(S2C_SYNC_FRIEND_INVITE));
            NetworkManager.Instance.ListenGameServer(KS2C_Protocol.s2c_add_friend_invite, OnAddFriendInvite, typeof(S2C_ADD_FRIEND_INVITE));
            NetworkManager.Instance.ListenGameServer(KS2C_Protocol.s2c_del_friend_invite, OnDelFriendInvite, typeof(S2C_DEL_FRIEND_INVITE));
        }

        #region S2C

        /// <summary>
        /// 创建好友数据
        /// </summary>
        /// <param name="data">协议结构</param>
        /// <returns>好友数据</returns>
        private FriendInfoVO CreateFriendInfo(FriendData data)
        {
            FriendInfoVO m_Info = new FriendInfoVO();
            m_Info.UID = data.uid;
			m_Info.TID = (int)data.tid;
			m_Info.Name = data.name;
            m_Info.Level = (int)data.level;
            m_Info.Status = (FriendInfoVO.FriendState)data.status;
            m_Info.AddTime = data.add_time;
			Debug.Log(m_Info.Status +"<=======名字=====>" + m_Info.TID);
			return m_Info;
        }

        /// <summary>
        /// 获取好友数据
        /// </summary>
        /// <param name="buf">协议内容</param>
        private void OnResetFriend(KProtoBuf buf)
        {
            S2C_RESET_FRIEND msg = buf as S2C_RESET_FRIEND;
            FriendProxy proxy = GameFacade.Instance.RetrieveProxy(ProxyName.FriendProxy) as FriendProxy;
            proxy.Clear();
            proxy.m_Status = (FriendInfoVO.FriendState)msg.status;
            proxy.SetFirendListSize(msg.friendMaxCount);
            proxy.SetBlackListSize(msg.blackMaxCount);
            //Debugger.LogError("OnResetFriend "+proxy.m_FirendListSize+","+proxy.m_BlackListSize);
        }
        /// <summary>
        /// 好友当前状态
        /// </summary>
        /// <param name="buf">协议内容</param>
        private void OnSetStatusBack(KProtoBuf buf)
        {
            S2C_SET_STATUS_BACK msg = buf as S2C_SET_STATUS_BACK;
            FriendProxy proxy = GameFacade.Instance.RetrieveProxy(ProxyName.FriendProxy) as FriendProxy;
			//proxy.SetMember(msg.);
        }

        /// <summary>
        /// 好友或黑名单数据变化
        /// </summary>
        /// <param name="buf">协议内容</param>
        private void OnSyncFriend(KProtoBuf buf)
        {
            S2C_SYNC_FRIEND msg = buf as S2C_SYNC_FRIEND;
            FriendProxy proxy = GameFacade.Instance.RetrieveProxy(ProxyName.FriendProxy) as FriendProxy;
            FriendInfoVO data = CreateFriendInfo(msg.friend_syc);
            if (msg.friend_syc.flag == 0)
            {
                proxy.AddFriend(data);
            }
            else
            {
                proxy.AddBlack(data);
            }
            GameFacade.Instance.SendNotification(NotificationName.MSG_FRIEND_LIST_CHANGED, data);
            //Debugger.LogError("OnSyncFriend 1 >" + data.UID + "." + data.Name);
        }

        /// <summary>
        /// 好友添加
        /// </summary>
        /// <param name="buf">协议内容</param>
        private void OnAddFriendBack(KProtoBuf buf)
        {
            S2C_ADD_FRIEND_BACK msg = buf as S2C_ADD_FRIEND_BACK;
			Debug.Log("接收Add Friend========>>>>>" + msg.code);
			FriendProxy proxy = GameFacade.Instance.RetrieveProxy(ProxyName.FriendProxy) as FriendProxy;

            if (msg.code == 1)
            {
                FriendInfoVO data = CreateFriendInfo(msg.friend_add);
                proxy.DelFriend(data.UID);
                proxy.DelBlack(data.UID);
                if (msg.friend_add.flag == 0)
                {
                    proxy.AddFriend(data);
				}
                else
                {
                   // proxy.AddBlack(data);
                }
                GameFacade.Instance.SendNotification(NotificationName.MSG_FRIEND_LIST_CHANGED, data);
            }
            //Debugger.LogError("OnAddFriendBack ");
        }

        /// <summary>
        /// 删除好友
        /// </summary>
        /// <param name="buf">协议内容</param>
        private void OnDelFriendback(KProtoBuf buf)
        {
            S2C_DEL_FRIEND_BACK msg = buf as S2C_DEL_FRIEND_BACK;
			Debug.Log("接收del Friend========>>>>>" + msg.code);
			FriendProxy proxy = GameFacade.Instance.RetrieveProxy(ProxyName.FriendProxy) as FriendProxy;

            if (msg.code == 1)
            {
                var friend = proxy.DelFriend(msg.uid);
                if (friend != null)
                {
                    GameFacade.Instance.SendNotification(NotificationName.MSG_FRIEND_LIST_CHANGED, friend);
                }
                else
                {
                    var black = proxy.DelBlack(msg.uid);
                    if (black != null)
                    {
                        GameFacade.Instance.SendNotification(NotificationName.MSG_FRIEND_LIST_CHANGED, black);
                    }
                }
            }
            //Debugger.LogError("OnDelFriendback ");
        }

        /// <summary>
        /// 添加黑名单
        /// </summary>
        /// <param name="buf">协议内容</param>
        private void OnAddBlackBack(KProtoBuf buf)
        {
            S2C_ADD_BLACK_BACK msg = buf as S2C_ADD_BLACK_BACK;

            FriendProxy proxy = GameFacade.Instance.RetrieveProxy(ProxyName.FriendProxy) as FriendProxy;
			if (msg.code == 1)
            {
                FriendInfoVO data = CreateFriendInfo(msg.friend_black);
                proxy.DelFriend(data.UID);
                proxy.DelBlack(data.UID);
                if (msg.friend_black.flag == 0)
                {
                    proxy.AddFriend(data);
                }
                else
                {
                    proxy.AddBlack(data);
                }
                GameFacade.Instance.SendNotification(NotificationName.MSG_FRIEND_LIST_CHANGED, data);
                //Debugger.LogError("add black");
            }
            //Debugger.LogError("OnAddBlackBack ");
        }

		/// <summary>
		/// 删除好友
		/// </summary>
		/// <param name="buf">协议内容</param>
		private void OnDelBlackBack(KProtoBuf buf)
		{
			S2C_DEL_BLACK_BACK msg = buf as S2C_DEL_BLACK_BACK;
			//Debug.Log("接收DelBlack========>>>>>" + msg.code);
			FriendProxy proxy = GameFacade.Instance.RetrieveProxy(ProxyName.FriendProxy) as FriendProxy;

			if (msg.code == 1)
			{
				var friend = proxy.DelFriend(msg.uid);
				if (friend != null)
				{
					GameFacade.Instance.SendNotification(NotificationName.MSG_FRIEND_LIST_CHANGED, friend);
				}
				else
				{
					var black = proxy.DelBlack(msg.uid);
					if (black != null)
					{
						GameFacade.Instance.SendNotification(NotificationName.MSG_FRIEND_LIST_CHANGED, black);
					}
				}
			}
			//Debugger.LogError("OnDelFriendback ");
		}

		/// <summary>
		/// 获取好友列表
		/// </summary>
		/// <param name="buf"></param>
		private void OnFriendList(KProtoBuf buf)
		{
			S2C_SYNC_FRIENDLIST msg = buf as S2C_SYNC_FRIENDLIST;
			FriendProxy proxy = GameFacade.Instance.RetrieveProxy(ProxyName.FriendProxy) as FriendProxy;
			for (int i = 0; i < msg.friendlist.Count; i++)
			{
				FriendInfoVO data = CreateFriendInfo(msg.friendlist[i]);
				if (msg.friendlist[i].flag == 0)
				{
					proxy.AddFriend(data);
				}
				else
				{
					proxy.AddBlack(data);
				}
			}
			GameFacade.Instance.SendNotification(NotificationName.MSG_FRIEND_LIST_CHANGED);
			//Debugger.LogError("OnSyncFriend 1 >" + data.UID + "." + data.Name);
		}

		/// <summary>
		/// 获取共同玩过的列表
		/// </summary>
		/// <param name="buf"></param>
		private void OnTogetherList(KProtoBuf buf)
		{
			S2C_SYNC_TOGETHERLIST msg = buf as S2C_SYNC_TOGETHERLIST;
		//	Debug.Log("ssssssssssssss");
			FriendProxy proxy = GameFacade.Instance.RetrieveProxy(ProxyName.FriendProxy) as FriendProxy;
			for (int i = 0; i < msg.togetherlist.Count; i++)
			{
				FriendInfoVO data = CreateFriendInfo(msg.togetherlist[i]);
				proxy.AddRecent(data);
			}

			GameFacade.Instance.SendNotification(NotificationName.MSG_FRIEND_LIST_CHANGED);
		}


		/// <summary>
		/// 同频好友邀请数据
		/// </summary>
		/// <param name="buf">协议内容</param>
		private void OnSyncFriendInvite(KProtoBuf buf)
        {
            S2C_SYNC_FRIEND_INVITE msg = buf as S2C_SYNC_FRIEND_INVITE;
            FriendProxy proxy = GameFacade.Instance.RetrieveProxy(ProxyName.FriendProxy) as FriendProxy;
            FriendInviteInfoVO m_Req = new FriendInviteInfoVO();
            m_Req.ID = msg.data.id;
            m_Req.UID = msg.data.uid;
            m_Req.Name = msg.data.name;
            m_Req.Level = msg.data.level;
            m_Req.AddTime = msg.data.addTime;
            proxy.AddFriendInvite(m_Req);
        }

        /// <summary>
        /// 添加好友邀请列表
        /// </summary>
        /// <param name="buf">协议内容</param>
        private void OnAddFriendInvite(KProtoBuf buf)
        {
            S2C_ADD_FRIEND_INVITE msg = buf as S2C_ADD_FRIEND_INVITE;
            FriendProxy proxy = GameFacade.Instance.RetrieveProxy(ProxyName.FriendProxy) as FriendProxy;
            FriendInviteInfoVO m_Req = new FriendInviteInfoVO();
            m_Req.ID = msg.data.id;
            m_Req.UID = msg.data.uid;
            m_Req.Name = msg.data.name;
            m_Req.Level = msg.data.level;
            m_Req.AddTime = msg.data.addTime;
            proxy.AddFriendInvite(m_Req);
            GameFacade.Instance.SendNotification(NotificationName.MSG_FRIEND_INVITE_LIST_CHANGED);
        }


        /// <summary>
        /// 删除好友邀请列表
        /// </summary>
        /// <param name="buf">协议内容</param>
        private void OnDelFriendInvite(KProtoBuf buf)
        {
            S2C_DEL_FRIEND_INVITE msg = buf as S2C_DEL_FRIEND_INVITE;
            FriendProxy proxy = GameFacade.Instance.RetrieveProxy(ProxyName.FriendProxy) as FriendProxy;
            proxy.DelFriendInvite(msg.id);
            GameFacade.Instance.SendNotification(NotificationName.MSG_FRIEND_INVITE_LIST_CHANGED);
        }

		/// <summary>
		/// 好友当前状态
		/// </summary>
		/// <param name="buf">协议内容</param>
		private void OnSyncPlayerInfoBack(KProtoBuf buf)
		{
			S2C_SYNC_PLAYERINFO msg = buf as S2C_SYNC_PLAYERINFO;

			ShipItemsProxy shipItemsProxy = GameFacade.Instance.RetrieveProxy(ProxyName.ShipItemsProxy) as ShipItemsProxy;
			shipItemsProxy.InitShipItemsByByRespond(msg);
		}
		#endregion

		/// <summary>
		/// 请求好友列表
		/// </summary>
		/// <param name="uid">玩家uid</param>
		public void RequestFriendList()
		{
			C2S_REQ_FRIENDLIST msg = new C2S_REQ_FRIENDLIST();
			msg.protocolID = (ushort)KC2S_Protocol.c2s_req_friendlist;
			//Debug.Log("发送>>>>>>>>>请求好友列表");
			NetworkManager.Instance.SendToGameServer(msg);
		}

		/// <summary>
		/// 请求共同玩过的列表
		/// </summary>
		/// <param name="uid">玩家uid</param>
		public void RequestRecentList()
		{
			C2S_REQ_TOGETHERLIST msg = new C2S_REQ_TOGETHERLIST();
			msg.protocolID = (ushort)KC2S_Protocol.c2s_req_togetherlist;
			NetworkManager.Instance.SendToGameServer(msg);
			Debug.Log("发送>>>>>>>>>请求共同玩过列表");
		}


		/// <summary>
		/// 请求添加指定玩家到好友列表
		/// </summary>
		/// <param name="uid">玩家uid</param>
		public void RequestAddToFriendList(ulong uid)
        {
            FriendProxy proxy = GameFacade.Instance.RetrieveProxy(ProxyName.FriendProxy) as FriendProxy;
            if (proxy.GetBlack(uid) != null)
            {
                RequestDeleteFromBlackList(uid);
            }
            C2S_ADD_FRIEND msg = new C2S_ADD_FRIEND();
            msg.protocolID = (ushort)KC2S_Protocol.c2s_add_friend;
            msg.uid = uid;
            NetworkManager.Instance.SendToGameServer(msg);
        }

        /// <summary>
        /// 请求从好友列表删除指定玩家
        /// </summary>
        /// <param name="uid">玩家id</param>
        public void RequestDeleteFromFriendList(ulong uid)
        {
            C2S_DEL_FRIEND msg = new C2S_DEL_FRIEND();
            msg.protocolID = (ushort)KC2S_Protocol.c2s_del_friend;
            msg.uid = uid;
            NetworkManager.Instance.SendToGameServer(msg);
        }

        /// <summary>
        /// 请求添加指定玩家到到黑名单
        /// </summary>
        /// <param name="uid">玩家ID</param>
        public void RequestAddToBlackList(ulong uid)
        {
            C2S_ADD_BLACK msg = new C2S_ADD_BLACK();
            msg.protocolID = (ushort)KC2S_Protocol.c2s_add_black;
            msg.uid = uid;
            NetworkManager.Instance.SendToGameServer(msg);
        }

        /// <summary>
        /// 请求从黑名单删除指定玩家
        /// </summary>
        /// <param name="uid">玩家ID</param>
        public void RequestDeleteFromBlackList(ulong uid)
        {
            C2S_DEL_FRIEND msg = new C2S_DEL_FRIEND();
            msg.protocolID = (ushort)KC2S_Protocol.c2s_del_black;
            msg.uid = uid;
            NetworkManager.Instance.SendToGameServer(msg);
        }


		/// <summary>
		/// 请求好友玩家详情
		/// </summary>
		/// <param name="uid">玩家ID</param>
		public void RequestPlayerInfo(ulong uid)
		{
			C2S_REQUEST_PLAYERINFO msg = new C2S_REQUEST_PLAYERINFO();
			msg.protocolID = (ushort)KC2S_Protocol.c2s_request_playerinfo;
			msg.id = uid;
			NetworkManager.Instance.SendToGameServer(msg);
		}

		/// <summary>
		/// 接受别人的好友请求
		/// </summary>
		/// <param name="id">请求ID</param>
		public void AcceptInvite(uint id)
        {
            C2S_HANDLE_FRIEND_REQ msg = new C2S_HANDLE_FRIEND_REQ();
            msg.protocolID = (ushort)KC2S_Protocol.c2s_handle_friend_req;
            msg.id = id;
            msg.accept = 1;
            NetworkManager.Instance.SendToGameServer(msg);
        }

        /// <summary>
        /// 拒绝别人的好友请求
        /// </summary>
        /// <param name="id">请求ID</param>
        public void RejectInvite(uint id)
        {
            C2S_HANDLE_FRIEND_REQ msg = new C2S_HANDLE_FRIEND_REQ();
            msg.protocolID = (ushort)KC2S_Protocol.c2s_handle_friend_req;
            msg.id = id;
            msg.accept = 2;
            NetworkManager.Instance.SendToGameServer(msg);
        }
    }
}
