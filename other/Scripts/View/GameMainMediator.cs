using Assets.Scripts.Define;
using Crucis.Protocol;
using PureMVC.Interfaces;
using PureMVC.Patterns.Mediator;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameMainMediator : Mediator
{
	public GameMainMediator() : base(UIPanel.GameMainMediator)
	{

	}

	public override void OnRegister()
	{
		base.OnRegister();

		GameplayManager.Instance.Initialize();

		BindStarmapOpen();
		BindEscKey();
	}

	public override void OnRemove()
	{
		base.OnRemove();
		UnbindStarmapOpen();
		UnbindEscKey();
	}

	public void SwtichMap(ulong areaId, Vector3 worldPos)
	{
		StateChangeHandler.HandleChangeState();
		DropHandler.HandleSyncDrop();
		SkillBroadCastRPCNet.Handle();

		LoadingPanelParamere loadingPanelParamere = MessageSingleton.Get<LoadingPanelParamere>();
		loadingPanelParamere.OnShown = () =>
		 {
			 GameplayProxy gameplayProxy = GameFacade.Instance.RetrieveProxy(ProxyName.GameplayProxy) as GameplayProxy;
			 if (gameplayProxy != null)
			 {
				 GameplayManager.Instance.Clear();
				 gameplayProxy.SetCurrentAreaUid(areaId);
				 Vector3 gameWorldPos = gameplayProxy.WorldPositionToServerAreaOffsetPosition(worldPos);
				 Map.MapManager.GetInstance().SetPlayerPosition(worldPos, gameWorldPos);
			 }
			 OnSwitchMapLoadingPanelShown();
		 };
		UIManager.Instance.OpenPanel(UIPanel.LoadingSecondPanel, loadingPanelParamere);

		CfgEternityProxy cfgEternityProxy = GameFacade.Instance.RetrieveProxy(ProxyName.CfgEternityProxy) as CfgEternityProxy;
		uint lastGamingMapId = cfgEternityProxy.GetLastGamingMapId();
		uint gamingMapId = cfgEternityProxy.GetCurrentGamingMapId();
		if (lastGamingMapId != Map.Constants.NOTSET_MAP_UID)
		{
			int lastMapType = cfgEternityProxy.GetMapByKey(lastGamingMapId).Value.GamingType;
			int mapType = cfgEternityProxy.GetMapByKey(gamingMapId).Value.GamingType;
			if (lastMapType == 4 && mapType == 4)
			{
				WwiseUtil.PlaySound(WwiseManager.voiceComboID, WwiseMusicSpecialType.SpecialType_Voice_StartCrossingTheGate, WwiseMusicPalce.Palce_1st, false, null);
			}
		}
	}

	void OnSwitchMapLoadingPanelShown()
	{
		uint MapId = (GameFacade.Instance.RetrieveProxy(ProxyName.CfgEternityProxy) as CfgEternityProxy).GetCurrentMapId();
		// before loading
		Map.MapManager.GetInstance().TryChangeMap(MapId, () =>
		{
			// after loading 
			CfgEternityProxy cfgEternityProxy = GameFacade.Instance.RetrieveProxy(ProxyName.CfgEternityProxy) as CfgEternityProxy;
			uint lastGamingMapId = cfgEternityProxy.GetLastGamingMapId();
			uint gamingMapId = cfgEternityProxy.GetCurrentGamingMapId();
			if (lastGamingMapId != Map.Constants.NOTSET_MAP_UID)
			{
				int lastMapType = cfgEternityProxy.GetMapByKey(lastGamingMapId).Value.GamingType;
				int mapType = cfgEternityProxy.GetMapByKey(gamingMapId).Value.GamingType;
				if (lastMapType == 4 && mapType == 4)
				{
					WwiseUtil.PlaySound(WwiseManager.voiceComboID, WwiseMusicSpecialType.SpecialType_Voice_EndCrossingTheGate, WwiseMusicPalce.Palce_1st, false, null);
				}
				else if (lastMapType == 4 && mapType == 2)
				{
					WwiseUtil.PlaySound(WwiseManager.voiceComboID, WwiseMusicSpecialType.SpecialType_Voice_From_DeepSpaceToSpaceStation, WwiseMusicPalce.Palce_1st, false, null);
				}
				else if (lastMapType == 2 && mapType == 4)
				{
					WwiseUtil.PlaySound(WwiseManager.voiceComboID, WwiseMusicSpecialType.SpecialType_Voice_From_SpaceStationToDeepSpace, WwiseMusicPalce.Palce_1st, false, null);
				}
			}

			Scene gameMainScene = SceneManager.GetSceneByName("GameMain");
			if (gameMainScene == null)
			{
				throw new System.Exception("not find scene 'GameMain'");
			}

			GameplayManager.Instance.SwitchMap(gameMainScene);
			GameFacade.Instance.SendNotification(NotificationName.MSG_SWITCH_SCENE_END, MapId);
		});
	}

	public override NotificationName[] ListNotificationInterests()
	{
		return base.ListNotificationInterests();
	}

	public override void HandleNotification(INotification notification)
	{
		base.HandleNotification(notification);
	}

	#region ESC

	private bool m_EscPressed = false;

	/// <summary>
	/// 绑定ESC键
	/// </summary>
	private void BindEscKey()
	{
		UnbindEscKey();

		HotkeyManager.Instance.Register("GameMainMediator_human_esc", HotKeyMapID.HUMAN, HotKeyID.EscForHuman, OnEsc);
		HotkeyManager.Instance.Register("GameMainMediator_ship_esc", HotKeyMapID.SHIP, HotKeyID.EscForShip, OnEsc);
		InputManager.Instance.OnInputActionMapChanged += OnInputMapChanged;
	}

	/// <summary>
	/// 取消绑定ESC键
	/// </summary>
	private void UnbindEscKey()
	{
		HotkeyManager.Instance.Unregister("GameMainMediator_human_esc");
		HotkeyManager.Instance.Unregister("GameMainMediator_ship_esc");
	}

	/// <summary>
	/// 处理Esc键
	/// </summary>
	/// <param name="act"></param>
	private void OnEsc(HotkeyCallback act)
	{
		if (act.started)
			m_EscPressed = true;
		else if (act.performed && m_EscPressed)
		{
			UIPanelBase current = UIManager.Instance.FocusPanel;
			if (current == null)
			{
				if (!IsLeaping())
					UIManager.Instance.OpenPanel(UIPanel.EscWatchPanel);
			}
		}
	}

	/// <summary>
	/// 输入表改变时
	/// </summary>
	/// <param name="a"></param>
	/// <param name="b"></param>
	private void OnInputMapChanged(HotKeyMapID b)
	{
		m_EscPressed = false;
	}


	/// <summary>
	/// 是否在跃迁中
	/// </summary>
	/// <returns>bool</returns>
	protected bool IsLeaping()
	{
		GameplayProxy proxy = Facade.RetrieveProxy(ProxyName.GameplayProxy) as GameplayProxy;
		SpacecraftEntity entity = proxy.GetEntityById<SpacecraftEntity>(proxy.GetMainPlayerUID());
		if (entity)
		{
            return entity.IsLeap();
		}
		return false;
	}

	#endregion

	#region HotKey StarmapOpen 星图
	private void BindStarmapOpen()
	{
		HotkeyManager.Instance.Register("GameMainMediator_ship_starmapopen", HotKeyMapID.SHIP, HotKeyID.StarmapOpen, OnStarmapOpen);
	}

	private void UnbindStarmapOpen()
	{
		HotkeyManager.Instance.Unregister("GameMainMediator_ship_starmapopen");
	}

	private void OnStarmapOpen(HotkeyCallback callback)
	{
		if (callback.performed)
		{
			CfgEternityProxy cfgEternityProxy = GameFacade.Instance.RetrieveProxy(ProxyName.CfgEternityProxy) as CfgEternityProxy;

			uint gamingMapId = cfgEternityProxy.GetCurrentGamingMapId();
			int mapType = cfgEternityProxy.GetMapByKey(gamingMapId).Value.GamingType;
			UIPanelBase current = UIManager.Instance.FocusPanel;
			if (current == null && mapType == 4)
			{
				UIManager.Instance.OpenPanel(UIPanel.StarAreaPanel);
			}
		}
	}

	#endregion
}