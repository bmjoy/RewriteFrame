using Assets.Scripts.Define;
using Eternity.FlatBuffer;
using PureMVC.Interfaces;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HudMapPanel : HudBase
{
    private const string ASSET_ADDRESS = Leyoutech.Core.Loader.Config.AssetAddressKey.PRELOADUI_HUD_MAPPANEL;

    /// <summary>
    /// 可见范围
    /// </summary>
    private RectTransform m_RangeImage;
    /// <summary>
    /// 视口
    /// </summary>
    private RectTransform m_Viewport;
    /// <summary>
    /// 箭头
    /// </summary>
    private RectTransform m_Arrow;
	/// <summary>
	/// 视野
	/// </summary>
    private RectTransform m_ImageView;
    /// <summary>
    /// 敌人
    /// </summary>
    private RectTransform m_Enemy;

	/// <summary>
	/// 地图名
	/// </summary>
	private TMP_Text m_MapName;

    /// <summary>
	/// 区域名
	/// </summary>
	private TMP_Text m_AreaName;

    /// <summary>
    /// proxy
    /// </summary>
    private CfgEternityProxy m_CfgEternityProxy;

	/// <summary>
	/// Language表id
	/// </summary>
	private readonly static uint PLAYER_MAP_ICON = 31302;

    public HudMapPanel() : base(UIPanel.HudMapPanel, ASSET_ADDRESS, PanelType.Hud) { }

    public override void Initialize()
    {
        m_RangeImage = FindComponent<RectTransform>("Content/Bg");
        m_Viewport = FindComponent<RectTransform>("Content/Ship/ShipView");
        m_Arrow = FindComponent<RectTransform>("Content/Ship/ShipIcon");
		m_ImageView = FindComponent<RectTransform>("Content/Ship/Image_View");
        m_Enemy = FindComponent<RectTransform>("Content/Icon");
        m_MapName= FindComponent<TMP_Text>("Content/Name/Label_Name");
        m_AreaName = FindComponent<TMP_Text>("Content/Name/Label_Area");
        m_CfgEternityProxy = GameFacade.Instance.RetrieveProxy(ProxyName.CfgEternityProxy) as CfgEternityProxy;
    }

    public override void OnShow(object msg)
    {
        base.OnShow(msg);
		ShowName();
		StartUpdate();
    }

	public override NotificationName[] ListNotificationInterests()
	{
		return new NotificationName[]
		{
			NotificationName.ChangeArea,
		};
	}

	public override void HandleNotification(INotification notification)
	{
		switch (notification.Name)
		{
			case NotificationName.ChangeArea:
				ShowName();
				break;
		}
	}

	public void ShowName()
	{
		Eternity.FlatBuffer.Map mapData = m_CfgEternityProxy.GetCurrentMapData();
		ulong area = Map.MapManager.GetInstance().GetCurrentAreaUid();
        m_MapName.text = TableUtil.GetLanguageString($"gamingmap_name_{mapData.GamingmapId}");
        m_AreaName.text = "-- " + TableUtil.GetLanguageString($"area_name_{mapData.GamingmapId}_{area}");
	}

    public override void OnHide(object msg)
    {
        base.OnHide(msg);
    }

    /// <summary>
    /// 更新
    /// </summary>
    protected override void Update()
    {
        bool visibleOld = GetTransform().gameObject.activeSelf;
        bool visibleNew = !IsWatchOrUIInputMode() && !IsDead() && !IsLeaping();

        if (visibleNew != visibleOld)
            GetTransform().gameObject.SetActive(visibleNew);

        if (!visibleNew)
            return;

        GameplayProxy proxy = Facade.RetrieveProxy(ProxyName.GameplayProxy) as GameplayProxy;

        SpacecraftEntity main = proxy.GetEntityById<SpacecraftEntity>(proxy.GetMainPlayerUID());
        if (main != null)
        {
            m_Arrow.localEulerAngles = new Vector3(0, 0, -main.transform.eulerAngles.y);
            m_Viewport.localEulerAngles = new Vector3(0, 0, -Camera.main.transform.eulerAngles.y);

            int index = 0;
            float maxDistance = m_RangeImage.rect.width / 2;
            List<SpacecraftEntity> entitys = proxy.GetEntities<SpacecraftEntity>();
            for (int i = 0; i < entitys.Count; i++)
            {
                SpacecraftEntity entity = entitys[i];
				/// 过滤掉主角
				if (entity != main)
				{
					float distance = Vector3.Distance(entity.transform.position, main.transform.position) * GameConstant.METRE_PER_UNIT;
					/// 是否更新小地图
					bool update = false;
					/// 玩家没有npc，不考虑贴边显示
					if (entity.GetHeroType() == KHeroType.htPlayer)
					{
						update = distance < GameConstant.DEFAULT_VISIBILITY_METRE_FOR_SHIP;
					}
					else
					{
						Npc npc = entity.GetNPCTemplateVO();
						bool overView = npc.OverView;
						if (overView)
						{
							distance = Mathf.Min(distance, GameConstant.DEFAULT_VISIBILITY_METRE_FOR_SHIP);
							update = true;
						}
						else if (distance < GameConstant.DEFAULT_VISIBILITY_METRE_FOR_SHIP)
						{
							update = true;
						}
					}

					if (update)
					{
						if (index >= m_ImageView.childCount)
						{
							Object.Instantiate(m_Enemy, m_ImageView);
						}
						m_ImageView.GetChild(index).localEulerAngles = new Vector3(0, 0, 0);
						UpdateTarget(m_ImageView.GetChild(index).GetComponent<RectTransform>(), maxDistance * (distance / GameConstant.DEFAULT_VISIBILITY_METRE_FOR_SHIP), main, entity);
						index++;
					}
				}
			}

            for (int i = index; i < m_ImageView.childCount; i++)
            {
				m_ImageView.GetChild(i).gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// 更新目标点
    /// </summary>
    /// <param name="view">敌方点</param>
    /// <param name="distance">距离</param>
    /// <param name="main">主角</param>
    /// <param name="target">目标</param>
    private void UpdateTarget(RectTransform view, float distance, SpacecraftEntity main, SpacecraftEntity target)
    {
        if (!view.gameObject.activeSelf)
        {
            view.gameObject.SetActive(true);
            view.Find("Image_Enemy").gameObject.SetActive(true);
        }

        Vector3 position = target.transform.position - main.transform.position;
        Vector3 direction = new Vector3(position.x, 0, position.z).normalized * distance;

        view.anchoredPosition = new Vector2(direction.x, direction.z);

		Image img = view.Find("Image_Enemy").GetComponent<Image>();
		KHeroType heroType = target.GetHeroType();
		/// 玩家不再npc表
		/// 先用指定ID
		if (heroType == KHeroType.htPlayer)
		{
			UIUtil.SetIconImage(img, TableUtil.GetIconBundle(PLAYER_MAP_ICON), TableUtil.GetIconAsset(PLAYER_MAP_ICON));
		}
		else
		{
			Npc npc = target.GetNPCTemplateVO();
			uint mapIcon = npc.MapIcon;
			if (mapIcon != 0)
			{
				/// TODO.
				/// 暂无考虑：同一NPC不同时刻改变阵营时，是否需要显示不同图标
				UIUtil.SetIconImage(img, TableUtil.GetIconBundle(mapIcon), TableUtil.GetIconAsset(mapIcon));
			}
			else
			{
				view.gameObject.SetActive(false);
				view.Find("Image_Enemy").gameObject.SetActive(false);
			}
		}
	}
}
