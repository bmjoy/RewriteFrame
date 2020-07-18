using Assets.Scripts.Define;
using PureMVC.Interfaces;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HudBattleStatePanel : HudBase
{
    /// <summary>
    /// 资源地址
    /// </summary>
    private const string ASSET_ADDRESS = Leyoutech.Core.Loader.Config.AssetAddressKey.PRELOADUI_HUD_MODEPANEL;

    private static Color FadeColor = new Color(1, 1, 1, 0.2f);

    /// <summary>
    /// 主动画
    /// </summary>
    private Animator m_Animator;
    /// <summary>
    /// 倒计时容器
    /// </summary>
    private RectTransform m_CountdownBox;
    /// <summary>
    /// 倒计时文本
    /// </summary>
    private TMP_Text m_CountdownText;
    /// <summary>
    /// 倒计时图像
    /// </summary>
    private Image m_CountdownImage;
    /// <summary>
    /// 巡航状态图标
    /// </summary>
    private Image m_CruiseIcon;
    /// <summary>
    /// 战斗状态图标
    /// </summary>
    private Image m_BattleIcon;
    /// <summary>
    /// 热键容器
    /// </summary>
    private RectTransform m_HotkeyBox;
    /// <summary>
    /// 被打时间
    /// </summary>
    private float m_HurtTime;
    /// <summary>
    /// 是否为战斗状态
    /// </summary>
    private bool m_IsBattleState;
    /// <summary>
    /// GameplayProxy
    /// </summary>
    private GameplayProxy m_GameplayProxy;

    public HudBattleStatePanel() : base(UIPanel.HudBattleStatePanel, ASSET_ADDRESS, PanelType.Hud)
    {
    }

    public override void Initialize()
    {
        base.Initialize();

        m_Animator = FindComponent<Animator>("Content/Mode");
        m_CountdownBox = FindComponent<RectTransform>("Content/Mode/CDImage");
        m_CountdownText = FindComponent<TMP_Text>("Content/Mode/CDImage/Label_CDTimes");
        m_CountdownImage = FindComponent<Image>("Content/Mode/CDImage/CD");
        m_CruiseIcon = FindComponent<Image>("Content/Mode/Image_cruise");
        m_BattleIcon = FindComponent<Image>("Content/Mode/Image_battle");
        m_HotkeyBox = FindComponent<RectTransform>("Content/Mode/HotKey");
    }

    public override void OnShow(object msg)
    {
        base.OnShow(msg);

        m_GameplayProxy = Facade.RetrieveProxy(ProxyName.GameplayProxy) as GameplayProxy;

        HotkeyManager.Instance.Register("toggleMode_" + GetTransform().GetInstanceID(), HotKeyMapID.SHIP, HotKeyID.ShipSwitchMode, OnToggleMode, m_HotkeyBox, "", HotkeyManager.HotkeyStyle.UI_SIMPLE);

        StartUpdate();
    }

    public override void OnHide(object msg)
    {
        HotkeyManager.Instance.Unregister("toggleMode_" + GetTransform().GetInstanceID());

        m_GameplayProxy = null;

        base.OnHide(msg);
    }

    public override NotificationName[] ListNotificationInterests()
    {
        return new NotificationName[]
        {
            NotificationName.SkillHurt,
            NotificationName.BuffHurt,
            NotificationName.HurtImmuno,
            NotificationName.MSG_CHANGE_BATTLE_STATE_FAIL
        };
    }

    public override void HandleNotification(INotification notification)
    {
        switch (notification.Name)
        {
            case NotificationName.SkillHurt:
                OnHurt((notification.Body as SkillHurtInfo).TargetID);
                break;
            case NotificationName.BuffHurt:
                OnHurt((notification.Body as BuffHurtInfo).targetID);
                break;
            case NotificationName.HurtImmuno:
                OnHurt((notification.Body as HurtImmuno).targetID);
                break;
            case NotificationName.MSG_CHANGE_BATTLE_STATE_FAIL:
                OnToggleFail();
                break;
        }
    }

    /// <summary>
    /// 切换模式
    /// </summary>
    /// <param name="callback">热键状态</param>
    private void OnToggleMode(HotkeyCallback callback)
    {
    }

    /// <summary>
    /// 受到伤害
    /// </summary>
    /// <param name="targetID">目标ID</param>
    private void OnHurt(uint targetID)
    {
        if (m_GameplayProxy.GetMainPlayerUID() == targetID)
            m_HurtTime = Time.time;
    }

    /// <summary>
    /// 切换状态失败
    /// </summary>
    private void OnToggleFail()
    {
        SpacecraftEntity main = GetMainEntity();
        if (main != null)
        {
            if(IsBattling())
            {
                m_Animator.ResetTrigger("ToggleFail");
                m_Animator.SetTrigger("ToggleFail");
            }
        }
    }

    /// <summary>
    /// 更新
    /// </summary>
    protected override void Update()
    {
        bool visibleOld = GetTransform().gameObject.activeSelf;
        bool visibleNew = !IsWatchOrUIInputMode() && !IsDead() && !IsLeaping();

        if (visibleNew!=visibleOld)
            GetTransform().gameObject.SetActive(visibleNew);

        if (!visibleNew)
            return;

        SpacecraftEntity main = GetMainEntity();
        if (main != null)
        {
            bool IsFighting = IsBattling();
            if(IsFighting)
            {
                if (!m_IsBattleState)
                    m_HurtTime = Time.time;

                float fireCD = main.GetFireCountdown();
                float fireCDMax = 10.0f;// main.GetAttribute(AircraftAttributeType.peerlessTopLimit);

                m_CountdownBox.gameObject.SetActive(IsFighting && fireCD > 0);
                if (m_CountdownBox.gameObject.activeSelf)
                {
                    m_CountdownText.text = string.Format(TableUtil.GetLanguageString("hud_text_id_1014"), fireCD > 0 ? (int)fireCD : 0);
                    m_CountdownImage.fillAmount = fireCD / fireCDMax;

                    m_CruiseIcon.color = FadeColor;
                    m_BattleIcon.color = FadeColor;
                }
                else
                {
                    m_CruiseIcon.color = Color.white;
                    m_BattleIcon.color = Color.white;
                }

                m_Animator.SetBool("Fighting", true);
                if (fireCD > 0)
                    m_Animator.SetBool("Hurting", true);
                else
                    m_Animator.SetBool("Hurting", (Time.time - m_HurtTime < 5.0f));
            }
            else
            {
                if (m_IsBattleState)
                    m_HurtTime = 0.0f;

                m_CountdownBox.gameObject.SetActive(false);
                m_CruiseIcon.color = Color.white;
                m_BattleIcon.color = Color.white;

                m_Animator.SetBool("Fighting", false);
                m_Animator.SetBool("Hurting", Time.time - m_HurtTime < 5.0f);
            }

            m_IsBattleState = IsFighting;
        }
    }
}
