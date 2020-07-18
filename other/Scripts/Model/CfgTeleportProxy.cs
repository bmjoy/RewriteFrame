using Eternity.FlatBuffer;
using PureMVC.Patterns.Proxy;

public partial class CfgEternityProxy : Proxy
{

    public uint GetTeleportChanelId(uint teleportId)
    {
        Teleport teleport = m_Config.TeleportsByKey(teleportId).Value;
        Chanel chanel = teleport.ChanelList(0).Value;
        return chanel.Id;
    }

    /// <summary>
    /// 获取传送点数量
    /// </summary>
    /// <returns></returns>
    public int GetTeleportCount()
    {
        return m_Config.TeleportsLength;
    }

    /// <summary>
    /// 获取传送点
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public Teleport GetTeleportByIndex(int index)
    {
        return m_Config.Teleports(index).Value;
    }

}