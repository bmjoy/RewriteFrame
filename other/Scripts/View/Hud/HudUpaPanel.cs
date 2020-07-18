using System.Collections;
using System.Collections.Generic;
using PureMVC.Interfaces;
using UnityEngine;

public class HudUpaPanel : HudBase
{
    /// <summary>
    /// 资源地址
    /// </summary>
    private const string ASSET_ADDRESS = Leyoutech.Core.Loader.Config.AssetAddressKey.PRELOADUI_HUD_UPA;
    /// <summary>
    /// 升级标志
    /// </summary>
    private Transform m_Upa;
    private CfgEternityProxy m_CfgEternityProxy;
    public HudUpaPanel() : base(UIPanel.HudUpaPanel, ASSET_ADDRESS, PanelType.Hud)
    {

    }
    public override void Initialize()
    {
        m_Upa = FindComponent<Transform>("Content");
        m_CfgEternityProxy = GameFacade.Instance.RetrieveProxy(ProxyName.CfgEternityProxy) as CfgEternityProxy;
    }
    public override void OnShow(object msg)
    {
        base.OnShow(msg);
    }
    /// <summary>
	/// 输入模式改变时
	/// </summary>
	protected override void OnInputMapChanged()
    {
        OnStateChanged();
    }
    /// <summary>
    /// 显示手表升级标志
    /// </summary>
    public void ShowUpa()
    {
        PlayerInfoVo player = NetworkManager.Instance.GetPlayerController().GetPlayerInfo();
        double exp = m_CfgEternityProxy.GetPlayerUpa(player.WatchLv).Exp;
        if (player.WatchExp >= exp && exp > 0)
        {
            if (!m_Upa.gameObject.activeSelf)
            {
                m_Upa.gameObject.SetActive(true);
            }
        }
        else
        {
            if (m_Upa.gameObject.activeSelf)
            {
                m_Upa.gameObject.SetActive(false);
            }
        }
    }
    public override NotificationName[] ListNotificationInterests()
    {
        return new NotificationName[]
        {
            NotificationName.MSG_PLAYER_WATCH_LEVEL_UP,
            NotificationName.MSG_PLAYER_WATCH_EXP_UP,
            NotificationName.MSG_CHANGE_BATTLE_STATE,
            NotificationName.MainHeroDeath,
            NotificationName.MainHeroRevive
        };
    }
    public override void HandleNotification(INotification notification)
    {
        switch (notification.Name)
        {
            case NotificationName.MSG_PLAYER_WATCH_LEVEL_UP:
            case NotificationName.MSG_PLAYER_WATCH_EXP_UP:
                ShowUpa();
                break;
            case NotificationName.MSG_CHANGE_BATTLE_STATE:
            case NotificationName.MainHeroDeath:
            case NotificationName.MainHeroRevive:
                OnStateChanged();
                break;
        }
    }
    private void OnStateChanged()
    {
        if ((!IsDead() && !IsLeaping() && !IsWatchOrUIInputMode()))
        {
            ShowUpa();
        }
        else
        {
            m_Upa.gameObject.SetActive(false);
        }
    }
    public override void OnHide(object msg)
    {
        base.OnHide(msg);
    }
}
