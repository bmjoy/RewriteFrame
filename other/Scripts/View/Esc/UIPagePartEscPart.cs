using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIPagePartEscPart : UIPagePart
{
    private const string ASSET_ADDRESS = Leyoutech.Core.Loader.Config.AssetAddressKey.PRELOADUI_COMMONPAGEPART_ESC;

    public override void OnShow(object msg)
    {
        base.OnShow(msg);
        LoadViewPart(ASSET_ADDRESS, OwnerView.PageBox);
    }

    public override void InstallHotkey()
    {
        base.InstallHotkey();
        OwnerView.DeleteHotKey("NavLeft");
        OwnerView.DeleteHotKey("NavRight");
        OwnerView.AddHotKey("NavLeft", HotKeyID.NavLeft, OnQCallback);
        OwnerView.AddHotKey("NavRight", HotKeyID.NavRight, OnECallback);
    }
}
