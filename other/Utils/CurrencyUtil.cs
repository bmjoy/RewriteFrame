using static GameConstant;

public class CurrencyUtil
{
	/// <summary>
	/// 游戏币
	/// </summary>
	/// <returns></returns>
	public static long GetGameCurrencyCount()
	{
		return GetCurrencyCountBy(CurrencyConst.GAME_CURRENCY_ITEM_TID);
	}

	/// <summary>
	/// 充值币
	/// </summary>
	/// <returns></returns>
	public static long GetRechargeCurrencyCount()
	{
		return GetCurrencyCountBy(CurrencyConst.RECHARGE_CURRENCY_ITEM_TID);
	}

    /// <summary>
	/// 天赋点
	/// </summary>
	/// <returns></returns>
	public static long GetTalentCurrencyCount()
    {
        return GetCurrencyCountBy(CurrencyConst.TALENTCURRENCY);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="itemTid"></param>
    /// <returns></returns>
    public static long GetCurrencyCountBy(uint itemTid)
	{
		switch (itemTid)
		{
			case CurrencyConst.GAME_CURRENCY_ITEM_TID:
			case CurrencyConst.RECHARGE_CURRENCY_ITEM_TID:
            case CurrencyConst.TALENTCURRENCY:
                return (GameFacade.Instance.RetrieveProxy(ProxyName.PackageProxy) as PackageProxy).GetItemCountByTID(itemTid);
		}
		throw new System.Exception("CurrencyType Error: " + itemTid);
	}

}
