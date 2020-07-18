using PureMVC.Interfaces;
using UnityEngine;

public class HudAltitudeGauge : HudBase
{
    private const string ASSET_ADDRESS = Leyoutech.Core.Loader.Config.AssetAddressKey.PRELOADUI_HUD_LIFTING;

    /// <summary>
    /// 根节点
    /// </summary>
    private Animator m_Content;
    /// <summary>
    /// 标尺
    /// </summary>
    private RadialSlider m_AltitudeRule;
    /// <summary>
    /// 箭头
    /// </summary>
    private RadialSlider m_AltitudeArrow;

    public HudAltitudeGauge() : base(UIPanel.HudAltitudeGauge, ASSET_ADDRESS, PanelType.Hud) { }

    public override void Initialize()
    {
        m_Content = FindComponent<Animator>("Content");
        m_AltitudeRule = FindComponent<RadialSlider>("Content/Ruler");
        m_AltitudeArrow = FindComponent<RadialSlider>("Content/Arrow");

        m_Content.gameObject.SetActive(false);
    }

    public override void OnShow(object msg)
    {
        base.OnShow(msg);

        StartUpdate();
    }

    public override NotificationName[] ListNotificationInterests()
    {
        return new NotificationName[]
        {
            NotificationName.MSG_CHANGE_BATTLE_STATE,
            NotificationName.MainHeroDeath,
            NotificationName.MainHeroRevive
        };
    }

    public override void HandleNotification(INotification notification)
    {
        switch (notification.Name)
        {
            case NotificationName.MSG_CHANGE_BATTLE_STATE:
            case NotificationName.MainHeroRevive:
            case NotificationName.MainHeroDeath:
                UpdateUIVisible();
                break;
        }
    }

    protected override void OnInputMapChanged()
    {
        UpdateUIVisible();
    }

    private void UpdateUIVisible()
    {
        m_Content.gameObject.SetActive(!IsWatchOrUIInputMode() && !IsDead() && !IsLeaping() && IsBattling());
    }

    protected override void Update()
    {
        if (!m_Content) return;
        if (!m_Content.gameObject.activeSelf) return;

        GameplayProxy proxy = Facade.RetrieveProxy(ProxyName.GameplayProxy) as GameplayProxy;

        SpacecraftEntity main = proxy.GetEntityById<SpacecraftEntity>(proxy.GetMainPlayerUID());
        if (main == null) return;

        Transform mainShadow = main.GetSyncTarget();
        if (mainShadow == null) return;

        Rigidbody rigidbody = mainShadow.GetComponent<Rigidbody>();
        if (rigidbody == null) return;

        SpacecraftMotionComponent mainMotion = main.GetEntityComponent<SpacecraftMotionComponent>();
        if (mainMotion == null) return;

        SpacecraftMotionInfo cruiseMotion = mainMotion.GetCruiseModeMotionInfo();

        Vector3 velocity = rigidbody.transform.InverseTransformDirection(rigidbody.velocity);

        float upSpeed = velocity.y;
        float upSpeedMax = cruiseMotion.LineVelocityMax.y;
        float upSpeedPercent = Mathf.Clamp01(Mathf.Abs(upSpeed) / Mathf.Abs(upSpeedMax));

        float dir = upSpeedPercent * (upSpeed > 0 ? 1 : -1);

        m_Content.SetBool("Opened", upSpeed != 0);
        m_AltitudeRule.FillAmount = 0.5f - upSpeedPercent * dir * 0.5f;
        m_AltitudeArrow.FillAmount = 0.5f + upSpeedPercent * dir * 0.5f;

        //Debug.LogError(string.Format("{0:N2}/{1:N2} = {2:N3}", upSpeed, upSpeedMax, upSpeedPercent));
    }
}
