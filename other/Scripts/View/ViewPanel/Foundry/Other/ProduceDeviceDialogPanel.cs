using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProduceDeviceDialogPanel : ProduceDialogBasePanel
{
    public ProduceDeviceDialogPanel() : base(UIPanel.ProduceDeviceDialogPanel, PanelType.Normal)
    {
        CurrentProduceType = ProduceType.Device;
    }
}
