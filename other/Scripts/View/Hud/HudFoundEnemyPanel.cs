using System;
using System.Collections;
using PureMVC.Interfaces;
using TMPro;
using UnityEngine;

public class HudFoundEnemyPanel : HudBase
{
    private const string ASSET_ADDRESS = Leyoutech.Core.Loader.Config.AssetAddressKey.PRELOADUI_HUD_FOUNDENEMYPANEL;
    private ulong m_AIUid;
    /// <summary>
    /// 画布相机
    /// </summary>
    private Camera m_Camera;
    /// <summary>
    /// 根节点
    /// </summary>
    private RectTransform m_Root;
    /// <summary>
    /// 图标
    /// </summary>
    private RectTransform m_Icon;

    public HudFoundEnemyPanel() : base(UIPanel.HudFoundEnemyPanel, ASSET_ADDRESS, PanelType.Hud) { }

    public override void Initialize()
    {
        m_Root = FindComponent<RectTransform>("Parent");
        m_Icon = FindComponent<RectTransform>("Parent/Content");
        m_Camera = m_Root.GetComponentInParent<Canvas>().worldCamera;
    }

    public override void OnShow(object msg)
    {
        base.OnShow(msg);
        MSAIBossProxy msab = GameFacade.Instance.RetrieveProxy(ProxyName.MSAIBossProxy) as MSAIBossProxy;
        m_AIUid = msab.GetCurrentAIUId();
        StartUpdate();
        UIManager.Instance.StartCoroutine(DelayToClose());
    }

    protected override void Update()
    {
        GameplayProxy gameplayProxy = Facade.RetrieveProxy(ProxyName.GameplayProxy) as GameplayProxy;
        SpacecraftEntity entity = gameplayProxy.GetEntityById<SpacecraftEntity>((uint)m_AIUid);
        if (entity == null || entity.transform == null)
        {
            UIManager.Instance.ClosePanel(this);
            return;
        }

        bool isInScreen = IsInScreen(entity.transform.position, Camera.main);
        //忽略屏幕外的
        if (isInScreen)
        {
            //屏幕内显示图标
            Vector3 screenPoint = Camera.main.WorldToScreenPoint(entity.transform.position);
            Vector2 localPoint = Vector2.zero;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(m_Root, screenPoint, m_Camera, out localPoint))
            {
                m_Icon.gameObject.SetActive(true);
                m_Icon.anchoredPosition = localPoint;
                //m_Animation.anchoredPosition = localPoint;
                //m_Icon.localScale = Vector3.one;
                //m_Animation.localScale = Vector3.one;
            }
        }
        else
        {
            m_Icon.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 延迟关闭
    /// </summary>
    /// <returns>IEnumerator</returns>
    private IEnumerator DelayToClose()
    {
        yield return new WaitForSeconds(4.0f);
        UIManager.Instance.ClosePanel(this);
    }
}
