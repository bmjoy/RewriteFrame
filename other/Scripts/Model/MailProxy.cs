using PureMVC.Patterns.Proxy;
using System.Collections.Generic;
using UnityEngine.Assertions;

public class MailProxy : Proxy
{
    /// <summary>
    /// 邮件数据
    /// </summary>
    private Dictionary<string, MailDataVO> m_Mails = new Dictionary<string, MailDataVO>();

    public MailProxy() : base(ProxyName.MailProxy)
    {

    }

    /// <summary>
    /// 添加邮件
    /// </summary>
    /// <param name="id"></param>
    /// <param name="maildate"></param>
    public void AddMail(string id, MailDataVO maildate)
    {
        m_Mails[id] = maildate;
    }

    /// <summary>
    /// 删除邮件
    /// </summary>
    /// <param name="id"></param>
    public void DeleteMail(string id)
    {
        m_Mails.Remove(id);
    }

    /// <summary>
    /// 清空邮件数据
    /// </summary>
    public void ClearMail()
    {
        m_Mails.Clear();
    }

    /// <summary>
    /// 取消新邮件标记
    /// </summary>
    public void ClearMailNew(string id)
    {
        Assert.IsTrue(m_Mails.TryGetValue(id, out MailDataVO mailData), "MailProxy => GetMailItem not exist id " + id);
        m_Mails[id].IsNew = 0;
    }

    /// <summary>
    /// 领取邮件奖励
    /// </summary>
    /// <param name="id"></param>
    public void GetMailReward(string id)
    {
		Assert.IsTrue(m_Mails.TryGetValue(id, out MailDataVO mailData), "MailProxy => GetMailItem not exist id " + id);
        m_Mails[id].Got = 1;
    }


    /// <summary>
    /// 读取邮件
    /// </summary>
    /// <param name="id"></param>
    public void ReadMail(string id)
    {
		Assert.IsTrue(m_Mails.TryGetValue(id, out MailDataVO mailData), "MailProxy => ReadMail not exist id " + id);
        m_Mails[id].Readed = 1;
    }

    /// <summary>
    /// 获取所有邮件
    /// </summary>
    /// <returns></returns>
    public Dictionary<string, MailDataVO> GetMails()
    {
        return m_Mails;
    }

    /// <summary>
    /// 查找邮件
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public MailDataVO GetMail(string id)
    {
		if (string.IsNullOrEmpty(id))
		{
			return null;
		}
		if (m_Mails.TryGetValue(id, out MailDataVO mailData))
		{
			return mailData;
		}
        return null;
    }
}