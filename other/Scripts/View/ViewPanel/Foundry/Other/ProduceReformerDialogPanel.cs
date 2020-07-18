using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProduceReformerDialogPanel : ProduceDialogBasePanel
{
    public ProduceReformerDialogPanel() : base(UIPanel.ProduceReformerDialogPanel, PanelType.Normal)
    {
        CurrentProduceType = ProduceType.Reformer;
    }

}
