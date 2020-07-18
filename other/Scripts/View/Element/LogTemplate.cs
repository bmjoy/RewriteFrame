using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LogTemplate : MonoBehaviour
{
    /// <summary>
    /// 日志内容根节点
    /// </summary>
    private Transform m_Root;
    /// <summary>
    /// 物品图标
    /// </summary>
    private Image m_Icon;
    /// <summary>
    /// 覆盖图标
    /// </summary>
    private Image m_OverlyingIcon;
    /// <summary>
    /// 日志描述
    /// </summary>
    private TMP_Text m_Describe;
    /// <summary>
    /// 日志接收时间
    /// </summary>
    private TMP_Text m_ReceiveTime;

    private LogDataVO m_LogDataVO;
    /// <summary>
    /// 初始化控件
    /// </summary>
    public void Init(LogDataVO dataVO)
    {
        if (m_Root == null)
        {
            m_Root = TransformUtil.FindUIObject<Transform>(transform, "Content");
        }
        if (m_Icon == null)
        {
            m_Icon = TransformUtil.FindUIObject<Image>(transform, "Content/Image_Icon");
        }
        if (m_OverlyingIcon == null)
        {
            m_OverlyingIcon = TransformUtil.FindUIObject<Image>(transform, "Content/Image_Icon2");
        }
        if (m_Describe == null)
        {
            m_Describe = TransformUtil.FindUIObject<TMP_Text>(transform, "Content/Label_Sender");
        }
        if (m_ReceiveTime == null)
        {
            m_ReceiveTime = TransformUtil.FindUIObject<TMP_Text>(transform, "Content/Label_LeftTime");
        }
        m_LogDataVO = dataVO;
        SetContent(dataVO);
    }

    /// <summary>
    /// 设置日志内容显示
    /// </summary>
    private void SetContent(LogDataVO dataVO)
    {        
        UIUtil.SetIconImage(m_Icon, TableUtil.GetItemIconBundle(dataVO.Tid), TableUtil.GetItemIconImage(dataVO.Tid));
        UIUtil.SetIconImage(m_OverlyingIcon, TableUtil.GetItemIconBundle(dataVO.Tid), TableUtil.GetItemIconImage(dataVO.Tid));      
        Color m_QualityColor = ColorUtil.GetColorByItemQuality(TableUtil.GetItemQuality(dataVO.Tid));
        string m_Name = TableUtil.GetItemName(dataVO.Tid);
        string m_ColorText = ColorUtil.AddColor(m_Name, m_QualityColor);
        string m_ShowText = "";
        if (dataVO.Num > 1)
        {
            m_ShowText = string.Format(TableUtil.GetLanguageString("log_text_1011"), m_ColorText, dataVO.Num);
        }
        else
        {
            m_ShowText = string.Format(TableUtil.GetLanguageString("log_text_1010"), m_ColorText);
        }        
        m_Describe.text = string.Format(m_ShowText);
    }

    private void Update()
    {
        if (m_LogDataVO == null && m_LogDataVO.Id == "")
        {
            return;
        }
        DateTime m_StartTime = TimeUtil.GetDateTime(m_LogDataVO.ReveiveTime / 1000);
        DateTime m_EndTime = TimeUtil.GetDateTime(ServerTimeUtil.Instance.GetNowTime());
        if (m_EndTime.Year == m_StartTime.Year && m_EndTime.Month == m_StartTime.Month && m_EndTime.Day == m_StartTime.Day && m_EndTime.Hour < 24)
        {
            m_ReceiveTime.text = TableUtil.GetLanguageString("log_text_1001");
            return;
        }
        DateTime m_LastDay = m_EndTime.AddDays(-1);
        if (m_StartTime.Year == m_LastDay.Year && m_StartTime.Month == m_LastDay.Month && m_StartTime.Day == m_LastDay.Day && m_StartTime.Hour < 24)
        {
            m_ReceiveTime.text = TableUtil.GetLanguageString("log_text_1002");
            return;
        }
        ulong m_SpendTime = ServerTimeUtil.Instance.GetNowTime() - m_LogDataVO.ReveiveTime / 1000;
        //收到日志时长
        long days = 0;
        long hours = 0;
        long minutes = 0;
        long seconds = 0;
        TimeUtil.Time_msToMinutesAndSeconds(m_SpendTime, ref days, ref hours, ref minutes, ref seconds);
        if (days < 7)
        {
            DateTime dateTime = TimeUtil.GetDateTime(m_LogDataVO.ReveiveTime / 1000);
            switch (dateTime.DayOfWeek)
            {
                case DayOfWeek.Monday:
                    m_ReceiveTime.text = TableUtil.GetLanguageString("log_text_1003");
                    break;
                case DayOfWeek.Tuesday:
                    m_ReceiveTime.text = TableUtil.GetLanguageString("log_text_1004");
                    break;
                case DayOfWeek.Wednesday:
                    m_ReceiveTime.text = TableUtil.GetLanguageString("log_text_1005");
                    break;
                case DayOfWeek.Thursday:
                    m_ReceiveTime.text = TableUtil.GetLanguageString("log_text_1006");
                    break;
                case DayOfWeek.Friday:
                    m_ReceiveTime.text = TableUtil.GetLanguageString("log_text_1007");
                    break;
                case DayOfWeek.Saturday:
                    m_ReceiveTime.text = TableUtil.GetLanguageString("log_text_1008");
                    break;
                case DayOfWeek.Sunday:
                    m_ReceiveTime.text = TableUtil.GetLanguageString("log_text_1009");
                    break;
            }
        }
        else
        {
            m_ReceiveTime.text = TimeUtil.GetDateTimeToString(m_LogDataVO.ReveiveTime / 1000);
        }
    }
}

