using Assets.Scripts.Define;
using Eternity.FlatBuffer;
using Map;
using PureMVC.Patterns.Proxy;
using System.Collections.Generic;
using UnityEngine;

public class TaskTrackingProxy : Proxy
{
	/// <summary>
	/// 所有场景有传送点信息
	/// key  : 场景ID
	/// value: 传送点列表
	/// </summary>
	private Dictionary<int, List<Teleport>> m_SceneID2Teleports;
	/// <summary>
	/// 所有场景的NPC信息
	/// key  : NpcID
	/// value: Npc信息
	/// </summary>
	private Dictionary<ulong, List<NpcInfo>> m_NpcID2NpcInfos;
	/// <summary>
	/// 所有区域的跃迁点信息
	/// </summary>
	private Dictionary<ulong, Dictionary<ulong, List<Leap>>> m_Scene2AreaLeapInfos;

	/// <summary>
	/// 是否已重建
	/// </summary>
	private bool m_Rebuilded;
	/// <summary>
	/// 跟踪列表
	/// </summary>
	private List<TrackingInfo> m_TrackingList = new List<TrackingInfo>();
	/// <summary>
	/// NPC的跟踪列表
	/// </summary>
	private Dictionary<uint, List<TrackingInfo>> m_ID2NpcList = new Dictionary<uint, List<TrackingInfo>>();
	/// <summary>
	/// 跃迁点的跟踪列表
	/// </summary>
	private Dictionary<uint, List<TrackingInfo>> m_ID2LeapList = new Dictionary<uint, List<TrackingInfo>>();
	/// <summary>
	/// 上次更新时的地图ID
	/// </summary>
	private uint m_LastMapID = 0;
	/// <summary>
	/// 上次更新时的区域ID
	/// </summary>
	private uint m_LastAreaID = 0;
	/// <summary>
	/// 去重后的任务目标
	/// </summary>
	private Dictionary<uint, uint> m_TID2UID = new Dictionary<uint, uint>();
	/// <summary>
	/// 已添加标记的目标
	/// </summary>
	private HashSet<uint> m_AreadyAddMission = new HashSet<uint>();
	/// <summary>
	/// 跟踪模式
	/// </summary>
	public enum TrackingMode
	{
		/// <summary>
		/// 无
		/// </summary>
		NONE,
		/// <summary>
		/// 跃迁点
		/// </summary>
		LeapPoint,
		/// <summary>
		/// NPC和坐标点
		/// </summary>
		NpcAndPoint
	}

	/// <summary>
	/// 跟踪信息
	/// </summary>
	public class TrackingInfo
	{
		/// <summary>
		/// 跟踪模式
		/// </summary>
		public TrackingMode Mode;
		/// <summary>
		/// 任务数据
		/// </summary>
		public MissionVO MissionVO;
		/// <summary>
		/// 任务tid
		/// </summary>
		public uint MissionTid;
		/// <summary>
		/// 任务类型
		/// </summary>
		public MissionType MissionType;
		/// <summary>
		/// 目标状态
		/// </summary>
		public MissionState MissionState;
		/// <summary>
		/// 跃迁点ID
		/// </summary>
		public ulong LeapPointID;
		/// <summary>
		/// 最终点
		/// </summary>
		public ulong EndingLeapPointID;
		/// <summary>
		/// NPC TID
		/// </summary>
		public long NpcTID;
		/// <summary>
		/// NPC UID
		/// </summary>
		public uint NpcUID;
		/// <summary>
		/// NPC区域
		/// </summary>
		public ulong NpcArea;
		/// <summary>
		/// 坐标点
		/// </summary>
		public Vector3 Position;

		/// <summary>
		/// 是否需要过远提示
		/// </summary>
		public bool FarNotice;
		/// <summary>
		/// 过远提示 阈值
		/// </summary>
		public float FarNoticeDistance;
	}

	public TaskTrackingProxy() : base(ProxyName.TaskTrackingProxy) { }

	#region 任务跟踪

	/// <summary>
	/// 获取所有任务跟踪
	/// </summary>
	/// <returns></returns>
	public List<TrackingInfo> GetAllTrackings()
	{
		if (!m_Rebuilded)
			RebuildTracking();

		UpdateNearTarget();

		return m_TrackingList;
	}

	/// <summary>
	/// 获取指定NPC的任务跟踪
	/// </summary>
	/// <param name="id"></param>
	/// <returns></returns>
	public List<TrackingInfo> GetNpcTrackings(uint id)
	{
		if (!m_Rebuilded)
			RebuildTracking();

		UpdateNearTarget();

		if (m_ID2NpcList.ContainsKey(id))
			return m_ID2LeapList[id];
		else
			return null;
    }
    /// <summary>
    /// 获取任务状态
    /// </summary>
    /// <param name="tid">npc的tid</param>
    /// <returns></returns>
    public TrackingInfo GetNpcMission(uint uid, uint tid)
    {
        if (!m_Rebuilded)
            RebuildTracking();

        UpdateNearTarget();

        if (!m_TID2UID.ContainsKey(tid) || m_TID2UID[tid] == uid)
        {
            if (m_ID2NpcList.ContainsKey(tid))
            {
                TrackingInfo branchMission = null;
                foreach (TrackingInfo tracking in m_ID2NpcList[tid])
                {
                    if (tracking.MissionType == MissionType.Main)
                        return tracking;
                    if (tracking.MissionType == MissionType.Branch)
                        branchMission = tracking;
                }
                return branchMission;
            }
        }
        return null;
    }
    /// <summary>
    /// 获取任务状态
    /// </summary>
    /// <param name="tid">npc的tid</param>
    /// <returns></returns>
    public MissionType GetNpcMissionType(uint uid, uint tid)
	{
		if (!m_Rebuilded)
			RebuildTracking();

		UpdateNearTarget();

		if (!m_TID2UID.ContainsKey(tid) || m_TID2UID[tid] == uid)
		{
			bool hasMain = false;
			bool hasSide = false;

			if (m_ID2NpcList.ContainsKey(tid))
			{
				foreach (TrackingInfo tracking in m_ID2NpcList[tid])
				{
					if (tracking.MissionType == MissionType.Main)
						hasMain = true;
					else if (tracking.MissionType == MissionType.Branch)
						hasSide = true;
				}
			}
			return hasMain ? MissionType.Main : (hasSide ? MissionType.Branch : MissionType.None);
		}
		return MissionType.None;
	}
	/// <summary>
	/// 设置已经添加任务指引的信息
	/// </summary>
	/// <param name="isRemove">  删除 true  添加 false</param>
	/// <returns></returns>
	public void SetAreadyAddMissionInfo(uint uid, uint tid, bool isRemove = false)
	{
		uid = uid > 0 ? uid : m_TID2UID[tid];

		if (isRemove && m_AreadyAddMission.Contains(uid))
			m_AreadyAddMission.Remove(uid);
		if (!isRemove && !m_AreadyAddMission.Contains(uid))
			m_AreadyAddMission.Add(uid);
	}

	public bool GetAreadyAddMissionInfo(uint uid, uint tid)
	{
		uid = uid > 0 ? uid : m_TID2UID[tid];
		if (m_AreadyAddMission.Contains(uid))
			return true;
		return false;
	}

	/// <summary>
	/// 获取指定跃迁点的任务跟踪
	/// </summary>
	/// <param name="id"></param>
	/// <returns></returns>
	public List<TrackingInfo> GetLeapTrackings(uint id)
	{
		if (!m_Rebuilded)
			RebuildTracking();

		//UpdateNearTarget();

		if (m_ID2LeapList.ContainsKey(id))
			return m_ID2LeapList[id];
		else
			return null;
	}

	/// <summary>
	/// 获取跳点的任务状态
	/// </summary>
	/// <param name="tid"></param>
	/// <returns></returns>
    /*
	public MissionType GetLeapMissionType(uint tid)
	{
		bool hasMain = false;
		bool hasSide = false;

		if (m_ID2LeapList.ContainsKey(tid))
		{
			foreach (TrackingInfo tracking in m_ID2LeapList[tid])
			{
				if (tracking.MissionType == MissionType.Main)
					hasMain = true;
				else if (tracking.MissionType == MissionType.Branch)
					hasSide = true;
			}
		}
		return hasMain ? MissionType.Main : (hasSide ? MissionType.Branch : MissionType.None);
	}
    */

	/// <summary>
	/// 重建跟踪数据
	/// </summary>
	public void RebuildTracking()
	{
		m_Rebuilded = false;

		m_TrackingList.Clear();
		m_ID2NpcList.Clear();
		m_ID2LeapList.Clear();

		CfgEternityProxy cfg = Facade.RetrieveProxy(ProxyName.CfgEternityProxy) as CfgEternityProxy;
		GameplayProxy proxy = Facade.RetrieveProxy(ProxyName.GameplayProxy) as GameplayProxy;

		Transform transform = proxy.GetMainPlayerSkinTransform();
		if (transform == null)
			return;

		m_Rebuilded = true;

		//构建任务跟踪数据
		RebuildTrackingList(cfg.GetCurrentGamingMapId(),//当前地图
			MapManager.GetInstance().GetCurrentAreaUid(),//当前区域
			proxy.ClientToServerAreaOffset(transform.position));//当前坐标

		//分类标记
		foreach (TrackingInfo tracking in m_TrackingList)
		{
			if (tracking.Mode == TrackingMode.LeapPoint)
			{
				if (!m_ID2LeapList.ContainsKey((uint)tracking.LeapPointID))
					m_ID2LeapList.Add((uint)tracking.LeapPointID, new List<TrackingInfo>());

				m_ID2LeapList[(uint)tracking.LeapPointID].Add(tracking);
			}
			else if (tracking.Mode == TrackingMode.NpcAndPoint)
			{
				if (!m_ID2NpcList.ContainsKey((uint)tracking.NpcTID))
					m_ID2NpcList.Add((uint)tracking.NpcTID, new List<TrackingInfo>());

				m_ID2NpcList[(uint)tracking.NpcTID].Add(tracking);
			}
		}
	}

	/// <summary>
	/// 构建任务跟踪数据
	/// </summary>
	/// <param name="fromSceneID"></param>
	/// <param name="fromAreaID"></param>
	/// <param name="fromPosition"></param>
	private void RebuildTrackingList(uint fromSceneID, ulong fromAreaID, Vector3 fromPosition)
	{
		MissionProxy missionProxy = Facade.RetrieveProxy(ProxyName.MissionProxy) as MissionProxy;
		GameplayProxy gameplayProxy = GameFacade.Instance.RetrieveProxy(ProxyName.GameplayProxy) as GameplayProxy;
		BaseEntity mainPlayer = gameplayProxy.GetEntityById<BaseEntity>(gameplayProxy.GetMainPlayerUID());

		uint roleLv = mainPlayer?.GetLevel() ?? 1;

		//可接
		Dictionary<uint, MissionVO> canAcceptList = missionProxy.GetCanAcceptMissions();
		if (canAcceptList != null && canAcceptList.Count > 0)
		{
			foreach (KeyValuePair<uint, MissionVO> item in canAcceptList)
			{
				if (item.Value.MissionConfig.LvLimit <= roleLv)
				{
					TrackingInfo tracking = FindNpc(fromSceneID, fromAreaID, fromPosition, (uint)item.Value.MissionMainConfig.Value.AcceptNpcId);
					if (tracking != null)
					{
						tracking.MissionVO = item.Value;
						tracking.MissionTid = item.Value.Tid;
						tracking.MissionType = item.Value.MissionType;
						tracking.MissionState = item.Value.MissionState;
						m_TrackingList.Add(tracking);
					}
				}
			}
		}

		//主线
		Dictionary<ulong, MissionVO> mainList = missionProxy.AllMissionDic[MissionType.Main];
		if (mainList != null && mainList.Count > 0)
		{
			foreach (KeyValuePair<ulong, MissionVO> item in mainList)
			{
				CheckMissionTracking(fromSceneID, fromAreaID, fromPosition, item.Value);
			}
		}

		//支线
		Dictionary<ulong, MissionVO> branchList = missionProxy.AllMissionDic[MissionType.Branch];
		if (branchList != null && branchList.Count > 0)
		{
			foreach (KeyValuePair<ulong, MissionVO> item in branchList)
			{
				CheckMissionTracking(fromSceneID, fromAreaID, fromPosition, item.Value);
			}
		}
	}

	/// <summary>
	/// 检查任务 添加跟踪
	/// </summary>
	/// <param name="fromSceneID"></param>
	/// <param name="fromAreaID"></param>
	/// <param name="fromPosition"></param>
	/// <param name="missionVO"></param>
	private void CheckMissionTracking(uint fromSceneID, ulong fromAreaID, Vector3 fromPosition, MissionVO missionVO)
	{
		if (missionVO.MissionState == MissionState.Finished)
		{
			TrackingInfo tracking = FindNpc(fromSceneID, fromAreaID, fromPosition, (uint)missionVO.MissionMainConfig.Value.SubmitNpcId);
			if (tracking != null)
			{
				tracking.MissionVO = missionVO;
				tracking.MissionTid = missionVO.Tid;
				tracking.MissionType = missionVO.MissionType;
				tracking.MissionState = missionVO.MissionState;
				m_TrackingList.Add(tracking);
			}
		}
		else if (missionVO.MissionState != MissionState.Failed)
		{
			foreach (KeyValuePair<uint, SortedDictionary<uint, MissionTargetVO>> targetList in missionVO.MissionGroupTargetList)
			{
				foreach (KeyValuePair<uint, MissionTargetVO> target in targetList.Value)
				{
					if (target.Value.TargetState == MissionState.Going)
					{
						{
							TrackingInfo tracking = FindNpc(fromSceneID, fromAreaID, fromPosition, (uint)target.Value.MissionTargetConfig.GuideNPC);
							if (tracking != null)
							{
								tracking.MissionVO = missionVO;
								tracking.MissionTid = missionVO.Tid;
								tracking.MissionType = missionVO.MissionType;
								tracking.MissionState = target.Value.TargetState;
								m_TrackingList.Add(tracking);
							}
						}

						switch (target.Value.MissionTargetType)
						{
							case MissionTargetType.Kill:
							case MissionTargetType.Chat:
							case MissionTargetType.Search:
								{
									TrackingInfo tracking = FindNpc(fromSceneID, fromAreaID, fromPosition, (uint)target.Value.MissionTargetConfig.Arg1);
									if (tracking != null)
									{
										tracking.MissionVO = missionVO;
										tracking.MissionTid = missionVO.Tid;
										tracking.MissionType = missionVO.MissionType;
										tracking.MissionState = target.Value.TargetState;
										m_TrackingList.Add(tracking);
									}
									break;
								}
							case MissionTargetType.Escort:
								{
									//护送的npc
									TrackingInfo tracking1 = FindNpc(fromSceneID, fromAreaID, fromPosition, (uint)target.Value.MissionTargetConfig.Arg1);
									if (tracking1 != null)
									{
										tracking1.MissionVO = missionVO;
										tracking1.MissionTid = missionVO.Tid;
										tracking1.MissionType = missionVO.MissionType;
										tracking1.MissionState = target.Value.TargetState;
										tracking1.FarNotice = true;
										tracking1.FarNoticeDistance = (uint)target.Value.MissionTargetConfig.Arg3;
										m_TrackingList.Add(tracking1);
									}
									//目标地
									TrackingInfo tracking2 = FindNpc(fromSceneID, fromAreaID, fromPosition, (uint)target.Value.MissionTargetConfig.Arg2);
									if (tracking2 != null)
									{
										tracking2.MissionVO = missionVO;
										tracking2.MissionTid = missionVO.Tid;
										tracking2.MissionType = missionVO.MissionType;
										tracking2.MissionState = target.Value.TargetState;
										m_TrackingList.Add(tracking2);
									}
									break;
								}
						}
					}
				}
			}
		}
	}

	/// <summary>
	/// 重新最接近的目标
	/// </summary>
	private void UpdateNearTarget()
	{
		CfgEternityProxy cfgProxy = Facade.RetrieveProxy(ProxyName.CfgEternityProxy) as CfgEternityProxy;

		uint mapID = cfgProxy.GetCurrentMapId();
		uint areaID = (uint)MapManager.GetInstance().GetCurrentAreaUid();
		if (m_LastMapID != mapID || m_LastAreaID != areaID)
		{
			RebuildTracking();

			m_LastMapID = mapID;
			m_LastAreaID = areaID;
			m_TID2UID.Clear();
		}

		bool isInSpace = cfgProxy.IsSpace();
		GameplayProxy gameplayProxy = Facade.RetrieveProxy(ProxyName.GameplayProxy) as GameplayProxy;
		foreach (TrackingInfo tracking in m_TrackingList)
		{
			if (tracking.Mode == TrackingMode.NpcAndPoint && tracking.NpcArea == areaID)
			{
				uint tid = (uint)tracking.NpcTID;
				uint uid = m_TID2UID.ContainsKey(tid) ? m_TID2UID[tid] : 0;
				if (uid != 0)
				{
					if (isInSpace)
					{
						SpacecraftEntity ship = gameplayProxy.GetEntityById<SpacecraftEntity>(uid);
						if (ship == null || (ship.GetHeroType() == Assets.Scripts.Define.KHeroType.htMonster && ship.GetAttribute(AttributeName.kHP) <= 0))
							uid = 0;
					}
					else
					{
						HumanEntity human = gameplayProxy.GetEntityById<HumanEntity>(uid);
						if (human == null)
							uid = 0;
					}
				}

				if (uid == 0)
				{
					m_TID2UID.Remove(tid);

					float distanceMin = float.MaxValue;
					if (isInSpace)
					{
						SpacecraftEntity main = gameplayProxy.GetEntityById<SpacecraftEntity>(gameplayProxy.GetMainPlayerUID());
						foreach (SpacecraftEntity entity in gameplayProxy.GetEntities<SpacecraftEntity>())
						{
							if (entity.GetHeroType() == Assets.Scripts.Define.KHeroType.htPlayer)
								continue;
							if (entity.GetHeroType() == Assets.Scripts.Define.KHeroType.htMonster && entity.GetAttribute(AttributeName.kHP) <= 0)
								continue;
							if (entity.GetTemplateID() == tid)
							{
								float distance = Vector3.Distance(main.transform.position, entity.transform.position);
								if (distance < distanceMin)
								{
									uid = entity.GetUId();
									distanceMin = distance;
								}
							}
						}
					}
					else
					{
						HumanEntity main = gameplayProxy.GetEntityById<HumanEntity>(gameplayProxy.GetMainPlayerUID());
						foreach (HumanEntity entity in gameplayProxy.GetEntities<HumanEntity>())
						{
							if (entity.GetHeroType() == Assets.Scripts.Define.KHeroType.htPlayer)
								continue;
							if (entity.GetTemplateID() == tid)
							{
								float distance = Vector3.Distance(main.transform.position, entity.transform.position);
								if (distance < distanceMin)
								{
									uid = entity.GetUId();
									distanceMin = distance;
								}
							}
						}
					}

					if (uid != 0)
						m_TID2UID[tid] = uid;
				}

				tracking.NpcUID = uid;
			}
			else
			{
				tracking.NpcUID = 0;
			}
		}
	}

	#endregion

	#region 传送门信息

	/// <summary>
	/// 传送门信息
	/// </summary>
	private class Teleport
	{
		public int fromScene;
		public ulong fromArea;
		public uint fromNpc;
		public Vector3 fromPosition;

		public int toScene;
		public int toArea;
		public uint toNpc;
		public Vector3 toPosition;
	}

	/// <summary>
	/// 传送门路径
	/// </summary>
	private class TeleportPath
	{
		public TeleportPath prev;
		public Teleport teleport;
		public int length;
	}

	/// <summary>
	/// 初始化传送点信息
	/// </summary>
	private void InitTeleports()
	{
		if (m_SceneID2Teleports != null)
			return;
		else
			m_SceneID2Teleports = new Dictionary<int, List<Teleport>>();

		CfgEternityProxy cfg = Facade.RetrieveProxy(ProxyName.CfgEternityProxy) as CfgEternityProxy;

		int teleportCount = cfg.GetTeleportCount();
		for (int i = 0; i < teleportCount; i++)
		{
			Eternity.FlatBuffer.Teleport from = cfg.GetTeleportByIndex(i);
			Eternity.FlatBuffer.Location? fromLocation = cfg.GetLocationByKey(from.LocationId);
            if (!fromLocation.HasValue)
                continue;

			int targetCount = from.ChanelListLength;
			for (int j = 0; j < targetCount; j++)
			{
				Chanel target = from.ChanelList(j).Value;
				Eternity.FlatBuffer.Location? targetLocation = cfg.GetLocationByKey(target.LocationId);
                if (!targetLocation.HasValue)
                    continue;

				Teleport teleport = new Teleport()
				{
					fromScene = from.StartGamingMap,
					fromArea = from.StartGamingmapArea,
					fromNpc = fromLocation.Value.NpcTid,
					fromPosition = CreateVector3(fromLocation.Value.Position),

					toScene = target.EndGamingMap,
					toArea = target.EndGamingMapArea,
					toNpc = targetLocation.Value.NpcTid,
					toPosition = CreateVector3(targetLocation.Value.Position)
				};

				if (!m_SceneID2Teleports.ContainsKey(teleport.fromScene))
					m_SceneID2Teleports.Add(teleport.fromScene, new List<Teleport>());

				m_SceneID2Teleports[teleport.fromScene].Add(teleport);
			}
		}
	}

	/// <summary>
	/// 查找最合适的传送点
	/// </summary>
	/// <returns>传送点ID</returns>
	private TeleportPath FindTeleportPath(int from, int to)
	{
		InitTeleports();

		//查找路径最短的传送点
		if (m_SceneID2Teleports.ContainsKey(from) && m_SceneID2Teleports.ContainsKey(to))
		{
			HashSet<int> arrivedScenes = new HashSet<int>();
			List<TeleportPath> findingPath1 = new List<TeleportPath>();
			List<TeleportPath> findingPath2 = new List<TeleportPath>();

			foreach (Teleport teleport in m_SceneID2Teleports[from])
			{
				findingPath1.Add(new TeleportPath() { prev = null, teleport = teleport, length = 1 });
				if (teleport.toScene == to)
					return findingPath1[findingPath1.Count - 1];
			}

			while (findingPath1.Count > 0)
			{
				foreach (TeleportPath path in findingPath1)
				{
					int currentSceneID = path.teleport.toScene;
					if (m_SceneID2Teleports.ContainsKey(currentSceneID) && !arrivedScenes.Contains(currentSceneID))
					{
						arrivedScenes.Add(currentSceneID);

						foreach (Teleport teleport in m_SceneID2Teleports[currentSceneID])
						{
							findingPath2.Add(new TeleportPath() { prev = path, teleport = teleport, length = path.length + 1 });
							if (teleport.toScene == to)
								return findingPath2[findingPath2.Count - 1];
						}
					}
				}

				List<TeleportPath> tmp = findingPath1;
				findingPath1 = findingPath2;
				findingPath2 = tmp;
				findingPath2.Clear();
			}
		}

		return null;
	}

	#endregion

	#region 跃迁点信息

	/// <summary>
	/// 跃迁点信息
	/// </summary>
	public class Leap
	{
		public ulong fromArea;
		public ulong fromLeap;
		public Vector3 fromPosition;

		public ulong toArea;
		public ulong toLeap;
		public Vector3 toPosition;
	}

	public class LeapPath
	{
		public LeapPath prev;
		public Leap leap;
		public int length;
	}

	/// <summary>
	/// 初始化跃迁点信息
	/// </summary>
	/// <param name="sceneID"></param>
	private void InitLeapInfos(uint sceneID)
	{
		if (m_Scene2AreaLeapInfos == null)
			m_Scene2AreaLeapInfos = new Dictionary<ulong, Dictionary<ulong, List<Leap>>>();

		if (m_Scene2AreaLeapInfos.ContainsKey(sceneID))
			return;

		m_Scene2AreaLeapInfos.Add(sceneID, new Dictionary<ulong, List<Leap>>());

		Dictionary<ulong, List<Leap>> areaID2LeapInfos = m_Scene2AreaLeapInfos[sceneID];

		CfgEternityProxy cfg = Facade.RetrieveProxy(ProxyName.CfgEternityProxy) as CfgEternityProxy;

		Eternity.FlatBuffer.Map? scene = cfg.GetMapByKey(sceneID);
		if (scene.HasValue)
		{
			Dictionary<ulong, ulong> leapID2AreaID = new Dictionary<ulong, ulong>();
			Dictionary<ulong, Vector3> leapID2Position = new Dictionary<ulong, Vector3>();

			int areaCount = scene.Value.AreaListLength;

			//找出所有跃迁点对应的区域ID
			for (int i = 0; i < areaCount; i++)
			{
				Eternity.FlatBuffer.Area area = scene.Value.AreaList(i).Value;
				int leapCount = area.LeapListLength;
				for (int j = 0; j < leapCount; j++)
				{
					LeapItem leap = area.LeapList(j).Value;
					leapID2AreaID.Add(leap.LeapId, area.AreaId);
					leapID2Position.Add(leap.LeapId, CreateVector3(leap.Position));
				}
			}
			//建立区域连接表
			for (int i = 0; i < areaCount; i++)
			{
				Eternity.FlatBuffer.Area area = scene.Value.AreaList(i).Value;

				ulong fromArea = area.AreaId;

				int leapCount = area.LeapListLength;
				for (int j = 0; j < leapCount; j++)
				{
					LeapItem leapItem = area.LeapList(j).Value;

					int leapTargetCount = leapItem.VisibleLeapListLength;
					for (int k = 0; k < leapTargetCount; k++)
					{
						uint toID = leapItem.VisibleLeapList(k);
						if (leapID2AreaID.ContainsKey(toID))
						{
							Leap leap = new Leap()
							{
								fromArea = fromArea,
								fromLeap = leapItem.LeapId,
								fromPosition = CreateVector3(leapItem.Position),

								toArea = leapID2AreaID[toID],
								toLeap = toID,
								toPosition = leapID2Position[toID]
							};

							if (!areaID2LeapInfos.ContainsKey(fromArea))
								areaID2LeapInfos.Add(fromArea, new List<Leap>());

							areaID2LeapInfos[fromArea].Add(leap);
						}
					}
				}
			}
		}
	}

	/// <summary>
	/// 查找最短的跃迁路径
	/// </summary>
	/// <param name="sceneID">场景ID</param>
	/// <param name="fromArea">起始区域</param>
	/// <param name="toArea">目标区域</param>
	/// <returns>FindPath</returns>
	public LeapPath FindLeapPath(uint sceneID, ulong fromArea, ulong toArea)
	{
		InitLeapInfos(sceneID);

		Dictionary<ulong, List<Leap>> areaID2LeapInfos = m_Scene2AreaLeapInfos[sceneID];

		if (areaID2LeapInfos.ContainsKey(fromArea) && areaID2LeapInfos.ContainsKey(toArea))
		{
			HashSet<ulong> arrivedAreas = new HashSet<ulong>();
			List<LeapPath> findingPath1 = new List<LeapPath>();
			List<LeapPath> findingPath2 = new List<LeapPath>();

			foreach (Leap leap in areaID2LeapInfos[fromArea])
			{
				findingPath1.Add(new LeapPath() { prev = null, leap = leap, length = 1 });
				if (leap.toArea == toArea)
					return findingPath1[findingPath1.Count - 1];
			}

			while (findingPath1.Count > 0)
			{
				foreach (LeapPath path in findingPath1)
				{
					ulong currentAreaID = path.leap.toArea;
					if (areaID2LeapInfos.ContainsKey(currentAreaID) && !arrivedAreas.Contains(currentAreaID))
					{
						arrivedAreas.Add(currentAreaID);

						foreach (Leap leapInfo in areaID2LeapInfos[currentAreaID])
						{
							findingPath2.Add(new LeapPath() { prev = path, leap = leapInfo, length = path.length + 1 });
							if (leapInfo.toArea == toArea)
								return findingPath2[findingPath2.Count - 1];
						}
					}
				}

				List<LeapPath> tmp = findingPath1;
				findingPath1 = findingPath2;
				findingPath2 = tmp;
				findingPath2.Clear();
			}
		}

		return null;
	}

	#endregion

	#region NPC信息

	/// <summary>
	/// NPC信息
	/// </summary>
	private struct NpcInfo
	{
		public ulong tid;
		public uint mapID;
		public ulong areaID;
		public Vector3 position;
	}

	/// <summary>
	/// 初始化NPC信息
	/// </summary>
	private void InitNpcInfos()
	{
		if (m_NpcID2NpcInfos != null)
			return;

		m_NpcID2NpcInfos = new Dictionary<ulong, List<NpcInfo>>();

		CfgEternityProxy cfg = Facade.RetrieveProxy(ProxyName.CfgEternityProxy) as CfgEternityProxy;

		int count = cfg.GetMapCount();
		for (int i = 0; i < count; i++)
		{
			Eternity.FlatBuffer.Map map = cfg.GetMapByIndex(i).Value;

			uint mapID = map.GamingmapId;

			int length = map.AreaListLength;
			for (int j = 0; j < length; j++)
			{
				Eternity.FlatBuffer.Area area = map.AreaList(j).Value;

				ulong areaID = area.AreaId;

				Dictionary<uint, List<Vector3>> npcs = new Dictionary<uint, List<Vector3>>();

				int size = area.CreatureListLength;
				for (int k = 0; k < size; k++)
				{
					Eternity.FlatBuffer.Creature npc = area.CreatureList(k).Value;
					Vec3 npcPosition = npc.Position.Value;

					uint id = npc.TplId;
					Vector3 position = new Vector3((float)npcPosition.X, (float)npcPosition.Y, (float)npcPosition.Z);

					if (!npcs.ContainsKey(id))
						npcs.Add(id, new List<Vector3>());

					npcs[id].Add(new Vector3((float)npcPosition.X, (float)npcPosition.Y, (float)npcPosition.Z));
				}

				foreach (uint npcID in npcs.Keys)
				{
					List<Vector3> positions = npcs[npcID];
					Vector3 min = positions[0];
					Vector3 max = positions[0];
					for (int k = 1; k < positions.Count; k++)
					{
						min = Vector3.Min(min, positions[k]);
						max = Vector3.Max(max, positions[k]);
					}

					if (!m_NpcID2NpcInfos.ContainsKey(npcID))
						m_NpcID2NpcInfos.Add(npcID, new List<NpcInfo>());

					m_NpcID2NpcInfos[npcID].Add(new NpcInfo() { tid = npcID, mapID = mapID, areaID = areaID, position = ((max - min) / 2 + min) });
				}
			}
		}
	}

	public void AddNpcInfo(uint npcTid, Vector3 pos, uint mapId = 0, ulong areaId = 0)
	{
		CfgEternityProxy cfg = Facade.RetrieveProxy(ProxyName.CfgEternityProxy) as CfgEternityProxy;
		if (mapId == 0)
		{
			mapId = cfg.GetCurrentGamingMapId();
		}
		if (areaId == 0)
		{
			areaId = MapManager.GetInstance().GetCurrentAreaUid();
		}
		if (!m_NpcID2NpcInfos.ContainsKey(npcTid))
		{
			m_NpcID2NpcInfos.Add(npcTid, new List<NpcInfo>());
		}
		m_NpcID2NpcInfos[npcTid].Add(new NpcInfo() { tid = npcTid, mapID = mapId, areaID = areaId, position = pos });
	}

	public void RemoveNpcInfo(uint npcTid)
	{
		m_NpcID2NpcInfos.Remove(npcTid);
	}


	/// <summary>
	/// 查找最合适的NPC路径
	/// </summary>
	/// <param name="fromSceneID"></param>
	/// <param name="fromAreaID"></param>
	/// <param name="npcID"></param>
	private TrackingInfo FindNpc(uint fromSceneID, ulong fromAreaID, Vector3 fromPosition, uint npcID)
	{
		InitNpcInfos();

		if (m_NpcID2NpcInfos.ContainsKey(npcID))
		{
			List<NpcInfo> sameAreaNpcs = new List<NpcInfo>();
			List<NpcInfo> sameSceneNpcs = new List<NpcInfo>();
			List<NpcInfo> differentSceneNpcs = new List<NpcInfo>();

			List<NpcInfo> infos = m_NpcID2NpcInfos[npcID];
			foreach (NpcInfo info in infos)
			{
				if (info.mapID == fromSceneID && info.areaID == fromAreaID)
					sameAreaNpcs.Add(info);
				else if (info.mapID == fromSceneID)
					sameSceneNpcs.Add(info);
				else
					differentSceneNpcs.Add(info);
			}

			if (sameAreaNpcs.Count > 0)
			{
				int index = -1;
				float distance = float.MaxValue;
				for (int i = 0; i < sameAreaNpcs.Count; i++)
				{
					float currDistance = (sameAreaNpcs[i].position - fromPosition).sqrMagnitude;
					if (currDistance < distance)
					{
						index = i;
						distance = currDistance;
					}
				}
				if (index != -1)
				{
					//相同区域, NPC坐标
					return new TrackingInfo() { Mode = TrackingMode.NpcAndPoint, NpcArea = fromAreaID, NpcTID = npcID, Position = sameAreaNpcs[index].position, EndingLeapPointID = fromAreaID };
				}
			}
			else if (sameSceneNpcs.Count > 0)
			{
				List<LeapPath> paths = new List<LeapPath>();
				foreach (NpcInfo info in sameSceneNpcs)
				{
					paths.Add(FindLeapPath(fromSceneID, fromAreaID, info.areaID));
				}

				int index = -1;
				float distance = float.MaxValue;
				for (int i = 0; i < paths.Count; i++)
				{
					if (paths[i] != null && paths[i].length < distance)
					{
						index = i;
						distance = paths[i].length;
					}
				}

				if (index != -1)
				{
					LeapPath path = paths[index];
					while (path != null)
					{
						if (path.leap.fromArea == fromAreaID)
						{
							//不同区域，跃迁点ID
							return new TrackingInfo() { Mode = TrackingMode.LeapPoint, LeapPointID = path.leap.toLeap, Position = path.leap.toPosition, EndingLeapPointID = paths[0].leap.toArea };
						}
						path = path.prev;
					}
				}
			}
			else if (differentSceneNpcs.Count > 0)
			{
				List<TeleportPath> paths = new List<TeleportPath>();
				foreach (NpcInfo info in differentSceneNpcs)
				{
					paths.Add(FindTeleportPath((int)fromSceneID, (int)info.mapID));
				}

				int index = -1;
				float distance = float.MaxValue;
				for (int i = 0; i < paths.Count; i++)
				{
					if (paths[i] != null && paths[i].length < distance)
					{
						index = i;
						distance = paths[i].length;
					}
				}

				if (index != -1)
				{
					TeleportPath path = paths[index];
					while (path != null)
					{
						if (path.teleport.fromScene == fromSceneID)
						{
							if (path.teleport.fromArea == fromAreaID)
							{
								//与传送门同区域,传送门ID
								return new TrackingInfo() { Mode = TrackingMode.NpcAndPoint, NpcArea = fromAreaID, NpcTID = path.teleport.fromNpc, Position = path.teleport.fromPosition, EndingLeapPointID = (ulong)paths[0].teleport.fromArea };
							}
							else
							{
								LeapPath leapPath = FindLeapPath(fromSceneID, fromAreaID, path.teleport.fromArea);
								while (leapPath != null)
								{
									if (leapPath.leap.fromArea == fromAreaID)
									{
										//与传送门不同区域,跃迁点ID
										return new TrackingInfo() { Mode = TrackingMode.LeapPoint, LeapPointID = leapPath.leap.toLeap, Position = leapPath.leap.toPosition, EndingLeapPointID = paths[0].teleport.fromArea };
									}
									leapPath = leapPath.prev;
								}
							}
							break;
						}
						path = path.prev;
					}
				}
			}
		}

		return null;
	}

	#endregion

	#region 复活点距离

	/// <summary>
	/// 计算复活点距离
	/// </summary>
	/// <param name="relifeToHuman">是否以人形态复活</param>
	/// <returns>距离(米)</returns>
	public float MeasureRelifeDistance(bool relifeToHuman)
	{
		CfgEternityProxy cfg = Facade.RetrieveProxy(ProxyName.CfgEternityProxy) as CfgEternityProxy;
		GameplayProxy proxy = Facade.RetrieveProxy(ProxyName.GameplayProxy) as GameplayProxy;

		Transform transform = proxy.GetMainPlayerSkinTransform();
		if (!cfg.IsSpace())
		{
			HumanEntity humanEntity = proxy.GetEntityById<HumanEntity>(proxy.GetMainPlayerUID());
			if (humanEntity != null)
				transform = humanEntity.transform;
		}
		else
		{
			SpacecraftEntity spacecraftEntity = proxy.GetEntityById<SpacecraftEntity>(proxy.GetMainPlayerUID());
			if (spacecraftEntity != null)
				transform = spacecraftEntity.transform;
		}

		if (transform == null)
			return 0;

		//当前位置
		uint fromSceneID = cfg.GetCurrentGamingMapId();
		uint fromAreaID = (uint)MapManager.GetInstance().GetCurrentAreaUid();
		Vector3 fromPosition = proxy.ClientToServerAreaOffset(transform.position);

		//复活位置
		uint relifeSceneID = cfg.GetMapByKey(fromSceneID).Value.SpaceGamingMap;
		ulong relifeArea = 0;
		Vector3 relifePosition = Vector3.zero;

		if (!FindRelifePosition(relifeToHuman, fromSceneID, fromAreaID, out relifeSceneID, out relifeArea, out relifePosition))
			return 0;

		//寻路
		float distance = 0;

		if (fromSceneID != relifeSceneID)
		{
			//不同场景
			TeleportPath teleportPath = FindTeleportPath((int)fromSceneID, (int)relifeSceneID);
			TeleportPath path;

			//开始部分
			path = teleportPath;
			while (path != null)
			{
				if (path.prev == null)
				{
					float multi = cfg.GetMapByKey((uint)path.teleport.fromScene).Value.PathType == 2 ? GameConstant.METRE_PER_UNIT : 1.0f;

					if (path.teleport.fromArea != fromAreaID)
					{
						//不同区域
						LeapPath leapPath = FindLeapPath((uint)path.teleport.fromScene, fromAreaID, (uint)path.teleport.fromArea);
						while (leapPath != null)
						{
							if (leapPath.prev == null)
								distance += (leapPath.leap.toPosition - fromPosition).magnitude * multi;
							else
								distance += (leapPath.leap.toPosition - leapPath.leap.fromPosition).magnitude * multi;

							leapPath = leapPath.prev;
						}
					}
					else
					{
						//同区域
						distance += (path.teleport.fromPosition - fromPosition).magnitude * multi;
					}
				}
				path = path.prev;
			}

			//中间部分
			path = teleportPath;
			while (path != null && path.prev != null)
			{
				float multi = cfg.GetMapByKey((uint)path.teleport.fromScene).Value.PathType == 2 ? GameConstant.METRE_PER_UNIT : 1.0f;
				LeapPath leapPath = FindLeapPath((uint)path.teleport.fromScene, (uint)path.prev.teleport.toArea, (uint)path.teleport.fromArea);
				while (leapPath != null)
				{
					distance += (leapPath.leap.toPosition - leapPath.leap.fromPosition).magnitude * multi;
					leapPath = leapPath.prev;
				}
				path = path.prev;
			}

			//目标部分
			path = teleportPath;
			if (path != null)
			{
				float multi = cfg.GetMapByKey((uint)path.teleport.toScene).Value.PathType == 2 ? GameConstant.METRE_PER_UNIT : 1.0f;
				distance += (path.teleport.toPosition - fromPosition).magnitude * multi;
			}
		}
		else
		{
			//同场景
			float multi = cfg.GetMapByKey((uint)fromSceneID).Value.PathType == 2 ? GameConstant.METRE_PER_UNIT : 1.0f;

			if (relifeArea != fromAreaID)
			{
				//不同区域
				LeapPath leapPath = FindLeapPath(fromSceneID, fromAreaID, relifeArea);
				LeapPath path;

				path = leapPath;
				while (path != null)
				{
					if (path.prev == null)
						distance += (path.leap.toPosition - fromPosition).magnitude * multi;
					else
						distance += (path.leap.toPosition - path.leap.fromPosition).magnitude * multi;

					path = path.prev;
				}

				path = leapPath;
				if (path != null)
				{
					distance += (relifePosition - path.leap.toPosition).magnitude * multi;
				}
			}
			else
			{
				//同区域
				distance += (relifePosition - fromPosition).magnitude * multi;
			}
		}

		return distance;
	}

	/// <summary>
	/// 查找复活信置
	/// </summary>
	/// <param name="humanRelife"></param>
	/// <param name="fromSceneID"></param>
	/// <param name="fromAreaID"></param>
	/// <param name="relifeScene"></param>
	/// <param name="relifeArea"></param>
	/// <param name="relifePosition"></param>
	/// <returns></returns>
	private bool FindRelifePosition(bool relifeToHuman, uint fromSceneID, uint fromAreaID, out uint relifeScene, out ulong relifeArea, out Vector3 relifePosition)
	{
		CfgEternityProxy cfg = Facade.RetrieveProxy(ProxyName.CfgEternityProxy) as CfgEternityProxy;

		if (relifeToHuman)
		{
			uint toSceneID = cfg.GetMapByKey(fromSceneID).Value.SpaceGamingMap;

			Eternity.FlatBuffer.Map? scene = cfg.GetMapByKey(toSceneID).Value;
			if (scene.HasValue)
			{
				int areaLength = scene.Value.AreaListLength;
				for (int i = 0; i < areaLength; i++)
				{
					Eternity.FlatBuffer.Area? area = scene.Value.AreaList(i);
					ulong npcID = area.Value.RelieveCreature;
					if (npcID != 0)
					{
						int npcLength = area.Value.CreatureListLength;
						for (int j = 0; j < npcLength; j++)
						{
							Eternity.FlatBuffer.Creature npc = area.Value.CreatureList(j).Value;
							if (npc.CreatureId == npcID)
							{
								relifeScene = toSceneID;
								relifeArea = area.Value.AreaId;
								relifePosition = CreateVector3(npc.Position);
								return true;
							}
						}
					}
				}
			}
			Debug.LogError("未找到人形态复活点" + fromSceneID + "," + fromAreaID);
		}
		else
		{
			Eternity.FlatBuffer.Map? scene = cfg.GetMapByKey(fromSceneID);

			ulong targetAreaID = 0;

			Eternity.FlatBuffer.Area? area = scene.Value.AreaListByKey(fromAreaID);
			if (area.HasValue)
			{
				if (area.Value.ChildrenAreaListLength > 0)
				{
					targetAreaID = fromAreaID;
				}
				else
				{
					if (area.Value.FatherArea > 0)
					{
						targetAreaID = (uint)area.Value.FatherArea;
					}
					else
					{
						int areaLength = scene.Value.AreaListLength;
						for (int i = 0; i < areaLength; i++)
						{
							if (scene.Value.AreaList(i).Value.AreaType == 2)
							{
								targetAreaID = scene.Value.AreaList(i).Value.AreaId;
								break;
							}
						}
					}
				}
			}

			if (targetAreaID != 0)
			{
				Eternity.FlatBuffer.Area? rootArea = scene.Value.AreaListByKey(targetAreaID);
				if (rootArea.HasValue)
				{
					ulong npcID = rootArea.Value.RelieveCreature;
					if (npcID != 0)
					{
						int npcLength = rootArea.Value.CreatureListLength;
						for (int j = 0; j < npcLength; j++)
						{
							Eternity.FlatBuffer.Creature npc = rootArea.Value.CreatureList(j).Value;
							if (npc.CreatureId == npcID)
							{
								relifeScene = fromSceneID;
								relifeArea = rootArea.Value.AreaId;
								relifePosition = CreateVector3(npc.Position);
								return true;
							}
						}
					}
				}
			}

			CfgStarmapProxy starmapProx = GameFacade.Instance.RetrieveProxy(ProxyName.CfgStarmapProxy) as CfgStarmapProxy;
			EditorFixedStar editorFixedStar = starmapProx.GetFixedStarByTid(cfg.GetCurrentMapData().BelongFixedStar);
			if (editorFixedStar != null)
			{
				relifeScene = editorFixedStar.ttGamingMapId;
				relifeArea = editorFixedStar.ttGamingAreaId;

				scene = cfg.GetMapByKey(relifeScene).Value;
				if (scene.HasValue)
				{
					Eternity.FlatBuffer.Area? rootArea = scene.Value.AreaListByKey(relifeArea);
					if (rootArea.HasValue)
					{
						ulong npcID = rootArea.Value.RelieveCreature;
						if (npcID != 0)
						{
							int npcLength = rootArea.Value.CreatureListLength;
							for (int j = 0; j < npcLength; j++)
							{
								Eternity.FlatBuffer.Creature npc = rootArea.Value.CreatureList(j).Value;
								if (npc.CreatureId == npcID)
								{
									relifePosition = CreateVector3(npc.Position);
									return true;
								}
							}
						}
					}
				}
			}

			Debug.LogError("未找到船形态复活点" + fromSceneID + "," + fromAreaID);
		}

		relifeScene = 0;
		relifeArea = 0;
		relifePosition = Vector3.zero;
		return false;
	}

	/// <summary>
	/// 创建Vector3
	/// </summary>
	/// <param name="v3"></param>
	/// <returns></returns>
	private Vector3 CreateVector3(Vec3? v3)
	{
		return new Vector3((float)v3.Value.X, (float)v3.Value.Y, (float)v3.Value.Z);
	}

	#endregion
}
