using Eternity.FlatBuffer;
using PureMVC.Patterns.Proxy;
using UnityEngine.Assertions;

public partial class CfgEternityProxy : Proxy
{
	/// <summary>
	/// 根据itemId获取TreasureSignal基础数据
	/// </summary>
	/// <param name="tid"></param>
	/// <returns></returns>
	public TreasureSignal TreasureSignalsByKey(uint tid)
	{
		TreasureSignal? signalVO = m_Config.TreasureSignalsByKey(tid);
		Assert.IsTrue(signalVO.HasValue, "CfgEternityProxy => TreasureSignalsByKey not exist tid " + tid);
		return signalVO.Value;
	}

}