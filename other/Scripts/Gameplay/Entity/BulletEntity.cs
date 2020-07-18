/*===============================
 * Author: [Allen]
 * Purpose: 子弹单位
 * Time: 2019/12/23 14:34:01
================================*/
using System.Collections.Generic;
using Assets.Scripts.Define;
using Gameplay.Battle.Skill;
using Gameplay.Battle.Timeline;

public class BulletEntity : BulletPhysics
    , IPerceptronTarget
    ,IFlyerProperty
{


    public override void InitializeComponents()
    {
        base.InitializeComponents();
        AddEntityComponent<FlyerActionComponent, IFlyerProperty>(this);                                            //飞行Action

    }

    public bool HasState(ComposableState state)
    {
        throw new System.NotImplementedException();
    }

    public bool HaveWeaponAndCrossSight(ulong uid)
    {
        throw new System.NotImplementedException();
    }

    public WeaponAndCrossSight GetWeaponAndCrossSight(ulong uid)
    {
        throw new System.NotImplementedException();
    }

    public void AddWeaponAndCrossSight(ulong uid, WeaponAndCrossSight Weapon)
    {
        throw new System.NotImplementedException();
    }

    public void DeleReformerWeaponAndCrossSight()
    {
        throw new System.NotImplementedException();
    }
}

