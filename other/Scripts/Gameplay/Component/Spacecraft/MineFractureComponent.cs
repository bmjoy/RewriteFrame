using Assets.Scripts.Define;
using Leyoutech.Core.Effect;
using Leyoutech.Utility;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public interface IMineFractureProperty
{
    Transform GetSkinTransform();
    double GetAttribute(AttributeName key);
	void RemoveCollider_Runtime(Collider unusedCollider);
    uint GetItemID();
}

public class MineFractureComponent : EntityComponent<IMineFractureProperty>
{
    private IMineFractureProperty m_Property;
    /// <summary>
    /// 累计伤害
    /// </summary>
    private double m_TotalDamage = 0f;
    /// <summary>
    /// 产生碎片伤害
    /// </summary>
    private double m_FractureDamage = 0f;
    /// <summary>
    /// 产生碎片特效名
    /// </summary>
    private string m_MineralFxName;
    /// <summary>
    /// 碎片数
    /// </summary>
    private int m_FractureCount = 0;
    /// <summary>
    /// 碎矿特效
    /// </summary>
    private readonly List<EffectController> m_MineralFxList = new List<EffectController>();

    public override void OnInitialize(IMineFractureProperty property)
    {
        m_Property = property;

        CfgEternityProxy cfgEternityProxy = GameFacade.Instance.RetrieveProxy(ProxyName.CfgEternityProxy) as CfgEternityProxy;
        m_MineralFxName = cfgEternityProxy.GetMineralFx(m_Property.GetItemID());

        m_FractureCount = cfgEternityProxy.GetItemModelByKey(m_Property.GetItemID()).MineralNum;
        m_FractureDamage = m_Property.GetAttribute(AttributeName.kHPMax) / m_FractureCount;
    }

    public override void OnDestroy()
    {
        foreach (var item in m_MineralFxList)
        {
            item.StopAndRecycleFX();
        }
        m_MineralFxList.Clear();
        base.OnDestroy();
    }

    public override void OnAddListener()
    {
        AddListener(ComponentEventName.MineDamage, OnMineDamage);
    }

    private void OnMineDamage(IComponentEvent obj)
    {
        MineDamage mineDamage = obj as MineDamage;
        OnMineFracture(mineDamage.HitCollider, mineDamage.Damage);
    }

    private void OnMineFracture(Collider collider, uint damage)
    {
        if (m_FractureCount == 0)
        {
            return;
        }

        m_TotalDamage += damage;
        if (m_TotalDamage >= m_FractureDamage)
        {
            m_TotalDamage = m_TotalDamage - m_FractureDamage;

            if (Leyoutech.Utility.EffectUtility.IsEffectNameValid(m_MineralFxName))
            {
                EffectController mineralFx = EffectManager.GetInstance().CreateEffect(m_MineralFxName, EffectManager.GetEffectGroupNameInSpace(false));
                mineralFx.transform.position = collider.gameObject.transform.position;
                m_MineralFxList.Add(mineralFx);
            }

            GameObject.Destroy(collider.gameObject);
			m_Property.RemoveCollider_Runtime(collider);
			SendEvent(ComponentEventName.PlayFragmentationSound, null);
		}
    }
}
