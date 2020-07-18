using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TalentSciencePanel : CompositeView
{
    public TalentSciencePanel() : base(UIPanel.TalentSciencePanel, PanelType.Normal)
    {
    }

    protected override void OnEscCallback(HotkeyCallback callback)
    {
        base.OnEscCallback(callback);
        UIManager.Instance.OpenPanel(UIPanel.EscRolePanel,1);

    }
}
