using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UITitleEscPart : BaseViewPart
{
    private const string ASSET_ADDRESS = Leyoutech.Core.Loader.Config.AssetAddressKey.PRELOADUI_COMMONTITLEPART_ESC;

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
        TMP_Text label = FindComponent<TMP_Text>("Title/Label_Title");
        if (label != null)
        {
            if (label)
            {
                label.text = State.MainTitle;
            }
        }
    }

    
}
