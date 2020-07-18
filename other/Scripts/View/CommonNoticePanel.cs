using PureMVC.Interfaces;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// 通用提示面板
/// </summary>
public class CommonNoticePanel : UIPanelBase
{
    private const string ASSET_ADDRESS = "Assets/Artwork/UI/Prefabs/CommonNoticePanel.prefab"; 

	/// <summary>
	/// 消息显示内容text
	/// </summary>
	private TextMeshProUGUI m_NoticeText;

	/// <summary>
	/// 消息背景image
	/// </summary>
	private Image m_NoticeBgImage;

	/// <summary>
	/// 消息Item
	/// </summary>
	private Transform m_NoticeItem;

	/// <summary>
	/// 消息根节点
	/// </summary>
	private Transform m_NoticeRoot;

	/// <summary>
	/// 消息队列
	/// </summary>
	private Queue<Transform> m_NoticeQueue;

	/// <summary>
	/// 消息显示时间
	/// </summary>
	private float m_ShowTime;

	/// <summary>
	/// 计时器
	/// </summary>
	private float m_Timer;

	public CommonNoticePanel() : base(UIPanel.CommonNoticePanel, ASSET_ADDRESS, PanelType.Notice)
	{

	}
	public override void Initialize()
	{
        base.Initialize();

        IsPermanent = true;

		m_NoticeQueue = new Queue<Transform>();
		m_NoticeItem = FindComponent<Transform>("Content/NoticeItem");
		m_NoticeRoot = FindComponent<Transform>("Content");
		m_NoticeItem.gameObject.SetActive(false);
		m_ShowTime = 5f;
	}

	public override void OnRefresh(object msg)
	{
	}

	public override void OnShow(object msg)
	{
		base.OnShow(msg);
		CanReceiveFocus = false;
		ServerTimeUtil.Instance.OnTick += OnUpdate;
	}

	public override void OnHide(object msg)
	{
		base.OnHide(msg);
		ServerTimeUtil.Instance.OnTick -= OnUpdate;
	}

	public override NotificationName[] ListNotificationInterests()
	{
		return new NotificationName[]
		{
			NotificationName.MSG_SENDCOMMONNOTICE,
		};
	}

	public override void HandleNotification(INotification notification)
	{
		switch (notification.Name)
		{
			case NotificationName.MSG_SENDCOMMONNOTICE:
				DisposeNotice(notification.Body);
				break;
			default:
				break;
		}
	}

	/// <summary>
	/// 处理消息 
	/// </summary>
	/// <param name="msg">消息内容</param>
	public void DisposeNotice(object msg)
	{
		string str = msg as string;
		Transform transformNotice = null;
		if (!string.IsNullOrEmpty(str))
		{
			if (m_NoticeItem.CountPooled() == 0)
			{
				m_NoticeItem.CreatePool(1,string.Empty);
			}
			transformNotice = m_NoticeItem.Spawn(m_NoticeRoot);
			transformNotice.localPosition = Vector3.zero;
			transformNotice.GetComponent<RectTransform>().offsetMax = Vector2.zero;
			transformNotice.GetComponent<RectTransform>().offsetMin = Vector2.zero;
			transformNotice.localScale = Vector3.one;
			transformNotice.gameObject.SetActive(true);
			m_NoticeText = TransformUtil.FindUIObject<TextMeshProUGUI>(transformNotice, "Label_Desc");
			m_NoticeBgImage = TransformUtil.FindUIObject<Image>(transformNotice, "Image");
			m_NoticeText.text = str;
			m_NoticeQueue.Enqueue(transformNotice);
		}
	}

	/// <summary>
	/// 计时器关闭消息
	/// </summary>
	public void OnUpdate()
	{
		if (m_NoticeQueue.Count > 0)
		{
			m_Timer += 1;
			if (m_Timer > m_ShowTime)
			{
				m_Timer = 0;
				m_NoticeQueue.Dequeue().Recycle();
			}
		}
	}
}

