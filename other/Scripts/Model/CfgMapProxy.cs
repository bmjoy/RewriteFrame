using PureMVC.Patterns.Proxy;
using UnityEngine.Assertions;

public partial class CfgEternityProxy : Proxy
{

    private uint m_LastGamingMapId = Map.Constants.NOTSET_MAP_UID;
    private uint m_GamingMapId = Map.Constants.NOTSET_MAP_UID;

    private Eternity.FlatBuffer.Map m_CurrentMapData;

    /// <summary>
    /// 获取地图数据
    /// </summary>
    /// <param name="tid"></param>
    /// <returns></returns>
    public Eternity.FlatBuffer.Map? GetMapByKey(uint tid)
    {
        Eternity.FlatBuffer.Map? map = m_Config.MapsByKey(tid);
        Assert.IsTrue(map.HasValue, "CfgEternityProxy => GetMapByKey not exist tid: " + tid);
        return map;
    }

    public Eternity.FlatBuffer.Map? GetMapByIndex(int index)
    {
        Eternity.FlatBuffer.Map? map = m_Config.Maps(index);
        Assert.IsTrue(map.HasValue, "CfgEternityProxy => GetMapByIndex not exist index: " + index);
        return map;
    }

    public int GetMapCount()
    {
        return m_Config.MapsLength;
    }

    public void SetCurrentMapData(uint gamingMapId)
    {
        m_LastGamingMapId = m_GamingMapId;
        m_GamingMapId = gamingMapId;
        m_CurrentMapData = GetMapByKey(gamingMapId).Value;
    }

    public Eternity.FlatBuffer.Map GetCurrentMapData()
    {
        return m_CurrentMapData;
    }

    public uint GetCurrentMapId()
    {
        return m_CurrentMapData.MapId;
    }

    public uint GetCurrentGamingMapId()
    {
        return m_CurrentMapData.GamingmapId;
    }

    public uint GetLastGamingMapId()
    {
        return m_LastGamingMapId;
    }

}