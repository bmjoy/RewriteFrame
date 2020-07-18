
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IBattleWeaponProperty
{
	Transform GetRootTransform();
	Rigidbody GetRigidbody();
	bool IsMain();
	uint GetUId();
	uint GetTemplateID();
	SpacecraftEntity GetOwner();
	SpacecraftEntity GetTarget();
	Transform GetSkinRootTransform();
	SpacecraftPresentation GetPresentation();
	void ClearBattleWeapon();
	bool HaveBattleWeapon(ulong uid);
	void AddBattleWeapon(ulong uid, BattleWeaponBase battleWeapon);
	BattleWeaponBase GetBattleWeapon(ulong uid);
	Dictionary<ulong, BattleWeaponBase>.ValueCollection GetAllBattleWeapons();
	void ResetBattleWeapons(Dictionary<ulong, BattleWeaponBase> weapons);
	void SetCanToggleWeapon(bool can);
	bool GetCanToggleWeapon();
}

/// <summary>
/// 飞船实装武器的组件
/// </summary>
public class BattleWeaponComponent : EntityComponent<IBattleWeaponProperty>
{
	IBattleWeaponProperty m_Property;
	/// <summary>
	/// 飞船上能装多少个武器
	/// </summary>
	private const int BATTLE_WEAPON_COUNT = 2;

	// Cache
	private CfgSkillSystemProxy m_CfgSkillProxy;
	private PlayerSkillProxy m_SkillProxy;
	private CfgLanguageProxy m_CfgLanguageProxy;
	private GameplayProxy m_GameplayProxy;


	#region 初始化
	public override void OnInitialize(IBattleWeaponProperty property)
	{
		m_Property = property;
        
		m_CfgSkillProxy = GameFacade.Instance.RetrieveProxy(ProxyName.CfgSkillSystemProxy) as CfgSkillSystemProxy;
		m_SkillProxy = GameFacade.Instance.RetrieveProxy(ProxyName.PlayerSkillProxy) as PlayerSkillProxy;
		m_CfgLanguageProxy = GameFacade.Instance.RetrieveProxy(ProxyName.CfgLanguageProxy) as CfgLanguageProxy;
		m_GameplayProxy = GameFacade.Instance.RetrieveProxy(ProxyName.GameplayProxy) as GameplayProxy;
	}

	/// <summary>
	/// 注册消息回调
	/// </summary>
	public override void OnAddListener()
	{
		if (m_Property.IsMain())
		{
			AddListener(ComponentEventName.WeaponSkillFinished, WeaponSkillFinished);
			
			//AddListener(ComponentEventName.Relive, ClearBattleWeaponRuntimeData);
			AddListener(ComponentEventName.ChangHeroState, ClearBattleWeaponRuntimeData);
			//AddListener(ComponentEventName.ChangeBattleState, ClearBattleWeaponRuntimeData);
			AddListener(ComponentEventName.UnsuccessfulReleaseOfSkill, OnUnsuccessfulReleaseOfSkill);

#if NewSkill
#else
            AddListener(ComponentEventName.PostWeaponFire, PostWeaponFire);
            AddListener(ComponentEventName.WeaponPowerChanged, UpdateBattleWeapons);
#endif
            //AddListener(ComponentEventName.ItemSyncEnd, UpdateBattleWeapons);
            AddListener(ComponentEventName.WeaponOperation, OnWeaponOperation);
			AddListener(ComponentEventName.PlayerWeaponToggleEnd, OnWeaponToggled);
			AddListener(ComponentEventName.ChangeTarget, OnTargetChanged);
		}
	}

	public override void OnUpdate(float delta)
	{
		foreach (var weapon in m_Property.GetAllBattleWeapons())
		{
			if (weapon == m_SkillProxy.GetCurrentBattleWeapon())
				weapon.OnUpdate(delta);
		}
	}
	#endregion

	/// <summary>
	/// 设置当前是否能切换武器
	/// </summary>
	/// <param name="can"></param>
	public void SetCanToggleWeaponNow(bool can)
	{
		m_Property.SetCanToggleWeapon(can);
	}


	/// <summary>
	/// 武器技能发射, 对武器造成影响. 计算新的弹道散布和后坐力
	/// </summary>
	private void PostWeaponFire(IComponentEvent entityEvent)
	{
		// 临时代码. 只有准星类型是1的武器, 才扩散弹道
		IWeapon weapon = m_SkillProxy.GetCurrentWeapon();
		BattleWeaponBase currentBattleWeapon = m_SkillProxy.GetCurrentBattleWeapon();
		currentBattleWeapon?.PostWeaponFire();
	}

	/// <summary>
	/// 武器技能结束
	/// </summary>
	private void WeaponSkillFinished(IComponentEvent entityEvent)
	{
		WeaponSkillFinishedEvent finishEvent = entityEvent as WeaponSkillFinishedEvent;

		IWeapon weapon = m_SkillProxy.GetCurrentWeapon();
		BattleWeaponBase currentBattleWeapon = m_SkillProxy.GetCurrentBattleWeapon();
		currentBattleWeapon?.OnWeaponSkillFinished(finishEvent.Success);
	}

	private void ClearBattleWeaponRuntimeData(IComponentEvent entityEvent)
	{
		ResetRuntimeDataOfAllWeapons();
	}

	/// <summary>
	/// 技能释放不成功, 客户端检查没过. 如果是武器技能, 清空武器状态
	/// </summary>
	private void OnUnsuccessfulReleaseOfSkill(IComponentEvent entityEvent)
	{
		UnsuccessfulReleaseOfSkillEvent skillEvent = entityEvent as UnsuccessfulReleaseOfSkillEvent;
		if (m_CfgSkillProxy.IsWeaponSkill(skillEvent.SkillID))
		{
			ResetRuntimeDataOfAllWeapons();
		}
	}

	/// <summary>
	/// 在武器变化的时候, 更新当前实装的武器的准星信息
	/// </summary>
	private void UpdateBattleWeapons(IComponentEvent entityEvent)
	{
		ItemOperateEvent itemEvent = entityEvent as ItemOperateEvent;

		Dictionary<ulong, BattleWeaponBase> newBattleWeapons = new Dictionary<ulong, BattleWeaponBase>();
		for (int iWeapon = 0; iWeapon < BATTLE_WEAPON_COUNT; iWeapon++)
		{
			IWeapon weapon = m_SkillProxy.GetWeaponByIndex(iWeapon);
			if (weapon == null)
				continue;

			// 获取iWeapon对应武器的UID
			ulong uid = weapon.GetUID();
			// 查看当前是否有这把武器的准星信息
			if (m_Property.HaveBattleWeapon(uid))
			{
				newBattleWeapons.Add(uid, m_Property.GetBattleWeapon(uid));
			}
			else
			{
				BattleWeaponBase.BattleWeaponType weaponType = BattleWeaponBase.GetBattleWeaponTypeByReticle(weapon.GetConfig().Reticle);
				newBattleWeapons.Add(uid, CreateNewBattleWeapon(weaponType, weapon));
			}
		}

		m_Property.ResetBattleWeapons(newBattleWeapons);
	}

	private void OnWeaponOperation(IComponentEvent entityEvent)
	{
		SkillHotkey skillHotkey = entityEvent as SkillHotkey;

		IWeapon weapon = m_SkillProxy.GetCurrentWeapon();
		if (weapon == null)
			return;

		// 获取iWeapon对应武器的UID
		ulong uid = weapon.GetUID();
		BattleWeaponBase battleWeapon = m_Property.GetBattleWeapon(uid);
        if (battleWeapon != null)
        {
            battleWeapon.WeaponOperation(skillHotkey);
        }
    }

	private void OnWeaponToggled(IComponentEvent entityEvent)
	{
		IWeapon weapon = m_SkillProxy.GetCurrentWeapon();
		BattleWeaponBase battleWeapon = m_Property.GetBattleWeapon(weapon.GetUID());
		if (battleWeapon != null)
		{
			battleWeapon.OnToggledToMainWeapon();
		}
	}

	private void OnTargetChanged(IComponentEvent entityEvent)
	{
		m_SkillProxy.GetCurrentBattleWeapon()?.OnTargetChanged(entityEvent as ChangeTargetEvent);
	}

	/// <summary>
	/// 重置所有实装武器的运行时属性
	/// </summary>
	private void ResetRuntimeDataOfAllWeapons()
	{
		foreach (var weapon in m_Property.GetAllBattleWeapons())
		{
			weapon.ResetWeaponRuntimeData();
		}
	}

	/// <summary>
	/// 创建一个新的运行时武器信息
	/// </summary>
	/// <param name="weaponType"></param>
	private BattleWeaponBase CreateNewBattleWeapon(BattleWeaponBase.BattleWeaponType weaponType, IWeapon weapon)
	{
		switch (weaponType)
		{
			case BattleWeaponBase.BattleWeaponType.MachineGun:
				return new BattleWeapon_MachineGun(weapon);
			case BattleWeaponBase.BattleWeaponType.Missile:
				Debug.Log("创建导弹类型的战斗武器");
				return new BattleWeapon_Missile_CenterLock(weapon);
			case BattleWeaponBase.BattleWeaponType.Shotgun:
				return new BattleWeapon_Shotgun(weapon);
			default:
				return new BattleWeaponBase(weapon);
		}
	}

	public override void OnDrawGizmo()
	{
		BattleWeaponBase battleWeapon = m_SkillProxy.GetCurrentBattleWeapon();
		BattleWeapon_Missile missile = battleWeapon as BattleWeapon_Missile;
		if (missile == null)
			return;

		MainCameraComponent mainCam = CameraManager.GetInstance().GetMainCamereComponent();
		Gizmos.color = Color.red;
		Gizmos.matrix = mainCam.transform.localToWorldMatrix;
		Gizmos.DrawFrustum(Vector3.zero, missile.m_Gizmo_FOV, 10, 0.5f, 1);
		Gizmos.matrix = Matrix4x4.identity;
	}
}
