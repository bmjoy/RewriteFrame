using Leyoutech.Core.Loader.Config;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EscWarShipPanel : CompositeView
{
    public EscWarShipPanel() : base(UIPanel.EscWarShipPanel, PanelType.Normal)
    {

    }

    public override void OnShow(object msg)
    {
        base.OnShow(msg);
        State.GetAction(UIAction.Common_Back).Callback -= OnEscCallback;
    }
}
