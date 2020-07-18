using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

class WarshipModPanel : CompositeView
{

	public WarshipModPanel() : base(UIPanel.WarshipModPanel, PanelType.Normal)
	{
	}
    public override void OnShow(object msg)
    {
        base.OnShow(msg);
       
    }

    public override void OnHide(object msg)
    {
        base.OnHide(msg);
      //  UIManager.Instance.ClosePanel(UIPanel.WarshipModelPanel);
    }

    /// <summary>
    /// esc操作
    /// </summary>
    /// <param name="callback"></param>
    protected override void OnEscCallback(HotkeyCallback callback)
    {
        if (callback.performed)
        {
            OnEscClick?.Invoke(callback);
        }
    }

    public UnityAction<HotkeyCallback> OnEscClick;

}