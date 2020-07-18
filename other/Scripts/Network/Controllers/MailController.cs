using Assets.Scripts.Define;
using Assets.Scripts.Lib.Net;
using Assets.Scripts.Proto;
using Game.Frame.Net;
using System.Collections.Generic;
using UnityEngine;

public class MailController : BaseNetController
{
    /// <summary>
    /// 是邮件列表否则是新邮件
    /// </summary>
    private bool m_IsGetList = false;
    /// <summary>
    /// 邮件列表收到邮件参数的次数
    /// </summary>
    private int m_GetMailParamTime = 0;
    /// <summary>
    /// 初始邮件数量
    /// </summary>
    private int m_MailCount = 0;
    #region proxy
    private MailProxy GetMailProxy()
    {
        return GameFacade.Instance.RetrieveProxy(ProxyName.MailProxy) as MailProxy;
    }
    #endregion

    public MailController()
    {
        RegisterNetworkMessages();
    }


    /// <summary>
    /// 注册网络消息
    /// </summary>
    private void RegisterNetworkMessages()
    {
        NetworkManager.Instance.ListenGameServer(KS2C_Protocol.s2c_mail_total_count, MailTotalCount, typeof(S2C_MAIL_TOTAL_COUNT));                         //邮箱当前邮件数量
        NetworkManager.Instance.ListenGameServer(KS2C_Protocol.s2c_mail_get_list, GetMailList, typeof(S2C_MAIL_GET_LIST));                                                    //邮件列表
        NetworkManager.Instance.ListenGameServer(KS2C_Protocol.s2c_mail_param_list, SetParamList, typeof(S2C_MAIL_PARAM_LIST));                              //邮件标题，发件人，内容参数
        NetworkManager.Instance.ListenGameServer(KS2C_Protocol.s2c_mail_detail_info, MailDetailInfo, typeof(S2C_MAIL_DETAIL_INFO));                                //返回邮件详细信息
        NetworkManager.Instance.ListenGameServer(KS2C_Protocol.s2c_mail_get_accessory, MailGetAccessory, typeof(S2C_MAIL_GET_ACCESSORY));             //返回领取附件
        NetworkManager.Instance.ListenGameServer(KS2C_Protocol.s2c_mail_delete, MailDelete, typeof(S2C_MAIL_DELETE));                                                         //返回删除邮件		
        NetworkManager.Instance.ListenGameServer(KS2C_Protocol.s2c_mail_new, MailNew, typeof(S2C_MAIL_NEW));                                                                    //通知有新邮件
        NetworkManager.Instance.ListenGameServer(KS2C_Protocol.s2c_mail_starred, MailMark, typeof(S2C_MAIL_STARRED));                                                           //标记邮件
    }

    #region S2C
    /// <summary>
    /// 邮件初始数量
    /// </summary>
    /// <param name="buf"></param>
    private void MailTotalCount(KProtoBuf buf)
    {
        S2C_MAIL_TOTAL_COUNT msg = buf as S2C_MAIL_TOTAL_COUNT;

        m_MailCount = (int)msg.count;
    }
    /// <summary>
    /// 邮件 返回邮件列表
    /// </summary>
    /// <param name="buf"></param>
    private void GetMailList(KProtoBuf buf)
    {
        S2C_MAIL_GET_LIST msg = buf as S2C_MAIL_GET_LIST;
        m_MailCount = (int)msg.totalMailsCount;
        m_IsGetList = true;
        for (int i = 0; i < msg.mailList.Count; i++)
        {
            MailSimpleData m_Data = msg.mailList[i];

            MailDataVO m_Maildate = new MailDataVO();

            m_Maildate.Id = m_Data.id;
            m_Maildate.Tid = m_Data.tid;
            m_Maildate.Index = m_Data.index;
            m_Maildate.MaxCount = m_Data.max_index;
            m_Maildate.ExpireTime = m_Data.expireTime;
            m_Maildate.SendTime = m_Data.recvTime;
            m_Maildate.Readed = m_Data.readed;
            m_Maildate.Got = m_Data.got;
            m_Maildate.IsMark = m_Data.starred;
            m_Maildate.HasAccessory = m_Data.has_accessory;
            m_Maildate.IsNew = m_Data.is_new;
            GetMailProxy().AddMail(m_Data.id, m_Maildate);
        }
    }

    private void SetParamList(KProtoBuf buf)
    {
        S2C_MAIL_PARAM_LIST msg = buf as S2C_MAIL_PARAM_LIST;
        if (!GetMailProxy().GetMails().TryGetValue(msg.id, out MailDataVO mailData))
            return;
        List<string> title = new List<string>();
        List<string> sender = new List<string>();
        List<string> content = new List<string>();
        string str = "";
        for (int i = 0; i < msg.param_list.Count; i++)
        {
            if (msg.param_list[i].is_string_param == 1)
            {
                str = msg.param_list[i].format_string;
            }
            else
            {
                ulong id = msg.param_list[i].format_id;
                switch ((MailParamTargetType)msg.param_list[i].target_type)
                {
                    case MailParamTargetType.MPTTTask:
                        str = TableUtil.GetLanguageString(string.Format("misson_name_", id));
                        break;
                }
            }
            switch ((MailParamType)msg.param_list[i].type)
            {
                case MailParamType.MPTTitle:
                    title.Add(str);
                    break;
                case MailParamType.MPTSender:
                    sender.Add(str);
                    break;
                case MailParamType.MPTContent:
                    content.Add(str);
                    break;
            }
        }       
        string m_SenderId = string.Format("mail_addressor_{0}", mailData.Tid);
        string m_TitleId = string.Format("mail_subject_{0}", mailData.Tid);
        string m_ContentId = string.Format("mail_content_{0}", mailData.Tid);
        mailData.Sender = string.Format(TableUtil.GetLanguageString(m_SenderId), sender.ToArray());
        mailData.Title = TableUtil.GetLanguageString(m_TitleId);
        mailData.Content = string.Format(TableUtil.GetLanguageString(m_ContentId), content.ToArray());
        if (m_IsGetList)
        {
            m_GetMailParamTime++;
            if (m_GetMailParamTime == m_MailCount && GetMailProxy().GetMails().Count == m_MailCount)
            {
                GameFacade.Instance.SendNotification(NotificationName.MSG_EMAIL_RENDERLIST);
                m_GetMailParamTime = 0;
            }
        }
        else
        {
            GameFacade.Instance.SendNotification(NotificationName.MSG_EMAIL_RENDERLIST);
        }
    }
    /// <summary>
    /// 邮件 返回邮件详细信息
    /// </summary>
    /// <param name="buf"></param>
    private void MailDetailInfo(KProtoBuf buf)
    {
        S2C_MAIL_DETAIL_INFO msg = buf as S2C_MAIL_DETAIL_INFO;
        string m_MailId = msg.id;
        if (GetMailProxy().GetMails().Count == 0)
            return;
        if (!GetMailProxy().GetMails().TryGetValue(m_MailId, out MailDataVO mailData))
            return;
        if (mailData.Readed == 0)
        {
            GetMailProxy().ReadMail(m_MailId);            
        }
        if (mailData.Items == null)
        {
            List<RewardDateVO> m_List = new List<RewardDateVO>();
            for (int t = 0; t < msg.items.Count; t++)
            {
                RewardDateVO rdata = new RewardDateVO();
                if (msg.items[t].id <= 0)
                    continue;

                rdata.Id = msg.items[t].id;
                rdata.Num = msg.items[t].num;
                rdata.Quality = TableUtil.GetItemQuality(rdata.Id);
                m_List.Add(rdata);
            }
            mailData.Items = m_List;
            GameFacade.Instance.SendNotification(NotificationName.MSG_EMAIL_RENDERLIST);
        }        
    }

    /// <summary>
    /// 邮件 返回领取附件
    /// </summary>
    /// <param name="buf"></param>
    private void MailGetAccessory(KProtoBuf buf)
    {
        S2C_MAIL_GET_ACCESSORY msg = buf as S2C_MAIL_GET_ACCESSORY;
        switch ((KMailErrorCode)msg.errCode)
        {
            case KMailErrorCode.KMailPlayerBagNotEnough:
                GameFacade.Instance.SendNotification(NotificationName.MSG_PACKAGE_NOTENOUGH);
                return;
        }
        string m_MailId = msg.id;
        if (GetMailProxy().GetMails().Count == 0)
            return;

        if (!GetMailProxy().GetMails().TryGetValue(m_MailId, out MailDataVO mailData))
            return;
        GetMailProxy().GetMailReward(m_MailId);
        GameFacade.Instance.SendNotification(NotificationName.MSG_EMAIL_RENDERLIST);       
    }


    /// <summary>
    /// 邮件 返回删除邮件
    /// </summary>
    /// <param name="buf"></param>
    private void MailDelete(KProtoBuf buf)
    {
        S2C_MAIL_DELETE msg = buf as S2C_MAIL_DELETE;

        for (int i = 0; i < msg.mails.Count; i++)
        {
            string m_MailId = msg.mails[i].ids;

            if (!GetMailProxy().GetMails().TryGetValue(m_MailId, out MailDataVO mailData))
                return;
            GetMailProxy().DeleteMail(m_MailId);                 
        }
        GameFacade.Instance.SendNotification(NotificationName.MSG_EMAIL_RENDERLIST);
    }



    /// <summary>
    /// 邮件 通知有新邮件
    /// </summary>
    /// <param name="buf"></param>
    private void MailNew(KProtoBuf buf)
    {
        S2C_MAIL_NEW msg = buf as S2C_MAIL_NEW;
        m_IsGetList = false;
        MailSimpleData m_Data = msg.new_mail;
        if (GetMailProxy().GetMails().TryGetValue(m_Data.id, out MailDataVO mailData))
            return;

        MailDataVO m_Maildate = new MailDataVO();

        m_Maildate.Id = m_Data.id;
        m_Maildate.Tid = m_Data.tid;
        m_Maildate.Index = m_Data.index;
        m_Maildate.MaxCount = m_Data.max_index;
        m_Maildate.ExpireTime = m_Data.expireTime;
        m_Maildate.SendTime = m_Data.recvTime;
        m_Maildate.Readed = m_Data.readed;
        m_Maildate.Got = m_Data.got;
        m_Maildate.IsMark = m_Data.starred;
        m_Maildate.HasAccessory = m_Data.has_accessory;
        m_Maildate.IsNew = m_Data.is_new;
        GetMailProxy().AddMail(m_Data.id, m_Maildate);
        GameFacade.Instance.SendNotification(NotificationName.MSG_EMAIL_RENDERLIST);
    }

    /// <summary>
    /// 标记取消标记邮件
    /// </summary>
    /// <param name="buf"></param>
    private void MailMark(KProtoBuf buf)
    {
        S2C_MAIL_STARRED msg = (S2C_MAIL_STARRED)buf;
        if (!GetMailProxy().GetMails().TryGetValue(msg.id, out MailDataVO mailData))
            return;
        mailData.IsMark = msg.starred;
        GameFacade.Instance.SendNotification(NotificationName.MSG_EMAIL_RENDERLIST);
    }
    #endregion



    #region  C2S
    /// <summary>
    /// 获得邮件列表
    /// </summary>
    /// <returns>true：不用请求了 false:重新请求数据</returns>
    public void C_to_S_GetEmailList(params object[] objs)
    {
        GetMailProxy().ClearMail();
        C2S_MAIL_GET_LIST msg = new C2S_MAIL_GET_LIST();
        msg.protocolID = (ushort)KC2S_Protocol.c2s_mail_get_list;
        NetworkManager.Instance.SendToGameServer(msg);
    }

    /// <summary>
    /// 读取邮件内容
    /// </summary>
    /// <param name="mailId">邮件id</param>
    public void C_to_S_GetMailContent(string id)
    {
        if (!GetMailProxy().GetMails().ContainsKey(id))
        {
            return;
        }
        C2S_MAIL_DETAIL_INFO msg = new C2S_MAIL_DETAIL_INFO();
        msg.protocolID = (ushort)KC2S_Protocol.c2s_mail_detail_info;
        msg.id = id;
        NetworkManager.Instance.SendToGameServer(msg);
    }

    /// <summary>
    /// 请求领取附件
    /// </summary>
    /// <param name="mailId">邮件id</param>
    public void C_to_S_GetMail_Accessory(string id,byte hasAccessory)
    {
        if (!GetMailProxy().GetMails().ContainsKey(id))
        {
            return;
        }
        C2S_MAIL_GET_ACCESSORY msg = new C2S_MAIL_GET_ACCESSORY();
        msg.protocolID = (ushort)KC2S_Protocol.c2s_mail_get_accessory;
        msg.id = id;
        msg.del_after_get_accessory = hasAccessory;
        NetworkManager.Instance.SendToGameServer(msg);
    }

    /// <summary>
    /// 请求删除邮件
    /// </summary>
    /// <param name="mailId">邮件id</param>
    public void C_to_S_GetMail_Delete(string id)
    {
        if (!GetMailProxy().GetMails().ContainsKey(id))
        {
            return;
        }
        C2S_MAIL_DELETE msg = new C2S_MAIL_DELETE();
        msg.protocolID = (ushort)KC2S_Protocol.c2s_mail_delete;
        msg.id = id;
        NetworkManager.Instance.SendToGameServer(msg);
    }

    /// <summary>
    /// 清除所有已读邮件
    /// </summary>
    public void C_to_S_DeleAllOpened()
    {
        C2S_MAIL_DELETE_ALL_READ msg = new C2S_MAIL_DELETE_ALL_READ();
        msg.protocolID = (ushort)KC2S_Protocol.c2s_mail_delete_all_read;
        NetworkManager.Instance.SendToGameServer(msg);
    }

    /// <summary>
    /// 标记邮件
    /// </summary>
    public void C_to_S_MarkMail(string id)
    {
        if (!GetMailProxy().GetMails().ContainsKey(id))
        {
            return;
        }
        C2S_MAIL_STARRED msg = new C2S_MAIL_STARRED();
        msg.protocolID = (ushort)KC2S_Protocol.c2s_mail_starred;
        msg.id = id;
        NetworkManager.Instance.SendToGameServer(msg);
    }

    public void C_to_S_ClearNew(List<string> ids)
    {
        C2S_MAIL_CLEAR_NEW msg = new C2S_MAIL_CLEAR_NEW();
        msg.protocolID = (ushort)KC2S_Protocol.c2s_mail_clear_new;
        msg.ids = ids;
        NetworkManager.Instance.SendToGameServer(msg);
    }
    #endregion

}
