using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using static TaskTrackingProxy;
using TMPro;

/// <summary>
/// 行星面板（子面板）
/// </summary>
public class StarPlantPart : BaseViewPart
{
	/// <summary>
	/// 资源地址
	/// </summary>
	private const string ASSET_ADDRESS = Leyoutech.Core.Loader.Config.AssetAddressKey.PRELOADUI_STARPLANTPANEL;
	/// <summary>
	/// 游戏数据proxy
	/// </summary>
	private CfgEternityProxy m_CfgEternityProxy;
	/// <summary>
	/// 任务跟踪数据proxy
	/// </summary>
	private TaskTrackingProxy m_TaskTrackingProxy;
	/// <summary>
	/// 队伍proxy
	/// </summary>
	private TeamProxy m_TeamProxy;
	/// <summary>
	/// 行星挂点
	/// </summary>
	private RectTransform m_AreaPointContainer;
	/// <summary>
	/// 行星模型显示图
	/// </summary>
	private Starmap3DViewer m_StarImage;
	/// <summary>
	/// 所有行星数据
	/// </summary>
	private Dictionary<uint, StarPlantElement> m_PointDic;
	/// <summary>
	/// 当前星系数据
	/// </summary>
	private EditorFixedStar m_Data;
	/// <summary>
	/// 上一次选中的
	/// </summary>
	private uint m_BeforeID;
	/// <summary>
	/// 当前选中的行星
	/// </summary>
	private StarPlantElement m_CurrentPoint;
	/// <summary>
	/// 任务追踪数据
	/// </summary>
	private List<TrackingInfo> m_MissionTrackings;
	/// <summary>
	/// 队伍成员数据
	/// </summary>
	private List<TeamMemberVO> m_TeamMembers;

	private CanvasGroup m_CanvasGroup;
	private TMP_Text m_Title;

	public override void OnShow(object msg)
	{
		base.OnShow(msg);
		m_CfgEternityProxy = GameFacade.Instance.RetrieveProxy(ProxyName.CfgEternityProxy) as CfgEternityProxy;
		m_TaskTrackingProxy = GameFacade.Instance.RetrieveProxy(ProxyName.TaskTrackingProxy) as TaskTrackingProxy;
		m_TeamProxy = GameFacade.Instance.RetrieveProxy(ProxyName.TeamProxy) as TeamProxy;
		if (msg is MsgStarmapPanelState)
		{
			m_Data = (msg as MsgStarmapPanelState).Data as EditorFixedStar;
			m_BeforeID = (uint)(msg as MsgStarmapPanelState).BeforeID;
		}
		LoadViewPart(ASSET_ADDRESS, OwnerView.OtherBox);
	}

	protected override void OnViewPartLoaded()
	{
		base.OnViewPartLoaded();
		RectTransform root = OwnerView.OtherBox.GetChild(0).GetComponent<RectTransform>();
		m_PointDic = new Dictionary<uint, StarPlantElement>();
		m_Title = FindComponent<TMP_Text>(root, "Content/Title/Label_Title");
		//m_CanvasGroup = FindComponent<CanvasGroup>(root, "Content");
		m_AreaPointContainer = FindComponent<RectTransform>(root, "Content/MapList/Viewport/Content");
		m_StarImage = FindComponent<RawImage>(root, "Content/StarImage").GetOrAddComponent<Starmap3DViewer>();

		m_Tweener = null;
		//m_CanvasGroup.alpha = 1;
		m_StarImage.enabled = false;

		State.GetAction(UIAction.Common_Select).Callback += OnSelect;
		State.GetAction(UIAction.StarPlant_ToGalaxy).Callback += OnEsc;
		//State.GetAction(UIAction.StarMap_Close).Callback += OnCloseClick;
		OwnerView.AddHotKey(HotKeyID.StarmapOpen, OnCloseClick);

		OnRefresh();

        UIManager.Instance.ClosePanel(UIPanel.StarAreaPanel);
        UIManager.Instance.ClosePanel(UIPanel.StarGalaxyPanel);
	}

	public override void OnHide()
	{
		State.GetAction(UIAction.Common_Select).Callback -= OnSelect;
		State.GetAction(UIAction.StarPlant_ToGalaxy).Callback -= OnEsc;
		//State.GetAction(UIAction.StarMap_Close).Callback -= OnCloseClick;
		OwnerView.DeleteHotKey(HotKeyID.StarmapOpen);
		m_Tweener = null;
		foreach (KeyValuePair<uint, StarPlantElement> item in m_PointDic)
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
		if (m_Data != null)
		{
			m_StarImage.SetModel(m_Data.fixedStarRes, Vector3.zero, Vector3.zero, Vector3.one);
			m_MissionTrackings = m_TaskTrackingProxy.GetAllTrackings();
			m_TeamMembers = m_TeamProxy.GetMembersList();

			if (m_BeforeID == 0)
			{
				m_BeforeID = m_CfgEternityProxy.GetCurrentGamingMapId();
			}
			DrawMap(m_CfgEternityProxy.GetCurrentGamingMapId(), m_BeforeID);

			if (m_CurrentPoint)
			{
				OwnerView.FocusTo(m_CurrentPoint.gameObject);
			}
			m_Title.text = TableUtil.GetLanguageString($"starmap_name_{m_Data.fixedStarId}");
		}
	}
	/// <summary>
	/// 绘制行星
	/// </summary>
	/// <param name="currentStarTid"></param>
	private void DrawMap(uint currentStarTid, uint before)
	{
		for (int i = 0; i < m_Data.planetList.Length; i++)
		{
			StarPlantElement cell =
				(OwnerView as StarPlantPanel).CellPrefab.Spawn(m_AreaPointContainer, m_Data.planetList[i].position.ToVector2())
				.GetOrAddComponent<StarPlantElement>();

			cell.transform.localScale = Vector3.one;
			cell.GetComponent<Toggle>().group = m_AreaPointContainer.GetOrAddComponent<ToggleGroup>();
			cell.SetData(m_Data.planetList[i], m_Data.planetList[i].gamingmapId == currentStarTid, m_Data.planetList[i].gamingmapId == before);
			CheckMissionTeamInPlant(cell);
			m_PointDic.Add(m_Data.planetList[i].gamingmapId, cell);
			cell.OnSelected = StarPointOnSelected;
			cell.OnClick = StarPointOnClick;
			cell.OnEnter = StartPointOnEnter;
			if (m_Data.planetList[i].gamingmapId == before)
			{
				m_CurrentPoint = cell;
			}
		}
	}

	private void CheckMissionTeamInPlant(StarPlantElement cell)
	{
		EditorStarMapArea[] data = cell.GetData().arealist;

		if (data?.Length > 0 && (m_MissionTrackings?.Count > 0 || m_TeamMembers.Count > 0))
		{
			bool hasMain = false;
			bool hasBranch = false;
			bool hasTeamMembers = false;
			List<uint> s = new List<uint>();

			for (int i = 0; i < data.Length; i++)
			{
				if (!hasMain || !hasBranch)//检查如果主线支线都有了 就不检查任务相关的了
				{
					for (int j = 0; j < m_MissionTrackings?.Count; j++)
					{
						if (m_MissionTrackings[j].EndingLeapPointID == data[i].areaId)
						{
							if (m_MissionTrackings[j].MissionType == MissionType.Main)
							{
								hasMain = true;
							}
							else if (m_MissionTrackings[j].MissionType == MissionType.Branch)
							{
								hasBranch = true;
							}
						}
					}
				}

				for (int k = 0; !hasTeamMembers && k < m_TeamMembers.Count; k++)
				{
					hasTeamMembers = (ulong)m_TeamMembers[k].AreaId == data[i].areaId;
				}

				if (hasMain && hasBranch && hasTeamMembers)//检查如果都有了 就不检查了
				{
					break;
				}
			}

			if (hasMain) { s.Add(31311); }
			if (hasBranch) { s.Add(31312); }
			if (hasTeamMembers) { s.Add(31316); }
			if (s.Count > 0)
			{
				cell.SetBottomIcon(s.ToArray());
				return;
			}
		}
		cell.SetBottomIcon(null);
	}

	private void StartPointOnEnter(StarmapPointElementBase cell)
	{
		StarPointOnSelected(cell);
	}

	/// <summary>
	/// 行星选中
	/// </summary>
	/// <param name="cell"></param>
	private void StarPointOnSelected(StarmapPointElementBase cell)
	{
		if (m_CurrentPoint)
		{
			m_CurrentPoint.SetToggleIsOn(false);
		}
		m_CurrentPoint = cell as StarPlantElement;
		m_CurrentPoint.SetToggleIsOn(true);


	}

	private Tweener m_Tweener;
	/// <summary>
	/// 行星点击事件
	/// </summary>
	/// <param name="cell"></param>
	private void StarPointOnClick(StarmapPointElementBase cell)
	{
		if (m_Tweener != null)
		{
			return;
		}

		cell.SetToggleIsOn(false);

		m_CurrentPoint = cell as StarPlantElement;
		MsgStarmapPanelState msg = new MsgStarmapPanelState();
		msg.Data = m_CurrentPoint.GetData();
		m_CurrentPoint = null;

		m_Tweener = cell.transform.DOMove(CameraManager.GetInstance().GetUICameraComponent().transform.position, 1f).SetEase(Ease.OutCirc)
			.OnComplete(
			() =>
			{
				m_Tweener = null;
				UIManager.Instance.OpenPanel(UIPanel.StarAreaPanel, msg);
			});
	}

	#region HotkeyHandler
	/// <summary>
	/// 选中按键
	/// </summary>
	/// <param name="callback"></param>
	private void OnSelect(HotkeyCallback callback)
	{
		if (callback.performed)
		{
			if (m_CurrentPoint)
			{
				StarPointOnClick(m_CurrentPoint);
			}
		}
	}
	/// <summary>
	/// 退出返回上一级
	/// </summary>
	/// <param name="callback"></param>
	private void OnEsc(HotkeyCallback callback)
	{
		if (callback.performed)
		{
			MsgStarmapPanelState msg = new MsgStarmapPanelState();
			msg.BeforeID = (ulong)m_Data.fixedStarId;
			UIManager.Instance.OpenPanel(UIPanel.StarGalaxyPanel, msg);
		}
	}
	/// <summary>
	/// 关闭星图界面
	/// </summary>
	/// <param name="callback"></param>
	private void OnCloseClick(HotkeyCallback callback)
	{
		if (callback.performed)
		{
			UIManager.Instance.ClosePanel(UIPanel.StarPlantPanel);
        }
	}
	#endregion

}