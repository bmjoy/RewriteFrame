using System.Collections.Generic;

public class MailDataVO
{
    /// <summary>
    /// 邮件id
    /// </summary>
    public string Id = "";
    /// <summary>
    /// 邮件表id
    /// </summary>
    public uint Tid;
    /// <summary>
    /// 第几封
    /// </summary>
    public uint Index;
    /// <summary>
    /// 共几封
    /// </summary>
    public uint MaxCount;
    /// <summary>
    /// 邮件标题
    /// </summary>
    public string Title = "";
    /// <summary>
    /// 邮件内容
    /// </summary>
    public string Content = "";
    /// <summary>
    /// 发送人
    /// </summary>
    public string Sender = "";
    /// <summary>
    /// 过期时间
    /// </summary>
    public long ExpireTime;
    /// <summary>
    /// 发送时间
    /// </summary>
    public long SendTime;
    /// <summary>
    /// 是否已读
    /// </summary>
    public byte Readed;
    /// <summary>
    /// 是否已领
    /// </summary>
    public byte Got;
    /// <summary>
    /// 是否新邮件
    /// </summary>
    public byte IsNew;
    /// <summary>
    /// 是否被标记
    /// </summary>
    public byte IsMark;
    /// <summary>
    /// 是否有附件
    /// </summary>
    public byte HasAccessory;
    /// <summary>
    /// 奖励
    /// </summary>
    public List<RewardDateVO> Items = null;
}