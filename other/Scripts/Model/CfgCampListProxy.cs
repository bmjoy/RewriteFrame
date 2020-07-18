using Assets.Scripts.Define;
using Eternity.FlatBuffer;
using PureMVC.Patterns.Proxy;
using UnityEngine.Assertions;

public partial class CfgEternityProxy : Proxy
{

	public int GetCampCount()
	{
		return m_Config.CampListsLength;
	}

	public CampList GetCampVOByKey(uint tid)
	{
		CampList? campList = m_Config.CampListsByKey(tid);
		Assert.IsTrue(campList.HasValue, "CfgCampListProxy => GetCampVOByKey not exist tid " + tid);
		return campList.Value;
	}

	public CampList GetCampVOByIndex(int index)
	{
		CampList? campList = m_Config.CampLists(index);
		Assert.IsTrue(campList.HasValue, "CfgCampListProxy => GetCampVOByIndex not exist index " + index);
		return campList.Value;
	}

	/// <summary>
	/// 获取阵营关系
	/// </summary>
	/// <param name="selfCampType">自已的阵营ID</param>
	/// <param name="targetCampType">对方的阵营ID</param>
	/// <returns></returns>
	public CampNode? GetCampRelationVO(uint selfCampType, uint targetCampType)
	{
		CampList campVO = GetCampVOByKey(selfCampType);
		int count = campVO.NodeCampListLength;
		for (int i = 0; i < count; i++)
		{
			CampNode? mergeObject = campVO.NodeCampList(i);
			if (mergeObject.HasValue && mergeObject.Value.CampOther == targetCampType)
			{
				return mergeObject.Value;
			}
		}
		return null;
	}

	/// <summary>
	/// 获取两个阵营之间的初始声望
	/// </summary>
	/// <param name="selfCampType">自已的阵营ID</param>
	/// <param name="targetCampType">对方的阵营ID</param>
	/// <returns>声望值</returns>
	public uint GetPrestige(uint selfCampType, uint targetCampType)
	{
		CampNode? mergeObject = GetCampRelationVO(selfCampType, targetCampType);
		if (mergeObject.HasValue)
		{
			return mergeObject.Value.PrestigeInit;
		}
		return 0;
	}

	/// <summary>
	/// 如果出现未定义的阵营组合, 则返回中立
	/// </summary>
	/// <param name="selfUID">自已的阵营ID</param>
	/// <param name="targetUID">对方的阵营ID</param>
	/// <returns>阵营关系</returns>
	public CampRelation GetRelation(uint selfCampType, uint targetCampType)
	{
		CampNode? mergeObject = GetCampRelationVO(selfCampType, targetCampType);
		if (!mergeObject.HasValue)
		{
			UnityEngine.Debug.LogErrorFormat("阵营信息不存在. Camp1: {0}, Camp2: {1}", selfCampType, targetCampType);
			return CampRelation.Undefined;
		}
		else
		{
			uint prestage = GetPrestige(selfCampType, targetCampType);
			if (prestage > mergeObject.Value.PrestigeEnemyMin && prestage < mergeObject.Value.PrestigeEnemyMax)
			{
				return CampRelation.Enemy;
			}
			else if (prestage > mergeObject.Value.PrestigeNeutralMin && prestage < mergeObject.Value.PrestigeNeutralMax)
			{
				return CampRelation.Neutrality;
			}
			else if (prestage > mergeObject.Value.PrestigeFriendMin && prestage < mergeObject.Value.PrestigeFriendMax)
			{
				return CampRelation.Friend;
			}
			else
			{
				// Debug.LogErrorFormat("Undefined camp type! Entity1 UID: {0}, Entity2 UID: {1}, Entity1 Camp: {2}, Entity2 Camp: {3}", selfUID, targetUID, selfHeroVO.Properties.CampID, targetHeroVO.Properties.CampID);
				return CampRelation.Undefined;
			}
		}
	}

}