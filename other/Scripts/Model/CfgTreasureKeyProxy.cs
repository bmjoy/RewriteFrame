using Eternity.FlatBuffer;
using PureMVC.Patterns.Proxy;
using UnityEngine.Assertions;

public partial class CfgEternityProxy : Proxy
{
	/// <summary>
	/// 根据index获取TreasureKey基础数据
	/// </summary>
	/// <param name="index">索引</param>
	/// <returns></returns>
	public TreasureKey TreasureKeyByIndex(int index)
	{
		TreasureKey? signalVO = m_Config.TreasureKeys(index);
		Assert.IsTrue(signalVO.HasValue, "CfgEternityProxy => TreasureKeyByKey not exist index " + index);
		return signalVO.Value;
	}



}