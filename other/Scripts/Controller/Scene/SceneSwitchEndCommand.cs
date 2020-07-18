using PureMVC.Interfaces;
using PureMVC.Patterns.Command;

/// <summary>
/// 场景切换结束时执行
/// </summary>
class SceneSwitchEndCommand : SimpleCommand
{
	public override void Execute(INotification notification)
	{
		CfgEternityProxy cfgEternityProxy = GameFacade.Instance.RetrieveProxy(ProxyName.CfgEternityProxy) as CfgEternityProxy;
		ShipItemsProxy shipItemsProxy = GameFacade.Instance.RetrieveProxy(ProxyName.ShipItemsProxy) as ShipItemsProxy;
		shipItemsProxy.ClearShipItems();

		SceneShipProxy sceneShipProxy = GameFacade.Instance.RetrieveProxy(ProxyName.SceneShipProxy) as SceneShipProxy;
		if (cfgEternityProxy.GetMapByKey(cfgEternityProxy.GetCurrentGamingMapId()).Value.GamingType == 2)
		{
			sceneShipProxy.ShowShip();
		}
		else
		{
			sceneShipProxy.HideShip();
		}

		//设置输入模式
		InputManager.Instance.SceneInputMap = cfgEternityProxy.IsSpace() ? HotKeyMapID.SHIP : HotKeyMapID.HUMAN;

		//设置HUD
		if (cfgEternityProxy.IsSpace())
		{
			OpenHudByShip();
		}
		else
		{
			OpenHudByHuman();
            //进入手表升级场景打开屏蔽hud
            if ((uint)notification.Body == 1100)
            {
                UIManager.Instance.OpenPanel(UIPanel.HudBlockKeyPanel);
            }
            else
            {
                UIManager.Instance.ClosePanel(UIPanel.HudBlockKeyPanel);
            }
		}

		//音效场景切换背景音乐
		WwiseManager.Instance.SceneSwitchEnd(cfgEternityProxy.GetCurrentGamingMapId());
	}

	/// <summary>
	/// 打开人形相关的HUD
	/// </summary>
	private void OpenHudByHuman()
	{
        UIManager.Instance.CloseAllHud();
        UIManager.Instance.CloseAllWindow();

        UIPanel[] panels = new UIPanel[]
		{
            UIPanel.HudGetItemPanel,
            UIPanel.HudHumanFlagPanel,
            UIPanel.HudNpcInteractiveFlagPanel,
            UIPanel.HudWatchPanel,
            UIPanel.HudMissionPanel,
            UIPanel.HudMissionFlagPanel,
            UIPanel.HudTeamPanel,
            UIPanel.HudMessagePanel,
            UIPanel.HudChatPanel,
            UIPanel.HudUpaPanel,
        };

		for (int i = 0; i < panels.Length; i++)
		{
			UIManager.Instance.OpenPanel(panels[i]);
		}
	}

	/// <summary>
	/// 打开船形相关的HUD
	/// </summary>
	private void OpenHudByShip()
    {
        UIManager.Instance.CloseAllHud();
        UIManager.Instance.CloseAllWindow();

        UIPanel[] panels = new UIPanel[]
		{
            UIPanel.HudVoicePanel,
            UIPanel.HudGetItemPanel,
            UIPanel.HudSkillProgress,
            UIPanel.HudShipFlagPanel,
            UIPanel.HudNpcInteractiveFlagPanel,
            UIPanel.HudWatchPanel,
            UIPanel.HudPanel,
            UIPanel.HudShipPanel,
            UIPanel.HudJumpPointPanel,
            UIPanel.HudCrossHairPanel,
            UIPanel.HudTargetInfoPanel,
            UIPanel.HudEdgeFlagPanel,
            UIPanel.HudHitNumber,
            UIPanel.HudNoticePanel,
            UIPanel.HudMissionPanel,
            UIPanel.HudMissionFlagPanel,
            UIPanel.HudTeamPanel,
            UIPanel.HudMessagePanel,
            UIPanel.HudChatPanel,
            UIPanel.HudMineralHologram,
            UIPanel.HudInstanceMessagePanel,
            UIPanel.HudOpenBoxPanel,
            UIPanel.HudDetectorPanel,
            UIPanel.HudUpaPanel,
			UIPanel.HudSceneNamePanel,
            UIPanel.HudAreaNamePanel,
            UIPanel.HudBattleStatePanel
        };

		for (int i = 0; i < panels.Length; i++)
		{
			UIManager.Instance.OpenPanel(panels[i]);
		}
	}
}
