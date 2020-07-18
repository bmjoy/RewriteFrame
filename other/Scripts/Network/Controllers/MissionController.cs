//#define MissionDebug

using Assets.Scripts.Lib.Net;
using Assets.Scripts.Proto;
using Game.Frame.Net;
using System.Collections.Generic;

public class MissionController : BaseNetController
{
	public MissionController()
	{
		NetworkManager.Instance.ListenGameServer(KS2C_Protocol.s2c_send_tasklist, OnReciveTaskList, typeof(S2C_SEND_TASKlIST));
		NetworkManager.Instance.ListenGameServer(KS2C_Protocol.s2c_receive_task, OnAcceptTask, typeof(S2C_RECEIVE_TASK));
		NetworkManager.Instance.ListenGameServer(KS2C_Protocol.s2c_task_process, OnMissionProgressChange, typeof(S2C_TASK_PROCESS));
		NetworkManager.Instance.ListenGameServer(KS2C_Protocol.s2c_commit_task, OnMissionCommited, typeof(S2C_COMMIT_TASK));
		NetworkManager.Instance.ListenGameServer(KS2C_Protocol.s2c_task_finishlist, OnFinishedMissionList, typeof(S2C_TASK_FINISHLIST));
		NetworkManager.Instance.ListenGameServer(KS2C_Protocol.s2c_drop_task, OnDropMission, typeof(S2C_DROP_TASK));
		NetworkManager.Instance.ListenGameServer(KS2C_Protocol.s2c_task_faillist, OnFaillistMission, typeof(S2C_TASK_FAILLIST));
		NetworkManager.Instance.ListenGameServer(KS2C_Protocol.s2c_task_failnotify, OnFailMission, typeof(S2C_TASK_FAILNOTIFY));
		NetworkManager.Instance.ListenGameServer(KS2C_Protocol.s2c_task_statenotify, OnMissionStateChange, typeof(S2C_TASK_STATENOTIFY));

		//客户端暂且不用 
		NetworkManager.Instance.ListenGameServer(KS2C_Protocol.s2c_send_npc_mission, OnNpcCanAcceptTaskInfo, typeof(S2C_SEND_NPC_MISSION));
		//
	}

	private MissionProxy GetMissionProxy()
	{
		return GameFacade.Instance.RetrieveProxy(ProxyName.MissionProxy) as MissionProxy;
	}

	private CfgEternityProxy GetCfgEternityProxy()
	{
		return GameFacade.Instance.RetrieveProxy(ProxyName.CfgEternityProxy) as CfgEternityProxy;
	}

	#region S2C
	/// <summary>
	/// 收到已接任务列表
	/// </summary>
	/// <param name="buf"></param>
	private void OnReciveTaskList(KProtoBuf buf)
	{
		S2C_SEND_TASKlIST msg = buf as S2C_SEND_TASKlIST;
		if (msg.taskContinueList?.Count > 0)
		{
			for (int i = 0; i < msg.taskContinueList.Count; i++)
			{
#if MissionDebug
				Debug.LogError("S2C 收到已接任务列表 任务id:" + msg.taskContinueList[i].taskID);
#endif
				GetMissionProxy().OnReciveMissionList(msg.taskContinueList[i].taskID, msg.taskContinueList[i].taskOID);
			}
		}
	}

	/// <summary>
	/// 接任务
	/// </summary>
	/// <param name="buf"></param>
	private void OnAcceptTask(KProtoBuf buf)
	{
		S2C_RECEIVE_TASK msg = buf as S2C_RECEIVE_TASK;
		if (msg.optResult == 0)
		{
#if MissionDebug
			Debug.LogError("S2C 接到新任务 任务id:" + msg.missionTID);
#endif
			GetMissionProxy().OnReciveMissionList(msg.missionTID, msg.taskOID);
			if (msg.processInfo?.Count > 0)
			{
				MissionProgressHandler(msg.processInfo);
			}
			GameFacade.Instance.SendNotification(NotificationName.MSG_MISSION_ACCEPT, new MsgMissionInfo(msg.missionTID, msg.taskOID));
			(GameFacade.Instance.RetrieveProxy(ProxyName.TaskTrackingProxy) as TaskTrackingProxy).RebuildTracking();
			GetMissionProxy().SendMsg_OnMissionRecived(msg.missionTID);
		}
	}

	/// <summary>
	/// 任务进度改变
	/// </summary>
	/// <param name="buf"></param>
	private void OnMissionProgressChange(KProtoBuf buf)
	{
		S2C_TASK_PROCESS msg = buf as S2C_TASK_PROCESS;
		if (msg.processInfo?.Count > 0)
		{
			MissionProgressHandler(msg.processInfo, true);
			GameFacade.Instance.SendNotification(NotificationName.MSG_MISSION_CHANGE);
			(GameFacade.Instance.RetrieveProxy(ProxyName.TaskTrackingProxy) as TaskTrackingProxy).RebuildTracking();
		}
	}

	/// <summary>
	/// 任务一些状态的变化
	/// 这个方法 只会触发一些 ui动画方面的 MSG
	/// </summary>
	/// <param name="buf"></param>
	private void OnMissionStateChange(KProtoBuf buf)
	{
		S2C_TASK_STATENOTIFY msg = buf as S2C_TASK_STATENOTIFY;

		//任务达成
		if (msg.rootOID > 0)
		{
			if (msg.root_state == 1)
			{
				GetMissionProxy().SendMsg_OnMissionCompleted(msg.rootOID);
				//GameFacade.Instance.SendNotification(NotificationName.MSG_MISSION_STATE_CHANGE);
				return;
			}
			else if (msg.root_state == 2)
			{
#if MissionDebug
				Debug.LogError("S2C 任务失败");
#endif
				GameFacade.Instance.SendNotification(NotificationName.MSG_MISSION_FAIL);
				return;
			}
		}

		if (msg.group_id > 0 || msg.node_id > 0)
		{
#if MissionDebug
			Debug.LogError("S2C 组或节点更新");
#endif
			GameFacade.Instance.SendNotification(NotificationName.MSG_MISSION_STATE_CHANGE);
		}

		//任务组达成完成条件
		if (msg.group_id > 0)
		{
			if (msg.group_state == 1)
			{
#if MissionDebug
				Debug.LogError("S2C 任务组 成功:" + msg.rootOID + " . " + msg.group_id);
#endif
				GetMissionProxy().SendMsg_OnMissionGroupCompleted(msg.group_id);
			}
			else if (msg.group_state == 2)
			{
#if MissionDebug
				Debug.LogError("S2C 任务组 失败:" + msg.rootOID + " . " + msg.group_id);
#endif
			}
		}
		//子任务达成完成条件
		if (msg.taskOID > 0 && msg.task_state == 1)
		{
			GetMissionProxy().SendMsg_OnSubMissionCompleted(msg.taskOID);
		}

	}

	/// <summary>
	/// 任务完成
	/// </summary>
	/// <param name="buf"></param>
	private void OnMissionCommited(KProtoBuf buf)
	{
		S2C_COMMIT_TASK msg = buf as S2C_COMMIT_TASK;
		if (msg.optResult == 0)
		{
			if (GetMissionProxy().TryRemoveMission(msg.taskOID, out MissionVO missionVO))
			{
#if MissionDebug
				Debug.LogError("S2C 任务完成:" + missionVO.Tid);
#endif
				uint tid = missionVO.Tid;
				GetMissionProxy().SaveCompletedMission(tid);
				GameFacade.Instance.SendNotification(NotificationName.MSG_MISSION_COMMIT, new MsgMissionInfo(tid, msg.taskOID));
				(GameFacade.Instance.RetrieveProxy(ProxyName.TaskTrackingProxy) as TaskTrackingProxy).RebuildTracking();
			}
		}
	}

	/// <summary>
	/// 放弃任务后返回
	/// </summary>
	/// <param name="buf"></param>
	private void OnDropMission(KProtoBuf buf)
	{
		S2C_DROP_TASK msg = buf as S2C_DROP_TASK;
		if (msg.optResult == 0)
		{
			if (GetMissionProxy().TryRemoveMission(msg.taskOID, out MissionVO missionVO))
			{
#if MissionDebug
				Debug.LogError("S2C 放弃任务:" + missionVO.Tid);
#endif
				//把放弃的任务重新塞进可接任务列表
				GetMissionProxy().AddMissionToCanList(missionVO);

				GameFacade.Instance.SendNotification(NotificationName.MSG_MISSION_ABANDON, new MsgMissionInfo(missionVO.Tid, msg.taskOID));
				(GameFacade.Instance.RetrieveProxy(ProxyName.TaskTrackingProxy) as TaskTrackingProxy).RebuildTracking();
			}
		}
	}

	private void OnNpcCanAcceptTaskInfo(KProtoBuf buf)
	{
		return;
	}

	/// <summary>
	/// 收到任务完成列表
	/// </summary>
	/// <param name="buf"></param>
	private void OnFinishedMissionList(KProtoBuf buf)
	{
		S2C_TASK_FINISHLIST msg = buf as S2C_TASK_FINISHLIST;
#if MissionDebug
		Debug.LogError("S2C 收到已完成任务列表:" + string.Join(",", msg.missionID));
#endif
		GetMissionProxy().InitCanAcceptMission(new HashSet<uint>(msg.missionID));
	}

	/// <summary>
	/// 失败列表
	/// </summary>
	/// <param name="buf"></param>
	private void OnFaillistMission(KProtoBuf buf)
	{
		S2C_TASK_FAILLIST msg = buf as S2C_TASK_FAILLIST;
		for (int i = 0; i < msg.taskOID.Count; i++)
		{
			GetMissionProxy().SetMissionStateToFailed(msg.taskOID[i]);
		}
	}

	/// <summary>
	/// 失败任务通知
	/// </summary>
	/// <param name="buf"></param>
	private void OnFailMission(KProtoBuf buf)
	{
		S2C_TASK_FAILNOTIFY msg = buf as S2C_TASK_FAILNOTIFY;
		for (int i = 0; i < msg.taskOID.Count; i++)
		{
			GetMissionProxy().SetMissionStateToFailed(msg.taskOID[i]);
		}
	}


	#region 通用的一些数据处理
	/// <summary>
	/// 任务进度改变数据处理
	/// </summary>
	/// <param name="processInfo"></param>
	private MissionState? MissionProgressHandler(List<TaskProcess> processInfo, bool check = false)
	{
		HashSet<ulong> parentIds = new HashSet<ulong>();
		for (int i = 0; i < processInfo.Count; i++)
		{
			parentIds.Add(processInfo[i].parent_id);
		}
		foreach (ulong parentId in parentIds)
		{
			GetMissionProxy().ClearMissionTargetList(parentId);
		}

		for (int i = 0; i < processInfo.Count; i++)
		{
			GetMissionProxy().ChangeMissionProgress(
				processInfo[i].parent_id,//根任务uid
				processInfo[i].group,//第一节点内的组tid
				processInfo[i].subTid,//目标任务tid
				processInfo[i].taskOID,//目标任务uid
				processInfo[i].doingValue,//进度值
				processInfo[i].finished,//达成标记
				processInfo[i].data1 / 1000); //倒计时的截止日期
		}
		MissionState? state = MissionState.Finished;
		foreach (ulong parentId in parentIds)
		{
			MissionState? temp = GetMissionProxy().CheckMissionStateToFinished(parentId);
			if (temp.HasValue && temp.Value != MissionState.Finished)
			{
				state = temp;
			}
			GetMissionProxy().CheckMissionTargetRelation(parentId);
		}
		return state;
	}
	#endregion

	#endregion


	#region C2S
	/// <summary>
	/// 接任务
	/// </summary>
	/// <param name="missionTid"></param>
	public void SendAcceptMission(uint missionTid)
	{
		C2S_RECEIVE_TASK msg = new C2S_RECEIVE_TASK();
		msg.protocolID = (ushort)KC2S_Protocol.c2s_receive_task;
		msg.taskID = missionTid;
		NetworkManager.Instance.SendToGameServer(msg);
#if MissionDebug
		Debug.LogError("C2S 接任务:" + missionTid);
#endif
	}

	/// <summary>
	/// 交任务
	/// </summary>
	/// <param name="missionUid"></param>
	public void SendSubmitMission(ulong missionUid)
	{
		C2S_COMMIT_TASK msg = new C2S_COMMIT_TASK();
		msg.protocolID = (ushort)KC2S_Protocol.c2s_commit_task;
		msg.taskOID = missionUid;
		NetworkManager.Instance.SendToGameServer(msg);
	}

	/// <summary>
	/// 放弃任务
	/// </summary>
	/// <param name="missionUid"></param>
	public void SendDropMission(ulong missionUid)
	{
		C2S_DROP_TASK msg = new C2S_DROP_TASK();
		msg.protocolID = (ushort)KC2S_Protocol.c2s_drop_task;
		msg.taskOID = missionUid;
		NetworkManager.Instance.SendToGameServer(msg);
	}

	/// <summary>
	/// 发送npc对话
	/// </summary>
	/// <param name="npcTid"></param>
	/// <param name="talkTid"></param>
	public void SendNpcTalk(uint npcTid, uint talkTid)
	{
#if MissionDebug
		Debug.LogError("C2S 发送NPC对话: NPC:" + npcTid + "  Talk:" + talkTid);
#endif
		C2S_NPC_TALK msg = new C2S_NPC_TALK();
		msg.protocolID = (ushort)KC2S_Protocol.c2s_npc_talk;
		msg.npcId = npcTid;
		msg.dialogId = talkTid;
		NetworkManager.Instance.SendToGameServer(msg);
	}
	#endregion
}

