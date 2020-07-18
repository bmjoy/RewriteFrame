using Eternity.FlatBuffer;
using Eternity.FlatBuffer.Enums;
using PureMVC.Patterns.Proxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Assertions;

/// <summary>
/// CfgSkillDataProxy
/// </summary>
public enum SkillStageType
{
    /// <summary>
    /// 无效
    /// </summary>
    None = 0,
    
    /// <summary>
    /// 开始阶段
    /// </summary>
    Begin,
    BreakBegin,

    /// <summary>
    /// 释放阶段
    /// </summary>
    Release,
    BreakRelease,

    /// <summary>
    /// 结束阶段
    /// </summary>
    End,
    BreakEnd,
}

public partial class CfgEternityProxy : Proxy
{
    public bool IsSkillExist(int skillId)
    {
        SkillData? skillData = m_Config.SkillDatasByKey(skillId);
        return skillData.HasValue;
    }

    /// <summary>
    /// 获得技能数据
    /// </summary>
    /// <param name="skillId"></param>
    /// <returns></returns>
    public SkillData GetSkillData(int skillId)
    {
        SkillData? skillData = m_Config.SkillDatasByKey(skillId);

        Assert.IsTrue(skillData.HasValue, $"CfgEternityProxy => GetSkillData not exist skillId.id =  {skillId}");

        return skillData.Value;
    }


    /// <summary>
    /// 获取技能基本数据配置
    /// </summary>
    /// <param name="skillId"></param>
    /// <returns></returns>
    public SkillBaseData GetSkillBaseData(int skillId)
    {
        SkillData skillData = GetSkillData(skillId);
        SkillBaseData? baseData = skillData.BaseData;

        Assert.IsTrue(baseData.HasValue, $"CfgEternityProxy::GetSkillBaseData->base data not exist skillId.id =  {skillId}");

        return baseData.Value;
    }


    /// <summary>
    /// 获取技能开始阶段配置
    /// </summary>
    /// <param name="skillId"></param>
    /// <returns></returns>
    public SkillBeginStageData GetSkillBeginStageData(int skillId)
    {
        SkillData skillData = GetSkillData(skillId);
        SkillBeginStageData? stageData = skillData.BeginStageData;
        Assert.IsTrue(stageData.HasValue, $"CfgEternityProxy::GetSkillBeginStageData->data not exist skillId.id =  {skillId}");
        return stageData.Value;
    }

    /// <summary>
    /// 获取技能释放阶段配置
    /// </summary>
    /// <param name="skillId"></param>
    /// <returns></returns>
    public SkillReleaseStageData GetSkillReleaseStageData(int skillId)
    {
        SkillData skillData = GetSkillData(skillId);
        SkillReleaseStageData? stageData = skillData.ReleaseStageData;
        Assert.IsTrue(stageData.HasValue, $"CfgEternityProxy::GetSkillReleaseStageData->data not exist skillId.id =  {skillId}");
        return stageData.Value;
    }


    /// <summary>
    /// 获取技能结束阶段配置
    /// </summary>
    /// <param name="skillId"></param>
    /// <returns></returns>
    public SkillEndStageData GetSkillEndStageData(int skillId)
    {
        SkillData skillData = GetSkillData(skillId);
        SkillEndStageData? stageData = skillData.EndStageData;
        Assert.IsTrue(stageData.HasValue, $"CfgEternityProxy::GetSkillEndStageData->data not exist skillId.id =  {skillId}");
        return stageData.Value;
    }

    /// <summary>
    /// 获取技能释放，子阶段数据
    /// </summary>
    /// <param name="skillId"></param>
    /// <param name="childIndex"></param>
    /// <returns></returns>
    public SkillReleaseChildData GetSkillReleaseChildData(int skillId,int childIndex = 0)
    {
        SkillReleaseStageData stageData = GetSkillReleaseStageData(skillId);
        SkillReleaseChildData? childData = stageData.Childs(childIndex);

        Assert.IsTrue(childData.HasValue, $"CfgEternityProxy::GetSkillReleaseChildData->data not exist skillId.id =  {skillId},index = {childIndex}");

        return childData.Value;
    }


    /// <summary>
    /// 根据所处阶段，获取对应轨道组
    /// </summary>
    /// <param name="skillId"></param>
    /// <param name="stageType"></param>
    /// <param name="childIndex"></param>
    /// <returns></returns>
    public TrackGroup GetTrackGroup(int skillId,SkillStageType stageType,int childIndex = 0)
    {
        TrackGroup? trackGroup = null;
        if(stageType == SkillStageType.Begin)
        {
            trackGroup = GetSkillBeginStageData(skillId).Group;
        }
        else if(stageType == SkillStageType.Release)
        {
            SkillReleaseChildData childData = GetSkillReleaseChildData(skillId, childIndex);
            trackGroup = childData.Group;
        }
        else if(stageType == SkillStageType.End)
        {
            trackGroup = GetSkillEndStageData(skillId).Group;
        }
        Assert.IsTrue(trackGroup.HasValue, $"CfgEternityProxy::GetTrackGroup->data not exist.id =  {skillId},stageType = {stageType} ,index = {childIndex}");
        return trackGroup.Value;
    }

    public CdType[] GetSkillReleaseCDTypes(int skillID)
    {
        SkillData skillData = GetSkillData(skillID);
        SkillBaseData baseData = skillData.BaseData.Value;

        List<CdType> cdTypes = new List<CdType>();

        for (int i = 0; i < baseData.CdDatasLength; ++i)
        {
            CdData cdData = baseData.CdDatas(i).Value;
            cdTypes.Add(cdData.CdType);
        }

        return cdTypes.ToArray();
    }
}

