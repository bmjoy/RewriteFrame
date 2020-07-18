using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProduceChipDialogPanel : ProduceDialogBasePanel
{
    public ProduceChipDialogPanel() : base(UIPanel.ProduceChipDialogPanel, PanelType.Normal)
    {
        CurrentProduceType = ProduceType.Chip;
    }

}
