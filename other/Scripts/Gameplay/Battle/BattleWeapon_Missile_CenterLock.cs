using Eternity.FlatBuffer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static BattleWeapon_Missile;
using static UnityEngine.Debug;

/// <summary>
/// ��������
/// ʹ�����ĵ�������׼Ŀ��. ��Ŀ�겻���������¼�������, һ��������ж�����
/// </summary>
public class BattleWeapon_Missile_CenterLock : BattleWeaponBase
{
	private enum State
	{
		/// <summary>
		/// ����״̬, ����������, Ҳ���ڷ���
		/// </summary>
		Idle,
		/// <summary>
		/// ����״̬, �������׼�ǻ����ĵж�Ŀ�굱����������Ŀ��
		/// </summary>
		Lock,
		/// <summary>
		/// ����״̬, ��������Ѿ�������Ŀ���ͷŵ������й���
		/// </summary>
		Attack,
	}	

	/// <summary>
	/// ���� �Ѿ�����/�������� ��Ŀ���״̬
	/// </summary>
	private Dictionary<SpacecraftEntity, TargetLockInfo> m_LockTargeMap;

	/// <summary>
	/// ����״̬
	/// </summary>
	private State m_State;

	/// <summary>
	/// ��������ר������
	/// </summary>
	private WeaponMissile m_WeaponFireData;

	// ���������Ĳ���

	/// <summary>
	/// ׼��������һ��������, �������Ͷ���һ������Ϊ�����ε�׶��
	/// ������ĺ���FOV
	/// </summary>
	private float m_ReticleHorizontalFOVParam;
	/// <summary>
	/// ���ι�����������
	/// </summary>
	private int m_MaxMissileCountInOneShotParam;
	/// <summary>
	/// ����һ��Ŀ����Ҫ��ʱ��. ��
	/// </summary>
	private float m_LockonTimeParam;
	/// <summary>
	/// ׼����׼����ݺ��
	/// </summary>
	private float m_ReticleAspect;

	/// <summary>
	/// ���ڼ�����Щ���屻׼�ǿ�ס�ĸ��������
	/// </summary>
	private Camera m_VirtualCameraForReticleSelection;
	

	public BattleWeapon_Missile_CenterLock(IWeapon weapon) : base(weapon)
	{
		m_LockTargeMap = new Dictionary<SpacecraftEntity, TargetLockInfo>();
		m_State = State.Idle;

		m_WeaponFireData = m_CfgEternityProxy.GetWeaponDataOfMissile(m_WeaponConfig.TypeDateSheetld);

		TranslateUniformParamToWeaponParam();

		// FIXME. ��̬�ڴ����
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
		// �������������� >= 0��ʹ��1��������������ʽ
		// �������������� < 0��ʹ��2��������������ʽ

		// 1��������������ʽ
		// �� 60 / A�� /�� 1 + B / 100��*1000

		// 2��������������ʽ
		// ABS���� 60 / A�� *�� 1 + B / 100 - 2��*1000��

		// A��1�����ӵ���
		// B����������

		// ��ע��		
		// 	������������ʽ������ֵΪ������������
		// 	���յ�λ��ֵΪ����

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
					SLog("��������״̬");
					// ����������ȴ�Ժ���ܷ���
					m_State = State.Lock;
					m_LockTargeMap.Clear();
					m_MainPlayer.SetCanToggleWeapon(false);
					NetworkManager.Instance.GetSkillController().SendMissileLockTarget(true);
				}
				break;
			case State.Lock:
				if (!cast)
				{
					SLog("�ɿ���������");
					if (ExistingLockedTarget() && m_PlayerSkillProxy.CanCurrentWeaponRelease())
					{
						SLog("������Ŀ��, ���빥��״̬, �ͷż���");
						m_State = State.Attack;
						CastSkill();
					}
					else if (ExistingLockedTarget() && !m_PlayerSkillProxy.CanCurrentWeaponRelease())
					{
						GameFacade.Instance.SendNotification(NotificationName.PlayerWeaponFireFail);
					}
					else
					{
						SLog("û������Ŀ��, ������ι�������");
						ResetWeaponRuntimeData();
					}

					NetworkManager.Instance.GetSkillController().SendMissileLockTarget(false);
				}
				break;
			case State.Attack:
				// �����������η���Ĺ�����, ����������, �����Խ�������״̬. By ���� (�ݶ�)
				break;
			default:
				break;
		}
	}

	public override SkillTarget CalculateNextTarget()
	{
		SLog("ѡ����Ŀ��");

		// if there is more than one locked target, return it
		// else DebugError
		if (ExistingLockedTarget())
		{
			// ���Դ���. ֱ��ȡ��һ��Ŀ��
			// �ڶ���: ��Ϊÿ��Ŀ�귢��һ�ε���
			// ������: ��һ����Ŀ����ּ���
			foreach (var pair in m_LockTargeMap)
			{
				if (pair.Value.LockTimes == 0)
					continue;

				SLog("ѡ���һ��Ŀ���ͷ�. {0}", pair.Key.name);
				
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
		SLog("ѡ��Ŀ���ͷ�. {0}", targetListStr);
	}

	public override void PostWeaponFire()
	{

	}

	public override void OnWeaponSkillFinished(bool success)
	{
		SLog("�����ͷ����", m_State);
		// ������ܷ���Ĺ�����, ��ʼ����һ������. �����ͷ����ʱ������յ�ǰ�����б�
		if (m_State != State.Lock)
		{
			ResetWeaponRuntimeData();
		}
	}

	/// <summary>
	/// ����Ƿ��Ѿ�ѡ�����µ�Ŀ��
	/// �����Ѿ������˵�Ŀ�������״̬
	/// </summary>
	/// <param name="deltaTime"></param>
	public override void OnUpdate(float deltaTime)
	{
		base.OnUpdate(deltaTime);

		//  ���Դ���
		if (Input.GetKeyDown(KeyCode.Keypad0))
		{
			ResetWeaponRuntimeData();
			Debug.LogError("������������״̬");
		}
		TranslateUniformParamToWeaponParam();
		//  ���Դ���

		if(m_State == State.Lock)
		{
			SpacecraftEntity newTarget = GetTargetSelectedByCenterRay();

			//NotifyHUDTargetSelected(newTarget);

			// ���������µ�Ŀ��
			if (IsThereSurplusMissile())
			{
				TryToLockThisTarget(newTarget);
			}

			// �����Ѿ������Ŀ��
			ClearLockingTargetWhichIsNotInLockingRect();

			// �������е�Ŀ��
			// iterate all locking and locked target 
			// locking:
			//		once countdown finished, set target state to Locked
			// locking and locked:
			//		if target is out of range / dead, unlock target
			List<SpacecraftEntity> cancelLockList = new List<SpacecraftEntity>();
			foreach (var pair in m_LockTargeMap)
			{
				SpacecraftEntity targetEntity = pair.Key;

                // UNDONE, �������Ŀ���ǲ����Ѿ���������̻����Ѿ�����. ����ǵĻ�, �ͽӴ�����
                if (targetEntity == null || targetEntity.IsDead())
                {
                    cancelLockList.Add(targetEntity);
                    continue;
                }

                TargetLockInfo targetLockInfo = pair.Value;
                if (targetLockInfo.LockState == TargetLockState.Locking&& IsThereSurplusMissile())
				{
                    if(IsThereSurplusMissile()) //������
                    {
                        if (targetLockInfo.LockTimeRemaining <= 0f)
                        {
                            targetLockInfo.LockState = TargetLockState.Locked;
                            targetLockInfo.LockTimeRemaining = 0f;
                            targetLockInfo.LockTimes++;

                            SLog("Ŀ���������. {0}", targetEntity.name);
                        }
                        else
                        {
                            targetLockInfo.LockTimeRemaining -= deltaTime;
                        }
                    }
                    else //����,��δ���תȦ��
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

			// ������Щ�Ѿ������õ�Ŀ��������
			for (int iEntity = 0; iEntity < cancelLockList.Count; iEntity++)
			{
				m_LockTargeMap.Remove(cancelLockList[iEntity]);
				SLog("Ŀ����Ϊ�������߳�����Ұ��ʧȥ����. {0}", iEntity);
			}
		}

		// ֪ͨUI��ǰ����������Ŀ��
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

		SLog("���õ�������״̬");
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
		// ��������ʹ�������Ŀ��ѡ��ʽ
	}

	/// <summary>
	/// ��ȡ����ʱ��
	/// </summary>
	/// <returns></returns>
	public float GetLockonTime()
	{
		return m_LockonTimeParam;
	}

	/// <summary>
	/// ��һ֡������Ŀ��
	/// </summary>
	private SpacecraftEntity m_TargetLockedInLastFrame;

	/// <summary>
	/// ѡ��һ���µ�Ŀ��
	/// 1. ����һ֡ѡ�е�Ŀ�겻ͬ
	/// 2. ��һ֡ѡ�е�Ŀ���Ѿ��������, ������һ������
	/// </summary>
	/// <returns>�Ƿ����µ�Ŀ��</returns>
	private bool PickNewTarget(out SpacecraftEntity newTarget)
	{
		PlayerSkillVO skillVO = m_PlayerSkillProxy.GetCurrentWeaponSkill();
		SkillSystemGrow skillGrow = skillVO.GetSkillGrow();
		float maxDistance = skillGrow.Range * SceneController.SPACE_ACCURACY_TO_CLIENT;
		MainCameraComponent mainCam = CameraManager.GetInstance().GetMainCamereComponent();
		Ray ray = new Ray(mainCam.GetPosition(), mainCam.GetForward());
		RaycastHit hitInfo = new RaycastHit();

		// UNDONE
		// ��һ��: ʹ����Ļ����������ΪĿ��ѡȡ����. NOW
		// �ڶ���: ʹ�� Physics.BoxCast 
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
	/// ���ĵ�����ѡ�е�Ŀ��
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
	/// �����������������Ѿ�������׼���е�Ŀ��
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
			// ����������Ŀ������Ѿ����ڿ���, ��ȡ������
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
			SLog("Ŀ�����������ǰ������, ʧȥ����. {0}", iEntity);
		}
	}

	private bool GetAllEntitySelectedByVirtualCamera(ref List<SpacecraftEntity> targetList)
	{
		MainCameraComponent mainCam = CameraManager.GetInstance().GetMainCamereComponent();

		SkillSystemGrow skillGrow = m_PlayerSkillProxy.GetCurrentWeaponSkill().GetSkillGrow();
		float range = skillGrow.Range * SceneController.SPACE_ACCURACY_TO_CLIENT;

		// ���Ե�ʰȡ. 
		// ʰȡ׶����Ϊһ�����, �Ѽ��������ΪԶ�ü���. ����FOVȡԶ�ü���Ĵ�С, Ͷ�������С��Box, ��ȡ����ʰȡ����Entity
		{
			// ���д��Բü������� = Physics.BoxCastAll(Ͷ�俪ʼ��, ���ӱ߳�, �������, SkillLayer)

			float halfBoxVerticalSideLength = range * Mathf.Tan(m_ReticleHorizontalFOVParam / m_ReticleAspect * 0.5f * Mathf.Deg2Rad);
			float halfBoxHorizontalSideLength = halfBoxVerticalSideLength * m_ReticleAspect;

			// Ͷ�俪ʼ�ĵ� = ������ü�������ĵ� + ������� * ���ӱ߳���һ��
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
            // ������������ķ����λ��
            m_VirtualCameraForReticleSelection.transform.position = mainCam.transform.position;
            m_VirtualCameraForReticleSelection.transform.rotation = mainCam.transform.rotation;

            // ����ϸ���ж�. ��ǰ�汾������Ŀ�����ĵ��ǲ����� ������� ����
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
			SLog("ѡ�д�Ŀ���������. {0}", newTarget.name);
		}

		// �µ�Ŀ�������ֿ���. 1. ���ڵ�ǰĿ���б����� �� ��Ϊ��, 2. ��Ŀ���б���, ���Ǵ�Ŀ���Ѿ��������, ���Խ�����һ������
		if (!m_LockTargeMap.ContainsKey(newTarget))
		{
			TargetLockInfo targetLockInfo = new TargetLockInfo();
			targetLockInfo.LockState = TargetLockState.Locking;
			targetLockInfo.LockTimeRemaining = m_LockonTimeParam;
			targetLockInfo.LockTimes = 0;
			m_LockTargeMap.Add(newTarget, targetLockInfo);

			SLog("������Ŀ��. {0}", newTarget.name);
		}
		else if (IsLockedEntity(newTarget))
		{
			m_LockTargeMap[newTarget].LockTimeRemaining = m_LockonTimeParam;
			m_LockTargeMap[newTarget].LockState = TargetLockState.Locking;

			SLog("������Ŀ��, Ϊ������һ����������. {0}", newTarget.name);
		}
		else
		{
			//SLog("Ŀ������������, ȴ��Ϊ��Ŀ�걻ѡ����. {0}", newTarget.name);
		}
	}

	/// <summary>
	/// �Ƿ������������Entity
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
	/// �Ƿ��Ѿ����������Ŀ��
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
	/// �Ƿ�����Ѿ�������Ŀ��
	/// </summary>
	/// <returns></returns>
	private bool ExistingLockedTarget()
	{
		// �޳���������������Ŀ��. ֻ�����Ѿ�������
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
	/// �ͷż��ܹ��������Ѿ�������Ŀ��
	/// </summary>
	private void CastSkill()
	{
		SLog("�ͷż���. ");
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

		// �൱��һ�ΰ����İ��º�̧��. �ڼ���ϵͳ�м��ܰ����Ĳ���Ҫ�ɶԳ���
		skillCastEvent.KeyPressed = false;
		m_MainPlayer.SendEvent(ComponentEventName.CastSkill, skillCastEvent);
//#endif

    }

    /// <summary>
    /// ���˹����������Ƿ��и���ĵ�ҩ�Թ����������Ŀ��
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
	/// ��ͳһ�Ĳ�������������ĵ�����ɢ����
	/// </summary>
	private void TranslateUniformParamToWeaponParam()
	{
		float accuracy = m_MainPlayer.GetWeaponAttribute(m_WeaponItem.GetUID(), crucis.attributepipeline.attribenum.AttributeName.kWeaponAccuracy);
		float stability = m_MainPlayer.GetWeaponAttribute(m_WeaponItem.GetUID(), crucis.attributepipeline.attribenum.AttributeName.kWeaponStability);

		// �������õ�ʱ���Ǻ���
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