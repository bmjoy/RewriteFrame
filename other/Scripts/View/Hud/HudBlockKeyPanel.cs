using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HudBlockKeyPanel : HudBase
{
    /// <summary>
    /// 资源地址
    /// </summary>
    private const string ASSET_ADDRESS = Leyoutech.Core.Loader.Config.AssetAddressKey.PRELOADUI_HUD_EMPTY;
    public HudBlockKeyPanel() : base(UIPanel.HudBlockKeyPanel, ASSET_ADDRESS, PanelType.Hud)
    {

    }

    public override void OnShow(object msg)
    {
        base.OnShow(msg);
        AddBlockKey(HotKeyMapID.SHIP, HotKeyID.EscForShip);
        AddBlockKey(HotKeyMapID.HUMAN, HotKeyID.EscForHuman);
        AddBlockKey(HotKeyMapID.HUMAN, HotKeyID.WatchOpen);
        AddBlockKey(HotKeyMapID.SHIP, HotKeyID.WatchOpen);
    }

    public override void OnHide(object msg)
    {
        DeleteBlockKey(HotKeyMapID.SHIP, HotKeyID.EscForShip);
        DeleteBlockKey(HotKeyMapID.HUMAN, HotKeyID.EscForHuman);
        DeleteBlockKey(HotKeyMapID.HUMAN, HotKeyID.WatchOpen);
        DeleteBlockKey(HotKeyMapID.SHIP, HotKeyID.WatchOpen);
        base.OnHide(msg);
    }
}
