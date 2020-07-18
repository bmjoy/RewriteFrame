using Eternity.FlatBuffer;
using PureMVC.Patterns.Proxy;
using UnityEngine.Assertions;

public partial class CfgEternityProxy : Proxy
{

    /// <summary>
    /// 根据tid获取道具基础数据
    /// </summary>
    /// <param name="tid"></param>
    /// <returns></returns>
    public Item GetItemByKey(uint tid)
    {
        Item? paintingVO = m_Config.ItemsByKey(tid);
        Assert.IsTrue(paintingVO.HasValue, "CfgEternityProxy => GetItemByKey not exist tid " + tid);
        return paintingVO.Value;
    }

    /// <summary>
    /// 获取皮肤资源
    /// </summary>
    /// <param name="itemId"></param>
    /// <returns></returns>
    public string GetItemModelAssetNameByKey(uint itemId)
    {
        int modelId = 0;
        modelId = GetItemByKey(itemId).Model;
        return GetModel(modelId).AssetName;
    }

    public Model GetItemModelByKey(uint itemTid)
    {
        return GetModel(GetItemByKey(itemTid).Model);
    }

    /// <summary>
    /// 获取矿石碎裂特效
    /// </summary>
    /// <param name="itemId"></param>
    /// <returns></returns>
    public string GetMineralFx(uint itemId)
    {
        return GetItemModelByKey(itemId).MineralFx;
    }
}