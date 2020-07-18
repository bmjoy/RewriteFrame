using Assets.Scripts.Define;
using Eternity.FlatBuffer;
using PureMVC.Interfaces;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HudNpcInteractiveFlagPanel : HudBase
{
	private const string ASSET_ADDRESS = Leyoutech.Core.Loader.Config.AssetAddressKey.PRELOADUI_HUD_NPCINTERACTIVEFLAGPANEL;

	/// <summary>
	/// 根结点
	/// </summary>
	private RectTransform m_Root;
	/// <summary>
	/// 画布相机
	/// </summary>
	private Camera m_Camera;
	/// <summary>
	/// 人形交互提示容器
	/// </summary>
	private RectTransform m_HumanBox;
	/// <summary>
	/// 船形交互提示容器
	/// </summary>
	private RectTransform m_ShipBox;
	/// <summary>
	/// 其它提示容器
	/// </summary>
	private RectTransform m_OtherTipBox;
	/// <summary>
	/// 任务护送提示容器
	/// </summary>
	private RectTransform m_MissionEscortBox;
	/// <summary>
	/// 任务护送提示容器 自动关闭计数
	/// </summary>
	private int m_MissionEscortDelayNum;
	/// <summary>
	/// 当前交互NPC的TID
	/// </summary>
	private int m_FocusNpcTid;
	/// <summary>
	/// 当前交互NPC偏移点
	/// </summary>
	private Vector3 m_FocusNpcOffset;
    /// <summary>
    /// 当前交互NPC配置
    /// </summary>
    private Npc m_FocusNpcVO;
    /// <summary>
    /// 当前提示类型
    /// </summary>
    private Dictionary<InteractiveTipType, bool> m_CurrentTipTypes = new Dictionary<InteractiveTipType, bool>();

	/// <summary>
	/// 无论如何 以 F 方式显示交互
	/// </summary>
	private bool m_MustUseHumanFBox;

	public enum InteractiveTipType
	{
		//采集
		Collector,
		//宝箱
		Precious,
		//提示右键开锁
		LockChest
	}

	public HudNpcInteractiveFlagPanel() : base(UIPanel.HudNpcInteractiveFlagPanel, ASSET_ADDRESS, PanelType.Hud) { }

	public override void Initialize()
	{
		m_Root = GetTransform().GetComponent<RectTransform>();
		m_Camera = m_Root.GetComponentInParent<Canvas>().worldCamera;

		m_HumanBox = FindComponent<RectTransform>("Content/Human");
		m_ShipBox = FindComponent<RectTransform>("Content/Ship");
		m_OtherTipBox = FindComponent<RectTransform>("Content/Others");
		m_MissionEscortBox = FindComponent<RectTransform>("Content/Others/CommonMessage");
	}

	public override void OnShow(object msg)
	{
		base.OnShow(msg);

		m_FocusNpcTid = 0;
		m_FocusNpcOffset = Vector3.zero;
		m_CurrentTipTypes.Clear();

		RebuildView();

		StartUpdate();
	}

	public override NotificationName[] ListNotificationInterests()
	{
		return new NotificationName[]
		{
			NotificationName.MSG_CHANGE_BATTLE_STATE,
			NotificationName.MSG_INTERACTIVE_SHOWFLAG,
			NotificationName.MSG_INTERACTIVE_HIDEFLAG,
			NotificationName.MSG_INTERACTIVE_SHOWTIP,
			NotificationName.MSG_INTERACTIVE_HIDETIP,
			NotificationName.MSG_INTERACTIVE_MISSIONESCORT,
		};
	}

	public override void HandleNotification(INotification notification)
	{
		switch (notification.Name)
		{
			case NotificationName.MSG_CHANGE_BATTLE_STATE:
				RebuildView();
				break;
			case NotificationName.MSG_INTERACTIVE_SHOWFLAG:
				OnNpcSelect(notification.Body as MsgInteractiveInfo);
				break;
			case NotificationName.MSG_INTERACTIVE_HIDEFLAG:
				OnNpcUnselect();
				break;
			case NotificationName.MSG_INTERACTIVE_SHOWTIP:
				OnShowTip((InteractiveTipType)notification.Body);
				break;
			case NotificationName.MSG_INTERACTIVE_HIDETIP:
				OnHideTip((InteractiveTipType)notification.Body);
				break;
			case NotificationName.MSG_INTERACTIVE_MISSIONESCORT:
				SetMissionEscortBox((bool)notification.Body);
				break;

		}
	}

	/// <summary>
	/// NPC选中时
	/// </summary>
	/// <param name="npcID">交互信息</param>
	private void OnNpcSelect(MsgInteractiveInfo msg)
	{
		int npcID = (int)msg.Tid;
		string describe = msg.Describe;
		m_MustUseHumanFBox = msg.MustUseHumanFBox;
		if (m_FocusNpcTid != npcID)
		{
			m_FocusNpcTid = npcID;
			m_FocusNpcOffset = Vector3.zero;

			CfgEternityProxy eternity = Facade.RetrieveProxy(ProxyName.CfgEternityProxy) as CfgEternityProxy;

            m_FocusNpcVO = eternity.GetNpcByKey((uint)m_FocusNpcTid);
			if (m_FocusNpcVO.FPosOffestLength >= 3)
			{
				m_FocusNpcOffset = new Vector3(m_FocusNpcVO.FPosOffest(0), m_FocusNpcVO.FPosOffest(1), m_FocusNpcVO.FPosOffest(2));
			}
		}
		RebuildView(describe);
	}

	/// <summary>
	/// NPC取消选择时
	/// </summary>
	private void OnNpcUnselect()
	{
		if (m_FocusNpcTid != 0)
		{
			m_FocusNpcTid = 0;
			RebuildView();
		}
	}

	/// <summary>
	/// 显示提示
	/// </summary>
	/// <param name="templateID">模板ID</param>
	private void OnShowTip(InteractiveTipType templateID)
	{
		if (!m_CurrentTipTypes.ContainsKey(templateID))
		{
			m_CurrentTipTypes.Add(templateID, true);
			RebuildView();
		}
		else
		{
			if (!m_CurrentTipTypes[templateID])
			{
				m_CurrentTipTypes[templateID] = true;
				RebuildView();
			}
		}
	}

	/// <summary>
	/// 隐藏提示
	/// </summary>
	private void OnHideTip(InteractiveTipType templateID)
	{
		if (m_CurrentTipTypes.ContainsKey(templateID) && m_CurrentTipTypes[templateID])
		{
			m_CurrentTipTypes[templateID] = false;
			RebuildView();
		}
	}

	/// <summary>
	/// 重建视图
	/// </summary>
	private void RebuildView(string describe = null)
	{
		DeleteHotKey("f");

		bool isBattle = IsBattling();

		HotKeyMapID hotKeyMapID = IsInSpace() ? HotKeyMapID.SHIP : HotKeyMapID.HUMAN;
		string hotKeyID = m_MustUseHumanFBox ? HotKeyID.LeapStart : IsInSpace() ? HotKeyID.InteractiveForShip : HotKeyID.InteractiveForHuman;
		RectTransform box = (!IsInSpace() || m_MustUseHumanFBox) ? m_HumanBox : m_ShipBox;
		string text = !string.IsNullOrEmpty(describe) ? TableUtil.GetLanguageString(describe) : "Interactoin";
		HotkeyManager.HotkeyStyle hotkeyStyle = (!IsInSpace() || m_MustUseHumanFBox) ? HotkeyManager.HotkeyStyle.HUMAN_INTERACTIVE : HotkeyManager.HotkeyStyle.SHIP_INTERACTIVE;

		if (m_FocusNpcTid != 0)
		{
            float time = 0;
            switch (m_FocusNpcVO.InteractionType)
            {
                case 0: time = 0.0f; break;
                case 1: time = 0.0f; break;
                case 2: time = 0.0f; break;
                case 3: time = 0.6f; break;
            }

            AddHotKey("f", hotKeyMapID, hotKeyID, OnHotKeyCallbackByHold, time, box, text, hotkeyStyle);
        }

		bool tipShowed = false;
		for (int i = 0; i < m_OtherTipBox.childCount; i++)
		{
			Transform child = m_OtherTipBox.GetChild(i);
			InteractiveTipType tipType = (InteractiveTipType)i;
			bool enabled = false;
			if (tipType == InteractiveTipType.LockChest)
			{
				enabled =/* m_FocusNpcTid == 0 &&*/ !tipShowed /*&& !isBattle*/ && m_CurrentTipTypes.ContainsKey(tipType) && m_CurrentTipTypes[tipType];
			}
			else
			{
				enabled = m_FocusNpcTid == 0 && !tipShowed && !isBattle && m_CurrentTipTypes.ContainsKey(tipType) && m_CurrentTipTypes[tipType];
			}

			child.gameObject.SetActive(enabled);
			if (enabled)
			{
				tipShowed = true;
				Transform hotkeyBox = child.Find("HotKey");
				if (hotkeyBox != null)
				{
					AddHotKey("f", hotKeyMapID, HotKeyID.ShipSwitchMode, OnHotKeyCallbackByTip, hotkeyBox, string.Empty, HotkeyManager.HotkeyStyle.UI_SIMPLE);
				}
			}
		}
	}
	/// <summary>
	/// 设置任务护送框
	/// </summary>
	/// <param name="isShow"></param>
	public void SetMissionEscortBox(bool isShow)
	{
		m_MissionEscortDelayNum = isShow ? 10 : 0;
		if (m_MissionEscortBox.gameObject.activeSelf == isShow)
		{
			return;
		}
		m_MissionEscortBox.gameObject.SetActive(isShow);
	}
	/// <summary>
	/// 收到提示相关联的热键回调时
	/// </summary>
	/// <param name="info"></param>
	private void OnHotKeyCallbackByTip(HotkeyCallback info)
	{
	}

    /// <summary>
    /// 收到交互相关联的热键回调时
    /// </summary>
    /// <param name="info"></param>
    private void OnHotKeyCallbackByHold(HotkeyCallback info)
    {
        if (info.performed)
        {
            UIManager.Instance.StartCoroutine(DelayExecute(0.5f));
        }
    }

    /// <summary>
    /// 延迟执行
    /// </summary>
    /// <returns></returns>
    private IEnumerator DelayExecute(float time)
	{
		yield return new WaitForSeconds(time);

		if (IsAllowInteractive())
			Interaction.InteractionManager.GetInstance().OnInteracted();
	}

	/// <summary>
	/// 更新交互面板
	/// </summary>
	protected override void Update()
	{
		if (m_MissionEscortBox.gameObject.activeSelf)
		{
			m_MissionEscortBox.gameObject.SetActive(--m_MissionEscortDelayNum > 0);
		}


		bool isInSpace = IsInSpace();
		bool allowInteractive = m_FocusNpcTid > 0 && IsAllowInteractive();
		bool allowTip = !IsDead() && !IsLeaping() && !IsWatchOrUIInputMode();

		m_HumanBox.gameObject.SetActive(allowInteractive && (m_MustUseHumanFBox || !isInSpace));
		m_ShipBox.gameObject.SetActive(allowInteractive && !m_MustUseHumanFBox && isInSpace);
		m_OtherTipBox.gameObject.SetActive(allowTip);

		if (m_HumanBox.gameObject.activeSelf)
		{
			GameplayProxy gameplayProxy = GameFacade.Instance.RetrieveProxy(ProxyName.GameplayProxy) as GameplayProxy;
			Transform focusNpc = gameplayProxy.GetSkinTransformByTemplateID(m_FocusNpcTid, KHeroType.htNpc);
			if (focusNpc != null)
			{
				Vector3 targetPosition = focusNpc.position + focusNpc.TransformDirection(m_FocusNpcOffset);
				Vector3 viewportPoint = Camera.main.WorldToViewportPoint(targetPosition);
				if (viewportPoint.x >= 0 && viewportPoint.y >= 0 && viewportPoint.x <= 1 && viewportPoint.y <= 1 && viewportPoint.z >= Camera.main.nearClipPlane)
				{
					Vector2 iconPosition;
					if (RectTransformUtility.ScreenPointToLocalPointInRectangle(m_Root, Camera.main.WorldToScreenPoint(targetPosition), m_Camera, out iconPosition))
					{
						m_HumanBox.gameObject.SetActive(true);
						m_HumanBox.anchoredPosition = new Vector2(iconPosition.x, iconPosition.y);
						return;
					}
				}
			}
		}
	}

	/// <summary>
	/// 是否允许交互
	/// </summary>
	/// <returns></returns>
	private bool IsAllowInteractive()
	{
		return !IsDead() && !IsBattling() && !IsLeaping() && !IsWatchOrUIInputMode();
	}

}
