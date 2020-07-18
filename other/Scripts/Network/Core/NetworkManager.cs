using Assets.Scripts.Lib.Net;
using Assets.Scripts.Proto;
using Crucis.Protocol;
using Game.Frame.Net;
using System;
using System.IO;
using System.Text;
using UnityEngine;

public class NetworkManager : Singleton<NetworkManager>
{
	private const int HEADER_LENGTH = sizeof(ushort);
	private const int PROTOCOLID_LENGTH = sizeof(ushort);
	/// <summary>
	/// 和歆哥确认过单条消息不会超过2048
	/// </summary>
	private const int DEFAULT_SEND_BUFFER_SIZE = 2048;

	private SocketClient m_SocketClient;

	private BinaryWriter m_SendWriter;

	private BetterList<BaseNetController> m_NetControllers;

	private ProtocolMapping m_GatewayProtocolMapping;
	private ProtocolMapping m_GameServerProtocolMapping;

	private BinaryWriter m_SendToGameServerWriter;
	private KC2G_PlayerToGS m_PlayerToGSProto;

	/// <summary>
	/// 握手是否成功
	/// </summary>
	private bool m_Handshake;
	private string m_MessageToken;
	private uint m_MessageTokenHash;
	private uint m_MessageIndex;

	private LoginController m_LoginController;
	private CharacterListController m_CharacterListController;
	private SceneController m_SceneController;
    private PlayerController m_PlayerController;
	private MissionController m_MissionController;
	private MailController m_MailController;
	private LogController m_LogController;
	private PackageController m_PackageController;
    private ShopController m_ShopController;
	private InstanceController m_InstanceController;
	private FoundryController m_FoundryController;
	private FriendController m_FriendController;
	private TeamController m_TeamController;
	private Game.Frame.Net.ChatController m_ChatContoller;
	private SkillController m_SkillController;
	private DropItemController m_DropItemController;
	private ReliveController m_reliveController;
	private MSAIBossController m_MSAIBossController;

    /// <summary>
    /// 初始化
    /// </summary>
    public void Initialize()
	{
		m_SocketClient = new SocketClient();

		m_SendWriter = new BinaryWriter(new MemoryStream(DEFAULT_SEND_BUFFER_SIZE));

		m_NetControllers = new BetterList<BaseNetController>();

		m_GatewayProtocolMapping = new ProtocolMapping();
		m_GameServerProtocolMapping = new ProtocolMapping();

		m_SendToGameServerWriter = new BinaryWriter(new MemoryStream(DEFAULT_SEND_BUFFER_SIZE));
		m_PlayerToGSProto = SingleInstanceCache.GetInstanceByType<KC2G_PlayerToGS>();

		m_Handshake = false;
		m_MessageToken = "";
		m_MessageTokenHash = 0;
		m_MessageIndex = 0;

		ListenGateway(KG2C_Protocol.g2c_gs_to_player, OnReceivedGameServerMessage, typeof(KG2C_GSToPlayer));

		#region NetController
		m_LoginController = new LoginController();
		m_CharacterListController = new CharacterListController();
		m_SceneController = new SceneController();
        m_PlayerController = new PlayerController();
		m_MailController = new MailController();
		m_LogController = new LogController();
		m_FriendController = new FriendController();
		m_TeamController = new TeamController();
		m_PackageController = new PackageController();
        m_ShopController = new ShopController();
		m_FoundryController = new FoundryController();
		m_InstanceController = new InstanceController();
		m_MissionController = new MissionController();
		m_ChatContoller = new Game.Frame.Net.ChatController();
		m_SkillController = new SkillController();
		m_DropItemController = new DropItemController();
		m_reliveController = new ReliveController();
		m_MSAIBossController = new MSAIBossController();
        #endregion
    }

	#region Get Net Controller
	/// <summary>
	/// 登录消息处理器
	/// </summary>
	public LoginController GetLoginController()
	{
		return m_LoginController;
	}

	/// <summary>
	/// 角色列表处理器
	/// </summary>
	public CharacterListController GetCharacterListController()
	{
		return m_CharacterListController;
	}

	/// <summary>
	/// SceneController
	/// </summary>
	/// <returns></returns>
	public SceneController GetSceneController()
	{
		return m_SceneController;
	}
    /// <summary>
    /// 玩家
    /// </summary>
    /// <returns></returns>
    public PlayerController GetPlayerController()
    {
        return m_PlayerController;
    }
	/// <summary>
	/// 任务
	/// </summary>
	/// <returns></returns>
	public MissionController GetMissionController()
	{
		return m_MissionController;
	}

	/// <summary>
	/// mail
	/// </summary>
	/// <returns></returns>
	public MailController GetMailController()
	{
		return m_MailController;
	}

	/// <summary>
	/// 日志
	/// </summary>
	/// <returns></returns>
	public LogController GetLogController()
	{
		return m_LogController;
	}

	/// <summary>
	/// 背包
	/// </summary>
	/// <returns></returns>
	public PackageController GetPackageController()
	{
		return m_PackageController;
	}

	/// <summary>
	/// 好友处理器
	/// </summary>
	/// <returns></returns>
	public FriendController GetFriendController()
	{
		return m_FriendController;
	}

	/// <summary>
	/// 副本，复活，相关
	/// </summary>
	/// <returns></returns>
	public InstanceController GetInstanceController()
	{
		return m_InstanceController;
	}

	/// <summary>
	/// 生产消息处理器
	/// </summary>
	public FoundryController GetFoundryController()
	{
		return m_FoundryController;
	}

	/// <summary>
	/// 队伍消息处理器
	/// </summary>
	/// <returns></returns>
	public TeamController GetTeamController()
	{
		return m_TeamController;
	}
    public ShopController GetShopController()
    {
        return m_ShopController;
    }
	/// <summary>
	/// 聊天消息处理器
	/// </summary>
	/// <returns></returns>
	public Game.Frame.Net.ChatController GetChatController()
	{
		return m_ChatContoller;
	}

	/// <summary>
	/// 技能消息处理器
	/// </summary>
	/// <returns></returns>
	public SkillController GetSkillController()
	{
		return m_SkillController;
	}

	/// <summary>
	/// 掉落消息处理器
	/// </summary>
	/// <returns></returns>
	public DropItemController GetDropItemController()
	{
		return m_DropItemController;
	}

	/// <summary>
	/// 复活消息处理
	/// </summary>
	/// <returns></returns>
	public ReliveController GetReliveController()
	{
		return m_reliveController;

	}

	/// <summary>
	/// AI战场消息(微软)
	/// </summary>
	/// <returns></returns>
	public MSAIBossController GetMSAIBossController()
	{
		return m_MSAIBossController;
	}

    #endregion

    /// <summary>
    /// 发送到网关
    /// </summary>
    public void SendToGatewayServer(KC2G_PROTOCOL_HEADER proto)
	{
        m_SendWriter.Seek(HEADER_LENGTH, SeekOrigin.Begin);
        proto.Pack(m_SendWriter);
        int length = (int)m_SendWriter.Seek(0, SeekOrigin.Current);
        m_SendWriter.Seek(0, SeekOrigin.Begin);
        m_SendWriter.Write((ushort)length);
        m_SendWriter.Flush();

        LogDetail("Send to Gateway", proto, proto.byProtocol, ((KC2G_Protocol)proto.byProtocol).ToString(), length);

		m_SocketClient.Send((m_SendWriter.BaseStream as MemoryStream).GetBuffer(), length);
	}

	/// <summary>
	/// 监视网关
	/// </summary>
	public void ListenGateway(KG2C_Protocol protocolId, ProtocolHandler handler, Type type)
	{
		m_GatewayProtocolMapping.AddProtocolHandler((int)protocolId, handler, type);
	}

	/// <summary>
	/// 发送到GameServer
	/// </summary>
	public void SendToGameServer(C2S_HEADER proto)
	{
		// 握手失败，不需要发消息
		if (!m_Handshake)
		{
			return;
		}

		m_PlayerToGSProto.byProtocol = (byte)KC2G_Protocol.c2g_player_to_gs;
		m_SendToGameServerWriter.Seek(HEADER_LENGTH, SeekOrigin.Begin);
		proto.Pack(m_SendToGameServerWriter);
		m_PlayerToGSProto._dataLength_ = (int)m_SendToGameServerWriter.Seek(0, SeekOrigin.Current);
		m_SendToGameServerWriter.Seek(0, SeekOrigin.Begin);
		m_SendToGameServerWriter.Write((ushort)(m_PlayerToGSProto._dataLength_ - HEADER_LENGTH));
		m_SendToGameServerWriter.Flush();
		m_PlayerToGSProto.data = (m_SendToGameServerWriter.BaseStream as MemoryStream).GetBuffer();

		#region 可能是加密、验证
		m_PlayerToGSProto.uSign = m_MessageTokenHash * m_MessageIndex * m_MessageToken[(int)(m_MessageIndex % m_MessageToken.Length)] * 47987731;
		m_PlayerToGSProto.uSign |= m_MessageToken[(int)((m_MessageIndex) % m_MessageToken.Length)];

		if (!string.IsNullOrEmpty(m_MessageToken))
		{
			int index = 0;
			for (int iData = sizeof(ushort); iData < m_PlayerToGSProto._dataLength_; iData++)
			{
				m_PlayerToGSProto.data[iData] = (byte)(m_PlayerToGSProto.data[iData] ^ m_MessageToken[index++ % m_MessageToken.Length]);
			}
		}

		m_MessageIndex++;
		m_MessageIndex += m_MessageToken[(int)(m_MessageIndex % m_MessageToken.Length)];
        #endregion

        LogDetail("Send to GameServer", proto, proto.protocolID, ((KC2S_Protocol)proto.protocolID).ToString(), m_PlayerToGSProto._dataLength_);

		SendToGatewayServer(m_PlayerToGSProto);
	}

	/// <summary>
	/// 监视GameServer
	/// </summary>
	/// <param name="protocolId"></param>
	/// <param name="handler"></param>
	/// <param name="type"></param>
	public void ListenGameServer(KS2C_Protocol protocolId, ProtocolHandler handler, Type type)
	{
		m_GameServerProtocolMapping.AddProtocolHandler((int)protocolId, handler, type);
	}

	internal void _AddController(BaseNetController controller)
	{
		Leyoutech.Utility.DebugUtility.LogVerbose(KConstants.LOG_TAG
			, string.Format("NetCore add NetController: ({0})"
				, controller.GetType()));

		m_NetControllers.Add(controller);
	}

	internal SocketClient _GetSocketClient()
	{
		return m_SocketClient;
	}

	internal void _SetHandshake(bool handshake)
	{
		Leyoutech.Utility.DebugUtility.Log(KConstants.LOG_TAG
			, "握手 " + handshake);
		m_Handshake = handshake;
	}

	/// <summary>
	/// 重置Token和索引
	/// </summary>
	/// <param name="token"></param>
	internal void _ResetTokenAndIndex(string token)
	{
		m_MessageToken = token;
		m_MessageTokenHash = 0;
		m_MessageIndex = 0;

		uint seed = (uint)(token[0] & token[token.Length - 1]);
		for (int iToken = 0; iToken < token.Length; iToken++)
		{
			m_MessageTokenHash = m_MessageTokenHash * seed + token[iToken];
		}

		m_MessageTokenHash = m_MessageTokenHash & 0x7FFFFFFF;
	}

	protected void Update()
	{
		m_LoginController._DoUpdate();
    }

    public void OnResiveMessage(OldPl message)
    {
        MemoryStream memoryStream = new MemoryStream(message.Body.ToByteArray());
        BinaryReader binaryReader = new BinaryReader(memoryStream);
        int packageLength = binaryReader.ReadUInt16();
        int protocolID = binaryReader.ReadUInt16();
        long beginPosition = memoryStream.Seek(-PROTOCOLID_LENGTH, SeekOrigin.Current);

        if (message.Body.Length < packageLength)
        {
            Leyoutech.Utility.DebugUtility.LogError(KConstants.LOG_TAG
                , string.Format("消息长度错误: protocolID:{0}({1})"
                    , protocolID
                    , (KS2C_Protocol)protocolID));
            return;
        }

        ProcessInfo processInfo = m_GatewayProtocolMapping.GetProcessInfo(protocolID);
        if (processInfo == null)
        {
            Leyoutech.Utility.DebugUtility.LogError(KConstants.LOG_TAG
                , string.Format("Gateway消息没有被处理: protocolID:{0}({1})"
                    , protocolID
                    , (KS2C_Protocol)protocolID));
            return;
        }

        processInfo.protocolData.UnPack(binaryReader);

        try
        {
            processInfo.HandleProtocol();
        }
        catch (Exception e)
        {
            Leyoutech.Utility.DebugUtility.LogError(KConstants.LOG_TAG
                , string.Format("处理协议:{0}({1})异常 Exception:\n{2}"
                    , protocolID
                    , (KS2C_Protocol)protocolID
                    , e.ToString()));
        }
    }

    /// <summary>
    /// 退出游戏 发送下线
    /// </summary>
    protected void OnApplicationQuit()
	{
		m_LoginController.ExitCurrentServer(3);
        NetWork.Instance?.Dispose();
	}

	private void OnReceivedGameServerMessage(KProtoBuf proto)
	{
		KG2C_GSToPlayer protoData = proto as KG2C_GSToPlayer;
		BinaryReader reader = new BinaryReader(new MemoryStream(protoData.data));
		while (reader.BaseStream.Position < protoData.data.Length)
		{
			int length = reader.ReadUInt16();
			long beginPosition = reader.BaseStream.Position;
			long endPosition = beginPosition + length;
			int protocolID = reader.ReadUInt16();

			reader.BaseStream.Seek(beginPosition, SeekOrigin.Begin);
			ProcessInfo processInfo = m_GameServerProtocolMapping.GetProcessInfo(protocolID);
			if (processInfo == null)
			{
				Leyoutech.Utility.DebugUtility.LogError(KConstants.LOG_TAG
					, string.Format("GameServer消息没有被处理: protocolID:{0}({1})"
						, protocolID
						, (KS2C_Protocol)protocolID));
				return;
			}

			try
			{
				processInfo.protocolData.UnPack(reader);
				LogDetail("Receive from GameServer", processInfo.protocolData, processInfo.protocolID, ((KS2C_Protocol)processInfo.protocolID).ToString(), length);
				processInfo.HandleProtocol();
			}
			catch (Exception e)
			{
				reader.BaseStream.Seek(beginPosition, SeekOrigin.Begin);
				StringBuilder builder = Leyoutech.Utility.StringUtility.AllocStringBuilderCache();
				for (int iByte = 0; iByte < length; iByte++)
				{
					byte iterByte = reader.ReadByte();
					builder.AppendFormat("{0:X2}", iterByte);
				}
				Leyoutech.Utility.DebugUtility.LogError(KConstants.LOG_TAG
					, string.Format("处理协议:{0}({1})异常 Buff:({2}) Exception:\n{3}"
						, protocolID
						, (KS2C_Protocol)protocolID
						, Leyoutech.Utility.StringUtility.ReleaseStringBuilderCacheAndReturnString()
						, e.ToString()));
			}
			finally
			{
				reader.BaseStream.Seek(endPosition, SeekOrigin.Begin);
			}
		}
	}

	[System.Diagnostics.Conditional(Leyoutech.Utility.DebugUtility.LOG_VERBOSE_CONDITIONAL)]
	private void LogDetail(string log, KProtoBuf proto, int protocolID, string protocolIDString, int length)
	{
		bool oldPrettryPrint = LitJson.JsonMapper.GetStaticJsonWriter().PrettyPrint;
		try
		{
			LitJson.JsonMapper.GetStaticJsonWriter().PrettyPrint = true;
			Leyoutech.Utility.DebugUtility.LogVerbose(KConstants.LOG_TAG
				, string.Format("{0} ProtocolID:{1}({2}) Length:({3})\n{4}"
					, log
					, protocolID
					, protocolIDString
					, length
					, LitJson.JsonMapper.ToJson(proto)));
		}
		catch (Exception e)
		{
			Leyoutech.Utility.DebugUtility.LogError(KConstants.LOG_TAG
				, string.Format("Log {0} ProtocolID:{1}({2}) Exception:\n{3}"
					, log
					, protocolID
					, protocolIDString
					, e.ToString()));
		}
		finally
		{
			LitJson.JsonMapper.GetStaticJsonWriter().PrettyPrint = oldPrettryPrint;
		}
	}
}