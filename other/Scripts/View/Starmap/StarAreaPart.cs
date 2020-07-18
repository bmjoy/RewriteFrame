using Assets.Scripts.Define;
using DG.Tweening;
using Eternity.Runtime.Enums;
using Map;
using PureMVC.Interfaces;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static InputManager;
using static TaskTrackingProxy;

/// <summary>
/// 星域面板（子面板）
/// </summary>
public class StarAreaPart : BaseViewPart
{
	/// <summary>
	/// 资源地址
	/// </summary>
	private const string ASSET_ADDRESS = Leyoutech.Core.Loader.Config.AssetAddressKey.PRELOADUI_STARAREAPANEL;
	/// <summary>
	/// 游戏数据proxy
	/// </summary>
	private CfgEternityProxy m_CfgEternityProxy;
	/// <summary>
	/// 星图数据proxy
	/// </summary>
	private CfgStarmapProxy m_CfgStarmapProxy;
	/// <summary>
	/// 任务跟踪数据proxy
	/// </summary>
	private TaskTrackingProxy m_TaskTrackingProxy;
	/// <summary>
	/// 任务proxy
	/// </summary>
	private MissionProxy m_MissionProxy;
	/// <summary>
	/// 队伍proxy
	/// </summary>
	private TeamProxy m_TeamProxy;
	/// <summary>
	/// 图例的显示隐藏动画
	/// </summary>
	private Animator m_LegendAnim;
	/// <summary>
	/// 星域模型背景显示图
	/// </summary>
	private Starmap3DViewer m_Starmap3DViewer;
	/// <summary>
	/// 星域根节点
	/// </summary>
	private RectTransform m_AreaPointContainer;
	/// <summary>
	/// 滑动列表
	/// </summary>
	private ScrollRect m_Scroller;
	/// <summary>
	/// 滚动灵敏度
	/// </summary>
	private float m_ScrollSensitivity;
	/// <summary>
	/// 星域tips
	/// </summary>
	private StarAreaPanelLeftTips m_StarAreaPanelLeftTips;
	/// <summary>
	/// 所有星域数据存储
	/// </summary>
	private Dictionary<ulong, StarAreaElement> m_PointDic;
	/// <summary>
	/// 线集合
	/// </summary>
	private List<GameObject> m_Lines;
	/// <summary>
	/// 当前选中星域
	/// </summary>
	private StarAreaElement m_CurrentCell;
	/// <summary>
	/// 行星系数据
	/// </summary>
	private EditorPlanet m_Data;
	/// <summary>
	/// 任务追踪数据
	/// </summary>
	private List<TrackingInfo> m_MissionTrackings;
	/// <summary>
	/// 队伍成员数据
	/// </summary>
	private List<TeamMemberVO> m_TeamMembers;

	private float m_MapW;
	private float m_MapH;
	private TMP_Text m_Title;

	public override void OnShow(object msg)
	{
		base.OnShow(msg);
		m_CfgEternityProxy = GameFacade.Instance.RetrieveProxy(ProxyName.CfgEternityProxy) as CfgEternityProxy;
		m_CfgStarmapProxy = GameFacade.Instance.RetrieveProxy(ProxyName.CfgStarmapProxy) as CfgStarmapProxy;
		m_TaskTrackingProxy = GameFacade.Instance.RetrieveProxy(ProxyName.TaskTrackingProxy) as TaskTrackingProxy;
		m_TeamProxy = GameFacade.Instance.RetrieveProxy(ProxyName.TeamProxy) as TeamProxy;
		m_MissionProxy = GameFacade.Instance.RetrieveProxy(ProxyName.MissionProxy) as MissionProxy;
		if (msg is MsgStarmapPanelState)
		{
			m_Data = (msg as MsgStarmapPanelState).Data as EditorPlanet;
		}
		else
		{
			Eternity.FlatBuffer.Map mapData = m_CfgEternityProxy.GetCurrentMapData();
			m_Data = m_CfgStarmapProxy.GetPlanet(mapData.BelongFixedStar, mapData.GamingmapId);
		}
		m_PointDic = new Dictionary<ulong, StarAreaElement>();
		m_Lines = new List<GameObject>();

		LoadViewPart(ASSET_ADDRESS, OwnerView.OtherBox);
	}

	protected override void OnViewPartLoaded()
	{
		base.OnViewPartLoaded();
		RectTransform root = OwnerView.OtherBox.GetChild(0).GetComponent<RectTransform>();
		m_Title = FindComponent<TMP_Text>(root, "Content/Title/Label_Title");
		m_LegendAnim = FindComponent<Animator>(root, "Content");
		m_AreaPointContainer = FindComponent<RectTransform>("Content/MapList/Viewport/Content");
		m_Starmap3DViewer = FindComponent<RawImage>(root, "Back/Image_Ball").GetOrAddComponent<Starmap3DViewer>();
		m_StarAreaPanelLeftTips = FindComponent<Transform>(root, "Content/AreaMessage").gameObject.AddComponent<StarAreaPanelLeftTips>();
		m_StarAreaPanelLeftTips.Initialize(this);
		m_StarAreaPanelLeftTips.MissionSelected = SelectedMission;
		m_Scroller = FindComponent<ScrollRect>(root, "Content/MapList");
		m_ScrollSensitivity = m_Scroller.scrollSensitivity;
		m_Scroller.onValueChanged.AddListener(OnMove);
		InitLegend();

		InputManager.Instance.OnInputDeviceChanged += OnInputDeviceChanged;

		//跃迁
		State.GetAction(UIAction.StarArea_Leap).Callback += OnLeap;
		State.GetAction(UIAction.StarArea_Leap).Enabled = false;
		//任务追踪
		State.GetAction(UIAction.StarArea_TrackMission).Callback += OnTrackMission;
		//放弃任务
		State.GetAction(UIAction.StarArea_DropMission).Callback += OnDropMission;
		//切换左侧cell
		State.GetAction(UIAction.StarArea_TipsToggle).Callback += OnToggleTips;
		//上一级
		State.GetAction(UIAction.StarArea_ToPlant).Callback += OnEsc;
		//显示隐藏图例
		State.GetAction(UIAction.StarArea_Legend).Callback += OnTab;

		OwnerView.AddHotKey(HotKeyID.StarmapOpen, OnCloseClick);

		OnRefresh();

        UIManager.Instance.ClosePanel(UIPanel.StarGalaxyPanel);
        UIManager.Instance.ClosePanel(UIPanel.StarPlantPanel);
    }

	/// <summary>
	/// 输入设备改变
	/// </summary>
	/// <param name="device"></param>
	private void OnInputDeviceChanged(GameInputDevice device)
	{
		if (device == GameInputDevice.KeyboardAndMouse)
		{
			m_Scroller.scrollSensitivity = 0;
		}
		else
		{
			m_Scroller.scrollSensitivity = m_ScrollSensitivity;
		}
	}

	public override void OnHide()
	{
		State.GetAction(UIAction.StarArea_Leap).Callback -= OnLeap;
		State.GetAction(UIAction.StarArea_TrackMission).Callback -= OnTrackMission;
		State.GetAction(UIAction.StarArea_TipsToggle).Callback -= OnToggleTips;
		State.GetAction(UIAction.StarArea_ToPlant).Callback -= OnEsc;
		State.GetAction(UIAction.StarArea_Legend).Callback -= OnTab;
		OwnerView.DeleteHotKey(HotKeyID.StarmapOpen);

		m_Scroller.onValueChanged.RemoveAllListeners();
		InputManager.Instance.OnInputDeviceChanged -= OnInputDeviceChanged;
		m_Starmap3DViewer.SetModel(null);
		m_StarAreaPanelLeftTips.OnHide();
		GameObject.Destroy(m_StarAreaPanelLeftTips);
		m_CurrentCell = null;
		foreach (KeyValuePair<ulong, StarAreaElement> item in m_PointDic)
		{
			UIEventListener.UIEventListener.AttachListener(item.Value.gameObject).onEnter = null;
			item.Value.Destroy();
			item.Value.Recycle();
			item.Value.gameObject.Recycle();
		}
		m_PointDic.Clear();
		while (m_Lines.Count > 0)
		{
			m_Lines[0].gameObject.Recycle();
			m_Lines.RemoveAt(0);
		}
		m_Lines.Clear();
		base.OnHide();
	}

	public void OnRefresh()
	{
		if (m_Data != null)
		{
			m_Title.text = TableUtil.GetLanguageString($"gamingmap_name_{m_Data.gamingmapId}");
			m_Starmap3DViewer.SetModel(m_Data.bgmapRes, Vector3.zero, Vector3.zero, Vector3.one);
			m_Starmap3DViewer.GetComponent<RectTransform>().sizeDelta = m_Data.bgmapScale.ToVector2();
			m_Starmap3DViewer.transform.localPosition = m_Data.bgmapPos.ToVector2();
			m_MissionTrackings = m_TaskTrackingProxy.GetAllTrackings();
			m_TeamMembers = m_TeamProxy.GetMembersList();
			DrawMap(MapManager.GetInstance().GetCurrentAreaUid());
			DrawLine();
			m_AreaPointContainer.sizeDelta = new Vector2(m_Data.minimapSize, m_Data.minimapSize);
			if (!m_CurrentCell)
			{
				foreach (var item in m_PointDic)
				{
					m_CurrentCell = item.Value as StarAreaElement;
					break;
				}
			}
			if (m_CurrentCell)
			{
				OwnerView.FocusTo(m_CurrentCell.gameObject);
				m_StarAreaPanelLeftTips.SetData(m_Data.gamingmapId, m_CurrentCell.GetData(), m_MissionTrackings, m_TeamMembers);
				m_StarAreaPanelLeftTips.gameObject.SetActive(true);
			}
			else
			{
				m_StarAreaPanelLeftTips.gameObject.SetActive(false);
			}
			State.GetAction(UIAction.StarArea_TipsToggle).Enabled = m_StarAreaPanelLeftTips.HasToggle();
			m_AreaPointContainer.DOLocalMove(-m_CurrentCell.transform.localPosition, 0.5F).SetEase(Ease.OutExpo);
		}
	}

	#region DarwMap&Line
	/// <summary>
	/// 绘制星域
	/// </summary>
	/// <param name="currentAreaUid"></param>
	private void DrawMap(ulong currentAreaUid)
	{
		for (int i = 0; i < m_Data.arealist.Length; i++)
		{
			StarAreaElement cell =
				(OwnerView as StarAreaPanel).PointPrefab.Spawn(m_AreaPointContainer, m_Data.arealist[i].position.ToVector2())
				.AddComponent<StarAreaElement>();

			cell.transform.localScale = Vector3.one;
			cell.GetComponent<Toggle>().group = m_AreaPointContainer.GetOrAddComponent<ToggleGroup>();
			cell.OnSelected = AreaPointOnSelected;
			UIEventListener.UIEventListener.AttachListener(cell.gameObject).onEnter = OnCellEnter;
			cell.SetData(m_Data.gamingmapId, m_Data.arealist[i], m_Data.arealist[i].areaId == currentAreaUid);
			SetMainIcon(cell);
			CheckMissionTeamInArea(cell);
			m_PointDic.Add(m_Data.arealist[i].areaId, cell);
			if (m_Data.arealist[i].areaId == currentAreaUid)
			{
				m_CurrentCell = cell;
			}
			if (Math.Abs(m_Data.arealist[i].position.ToVector2().x) > m_MapW)
			{
				m_MapW = Math.Abs(m_Data.arealist[i].position.ToVector2().x);
			}
			if (Math.Abs(m_Data.arealist[i].position.ToVector2().y) > m_MapH)
			{
				m_MapH = Math.Abs(m_Data.arealist[i].position.ToVector2().y);
			}
		}

	}
	/// <summary>
	/// 绘制星域连线
	/// </summary>
	private void DrawLine()
	{
		EditorStarMapArea areaData;
		for (int i = 0; i < m_Data.arealist.Length; i++)
		{
			areaData = m_Data.arealist[i];
			if (areaData.childrenAreaList != null && areaData.childrenAreaList.Length > 0)
			{
				for (int j = 0; j < areaData.childrenAreaList.Length; j++)
				{
					GameObject go = (OwnerView as StarAreaPanel).LinePrefab.Spawn();
					go.transform.SetParent(m_AreaPointContainer);
					go.transform.SetAsFirstSibling();
					go.transform.localPosition = m_PointDic[areaData.areaId].transform.localPosition;
					go.transform.localScale = Vector3.one;
					go.transform.up = (m_PointDic[areaData.childrenAreaList[j]].transform.localPosition - m_PointDic[areaData.areaId].transform.localPosition).normalized;
					go.GetComponent<RectTransform>().sizeDelta = new Vector2(2, Vector3.Distance(m_PointDic[areaData.areaId].transform.localPosition, m_PointDic[areaData.childrenAreaList[j]].transform.localPosition));
					m_Lines.Add(go);
				}
			}
		}
	}
	#endregion

	private void SetMainIcon(StarAreaElement cell)
	{
		cell.SetTopIcon(m_CfgEternityProxy.GetIconIDByEnum((StarmapLeapIcon)cell.GetData().areaType), cell.GetData().area_leap_type == 2);
	}

	private void CheckMissionTeamInArea(StarAreaElement cell)
	{
		if (m_MissionTrackings?.Count > 0 || m_TeamMembers.Count > 0)
		{
			bool hasMain = false;
			bool hasBranch = false;
			List<uint> s = new List<uint>();

			for (int i = 0; i < m_MissionTrackings?.Count; i++)
			{
				if (m_MissionTrackings[i].EndingLeapPointID == cell.GetData().areaId)
				{
					if (m_MissionTrackings[i].MissionType == MissionType.Main)
					{
						hasMain = true;
					}
					else if (m_MissionTrackings[i].MissionType == MissionType.Branch)
					{
						hasBranch = true;
					}
				}
			}
			if (hasMain) { s.Add(31311); }
			if (hasBranch) { s.Add(31312); }
			for (int i = 0; i < m_TeamMembers.Count; i++)
			{
				if ((ulong)m_TeamMembers[i].AreaId == cell.GetData().areaId)
				{
					s.Add(31316);
				}
				break;
			}
			if (s.Count > 0)
			{
				cell.SetBottomIcon(s.ToArray());
				return;
			}
		}
		cell.SetBottomIcon(null);
	}

	private void OnCellEnter(GameObject go, params object[] args)
	{
		if (InputManager.Instance.CurrentInputDevice != InputManager.GameInputDevice.KeyboardAndMouse)
		{
			OwnerView.FocusTo(go);
			SetCurrentCell(go.GetComponent<StarAreaElement>());
		}
	}

	/// <summary>
	/// 星域选中事件
	/// </summary>
	/// <param name="cell"></param>
	private void AreaPointOnSelected(StarmapPointElementBase cell)
	{
		SetCurrentCell(cell as StarAreaElement);
	}

	private void SetCurrentCell(StarAreaElement cell, float duration = 0.5f)
	{
		if (m_CurrentCell)
		{
			m_CurrentCell.SetToggleIsOn(false);
		}
		m_CurrentCell = cell;
		m_CurrentCell.SetToggleIsOn(true);
		m_AutoMoveing = true;
		m_AreaPointContainer.DOLocalMove(-m_CurrentCell.transform.localPosition, 0.5f).SetEase(Ease.OutExpo).OnComplete(() => { m_AutoMoveing = false; });

		m_StarAreaPanelLeftTips.SetData(m_Data.gamingmapId, m_CurrentCell.GetData(), m_MissionTrackings, m_TeamMembers);

		State.GetAction(UIAction.StarArea_TipsToggle).Enabled = m_StarAreaPanelLeftTips.HasToggle();
		State.GetAction(UIAction.StarArea_Leap).Enabled = m_Data.gamingmapId == m_CfgEternityProxy.GetCurrentGamingMapId() && m_CurrentCell.GetData().areaId != MapManager.GetInstance().GetCurrentAreaUid();
	}


	private void SetCurrentToNull()
	{
		if (m_CurrentCell)
		{
			m_CurrentCell.SetToggleIsOn(false);
			State.GetAction(UIAction.StarArea_TipsToggle).Enabled = false;
			State.GetAction(UIAction.StarArea_Leap).Enabled = false;

			m_CurrentCell = null;
		}
	}

	public override NotificationName[] ListNotificationInterests()
	{
		return new NotificationName[]
		{
			 NotificationName.MSG_MISSION_ABANDON,
		};
	}

	public override void HandleNotification(INotification notification)
	{
		switch (notification.Body)
		{
			case NotificationName.MSG_MISSION_ABANDON:
				m_MissionTrackings = m_TaskTrackingProxy.GetAllTrackings();
				foreach (StarAreaElement item in m_PointDic.Values)
				{
					CheckMissionTeamInArea(item);
				}
				break;
		}
	}

	#region 新选中方式
	//开启主动吸附
	private bool m_OpenAdsorption;
	private bool m_AutoMoveing;
	private void OnMove(Vector2 value)
	{
		if (m_AutoMoveing)
		{
			return;
		}

		if (m_OpenAdsorption)
		{
			foreach (KeyValuePair<ulong, StarAreaElement> item in m_PointDic)
			{
				if (Vector2.Distance(item.Value.transform.position, Vector2.zero) <= 1)
				{
					m_OpenAdsorption = false;
					SetCurrentCell(item.Value, 0.2f);
					break;
				}
			}
		}
		else
		{
			if (Vector2.Distance(m_CurrentCell.transform.position, Vector2.zero) > 1)
			{
				m_OpenAdsorption = true;
				SetCurrentToNull();
			}
		}
	}


	#endregion

	#region HotkeyHandler
	private void OnLeap(HotkeyCallback callback)
	{
		if (callback.performed)
		{
			GameplayProxy proxy = GameFacade.Instance.RetrieveProxy(ProxyName.GameplayProxy) as GameplayProxy;
			SpacecraftEntity entity = proxy.GetEntityById<SpacecraftEntity>(proxy.GetMainPlayerUID());
			if (entity != null)
			{
				if (!entity.IsLeap())
				{
					(GameFacade.Instance.RetrieveProxy(ProxyName.AutoNavigationProxy) as AutoNavigationProxy)?
						.GoTo(m_CfgEternityProxy.GetCurrentGamingMapId(), MapManager.GetInstance().GetCurrentAreaUid(), m_Data.gamingmapId, m_CurrentCell.GetData().areaId);
					UIManager.Instance.ClosePanel(UIPanel.StarAreaPanel);
				}
			}
		}
	}


	/// <summary>
	/// 位置复位
	/// </summary>
	/// <param name="callback"></param>
	private void OnR3(HotkeyCallback callback)
	{
		if (callback.performed)
		{
			m_AreaPointContainer.DOLocalMove(Vector3.zero, 0.5F).SetEase(Ease.OutExpo);
		}
	}
	/// <summary>
	/// 返回上一级
	/// </summary>
	/// <param name="callback"></param>
	private void OnEsc(HotkeyCallback callback)
	{
		if (callback.performed)
		{
			MsgStarmapPanelState msg = new MsgStarmapPanelState();
			msg.Data = m_CfgStarmapProxy.GetFixedStarByTid(m_Data.fixedStarId);
			msg.BeforeID = m_Data.gamingmapId;
			UIManager.Instance.OpenPanel(UIPanel.StarPlantPanel, msg);
		}
	}
	/// <summary>
	/// 关闭星图面板
	/// </summary>
	/// <param name="callback"></param>
	private void OnCloseClick(HotkeyCallback callback)
	{
		if (callback.performed)
		{
			UIManager.Instance.ClosePanel(UIPanel.StarAreaPanel);
        }
	}
	#endregion

	#region 图例
	private bool m_LegendInit;
	private bool m_LegendHided;
	private void InitLegend()
	{
		if (m_LegendInit) return;
		m_LegendInit = true;

		RectTransform legendContainer = FindComponent<RectTransform>("Content/Legend");
		UIManager.Instance.GetUIElement(Leyoutech.Core.Loader.Config.AssetAddressKey.PRELOADUIELEMENT_STARLEGENDELEMENT,
			(cell) =>
			{
				foreach (StarmapLeapIcon id in Enum.GetValues(typeof(StarmapLeapIcon)))
				{
					GameObject go = cell.Spawn(legendContainer);
					UIUtil.SetIconImage(TransformUtil.FindUIObject<Image>(go.transform, "Content/Image_IconType"), m_CfgEternityProxy.GetIconIDByEnum(id));
					TransformUtil.FindUIObject<TMP_Text>(go.transform, "Content/Label_Name").text = TableUtil.GetLanguageString(id);
				}
				LayoutRebuilder.ForceRebuildLayoutImmediate(legendContainer);
			}
		);
	}

	private void OnTab(HotkeyCallback callback)
	{
		if (callback.performed)
		{
			m_LegendAnim.SetTrigger(m_LegendHided ? "Show" : "Hide");
			State.GetAction(UIAction.StarArea_Legend).State = m_LegendHided ? 0 : 1;
			m_LegendHided = !m_LegendHided;
		}
	}
	#endregion

	#region 任务
	private uint m_MissionTID;
	private ulong m_MissionUID;
	private void SelectedMission(MissionVO mission)
	{
		m_MissionUID = mission?.Uid ?? 0;
		if (mission != null && mission.MissionType != MissionType.Main)
		{
			m_MissionTID = mission.Tid;
		}
		else
		{
			m_MissionTID = 0;
		}
		State.GetAction(UIAction.StarArea_TrackMission).Enabled = m_MissionTID > 0;
		State.GetAction(UIAction.StarArea_DropMission).Enabled = m_MissionTID > 0;

		State.GetAction(UIAction.StarArea_TrackMission).State = m_MissionProxy.GetMissionTrack().IndexOf(m_MissionTID) < 0 ? 0 : 1;
	}

	private void OnTrackMission(HotkeyCallback callback)
	{
		if (callback.performed && m_MissionTID > 0)
		{
			if (m_MissionProxy.GetMissionTrack().IndexOf(m_MissionTID) < 0)
			{
				m_MissionProxy.AddMissionTrack(m_MissionTID);
				m_StarAreaPanelLeftTips.SetCellAnimVisiable(true);
				State.GetAction(UIAction.StarArea_TrackMission).State = 0;
			}
			else
			{
				m_MissionProxy.RemoveMissionTrack(m_MissionTID);
				m_StarAreaPanelLeftTips.SetCellAnimVisiable(false);
				State.GetAction(UIAction.StarArea_TrackMission).State = 1;
			}
		}
	}

	#region 任务放弃
	private void OnDropMission(HotkeyCallback cellback)
	{
		if (cellback.performed && m_MissionUID > 0)
		{
			NetworkManager.Instance.GetMissionController().SendDropMission(m_MissionUID);
		}
	}
	#endregion
	#endregion

	#region tips
	public void SetTips(object data)
	{
		State.SetTipData(data);
	}

	public void RefreshTipsData()
	{
		State.SetTipData(State.GetTipData());
	}

	private void OnToggleTips(HotkeyCallback callback)
	{
		if (callback.performed)
		{
			m_StarAreaPanelLeftTips.SelectNextCell();
		}
	}
	#endregion

}