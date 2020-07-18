using Eternity.FlatBuffer;
using PureMVC.Patterns.Proxy;
using UnityEngine;
using UnityEngine.Assertions;

public partial class CfgEternityProxy : Proxy
{

    /// <summary>
    /// 获取颜色数据
    /// </summary>
    /// <param name="tid">tid</param>
    /// <returns></returns>
    public Color GetGlobalColor(int tid)
    {
		GlobalColor? gColor = m_Config.GlobalColorsByKey((uint)tid);
        Assert.IsTrue(gColor.HasValue, "CfgEternityProxy => GetGlobalColor not exist tid " + tid);
        return new Color
        (
            gColor.Value.ColorR / 255f,
            gColor.Value.ColorG / 255f,
            gColor.Value.ColorB / 255f,
            gColor.Value.ColorA / 255f
        );
    }

}