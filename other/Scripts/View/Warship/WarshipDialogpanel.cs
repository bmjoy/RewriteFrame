using PureMVC.Interfaces;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

public class WarshipDialogPanel : CompositeView
{
    /// <summary>
    /// 打开界面的初始船
    /// </summary>
    private IShip m_LastShip;
    /// <summary>
    /// 打开界面的出战船
    /// </summary>
    private IShip m_AppointShip;
    /// <summary>
    /// 上次选中的按钮
    /// </summary>
    private Toggle m_BeforeToggle;
    public IShip LastShip
    {
        get { return m_LastShip; }
        set { m_LastShip = value;}
        //get => m_LastShip;
        //set => m_LastShip = value;
    }
    public IShip AppointShip
    {
        get => m_AppointShip;
        set => m_AppointShip = value;
    }
    public Toggle BeforeToggle
    {
        get => m_BeforeToggle;
        set => m_BeforeToggle = value;
    }
    public WarshipDialogPanel() : base(UIPanel.WarshipDialogPanel, PanelType.Normal)
    {
    }

    public override void OnShow(object msg)
    {
        base.OnShow(msg);
    }

    public override void OnHide(object msg)
    {
        base.OnHide(msg);
        //UIManager.Instance.ClosePanel(UIPanel.WarshipModelPanel);
    }

    protected override void OnEscCallback(HotkeyCallback callback)
    {
        if (callback.performed)
        {
            UIManager.Instance.ClosePanel(this);
          
              GameFacade.Instance.SendNotification(NotificationName.MSG_CLOSE_MAIN_PANEL);
        }
    }

}


/// <summary>
/// Warship面板类型
/// </summary>
public enum WarshipPanelState
{
    /// <summary>
    /// 主面板
    /// </summary>
    Main,
    /// <summary>
    /// 武器列表
    /// </summary>
    ListWeapon,
    /// <summary>
    /// 转化炉列表
    /// </summary>
    ListReformer,
    /// <summary>
    /// 装备列表
    /// </summary>
    ListEquip,
    /// <summary>
    /// mod列表
    /// </summary>
    ListMod,
    /// <summary>
    /// 主船mod
    /// </summary>
    ModMainShip,
    /// <summary>
    /// 主武器mod
    /// </summary>
    ModMainWeapon,
}