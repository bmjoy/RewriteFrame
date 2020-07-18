using Assets.Scripts.Proto;
using Game.Frame.Net;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using Utils.Timer;

public interface IHumanMotionProperty
{
    bool IsMain();

    Vector3 GetBornServerPosition();

    uint GetUId();

    void SetMovementLocalRotation(Quaternion rotation);

    float GetRunSpeed();

    Transform GetSkinTransform();

    void SetLocalPosition(Vector3 position);
    Vector3 GetLocalPosition();

    Vector3 InverseTransformDirection(Vector3 direction);
    Vector3 TransformDirection(Vector3 direction);

    Transform GetTransform();

    Transform GetMovementRotationTransform();
}

public class HumanMotionComponent : EntityComponent<IHumanMotionProperty>
{
    private IHumanMotionProperty m_HumanMotionProperty;

    /// <summary>
    /// 移动速率 TODO 走配置
    /// </summary>
    /// public float m_RunSpeed = 4;

    /// <summary>
    /// 显示层旋转角速度 TODO 走配置
    /// </summary>
    public float m_RotateSpeed = 8;

    /// <summary>
    /// 摄像机旋转
    /// </summary>
    private Quaternion m_tagetRotation;

    /// <summary>
    /// 导航网格
    /// </summary>
    private NavMeshPath m_NavMeshPath;

    /// <summary>
    /// 更新位置标记
    /// </summary>
    private bool m_IsUpdatePosition = false;

    /// <summary>
    /// 更新旋转标记
    /// </summary>
    private bool m_IsUpdateRotation = false;

    /// <summary>
    /// 摇杆信息
    /// </summary>
    private Vector3 m_EngineAxis;

    /// <summary>
    /// 服务器同步目标位置
    /// </summary>
    private Vector3 m_ServerTargetPosition;

    /// <summary>
    /// 服务器位置
    /// </summary>
    private Vector3 m_ServerPosition;

    /// <summary>
    /// 上一次发送位置tick
    /// </summary>
    private long m_lastSendMoveRequestTick;

    /// <summary>
    /// 当前寻路索引
    /// </summary>
    private uint m_CurrentFindPathIndex;

    /// <summary>
    /// TimerID
    /// </summary>
    private uint m_TimerId = 0;

    /// <summary>
    /// 移动同步阈值
    /// </summary>
    private float m_MoveThreshold; 

#if SHOW_HUMAN_ROAD_POINT
    /// <summary>
    /// 客户端路径
    /// </summary>
    private DrawLines m_ClineLines;

    /// <summary>
    /// 服务器路径
    /// </summary>
    private DrawLines m_ServerLines;
#endif

    public override void OnInitialize(IHumanMotionProperty property)
    {
        m_HumanMotionProperty = property;

        GameplayProxy gameplayProxy = GameFacade.Instance.RetrieveProxy(ProxyName.GameplayProxy) as GameplayProxy;
        m_NavMeshPath = gameplayProxy.GetNavMeshPath();

        m_MoveThreshold = m_HumanMotionProperty.GetRunSpeed();

        m_ServerPosition = m_HumanMotionProperty.GetBornServerPosition();
        m_ServerTargetPosition = m_HumanMotionProperty.GetBornServerPosition();

        CorrectionServerPositionY(ref m_ServerTargetPosition);

        m_HumanMotionProperty.SetLocalPosition(m_ServerTargetPosition);
        m_tagetRotation = m_HumanMotionProperty.GetMovementRotationTransform().rotation;

#if SHOW_HUMAN_ROAD_POINT
        m_ClineLines = m_HumanMotionProperty.GetTransform().gameObject.AddComponent<DrawLines>();
        m_ServerLines = m_HumanMotionProperty.GetTransform().gameObject.AddComponent<DrawLines>();
        m_ServerLines.LineColor = Color.red;

        m_ClineLines.AddPoint(m_HumanMotionProperty.GetLocalPosition());
        m_ServerLines.AddPoint(m_HumanMotionProperty.GetLocalPosition());
#endif
    }

    public override void OnAddListener()
    {
        if (m_HumanMotionProperty.IsMain())
        {
            AddListener(ComponentEventName.ChangeHumanInputState, OnInputStateChange);
            m_TimerId = RealTimerUtil.Instance.Register(1f, RealTimerUtil.EVERLASTING, OnSyncMainPlayerMoveState);
        }
#if SHOW_HUMAN_ROAD_POINT
        AddListener(ComponentEventName.RespondMove, OnRespondMove);
#else
        else
        {
            AddListener(ComponentEventName.HumanMoveRespond, OnHumanMoveRespond);
        }
#endif

        AddListener(ComponentEventName.RespondForceChangePos, OnRespondMoveFailNotify);
    }

    public override void OnDestroy()
    {
        base.OnDestroy();

        RealTimerUtil.Instance.Unregister(m_TimerId);
    }

    private void OnSyncMainPlayerMoveState(int obj)
    {
        SendShipNew();
    }

    public override void OnUpdate(float delta)
    {
        UpdateRotation(delta);

        if (!m_HumanMotionProperty.IsMain())
        {
            /// 与服务器位置差大于阈值 直接设置服务器坐标
            if ((m_ServerPosition - m_HumanMotionProperty.GetLocalPosition()).magnitude > m_MoveThreshold)
            {
                m_HumanMotionProperty.SetLocalPosition(m_ServerPosition);
                return;
            }

            Vector3 worldDirection = (m_ServerTargetPosition - m_HumanMotionProperty.GetLocalPosition());

            Vector3 engine = Vector3.zero;

            if (worldDirection.magnitude > 0.05f)
            {
                engine = m_HumanMotionProperty.GetMovementRotationTransform().InverseTransformDirection(m_ServerTargetPosition - m_HumanMotionProperty.GetLocalPosition()).normalized;
            }

            if (m_EngineAxis != engine)
            {
                m_EngineAxis = engine;
                SendEvent(ComponentEventName.HumanAnimationChangeState, new HumanAnimationChangeStateEvent() { EngineAxis = engine });
            }
        }

        float runSpeed = m_HumanMotionProperty.GetRunSpeed();
        if (m_EngineAxis != Vector3.zero)
        {
            Vector3 nextPosition = Vector3.zero;

            bool isCanMove = false;
            Vector3 currentPosition = m_HumanMotionProperty.GetLocalPosition();
            Vector3 worldDirection = m_HumanMotionProperty.TransformDirection(m_EngineAxis).normalized;
            Vector3 moveVector = worldDirection * runSpeed * delta;
            Vector3 targetPostion = currentPosition + moveVector;
            if (NavMesh.CalculatePath(currentPosition, targetPostion, NavMesh.AllAreas, m_NavMeshPath)
             && m_NavMeshPath.corners.Length > 1)
            {
                isCanMove = true;
                nextPosition = Vector3.MoveTowards(currentPosition, m_NavMeshPath.corners[1], runSpeed * delta);
            }
            else
            {
                NavMeshHit hit;
                if (NavMesh.SamplePosition(targetPostion, out hit, 1, NavMesh.AllAreas))
                {
                    targetPostion = hit.position;
                    if (NavMesh.CalculatePath(currentPosition, targetPostion, NavMesh.AllAreas, m_NavMeshPath))
                    {
                        if (m_NavMeshPath.corners.Length > 1)
                        {
                            isCanMove = true;
                            nextPosition = Vector3.MoveTowards(currentPosition, m_NavMeshPath.corners[1], runSpeed * delta);
                        }
                    }
                }
            }

            if (isCanMove)
            {
                m_HumanMotionProperty.SetLocalPosition(nextPosition);
            }
        }
    }

    private void UpdateRotation(float delta)
    {
        Transform skinTransform = m_HumanMotionProperty.GetSkinTransform();
        if (skinTransform != null && skinTransform.rotation != m_tagetRotation)
        {
            skinTransform.rotation = Quaternion.Lerp(skinTransform.rotation, m_tagetRotation, m_RotateSpeed * delta);
            m_HumanMotionProperty.SetMovementLocalRotation(m_tagetRotation);
        }
    }

    /// <summary>
    /// 修正服务器Y坐标
    /// </summary>
    private void CorrectionServerPositionY(ref Vector3 serverPosition)
    {
        NavMeshHit hit;
        if (NavMesh.SamplePosition(serverPosition, out hit, 100, NavMesh.AllAreas))
        {
            serverPosition.y = hit.position.y;
        }
    }

    /// <summary>
    /// 输入状态改变事件
    /// </summary>
    /// <param name="componentEvent"></param>
    private void OnInputStateChange(IComponentEvent componentEvent)
    {
        ChangeHumanInputStateEvent inputStateChangeEvent = (ChangeHumanInputStateEvent)componentEvent;

        Vector3 engineAxis = inputStateChangeEvent.EngineAxis;

        Quaternion cameraRotation = m_tagetRotation;
        if (engineAxis != Vector3.zero)
        {
            Vector3 globalDIR = CameraManager.GetInstance().GetMainCamereComponent().transform.TransformDirection(Vector3.forward);
            Vector3 localDIR = m_HumanMotionProperty.InverseTransformDirection(new Vector3(globalDIR.x, 0, globalDIR.z));
            cameraRotation = Quaternion.LookRotation(localDIR);
        }

        SetState(engineAxis, cameraRotation);
    }

    /// <summary>
    /// 设置移动状态
    /// </summary>
    /// <param name="engineAxis"></param>
    /// <param name="cameraRotation"></param>
    public void SetState(Vector3 engineAxis, Quaternion cameraRotation)
    {
        m_IsUpdatePosition = false;
        m_IsUpdateRotation = false;

        engineAxis = engineAxis.normalized;

        if (m_EngineAxis != engineAxis)
        {
            m_IsUpdatePosition = true;
            m_EngineAxis = engineAxis;
        }

        if (!m_tagetRotation.Equals(cameraRotation))
        {
            m_IsUpdateRotation = true;
            m_tagetRotation = cameraRotation;
        }

        if (m_IsUpdatePosition || m_IsUpdateRotation)
        {
            SendEvent(ComponentEventName.HumanAnimationChangeState, new HumanAnimationChangeStateEvent() { EngineAxis = m_EngineAxis });

            if (m_HumanMotionProperty.IsMain())
            {
                SendShipNew();

                RealTimerUtil.Instance.RefreshNextUpdateTime(m_TimerId);
            }
        }
    }
    
    /// <summary>
    /// 向服务器发送自己的移动数据
    /// </summary>
    /// <param name="engineAxis"></param>
    /// <param name="rotation"></param>
    private void SendShipNew()
    {
        C2S_HUMAN_MOVE_NEW msg = SingleInstanceCache.GetInstanceByType<C2S_HUMAN_MOVE_NEW>();
        msg.protocolID = (byte)KC2S_Protocol.c2s_human_move_new;
        msg.heroID = m_HumanMotionProperty.GetUId();
        msg.positon_x = m_HumanMotionProperty.GetLocalPosition().x;
        msg.positon_y = m_HumanMotionProperty.GetLocalPosition().y;
        msg.positon_z = m_HumanMotionProperty.GetLocalPosition().z;
        msg.rotation_x = m_tagetRotation.x;
        msg.rotation_y = m_tagetRotation.y;
        msg.rotation_z = m_tagetRotation.z;
        msg.rotation_w = m_tagetRotation.w;
        msg.engine_axis_x = m_EngineAxis.x;
        msg.engine_axis_y = m_EngineAxis.y;
        msg.engine_axis_z = m_EngineAxis.z;
        if (m_EngineAxis == Vector3.zero)
        {
            msg.run_flag = 0;
        }
        else
        {
            msg.run_flag = 1;
        }

        //Debug.LogError("C2S_HUMAN_MOVE_NEW " + JsonUtility.ToJson(msg));

#if SHOW_HUMAN_ROAD_POINT
        m_ClineLines.AddPoint(m_HumanMotionProperty.GetLocalPosition());
#endif

#if ENABLE_SYNCHRONOUS_HUMAN_SELF_LOG
        FightLogToFile.Instance.Write("===== SendShipNew =====\n");
        FightLogToFile.Instance.Write("time " + ClockUtil.Instance().GetMillisecond() + "\n");
        FightLogToFile.Instance.Write("time difference " + GetClinetSendTick() + "\n");
        FightLogToFile.Instance.WriteToJson("C2S_SHIP_MOVE_NEW", msg);
        FightLogToFile.Instance.Write("\n");
#endif

        SendToGameServer(msg);
    }

    /// <summary>
    /// 服务器广播非自己的移动信息
    /// </summary>
    /// <param name="componentEvent"></param>
    private void OnHumanMoveRespond(IComponentEvent componentEvent)
    {
        HumanMoveRespond humanMoveRespond = componentEvent as HumanMoveRespond;

        /// 当前帧所在坐标
        Vector3 currentPosition = m_HumanMotionProperty.GetLocalPosition();

        m_ServerTargetPosition = humanMoveRespond.TargetPosition;
        m_ServerPosition = humanMoveRespond.Position;

        //Debug.LogError("OnHumanMoveRespond " + JsonUtility.ToJson(humanMoveRespond));

        m_tagetRotation = humanMoveRespond.Rotation;

        m_HumanMotionProperty.SetMovementLocalRotation(humanMoveRespond.Rotation);
    }

    /// <summary>
    /// 服务器返回移动失败 修复移动位置
    /// </summary>
    /// <param name="componentEvent"></param>
    private void OnRespondMoveFailNotify(IComponentEvent componentEvent)
    {
		RespondForceChangePos respondMoveFailNotify = componentEvent as RespondForceChangePos;

#if ENABLE_SYNCHRONOUS_HUMAN_SELF_LOG
        FightLogToFile.Instance.Write("===== OnRespondMoveFailNotify =====\n");
        FightLogToFile.Instance.Write("time " + ClockUtil.Instance().GetMillisecond() + "\n");
        FightLogToFile.Instance.WriteToJson("C2S_SHIP_MOVE_NEW", respondMoveFailNotify);
        FightLogToFile.Instance.Write("\n");
#endif

        //CorrectionServerPositionY(ref respondMoveFailNotify.Position);

        m_HumanMotionProperty.SetLocalPosition(respondMoveFailNotify.Position);
        m_HumanMotionProperty.SetMovementLocalRotation(respondMoveFailNotify.Rotation);

        SetState(m_EngineAxis, m_tagetRotation);
    }

    private ulong GetClinetSendTick()
    {
        ulong clinetSendTick = 0;

        if (m_lastSendMoveRequestTick != 0)
        {
            clinetSendTick = (ulong)(ClockUtil.Instance().GetMillisecond() - m_lastSendMoveRequestTick);
        }

        m_lastSendMoveRequestTick = ClockUtil.Instance().GetMillisecond();

        return clinetSendTick;
    }
}
