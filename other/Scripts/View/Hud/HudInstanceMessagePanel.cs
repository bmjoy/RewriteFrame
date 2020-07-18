using PureMVC.Interfaces;
using UnityEngine;

public class HudInstanceMessagePanel : HudBase
{
	private const string ASSET_ADDRESS = Leyoutech.Core.Loader.Config.AssetAddressKey.PRELOADUI_HUD_INSTANCEMESSAGEPANEL;

	/// <summary>
	/// 显示时间
	/// </summary>
	private float m_ShowTime;

	/// <summary>
	/// 计时器
	/// </summary>
	private float m_Timer;

	/// <summary>
	/// 开始倒计时
	/// </summary>
	private bool m_IsStart;

	/// <summary>
	/// boss面板
	/// </summary>
	private GameObject m_BossPanel;

    /// <summary>
    /// 游戏配置数据
    /// </summary>
    private CfgEternityProxy m_CfgEternityProxy;

	public HudInstanceMessagePanel() : base(UIPanel.HudInstanceMessagePanel, ASSET_ADDRESS, PanelType.Hud) { }

	public override void Initialize()
	{
		m_CfgEternityProxy= GameFacade.Instance.RetrieveProxy(ProxyName.CfgEternityProxy) as CfgEternityProxy;

		m_ShowTime = m_CfgEternityProxy.GetGamingConfig(1).Value.Dungeon.Value.BaoLuanZhiDi.Value.BossHudTime;

		m_BossPanel = FindComponent<Transform>("BossTip").gameObject;
		m_BossPanel.SetActive(false);
	}

	public override void OnShow(object msg)
	{
		base.OnShow(msg);

        ServerTimeUtil.Instance.OnTick -= OnUpdate;
        ServerTimeUtil.Instance.OnTick += OnUpdate;
	}

	public override void OnHide(object msg)
    {
        ServerTimeUtil.Instance.OnTick -= OnUpdate;

        base.OnHide(msg);
	}

	public override NotificationName[] ListNotificationInterests()
	{
		return new NotificationName[]
		{
			NotificationName.MSG_MESSAGE_BOSS_SHOW,
		};
	}

	public override void HandleNotification(INotification notification)
	{
		switch (notification.Name)
		{
			case NotificationName.MSG_MESSAGE_BOSS_SHOW:
				m_IsStart = true;
				m_BossPanel.SetActive(true);
				break;
			default:
				break;
		}
	}

	/// <summary>
	/// 计时器关闭消息
	/// </summary>
	public void OnUpdate()
	{
		if (m_IsStart)
		{
			m_Timer += 1;

			if (m_Timer > m_ShowTime)
			{
				m_Timer = 0;
				m_BossPanel.SetActive(false);
				m_IsStart = false;
			}
            else
            {
                m_BossPanel.SetActive(!IsWatchOrUIInputMode() && !IsDead() && !IsLeaping());
            }
        }
	}
}
