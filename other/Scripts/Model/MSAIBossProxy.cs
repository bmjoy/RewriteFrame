using Assets.Scripts.Define;
using Assets.Scripts.Proto;
using Leyoutech.Core.Effect;
using Eternity.FlatBuffer;
using Game.VFXController;
using Map;
using PureMVC.Patterns.Proxy;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KHeroType = Assets.Scripts.Define.KHeroType;
using LeyouDebug = Leyoutech.Utility.DebugUtility;

public class AIPlotInfo
{
	public AIPlotState m_AIPlotState;
	public byte m_InScene;
}

public class MSAIBossProxy : Proxy
{
	private PlayerAIState m_PlayerAIState;

	/// <summary>
	/// AI区域状态
	/// </summary>
	private Dictionary<ulong, AIPlotInfo> m_AIBossState = new Dictionary<ulong, AIPlotInfo>(2);    //key 为区域id

	/// <summary>
	/// AI区域音乐盒
	/// </summary>
	private Dictionary<ulong, GameObject> m_AISoundBoxs = new Dictionary<ulong, GameObject>();    //key 为区域id

	/// <summary>
	/// 前一帧怪物和主角的距离
	/// </summary>
	private Dictionary<uint, float> m_PreFrameMonsterDis = new Dictionary<uint, float>(8);   //key 怪物uid

	private List<uint> m_NearbyMonsterList = new List<uint>(8);         //new hero 消息发来的怪物id

	/// <summary>
	/// 服务器发来的怪物位置
	/// </summary>
	private Dictionary<uint, Vector3> m_MonsterPos = new Dictionary<uint, Vector3>(8);

	/// <summary>
	/// 靠近AI语音播放靠近怪巢的信息
	/// </summary>
	private Dictionary<ulong, AI_Distance_Info> m_NearbyAIVoicTimeDic = new Dictionary<ulong, AI_Distance_Info>(2);

	/// <summary>
	/// AI召唤的所有怪物
	/// </summary>
	private Dictionary<ulong, MonsterList> m_AICallMonsterDic = new Dictionary<ulong, MonsterList>(8);    //key a区域id

	/// <summary>
	/// 显示发现AI界面
	/// </summary>
	private Dictionary<ulong, bool> m_NeedShowFound = new Dictionary<ulong, bool>(8);    //key a区域id

	/// <summary>
	/// AI身上10秒倒计时时候的特效
	/// </summary>
	private EffectController m_AIActionEffect;

	/// <summary>
	/// 当前AI 区域id
	/// </summary>
	private ulong m_CurrentAIUid = 0;

	/// <summary>
	/// 上一次的AI 区域id
	/// </summary>
	private ulong m_LastAIUid = 0;

	/// <summary>
	/// 是否播过预警
	/// </summary>
	private bool m_AreadyPlayForeWarn = false;
	private bool m_HasCallBossSuccess = false;
	private bool m_PlayerInBattle = false;
	private bool m_NeedAddUpdate = true;
	private bool m_IsPlayIngEff = false;
	private bool m_IsPlayIngAIEnd = false;
	private ulong m_CurentTime = 0;

	/// <summary>
	/// 预警半径
	/// </summary>
	private readonly static int FORE_WARN_MAXRADIUS = 40000;

	/// <summary>
	/// ai区域id
	/// </summary>
	private readonly static int AI_AREA_ID = 2947667;

	/// <summary>
	///玩家靠近怪巢语音的距离
	/// </summary>
	private readonly static int DISTANCE_FROM_AI = 5500;

	/// <summary>
	/// 播靠近怪巢语音间隔(只在一直在里面生效)
	/// </summary>
	private readonly static ulong WARN_VOICE_INTERVAL_RANGLE = 3000;

	/// <summary>
	/// 播靠近怪巢语音IDs
	/// </summary>
	private int[] m_NearbyVoices = new int[3];

	private CfgEternityProxy m_CfgEternityProxy;

	private GameplayProxy m_GameplayProxy;

	private GamingConfigJujubeBattlefield m_JujubeBattlefield;

	public MSAIBossProxy() : base(ProxyName.MSAIBossProxy)
	{
		m_CfgEternityProxy = GameFacade.Instance.RetrieveProxy(ProxyName.CfgEternityProxy) as CfgEternityProxy;
		m_GameplayProxy = Facade.RetrieveProxy(ProxyName.GameplayProxy) as GameplayProxy;
		m_JujubeBattlefield = m_CfgEternityProxy.GetGamingConfig(1).Value.JujubeBattlefield.Value;
		m_NearbyVoices[0] = (int)m_JujubeBattlefield.SystemVoice.Value.NearStrangeNest;
		m_NearbyVoices[1] = (int)m_JujubeBattlefield.SystemVoice.Value.NearStrangeNestII;
		m_NearbyVoices[2] = (int)m_JujubeBattlefield.SystemVoice.Value.NearStrangeNestIII;
	}

	private bool CheckNpcType(KHeroType npcType)
	{
		switch (npcType)
		{
			case KHeroType.htMonster:
				return true;
			case KHeroType.htBoss:
				return true;
			case KHeroType.htPlotMonster:
				return true;
			case KHeroType.htEliteMonster1:
				return true;
			case KHeroType.htEliteMonster2:
				return true;
			case KHeroType.htEliteMonster3:
				return true;
			default:
				return false;
		}
	}

	/// <summary>
	/// 保存附近怪物信息
	/// </summary>
	public void SaveNewMonster(S2C_SYNC_NEW_HERO msg)
	{
		if (m_NeedAddUpdate)
		{
			m_NeedAddUpdate = false;
			ServerTimeUtil.Instance.OnTick += Update;
		}

		if (msg == null || msg.ownerPlayerID != 0)
		{
			return;
		}

		Npc npcData = m_CfgEternityProxy.GetNpcByKey((uint)msg.templateID);
		KHeroType npcType = (KHeroType)npcData.NpcType;
		if (CheckNpcType(npcType))
		{
			if (!m_NearbyMonsterList.Contains(msg.id))
			{
				m_NearbyMonsterList.Add(msg.id);
			}
		}
	}

	/// <summary>
	/// 删除怪物信息
	/// </summary>
	public void RemoveMonster(S2C_REMOVE_SCENE_OBJ msg)
	{
		uint objID = msg.objID;
		if (m_NearbyMonsterList.Contains(objID))
		{
			m_NearbyMonsterList.Remove(objID);
		}

		if (m_MonsterPos.ContainsKey(objID))
		{
			m_MonsterPos.Remove(objID);
		}

		Destroy(objID);
	}

	public void CleanAllMonster()
	{
		// 暴力执行
		m_NearbyMonsterList.Clear();
		m_MonsterPos.Clear();

		foreach (var info in m_AIBossState)
		{
			DestroySoundBox(info.Key);
		}
		m_AIBossState.Clear();

		m_AICallMonsterDic.Clear();
		m_NeedShowFound.Clear();
		m_NearbyAIVoicTimeDic.Clear();
		m_CurrentAIUid = 0;
	}

	/// <summary>
	/// 返回是否可播普通的预警||战斗
	/// </summary>
	/// <returns></returns>
	private bool CanPalyNormalBattle()
	{
		uint gamingMapId = m_CfgEternityProxy.GetCurrentGamingMapId();
		int mapType = m_CfgEternityProxy.GetMapByKey(gamingMapId).Value.GamingType;
		/// 根据地图类型过滤
		if (mapType == (int)KMapType.mapTeam)
		{
			return false;
		}
		return true;
	}

	/// <summary>
	///保存玩家是否进入战斗状态(加入仇恨列表)
	/// </summary>
	public void SavePlayerInBattle(bool f)
	{
		m_PlayerInBattle = f;
		if (f)
		{
			PlayForeWarnVoice(true);
			if (!IsInAIArea())
			{
				int soundId = 0;
				/// 在导演中
				if (m_CurrentAIUid > 0)
				{
					SpacecraftEntity aiEntity = m_GameplayProxy.GetEntityById<SpacecraftEntity>((uint)m_CurrentAIUid);
					if (aiEntity)
					{
						Npc npc = aiEntity.GetNPCTemplateVO();
						if (npc.SoundFightBegin > 0)
						{
							soundId = (int)npc.SoundFightBegin;
						}
					}
				}
				else if (CanPalyNormalBattle())
				{
					soundId = (int)SoundID.CombatOpen;
				}

				if (soundId != 0)
				{
					WwiseUtil.PlaySound(soundId, false, null);
				}
			}
			else
			{
				WwiseUtil.PlaySound((int)SoundID.AIAreaCombat, false, null);
				if (m_AIBossState.ContainsKey(m_CurrentAIUid) && m_AIBossState[m_CurrentAIUid].m_AIPlotState == AIPlotState.CallingBossSuccess)
				{
					WwiseUtil.PlaySound((int)SoundID.AIAreaCallingBossSuccess, false, null);
				}
			}
			SetPlayerState(PlayerAIState.InBattleState);
		}
		else
		{
			if (!IsInAIArea())
			{
				int soundId = 0;
				if (m_CurrentAIUid > 0)
				{
					SpacecraftEntity aiEntity = m_GameplayProxy.GetEntityById<SpacecraftEntity>((uint)m_CurrentAIUid);
					if (aiEntity)
					{
						Npc npc = aiEntity.GetNPCTemplateVO();
						if (npc.SoundFightBegin > 0)
						{
							soundId = (int)npc.SoundFightEnd;
						}
					}
				}
				else if (CanPalyNormalBattle())
				{
					soundId = (int)SoundID.CombatEnd;
				}

				if (soundId != 0)
				{
					WwiseUtil.PlaySound(soundId, false, null);
				}
			}
		}
	}

	public bool GetPlayerIsInBattle()
	{
		return m_PlayerInBattle;
	}

	public void SetPlayerIsInBattle(bool playerInBattle)
	{
		/// TODO
		/// 切场景收不到服务端移除仇恨
		/// 客户端强行处理一下
		if (m_PlayerInBattle && !playerInBattle && !IsInAIArea())
		{
			WwiseUtil.PlaySound((int)SoundID.CombatEnd, false, null);
		}
		m_PlayerInBattle = playerInBattle;
	}

	/// <summary>
	/// 判断是否在AI场景中
	/// </summary>
	private bool IsInAIArea()
	{
		uint mapID = m_CfgEternityProxy.GetCurrentMapId();
		uint areaID = (uint)MapManager.GetInstance().GetCurrentAreaUid();
		if (areaID == AI_AREA_ID)
			return true;
		return false;
	}

	/// <summary>
	/// 是否在ai区域
	/// </summary>
	/// <returns></returns>
	private bool IsInAI()
	{
		return (m_CurrentAIUid > 0 && m_AIBossState.ContainsKey(m_CurrentAIUid) && m_AIBossState[m_CurrentAIUid].m_InScene == 0);
	}

	/// <summary>
	/// 保存AI状态信息
	/// </summary>
	public void SaveAIState(S2C_CHANGE_PLOT_STATE msg)
	{
		ulong npc_id = msg.npc_id;
		byte in_scene = msg.in_scene;
		AIPlotState plot_state = (AIPlotState)msg.plot_state;

		if (m_CurrentAIUid > 0 && m_CurrentAIUid == npc_id && in_scene > 0)
		{
			m_LastAIUid = m_CurrentAIUid;
			OpenCountDownTimePanel(false);
			if (IsInAIArea() && !m_IsPlayIngAIEnd)
			{
				WwiseUtil.PlaySound((int)SoundID.AIAreaEnd, false, null);
				m_IsPlayIngAIEnd = true;
			}
		}

		/// TODO
		if (m_AIBossState.ContainsKey(m_CurrentAIUid) && m_AIBossState[m_CurrentAIUid].m_AIPlotState == AIPlotState.BeginBossTimeout)
		{
			StopAIActionEffect(m_AIActionEffect);
		}

		if (in_scene == 0 && m_CurrentAIUid == 0)       //==0 means in AI 
		{
			m_CurrentAIUid = npc_id;
			m_IsPlayIngEff = false;
			m_IsPlayIngAIEnd = false;
		}

		if (m_AIBossState.ContainsKey(npc_id))
		{
			m_AIBossState[npc_id].m_AIPlotState = plot_state;
			m_AIBossState[npc_id].m_InScene = in_scene;
		}
		else
		{
			AIPlotInfo aIPlotInfo = new AIPlotInfo();
			aIPlotInfo.m_AIPlotState = plot_state;
			aIPlotInfo.m_InScene = in_scene;
			m_AIBossState.Add(npc_id, aIPlotInfo);
		}

		if (plot_state == AIPlotState.CallingBossSuccess && in_scene > 0)
		{
			m_HasCallBossSuccess = true;
		}

		if (in_scene == 0)
		{
			SendMSAIBossMsgNotify(npc_id, m_PlayerAIState);
		}
	}

	/// <summary>
	/// 保存AI召唤的所有怪物信息
	/// </summary>
	public void SaveAICallMonsterInfo(S2C_PLOT_MONSTER_LIST msg)
	{
		if (m_AICallMonsterDic.ContainsKey(msg.npc_id))
		{
			m_AICallMonsterDic[msg.npc_id].m_MonsterList = msg.monster_uids;
		}
		else
		{
			MonsterList ml = new MonsterList();
			ml.m_MonsterList = msg.monster_uids;
			m_AICallMonsterDic.Add(msg.npc_id, ml);
		}
		int count = msg.monster_uids.Count;
		for (int i = 0; i < count; ++i)
		{
			uint uid = (uint)msg.monster_uids[i];
			SpacecraftEntity ship = m_GameplayProxy.GetEntityById<SpacecraftEntity>(uid);
			if (ship != null && ship.GetHeroType() == Assets.Scripts.Define.KHeroType.htEliteMonster2)
			{
				m_HasCallBossSuccess = true;
			}
		}
	}

	/// <summary>
	/// 获取当前玩家所在的AI区域id
	/// </summary>
	public ulong GetCurrentAIUId()
	{
		return m_CurrentAIUid;
	}

	/// <summary>
	/// 是否需要改变死亡的特效 
	/// </summary>
	public bool IsChangeDeadEff(uint uid)
	{
		ulong areaid = 0;
		foreach (var item in m_AICallMonsterDic)
		{
			if (item.Value.m_MonsterList.Contains(uid))
			{
				areaid = item.Key;
				break;
			}
		}
		if (areaid <= 0 || !m_AIBossState.ContainsKey(areaid))
		{
			return false;
		}
		if (m_AIBossState[areaid].m_AIPlotState != AIPlotState.LostPlayer)
		{
			return false;
		}

		return true;
	}

	private void Update()
	{
		int count = m_NearbyMonsterList.Count;
		if (count == 0)
		{
			PlayForeWarnVoice(false);
			return;
		}
		ulong cureenttime = ServerTimeUtil.Instance.GetNowTimeMSEL();
		if (cureenttime - m_CurentTime < 1000)
		{
			return;
		}

		m_CurentTime = cureenttime;
		if (IsDead() || IsLeaping() || !IsInSpace())
		{
			OpenCountDownTimePanel(false);
			return;
		}
		SpeekWarnVoiceNearbyAI(cureenttime);

		BaseEntity main = m_GameplayProxy.GetEntityById<BaseEntity>(m_GameplayProxy.GetMainPlayerUID());
		bool isInSpace = IsInSpace();
		bool isEscapeFromAI = false;
		bool isEscapeFromM = true;
		for (int i = 0; i < count; ++i)
		{
			uint uid = m_NearbyMonsterList[i];
			SpacecraftEntity ship = m_GameplayProxy.GetEntityById<SpacecraftEntity>(uid);
			if (ship != null && (ship.GetAttribute(AttributeName.kHP) > 0))
			{
				Vector3 targetPosition = ship.transform.position;
				float targetDistance = Vector3.Distance(targetPosition, main.transform.position) * (isInSpace ? GameConstant.METRE_PER_UNIT : 1);
				if (!m_PreFrameMonsterDis.ContainsKey(uid))
				{
					if (m_CurrentAIUid == uid)
					{
						isEscapeFromAI = false;
						isEscapeFromM = false;
					}

					m_PreFrameMonsterDis.Add(uid, targetDistance);
				}
				else
				{
					if (uid == m_CurrentAIUid && m_PreFrameMonsterDis[uid] < targetDistance)
					{
						isEscapeFromAI = true;
					}
					if (m_PreFrameMonsterDis[uid] > targetDistance)
					{
						isEscapeFromM = false;
					}
					m_PreFrameMonsterDis[uid] = targetDistance;
				}
			}
			else if (ship == null && m_MonsterPos.ContainsKey(uid))
			{
				Vector3 targetPosition = m_GameplayProxy.ServerAreaOffsetToClientPosition(m_MonsterPos[uid]);
				float targetDistance = Vector3.Distance(targetPosition, main.transform.position) * (isInSpace ? GameConstant.METRE_PER_UNIT : 1);
				if (m_PreFrameMonsterDis.ContainsKey(uid))
				{
					if (m_CurrentAIUid == uid)
					{
						isEscapeFromAI = false;
						isEscapeFromM = false;
					}

					m_PreFrameMonsterDis[uid] = targetDistance;
				}
				else
				{
					if (uid == m_CurrentAIUid && m_PreFrameMonsterDis[uid] < targetDistance)
					{
						isEscapeFromAI = true;
					}
					if (m_PreFrameMonsterDis[uid] > targetDistance)
					{
						isEscapeFromM = false;
					}
					m_PreFrameMonsterDis.Add(uid, targetDistance);
				}
			}

		}

		bool forewarn = false;
		foreach (var item in m_PreFrameMonsterDis)
		{
			if (item.Value <= FORE_WARN_MAXRADIUS)
			{
				forewarn = true;
				break;
			}
		}
		PlayForeWarnVoice(forewarn);
		if (m_PlayerInBattle && isEscapeFromAI && isEscapeFromM)
		{
			SetPlayerState(PlayerAIState.EscapeFromAI);
		}
	}

	private void SpeekWarnVoiceNearbyAI(ulong ServerTime)
	{
		if (m_CurrentAIUid <= 0)
		{
			return;
		}

		if (IsInAIArea())
		{
			bool isHave = m_NearbyAIVoicTimeDic.ContainsKey(m_CurrentAIUid);
			if (isHave && m_NearbyAIVoicTimeDic[m_CurrentAIUid].times >= 3)
			{
				return;
			}
			BaseEntity main = m_GameplayProxy.GetEntityById<BaseEntity>(m_GameplayProxy.GetMainPlayerUID());
			SpacecraftEntity AI = m_GameplayProxy.GetEntityById<SpacecraftEntity>((uint)m_CurrentAIUid);
			if (main == null || main.transform == null || AI == null || AI.GetSkinTransform() == null)
			{
				return;
			}

			// TODO.
			// 先放这
			if (!m_IsPlayIngEff && m_AIBossState[m_CurrentAIUid].m_AIPlotState > AIPlotState.Idle && m_AIBossState[m_CurrentAIUid].m_AIPlotState < AIPlotState.BeginCallingBoss)
			{
				Vector3 targetPosition = AI.transform.position;
				Vector3 viewportPoint = Camera.main.WorldToViewportPoint(targetPosition);
				bool inScreen = viewportPoint.x >= 0 && viewportPoint.y >= 0 &&
					viewportPoint.x <= 1 && viewportPoint.y <= 1 && viewportPoint.z > Camera.main.nearClipPlane;
				if (inScreen)
				{
					m_IsPlayIngEff = true;
					PlayFoundEnemyPanel();
				}
			}
			float targetDistance = Vector3.Distance(AI.transform.position, main.transform.position) * GameConstant.METRE_PER_UNIT;
			if (targetDistance <= DISTANCE_FROM_AI)
			{
				if (isHave)
				{
					if (!m_NearbyAIVoicTimeDic[m_CurrentAIUid].isStayInAI)
					{
						m_NearbyAIVoicTimeDic[m_CurrentAIUid].times += 1;
						m_NearbyAIVoicTimeDic[m_CurrentAIUid].serverTime = ServerTime;
						int times = m_NearbyAIVoicTimeDic[m_CurrentAIUid].times;
						SendNotification(m_NearbyVoices[times - 1]);
					}
					else if (m_NearbyAIVoicTimeDic[m_CurrentAIUid].isStayInAI &&
						ServerTime - m_NearbyAIVoicTimeDic[m_CurrentAIUid].serverTime > WARN_VOICE_INTERVAL_RANGLE)
					{
						m_NearbyAIVoicTimeDic[m_CurrentAIUid].times += 1;
						m_NearbyAIVoicTimeDic[m_CurrentAIUid].serverTime = ServerTime;
						int times = m_NearbyAIVoicTimeDic[m_CurrentAIUid].times;
						SendNotification(m_NearbyVoices[times - 1]);
					}
				}
				else
				{
					AI_Distance_Info AIiNFO = new AI_Distance_Info(1, ServerTime, true);
					m_NearbyAIVoicTimeDic.Add(m_CurrentAIUid, AIiNFO);
					SendNotification(m_NearbyVoices[0]);
				}
			}
			else if (isHave)
			{
				m_NearbyAIVoicTimeDic[m_CurrentAIUid].isStayInAI = false;
			}
		}
	}

	/// <summary>
	/// 打开缩圈界面
	/// </summary>
	private void PlayFoundEnemyPanel()
	{
		if (m_CurrentAIUid > 0 && !m_NeedShowFound.ContainsKey(m_CurrentAIUid))
		{
			SpacecraftEntity ship = m_GameplayProxy.GetEntityById<SpacecraftEntity>((uint)m_CurrentAIUid);
			if (ship != null)
			{
				Vector3 targetPosition = ship.transform.position;
				Vector3 viewportPoint = Camera.main.WorldToViewportPoint(targetPosition);
				bool inScreen = viewportPoint.x >= 0.15 && viewportPoint.y >= 0.15 &&
					viewportPoint.x <= 0.85 && viewportPoint.y <= 0.85 && viewportPoint.z > Camera.main.nearClipPlane;

				if (inScreen)
				{
					m_NeedShowFound.Add(m_CurrentAIUid, true);
					UIManager.Instance.OpenPanel(UIPanel.HudFoundEnemyPanel, null);
				}
			}
		}
	}

	/// <summary>
	/// 播放预警音效
	/// </summary>
	private void PlayForeWarnVoice(bool isplay)
	{
		if (!CanPalyNormalBattle())
		{
			return;
		}

		if (isplay && !m_AreadyPlayForeWarn)
		{
			m_AreadyPlayForeWarn = true;
			if (IsInAIArea())
			{
				WwiseUtil.PlaySound((int)SoundID.CombatWranEnd, false, null);
				WwiseUtil.PlaySound((int)SoundID.CombatEnd, false, null);
				WwiseUtil.PlaySound((int)SoundID.AIAreaWran, false, null);
			}
			else
			{
				WwiseUtil.PlaySound((int)SoundID.CombatWranOpen, false, null);
			}
		}
		else if (!isplay && m_AreadyPlayForeWarn) // && !m_PlayerInBattle? 预警未必代表加入仇恨
		{
			if (IsInAIArea())
			{
				WwiseUtil.PlaySound((int)SoundID.AIAreaEnd, false, null);
			}
			else
			{
				m_AreadyPlayForeWarn = false;
				WwiseUtil.PlaySound((int)SoundID.CombatWranEnd, false, null);
			}

		}
	}

	/// <summary>
	/// 保存玩家状态
	/// </summary>
	private void SetPlayerState(PlayerAIState pas)
	{
		if (pas == m_PlayerAIState)
		{
			return;
		}
		m_PlayerAIState = pas;
	}

	private void SendMSAIBossMsgNotify(ulong npcid, PlayerAIState state)
	{
		MSG_MSAIBoss_Info info = new MSG_MSAIBoss_Info();
		info.aiState = m_AIBossState[npcid].m_AIPlotState;
		info.playerState = state;
		GameFacade.Instance.SendNotification(NotificationName.AIPlotState_Change, info);
		Triggle(npcid);
	}

	/// <summary>
	/// 是否在太空中
	/// </summary>
	/// <returns>bool</returns>
	private bool IsInSpace()
	{
		return m_CfgEternityProxy.IsSpace();
	}

	/// <summary>
	/// 是否已经死亡
	/// </summary>
	/// <returns>bool</returns>
	private bool IsDead()
	{
		SpacecraftEntity entity = m_GameplayProxy.GetEntityById<SpacecraftEntity>(m_GameplayProxy.GetMainPlayerUID());
		if (entity)
		{
            return entity.IsDead();
		}
		return false;
	}

	/// <summary>
	/// 是否在跃迁中
	/// </summary>
	/// <returns>bool</returns>
	private bool IsLeaping()
	{
		SpacecraftEntity entity = m_GameplayProxy.GetEntityById<SpacecraftEntity>(m_GameplayProxy.GetMainPlayerUID());
        if (entity)
        {
            return entity.IsLeap();
        }

		return false;
	}

	/// <summary>
	/// 根据状态触发效果
	/// </summary>
	private void Triggle(ulong npcid)
	{
		if (npcid > 0) //&& m_AIBossState[m_CurrentAIUid] == AIPlotState.Invaded)
		{
			SpacecraftEntity ship = m_GameplayProxy.GetEntityById<SpacecraftEntity>((uint)npcid);
			switch (m_AIBossState[npcid].m_AIPlotState)
			{
				case AIPlotState.Invaded:
					if (ship != null && m_LastAIUid != m_CurrentAIUid)
					{
						//播放入侵语音1
						SendNotification((int)m_JujubeBattlefield.SystemVoice.Value.AccessArea, PlayInvaded);
					}
					break;
				case AIPlotState.Invaded2:
					if (ship != null && m_AIBossState[npcid].m_InScene == 0)
					{
						PlayInvadedTwo(npcid);
					}
					break;
				case AIPlotState.CloseInvaded2:
					DestroySoundBox(npcid);
					break;
				case AIPlotState.MonsterRetreat:
					if (m_PlayerInBattle && !IsDead())
					{
						//播放撤退1音乐1016to1019
						if (ship != null && ship.GetSkinRootTransform() != null)
						{
							WwiseUtil.PlaySound((int)SoundID.AIAreaEvent, false, ship.GetSkinRootTransform());
						}
						UIManager.Instance.StartCoroutine(DelayToPlayVoice(2f, (int)m_JujubeBattlefield.SystemVoice.Value.MobRetreat));
						WwiseUtil.PlaySound((int)SoundID.AIAreaMonsterRetreat, false, null);
					}
					break;
				case AIPlotState.BeginCallingBoss:
					if (m_PlayerInBattle && !IsDead())
					{
						//播放激战音乐
						WwiseUtil.PlaySound((int)SoundID.AIAreaCallingBossSuccess, false, null);
					}
					break;
				case AIPlotState.CallingBossSuccess:
					if (m_PlayerInBattle && !IsDead())
					{
						//播放进入boss战音乐
						SendNotification((int)m_JujubeBattlefield.SystemVoice.Value.BossBattle);
					}
					break;
				case AIPlotState.MonsterRetreatFail:
				case AIPlotState.CallingBossFail:
					if (!IsDead())
					{
						//播放胜利1音乐
						if (ship != null)
						{
							SendNotification((int)m_JujubeBattlefield.SystemVoice.Value.WipeOutMobs);
						}
					}
					OpenCountDownTimePanel(false);
					break;
				case AIPlotState.DefeatBoss:
					if (!IsDead())
					{
						//播放胜利2音乐
						WwiseUtil.PlaySound((int)SoundID.AIAreaDefeatBoss, false, null);
					}

					OpenCountDownTimePanel(false);
					break;
				case AIPlotState.BeginBossTimeout:
					if (!IsDead() && m_PlayerInBattle)
					{
						//播放10秒倒计时音乐1013to1016
						SendNotification((int)m_JujubeBattlefield.SystemVoice.Value.TenSeconds);
						WwiseUtil.PlaySound((int)SoundID.AIAreaBeginBossTimeout, false, null);
					}
					AddEffectOnAIAction();
					break;
				case AIPlotState.BossTimeout:
					OpenCountDownTimePanel(false);
					AddEffectOnPlayer();
					break;
				case AIPlotState.LostPlayer:
					OnLostPlayer(npcid);
					break;
				case AIPlotState.PlayerOutOfScene:
					OpenCountDownTimePanel(false);
					break;
			}
		}
	}

	/// <summary>
	/// 丢失玩家
	/// </summary>
	private void OnLostPlayer(ulong uid)
	{
		/// 导演存在不自杀情况，手动清理
		if (uid == m_CurrentAIUid)
		{
			m_CurrentAIUid = 0;
			m_LastAIUid = 0;
		}

		if (m_AIBossState.ContainsKey(uid))
		{
			DestroySoundBox(uid);
			m_AIBossState.Remove(uid);
		}
	}

	/// <summary>
	/// 打开倒计时界面
	/// </summary>
	public void OpenCountDownTimePanel(bool isneed, uint time = 10)
	{
		if (isneed)
		{
			MSG_TimeInfo info = new MSG_TimeInfo();
			info.time = time;
			info.CallbackAction = PlayTimeOverAction;
			UIManager.Instance.OpenPanel(UIPanel.HudDangerTipPanel, info);
		}
		else
		{
			UIManager.Instance.ClosePanel(UIPanel.HudDangerTipPanel);
		}
	}

	private void PlayTimeOverAction()
	{
		if (!IsDead())
		{
			SendNotification((int)m_JujubeBattlefield.SystemVoice.Value.CountdownExplosion);
		}
	}

	/// <summary>
	/// 播放另一种怪物的死亡特效
	/// </summary>
	public void AddMonsterEffect(uint uid)
	{
		SpacecraftEntity ship = m_GameplayProxy.GetEntityById<SpacecraftEntity>(uid);
		if (ship != null && ship.GetSkinRootTransform())
		{
			ship.GetSkinRootTransform().gameObject.SetActive(false);

			EffectController dropItemFX = EffectManager.GetInstance().CreateEffect(m_JujubeBattlefield.Basic.Value.MonsterTeleportHome, EffectManager.GetEffectGroupNameInSpace(false));
			dropItemFX.transform.SetParent(null, false);
			dropItemFX.transform.localPosition = ship.GetSkinRootTransform().position;

			WwiseUtil.PlaySound(10002, false, null);
		}
	}

	/// <summary>
	/// 给导演挂倒计时特效
	/// </summary>
	private void AddEffectOnAIAction()
	{
		if (m_CurrentAIUid > 0)
		{
			SpacecraftEntity ship = m_GameplayProxy.GetEntityById<SpacecraftEntity>((uint)m_CurrentAIUid);

			if (ship != null && ship.GetSkinRootTransform() != null)
			{
				m_AIActionEffect = EffectManager.GetInstance().CreateEffect(m_JujubeBattlefield.Director.Value.MonsterLairCountdown, EffectManager.GetEffectGroupNameInSpace(false));
				m_AIActionEffect.transform.SetParent(ship.GetSkinRootTransform(), false);

				WwiseUtil.PlaySound(10003, false, null);
				UIManager.Instance.StartCoroutine(DelayToStopEffect(m_AIActionEffect, 9.8f));
			}
		}
	}

	/// <summary>
	/// 播放全屏特效
	/// </summary>
	private void AddEffectOnPlayer(int type = 0)
	{
		if (m_CurrentAIUid > 0 && !IsDead() && IsInSpace())
		{
			string effPath = string.Empty;
			if (type == 1)
			{
				effPath = m_JujubeBattlefield.Basic.Value.InterferenceEffect;
				WwiseUtil.PlaySound((int)WwiseMusic.Interference_FX_Sound, false, null);
			}
			else
			{
				effPath = m_JujubeBattlefield.Director.Value.FailureSuicide;
			}

			SpacecraftEntity main = m_GameplayProxy.GetEntityById<SpacecraftEntity>(m_GameplayProxy.GetMainPlayerUID());

			m_AIActionEffect = EffectManager.GetInstance().CreateEffect(effPath, EffectManager.GetEffectGroupNameInSpace(true));
			m_AIActionEffect.transform.SetParent(main.GetSkinRootTransform(), false);
			m_AIActionEffect.SetCreateForMainPlayer(true);
		}
	}

	/// <summary>
	/// 播放入侵语音
	/// </summary>
	private void PlayInvaded()
	{
		if (m_CurrentAIUid <= 0)
		{
			return;
		}

		AddEffectOnPlayer(1);
		SpacecraftEntity AI = m_GameplayProxy.GetEntityById<SpacecraftEntity>((uint)m_CurrentAIUid);
		if (AI == null)
		{
			return;
		}

		Vector3 targetPosition = AI.transform.position;
		Vector3 viewportPoint = Camera.main.WorldToViewportPoint(targetPosition);
		bool inScreen = viewportPoint.x >= 0 && viewportPoint.y >= 0 &&
			viewportPoint.x <= 1 && viewportPoint.y <= 1 && viewportPoint.z > Camera.main.nearClipPlane;
		if (inScreen)
		{
			m_IsPlayIngEff = true;
			PlayFoundEnemyPanel();
		}


		SendNotification((int)m_JujubeBattlefield.SystemVoice.Value.HackedWarning);
	}

	/// <summary>
	/// 播放入侵2效果(专门给区域-精英战用的)
	/// </summary>
	private void PlayInvadedTwo(ulong aiUid)
	{
		if (aiUid <= 0)
		{
			return;
		}
		SpacecraftEntity AI = m_GameplayProxy.GetEntityById<SpacecraftEntity>((uint)aiUid);
		if (AI)
		{
			Npc npc = AI.GetNPCTemplateVO();
			if (npc.Behavior > 0)
			{
				PlotBehavior plotBehavior =  m_CfgEternityProxy.PlotBehaviorsByKey((uint)npc.Behavior);
				/// 特效
				string fightBeginFx = plotBehavior.FightBeginFx;
				if (!string.IsNullOrEmpty(fightBeginFx))
				{
					SpacecraftEntity main = m_GameplayProxy.GetEntityById<SpacecraftEntity>(m_GameplayProxy.GetMainPlayerUID());
					m_AIActionEffect = EffectManager.GetInstance().CreateEffect(fightBeginFx, EffectManager.GetEffectGroupNameInSpace(true));
					m_AIActionEffect.transform.SetParent(main.GetSkinRootTransform(), false);
					m_AIActionEffect.SetCreateForMainPlayer(true);
					/// 特效声音
					if (plotBehavior.FightBeginFxSound > 0)
					{
						WwiseUtil.PlaySound((int)plotBehavior.FightBeginFxSound, false, null);
					}
				}
				/// HUD
				if (Enum.TryParse(plotBehavior.FightBeginHud, out UIPanel panelName))
				{
					MSG_WarningHUDInfo info = new MSG_WarningHUDInfo();
					info.languageId = plotBehavior.FightBeginHudText;
					info.time = plotBehavior.FightBeginHudTime;
					UIManager.Instance.OpenPanel(panelName, info);
				}

				CreateSoundBox(aiUid);
				if (plotBehavior.FightBeginVideo > 0)
				{
					SendNotification((int)plotBehavior.FightBeginVideo);
				}
			}
		}
	}

	/// <summary>
	/// 创建音乐盒子
	/// </summary>
	/// <param name="aiUid"></param>
	private void CreateSoundBox(ulong aiUid)
	{
		if (aiUid <= 0 || m_AISoundBoxs.ContainsKey(aiUid))
		{
			/// TODO.
			/// or play sound box begin music
			Debug.LogError("aiUid is 0 or Plot repeated trigger the Invaded2");
			return;
		}

		SpacecraftEntity AI = m_GameplayProxy.GetEntityById<SpacecraftEntity>((uint)aiUid);
		if (AI)
		{
			Npc npc = AI.GetNPCTemplateVO();
			if (npc.Behavior > 0)
			{
				PlotBehavior plotBehavior = m_CfgEternityProxy.PlotBehaviorsByKey((uint)npc.Behavior);
				if (plotBehavior.FightBeginSound > 0)
				{
					GameObject soundBox = WwiseManager.CreatTAkAmbientSphereBox((int)plotBehavior.FightBeginSound, (int)plotBehavior.FightEndSound);
					soundBox.transform.position = AI.GetRootTransform().position;
					float scale = plotBehavior.SoundBoxSize;
					soundBox.transform.localScale = new Vector3(scale, scale, scale);
					m_AISoundBoxs.Add(aiUid, soundBox);
				}
			}
		}
	}

	/// <summary>
	/// 销毁音乐盒子
	/// </summary>
	public void DestroySoundBox(ulong aiUid)
	{
		if (aiUid <= 0)
		{
			return;
		}
		if (m_AISoundBoxs.ContainsKey(aiUid))
		{
			GameObject.Destroy(m_AISoundBoxs[aiUid]);
			m_AISoundBoxs.Remove(aiUid);
		}
	}

	public void CheckBuffPlaySound(uint buffId, bool isOpen)
	{
		CfgSkillSystemProxy skillProxy = GameFacade.Instance.RetrieveProxy(ProxyName.CfgSkillSystemProxy) as CfgSkillSystemProxy;
		SkillBuff configVO = skillProxy.GetBuff((int)buffId);
		if (isOpen && configVO.SoundAppearance > 0)
		{
			WwiseUtil.PlaySound(configVO.SoundAppearance, false, null);
		}
		else if (!isOpen && configVO.SoundEnd > 0)
		{
			WwiseUtil.PlaySound(configVO.SoundEnd, false, null);
		}
	}

	/// <summary>
	/// 删除AOE_chongneng特效
	/// </summary>
	private IEnumerator DelayToStopEffect(EffectController vFX, float time)
	{
		yield return new WaitForSeconds(time);
		StopAIActionEffect(vFX);
	}

	private void StopAIActionEffect(EffectController vFX)
	{
		if (vFX != null && vFX == m_AIActionEffect)
		{
			vFX.StopAndRecycleFX();
			WwiseUtil.PlaySound(10008, false, null);
			m_AIActionEffect = null;
		}
	}

	/// <summary>
	/// 延迟播语音
	/// </summary>
	private IEnumerator DelayToPlayVoice(float time, int groupId)
	{
		yield return new WaitForSeconds(time);
		SendNotification(groupId);
	}

	/// <summary>
	/// 播放入侵语音2  在入侵语音1播放完
	/// </summary>
	private void DelayToPlayInvaded2()
	{
		if (m_AIBossState[m_CurrentAIUid].m_AIPlotState == AIPlotState.Invaded)
			WwiseUtil.PlaySound(WwiseManager.voiceComboID, WwiseMusicSpecialType.SpecialType_Voice_GetTheTask, WwiseMusicPalce.Palce_1st, false, null);
	}

	/// <summary>
	/// 播放撤退语音2  在撤退语音1播放完
	/// </summary>
	private void DelayToPlayRetreat2()
	{
		if (m_AIBossState[m_CurrentAIUid].m_AIPlotState == AIPlotState.MonsterRetreat)
			WwiseUtil.PlaySound(WwiseManager.voiceComboID, WwiseMusicSpecialType.SpecialType_Voice_GetTheTask, WwiseMusicPalce.Palce_1st, false, null);
	}

	/// <summary>
	/// 清空AI信息
	/// </summary>
	private void Destroy(ulong aiuid)
	{
		if (m_AIBossState.ContainsKey(aiuid))
		{
			DestroySoundBox(aiuid);
			m_AIBossState.Remove(aiuid);
		}

		if (m_AICallMonsterDic.ContainsKey(aiuid))
			m_AICallMonsterDic.Remove(aiuid);
		if (m_NeedShowFound.ContainsKey(aiuid))
			m_NeedShowFound.Remove(aiuid);
		if (m_NearbyAIVoicTimeDic.ContainsKey(aiuid))
			m_NearbyAIVoicTimeDic.Remove(aiuid);

		if (m_CurrentAIUid == aiuid)
		{
			m_CurrentAIUid = 0;
			if (IsInAIArea() && !m_IsPlayIngAIEnd)
			{
				WwiseUtil.PlaySound((int)SoundID.AIAreaEnd, false, null);
				m_IsPlayIngAIEnd = true;
			}
		}
	}

	private void SendNotification(int groupId, Action action = null, uint npcId = 0)
	{
		PlayParameter playParameter = new PlayParameter();
		playParameter.groupId = groupId;
		playParameter.action = action;
		playParameter.npcId = npcId;
		GameFacade.Instance.SendNotification(NotificationName.VideoPhoneChange, playParameter);
	}
}
