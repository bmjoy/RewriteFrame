using Eternity.FlatBuffer;
using Leyoutech.Core.Loader.Config;
using PureMVC.Interfaces;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AccountUpaPanel : UIPanelBase
{
    /// <summary>
    /// 段位图标
    /// </summary>
    private Image m_UpaIcon;
    /// <summary>
    /// 段位等级文本
    /// </summary>
    private TMP_Text m_LevelLabel;
    /// <summary>
    /// 段位名称
    /// </summary>
    private TMP_Text m_Name;
    /// <summary>
    /// 奖励容器
    /// </summary>
    private Transform m_RewardContainer;
    /// <summary>
    /// 热键容器
    /// </summary>
    private Transform m_HotkeyRoot;
    /// <summary>
    /// 段位等级
    /// </summary>
    private uint m_Level = 1;
    /// <summary>
    /// CfgEternityProxy
    /// </summary>
    private CfgEternityProxy m_CfgEternityProxy;
    /// <summary>
    /// 段位数据
    /// </summary>
    private LevelUpa m_LevelUpa;
    /// <summary>
    /// 奖励列表
    /// </summary>
    private List<RewardDateVO> m_RewarList = new List<RewardDateVO>();
    public AccountUpaPanel() : base(UIPanel.AccountUpaPanel, AssetAddressKey.PRELOADUI_ACCOUNTUPAPANEL, PanelType.Normal)
    {

    }

    public override void Initialize()
    {
        m_UpaIcon = FindComponent<Image>("Content/Describe/Middle/Image_RankIcon");
        m_LevelLabel = FindComponent<TMP_Text>("Content/RankItem/Label_Rank");
        m_Name = FindComponent<TMP_Text>("Content/RankItem/Label_RankName");
        m_RewardContainer = FindComponent<Transform>("Content/Other/Reward/IconItem");
        m_HotkeyRoot = FindComponent<Transform>("Control/Content/View_Bottom/List");
        m_CfgEternityProxy = GameFacade.Instance.RetrieveProxy(ProxyName.CfgEternityProxy) as CfgEternityProxy;
    }

    public override void OnRefresh(object msg)
    {
        m_Level = NetworkManager.Instance.GetPlayerController().GetPlayerInfo().WatchLv;
        m_LevelUpa = m_CfgEternityProxy.GetPlayerUpa(m_Level);
        UIUtil.SetIconImage(m_UpaIcon, (uint)m_LevelUpa.Icon);
        m_LevelLabel.text = m_Level.ToString();
        m_Name.text = m_LevelUpa.Name;
        LevelRewards();
    }

    public override void OnGotFocus()
    {
        AddHotKey(HotKeyID.FuncX, ClosePanel, m_HotkeyRoot, TableUtil.GetLanguageString("common_hotkey_id_1001"));
    }
    public override NotificationName[] ListNotificationInterests()
    {
        return new NotificationName[]
        {
            NotificationName.MSG_LEVELUP_REWARD_LIST,
        };
    }
    public override void HandleNotification(INotification notification)
    {
        switch (notification.Name)
        {
            case NotificationName.MSG_LEVELUP_REWARD_LIST:
                m_RewarList = (List<RewardDateVO>)notification.Body;
                LevelRewards();
                break;
        }
    }
    /// <summary>
    /// 等级奖励
    /// </summary>
    private void LevelRewards()
    {
        int index = 0;
        if (m_RewarList.Count == 0)
        {
            m_RewardContainer.gameObject.SetActive(false);
            return;
        }
        m_RewardContainer.gameObject.SetActive(true);
        for (; index < m_RewarList.Count; index++)
        {
            RewardDateVO mailReward = m_RewarList[index];
            Transform node = index < m_RewardContainer.childCount ? m_RewardContainer.GetChild(index) : Object.Instantiate(m_RewardContainer.GetChild(0), m_RewardContainer);
            Image icon = FindComponent<Image>(node, "Icon");
            Image quality = FindComponent<Image>(node, "Quality");
            TMP_Text num = FindComponent<TMP_Text>(node, "label_Num");
            num.text = mailReward.Num.ToString();
            quality.color = ColorUtil.GetColorByItemQuality(mailReward.Quality);
            string iconName = TableUtil.GetItemSquareIconImage(mailReward.Id);
            UIUtil.SetIconImage(icon, TableUtil.GetItemIconBundle(mailReward.Id), iconName);
            node.gameObject.SetActive(true);
        }
        for (; index < m_RewardContainer.childCount; index++)
        {
            m_RewardContainer.GetChild(index).gameObject.SetActive(false);
        }
    }
    /// <summary>
    /// 关闭面板
    /// </summary>
    /// <param name="callbackContext"></param>
    private void ClosePanel(HotkeyCallback callbackContext)
    {
        if (callbackContext.performed)
        {
            UIManager.Instance.ClosePanel(this);
        }
    }

    public override void OnHide(object msg)
    {
        m_RewarList.Clear();
        base.OnHide(msg);
    }
}
