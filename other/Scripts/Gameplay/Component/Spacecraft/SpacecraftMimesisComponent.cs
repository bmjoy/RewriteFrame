using System;
using Assets.Scripts.Define;
using DebugPanel;
using UnityEngine;

public interface ISpacecraftMimesisComponentProperty
{
    T AddComponent<T>() where T : Component;
    Transform GetSkinRootTransform();
    Vector3 GetRotateAxis();
    Vector3 GetEngineAxis();
    MotionType GetMotionType();
    double GetAttribute(AttributeName key);
    Vector3 GetMouseDelta();
    float GeMimesisRollMaxAngles4Dof();
    float GetMimesisYMaxAngles4Dof();
    float GetMimesisXDownMaxAngles4Dof();
    float GetMimesisXUpMaxAngles4Dof();
    float GeMimesisRollTimeScale4Dof();
	float GetMimesisMaximumAngle6Dof();
	Vector3 GetMouseOffset();
    float GetDof6LookAtZ();
	bool GetIsRightStick();
    bool IsLeap();
    HeroState GetCurrentState();
}

/// <summary>
/// 船形态拟态逻辑
/// </summary>
public class SpacecraftMimesisComponent : EntityComponent<ISpacecraftMimesisComponentProperty>
{
    private ISpacecraftMimesisComponentProperty m_SpacecraftMimesisComponentProperty;

    private pdPlane m_pdPlane;

    private bool isNew = true;

	private GameplayProxy m_GamePlayProxy;

	public override void OnInitialize(ISpacecraftMimesisComponentProperty property)
    {
        m_SpacecraftMimesisComponentProperty = property;
		m_GamePlayProxy = GameFacade.Instance.RetrieveProxy(ProxyName.GameplayProxy) as GameplayProxy;
		if (m_SpacecraftMimesisComponentProperty.GetMotionType() == MotionType.Mmo)
        {
            m_pdPlane = m_SpacecraftMimesisComponentProperty.AddComponent<pdPlane>();
            m_pdPlane.Init(
                m_SpacecraftMimesisComponentProperty.GetRotateAxis,
                m_SpacecraftMimesisComponentProperty.GetEngineAxis,
                m_SpacecraftMimesisComponentProperty.GetSkinRootTransform()
            );

            m_pdPlane.ChangeMotionType(MotionType.Mmo);
        }
    }

    public override void OnAddListener()
    {
        if (m_SpacecraftMimesisComponentProperty.GetMotionType() == MotionType.Mmo)
        {
            AddListener(ComponentEventName.RefreshSpacecraftMimesisData, OnRefreshSpacecraftMimesisData);
        }
    }

	public override void OnUpdate(float delta)
    {
		if (m_SpacecraftMimesisComponentProperty.GetCurrentState().IsHasSubState(EnumSubState.Leaping))
		{
			return;
		}

		Vector3 targetEulerAngles = new Vector3();
        if (m_SpacecraftMimesisComponentProperty.GetMotionType() == MotionType.Dof4)
        {
            float engineX = m_SpacecraftMimesisComponentProperty.GetEngineAxis().x;
            float mouseDeltaX = Mathf.Clamp(m_SpacecraftMimesisComponentProperty.GetMouseDelta().x, -1f, 1f);
            float mouseDeltaY = Mathf.Clamp(m_SpacecraftMimesisComponentProperty.GetMouseDelta().y, -1f, 1f);
            float x = -(engineX + mouseDeltaX);
            float y = mouseDeltaY;

			targetEulerAngles = Vector3.zero;
			float angle = m_SpacecraftMimesisComponentProperty.GeMimesisRollMaxAngles4Dof();
			if (m_SpacecraftMimesisComponentProperty.GetIsRightStick())
			{
				if (mouseDeltaY > 0)
				{
					targetEulerAngles.x = -y * m_SpacecraftMimesisComponentProperty.GetMimesisXDownMaxAngles4Dof();
				}
				else
				{
					targetEulerAngles.x = -y * m_SpacecraftMimesisComponentProperty.GetMimesisXUpMaxAngles4Dof();
				}
				targetEulerAngles.y = mouseDeltaX * m_SpacecraftMimesisComponentProperty.GetMimesisYMaxAngles4Dof();
			}
			targetEulerAngles.z = x * angle;

            //Debug.LogError($"GetMouseDelta = {m_SpacecraftMimesisComponentProperty.GetMouseDelta()} EngineAxis = {m_SpacecraftMimesisComponentProperty.GetEngineAxis()}");
        }
        else if (m_SpacecraftMimesisComponentProperty.GetMotionType() == MotionType.Dof6)
        {
            Vector3 mouseOffset = m_SpacecraftMimesisComponentProperty.GetMouseOffset();
            mouseOffset.z = m_SpacecraftMimesisComponentProperty.GetDof6LookAtZ();

            /// 第一步计算船头朝向
            Vector3 lookAtPoint = m_SpacecraftMimesisComponentProperty.GetSkinRootTransform().localPosition + mouseOffset;
            Quaternion lookAtQuaternion = new Quaternion();
            lookAtQuaternion.SetLookRotation(lookAtPoint);
            Vector3 lookAtEulerAngles = lookAtQuaternion.eulerAngles;

            /// 第二步计算绕z周旋转角度
            lookAtEulerAngles.z = Mathf.Clamp(mouseOffset.x / (Screen.width / 2), -1, 1) * (-m_SpacecraftMimesisComponentProperty.GetMimesisMaximumAngle6Dof());

            targetEulerAngles = lookAtEulerAngles;
        }
        
        m_SpacecraftMimesisComponentProperty.GetSkinRootTransform().localRotation =
            Quaternion.Lerp(m_SpacecraftMimesisComponentProperty.GetSkinRootTransform().localRotation,
            Quaternion.Euler(targetEulerAngles),
            Time.deltaTime * m_SpacecraftMimesisComponentProperty.GeMimesisRollTimeScale4Dof()
            );

        if (m_pdPlane != null)
        {
            m_pdPlane.DoUpdate(delta);
        }
    }

    private void OnRefreshSpacecraftMimesisData(IComponentEvent obj)
    {
        RefreshSpacecraftMimesisDataEvent refreshSpacecraftMimesisDataEvent = obj as RefreshSpacecraftMimesisDataEvent;
        SpacecraftMotionInfo.MimesisInfo MimesisData = refreshSpacecraftMimesisDataEvent.MimesisData;

        pdPlaneTweakableProperties jetProerties = new pdPlaneTweakableProperties();

        jetProerties.PitchSteering.MaxDummyAcceleration = MimesisData.AngularAcceleration.x;
        jetProerties.PitchSteering.MaxDummyAngular = MimesisData.MaxAngular.x;

        jetProerties.YawSteering.MaxDummyAcceleration = MimesisData.AngularAcceleration.y;
        jetProerties.YawSteering.MaxDummyAngular = MimesisData.MaxAngular.y;

        jetProerties.RollSteering.MaxDummyAcceleration = MimesisData.AngularAcceleration.z;
        jetProerties.RollSteering.MaxDummyAngular = MimesisData.MaxAngular.z;

        m_pdPlane.SetTweakableProerties(jetProerties);
    }

    public override void DoGUI(Config config)
    {
        base.DoGUI(config);

        m_pdPlane.DoGUI(config);
    }
}