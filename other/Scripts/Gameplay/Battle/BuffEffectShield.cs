using System.Collections;
using System.Collections.Generic;
using Leyoutech.Core.Effect;
using Game.VFXController;
using UnityEngine;

/// <summary>
/// 可以完全格挡攻击的Buff
/// 而且特效有一个护盾的Collider
/// </summary>
public class BuffEffectShield : BuffEffectBase
{
	/// <summary>
	/// buffID 到 受击特效 的映射表. 当有buff带有受击特效时, 每次被击中, 都会调用受击特效的OnHit接口
	/// 比如护盾, 每次被击中都有个水波纹效果
	/// TODO: LOD
	/// </summary>
	private List<VFXReactiveEffect> m_ReactiveVFXList;

	private List<Collider> m_ColliderList;

	public BuffEffectShield(Buff buff) : base(buff)
	{
		m_ReactiveVFXList = new List<VFXReactiveEffect>();
		m_ColliderList = new List<Collider>();
	}

	public override void OnBuffAdd()
	{

	}

	public override void OnBuffRemove()
	{
        base.OnBuffRemove();

        // 护盾类型的Buff带有Collider. 用于阻挡技能的投射物
        for (int iCollider = 0; iCollider < m_ColliderList.Count; iCollider++)
		{
			m_Buff.BuffProperty.RemoveCollider_Runtime(m_ColliderList[iCollider]);
		}

		m_ReactiveVFXList.Clear();
	}

	public override void OnLoopFXLoaded(EffectController vfx)
	{
		// TODO, GetComponentsInChildren 是一个比较慢的操作, 考虑直接放到VFXController里, 做特效的时候直接拖上
		Collider[] colliders = vfx.GetComponentsInChildren<Collider>(true);
		for (int iCollider = 0; iCollider < colliders.Length; iCollider++)
		{
			m_ColliderList.Add(colliders[iCollider]);
			m_Buff.BuffProperty.AddCollider_Runtime(colliders[iCollider]);
			LayerUtil.SetGameObjectToLayer(colliders[iCollider].gameObject, m_Buff.BuffProperty.GetRootTransform().gameObject.layer, false);
		}

		VFXReactiveEffect[] reactiveEffects = vfx.GetComponentsInChildren<VFXReactiveEffect>();
		for (int iEffect = 0; iEffect < reactiveEffects.Length; iEffect++)
		{
			m_ReactiveVFXList.Add(reactiveEffects[iEffect]);
		}
	}

	public override void OnEvent(ComponentEventName eventName, IComponentEvent eventParam)
	{
		if (eventName == ComponentEventName.MSG_SKILL_HIT)
		{
			SkillHitNotification skillHitInfo = eventParam as SkillHitNotification;
			if (skillHitInfo.TargetEntity != m_Buff.BuffProperty.GetOwner())
			{
				return;
			}

			for (int iEffect = 0; iEffect < m_ReactiveVFXList.Count; iEffect++)
			{
				m_ReactiveVFXList[iEffect].OnHit(skillHitInfo.HitPoint);
			}
		}
	}
}
