using Assets.Scripts.Define;
using Assets.Scripts.Lib.Net;
using Assets.Scripts.Proto;
using Game.Frame.Net;
using System.Collections.Generic;

/// <summary>
/// 副本，复活，相关
/// </summary>
public class InstanceController : BaseNetController
{
	/// <summary>
	/// 复活类型
	/// </summary>
    private int m_ReliveType = 0;
	/// <summary>
	/// 复活时间
	/// </summary>
    private int m_ReliveTime = 0;


    /// <summary>
    /// 获得复活类型
    /// </summary>
    /// <returns></returns>
    public int GetReliveType()
    {
        return m_ReliveType;
    }

    /// <summary>
    /// 获得复活时间
    /// </summary>
    /// <returns></returns>
    public int GetReliveTime()
    {
        return m_ReliveTime;
    }



    public InstanceController()
    {

        NetworkManager.Instance.ListenGameServer(KS2C_Protocol.s2c_pve_end, OnPveEnd, typeof(S2C_PVE_END));
        NetworkManager.Instance.ListenGameServer(KS2C_Protocol.s2c_error_code, OnErrorCode, typeof(S2C_ERROR_CODE));

        NetworkManager.Instance.ListenGameServer(KS2C_Protocol.s2c_pve_mission_value_change, OnInstanceMissionChange, typeof(S2C_PVE_MISSION_VALUE_CHANGE));
        NetworkManager.Instance.ListenGameServer(KS2C_Protocol.s2c_pve_mission_values, OnInstanceMissionDatas, typeof(S2C_PVE_MISSION_VALUES));

        NetworkManager.Instance.ListenGameServer(KS2C_Protocol.s2c_reward_list, OnPveReward, typeof(S2C_REWARD_LIST));
        NetworkManager.Instance.ListenGameServer(KS2C_Protocol.s2c_fight_result, OnPveFightSettlement, typeof(S2C_FIGHT_RESULT));

        NetworkManager.Instance.ListenGameServer(KS2C_Protocol.s2c_show_tips_info, OnInstanceTip, typeof(S2C_SHOW_TIPS_INFO));

        NetworkManager.Instance.ListenGameServer(KS2C_Protocol.s2c_syn_fight_status, OnFightStatus, typeof(S2C_SYN_FIGHT_STATUS));
    }

    private void OnFightStatus(KProtoBuf buf)
    {

    }

    /// <summary>
    /// 副本结束
    /// </summary>
    /// <param name="buf"></param>
    private void OnPveEnd(KProtoBuf buf)
    {
		PveProxy pveProxy = (PveProxy)GameFacade.Instance.RetrieveProxy(ProxyName.PveProxy);
		if (pveProxy.m_ExitSended) return;

        var msg = buf as S2C_PVE_END;

        C2S_ExitPve req = SingleInstanceCache.GetInstanceByType<C2S_ExitPve>();
        req.protocolID = (ushort)KC2S_Protocol.c2s_exit_pve;
        NetworkManager.Instance.SendToGameServer(req);

        pveProxy.m_ExitSended = true;
    }

    /// <summary>
    /// 处理错误
    /// </summary>
    /// <param name="buf"></param>
    private void OnErrorCode(KProtoBuf buf)
    {
        var msg = buf as S2C_ERROR_CODE;

		CfgLanguageProxy proxy = (CfgLanguageProxy)GameFacade.Instance.RetrieveProxy(ProxyName.CfgLanguageProxy);
        if (msg.error == (int)KErrorCode.errSwitchMapCountLimit)
        {
			GameFacade.Instance.SendNotification(NotificationName.MSG_HUD_NOTICE, proxy.GetLocalization(20001));
        }
        else if (msg.error == (int)KErrorCode.errSwitchMapNoOpen)
        {
			GameFacade.Instance.SendNotification(NotificationName.MSG_HUD_NOTICE, proxy.GetLocalization(20002));
        }
        else if (msg.error == (int)KErrorCode.errSwitchMapPlayerLimit)
        {
			GameFacade.Instance.SendNotification(NotificationName.MSG_HUD_NOTICE, proxy.GetLocalization(20003));
        }
        else if (msg.error == (int)KErrorCode.errWithNoShip)
        {
            GameFacade.Instance.SendNotification(NotificationName.MSG_HUD_NOTICE, proxy.GetLocalization(20004));
        }
    }


    #region 副本任务
	/// <summary>
	///副本任务改变
	/// </summary>
	/// <param name="buf"></param>
    private void OnInstanceMissionChange(KProtoBuf buf)
    {
        var msg = buf as S2C_PVE_MISSION_VALUE_CHANGE;
		PveProxy proxy = (PveProxy)GameFacade.Instance.RetrieveProxy(ProxyName.PveProxy);

		var info = new PveMissionInfoVO();
        info.ID = msg.mission_id;
        info.Value = msg.value;

        proxy.SetPveMission(info);
        proxy.ResetCountdown(msg.left_milisecond/1000);
    }
	/// <summary>
	/// 副本任务数据
	/// </summary>
	/// <param name="buf"></param>
    private void OnInstanceMissionDatas(KProtoBuf buf)
    {
        var msg = buf as S2C_PVE_MISSION_VALUES;
		PveProxy proxy = (PveProxy)GameFacade.Instance.RetrieveProxy(ProxyName.PveProxy);

		var list = new List<PveMissionInfoVO>();
        for(int i=0;i<msg.values.Count;i++)
        {
            var info = new PveMissionInfoVO();
            info.ID = msg.values[i].mission_id;
            info.Value = msg.values[i].value;
            list.Add(info);
        }
        proxy.UpdatePveMission(list);
        proxy.ResetCountdown(msg.left_milisecond / 1000);
    }

    #region 结算消息
	/// <summary>
	/// pve 结算奖励
	/// </summary>
	/// <param name="buf"></param>
    private void OnPveReward(KProtoBuf buf)
    {
        var msg = buf as S2C_REWARD_LIST;

        var list = new List<RewardDateVO>();
        foreach (var reward in msg.rewards)
        {
            var item = new RewardDateVO();
            item.Id = (uint)reward.id;
            item.Num = reward.count;
            list.Add(item);
        }
		PveProxy pveProxy = (PveProxy)GameFacade.Instance.RetrieveProxy(ProxyName.PveProxy);
		pveProxy.SetSettlementRewards((int)msg.result, list);
    }
	/// <summary>
	/// pve 战斗结算处理
	/// </summary>
	/// <param name="buf"></param>
    private void OnPveFightSettlement(KProtoBuf buf)
    {
        var msg = buf as S2C_FIGHT_RESULT;

        var list = new List<PveFightInfoVO>();
        foreach(var value in msg.player_results)
        {
            var info = new PveFightInfoVO();
            //info.PlayerID = value.playerID;
            //info.PlayerName = value.playerName;
            info.DpsCount = value.damage;
            info.HurtCount = value.damaged;
            info.KillCount = value.kill_count;
            info.DeathCount = value.die_count;
            list.Add(info);
        }
		PveProxy pveProxy = (PveProxy)GameFacade.Instance.RetrieveProxy(ProxyName.PveProxy);

		pveProxy.SetSettlementFightInfos(list);
    }

    #endregion
	/// <summary>
	/// server 返回实例tip
	/// </summary>
	/// <param name="buf"></param>
    private void OnInstanceTip(KProtoBuf buf)
    {
        var msg = buf as S2C_SHOW_TIPS_INFO;

        //UnityEngine.Debug.LogError("OnInstanceTip({" + msg.tips_id + "})");
    }

    #endregion


    
}
