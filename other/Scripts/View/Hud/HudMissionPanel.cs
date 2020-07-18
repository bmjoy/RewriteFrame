using PureMVC.Interfaces;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class HudMissionPanel : HudBase
{
	private const string ASSET_ADDRESS = Leyoutech.Core.Loader.Config.AssetAddressKey.PRELOADUI_HUD_MISSIONPANEL;

	private RectTransform m_RootNode;
	private RectTransform m_MainMissionAnimator;
	private RectTransform m_BranchMissionAnimator;
	private RectTransform m_DailyMissionAnimator;

	public HudMissionPanel() : base(UIPanel.HudMissionPanel, ASSET_ADDRESS, PanelType.Hud) { }

	public override void Initialize()
	{
		m_RootNode = FindComponent<RectTransform>("Content");
		m_MainMissionAnimator = FindComponent<RectTransform>("Content/LeftMission/MainMission");
		m_BranchMissionAnimator = FindComponent<RectTransform>("Content/LeftMission/SideMission");
		m_DailyMissionAnimator = FindComponent<RectTransform>("Content/LeftMission/DailyMission");
	}

	public override void OnShow(object msg)
	{
		base.OnShow(msg);

		m_MainMissionAnimator.gameObject.SetActive(false);
		m_BranchMissionAnimator.gameObject.SetActive(false);
		m_DailyMissionAnimator.gameObject.SetActive(false);

		UpdateMissionList();
	}

	public override void OnHide(object msg)
	{
		base.OnHide(msg);
	}

	public override NotificationName[] ListNotificationInterests()
	{
		return new NotificationName[]
		{
			NotificationName.MSG_MISSION_ACCEPT,
			NotificationName.MSG_MISSION_COMMIT,
			NotificationName.MSG_MISSION_ABANDON,
			NotificationName.MSG_MISSION_CHANGE,
			NotificationName.MSG_MISSION_STATE_CHANGE,

			NotificationName.MSG_CHANGE_BATTLE_STATE,
			NotificationName.MainHeroDeath,
			NotificationName.MainHeroRevive,
		};
	}

	public override void HandleNotification(INotification notification)
	{
		switch (notification.Name)
		{
			case NotificationName.MSG_MISSION_ACCEPT:
				OnMissionAccepted();
				break;
			case NotificationName.MSG_MISSION_COMMIT:
				OnMissionFinished();
				break;
			case NotificationName.MSG_MISSION_ABANDON:
			case NotificationName.MSG_MISSION_CHANGE:
				UpdateMissionList();
				break;
			case NotificationName.MSG_CHANGE_BATTLE_STATE:
			case NotificationName.MainHeroDeath:
			case NotificationName.MainHeroRevive:
				OnStateChanged();
				break;
			case NotificationName.MSG_MISSION_STATE_CHANGE:
				OnTargetFinish();
				break;
		}
	}

	/// <summary>
	/// 输入模式改变时
	/// </summary>
	protected override void OnInputMapChanged()
	{
		OnStateChanged();
	}

	#region 视图处理

	/// <summary>
	/// 任务接受时
	/// </summary>
	private void OnMissionAccepted()
	{
		WwiseUtil.PlaySound(WwiseManager.voiceComboID, WwiseMusicSpecialType.SpecialType_Voice_GetTheTask, WwiseMusicPalce.Palce_1st, false, null);

		UpdateMissionList();
	}

	/// <summary>
	/// 任务完成时
	/// </summary>
	private void OnMissionFinished()
	{
		WwiseUtil.PlaySound(WwiseManager.voiceComboID, WwiseMusicSpecialType.SpecialType_Voice_FinishTheTask, WwiseMusicPalce.Palce_1st, false, null);

		UpdateMissionList();
	}

	/// <summary>
	/// 目标完成时
	/// </summary>
	private void OnTargetFinish()
	{
		WwiseUtil.PlaySound(WwiseManager.voiceComboID, WwiseMusicSpecialType.SpecialType_Voice_TaskTargetUpdate, WwiseMusicPalce.Palce_1st, false, null);
	}

	/// <summary>
	/// 状态改变时
	/// </summary>
	private void OnStateChanged()
	{
		//if (!IsInSpace())
		//    m_RootNode.gameObject.SetActive(true);
		//else
		m_RootNode.gameObject.SetActive((!IsDead() && !IsLeaping() && !IsWatchOrUIInputMode()));
	}

	/// <summary>
	/// 打开任务列表
	/// </summary>
	private void UpdateMissionList()
	{
		OnStateChanged();
		m_TimeTextDic.Clear();
		MissionProxy missionProxy = Facade.RetrieveProxy(ProxyName.MissionProxy) as MissionProxy;
		Dictionary<ulong, MissionVO> mainMissionDic = missionProxy.AllMissionDic[MissionType.Main];
		MissionVO mainMission = null;//显示主线 或者 待接主线（可不可接无所谓）
		if (mainMissionDic.Count > 0)
		{
			foreach (var item in mainMissionDic)
			{
				mainMission = item.Value;
				break;
			}
		}
		else
		{
			Dictionary<uint, MissionVO> canAcceptList = missionProxy.GetCanAcceptMissions();
			if (canAcceptList != null && canAcceptList.Count > 0)
			{
				foreach (var item in canAcceptList)
				{
					if (item.Value.MissionType == MissionType.Main)
					{
						mainMission = item.Value;
						break;
					}
				}
			}
		}

		List<uint> trackMissionTids = missionProxy.GetMissionTrack();
		List<MissionVO> trackBranchMissions = new List<MissionVO>();
		List<MissionVO> trackDailyMissions = new List<MissionVO>();
		//TODO gaoyu
		/* 这里要在下面的临时代码 去掉后 打开它
		for (int i = 0; i < trackMissionTids?.Count; i++)
		{
			MissionVO mission = missionProxy.GetMissionBy(trackMissionTids[i]);
			switch (mission.MissionType)
			{
				case MissionType.Branch:
					trackBranchMissions.Add(mission);
					break;
				case MissionType.Daily:
					trackDailyMissions.Add(mission);
					break;
			}
		}
		*/
		//HACK gaoyu
		//这是一个临时需求 by 张子华
		#region 任务 临时支线任务
		Dictionary<ulong, MissionVO> branchMission = missionProxy.AllMissionDic[MissionType.Branch];
		if (branchMission?.Count > 0)
		{
			trackBranchMissions.Add(branchMission.Values.ToList()[0]);
		}
		#endregion

		m_MainMissionAnimator.gameObject.SetActive(mainMission != null);
		m_BranchMissionAnimator.gameObject.SetActive(trackBranchMissions.Count > 0);
		m_DailyMissionAnimator.gameObject.SetActive(trackDailyMissions.Count > 0);

		if (mainMission != null)
		{
			UpdateMissionList(mainMission, FindComponent<RectTransform>("Content/LeftMission/MainMission"));
		}

		for (int i = 0; i < trackBranchMissions.Count; i++)
		{
			UpdateMissionList(trackBranchMissions[i], FindComponent<RectTransform>("Content/LeftMission/SideMission"));
		}

		for (int i = 0; i < trackDailyMissions.Count; i++)
		{
			UpdateMissionList(trackDailyMissions[i], FindComponent<RectTransform>("Content/LeftMission/DailyMission"));
		}
		CheckTime();
	}

	/// <summary>
	/// 更新任务列表
	/// </summary>
	private void UpdateMissionList(MissionVO mission, RectTransform view)
	{
		TMP_Text rowTitle = view.Find("TitleLine/Label_Name").GetComponent<TMP_Text>();
		RectTransform rowBox = view.Find("Content").GetComponent<RectTransform>();
		RectTransform rowFinish = view.Find("Label_Finish").GetComponent<RectTransform>();
		RectTransform rowTemplate = rowBox.GetChild(0).GetComponent<RectTransform>();

		rowTitle.text = TableUtil.GetLanguageString("mission_name_" + mission.Tid);
		rowBox.gameObject.SetActive(mission.MissionState != MissionState.Finished);
		rowFinish.gameObject.SetActive(mission.MissionState == MissionState.Finished);

		CfgEternityProxy cfgEternityProxy = GameFacade.Instance.RetrieveProxy(ProxyName.CfgEternityProxy) as CfgEternityProxy;

		bool finised = true;
		int index = 0;
		if (mission.MissionState == MissionState.CanAccept)
		{
			string str = string.Empty;
			if (GetRoleLevel() < mission.MissionConfig.LvLimit)
			{
				str = string.Format(TableUtil.GetLanguageString("mission_title_1011"), mission.MissionConfig.LvLimit);
			}
			else if (mission.MissionMainConfig.Value.AcceptNpcId > 0)
			{
				str = TableUtil.GetNpcName((uint)mission.MissionMainConfig.Value.AcceptNpcId);
				str = string.Format(TableUtil.GetLanguageString("mission_title_1012"), str);
			}
			RectTransform row = index < rowBox.childCount ? rowBox.GetChild(index).GetComponent<RectTransform>() : Object.Instantiate(rowTemplate, rowBox);
			row.gameObject.SetActive(true);
			UpdateMissionRow(str, string.Empty, row, true, false, false);
			finised = false;
			index++;
		}
		else
		{
			foreach (KeyValuePair<uint, SortedDictionary<uint, MissionTargetVO>> group in mission.MissionGroupTargetList)
			{
				RectTransform row = index < rowBox.childCount ? rowBox.GetChild(index).GetComponent<RectTransform>() : Object.Instantiate(rowTemplate, rowBox);
				if (index != 0 && !finised)
				{
					row.gameObject.SetActive(true);
					UpdateMissionRow(string.Empty, string.Empty, row, false, true, false);
					index++;
					row = index < rowBox.childCount ? rowBox.GetChild(index).GetComponent<RectTransform>() : Object.Instantiate(rowTemplate, rowBox);
				}
				bool frist = true;
				foreach (KeyValuePair<uint, MissionTargetVO> target in group.Value)
				{
					if (!target.Value.DoneToFinish)
					{
						continue;
					}
					row = index < rowBox.childCount ? rowBox.GetChild(index).GetComponent<RectTransform>() : Object.Instantiate(rowTemplate, rowBox);
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
						if (target.Value.TargetState == MissionState.Failed)
						{
							str = TableUtil.GetLanguageString("mission_title_1013");
						}
					}

					row.gameObject.SetActive(true);
					row.name = target.Value.Tid + "  " + TableUtil.GetMissionTargetDesc(target.Value.Tid);
					if (target.Value.MissionTargetConfig.Arg4 > 0 && target.Value.Relation != null)
					{
						if (target.Value.Relation.MissionTargetType == MissionTargetType.TimeOut)
						{
							UpdateMissionRow(TableUtil.GetMissionTargetDesc(target.Value.Tid), str, row, false, false, frist, target.Value.TargetState, (ulong)target.Value.Relation.Value);
						}
					}
					else
					{
						UpdateMissionRow(TableUtil.GetMissionTargetDesc(target.Value.Tid), str, row, false, false, frist, target.Value.TargetState);
					}
					finised = target.Value.TargetState == MissionState.Finished;
					index++;
					frist = false;
				}
			}
		}

		for (int i = index; i < rowBox.childCount; i++)
		{
			rowBox.GetChild(i).gameObject.SetActive(false);
		}
	}

	/// <summary>
	/// 任务行（target）数据
	/// </summary>
	/// <param name="targetFieldText"></param>
	/// <param name="numberFieldText"></param>
	/// <param name="view"></param>
	/// <param name="isCan"></param>
	/// <param name="isShowOr"></param>
	/// <param name="isFirstInGroup"></param>
	/// <param name="missionState"></param>
	private void UpdateMissionRow(string targetFieldText, string numberFieldText, RectTransform view, bool isCan, bool isShowOr, bool isFirstInGroup, MissionState missionState = MissionState.None, ulong deadline = 0)
	{
		//正常进行
		Transform normalBox = view.Find("TargetNormal");
		TMP_Text targetFieldNormal = view.Find("TargetNormal/Left/Label_Target").GetComponent<TMP_Text>();
		TMP_Text numberFieldNormal = view.Find("TargetNormal/Label_Num").GetComponent<TMP_Text>();
		TMP_Text timeFieldNormal = view.Find("TargetNormal/Label_time").GetComponent<TMP_Text>();
		Transform orBox = view.Find("TargetNormal/Label_or");
		Transform pointBox = view.Find("TargetNormal/Left/Image_point/Image");
		//灰色 已经完成
		Transform grayBox = view.Find("TargetGray");
		TMP_Text targetFieldGray = view.Find("TargetGray/Left/Label_Target").GetComponent<TMP_Text>();
		Transform pointBoxGray = view.Find("TargetGray/Left/Image_point/Image");
		TMP_Text numberFieldGray = view.Find("TargetGray/Label_Num").GetComponent<TMP_Text>();
		//失败了的
		Transform failBox = view.Find("TargetFail");
		TMP_Text targetFieldFail = view.Find("TargetFail/Left/Label_Target").GetComponent<TMP_Text>();
		TMP_Text numberFieldFail = view.Find("TargetFail/Label_Num").GetComponent<TMP_Text>();
		Transform pointBoxFail = view.Find("TargetFail/Left/Image_point/Image");


		orBox.gameObject.SetActive(isShowOr);
		pointBox.gameObject.SetActive(!isShowOr && isFirstInGroup);
		pointBoxGray.gameObject.SetActive(!isShowOr && isFirstInGroup);
		pointBoxFail.gameObject.SetActive(!isShowOr && isFirstInGroup);
		targetFieldNormal.gameObject.SetActive(!isShowOr);
		numberFieldNormal.gameObject.SetActive(!isShowOr);
		if (isShowOr)
		{
			normalBox.gameObject.SetActive(isShowOr);
			grayBox.gameObject.SetActive(!isShowOr);
			failBox.gameObject.SetActive(!isShowOr);
			timeFieldNormal.gameObject.SetActive(false);
			return;
		}
		if (isCan)
		{
			normalBox.gameObject.SetActive(true);
			grayBox.gameObject.SetActive(false);
			failBox.gameObject.SetActive(false);
			targetFieldNormal.gameObject.SetActive(true);
			numberFieldNormal.gameObject.SetActive(false);
			timeFieldNormal.gameObject.SetActive(false);
			targetFieldNormal.text = targetFieldGray.text = targetFieldText;
		}
		else
		{
			targetFieldFail.text = targetFieldNormal.text = targetFieldGray.text = targetFieldText;
			numberFieldFail.text = numberFieldNormal.text = numberFieldText;
			normalBox.gameObject.SetActive(missionState != MissionState.Finished && missionState != MissionState.Failed);
			grayBox.gameObject.SetActive(missionState == MissionState.Finished);
			failBox.gameObject.SetActive(missionState == MissionState.Failed);
			if (missionState == MissionState.Going && deadline > ServerTimeUtil.Instance.GetNowTime())
			{
				m_TimeTextDic.Add(timeFieldNormal, deadline);
				timeFieldNormal.gameObject.SetActive(true);
				CheckShowTime(timeFieldNormal, deadline);
			}
			else
			{
				timeFieldNormal.gameObject.SetActive(false);
			}
		}
	}

	private Dictionary<TMP_Text, ulong> m_TimeTextDic = new Dictionary<TMP_Text, ulong>();

	private void CheckTime()
	{
		ServerTimeUtil.Instance.OnTick -= Tick;
		if (m_TimeTextDic.Count > 0)
		{
			ServerTimeUtil.Instance.OnTick += Tick;
		}
	}

	private List<KeyValuePair<TMP_Text, ulong>> timeTextList;
	private void Tick()
	{
		if (GetGameObject().activeInHierarchy && m_TimeTextDic.Count > 0)
		{
			timeTextList = m_TimeTextDic.ToList();
			foreach (var item in timeTextList)
			{
				CheckShowTime(item.Key, item.Value);
				timeTextList = m_TimeTextDic.ToList();
			}
		}
	}

	private void CheckShowTime(TMP_Text text, ulong deadline)
	{
		if (ServerTimeUtil.Instance.GetNowTime() < deadline)
		{
			text.text = $"[{TimeUtil.GetTimeStr((long)deadline - (long)ServerTimeUtil.Instance.GetNowTime())}]";
			text.gameObject.SetActive(true);
		}
		else
		{
			text.gameObject.SetActive(false);
			m_TimeTextDic.Remove(text);
		}
	}


	private uint GetRoleLevel()
	{
		GameplayProxy gameplayProxy = GameFacade.Instance.RetrieveProxy(ProxyName.GameplayProxy) as GameplayProxy;
		BaseEntity mainPlayer = gameplayProxy.GetEntityById<BaseEntity>(gameplayProxy.GetMainPlayerUID());
		return mainPlayer?.GetLevel() ?? 1;
	}

	#endregion
}
