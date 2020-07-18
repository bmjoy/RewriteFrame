using System;
using UnityEngine;

public interface ISpacecraftCinemachineProperty
{
    Transform GetRootTransform();

	Transform GetVirtualCameraTransform();

	Vector3 GetEngineAxis();

    Vector3 GetRotateAxis();

	Vector2 GetDefaultCMAxisValue();

	EnumMotionMode GetMotionMode();

	MotionType GetMotionType();
}

/// <summary>
/// 船形态摄像机机位逻辑
/// </summary>
public class SpacecraftCinemachineComponent : EntityComponent<ISpacecraftCinemachineProperty>
{
	private ISpacecraftCinemachineProperty m_Property;

	public override void OnInitialize(ISpacecraftCinemachineProperty property)
    {
        m_Property = property;
        InitCamera();
		OnChangeMotionType(null);
	}

    private void InitCamera()
    {
        Transform transform = m_Property.GetRootTransform();
		Transform virtualTransform = m_Property.GetVirtualCameraTransform();
		Vector2 axis = m_Property.GetDefaultCMAxisValue();
        axis.x += transform.localEulerAngles.y;
        MainCameraComponent mainCameraComponent = CameraManager.GetInstance().GetMainCamereComponent();
        mainCameraComponent.SetFollowAndLookAtCMFreeLookAxisValue(MainCameraComponent.CMType.Spacecraft, transform, transform, axis.x, axis.y);
		mainCameraComponent.SetFollowAndLookAt(MainCameraComponent.CMType.Jet, virtualTransform, virtualTransform);
		mainCameraComponent.SetFollowAndLookAt(MainCameraComponent.CMType.JetSpeedUp, virtualTransform, virtualTransform);
        mainCameraComponent.SetFollowAndLookAt(MainCameraComponent.CMType.LeapPrepare, transform, transform);
        mainCameraComponent.SetFollowAndLookAt(MainCameraComponent.CMType.Leaping, transform, transform);
        mainCameraComponent.SetFollowAndLookAt(MainCameraComponent.CMType.LeapFinish, transform, transform);
        mainCameraComponent.SetFollowAndLookAtCMFreeLookAxisValue(MainCameraComponent.CMType.TransferIn, transform, transform, axis.x, axis.y);
    }

    public override void OnAddListener()
    {
        AddListener(ComponentEventName.ChangeMotionType, OnChangeMotionType);
        AddListener(ComponentEventName.Relive, OnRelive);
		//AddListener(ComponentEventName.SpacecraftLeapEnd, OnSpacecraftLeapEnd);
#if SPACECRAFT_4DOF_FIX_CAMERA
        AddListener(ComponentEventName.ChangeSpacecraftInputState, OnChangeSpacecraftInputState);
#endif
	}

	//private void OnSpacecraftLeapEnd(IComponentEvent obj)
	//{
		//MainCameraComponent mainCameraComponent = CameraManager.GetInstance().GetMainCamereComponent();
		//mainCameraComponent.RequestChangeCM(MainCameraComponent.CMType.LeapFinish);
		//if (m_Property.GetMotionMode() == EnumMotionMode.Dof6ReplaceOverload)
		//{
		//	mainCameraComponent.RequestChangeCM(MainCameraComponent.CMType.Jet);
		//}
		//else
		//{
		//	mainCameraComponent.RequestChangeCM(MainCameraComponent.CMType.Spacecraft);
		//}
	//}

	private void OnRelive(IComponentEvent obj)
    {
        InitCamera();
    }

#if SPACECRAFT_4DOF_FIX_CAMERA
    private void OnChangeSpacecraftInputState(IComponentEvent obj)
    {
        Vector3 engineAxis = m_Property.GetEngineAxis();
        Vector3 rotationAxis = m_Property.GetRotateAxis();

        if (engineAxis == Vector3.zero && rotationAxis == Vector3.zero)
        {
            CameraManager.GetInstance().GetMainCamereComponent().ChangeCM(MainCameraComponent.CMType.Spacecraft);
        }
        else
        {
            CameraManager.GetInstance().GetMainCamereComponent().ChangeCM(MainCameraComponent.CMType.Jet);
        }
    }
#endif

	private void OnChangeMotionType(IComponentEvent componentEvent)
    {
		MotionType motionType = m_Property.GetMotionType();
		MainCameraComponent mainCameraComponent = CameraManager.GetInstance().GetMainCamereComponent();
		switch (motionType)
		{
			case MotionType.Mmo:
				if (mainCameraComponent.GetLastCMType() != MainCameraComponent.CMType.Spacecraft
					&& !mainCameraComponent.HasInChanngingCMs(MainCameraComponent.CMType.Spacecraft))
				{
					mainCameraComponent.RequestChangeCM(MainCameraComponent.CMType.Spacecraft);
				}
				break;
			case MotionType.Dof4:
				if (mainCameraComponent.GetLastCMType() != MainCameraComponent.CMType.Jet
					&& !mainCameraComponent.HasInChanngingCMs(MainCameraComponent.CMType.Jet))
				{
					mainCameraComponent.ForceChangeCM(MainCameraComponent.CMType.Jet);
				}
				break;
			case MotionType.Dof6:
				if (mainCameraComponent.GetLastCMType() != MainCameraComponent.CMType.JetSpeedUp
					&& !mainCameraComponent.HasInChanngingCMs(MainCameraComponent.CMType.JetSpeedUp))
				{
					mainCameraComponent.ForceChangeCM(MainCameraComponent.CMType.JetSpeedUp);
				}
				break;
			default:
                break;
        }
	}
}
