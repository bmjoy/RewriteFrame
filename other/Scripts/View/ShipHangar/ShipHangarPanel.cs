using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ShipHangarPanel : CompositeView
{
    /// <summary>
    /// 船Proxy
    /// </summary>
    private ShipProxy m_ShipProxy;
    public UnityAction<HotkeyCallback> OnEscClick;
    public ShipHangarPanel() : base(UIPanel.ShipHangarPanel, PanelType.Normal)
    {

    }
    public override void Initialize()
    {
        base.Initialize();
        m_ShipProxy = GameFacade.Instance.RetrieveProxy(ProxyName.ShipProxy) as ShipProxy;
        m_ShipProxy.InitShipPackage();
    }
    public override void OnShow(object msg)
    {
        base.OnShow(msg);       
    }
    protected override void OnEscCallback(HotkeyCallback callback)
    {
        OnEscClick?.Invoke(callback);
    }
    public override void OnHide(object msg)
    {
        base.OnHide(msg);          
    }
}

