using Assets.Scripts.Define;
using Leyoutech.Core.Effect;
using Leyoutech.Core.Loader.Config;
using Eternity.FlatBuffer;
using Game.VFXController;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System.Collections;
using System;
using Leyoutech.Utility;

public interface ISpacecraftAvatarProperty
{
	string GetModelName();
    Transform GetSkinRootTransform();
    void SetSkinTransform(Transform transform);
	KHeroType GetHeroType();
	bool IsMain();
	uint UId();
	void SetPresentation(SpacecraftPresentation container);
	Npc GetNPCTemplateVO();
	SpacecraftEntity GetOwner();
	SpacecraftPresentation GetPresentation();
	List<Collider> GetAllColliders();
	void AddCollider_Runtime(Collider newCollider);
	void ResetLODColliders(List<Collider> colliders);

	Transform GetRootTransform();
	Transform GetSkinTransform();

	float GetFireCountdown();
	void SetFireCountdown(float countdown);
	float GetUnderAttackWarningToneCountdown();
	void SetUnderAttackWarningToneCountdown(float countdown);
    Transform GetSyncTarget();
	void SetLODLevel(int LODLevel);
	int GetLODLevel();
    void SetSkinVisiable(bool isShow);
	bool IsSeal();
    bool IsDead();
}

/// <summary>
/// 船形态模型拼装逻辑
/// </summary>
public class SpacecraftAvatarComponent : EntityComponent<ISpacecraftAvatarProperty>
{
	/// <summary>
	/// 每隔多少帧, Collider的LOD逻辑更新一次
	/// </summary>
	private const int COLLIDER_LOD_UPDATE_INTERVAL = 100;

    private GameObject m_Model;
	private SpacecraftPresentation m_Presentation;
	private ISpacecraftAvatarProperty m_Property;

	/// <summary>
	/// 转化炉的聚气特效
	/// 这是一个循环特效, 需要在松开聚气按钮的时候停止播放
	/// </summary>
	private EffectController m_ReformerReadyFX;
	/// <summary>
	/// 过载循环特效
	/// </summary>
	private EffectController m_OverloadLoopFX;
	/// <summary>
	/// 转化炉开启后的循环特效
	/// </summary>
	private EffectController m_ReformerLoopFX;

	// TODO, 这里改成三种可见方式
	// 1. 只隐藏meshrender; 2. 隐藏meshrender 和 所有循环特效; 3. 隐藏meshrender 和 所有循环特效 和 所有collider

	/// <summary>
	/// 可见性的引用计数. 大于等于0表示可见, 小于0表示不可见
	/// 每次设置可见性时增减这个值
	/// </summary>
	private List<int> m_RendererVisible;
	/// <summary>
	/// Collider Is Active的引用计数. 同上
	/// </summary>
	private int m_ColliderActive = 0;

	private bool m_Died;
	private int m_DeathFXIndex;

	private Vector3 m_MoveValue;

	/// <summary>
	/// 隐身特效实例
	/// </summary>
	private EffectController m_HideFxInstance = null;

	/// <summary>
	/// 隐身特效实例
	/// </summary>
	private EffectController m_LineFxInstance = null;

	/// <summary>
	/// 配合连线特效实例
	/// </summary>
	private EffectController m_EnergyBodyFxInstance = null;

	/// <summary>
	/// 缓存LightBeam脚本
	/// </summary>
	private VFXBeam m_BeamCache;

	/// <summary>
	/// 死亡停留特效实例
	/// </summary>
	private EffectController m_DeadCorpseFxInstance = null;

    /// <summary>
    /// 死亡滑行特效实例
    /// </summary>
    private EffectController m_DeadSlidingFxInstance = null;

	/// <summary>
	/// 选中的目标会有一个描边效果
	/// </summary>
	private EffectController m_OutlineVFX;

	/// <summary>
	/// 更新ColliderLOD逻辑的计数器
	/// </summary>
	private int m_ColliderLODUpdateCounter;

	// Cache
	private CfgEternityProxy m_EternityProxy;
	private GameplayProxy m_GameplayProxy;

	public override void OnInitialize(ISpacecraftAvatarProperty property)
	{
		m_Property = property;
		m_RendererVisible = new List<int>((int)ChangeSpacecraftVisibilityEvent.VisibilityType.Count);
		m_EternityProxy = GameFacade.Instance.RetrieveProxy(ProxyName.CfgEternityProxy) as CfgEternityProxy;
		m_GameplayProxy = GameFacade.Instance.RetrieveProxy(ProxyName.GameplayProxy) as GameplayProxy;
		AssetUtil.LoadAssetAsync(property.GetModelName(),
            (pathOrAddress, returnObject, userData) =>
            {
				if (returnObject != null)
                {
                    OnLoadModel((GameObject)returnObject, property.GetModelName());
                }
                else
                {
                    Debug.LogError(string.Format("资源加载成功，但返回null,  pathOrAddress = {0}", pathOrAddress));
                }
            });
    }

	public override void OnAddListener()
	{
		AddListener(ComponentEventName.ActivateBurst, OnBurstStateChanged);
		AddListener(ComponentEventName.BurstPressed, OnBurstPressed);
		AddListener(ComponentEventName.SetSpacecraftVisibility, OnChangeVisibility);
		AddListener(ComponentEventName.ActivateSpacecraftCollider, OnActivateCollider);
		AddListener(ComponentEventName.MaxPeerlessReached, OnMaxPeerlessReached);
		AddListener(ComponentEventName.SpacecraftIsSelectedAsTarget, OnSelectedAsTarget);
		///AddListener(ComponentEventName.ShowDeviceDeadFX, OnShowDeviceDeadFX);
		AddListener(ComponentEventName.LineEffectEnd, OnLineEffectEnd);
		AddListener(ComponentEventName.SealEnd, OnSealEnd);
		AddListener(ComponentEventName.Dead, OnDead);
	}

	public override void OnUpdate(float deltaTime)
	{
		// 因为现在无法控制 AddressableBundle, AvatarComponent, VFXReplaceMesh的更新顺序, 所以在这里强制保证加载完模型以后的下一帧才显示模型
		// FIXME, 以后会整理飞船的所有显示的节点结构. 所以, 不要逼逼, 过了这个9.13版本我就有空改了
		//if (m_ModelLoadCompleteInThisFrame && Time.time > m_TimeOfModelLoadComplete)
		//{
		//	m_Presentation.SetVisibilityOfEntireSpacecraft(true);

		//	m_ModelLoadCompleteInThisFrame = false;
		//}

		//  开火倒计时
		float fireCountdown = m_Property.GetFireCountdown();
		fireCountdown -= Time.deltaTime;
		fireCountdown = Mathf.Clamp(fireCountdown, 0, float.MaxValue);
		m_Property.SetFireCountdown(fireCountdown);

		// 受击提示音效的倒计时
		float underAttackCountdown = m_Property.GetUnderAttackWarningToneCountdown();
		underAttackCountdown -= Time.deltaTime;
		underAttackCountdown = Mathf.Clamp(underAttackCountdown, 0, float.MaxValue);
		m_Property.SetUnderAttackWarningToneCountdown(underAttackCountdown);

        //调整描边特效的颜色
        AdjustOutlineEffectColor();

		// 更新Collider的LOD
		if (m_ColliderLODUpdateCounter++ == COLLIDER_LOD_UPDATE_INTERVAL)
		{
			m_ColliderLODUpdateCounter = 0;
			UpdateColliderLOD(false);
		}
	}

	public override void OnDestroy()
	{
		base.OnDestroy();

		//AssetManager.ReleaseInstance(m_Model);
		if (m_ReformerReadyFX)
		{
			m_ReformerReadyFX.RecycleFX();
			m_ReformerReadyFX = null;
		}
		if (m_OverloadLoopFX)
		{
			m_OverloadLoopFX.RecycleFX();
			m_OverloadLoopFX = null;
		}
		if (m_ReformerLoopFX)
		{
			m_ReformerLoopFX.RecycleFX();
			m_ReformerLoopFX = null;
		}
		if (m_DeadCorpseFxInstance)
		{
			m_DeadCorpseFxInstance.RecycleFX();
			m_DeadCorpseFxInstance = null;
		}
		if (m_DeadSlidingFxInstance)
		{
			m_DeadSlidingFxInstance.RecycleFX();
			m_DeadSlidingFxInstance = null;
		}
		if (m_OutlineVFX)
		{
			m_OutlineVFX.RecycleFX();
			m_OutlineVFX = null;
		}
		if (m_HideFxInstance)
		{
			m_HideFxInstance.RecycleFX();
			m_HideFxInstance = null;
		}
		if (m_LineFxInstance)
		{
			m_LineFxInstance.RecycleFX();
			m_LineFxInstance = null;
		}
		if (m_EnergyBodyFxInstance)
		{
			m_EnergyBodyFxInstance.RecycleFX();
			m_EnergyBodyFxInstance = null;
		}
	}

	/// <summary>
	/// 转化炉的开启关闭特效
	/// </summary>
	/// <param name="componentEvent"></param>
	private void OnBurstStateChanged(IComponentEvent componentEvent)
	{
		//ActivateBurstEvent burstEvent = componentEvent as ActivateBurstEvent;
		//if (burstEvent.Active)
		//{
			//if (m_Presentation != null)
			//{
			//	if (Leyoutech.Utility.EffectUtility.IsEffectNameValid(m_Presentation.ReformerBeginFXAddress))
			//	{
			//		EffectController ReformerBeginFX = EffectManager.GetInstance().CreateEffect(m_Presentation.ReformerBeginFXAddress, EffectManager.GetEffectGroupNameInSpace(m_Property.IsMain()));
			//		ReformerBeginFX.transform.SetParent(m_Property.GetSkinRootTransform(), false);
			//		ReformerBeginFX.SetCreateForMainPlayer(m_Property.IsMain());
			//	}

			//	if (m_ReformerLoopFX == null)
			//	{
   //                 if (Leyoutech.Utility.EffectUtility.IsEffectNameValid(m_Presentation.ReformerLoopFXAddress))
   //                 {
   //                     m_ReformerLoopFX = EffectManager.GetInstance().CreateEffect(m_Presentation.ReformerLoopFXAddress, EffectManager.GetEffectGroupNameInSpace(m_Property.IsMain()));
   //                     m_ReformerLoopFX.transform.SetParent(m_Property.GetSkinRootTransform(), false);
   //                     m_ReformerLoopFX.SetCreateForMainPlayer(m_Property.IsMain());
   //                     m_ReformerLoopFX.SetAutoRecycleWhenNotExistLivingParticles(false);
   //                 }
   //             }
			//	else
			//	{
			//		m_ReformerLoopFX.PlayFX();
			//	}
			//}

            ///PlayBattleStateAudio(WwiseMusicSpecialType.SpecialType_Voice_Converter_Explosion);
        //}
        //else
		//{
			//if (m_Presentation != null)
			//{
   //             if (Leyoutech.Utility.EffectUtility.IsEffectNameValid(m_Presentation.ReformerEndFXAddress))
   //             {
			//	    EffectController ReformerEndFX = EffectManager.GetInstance().CreateEffect(m_Presentation.ReformerEndFXAddress, EffectManager.GetEffectGroupNameInSpace(m_Property.IsMain()));
			//	    ReformerEndFX.transform.SetParent(m_Property.GetSkinRootTransform(), false);
			//	    ReformerEndFX.SetCreateForMainPlayer(m_Property.IsMain());
   //             }
			//}
			
			//m_ReformerLoopFX?.StopFX();

            ///PlayBattleStateAudio(WwiseMusicSpecialType.SpecialType_Voice_Converter_Explosion_StateOver);
        //}
    }

	/// <summary>
	/// 转化炉的聚气
	/// 这是一个纯客户端表现的事. 聚气0.5秒后再按某个键开启转化炉
	/// </summary>
	/// <param name="componentEvent"></param>
	private void OnBurstPressed(IComponentEvent componentEvent)
	{
		ActivateBurstPressedEvent burstReadyEvent = componentEvent as ActivateBurstPressedEvent;

		if (burstReadyEvent.Ready)
		{
			if (m_Presentation != null)
			{
				if (m_ReformerReadyFX == null && Leyoutech.Utility.EffectUtility.IsEffectNameValid(m_Presentation.ReformerChargingFXAddress))
				{
					m_ReformerReadyFX = EffectManager.GetInstance().CreateEffect(m_Presentation.ReformerChargingFXAddress, EffectManager.GetEffectGroupNameInSpace(m_Property.IsMain()));
					m_ReformerReadyFX.transform.SetParent(m_Property.GetSkinRootTransform(), false);
					m_ReformerReadyFX.SetCreateForMainPlayer(m_Property.IsMain());
					m_ReformerReadyFX.SetAutoRecycleWhenNotExistLivingParticles(false);
				}
				else
				{
					m_ReformerReadyFX.PlayFX();
				}
			}
		}
		else
		{
			m_ReformerReadyFX?.StopFX();
		}
	}
	

	// 因为现在无法控制 AddressableBundle, AvatarComponent, VFXReplaceMesh的更新顺序, 所以强制保证加载完模型以后的下一帧才显示模型
	// FIXME, 以后会整理飞船的所有显示的节点结构. 所以, 不要逼逼, 过了这个9.13版本我就有空改了

	/// <summary>
	/// 这一帧刚Load完, 先隐藏再显示
	/// </summary>
	private bool m_ModelLoadCompleteInThisFrame;
	/// <summary>
	/// 加载完模型的这一帧的时间
	/// </summary>
	private float m_TimeOfModelLoadComplete;

	/// <summary>
	/// 飞船模型载入完毕
	/// </summary>
	/// <param name="asyncOperation"></param>
	private void OnLoadModel(GameObject asyncOperation,string path)
    {
        m_Model = asyncOperation;
        if (m_Model == null)
        {
            throw new System.Exception("m_Model is null");
        }

		if (!asyncOperation.IsPooled())
		{
			asyncOperation.CreatePool(1, path);
		}
		m_Model = asyncOperation.Spawn(m_Property.GetSkinRootTransform());

		if (m_Model.transform.parent == null)
        {
			m_Model.Recycle();
            return;
        }

		m_Model.transform.localPosition = Vector3.zero;
        m_Model.transform.localRotation = Quaternion.identity;
        m_Model.transform.localScale = Vector3.one;

		m_Property.SetSkinTransform(m_Model.transform);

		SendEvent(ComponentEventName.AvatarLoadFinish, new AvatarLoadFinishEvent()
        {
			SpacecraftPresentation = m_Model.GetComponentInChildren<SpacecraftPresentation>(),
            Animator = m_Model.GetComponentInChildren<Animator>(),
			Animators = m_Model.GetComponentsInChildren<Animator>()
		});

		if (m_Property.GetHeroType() == KHeroType.htDetector)
		{
			SendEvent(ComponentEventName.OnGetMeshRenderer, new GetMeshRendererEvent()
			{
				MeshRenderer = m_Model.GetComponentInChildren<MeshRenderer>(),
				/// TODO.
				Transform = m_Model.transform.Find("Effect_A")
			});
		}

		int spacecraftLayer = LayerUtil.GetLayerByHeroType(m_Property.GetHeroType(), m_Property.IsMain());
		LayerUtil.SetGameObjectToLayer(m_Model, spacecraftLayer, true);
        if (m_Property.GetSyncTarget() != null)
        {
		    LayerUtil.SetGameObjectToLayer(m_Property.GetSyncTarget().gameObject, spacecraftLayer, true);
        }

		m_Presentation = m_Model.GetComponent<SpacecraftPresentation>();
		if (m_Presentation == null)
		{
			Debug.LogWarning(string.Format("这个错误不可忽略! 飞船 {0} 没有挂点和表现信息, 找美术加上.", m_Model.name));
		}
		else
		{
			m_Property.SetPresentation(m_Presentation);
			// 复制一份CapsuleCollider给同步模块使用. 这个复制出来的CapsuleCollider不会被LODSwitchItem影响, 所以不会被Disable
			// 只有玩家才会与场景碰撞. 怪物和NPC都不与场景碰撞. 程旭与王梓晨商议决定
			if (m_Property.GetHeroType() == KHeroType.htPlayer)
			{
				CapsuleCollider capsuleCollider = m_Presentation.GetCapsuleCollider();
				if (capsuleCollider == null)
				{
					Debug.LogError(string.Format("这个错误不可忽略! 飞船 {0} 没有胶囊碰撞体, 找美术加上.", m_Model.name));
				}

				GameObject colliderCopy = GameObject.Instantiate(capsuleCollider.gameObject, m_Property.GetSyncTarget());
				colliderCopy.transform.position = capsuleCollider.transform.position;
				colliderCopy.transform.rotation = capsuleCollider.transform.rotation;
				colliderCopy.name = string.Format("{0}_ForMotionSync", capsuleCollider.name);
				LayerUtil.SetGameObjectToLayer(colliderCopy, GameConstant.LayerTypeID.ServerSynchronization, false);
			}
			else
            {
                if (m_Property.GetHeroType() == KHeroType.htMine)
                {
                    List<Collider> capsuleColliders = m_Presentation.GetAllCapsuleColliders();
                    if (capsuleColliders != null)
                    {
                        foreach (var item in capsuleColliders)
                        {
                            item.gameObject.layer = GameConstant.LayerTypeID.SkillCrossSpacecraftBlock;
                        }
                    }                        
                }
				
				//if (EffectUtility.IsEffectNameValid(m_Presentation.BirthFXAddress))
				//{
				//	EffectController BirthFX = EffectManager.GetInstance().CreateEffect(m_Presentation.BirthFXAddress, EffectManager.GetEffectGroupNameInSpace(false), OnLoadBrithFXComplete,this);
				//	BirthFX.transform.SetParent(m_Property.GetSkinRootTransform(), false);
				//	BirthFX.SetCreateForMainPlayer(false);

				//	m_Presentation.SetVisibilityOfEntireSpacecraft(false);
				//}

				m_ModelLoadCompleteInThisFrame = true;
				m_TimeOfModelLoadComplete = Time.time;
			}

			//if (m_Died && m_Presentation.DeathFXAddressList != null && m_Presentation.DeathFXAddressList.Count != 0)
			//{
			//	EffectController deathFX = EffectManager.GetInstance().CreateEffect(m_Presentation.DeathFXAddressList[m_DeathFXIndex], EffectManager.GetEffectGroupNameInSpace(false));
			//	deathFX.transform.SetParent(m_Property.GetSkinRootTransform(), false);
			//	deathFX.SetCreateForMainPlayer(false);
			//}
		}

		UpdateVisibility();
		UpdateEnableOfAllCollider();
		UpdateColliderLOD(true);

        if (m_Property.IsDead())
        {
            m_Property.SetSkinVisiable(false);
        }

		/// 是探测器
		if (m_Property.GetOwner().m_EntityFatherOwnerID != 0 && m_Property.GetHeroType() == KHeroType.htDisturbor && !m_Property.IsDead())
		{
			AddLineEffect();
		}

		/// 封印
		if (m_Property.IsSeal())
		{
			AddHideEffect();
		}

		/// Npc出生音乐
		if (m_Property.GetHeroType() != KHeroType.htPlayer)
		{
			Npc npcVO = m_Property.GetNPCTemplateVO();
			if (npcVO.SoundAlive > 0)
			{
				SendEvent(ComponentEventName.PlaySound, new PlaySound()
				{
					SoundID = (int)npcVO.SoundAlive
				});
			}
		}
	}

	private void OnLoadBrithFXComplete(EffectController effect ,System.Object usedata)
	{
        SpacecraftAvatarComponent component = (SpacecraftAvatarComponent)usedata;

        if(component != null)
        {
            component.m_Presentation.SetVisibilityOfEntireSpacecraft(true);
            component.m_ModelLoadCompleteInThisFrame = false;
        }
    }

	private void OnLineEffectEnd(IComponentEvent componentEvent)
	{
		AddEnergyEffect();
	}

	private void OnSealEnd(IComponentEvent componentEvent)
	{
		ChangeHideEffectState();
	}

	private void OnDead(IComponentEvent componentEvent)
	{
		if (m_ReformerReadyFX != null)
			m_ReformerReadyFX.StopFX();

		if (m_OverloadLoopFX != null)
			m_OverloadLoopFX.StopFX();

		if (m_ReformerLoopFX != null)
			m_ReformerLoopFX.StopFX();

		if (m_HideFxInstance != null)
			m_HideFxInstance.StopFX();

		/// 探测器死亡播连线收回
		if (m_LineFxInstance != null)
		{
			if (m_Property.GetHeroType() == KHeroType.htDisturbor)
			{
				RemoveLineEffect();
			}
			else
			{
				m_LineFxInstance.StopFX();
			}
		}

		if (m_EnergyBodyFxInstance != null)
			m_EnergyBodyFxInstance.StopFX();

		if (m_DeadCorpseFxInstance != null)
			m_DeadCorpseFxInstance.StopFX();

		if (m_DeadSlidingFxInstance != null)
			m_DeadSlidingFxInstance.StopFX();

		if (m_OutlineVFX != null)
			m_OutlineVFX.StopFX();

		if (m_Property.GetHeroType() == KHeroType.htMine && m_Property.GetOwner().m_EntityFatherOwnerID > 0)
		{
			m_GameplayProxy.RemoveEntityFromEntityGroup(m_Property.GetOwner().m_EntityFatherOwnerID, m_Property.GetOwner());
		}
	}

	/// <summary>
	/// 添加隐身特效
	/// </summary>
	private void AddHideEffect()
	{
		/// TODO.读表
		if (m_HideFxInstance == null)
		{
			m_HideFxInstance = EffectManager.GetInstance().CreateEffect(m_EternityProxy.GetGamingConfig(1).Value.Treasure.Value.Effect.Value.InvisibleEffect, EffectManager.GetEffectGroupNameInSpace(m_Property.IsMain()));
			m_HideFxInstance.transform.SetParent(m_Property.GetSkinRootTransform(), false);
			m_HideFxInstance.SetCreateForMainPlayer(m_Property.IsMain());
		}
		else
		{
			m_HideFxInstance.PlayFX();
		}
	}

	/// <summary>
	/// 改变隐身特效状态
	/// </summary>
	private void ChangeHideEffectState()
	{
		VFXController vfxController = m_HideFxInstance.GetEffectObject();
		Animator animator = vfxController.GetComponentInChildren<Animator>();
		if (animator)
		{
			animator.SetTrigger("Materialized");
		}

		if (m_EnergyBodyFxInstance)
		{
			StopEnergyEffect();
		}
	}

	/// <summary>
	/// 添加连线特效
	/// </summary>
	private void AddLineEffect()
	{
		/// TODO.读表
		if (m_LineFxInstance == null)
		{
			m_LineFxInstance = EffectManager.GetInstance().CreateEffect(m_EternityProxy.GetGamingConfig(1).Value.Treasure.Value.Effect.Value.LinkEffect, EffectManager.GetEffectGroupNameInSpace(m_Property.IsMain()), OnLineEffectLoad , this);
			m_LineFxInstance.transform.SetParent(m_Property.GetRootTransform(), false);
			m_LineFxInstance.SetCreateForMainPlayer(m_Property.IsMain());
		}
		else
		{
			m_LineFxInstance.PlayFX();
		}
	}

	/// <summary>
	/// 移除连线特效
	/// </summary>
	private void RemoveLineEffect()
	{
		if (m_LineFxInstance != null)
		{
			VFXController vfxController = m_LineFxInstance.GetEffectObject();
			Animator animator = vfxController.GetComponentInChildren<Animator>();
			if (animator)
			{
				animator.SetTrigger("dissolve");
			}
		}
	}

	/// <summary>
	/// 连线特效回调函数
	/// </summary>
	/// <param name="obj"></param>
	private void OnLineEffectLoad(EffectController effect , System.Object usedata)
	{
        if(usedata != null)
        {
            SpacecraftAvatarComponent component = (SpacecraftAvatarComponent)usedata;

            uint ownerHeroID = component.m_Property.GetOwner().m_EntityFatherOwnerID;
            if (ownerHeroID != 0 && component.m_Property.GetHeroType() == KHeroType.htDisturbor)
            {
                SpacecraftEntity target = component.m_GameplayProxy.GetEntityById<SpacecraftEntity>(ownerHeroID) as SpacecraftEntity;

                effect.SetBeamTarget(component.m_Property.GetSkinTransform(), target.GetSkinTransform(), Vector3.zero, false, target.GetAllColliders());
                /// TODO.
                /// 时间配合特效里的动画时间
                UIManager.Instance.StartCoroutine(Excute(0.25f, () =>
                {
                    target.SendEvent(ComponentEventName.LineEffectEnd, null);
                }));
            }
        }
	}

	/// <summary>
	/// 延迟调用
	/// </summary>
	/// <param name="seconds">秒数</param>
	/// <param name="callBack">回调函数</param>
	/// <returns></returns>
	public static IEnumerator Excute(float seconds, Action callBack)
	{
		yield return new WaitForSeconds(seconds);
		callBack();
	}

	/// <summary>
	/// 被连接者添加连线效果特效
	/// </summary>
	private void AddEnergyEffect()
	{
		/// TODO.读表
		if (m_EnergyBodyFxInstance == null)
		{
			m_EnergyBodyFxInstance = EffectManager.GetInstance().CreateEffect(m_EternityProxy.GetGamingConfig(1).Value.Treasure.Value.Effect.Value.SurroundEffect, EffectManager.GetEffectGroupNameInSpace(m_Property.IsMain()));
			m_EnergyBodyFxInstance.transform.SetParent(m_Property.GetSkinRootTransform(), false);
			m_EnergyBodyFxInstance.SetCreateForMainPlayer(m_Property.IsMain());

			/// 音效
			TreasureHuntProxy treasure = GameFacade.Instance.RetrieveProxy(ProxyName.TreasureHuntProxy) as TreasureHuntProxy;
			treasure.DisturborSoundEffect(m_Property.UId());
		}
		else
		{
			m_EnergyBodyFxInstance.PlayFX();
		}
	}

	/// <summary>
	/// 移除连线效果特效
	/// </summary>
	private void StopEnergyEffect()
	{
		if (m_EnergyBodyFxInstance != null)
		{
			m_EnergyBodyFxInstance.RecycleFX();
			m_EnergyBodyFxInstance = null;
		}
	}

	private bool IsVisible(ChangeSpacecraftVisibilityEvent.VisibilityType type)
	{
		return m_RendererVisible[(int)type] >= 0;
	}

	private bool IsColliderEnable()
	{
		return m_ColliderActive >= 0;
	}

	private void UpdateVisibility()
	{
		if (!m_Presentation)
			return;

		for (int iType = 0; iType < m_RendererVisible.Count; iType++)
		{
			bool visible = m_RendererVisible[iType] > 0;
			switch ((ChangeSpacecraftVisibilityEvent.VisibilityType)iType)
			{
				case ChangeSpacecraftVisibilityEvent.VisibilityType.MainBody:
					m_Presentation.SetVisibilityOfMainBody(visible);
					break;
				case ChangeSpacecraftVisibilityEvent.VisibilityType.VFX:
					m_Presentation.SetVisibilityOfVFX(visible);
					break;
				case ChangeSpacecraftVisibilityEvent.VisibilityType.MainBodyAndVFX:
					m_Presentation.SetVisibilityOfMainBody(visible);
					m_Presentation.SetVisibilityOfVFX(visible);
					break;
				case ChangeSpacecraftVisibilityEvent.VisibilityType.EntireSpacecraft:
					m_Presentation.SetVisibilityOfEntireSpacecraft(visible);
					break;
			}
		}
	}
	
	private void UpdateEnableOfAllCollider()
	{
		m_Presentation?.EnableAllCollidersByOtherScript(IsColliderEnable());
	}

	private void OnChangeVisibility(IComponentEvent notification)
	{
		ChangeSpacecraftVisibilityEvent visibileNofity = notification as ChangeSpacecraftVisibilityEvent;

		bool oldVisible = m_RendererVisible[(int)visibileNofity.PartType] > 0;
		if (visibileNofity.Visible)
			m_RendererVisible[(int)visibileNofity.PartType]++;
		else
			m_RendererVisible[(int)visibileNofity.PartType]--;

		if (m_Presentation != null && (m_RendererVisible[(int)visibileNofity.PartType] > 0 != oldVisible))
			UpdateVisibility();
	}

	private void OnActivateCollider(IComponentEvent notification)
	{
		ActivateSpacecraftColliderEvent colliderActive = notification as ActivateSpacecraftColliderEvent;

		bool oldActive = IsColliderEnable();
		if (colliderActive.active)
			m_ColliderActive++;
		else
			m_ColliderActive--;

		if (IsColliderEnable() != oldActive)
			UpdateEnableOfAllCollider();
	}

	/// <summary>
	/// 无双值打到最大
	/// </summary>
	/// <param name="componentEvent"></param>
	private void OnMaxPeerlessReached(IComponentEvent componentEvent)
	{
        PlayBattleStateAudio(WwiseMusicSpecialType.SpecialType_Voice_ConverterPower_Full);
    }

    /// <summary>
    /// 被主角选中或者取消选中
    /// </summary>
    /// <param name="componentEvent"></param>
    private void OnSelectedAsTarget(IComponentEvent componentEvent)
	{
		BeSelectedAsTarget selectInfo = componentEvent as BeSelectedAsTarget;
		if (selectInfo.isSelected)
		{
			if (m_Presentation)
			{
				if (m_OutlineVFX == null)
				{
                    if (m_Property.GetHeroType() == KHeroType.htMine)
                    {
                        m_OutlineVFX = EffectManager.GetInstance().CreateEffect(AssetAddressKey.FX_SELECTIONOUTLINE_MINERAL, EffectManager.GetEffectGroupNameInSpace(false));
                        m_OutlineVFX.transform.SetParent(m_Property.GetSkinRootTransform(), false);
                        m_OutlineVFX.SetAutoRecycleWhenNotExistLivingParticles(false);
                        m_OutlineVFX.SetCreateForMainPlayer(false);
                    }
                    else
                    {
                        m_OutlineVFX = EffectManager.GetInstance().CreateEffect(AssetAddressKey.FX_SELECTIONOUTLINE, EffectManager.GetEffectGroupNameInSpace(false));
                        m_OutlineVFX.transform.SetParent(m_Property.GetSkinRootTransform(), false);
                        m_OutlineVFX.SetAutoRecycleWhenNotExistLivingParticles(false);
                        m_OutlineVFX.SetCreateForMainPlayer(false);
                    }
                }
				else
				{
					m_OutlineVFX.PlayFX();
				}
			}
		}
		else
		{
			m_OutlineVFX?.StopFX();
		}
    }

    /// <summary>
    /// 调整描边特效颜色
    /// </summary>
    private void AdjustOutlineEffectColor()
    {
        if (m_Property != null && m_Property.GetHeroType() == KHeroType.htMine)
        {
            if (m_OutlineVFX != null)
            {
                VFXController effectObject = m_OutlineVFX.GetEffectObject();
                if (effectObject)
                {
                    Animator animator = effectObject.GetComponent<Animator>();
                    if (animator)
                    {
                        float hp = (float)m_Property.GetOwner().GetAttribute(AttributeName.kHP);
                        float hpMax = (float)m_Property.GetOwner().GetAttribute(AttributeName.kHPMax);

                        float hpProgress = hpMax <= 0 ? 0 : hp / hpMax;

                        animator.SetFloat("Color", 0);
                    }
                }
            }
        }
    }

    /// <summary>
    /// 显示设备死亡特效
    /// </summary>
    /// <param name="componentEvent"></param>
 //   private void OnShowDeviceDeadFX(IComponentEvent componentEvent)
	//{
	//	if (m_Presentation.DeathFXAddressList.Count == 0)
	//		return;
		
	//	EffectController deathFX = EffectManager.GetInstance().CreateEffect(m_Presentation.DeathFXAddressList[0], EffectManager.GetEffectGroupNameInSpace(false));
	//	deathFX.transform.SetParent(m_Property.GetSkinRootTransform(), false);
	//	deathFX.SetCreateForMainPlayer(false);
	//}

	/// <summary>
	/// 战斗状态提示音效.
	/// 提示音的英语不会写-_-
	/// </summary>
	/// <param name="audioID"></param>
	private void PlayBattleStateAudio(WwiseMusicSpecialType SpecialType)
    {
        if (m_Property.IsMain() && !m_Property.IsDead())
		{
            WwiseUtil.PlaySound(WwiseManager.voiceComboID, SpecialType, WwiseMusicPalce.Palce_1st, false, null);
        }
    }

	private void UpdateColliderLOD(bool forceResetLODCollider)
	{
		if (m_Presentation == null)
			return;

		Camera mainCamera = CameraManager.GetInstance().GetMainCamereComponent().GetCamera();
		

		if (forceResetLODCollider)
		{
			m_Presentation.TrySetCurrentLODLevelOfColliders(0);
			m_Property.ResetLODColliders(m_Presentation.GetCollidersOfCurrentLODLevel());
		}
		else
		{
			bool needUpdateColliderList = m_Presentation.UpdateColliderLOD(mainCamera);
			if (needUpdateColliderList)
			{
				m_Property.ResetLODColliders(m_Presentation.GetCollidersOfCurrentLODLevel());
			}
		}
	}
}