using DG.Tweening;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static InputManager;
using static TaskTrackingProxy;

/// <summary>
/// 星系面板（子面板）
/// </summary>
public class StarGalaxyPart : BaseViewPart
{
	/// <summary>
	/// 资源路径
	/// </summary>
	private const string ASSET_ADDRESS = Leyoutech.Core.Loader.Config.AssetAddressKey.PRELOADUI_STARGALAXYPANEL;
	/// <summary>
	/// 游戏数据proxy
	/// </summary>
	private CfgEternityProxy m_CfgEternityProxy;
	/// <summary>
	/// 星图proxy
	/// </summary>
	private CfgStarmapProxy m_CfgStarmapProxy;
	/// <summary>
	/// 任务跟踪数据proxy
	/// </summary>
	private TaskTrackingProxy m_TaskTrackingProxy;
	/// <summary>
	/// 队伍proxy
	/// </summary>
	private TeamProxy m_TeamProxy;
	/// <summary>
	/// 星系根节点
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
	/// 所有星系数据存储
	/// </summary>
	private Dictionary<int, StarGalaxyElement> m_PointDic;
	/// <summary>
	/// 当前所在星系
	/// </summary>
	private StarGalaxyElement m_CurrentCell;
	/// <summary>
	/// 上次点的id
	/// </summary>
	private int m_BeforeID;
	/// <summary>
	/// 任务追踪数据
	/// </summary>
	private List<TrackingInfo> m_MissionTrackings;
	/// <summary>
	/// 队伍成员数据
	/// </summary>
	private List<TeamMemberVO> m_TeamMembers;
	private TMP_Text m_Title;

	public override void OnShow(object msg)
	{
		base.OnShow(msg);
		if (msg is MsgStarmapPanelState)
		{
			m_BeforeID = (int)(msg as MsgStarmapPanelState).BeforeID;
		}

		m_CfgEternityProxy = GameFacade.Instance.RetrieveProxy(ProxyName.CfgEternityProxy) as CfgEternityProxy;
		m_CfgStarmapProxy = GameFacade.Instance.RetrieveProxy(ProxyName.CfgStarmapProxy) as CfgStarmapProxy;
		m_TaskTrackingProxy = GameFacade.Instance.RetrieveProxy(ProxyName.TaskTrackingProxy) as TaskTrackingProxy;
		m_TeamProxy = GameFacade.Instance.RetrieveProxy(ProxyName.TeamProxy) as TeamProxy;
		LoadViewPart(ASSET_ADDRESS, OwnerView.OtherBox);
	}

	protected override void OnViewPartLoaded()
	{
		base.OnViewPartLoaded();
		RectTransform root = OwnerView.OtherBox.GetChild(0).GetComponent<RectTransform>();
		m_PointDic = new Dictionary<int, StarGalaxyElement>();
		m_Title = FindComponent<TMP_Text>(root, "Content/Title/Label_Title");
		m_AreaPointContainer = FindComponent<RectTransform>(root, "Content/MapList/Viewport/Content");
		m_Scroller = FindComponent<ScrollRect>(root, "Content/MapList");
		m_ScrollSensitivity = m_Scroller.scrollSensitivity;

		InputManager.Instance.OnInputDeviceChanged += OnInputDeviceChanged;
		State.GetAction(UIAction.Common_Select).Callback += OnSelect;
		State.GetAction(UIAction.Common_Back).Callback += OnCloseClick;
		OwnerView.AddHotKey(HotKeyID.StarmapOpen, OnCloseClick);

		OnRefresh();

        UIManager.Instance.ClosePanel(UIPanel.StarAreaPanel);
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
		State.GetAction(UIAction.Common_Select).Callback -= OnSelect;
		State.GetAction(UIAction.Common_Back).Callback -= OnCloseClick;
		OwnerView.DeleteHotKey(HotKeyID.StarmapOpen);
		InputManager.Instance.OnInputDeviceChanged -= OnInputDeviceChanged;
		foreach (KeyValuePair<int, StarGalaxyElement> item in m_PointDic)
		{
			item.Value.Destroy();
			item.Value.Recycle();
			item.Value.gameObject.Recycle();
		}
		m_PointDic.Clear();
		base.OnHide();
	}

	public void OnRefresh()
	{
		if (m_PointDic.Count == 0)
		{
			Eternity.FlatBuffer.Map mapData = m_CfgEternityProxy.GetCurrentMapData();
			m_MissionTrackings = m_TaskTrackingProxy.GetAllTrackings();
			m_TeamMembers = m_TeamProxy.GetMembersList();
			DrawMap(mapData.BelongFixedStar);
			OwnerView.FocusTo(m_CurrentCell.gameObject);
		}
		m_Title.text = TableUtil.GetLanguageString("mapstar_title_1001");
		m_AreaPointContainer.DOLocalMove(Vector3.zero, 0.5F).SetEase(Ease.OutExpo);
	}
	/// <summary>
	/// 绘制星系
	/// </summary>
	/// <param name="currentStarTid"></param>
	private void DrawMap(int currentStarTid)
	{
		int count = m_CfgStarmapProxy.GetFixedStarCount();
		for (int i = 0; i < count; i++)
		{
			EditorFixedStar data = m_CfgStarmapProxy.GetFixedStarByIndex(i);
			StarGalaxyElement cell =
				 (OwnerView as StarGalaxyPanel).CellPrefab.Spawn(m_AreaPointContainer, data.position.ToVector2())
				.GetOrAddComponent<StarGalaxyElement>();

			cell.GetComponent<Toggle>().group = m_AreaPointContainer.GetOrAddComponent<ToggleGroup>();
			cell.transform.localScale = Vector3.one;
			cell.transform.eulerAngles = Vector3.zero;
			cell.SetData(data, data.fixedStarId == currentStarTid, data.fixedStarId == m_BeforeID);
			CheckMissionTeamInStar(cell);
			m_PointDic.Add(data.fixedStarId, cell);
			cell.OnSelected = StarPointOnSelected;
			cell.OnClick = StarPointOnClick;
			if (data.fixedStarId == m_BeforeID)
			{
				m_CurrentCell = cell;
			}
		}
	}

	private void CheckMissionTeamInStar(StarGalaxyElement cell)
	{
		EditorPlanet[] planetData = cell.GetData().planetList;
		List<EditorStarMapArea> data = new List<EditorStarMapArea>();
		for (int i = 0; i < planetData?.Length; i++)
		{
			data.AddRange(planetData[i].arealist);
		}

		if (data.Count > 0 && (m_MissionTrackings?.Count > 0 || m_TeamMembers.Count > 0))
		{
			bool hasMain = false;
			bool hasTeamMembers = false;
			List<uint> s = new List<uint>();

			for (int i = 0; i < data.Count; i++)
			{
				for (int j = 0; !hasMain && j < m_MissionTrackings?.Count; j++)
				{
					if (m_MissionTrackings[j].EndingLeapPointID == data[i].areaId)
					{
						hasMain = m_MissionTrackings[j].MissionType == MissionType.Main;
					}
				}

				for (int k = 0; !hasTeamMembers && k < m_TeamMembers.Count; k++)
				{
					hasTeamMembers = (ulong)m_TeamMembers[k].AreaId == data[i].areaId;
				}

				if (hasMain && hasTeamMembers)//检查如果都有了 就不检查了
				{
					break;
				}
			}

			if (hasMain) { s.Add(31311); }
			if (hasTeamMembers) { s.Add(31316); }
			if (s.Count > 0)
			{
				cell.SetBottomIcon(s.ToArray());
				return;
			}
		}
		cell.SetBottomIcon(null);
	}

	/// <summary>
	/// 星域划过事件
	/// </summary>
	/// <param name="cell"></param>
	private void StarPointOnSelected(StarmapPointElementBase cell)
	{
		if (m_CurrentCell)
		{
			m_CurrentCell.SetToggleIsOn(false);
		}
		m_CurrentCell = cell as StarGalaxyElement;
		m_CurrentCell.SetToggleIsOn(true);
	}
	/// <summary>
	/// 星域点击事件
	/// </summary>
	/// <param name="cell"></param>
	private void StarPointOnClick(StarmapPointElementBase cell)
	{
		m_CurrentCell = cell as StarGalaxyElement;
		MsgStarmapPanelState msg = new MsgStarmapPanelState();
		msg.Data = m_CurrentCell.GetData();
		m_CurrentCell = null;
		UIManager.Instance.OpenPanel(UIPanel.StarPlantPanel,msg);
	}

	#region HotKeyHandler
	/// <summary>
	/// 选中按键
	/// </summary>
	/// <param name="callback"></param>
	private void OnSelect(HotkeyCallback callback)
	{
		if (callback.performed)
		{
			if (m_CurrentCell)
			{
				StarPointOnClick(m_CurrentCell);
			}
		}
	}
	/// <summary>
	/// 退出按键
	/// </summary>
	/// <param name="callback"></param>
	private void OnCloseClick(HotkeyCallback callback)
	{
		if (callback.performed)
		{
			UIManager.Instance.ClosePanel(UIPanel.StarGalaxyPanel);
		}
	}
	#endregion
}
