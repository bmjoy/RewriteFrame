using Eternity.FlatBuffer;
using PureMVC.Patterns.Proxy;
using UnityEngine.Assertions;

public partial class CfgEternityProxy : Proxy
{
	/// <summary>
	/// 根据itemId获取PackageItem基础数据
	/// </summary>
	/// <param name="tid"></param>
	/// <returns></returns>
	public PackageItem PackageItemsByKey(uint tid)
	{
		PackageItem? packageItemVO = m_Config.PackageItemsByKey(tid);
		Assert.IsTrue(packageItemVO.HasValue, "CfgEternityProxy => PackageItemsByKey not exist tid " + tid);
		return packageItemVO.Value;
	}

}