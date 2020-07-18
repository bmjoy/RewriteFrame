using System.Collections;
using System.Collections.Generic;
using PureMVC.Interfaces;
using UnityEngine;

public class ShopPanel : CompositeView
{
    public ShopPanel() : base(UIPanel.ShopPanel, PanelType.Normal)
    {

    }

    public override void Initialize()
    {
        base.Initialize();

    }

    public override void OnShow(object msg)
    {
        base.OnShow(msg);        
    }
    public override void OnHide(object msg)
    {
        base.OnHide(msg);
    }
}
