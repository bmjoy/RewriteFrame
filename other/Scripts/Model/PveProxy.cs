using Assets.Scripts.Proto;
using PureMVC.Patterns.Proxy;
using System.Collections.Generic;
using UnityEngine;

public class PveProxy : Proxy//AbsGSController, IModel
{
	/// <summary>
	/// Pve任务列表
	/// </summary>
    private List<PveMissionInfoVO> m_PveMissionList = new List<PveMissionInfoVO>();

	/// <summary>
	/// 结算奖励列表
	/// </summary>
    private List<RewardDateVO> m_PveRewards = null;

	/// <summary>
	/// 战斗统计列表
	/// </summary>
    private List<PveFightInfoVO> pveFights = null;

	/// <summary>
	/// 倒计时
	/// </summary>
    private float m_CountdownTime;

	/// <summary>
	/// 倒计时开始时间
	/// </summary>
    private float m_CountdownTimeBegin;

	/// <summary>
	/// 结算结果  1 成功，0 失败
	/// </summary>
	public int m_SettlementResult = -1;

	/// <summary>
	/// 退出消息是否已发送
	/// </summary>
    public bool m_ExitSended = false;


	public PveProxy() : base(ProxyName.PveProxy) { }

	/// <summary>
	/// 获取Pve任务
	/// </summary>
	/// <returns>Pve任务列表</returns>
	public List<PveMissionInfoVO> GetPveMission()
	{
		return m_PveMissionList;
	}

	/// <summary>
	/// 更新Pve任务
	/// </summary>
	/// <param name="list">Pve任务列表</param>
	public void UpdatePveMission(List<PveMissionInfoVO> list)
    {
        //var str = "";

        m_PveMissionList.Clear();
        foreach(var item in list)
        {
            m_PveMissionList.Add(item);
            //str += item.ID + " = " + item.Value + " , ";
        }

		SendNotification(NotificationName.MSG_PVE_SCHEDULE_UPDATE);

        //Debugger.LogError("UpdatePveMission({" + str + "})");
    }

	/// <summary>
	/// 修改Pve任务
	/// </summary>
	/// <param name="info">Pve任务</param>
    public void SetPveMission(PveMissionInfoVO info)
    {
        var index = -1;
        for (int i = 0; i < m_PveMissionList.Count; i++)
        {
            if (m_PveMissionList[i].ID == info.ID)
            {
                index = i;
            }
        }

        if (index != -1)
        {
            m_PveMissionList[index] = info;
			SendNotification(NotificationName.MSG_PVE_SCHEDULE_CHANGE, info);

            //Debugger.LogError("SetPveMission({" + info.ID + "=" + info.Value + "})");
        }
        else
        {
            m_PveMissionList.Add(info);
			SendNotification(NotificationName.MSG_PVE_SCHEDULE_ADD, info);

            //Debugger.LogError("AddPveMission({" + info.ID + "=" + info.Value + "})");
        }
    }

	/// <summary>
	/// 副本倒计时
	/// </summary>
	public float GetCountdownTime()
	{
		return Mathf.Max(0, (m_CountdownTime - (Time.time - m_CountdownTimeBegin)));
	}

	/// <summary>
	/// 重置倒计时
	/// </summary>
	/// <param name="time">倒计时时长</param>
	public void ResetCountdown(float time)
    {
        m_CountdownTime = time;
        m_CountdownTimeBegin = Time.time;
    }

	/// <summary>
	/// 获取结算奖励
	/// </summary>
	/// <returns>奖励数据列表</returns>
    public List<RewardDateVO> GetSettlementRewards()
    {
        return m_PveRewards;
    }

	/// <summary>
	/// 设置结算奖励
	/// </summary>
	/// <param name="result">结算状态</param>
	/// <param name="rewards">结算奖励</param>
    public void SetSettlementRewards(int result, List<RewardDateVO> rewards)
    {
        m_PveRewards = rewards;
        m_SettlementResult = result;
		//Debugger.LogError("SetSettlementRewards({})");

		SendNotification(m_SettlementResult >0? NotificationName.MSG_PVE_Animator_Victory : NotificationName.MSG_PVE_Animator_defeat);
    }

	/// <summary>
	/// 获取结算的战斗统计
	/// </summary>
	/// <returns></returns>
    public List<PveFightInfoVO> GetSettlementFightInfos()
    {
        return pveFights;
    }

	/// <summary>
	/// 设置结算的战斗统计
	/// </summary>
	/// <param name="fights"></param>
    public void SetSettlementFightInfos(List<PveFightInfoVO> fights)
    {
        m_ExitSended = false;
        pveFights = fights;

		SendNotification(NotificationName.MSG_PVE_Settlement_InjuryStatistics);
	}

}
