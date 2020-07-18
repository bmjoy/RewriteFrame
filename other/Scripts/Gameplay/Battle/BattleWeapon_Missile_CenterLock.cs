using Eternity.FlatBuffer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static BattleWeapon_Missile;
using static UnityEngine.Debug;

/// <summary>
/// 导弹武器
/// 使用中心点射线瞄准目标. 在目标不出框的情况下继续锁定, 一旦出框就中断锁定
/// </summary>
public class BattleWeapon_Missile_CenterLock : BattleWeaponBase
{
	private enum State
	{
		/// <summary>
		/// 空闲状态, 即不在锁定, 也不在发射
		/// </summary>
		Idle,
		/// <summary>
		/// 锁定状态, 会把所有准星划过的敌对目标当做导弹攻击目标
		/// </summary>
		Lock,
		/// <summary>
		/// 攻击状态, 会对所有已经锁定的目标释放导弹进行攻击
		/// </summary>
		Attack,
	}	

	/// <summary>
	/// 所有 已经锁定/正在锁定 的目标的状态
	/// </summary>
	private Dictionary<SpacecraftEntity, TargetLockInfo> m_LockTargeMap;

	/// <summary>
	/// 武器状态
	/// </summary>
	private State m_State;

	/// <summary>
	/// 导弹武器专有属性
	/// </summary>
	private WeaponMissile m_WeaponFireData;

	// 所有武器的参数

	/// <summary>
	/// 准星区域是一个长方形, 从摄像机投射出一个截面为长方形的锥体
	/// 这个它的横向FOV
	/// </summary>
	private float m_ReticleHorizontalFOVParam;
	/// <summary>
	/// 单次攻击导弹数量
	/// </summary>
	private int m_MaxMissileCountInOneShotParam;
	/// <summary>
	/// 锁定一个目标需要的时间. 秒
	/// </summary>
	private float m_LockonTimeParam;
	/// <summary>
	/// 准星瞄准框的纵横比
	/// </summary>
	private float m_ReticleAspect;

	/// <summary>
	/// 用于计算那些物体被准星框住的辅助摄像机
	/// </summary>
	private Camera m_VirtualCameraForReticleSelection;
	

	public BattleWeapon_Missile_CenterLock(IWeapon weapon) : base(weapon)
	{
		m_LockTargeMap = new Dictionary<SpacecraftEntity, TargetLockInfo>();
		m_State = State.Idle;

		m_WeaponFireData = m_CfgEternityProxy.GetWeaponDataOfMissile(m_WeaponConfig.TypeDateSheetld);

		TranslateUniformParamToWeaponParam();

		// FIXME. 动态内存分配
		m_VirtualCameraForReticleSelection = new GameObject().AddComponent<Camera>();
		m_VirtualCameraForReticleSelection.enabled = false;
		m_VirtualCameraForReticleSelection.fieldOfView = m_ReticleHorizontalFOVParam / m_ReticleAspect;
		m_VirtualCameraForReticleSelection.aspect = m_ReticleAspect;
		GameObject.DontDestroyOnLoad(m_VirtualCameraForReticleSelection.gameObject);
	}

	~BattleWeapon_Missile_CenterLock()
	{
	}

    public override void OnRelease()
    {
        base.OnRelease();

        if(m_VirtualCameraForReticleSelection != null)
            Object.Destroy(m_VirtualCameraForReticleSelection.gameObject);
        m_VirtualCameraForReticleSelection = null;
    }

    public override bool CanWeaponFire()
	{
		return m_PlayerSkillProxy.CanWeaponRelease(m_WeaponItem.GetUID());
	}

	public override float CalculateFireInterval()
	{
		// 当武器射速属性 >= 0，使用1号武器射击间隔公式
		// 当武器射速属性 < 0，使用2号武器射击间隔公式

		// 1号武器射击间隔公式
		// （ 60 / A） /（ 1 + B / 100）*1000

		// 2号武器射击间隔公式
		// ABS（（ 60 / A） *（ 1 + B / 100 - 2）*1000）

		// A：1分钟子弹数
		// B：武器射速

		// 备注：		
		// 	武器射击间隔公式最终数值为整数四舍五入
		// 	最终单位数值为毫秒

		float weaponFireSpeed = m_MainPlayer.GetWeaponAttribute(m_WeaponItem.GetUID(), crucis.attributepipeline.attribenum.AttributeName.kLightHeatCastSpeed);
		if (weaponFireSpeed >= 0)
		{
			m_FireInterval = 60f / m_WeaponFireData.MinuteBulletNumber / (1 + weaponFireSpeed / 100f);
		}
		else
		{
			m_FireInterval = Mathf.Abs(60f / m_WeaponFireData.MinuteBulletNumber * (1 + weaponFireSpeed / 100f - 2f));
		}

		return m_FireInterval;
	}

	public override void WeaponOperationImplementation(SkillHotkey skillHotkey)
	{
		// if current state is idle.
		//		hotkey pressed, start to lock
		// if current state is lock.
		//		hotkey released and there are more than one target we have locked, start to attack. else set state to idle

		bool cast = skillHotkey.ActionPhase == HotkeyPhase.Started;

		switch (m_State)
		{
			case State.Idle:
				if (cast && CanWeaponFire())
				{
					SLog("进入锁定状态");
					// 导弹技能冷却以后才能发射
					m_State = State.Lock;
					m_LockTargeMap.Clear();
					m_MainPlayer.SetCanToggleWeapon(false);
					NetworkManager.Instance.GetSkillController().SendMissileLockTarget(true);
				}
				break;
			case State.Lock:
				if (!cast)
				{
					SLog("松开武器按键");
					if (ExistingLockedTarget() && m_PlayerSkillProxy.CanCurrentWeaponRelease())
					{
						SLog("有锁定目标, 进入攻击状态, 释放技能");
						m_State = State.Attack;
						CastSkill();
					}
					else if (ExistingLockedTarget() && !m_PlayerSkillProxy.CanCurrentWeaponRelease())
					{
						GameFacade.Instance.SendNotification(NotificationName.PlayerWeaponFireFail);
					}
					else
					{
						SLog("没有锁定目标, 结束这次攻击流程");
						ResetWeaponRuntimeData();
					}

					NetworkManager.Instance.GetSkillController().SendMissileLockTarget(false);
				}
				break;
			case State.Attack:
				// 导弹正在依次发射的过程中, 按下鼠标左键, 不可以进入锁定状态. By 刘斌 (暂定)
				break;
			default:
				break;
		}
	}

	public override SkillTarget CalculateNextTarget()
	{
		SLog("选择技能目标");

		// if there is more than one locked target, return it
		// else DebugError
		if (ExistingLockedTarget())
		{
			// 测试代码. 直接取第一个目标
			// 第二版: 改为每个目标发射一次导弹
			// 第三版: 做一个多目标多轮技能
			foreach (var pair in m_LockTargeMap)
			{
				if (pair.Value.LockTimes == 0)
					continue;

				SLog("选择第一个目标释放. {0}", pair.Key.name);
				
				return new SkillTarget(pair.Key, pair.Key.GetSkinRootTransform().position);
			}

			return null;
		}
		else
		{
			return null;
		}
	}

	public override void CalculateNextTargets(ref List<SkillTarget> targetList)
	{
		targetList.Clear();
		string targetListStr = "";
		foreach (var pair in m_LockTargeMap)
		{
			SpacecraftEntity target = pair.Key;
			TargetLockInfo lockInfo = pair.Value;
			for (int iLockTime = 0; iLockTime < lockInfo.LockTimes; iLockTime++)
			{
				targetList.Add(new SkillTarget(target, target.GetSkinRootTransform().position));
			}
			targetListStr += target.name + ", ";
		}
		SLog("选择目标释放. {0}", targetListStr);
	}

	public override void PostWeaponFire()
	{

	}

	public override void OnWeaponSkillFinished(bool success)
	{
		SLog("技能释放完成", m_State);
		// 如果技能发射的过程中, 开始了下一次锁定. 技能释放完成时不能清空当前锁定列表
		if (m_State != State.Lock)
		{
			ResetWeaponRuntimeData();
		}
	}

	/// <summary>
	/// 检测是否已经选中了新的目标
	/// 更新已经锁定了的目标的锁定状态
	/// </summary>
	/// <param name="deltaTime"></param>
	public override void OnUpdate(float deltaTime)
	{
		base.OnUpdate(deltaTime);

		//  调试代码
		if (Input.GetKeyDown(KeyCode.Keypad0))
		{
			ResetWeaponRuntimeData();
			Debug.LogError("清理武器导弹状态");
		}
		TranslateUniformParamToWeaponParam();
		//  调试代码

		if(m_State == State.Lock)
		{
			SpacecraftEntity newTarget = GetTargetSelectedByCenterRay();

			//NotifyHUDTargetSelected(newTarget);

			// 尝试锁定新的目标
			if (IsThereSurplusMissile())
			{
				TryToLockThisTarget(newTarget);
			}

			// 清理已经出框的目标
			ClearLockingTargetWhichIsNotInLockingRect();

			// 更新已有的目标
			// iterate all locking and locked target 
			// locking:
			//		once countdown finished, set target state to Locked
			// locking and locked:
			//		if target is out of range / dead, unlock target
			List<SpacecraftEntity> cancelLockList = new List<SpacecraftEntity>();
			foreach (var pair in m_LockTargeMap)
			{
				SpacecraftEntity targetEntity = pair.Key;

                // UNDONE, 检查所有目标是不是已经超出了射程或者已经死了. 如果是的话, 就接触锁定
                if (targetEntity == null || targetEntity.IsDead())
                {
                    cancelLockList.Add(targetEntity);
                    continue;
                }

                TargetLockInfo targetLockInfo = pair.Value;
                if (targetLockInfo.LockState == TargetLockState.Locking&& IsThereSurplusMissile())
				{
                    if(IsThereSurplusMissile()) //还不够
                    {
                        if (targetLockInfo.LockTimeRemaining <= 0f)
                        {
                            targetLockInfo.LockState = TargetLockState.Locked;
                            targetLockInfo.LockTimeRemaining = 0f;
                            targetLockInfo.LockTimes++;

                            SLog("目标锁定完成. {0}", targetEntity.name);
                        }
                        else
                        {
                            targetLockInfo.LockTimeRemaining -= deltaTime;
                        }
                    }
                    else //够了,将未完成转圈的
                    {
                        if (targetLockInfo.LockTimes == 0)
                        {
                            cancelLockList.Add(targetEntity);
                        }
                        else
                        {
                            targetLockInfo.LockState = BattleWeapon_Missile.TargetLockState.Locked;
                            targetLockInfo.LockTimeRemaining = 0;
                        }
                    }
				}
			}

			// 对于那些已经不可用的目标解除锁定
			for (int iEntity = 0; iEntity < cancelLockList.Count; iEntity++)
			{
				m_LockTargeMap.Remove(cancelLockList[iEntity]);
				SLog("目标因为死亡或者超出视野而失去锁定. {0}", iEntity);
			}
		}

		// 通知UI当前锁定的所有目标
		PlayerMissileWeaponTargetSelectionInfo targetInfoNotify = MessageSingleton.Get<PlayerMissileWeaponTargetSelectionInfo>();
		//targetInfoNotify.TargeList = m_LockTargeMap;
		GameFacade.Instance.SendNotification(NotificationName.PlayerMissileWeaponTargetSelection, targetInfoNotify);
	}

	public override void ResetWeaponRuntimeData()
	{
		// reset State to idle, clear all locking targets
		m_State = State.Idle;
		m_LockTargeMap.Clear();

		m_MainPlayer.SetCanToggleWeapon(true);

		SLog("重置导弹武器状态");
	}

	public override void OnToggledToMainWeapon()
	{
		NotifyHUDCrosshair();
	}

	public override MsgPlayerWeaponCrosshair GetRelativeHeightOfReticle()
	{
		MsgPlayerWeaponCrosshair crosshair = new MsgPlayerWeaponCrosshair();
		Camera cam = CameraManager.GetInstance()?.GetMainCamereComponent()?.GetCamera();
		if (cam != null)
		{
			crosshair.HorizontalRelativeHeight = m_ReticleHorizontalFOVParam / (cam.fieldOfView * cam.aspect);
			crosshair.VerticalRelativeHeight = m_ReticleHorizontalFOVParam / m_ReticleAspect / cam.fieldOfView;
			crosshair.MissileCountInOneShot = m_MaxMissileCountInOneShotParam;
			return crosshair;
		}
		else
		{
			return base.GetRelativeHeightOfReticle();
		}
	}

	public override void OnTargetChanged(ChangeTargetEvent eventInfo)
	{
		// 导弹武器使用特殊的目标选择方式
	}

	/// <summary>
	/// 获取锁定时间
	/// </summary>
	/// <returns></returns>
	public float GetLockonTime()
	{
		return m_LockonTimeParam;
	}

	/// <summary>
	/// 上一帧锁定的目标
	/// </summary>
	private SpacecraftEntity m_TargetLockedInLastFrame;

	/// <summary>
	/// 选中一个新的目标
	/// 1. 与上一帧选中的目标不同
	/// 2. 上一帧选中的目标已经锁定完成, 进行下一轮锁定
	/// </summary>
	/// <returns>是否有新的目标</returns>
	private bool PickNewTarget(out SpacecraftEntity newTarget)
	{
		PlayerSkillVO skillVO = m_PlayerSkillProxy.GetCurrentWeaponSkill();
		SkillSystemGrow skillGrow = skillVO.GetSkillGrow();
		float maxDistance = skillGrow.Range * SceneController.SPACE_ACCURACY_TO_CLIENT;
		MainCameraComponent mainCam = CameraManager.GetInstance().GetMainCamereComponent();
		Ray ray = new Ray(mainCam.GetPosition(), mainCam.GetForward());
		RaycastHit hitInfo = new RaycastHit();

		// UNDONE
		// 第一版: 使用屏幕中心射线作为目标选取射线. NOW
		// 第二版: 使用 Physics.BoxCast 
		if (Physics.Raycast(ray, out hitInfo, maxDistance, LayerUtil.GetLayersIntersectWithSkillProjectile(true)))
		{
			SpacecraftEntity pickedTarget = hitInfo.collider.attachedRigidbody?.GetComponent<SpacecraftEntity>();
			newTarget = pickedTarget;
			return true;
		}
		else
		{
			newTarget = null;
			return false;
		}
	}

	/// <summary>
	/// 中心点射线选中的目标
	/// </summary>
	/// <returns></returns>
	private SpacecraftEntity GetTargetSelectedByCenterRay()
	{
		PlayerSkillVO skillVO = m_PlayerSkillProxy.GetCurrentWeaponSkill();
		SkillSystemGrow skillGrow = skillVO.GetSkillGrow();
		float maxDistance = skillGrow.Range * SceneController.SPACE_ACCURACY_TO_CLIENT;
		MainCameraComponent mainCam = CameraManager.GetInstance().GetMainCamereComponent();
		Ray ray = new Ray(mainCam.GetPosition(), mainCam.GetForward());
		RaycastHit hitInfo = new RaycastHit();
		if (Physics.Raycast(ray, out hitInfo, maxDistance, LayerUtil.GetLayersIntersectWithSkillProjectile(true)))
		{
			return hitInfo.collider.attachedRigidbody?.GetComponent<SpacecraftEntity>();
		}
		else
		{
			return null;
		}
	}

	/// <summary>
	/// 清理正在锁定但是已经不在瞄准框中的目标
	/// </summary>
	private void ClearLockingTargetWhichIsNotInLockingRect()
	{
		List<SpacecraftEntity> targetList = new List<SpacecraftEntity>();
		bool targetSelected = GetAllEntitySelectedByVirtualCamera(ref targetList);

		List<SpacecraftEntity> cancelLockList = new List<SpacecraftEntity>();
		foreach (var pair in m_LockTargeMap)
		{
			SpacecraftEntity lockTarget = pair.Key;
			TargetLockInfo lockInfo = pair.Value;
			// 正在锁定的目标如果已经不在框中, 就取消锁定
			if (lockInfo.LockState == TargetLockState.Locking && !targetList.Contains(lockTarget))
			{
				cancelLockList.Add(lockTarget);
			}
		}

		for (int iEntity = 0; iEntity < cancelLockList.Count; iEntity++)
		{
			SpacecraftEntity entity = cancelLockList[iEntity];
			if (m_LockTargeMap[entity].LockTimes == 0)
			{
				m_LockTargeMap.Remove(entity);
			}
			else
			{
				m_LockTargeMap[entity].LockState = TargetLockState.Locked;
				m_LockTargeMap[entity].LockTimeRemaining = 0;
			}
			SLog("目标在锁定完成前出框了, 失去锁定. {0}", iEntity);
		}
	}

	private bool GetAllEntitySelectedByVirtualCamera(ref List<SpacecraftEntity> targetList)
	{
		MainCameraComponent mainCam = CameraManager.GetInstance().GetMainCamereComponent();

		SkillSystemGrow skillGrow = m_PlayerSkillProxy.GetCurrentWeaponSkill().GetSkillGrow();
		float range = skillGrow.Range * SceneController.SPACE_ACCURACY_TO_CLIENT;

		// 粗略的拾取. 
		// 拾取锥体视为一个相机, 把技能射程视为远裁剪面. 根据FOV取远裁剪面的大小, 投射这个大小的Box, 获取所有拾取到的Entity
		{
			// 所有粗略裁剪的物体 = Physics.BoxCastAll(投射开始点, 盒子边长, 相机方向, SkillLayer)

			float halfBoxVerticalSideLength = range * Mathf.Tan(m_ReticleHorizontalFOVParam / m_ReticleAspect * 0.5f * Mathf.Deg2Rad);
			float halfBoxHorizontalSideLength = halfBoxVerticalSideLength * m_ReticleAspect;

			// 投射开始的点 = 相机近裁剪面的中心点 + 相机方向 * 盒子边长的一半
			Vector3 startPoint = mainCam.transform.position + (mainCam.GetCamera().nearClipPlane + halfBoxVerticalSideLength) * mainCam.GetForward();
			RaycastHit[] hitInfos = Physics.BoxCastAll(startPoint,
											new Vector3(halfBoxHorizontalSideLength, halfBoxVerticalSideLength, halfBoxVerticalSideLength),
											mainCam.GetForward(),
											Quaternion.identity, range,
											LayerUtil.GetLayersIntersectWithSkillProjectile(true));
			
			for (int iHit = 0; iHit < hitInfos.Length; iHit++)
			{
				Rigidbody rigidBody = hitInfos[iHit].rigidbody;
				if (rigidBody == null)
					continue;

				SpacecraftEntity entity = rigidBody.GetComponent<SpacecraftEntity>();
				if (entity != null)
				{
					targetList.Add(entity);
				}
			}
		}

        if (m_VirtualCameraForReticleSelection != null)
        {
            // 更新虚拟相机的方向和位置
            m_VirtualCameraForReticleSelection.transform.position = mainCam.transform.position;
            m_VirtualCameraForReticleSelection.transform.rotation = mainCam.transform.rotation;

            // 更精细的判断. 当前版本仅计算目标中心点是不是在 虚拟相机 里面
            for (int iTarget = 0; iTarget < targetList.Count; iTarget++)
            {
                Vector3 viewportPos = m_VirtualCameraForReticleSelection.WorldToViewportPoint(targetList[iTarget].transform.position);
                if (viewportPos.x < 0 || viewportPos.y < 0 || viewportPos.x > 1 || viewportPos.y > 1)
                {
                    targetList.RemoveAt(iTarget);
                    iTarget--;
                }
            }
        }
		return targetList.Count > 0;
	}

	public void TryToLockThisTarget(SpacecraftEntity newTarget)
	{
		if (newTarget == null || !m_GameplayProxy.CanAttackToTarget(m_MainPlayer, newTarget))
		{
			return;
		}
		else
		{
			SLog("选中此目标进行锁定. {0}", newTarget.name);
		}

		// 新的目标有两种可能. 1. 不在当前目标列表里面 且 不为空, 2. 在目标列表里, 但是此目标已经锁定完成, 可以进行下一轮锁定
		if (!m_LockTargeMap.ContainsKey(newTarget))
		{
			TargetLockInfo targetLockInfo = new TargetLockInfo();
			targetLockInfo.LockState = TargetLockState.Locking;
			targetLockInfo.LockTimeRemaining = m_LockonTimeParam;
			targetLockInfo.LockTimes = 0;
			m_LockTargeMap.Add(newTarget, targetLockInfo);

			SLog("锁定新目标. {0}", newTarget.name);
		}
		else if (IsLockedEntity(newTarget))
		{
			m_LockTargeMap[newTarget].LockTimeRemaining = m_LockonTimeParam;
			m_LockTargeMap[newTarget].LockState = TargetLockState.Locking;

			SLog("锁定老目标, 为其增加一层锁定层数. {0}", newTarget.name);
		}
		else
		{
			//SLog("目标正在锁定中, 却作为新目标被选中了. {0}", newTarget.name);
		}
	}

	/// <summary>
	/// 是否正在锁定这个Entity
	/// </summary>
	/// <param name="entity"></param>
	/// <returns></returns>
	private bool IsLockingEntity(SpacecraftEntity entity)
	{
		if (m_LockTargeMap.ContainsKey(entity))
		{
			return m_LockTargeMap[entity].LockState == TargetLockState.Locking;
		}
		else
		{
			return false;
		}
	}

	/// <summary>
	/// 是否已经锁定了这个目标
	/// </summary>
	/// <param name="entity"></param>
	/// <returns></returns>
	private bool IsLockedEntity(SpacecraftEntity entity)
	{
		if (m_LockTargeMap.ContainsKey(entity))
		{
			return m_LockTargeMap[entity].LockState == TargetLockState.Locked;
		}
		else
		{
			return false;
		}
	}
	
	/// <summary>
	/// 是否存在已经锁定的目标
	/// </summary>
	/// <returns></returns>
	private bool ExistingLockedTarget()
	{
		// 剔除所有正在锁定的目标. 只留下已经锁定的
		int count = 0;
		foreach (var pair in m_LockTargeMap)
		{
			TargetLockInfo lockInfo = pair.Value;
			if (lockInfo.LockState == TargetLockState.Locked || lockInfo.LockTimes >= 1)
				count++;
		}
		
		return count > 0;
	}

	/// <summary>
	/// 释放技能攻击所有已经锁定的目标
	/// </summary>
	private void CastSkill()
	{
		SLog("释放技能. ");
		foreach (var pair in m_LockTargeMap)
		{
			TargetLockInfo targetLockInfo = pair.Value;
			targetLockInfo.LockTimeRemaining = m_LockonTimeParam;
		}

		SkillCastEvent skillCastEvent = new SkillCastEvent();
		skillCastEvent.IsWeaponSkill = true;
		skillCastEvent.KeyPressed = true;

//#if NewSkill
//#else
        m_MainPlayer.SendEvent(ComponentEventName.CastSkill, skillCastEvent);

		// 相当于一次按键的按下和抬起. 在技能系统中技能按键的操作要成对出现
		skillCastEvent.KeyPressed = false;
		m_MainPlayer.SendEvent(ComponentEventName.CastSkill, skillCastEvent);
//#endif

    }

    /// <summary>
    /// 本此攻击流程中是否还有富余的弹药以供锁定更多的目标
    /// </summary>
    /// <returns></returns>
    private bool IsThereSurplusMissile()
	{
		int lockCount = 0;

		foreach (var pair in m_LockTargeMap)
		{
			TargetLockInfo lockInfo = pair.Value;
			lockCount += lockInfo.LockTimes;
		}

		return m_MaxMissileCountInOneShotParam > lockCount;
	}

	/// <summary>
	/// 把统一的参数翻译成武器的弹道扩散参数
	/// </summary>
	private void TranslateUniformParamToWeaponParam()
	{
		float accuracy = m_MainPlayer.GetWeaponAttribute(m_WeaponItem.GetUID(), crucis.attributepipeline.attribenum.AttributeName.kWeaponAccuracy);
		float stability = m_MainPlayer.GetWeaponAttribute(m_WeaponItem.GetUID(), crucis.attributepipeline.attribenum.AttributeName.kWeaponStability);

		// 表里配置的时间是毫秒
		m_LockonTimeParam = (m_WeaponFireData.LockTime - accuracy * m_WeaponFireData.TimeCoefficient) / 1000f;
		m_MaxMissileCountInOneShotParam = (int)(m_WeaponFireData.MagazineNumber + stability * m_WeaponFireData.MagazineQuantityCoefficient);
		m_ReticleHorizontalFOVParam = m_WeaponFireData.AimingSize;
		m_ReticleAspect = m_WeaponFireData.AngleRange;

		if (m_VirtualCameraForReticleSelection != null)
		{
			m_VirtualCameraForReticleSelection.fieldOfView = m_ReticleHorizontalFOVParam / m_ReticleAspect;
			m_VirtualCameraForReticleSelection.aspect = m_ReticleAspect;
		}
	}

	private void SLog(string log, params object[] param)
	{
		Debug.LogFormat("MissileTest| " + log, param);
	}
}