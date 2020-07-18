using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProduceShipDialogPanel : ProduceDialogBasePanel
{
    public ProduceShipDialogPanel() : base(UIPanel.ProduceShipDialogPanel, PanelType.Normal)
    {
        CurrentProduceType = ProduceType.Ship;
    }

}
