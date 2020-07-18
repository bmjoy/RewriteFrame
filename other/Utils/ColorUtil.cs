/*===============================
 * Purpose: 公共函数
 * Time: 2019/4/4 15:40:18
================================*/


using Assets.Scripts.Define;
using UnityEngine;

public  static class ColorUtil
{
    /// <summary>
    /// 文字变色
    /// </summary>
    private static string m_AddColorText = "<#{0}>{1}</color>";
    public static string AddColor(string text, Color color)
    {
        return string.Format(m_AddColorText, ColorUtility.ToHtmlStringRGB(color), text);
    }
    /// <summary>
    ///通过道具品质，获得颜色
    /// </summary>
    /// <param name="quality">品质</param>
    public static Color GetColorByItemQuality(int quality)
    {
        if (quality < 1 || quality > 8)
            return Color.clear;

		return GetColorByTable(quality);
	}

	/// <summary>
	/// 获取颜色
	/// </summary>
	/// <param name="tid">tid</param>
	public static Color GetColorByTable(int tid)
	{
		CfgEternityProxy cfgEternityProxy = (CfgEternityProxy)GameFacade.Instance.RetrieveProxy(ProxyName.CfgEternityProxy);
		return cfgEternityProxy.GetGlobalColor(tid);
	}

	public static class ShipColor
	{
		public static Color GetColorByPrime(bool isPrime)
		{
			if (isPrime)
			{
				return GetColorByItemQuality(8);
			}
			return GetColorByItemQuality(1);
		}
	}


}
