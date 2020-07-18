using Leyoutech.Core.Loader.Config;

public class GameFitMask : UIPanelBase
{
    private const string ASSET_ADDRESS = AssetAddressKey.PRELOADUI_GAMEFITMASK;

    public GameFitMask() : base(UIPanel.GameFitMask, ASSET_ADDRESS, PanelType.Mask)
    {
        CanReceiveFocus = false;
    }
}
