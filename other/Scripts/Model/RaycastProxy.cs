using Assets.Scripts.Define;
using PureMVC.Patterns.Proxy;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaycastProxy : Proxy
{
	/// <summary>
	/// 当前目标
	/// </summary>
	private SpacecraftEntity m_CurrentTarget;

	/// <summary>
	/// 当前目标是否很远
	/// </summary>
	private bool m_CurrentTargetIsFar = false;

	/// <summary>
	/// 自动锁定距离乘数
	/// </summary>
	private float m_AutoLockDistanceMulti = 1.0f;

	/// <summary>
	/// 主角
	/// </summary>
	private SpacecraftEntity m_MainSpacecraftEntity;

	/// <summary>
	/// GameplayProxy
	/// </summary>
	private GameplayProxy m_GameplayProxy;

	/// <summary>
	/// PlayerSkillProxy
	/// </summary>
	private PlayerSkillProxy m_PlayerSkillProxy;

	/// <summary>
	/// 相机
	/// </summary>
	private Camera m_Camera;

	public RaycastProxy() : base(ProxyName.RaycastProxy)
	{

	}

	/// <summary>
	/// 获取主角
	/// </summary>
	/// <returns></returns>
	public SpacecraftEntity GetMainSpacecraftEntity()
	{
		if (m_MainSpacecraftEntity == null)
		{
			m_MainSpacecraftEntity = GetGameplayProxy().GetEntityById<SpacecraftEntity>(GetGameplayProxy().GetMainPlayerUID());
		}
		return m_MainSpacecraftEntity;
	}

	/// <summary>
	/// 获取GameplayProxy
	/// </summary>
	/// <returns></returns>
	public GameplayProxy GetGameplayProxy()
	{
		if (m_GameplayProxy == null)
		{
			m_GameplayProxy = Facade.RetrieveProxy(ProxyName.GameplayProxy) as GameplayProxy;
		}
		return m_GameplayProxy;
	}

	/// <summary>
	/// 获取PlayerSkillProxy
	/// </summary>
	/// <returns></returns>
	public PlayerSkillProxy GetPlayerSkillProxy()
	{
		if (m_PlayerSkillProxy == null)
		{
			m_PlayerSkillProxy = Facade.RetrieveProxy(ProxyName.PlayerSkillProxy) as PlayerSkillProxy;
		}
		return m_PlayerSkillProxy;
	}

	/// <summary>
	/// 获取相机
	/// </summary>
	/// <returns></returns>
	public Camera GetCamera()
	{
		if (m_Camera == null)
		{
			m_Camera = Camera.main;
		}
		return m_Camera;
	}

	public bool GetCurrentTargetIsFar()
	{
		return m_CurrentTargetIsFar;
	}
	#region 射线检测

	/// <summary>
	/// 目标检测  无过滤的
	/// </summary>
	public SpacecraftEntity Raycast()
	{
		if (GetMainSpacecraftEntity())
		{
			float rayLength = 0;
			if (!GetPlayerSkillProxy().UsingReformer())
			{
				if (GetPlayerSkillProxy().GetCurrentWeaponSkill() != null)
				{
					rayLength = GetPlayerSkillProxy().GetCurrentWeaponSkill().GetSkillGrow().Range / GameConstant.METRE_PER_UNIT;
				}
			}
			else
			{
				if (GetPlayerSkillProxy().GetReformerSkill() != null)
				{
					rayLength = GetPlayerSkillProxy().GetReformerSkill().GetSkillGrow().Range / GameConstant.METRE_PER_UNIT;
				}
			}

			Collider targetCollider;
			RaycastTarget(GetMainSpacecraftEntity(), rayLength, out SpacecraftEntity target, out targetCollider);
			return target;
		}

		return null;
	}

	/// <summary>
	/// 目标检测 过滤的
	/// </summary>
	public SpacecraftEntity RaycastTarget(float m_AutoLockDistance)
	{
		m_CurrentTargetIsFar = false;
		if (GetMainSpacecraftEntity())
		{
			SpacecraftEntity target;
			Collider targetCollider;
			bool targetIsFar = false;

			//float rayLength = GameConstant.DEFAULT_VISIBILITY_METRE_FOR_SHIP / GameConstant.METRE_PER_UNIT;
			float rayLength = 0;
			if (!GetPlayerSkillProxy().UsingReformer())
			{
				if (GetPlayerSkillProxy().GetCurrentWeaponSkill() != null)
				{
					rayLength = GetPlayerSkillProxy().GetCurrentWeaponSkill().GetSkillGrow().Range / GameConstant.METRE_PER_UNIT;
				}
			}
			else
			{
				if (GetPlayerSkillProxy().GetReformerSkill() != null)
				{
					rayLength = GetPlayerSkillProxy().GetReformerSkill().GetSkillGrow().Range / GameConstant.METRE_PER_UNIT;
				}
			}

			RaycastTarget(GetMainSpacecraftEntity(), rayLength, out target, out targetCollider);
			if (target == null)
			{
				SpacecraftEntity currHero = null;
				float currDistance = float.MaxValue;
				float lockDistance = m_AutoLockDistance * m_AutoLockDistanceMulti;
				Vector2 centerPoint = new Vector2(Screen.width / 2, Screen.height / 2);

				foreach (SpacecraftEntity entity in GetGameplayProxy().GetEntities<SpacecraftEntity>())
				{
					if (entity == GetMainSpacecraftEntity())
						continue;
					if (entity.GetHeroType() == KHeroType.htMonsterChest)
						continue;
					if (entity.GetHeroType() == KHeroType.htNpc)
						continue;
					if (entity.GetHeroType() == KHeroType.htPlayer)
						continue;
					if (entity.IsInvisible())
						continue;
					if (Vector3.Distance(entity.transform.position, GetMainSpacecraftEntity().transform.position) >= rayLength)
						continue;

					Vector2 heroPoint = (Vector2)Camera.main.WorldToScreenPoint(entity.transform.position);
					float heroDistance = Vector2.Distance(heroPoint, centerPoint);

					//TODO.死亡不选中
					if ((!entity.IsDead() || (entity.IsDead() && GetGameplayProxy().CanReviveToTarget(GetMainSpacecraftEntity(), entity))) && heroDistance < lockDistance && heroDistance < currDistance)
					{
						currDistance = heroDistance;
						currHero = entity;
					}
				}

				target = currHero;
				targetIsFar = true;
			}

			//mainHero.Motion<BaseMotion>().CameraRotationSensitivityMulti = mainHero.GetTarget() != null ? mainHero.autoLockDrag : 1.0f;

			if (target != m_CurrentTarget || targetCollider != GetMainSpacecraftEntity().GetTargetCollider())
			{
				m_CurrentTargetIsFar = targetIsFar;
				m_CurrentTarget = target;
				GetMainSpacecraftEntity().SetTarget(target, targetCollider);
			}
		}
		return m_CurrentTarget;
	}

	/// <summary>
	/// 射线检测
	/// </summary>
	/// <param name="main">主角</param>
	/// <param name="distance">射线长度</param>
	/// <param name="target">碰到的目标</param>
	/// <param name="collider">碰到的碰撞体</param>
	private void RaycastTarget(SpacecraftEntity main, float distance, out SpacecraftEntity target, out Collider collider)
	{
		if (GetCamera() != null)
		{
			Ray centerRay = GetCamera().ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
			// 与武器的目标选择逻辑统一
			centerRay.origin = GetCamera().transform.position;
			centerRay.direction = centerRay.direction.normalized;
			int layerToSelect = (1 << GameConstant.LayerTypeID.HumanNPC) | (1 << GameConstant.LayerTypeID.SpacecraftNPC)
				| (1 << GameConstant.LayerTypeID.SpacecraftOtherPlayer) | (1 << GameConstant.LayerTypeID.HumanOtherPlayer);
			var allHitInfo = Physics.RaycastAll(centerRay, distance, layerToSelect);
			foreach (var hitInfo in allHitInfo)
			{
				if (hitInfo.collider.attachedRigidbody == null) { continue; }

				SpacecraftEntity entity = hitInfo.collider.attachedRigidbody.GetComponent<SpacecraftEntity>();
				if (entity == null) { continue; }
				if (entity == main) { continue; }
				if (entity.GetHeroType() == KHeroType.htMonsterChest) { continue; }
				if (entity.GetHeroType() == KHeroType.htNpc) { continue; }
				if (entity.GetHeroType() == KHeroType.htPlayer) { continue; }

				target = entity;
				collider = hitInfo.collider;

				return;
			}
		}

		target = null;
		collider = null;
	}

	#endregion

	/// <summary>
	/// 设置m_CurrentTarget
	/// </summary>
	/// <param name="entity"></param>
	public void SetCurrentTarget(SpacecraftEntity entity)
	{
		m_CurrentTarget = entity;
	}
}
