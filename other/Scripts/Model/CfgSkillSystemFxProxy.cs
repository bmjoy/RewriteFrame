using Eternity.FlatBuffer;
using FlatBuffers;
using PureMVC.Patterns.Proxy;
using UnityEngine;

public partial class CfgEternityProxy : Proxy
{
	public SkillSystemFx GetSkillFx(int fxID)
	{
#if UNITY_EDITOR
		if (!m_Config.SkillSystemFxsByKey((uint)fxID).HasValue)
			Debug.LogErrorFormat("找不到SkillFxID为 {0} 的技能", fxID);
#endif
		return m_Config.SkillSystemFxsByKey((uint)fxID).Value;
	}
}