using PureMVC.Patterns.Proxy;
using System.Collections.Generic;

// reviewed by william 2019.5.5

public class ChatProxy : Proxy
{
    /// <summary>
    ///  每频道消息数量上限
    /// </summary>
    private const int CHANNEL_MESSAGE_MAX_COUNT = 100;

    /// <summary>
    /// 所有频道
    /// </summary>
    private Dictionary<ChatChannel, List<ChatMessageInfoVO>> m_AllChannelsMessages = new Dictionary<ChatChannel, List<ChatMessageInfoVO>>();

    /// <summary>
    /// 当前频道
    /// </summary>
    public ChatChannel CurrentChannel = ChatChannel.All;

    public ChatProxy() : base(ProxyName.ChatProxy)
    {

    }

    /// <summary>
    /// 获取频道消息
    /// </summary>
    /// <param name="channel"></param>
    /// <returns></returns>
    public List<ChatMessageInfoVO> GetMessages(ChatChannel channel)
    {
        List<ChatMessageInfoVO> messages;
        if (!m_AllChannelsMessages.TryGetValue(channel, out messages))
        {
            m_AllChannelsMessages[channel] = messages = new List<ChatMessageInfoVO>();
        }
        return messages;
    }

    /// <summary>
    /// 添加消息
    /// </summary>
    /// <param name="channel"></param>
    /// <param name="text"></param>
    /// <param name="fromID"></param>
    /// <param name="fromName"></param>
    public void AddMessage(ChatChannel channel, string text, ulong fromID = 0, string fromName = null)
    {
        ChatMessageInfoVO channelMessage = new ChatMessageInfoVO();
        channelMessage.Time = System.DateTime.Now;
        channelMessage.Channel = channel;
        channelMessage.Message = text;
        channelMessage.FromID = fromID;
        channelMessage.FromName = fromName;

        List<ChatMessageInfoVO> messages = GetMessages(channel);
        messages.Add(channelMessage);

        while (messages.Count > CHANNEL_MESSAGE_MAX_COUNT)
        {
            messages.RemoveAt(0);
        }

        MsgChatChannelMessageChanged message = MessageSingleton.Get<MsgChatChannelMessageChanged>();
        message.m_channel = channel;
        SendNotification(NotificationName.ChatMessageChanged, message);

        if (fromID == 0 && fromName == null)
        {
            return;
        }

        if (channel != ChatChannel.All && channel != ChatChannel.CombatLog)
        {
            channel = ChatChannel.All;

            messages = GetMessages(channel);
            messages.Add(channelMessage);

            while (messages.Count > CHANNEL_MESSAGE_MAX_COUNT)
            {
                messages.RemoveAt(0);
            }

            message = MessageSingleton.Get<MsgChatChannelMessageChanged>();
            message.m_channel = channel;
            SendNotification(NotificationName.ChatMessageChanged, message);
        }
    }
}