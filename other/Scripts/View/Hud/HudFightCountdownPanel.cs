using Assets.Scripts.Define;
using PureMVC.Interfaces;
using TMPro;
using UnityEngine;

public class HudFightCountdownPanel : HudBase
{
    private const string ASSET_ADDRESS = Leyoutech.Core.Loader.Config.AssetAddressKey.PRELOADUI_HUD_FCPANEL;

    /// <summary>
    /// 主动画
    /// </summary>
    private Animator m_Animator;
    /// <summary>
    /// 倒计时容器
    /// </summary>
    private RectTransform m_CooldownBox;
    /// <summary>
    /// 倒计时文本
    /// </summary>
    private TMP_Text m_CooldownText;
    /// <summary>
    /// 血值
    /// </summary>
    private TMP_Text m_HpText;
    /// <summary>
    /// 护盾
    /// </summary>
    private TMP_Text m_SpText;
    /// <summary>
    /// 状态文字
    /// </summary>
    private TMP_Text m_StateText;
    /// <summary>
    /// 热键容器
    /// </summary>
    private RectTransform m_HotkeyBox;

    /// <summary>
    /// 被打时间
    /// </summary>
    private float m_HurtTime;
    /// <summary>
    /// 状态
    /// </summary>
    private State m_State = State.None;

    private enum State { None, Normal, Hurt, BattleCD, Battle }

    public HudFightCountdownPanel() : base(UIPanel.HudFightCountdownPanel, ASSET_ADDRESS, PanelType.Hud) { }

    public override void Initialize()
    {
        m_Animator = GetTransform().GetComponent<Animator>();

        m_CooldownBox = FindComponent<RectTransform>("Box/Tip");
        m_CooldownText = FindComponent<TMP_Text>("Box/Tip/Label_FCCD");

        m_HpText = FindComponent<TMP_Text>("Box/Text/HP/label_Number");
        m_SpText = FindComponent<TMP_Text>("Box/Text/SP/label_Number");

        m_StateText = FindComponent<TMP_Text>("Box/Middle/label_Tip");
        m_HotkeyBox = FindComponent<RectTransform>("Box/Middle/Hotkey");
    }

    public override void OnShow(object msg)
    {
        base.OnShow(msg);

        HotkeyManager.Instance.Register("toggleMode_" + GetTransform().GetInstanceID(), HotKeyMapID.SHIP, HotKeyID.ShipSwitchMode, OnToggleMode, m_HotkeyBox, "", HotkeyManager.HotkeyStyle.UI_SIMPLE);

        StartUpdate();
    }

    public override void OnHide(object msg)
    {
        HotkeyManager.Instance.Unregister("toggleMode_" + GetTransform().GetInstanceID());

        base.OnHide(msg);
    }

    public override NotificationName[] ListNotificationInterests()
    {
        return new NotificationName[]
        {
            NotificationName.SkillHurt,
            NotificationName.BuffHurt,
            NotificationName.HurtImmuno
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
        GameplayProxy proxy = Facade.RetrieveProxy(ProxyName.GameplayProxy) as GameplayProxy;
        if (proxy.GetMainPlayerUID() == targetID && !IsBattling())
        {
            m_HurtTime = Time.time;

            if (m_State == State.Normal || m_State == State.Hurt)
                m_Animator.SetTrigger("Injured");
        }
    }


    /// <summary>
    /// 更新
    /// </summary>
    protected override void Update()
    {
        SpacecraftEntity main = GetMainEntity();
        if (main != null)
        {
            bool IsFighting = IsBattling();

            float fireCD = main.GetFireCountdown();
            //float fireCDMax = 10.0f;// main.GetAttribute(AircraftAttributeType.peerlessTopLimit);

            m_CooldownBox.gameObject.SetActive(IsFighting && fireCD > 0);
            if (m_CooldownBox.gameObject.activeSelf)
                m_CooldownText.text = string.Format(TableUtil.GetLanguageString("hud_text_id_1014"), fireCD > 0 ? (int)fireCD : 0);

            m_HpText.text = Mathf.FloorToInt((float)main.GetAttribute(AttributeName.kHP)).ToString();
            m_SpText.text = Mathf.FloorToInt((float)main.GetAttribute(AttributeName.kShieldValue)).ToString();

            State state = State.Normal;
            if (IsFighting)
                state = fireCD > 0 ? State.BattleCD : State.Battle;
            else
                state = Time.time - m_HurtTime < 10.0f ? State.Hurt : State.Normal;

            if (m_State != state)
            {
                m_Animator.ResetTrigger("Normal");
                m_Animator.ResetTrigger("Injured");
                m_Animator.ResetTrigger("Battle");
                m_Animator.ResetTrigger("Ready");

                m_State = state;
                switch (m_State)
                {
                    case State.Normal:
                        m_StateText.text = TableUtil.GetLanguageString("hud_text_id_1009");
                        m_HotkeyBox.gameObject.SetActive(false);
                        m_Animator.SetTrigger("Normal");
                        break;
                    case State.Hurt:
                        m_StateText.text = TableUtil.GetLanguageString("hud_text_id_1010");
                        m_HotkeyBox.gameObject.SetActive(true);
                        m_Animator.SetTrigger("Injured");
                        break;
                    case State.BattleCD:
                        m_StateText.text = TableUtil.GetLanguageString("hud_text_id_1011");
                        m_HotkeyBox.gameObject.SetActive(false);
                        m_Animator.SetTrigger("Battle");
                        break;
                    case State.Battle:
                        m_StateText.text = TableUtil.GetLanguageString("hud_text_id_1012");
                        m_HotkeyBox.gameObject.SetActive(true);
                        m_Animator.SetTrigger("Ready");
                        break;
                }
            }
        }
    }
}
