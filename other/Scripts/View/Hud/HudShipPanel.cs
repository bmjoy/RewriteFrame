using Assets.Scripts.Define;
using PureMVC.Interfaces;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HudShipPanel : HudBase
{
    private const string ASSET_ADDRESS = Leyoutech.Core.Loader.Config.AssetAddressKey.PRELOADUI_HUD_HP;

    /// <summary>
    /// 根动画
    /// </summary>
    private Animator m_Animator;
    /// <summary>
    /// 血条
    /// </summary>
    private Image m_HpImage;
    /// <summary>
    /// 血值文字容器
    /// </summary>
    private RectTransform m_HpLabelBox;
    /// <summary>
    /// 血值文字
    /// </summary>
    private TMP_Text m_HpLabel;
    /// <summary>
    /// 护盾条
    /// </summary>
    private Image m_SpImage;
    /// <summary>
    /// 护盾文字
    /// </summary>
    private TMP_Text m_SpLabel;
    /// <summary>
    /// 高度指示容器
    /// </summary>
    private RectTransform m_HeightBox;
    /// <summary>
    /// 高度指示箭头
    /// </summary>
    private RectTransform m_HeightArrow;
    /// <summary>
    /// 高度指示标尺
    /// </summary>
    private RawImage m_HeightRule;
    /// <summary>
    /// 速度条1
    /// </summary>
    private Image m_SpeedImage1;
    /// <summary>
    /// 速度条2
    /// </summary>
    private Image m_SpeedImage2;
    /// <summary>
    /// 速度文本
    /// </summary>
    private TMP_Text m_SpeedText;
    /// <summary>
    /// 电量条
    /// </summary>
    private Image m_ElectricImage;
    /// <summary>
    /// 电量文本
    /// </summary>
    private TMP_Text m_ElectricText;
    /// <summary>
    /// 负载指示容器
    /// </summary>
    private RectTransform m_OverloadBox;
    /// <summary>
    /// 负载指示条
    /// </summary>
    private Image m_OverloadBar;
    /// <summary>
    /// 负载指示箭头
    /// </summary>
    private RectTransform m_OverloadArrow;

    public HudShipPanel() : base(UIPanel.HudShipPanel, ASSET_ADDRESS, PanelType.Hud) { }

    public override void Initialize()
    {
        m_Animator = GetTransform().GetComponent<Animator>();

        m_HpImage = FindComponent<Image>("Content/Left/HpSlider/Image_BarMask/Slider");
        m_HpLabelBox = FindComponent<RectTransform>("Content/Left/HpSlider/Label_HpBox");
        m_HpLabel = FindComponent<TMP_Text>("Content/Left/HpSlider/Label_HpBox/Label_HP");

        m_SpImage = FindComponent<Image>("Content/Left/ShieldSlider/Image_BarMask/Slider");
        m_SpLabel = FindComponent<TMP_Text>("Content/Left/ShieldSlider/Label_Full");

        m_HeightBox = FindComponent<RectTransform>("Content/Left/Height");
        m_HeightRule = FindComponent<RawImage>("Content/Left/Height/Image_Scale");
        m_HeightArrow = FindComponent<RectTransform>("Content/Left/Height/Image_Scale/Image_Arrow");

        m_SpeedImage1 = FindComponent<Image>("Content/Right/SpeedSlider_2/Image_BarMask");
        m_SpeedImage2 = FindComponent<Image>("Content/Right/SpeedSlider_1/Image_BarMask");
        m_SpeedText = FindComponent<TMP_Text>("Content/Right/Label_Number");

        m_ElectricImage = FindComponent<Image>("Content/Right/ElectricSlider/Image_BarMask/Slider");
        m_ElectricText = FindComponent<TMP_Text>("Content/Right/Image_MoveLine/Label_Number");

        m_OverloadBox = FindComponent<RectTransform>("Content/Right/Image_MoveLine");
        m_OverloadBar = FindComponent<Image>("Content/Right/Image_MoveLine/Image_Bar");
        m_OverloadArrow = FindComponent<RectTransform>("Content/Right/Image_MoveLine/Image_Bar/Image_Arrow");
    }

    public override void OnShow(object msg)
    {
        base.OnShow(msg);

        StartUpdate();

        RefreshAnimState();
    }

    public override NotificationName[] ListNotificationInterests()
    {
        return new NotificationName[]
        {
            NotificationName.MSG_CHANGE_BATTLE_STATE,
            NotificationName.MainHeroDeath,
            NotificationName.MainHeroRevive,
            NotificationName.MSG_HUMAN_ENTITY_ON_ADDED
        };
    }

    public override void HandleNotification(INotification notification)
    {
        switch (notification.Name)
        {
            case NotificationName.MSG_CHANGE_BATTLE_STATE:
            case NotificationName.MainHeroDeath:
            case NotificationName.MainHeroRevive:
                RefreshAnimState();
                break;
            case NotificationName.MSG_HUMAN_ENTITY_ON_ADDED:
                {
                    MsgEntityInfo entityInfo = (MsgEntityInfo)notification.Body;
                    if (entityInfo.IsMain)
                        RefreshAnimState();
                }
                break;
        }
    }

    /// <summary>
    /// 输入模式改变时
    /// </summary>
    protected override void OnInputMapChanged()
    {
        RefreshAnimState();
    }

    /// <summary>
    /// 更新
    /// </summary>
    protected override void Update()
    {
        SpacecraftEntity main = GetMainEntity();
        if (main == null) return;

        Transform mainShadow = main.GetSyncTarget();
        if (mainShadow == null) return;

        Rigidbody rigidbody = mainShadow.GetComponent<Rigidbody>();
        if (rigidbody == null) return;

        SpacecraftMotionComponent motionComponent = main.GetEntityComponent<SpacecraftMotionComponent>();
        if (motionComponent == null) return;

        Vector3 shipVelocity = rigidbody.transform.InverseTransformDirection(rigidbody.velocity);
        SpacecraftMotionInfo shipMotionInfo = motionComponent.GetCruiseModeMotionInfo();

        bool isBattle = IsBattling();
        bool isOverload = IsOverload();

        float hp = (float)main.GetAttribute(AttributeName.kHP);
        float hpMax = (float)main.GetAttribute(AttributeName.kHPMax);
        float hpPercent = Mathf.Clamp01(hpMax > 0 ? hp / hpMax : 0);

        float sp = (float)main.GetAttribute(AttributeName.kShieldValue);
        float spMax = (float)main.GetAttribute(AttributeName.kShieldMax);
        float spPercent = Mathf.Clamp01(spMax > 0 ? sp / spMax : 0);

        float electric = (float)main.GetAttribute(AttributeName.kPowerValue);
        float electricMax = (float)main.GetAttribute(AttributeName.kPowerMax);
        float electricPercent = Mathf.Clamp01(electricMax > 0 ? electric / electricMax : 0);

        float velocityForward = shipVelocity.z;
        float velocityForwardMax = shipMotionInfo.LineVelocityMax.z;
        float velocityForwardPercent = velocityForwardMax > 0 ? Mathf.Clamp01(Mathf.Abs(velocityForward) / Mathf.Abs(velocityForwardMax)) : 0;

        float velocityUp = shipVelocity.y;
        float velocityUpMax = shipMotionInfo.LineVelocityMax.y;
        float velocityUpPercent = velocityUpMax > 0 ? Mathf.Clamp01(Mathf.Abs(velocityUp) / Mathf.Abs(velocityUpMax)) : 0;

        m_HpImage.fillAmount = Mathf.Lerp(0.171f, 0.329f, hpPercent);
        m_HpLabelBox.localEulerAngles = Vector3.forward * Mathf.Lerp(31.8f, -26.0f, hpPercent);
        m_HpLabel.text = string.Format("{0}%", Mathf.FloorToInt(hpPercent * 100));

        m_SpImage.fillAmount = Mathf.Lerp(0.181f, 0.319f, spPercent);
        m_SpLabel.gameObject.SetActive(Mathf.Approximately(spPercent, 1.0f));

        m_ElectricImage.fillAmount = Mathf.Lerp(0.181f, 0.319f, electricPercent);
        m_ElectricText.text = string.Format("{0}/{1}", Mathf.CeilToInt(electric), Mathf.FloorToInt(electricMax));
        m_OverloadBox.localEulerAngles = Vector3.forward * Mathf.Lerp(-21.86f, 26.0f, electricPercent);

        m_SpeedImage1.fillAmount = Mathf.Lerp(0.171f, 0.248f, Mathf.Clamp01(velocityForwardPercent / 0.5f));
        m_SpeedImage2.fillAmount = Mathf.Lerp(0.252f, 0.328f, Mathf.Clamp01((velocityForwardPercent - 0.5f) / 0.5f));

        velocityForward *= GameConstant.METRE_PER_UNIT;
        m_SpeedText.gameObject.SetActive(velocityForward > 0);
        if (m_SpeedText.gameObject.activeSelf)
        {
            if (velocityForward < 1000)
                m_SpeedText.text = string.Format("{0}M", (int)velocityForward);
            else
                m_SpeedText.text = string.Format("{0:N2}KM", velocityForward / 1000.0f);
        }

        float overloadProgress = main.GetOverloadProgress();
        m_OverloadBar.gameObject.SetActive(isBattle && overloadProgress > 0 && overloadProgress < 1.0f);
        if (m_OverloadBar.gameObject.activeSelf)
        {
            m_OverloadBar.fillAmount = overloadProgress;
            m_OverloadArrow.anchorMin = new Vector2(m_OverloadArrow.anchorMin.x, 1 - overloadProgress);
            m_OverloadArrow.anchorMax = new Vector2(m_OverloadArrow.anchorMax.x, 1 - overloadProgress);
        }

        /*
        float upSpeedPercent = Mathf.Clamp(velocityUp / velocityUpMax, -1, 1);
        m_HeightRule.uvRect = new Rect(0, m_HeightRule.uvRect.y + upSpeedPercent * 0.01f, 1, 1);
        if (Mathf.Abs(upSpeedPercent) > 0.01f)
        {
            m_HeightBox.gameObject.SetActive(true);
            m_HeightArrow.anchorMax = m_HeightArrow.anchorMin = new Vector2(m_HeightArrow.anchorMin.x, 0.5f + upSpeedPercent * 0.5f);
        }
        else
        {
            if (Mathf.Abs(m_HeightArrow.anchorMin.y - 0.5f) < 0.01f)
            {
                m_HeightBox.gameObject.SetActive(false);
            }
            else
            {
                m_HeightBox.gameObject.SetActive(true);
                m_HeightArrow.anchorMax = m_HeightArrow.anchorMin = new Vector2(m_HeightArrow.anchorMin.x, Mathf.Lerp(m_HeightArrow.anchorMin.y, 0.5f, Time.deltaTime * 3.0f));
            }
        }
        */
    }

    /// <summary>
    /// 设置跃迁状态
    /// </summary>
    private void RefreshAnimState()
    {
        if(IsWatchOrUIInputMode())
        {
            GetTransform().gameObject.SetActive(false);
        }
        else
        {
            GetTransform().gameObject.SetActive(true);

            m_Animator.ResetTrigger("Closed");
            m_Animator.ResetTrigger("Open");

            m_Animator.SetTrigger(!IsBattling() || IsLeaping() || IsDead() ? "Closed" : "Open");
        }
    }
}
