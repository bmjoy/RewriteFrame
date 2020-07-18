using Eternity.FlatBuffer;
using PureMVC.Patterns.Proxy;
using UnityEngine.Assertions;

public partial class CfgEternityProxy : Proxy
{

	/// <summary>
	/// 根据NpcId获取Mine基础数据
	/// </summary>
	/// <param name="tid"></param>
	/// <returns></returns>
	public Mine GetMineByKey(uint npcId)
	{
		Mine? mineVO = m_Config.MinesByKey(npcId);
		Assert.IsTrue(mineVO.HasValue, "CfgEternityProxy => GetMineByKey not exist tid " + npcId);
		return mineVO.Value;
	}

	/// <summary>
	/// 根据npcid获取血条段数长度
	/// </summary>
	/// <param name="tid">tid</param>
	/// <returns></returns>
	public int GetDoppingBloodVolumeLengthByKey(uint tid)
	{
		
		return GetMineByKey(tid).DroppingBloodVolumeLength;
	}

}