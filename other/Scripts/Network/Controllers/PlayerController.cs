using Assets.Scripts.Lib.Net;
using Assets.Scripts.Proto;
using Game.Frame.Net;
using System.Collections.Generic;

public class PlayerController : BaseNetController
{
    /// <summary>
    /// 当前玩家信息
    /// </summary>
    private PlayerInfoVo m_PlayerInfo = new PlayerInfoVo();
    public PlayerInfoVo GetPlayerInfo()
    {
        return m_PlayerInfo;
    }
    public PlayerController()
    {
             
        NetworkManager.Instance.ListenGameServer(KS2C_Protocol.s2c_sync_player_base_info, OnSyncPlayerBaseInfo, typeof(S2C_SYNC_PLAYER_BASE_INFO));
        NetworkManager.Instance.ListenGameServer(KS2C_Protocol.s2c_sync_player_state_info, OnSyncPlayerStateInfo, typeof(S2C_SYNC_PLAYER_STATE_INFO));
        NetworkManager.Instance.ListenGameServer(KS2C_Protocol.s2c_add_exp, OnAddExp, typeof(S2C_ADD_EXP));
        NetworkManager.Instance.ListenGameServer(KS2C_Protocol.s2c_update_level, OnUpdateLevel, typeof(S2C_UPDATE_LEVEL));
        NetworkManager.Instance.ListenGameServer(KS2C_Protocol.s2c_levelup_reward_list, OnRewardList, typeof(S2C_LEVELUP_REWARD_LIST));

        //--------------------------------------------原来PlayerController内容-----------------------------------------
        NetworkManager.Instance.ListenGameServer(KS2C_Protocol.s2c_job_info, (KProtoBuf buf) => { }, typeof(S2C_JOB_INFO));
        NetworkManager.Instance.ListenGameServer(KS2C_Protocol.s2c_sync_new_player_info, (KProtoBuf buf) => { }, typeof(S2C_SYNC_NEW_PLAYER_INFO));
        NetworkManager.Instance.ListenGameServer(KS2C_Protocol.s2c_update_player_info, (KProtoBuf buf) => { }, typeof(S2C_UPDATE_PLAYER_INFO));
        NetworkManager.Instance.ListenGameServer(KS2C_Protocol.s2c_sync_hero_data, (KProtoBuf buf) => { }, typeof(S2C_SYNC_HERO_DATA));

        NetworkManager.Instance.ListenGameServer(KS2C_Protocol.s2c_sync_all_attribute, (KProtoBuf buf) => { }, typeof(S2C_SYNC_ALL_ATTRIBUTE));
        NetworkManager.Instance.ListenGameServer(KS2C_Protocol.s2c_money_infos, (KProtoBuf buf) => { }, typeof(S2C_MONEY_INFOS));
        NetworkManager.Instance.ListenGameServer(KS2C_Protocol.s2c_money_change, (KProtoBuf buf) => { }, typeof(S2C_MONEY_CHANGE));
        //-----------------------------------------------------------------------------------------------------
    }

    #region S2C
    /// <summary>
    /// 同步玩家基础信息
    /// </summary>
    /// <param name="buf"></param>
    private void OnSyncPlayerBaseInfo(KProtoBuf buf)
    {
        S2C_SYNC_PLAYER_BASE_INFO respond = buf as S2C_SYNC_PLAYER_BASE_INFO;
        m_PlayerInfo.Uid = respond.uPlayerID;

    }
    /// <summary>
    /// 同步玩家状态信息
    /// </summary>
    /// <param name="buf"></param>
    private void OnSyncPlayerStateInfo(KProtoBuf buf)
    {
        S2C_SYNC_PLAYER_STATE_INFO respond = buf as S2C_SYNC_PLAYER_STATE_INFO;
        m_PlayerInfo.Level = respond.nLevel;
        m_PlayerInfo.WatchLv = respond.dan_level;
        m_PlayerInfo.Exp = respond.dExp;
        m_PlayerInfo.WatchExp = respond.dan_exp;
    }
    /// <summary>
    /// 添加经验
    /// </summary>
    /// <param name="buf"></param>
    private void OnAddExp(KProtoBuf buf)
    {
        S2C_ADD_EXP respond = buf as S2C_ADD_EXP;
        if (m_PlayerInfo.Uid != respond.nuid)
            return;
        if (m_PlayerInfo.Exp != respond.curExp)
        {
            m_PlayerInfo.Exp = respond.curExp;
        }
        if (m_PlayerInfo.WatchExp != respond.cur_dan_exp)
        {
            m_PlayerInfo.WatchExp = respond.cur_dan_exp;
            GameFacade.Instance.SendNotification(NotificationName.MSG_PLAYER_WATCH_EXP_UP);
        }
    }
    /// <summary>
    /// 更新等级
    /// </summary>
    /// <param name="buf"></param>
    private void OnUpdateLevel(KProtoBuf buf)
    {
        S2C_UPDATE_LEVEL respond = buf as S2C_UPDATE_LEVEL;
        if (m_PlayerInfo.Uid != respond.nuid)
            return;
        if (m_PlayerInfo.Level != respond.nlevel)
        {
            m_PlayerInfo.Level = respond.nlevel;
            GameFacade.Instance.SendNotification(NotificationName.MSG_PLAYER_LEVEL_UP, m_PlayerInfo.Level);
        }
        if (m_PlayerInfo.WatchLv != respond.dan_level)
        {
            m_PlayerInfo.WatchLv = respond.dan_level;
            GameFacade.Instance.SendNotification(NotificationName.MSG_PLAYER_WATCH_LEVEL_UP);
            UIManager.Instance.OpenPanel(UIPanel.AccountUpaPanel);
        }
        m_PlayerInfo.Exp = respond.dExp;
        m_PlayerInfo.WatchExp = respond.dan_exp;
    }
    /// <summary>
    /// 同步手表升级奖励列表
    /// </summary>
    /// <param name="buf"></param>
    private void OnRewardList(KProtoBuf buf)
    {
        S2C_LEVELUP_REWARD_LIST respond = buf as S2C_LEVELUP_REWARD_LIST;
        List<RewardDateVO> list = new List<RewardDateVO>();
        foreach (RewardInfo reward in respond.rewards)
        {
            var item = new RewardDateVO();
            item.Id = (uint)reward.id;
            item.Num = reward.count;
            item.Quality = TableUtil.GetItemQuality(item.Id);
            list.Add(item);
        }
        GameFacade.Instance.SendNotification(NotificationName.MSG_LEVELUP_REWARD_LIST, list);
    }
    #endregion

    #region C2S
    /// <summary>
    /// 请求升级玩家手表
    /// </summary>
    public void RequestLevelUpWatch()
    {
        C2S_REQUEST_LEVELUP_DAN msg = SingleInstanceCache.GetInstanceByType<C2S_REQUEST_LEVELUP_DAN>();
        msg.protocolID = (ushort)KC2S_Protocol.c2s_request_levelup_dan;
        msg.uid = m_PlayerInfo.Uid;
        NetworkManager.Instance.SendToGameServer(msg);
    }
    #endregion
}
