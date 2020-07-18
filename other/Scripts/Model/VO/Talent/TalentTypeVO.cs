using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Eternity.FlatBuffer;
public class TalentTypeVO 
{
    /// <summary>
    /// 天赋ID
    /// </summary>
    public int Id;

    /// <summary>
    /// 等级
    /// </summary>
    public int Level;
    /// <summary>
    /// IconId 
    /// </summary>
    public uint IconId;
    /// <summary>
    /// 解锁ID
    /// </summary>
    public uint UnLockId;
    /// <summary>
    /// 类型
    /// </summary>
    public int Type;
    /// <summary>
    /// 消耗ID
    /// </summary>
    public int EffectId;
    /// <summary>
    /// 天赋树个体
    /// </summary>
    public TalentTypeElement m_TalentTypeElement;
    /// <summary>
    /// 表数据
    /// </summary>
    public Talent? MTalent;

}
