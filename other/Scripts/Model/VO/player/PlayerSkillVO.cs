using Eternity.FlatBuffer;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSkillVO
{
	private int m_TemplateID;
	/// <summary>
	/// 快捷方式, 技能数据表信息
	/// </summary>
	private CfgSkillSystemProxy m_CfgSkillProxy;
	/// <summary>
	/// 快捷方式, 技能信息
	/// </summary>
	private SkillSystemGrow skillGrow;
	/// <summary>
	/// 快捷方式, 技能投射物表现信息
	/// </summary>
	private SkillSystemPath m_SkillPath;
	/// <summary>
	/// 快捷方式, 技能的命中效果信息
	/// </summary>
	private List<SkillSystemEffect> m_SkillEffectList;
	/// <summary>
	/// 快捷方式, 技能的特效信息
	/// </summary>
	private List<SkillSystemFx> m_SkillFxList;

	/// <summary>
	/// 技能结束CD的时间, 使用 Time.time
	/// </summary>
	private float m_CDEndTime;

	/// <summary>
	/// Trgger技能结束TriggerCD的时间, 使用 Time.time
	/// </summary>
	private float m_TriggerCDEndTime;

	/// <summary>
	/// 技能结束引导的时间, 使用 Time.time
	/// </summary>
	private float m_GuideEndTime;

	private bool m_IsValid;

	// FIXME 技能. 预计SkillVO这里做一个池子. 消除GC
	//static PlayerSkillVO s_SkillVOTemplate;
	static public PlayerSkillVO CreateSkillVO(int skillID)
	{
		CfgSkillSystemProxy cfgSkillProxy = GameFacade.Instance.RetrieveProxy(ProxyName.CfgSkillSystemProxy) as CfgSkillSystemProxy;

		SkillSystemGrow skillGrow = cfgSkillProxy.GetSkillGrow(skillID);
		if (skillGrow.ByteBuffer == null)
		{
			Debug.LogErrorFormat("找不到SkillGrow. SkillID: {0}", skillID);
			return null;
		}

		SkillSystemPath skillPath = cfgSkillProxy.GetSkillPath(skillGrow.PathID);
		if (skillPath.ByteBuffer == null)
		{
			Debug.LogErrorFormat("找不到SkillPath. PathID: {0}", skillGrow.PathID);
			return null;
		}

		List<SkillSystemFx>  skillFxList = cfgSkillProxy.GetSkillFxList(skillGrow.PathID);
		for (int iEffect = 0; iEffect < skillFxList.Count; iEffect++)
		{
			if (skillFxList[iEffect].ByteBuffer == null)
			{
				Debug.LogErrorFormat("找不到skillFxList. PathID: {0}", skillGrow.PathID);
				return null;
			}
		}

		List<SkillSystemEffect>  skillEffectList = cfgSkillProxy.GetSkillEffectList(skillID);
		for (int iEffect = 0; iEffect < skillFxList.Count; iEffect++)
		{
			if (skillEffectList[iEffect].ByteBuffer == null)
			{
				Debug.LogErrorFormat("找不到skillEffectList. skillID: {0}", skillID);
				return null;
			}
		}

		return new PlayerSkillVO(skillID);
	}

	private PlayerSkillVO()
	{
	}

	private PlayerSkillVO(int id)
	{
		m_CfgSkillProxy = GameFacade.Instance.RetrieveProxy(ProxyName.CfgSkillSystemProxy) as CfgSkillSystemProxy;

		m_TemplateID = id;
		skillGrow = m_CfgSkillProxy.GetSkillGrow(m_TemplateID);
		if (skillGrow.ByteBuffer != null)
		{
			m_SkillPath = m_CfgSkillProxy.GetSkillPath(skillGrow.PathID);
			m_SkillFxList = m_CfgSkillProxy.GetSkillFxList(skillGrow.PathID);
			m_SkillEffectList = m_CfgSkillProxy.GetSkillEffectList(m_TemplateID);
			m_CDEndTime = Time.time;
			m_TriggerCDEndTime = Time.time;

			m_IsValid = true;
		}
		else
		{
			m_IsValid = false;
			Debug.LogErrorFormat("技能不存在, 请查Skill_system_grow表. ID: {0}", id);
		}
	}

	/// <summary>
	/// 技能的类型ID, 即表里的ID
	/// </summary>
	public int GetID()
	{
		return m_TemplateID;
	}

	public bool IsValid()
	{
		return m_IsValid;
	}

	public SkillSystemGrow GetSkillGrow()
	{
		return skillGrow;
	}

	public SkillSystemPath GetSkillPath()
	{
		return m_SkillPath;
	}

	public List<SkillSystemFx> GetSkillFxList()
	{
		return m_SkillFxList;
	}

	public List<SkillSystemEffect> GetSkillEffectList()
	{
		return m_SkillEffectList;
	}

	/// <summary>
	/// 名称
	/// </summary>
	public string GetSkillName()
	{
		return m_CfgSkillProxy.GetSkillName(m_TemplateID);
	}

	/// <summary>
	/// 描述
	/// </summary>
	public string GetSkillDesc()
	{
		return m_CfgSkillProxy.GetSkillDesc(m_TemplateID);
	}

	/// <summary>
	/// 开始引导
	/// </summary>
	public void StartChannelling()
	{
#if UNITY_EDITOR
		if (skillGrow.CastWay == (int)SkillCategory.AutoChannel 
			|| skillGrow.CastWay == (int)SkillCategory.ManualChannel
			|| skillGrow.CastWay == (int)SkillCategory.RapidFire
			|| skillGrow.CastWay == (int)SkillCategory.MiningBeam)
#endif
			m_GuideEndTime = Time.time + skillGrow.CastWayArgs(1);
	}

	/// <summary>
	/// 开始冷却
	/// 不应该直接调用这个接口, 应该调用PlayerSkillProxy.StartCD
	/// </summary>
	public void StartCD()
	{
		m_CDEndTime = Time.time + skillGrow.Cooldown / 1000f;
	}

	/// <summary>
	/// 是否正在CD
	/// 不应该直接调用这个接口, 应该调用PlayerSkillProxy.GetRemainingCD
	/// </summary>
	public float GetRemainingCD()
	{
		return Mathf.Clamp(m_CDEndTime - Time.time, 0, 99999f);
	}

	public void StartTriggerCD()
	{
		float triggerCD = 0;
		if (m_CfgSkillProxy.IsTriggerSkill(m_TemplateID))
		{
			if (skillGrow.CastWay == (int)SkillCategory.Immediate_Trigger)
				triggerCD = skillGrow.CastWayArgs(0);
			else if (skillGrow.CastWay == (int)SkillCategory.Chanting_Trigger)
				triggerCD = skillGrow.CastWayArgs(1);
		}
		m_TriggerCDEndTime = Time.time + triggerCD / 1000f;
	}

	public bool IsTriggerCDFinished()
	{
		return m_TriggerCDEndTime <= Time.time;
	}
}
