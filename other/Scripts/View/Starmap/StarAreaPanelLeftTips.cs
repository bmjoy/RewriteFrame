using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static TaskTrackingProxy;

/// <summary>
/// 星域tips
/// </summary>
public class StarAreaPanelLeftTips : MonoBehaviour
{
	private GameObject m_CellPrefab;
	/// <summary>
	/// 容器
	/// </summary>
	private RectTransform m_ListCellContainer;
	/// <summary>
	/// 星域名称
	/// </summary>
	private TMP_Text m_NameLabel;
	/// <summary>
	/// 星域类型
	/// </summary>
	private TMP_Text m_TypeLabel;
	/// <summary>
	/// 星域描述
	/// </summary>
	private TMP_Text m_DescLabel;
	/// <summary>
	/// 主跃迁图片
	/// </summary>
	private Image m_Image1;
	/// <summary>
	/// 普通跃迁图片
	/// </summary>
	private Image m_Image2;
	/// <summary>
	/// 是否已经初始化
	/// </summary>
	private bool m_Inited;

	private ToggleGroup m_ToggleGroup;

	private List<(GameObject cell, uint icon, string text, object tipsData)> m_TempData;

	private List<GameObject> m_Cells;

	private GameObject m_CurrentSelectedCell;

	private StarAreaPart m_AreaPart;

	/// <summary>
	/// 初始化控件
	/// </summary>
	public void Initialize(StarAreaPart part)
	{
		if (m_Inited)
		{
			return;
		}
		m_Inited = true;
		m_AreaPart = part;
		m_Image1 = TransformUtil.FindUIObject<Image>(transform, "Content/NameType/Image_Icon1");
		m_Image2 = TransformUtil.FindUIObject<Image>(transform, "Content/NameType/Image_Icon2");

		m_NameLabel = TransformUtil.FindUIObject<TMP_Text>(transform, "Content/NameType/Label_Name");
		m_TypeLabel = TransformUtil.FindUIObject<TMP_Text>(transform, "Content/NameType/Label_Type");
		m_DescLabel = TransformUtil.FindUIObject<TMP_Text>(transform, "Content/Describe/Label_Describe");

		m_ListCellContainer = TransformUtil.FindUIObject<RectTransform>(transform, "Content/List/Viewport/Content");
		m_ToggleGroup = m_ListCellContainer.GetOrAddComponent<ToggleGroup>();
		m_ToggleGroup.allowSwitchOff = true;
		UIManager.Instance.GetUIElement(Leyoutech.Core.Loader.Config.AssetAddressKey.PRELOADUIELEMENT_STARAREALISTELEMENT,
			(cell) =>
			{
				m_CellPrefab = cell;
			}
		);
		m_Cells = new List<GameObject>();
		m_TempData = new List<(GameObject, uint, string, object)>();
	}
	/// <summary>
	/// 设置tips数据显示
	/// </summary>
	/// <param name="gamingmapId"></param>
	/// <param name="data"></param>
	public void SetData(uint gamingmapId, EditorStarMapArea data, List<TrackingInfo> missionTrackData, List<TeamMemberVO> teamMembersData)
	{
		m_AreaPart.SetTips(null);
		m_NameLabel.text = TableUtil.GetLanguageString($"area_name_{gamingmapId}_{data.areaId}");
		m_DescLabel.text = TableUtil.GetLanguageString($"description_leap_name_{gamingmapId}_{data.areaId}");
		m_TypeLabel.text = TableUtil.GetLanguageString($"starmap_areatype_{data.area_leap_type}");

		m_Image1.enabled = data.area_leap_type == 1;
		m_Image2.enabled = data.area_leap_type == 2;

		m_TempData.Clear();

		for (int i = 0; i < missionTrackData?.Count; i++)
		{
			if (missionTrackData[i].EndingLeapPointID == data.areaId)
			{
				if (missionTrackData[i].MissionType == MissionType.Main)
				{
					m_TempData.Add((null, 31311, TableUtil.GetLanguageString("mission_name_" + missionTrackData[i].MissionTid), missionTrackData[i].MissionVO));
				}
				else if (missionTrackData[i].MissionType == MissionType.Branch)
				{
					m_TempData.Add((null, 31312, TableUtil.GetLanguageString("mission_name_" + missionTrackData[i].MissionTid), missionTrackData[i].MissionVO));
				}
			}
		}
		for (int i = 0; i < teamMembersData?.Count; i++)
		{
			if ((ulong)teamMembersData[i].AreaId == data.areaId)
			{
				m_TempData.Add((null, 31316, teamMembersData[i].Name, teamMembersData[i]));
			}
		}


		for (int i = 0; i < m_Cells.Count; i++)
		{
			m_Cells[i].SetActive(false);
			m_Cells[i].GetComponent<Toggle>().onValueChanged.RemoveAllListeners();
			m_Cells[i].GetComponent<Toggle>().isOn = false;
		}

		MissionSelected(null);

		for (int i = 0; i < m_TempData.Count; i++)
		{
			GameObject go;
			object tipsData = m_TempData[i].tipsData;
			if (i < m_Cells.Count)
			{
				go = m_Cells[i];
			}
			else
			{
				go = GetCell();
				m_Cells.Add(go);
			}
			m_TempData[i] = (go, m_TempData[i].icon, m_TempData[i].text, m_TempData[i].tipsData);
			go.GetComponent<Toggle>().onValueChanged.AddListener(
				(ison) =>
				{
					m_CurrentSelectedCell = ison ? go : null;
					if (ison)
					{
						m_AreaPart.SetTips(tipsData);
						switch (tipsData)
						{
							case MissionVO val:
								MissionSelected(val);
								break;
							default:
								MissionSelected(null);
								break;
						}
					}
					else
					{
						MissionSelected(null);
						m_AreaPart.SetTips(null);
					}
					go.GetComponent<Animator>().SetBool("IsOn", ison);
				}
			);
			UIUtil.SetIconImage(TransformUtil.FindUIObject<Image>(go.transform, "Content/Icon"), m_TempData[i].icon);
			TransformUtil.FindUIObject<TMP_Text>(go.transform, "Content/Label_Name").text = m_TempData[i].text;
			go.SetActive(true);
		}
	}

	private GameObject GetCell()
	{
		GameObject go = m_CellPrefab.Spawn(m_ListCellContainer);
		go.GetComponent<Toggle>().group = m_ToggleGroup;
		TransformUtil.FindUIObject<Transform>(go.transform, "Content/Tracking").gameObject.SetActive(false);
		return go;
	}

	public void SetCellAnimVisiable(bool value)
	{
		TransformUtil.FindUIObject<Transform>(m_CurrentSelectedCell.transform, "Content/Tracking").gameObject.SetActive(value);
		m_AreaPart.RefreshTipsData();
	}

	public void SelectNextCell()
	{
		if (m_TempData.Count == 0) return;
		if (!m_CurrentSelectedCell)
		{
			m_TempData[0].cell.GetComponent<Toggle>().isOn = true;
		}
		else
		{
			m_CurrentSelectedCell.GetComponent<Toggle>().isOn = false;
			int i = 0;
			for (; i < m_TempData.Count; i++)
			{
				if (m_CurrentSelectedCell == m_TempData[i].cell)
				{
					i++;
					if (i < m_TempData.Count)
					{
						m_TempData[i].cell.GetComponent<Toggle>().isOn = true;
					}
					else
					{
						m_TempData[0].cell.GetComponent<Toggle>().isOn = true;
					}
					return;
				}
			}
		}
	}

	public void OnHide()
	{
		MissionSelected = null;
		for (int i = 0; i < m_Cells.Count; i++)
		{
			m_Cells[i].SetActive(false);
			m_Cells[i].GetComponent<Toggle>().onValueChanged.RemoveAllListeners();
			m_Cells[i].GetComponent<Toggle>().isOn = false;
		}
	}

	public bool HasToggle()
	{
		return m_TempData.Count > 0;
	}

	public UnityAction<MissionVO> MissionSelected { get; set; }


}
