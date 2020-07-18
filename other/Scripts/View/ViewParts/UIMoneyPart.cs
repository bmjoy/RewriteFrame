using PureMVC.Interfaces;

public class UIMoneyPart : BaseViewPart
{
    private const string ASSET_ADDRESS = Leyoutech.Core.Loader.Config.AssetAddressKey.PRELOADUI_COMMONTOPMONEY;

    /// <summary>
    /// 货币A
    /// </summary>
    private UIIconAndLabel m_Money1;
    /// <summary>
    /// 贷币B
    /// </summary>
    private UIIconAndLabel m_Money2;

    public override void OnShow(object msg)
    {
        base.OnShow(msg);

        if(OwnerView.OtherBox)
            LoadViewPart(ASSET_ADDRESS, OwnerView.OtherBox);
    }

    /// <summary>
    /// 部件加载时
    /// </summary>
    protected override void OnViewPartLoaded()
    {
        m_Money1 = FindComponent<UIIconAndLabel>("Content/Coin1");
        m_Money2 = FindComponent<UIIconAndLabel>("Content/Coin2");

        UpdateMoneys();
    }

    /// <summary>
    /// 部件卸载时
    /// </summary>
    protected override void OnViewPartUnload()
    {
        m_Money1 = null;
        m_Money2 = null;
    }

    public override NotificationName[] ListNotificationInterests()
    {
        return new NotificationName[]
       {
            NotificationName.MSG_CURRENCY_CHANGED
       };
    }

    public override void HandleNotification(INotification notification)
    {
        base.HandleNotification(notification);
        switch (notification.Name)
        {
            case NotificationName.MSG_CURRENCY_CHANGED:
                UpdateMoneys();
                break;
        }
    }

    private void UpdateMoneys()
    {
        if(m_Money1 && m_Money1.Label)
            m_Money1.Label.text = CurrencyUtil.GetRechargeCurrencyCount().ToString();
        if(m_Money2 && m_Money2.Label)
            m_Money2.Label.text = CurrencyUtil.GetGameCurrencyCount().ToString();
    }
}
