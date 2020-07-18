using Assets.Scripts.Define;
using Assets.Scripts.Proto;
using DG.Tweening;
using PureMVC.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class HudSkillProgress : UIPanelBase
{
    private const string ASSET_ADDRESS = Leyoutech.Core.Loader.Config.AssetAddressKey.PRELOADUI_HUD_SKILLPROGRESS;

    /// <summary>
    /// 内容节点
    /// </summary>
	private RectTransform m_Content;
    /// <summary>
    /// 进条条
    /// </summary>
	private Image m_Progress;

    /// <summary>
    /// 本次计时的最大时间
    /// </summary>
    private float m_TotalTime;
    /// <summary>
    /// 本次计时到目前为止已经走过的时间
    /// </summary>
    private float m_CurrentTime;
    /// <summary>
    /// 是否已中断
    /// </summary>
	private bool m_Interrupted;

    /// <summary>
    /// 协程
    /// </summary>
    private Coroutine m_Coroutine;


    public HudSkillProgress() : base(UIPanel.HudSkillProgress, ASSET_ADDRESS, PanelType.Hud) { }

    public override void Initialize()
    {
        m_Content = FindComponent<RectTransform>("Content");
        m_Progress = FindComponent<Image>("Content/Progress");
    }

    public override NotificationName[] ListNotificationInterests()
    {
        return new NotificationName[]
        {
            NotificationName.MSG_SKILL_START_CHANTING,
            NotificationName.MSG_SKILL_STOP_CHANTING,
            NotificationName.MSG_SKILL_START_CHARGING,
            NotificationName.MSG_SKILL_STOP_CHARGING,
            NotificationName.MSG_SKILL_START_CHANNELLING,
            NotificationName.MSG_SKILL_STOP_CHANNELLING,
            NotificationName.ShowSkillProgressBar,
            NotificationName.HidSkillProgressBar
            };
    }


    public override void HandleNotification(INotification notification)
    {
        switch (notification.Name)
        {
            case NotificationName.MSG_SKILL_START_CHANTING:
            case NotificationName.MSG_SKILL_START_CHARGING:
            case NotificationName.MSG_SKILL_START_CHANNELLING:
                StartProgress(notification.Body as SkillStartPeriodNotify);
                break;
            case NotificationName.ShowSkillProgressBar:
                StartTimeProgress(notification.Body as MsgShowSkillTimeProgressBar);
                break;
            case NotificationName.MSG_SKILL_STOP_CHANTING:
            case NotificationName.MSG_SKILL_STOP_CHARGING:
            case NotificationName.MSG_SKILL_STOP_CHANNELLING:
                StopProgress(notification.Body as SkillFinishPeriodNotify);
                break;
            case NotificationName.HidSkillProgressBar:
                StopTimeProgress(notification.Body as MsgHideSkillTimeProgressBar);
                break;
        }
    }

    private IEnumerator SkillPeriodProgress()
    {
        while (m_CurrentTime < m_TotalTime)
        {
            if (m_Interrupted)
            {
                m_CurrentTime = m_TotalTime;
                continue;
            }

            m_CurrentTime += Time.deltaTime;
            m_CurrentTime = m_CurrentTime > m_TotalTime ? m_TotalTime : m_CurrentTime;
            m_Progress.fillAmount = m_CurrentTime / m_TotalTime;

            yield return new WaitForEndOfFrame();
        }

        m_Content.gameObject.SetActive(false);
    }

    public override void OnRefresh(object msg)
    {
        if (!m_Content.gameObject.activeInHierarchy)
            return;

        m_CurrentTime += Time.deltaTime;
        m_CurrentTime = m_CurrentTime > m_TotalTime ? m_TotalTime : m_CurrentTime;
        m_Progress.fillAmount = m_CurrentTime / m_TotalTime;
    }

    /// <summary>
    /// 目标展示窗口不关闭, 只是控制Content节点的显隐来控制是否显示
    /// </summary>
    private void StartProgress(SkillStartPeriodNotify notify)
    {
        if (notify == null)
        {
            Debug.LogError("传入参数错误");
            return;
        }

        m_Content.gameObject.SetActive(true);
        m_TotalTime = notify.Duration;
        m_CurrentTime = notify.BeginTime;
        m_Interrupted = false;

        m_Coroutine = UIManager.Instance.StartCoroutine(SkillPeriodProgress());
    }

    /// <summary>
    /// 目标展示窗口不关闭, 只是控制Content节点的显隐来控制是否显示
    /// </summary>
    private void StopProgress(SkillFinishPeriodNotify notify)
    {
        if (notify == null)
        {
            Debug.LogError("传入参数错误");
            return;
        }

        // 本次吟唱/蓄力/引导是不是成功结束. 这关系到播放不同的结束动画
        m_Interrupted = !notify.Success;

        m_Content.gameObject.SetActive(false);
        m_TotalTime = 0;
        m_CurrentTime = 0f;
    }


    /// <summary>
    ///显示时间类型进度条
    /// </summary>
    private void StartTimeProgress(MsgShowSkillTimeProgressBar notify)
    {
        if (notify == null)
        {
            Leyoutech.Utility.DebugUtility.LogWarning("技能进度条UI", "UI  参数错误！");
            return;
        }

        m_Content.gameObject.SetActive(true);
        m_TotalTime = notify.Duration;
        m_CurrentTime = 0;
        m_Interrupted = false;

        m_Coroutine = UIManager.Instance.StartCoroutine(SkillPeriodProgress());
    }

    /// <summary>
    /// 隐藏时间类型进度条
    /// </summary>
    private void StopTimeProgress(MsgHideSkillTimeProgressBar notify)
    {
        if (notify == null)
        {
            Leyoutech.Utility.DebugUtility.LogWarning("技能进度条UI", "UI  参数错误！");
            return;
        }

        m_Content.gameObject.SetActive(false);
        m_TotalTime = 0;
        m_CurrentTime = 0f;
        if (m_Coroutine != null)
            UIManager.Instance.StopCoroutine(m_Coroutine);
    }
}
