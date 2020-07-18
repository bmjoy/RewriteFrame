using Assets.Scripts.Define;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MailTemplate : MonoBehaviour, IPointerEnterHandler
{   
    /// <summary>
    /// 邮件内容根节点
    /// </summary>
    private Transform m_Root;
    /// <summary>
    /// 邮件图标
    /// </summary>
    private Image m_MailIcon;
    /// <summary>
    /// 覆盖图标
    /// </summary>
    private Image m_OverlyingIcon;
    /// <summary>
    /// 邮件名称
    /// </summary>
    private TextMeshProUGUI m_MailName;
    /// <summary>
    /// 过期时间
    /// </summary>
    private TextMeshProUGUI m_ResidualTime;
    /// <summary>
    /// 发件人
    /// </summary>
    private TextMeshProUGUI m_SendName;
    /// <summary>
    /// 新邮件标志
    /// </summary>
    private Transform m_MailNew;
    /// <summary>
    /// 邮件标记标志
    /// </summary>
    private Transform m_MailMark;
    /// <summary>
    /// 邮件id
    /// </summary>
    private string m_Mailid;
    /// <summary>
    /// 邮件数据
    /// </summary>
    private MailDataVO m_Data;
    /// <summary>
    /// 邮件proxy
    /// </summary>
    private MailProxy m_MailProxy;
    /// <summary>
    /// 鼠标划过事件
    /// </summary>
    private UnityAction<string> m_Hover;
    /// <summary>
    /// 初始化内容并设置Tips
    /// </summary>
    /// <param name="mailDate"></param>
    public void Init(MailDataVO mailDate,UnityAction<string> action)
    {
        if (m_Root == null)
        {
            m_Root = TransformUtil.FindUIObject<Transform>(transform, "Content");
        }
        if (m_MailIcon == null)
        {
            m_MailIcon = TransformUtil.FindUIObject<Image>(transform, "Content/Image_Icon");
        }
        if (m_OverlyingIcon == null)
        {
            m_OverlyingIcon = TransformUtil.FindUIObject<Image>(transform, "Content/Image_Icon2");
        }
        if (m_SendName == null)
        {
            m_SendName = TransformUtil.FindUIObject<TextMeshProUGUI>(transform, "Content/Label_Name");
        }
        if (m_ResidualTime == null)
        {
            m_ResidualTime = TransformUtil.FindUIObject<TextMeshProUGUI>(transform, "Content/Label_LeftTime");
        }
        if (m_MailName == null)
        {
            m_MailName = TransformUtil.FindUIObject<TextMeshProUGUI>(transform, "Content/Label_Sender");
        }
        if (m_MailNew == null)
        {
            m_MailNew = TransformUtil.FindUIObject<Transform>(transform, "Content/Image_New");
        }
        if (m_MailMark == null)
        {
            m_MailMark = TransformUtil.FindUIObject<Transform>(transform, "Content/Image_Mark");
        }   
        m_Data = mailDate;
        m_Hover = action;
        SetContent();
    }

    /// <summary>
    /// 悬停到邮件
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (m_Data.IsNew == 1)
        {           
            m_Data.IsNew = 0;
            m_MailNew.gameObject.SetActive(m_Data.IsNew == 1);
            m_Hover?.Invoke(m_Data.Id);
        }        
    }

    /// <summary>
    /// 设置邮件相关内容
    /// </summary>
    private void SetContent()
    {
        StopAllCoroutines();     
        if (m_Data.MaxCount == 0)
        {
            m_MailName.text = m_Data.Title;
        }
        else
        {
            string m_MailCount = string.Format("({0}/{1})", m_Data.Index, m_Data.MaxCount);
            m_MailName.text = m_Data.Title + m_MailCount;
        }
        MailState m_MailState;
        if (m_Data.Readed > 0)
        {
            if (m_Data.Got == 0 && m_Data.HasAccessory == 1)
            {
                m_MailState = MailState.Opened_Items;
            }
            else
            {
                m_MailState = MailState.Opened_Null;
            }
        }
        else
        {
            if (m_Data.Got == 0 && m_Data.HasAccessory == 1)
            {
                m_MailState = MailState.UnRead_Items;
            }
            else
            {
                m_MailState = MailState.UnRead_Null;
            }
        }
        string m_IconName = string.Format("Social_Icon_Mail0{0}", (int)m_MailState);
        UIUtil.SetIconImage(m_MailIcon, GameConstant.COMMON_ICON_ATLAS_ASSETADDRESS, m_IconName);
        UIUtil.SetIconImage(m_OverlyingIcon, GameConstant.COMMON_ICON_ATLAS_ASSETADDRESS, m_IconName);
        long spendTime = (long)ServerTimeUtil.Instance.GetNowTime() - m_Data.SendTime;
        long t = m_Data.ExpireTime - m_Data.SendTime - spendTime;
        if (t <= 0)
        {
            SetTime(0);
        }
        else
        {
            StartCoroutine(SetTime(t, true));
        }
        m_MailMark.gameObject.SetActive(m_Data.IsMark == 1);
        m_MailNew.gameObject.SetActive(m_Data.IsNew == 1);
        m_SendName.text = m_Data.Sender;
    }
    /// <summary>
    /// 过期时间倒计时
    /// </summary>
    /// <param name="time"></param>
    /// <param name="loop"></param>
    /// <returns></returns>
    private IEnumerator SetTime(long time, bool loop)
    {
        while (loop && time > 0)
        {
            SetTime(time);
            yield return new WaitForSecondsRealtime(1F);
            time--;
        }
        yield return 0;
    }
    /// <summary>
    /// 设置过期时间
    /// </summary>
    /// <param name="time"></param>
    private void SetTime(long time)
    {
        if (m_Data.ExpireTime == 0)
        {
            m_ResidualTime.text = TableUtil.GetLanguageString("mailbox_text_id_1025");
            return;
        }
        //剩余时间
        long days = 0;
        long hours = 0;
        long minutes = 0;
        long seconds = 0;
        string lefttime;
        TimeUtil.Time_msToMinutesAndSeconds(time, ref days, ref hours, ref minutes, ref seconds);

        if (days >= 1)
        {
            lefttime = string.Format(TableUtil.GetLanguageString("mailbox_text_id_1013"), days);
        }
        else
        {
            if (hours >= 1)
            {
                lefttime = string.Format(TableUtil.GetLanguageString("mailbox_text_id_1014"), hours);
            }
            else
            {
                if (minutes >= 1)
                {
                    lefttime = string.Format(TableUtil.GetLanguageString("mailbox_text_id_1015"), minutes);
                }
                else
                {
                    lefttime = string.Format(TableUtil.GetLanguageString("mailbox_text_id_1016"), seconds);
                }
            }
        }
        m_ResidualTime.text = lefttime;
    }
}

public enum MailState
{
    /// <summary>
    /// 空状态
    /// </summary>
    Null = 0,
    /// <summary>
    /// 未读空邮件
    /// </summary>
    UnRead_Null,
    /// <summary>
    /// 未读带奖励邮件
    /// </summary>
    UnRead_Items,
    /// <summary>
    /// 已读空邮件
    /// </summary>
    Opened_Null,
    /// <summary>
    /// 已读带奖励邮件
    /// </summary>
    Opened_Items
}


