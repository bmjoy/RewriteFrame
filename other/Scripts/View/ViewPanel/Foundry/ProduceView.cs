using Eternity.FlatBuffer;
using Leyoutech.Core.Loader.Config;
using PureMVC.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProduceView : CompositeView
{
    /// <summary>
	/// 按键进度
	/// </summary>
	private Image m_DownProgressImage;
    /// <summary>
    /// get按键进度
    /// </summary>
    /// <returns></returns>
    public Image GetDownProgressImage() { return m_DownProgressImage; }

    /// <summary>
    /// 当前生产类型
    /// </summary>
    protected ProduceType m_CurrentType;

    /// <summary>
    /// set按键进度
    /// </summary>
    /// <param name="value"></param>
    public void SetDownProgressImage(Image value) {  m_DownProgressImage = value; }


    public ProduceView(UIPanel panelName, PanelType panelType) : base(panelName, panelType)
    {

    }

    public override void OnShow(object msg)
    {
        base.OnShow(msg);
        MsgOpenProduce msgOpenProduce = (MsgOpenProduce)msg;
        m_CurrentType = msgOpenProduce.CurrentProduceType;
    }


    protected override void OnEscCallback(HotkeyCallback callback)
    {
        base.OnEscCallback(callback);
        switch (m_CurrentType)
        {
            case ProduceType.HeavyWeapon:
                UIManager.Instance.OpenPanel(UIPanel.ProduceWeaponDialogPanel);
                break;
            case ProduceType.Reformer:
                UIManager.Instance.OpenPanel(UIPanel.ProduceReformerDialogPanel);
                break;
            case ProduceType.Chip:
                UIManager.Instance.OpenPanel(UIPanel.ProduceChipDialogPanel);
                break;
            case ProduceType.Device:
                UIManager.Instance.OpenPanel(UIPanel.ProduceDeviceDialogPanel);
                break;
            case ProduceType.Ship:
                UIManager.Instance.OpenPanel(UIPanel.ProduceShipDialogPanel);
                break;
            default:
                break;
        }
    }

}
