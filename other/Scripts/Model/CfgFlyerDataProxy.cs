using Eternity.FlatBuffer;
using PureMVC.Patterns.Proxy;
using UnityEngine.Assertions;

public enum FlyerStageType
{
    None = 0,
    Flying,
    End,
}

public partial class CfgEternityProxy : Proxy
{
    public FlyerData GetFlyerData(int flyerId)
    {
        FlyerData? flyerData = m_Config.FlyerDatasByKey(flyerId);
        Assert.IsTrue(flyerData.HasValue, $"CfgEternityProxy => GetFlyerData not exist flyerId =  {flyerId}");

        return flyerData.Value;
    }
}