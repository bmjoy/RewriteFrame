using Assets.Scripts.Define;
using Leyoutech.Core.Effect;
using Eternity.FlatBuffer;
using System;
using System.Collections.Generic;
using UnityEngine;

public interface ISpacecraftHitWarningProperty
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
	Transform GetRootTransform();

	float GetFireCountdown();
	void SetFireCountdown(float countdown);
	float GetUnderAttackWarningToneCountdown();
	void SetUnderAttackWarningToneCountdown(float countdown);
	Transform GetSyncTarget();
}

public sealed class SpacecraftHitWarningComponent : EntityComponent<ISpacecraftHitWarningProperty>
{

	enum AttackWarningDirection
	{
		Forward,
		Backward,
		Left,
		Right,
		Up,
		Down,
		UpLeft,
		UpRight,
		DownLeft,
		DownRight,
	}

	private float ForwardRange = 30;
	private float BackRange = 30;
	private float UpRange = 30;
	private float DownRange = 30;
	private float LeftRange = 30;
	private float RightRange = 30;

	private Dictionary<AttackWarningDirection, string> m_WarningMap;

	private ISpacecraftHitWarningProperty m_Property;
	private GameplayProxy m_GameplayProxy;
	private CfgEternityProxy m_CfgEternityProxy;

	public override void OnInitialize(ISpacecraftHitWarningProperty property)
    {
        m_Property = property;
		m_GameplayProxy = GameFacade.Instance.RetrieveProxy(ProxyName.GameplayProxy) as GameplayProxy;
		m_CfgEternityProxy = GameFacade.Instance.RetrieveProxy(ProxyName.CfgEternityProxy) as CfgEternityProxy;

		GamingConfighitWarningCameraEffect effect = m_CfgEternityProxy.GetGamingConfig(1).Value.HitWarningCameraEffect.Value;
		m_WarningMap = new Dictionary<AttackWarningDirection, string>();
		m_WarningMap[AttackWarningDirection.Forward] = effect.Forward;
		m_WarningMap[AttackWarningDirection.Backward] = effect.Backward;
		m_WarningMap[AttackWarningDirection.Left] = effect.Left;
		m_WarningMap[AttackWarningDirection.Right] = effect.Right;
		m_WarningMap[AttackWarningDirection.Up] = effect.Up;
		m_WarningMap[AttackWarningDirection.Down] = effect.Down;
		m_WarningMap[AttackWarningDirection.UpLeft] = effect.UpLeft;
		m_WarningMap[AttackWarningDirection.UpRight] = effect.UpRight;
		m_WarningMap[AttackWarningDirection.DownLeft] = effect.DownLeft;
		m_WarningMap[AttackWarningDirection.DownRight] = effect.DownRight;
	}

    public override void OnAddListener()
	{
		AddListener(ComponentEventName.Event_s2c_skill_effect, OnSkillEffect);
	}

	private void OnSkillEffect(IComponentEvent componentEvent)
	{
		S2C_SKILL_EFFECT_Event respond = componentEvent as S2C_SKILL_EFFECT_Event;

		SpacecraftEntity caster = m_GameplayProxy.GetEntityById<SpacecraftEntity>(respond.msg.wCasterID) as SpacecraftEntity;
		SpacecraftEntity target = m_GameplayProxy.GetEntityById<SpacecraftEntity>(respond.msg.wTargetHeroID) as SpacecraftEntity;
		Camera mainCam = CameraManager.GetInstance().GetMainCamereComponent()?.GetCamera();
		if (caster != null && target != null && target.IsMain() && mainCam != null)
		{
			Vector3 targetToAttacker = caster.GetRootTransform().position - target.GetRootTransform().position;
			AttackWarningDirection warningDir = GetAttackDirection(targetToAttacker);

			Debug.LogFormat("HitWarning | SKillEffect. {0}", warningDir);

			EffectController hitWarningFX = EffectManager.GetInstance().CreateEffect(m_WarningMap[warningDir], EffectManager.GetEffectGroupNameInSpace(true));
			hitWarningFX.transform.SetParent(m_Property.GetSkinRootTransform(), false);
		}
	}

	private AttackWarningDirection GetAttackDirection(Vector3 targetToAttacker)
	{
		Camera mainCam = CameraManager.GetInstance().GetMainCamereComponent().GetCamera();

		Vector3 targetToAttackerInViewSpace = mainCam.transform.InverseTransformDirection(targetToAttacker);

		// FIXME: 所有Angle改成点积

		// 判断前后
		float angle = Vector3.Angle(targetToAttackerInViewSpace.normalized, Vector3.forward);
		if (angle < ForwardRange)
		{
			return AttackWarningDirection.Forward;
		}
		else if (angle > 180 - BackRange)
		{
			return AttackWarningDirection.Backward;
		}

		Vector3 dirProjToXY = targetToAttackerInViewSpace;
		dirProjToXY.z = 0;
		dirProjToXY.Normalize();

		// 判断上下
		angle = Vector3.Angle(dirProjToXY, Vector3.up);
		if (angle < UpRange)
		{
			return AttackWarningDirection.Up;
		}
		else if (angle > 180 - DownRange)
		{
			return AttackWarningDirection.Down;
		}

		// 判断左右
		angle = Vector3.Angle(dirProjToXY, -Vector3.right);
		if (angle < LeftRange)
		{
			return AttackWarningDirection.Left;
		}
		else if (angle > 180 - RightRange)
		{
			return AttackWarningDirection.Right;
		}

		// 左上, 右上, 左下, 右下
		bool upward = Vector3.Dot(Vector3.up, dirProjToXY) > 0;
		bool leftward = Vector3.Dot(-Vector3.right, dirProjToXY) > 0;
		if (upward)
		{
			if (leftward)
				return AttackWarningDirection.UpLeft;
			else
				return AttackWarningDirection.UpRight;
		}
		else
		{
			if (leftward)
				return AttackWarningDirection.DownLeft;
			else
				return AttackWarningDirection.DownRight;
		}
	}
}