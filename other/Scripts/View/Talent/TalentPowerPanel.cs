using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TalentPowerPanel : CompositeView
{
    public TalentPowerPanel() : base(UIPanel.TalentPowerPanel, PanelType.Normal)
    {
    }
    protected override void OnEscCallback(HotkeyCallback callback)
    {
        base.OnEscCallback(callback);
        UIManager.Instance.OpenPanel(UIPanel.EscRolePanel,2);
    }
}
