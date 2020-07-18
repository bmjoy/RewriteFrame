using Eternity.FlatBuffer;
using PureMVC.Patterns.Proxy;
using System.Collections.Generic;
using UnityEngine.Assertions;

public partial class CfgEternityProxy : Proxy
{
	/// <summary>
	/// 任务序列
	/// </summary>
	private Dictionary<uint, List<uint>> m_SortedMissions;

	/// <summary>
	/// get mission data
	/// </summary>
	/// <param name="tid"></param>
	/// <returns></returns>
	public MissionData GetMissionByKey(uint tid)
	{
		MissionData? data = m_Config.MissionDatasByKey(tid);
		Assert.IsTrue(data.HasValue, "任务 " + tid + " 数据不存在");
		return data.Value;
	}

	/// <summary>
	/// get missiontTarget data
	/// </summary>
	/// <param name="tid"></param>
	/// <returns></returns>
	public MissionTarget GetMissionTargetByKey(uint tid)
	{
		MissionTarget? data = m_Config.MissionTargetsByKey(tid);
		Assert.IsTrue(data.HasValue, "任务目标 " + tid + " 数据不存在");
		return data.Value;
	}

	/// <summary>
	/// 任务排序
	/// 调整方便索引的树形结构
	/// </summary>
	public void SortMissionList()
	{
		if (m_SortedMissions != null)
		{
			return;
		}
		m_SortedMissions = new Dictionary<uint, List<uint>>();

		int missionCount = m_Config.MissionDatasLength;
		for (int i = 0; i < missionCount; i++)
		{
			MissionData mission = m_Config.MissionDatas(i).Value;
			if (mission.IsRoot == 1)
			{
				if (!m_SortedMissions.TryGetValue((uint)mission.PreID, out List<uint> missionList))
				{
					missionList = new List<uint>();
					m_SortedMissions.Add((uint)mission.PreID, missionList);
				}
				missionList.Add(mission.Id);
			}
		}
	}

	/// <summary>
	/// 根据任务id 获取后续任务（子，不含孙）列表
	/// </summary>
	/// <param name="tid"></param>
	/// <returns></returns>
	public List<uint> GetNextMissionsBy(uint tid)
	{
		m_SortedMissions.TryGetValue(tid, out List<uint> value);
		return value;
	}

	/// <summary>
	/// get mission_main data
	/// </summary>
	/// <param name="tid"></param>
	/// <returns></returns>
	public MissionMain GetMissionMainByKey(uint tid)
	{
		MissionMain? data = m_Config.MissionMainsByKey(tid);
		Assert.IsTrue(data.HasValue, "CfgEternityProxy => MissionMain not exist tid " + tid);
		return data.Value;
	}

	/// <summary>
	/// 获取任务首节点数据
	/// </summary>
	/// <param name="tid"></param>
	/// <returns></returns>
	public MissionTreeNode GetMissionFirstNodeByTid(uint tid)
	{
		MissionTreeNode? data = m_Config.MissionTreeNodesByKey(tid);
		Assert.IsTrue(data.HasValue, "任务首节点 " + tid + " 数据不存在");
		return data.Value;
	}

	/// <summary>
	/// 获取任务组数据
	/// </summary>
	/// <param name="tid"></param>
	/// <returns></returns>
	public MissionNodeGroup GetMissionNodeGroup(uint tid)
	{
		MissionNodeGroup? data = m_Config.MissionNodeGroupsByKey(tid);
		Assert.IsTrue(data.HasValue, "任务组 " + tid + " 数据不存在");
		return data.Value;
	}

	/// <summary>
	/// 根据任务组id 获取视讯id
	/// </summary>
	/// <param name="groupTid"></param>
	/// <returns></returns>
	public int GetVideoPhoneTidByMissionGroupTid(uint groupTid)
	{
		VideoPhoneMissionGroup? value = m_Config.VideoPhoneMissionGroupsByKey(groupTid);
		if (value.HasValue)
		{
			return value.Value.Videophone;
		}
		return 0;
	}

	/// <summary>
	/// 获取TaskICON
	/// </summary>
	public MissionIconID GetMissionIconIdBy(MissionType missionType)
	{
		switch (missionType)
		{
			case MissionType.Main:
				return MissionIconID.MajorIconId;
			case MissionType.Branch:
				return MissionIconID.ExtensionIconId;
			case MissionType.Daily:
				return MissionIconID.DayDailyIconId;
			case MissionType.World:
				return MissionIconID.WorldTaskIconId;
			case MissionType.Dungeon:
				return MissionIconID.DuplicateTaskIconId;
			default:
				return MissionIconID.Error;
		}
	}

}