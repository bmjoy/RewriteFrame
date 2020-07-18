using PureMVC.Interfaces;
using System.Collections;
using TMPro;
using UnityEngine;

public class HudNoticePanel : HudBase
{
    private const string ASSET_ADDRESS = Leyoutech.Core.Loader.Config.AssetAddressKey.PRELOADUI_HUD_MESSAGETOP;

    /// <summary>
    /// 动画控制器
    /// </summary>
    private Animator m_Animator;
    /// <summary>
    /// 文字内容框
    /// </summary>
    private TMP_Text m_Notice;
    /// <summary>
    /// 战斗图标
    /// </summary>
    private RectTransform m_BattleIcon;

    public HudNoticePanel() : base(UIPanel.HudNoticePanel, ASSET_ADDRESS, PanelType.Hud) { }

    public override void Initialize()
    {
        m_Animator = FindComponent<Animator>("Content/Message");
        m_Notice = FindComponent<TMP_Text>("Content/Message/Label_Message");
        m_BattleIcon = FindComponent<RectTransform>("Content/Message/CombatIcon");
    }

    public override void OnShow(object msg)
    {
        base.OnShow(msg);
        m_Animator.gameObject.SetActive(false);
    }

    public override NotificationName[] ListNotificationInterests()
    {
        return new NotificationName[]
        {
            NotificationName.MSG_CHANGE_BATTLE_STATE,
            NotificationName.MSG_CHANGE_BATTLE_STATE_FAIL
        };
    }

    public override void HandleNotification(INotification notification)
    {
        switch ((NotificationName)notification.Name)
        {
            case NotificationName.MSG_CHANGE_BATTLE_STATE:
                OnStateChanged(notification.Body as ChangeBattleStateNotify);
                break;
            case NotificationName.MSG_CHANGE_BATTLE_STATE_FAIL:
                OnToggleBattleStateFail();
                break;
        }
    }

    /// <summary>
    /// 状态改变
    /// </summary>
    private void OnStateChanged(ChangeBattleStateNotify notice)
    {
        if (notice.NewMainState != notice.OldMainState && notice.IsSelf)
        {
            if (notice.NewMainState == EnumMainState.Fight && (notice.OldMainState == EnumMainState.Cruise || notice.OldMainState == EnumMainState.Born))
                UIManager.Instance.StartCoroutine(DelayShow(TableUtil.GetLanguageString("hud_text_id_1001")));
            else if (notice.NewMainState == EnumMainState.Cruise && notice.OldMainState == EnumMainState.Fight)
                UIManager.Instance.StartCoroutine(DelayShow(TableUtil.GetLanguageString("hud_text_id_1002")));
        }
    }

    /// <summary>
    /// 切换战斗状态失败
    /// </summary>
    private void OnToggleBattleStateFail()
    {
        ShowNow(TableUtil.GetLanguageString("hud_text_id_1043"));
        //UIManager.Instance.StartCoroutine(DelayShow(TableUtil.GetLanguageString("hud_text_id_1043")));
    }

    private void ShowNow(string notice)
    {
        if (!IsDead())
        {
            m_Notice.text = notice;
            m_BattleIcon.gameObject.SetActive(false);
            m_Animator.gameObject.SetActive(false);
            m_Animator.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// 延迟协程
    /// </summary>
    /// <param name="notice">ChangeBattleStateNotify消息</param>
    /// <returns>IEnumerator</returns>
    private IEnumerator DelayShow(string notice)
    {
        yield return new WaitForSeconds(0.5f);

        if (!IsDead())
        {

            m_Notice.text = notice;
            m_BattleIcon.gameObject.SetActive(false);
            m_Animator.gameObject.SetActive(false);
            m_Animator.gameObject.SetActive(true);
        }
    }
}
