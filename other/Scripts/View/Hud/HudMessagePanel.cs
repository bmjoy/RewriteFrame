using Leyoutech.Core.Loader.Config;
using PureMVC.Interfaces;
using System.Collections.Generic;
using UnityEngine;

public class HudMessagePanel : UIPanelBase
{
	private const string ASSET_ADDRESS = AssetAddressKey.PRELOADUI_HUD_MESSAGEPANEL;

	/// <summary>
	/// 是否已暂傍
	/// </summary>
	private bool m_Paused = true;
	/// <summary>
	/// 恢复协程
	/// </summary>
	private Coroutine m_ResumeCoroutine;
	/// <summary>
	/// 消息列表
	/// </summary>
	private static List<MessageData> m_MessageList = new List<MessageData>();
	/// <summary>
	/// 根节点
	/// </summary>
	private RectTransform m_RootNode;
	/// <summary>
	/// 当前消息
	/// </summary>
	private MessageData m_CurrentMessage;
	/// <summary>
	/// 当前消息视图
	/// </summary>
	private Transform m_CurrentMessageView;
	/// <summary>
	/// 当前动画事件
	/// </summary>
	private UIAnimationEvent m_CurrentAnimationEvent;

	public HudMessagePanel() : base(UIPanel.HudMessagePanel, ASSET_ADDRESS, PanelType.Dialugue) { }

	public override void Initialize()
	{
		m_RootNode = FindComponent<RectTransform>("Content");
	}

	public override void OnShow(object msg)
	{
		base.OnShow(msg);

		m_Paused = true;

		ResumeMessageAnimation(1.0f);
	}

	public override NotificationName[] ListNotificationInterests()
	{
		return new NotificationName[]
		{
			NotificationName.MSG_SWITCH_SCENE_START,
			NotificationName.MSG_SWITCH_SCENE_END,

			NotificationName.MSG_MISSION_ACCEPT,
			NotificationName.MSG_MISSION_COMMIT,
			NotificationName.MSG_MISSION_FAIL,
			NotificationName.MSG_MISSION_STATE_CHANGE,
			NotificationName.MSG_PLAYER_LEVEL_UP,
			NotificationName.MSG_PLAYER_SHIP_LEVEL_UP,
		};
	}
	/*
    /// <summary>
    /// Debug函数
    /// </summary>
    protected override void Update()
    {
        if(Input.GetKeyDown(KeyCode.Alpha1))
        {
            Facade.SendNotification(NotificationName.MSG_MISSION_ACCEPT);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            Facade.SendNotification(NotificationName.MSG_MISSION_COMMIT);
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            Facade.SendNotification(NotificationName.MSG_MISSION_FAIL);
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            Facade.SendNotification(NotificationName.MSG_PLAYER_LEVEL_UP, 1);
        }
        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            Facade.SendNotification(NotificationName.MSG_PLAYER_SHIP_LEVEL_UP, 2);
        }
    }*/

	public override void HandleNotification(INotification notification)
	{
		int priority = 0;
		string prefabPath = string.Empty;
		object data = notification.Body;

		switch (notification.Name)
		{
			//逻辑消息
			case NotificationName.MSG_SWITCH_SCENE_START:
				PauseMessageAniamtion();
				break;
			case NotificationName.MSG_SWITCH_SCENE_END:
				ResumeMessageAnimation();
				break;

			//动画消息
			case NotificationName.MSG_MISSION_ACCEPT:
				prefabPath = AssetAddressKey.PRELOADUI_HUD_MISSIONSTATEACCEPT;
				break;
			case NotificationName.MSG_MISSION_COMMIT:
			case NotificationName.MSG_MISSION_STATE_CHANGE:
				prefabPath = AssetAddressKey.PRELOADUI_HUD_MISSIONSTATEFINISH;
				break;
			case NotificationName.MSG_MISSION_FAIL:
				prefabPath = AssetAddressKey.PRELOADUI_HUD_MISSIONSTATEFAIL;
				break;
			case NotificationName.MSG_PLAYER_LEVEL_UP:
				//priority = 2;
				prefabPath = AssetAddressKey.PRELOADUI_HUD_UPLEVELROLE;
				break;
			case NotificationName.MSG_PLAYER_SHIP_LEVEL_UP:
				//priority = 2;
				prefabPath = AssetAddressKey.PRELOADUI_HUD_UPLEVELSHIP;
				break;
		}

		if (!string.IsNullOrEmpty(prefabPath))
		{
			AddMessage(new MessageData() { type = notification.Name, priority = priority, prefabPath = prefabPath, argument = data });
		}
	}

	/// <summary>
	/// 初始化消息动画
	/// </summary>
	private void InitializeMessageAnimation()
	{
		switch (m_CurrentMessage.type)
		{
			case NotificationName.MSG_MISSION_ACCEPT:
			case NotificationName.MSG_MISSION_FAIL:
				break;
			case NotificationName.MSG_MISSION_COMMIT:
				{
					UIIconAndLabel label = m_CurrentAnimationEvent.GetComponent<UIIconAndLabel>();
					if (label && label.Label)
					{
						label.Label.text = TableUtil.GetLanguageString("mission_title_1005");
					}
				}
				break;
			case NotificationName.MSG_MISSION_STATE_CHANGE:
				{
					UIIconAndLabel label = m_CurrentAnimationEvent.GetComponent<UIIconAndLabel>();
					if (label && label.Label)
					{
						label.Label.text = TableUtil.GetLanguageString("mission_title_1015");
					}
				}
				break;
			case NotificationName.MSG_PLAYER_LEVEL_UP:
				{
					UIIconAndLabel label = m_CurrentAnimationEvent.GetComponent<UIIconAndLabel>();
					if (label && label.Label)
					{
						label.Label.text = m_CurrentMessage.argument != null ? "Lv." + m_CurrentMessage.argument.ToString() : string.Empty;
					}
				}
				break;
			case NotificationName.MSG_PLAYER_SHIP_LEVEL_UP:
				{
					MsgShipLevelUp msg = (MsgShipLevelUp)m_CurrentMessage.argument;
					UIIconAndLabel shipLabel = m_CurrentAnimationEvent.GetComponent<UIIconAndLabel>();
					if (shipLabel && shipLabel.Label)
					{
						shipLabel.Label.text = m_CurrentMessage.argument != null ? "Lv." + msg.m_level : string.Empty;
					}
					if (shipLabel && shipLabel.Icon)
					{
						UIUtil.SetIconImage(shipLabel.Icon, TableUtil.GetItemIconTid(msg.m_Tid));
					}
					if (shipLabel && shipLabel.Info)
					{
						string name = TableUtil.GetItemName(msg.m_Tid);
						shipLabel.Info.text = string.Format(TableUtil.GetLanguageString("hud_text_id_1036"), name);
					}
				}
				break;
		}
	}

	/// <summary>
	/// 暂停消息动画
	/// </summary>
	private void PauseMessageAniamtion()
	{
		if (m_ResumeCoroutine != null)
		{
			UIManager.Instance.StopCoroutine(m_ResumeCoroutine);
			m_ResumeCoroutine = null;
		}

		if (m_CurrentAnimationEvent)
		{
			m_CurrentAnimationEvent.OnAnimationEvent -= OnMessagePlayComplete;
			m_CurrentAnimationEvent = null;
		}

		if (m_CurrentMessageView)
		{
			m_CurrentMessageView.Recycle();
			m_CurrentMessageView = null;
		}

		m_CurrentMessage = null;
		m_Paused = true;
	}

	/// <summary>
	/// 恢复消息动画
	/// </summary>
	private void ResumeMessageAnimation(float delayTime = 0)
	{
		if (m_ResumeCoroutine != null)
		{
			UIManager.Instance.StopCoroutine(m_ResumeCoroutine);
			m_ResumeCoroutine = null;
		}

		if (delayTime <= 0)
		{
			m_Paused = false;
			PlayNextMessage();
		}
		else
		{
			m_ResumeCoroutine = UIManager.Instance.StartCoroutine(DelayResumeMessageAnimation(delayTime));
		}
	}

	private System.Collections.IEnumerator DelayResumeMessageAnimation(float delayTime)
	{
		yield return new WaitForSeconds(delayTime);

		ResumeMessageAnimation();
	}

	/// <summary>
	/// 添加一个消息
	/// </summary>
	/// <param name="msg">消息</param>
	private void AddMessage(MessageData msg)
	{
		m_MessageList.Add(msg);

		if (!m_Paused && m_CurrentMessage == null)
			PlayNextMessage();
	}

	/// <summary>
	/// 播放下一个消息
	/// </summary>
	private void PlayNextMessage()
	{
		if (m_MessageList.Count > 0)
		{
			m_MessageList.Sort((a, b) => { return b.priority - a.priority; });

			m_CurrentMessage = m_MessageList[0];

			m_MessageList.RemoveAt(0);

			AssetUtil.LoadAssetAsync(m_CurrentMessage.prefabPath,
				(pathOrAddress, returnObject, userData) =>
				{
					if (!m_RootNode)
						return;

					if (returnObject != null)
					{
						GameObject prefab = (GameObject)returnObject;

						prefab.CreatePool(pathOrAddress);

						m_CurrentMessageView = prefab.Spawn(m_RootNode).transform;
						m_CurrentMessageView.gameObject.SetActive(true);

						m_CurrentAnimationEvent = FindComponent<UIAnimationEvent>(m_CurrentMessageView, "Content/Message");
						m_CurrentAnimationEvent.OnAnimationEvent -= OnMessagePlayComplete;
						m_CurrentAnimationEvent.OnAnimationEvent += OnMessagePlayComplete;

						InitializeMessageAnimation();
					}
					else
					{
						Debug.LogError(string.Format("资源加载成功，但返回null,  pathOrAddress = {0}", pathOrAddress));

						PlayNextMessage();
					}
				}
			);
		}
	}

	/// <summary>
	/// 当消息动画播放完成时
	/// </summary>
	/// <param name="key">动画事件</param>
	private void OnMessagePlayComplete(string key)
	{
		if (!string.Equals(key, "AnimationComplete"))
			return;

		if (m_CurrentAnimationEvent)
		{
			m_CurrentAnimationEvent.OnAnimationEvent -= OnMessagePlayComplete;
			m_CurrentAnimationEvent = null;
		}

		if (m_CurrentMessageView)
		{
			m_CurrentMessageView.Recycle();
			m_CurrentMessageView = null;
		}

		m_CurrentMessage = null;

		PlayNextMessage();
	}

	/// <summary>
	/// 消息数据
	/// </summary>
	private class MessageData
	{
		/// <summary>
		/// 消息类型
		/// </summary>
		public NotificationName type;
		/// <summary>
		/// 优先级
		/// </summary>
		public int priority;
		/// <summary>
		/// 预置件地址
		/// </summary>
		public string prefabPath;
		/// <summary>
		/// 附加参数
		/// </summary>
		public object argument;
	}
}
