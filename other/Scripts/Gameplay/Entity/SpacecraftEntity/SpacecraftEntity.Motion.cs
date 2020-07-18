using UnityEngine;

public partial class SpacecraftEntity
{
    #region 运动模型参数
    /// <summary>
    /// 6Dof LookAt点Z轴
    /// </summary>
    [SerializeField]
    private float m_Dof6LookAtZ = 1000f;
    public float GetDof6LookAtZ()
    {
        return m_Dof6LookAtZ;
    }
    public void SetDof6LookAtZ(float val)
    {
        m_Dof6LookAtZ = val;
    }

    /// <summary>
    /// 4dof鼠标灵敏度
    /// </summary>
    [SerializeField]
    private Vector2 MouseDeltaFactor4Dof = Vector2.zero;
    public Vector2 GetMouseDeltaFactor4Dof()
    {
        return MouseDeltaFactor4Dof;
    }
    public void SetMouseDeltaFactor4Dof(float x, float y)
    {
        MouseDeltaFactor4Dof.x = x;
        MouseDeltaFactor4Dof.y = y;
    }

	/// <summary>
	/// 4dof遥杆灵敏度
	/// </summary>
	[SerializeField]
	private Vector2 StickDeltaFactor4Dof = Vector2.zero;
	public Vector2 GetStickDeltaFactor4Dof()
	{
		return StickDeltaFactor4Dof;
	}
	public void SetStickDeltaFactor4Dof(float x, float y)
	{
		StickDeltaFactor4Dof.x = x;
		StickDeltaFactor4Dof.y = y;
	}

	/// <summary>
	/// 6dof鼠标灵敏度
	/// </summary>
	[SerializeField]
    private Vector2 MouseDeltaFactor6Dof = Vector2.zero;
    public Vector2 GetMouseDeltaFactor6Dof()
    {
        return MouseDeltaFactor6Dof;
    }
    public void SetMouseDeltaFactor6Dof(float x, float y)
    {
        MouseDeltaFactor6Dof.x = x;
        MouseDeltaFactor6Dof.y = y;
    }

	/// <summary>
	/// 6dof遥杆灵敏度
	/// </summary>
	[SerializeField]
	private Vector2 StickDeltaFactor6Dof = Vector2.zero;
	public Vector2 GetStickDeltaFactor6Dof()
	{
		return StickDeltaFactor6Dof;
	}
	public void SetStickDeltaFactor6Dof(float x, float y)
	{
		StickDeltaFactor6Dof.x = x;
		StickDeltaFactor6Dof.y = y;
	}

	/// <summary>
	/// 6dof准心恢复时间
	/// </summary>
	[SerializeField]
	private float CrossRecoverTime6Dof = 1.0f;
	public float GetCrossRecoverTime6Dof()
	{
		return CrossRecoverTime6Dof;
	}
	public void SetCrossRecoverTime6Dof(float val)
	{
		CrossRecoverTime6Dof = val;
	}

	/// <summary>
	/// 4dof拟态Roll最大角度
	/// </summary>
	[SerializeField]
    private float MimesisRollMaxAngles4Dof = 45f;
    public float GeMimesisRollMaxAngles4Dof()
    {
        return MimesisRollMaxAngles4Dof;
    }
    public void SetMimesisRollMaxAngles4Dof(float value)
    {
        MimesisRollMaxAngles4Dof = value;
    }

	/// <summary>
	/// 4dof拟态y轴旋转最大角度
	/// </summary>
	[SerializeField]
	private float MimesisYMaxAngles4Dof = 45f;
	public float GetMimesisYMaxAngles4Dof()
	{
		return MimesisYMaxAngles4Dof;
	}
	public void SetMimesisYMaxAngles4Dof(float value)
	{
		MimesisYMaxAngles4Dof = value;
	}

	/// <summary>
	/// 4dof拟态x轴旋转最大角度(下)
	/// </summary>
	[SerializeField]
	private float MimesisXDownMaxAngles4Dof = 20f;
	public float GetMimesisXDownMaxAngles4Dof()
	{
		return MimesisXDownMaxAngles4Dof;
	}
	public void SetMimesisXDownMaxAngles4Dof(float value)
	{
		MimesisXDownMaxAngles4Dof = value;
	}

	/// <summary>
	/// 4dof拟态x轴旋转最大角度(上)
	/// </summary>
	[SerializeField]
	private float MimesisXUpMaxAngles4Dof = 30f;
	public float GetMimesisXUpMaxAngles4Dof()
	{
		return MimesisXUpMaxAngles4Dof;
	}
	public void SetMimesisXUpMaxAngles4Dof(float value)
	{
		MimesisXUpMaxAngles4Dof = value;
	}

	/// <summary>
	/// 4dof拟态Roll时间缩放
	/// </summary>
	[SerializeField]
    private float MimesisRollTimeScale4Dof = 10f;
    public float GeMimesisRollTimeScale4Dof()
    {
        return MimesisRollTimeScale4Dof;
    }
    public void SetMimesisRollTimeScale4Dof(float value)
    {
        MimesisRollTimeScale4Dof = value;
    }

    /// <summary>
    /// 6dof拟态最大角度
    /// </summary>
    [SerializeField]
    private float MimesisMaximumAngle6Dof = 45f;
    public float GetMimesisMaximumAngle6Dof()
    {
        return MimesisMaximumAngle6Dof;
    }
    public void SetMimesisMaximumAngle6Dof(float value)
    {
        MimesisMaximumAngle6Dof = value;
    }

    /// <summary>
    /// 应用参数
    /// </summary>
    [SerializeField]
    private bool m_IsApplyMotionInfo;
    public bool GetIsApplyMotionInfo()
    {
        return m_IsApplyMotionInfo;
    }
    public void SetIsApplyMotionInfo(bool isApplyMotionInfo)
    {
        m_IsApplyMotionInfo = isApplyMotionInfo;
    }

    /// <summary>
    /// 4dof飞船平移时摄像机跟随虚拟模块的横向偏移
    /// </summary>
    [SerializeField]
    private float VirtualOffset4Dof = 0.08f;
    public float GetVirtualOffset4Dof()
    {
        return VirtualOffset4Dof;
    }
    public void SetVirtualOffset4Dof(float offset)
    {
        VirtualOffset4Dof = offset;
    }

    /// <summary>
    /// 4dof飞船平移时虚拟模块的横向偏移速率(每帧偏移)
    /// </summary>
    [SerializeField]
    private float VirtualOffsetRate4Dof = 0.008f;
    public float GetVirtualOffsetRate4Dof()
    {
        return VirtualOffsetRate4Dof;
    }
    public void SetVirtualOffsetRate4Dof(float rate)
    {
        VirtualOffsetRate4Dof = rate;
    }

    /// <summary>
    /// 4dof飞船平移恢复常态时速率
    /// </summary>
    [SerializeField]
    private float VirtualRecoverRate4Dof = 6.0f;
    public float GetVirtualRecoverRate4Dof()
    {
        return VirtualRecoverRate4Dof;
    }
    public void SetVirtualRecoverRate4Dof(float rate)
    {
        VirtualRecoverRate4Dof = rate;
    }

	/// <summary>
	/// 飞船转向开始偏移速度4dof
	/// </summary>
	[SerializeField]
	private float TurnBegin4DofLerpSpeed = 5.0f;
	public float GetTurnBegin4DofLerpSpeed()
	{
		return TurnBegin4DofLerpSpeed;
	}
	public void SetTurnBegin4DofLerpSpeed(float rate)
	{
		TurnBegin4DofLerpSpeed = rate;
	}

	/// <summary>
	/// 飞船转向恢复偏移时间4dof
	/// </summary>
	[SerializeField]
	private float TurnEnd4DofLerpSpeed = 5.0f;
	public float GetTurnEnd4DofLerpSpeed()
	{
		return TurnEnd4DofLerpSpeed;
	}
	public void SetTurnEnd4DofLerpSpeed(float rate)
	{
		TurnEnd4DofLerpSpeed = rate;
	}

	/// <summary>
	/// 飞船转向开始偏移速度6dof
	/// </summary>
	[SerializeField]
	private float TurnBegin6DofLerpSpeed = 5.0f;
	public float GetTurnBegin6DofLerpSpeed()
	{
		return TurnBegin6DofLerpSpeed;
	}
	public void SetTurnBegin6DofLerpSpeed(float rate)
	{
		TurnBegin6DofLerpSpeed = rate;
	}

	/// <summary>
	/// 飞船转向恢复偏移时间4dof
	/// </summary>
	[SerializeField]
	private float TurnEnd6DofLerpSpeed = 5.0f;
	public float GetTurnEnd6DofLerpSpeed()
	{
		return TurnEnd6DofLerpSpeed;
	}
	public void SetTurnEnd6DofLerpSpeed(float rate)
	{
		TurnEnd6DofLerpSpeed = rate;
	}

	/// <summary>
	/// 4Dof飞船转向偏移系数（X）
	/// </summary>
	[SerializeField]
	private float Turn4DofFactorX = 25.0f;
	public float GetTurn4DofFactorX()
	{
		return Turn4DofFactorX;
	}
	public void SetTurn4DofFactorX(float rate)
	{
		Turn4DofFactorX = rate;
	}

	/// <summary>
	/// 4Dof飞船转向偏移系数（上）
	/// </summary>
	[SerializeField]
	private float Turn4DofFactorUp = 100.0f;
	public float GetTurn4DofFactorUp()
	{
		return Turn4DofFactorUp;
	}
	public void SetTurn4DofFactorUp(float rate)
	{
		Turn4DofFactorUp = rate;
	}

	/// <summary>
	/// 4Dof飞船转向偏移系数（下）
	/// </summary>
	[SerializeField]
	private float Turn4DofFactorDown = 20.0f;
	public float GetTurn4DofFactorDown()
	{
		return Turn4DofFactorDown;
	}
	public void SetTurn4DofFactorDown(float rate)
	{
		Turn4DofFactorDown = rate;
	}

	/// <summary>
	/// 6Dof飞船转向偏移系数
	/// </summary>
	[SerializeField]
	private float Turn6DofFactor = 0.16f;
	public float GetTurn6DofFactor()
	{
		return Turn6DofFactor;
	}
	public void SetTurn6DofFactor(float rate)
	{
		Turn6DofFactor = rate;
	}

	#endregion

		/// <summary>
		/// 运动模型组件
		/// </summary>
	private BehaviorController m_behaviorController;
    public BehaviorController GetBehaviorController()
    {
        return m_behaviorController;
    }

    /// <summary>
    /// 是否强制刷新运动模型
    /// </summary>
    private bool m_IsForceRefreshMotionMode;
    public bool GetIsForceRefreshMotionMode()
    {
        return m_IsForceRefreshMotionMode;
    }
    public void SetIsForceRefreshMotionMode(bool isForceRefreshMotionMode)
    {
        m_IsForceRefreshMotionMode = isForceRefreshMotionMode;
    }

    /// <summary>
    /// 当前运动模型参数
    /// </summary>
    [SerializeField]
    private SpacecraftMotionInfo m_CurrentSpacecraftMotionInfo;
    public SpacecraftMotionInfo GetCurrentSpacecraftMotionInfo()
    {
        return m_CurrentSpacecraftMotionInfo;
    }
    public void SetCurrentSpacecraftMotionInfo(SpacecraftMotionInfo spacecraftMotionInfo)
    {
        m_CurrentSpacecraftMotionInfo = spacecraftMotionInfo;
    }

    /// <summary>
    /// 运动模式
    /// </summary>
    private EnumMotionMode m_MotionMode;
    public EnumMotionMode GetMotionMode()
    {
        return m_MotionMode;
    }

    /// <summary>
    /// 鼠标偏移量
    /// </summary>
    private Vector3 m_MouseOffset;
    public Vector3 GetMouseOffset()
    {
        return m_MouseOffset;
    }
    public void SetMouseOffset(Vector3 offset)
    {
        m_MouseOffset = offset;
    }

	private bool m_IsRightStick = false;
	public bool GetIsRightStick()
	{
		return m_IsRightStick;
	}
	public void SetIsRightStick(bool isRightStick)
	{
		m_IsRightStick = isRightStick;
	}

	private void InitMotion()
    {
        if (GetMotionMode() == EnumMotionMode.Dof6ReplaceOverload)
        {
            SetMotionType(MotionType.Dof4);
        }
        else
        {
            SetMotionType(MotionType.Mmo);
        }
    }
}
