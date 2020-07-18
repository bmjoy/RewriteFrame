using Assets.Scripts.Define;
using Leyoutech.Core.Effect;
using Eternity.FlatBuffer;
using System;
using UnityEngine;

public interface ISpacecraftPlayerAnimatorProperty
{
    uint GetItemID();
    Vector3 GetRotateAxis();
    Vector3 GetEngineAxis();
    bool IsMain();
    KHeroType GetHeroType();
	SpacecraftEntity GetOwner();
    SpacecraftMotionInfo GetCurrentSpacecraftMotionInfo();
    Rigidbody GetRigidbody();
    Transform GetRootTransform();
    MotionType GetMotionType();
    HeroState GetCurrentState();
	SpacecraftPresentation GetPresentation();
}

/// <summary>
/// 船形态Animator相关逻辑
/// </summary>
public class SpacecraftPlayerAnimatorComponent : EntityComponent<ISpacecraftPlayerAnimatorProperty>
{
    /// <summary>
    /// 阈值
    /// </summary>
    private const float THRESHOLD = 0.05f;

    /// <summary>
    /// Animator参数信息
    /// </summary>
    private readonly AnimatorParameterInfo Velocity = new AnimatorParameterInfo() { Name = "velocity", Type = AnimatorControllerParameterType.Float };
    private readonly AnimatorParameterInfo Attack = new AnimatorParameterInfo() { Name = "attack", Type = AnimatorControllerParameterType.Trigger };
    private readonly AnimatorParameterInfo Normal = new AnimatorParameterInfo() { Name = "normal", Type = AnimatorControllerParameterType.Trigger };
    /// <summary>
    /// 飞船特效挂点
    /// </summary>
    private SpacecraftPresentation m_SpacecraftHnagingPointList;
    /// <summary>
    /// Animator组件
    /// </summary>
    private Animator m_Animator;
    private Animator m_StateAnimator;
    /// <summary>
    /// 当前Animator的Velocity目标值
    /// </summary>
    private float m_AnimatorParameterVelocityTarget;
    /// <summary>
    /// 当前Animator的Velocity值
    /// </summary>
    private float m_AnimatorParameterVelocity;
    /// <summary>
    /// 音效播放状态
    /// </summary>
    bool m_IsPlayingSound = false;

    private Vector3 m_EngineAxis;
    private Vector3 m_RotateAxis;
    private ISpacecraftPlayerAnimatorProperty m_SpacecraftPlayerAnimatorProperty;

    private bool m_IsMain;
    private int m_musicComboID = -1;
    private KHeroType m_KHeroType;

	private EffectController m_LeapOwnPre;
	private EffectController m_LeapOwnPreBurst;
	private EffectController m_LeapOwnOutScreen;
	private EffectController m_LeapCamera;
    private EffectController m_LeapTPSIn;
    private Animator m_LeapCameraAnimator;

	/// <summary>
	/// 上一次的MotionType
	/// </summary>
	private MotionType m_LastMotionType;
	/// <summary>
	/// 是否在跃迁中
	/// </summary>
	private bool m_IsLeaping = false;

	private enum EngineAxisZ
    {
        NONE,
        FORWARD,
        BACKWARD,
    }
    private enum EngineAxisY
    {
        NONE,
        UPWARD,
        DOWNWARD,
    }
    private enum EngineAxisX
    {
        NONE,
        LEFT,
        RIGHT,
    }
    private enum RotateAxisY
    {
        NONE,
        LEFT,
        RIGHT,
    }

    EngineAxisZ m_EngineAxisZ = EngineAxisZ.NONE;
    EngineAxisY m_EngineAxisY = EngineAxisY.NONE;
    RotateAxisY m_RotateAxisY = RotateAxisY.NONE;

    public override void OnInitialize(ISpacecraftPlayerAnimatorProperty property)
    {
        m_SpacecraftPlayerAnimatorProperty = property;

        m_IsMain = property.IsMain();
        m_KHeroType = property.GetHeroType();
        if(m_KHeroType == KHeroType.htPlayer)
        {
            CfgEternityProxy cfgeternityproxy = GameFacade.Instance.RetrieveProxy(ProxyName.CfgEternityProxy) as CfgEternityProxy;
            m_musicComboID = cfgeternityproxy.GetItemByKey(property.GetItemID()).ItemUnion<Warship>().Value.MusicComboID;
		}
	}

	public override void OnAddListener()
	{
        AddListener(ComponentEventName.AvatarLoadFinish, OnSpacecraftAvatarLoadFinishEvent);
		AddListener(ComponentEventName.ShipJumpResponse, OnShipJumpResponse);
		AddListener(ComponentEventName.Dead, OnDead);
		AddListener(ComponentEventName.SpacecraftChangeState, OnSpacecraftChangeState);
		AddListener(ComponentEventName.PlayStateAnima, OnPlayStateAnima);
		AddListener(ComponentEventName.PlayDeviceDeadAnimation, OnPlayDeviceDeadAnimation);
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
		if (m_LeapOwnPre)
		{
			m_LeapOwnPre.RecycleFX();
			m_LeapOwnPre = null;
		}
		if (m_LeapOwnPreBurst)
		{
			m_LeapOwnPreBurst.RecycleFX();
			m_LeapOwnPreBurst = null;
		}
		if (m_LeapOwnOutScreen)
		{
			m_LeapOwnOutScreen.RecycleFX();
			m_LeapOwnOutScreen = null;
		}
		if (m_LeapCamera)
		{
			m_LeapCamera.RecycleFX();
			m_LeapCamera = null;
		}
		if (m_LeapTPSIn)
		{
			m_LeapTPSIn.RecycleFX();
			m_LeapTPSIn = null;
		}
	}

	public void OnSpacecraftChangeState(IComponentEvent obj)
	{
		if (m_StateAnimator != null)
		{
			SpacecraftChangeState state = obj as SpacecraftChangeState;
			EnumMainState oldMainState = state.OldMainState;
			EnumMainState newMainState = state.NewMainState;

			if (oldMainState == EnumMainState.Cruise && newMainState == EnumMainState.Fight)
			{
				m_StateAnimator.SetTrigger(Attack.Name);
			}
			else if (oldMainState == EnumMainState.Fight && newMainState == EnumMainState.Cruise)
			{
				m_StateAnimator.SetTrigger(Normal.Name);
			}
		}
	}

	public void OnPlayStateAnima(IComponentEvent obj)
	{
		if (m_StateAnimator != null)
		{
			PlayStateAnima state = obj as PlayStateAnima;
			m_StateAnimator.SetTrigger(state.Name);
		}
	}

	public void OnPlayDeviceDeadAnimation(IComponentEvent obj)
	{
		m_Animator.SetTrigger("dead");
	}

	public override void OnFixedUpdate()
    {
        if (m_Animator == null)
        {
            return;
        }

        OnSpacecraftAnimationChangeStateEvent();

        if (m_AnimatorParameterVelocityTarget >= 0 && m_AnimatorParameterVelocity != m_AnimatorParameterVelocityTarget)
        {
			/// Magic Number: 美术调的效果
            m_AnimatorParameterVelocity = Mathf.Lerp(m_AnimatorParameterVelocity, m_AnimatorParameterVelocityTarget, 8.0f * Time.fixedDeltaTime);
            m_Animator.SetFloat(Velocity.Name, m_AnimatorParameterVelocity);
        }

        if (m_LeapCameraAnimator != null)
        {
            Rigidbody rigidbody = m_SpacecraftPlayerAnimatorProperty.GetRigidbody();
            if (m_SpacecraftPlayerAnimatorProperty.GetHeroType() == KHeroType.htPlayer && rigidbody != null)
            {
                // 尾焰音效
                float velocityRate = Mathf.Abs(m_SpacecraftPlayerAnimatorProperty.GetRootTransform().InverseTransformVector(rigidbody.velocity).z / m_SpacecraftPlayerAnimatorProperty.GetCurrentSpacecraftMotionInfo().LineVelocityMax.z);
                if (!float.IsNaN(velocityRate))
                {
                    velocityRate = Mathf.Clamp01(velocityRate);
                    if (m_IsMain)
                    {
                        // 速度线 Magic Number: 美术调的效果
                        if (m_SpacecraftPlayerAnimatorProperty.GetCurrentState().IsHasSubState(EnumSubState.Leaping))
                        {
                            m_Animator.SetFloat(Velocity.Name, 1.0f);
                            m_LeapCameraAnimator.SetFloat(Velocity.Name, 1.0f);
                        }
                        else if (m_SpacecraftPlayerAnimatorProperty.GetCurrentState().GetMainState() == EnumMainState.Fight)
                        {
                            m_LeapCameraAnimator.SetFloat(Velocity.Name, velocityRate * 0.3f);
                        }
                        else if (m_SpacecraftPlayerAnimatorProperty.GetCurrentState().GetMainState() == EnumMainState.Cruise)
                        {
                            m_LeapCameraAnimator.SetFloat(Velocity.Name, velocityRate * 0.6f);
                        }
                    }
                    WwiseManager.SetParameter(WwiseRtpc.Rtpc_ShipVelocity, velocityRate);
                }
            }
        }

        /// 速度线朝向飞船速度方向
        if (m_LeapCamera != null)
        {
            Rigidbody rigidbody = m_SpacecraftPlayerAnimatorProperty.GetRigidbody();
            if (rigidbody != null)
            {
                Vector3 direction = rigidbody.velocity.normalized;
                if(!direction.Equals(Vector3.zero))
                  m_LeapCamera.transform.rotation = Quaternion.LookRotation(direction);
            }
        }
	}

    private System.Collections.IEnumerator DelayCreateLeapTPSInEffect()
    {
        Transform parent = m_SpacecraftPlayerAnimatorProperty.GetOwner().GetSkinRootTransform();
        Vector3 position = parent.position;
        Quaternion rotation = parent.rotation;
        parent = null;
        yield return new WaitForSeconds(0.0f);

		if (!m_LeapTPSIn)
		{
			m_LeapTPSIn = EffectManager.GetInstance().CreateEffect(
												Leyoutech.Core.Loader.Config.AssetAddressKey.FX_EFFECT_SKILL_YUEQIAN_THIRD_PERSON_IN
												, EffectManager.GetEffectGroupNameInSpace(m_IsMain));
			m_LeapTPSIn.transform.SetParent(null, false);
			m_LeapTPSIn.transform.localPosition = position;
			m_LeapTPSIn.transform.localRotation = rotation;
			m_LeapTPSIn.SetCreateForMainPlayer(m_IsMain);
		}
		m_LeapTPSIn.PlayFX();
	}

    private void OnShipJumpResponse(IComponentEvent obj)
	{
		ShipJumpResponseEvent @event = obj as ShipJumpResponseEvent;
		Assets.Scripts.Proto.S2C_SHIP_JUMP_RESPONSE data = @event.Data;

		switch (data.request_jump)
		{
			// 请求跃迁 
			case 1:
				if (!m_LeapOwnPre)
				{
					m_LeapOwnPre = EffectManager.GetInstance()
						.CreateEffect(Leyoutech.Core.Loader.Config.AssetAddressKey.FX_EFFECT_SKILL_YUEQIAN_OWN_PRE
							, EffectManager.GetEffectGroupNameInSpace(m_IsMain));
					m_LeapOwnPre.transform.SetParent(m_SpacecraftPlayerAnimatorProperty.GetOwner().GetSkinRootTransform(), false);
					m_LeapOwnPre.SetCreateForMainPlayer(m_IsMain);
					m_LeapOwnPre.SetAutoRecycleWhenNotExistLivingParticles(false);
				}
				m_LeapOwnPre.PlayFX();
				break;
			// 中止跃迁
			case 0:
				m_LeapOwnPre?.StopFX();
				break;
			default:
				Leyoutech.Utility.DebugUtility.Assert(false, string.Format("not handle request_jump({0})", data.request_jump));
				break;
		}
	}

	private void OnLeapCameraCreated(EffectController arg1 , System.Object usedata)
	{
		Leyoutech.Utility.DebugUtility.Log(BaseEntity.LEAP_LOG_TAG
			, string.Format("Animator leap created leap vfx: {0} Player: {1}"
				, Leyoutech.Core.Loader.Config.AssetAddressKey.FX_SOCKET_FX_CAMERA
				, m_SpacecraftPlayerAnimatorProperty.GetOwner().UId())
			, m_SpacecraftPlayerAnimatorProperty.GetOwner());


        SpacecraftPlayerAnimatorComponent component = (SpacecraftPlayerAnimatorComponent)usedata;
        if(component != null)
        {
            component. m_LeapCameraAnimator = component.m_LeapCamera.transform.GetChild(0).GetComponent<Animator>();
        }
	}

	private void OnSpacecraftAvatarLoadFinishEvent(IComponentEvent componentEvent)
    {
        AvatarLoadFinishEvent spacecraftAvatarLoadFinishEvent = componentEvent as AvatarLoadFinishEvent;
        m_SpacecraftHnagingPointList = spacecraftAvatarLoadFinishEvent.SpacecraftPresentation;
        m_Animator = spacecraftAvatarLoadFinishEvent.Animator;
		Animator[] animators = spacecraftAvatarLoadFinishEvent.Animators;
		for (int i = 0; i < animators.Length; i++)
		{
			// TODO.
			// 先以命名为规则
			if (animators[i].transform.name.Contains("_action"))
			{
				m_StateAnimator = animators[i];
				AnimationClip[] animationClips = m_StateAnimator.runtimeAnimatorController.animationClips;
				AnimatorOverrideController animatorOverrideController;
				animatorOverrideController = new AnimatorOverrideController(m_StateAnimator.runtimeAnimatorController);
				m_StateAnimator.runtimeAnimatorController = animatorOverrideController;
				EnumMainState state = m_SpacecraftPlayerAnimatorProperty.GetCurrentState().GetMainState();
				if (state == EnumMainState.Fight)
				{
					for (int j = 0; j < animationClips.Length; j++)
					{
						// TODO.
						if (animationClips[j].name.Contains("_attacked"))
						{
							animatorOverrideController["DefaultState"] = animationClips[j];
							break;
						}
					}
				}
				else
				{
					for (int j = 0; j < animationClips.Length; j++)
					{
						// TODO.
						if (animationClips[j].name.Contains("_normal_state"))
						{
							animatorOverrideController["DefaultState"] = animationClips[j];
							break;
						}
					}
				}
				break;
			}
		}
		

		//播放尾焰音效
		if (m_SpacecraftPlayerAnimatorProperty != null)
        {
            WwiseUtil.PlaySound(m_musicComboID, WwiseMusicSpecialType.SpecialType_WarShipEngine_1,
                m_IsMain ? WwiseMusicPalce.Palce_1st : WwiseMusicPalce.Palce_3st,
                false, m_SpacecraftPlayerAnimatorProperty.GetRootTransform());
        }

        if (m_IsMain)
        {
			//添加速度线
			if (!m_LeapCamera)
			{
				m_LeapCamera = EffectManager.GetInstance().CreateEffect(
														Leyoutech.Core.Loader.Config.AssetAddressKey.FX_SOCKET_FX_CAMERA
														, EffectManager.GetEffectGroupNameInSpace(m_IsMain)
														, OnLeapCameraCreated,this);
				m_LeapCamera.transform.SetParent(m_SpacecraftPlayerAnimatorProperty.GetOwner().GetSkinRootTransform(), false);
				m_LeapCamera.SetCreateForMainPlayer(m_IsMain);
				m_LeapCamera.SetAutoRecycleWhenNotExistLivingParticles(false);
			}
			m_LeapCamera.PlayFX();
		}
	}

    private void OnSpacecraftAnimationChangeStateEvent()
    {
        if (m_SpacecraftHnagingPointList == null)
        {
            return;
        }

        Vector3 EngineAxis = m_SpacecraftPlayerAnimatorProperty.GetEngineAxis();
        Vector3 RotateAxis = m_SpacecraftPlayerAnimatorProperty.GetRotateAxis();
        m_AnimatorParameterVelocityTarget = EngineAxis.z;

        //Debug.LogError("OnSpacecraftAnimationChangeStateEvent EngineAxis:" + JsonUtility.ToJson(EngineAxis) + " RotateAxis:" + JsonUtility.ToJson(RotateAxis));

        EngineAxisZ engineAxisZ = EngineAxisZ.NONE;
        EngineAxisY engineAxisY = EngineAxisY.NONE;
        EngineAxisX engineAxisX = EngineAxisX.NONE;
        RotateAxisY rotateAxisY = RotateAxisY.NONE;

        if (EngineAxis != m_EngineAxis)
        {
            if (EngineAxis.z < -THRESHOLD)
            {
                engineAxisZ = EngineAxisZ.BACKWARD;
            }
            else
            {
                engineAxisZ = EngineAxisZ.NONE;
            }

            if (EngineAxis.y > THRESHOLD)
            {
                engineAxisY = EngineAxisY.UPWARD;
            }
            else if (EngineAxis.y < -THRESHOLD)
            {
                engineAxisY = EngineAxisY.DOWNWARD;
            }
            else
            {
                engineAxisY = EngineAxisY.NONE;
            }

            if (EngineAxis.x > THRESHOLD)
            {
                engineAxisX = EngineAxisX.RIGHT;
            }
            else if (EngineAxis.x < -THRESHOLD)
            {
                engineAxisX = EngineAxisX.LEFT;
            }
            else
            {
                engineAxisX = EngineAxisX.NONE;
            }

            m_EngineAxis = EngineAxis;

            if (m_EngineAxisZ != engineAxisZ)
            {
                m_EngineAxisZ = engineAxisZ;

                switch (m_EngineAxisZ)
                {
                    case EngineAxisZ.NONE:
                        m_SpacecraftHnagingPointList.remove_hanging_point_hou_effect();
                        break;
                    case EngineAxisZ.FORWARD:
                        break;
                    case EngineAxisZ.BACKWARD:
                        m_SpacecraftHnagingPointList.add_hanging_point_hou_effect();
                        break;
                    default:
                        break;
                }
            }

            if (m_EngineAxisY != engineAxisY)
            {
                m_EngineAxisY = engineAxisY;

                switch (m_EngineAxisY)
                {
                    case EngineAxisY.NONE:
                        m_SpacecraftHnagingPointList.remove_hanging_point_xia_effect();
                        m_SpacecraftHnagingPointList.remove_hanging_point_shang_effect();
                        break;
                    case EngineAxisY.UPWARD:
                        m_SpacecraftHnagingPointList.add_hanging_point_shang_effect();
                        m_SpacecraftHnagingPointList.remove_hanging_point_xia_effect();
                        break;
                    case EngineAxisY.DOWNWARD:
                        m_SpacecraftHnagingPointList.add_hanging_point_xia_effect();
                        m_SpacecraftHnagingPointList.remove_hanging_point_shang_effect();
                        break;
                    default:
                        break;
                }
            }

            //if (m_EngineAxisX != engineAxisX)
            //{
            //    m_EngineAxisX = engineAxisX;

            //    switch (m_EngineAxisX)
            //    {
            //        case EngineAxisX.NONE:
            //            m_SpacecraftHnagingPointList.add_hanging_point_you_effect();
            //            m_SpacecraftHnagingPointList.remove_hanging_point_zuo_effect();
            //            break;
            //        case EngineAxisX.LEFT:
            //            m_SpacecraftHnagingPointList.add_hanging_point_zuo_effect();
            //            m_SpacecraftHnagingPointList.remove_hanging_point_you_effect();
            //            break;
            //        case EngineAxisX.RIGHT:
            //            m_SpacecraftHnagingPointList.remove_hanging_point_zuo_effect();
            //            m_SpacecraftHnagingPointList.remove_hanging_point_you_effect();
            //            break;
            //        default:
            //            break;
            //    }
            //}
        }

        if (RotateAxis != m_RotateAxis)
        {
            if (RotateAxis.y > THRESHOLD)
            {
                rotateAxisY = RotateAxisY.RIGHT;
            }
            else if (RotateAxis.y < -THRESHOLD)
            {
                rotateAxisY = RotateAxisY.LEFT;
            }
            else
            {
                rotateAxisY = RotateAxisY.NONE;
            }

            m_RotateAxis = RotateAxis;

            if (m_RotateAxisY != rotateAxisY)
            {
                m_RotateAxisY = rotateAxisY;

                switch (m_RotateAxisY)
                {
                    case RotateAxisY.NONE:
                        m_SpacecraftHnagingPointList.remove_hanging_point_zuozhuan_effect();
                        m_SpacecraftHnagingPointList.remove_hanging_point_youzhuan_effect();
                        break;
                    case RotateAxisY.LEFT:
                        m_SpacecraftHnagingPointList.add_hanging_point_zuozhuan_effect();
                        m_SpacecraftHnagingPointList.remove_hanging_point_youzhuan_effect();
                        break;
                    case RotateAxisY.RIGHT:
                        m_SpacecraftHnagingPointList.add_hanging_point_youzhuan_effect();
                        m_SpacecraftHnagingPointList.remove_hanging_point_zuozhuan_effect();
                        break;
                    default:
                        break;
                }
            }
        }

        PlaySideFlameSound(m_EngineAxis != Vector3.zero || m_RotateAxis != Vector3.zero, m_SpacecraftPlayerAnimatorProperty.GetRootTransform());
    }

    /// <summary>
    /// 侧边喷火音效
    /// </summary>
    private void PlaySideFlameSound(bool play,Transform SoundParent)
    {
        if (m_IsPlayingSound == play)
        {
            return;
        }

        m_IsPlayingSound = play;

        if (m_IsPlayingSound)
        {
            WwiseUtil.PlaySound(m_musicComboID, WwiseMusicSpecialType.SpecialType_WarShipEngine_2,
                m_IsMain ? WwiseMusicPalce.Palce_1st : WwiseMusicPalce.Palce_3st,
                false, SoundParent);
        }
        else
        {
            WwiseUtil.PlaySound(m_musicComboID, WwiseMusicSpecialType.SpecialType_WarShipEngine_3,
                m_IsMain ? WwiseMusicPalce.Palce_1st : WwiseMusicPalce.Palce_3st,
                false, SoundParent);
        }
    }

	private void OnDead(IComponentEvent obj)
	{
		if (m_LeapOwnPre != null)
			m_LeapOwnPre.StopFX();

		if (m_LeapOwnPreBurst != null)
			m_LeapOwnPreBurst.StopFX();

		if (m_LeapOwnOutScreen != null)
			m_LeapOwnOutScreen.StopFX();

		if (m_LeapCamera != null)
			m_LeapCamera.StopFX();

		if (m_LeapTPSIn != null)
			m_LeapTPSIn.StopFX();
	}

	private void ClearLeapEffect()
	{
		if (m_LeapOwnPre != null)
		{
			m_LeapOwnPre.StopAndRecycleFX();
			m_LeapOwnPre = null;
		}

		if (m_LeapOwnPreBurst != null)
		{
			m_LeapOwnPreBurst.StopAndRecycleFX();
			m_LeapOwnPreBurst = null;
		}

		if (m_LeapOwnPreBurst != null)
		{
			m_LeapOwnPreBurst.StopAndRecycleFX();
			m_LeapOwnPreBurst = null;
		}
	}

}