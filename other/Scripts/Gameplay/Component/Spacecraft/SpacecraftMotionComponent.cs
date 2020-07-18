using Assets.Scripts.Define;
using Crucis.Protocol;
using System;
using System.Collections.Generic;
using UnityEngine;

public interface ISpacecraftMotionProperty
{
    Transform GetRootTransform();
	void ChangeVirtualLocalPositionX(float x);
	Transform GetVirtualCameraTransform();
	Rigidbody GetRigidbody();
    bool IsMain();
    uint GetUId();
    double GetAttribute(AttributeName key);
    MotionType GetMotionType();
	KHeroType GetHeroType();
    Transform GetSyncTarget();
    uint UId();
    HeroState GetCurrentState();
    bool GetIsApplyMotionInfo();
    void SetIsApplyMotionInfo(bool isApplyMotionInfo);
	float GetVirtualOffset4Dof();
	float GetVirtualOffsetRate4Dof();
	float GetVirtualRecoverRate4Dof();
	EnumMotionMode GetMotionMode();
    void SetCurrentSpacecraftMotionInfo(SpacecraftMotionInfo spacecraftMotionInfo);
    bool GetIsForceRefreshMotionMode();
    void SetIsForceRefreshMotionMode(bool isChangeMotionType);
    bool IsLeap();
    bool IsDead();
    BehaviorController GetBehaviorController();
}

[Serializable]
public struct SpacecraftMotionInfo
{
    /// <summary>
    /// 最大线速度
    /// </summary>
    public Vector3 LineVelocityMax;
    /// <summary>
    /// 线加速度
    /// </summary>
    public Vector3 LineAcceleration;
    /// <summary>
    /// 反向最大线速度
    /// </summary>
    public Vector3 ReverseLineVelocityMax;
    /// <summary>
    /// 反向线加速度
    /// </summary>
    public Vector3 ReverseLineAcceleration;
    /// <summary>
    /// 阻力线加速度
    /// </summary>
    public Vector3 ResistanceLineAcceleration;
    /// <summary>
    /// 最大角速度
    /// </summary>
    public Vector3 AngularVelocityMax;
    /// <summary>
    /// 角加速度
    /// </summary>
    public Vector3 AngularAcceleration;
    /// <summary>
    /// 阻力角加速度
    /// </summary>
    public Vector3 ResistanceAngularAcceleration;
    /// <summary>
    /// 跃迁加速度
    /// </summary>
    public float CruiseLeapAcceleration;
    /// <summary>
    /// 跃迁反向加速度
    /// </summary>
    public float CruiseLeapReverseAcceleration;

    public struct MimesisInfo
    {
        /// <summary>
        /// 拟态角加速度
        /// </summary>
        public Vector3 AngularAcceleration;

        /// <summary>
        /// 最大转向角度
        /// </summary>
        public Vector3 MaxAngular;
    }

    public MimesisInfo MimesisData;
}

/// <summary>
/// 船形态运动相关逻辑
/// </summary>
public class SpacecraftMotionComponent : EntityComponent<ISpacecraftMotionProperty>
{
    private ISpacecraftMotionProperty m_Property;

    private Transform m_Transform;

    private Rigidbody m_Rigidbody;

    /// <summary>
    /// 飞船移动控制组件
    /// </summary>
    private BehaviorController m_BehaviorController;

    /// <summary>
    /// 摇杆向量
    /// </summary>
    private Vector3 m_EngineAxis;
    /// <summary>
    /// 摇杆向量
    /// </summary>
    private Vector3 m_cameraRotate;

	/// <summary>
	/// 飞船平移时摄像机跟随虚拟模块的横向偏移
	/// </summary>
	private float m_VirtualOffset;

	/// <summary>
	/// 飞船平移时摄像机虚拟模块的横向偏移速率
	/// </summary>
	private float m_VirtualOffsetRate;

	/// <summary>
	/// 飞船平移恢复常态时速率
	/// </summary>
	private float m_VirtualRecoverRate;

	private GameplayProxy m_GameplayProxy;

	private SpacecraftMotionInfo m_FightModeMotionInfo;
    private SpacecraftMotionInfo m_CruiseModeMotionInfo;
    private SpacecraftMotionInfo m_OverloadModeMotionInfo;
    private SpacecraftMotionInfo m_CurrentMotionInfo;

    private float m_MoveValueX;
    private float m_PosValueX;

    /// <summary>
    /// 获取巡航状态运动信息
    /// </summary>
    /// <returns></returns>
    public SpacecraftMotionInfo GetCruiseModeMotionInfo()
    {
        return m_CruiseModeMotionInfo;
    }

    public override void OnInitialize(ISpacecraftMotionProperty property)
    {
        m_Property = property;
        m_Transform = m_Property.GetRootTransform();
        m_Rigidbody = m_Property.GetRigidbody();
        m_GameplayProxy = GameFacade.Instance.RetrieveProxy(ProxyName.GameplayProxy) as GameplayProxy;

		InitializeMovionAttribute();

        m_BehaviorController = m_Property.GetBehaviorController();
        m_BehaviorController.Callback += OnBehaviorControllerEvent;
        
        RefreshSpacecraftMotionState();

        /// TODO 创建运动流
        if (m_Property.IsMain())
        {
            HeroMoveHandler.StartHeroMoveStream();
        } 
    }

    public override void OnDestroy()
    {
        base.OnDestroy();

        m_BehaviorController.Callback -= OnBehaviorControllerEvent;
        if (m_Property.IsMain())
        {
            Map.MapManager.GetInstance()._OnReleasedArea -= OnReleasedArea;
        }
    }

    public override void OnAddListener()
    {
        if (m_Property.IsMain())
        {
			AddListener(ComponentEventName.ChangeSpacecraftInputState, OnInputStateChange);
            Map.MapManager.GetInstance()._OnReleasedArea += OnReleasedArea;
        }  

        AddListener(ComponentEventName.SetOffset, OnSetOffset);
        AddListener(ComponentEventName.Relive, OnRelive);
        AddListener(ComponentEventName.G2C_HeroMove, OnRPCRespondMove);
        AddListener(ComponentEventName.ChangHeroState, OnChangeHeroState);
        AddListener(ComponentEventName.ChangeProperty, OnChangeProperty);
    }

    private void OnChangeProperty(IComponentEvent obj)
    {
        ChangeProperty changeProperty = obj as ChangeProperty;
        RefreshSpacecraftMotionState(changeProperty);
    }

    private void OnReleasedArea(ulong obj)
    {
        ResetCollision();
    }

    private void OnRelive(IComponentEvent obj)
    {
        ResetCollision();
    }

    private void ResetCollision()
    {
        Log("RestCollision");
        Quaternion quaternion = new Quaternion(0f, m_Rigidbody.rotation.y, 0f, m_Rigidbody.rotation.w).normalized;
        m_Rigidbody.freezeRotation = false;
        m_Rigidbody.rotation = quaternion;
        m_Transform.rotation = quaternion;
    }

	private void OnChangeHeroState(IComponentEvent obj)
	{
    }

	public override void OnUpdate(float delta)
	{
        if (m_Property.GetIsApplyMotionInfo())
        {
            m_Property.SetIsApplyMotionInfo(false);
            //RefreshSpacecraftMotionState();
        }

		if (m_Property.GetMotionType() == MotionType.Dof4)
		{
			/// 横向偏移
			/// TODO 测试
			m_VirtualOffset = m_Property.GetVirtualOffset4Dof();
			m_VirtualOffsetRate = m_Property.GetVirtualOffsetRate4Dof();
			m_VirtualRecoverRate = m_Property.GetVirtualRecoverRate4Dof();

			if (m_EngineAxis.x != 0)
			{
				m_MoveValueX -= m_EngineAxis.x * m_VirtualOffsetRate;
				m_MoveValueX = Mathf.Max(-m_VirtualOffset, m_MoveValueX);
				m_MoveValueX = Mathf.Min(m_VirtualOffset, m_MoveValueX);
				m_Property.ChangeVirtualLocalPositionX(m_MoveValueX);
			}
			else if (m_EngineAxis.x == 0 && m_MoveValueX != 0)
			{
				float x = Mathf.Lerp(m_MoveValueX, 0, Time.deltaTime * m_VirtualRecoverRate);
				if (Mathf.Abs(x) < 0.001)
				{
					x = 0;
				}
				m_MoveValueX = x;
				m_Property.ChangeVirtualLocalPositionX(x);
			}
		}
		else if (m_Property.GetMotionType() == MotionType.Dof6)
		{
			Transform trans = m_Property.GetVirtualCameraTransform();
			if (trans && trans.localPosition.x != 0)
			{
				float x = Mathf.Lerp(m_MoveValueX, 0, Time.deltaTime * m_VirtualRecoverRate);
				if (Mathf.Abs(x) < 0.001)
				{
					x = 0;
				}
				m_MoveValueX = x;
				m_Property.ChangeVirtualLocalPositionX(x);
			}
		}

        delta = 0.1f;

        if (Vector3.Distance(m_Transform.position, m_Rigidbody.position) > 5f || m_Property.GetCurrentState().GetMainState() == EnumMainState.Born)
        {
            m_Transform.position = m_Rigidbody.position;
            m_Transform.rotation = m_Rigidbody.rotation;
        }
        else
        {
            m_Transform.position = Vector3.Lerp(m_Transform.position, m_Rigidbody.position, delta);
            m_Transform.rotation = Quaternion.Lerp(m_Transform.rotation, m_Rigidbody.rotation, delta);
        }
    }

    private void RefreshSpacecraftMotionState(ChangeProperty changeProperty = null)
    {
		InitializeMovionAttribute();
		if (m_Property.GetCurrentState().IsHasSubState(EnumSubState.Overload))
        {
            m_CurrentMotionInfo = m_OverloadModeMotionInfo;
        }
        else if (m_Property.GetCurrentState().GetMainState() == EnumMainState.Fight)
        {
            m_CurrentMotionInfo = m_FightModeMotionInfo;
        }
        else
        {
            m_CurrentMotionInfo = m_CruiseModeMotionInfo;
        }

        /// 6dof运动类型使用过载运动参数
        if (m_Property.GetMotionType() == MotionType.Dof6)
        {
            m_CurrentMotionInfo = m_OverloadModeMotionInfo;
        }

        SendEvent(ComponentEventName.RefreshSpacecraftMimesisData, new RefreshSpacecraftMimesisDataEvent()
        {
            MimesisData = m_CurrentMotionInfo.MimesisData
        });

        if (changeProperty != null)
        {
            m_CurrentMotionInfo = changeProperty.SpacecraftMotionInfo;
        }

        m_Property.SetCurrentSpacecraftMotionInfo(m_CurrentMotionInfo);

        m_BehaviorController.InitBehaviorAbility(
            m_CurrentMotionInfo.LineAcceleration,
            m_CurrentMotionInfo.ReverseLineAcceleration,
            m_CurrentMotionInfo.ResistanceLineAcceleration,
            m_CurrentMotionInfo.LineVelocityMax,
            m_CurrentMotionInfo.ReverseLineVelocityMax,
            m_CurrentMotionInfo.AngularAcceleration,
            m_CurrentMotionInfo.ResistanceAngularAcceleration,
            m_CurrentMotionInfo.AngularVelocityMax,
            m_CurrentMotionInfo.CruiseLeapAcceleration,
            m_CurrentMotionInfo.CruiseLeapReverseAcceleration
        );

        if (m_Property.GetHeroType() == KHeroType.htPlayer)
        {
            switch (m_Property.GetMotionType())
            {
                case MotionType.Mmo:
                    if (m_BehaviorController.HasBehaviorType(BehaviorController.BehaviorType.BT_WorldForceCtlMove))
                        m_BehaviorController.WorldForceControlMove(m_EngineAxis, m_cameraRotate);
                    break;
                case MotionType.Dof4:
                    if (m_BehaviorController.HasBehaviorType(BehaviorController.BehaviorType.BT_4DOFForwardMove))
                        m_BehaviorController.DOF4ForwardMove(m_EngineAxis, m_cameraRotate);
                    break;
                case MotionType.Dof6:
                    if (m_BehaviorController.HasBehaviorType(BehaviorController.BehaviorType.BT_6DOFForwardMove))
                        m_BehaviorController.DOF6ForwardMove(m_EngineAxis, m_cameraRotate);
                    break;
                default:
                    break;
            }
        }
    }

    private void OnSetOffset(IComponentEvent componentEvent)
    {
        SetOffsetEvent setOffsetEvent = componentEvent as SetOffsetEvent;
        m_BehaviorController.OnDragWorldPosition(setOffsetEvent.Offset);
    }

    private void OnBehaviorControllerEvent(string str)
    {
        if (str.Equals("forward_to_the_target"))
        {
			m_Property.GetCurrentState().AddSubState(EnumSubState.LeapTurnEnd);
		}
        else if (str.Equals("state_leap2flat"))
        {
			m_Property.GetCurrentState().RemoveSubState(EnumSubState.Leaping);
			m_Property.GetCurrentState().AddSubState(EnumSubState.LeapArrive);
            SendEvent(ComponentEventName.SpacecraftLeapEnd, null);

            /// 关闭限制区域
            Map.MapManager.GetInstance().GetCurrentMapController().SetLimitChangeToAreaUid();
        }
    }

	private void OnInputStateChange(IComponentEvent componentEvent)
	{
        if (m_Property.IsDead() || m_Property.IsLeap())
        {
            return;
        }		

		ChangeSpacecraftInputStateEvent changeSpacecraftInputStateEvent = componentEvent as ChangeSpacecraftInputStateEvent;
		Vector3 engineAxis = changeSpacecraftInputStateEvent.EngineAxis;

        if (m_Property.GetMotionType() == MotionType.Dof4)
		{
			Vector3 rotateAxis = changeSpacecraftInputStateEvent.RotateAxis;

			Vector3 localforce = engineAxis;
			Vector3 cameraRotate = rotateAxis;        

			if (m_Property.GetIsForceRefreshMotionMode() 
                || localforce != m_EngineAxis 
                || (cameraRotate != m_cameraRotate && cameraRotate != Vector3.zero))
			{   
                m_Property.SetIsForceRefreshMotionMode(false);

                m_EngineAxis = localforce;
				m_cameraRotate = cameraRotate;
                m_BehaviorController.DOF4ForwardMove(localforce, m_cameraRotate);
				SendShipNew(m_cameraRotate, m_EngineAxis);
			}
        }
        else if (m_Property.GetMotionType() == MotionType.Dof6)
		{
            Debug.LogError("m_Property.GetMotionType() == MotionType.Dof6");
			Vector3 rotateAxis = changeSpacecraftInputStateEvent.RotateAxis;

			Vector3 localforce = engineAxis;
			Vector3 cameraRotate = rotateAxis;

			if (m_Property.GetIsForceRefreshMotionMode() 
                || localforce != m_EngineAxis 
                || cameraRotate != Vector3.zero
				|| (m_cameraRotate != Vector3.zero && cameraRotate == Vector3.zero))
			{
                m_Property.SetIsForceRefreshMotionMode(false);

                m_EngineAxis = localforce;
				m_cameraRotate.x = -cameraRotate.y;
				m_cameraRotate.y = cameraRotate.x;
				m_cameraRotate.z = 0;
                m_BehaviorController.DOF6ForwardMove(localforce, m_cameraRotate);
                SendShipNew(m_cameraRotate, m_EngineAxis);
			}
        }
        else
		{
			Vector3 cameraRotate = CameraManager.GetInstance().GetMainCamereComponent().transform.TransformDirection(Vector3.forward).normalized;
			cameraRotate.y = 0f;

			float engineAxisY = engineAxis.y;

			Quaternion xzQuaternion = Quaternion.LookRotation(cameraRotate);
			Vector3 orientations = xzQuaternion * engineAxis;
			Vector3 worldforce = orientations;
			worldforce.y = engineAxisY;

			if (worldforce != m_EngineAxis || (m_EngineAxis != Vector3.zero && orientations != m_cameraRotate))
			{
				m_EngineAxis = worldforce;
				m_cameraRotate = orientations;
				m_BehaviorController.WorldForceControlMove(worldforce, orientations);
				SendShipNew(orientations, worldforce);
			}
		}
	}

    private void SendShipNew(Vector3 rotateAxis, Vector3 engineAxis)
    {
        if (m_Property.GetMotionType() == MotionType.Mmo)
        {
            HeroMoveHandler.SyncMMoMove(
               engineAxis,
               rotateAxis,
               m_Property.GetCurrentState().GetOnlyServerState(),
               (uint)m_GameplayProxy.GetCurrentAreaUid(),
               m_GameplayProxy.ClientToServerAreaOffset(m_Rigidbody.position),
               m_Rigidbody.rotation,
               m_Rigidbody.velocity,
               m_Rigidbody.angularVelocity
           );
        }
        else if (m_Property.GetMotionType() == MotionType.Dof4)
        {
            HeroMoveHandler.SyncDof4Move(
                engineAxis,
                rotateAxis,
                m_Property.GetCurrentState().GetOnlyServerState(),
                (uint)m_GameplayProxy.GetCurrentAreaUid(),
                m_GameplayProxy.ClientToServerAreaOffset(m_Rigidbody.position),
                m_Rigidbody.rotation,
                m_Rigidbody.velocity,
                m_Rigidbody.angularVelocity
            );
        }
        else if (m_Property.GetMotionType() == MotionType.Dof6)
        {
            HeroMoveHandler.SyncDof6Move(
                engineAxis,
                rotateAxis,
                m_Property.GetCurrentState().GetOnlyServerState(),
                (uint)m_GameplayProxy.GetCurrentAreaUid(),
                m_GameplayProxy.ClientToServerAreaOffset(m_Rigidbody.position),
                m_Rigidbody.rotation,
                m_Rigidbody.velocity,
                m_Rigidbody.angularVelocity
            );
        }        
    }

    private void OnRPCRespondMove(IComponentEvent componentEvent)
    {
        G2C_HeroMove g2C_HeroMove = componentEvent as G2C_HeroMove;
        HeroMoveParams respond = g2C_HeroMove.Respond;
        m_GameplayProxy.SetCurrentAreaUid(respond.Areaid);
        Vector3 clientPosition = m_GameplayProxy.ServerAreaOffsetToClientPosition(respond.Position);

        Vector3 LineVelocity = respond.LineVelocity;
        Vector3 AngularVelocity = respond.AngularVelocity;
        if (respond.HeroMoveUnit.SmoothMove == null && !m_Property.IsMain())
        {
            m_Rigidbody.position = clientPosition;
            m_Rigidbody.rotation = respond.Rotation;
            m_Rigidbody.rotation.Normalize();

            m_Rigidbody.velocity = respond.LineVelocity;
            m_Rigidbody.angularVelocity = respond.AngularVelocity;
        }

        if (respond.HeroMoveUnit.MmoMove != null)
        {
            Vector3 EngineAxis = respond.HeroMoveUnit.MmoMove.EngineAxis;
            Vector3 RotateAxis = respond.HeroMoveUnit.MmoMove.RotateAxis;
            m_BehaviorController.WorldForceControlMove(EngineAxis, RotateAxis);
            m_EngineAxis = EngineAxis;
            m_cameraRotate = RotateAxis;
        }
        else if (respond.HeroMoveUnit.Dof4Move != null)
        {
            Vector3 EngineAxis = respond.HeroMoveUnit.Dof4Move.EngineForce;
            Vector3 RotateAxis = respond.HeroMoveUnit.Dof4Move.Orientations;
            m_BehaviorController.DOF4ForwardMove(EngineAxis, RotateAxis);
            m_EngineAxis = EngineAxis;
            m_cameraRotate = RotateAxis;
        }
        else if (respond.HeroMoveUnit.Dof6Move != null)
        {
            Vector3 EngineAxis = respond.HeroMoveUnit.Dof6Move.EngineForce;
            Vector3 RotateAxis = respond.HeroMoveUnit.Dof6Move.EngineTorque;
            m_BehaviorController.DOF6ForwardMove(EngineAxis, RotateAxis);
            m_EngineAxis = EngineAxis;
            m_cameraRotate = RotateAxis;
        }
        else if (respond.HeroMoveUnit.SmoothMove != null)
        {
            if (respond.HeroMoveUnit.SmoothMove.Targets.Count > 0)
            {
                List<Vector3> tagets = new List<Vector3>();
                foreach (var point in respond.HeroMoveUnit.SmoothMove.Targets)
                {
                    tagets.Add(point);
                }

                m_BehaviorController.SmoothMoveTargets(tagets, PhysxMotionType.Spacecraft);
            }
        }
        else if (respond.HeroMoveUnit.BindMove != null)
        {
            m_BehaviorController.BindMove(respond.HeroMoveUnit.BindMove);
        }
        else if (respond.HeroMoveUnit.Leap != null)
        {
            /// 当前区域坐标系下 跃迁终点的坐标
            Vector3 currentAreaLeapEndPointOffsetPosition = respond.HeroMoveUnit.Leap.Target;
            if (m_Property.IsMain())
            {
                m_GameplayProxy.SetLeapEndAreaOffsetPosition(currentAreaLeapEndPointOffsetPosition);
                m_GameplayProxy.SetLeapTargetAreaUid((ulong)respond.HeroMoveUnit.Leap.ToAreaId);
                m_GameplayProxy.SetLeapStartAreaOffsetPosition(respond.Position);
                Map.MapManager.GetInstance().GetCurrentMapController().SetLimitChangeToAreaUid((ulong)respond.HeroMoveUnit.Leap.ToAreaId);

                Vector3 worldPosition;
                m_GameplayProxy.ServerAreaOffsetPositionToWorldPosition(out worldPosition, currentAreaLeapEndPointOffsetPosition);
                m_GameplayProxy.SetLeapTargetAreaOffset(m_GameplayProxy.WorldPositionToServerAreaOffsetPosition(worldPosition, (ulong)respond.HeroMoveUnit.Leap.ToAreaId));
            }

            m_BehaviorController.Leap(new List<Vector3>() { currentAreaLeapEndPointOffsetPosition });
            //Log("=========================");
            //m_GameplayProxy.LogTotalPositionOffset();
            //Log($"OnShipJumpResponse currentAreaOffsetPosition={currentAreaLeapEndPointOffsetPosition}");
            //Log($"OnShipJumpResponse LeapEndAreaOffsetPosition={m_GameplayProxy.GetLeapEndAreaOffsetPosition()}");
            //Log($"OnShipJumpResponse LeapTargetAreaOffse={m_GameplayProxy.GetLeapTargetAreaOffset()}");
            //Log($"OnShipJumpResponse area_id={respond.HeroMoveUnit.Leap.ToAreaId}");
        }
        else if (respond.HeroMoveUnit.LeapCancel != null)
        {
            m_BehaviorController.LeapBreak();
        }
        else 
        {
            Vector3 position = respond.Position;
            Quaternion rotation = respond.Rotation;
            Log("ForecMove", $"Uid={m_Property.GetUId()} Position={position} rotation={rotation}");
        }
    }

    private void OnStateChange(ulong newState)
    {
  //      ulong oldState = m_SpacecraftMotionProperty.GetAllState();
		//uint oldMoveState = (uint)(oldState >> 32);
		//uint newMoveState = (uint)(newState >> 32);
		//m_SpacecraftMotionProperty.SetAllState(newState);
		//SendEvent(ComponentEventName.SpacecraftChangeState, new SpacecraftChangeState()
		//{
		//	OldMoveState = oldMoveState,
		//	NewMoveState = newMoveState
		//});

        //if (m_SpacecraftMotionProperty.HasState(ComposableState.IsSmallLeaping))
        //{
        //    HeroState currentHeroStata = m_SpacecraftMotionProperty.GetCurrentState();
        //    HeroState previousHeroState = m_SpacecraftMotionProperty.GetPreviousState();
        //    GameFacade.Instance.SendNotification(NotificationName.MSG_CHANGE_BATTLE_STATE, new ChangeBattleStateNotify()
        //    {
        //        IsSelf = m_SpacecraftMotionProperty.IsMain(),
        //        NewState = currentHeroStata,
        //        OldStata = previousHeroState
        //    });
        //}

		//if (m_SpacecraftMotionProperty.HasState(ComposableState.FightingStatus) == m_SpacecraftMotionProperty.HasState(ComposableState.CruiseStatus))
		//{
		//	Debug.AssertFormat(m_SpacecraftMotionProperty.HasState(ComposableState.FightingStatus) != m_SpacecraftMotionProperty.HasState(ComposableState.CruiseStatus), "同时有战斗和巡航状态或者都没有");
		//}

        #region wzc
        //bool oldFighting = (oldMoveState & 1 << (int)ComposableState.FightingStatus) > 0;
        //bool newFighting = (newMoveState & 1 << (int)ComposableState.FightingStatus) > 0;

        //if (oldFighting != newFighting)
        //{
        //    // 战斗模式
        //    ChangeBattleStateEvent battleEvent = new ChangeBattleStateEvent();
        //    battleEvent.BattleState = m_SpacecraftMotionProperty.HasState(ComposableState.FightingStatus) ? BattleState.Battle : BattleState.Cruise;
        //    SendEvent(ComponentEventName.ChangeBattleState, battleEvent);

        //    /*
        //    if (m_SpacecraftMotionProperty.IsMain())
        //    {
        //        ChangeBattleStateNotify notify = new ChangeBattleStateNotify();
        //        notify.BattleState = m_SpacecraftMotionProperty.HasState(ComposableState.FightingStatus) ? BattleState.Battle : BattleState.Cruise;
        //        GameFacade.Instance.SendNotification(NotificationName.MSG_CHANGE_BATTLE_STATE, notify);
        //    }
        //    */
        //}

        //bool oldOverload = (oldMoveState & 1 << (int)ComposableState.OverLoadStatus) > 0;
        //bool newOverload = (newMoveState & 1 << (int)ComposableState.OverLoadStatus) > 0;

        //if (oldOverload != newOverload)
        //{
        //    // 过载模式
        //    ActivateOverloadEvent overloadEvent = new ActivateOverloadEvent();
        //    overloadEvent.Active = m_SpacecraftMotionProperty.HasState(ComposableState.OverLoadStatus);
        //    SendEvent(ComponentEventName.ActivateOverload, overloadEvent);

        //    if (m_SpacecraftMotionProperty.IsMain())
        //    {
        //        ActivateOverloadNotify notify = new ActivateOverloadNotify();
        //        overloadEvent.Active = m_SpacecraftMotionProperty.HasState(ComposableState.OverLoadStatus);
        //        GameFacade.Instance.SendNotification(NotificationName.MSG_ACTIVATE_OVERLOAD, notify);
        //    }
        //}
        #endregion
		//bool oldBurst = (oldMoveState & 1 << (int)ComposableState.PeerlessStatus) > 0;
		//bool newBurst = (newMoveState & 1 << (int)ComposableState.PeerlessStatus) > 0;
		//if (oldBurst != newBurst)
		//{
		//	if (newBurst)
		//	{
		//		m_SpacecraftMotionProperty.SetBurstReady(false);
		//	}

		//	ActivateBurstEvent burstEvent = new ActivateBurstEvent();
            //burstEvent.Active = m_SpacecraftMotionProperty.HasState(ComposableState.PeerlessStatus);
   //         burstEvent.Active = m_SpacecraftMotionProperty.GetCurrentState().IsHasSubState(EnumSubState.Peerless);
			//SendEvent(ComponentEventName.ActivateBurst, burstEvent);

            //if (m_SpacecraftMotionProperty.IsMain())
            //    GameFacade.Instance.SendNotification(NotificationName.PlayerWeaponToggleEnd);
		//}
        //bool newBack = (newMoveState & 1 << (int)ComposableState.BackToAnchor) > 0;
        //bool oldBack = (oldMoveState & 1 << (int)ComposableState.BackToAnchor) > 0;
        //if (newBack != oldBack)
        //{
        //    MSAIBossProxy msap = GameFacade.Instance.RetrieveProxy(ProxyName.MSAIBossProxy) as MSAIBossProxy;
        //    if (newBack)
        //        msap.OnSaveMonsterRetraList(m_SpacecraftMotionProperty.GetUId());
        //    else
        //        msap.OnRemoveMonsterRetra(m_SpacecraftMotionProperty.GetUId());

        //}


  //      if (m_SpacecraftMotionProperty.GetMotionType() == MotionType.Leap)
		//{
		//	if (!m_SpacecraftMotionProperty.HasState(ComposableState.IsSmallLeapPreparing) &&
		//		!m_SpacecraftMotionProperty.HasState(ComposableState.IsSmallLeaping))
		//	{
		//		MotionType type = MotionType.Mmo;
		//		if (m_SpacecraftMotionProperty.GetMotionMode() == EnumMotionMode.Dof6ReplaceOverload)
		//		{
		//			type = MotionType.Dof4;
		//		}

		//		SendEvent(ComponentEventName.ChangeMotionType, new ChangeMotionTypeEvent()
		//		{
		//			MotionType = type,
		//		});
  //              UIManager.Instance.OpenPanel(UIPanel.HudAreaNamePanel);
  //              GameFacade.Instance.SendNotification(NotificationName.ChangeArea);
  //              if (m_SpacecraftMotionProperty.IsMain())
		//		{
		//			WwiseUtil.PlaySound(WwiseManager.voiceComboID, WwiseMusicSpecialType.SpecialType_Voice_Converter_Explosion_StateOver, WwiseMusicPalce.Palce_1st, false, null);
		//		}
				//MineDropItemManager.Instance.CheckSyncDropItem();
		//	}
		//}
		//else if (m_SpacecraftMotionProperty.HasState(ComposableState.IsSmallLeaping))
		//{
			//SendEvent(ComponentEventName.ChangeMotionType, new ChangeMotionTypeEvent()
			//{
			//	MotionType = MotionType.Leap,
			//});

            //WwiseUtil.PlaySound(m_MusicComboID
            //    , WwiseMusicSpecialType.SpecialType_JumpBegin
            //    , m_SpacecraftMotionProperty.IsMain()
            //        ? WwiseMusicPalce.Palce_1st
            //        : WwiseMusicPalce.Palce_3st
            //    , false, m_SpacecraftMotionProperty.GetRootTransform());

			/// TODO.跃迁清下缓存
			//MineDropItemManager.Instance.DestoryAllDropGameObject();
		//}

		//if (oldState != newState)
        //{
            //GameFacade.Instance.SendNotification(NotificationName.StateChanged);
        //}

   //     if (m_SpacecraftMotionProperty.HasState(ComposableState.IsSmallLeapCancel))
   //     {
   //         UnityEngine.Debug.Log("OnStateChange LeapBreak");
   //         m_BehaviorController.LeapBreak();
			//MotionType type = MotionType.Mmo;
			//if (m_SpacecraftMotionProperty.GetMotionMode() == EnumMotionMode.Dof6ReplaceOverload)
			//{
			//	type = MotionType.Dof4;
			//}

			//SendEvent(ComponentEventName.ChangeMotionType, new ChangeMotionTypeEvent()
   //         {
   //             MotionType = type,
   //         });

            //WwiseUtil.PlaySound(m_MusicComboID
            //    , WwiseMusicSpecialType.SpecialType_JumpPreparationCancel
            //    , m_SpacecraftMotionProperty.IsMain()
            //        ? WwiseMusicPalce.Palce_1st
            //        : WwiseMusicPalce.Palce_3st
            //    , false, m_SpacecraftMotionProperty.GetRootTransform());
            //if (m_SpacecraftMotionProperty.IsMain())
            //{
            //    WwiseUtil.PlaySound(WwiseManager.voiceComboID, WwiseMusicSpecialType.SpecialType_Voice_Off_Transition, WwiseMusicPalce.Palce_1st, false, null);
            //}
        //}
    }

    /// <summary>
    /// 初始化移动属性
    /// </summary>
    /// <param name="property"></param>
    private void InitializeMovionAttribute()
    {
        /// 战斗状态
        m_FightModeMotionInfo = new SpacecraftMotionInfo()
        {
            LineVelocityMax = new Vector3(
               GetAttribute(AttributeName.kFightRightSpeedMax),
               GetAttribute(AttributeName.kFightUpSpeedMax),
               GetAttribute(AttributeName.kFightFrontSpeedMax)
            ),
            ReverseLineVelocityMax = new Vector3(
                GetAttribute(AttributeName.kFightLeftSpeedMax),
                GetAttribute(AttributeName.kFightDownSpeedMax),
                GetAttribute(AttributeName.kFightBackSpeedMax)
            ),

            LineAcceleration = new Vector3(
                GetAttribute(AttributeName.kFightRightSpeedAccelerate),
                GetAttribute(AttributeName.kFightUpSpeedAccelerate),
                GetAttribute(AttributeName.kFightFrontSpeedAccelerate)
            ),
            ReverseLineAcceleration = new Vector3(
                GetAttribute(AttributeName.kFightLeftSpeedAccelerate),
                GetAttribute(AttributeName.kFightDownSpeedAccelerate),
                GetAttribute(AttributeName.kFightBackSpeedAccelerate)
            ),

            ResistanceLineAcceleration = new Vector3(
                GetAttribute(AttributeName.kFightLeftRightSpeedDecelerate),
                GetAttribute(AttributeName.kFightUpDownSpeedDecelerate),
                GetAttribute(AttributeName.kFightFrontBackSpeedDecelerate)
            ),

            AngularVelocityMax = new Vector3(
                GetAttribute(AttributeName.kFightXMaxAngleSpeed),
                GetAttribute(AttributeName.kFightYMaxAngleSpeed),
                GetAttribute(AttributeName.kFightZMaxAngleSpeed)
            ),
            AngularAcceleration = new Vector3(
                GetAttribute(AttributeName.kFightXAngleSpeedAccelerate),
                GetAttribute(AttributeName.kFightYAngleSpeedAccelerate),
                GetAttribute(AttributeName.kFightZAngleSpeedAccelerate)
            ),
            ResistanceAngularAcceleration = new Vector3(
                GetAttribute(AttributeName.kFightXAngleSpeedDecelerate),
                GetAttribute(AttributeName.kFightYAngleSpeedDecelerate),
                GetAttribute(AttributeName.kFightZAngleSpeedDecelerate)
            ),
            MimesisData = new SpacecraftMotionInfo.MimesisInfo()
            {
                AngularAcceleration = new Vector3(
                    GetAttribute(AttributeName.kXFightMimicryAngleSpeedAccelerate),
                    GetAttribute(AttributeName.kYFightMimicryAngleSpeedAccelerate),
                    GetAttribute(AttributeName.kZFightMimicryAngleSpeedAccelerate)
                ),
                MaxAngular = new Vector3(
                    GetAttribute(AttributeName.kXFightMimicryAngle),
                    GetAttribute(AttributeName.kYFightMimicryAngle),
                    GetAttribute(AttributeName.kZFightMimicryAngle)
                )
            }
        };
		/// 巡航状态
		m_CruiseModeMotionInfo = new SpacecraftMotionInfo()
		{
			LineVelocityMax = new Vector3(
				GetAttribute(AttributeName.kRightSpeedMax),
				GetAttribute(AttributeName.kUpSpeedMax),
				GetAttribute(AttributeName.kFrontSpeedMax)
			),
			ReverseLineVelocityMax = new Vector3(
				GetAttribute(AttributeName.kLeftSpeedMax),
				GetAttribute(AttributeName.kDownSpeedMax),
				GetAttribute(AttributeName.kBackSpeedMax)
			),

			LineAcceleration = new Vector3(
				GetAttribute(AttributeName.kRightSpeedAccelerate),
				GetAttribute(AttributeName.kUpSpeedAccelerate),
				GetAttribute(AttributeName.kFrontSpeedAccelerate)
			),
			ReverseLineAcceleration = new Vector3(
				GetAttribute(AttributeName.kLeftSpeedAccelerate),
				GetAttribute(AttributeName.kDownSpeedAccelerate),
				GetAttribute(AttributeName.kBackSpeedAccelerate)
			),

			ResistanceLineAcceleration = new Vector3(
				GetAttribute(AttributeName.kLeftRightSpeedDecelerate),
				GetAttribute(AttributeName.kUpDownSpeedDecelerate),
				GetAttribute(AttributeName.kFrontBackSpeedDecelerate)
			),

			AngularVelocityMax = new Vector3(
			   GetAttribute(AttributeName.kXAngleSpeedMax),
			   GetAttribute(AttributeName.kYAngleSpeedMax),
			   GetAttribute(AttributeName.kZAngleSpeedMax)
			),
			AngularAcceleration = new Vector3(
				GetAttribute(AttributeName.kXAngleSpeedAccelerate),
				GetAttribute(AttributeName.kYAngleSpeedAccelerate),
				GetAttribute(AttributeName.kZAngleSpeedAccelerate)
			),
			ResistanceAngularAcceleration = new Vector3(
				GetAttribute(AttributeName.kXAngleSpeedDecelerate),
				GetAttribute(AttributeName.kYAngleSpeedDecelerate),
				GetAttribute(AttributeName.kZAngleSpeedDecelerate)
			),
			CruiseLeapAcceleration = GetAttribute(AttributeName.kJumpSpeedAccelerate),
			CruiseLeapReverseAcceleration = GetAttribute(AttributeName.kJumpSpeedDecelerate),
            MimesisData = new SpacecraftMotionInfo.MimesisInfo()
            {
                AngularAcceleration = new Vector3(
                    GetAttribute(AttributeName.kXMimicryMaxAngleAccelerate),
                    GetAttribute(AttributeName.kYMimicryMaxAngleAccelerate),
                    GetAttribute(AttributeName.kZMimicryMaxAngleAccelerate)
                ),
                MaxAngular = new Vector3(
                    GetAttribute(AttributeName.kXMimicryMaxAngle),
                    GetAttribute(AttributeName.kYMimicryMaxAngle),
                    GetAttribute(AttributeName.kZMimicryMaxAngle)
                )
            }
        };
		/// 过载状态
		m_OverloadModeMotionInfo = new SpacecraftMotionInfo()
		{
			LineVelocityMax = new Vector3(
				GetAttribute(AttributeName.kOverloadRightSpeedMax),
				GetAttribute(AttributeName.kOverloadUpSpeedMax),
				GetAttribute(AttributeName.kOverloadFrontSpeedMax)
			),
			ReverseLineVelocityMax = new Vector3(
				GetAttribute(AttributeName.kOverloadLeftSpeedMax),
				GetAttribute(AttributeName.kOverloadDownSpeedMax),
				GetAttribute(AttributeName.kOverloadBackSpeedMax)
			),

			LineAcceleration = new Vector3(
				GetAttribute(AttributeName.kOverloadRightSpeedAccelerate),
				GetAttribute(AttributeName.kOverloadUpSpeedAccelerate),
				GetAttribute(AttributeName.kOverloadFrontSpeedAccelerate)
			),
			ReverseLineAcceleration = new Vector3(
				GetAttribute(AttributeName.kOverloadLeftSpeedAccelerate),
				GetAttribute(AttributeName.kOverloadDownSpeedAccelerate),
				GetAttribute(AttributeName.kOverloadBackSpeedAccelerate)
			),

			ResistanceLineAcceleration = new Vector3(
				GetAttribute(AttributeName.kOverloadLeftRightSpeedDecelerate),
				GetAttribute(AttributeName.kOverloadUpDownSpeedDecelerate),
				GetAttribute(AttributeName.kOverloadFrontBackSpeedDecelerate)
			),

			AngularVelocityMax = new Vector3(
				GetAttribute(AttributeName.kXOverloadAngleSpeedMax),
				GetAttribute(AttributeName.kYOverloadAngleSpeedMax),
				GetAttribute(AttributeName.kZOverloadAngleSpeedMax)
			),
			AngularAcceleration = new Vector3(
				GetAttribute(AttributeName.kXOverloadAngleSpeedAccelerate),
				GetAttribute(AttributeName.kYOverloadAngleSpeedAccelerate),
				GetAttribute(AttributeName.kZOverloadAngleSpeedAccelerate)
			),
			ResistanceAngularAcceleration = new Vector3(
				GetAttribute(AttributeName.kXOverloadAngleSpeedDecelerate),
				GetAttribute(AttributeName.kYOverloadAngleSpeedDecelerate),
				GetAttribute(AttributeName.kZOverloadAngleSpeedDecelerate)
			),
            MimesisData = new SpacecraftMotionInfo.MimesisInfo()
            {
                AngularAcceleration = new Vector3(
                    GetAttribute(AttributeName.kXOverloadMimicryAngleSpeedAccelerate),
                    GetAttribute(AttributeName.kYOverloadMimicryAngleSpeedAccelerate),
                    GetAttribute(AttributeName.kZOverloadMimicryAngleSpeedAccelerate)
                ),
                MaxAngular = new Vector3(
                    GetAttribute(AttributeName.kXOverloadMimicryMaxAngle),
                    GetAttribute(AttributeName.kYOverloadMimicryMaxAngle),
                    GetAttribute(AttributeName.kZOverloadMimicryMaxAngle)
                )
            }
        };
	}

    float GetAttribute(AttributeName key)
    {
        return (float)(m_Property.GetAttribute(key));
    }
}