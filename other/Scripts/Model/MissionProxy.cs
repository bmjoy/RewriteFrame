//#define MissionDebug

using Eternity.FlatBuffer;
using Leyoutech.Core.Effect;
using Leyoutech.Core.Loader;
using PureMVC.Patterns.Proxy;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MissionProxy : Proxy
{
	private CfgEternityProxy m_CfgMissionProxy;

	/// <summary>
	/// 可接任务
	/// </summary>
	private Dictionary<uint, MissionVO> m_CanAcceptMissions;

	/// <summary>
	/// 所有任务（用于其他模块按类型获取一组任务）
	/// </summary>
	public Dictionary<MissionType, Dictionary<ulong, MissionVO>> AllMissionDic { get; }

	/// <summary>
	/// 所有任务（用于按uid直接查找任务）
	/// </summary>
	private Dictionary<ulong, MissionVO> AllMissionList;

	public Dictionary<uint, MissionVO> GetCanAcceptMissions()
	{
		return m_CanAcceptMissions;
	}

	public MissionProxy() : base(ProxyName.MissionProxy)
	{
		GetCfgMissionProxy().SortMissionList();

		Array enumArray = Enum.GetValues(typeof(MissionType));
		AllMissionDic = new Dictionary<MissionType, Dictionary<ulong, MissionVO>>();
		AllMissionList = new Dictionary<ulong, MissionVO>();
		foreach (MissionType item in enumArray)
		{
			AllMissionDic.Add(item, new Dictionary<ulong, MissionVO>());
		}
	}

	#region proxy
	private CfgEternityProxy GetCfgMissionProxy()
	{
		if (m_CfgMissionProxy == null)
		{
			m_CfgMissionProxy = GameFacade.Instance.RetrieveProxy(ProxyName.CfgEternityProxy) as CfgEternityProxy;
		}
		return m_CfgMissionProxy;
	}
	#endregion

	/// <summary>
	/// 收到任务
	/// </summary>
	/// <param name="tid"></param>
	/// <param name="uid"></param>
	public void OnReciveMissionList(uint tid, ulong uid)
	{
		if (AllMissionList.ContainsKey(uid))
		{
			return;
		}

		MissionData mission = GetCfgMissionProxy().GetMissionByKey(tid);

		if (m_CanAcceptMissions.TryGetValue(tid, out MissionVO missionVO))
		{
			//从可接可借列表移除
			m_CanAcceptMissions.Remove(tid);
		}
		else//如果不在可接列表的 （比如进入fb的时候 发来一个任务）
		{
			//创建任务
			missionVO = CreateMission(tid);
		}
		missionVO.Uid = uid;
		missionVO.MissionState = MissionState.Going;
		AllMissionList.Add(uid, missionVO);
		AllMissionDic[(MissionType)mission.Type].Add(uid, missionVO);


		#region 音效
		////HACK gaoyu
		//这是一个最终需求么? by 张子华
		if (missionVO.MissionMainConfig.HasValue && missionVO.MissionMainConfig.Value.StartBGM > 0)
		{
			WwiseUtil.PlaySound(missionVO.MissionMainConfig.Value.StartBGM, false, null);
		}
		#endregion
	}

	/// <summary>
	/// 根据任务uid获取一个任务
	/// </summary>
	/// <param name="missionUid"></param>
	/// <returns></returns>
	public MissionVO GetMissionBy(ulong missionUid)
	{
		if (AllMissionList.TryGetValue(missionUid, out MissionVO value))
		{
			return value;
		}
		return null;
	}

	/// <summary>
	/// 根据任务itd获取一个任务
	/// </summary>
	/// <param name="missionTid"></param>
	/// <returns></returns>
	public MissionVO GetMissionBy(uint missionTid)
	{
		if (m_CanAcceptMissions.TryGetValue(missionTid, out MissionVO value))
		{
			return value;
		}
		foreach (MissionVO item in AllMissionList.Values)
		{
			if (item.Tid == missionTid)
			{
				return item;
			}
		}
		return null;
	}

	/// <summary>
	/// 检查是否有某个uid的任务
	/// </summary>
	/// <param name="missionUid"></param>
	/// <returns></returns>
	public bool HasMissionBy(ulong missionUid)
	{
		return AllMissionList.ContainsKey(missionUid);
	}

	#region 任务数据的各种 进度/状态 修改

	/// <summary>
	/// 移除任务
	/// </summary>
	/// <param name="missionUid"></param>
	public bool TryRemoveMission(ulong missionUid, out MissionVO missionVO)
	{
		missionVO = GetMissionBy(missionUid);
		if (missionVO != null)
		{
			AllMissionList.Remove(missionUid);
			AllMissionDic[missionVO.MissionType].Remove(missionUid);

			#region 音效
			if (missionVO.MissionMainConfig.HasValue && missionVO.MissionMainConfig.Value.OverBGM > 0)
			{
				WwiseUtil.PlaySound(missionVO.MissionMainConfig.Value.OverBGM, false, null);
			}
			#endregion
		}
		RemoveMissionTrack(missionVO.Tid);
		return missionVO != null;
	}

	/// <summary>
	/// 保存完成的任务
	/// </summary>
	/// <param name="missionTid"></param>
	public void SaveCompletedMission(uint missionTid)
	{
		AddCanReciveMission(missionTid);
	}

	/// <summary>
	/// 清理任务目标数据
	/// </summary>
	/// <param name="missionUid"></param>
	public void ClearMissionTargetList(ulong missionUid)
	{
		MissionVO missionVO = GetMissionBy(missionUid);
		if (missionVO != null)
		{
			foreach (KeyValuePair<uint, SortedDictionary<uint, MissionTargetVO>> item1 in missionVO.MissionGroupTargetList)
			{
				item1.Value.Clear();
			}
			missionVO.MissionGroupTargetList.Clear();
		}
	}

	/// <summary>
	/// 任务进度数据改变
	/// </summary>
	/// <param name="missionUid"></param>
	/// <param name="groupTid"></param>
	/// <param name="targetTid"></param>
	/// <param name="targetUid"></param>
	/// <param name="targetValue"></param>
	public void ChangeMissionProgress(ulong missionUid, uint groupTid, uint targetTid, ulong targetUid, long targetValue, int isDone, long timeData)
	{
		MissionVO missionVO = GetMissionBy(missionUid);
		if (missionVO != null)
		{
			if (!missionVO.MissionGroupTargetList.TryGetValue(groupTid, out SortedDictionary<uint, MissionTargetVO> targetList))
			{
				targetList = new SortedDictionary<uint, MissionTargetVO>();
				missionVO.MissionGroupTargetList.Add(groupTid, targetList);
			}
			if (!targetList.TryGetValue(targetTid, out MissionTargetVO targetVO))
			{
				targetVO = CreateMissionTargetVO(targetTid);
				targetList.Add(targetTid, targetVO);
			}
			targetVO.Uid = targetUid;
			targetVO.TargetState = isDone == 1 ? MissionState.Finished : isDone == 2 ? MissionState.Failed : MissionState.Going;
			if (targetVO.MissionTargetType == MissionTargetType.TimeOut)
			{
				targetVO.Value = timeData;
			}
			else
			{
				targetVO.Value = targetValue;
			}
#if MissionDebug
			Debug.LogError($"S2C 收到任务进度改变: Mission:{missionVO.Tid} Target:{targetTid} Value:{targetValue} Done?{isDone} State:{targetVO.TargetState}");
#endif
		}
	}

	/// <summary>
	/// 检查任务状态到完成
	/// </summary>
	/// <param name="missionUid"></param>
	public MissionState? CheckMissionStateToFinished(ulong missionUid)
	{
		MissionVO missionVO = GetMissionBy(missionUid);
		if (missionVO != null)
		{
			foreach (var item in missionVO.MissionGroupTargetList)
			{
				foreach (var item1 in item.Value)
				{
					if (item1.Value.TargetState != MissionState.Finished && item1.Value.DoneToFinish)
					{
						if (missionVO.MissionState == MissionState.Finished)
						{
							missionVO.MissionState = MissionState.Going;
						}
						return missionVO.MissionState;
					}
				}
			}
			if (missionVO.MissionState == MissionState.Going)
			{
				missionVO.MissionState = MissionState.Finished;
			}
			return missionVO.MissionState;
		}
		return null;
	}

	/// <summary>
	/// 刷新任务target关联
	/// arg4 如果>0 就表明关联了 另一个target
	/// 多为表现负面target的显示
	/// </summary>
	/// <param name="missionUid"></param>
	public void CheckMissionTargetRelation(ulong missionUid)
	{
		MissionVO missionVO = GetMissionBy(missionUid);
		if (missionVO != null && missionVO.MissionState == MissionState.Going)
		{
			foreach (var item in missionVO.MissionGroupTargetList)
			{
				foreach (var item1 in item.Value)
				{
					if (item1.Value.TargetState == MissionState.Going && item1.Value.DoneToFinish)
					{
						if (item1.Value.MissionTargetConfig.Arg4 > 0)
						{
							item.Value.TryGetValue((uint)item1.Value.MissionTargetConfig.Arg4, out item1.Value.Relation);
						}
					}
				}
			}
		}
	}

	/// <summary>
	/// 将任务状态置为失败
	/// 状态不可逆
	/// </summary>
	/// <param name="missionUid"></param>
	public void SetMissionStateToFailed(ulong missionUid)
	{
		MissionVO missionVO = GetMissionBy(missionUid);
		if (missionVO != null)
		{
#if MissionDebug
			Debug.LogError("S2C 收到失败任务:" + string.Join(",", missionVO.Tid));
#endif
			missionVO.MissionState = MissionState.Failed;
		}
	}

	/// <summary>
	/// 将任务状态置为成功
	/// </summary>
	/// <param name="missionUid"></param>
	public void SetMissionStateToCompleted(ulong missionUid)
	{
		MissionVO missionVO = GetMissionBy(missionUid);
		if (missionVO != null)
		{
			missionVO.MissionState = MissionState.Finished;
		}
	}

	#endregion

	#region CreateMissionVO
	/// <summary>
	/// 构建任务数据
	/// </summary>
	/// <param name="tid"></param>
	/// <param name="uid"></param>
	/// <returns></returns>
	private MissionVO CreateMission(uint tid)
	{
		MissionData missionCfg = GetCfgMissionProxy().GetMissionByKey(tid);

		//任务总表的数据  第一层
		MissionVO mission = new MissionVO();
		mission.MissionConfig = missionCfg;
		mission.Tid = tid;
		mission.MissionType = (MissionType)missionCfg.Type;

		//主线 或者 支线任务
		//需要 加入npc数据
		if (mission.MissionType == MissionType.Main || mission.MissionType == MissionType.Branch)
		{
			mission.MissionMainConfig = GetCfgMissionProxy().GetMissionMainByKey(tid);
		}

		//任务首节点
		MissionTreeNode firstNode = GetCfgMissionProxy().GetMissionFirstNodeByTid(mission.MissionConfig.FirstNode);

		int groupCount = firstNode.GroupsLength;
		for (int i = 0; i < groupCount; i++)
		{
			//组 第三层
			MissionNodeGroup group = GetCfgMissionProxy().GetMissionNodeGroup(firstNode.Groups(i));
			if (!mission.MissionGroupTargetList.ContainsKey(group.Id))
			{
				mission.MissionGroupTargetList.Add(group.Id, new SortedDictionary<uint, MissionTargetVO>());
			}
			int missionCount = group.SubMissionsLength;
			for (int j = 0; j < missionCount; j++)
			{
				//任务目标 第四层
				if (!mission.MissionGroupTargetList[group.Id].ContainsKey(group.SubMissions(j)))
				{
					MissionTargetVO targetVO = CreateMissionTargetVO(group.SubMissions(j));
					mission.MissionGroupTargetList[group.Id].Add(targetVO.Tid, targetVO);
				}
			}
		}

		return mission;
	}

	/// <summary>
	/// 创建任务目标
	/// </summary>
	/// <param name="missionTid">子任务id</param>
	/// <returns></returns>
	private MissionTargetVO CreateMissionTargetVO(uint missionTid)
	{
		MissionTarget cfg = GetCfgMissionProxy().GetMissionTargetByKey(GetCfgMissionProxy().GetMissionByKey(missionTid).TargetId);
		MissionTargetVO vo = new MissionTargetVO();
		vo.Tid = cfg.Id;
		vo.MissionTargetConfig = cfg;
		vo.MissionTargetType = (MissionTargetType)cfg.Type;
		vo.DoneToFinish = cfg.FinishType == 1;//1是达成型即完成
		vo.HudVisible = cfg.HudVisible == 1;//1是可见

		return vo;
	}

	#endregion

	#region 可接任务列表数据刷新
	/// <summary>
	/// 使用登陆时初始化的完成列表初步筛选出前置满足的任务
	/// </summary>
	/// <param name="completedMissions"></param>
	public void InitCanAcceptMission(HashSet<uint> completedMissions)
	{
		m_CanAcceptMissions = new Dictionary<uint, MissionVO>();
		List<uint> nextIDs;
		completedMissions.Add(0);//这个0 是无前置任务的根节点
		foreach (uint completedID in completedMissions)
		{
			nextIDs = GetCfgMissionProxy().GetNextMissionsBy(completedID);
			for (int i = 0; i < nextIDs?.Count; i++)
			{
				if (!completedMissions.Contains(nextIDs[i]) && !m_CanAcceptMissions.ContainsKey(nextIDs[i]))
				{
					MissionVO item = CreateMission(nextIDs[i]);
					item.MissionState = MissionState.CanAccept;
					m_CanAcceptMissions.Add(nextIDs[i], item);
				}
			}
		}
	}

	/// <summary>
	/// 根据完成任务的id加入全部后续任务
	/// 因为是刚完成的任务 所以后续任务必定没有接过
	/// 所以可以不用检查直接加入可接列表
	/// </summary>
	/// <param name="completedMission"></param>
	public void AddCanReciveMission(uint completedMission)
	{
		List<uint> nextIDs = GetCfgMissionProxy().GetNextMissionsBy(completedMission);
		if (nextIDs != null && nextIDs.Count > 0)
		{
			for (int i = 0; nextIDs != null && i < nextIDs.Count; i++)
			{
				MissionVO item = CreateMission(nextIDs[i]);
				item.MissionState = MissionState.CanAccept;
				m_CanAcceptMissions.Add(nextIDs[i], item);
			}
#if MissionDebug
			Debug.LogError("S2C " + completedMission + "完成 加入可接任务:" + string.Join(",", nextIDs));
#endif
		}
	}

	/// <summary>
	/// 塞一个任务到可接任务列表
	/// </summary>
	/// <param name="missionVO"></param>
	public void AddMissionToCanList(MissionVO missionVO)
	{
		//重置一下，避免数据残留，主要是阶段性的数据
		missionVO = CreateMission(missionVO.Tid);
		missionVO.MissionState = MissionState.CanAccept;
		m_CanAcceptMissions.Add(missionVO.Tid, missionVO);
#if MissionDebug
		Debug.LogError("S2C 放弃任务后将其加入可接列表:" + missionVO.Tid);
#endif
	}

	#endregion

	#region 任务追踪
	private const string TrackDefStr = "";
	private const string TrackMissionPrefix = "TrackMission";
	private ulong m_RoleUid;
	private List<uint> m_TrackMissionListTrack;

	private ulong GetRoleUid()
	{
		if (m_RoleUid == 0)
		{
			ServerListProxy proxy = GameFacade.Instance.RetrieveProxy(ProxyName.ServerListProxy) as ServerListProxy;
			m_RoleUid = proxy.GetCurrentCharacterVO().UId;
		}
		return m_RoleUid;
	}

	/// <summary>
	/// 将任务加入跟踪列表
	/// </summary>
	/// <param name="missionUid"></param>
	/// <returns></returns>
	public bool AddMissionTrack(uint missionUid)
	{
		if (m_TrackMissionListTrack == null)
		{
			GetMissionTrack();
		}
		if (m_TrackMissionListTrack.Count < 1)
		{
			if (!m_TrackMissionListTrack.Contains(missionUid))
			{
				m_TrackMissionListTrack.Add(missionUid);
				SaveMissionTrackString();
				return true;
			}
		}
		return false;
	}

	/// <summary>
	/// 从跟踪列表删除
	/// </summary>
	/// <param name="missionUid"></param>
	/// <returns></returns>
	public bool RemoveMissionTrack(uint missionUid)
	{
		if (m_TrackMissionListTrack == null)
		{
			GetMissionTrack();
		}
		if (m_TrackMissionListTrack.Remove(missionUid))
		{
			SaveMissionTrackString();
			return true;
		}
		return false;
	}

	/// <summary>
	/// 获取跟踪任务列表
	/// </summary>
	/// <returns></returns>
	public List<uint> GetMissionTrack()
	{
		if (m_TrackMissionListTrack == null)
		{
			m_TrackMissionListTrack = new List<uint>();
			string str = PlayerPrefs.GetString(GetRoleUid() + TrackMissionPrefix, TrackDefStr);
			if (String.IsNullOrEmpty(str))
			{
				return m_TrackMissionListTrack;
			}
			m_TrackMissionListTrack = str.Split(',').ToList().ConvertAll(Convert.ToUInt32);

			//检查 如果任务id已经不在 本地数据里了 就删了本地存储
			for (int i = 0; i < m_TrackMissionListTrack.Count; i++)
			{
				if (!HasMissionBy(m_TrackMissionListTrack[i]))
				{
					m_TrackMissionListTrack.Remove(m_TrackMissionListTrack[i]);
					i--;
					SaveMissionTrackString();
				}
			}
		}
		return m_TrackMissionListTrack;
	}

	/// <summary>
	/// 保存任务跟踪数据到 PlayerPrefs
	/// </summary>
	private void SaveMissionTrackString()
	{
		if (m_TrackMissionListTrack == null)
		{
			return;
		}
		PlayerPrefs.SetString(GetRoleUid() + TrackMissionPrefix, string.Join(",", m_TrackMissionListTrack));
	}

	#endregion

	public bool OpenMissionPanel(ulong npcUid, Npc npcVO, Transform transform)
	{
		uint? talkId;
		MissionVO missionVO = null;
		MissionOpenMsgInfo info = new MissionOpenMsgInfo();
		info.NpcTransform = transform;

		//检查是否有已完成可以交的任务
		missionVO = GetCanSubmitMissionBy(npcVO.Id);
		if (missionVO != null)
		{
			info.MissionVO = missionVO;
			UIManager.Instance.OpenPanel(UIPanel.MissionDialogPanel, null, info);
			talkId = (uint)(missionVO.MissionMainConfig?.UnfinishedDialog ?? 0);
			OpenNpcChat(npcVO, transform, talkId.Value, npcVO.Id);
			return true;
		}

		//检查是否有对话任务需要对话
		talkId = GetTalkMissionTalkTidBy(npcVO.Id);
		if (talkId.HasValue)
		{
			OpenNpcChat(npcVO, transform, talkId.Value, npcVO.Id, true);


			#region
			//HACK gaoyu
			//这是一个临时需求 by 张子华
			if (talkId.Value == 510109)
			{
				AssetManager.GetInstance().InstanceAssetAsync("Inter_ARJunk_S2Signal02",
					(address, uObj, userData) =>
					{
						GameObject gObj = uObj as GameObject;
						gObj.transform.SetParent(transform, false);
					});
				WwiseUtil.PlaySound(9010, false, transform);//李贺新 临时音效 修复什么东西的音效
			}

			#endregion




			return true;
		}

		//检查是否有可接任务
		missionVO = GetAcceptTaskTidBy(npcVO.Id);
		if (missionVO != null)
		{
			info.MissionVO = missionVO;
			UIManager.Instance.OpenPanel(UIPanel.MissionDialogPanel, null, info);
			talkId = (uint)(missionVO.MissionMainConfig?.AcceptDialog ?? 0);
			OpenNpcChat(npcVO, transform, talkId.Value, npcVO.Id);
			return true;
		}

		//找一个进行中的任务
		missionVO = GetGoingMissionBy(npcVO.Id);
		if (missionVO != null)
		{
			talkId = (uint)(missionVO.MissionMainConfig?.UnfinishedDialog ?? 0);
			OpenNpcChat(npcVO, transform, talkId.Value, npcVO.Id);
			return false;
		}

		return false;
	}

	/// <summary>
	/// 打开npc对话
	/// </summary>
	public void OpenNpcChat(Npc NpcVO, Transform transform, uint chaTid, uint npcTid, bool sendToServer = false)
	{
		if (chaTid == 0)
		{
			UnityEngine.Random.InitState((int)DateTime.Now.Ticks);
			int? dialogue = NpcVO.Function?.Arg2Length;
			if (dialogue.HasValue && dialogue.Value > 0)
			{
				int id = UnityEngine.Random.Range(0, dialogue.Value);
				chaTid = (uint)NpcVO.Function.Value.Arg2(id);
			}
		}

		DialogueInfo info = new DialogueInfo();
		info.DialogueTid = chaTid;
		info.SoundParent = transform;
		info.NpcTid = npcTid;
		info.NeedSendToServer = sendToServer;
		GameFacade.Instance.SendNotification(NotificationName.MSG_DIALOGUE_SHOW, info);
	}

	#region 任务检索
	/// <summary>
	/// 获取一个可交的任务
	/// </summary>
	/// <param name="npcTid"></param>
	/// <param name="missionVO"></param>
	/// <returns></returns>
	public MissionVO GetCanSubmitMissionBy(uint npcTid)
	{
		foreach (KeyValuePair<ulong, MissionVO> mission in AllMissionList)
		{
			if (mission.Value.MissionState == MissionState.Finished && mission.Value.MissionMainConfig?.SubmitNpcId == npcTid)
			{
				return mission.Value;
			}
		}
		return null;
	}

	/// <summary>
	/// 根据 NpcTid 查找是否有对应的对话任务的对话tid
	/// </summary> 
	/// <param name="npcTid"></param>
	/// <returns></returns>
	public uint? GetTalkMissionTalkTidBy(uint npcTid)
	{
		foreach (KeyValuePair<ulong, MissionVO> mission in AllMissionList)
		{
			if (mission.Value.MissionState == MissionState.Going)
			{
				foreach (var item in mission.Value.MissionGroupTargetList)
				{
					foreach (var item1 in item.Value)
					{
						if (item1.Value.TargetState == MissionState.Going
							&& item1.Value.MissionTargetType == MissionTargetType.Chat
							&& item1.Value.MissionTargetConfig.Arg1 == npcTid)
						{
							return (uint)item1.Value.MissionTargetConfig.Arg2;
						}
					}
				}
			}
		}

		return null;
	}

	/// <summary>
	/// 根据npctid 查找对话target 需要 f键 特殊文字的 返回特殊文字
	/// </summary>
	/// <param name="npcTid"></param>
	/// <returns></returns>
	public string GetTalkMissionFKeyText(uint npcTid)
	{
		foreach (KeyValuePair<ulong, MissionVO> mission in AllMissionList)
		{
			if (mission.Value.MissionState == MissionState.Going)
			{
				foreach (var item in mission.Value.MissionGroupTargetList)
				{
					foreach (var item1 in item.Value)
					{
						if (item1.Value.TargetState == MissionState.Going
							&& item1.Value.MissionTargetType == MissionTargetType.Chat
							&& item1.Value.MissionTargetConfig.Arg1 == npcTid)
						{
							return item1.Value.MissionTargetConfig.InteractionTxt;
						}
					}
				}
			}
		}
		return null;
	}

	/// <summary>
	/// 根据 NpcTid ,获取一个可接任务
	/// </summary>
	/// <param name="npcTid"></param>
	/// <returns></returns>
	public MissionVO GetAcceptTaskTidBy(uint npcTid)
	{
		Dictionary<uint, MissionVO> canList = m_CanAcceptMissions;
		foreach (KeyValuePair<uint, MissionVO> item in canList)
		{
			if (npcTid == item.Value.MissionMainConfig.Value.AcceptNpcId && GetRoleLevel() >= item.Value.MissionConfig.LvLimit)
			{
				return item.Value;
			}
		}
		return null;
	}

	/// <summary>
	/// 根据 NpcTid ,获取一个进行中的任务
	/// </summary>
	/// <param name="npcTid"></param>
	/// <returns></returns>
	public MissionVO GetGoingMissionBy(uint npcTid)
	{
		foreach (KeyValuePair<ulong, MissionVO> mission in AllMissionList)
		{
			if (mission.Value.MissionState == MissionState.Going && mission.Value.MissionMainConfig?.SubmitNpcId == npcTid)
			{
				return mission.Value;
			}
		}
		return null;
	}

	#endregion

	#region 检查并推送通讯窗消息
	/// <summary>
	/// 任务接受
	/// </summary>
	/// <param name="missionTid"></param>
	public void SendMsg_OnMissionRecived(uint missionTid)
	{
		MissionData mission = GetCfgMissionProxy().GetMissionByKey(missionTid);
		if (mission.AcceptVideophone > 0)
		{
			SendVideoPhoneChange(mission.AcceptVideophone);
		}
	}

	/// <summary>
	/// 任务达成
	/// </summary>
	/// <param name="missionTid"></param>
	public void SendMsg_OnMissionCompleted(ulong missionUid)
	{
		MissionVO missionVO = GetMissionBy(missionUid);
		if (missionVO.MissionConfig.SubmitVideophone > 0)
		{
			SendVideoPhoneChange(missionVO.MissionConfig.SubmitVideophone);
		}
	}

	/// <summary>
	/// 任务组达成
	/// </summary>
	/// <param name="groupId"></param>
	public void SendMsg_OnMissionGroupCompleted(uint groupId)
	{
		int id = GetCfgMissionProxy().GetVideoPhoneTidByMissionGroupTid(groupId);
		if (id > 0)
		{
			SendVideoPhoneChange((uint)id);
		}
	}

	/// <summary>
	/// 子任务达成
	/// </summary>
	/// <param name="subMissionUid"></param>
	public void SendMsg_OnSubMissionCompleted(ulong subMissionUid)
	{
		foreach (KeyValuePair<ulong, MissionVO> mission in AllMissionList)
		{
			if (mission.Value.MissionState == MissionState.Going)
			{
				foreach (var item in mission.Value.MissionGroupTargetList)
				{
					foreach (var item1 in item.Value)
					{
						if (item1.Value.Uid == subMissionUid)
						{
							MissionData missionCfg = GetCfgMissionProxy().GetMissionByKey(item1.Key);
							if (missionCfg.SubmitVideophone > 0)
							{
								SendVideoPhoneChange(missionCfg.SubmitVideophone);
								return;
							}
						}
					}
				}
			}
		}
	}
	#endregion

	private uint GetRoleLevel()
	{
		GameplayProxy gameplayProxy = GameFacade.Instance.RetrieveProxy(ProxyName.GameplayProxy) as GameplayProxy;
		BaseEntity mainPlayer = gameplayProxy.GetEntityById<BaseEntity>(gameplayProxy.GetMainPlayerUID());
		return mainPlayer?.GetLevel() ?? 1;
	}

	private void SendVideoPhoneChange(uint groupId, Action action = null, uint npcId = 0)
	{
		PlayParameter playParameter = new PlayParameter();
		playParameter.groupId = (int)groupId;
		playParameter.action = action;
		playParameter.npcId = npcId;
		GameFacade.Instance.SendNotification(NotificationName.VideoPhoneChange, playParameter);
	}
}