public class UITitlePart : BaseViewPart
{
    private const string ASSET_ADDRESS = Leyoutech.Core.Loader.Config.AssetAddressKey.PRELOADUI_COMMONTITLEPART;

    public override void OnShow(object msg)
    {
        base.OnShow(msg);

        LoadViewPart(ASSET_ADDRESS, OwnerView.TitleBox);
    }

    /// <summary>
    /// 部件加载时
    /// </summary>
    protected override void OnViewPartLoaded()
    {
        UIIconAndLabel iconAndLabel = FindComponent<UIIconAndLabel>("Content");
        if(iconAndLabel != null)
        {
            if (iconAndLabel.Icon)
            {
                uint iconID = (uint)State.Icon;
                if (iconID != 0)
                    UIUtil.SetIconImage(iconAndLabel.Icon, (uint)State.Icon);
                else
                    iconAndLabel.Icon.sprite = null;
            }

            if (iconAndLabel.Label)
                iconAndLabel.Label.text = State.MainTitle;

            if (iconAndLabel.Info)
                iconAndLabel.Info.text = State.SecondaryTitle;

        }
    }

    /// <summary>
    /// 部件卸载时
    /// </summary>
    protected override void OnViewPartUnload()
    {
    }
}

