using Eternity.FlatBuffer;
using FlatBuffers;
using PureMVC.Patterns.Proxy;
using UnityEngine;

public partial class CfgEternityProxy : Proxy
{
	public SkillSystemPath GetSkillPath(int pathID)
	{
#if UNITY_EDITOR
		if (!m_Config.SkillSystemPathsByKey((uint)pathID).HasValue)
			Debug.LogErrorFormat("找不到SkillPathID为 {0} 的技能", pathID);
#endif
		return m_Config.SkillSystemPathsByKey((uint)pathID).Value;
	}
}