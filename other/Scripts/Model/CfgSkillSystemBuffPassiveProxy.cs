using Eternity.FlatBuffer;
using FlatBuffers;
using PureMVC.Patterns.Proxy;
using UnityEngine;

public partial class CfgEternityProxy : Proxy
{
	public SkillBuff GetBuff(int bufID)
	{
#if UNITY_EDITOR
		if (!m_Config.SkillBuffsByKey((uint)bufID).HasValue)
			Debug.LogErrorFormat("找不到SkillBuffID为 {0} 的技能", bufID);
#endif
		return m_Config.SkillBuffsByKey((uint)bufID).Value;
	}
}