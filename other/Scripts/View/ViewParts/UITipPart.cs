public class UITipPart : UITipTalentPart
{
    protected override object AdapterTipData(object data)
    {
        PackageProxy m_PackageProxy = GameFacade.Instance.RetrieveProxy(ProxyName.PackageProxy) as PackageProxy;
        if (data is LogDataVO)
        {
            return m_PackageProxy.CreateItem(0, (data as LogDataVO).Tid, 0, 0, 0, 0, 0, 0);
        }
        if (data is ShopWindowVO)
        {
            return m_PackageProxy.CreateItem(0, (data as ShopWindowVO).Tid, 0, 0, 0, 0, 0, 0);
        }
        if (data is ShopSellBackVO)
        {
            return m_PackageProxy.CreateItem(0, (uint)(data as ShopSellBackVO).Tid, 0, 0, 0, 0, 0, 0);
        }
        if (data is ProduceInfoVO)
        {
            return m_PackageProxy.CreateItem(0, (uint)(data as ProduceInfoVO).TID, 0, 0, 0, 0, 0, 0);
        }
        if (data is IShip)
        {
            return m_PackageProxy.GetItem<ItemWarShipVO>((data as IShip).GetUID());
        }
        if (data is IMod)
        {
            return m_PackageProxy.GetItem<ItemModVO>((data as IMod).GetUID());
        }
        if (data is IWeapon)
        {
            return m_PackageProxy.GetItem<ItemWeaponVO>((data as IWeapon).GetUID());
        }
        return base.AdapterTipData(data);
    }
}