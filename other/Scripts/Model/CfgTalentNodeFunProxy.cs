using PureMVC.Patterns.Proxy;
using UnityEngine.Assertions;
using Eternity.FlatBuffer;
public partial class CfgEternityProxy : Proxy
{

    /// <summary>
    /// 获取天赋效果
    /// </summary>
    /// <param name="index">tid</param>
    /// <returns></returns>
    public TalentNodeFun GetTalentNodeFunsByKey(uint tid)
    {
        TalentNodeFun? talentNodeFun = m_Config.TalentNodeFunsByKey(tid).Value;
        Assert.IsTrue(talentNodeFun.HasValue, "CfgEternityProxy => TalentNodeFunsByKey not exist tid " + tid);

        return talentNodeFun.Value;
    }

    /// <summary>
    /// 获取天赋效果
    /// </summary>
    /// <param name="index">tid</param>
    /// <returns></returns>
    public TalentNodeFunAttr?[] GetTalentNodeFunAttrByKey(uint tid)
    {
        TalentNodeFun? talentNodeFun = GetTalentNodeFunsByKey(tid);
        Assert.IsTrue(talentNodeFun.HasValue, "CfgEternityProxy => GetTalentNodeFunsByKey not exist tid " + tid);
        TalentNodeFunAttr?[] talentNodeFuns = new TalentNodeFunAttr?[talentNodeFun.Value.NodeAttrsLength];
        for (int i = 0; i < talentNodeFun.Value.NodeAttrsLength; i++)
        {
            talentNodeFuns[i] = talentNodeFun.Value.NodeAttrs(i);
        }
        return talentNodeFuns;
    }

    /// <summary>
    /// 获取最大等级
    /// </summary>
    /// <param name="tid">tid</param>
    /// <returns></returns>
    public int  GetTalentMaxLevel(uint tid)
    {
        TalentNodeFunAttr?[] talentNodeFunAttrs = GetTalentNodeFunAttrByKey(tid);
        for (int i = 0; i < talentNodeFunAttrs.Length; i++)
        {
            if (talentNodeFunAttrs[i].Value.UpgradeCost == -1)
            {
                return talentNodeFunAttrs[i].Value.Level;
            }
        }
        return 1;
    }

    /// <summary>
    /// 获得消耗
    /// </summary>
    /// <returns></returns>
    public int GetUpLevelCost(uint tid,int level)
    {
        TalentNodeFunAttr?[] talentNodeFunAttrs = GetTalentNodeFunAttrByKey(tid);
        for (int i = 0; i < talentNodeFunAttrs.Length; i++)
        {
            if (talentNodeFunAttrs[i].Value.Level == level)
            {
                return talentNodeFunAttrs[i].Value.UpgradeCost == -1 ? 0: talentNodeFunAttrs[i].Value.UpgradeCost;
            }
        }
        return 1;
    }

}
