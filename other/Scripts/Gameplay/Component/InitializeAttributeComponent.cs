using Assets.Scripts.Define;
using Assets.Scripts.Proto;
using UnityEngine;

public interface IInitializeAttributeProperty
{
    S2C_SYNC_NEW_HERO GetNewHeroRespond();

    void SetAttribute(AttributeName key, double value);

	bool IsMain();
}

/// <summary>
/// 初始化属性
/// </summary>
public class InitializeAttributeComponent : EntityComponent<IInitializeAttributeProperty>
{
    IInitializeAttributeProperty m_InitializeAttributeProperty;

    public override void OnInitialize(IInitializeAttributeProperty property)
    {
        S2C_SYNC_NEW_HERO respond = property.GetNewHeroRespond();

        property.SetAttribute(AttributeName.kHP, respond.HP);
        property.SetAttribute(AttributeName.kHPMax, respond.MaxHP);
        property.SetAttribute(AttributeName.kShieldValue, respond.shield_value);
        property.SetAttribute(AttributeName.kShieldMax, respond.shield_value_max);
        property.SetAttribute(AttributeName.kPowerValue, respond.EnergyPower);
        property.SetAttribute(AttributeName.kPowerMax, respond.MaxEnergyPower);
        //新屬性目前沒有
        //property.SetAttribute(AttributeName.DefenseShield, respond.defense_shield_value);
        //property.SetAttribute(AttributeName.DefenseShieldTopLimit, respond.defense_shield_value_max);
        //property.SetAttribute(AttributeName.ManaShield, respond.mana_shield_value);
        //property.SetAttribute(AttributeName.ManaShieldTopLimit, respond.mana_shield_value_max);
        //property.SetAttribute(AttributeName.SuperMagnetic, respond.SuperMagnetic);
        //property.SetAttribute(AttributeName.SuperMagneticTopLimit, respond.MaxSuperMagnetic);
        //property.SetAttribute(AttributeName.RadiiOfSight, respond.ViewSight);

        //新屬性目前沒有
        if (property.IsMain())
        {
            Debug.Log("无双值当前: " + respond.current_peerless);
            Debug.Log("无双值最大: " + respond.max_peerless);
        }
        ///property.SetPeerless((uint)respond.current_peerless);
		property.SetAttribute(AttributeName.kConverterValue, respond.current_peerless);
		property.SetAttribute(AttributeName.kConverterMax, respond.max_peerless);

        //for (int i = 0; i < respond.speed_value_list.Length; i++)
        //{
        //    property.SetAttribute((AttributeName)respond.speed_value_list[i].id, respond.speed_value_list[i].value);
        //}
        //for (int i = 0; i < respond.overload_speed_value_list.Length; i++)
        //{
        //    property.SetAttribute((AttributeName)respond.overload_speed_value_list[i].id, respond.overload_speed_value_list[i].value);
        //}
        //for (int i = 0; i < respond.fight_speed_value_list.Length; i++)
        //{
        //    property.SetAttribute((AttributeName)respond.fight_speed_value_list[i].id, respond.fight_speed_value_list[i].value);
        //}
        // property.SetAttribute((AttributeName)respond.human_speed_value_list.id, respond.human_speed_value_list.value);
    }
}
