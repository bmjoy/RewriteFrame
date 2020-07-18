using System;
using PureMVC.Interfaces;
using TMPro;
using UnityEngine;

public class HudDangerTipPanel : HudBase
{
    private const string ASSET_ADDRESS = Leyoutech.Core.Loader.Config.AssetAddressKey.PRELOADUI_HUD_DANGERTIPPANEL;

    /// <summary>
    /// 时间字段(秒)
    /// </summary>
    private TMP_Text m_TimerText;
    /// <summary>
    /// 时间字段(毫秒)
    /// </summary>
    private TMP_Text m_MsTimerText;
    /// <summary>
    /// 时间字段(毫秒)
    /// </summary>
    private GameObject m_MsTimeGameObject;

    /// <summary>
    ///上一次update的服务器时间
    /// </summary>
    private ulong m_ForeFrameTime;
    /// <summary>
    ///上一秒的服务器时间
    /// </summary>
    private ulong m_SecondTime;
    /// <summary>
    ///倒计时时间
    /// </summary>
    private int m_Timer;
    /// <summary>
    ///是否播了毫秒音效
    /// </summary>
    private bool m_IsPlaySMusic = false;
    /// <summary>
    /// 回调
    /// </summary>
    private Action m_CallBackAction = null;

    public HudDangerTipPanel() : base(UIPanel.HudDangerTipPanel, ASSET_ADDRESS, PanelType.Hud) { }

    public override void Initialize()
    {
        m_TimerText = FindComponent<TMP_Text>("Base/Label_Number");
        m_MsTimerText = FindComponent<TMP_Text>("Base/Label_Number2");
        m_MsTimeGameObject = (m_MsTimerText != null) ? m_MsTimerText.gameObject : null;
    }

    public override void OnShow(object msg)
    {
        base.OnShow(msg);
        StopUpdate();
        MSG_TimeInfo timeInfo = msg as MSG_TimeInfo;
        if (timeInfo != null && timeInfo.time > 0)
        {
            m_TimerText.gameObject.SetActive(true);
            m_CallBackAction = timeInfo.CallbackAction;
            uint time = timeInfo.time / 1000;
            uint min = time / 60;
            uint sec = time - 60 * min;
            uint ssec = timeInfo.time - 60 * min * 1000 - sec * 1000;
            string ms = min.ToString();
            string ss = sec.ToString();

            if (min > 0)
            {
                if (sec < 10)
                    ss = 0 + ss;
                m_TimerText.text = ms + ":" + ss;
                if (m_MsTimeGameObject)
                    m_MsTimeGameObject.SetActive(min <= 0 && sec < 10);
            }
            else if(min <= 0)
            {
                if (sec < 10)
                {
                    m_IsPlaySMusic = true;
                    WwiseUtil.PlaySound(51, false, m_TimerText.transform);
                    m_TimerText.text = ss;
                    m_MsTimerText.text = "." + ssec;
                }
                else
                    m_TimerText.text = ss;
            }

            m_ForeFrameTime = ServerTimeUtil.Instance.GetNowTimeMSEL();
            m_SecondTime = m_ForeFrameTime;
            m_Timer = (int)timeInfo.time;
            StartUpdate();
        }
        else
        {
            WwiseUtil.PlaySound(53, false, m_TimerText.transform);
            m_IsPlaySMusic = false;
            m_TimerText.gameObject.SetActive(false);

            m_MsTimeGameObject.SetActive(false);
        }

    }

    public override void OnHide(object msg)
    {
        base.OnHide(msg);
        if (m_IsPlaySMusic)
        {
            WwiseUtil.PlaySound(53, false, m_TimerText.transform);
            m_IsPlaySMusic = false;
        }
    }

    protected override void Update()
    {
        if (m_Timer > 0)
        {
            GetTransform().GetChild(0).gameObject.SetActive(!IsWatchOrUIInputMode() && !IsDead() && !IsLeaping());
            GetTransform().GetChild(1).gameObject.SetActive(!IsWatchOrUIInputMode() && !IsDead() && !IsLeaping());
            ulong currentTime = ServerTimeUtil.Instance.GetNowTimeMSEL();
            int time = (int)(currentTime - m_ForeFrameTime);
            ulong second = currentTime - m_SecondTime;
            m_Timer = (int)(m_Timer - time);
            if (m_Timer < 0)
                m_Timer = 0;
            int ts = m_Timer / 1000;
            int min = ts / 60;
            int sec = ts - 60 * min;
            int ssec = m_Timer - 60 * min * 1000 - sec * 1000;
            string ms = min.ToString();
            string ss = sec.ToString();
            bool isactive = min <= 0 && sec < 10;
            if (m_MsTimeGameObject && m_MsTimeGameObject.activeSelf != isactive)
                m_MsTimeGameObject.SetActive(isactive);
            if (min > 0)
            {
                if (sec < 10)
                    ss = 0 + ss;
                m_TimerText.text = ms + ":" + ss;
            }
            else if (min <= 0)
            {
                if (sec < 10)
                {
                    m_TimerText.text = ss;
                    m_MsTimerText.text = "." + ssec;
                    if (!m_IsPlaySMusic)
                    {
                        m_IsPlaySMusic = true;
                        WwiseUtil.PlaySound(51, false, m_TimerText.transform);
                    }
                }
                else
                    m_TimerText.text = ss;
            }
            m_ForeFrameTime = currentTime;
            if (second >= 1000 && m_IsPlaySMusic)
            {
                WwiseUtil.PlaySound(52, false, m_TimerText.transform);
                m_SecondTime = currentTime;

            }
        }
        else
        {
            m_CallBackAction?.Invoke();
            WwiseUtil.PlaySound(53, false, m_TimerText.transform);
            m_IsPlaySMusic = false;
            StopUpdate();
            UIManager.Instance.ClosePanel(this);
        }
    }
}
