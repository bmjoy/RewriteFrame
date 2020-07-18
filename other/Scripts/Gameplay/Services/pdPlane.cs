using System;
using DebugPanel;
using UnityEngine;

public class pdPlane : MonoBehaviour
{
    public delegate Vector3 GetRotateAxisDelegate();
    public delegate Vector3 GetEngineAxisDelegate();

    /// <summary>
    /// 运动模型
    /// </summary>
    private MotionType m_MotionType;
    /// <summary>
    /// 假旋转的角度
    /// </summary>
    private Vector3 m_DummyAngular = Vector3.zero;
    /// <summary>
    /// 假旋转的角加速度
    /// </summary>
    private Vector3 m_DummyAngularAcceleration = Vector3.zero;

    /// <summary>
    /// 当前节点，Cache下来是为了效率
    /// </summary>
    private Transform m_Transform;
    /// <summary>
    /// 当前节点的子节点
    /// 用于应用假旋转
    /// </summary>
    private Transform m_SpcaecraftRootTransform;
    /// <summary>
    /// 飞船运动相关的属性
    /// </summary>
    private pdPlaneTweakableProperties m_TweakableProerties;

    private GetRotateAxisDelegate m_GetRotateAxisDelegate;

    private GetEngineAxisDelegate m_GetEngineAxisDelegate;

    private ForDebug _ForDebug;

    public void Init(GetRotateAxisDelegate getRotateAxisDelegate, GetEngineAxisDelegate getEngineAxisDelegate, Transform spcaecraftRootTransform)
    {
        m_GetRotateAxisDelegate = getRotateAxisDelegate;
        m_GetEngineAxisDelegate = getEngineAxisDelegate;

        m_SpcaecraftRootTransform = spcaecraftRootTransform;
    }

    public void ChangeMotionType(MotionType motionType)
    {
        m_MotionType = motionType;

        // TEMP 切换运动模型时清除临时数值
        m_DummyAngular = Vector3.zero;
        m_DummyAngularAcceleration = Vector3.zero;
    }

    public void SetTweakableProerties(pdPlaneTweakableProperties tweakableProerties)
    {
        m_TweakableProerties = tweakableProerties;
    }

    public void DoUpdate(float delta)
    {
        // for performance
        float invertDelta = 1.0f / delta;
        switch (m_MotionType)
        {
            case MotionType.Mmo:
                DoUpdateMotion_Spacecraft(delta, invertDelta);
                break;
            default:
                Debug.Assert(false, "Not implement motion type: " + m_MotionType.ToString());
                break;
        }
    }

    public void DoGUI(Config config)
    {
        GUILayout.Box(string.Format("RotateDummyAxis:{0} Last:{1} Delta:{2}"
           , Leyoutech.Utility.StringUtility.ConvertToDisplay(_ForDebug.RotateDummyAxis)
           , Leyoutech.Utility.StringUtility.ConvertToDisplay(_ForDebug.LastRotateDummyAxis)
           , Leyoutech.Utility.StringUtility.ConvertToDisplay(_ForDebug.RotateDummyAxis - _ForDebug.LastRotateDummyAxis)), config.BoxStyle);

        GUILayout.Box(string.Format("DummyAngular:{0} Last:{1} Delta:{2}"
          , Leyoutech.Utility.StringUtility.ConvertToDisplay(_ForDebug.DummyAngular)
          , Leyoutech.Utility.StringUtility.ConvertToDisplay(_ForDebug.LastDummyAngular)
          , Leyoutech.Utility.StringUtility.ConvertToDisplay(_ForDebug.DummyAngular - _ForDebug.LastDummyAngular)), config.BoxStyle);
    }

    protected void Awake()
    {
        m_Transform = transform;
        m_SpcaecraftRootTransform = transform.Find("PlaneRoot");
    }

    protected void OnDestroy()
    {
        m_SpcaecraftRootTransform = null;
        m_Transform = null;
    }

    private void DoUpdateMotion_Spacecraft(float deltaTime, float invertDeltaTime)
    {
        // 获取用户转向输入
        Vector3 rotateAxis = m_GetRotateAxisDelegate();
        // 引擎推力的杆量输入
        Vector3 engineAxis = m_GetEngineAxisDelegate();
        // 根据用户的杆量算出一个假的转向杆量(为了更好的表现)
        Vector3 rotateDummyAxis = new Vector3(-engineAxis.y, 0, -rotateAxis.y).normalized;
        _ForDebug.LastDummyAngular = _ForDebug.RotateDummyAxis;
        _ForDebug.RotateDummyAxis = rotateDummyAxis;

        // 更新假的旋转
        Vector3 newDummyAngular = CaculateDummyAngular(rotateDummyAxis, deltaTime, invertDeltaTime);
        CaculateDummyAngularAccelerationLimit(m_TweakableProerties.PitchSteering
            , rotateDummyAxis.x
            , m_DummyAngular.x
            , ref newDummyAngular.x
            , ref m_DummyAngularAcceleration.x);
        m_DummyAngular = newDummyAngular;
        _ForDebug.LastDummyAngular = _ForDebug.DummyAngular;
        _ForDebug.DummyAngular = m_DummyAngular;
        m_SpcaecraftRootTransform.localEulerAngles = m_DummyAngular;
    }

    private void DoUpdateMotion_Jet(float deltaTime, float invertDeltaTime)
    {
        // 获取用户转向输入
        Vector3 rotateAxis = m_GetRotateAxisDelegate();
        // 引擎推力的杆量输入
        Vector3 engineAxis = m_GetEngineAxisDelegate();
        // 根据用户的杆量算出一个假的转向杆量(为了更好的表现)
        Vector3 rotateDummyAxis = new Vector3(0, 0, -rotateAxis.y).normalized;

        // 更新假的旋转
        Vector3 newDummyAngular = CaculateDummyAngular(rotateDummyAxis, deltaTime, invertDeltaTime);
        m_DummyAngular = newDummyAngular;
        m_SpcaecraftRootTransform.localEulerAngles = m_DummyAngular;
    }

    /// <summary>
    /// 根据杆量计算假的旋转角度
    /// </summary>
    private Vector3 CaculateDummyAngular(Vector3 rotateDummyAxis, float deltaTime, float invertDeltaTime)
    {
        Vector3 maxDummyAngular = new Vector3(m_TweakableProerties.PitchSteering.MaxDummyAngular
            , m_TweakableProerties.YawSteering.MaxDummyAngular
            , m_TweakableProerties.RollSteering.MaxDummyAngular);
        Vector3 maxDummyAngularAcceleration = new Vector3(m_TweakableProerties.PitchSteering.MaxDummyAcceleration
            , m_TweakableProerties.YawSteering.MaxDummyAcceleration
            , m_TweakableProerties.RollSteering.MaxDummyAcceleration);
        Vector3 targetDummyAngular = new Vector3(CaculateTargetDummyAngular(rotateDummyAxis.x, maxDummyAngular.x)
            , CaculateTargetDummyAngular(rotateDummyAxis.y, maxDummyAngular.y)
            , CaculateTargetDummyAngular(rotateDummyAxis.z, maxDummyAngular.z));
        Vector3 newDummyAngular = hwmMath.NormalizeAngle180(hwmMath.UniformAccelerationTo(m_DummyAngular
            , targetDummyAngular
            , ref m_DummyAngularAcceleration
            , maxDummyAngularAcceleration
            , maxDummyAngular
            , deltaTime
            , invertDeltaTime));
        return newDummyAngular;
    }

    /// <summary>
    /// 限制假旋转的角加速度
    /// </summary>
    private void CaculateDummyAngularAccelerationLimit(pdPlaneTweakableProperties.SteeringProperties steering
        , float axis
        , float oldDummyAngular
        , ref float newDummyAngular
        , ref float dummyAngularAcceleration)
    {
        // 已经达到Pitch的限制了，Pitch不能再向上转了，那Pitch的角速度应该为0
        if (steering.MaxDummyAngular <= Mathf.Abs(newDummyAngular))
        {
            newDummyAngular = steering.MaxDummyAngular * Mathf.Sign(newDummyAngular);
            dummyAngularAcceleration = 0;
        }
        // 无Pitch输入且接近水平面时Pitch归零
        else if (Mathf.Abs(axis) < Mathf.Epsilon
             && Mathf.Sign(newDummyAngular) != Mathf.Sign(oldDummyAngular))
        {
            newDummyAngular = 0;
            dummyAngularAcceleration = 0;
        }
    }

    /// <summary>
    /// 计算假旋转的目标角度
    /// </summary>
    private float CaculateTargetDummyAngular(float axis, float maxDummyAngular)
    {
        return Mathf.Clamp(Mathf.Asin(axis) * maxDummyAngular
            , -maxDummyAngular
            , maxDummyAngular);
    }

    private struct ForDebug
    {
        public Vector3 LastRotateDummyAxis;
        public Vector3 RotateDummyAxis;

        public Vector3 LastDummyAngular;
        public Vector3 DummyAngular;
    }
}