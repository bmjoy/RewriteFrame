using Eternity.FlatBuffer;
using FlatBuffers;
using PureMVC.Patterns.Proxy;
using System.Collections.Generic;
using UnityEngine;

public partial class CfgEternityProxy : Proxy
{
	public SkillSystemGrow GetSkillGrow(int skillGrowID)
	{
#if UNITY_EDITOR
		if (!m_Config.SkillSystemGrowsByKey((uint)skillGrowID).HasValue)
			Debug.LogErrorFormat("找不到SkillGrowID为 {0} 的技能", skillGrowID);
#endif
		return m_Config.SkillSystemGrowsByKey((uint)skillGrowID).Value;
	}

	public string GetIndexStringOfSkillName(int skillID)
	{
		return string.Format("skill_system_grow_nameId_{0}", skillID);
	}

	public string GetIndexStringOfSkillDescription(int skillID)
	{
		return string.Format("skill_system_grow_desc_{0}", skillID);
	}
}