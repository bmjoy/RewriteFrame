using Eternity.FlatBuffer;
using PureMVC.Patterns.Proxy;
using UnityEngine.Assertions;

public partial class CfgEternityProxy : Proxy
{
    public string GetEffectPath(uint id)
    {
        Fx? fx = m_Config.FxsByKey(id);
        Assert.IsTrue(fx.HasValue, "CfgEternityProxy => GetEffectPath not exist tid " + id);

        return fx.Value.FxName;
    }
}