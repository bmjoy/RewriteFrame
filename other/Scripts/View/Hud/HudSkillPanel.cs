using Assets.Scripts.Define;
using PureMVC.Interfaces;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HudSkillPanel : HudBase
{
    private const string ASSET_ADDRESS = Leyoutech.Core.Loader.Config.AssetAddressKey.PRELOADUI_HUD_SKILLPANEL;

    /// <summary>
    /// 技能按钮1
    /// </summary>
    private RectTransform m_SkillButton1;
    /// <summary>
    /// 技能按钮2
    /// </summary>
    private RectTransform m_SkillButton2;
    /// <summary>
    /// 技能按钮3
    /// </summary>
    private RectTransform m_SkillButton3;
    /// <summary>
    /// 技能按钮4
    /// </summary>
    private RectTransform m_SkillButton4;

    /// <summary>
    /// 热键1
    /// </summary>
    private RectTransform m_SkillHotkey1;
    /// <summary>
    /// 热键2
    /// </summary>
    private RectTransform m_SkillHotkey2;
    /// <summary>
    /// 热键3
    /// </summary>
    private RectTransform m_SkillHotkey3;
    /// <summary>
    /// 热键4
    /// </summary>
    private RectTransform m_SkillHotkey4;

    /// <summary>
    /// GameplayProxy
    /// </summary>
    private GameplayProxy m_GameplayProxy;
    /// <summary>
    /// SkillProxy
    /// </summary>
    private PlayerSkillProxy m_SkillProxy;

    /// <summary>
    /// 用于检查当前是否在CD中
    /// </summary>
    private bool m_IsInCooldowning = false;

    public HudSkillPanel() : base(UIPanel.HudSkillPanel, ASSET_ADDRESS, PanelType.Hud) { }

    public override void Initialize()
    {
        m_SkillButton1 = FindComponent<RectTransform>("Content/SkillButtonList/SkillButton1");
        m_SkillButton2 = FindComponent<RectTransform>("Content/SkillButtonList/SkillButton2");
        m_SkillButton3 = FindComponent<RectTransform>("Content/SkillButtonList/SkillButton3");
        m_SkillButton4 = FindComponent<RectTransform>("Content/SkillButtonList/SkillButton4");

        m_SkillHotkey1 = m_SkillButton1.Find("HotKey").GetComponent<RectTransform>();
        m_SkillHotkey2 = m_SkillButton2.Find("HotKey").GetComponent<RectTransform>();
        m_SkillHotkey3 = m_SkillButton3.Find("HotKey").GetComponent<RectTransform>();
        m_SkillHotkey4 = m_SkillButton4.Find("HotKey").GetComponent<RectTransform>();
    }

    public override void OnShow(object msg)
    {
        base.OnShow(msg);

        m_SkillProxy = GameFacade.Instance.RetrieveProxy(ProxyName.PlayerSkillProxy) as PlayerSkillProxy;
        m_GameplayProxy = GameFacade.Instance.RetrieveProxy(ProxyName.GameplayProxy) as GameplayProxy;

        AddHotKey(HotKeyMapID.SHIP, HotKeyID.ShipSkill1, OnSkill1, m_SkillHotkey1, "", HotkeyManager.HotkeyStyle.UI_SIMPLE);
        AddHotKey(HotKeyMapID.SHIP, HotKeyID.ShipSkill2, OnSkill2, m_SkillHotkey2, "", HotkeyManager.HotkeyStyle.UI_SIMPLE);
        AddHotKey(HotKeyMapID.SHIP, HotKeyID.ShipSkill3, OnSkill3, m_SkillHotkey3, "", HotkeyManager.HotkeyStyle.UI_SIMPLE);
        AddHotKey(HotKeyMapID.SHIP, HotKeyID.ShipSkill4, OnSkill4, m_SkillHotkey4, "", HotkeyManager.HotkeyStyle.UI_SIMPLE);

        UpdateSkillIcon();
        UpdateSkillCooldown();

        StartUpdate();
    }

    public override void OnHide(object msg)
    {
        m_SkillProxy = null;
        m_GameplayProxy = null;

        base.OnHide(msg);
    }

    /// <summary>
    /// 热键1按下时
    /// </summary>
    /// <param name="callback">热键参数</param>
    private void OnSkill1(HotkeyCallback callback)
    {
        if (callback.started)
            ReleaseSkill(m_SkillButton1);
        else if (callback.performed)
            PlayAudio();
    }
    /// <summary>
    /// 热键2按下时
    /// </summary>
    /// <param name="callback">热键参数</param>
    private void OnSkill2(HotkeyCallback callback)
    {
        if (callback.started)
            ReleaseSkill(m_SkillButton2);
        else if (callback.performed)
            PlayAudio();
    }
    /// <summary>
    /// 热键3按下时
    /// </summary>
    /// <param name="callback">热键参数</param>
    private void OnSkill3(HotkeyCallback callback)
    {
        if (callback.started)
            ReleaseSkill(m_SkillButton3);
        else if (callback.performed)
            PlayAudio();
    }
    /// <summary>
    /// 热键3按下时
    /// </summary>
    /// <param name="callback">热键参数</param>
    private void OnSkill4(HotkeyCallback callback)
    {
        if (callback.started)
            ReleaseSkill(m_SkillButton4);
        else if (callback.performed)
            PlayAudio();
    }

    /// <summary>
    /// 释放技能
    /// </summary>
    /// <param name="index">技能按钮</param>
    private void ReleaseSkill(RectTransform button)
    {
        if (button == null)
            return;

        if(IsBattling())
        {
            Animator btnAnimator = button.GetComponent<Animator>();
            if (btnAnimator)
                btnAnimator.SetTrigger("Pressed");
        }
    }

    /// <summary>
    /// 播放音效。
    /// </summary>
    private void PlayAudio()
    {
        if (IsBattling())
            WwiseUtil.PlaySound((int)WwiseMusic.Music_Hud_SkillKey_Switch, false, null);
    }

    public override NotificationName[] ListNotificationInterests()
    {
        return new NotificationName[]
        {
            NotificationName.MSG_SHIP_SKILL_CHANGED,
            NotificationName.MSG_SHIP_SKILL_CD_CHANGED,
        };
    }

    public override void HandleNotification(INotification notification)
    {
        if(notification.Name == NotificationName.MSG_SHIP_SKILL_CHANGED)
        {
            UpdateSkillIcon();
            UpdateSkillCooldown();
        }else if(notification.Name == NotificationName.MSG_SHIP_SKILL_CD_CHANGED)
        {
            UpdateSkillCooldown();
        }
    }

    /// <summary>
    /// 更新技能图标（打开时、技能升级时）
    /// </summary>
    private void UpdateSkillIcon()
    {
        PlayerShipSkillVO[] skills = m_SkillProxy.GetShipSkills();

        if (skills == null || skills.Length == 0)
            return;
            
        if (skills.Length > 0)
            UpdateSkillIcon(m_SkillButton1, skills[0]);
        if (skills.Length > 1)
            UpdateSkillIcon(m_SkillButton2, skills[1]);
        if (skills.Length > 2)
            UpdateSkillIcon(m_SkillButton3, skills[2]);
        if (skills.Length > 3)
            UpdateSkillIcon(m_SkillButton4, skills[3]);
    }

    /// <summary>
    /// 更新技能图标（打开时、技能升级时）
    /// </summary>
    /// <param name="button">按钮</param>
    /// <param name="skill">数据</param>
    private void UpdateSkillIcon(RectTransform button, PlayerShipSkillVO skill)
    {
        Image icon = button.Find("Image_SkillIcon").GetComponent<Image>();

        bool skillIsValid = skill != null;
        if (skill != null)
        {
            UIUtil.SetIconImage(icon, skillIsValid ? TableUtil.GetIconBundle((uint)skill.GetIcon()) : "",
                                      skillIsValid ? TableUtil.GetIconAsset((uint)skill.GetIcon()) : "");
        }
        else
        {
            icon.sprite = null;
            icon.gameObject.SetActive(skillIsValid);
        }
    }

    /// <summary>
    /// 更新
    /// </summary>
    protected override void Update()
    {
        UpdateSkillCooldown();
    }

    /// <summary>
    /// 更新技能冷却
    /// </summary>
    private void UpdateSkillCooldown()
    {
        PlayerShipSkillVO[] skills = m_SkillProxy.GetShipSkills();

        bool cooldowning = false;
        if (skills != null)
        {
            if (skills.Length > 0)
                cooldowning |= UpdateSkillCooldown(m_SkillButton1, skills[0]);
            if (skills.Length > 1)
                cooldowning |= UpdateSkillCooldown(m_SkillButton2, skills[1]);
            if (skills.Length > 2)
                cooldowning |= UpdateSkillCooldown(m_SkillButton3, skills[2]);
            if (skills.Length > 3)
                cooldowning |= UpdateSkillCooldown(m_SkillButton4, skills[3]);
        }
        if(m_IsInCooldowning != cooldowning)
        {
            m_IsInCooldowning = cooldowning;
            if(m_IsInCooldowning)
            {
                StartUpdate();
            }else
            {
                StopUpdate();
            }
        }
    }

    /// <summary>
    /// 更新技能冷却
    /// </summary>
    /// <param name="button">技能按钮</param>
    /// <param name="skill">技能数据</param>
    private bool UpdateSkillCooldown(RectTransform button, PlayerShipSkillVO skill)
    {
        //cd
        Animator animator = button.GetComponent<Animator>();
        Transform cdBox = button.Find("CDImage").GetComponent<Transform>();
        Image cdMask = button.Find("CDImage/CD").GetComponent<Image>();
        TMP_Text cdField = button.Find("CDImage/Label_CDTimes").GetComponent<TMP_Text>();

        float cooldown = 0;
        if (skill != null)
        {
            PlayerSkillCDVO cdVO = m_SkillProxy.GetActiveCDVO(skill.GetID());
            if(cdVO!=null)
            {
                cooldown = cdVO.GetRemainingTime();
            }

            if(cooldown>0.0f)
            {
                cdBox.gameObject.SetActive(true);

                cdMask.fillAmount = cdVO.GetProgress();
                cdField.text = string.Format("{0:N1}", cooldown);
            }else
            {
                cdBox.gameObject.SetActive(false);
            }
        }else
        {
            cdBox.gameObject.SetActive(false);
        }

        if (animator)
        {
            animator.SetInteger("State", cooldown > 0 ? 2 : 0);
        }

        return cooldown > 0.0f;
    }
}