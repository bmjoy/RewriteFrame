using Assets.Scripts.Define;
using Assets.Scripts.Proto;
using Leyoutech.Core.Effect;
using Game.VFXController;
using System.Collections.Generic;
using UnityEngine;

public interface IBuffProperty
{
	SpacecraftEntity GetOwner();
	bool IsMain();
	void AddCollider_Runtime(Collider collider);
	void RemoveCollider_Runtime(Collider unusedCollider);
	Transform GetSkinRootTransform();
	Transform GetSkinTransform();
	Transform GetRootTransform();
	void AddBuff(BuffVO buff);
	Buff GetBuff(uint id);
	void RemoveBuff(uint id);
	Dictionary<uint, Buff> GetAllBuffs();
	KHeroType GetHeroType();
	SpacecraftPresentation GetPresentation();

	void AddVFX(EffectController vfx);
	void RemoveVFX(EffectController vfx);
	List<EffectController> GetAllVFXs();

	void SetInvisible(bool invisible);
	bool IsInvisible();

	S2C_SYNC_NEW_HERO GetNewHeroRespond();

    ulong EntityId();
}

/// <summary>
/// 飞船的Buff管理组件
/// 目前所有的Buff相关的特效(开始, 循环, 结束)都是在这个类里统一播放. 因为这些的逻辑是一致的
/// 每种Buff的自有效果, 在 BuffEffectXXX 类中做. BuffEffectXXX 继承自 BuffEffectBase
///		Buff自有效果, 比如隐身. 可以让敌方完全看不见, 而友方能看见. 而且无法选中隐身的人作为目标, 但是有刚体的弹道可以击中他
/// </summary>
public class BuffComponent : EntityComponent<IBuffProperty>
{
	private IBuffProperty m_Property;

	public override void OnInitialize(IBuffProperty property)
	{
		m_Property = property;
		List<KHeroBuffList> buffList = m_Property.GetNewHeroRespond().bufferList;
		AddBuffEvent buffEvent = new AddBuffEvent();
		for (int iBuff = 0; iBuff < buffList.Count; iBuff++)
		{
            KHeroBuffList kHeroBuff = buffList[iBuff];
            buffEvent.buff = new BuffVO(kHeroBuff.bufferID, kHeroBuff.overlapCount, Time.time, kHeroBuff.nTime / 1000.0f, kHeroBuff.link_id , kHeroBuff.is_master !=0);
			OnAddBuff(buffEvent);
		}
	}

	public override void OnAddListener()
	{
		AddListener(ComponentEventName.MSG_SKILL_HIT, OnSkillHit);
		AddListener(ComponentEventName.BuffAdd, OnAddBuff);
		AddListener(ComponentEventName.BuffRemove, OnRemoveBuff);
	}

	// TODO BUFF. 想办法使用统一的通知. 或者直接把Buff的响应函数注册为消息回调
	private void OnSkillHit(IComponentEvent componentEvent)
	{
		foreach (KeyValuePair<uint, Buff> buffID2Value in m_Property.GetAllBuffs())
		{
			buffID2Value.Value.BuffEffect.OnEvent(ComponentEventName.MSG_SKILL_HIT, componentEvent);
		}
	}

	private void OnAddBuff(IComponentEvent componentEvent)
	{
		AddBuffEvent notification = (AddBuffEvent)componentEvent;

		m_Property.AddBuff(notification.buff);
		m_Property.GetBuff(notification.buff.ID)?.OnAddBuff();
	}

	private void OnRemoveBuff(IComponentEvent componentEvent)
	{
		RemoveBuffEvent notification = (RemoveBuffEvent)componentEvent;

		m_Property.GetBuff(notification.buffID)?.OnRemoveBuff();
		m_Property.RemoveBuff(notification.buffID);
	}

	public override void OnDestroy()
	{
		foreach (var buffID2Value in m_Property.GetAllBuffs())
		{
			buffID2Value.Value.OnRemoveBuff();
		}
	}
}
