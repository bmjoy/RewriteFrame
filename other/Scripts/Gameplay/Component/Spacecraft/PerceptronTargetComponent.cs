/*===============================
 * Author: [Allen]
 * Purpose: IPerceptronTarget
 * Time: 2019/12/18 21:01:30
================================*/

using Eternity.FlatBuffer;
using Gameplay.Battle.Timeline;
using System;
using System.Collections.Generic;
using UnityEngine;

public interface IPerceptronTarget
{
    BaseEntity GetOwner();
    HeroState GetCurrentState();

    void SetPerceptronTarget(PerceptronTarget perceptronTarget);

    int GetCurrSkillId();

    bool GetSkillBttonIsDown();
    void ToDoChangeGuideSkillTargetAction();

    /// <summary>
    /// 是否包含 武器&准星 对象
    /// </summary>
     bool HaveWeaponAndCrossSight(ulong uid);

    /// <summary>
    /// 获取武器&准星对象
    /// </summary>
     WeaponAndCrossSight GetWeaponAndCrossSight(ulong uid);    

    /// <summary>
    /// 增加武器&准星 对象
    /// </summary>
     void AddWeaponAndCrossSight(ulong uid, WeaponAndCrossSight Weapon);

    /// <summary>
    /// 删除转化炉武器对象
    /// </summary>
     void DeleReformerWeaponAndCrossSight();
}

public class PerceptronTargetComponent : EntityComponent<IPerceptronTarget>
{
    private IPerceptronTarget m_IPerceptronTarget;

    private PerceptronTarget m_PerceptronTarget;
    private PlayerSkillProxy m_SkillProxy;

    /// <summary>
    /// 飞船上能装多少个武器
    /// </summary>
    private const int BATTLE_WEAPON_COUNT = 2;


    public override void OnInitialize(IPerceptronTarget property)
    {
        m_SkillProxy = GameFacade.Instance.RetrieveProxy(ProxyName.PlayerSkillProxy) as PlayerSkillProxy;

        m_IPerceptronTarget = property;
        m_PerceptronTarget = new PerceptronTarget(property.GetOwner(), GetCurrSkillId, GetSkillBttonIsDown, ToDoChangeGuideSkillTargetAction);
        m_IPerceptronTarget.SetPerceptronTarget(m_PerceptronTarget);
    }

    /// <summary>
    /// 添加监听
    /// </summary>
    public override void OnAddListener()
    {
        if (m_IPerceptronTarget.GetOwner().IsMain())
        {
#if NewSkill
            AddListener(ComponentEventName.ChangHeroState, ReformerWeapons);
            AddListener(ComponentEventName.WeaponPowerChanged, UpdateBattleWeapons);
            AddListener(ComponentEventName.WeaponOperation, WeaponHotKeyOperation);
            AddListener(ComponentEventName.PostWeaponFire, ChangeCrossSightSize);
            AddListener(ComponentEventName.WeaponSkillFinish, WeaponSkillFinish);
#else
#endif
        }
        else
        {
            AddListener(ComponentEventName.BroadCastSkill_BeginTargets, BroadCastSkillBeginTargets);
            AddListener(ComponentEventName.BroadCastSkill_Accumulation, BroadCastSkillAccumulation);
            AddListener(ComponentEventName.BroadCastSkill_ChangeTargets, BroadCastSkillChangeTargets);
        }
    }

    public override void OnUpdate(float delta)
    {
        base.OnUpdate(delta);

        m_PerceptronTarget.OnUpdata();

        if (m_IPerceptronTarget.GetOwner().IsMain())
        {
            //武器&准星循环
            WeaponAndCrossSight weapon = m_SkillProxy.GetCurrentWeaponAndCrossSight();
            if (weapon != null)
                weapon.OnUpdate(delta);
        }
    }


    public override void OnDestroy()
    {
        base.OnDestroy();
        m_IPerceptronTarget.SetPerceptronTarget(null);
        m_PerceptronTarget.Clear();
        m_PerceptronTarget = null;
    }

    public int GetCurrSkillId()
    {
        return m_IPerceptronTarget.GetCurrSkillId();
    }

    public bool GetSkillBttonIsDown()
    {
        return m_IPerceptronTarget.GetSkillBttonIsDown();
    }

    public void ToDoChangeGuideSkillTargetAction()
    {
        m_IPerceptronTarget.ToDoChangeGuideSkillTargetAction();
    }

    /// <summary>
    /// 在武器变化的时候, 更新当前实装的武器的准星信息
    /// </summary>
    private void UpdateBattleWeapons(IComponentEvent entityEvent)
    {
        PlayerSkillProxy m_SkillProxy = GameFacade.Instance.RetrieveProxy(ProxyName.PlayerSkillProxy) as PlayerSkillProxy;
        for (int iWeapon = 0; iWeapon < BATTLE_WEAPON_COUNT; iWeapon++)
        {
            IWeapon weapon = m_SkillProxy.GetWeaponByIndex(iWeapon);
            if (weapon == null)
                continue;

            ulong uid = weapon.GetUID();

            if (!m_IPerceptronTarget.HaveWeaponAndCrossSight(uid))
            {
                WeaponAndCrossSight weaponAndCross = WeaponAndCrossSightFactory.CreatTWeapon(uid,
                    weapon.GetConfig().Id, weapon.GetBaseConfig().SkillId,
                    (WeaponAndCrossSight.WeaponAndCrossSightTypes)weapon.GetConfig().Reticle);

                //Debug.LogError(string.Format("创建准星 uid = {0}, tid = {1} , type = {2}, null? {3}", uid, weapon.GetConfig().Id, weapon.GetConfig().Reticle , (weaponAndCross== null)));

                if (weaponAndCross == null)
                    continue;
                m_IPerceptronTarget.AddWeaponAndCrossSight(uid, weaponAndCross);
            }
        }
    }

    /// <summary>
    /// 转化炉切换
    /// </summary>
    /// <param name="entityEvent"></param>
    private void ReformerWeapons(IComponentEvent entityEvent)
    {
        if (m_IPerceptronTarget.GetCurrentState().IsHasSubState(EnumSubState.Peerless))
        {
            //转化炉模式
            if (!m_SkillProxy.UsingReformer())
                return;

            IReformer reformer = m_SkillProxy.GetReformer();
            if (reformer == null)
                return;
            ulong uid = reformer.GetUID();
            if (!m_IPerceptronTarget.HaveWeaponAndCrossSight(uid))
            {
                WeaponAndCrossSight weaponAndCross = WeaponAndCrossSightFactory.CreatTWeapon(uid,
                    reformer.GetConfig().Id, reformer.GetBaseConfig().SkillId,
                    (WeaponAndCrossSight.WeaponAndCrossSightTypes)reformer.GetConfig().Reticle);

                //Debug.LogError(string.Format("创建准星 uid = {0}, tid = {1} , type = {2}, null? {3}", uid, weapon.GetConfig().Id, weapon.GetConfig().Reticle , (weaponAndCross== null)));

                if (weaponAndCross == null)
                    return;
                m_IPerceptronTarget.AddWeaponAndCrossSight(uid, weaponAndCross);
            }
        }
        else
        {
            //非转化炉模式
            m_IPerceptronTarget.DeleReformerWeaponAndCrossSight();
        }
    }



    /// <summary>
    /// 武器热键响应
    /// </summary>
    private void WeaponHotKeyOperation(IComponentEvent entityEvent)
    {
        SkillHotkey skillHotkey = entityEvent as SkillHotkey;
        IWeapon weapon = m_SkillProxy.GetCurrentWeapon();
        if (weapon == null)
            return;

        //武器&准星循环
        WeaponAndCrossSight weaponCS = m_SkillProxy.GetCurrentWeaponAndCrossSight();
        if (weaponCS != null)
            weaponCS.OnHotKey( skillHotkey);
    }

    /// <summary>
    ///通知武器准星Size变化
    /// </summary>
    private void ChangeCrossSightSize(IComponentEvent entityEvent)
    {
        //武器&准星循环
        WeaponAndCrossSight weaponCS = m_SkillProxy.GetCurrentWeaponAndCrossSight();
        if (weaponCS != null)
            weaponCS.ChangeCrossSightSize();
    }

    /// <summary>
    ///武器释放的技能结束
    /// </summary>
    private void WeaponSkillFinish(IComponentEvent entityEvent)
    {
        WeaponSkillFinish weaponskillfinish = entityEvent as WeaponSkillFinish;
        if (!weaponskillfinish.IsMain)
            return;
        PlayerShipSkillVO skillVO = m_SkillProxy.GetCurrentWeaponSkillVO();
        if(skillVO != null && skillVO.GetID() == weaponskillfinish.skillId && weaponskillfinish.skillId>0)
        {
            WeaponAndCrossSight weaponCS = m_SkillProxy.GetCurrentWeaponAndCrossSight();
            if (weaponCS != null)
                weaponCS.WeaponSkillFinish();
        }
    }

    /// <summary>
    ///广播技能目标列表
    /// </summary>
    private void BroadCastSkillBeginTargets(IComponentEvent entityEvent)
    {
        BroadCastSkill_BeginTargets bctarges = entityEvent as BroadCastSkill_BeginTargets;
        m_PerceptronTarget.SetBroadCastTargets(bctarges.targets, bctarges.direction,true);

        BroadCastSkill_ReleaseSkill releaseSkill = new BroadCastSkill_ReleaseSkill();
        releaseSkill.skillId = bctarges.skillId;
        SendEvent(ComponentEventName.BroadCastSkill_ReleaseSkill , releaseSkill);
    }

    /// <summary>
    /// 广播蓄力索引，目标列表等
    /// </summary>
    private void BroadCastSkillAccumulation(IComponentEvent entityEvent)
    {
        BroadCastSkill_Accumulation bctarges = entityEvent as BroadCastSkill_Accumulation;
        m_PerceptronTarget.SetBroadCastTargets(bctarges.targets, bctarges.direction, true);

        //成功
        AccumulationIndexResult accResult = new AccumulationIndexResult();
        accResult.skillId = (int)bctarges.skillId;
        accResult.accumulationIndex = (int)bctarges.groupIndex;
        SendEvent(ComponentEventName.AccumulationIndex, accResult);
    }

    /// <summary>
    ///广播技能目标列表
    /// </summary>
    private void BroadCastSkillChangeTargets(IComponentEvent entityEvent)
    {
        BroadCastSkill_ChangeTargets bctarges = entityEvent as BroadCastSkill_ChangeTargets;
        m_PerceptronTarget.SetBroadCastTargets(bctarges.targets, bctarges.direction, false);
    }
}