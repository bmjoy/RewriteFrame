using Assets.Scripts.Define;
using Assets.Scripts.Proto;
using Game.Frame.Net;
using System.Collections.Generic;
using UnityEngine;

public interface IPicktargetProperty
{
	void SetTarget(SpacecraftEntity target, Collider targetCollider);
	SpacecraftEntity GetTarget();
	Collider GetTargetCollider();
	SpacecraftEntity GetOwner();
}

/// <summary>
/// 船形态运动相关逻辑
/// </summary>
public class PickTargetComponent : EntityComponent<IPicktargetProperty>
{
    private IPicktargetProperty m_PickTargetProperty;

    private Transform m_Transform;
	
    public override void OnInitialize(IPicktargetProperty property)
    {
        m_PickTargetProperty = property;
	}

    public override void OnAddListener()
    {
		//AddListener(ComponentEventName.ChangeSpacecraftInputState, OnInputStateChange);
    }

	public override void OnUpdate(float delta)
	{
		//OnScreenRaycast();
	}

	// TODO 技能, 瞄准距离
	private float VisibilityRange = 50000;
	private float autoLockDistance = 10000;
	private float autoLockDistanceMulti = 10000;

	private void OnScreenRaycast()
	{
		SpacecraftEntity target;
		Vector3 targetPoint;
		Collider targetCollider;

		if (RaycastTarget(VisibilityRange, out target, out targetPoint, out targetCollider))
		{
		}
		else
		{
			// TODO 技能, 自动瞄准
			// 			SpacecraftEntity currHero = null;
			// 			var currDistance = float.MaxValue;
			// 			var lockDistance = autoLockDistance * autoLockDistanceMulti;
			// 
			// 			var centerPoint = new Vector2(Screen.width / 2, Screen.height / 2);
			// 			foreach (var hero in playerEntityList)
			// 			{
			// 				if (hero == mainHero) continue;
			// 				var heroPoint = (Vector2)Camera.main.WorldToScreenPoint(hero.Motion<BaseMotion>().horizonAxis.position);
			// 				var heroDistance = Vector2.Distance(heroPoint, centerPoint);
			// 				if (heroDistance < lockDistance && heroDistance < currDistance)
			// 				{
			// 					currDistance = heroDistance;
			// 					currHero = hero;
			// 				}
			// 			}
			// 			foreach (var hero in npcEntityList)
			// 			{
			// 				if (hero == mainHero) continue;
			// 				if (hero.npcTemplateVO.MotionType == 0) continue;
			// 				if ((KHeroType)hero.subType == KHeroType.htMonsterChest) continue;
			// 				var heroPoint = (Vector2)Camera.main.WorldToScreenPoint(hero.Motion<BaseMotion>().horizonAxis.position);
			// 				var heroDistance = Vector2.Distance(heroPoint, centerPoint);
			// 				if (heroDistance < lockDistance && heroDistance < currDistance)
			// 				{
			// 					currDistance = heroDistance;
			// 					currHero = hero;
			// 				}
			// 			}
			// 
			// 			mainHero.ChangeTarget(currHero);
		}

		// TODO 技能, 自动瞄准
		//mainHero.Motion<BaseMotion>().CameraRotationSensitivityMulti = mainHero.GetTarget() != null ? mainHero.autoLockDrag : 1.0f;


		if (m_PickTargetProperty.GetTarget() != target || m_PickTargetProperty.GetTargetCollider() != targetCollider)
		{
			m_PickTargetProperty.SetTarget(target, targetCollider);
			ChangeTargetEvent eventInfo = new ChangeTargetEvent();
			eventInfo.newTarget = target;
			SendEvent(ComponentEventName.ChangeTarget, eventInfo);
		}
	}

	private bool RaycastTarget(float distance, out SpacecraftEntity target, out Vector3 targetPoint, out Collider collider)
	{
		if (Camera.main != null)
		{
			RaycastHit[] allHitInfo = Physics.RaycastAll(Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0)), distance);
			foreach (var hitInfo in allHitInfo)
			{
				if (hitInfo.collider.attachedRigidbody == null)
				{
					continue;
				}
				
				SpacecraftEntity entity = hitInfo.collider.attachedRigidbody.GetComponent<SpacecraftEntity>();

				if (entity == null)
				{
					continue;
				}
				if (entity == m_PickTargetProperty.GetOwner())
				{
					continue;
				}
				if (entity.GetHeroType() == KHeroType.htMonsterChest)
				{
					continue;
				}
				if (entity.GetHeroType() == KHeroType.htNpc)
				{
					continue;
				}

				target = entity;
				targetPoint = hitInfo.point;
				collider = hitInfo.collider;
				return true;
			}
		}

		target = null;
		targetPoint = Vector3.zero;
		collider = null;
		return false;
	}
}