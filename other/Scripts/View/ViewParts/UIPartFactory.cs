
public class UIPartFactory
{
    /// <summary>
    /// 创建UI部件
    /// </summary>
    /// <param name="id">部件ID</param>
    /// <returns>部件实例</returns>
    public static BaseViewPart CreateUIPart(UIPartID id)
    {
        switch(id)
        {
            case UIPartID.BackPart  : return new UIBackPart();
            case UIPartID.TitlePart : return new UITitlePart();
            case UIPartID.PagePart  : return new UIPagePart();
            case UIPartID.FilterPart: return new UIFilterPart();
            case UIPartID.HotkeyPart: return new UIHotkeyPart();
            case UIPartID.TipPart   : return new UITipPart();
            case UIPartID.ModelPart : return new UIModelPart();       
            case UIPartID.MoneyPart : return new UIMoneyPart();

            case UIPartID.ShopListPart: return new UIShopList();
            case UIPartID.PackageListPart: return new UIPackageList();
            case UIPartID.UIMailList:return new UIMailList();
			case UIPartID.WarshipListPart:return new UIWarshipList();
			case UIPartID.UIProduceWeaponPartList: return new UIProduceWeaponPartList();
            case UIPartID.UIProducChipPartList: return new UIProducChipPartList();
            case UIPartID.UIProduceDevicePartList: return new UIProduceDevicePartList();
            case UIPartID.UIProduceReformerPartList: return new UIProduceReformerPartList();
            case UIPartID.UIProducePressPart: return new UIProducePressPart();
            case UIPartID.UISocialList: return new UISocialList();
            case UIPartID.UIShipHangarList:return new UIShipHangarList();
            case UIPartID.UIProduceShipPartList: return new UIProduceShipPartList();
            case UIPartID.UITalentListPart: return new UITalentListPart();
            case UIPartID.UIProduceDialogBaseListPart: return new UIProduceDialogBaseListPart(); 
            case UIPartID.UIWarshipDialogListPart: return new UIWarshipDialogListPart();
            case UIPartID.UIWarshipModListPart: return new UIWarshipModListPart();
            case UIPartID.UIWarshipModelPart: return new UIWarshipModelPart();
            case UIPartID.UIReviveList: return new UIReviveList();
            case UIPartID.UIServerList:return new UIServerList();       
            case UIPartID.TitleEscPart: return new UITitleEscPart();
            case UIPartID.PageEscPart: return new UIPagePartEscPart();
            case UIPartID.UIEscList: return new UIEscList();
            case UIPartID.UIAgreementList:return new UIAgreementList();
			case UIPartID.UIStarGalaxyPart: return new StarGalaxyPart();
			case UIPartID.UIStarPlantPart: return new StarPlantPart();
			case UIPartID.UIStarAreaPart:return new StarAreaPart();
            case UIPartID.UIEscRoleContentPart:return new UIEscRoleContentPart();
            case UIPartID.UIEscWarShipContentPart:return new UIEscWarShipContentPart();
            case UIPartID.UIEscPermitContentPart:return new UIEscPermitContentPart();
            case UIPartID.UIEscSettingContentPart:return new UIEscSettingContentPart();
        }

        return null;
    }
}