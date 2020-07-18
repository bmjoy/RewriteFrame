using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class WarshipListPanel : CompositeView
{
	public MsgWarshipPanelState Data { get; set; }

	public WarshipListPanel() : base(UIPanel.WarshipListPanel, PanelType.Normal) { }
    public WarshipListPanel(UIPanel name, PanelType type) : base(name, type) { }
  

    public override void OnShow(object msg)
	{
		Data = (MsgWarshipPanelState)msg;
	//	State.SetHotkeyBox(GetHotkeyBox());
		base.OnShow(msg);
       
    }

	public override void OnHide(object msg)
	{
		base.OnHide(msg);
		Data = null;
    }

	protected override void OnEscCallback(HotkeyCallback callback)
	{
		OnEscClick?.Invoke(callback);
	}

	public UnityAction<HotkeyCallback> OnEscClick;

   
}