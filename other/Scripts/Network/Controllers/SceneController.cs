using Assets.Scripts.Lib.Net;
using Assets.Scripts.Proto;
using Crucis.Protocol;
using Game.Frame.Net;
using System;
using UnityEngine;
using UnityEngine.Assertions;
using Utils.Timer;
public class SceneController : BaseNetController
{
	/// <summary>
	/// 转换到客户端精度
	/// </summary>
	public const float SPACE_ACCURACY_TO_CLIENT = 0.001f;

    /// <summary>
    /// 技能单位精度
    /// </summary>
    public const float SKILL_PRECISION =1f;

    /// <summary>
    /// 子弹跟目标点最先判断距离，认为飞行结束
    /// </summary>
    public const float BULLET_OVER_DISTANCE = 0.2f;

    /// <summary>
    /// 是否同步场景时间戳
    /// </summary>
    public bool m_SceneTimeStampSync;

	/// <summary>
	/// 时间戳延迟
	/// </summary>
	private ulong m_TimeStampDelay;

    /// <summary>
    /// 时间线同步Timer
    /// </summary>
    private uint m_TimerSyncSceneTimeStamp = 0;

    public SceneController()
	{
		NetworkManager.Instance.ListenGameServer(KS2C_Protocol.s2c_switch_map, OnSwitchMap, typeof(S2C_SWITCH_MAP));
		NetworkManager.Instance.ListenGameServer(KS2C_Protocol.s2c_sync_scene_time_frame, OnSyncSceneTimeStamp, typeof(S2C_SYNC_SCENE_TIME_FRAME));       
    }

	/// <summary>
	/// 切换地图
	/// </summary>
	/// <param name="buf">协议内容</param>
	private void OnSwitchMap(KProtoBuf buf)
	{
		S2C_SWITCH_MAP respond = buf as S2C_SWITCH_MAP;

		CfgEternityProxy cfgEternityProxy = GameFacade.Instance.RetrieveProxy(ProxyName.CfgEternityProxy) as CfgEternityProxy;
		if (cfgEternityProxy.GetCurrentMapData().ByteBuffer == null || cfgEternityProxy.GetCurrentMapId() != respond.mapID)
        {
            uint gamingMapId = respond.mapID;
            GameFacade.Instance.SendNotification(NotificationName.MSG_SWITCH_SCENE_START);
            
            cfgEternityProxy.SetCurrentMapData(gamingMapId);

            Vector3 worldPos = new Vector3(respond.posX, respond.posY, respond.posZ);
            ulong areaId = respond.area_id;
            GameMainMediator gameMainMediator = GameFacade.Instance.RetrieveMediator(UIPanel.GameMainMediator) as GameMainMediator;
			if (gameMainMediator == null)
			{
				gameMainMediator = new GameMainMediator();
				GameFacade.Instance.RegisterMediator(gameMainMediator);
				gameMainMediator.SwtichMap(areaId,worldPos);
			}
			else
			{
                gameMainMediator.SwtichMap(areaId, worldPos);
            }
                     
            // TODO.
            //切图直接清缓存
            MSAIBossProxy aiproxy = GameFacade.Instance.RetrieveProxy(ProxyName.MSAIBossProxy) as MSAIBossProxy;
			aiproxy.CleanAllMonster();

			MineDropItemManager.Instance.ClearAllInfo();
			DropItemManager.Instance.ClearInfo();
		}
	}
	/// <summary>
	/// 时间戳
	/// </summary>
	/// <returns></returns>
	public ulong TimeStamp()
	{
		return (ulong)(DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000 + m_TimeStampDelay;
	}

	/// <summary>
	/// 同步时间戳
	/// </summary>
	/// <param name="buf"></param>
	private void OnSyncSceneTimeStamp(KProtoBuf buf)
	{
		S2C_SYNC_SCENE_TIME_FRAME respond = buf as S2C_SYNC_SCENE_TIME_FRAME;
		m_TimeStampDelay = respond.timeFrame - (ulong)(DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000;
		m_SceneTimeStampSync = true;

        if (m_TimerSyncSceneTimeStamp == 0)
        {
            m_TimerSyncSceneTimeStamp = RealTimerUtil.Instance.Register(1.0f, RealTimerUtil.EVERLASTING, OnAdjustmentTimeTimer);
        }
    }
	/// <summary>
	/// 同步时间
	/// </summary>
	/// <param name="times"></param>
	private void OnAdjustmentTimeTimer(int times)
	{
		C2S_CLIENT_FRAME msg = new C2S_CLIENT_FRAME();
		msg.protocolID = (ushort)KC2S_Protocol.c2s_client_frame;
		msg.client_frame = (ulong)ClockUtil.Instance().GetMillisecond();
		NetworkManager.Instance.SendToGameServer(msg);
	}

	/// <summary>
	/// 传送
	/// </summary>
	/// <param name="teleportId"></param>
	public void SwitchMap(uint teleportId)
	{
		Debug.Log("[LOG] SceneController.SwitchMap: " + teleportId);
        C2S_SWITCH_MAP msg = SingleInstanceCache.GetInstanceByType<C2S_SWITCH_MAP>();
		msg.protocolID = (ushort)KC2S_Protocol.c2s_switch_map;
		msg.telportID = teleportId;
		msg.chanel_id = (GameFacade.Instance.RetrieveProxy(ProxyName.CfgEternityProxy) as CfgEternityProxy).GetTeleportChanelId(teleportId);
        NetworkManager.Instance.SendToGameServer(msg);
	}

    /// <summary>
    /// 请求开始跃迁
    /// </summary>
    public void RequestStartLeap(ulong currentAreaID,ulong targetAreaID)
    {
        GameplayProxy gameplayProxy = GameFacade.Instance.RetrieveProxy(ProxyName.GameplayProxy) as GameplayProxy;
        SpacecraftEntity spacecraftEntity = gameplayProxy.GetMainPlayer();
        Assert.IsTrue(spacecraftEntity != null, "gameplayProxy.GetMainPlayer() is null !!!");
        Rigidbody rigidbody = spacecraftEntity.GetRigidbody();
        Assert.IsTrue(rigidbody != null, "rigidbody is null !!!");

        HeroMoveHandler.SyncLeap(
            targetAreaID,
            spacecraftEntity.GetCurrentState().GetOnlyServerState(),
            (uint)currentAreaID,
            gameplayProxy.ClientToServerAreaOffset(rigidbody.position),
            rigidbody.rotation,
            rigidbody.velocity,
            rigidbody.angularVelocity);
    }

    /// <summary>
    /// 请求终止跃迁
    /// </summary>
    public void RequestStopLeap(ulong currentAreaID, ulong targetAreaID)
    {
        GameplayProxy gameplayProxy = GameFacade.Instance.RetrieveProxy(ProxyName.GameplayProxy) as GameplayProxy;
        SpacecraftEntity spacecraftEntity = gameplayProxy.GetMainPlayer();
        Assert.IsTrue(spacecraftEntity != null, "gameplayProxy.GetMainPlayer() is null !!!");
        Rigidbody rigidbody = spacecraftEntity.GetRigidbody();
        Assert.IsTrue(rigidbody != null, "rigidbody is null !!!");

        HeroMoveHandler.SyncLeapCancel(
             spacecraftEntity.GetCurrentState().GetOnlyServerState(),
             (uint)currentAreaID,
             gameplayProxy.ClientToServerAreaOffset(rigidbody.position),
             rigidbody.rotation,
             rigidbody.velocity,
             rigidbody.angularVelocity);
    }
}

