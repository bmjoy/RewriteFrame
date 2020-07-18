using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProduceDialogBasePanel : CompositeView
{
    /// <summary>
    /// 当前生产类型
    /// </summary>
    public ProduceType CurrentProduceType;

    /// <summary>
    /// 是否通过Esc 关闭
    /// </summary>
    public bool CloseByEsc = true;
    public ProduceDialogBasePanel(UIPanel panelName, PanelType panelType) : base(panelName, panelType)
    {

    }
    protected override void OnEscCallback(HotkeyCallback callback)
    {
        base.OnEscCallback(callback);
        CloseByEsc = true;
    }
}
