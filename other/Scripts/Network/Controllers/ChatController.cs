using Assets.Scripts.Define;
using Assets.Scripts.Lib.Net;
using Assets.Scripts.Proto;

namespace Game.Frame.Net
{
	public class ChatController : AbsRpcController
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public ChatController() : base()
        {
            ListenServerMessage();
        }

        #region 处理服务器端消息

        /// <summary>
        /// 绑定服务器消息
        /// </summary>
        private void ListenServerMessage()
        {
            NetworkManager.Instance.ListenGameServer(KS2C_Protocol.s2c_error_code, OnErrorCode, typeof(S2C_ERROR_CODE));
            NetworkManager.Instance.ListenGameServer(KS2C_Protocol.s2c_chat_message, OnChatMessage, typeof(S2C_CHAT_MESSAGE));
        }

		/// <summary>
		/// 收到聊天消息
		/// </summary>
		/// <param name="buf"></param>
        private void OnChatMessage(KProtoBuf buf)
        {
			S2C_CHAT_MESSAGE msg = buf as S2C_CHAT_MESSAGE;

			ChatProxy proxy = GameFacade.Instance.RetrieveProxy(ProxyName.ChatProxy) as ChatProxy;

            proxy.AddMessage(s2c_channel(msg.channel), msg.message, msg.fromID, msg.fromName);
        }

        /// <summary>
        /// 收到错误提示
        /// </summary>
        /// <param name="buf"></param>
        private void OnErrorCode(KProtoBuf buf)
        {
			S2C_ERROR_CODE msg = buf as S2C_ERROR_CODE;

			ChatProxy chatProxy = GameFacade.Instance.RetrieveProxy(ProxyName.ChatProxy) as ChatProxy;
			CfgLanguageProxy languageProxy = GameFacade.Instance.RetrieveProxy(ProxyName.CfgLanguageProxy) as CfgLanguageProxy;

			switch ((KErrorCode)msg.error)
            {
                case KErrorCode.errSendBlackMsg:
					chatProxy.AddMessage(chatProxy.CurrentChannel, languageProxy.GetLocalization(3007));
                    break;
                case KErrorCode.errSendNotOnLineMsg:
					chatProxy.AddMessage(chatProxy.CurrentChannel, languageProxy.GetLocalization(3006));
                    break;
            }
        }

		#endregion

		#region 发送GM指令
		public void SendGM_OtherKillPlayer()
		{
			SendGM("killplayer");
		}

		public void SendGM_KillPlayer(ulong playerID)
		{
			SendGM($"killplayer {playerID}");
		}

		public void SendGM_AddNpc(int npcID)
		{
			SendGM($"addnpc {npcID}");
		}

		public void SendGM_AddNpc(int npcID, UnityEngine.Vector3 position)
		{
			SendGM($"addnpc {npcID} {position.x} {position.y} {position.z}");
		}

		public void SendGM(string message)
		{
			message = $"/gm {message}";
			Leyoutech.Utility.DebugUtility.Log("GM", $"Send GM({message})");
			OnChatSend(ChatChannel.World, message);
		}
		#endregion

		#region 处理客户端消息

		/// <summary>
		/// 发送消息到服务器
		/// </summary>
		/// <param name="channel">频道</param>
		/// <param name="message">内容</param>
		/// <param name="talkTargetPlayerID">私聊对象ID</param>
		public void OnChatSend(ChatChannel channel, string message, ulong talkTargetPlayerID = 0)
		{
			var msg = new C2S_CHAT_MESSAGE();
			msg.protocolID = (ushort)KC2S_Protocol.c2s_chat_message;
			msg.channel = c2s_channel(channel);
			msg.message = message;

			if (talkTargetPlayerID != 0)
			{
				msg.toID = talkTargetPlayerID;
			}

			//Debugger.LogError("OnChatSend:" + msg.channel+":"+msg.message+"  =>"+msg.toID);

			NetworkManager.Instance.SendToGameServer(msg);
		}

        #endregion

        /// <summary>
        /// 转换后端的频道ID到前端的频道ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private ChatChannel s2c_channel(int id)
        {
            switch (id)
            {
                case 0: return ChatChannel.Whisper; //PERSONAL = 0;     //个人
                case 5: return ChatChannel.World;    //ROOM_WORD = 5;    //世界
                case 2: return ChatChannel.Group;    //ROOM_TEAM = 2;    // 队伍
                case 3: return ChatChannel.Union;    //ROOM_GROUP = 3;   //群组
                case 4: return ChatChannel.Station;  //ROOM_SCENE = 4;   //场景
                case 1: return ChatChannel.Faction;  //ROOM_SOCIATY = 1; //公会
            }

            return ChatChannel.NONE;
        }

        /// <summary>
        /// 转换前端的频道ID到后端的频道ID
        /// </summary>
        /// <param name="channel"></param>
        /// <returns></returns>
        private ushort c2s_channel(ChatChannel channel)
        {
            switch (channel)
            {
                case ChatChannel.Whisper: return 0; //PERSONAL = 0;     //个人
                case ChatChannel.World: return 5; //ROOM_WORD = 5;    //世界
                case ChatChannel.Group: return 2; //ROOM_TEAM = 2;    // 队伍
                case ChatChannel.Union: return 3; //ROOM_GROUP = 3;   //群组
                case ChatChannel.Station: return 4; //ROOM_SCENE = 4;   //场景
                case ChatChannel.Faction: return 1; //ROOM_SOCIATY = 1; //公会
            }

            return 5;
        }
    }
}
