using Assets.Scripts.Define;
using Eternity.FlatBuffer;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Leyoutech.Core.Loader.Config;

/// <summary>
/// 边缘引导标记
/// </summary>
public class HudEdgeFlagPanel : HudBase
{
	private const string ASSET_ADDRESS = AssetAddressKey.PRELOADUI_HUD_EDGEFLAGPANEL;

    /// <summary>
    /// 根结点
    /// </summary>
    private RectTransform m_Flags;
    /// <summary>
    /// 缩放容器
    /// </summary>
    private RectTransform m_ScaleBox;
    /// <summary>
    /// 检测器模板
    /// </summary>
    private RectTransform m_DetectorTemplate;

    /// <summary>
    /// 模板回收池
    /// </summary>
    private Dictionary<Transform, Transform> m_TemplateToPool = new Dictionary<Transform, Transform>();
    /// <summary>
    /// 已经建立的面板
    /// </summary>
    private Dictionary<uint, EntityView> m_UIDToRectTranform1 = new Dictionary<uint, EntityView>();
    /// <summary>
    /// 已经建立的面板
    /// </summary>
    private Dictionary<uint, EntityView> m_UIDToRectTranform2 = new Dictionary<uint, EntityView>();

    /// <summary>
    /// 外部引用
    /// </summary>
    private CfgEternityProxy m_CfgEternityProxy;

    /// <summary>
    /// 缓存结构
    /// </summary>
    private struct EntityView
    {
        /// <summary>
        /// 实体ID
        /// </summary>
        public uint uid;
        /// <summary>
        /// 关联的UI
        /// </summary>
        public RectTransform view;
        /// <summary>
        /// 对应的UI池
        /// </summary>
        public Transform viewPool;
    }

    public HudEdgeFlagPanel() : base(UIPanel.HudEdgeFlagPanel, ASSET_ADDRESS, PanelType.Hud) { }

	public override void Initialize()
	{
		m_Flags = FindComponent<RectTransform>("Content/FlagBox");
        m_ScaleBox = FindComponent<RectTransform>("Content/ScaleBox");
        m_DetectorTemplate = FindComponent<RectTransform>("Templates/Detector");
    }

    public override void OnShow(object msg)
	{
		base.OnShow(msg);

        RectTransform poolBox = FindComponent<RectTransform>("TemplatePool");
        if (poolBox == null)
        {
            poolBox = new GameObject("TemplatePool", typeof(RectTransform)).GetComponent<RectTransform>();
            poolBox.SetParent(GetTransform());
            poolBox.gameObject.SetActive(false);
            poolBox.SetAsFirstSibling();
        }

        m_TemplateToPool.Clear();

        RectTransform templates = FindComponent<RectTransform>("Templates");
        for (int i = 0; i < templates.childCount; i++)
        {
            Transform template = templates.GetChild(i);
            Transform templatePool = poolBox.Find(template.name);
            if (!templatePool)
            {
                templatePool = new GameObject(template.name, typeof(RectTransform)).GetComponent<RectTransform>();
                templatePool.SetParent(poolBox);
            }
            m_TemplateToPool.Add(template, templatePool);
        }

        m_CfgEternityProxy = GameFacade.Instance.RetrieveProxy(ProxyName.CfgEternityProxy) as CfgEternityProxy;

        StartUpdate();
	}

	public override void OnHide(object msg)
    {
        //回收所有视图
        foreach (EntityView info in m_UIDToRectTranform1.Values)
        {
            info.view.SetParent(info.viewPool);
        }
        m_UIDToRectTranform1.Clear();

        foreach (EntityView info in m_UIDToRectTranform2.Values)
        {
            info.view.SetParent(info.viewPool);
        }
        m_UIDToRectTranform2.Clear();

        base.OnHide(msg);
	}

    /// <summary>
    /// 更新所有任务标记
    /// </summary>
    protected override void Update()
    {
        if (!IsDead() && !IsWatchOrUIInputMode() && !IsLeaping())
        {
            float w = m_Flags.rect.width;
            float h = m_Flags.rect.height;

            m_ScaleBox.localScale = w > h ? new Vector3(1, h / w, 1) : new Vector3(w / h, 1, 1);

            GameplayProxy sceneProxy = Facade.RetrieveProxy(ProxyName.GameplayProxy) as GameplayProxy;
            SpacecraftEntity main = sceneProxy.GetEntityById<SpacecraftEntity>(sceneProxy.GetMainPlayerUID());

            if (!main)
                return;

            List<SpacecraftEntity> entitys = sceneProxy.GetEntities<SpacecraftEntity>();
            for (int i = 0; i < entitys.Count; i++)
            {
                SpacecraftEntity entity = entitys[i];
                KHeroType heroType = entity.GetHeroType();
                Vector3 titleOffset = Vector3.zero;

                //忽略自已
                if (entity == main)
                    continue;

                //忽略不需要显示的NPC
                if (heroType != KHeroType.htPlayer)
                {
                    Npc entityVO = m_CfgEternityProxy.GetNpcByKey(entity.GetTemplateID());
                    //if (entityVO.Display == 0)
                        //continue;

                    if (entityVO.HeroHeaderLength >= 3)
                        titleOffset = new Vector3(entityVO.HeroHeader(0), entityVO.HeroHeader(1), entityVO.HeroHeader(2));
                }

                //忽略死亡的( 除了矿石 )
                if (heroType != KHeroType.htMine && heroType != KHeroType.htPreicous && (entity.GetAttribute(AttributeName.kHP) <= 0 || entity.GetCurrentState().GetMainState() == EnumMainState.Dead))
                    continue;

                //忽略不支持的
                RectTransform template = GetTemplate(main, entity);
                if (!template)
                    continue;

                //忽略过远的
                float distance = (entity.transform.position - main.transform.position).magnitude;
                if (entity.GetHeroType() == KHeroType.htMine && distance > entity.GetNPCTemplateVO().TriggerRange)
                    continue;
                distance = distance * GameConstant.METRE_PER_UNIT;
                if (distance >= GameConstant.DEFAULT_VISIBILITY_METRE_FOR_SHIP)
                    continue;

                //忽略屏幕内的
                Vector3 screenPosition = Camera.main.WorldToScreenPoint(entity.transform.TransformPoint(titleOffset));
                if (screenPosition.z > Camera.main.nearClipPlane && screenPosition.x >= 0 && screenPosition.x <= Camera.main.pixelWidth && screenPosition.y > 0 && screenPosition.y <= Camera.main.pixelHeight)
                {
                    // is in screen
                }
                else
                {
                    //忽略屏幕外的
                    Transform templatePool = m_TemplateToPool[template];

                    uint uid = entity.GetUId();

                    EntityView info;
                    if (m_UIDToRectTranform1.ContainsKey(uid))
                    {
                        info = m_UIDToRectTranform1[uid];
                        m_UIDToRectTranform1.Remove(uid);
                        m_UIDToRectTranform2[uid] = info;
                    }
                    else
                    {
                        RectTransform view = templatePool.childCount > 0 ? templatePool.GetChild(0).GetComponent<RectTransform>() : Object.Instantiate(template, m_Flags);
                        view.SetParent(m_Flags);
                        view.transform.SetAsLastSibling();
                        view.gameObject.SetActive(true);

                        info = new EntityView() { uid = uid, view = view, viewPool = templatePool };
                        m_UIDToRectTranform2[uid] = info;

                        OnTargetCreate(view, entity);
                    }

                    OnTargetUpdate(info.view, entity.transform.position, distance);
                }
            }
        }


        //回收无效的
        foreach (EntityView info in m_UIDToRectTranform1.Values)
        {
            info.view.SetParent(info.viewPool);
        }
        m_UIDToRectTranform1.Clear();

        Dictionary<uint, EntityView> tmp = m_UIDToRectTranform1;
        m_UIDToRectTranform1 = m_UIDToRectTranform2;
        m_UIDToRectTranform2 = tmp;
    }

    /// <summary>
    /// 标记创建时
    /// </summary>
    /// <param name="flag">视图</param>
    /// <param name="entity">实体</param>
    private void OnTargetCreate(RectTransform flag, SpacecraftEntity entity)
    {
        Image flagIcon = flag.transform.Find("IconMissionElement/Icon").GetComponent<Image>();
        if (flagIcon)
        {
            uint iconID = 0;
            switch (entity.GetHeroType())
            {
                case KHeroType.htPreicous: iconID = 32008; break;
                case KHeroType.htDetector: iconID = 32009; break;
            }
            if (iconID != 0)
            {
                flagIcon.gameObject.SetActive(true);
                UIUtil.SetIconImage(flagIcon, iconID);
            }
            else
            {
                flagIcon.gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// 刷新标记
    /// </summary>
    /// <param name="flag">视图</param>
    /// <param name="targetPosition">目标位置</param>
    /// <param name="distance">目标距离</param>
    private void OnTargetUpdate(RectTransform flag, Vector3 targetPosition, float distance)
    {
        RectTransform flagArrow = flag.transform.Find("Arrow").GetComponent<RectTransform>();
        RectTransform flagArrowImg = flag.transform.Find("Arrow/Arrow").GetComponent<RectTransform>();
        Image flagIcon = flag.transform.Find("IconMissionElement/Icon").GetComponent<Image>();
        TMP_Text flagDistance = flag.transform.Find("Label_DistanceText").GetComponent<TMP_Text>();

        Vector3 inCameraPosition = Camera.main.transform.InverseTransformPoint(targetPosition);

        Vector2 arrowPosition = m_ScaleBox.InverseTransformPoint(m_Flags.TransformPoint(((Vector2)inCameraPosition)));
        arrowPosition = arrowPosition.normalized * (Mathf.Max(m_Flags.rect.width, m_Flags.rect.height) / 2);
        arrowPosition = m_Flags.InverseTransformPoint(m_ScaleBox.TransformPoint(arrowPosition));

        flag.anchoredPosition = arrowPosition;
        flag.localScale = Vector3.one;
        flagArrow.transform.localRotation = Quaternion.Euler(0, 0, Mathf.Atan2(inCameraPosition.y, inCameraPosition.x) * Mathf.Rad2Deg - 90);
        flagArrow.gameObject.SetActive(true);

        //距离显示
        if (flagDistance)
        {
            flagDistance.text = FormatMetre(distance);
        }
    }

    /// <summary>
    /// 获取模板
    /// </summary>
    /// <param name="main">主角</param>
    /// <param name="target">目标</param>
    /// <returns>模板</returns>
    private RectTransform GetTemplate(SpacecraftEntity main, SpacecraftEntity target)
    {
        switch (target.GetHeroType())
        {
            case KHeroType.htPreicous:
            case KHeroType.htDetector:
                return m_DetectorTemplate;
        }
        return null;
    }


    /// <summary>
    /// 格式化单位米
    /// </summary>
    /// <param name="metre">米数</param>
    /// <returns>text</returns>
    private string FormatMetre(float metre)
    {
        return metre < 1000 ? string.Format("[{0:N1}m]", metre) : string.Format("[{0:N1}km]", metre / 1000.0f);
    }
}
