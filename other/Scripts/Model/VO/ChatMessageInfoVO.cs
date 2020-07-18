// reviewed by william 2019.5.5

public class ChatMessageInfoVO
{
    /// <summary>
    /// 时间
    /// </summary>
    public System.DateTime Time;

    /// <summary>
    /// 频道
    /// </summary>
    public ChatChannel Channel;

    /// <summary>
    /// 消息
    /// </summary>
    public string Message;

    /// <summary>
    /// 消息已解码
    /// </summary>
    public bool MessageDecoded;

    /// <summary>
    /// 发送者ID
    /// </summary>
    public ulong FromID;

    /// <summary>
    /// 发送者名称
    /// </summary>
    public string FromName;
}

/// <summary>
/// 聊天频道
/// </summary>
public enum ChatChannel
{
    /// <summary>
    /// 无效
    /// </summary>
    NONE = -1,

    /// <summary>
    /// 所有
    /// </summary>
    All = 0,

    /// <summary>
    /// 世界
    /// </summary>
    World = 1,

    /// <summary>
    ///集团 ？？？
    /// </summary>
    Faction = 2,
    /// <summary>
    /// 公会
    /// </summary>
    Union = 3,

    /// <summary>
    /// 地区
    /// </summary>
    Station = 4,

    /// <summary>
    /// 队伍
    /// </summary>
    Group = 5,

    /// <summary>
    /// 私聊
    /// </summary>
    Whisper = 6,

    /// <summary>
    /// 日志？？？
    /// </summary>
    CombatLog = 7,
}