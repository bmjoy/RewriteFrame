//#define MissionDebug

using Eternity.FlatBuffer;
using PureMVC.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Model = Eternity.FlatBuffer.Model;

public class MissionDialogPanel : UIPanelBase
{
	private const string ASSET_ADDRESS = "Assets/Artwork/UI/Prefabs/MissionDialogPanel.prefab";

	private const string MISSION_NAME = "mission_name_";

	private const string MISSION_DESC = "mission_main_detailedDesc_";

	/// <summary>
	/// npcName
	/// </summary>
	private TMP_Text m_NpcName;
	/// <summary>
	/// npc描述
	/// </summary>
	private TMP_Text m_NpcDesc;
	/// <summary>
	/// 任务标识背景 任务
	/// </summary>
	private Image m_IconBg;

	/// <summary>
	/// 任务标识 任务icon
	/// </summary>
	private Image m_Icon;
	/// <summary>
	/// npc模型
	/// </summary>
	private RawImage m_UI3dImage;

	/// <summary>
	/// 任务名
	/// </summary>
	private TMP_Text m_MissionNameLabel;
	/// <summary>
	/// 任务描述容器
	/// </summary>
	private RectTransform m_DescContainer;
	/// <summary>
	/// 操作确认面板上的Icon集合
	/// </summary>
	private List<AbstractIconBase> m_IconList = new List<AbstractIconBase>();
	/// <summary>
	/// 任务描述文字
	/// </summary>
	private TMP_Text m_DescLabel;
	/// <summary>
	/// 任务目标容器
	/// </summary>

	private RectTransform m_TemplateRect;
	private RectTransform m_TargetParentRect;

	/// <summary>
	/// 任务奖励容器
	/// </summary>
	private RectTransform m_RewardRec;
	/// <summary>
	/// 任务奖励容器
	/// </summary>
	private RectTransform m_RewardsContainer;
	/// <summary>
	/// hotkey挂点
	/// </summary>
	private Transform m_MissionHotKeyParent;

	/// <summary>
	/// layout列表
	/// 需要动态刷新的
	/// </summary>
	private List<RectTransform> m_LayeroutTrans;

	private CfgLanguageProxy m_CfgLanguageProxy;

	private CfgEternityProxy m_CfgEternityProxy;

	private MissionVO m_MissionVO;

	private Transform m_NpcTransform;
	private Dictionary<int, int> m_RewardInfo = new Dictionary<int, int>(5);

	public MissionDialogPanel() : base(UIPanel.MissionDialogPanel, ASSET_ADDRESS, PanelType.Normal) { }


	public override void Initialize()
	{
		m_CfgLanguageProxy = Facade.RetrieveProxy(ProxyName.CfgLanguageProxy) as CfgLanguageProxy;
		m_CfgEternityProxy = GameFacade.Instance.RetrieveProxy(ProxyName.CfgEternityProxy) as CfgEternityProxy;

		m_NpcName = FindComponent<TMP_Text>("Content/NpcDescBar/NpcDesc/Label_Name");
		m_NpcDesc = FindComponent<TMP_Text>("Content/NpcDescBar/NpcDesc/Label_Introduction");
		m_LayeroutTrans = new List<RectTransform>();
		m_IconBg = FindComponent<Image>("Content/MissionContent/TitleLine/Icon/Image_Icon");
		m_Icon = FindComponent<Image>("Content/MissionContent/TitleLine/Icon/Border1");
		m_UI3dImage = FindComponent<RawImage>("NPC");
		m_MissionNameLabel = FindComponent<TMP_Text>("Content/MissionContent/TitleLine/Label_Name");
		m_DescContainer = FindComponent<RectTransform>("Content/MissionContent/MissionMessage/Describe");
		//m_DescBrifLabel = FindComponent<TMP_Text>("Content/MissionContent/MissionMessage/Describe/Label_Title");
		m_DescLabel = FindComponent<TMP_Text>("Content/MissionContent/MissionMessage/Describe/Label_Describe");
		m_TargetParentRect = FindComponent<RectTransform>("Content/MissionContent/MissionMessage/Target");
		m_TemplateRect = FindComponent<RectTransform>("Content/MissionContent/MissionMessage/Target/Target");
		m_RewardRec = FindComponent<RectTransform>("Content/MissionContent/MissionMessage/Reward");
		m_RewardsContainer = FindComponent<RectTransform>("Content/MissionContent/MissionMessage/Reward/IconItem");
		m_MissionHotKeyParent = FindComponent<Transform>("Control/HotKeyList");
	}

	public override NotificationName[] ListNotificationInterests()
	{
		return new NotificationName[]
		{
			NotificationName.MSG_MISSION_COMMIT
		};
	}

	public override void HandleNotification(INotification notification)
	{
		switch (notification.Name)
		{
			case NotificationName.MSG_MISSION_COMMIT:
				OpenChat(notification.Body as MsgMissionInfo);
				break;
		}
	}

	public override void OnShow(object msg)
	{
		base.OnShow(msg);
		RefreshHotKeyState(false);
		MissionOpenMsgInfo data = (MissionOpenMsgInfo)msg;
		m_MissionVO = data.MissionVO;
		m_NpcTransform = data.NpcTransform;
	}

	public override void OnRefresh(object msg)
	{
		RefreshHotKeyState(true);
		SetData();
	}

	public override void OnGotFocus()
	{
		base.OnGotFocus();
		if (m_MissionVO.MissionState == MissionState.CanAccept)
		{
			AddHotKey("f", HotKeyID.FuncA, F_OnClick, m_MissionHotKeyParent, TableUtil.GetLanguageString("mission_hotkey_id_1001"));
		}
		else
		{
			AddHotKey("f", HotKeyID.FuncA, F_OnClick, m_MissionHotKeyParent, TableUtil.GetLanguageString("mission_hotkey_id_1004"));
		}
		AddHotKey(HotKeyID.FuncB, OnEscClose, m_MissionHotKeyParent, TableUtil.GetLanguageString("common_hotkey_id_1002"));

	}

	public override void OnHide(object msg)
	{
		RetrieveIcon();
		base.OnHide(msg);
		m_IconList.Clear();
	}


	private void SetData()
	{
		if (m_MissionVO.MissionState == MissionState.CanAccept)
		{
			ShowNpcModel(m_MissionVO.MissionMainConfig.Value.AcceptNpcId);
		}
		else
		{
			ShowNpcModel(m_MissionVO.MissionMainConfig.Value.SubmitNpcId);
		}

		m_MissionNameLabel.text = TableUtil.GetLanguageString(MISSION_NAME + m_MissionVO.Tid);
		m_DescLabel.text = TableUtil.GetLanguageString(MISSION_DESC + m_MissionVO.Tid);

		UpdateTarget();//目标
		UpdateIcon();//图标
		AddRewardItemIcon();//奖励
	}

	private void UpdateIcon()
	{
		uint IconId = (uint)m_CfgEternityProxy.GetMissionIconIdBy(m_MissionVO.MissionType);
		Icon icon = m_CfgEternityProxy.GetIconName(IconId);
		UIUtil.SetIconImage(m_IconBg, icon.Atlas, icon.SquareName);
	}

	private void UpdateTarget()
	{
		int index = 0;
		foreach (KeyValuePair<uint, SortedDictionary<uint, MissionTargetVO>> group in m_MissionVO.MissionGroupTargetList)
		{
			RectTransform row = (index + 2) < m_TargetParentRect.childCount
				? m_TargetParentRect.GetChild(index + 2).GetComponent<RectTransform>()
				: UnityEngine.Object.Instantiate(m_TemplateRect, m_TargetParentRect);

			if (index != 0)
			{
				row.gameObject.SetActive(m_MissionVO.MissionState != MissionState.Finished);
				UpdateMissionRow("", "", row, true, false);
				index++;
				row = (index + 2) < m_TargetParentRect.childCount
									? m_TargetParentRect.GetChild(index + 2).GetComponent<RectTransform>()
									: UnityEngine.Object.Instantiate(m_TemplateRect, m_TargetParentRect);
			}
			bool frist = true;
			foreach (KeyValuePair<uint, MissionTargetVO> target in group.Value)
			{
				if (!target.Value.DoneToFinish)
				{
					continue;
				}
				row = (index + 2) < m_TargetParentRect.childCount
					? m_TargetParentRect.GetChild(index + 2).GetComponent<RectTransform>()
					: UnityEngine.Object.Instantiate(m_TemplateRect, m_TargetParentRect);
				string str = string.Empty;
				if (target.Value.DoneToFinish)//正常条件
				{
					switch (target.Value.MissionTargetType)
					{
						case MissionTargetType.Kill:
						case MissionTargetType.CollectItem:
						case MissionTargetType.HaveItem:
							str = $"[{target.Value.Value}/{target.Value.MissionTargetConfig.Arg2}]";
							break;
						case MissionTargetType.Escort:
						default:
							str = target.Value.TargetState == MissionState.Finished ? "[1/1]" : "[0/1]";
							break;
					}
				}

				row.gameObject.SetActive(true);
				UpdateMissionRow(TableUtil.GetMissionTargetDesc(target.Value.Tid), str, row, false, frist && m_MissionVO.MissionState == MissionState.CanAccept);
				index++;
				frist = false;
			}
		}

		for (int i = index + 2; i < m_TargetParentRect.childCount; i++)
		{
			m_TargetParentRect.GetChild(i).gameObject.SetActive(false);
		}
	}

	private void UpdateMissionRow(string targetFieldText, string numberFieldText, RectTransform view, bool isShowOr, bool isFirstInGroup)
	{
		RectTransform normalBox = view.Find("TargetNormal").GetComponent<RectTransform>();
		TMP_Text targetFieldNormal = view.Find("TargetNormal/Left/Label_Target").GetComponent<TMP_Text>();
		TMP_Text numberFieldNormal = view.Find("TargetNormal/Label_Num").GetComponent<TMP_Text>();
		Transform orBox = view.Find("TargetNormal/Label_or");
		Transform pointBox = view.Find("TargetNormal/Left/Image_point/Image");
		RectTransform grayBox = view.Find("TargetGray").GetComponent<RectTransform>();
		TMP_Text targetFieldGray = view.Find("TargetGray/Left/Label_Target").GetComponent<TMP_Text>();
		RectTransform pointBoxGray = view.Find("TargetGray/Left/Image_point/Image").GetComponent<RectTransform>();
		TMP_Text numberFieldGray = view.Find("TargetGray/Label_Num").GetComponent<TMP_Text>();
		TMP_Text orText = orBox.GetComponent<TMP_Text>();
		string orString = TableUtil.GetLanguageString("mission_title_1010");
		orText.text = orString;
		orBox.gameObject.SetActive(isShowOr);
		pointBox.gameObject.SetActive(isFirstInGroup);
		pointBoxGray.gameObject.SetActive(isFirstInGroup);
		targetFieldNormal.gameObject.SetActive(!isShowOr);
		numberFieldNormal.gameObject.SetActive(!isShowOr);
		if (isShowOr)
		{
			normalBox.gameObject.SetActive(isShowOr);
			grayBox.gameObject.SetActive(!isShowOr);
			return;
		}

		targetFieldNormal.text = targetFieldGray.text = targetFieldText;
		numberFieldNormal.text = numberFieldText;
		normalBox.gameObject.SetActive(m_MissionVO.MissionState == MissionState.CanAccept);
		grayBox.gameObject.SetActive(m_MissionVO.MissionState != MissionState.CanAccept);
	}

	/// <summary>
	/// 添加奖励icon
	/// </summary>
	private void AddRewardItemIcon()
	{
		MissionMain cfg = m_MissionVO.MissionMainConfig.Value;

		if (cfg.ItemId1 > 0 && !m_RewardInfo.ContainsKey(cfg.ItemId1)) m_RewardInfo.Add(cfg.ItemId1, cfg.ItemNum1);
		if (cfg.ItemId2 > 0 && !m_RewardInfo.ContainsKey(cfg.ItemId2)) m_RewardInfo.Add(cfg.ItemId2, cfg.ItemNum2);
		if (cfg.ItemId3 > 0 && !m_RewardInfo.ContainsKey(cfg.ItemId3)) m_RewardInfo.Add(cfg.ItemId3, cfg.ItemNum3);
		if (cfg.ItemId4 > 0 && !m_RewardInfo.ContainsKey(cfg.ItemId4)) m_RewardInfo.Add(cfg.ItemId4, cfg.ItemNum4);
		if (cfg.ItemId5 > 0 && !m_RewardInfo.ContainsKey(cfg.ItemId5)) m_RewardInfo.Add(cfg.ItemId5, cfg.ItemNum5);

		foreach (var item in m_RewardInfo)
		{
			if (item.Value > 0 && item.Key > 0)
			{
				Item it = m_CfgEternityProxy.GetItemByKey((uint)item.Key);
				IconManager.Instance.LoadItemIcon<IconCommon>(IconConstName.ICON_COMMON, m_RewardsContainer,
					(icon) =>
					{
						m_IconList.Add(icon);
						icon.SetData(it.Icon, it.Quality, item.Value);
					}
				);
			}
		}
	}

	#region 设置当前角色模型

	/// <summary>
	/// 设置npc模型
	/// </summary>
	/// <param name="tid">模型ID</param>
	private void ShowNpcModel(int npcid)
	{
		Npc npc = m_CfgEternityProxy.GetNpcByKey((uint)npcid);
		m_NpcName.text = TableUtil.GetNpcName((uint)npcid);
		m_NpcDesc.text = TableUtil.GetNpcDesc((uint)npcid);
		m_UI3dImage.gameObject.SetActive(false);
		CfgEternityProxy cfe = GameFacade.Instance.RetrieveProxy(ProxyName.CfgEternityProxy) as CfgEternityProxy;
		string prefabName = cfe.GetNpcModelByKey((uint)npcid);
		Model NpcModel = m_CfgEternityProxy.GetModel(npc.Model);
		Npc3DViewer npc3DViewer = m_UI3dImage.GetComponent<Npc3DViewer>();
		if (npc3DViewer == null)
		{
			npc3DViewer = m_UI3dImage.gameObject.AddComponent<Npc3DViewer>();
		}
		npc3DViewer.SetModel(prefabName, GetNpcUiPos(NpcModel), GetNpcUiRotation(NpcModel), GetNpcUiScale(NpcModel));

	}

	private Vector3 GetNpcUiPos(Eternity.FlatBuffer.Model NpcTemplateVO)
	{
		if (NpcTemplateVO.UiPositionLength == 3)
		{
			return new Vector3(NpcTemplateVO.UiPosition(0), NpcTemplateVO.UiPosition(1), NpcTemplateVO.UiPosition(2));
		}
		return Vector3.zero;
	}

	private Vector3 GetNpcUiRotation(Model NpcTemplateVO)
	{
		if (NpcTemplateVO.UiPositionLength == 3)
		{
			return new Vector3(NpcTemplateVO.UiRotation(0), NpcTemplateVO.UiRotation(1), NpcTemplateVO.UiRotation(2));
		}
		return Vector3.zero;
	}

	private Vector3 GetNpcUiScale(Model NpcTemplateVO)
	{
		if (NpcTemplateVO.UiScale > 0)
		{
			return NpcTemplateVO.UiScale * Vector3.one;
		}
		return Vector3.one;
	}
	#endregion

	/// <summary>
	/// 重置hotkey按钮状态机
	/// </summary>
	/// <param name="callbackContext"></param>
	private void RefreshHotKeyState(bool active)
	{
		m_MissionHotKeyParent.gameObject.SetActive(active);

		SetHotKeyEnabled("f", active);
		SetHotKeyEnabled(HotKeyID.FuncB, active);

	}

	private IEnumerator Excute(float seconds, Action callBack)
	{
		yield return new WaitForSeconds(seconds);
		callBack();
	}
	/// <summary>
	/// esc按键
	/// </summary>
	/// <param name="callbackContext"></param>
	private void OnEscClose(HotkeyCallback callbackContext)
	{
		if (callbackContext.performed)
		{
			Close(false);
		}
	}

	/// <summary>
	/// f键
	/// </summary>
	/// <param name="callbackContext"></param>
	private void F_OnClick(HotkeyCallback callbackContext)
	{
		if (callbackContext.performed)
		{
			if (m_MissionVO.MissionState == MissionState.CanAccept)
			{
				NetworkManager.Instance.GetMissionController().SendAcceptMission(m_MissionVO.Tid);
				Close();
			}
			else if (m_MissionVO.MissionState == MissionState.Finished)
			{
				NetworkManager.Instance.GetMissionController().SendSubmitMission(m_MissionVO.Uid);
#if MissionDebug
				Debug.LogError("C2S 交任务:" + m_MissionVO.Tid);
#endif
			}
		}
	}
	/// <summary>
	/// 回收Icon
	/// </summary>
	private void RetrieveIcon()
	{
		for (int i = 0; i < m_IconList.Count; i++)
		{
			if (m_IconList[i] != null)
			{
				IconManager.Instance.RetrieveIcon(m_IconList[i]);
			}
		}
		m_IconList.Clear();
		m_RewardInfo.Clear();
	}

	/// <summary>
	/// 关闭
	/// </summary>
	private void Close(bool needOpenChat = false)
	{
		UIManager.Instance.ClosePanel(this);
	}

	private void OpenChat(MsgMissionInfo info)
	{
		if (info.MissionTid != m_MissionVO.Tid)
		{
			return;
		}

		uint chatid = (uint)m_CfgEternityProxy.GetMissionMainByKey(m_MissionVO.Tid).SubmitDialog;
		if (chatid != 0)
		{
			DialogueInfo msg = new DialogueInfo();
			msg.DialogueTid = chatid;
			msg.SoundParent = m_NpcTransform;
			GameFacade.Instance.SendNotification(NotificationName.MSG_DIALOGUE_SHOW, msg);
		}
		Close();
	}

}
