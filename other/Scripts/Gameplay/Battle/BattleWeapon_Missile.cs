using Eternity.FlatBuffer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.Debug;

/// <summary>
/// ��������
/// ʹ�ÿ���׼����Ŀ��, һ����׼�Ϳ�ʼ����
/// </summary>
public class BattleWeapon_Missile : BattleWeaponBase
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

	public enum TargetLockState
	{
		/// <summary>
		/// ��������. תȦ
		/// </summary>
		Locking,
		/// <summary>
		/// �Ѿ�������
		/// </summary>
		Locked
	}

	public class TargetLockInfo
	{
		/// <summary>
		/// ����״̬. ��������, �Ѿ�����
		/// </summary>
		public TargetLockState LockState;
		/// <summary>
		/// ��������. ÿ��һ���Ŀ�귢��һ�ε���
		/// </summary>
		public int LockTimes;
		/// <summary>
		/// ʣ������ʱ��
		/// </summary>
		public float LockTimeRemaining;

		/// <summary>
		/// UI��
		/// </summary>
		public float LockTimePercent;
	}

	/// <summary>
	/// ���� �Ѿ�����/�������� ��Ŀ���״̬
	/// </summary>
	private Dictionary<SpacecraftEntity, TargetLockInfo> m_LockTargeMap;

	/// <summary>
	/// ����״̬
	/// </summary>
	private State m_State;

	// ���������Ĳ���
	/// <summary>
	/// ׼��������һ��������, �������Ͷ���һ������Ϊ�����ε�׶��
	/// �������FOV
	/// </summary>
	private float m_ReticleFOVParam;
	/// <summary>
	/// ���ι�����������
	/// </summary>
	private int m_MaxMissileCountInOneShotParam;
	/// <summary>
	/// ����һ��Ŀ����Ҫ��ʱ��. ��
	/// </summary>
	private float m_LockonTimeParam;

	/// <summary>
	/// ���ڼ�����Щ���屻׼�ǿ�ס�ĸ��������
	/// </summary>
	private Camera m_VirtualCameraForReticleSelection;
	

	public BattleWeapon_Missile(IWeapon weapon) : base(weapon)
	{
		m_LockTargeMap = new Dictionary<SpacecraftEntity, TargetLockInfo>();
		m_State = State.Idle;

		// TEST
		TranslateUniformParamToWeaponParam();
		// TEST

		// FIXME. ��̬�ڴ����
		m_VirtualCameraForReticleSelection = new GameObject().AddComponent<Camera>();
		m_VirtualCameraForReticleSelection.enabled = false;
		m_VirtualCameraForReticleSelection.fieldOfView = m_ReticleFOVParam;
		m_VirtualCameraForReticleSelection.aspect = 1f;
	}

	~BattleWeapon_Missile()
	{
	}

    public override void OnRelease()
    {
        base.OnRelease();
        if(m_VirtualCameraForReticleSelection != null)
            Object.Destroy(m_VirtualCameraForReticleSelection.gameObject);
        m_VirtualCameraForReticleSelection = null;
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
				if (cast)
				{
					SLog("��������״̬");
					m_State = State.Lock;
					m_LockTargeMap.Clear();
					m_MainPlayer.SetCanToggleWeapon(false);
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
		SLog("�����ͷ����");
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
		// ���Դ���

		List<SpacecraftEntity> targetList = new List<SpacecraftEntity>();
		// ������Ŀ��, ͬʱ�����������߼�, ����Ҫ��ϸ����. ������ʱ��ע��. ѡ��Ŀ����������߼���HUDִ��
		//bool targetSelected = GetAllEntitySelectedByVirtualCamera(ref targetList);
		//NotifyHUDTargetSelected(targetSelected ? targetList[0] : null);

		// ���������µ�Ŀ��
		if (m_State == State.Lock && IsThereSurplusMissile())
		{
			for (int iTarget = 0; iTarget < targetList.Count; iTarget++)
			{
				TryToLockThisTarget(targetList[iTarget]);
			}
		}

		PlayerMissileWeaponTargetSelectionInfo targetInfoNotify = MessageSingleton.Get<PlayerMissileWeaponTargetSelectionInfo>();
		// FIXME. ��̬�ڴ����
		//targetInfoNotify.TargeList = m_LockTargeMap;
		GameFacade.Instance.SendNotification(NotificationName.PlayerMissileWeaponTargetSelection, targetInfoNotify);

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
			TargetLockInfo targetLockInfo = pair.Value;
			if (targetLockInfo.LockState == TargetLockState.Locking)
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

			// UNDONE, �������Ŀ���ǲ����Ѿ���������̻����Ѿ�����. ����ǵĻ�, �ͽӴ�����
			if (targetEntity == null || targetEntity.IsDead())
			{
				cancelLockList.Add(targetEntity);
			}
		}

		// �Ӵ��Ѿ������õ�Ŀ�������
		for (int iEntity = 0; iEntity < cancelLockList.Count; iEntity++)
		{
			m_LockTargeMap.Remove(cancelLockList[iEntity]);
			SLog("Ŀ����Ϊ�������߳�����Ұ��ʧȥ����. {0}", iEntity);
		}
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
		Camera cam = CameraManager.GetInstance()?.GetMainCamereComponent()?.GetCamera();
		if (cam != null)
		{
			MsgPlayerWeaponCrosshair crosshairOffset = new MsgPlayerWeaponCrosshair();
			crosshairOffset.HorizontalRelativeHeight = m_ReticleFOVParam / (cam.fieldOfView * cam.aspect);
			crosshairOffset.VerticalRelativeHeight = m_ReticleFOVParam / cam.fieldOfView;
			GameFacade.Instance.SendNotification(NotificationName.PlayerWeaponCrosshairScale, crosshairOffset);
		}
	}

	public override MsgPlayerWeaponCrosshair GetRelativeHeightOfReticle()
	{
		MsgPlayerWeaponCrosshair crosshair = new MsgPlayerWeaponCrosshair();
		Camera cam = CameraManager.GetInstance()?.GetMainCamereComponent()?.GetCamera();
		if (cam != null)
		{
			crosshair.HorizontalRelativeHeight = m_ReticleFOVParam / (cam.fieldOfView * cam.aspect);
			crosshair.VerticalRelativeHeight = m_ReticleFOVParam / cam.fieldOfView;
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

	public Vector3 m_Gizmo_BoxStart;
	public Vector3 m_Gizmo_BoxEnd;
	public float m_Gizmo_BoxSideLength;
	public float m_Gizmo_FOV;

	private bool GetAllEntitySelectedByVirtualCamera(ref List<SpacecraftEntity> targetList)
	{
		MainCameraComponent mainCam = CameraManager.GetInstance().GetMainCamereComponent();

		SkillSystemGrow skillGrow = m_PlayerSkillProxy.GetCurrentWeaponSkill().GetSkillGrow();
		float range = skillGrow.Range * SceneController.SPACE_ACCURACY_TO_CLIENT;

		// ���Ե�ʰȡ. 
		// ʰȡ׶����Ϊһ�����, �Ѽ��������ΪԶ�ü���. ����FOVȡԶ�ü���Ĵ�С, Ͷ�������С��Box, ��ȡ����ʰȡ����Entity
		{
			// ���д��Բü������� = Physics.BoxCastAll(Ͷ�俪ʼ��, ���ӱ߳�, �������, SkillLayer)

			float halfBoxSideLength = range * Mathf.Tan(m_ReticleFOVParam * 0.5f * Mathf.Deg2Rad);
			// Ͷ�俪ʼ�ĵ� = ������ü�������ĵ� + ������� * ���ӱ߳���һ��
			Vector3 startPoint = mainCam.transform.position + (mainCam.GetCamera().nearClipPlane + halfBoxSideLength) * mainCam.GetForward();
			RaycastHit[] hitInfos = Physics.BoxCastAll(startPoint,
														Vector3.one * halfBoxSideLength * 2f,
														mainCam.GetForward(),
														Quaternion.identity, range,
														LayerUtil.GetLayersIntersectWithSkillProjectile(true));

			// TEST GIZMO
			m_Gizmo_BoxStart = startPoint;
			m_Gizmo_BoxEnd = startPoint + (mainCam.GetCamera().nearClipPlane + halfBoxSideLength + range) * mainCam.GetForward();
			m_Gizmo_BoxSideLength = halfBoxSideLength * 2f;
			m_Gizmo_FOV = m_ReticleFOVParam;
			// TEST GIZMO

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

        if(m_VirtualCameraForReticleSelection !=null)
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
		// UNDONE. ��Ҫ�����Ӫ�ж��߼�. �ж��ǲ��ǵ���. �ȴ���Ӫ�������
		if (newTarget == null)
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

		SkillCastEvent skillCastEvent = new SkillCastEvent();
		skillCastEvent.IsWeaponSkill = true;
		skillCastEvent.KeyPressed = true;

//#if NewSkill
//#else
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
		
		m_ReticleFOVParam = m_WeaponSpreadParam.param1;
		m_MaxMissileCountInOneShotParam = (int)m_WeaponSpreadParam.param2;
		m_LockonTimeParam = m_WeaponSpreadParam.param3;
	}

	private void SLog(string log, params object[] param)
	{
		Debug.LogFormat("MissileTest| " + log, param);
	}
}