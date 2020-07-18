using Assets.Scripts.Define;
using Leyoutech.Core.Effect;
using Game.VFXController;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameConstant;

/// <summary>
/// 隐身Buff总结
/// 
/// 隐身时, 对于友方来说:
///		看不见船体本身
///		包括隐身特效在内的所有船体特效
///		隐身船的Collider依然可用, 可以选中作为技能目标
///		
///	隐身时, 对于敌方来说:
///		看不见船体本身
///		看不见包括隐身特效在内的所有船体特效
///		隐身船的Collider可以与技能投射物发生碰撞, 但是不会被射线拾取到
///		
/// 实现方法:
/// 隐藏船体的功能由VFXReplaceMeshWithSpacecraft去做, 增加一个参数"HidePrimarySpacecraft", 控制是否隐藏原来的船体
/// 对于敌方完全隐藏所有特效的功能在 BuffEffectInvisible 里做, 隐藏所有的 SpacecraftEntity中缓存的船体特效
/// 对于隐身者的Collider不会被拾取的问题, 也由 BuffEffectInvisible 来做, 把隐身的船设为一个特殊的Layer: UnselectableSpacecraft
/// </summary>
public class BuffEffectInvisible : BuffEffectBase
{
	public BuffEffectInvisible(Buff buff) : base(buff)
	{

	}

	public override void OnBuffAdd()
	{
		m_Buff.BuffProperty.SetInvisible(true);
	}

	public override void OnBuffRemove()
	{
        base.OnBuffRemove();
        GameplayProxy gameplayProxy = GameFacade.Instance.RetrieveProxy(ProxyName.GameplayProxy) as GameplayProxy;
		SpacecraftEntity mainPlayer = gameplayProxy.GetMainPlayer();
		if (mainPlayer != null)
		{
			if (gameplayProxy.CanAttackToTarget(mainPlayer, m_Buff.BuffProperty.GetOwner()))
			{
				// 隐身的时候只剩下特效. 如果是敌方隐身, 连特效都看不见了
				// 隐身时隐藏船的本体的功能是在VFXReplaceMeshWithSpacecraft里面做的
				if (gameplayProxy.CanAttackToTarget(mainPlayer, m_Buff.BuffProperty.GetOwner()))
				{
					List<EffectController> vfxList = m_Buff.BuffProperty.GetAllVFXs();
					foreach (EffectController iVfx in vfxList)
					{
						iVfx.SetCreateForMainPlayer(false);
						iVfx.PlayFX();
					}
				}

				m_Buff.BuffProperty.GetPresentation().SetVisibilityOfVFX(true);

				// 现在创建隐身特效时直接把SkinRoot下面的所有东西都隐藏了. 
				int spacecraftLayer = LayerUtil.GetLayerByHeroType(m_Buff.BuffProperty.GetHeroType(), m_Buff.BuffProperty.IsMain());
				LayerUtil.SetGameObjectToLayer(m_Buff.BuffProperty.GetSkinRootTransform().gameObject, spacecraftLayer, true);
			}
		}

		m_Buff.BuffProperty.SetInvisible(false);
	}

	public override void OnLoopFXLoaded(EffectController vfx)
	{
		GameplayProxy gameplayProxy = GameFacade.Instance.RetrieveProxy(ProxyName.GameplayProxy) as GameplayProxy;
		SpacecraftEntity mainPlayer = gameplayProxy.GetMainPlayer();
		if (mainPlayer != null)
		{
			if (gameplayProxy.CanAttackToTarget(mainPlayer, m_Buff.BuffProperty.GetOwner()))
			{
				// 隐身的时候只剩下特效. 如果是敌方隐身, 连特效都看不见了
				List<EffectController> vfxList = m_Buff.BuffProperty.GetAllVFXs();
				foreach (EffectController iVfx in vfxList)
				{
					// HACK. 如果不加这个if判断, 就会把隐身启动的特效一起隐藏掉
					// 这里如果要完善这个逻辑, 需要做较多的工作. 我就直接偷个懒了.
					if (!iVfx.GetEffectObject().AutoStop)
						iVfx.StopAndRecycleFX(true);
				}

				m_Buff.BuffProperty.GetPresentation().SetVisibilityOfVFX(false);

				LayerUtil.SetGameObjectToLayer(m_Buff.BuffProperty.GetSkinRootTransform().gameObject, LayerTypeID.UnselectableSpacecraft, true);
			}
		}
	}
}
