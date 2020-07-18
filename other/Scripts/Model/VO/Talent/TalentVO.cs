using Eternity.FlatBuffer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TalentVO
{
    /// <summary>
    /// 天赋节点ID
    /// </summary>
    public int Id;
    /// <summary>
    /// 图标Id
    /// </summary>
    public uint IconId;
    /// <summary>
    /// 天赋树ID
    /// </summary>
    public uint TalentRootId;
    /// <summary>
    /// 天赋树vo
    /// </summary>
    public TalentTypeVO MTalentTypeVO;
    /// <summary>
    /// 上一个天赋节点ID
    /// </summary>
    public int BackNodeId;
    /// <summary>
    /// 当前等级
    /// </summary>
    public int Level;
    /// <summary>
    /// 当前类型
    /// </summary>
    public int Type;
    /// <summary>
    /// 最大等级
    /// </summary>
    public int MaxLevel=3;
    /// <summary>
    /// 状态
    /// </summary>
    public TalentState State;
    /// <summary>
    /// 解锁条件ID
    /// </summary>
    public int UnLockId;
    /// <summary>
    /// 更节点解锁条件ID
    /// </summary>
    public int UnRootLockId;
    /// <summary>
    /// 消耗数量
    /// </summary>
    public int EffectNum;
    /// <summary>
    /// 天赋个体
    /// </summary>
    public TalentElement MTalentElement;
    /// <summary>
    /// 表数据
    /// </summary>
    public TalentSubNode? MTalentSubNode;
}
