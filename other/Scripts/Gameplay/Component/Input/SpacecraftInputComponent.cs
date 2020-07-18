using Assets.Scripts.Define;
using Assets.Scripts.Proto;
using Cinemachine;
using Crucis.Protocol;
using Crucis.Protocol.GameSession;
using DebugPanel;
using DG.Tweening;
using Game.Frame.Net;
using UnityEngine;
using UnityEngine.Assertions;
using static Cinemachine.CinemachineCore;

public interface ISpacecraftInputProperty
{
	bool IsMain();
	Transform GetSkinRootTransform();

	void SetRotateAxis(Vector3 vector);

	void SetEngineAxis(Vector3 vector);

	void SetOverloadProgress(float value);

	float GetFireCountdown();
	void SetFireCountdown(float countdown);
	uint GetUId();

	void SetAttribute(AttributeName key, double value);
	double GetAttribute(AttributeName key);

	void SetBurstReady(bool value);
	bool GetBurstReady();
    void SetMouseDelta(Vector3 vector);
    Quaternion BornServerRotation();

	Vector2 GetMouseDeltaFactor4Dof();
	Vector2 GetStickDeltaFactor4Dof();
	Vector2 GetMouseDeltaFactor6Dof();
	Vector2 GetStickDeltaFactor6Dof();
	float GetCrossRecoverTime6Dof();
	float GetTurnBegin4DofLerpSpeed();
	float GetTurnEnd4DofLerpSpeed();
	float GetTurnBegin6DofLerpSpeed();
	float GetTurnEnd6DofLerpSpeed();
	float GetTurn4DofFactorX();
	float GetTurn4DofFactorUp();
	float GetTurn4DofFactorDown();
	float GetTurn6DofFactor();

	bool GetIsRightStick();
	void SetIsRightStick(bool isRightStick);

	void SetMouseOffset(Vector3 offset);
	EnumMotionMode GetMotionMode();
    MotionType GetMotionType();
    void SetMotionType(MotionType motionType);

    Transform GetSyncTarget();
    Rigidbody GetRigidbody();
    HeroState GetCurrentState();
    int GetCurrSkillId();
    bool IsLeap();
}

/// <summary>
/// 船形态输入逻辑
/// </summary>
public class SpacecraftInputComponent : EntityComponent<ISpacecraftInputProperty>
{
	private ISpacecraftInputProperty m_SpacecraftInputProperty;

    /// <summary>
    /// Cinemachine获取轴输入值回调
    /// </summary>
    private AxisInputDelegate m_CinemachineGetInputAxisDelegate;

    /// <summary>
    /// 是否进入按下OnSpeedUp标记
    /// </summary>
    bool m_IsOnSpeedUp = false;
    bool m_NewIsOnSpeedUp = false;

    private Vector3 m_EngineAxis;

	private Vector3 m_RotateAxis;

	private Vector2 m_Rotation;
	private Vector2 m_LastRotation;

	/// <summary>
	/// 鼠标or右摇杆输入值
	/// </summary>
	private Vector2 m_MouseStick = Vector2.zero;

	/// <summary>
	/// 6dof下鼠标or右摇杆输入值
	/// </summary>
	private Vector2 m_MouseStick6 = Vector2.zero;

	/// <summary>
	/// 用来6dof准心恢复计算相关
	/// </summary>
	private bool m_IsTweening = false;
	private Vector2 m_TmpMouseStick = Vector2.zero;
	private Vector3 m_TmpRotateAxis = Vector3.zero;

	// Cache
	private PlayerSkillProxy m_PlayerSkillProxy;
	private GameplayProxy m_GamePlayProxy;
	private CfgEternityProxy m_EternityProxy;

	private static int ms_LastInputFrameCount = 0;

    /// <summary>
    /// 转化炉爆发键是否已按下
    /// </summary>
    private bool m_BurstReadyPressed = false;
    /// <summary>
    /// 转化炉爆发键松开时的时间
    /// </summary>
    private float m_BurstReadyReleaseTime = 0;

	private float m_SixDofCursorRange = 1;


	public override void OnInitialize(ISpacecraftInputProperty property)
	{
		m_SpacecraftInputProperty = property;
		m_PlayerSkillProxy = GameFacade.Instance.RetrieveProxy(ProxyName.PlayerSkillProxy) as PlayerSkillProxy;
		m_GamePlayProxy = GameFacade.Instance.RetrieveProxy(ProxyName.GameplayProxy) as GameplayProxy;
		m_EternityProxy = GameFacade.Instance.RetrieveProxy(ProxyName.CfgEternityProxy) as CfgEternityProxy;

		Vector3 rotation = property.GetRigidbody() ? property.GetRigidbody().rotation.eulerAngles : Vector3.zero;
		m_Rotation.x = rotation.x;
		m_Rotation.y = rotation.y;
        ClampRotation();
        Quaternion quaternion = Quaternion.Euler(m_Rotation);
		m_RotateAxis = (quaternion * Vector3.forward).normalized;

		m_SixDofCursorRange = m_EternityProxy.GetGamingConfig(1).Value.CmCharacter.Value.SixDofCursorRange;
	}

	public override void OnAddListener()
	{
		AddListener(ComponentEventName.ChangeMotionType, OnChangeMotionType);
		AddListener(ComponentEventName.InputSample, OnInputSample);
        AddListener(ComponentEventName.ResetRotation, OnResetRotation);
		AddListener(ComponentEventName.Relive, OnRelive);

		m_CinemachineGetInputAxisDelegate = CinemachineCore.GetInputAxis;
		CinemachineCore.GetInputAxis = OnCinemachinGetInputAxis;

		HotkeyManager.Instance.Register("SpacecraftInputComponent_1", HotKeyMapID.SHIP, HotKeyID.ShipMove, OnMoveStick);
		HotkeyManager.Instance.Register("SpacecraftInputComponent_1_1", HotKeyMapID.SHIP, HotKeyID.ShipSpeedUp, OnSpeedUp);
		HotkeyManager.Instance.Register("SpacecraftInputComponent_2", HotKeyMapID.SHIP, HotKeyID.ShipAscend, OnAscend);
		HotkeyManager.Instance.Register("SpacecraftInputComponent_3", HotKeyMapID.SHIP, HotKeyID.ShipDescend, OnDescend);
		HotkeyManager.Instance.Register("SpacecraftInputComponent_5", HotKeyMapID.SHIP, HotKeyID.ShipCamera, OnCameraStick);
		HotkeyManager.Instance.Register("SpacecraftInputComponent_5_1", HotKeyMapID.LEAP, HotKeyID.LeapCamera, OnCameraStick);

		HotkeyManager.Instance.Register("SpacecraftInputComponent_6", HotKeyMapID.SHIP, HotKeyID.WeaponAutoAim, OnAutoAim);
		HotkeyManager.Instance.Register("SpacecraftInputComponent_7", HotKeyMapID.SHIP, HotKeyID.WeaponReload, OnReload);
		HotkeyManager.Instance.Register("SpacecraftInputComponent_8", HotKeyMapID.SHIP, HotKeyID.ShipSwitchMode, OnSwitchMode);
        HotkeyManager.Instance.Register("SpacecraftInputComponent_9", HotKeyMapID.SHIP, HotKeyID.ShipOverload, OnOverload, 0.5f);
		HotkeyManager.Instance.Register("SpacecraftInputComponent_b", HotKeyMapID.SHIP, HotKeyID.WeaponToggleLeft, OnChangeWeaponLeft);
		HotkeyManager.Instance.Register("SpacecraftInputComponent_c", HotKeyMapID.SHIP, HotKeyID.WeaponToggleRight, OnChangeWeaponRight);
        HotkeyManager.Instance.Register("SpacecraftInputComponent_h", HotKeyMapID.SHIP, HotKeyID.ShipReadyBurst, OnReadyToBurst, 0.5f);
        HotkeyManager.Instance.Register("SpacecraftInputComponent_i", HotKeyMapID.SHIP, HotKeyID.WeaponFire, OnWeaponFire);

  //      HotkeyManager.Instance.Register("SpacecraftInputComponent_Skill1", HotKeyMapID.SHIP, HotKeyID.ShipSkill1, OnSkillHotkey1);
		//HotkeyManager.Instance.Register("SpacecraftInputComponent_Skill2", HotKeyMapID.SHIP, HotKeyID.ShipSkill2, OnSkillHotkey2);
		//HotkeyManager.Instance.Register("SpacecraftInputComponent_Skill3", HotKeyMapID.SHIP, HotKeyID.ShipSkill3, OnSkillHotkey3);
        //HotkeyManager.Instance.Register("SpacecraftInputComponent_Skill4", HotKeyMapID.SHIP, HotKeyID.ShipSkill4, OnSkillHotkey4);

#if NewSkill
        HotkeyManager.Instance.Register("SpacecraftInputComponent_Skill1", HotKeyMapID.SHIP, HotKeyID.ShipSkill1, OnNewSkillHotkey1);
        HotkeyManager.Instance.Register("SpacecraftInputComponent_Skill2", HotKeyMapID.SHIP, HotKeyID.ShipSkill2, OnNewSkillHotkey2);
        HotkeyManager.Instance.Register("SpacecraftInputComponent_Skill3", HotKeyMapID.SHIP, HotKeyID.ShipSkill3, OnNewSkillHotkey3);
        HotkeyManager.Instance.Register("SpacecraftInputComponent_Skill4", HotKeyMapID.SHIP, HotKeyID.ShipSkill4, OnNewSkillHotkey4);
#endif



        InputManager.Instance.OnInputActionMapChanged += OnInputMapChanged;
    }

    private void OnInputMapChanged(HotKeyMapID arg2)
    {
        m_EngineAxis = Vector3.zero;
        m_MouseStick = Vector3.zero;
    }

    public override void OnDestroy()
    {
        base.OnDestroy();

        CinemachineCore.GetInputAxis = m_CinemachineGetInputAxisDelegate;
        InputManager.Instance.OnInputActionMapChanged -= OnInputMapChanged;
    }

	#region Input
	public float OnCinemachinGetInputAxis(string axisName)
	{
		if (axisName == "Mouse X")
			return m_MouseStick.x;
		else if (axisName == "Mouse Y")
			return m_MouseStick.y;

		return 0;
	}

	public void OnMoveStick(HotkeyCallback obj)
	{
        var vector = obj.ReadValue<Vector2>();
        var engine = m_EngineAxis;
        m_EngineAxis = GetDirection(vector);
        m_EngineAxis.y = engine.y;
    }

    #region 优化手柄左摇杆万向转8方向
    private readonly float L1 = Mathf.Tan(67.5f * Mathf.Deg2Rad);
    private readonly float L2 = Mathf.Tan(22.5f * Mathf.Deg2Rad);
    private readonly float L3 = Mathf.Tan(-22.5f * Mathf.Deg2Rad);
    private readonly float L4 = Mathf.Tan(-67.5f * Mathf.Deg2Rad);
    private Vector3 GetDirection(Vector2 vector)
    {
        float slope = vector.y / vector.x;
        Vector2 normalized = Vector2.zero;
        Vector3 ret = Vector3.zero;

        if (slope <= L4 || slope >= L1)
        {
            if (vector.y > 0)
            {
                normalized = new Vector2(0, 1).normalized;
            }
            else
            {
                normalized = new Vector2(0, -1).normalized;
            }
        }
        if (slope >= L2 && slope <= L1)
        {
            if (vector.x > 0)
            {
                normalized = new Vector2(1, 1).normalized;
            }
            else
            {
                normalized = new Vector2(-1, -1).normalized;
            }
        }
        if (L3 <= slope && slope <= L2)
        {
            if (vector.x > 0)
            {
                normalized = new Vector2(1, 0).normalized;
            }
            else
            {
                normalized = new Vector2(-1, 0).normalized;
            }
        }
        if (L3 >= slope && slope >= L4)
        {
            if (vector.y > 0)
            {
                normalized = new Vector2(-1, 1).normalized;
            }
            else
            {
                normalized = new Vector2(1, -1).normalized;
            }
        }

        ret.x = normalized.x;
        ret.z = normalized.y;

        ret = ret * vector.magnitude;

        return ret;
    }
    #endregion

    private void OnAscend(HotkeyCallback obj)
	{
		if (obj.started)
		{
			m_EngineAxis.y = 1f;
		}
        if (obj.performed && m_EngineAxis.y == 1f)
        {
            m_EngineAxis.y = 0f;
        }
    }

	private void OnDescend(HotkeyCallback obj)
	{
		if (obj.started)
		{
			m_EngineAxis.y = -1f;
        }
        if (obj.performed && m_EngineAxis.y == -1f)
		{
			m_EngineAxis.y = 0f;
        }
    }

	private void OnRelive(IComponentEvent obj)
	{
		m_RotateAxis = Vector3.forward;
		m_Rotation = Vector2.zero;
	}

    private void ClampRotation()
    {
        if (m_Rotation.x >= 90 && m_Rotation.x < 180)
        {
            m_Rotation.y += 180;
            m_Rotation.x = 180 - m_Rotation.x;
        }
        else if (m_Rotation.x >= 180 && m_Rotation.x < 270)
        {
            m_Rotation.y += 180;
            m_Rotation.x -= 180;
        }
        else if (m_Rotation.x >= 270 && m_Rotation.x < 360)
        {
            m_Rotation.x = m_Rotation.x - 360;
        }
    }


    public void OnResetRotation(IComponentEvent componentEvent)
	{
		ResetRotation resetRotation = componentEvent as ResetRotation;
		if (resetRotation.Type == MotionType.Dof4)
		{
			m_IsTweening = false;
			m_MouseStick = Vector2.zero;
			m_MouseStick6 = Vector2.zero;
			m_RotateAxis = Vector3.zero;
			CameraManager.GetInstance().GetMainCamereComponent().SetTrackedObjectOffset(MainCameraComponent.CMType.Jet, Vector2.zero);
		}
		else if (resetRotation.Type == MotionType.Dof6)
		{
			m_IsTweening = false;
			m_MouseStick = Vector2.zero;
			Vector3 rotation = m_SpacecraftInputProperty.GetSyncTarget().eulerAngles;
			m_Rotation.x = rotation.x;
			m_Rotation.y = rotation.y;
            ClampRotation();
            Quaternion quaternion = Quaternion.Euler(m_Rotation);
			m_RotateAxis = (quaternion * Vector3.forward).normalized;
			CameraManager.GetInstance().GetMainCamereComponent().SetTrackedObjectOffset(MainCameraComponent.CMType.JetSpeedUp, Vector2.zero);
		}
	}

	public void OnSpeedUp(HotkeyCallback callbackContext)
	{
        if (callbackContext.started)
        {
            m_NewIsOnSpeedUp = true;
            //Debug.LogError("OnSpeedUp m_NewIsOnSpeedUp true");            
        }
        else if (callbackContext.performed)
        {
            m_NewIsOnSpeedUp = false;
            //Debug.LogError("OnSpeedUp m_NewIsOnSpeedUp false");
        }
    }

	private Vector2 m_TrackedObjectOffset = Vector2.zero;
	private Vector2 m_TmpTarget;

	private float m_MaxAngle = 15.0f;
	private void Set4DofCameraRot()
	{
        m_MouseStick.x = Mathf.Clamp(m_MouseStick.x, -100, 100);
        m_MouseStick.y = Mathf.Clamp(m_MouseStick.y, -100, 100);
		bool isRightStick = m_SpacecraftInputProperty.GetIsRightStick();
		Vector2 factor = isRightStick ? m_SpacecraftInputProperty.GetStickDeltaFactor4Dof() : m_SpacecraftInputProperty.GetMouseDeltaFactor4Dof();
		if (isRightStick)
		{
			m_SpacecraftInputProperty.SetMouseDelta(m_MouseStick);
			if (m_MouseStick.magnitude < 0.01f)
			{
				m_TrackedObjectOffset = Vector2.Lerp(m_TrackedObjectOffset, Vector2.zero, m_SpacecraftInputProperty.GetTurnEnd4DofLerpSpeed() * Time.deltaTime);
			}
			else
			{
				m_TmpTarget.x = (m_MouseStick.normalized * (m_MouseStick.magnitude / 100.0f) * m_SpacecraftInputProperty.GetTurn4DofFactorX()).x;
				if (m_MouseStick.y > 0)
				{
					m_TmpTarget.y = (m_MouseStick.normalized * (m_MouseStick.magnitude / 100.0f) * m_SpacecraftInputProperty.GetTurn4DofFactorDown()).y;
				}
				else
				{
					m_TmpTarget.y = (m_MouseStick.normalized * (m_MouseStick.magnitude / 100.0f) * m_SpacecraftInputProperty.GetTurn4DofFactorUp()).y;
				}
				m_TrackedObjectOffset = Vector2.Lerp(m_TrackedObjectOffset, m_TmpTarget, m_SpacecraftInputProperty.GetTurnBegin4DofLerpSpeed() * Time.deltaTime);
			}
			CameraManager.GetInstance().GetMainCamereComponent().SetTrackedObjectOffset(MainCameraComponent.CMType.Jet, m_TrackedObjectOffset);
		}
		else
		{
			m_SpacecraftInputProperty.SetMouseDelta(m_MouseStick / 100);
		}

        var scaledRotateSpeed = factor * Time.deltaTime;

        ClampRotation();

        m_Rotation.y += m_MouseStick.x * scaledRotateSpeed.y;
        m_Rotation.x -= m_MouseStick.y * scaledRotateSpeed.x;
		if (Mathf.Abs(m_Rotation.y - m_LastRotation.y) > m_MaxAngle)
		{
			if (m_Rotation.y > m_LastRotation.y)
			{
				m_Rotation.y = m_LastRotation.y + m_MaxAngle;
			}
			else
			{
				m_Rotation.y = m_LastRotation.y - m_MaxAngle;
			}
		}
		if (Mathf.Abs(m_Rotation.x - m_LastRotation.x) > m_MaxAngle)
		{
			if (m_Rotation.x > m_LastRotation.x)
			{
				m_Rotation.x = m_LastRotation.x + m_MaxAngle;
			}
			else
			{
				m_Rotation.x = m_LastRotation.x - m_MaxAngle;
			}
		}
		///m_Rotation.y = m_Rotation.y % 360;
        m_Rotation.x = Mathf.Clamp(m_Rotation.x, -89.9f, 89.9f);

        Quaternion quaternion = Quaternion.Euler(m_Rotation);
		m_RotateAxis = (quaternion * Vector3.forward).normalized;
    }

	private void Set6DofCameraRot()
	{
		m_MouseStick6.x = Mathf.Clamp(m_MouseStick6.x, -50, 50);
		m_MouseStick6.y = Mathf.Clamp(m_MouseStick6.y, -50, 50);
		bool isRightStick = m_SpacecraftInputProperty.GetIsRightStick();
		if (isRightStick && m_MouseStick6.magnitude < 0.01)
		{
			if (m_MouseStick != Vector2.zero && m_RotateAxis != Vector3.zero)
			{
				m_TrackedObjectOffset = Vector2.Lerp(m_TrackedObjectOffset, Vector2.zero, m_SpacecraftInputProperty.GetTurnEnd6DofLerpSpeed() * Time.deltaTime);
                CameraManager.GetInstance().GetMainCamereComponent().SetTrackedObjectOffset(MainCameraComponent.CMType.JetSpeedUp, m_TrackedObjectOffset);

                m_IsTweening = true;
				float recoverTime = m_SpacecraftInputProperty.GetCrossRecoverTime6Dof();
				DOTween.To(() => m_MouseStick, x => m_TmpMouseStick = x, Vector2.zero, recoverTime);
				DOTween.To(() => m_RotateAxis, x => m_TmpRotateAxis = x, Vector3.zero, recoverTime);
			}
		}
		else
		{
			m_IsTweening = false;
			m_MouseStick += m_MouseStick6 * (isRightStick ? m_SpacecraftInputProperty.GetStickDeltaFactor6Dof() : m_SpacecraftInputProperty.GetMouseDeltaFactor6Dof());

			float factor = m_SpacecraftInputProperty.GetTurn6DofFactor();
			m_TmpTarget = (m_MouseStick.magnitude > 0.1f ? 1.0f : 0.0f)
                * new Vector2(m_MouseStick.x / (Screen.width * 0.5f) * factor
				, m_MouseStick.y / (Screen.height * 0.5f) * factor);
			m_TrackedObjectOffset = Vector2.Lerp(m_TrackedObjectOffset, m_TmpTarget, m_SpacecraftInputProperty.GetTurnBegin6DofLerpSpeed() * Time.deltaTime);
           CameraManager.GetInstance().GetMainCamereComponent().SetTrackedObjectOffset(MainCameraComponent.CMType.JetSpeedUp, m_TrackedObjectOffset);

            ClampMouseStickPos();
			m_RotateAxis = m_MouseStick * Time.deltaTime;
		}
	}

    public override void DoGUI(Config config)
    {
        base.DoGUI(config);
	}

	public void OnCameraStick(HotkeyCallback callbackContext)
	{
		switch (m_SpacecraftInputProperty.GetMotionType())
		{
			case MotionType.Mmo:
				{
					m_MouseStick = callbackContext.ReadValue<Vector2>();

					if (callbackContext.control.name == "delta")
					{
						m_MouseStick *= 0.05f;
					}
					m_MouseStick.y *= -1;

					//摇杆死区
					if (Mathf.Abs(m_MouseStick.x) < 0.1f)
						m_MouseStick.x = 0;

					if (Mathf.Abs(m_MouseStick.y) < 0.1f)
						m_MouseStick.y = 0;
				}
				break;
			case MotionType.Dof4:
				{
                    if (m_SpacecraftInputProperty.IsLeap())
					{
						return;
					}
					m_MouseStick = callbackContext.ReadValue<Vector2>();
					if (callbackContext.control.name == "delta")
					{
						m_SpacecraftInputProperty.SetIsRightStick(false);
						if (m_MouseStick.sqrMagnitude < 0.01)
						{
							m_SpacecraftInputProperty.SetMouseDelta(Vector3.zero);
							return;
						}
						Set4DofCameraRot();
					}
					else if (callbackContext.control.name == "rightStick")
					{
						m_SpacecraftInputProperty.SetIsRightStick(true);
					}
				}
				break;
			case MotionType.Dof6:
				{
					m_MouseStick6 = callbackContext.ReadValue<Vector2>();
					if (callbackContext.control.name == "delta")
					{
						m_SpacecraftInputProperty.SetIsRightStick(false);
						Set6DofCameraRot();
					}
					else if (callbackContext.control.name == "rightStick")
					{
						m_SpacecraftInputProperty.SetIsRightStick(true);
					}
				}
				break;
			default:
				break;
		}
	}

	private void ClampMouseStickPos()
	{
		
		float a = (Screen.width / 2) * m_SixDofCursorRange;
		float b = (Screen.height / 2) * m_SixDofCursorRange;
		/// 圆
		if (a == b)
		{
			float r = CalCircularPointRelationship(a, m_MouseStick.x, m_MouseStick.y);
			/// 在圆内
			if (r <= 0)
			{
				return;
			}
			/// 在圆外，点限制在圆上
			else
			{
				if (m_MouseStick.x == 0)
				{
					m_MouseStick.y = m_MouseStick.y > 0 ? b : -b;
				}
				else
				{
					/// y = kx
					/// x*x + y*y = a*a or (b*b)
					float k = m_MouseStick.y / m_MouseStick.x;
					float x = a / Mathf.Sqrt(1 + k * k);
					m_MouseStick.x = m_MouseStick.x > 0 ? x : -x;
					m_MouseStick.y = k * m_MouseStick.x;
				}
			}
		}
		else
		{
			float r = CalEllipsePointRelationship(a, b, m_MouseStick.x, m_MouseStick.y);
			/// 在椭圆内
			if (r <= 1)
			{
				return;
			}
			/// 在椭圆外，点限制在椭圆上
			else
			{
				if (m_MouseStick.x == 0)
				{
					m_MouseStick.y = m_MouseStick.y > 0 ? b : -b;
				}
				else
				{
					/// y = kx
					/// x*x/a*a + y*y/b*b = 1
					float k = m_MouseStick.y / m_MouseStick.x;
					float x = (a * b) / Mathf.Sqrt(b * b + a * a * k * k);
					m_MouseStick.x = m_MouseStick.x > 0 ? x : -x;
					m_MouseStick.y = k * m_MouseStick.x;
				}
			}
		}
	}

	/// <summary>
	/// 圆公式
	/// </summary>
	/// <param name="r">半径</param>
	/// <param name="x">x</param>
	/// <param name="y">y</param>
	/// <returns></returns>
	private float CalCircularPointRelationship(float r, float x, float y)
	{
		return (x * x) + (y * y) - r * r;
	}

	/// <summary>
	/// 椭圆公式
	/// </summary>
	/// <param name="a">长轴</param>
	/// <param name="b">短轴</param>
	/// <param name="x">x</param>
	/// <param name="y">y</param>
	/// <returns></returns>
	private float CalEllipsePointRelationship(float a, float b, float x, float y)
	{
		return (x * x) / (a * a) + (y * y) / (b * b);
	}

	public void OnAutoAim(HotkeyCallback callbackContext)
	{
		switch (m_SpacecraftInputProperty.GetMotionType())
		{
			case MotionType.Mmo:
				{
					if (callbackContext.performed)
					{
						// UNDONE 战斗, 瞄准最近敌人. 无法测试, 等待AddNpc消息的修复
						Debug.Log("自动瞄准");
						CameraManager.GetInstance().GetMainCamereComponent().SetLookAt(MainCameraComponent.CMType.Spacecraft, m_GamePlayProxy.GetNeareastEnemy().transform);
					}
				}
				break;
			default:
				break;
		}
	}

	public void OnReload(HotkeyCallback callbackContext)
	{
		switch (m_SpacecraftInputProperty.GetMotionType())
		{
			case MotionType.Mmo:
				{
					if (callbackContext.performed)
					{
						RequestReload();
					}
				}
				break;
			default:
				break;
		}
	}

	private void RequestReload()
	{
		IWeapon weapon = m_PlayerSkillProxy.GetCurrentWeapon();
		if (weapon != null)
		{
// 			C2S_CHANGE_MAGAZINE msg = SingleInstanceCache.GetInstanceByType<C2S_CHANGE_MAGAZINE>();
// 			msg.protocolID = (ushort)KC2S_Protocol.c2s_change_magazine;
// 			msg.weapon_tid = weapon.GetUID();
// 			SendToGameServer(msg);

            c2s_WeaponReload msg = new c2s_WeaponReload();
            msg.WeaponUid = weapon.GetUID();
            RequestWeaponReloadRPC.RequestWeaponReload(msg);
        }
    }

    private async void RpcChangeState(StateActionOpType stateActionOpType)
    {
        //Debug.LogError("RpcChangeState " + stateActionOpType);
        StateActionOpResponse stateActionOpResponse = await StateActionOpRPC.StateActionOp(stateActionOpType);
        Assert.IsTrue(stateActionOpResponse.Success != null, "Network Exception !!!");
        if ((NormalExceptionCode)stateActionOpResponse.Success.Success_.Code != NormalExceptionCode.NecNoneError)
        {
            Debug.LogErrorFormat($"Network Exeption Code:{stateActionOpResponse.Success.Success_.Code} Content:{stateActionOpResponse.Success.Success_.Content}");
        }
    }

    /// <summary>
    /// 切换战斗巡航状态
    /// </summary>
    public void OnSwitchMode(HotkeyCallback callbackContext)
    {
        switch (m_SpacecraftInputProperty.GetMotionType())
        {
            case MotionType.Mmo:
            case MotionType.Dof4:
            case MotionType.Dof6:
                {
                    if (callbackContext.performed && !m_BurstReadyPressed && (Time.time - m_BurstReadyReleaseTime > 0.1f))
                    {
                        if (m_SpacecraftInputProperty.GetCurrentState().GetMainState() == EnumMainState.Cruise)
                        {
                            RpcChangeState(StateActionOpType.PutIntoGearFighting);
                        }
                        else
                        {
                            if (m_SpacecraftInputProperty.GetFireCountdown() == 0)
                            {
                                if (m_SpacecraftInputProperty.GetCurrentState().IsHasSubState(EnumSubState.Overload))
                                {
                                    RpcChangeState(StateActionOpType.PutOffGearOverLoad);
                                }
                                RpcChangeState(StateActionOpType.PutIntoGearCruise);
                            }
                            else
                            {
                                PlayBattleStateAudio(WwiseMusicSpecialType.SpecialType_Voice_Unable_Switch_State);
                                GameFacade.Instance.SendNotification(NotificationName.MSG_CHANGE_BATTLE_STATE_FAIL);
                            }
                        }
                    }
                }
                break;
            default:
                break;
        }
    }


    /// <summary>
    /// 切换过载状态
    /// </summary>
    /// <param name="callbackContext"></param>
	public void OnOverload(HotkeyCallback callbackContext)
	{
		if (m_SpacecraftInputProperty.GetCurrentState().GetMainState() == EnumMainState.Cruise)
			return;

		switch (m_SpacecraftInputProperty.GetMotionType())
		{
			case MotionType.Mmo:
                {
                    if (callbackContext.started)
                        m_SpacecraftInputProperty.SetOverloadProgress(callbackContext.GetHoldProgress());
                    else if (callbackContext.cancelled)
                        m_SpacecraftInputProperty.SetOverloadProgress(0);
                    else if (callbackContext.performed)
                    {
                        m_SpacecraftInputProperty.SetOverloadProgress(0);
                        if (m_SpacecraftInputProperty.GetCurrentState().IsHasSubState(EnumSubState.Overload))
                        {
                            RpcChangeState(StateActionOpType.PutOffGearOverLoad);
                        }
                        else
                        {
                            if (m_SpacecraftInputProperty.GetAttribute(AttributeName.kPowerValue) > 0)
                            {
                                RpcChangeState(StateActionOpType.PutIntoGearOverLoad);
                            }
                        }
                    }
                }
				break;
			default:
				break;
		}
	}

	public void OnChangeWeaponLeft(HotkeyCallback callbackContext)
	{
		switch (m_SpacecraftInputProperty.GetMotionType())
		{
			case MotionType.Mmo:
            case MotionType.Dof4:
            case MotionType.Dof6:
                {
                    if (!callbackContext.performed
                        || m_SpacecraftInputProperty.GetCurrentState().IsHasSubState(EnumSubState.Peerless) //转化炉模式
                        || m_SpacecraftInputProperty.GetCurrentState().GetMainState() != EnumMainState.Fight//非战斗模式
                        || m_SpacecraftInputProperty.GetCurrSkillId() > 0 //有技能在释放                        
                        )
                        return;

                    m_PlayerSkillProxy.ToggleCurrentWeapon();
				}
				break;
			default:
				break;
		}
	}

	public void OnChangeWeaponRight(HotkeyCallback callbackContext)
	{
		switch (m_SpacecraftInputProperty.GetMotionType())
		{
			case MotionType.Mmo:
            case MotionType.Dof4:
            case MotionType.Dof6:
				{
                    if (!callbackContext.performed
                        || m_SpacecraftInputProperty.GetCurrentState().IsHasSubState(EnumSubState.Peerless) //转化炉模式
                        || m_SpacecraftInputProperty.GetCurrentState().GetMainState() != EnumMainState.Fight//非战斗模式
                        || m_SpacecraftInputProperty.GetCurrSkillId() > 0 //有技能在释放                        
                        )
                        return;

                    m_PlayerSkillProxy.ToggleCurrentWeapon();
				}
				break;
			default:
				break;
		}
	}

    /// <summary>
    /// 爆发键回调
    /// </summary>
    /// <param name="callbackContext">热键状态</param>
	public void OnReadyToBurst(HotkeyCallback callbackContext)
	{
		uint curPeerless = (uint)m_SpacecraftInputProperty.GetAttribute(AttributeName.kConverterValue);
		uint maxPeerless = (uint)m_SpacecraftInputProperty.GetAttribute(AttributeName.kConverterMax);
		bool isFightState = m_SpacecraftInputProperty.GetCurrentState().GetMainState() == EnumMainState.Fight;
		bool isFireWeaponSkilling = m_SpacecraftInputProperty.GetCurrSkillId() < 0;
		bool allowBurst = curPeerless >= maxPeerless && isFightState && isFireWeaponSkilling;

		if (callbackContext.started)
		{
			if (allowBurst)
			{
				if (!m_BurstReadyPressed)
				{
					SendEvent(ComponentEventName.BurstPressed, new ActivateBurstPressedEvent() { Ready = true });
				}

				m_BurstReadyPressed = true;
			}
		}
		else if (callbackContext.performed)
		{
			if (allowBurst)
				m_SpacecraftInputProperty.SetBurstReady(true);
		}
		else if (callbackContext.ended)
		{
			m_BurstReadyPressed = false;
			m_BurstReadyReleaseTime = Time.time;
			m_SpacecraftInputProperty.SetBurstReady(false);
			SendEvent(ComponentEventName.BurstPressed, new ActivateBurstPressedEvent() { Ready = false });
		}
		else if (callbackContext.cancelled)
		{
			m_BurstReadyPressed = false;
			m_BurstReadyReleaseTime = Time.time;
			m_SpacecraftInputProperty.SetBurstReady(false);
		}
	}


    private bool LeftClickIsState = false;
    private bool  OldLeftClickIsState = false;
    private string LeftClickActionName;  

    private void WeaponFireUpdate()
    {
        if (!m_BurstReadyPressed 
            && m_SpacecraftInputProperty.GetCurrentState().GetMainState() == EnumMainState.Fight 
            && LeftClickIsState
            && !OldLeftClickIsState)
        {
           // Leyoutech.Utility.DebugUtility.Log("武器", "input 热键按下，请求释放技能");

            OldLeftClickIsState = LeftClickIsState;
            SkillHotkey skillKey = new SkillHotkey();
            skillKey.Hotkey = LeftClickActionName;
            skillKey.IsWeaponSkill = true;
            skillKey.ActionPhase = HotkeyPhase.Started;
            SendEvent(ComponentEventName.WeaponOperation, skillKey);
        }
    }

    public void OnWeaponFire(HotkeyCallback callbackContext)
	{
        if (m_SpacecraftInputProperty.GetMotionType() == MotionType.Mmo ||
            m_SpacecraftInputProperty.GetMotionType() == MotionType.Dof4)
        {
            if (callbackContext.started && !m_BurstReadyPressed && (Time.time - m_BurstReadyReleaseTime > 0.1f))
            {
                if (m_SpacecraftInputProperty.GetCurrentState().GetMainState() == EnumMainState.Cruise)
                {
                    RpcChangeState(StateActionOpType.PutIntoGearFighting);
                }
            }
        }

        LeftClickIsState = callbackContext.phase == HotkeyPhase.Started;
        LeftClickActionName = callbackContext.action.name;

        if (!m_BurstReadyPressed && !LeftClickIsState)
		{
           // Leyoutech.Utility.DebugUtility.Log("武器", "Input 热键抬起，请求释放技能结束");

            SkillHotkey skillKey = new SkillHotkey();
			skillKey.Hotkey = callbackContext.action.name;
			skillKey.IsWeaponSkill = true;
			skillKey.ActionPhase = callbackContext.phase;

			SendEvent(ComponentEventName.WeaponOperation, skillKey);

            LeftClickIsState = false;
            LeftClickActionName = string.Empty;
            OldLeftClickIsState = false;
        }
        else
		{
			// InputSystem中一个实体按键不能同时设置到两个不同的Action中.
			// 所以把开火和激活Burst模式都放到武器开火的Action中处理.
			if (m_SpacecraftInputProperty.GetBurstReady()
                && m_SpacecraftInputProperty.GetCurrentState().GetMainState() == EnumMainState.Fight
                && !m_SpacecraftInputProperty.GetCurrentState().IsHasSubState(EnumSubState.Peerless)
                && callbackContext.performed)
			{
				if (m_SpacecraftInputProperty.GetAttribute(AttributeName.kConverterValue) >= m_SpacecraftInputProperty.GetAttribute(AttributeName.kConverterMax))
				{
                    RpcChangeState(StateActionOpType.PutIntoGearPeerLess);
				}
			}
		}
	}

	public void SendSpacecraftSkillCastEvent(HotkeyCallback callbackContext, int skillIndex)
	{
		SkillCastEvent skillCastEvent = new SkillCastEvent();
		skillCastEvent.IsWeaponSkill = false;
		skillCastEvent.SkillIndex = skillIndex;
		skillCastEvent.KeyPressed = callbackContext.started;

		SendEvent(ComponentEventName.CastSkill, skillCastEvent);
	}

	public void OnSkillHotkey1(HotkeyCallback callbackContext)
	{
		SendSpacecraftSkillCastEvent(callbackContext, 0);
	}

	public void OnSkillHotkey2(HotkeyCallback callbackContext)
	{
		SendSpacecraftSkillCastEvent(callbackContext, 1);
	}

	public void OnSkillHotkey3(HotkeyCallback callbackContext)
	{
		SendSpacecraftSkillCastEvent(callbackContext, 2);
	}
    public void OnSkillHotkey4(HotkeyCallback callbackContext)
    {
        SendSpacecraftSkillCastEvent(callbackContext, 3);
    }




    public void SendNewSpacecraftSkillCastEvent(HotkeyCallback callbackContext, int skillIndex)
    {
        if (m_SpacecraftInputProperty.GetCurrentState().IsHasSubState(EnumSubState.Peerless) ) //转化炉模式只允许释放武器技能
            return;

        SkillCastEvent skillCastEvent = new SkillCastEvent();
        skillCastEvent.IsWeaponSkill = false;
        skillCastEvent.SkillIndex = skillIndex;
        skillCastEvent.KeyPressed = callbackContext.started;

        SendEvent(ComponentEventName.SkillButtonResponse, skillCastEvent);
    }

    public void OnNewSkillHotkey1(HotkeyCallback callbackContext)
    {
        SendNewSpacecraftSkillCastEvent(callbackContext, 0);
    }

    public void OnNewSkillHotkey2(HotkeyCallback callbackContext)
    {
        SendNewSpacecraftSkillCastEvent(callbackContext, 1);
    }
    public void OnNewSkillHotkey3(HotkeyCallback callbackContext)
    {
        SendNewSpacecraftSkillCastEvent(callbackContext, 2);
    }

    public void OnNewSkillHotkey4(HotkeyCallback callbackContext)
    {
        SendNewSpacecraftSkillCastEvent(callbackContext, 3);
    }

    #endregion

	private void UpdateRightStick()
	{
		if (m_SpacecraftInputProperty.GetIsRightStick())
		{
			switch (m_SpacecraftInputProperty.GetMotionType())
			{
				case MotionType.Dof4:
					{
                        Set4DofCameraRot();
                        if (m_MouseStick.sqrMagnitude < 0.01)
                        {
                            m_SpacecraftInputProperty.SetMouseDelta(Vector3.zero);
                            return;
                        }
					}
					break;
				case MotionType.Dof6:
					{
						Set6DofCameraRot();
					}
					break;
				default:
					break;
			}
		}
	}

	public override void OnUpdate(float delta)
	{
		if (m_IsTweening)
		{
			m_MouseStick = m_TmpMouseStick;
			m_RotateAxis = m_TmpRotateAxis;
		}
		UpdateRightStick();

		m_SpacecraftInputProperty.SetEngineAxis(m_EngineAxis);
		m_SpacecraftInputProperty.SetMouseOffset(m_MouseStick);
		WeaponFireUpdate();

        if (m_IsOnSpeedUp != m_NewIsOnSpeedUp)
        {
            if (m_SpacecraftInputProperty.GetMotionType() == MotionType.Dof4 && m_NewIsOnSpeedUp)
            {
                //Debug.LogError("RequestChangeBattleState OverLoadStatus true");
                if (m_SpacecraftInputProperty.GetAttribute(AttributeName.kPowerValue) > 0)
                {
                    RpcChangeState(StateActionOpType.PutIntoGearOverLoad);
                }
                m_IsOnSpeedUp = m_NewIsOnSpeedUp;
            }
            else if (m_SpacecraftInputProperty.GetMotionType() == MotionType.Dof6 && !m_NewIsOnSpeedUp)
            {
                //Debug.LogError("RequestChangeBattleState OverLoadStatus false");
                RpcChangeState(StateActionOpType.PutOffGearOverLoad);
                m_IsOnSpeedUp = m_NewIsOnSpeedUp;
            }
        }
        if (m_SpacecraftInputProperty.GetCurrentState().GetMainState() == EnumMainState.Cruise)
        {
            m_IsOnSpeedUp = false;
        }
	}

	private float GetAxis(KeyCode minKey, KeyCode maxKey)
	{
		return Input.GetKey(minKey)
			? -1
			: Input.GetKey(maxKey)
				? 1
				: 0;
	}

	/// <summary>
	/// 输入采样事件
	/// </summary>
	/// <param name="componentEvent"></param>
	private void OnInputSample(IComponentEvent componentEvent)
	{
		m_LastRotation.x = m_Rotation.x;
		m_LastRotation.y = m_Rotation.y;
		SendEvent(ComponentEventName.ChangeSpacecraftInputState, new ChangeSpacecraftInputStateEvent()
		{
			EngineAxis = m_SpacecraftInputProperty.GetMotionType() == MotionType.Dof6 ? Vector3.forward : m_EngineAxis,
			RotateAxis = (m_SpacecraftInputProperty.GetMotionType() == MotionType.Dof4 || m_SpacecraftInputProperty.GetMotionType() == MotionType.Dof6) ? m_RotateAxis : Vector3.zero
		});
	}

	private void OnChangeMotionType(IComponentEvent obj)
	{
		InputManager.Instance.SceneInputMap = HotKeyMapID.SHIP;
	}

	/// <summary>
	/// 战斗状态提示音效.
	/// 提示音的英语不会写-_-
	/// </summary>
	/// <param name="audioID"></param>
	private void PlayBattleStateAudio(WwiseMusicSpecialType SpecialType)
	{
		if (m_SpacecraftInputProperty.IsMain())
		{
            WwiseUtil.PlaySound(WwiseManager.voiceComboID, SpecialType, WwiseMusicPalce.Palce_1st, false,null);
        }
    }
}