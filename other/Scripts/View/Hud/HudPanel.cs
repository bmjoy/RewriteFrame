using Assets.Scripts.Define;
using PureMVC.Interfaces;
using UnityEngine;

class HudPanel : HudBase
{
    private const string ASSET_ADDRESS = Leyoutech.Core.Loader.Config.AssetAddressKey.PRELOADUI_HUD_PANEL;

    /// <summary>
    /// 根动画
    /// </summary>
    private Animator m_Animator;
    /// <summary>
    /// 技能
    /// </summary>
    private RectTransform m_SkillBox;
    /// <summary>
    /// 转化炉
    /// </summary>
    private RectTransform m_SpecialSkillBox;
    /// <summary>
    /// 武器容器
    /// </summary>
    private RectTransform m_WeaponBox;
    /// <summary>
    /// 战斗状态倒计时
    /// </summary>
    private RectTransform m_FightStateBox;
    /// <summary>
    /// 地图容器
    /// </summary>
    private RectTransform m_MapBox;
    /// <summary>
    /// 道具容器
    /// </summary>
    private RectTransform m_ItemBox;
    /// <summary>
    /// 任务面板
    /// </summary>
    private RectTransform m_MissionBox;
    /// <summary>
    /// 技能道具容器
    /// </summary>
    private RectTransform m_SkillItemBox;


    public HudPanel() : base(UIPanel.HudPanel, ASSET_ADDRESS, PanelType.Hud) { }

    public override void Initialize()
    {
        m_Animator = GetTransform().GetComponent<Animator>();
        m_SkillBox = FindComponent<RectTransform>("Content/SkillBox/Box");
        m_SpecialSkillBox = FindComponent<RectTransform>("Content/SpecialSkillBox/Box");
        m_WeaponBox = FindComponent<RectTransform>("Content/WeaponBigBox/Box");
        m_FightStateBox = FindComponent<RectTransform>("Content/FCBox/Box");
        m_MapBox = FindComponent<RectTransform>("Content/MapBox/Box");
        m_ItemBox = FindComponent<RectTransform>("Content/Item/Box");
        m_MissionBox = FindComponent<RectTransform>("Content/MissionBox");
        m_SkillItemBox = FindComponent<RectTransform>("Content/SkillItemBox/Box");
    }

    public override void OnShow(object msg)
    {
        base.OnShow(msg);

        UIManager.Instance.OpenPanel(UIPanel.HudSkillPanel, m_SkillBox);
        UIManager.Instance.OpenPanel(UIPanel.HudFurnacePanel, m_SpecialSkillBox);
        UIManager.Instance.OpenPanel(UIPanel.HudWeaponPanel, m_WeaponBox);
        UIManager.Instance.OpenPanel(UIPanel.HudFightCountdownPanel, m_FightStateBox);
        UIManager.Instance.OpenPanel(UIPanel.HudMapPanel, m_MapBox);

        RefreshAnimState();

        AddHotKey(HotKeyMapID.SHIP, HotKeyID.DebugKey, OnDebugClick);
    }

    public override void OnHide(object msg)
    {
        UIManager.Instance.ClosePanel(UIPanel.HudSkillPanel);
        UIManager.Instance.ClosePanel(UIPanel.HudFurnacePanel);
        UIManager.Instance.ClosePanel(UIPanel.HudWeaponPanel);
        UIManager.Instance.ClosePanel(UIPanel.HudFightCountdownPanel);
        UIManager.Instance.ClosePanel(UIPanel.HudMapPanel);

        base.OnHide(msg);
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
            case NotificationName.MainHeroRevive:
                RefreshAnimState();
                if (notification.Name == NotificationName.MainHeroRevive)
                {
                    UIManager.Instance.OpenPanel(UIPanel.HudSceneNamePanel);
                    UIManager.Instance.OpenPanel(UIPanel.HudAreaNamePanel);
                }
                break;
            case NotificationName.MainHeroDeath:
                RefreshAnimState();
                UIManager.Instance.CloseAllWindow();
                UIManager.Instance.OpenPanel(UIPanel.RevivePanel, notification.Body);
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
    /// 输入图改变
    /// </summary>
    protected override void OnInputMapChanged()
    {
        RefreshAnimState();
    }

    /// <summary>
    /// 设置跃迁状态
    /// </summary>
    private void RefreshAnimState()
    {
        SpacecraftEntity main = GetMainEntity();
        if (main != null)
        {
            //当前状态
            bool isBattle = IsBattling();
            bool isBurstActivated = IsPeerless();

            if(IsDead() || IsWatchOrUIInputMode())
                m_Animator.SetInteger("State", 0);
            else if (isBurstActivated)
                m_Animator.SetInteger("State", 3);
            else if (isBattle)
                m_Animator.SetInteger("State", 2);
            else
                m_Animator.SetInteger("State", 1);
        }
        else
            m_Animator.SetInteger("State", 0);
    }

    /// <summary>
    /// 处理debug键
    /// </summary>
    /// <param name="callback">热键信息</param>
    private void OnDebugClick(HotkeyCallback callback)
    {
        if (callback.performed)
        {
            UIManager.Instance.OpenPanel(UIPanel.WarshipDialogPanel);

            //UIManager.Instance.OpenPanel(PanelName.HudOpenBoxPanel);
            //UIManager.Instance.OpenPanel(PanelName.ShopPanel);
        }
    }
}
