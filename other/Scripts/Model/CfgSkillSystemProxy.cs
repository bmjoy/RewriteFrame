using Eternity.FlatBuffer;
using PureMVC.Patterns.Proxy;
using System.Collections.Generic;

public class CfgSkillSystemProxy : Proxy
{
	#region Private变量
	public CfgEternityProxy m_CfgEternityProxy;
	#endregion

	#region Public函数

	public CfgSkillSystemProxy() : base(ProxyName.CfgSkillSystemProxy)
	{
		m_CfgEternityProxy = GameFacade.Instance.RetrieveProxy(ProxyName.CfgEternityProxy) as CfgEternityProxy;
	}

	public SkillSystemGrow GetSkillGrow(int skillID)
	{
		return m_CfgEternityProxy.GetSkillGrow(skillID);
	}

	public SkillSystemPath GetSkillPath(int skillPathID)
	{
		return m_CfgEternityProxy.GetSkillPath(skillPathID);
	}

	public List<SkillSystemEffect> GetSkillEffectList(int skillID)
	{
		List<SkillSystemEffect> skillEffectList = new List<SkillSystemEffect>();
		SkillSystemGrow skillGrow = GetSkillGrow(skillID);
		if (skillGrow.ByteBuffer == null)
			return null;

		for (int iEffect = 0; iEffect < skillGrow.GetEffectIdArray().Length; iEffect++)
		{
			SkillSystemEffect skillEffect = m_CfgEternityProxy.GetSkillEffect(skillGrow.GetEffectIdArray()[iEffect]);
			if (skillEffect.ByteBuffer == null)
				continue;

			skillEffectList.Add(skillEffect);
		}

		return skillEffectList;
	}

	public List<SkillSystemFx> GetSkillFxList(int skillPathID)
	{
		List<SkillSystemFx> skillFxList = new List<SkillSystemFx>();
		SkillSystemPath skillPath = GetSkillPath(skillPathID);
		if (skillPath.ByteBuffer == null)
			return null;

		for (int iFx = 0; iFx < skillPath.FxIdLength; iFx++)
		{
			SkillSystemFx skillFx = m_CfgEternityProxy.GetSkillFx(skillPath.GetFxIdArray()[iFx]);
			if (skillFx.ByteBuffer == null)
				continue;

			skillFxList.Add(skillFx);
		}

		return skillFxList;
	}

	public SkillSystemFx GetSkillFx(int skillFxID)
	{
		return m_CfgEternityProxy.GetSkillFx(skillFxID);
	}

	public SkillBuff GetBuff(int buffID)
	{
		return m_CfgEternityProxy.GetBuff(buffID);
	}

	/// <summary>
	/// 获取技能名称
	/// </summary>
	public string GetSkillName(int id)
	{
		return TableUtil.GetLanguageString(m_CfgEternityProxy.GetIndexStringOfSkillName(id));
	}

	/// <summary>
	/// 获取技能描述
	/// </summary>
	public string GetSkillDesc(int id)
	{
		return TableUtil.GetLanguageString(m_CfgEternityProxy.GetIndexStringOfSkillDescription(id));
	}

	/// <summary>
	/// 是不是武器技能
	/// 决定技能收不收公共CD影响
	/// </summary>
	public bool IsWeaponSkill(int skillID)
	{
		return GetSkillGrow(skillID).SkillType == (int)SkillType.WeaponSkill;
	}
	/// <summary>
	/// 技能是不是立即释放的
	/// </summary>
	public bool SkillReleaseImmediatly(int skillID)
	{
		SkillSystemGrow grow = GetSkillGrow(skillID);
		return grow.CastWay == (int)SkillCategory.Immediate || grow.CastWay == (int)SkillCategory.Immediate_MultiShot
				|| grow.CastWay == (int)SkillCategory.Immediate_Trigger;
	}

	/// <summary>
	/// 技能是不是需要吟唱的
	/// </summary>
	public bool SkillNeedChanting(int skillID)
	{
		SkillSystemGrow grow = GetSkillGrow(skillID);
		return grow.CastWay == (int)SkillCategory.Chanting || grow.CastWay == (int)SkillCategory.Chanting_Trigger;
	}

	/// <summary>
	/// 技能是不是需要蓄力的
	/// </summary>
	public bool SkillNeedCharging(int skillID)
	{
		SkillSystemGrow grow = GetSkillGrow(skillID);
		return grow.CastWay == (int)SkillCategory.Charge || grow.CastWay == (int)SkillCategory.Charge_MultiShot;
	}

	/// <summary>
	/// 技能是不是引导的
	/// </summary>
	public bool SkillNeedChannelling(int skillID)
	{
		SkillSystemGrow grow = GetSkillGrow(skillID);
		return grow.CastWay == (int)SkillCategory.ManualChannel 
			|| grow.CastWay == (int)SkillCategory.AutoChannel 
			|| grow.CastWay == (int)SkillCategory.RapidFire 
			|| grow.CastWay == (int)SkillCategory.MiningBeam;
	}

	/// <summary>
	/// 技能是不是多轮的
	/// </summary>
	public bool SkillHasMultiShot(int skillID)
	{
		SkillSystemGrow grow = GetSkillGrow(skillID);
		return grow.CastWay == (int)SkillCategory.Charge_MultiShot || grow.CastWay == (int)SkillCategory.Immediate_MultiShot;
	}

	/// <summary>
	/// 技能是不是有Trigger机制
	/// Trigger: 按住技能按键持续释放
	/// </summary>
	public bool IsTriggerSkill(int skillID)
	{
		SkillSystemGrow grow = GetSkillGrow(skillID);
		return grow.CastWay == (int)SkillCategory.Immediate_Trigger || grow.CastWay == (int)SkillCategory.Chanting_Trigger;
	}

	/// <summary>
	/// 光束引导技能是一种特化的引导技能, 只释放一次技能实体, 而不是每次间隔都释放, 引导间隔只是服务器用了计算伤害
	/// </summary>
	/// <param name="skillID"></param>
	/// <returns></returns>
	public bool IsChannellingBeamSkill(int skillID)
	{
		SkillSystemGrow grow = GetSkillGrow(skillID);
		SkillSystemPath path = GetSkillPath(grow.PathID);

		return SkillNeedChannelling(skillID) && path.PathType == (int)SkillPathType.LightBeam_DestroyByEvent;
	}

	/// <summary>
	/// 技能是不是需要一个目标才能释放
	/// </summary>
	public bool SkillNeedTarget(int skillID)
	{
		SkillSystemGrow grow = GetSkillGrow(skillID);
		return grow.CastWay == (int)SkillCategory.ManualChannel 
			|| grow.CastWay == (int)SkillCategory.AutoChannel
			|| grow.CastWay == (int)SkillCategory.MiningBeam;
	}

	/// <summary>
	/// 技能支持锁定多个目标
	/// </summary>
	/// <param name="skillID"></param>
	/// <returns></returns>
	public bool SkillSupportMultiTargets(int skillID)
	{
		SkillSystemGrow grow = GetSkillGrow(skillID);
		return grow.CastWay == (int)SkillCategory.Immediate_MultiTarget;
	}

	/// <summary>
	/// 弹道类型是锁定目标的
	/// </summary>
	public bool SkillPathCanLockTarget(SkillPathType pathType)
	{
		switch (pathType)
		{
			case SkillPathType.Acceleration_Lock:
			case SkillPathType.AccelerationWithInitialVelocity_Lock:
			case SkillPathType.TwoStageAcceleration_Lock:
				return true;
			default:
				return false;
		}
	}

	/// <summary>
	/// 技能释放间隔, 单位: 毫秒
	/// </summary>
	/// <param name="skillID"></param>
	/// <returns></returns>
	public float GetSkillReleaseInterval(int skillID)
	{
		SkillSystemGrow grow = GetSkillGrow(skillID);
		SkillCategory category = (SkillCategory)grow.CastWay;
		switch (category)
		{
			case SkillCategory.Immediate:
			case SkillCategory.Immediate_MultiShot:
			case SkillCategory.Chanting:
			case SkillCategory.Charge:
			case SkillCategory.Charge_MultiShot:
				return grow.Cooldown;
			case SkillCategory.Chanting_Trigger:
				return grow.CastWayArgs(1);
			case SkillCategory.Immediate_Trigger: 
				return grow.CastWayArgs(0);
			case SkillCategory.ManualChannel:
			case SkillCategory.AutoChannel:
			case SkillCategory.RapidFire:
			case SkillCategory.MiningBeam:
				return grow.CastWayArgs(0);
			default:
				return 1000f;
		}
	}

	/// <summary>
	/// 判断特效是否作为特效发射点的子节点创建
	/// </summary>
	public bool IfFxAttachedToParent(SkillFxType fxType, SkillSystemFx skillFx)
	{
		switch (fxType)
		{
			case SkillFxType.Launch:
				return skillFx.LauncherFxType == 1;
			case SkillFxType.Hit:
				return skillFx.HitFxType == 1;
			case SkillFxType.PeriodicBegin:
			case SkillFxType.PeriodicLoop:
			case SkillFxType.PeriodicEnd:
				return true;

			default:
				return false;
		}
	}
	#endregion

	#region 虚函数

	public bool HasNotification(string msg)
	{
		throw new System.NotImplementedException();
	}

	public void RegistNotifications()
	{
		m_CfgEternityProxy = GameFacade.Instance.RetrieveProxy(ProxyName.CfgEternityProxy) as CfgEternityProxy;
	}

	public void UnregistNotifications()
	{
	}
	#endregion
}