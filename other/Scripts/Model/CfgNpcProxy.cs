using Eternity.FlatBuffer;
using PureMVC.Patterns.Proxy;
using UnityEngine.Assertions;

public partial class CfgEternityProxy : Proxy
{

    /// <summary>
    /// 根据itemId获取NPC基础数据
    /// </summary>
    /// <param name="tid"></param>
    /// <returns></returns>
    public Npc GetNpcByKey(uint itemId)
    {
        Npc? npcVO = m_Config.NpcsByKey(itemId);
        Assert.IsTrue(npcVO.HasValue, "CfgEternityProxy => GetNpcByKey not exist tid " + itemId);
        return npcVO.Value;
    }

    /// <summary>
	/// 根据npcid获取预设体名字
	/// </summary>
	/// <param name="tid">tid</param>
	/// <returns></returns>
	public string GetNpcModelByKey(uint tid)
    {
        int modelId = 0;
        modelId = GetNpcByKey(tid).Model;
        return GetModel(modelId).AssetName;
    }

}