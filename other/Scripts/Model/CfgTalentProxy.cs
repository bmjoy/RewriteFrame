using PureMVC.Patterns.Proxy;
using UnityEngine.Assertions;
using Eternity.FlatBuffer;

public partial class CfgEternityProxy : Proxy
{
    private Talent m_CurrentTalentData;

    /// <summary>
    /// 获取天赋数据
    /// </summary>
    /// <param name="tid"></param>
    /// <returns></returns>
    public Talent? GetTalentByKey(uint tid)
    {
        Talent? talent = m_Config.TalentsByKey(tid);
        Assert.IsTrue(talent.HasValue, "CfgEternityProxy => GetTalentByKey not exist tid: " + tid);
        return talent;
    }

    public Talent? GetTalentByIndex(int index)
    {
        Talent? talent = m_Config.Talents(index);
        Assert.IsTrue(talent.HasValue, "CfgEternityProxy => GetTalentByIndex not exist index: " + index);
        return talent;
    }

    /// <summary>
    /// 获取天赋节点数据
    /// </summary>
    /// <param name="tid">天赋树tid</param>
    /// <param name="id">节点id</param>
    /// <returns></returns>
    public TalentSubNode? GetTalentSubNodeByKey(uint tid,uint id)
    {
        TalentSubNode? talentNode = m_Config.TalentsByKey(tid).Value.NodesByKey(id);
        Assert.IsTrue(talentNode.HasValue, "CfgEternityProxy => NodesByKey not exist tid: " + tid);
        return talentNode; 
    }

    /// <summary>
    /// 获取天赋节点数据
    /// </summary>
    /// <param name="tid">天赋树tid</param>
    /// <param name="j">index</param>
    /// <returns></returns>
    public TalentSubNode? GetTalentSubNodeByIndex(uint tid, int j)
    {
        TalentSubNode? talentNode = m_Config.TalentsByKey(tid).Value.Nodes(j);
        Assert.IsTrue(talentNode.HasValue, "CfgEternityProxy => Nodes not exist tid: " + tid);
        return talentNode; 
    }

    /// <summary>
    /// 获取天赋的名字
    /// </summary>
    /// <param name="key">key</param>
    /// <param name="enUs">是否是英文</param>
    /// <returns></returns>
    public string GetTalentName(string key, bool enUs = true)
    {
        LanguageTalent? Languagename = m_Config.LanguageTalentsByKey(key);
        Assert.IsTrue(Languagename.HasValue, "CfgEternityProxy => Nodes not exist tid: " + key);
        if (!enUs)
        {
            return Languagename.Value.Chs;
        }
        return Languagename.Value.EnUs;
    }

    /// <summary>
    /// 天赋树中天赋的个数
    /// </summary>
    /// <param name="id">天赋树ID</param>
    /// <returns></returns>
    public int GetTalentNodeLength(uint id)
    {
        return GetTalentByKey(id).Value.NodesLength;
    }

   


}
