using Assets.Scripts.Define;
using Assets.Scripts.Lib.Net;
using Assets.Scripts.Proto;
using Game.Frame.Net;
using UnityEngine;
using LeyouDebug = Leyoutech.Utility.DebugUtility;

public class MSAIBossController : BaseNetController
{
    public MSAIBossController()
    {
        NetworkManager.Instance.ListenGameServer(KS2C_Protocol.s2c_enter_battle, OnSaveMainPlayeEnterBattleProtol, typeof(S2C_ENTER_BATTLE));
        NetworkManager.Instance.ListenGameServer(KS2C_Protocol.s2c_leave_battle, OnSaveMainPlayeEndBattleProtol, typeof(S2C_LEAVE_BATTLE));
        NetworkManager.Instance.ListenGameServer(KS2C_Protocol.s2c_change_plot_state, OnSaveAIBossStateProtol, typeof(S2C_CHANGE_PLOT_STATE));
        NetworkManager.Instance.ListenGameServer(KS2C_Protocol.s2c_plot_monster_list, OnSaveAIMonsterListProtol, typeof(S2C_PLOT_MONSTER_LIST));
        NetworkManager.Instance.ListenGameServer(KS2C_Protocol.s2c_sync_new_hero, OnSaveNewMonster, typeof(S2C_SYNC_NEW_HERO));
        NetworkManager.Instance.ListenGameServer(KS2C_Protocol.s2c_remove_scene_obj, OnRemoveMonster, typeof(S2C_REMOVE_SCENE_OBJ));
        NetworkManager.Instance.ListenGameServer(KS2C_Protocol.s2c_change_plot_time, OnCountDownTimeChange, typeof(S2C_CHANGE_PLOT_TIME));
    }

    public MSAIBossProxy GetMSAIBossProxy()
    {
        return GameFacade.Instance.RetrieveProxy(ProxyName.MSAIBossProxy) as MSAIBossProxy;
    }

	// 玩家加入仇恨列表
	private void OnSaveMainPlayeEnterBattleProtol(KProtoBuf buf)
    {
        //Leyoutech.Utility.DebugUtility.LogError("AIBOSS:", "enterBattle");
        GetMSAIBossProxy().SavePlayerInBattle(true);
    }

    private void OnSaveMainPlayeEndBattleProtol(KProtoBuf buf)
    {
        //Leyoutech.Utility.DebugUtility.LogError("AIBOSS:", "endbattle");
        GetMSAIBossProxy().SavePlayerInBattle(false);
    }

    private void OnSaveNewMonster(KProtoBuf buf)
    {
        S2C_SYNC_NEW_HERO msg = buf as S2C_SYNC_NEW_HERO;
        GetMSAIBossProxy().SaveNewMonster(msg);

    }

    private void OnRemoveMonster(KProtoBuf buf)
    {
        S2C_REMOVE_SCENE_OBJ msg = buf as S2C_REMOVE_SCENE_OBJ;
        GetMSAIBossProxy().RemoveMonster(msg);

    }

    private void OnCountDownTimeChange(KProtoBuf buf)
    {
        S2C_CHANGE_PLOT_TIME msg = buf as S2C_CHANGE_PLOT_TIME;
        for(int i = 0; i < msg.time_list.Count; ++i)
        {
			//LeyouDebug.LogErrorFormat("AIBOSS: ", "S2C_CHANGE_PLOT_TIME type = {0}, time = {1}", 
			//((KPlotTimeType)msg.time_list[i].type).ToString(), msg.time_list[i].time);
			if ((KPlotTimeType)msg.time_list[i].type == KPlotTimeType.pttWholeTime && msg.time_list[i].time > 0)
			{
				GetMSAIBossProxy().OpenCountDownTimePanel(true, msg.time_list[i].time);
			}
			else if ((KPlotTimeType)msg.time_list[i].type == KPlotTimeType.pttWholeTime && msg.time_list[i].time <= 0)
			{
				GetMSAIBossProxy().OpenCountDownTimePanel(false);
			}

        }
    }

	// AI状态（入口）
	private void OnSaveAIBossStateProtol(KProtoBuf buf)
    {
        S2C_CHANGE_PLOT_STATE msg = buf as S2C_CHANGE_PLOT_STATE;
        GetMSAIBossProxy().SaveAIState(msg);
        //Leyoutech.Utility.DebugUtility.LogErrorFormat("AIBOSS: ", "S2C_CHANGE_PLOT_STATE npc_id = {0}, plot_state = {1}", msg.npc_id, ((AIPlotState)msg.plot_state).ToString());
    }

    private void OnSaveAIMonsterListProtol(KProtoBuf buf)
    {
        S2C_PLOT_MONSTER_LIST msg = buf as S2C_PLOT_MONSTER_LIST;
        //foreach (ulong uid in msg.monster_uids)
        //{
        //LeyouDebug.LogErrorFormat("AIBOSS: ", "S2C_PLOT_MONSTER_LIST npc_id = {0}, monsterUid = {1}", msg.npc_id, msg.monster_uids.Count);
        //}
        GetMSAIBossProxy().SaveAICallMonsterInfo(msg);
    }

}
