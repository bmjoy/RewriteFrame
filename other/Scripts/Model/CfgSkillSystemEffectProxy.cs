using Eternity.FlatBuffer;
using FlatBuffers;
using PureMVC.Patterns.Proxy;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public partial class CfgEternityProxy : Proxy
{
	public SkillSystemEffect GetSkillEffect(int effectID)
	{
#if UNITY_EDITOR
		if (!m_Config.SkillSystemEffectsByKey((uint)effectID).HasValue)
			Debug.LogErrorFormat("找不到SkillEffectID为 {0} 的技能", effectID);
#endif
		return m_Config.SkillSystemEffectsByKey((uint)effectID).GetValueOrDefault();
	}
}