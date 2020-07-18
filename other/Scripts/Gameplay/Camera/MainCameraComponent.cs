using Cinemachine;
using DebugPanel;
using Leyoutech.Core.Loader.Config;
using Eternity.FlatBuffer;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.PostProcessing;
using Track;
using static InputManager;

public class MainCameraComponent : BaseCameraComponent
	, Track.ITrackObject
{
	private const string CM_GAMEOBJECT_NAME_STARTWITHS = "CM_";
	private readonly string[] ANTIALIASING_TO_DISPLAY = new string[] { "None", "FXAA", "SMAA", "TAA" };

#pragma warning disable 0649
	[SerializeField]
	private MainCameraTransitionBlenderSettings m_MainCameraTransitionBlenderSettings;
	[SerializeField]
	private CRenderer.MainCameraRenderer.Resource m_CameraRendererResource;
#pragma warning restore 0649

	public ForDebug _ForDebug;

	/// <summary>
	/// (from CM, to CM)
	/// </summary>
	internal Action<CMType, CMType> _OnCMChanged;

	private CinemachineBrain m_MainCMBrain;
	private CMData[] m_CMs;
	private PostProcessLayer m_PostProcessLayer;
	private HxVolumetricCamera m_HXVCamera;

	private CMType m_LastCMType;

	/// <summary>
	/// 人形态相机的Axis参数
	/// </summary>
	private CMAxisSetting m_CM_Character_AxisSetting;
	/// <summary>
	/// 船形态相机的Axis参数
	/// </summary>
	private CMAxisSetting m_CM_Spacecraft_AxisSetting;

	/// <summary>
	/// 当前请求切换的CM队列
	/// <see cref="RequestChangeCM(CMType)"/>
	/// </summary>
	private Queue<CMType> m_ChangingCMs;
	/// <summary>
	/// 上一次切换CM的插值剩余时间
	/// <see cref="RequestChangeCM(CMType)"/>
	/// </summary>
	private float m_ChangingCMBlendReaminTime;

	private CRenderer.MainCameraRenderer m_CameraRenderer;

	public override void Initialize()
	{
		base.Initialize();

		m_UpdateProperties = UpdatePropertyFlag.HalfTanFov
			| UpdatePropertyFlag.Transform;

		m_MainCMBrain = gameObject.GetComponent<CinemachineBrain>();
		m_PostProcessLayer = gameObject.GetComponent<PostProcessLayer>();
		m_HXVCamera = gameObject.GetComponent<HxVolumetricCamera>();

		m_LastCMType = CMType.Notset;
		m_CMs = new CMData[(int)CMType.Count];
		Transform cmRoot = transform.parent;
		for (int iCM = 0; iCM < m_CMs.Length; iCM++)
		{
			CMData cmData = new CMData(cmRoot.Find(CM_GAMEOBJECT_NAME_STARTWITHS + ((CMType)iCM).ToString())
					.GetComponent<CinemachineVirtualCameraBase>()
				, (CMType)iCM);
			m_CMs[iCM] = cmData;
		}

#if UNITY_EDITOR
		// 预览场景时，不需要读表里的相机参数，也不需要切换手柄鼠标
		if (!CameraManager._s_IsRoaming)
#endif
		{
			GamingConfig gamingConfig = ((CfgEternityProxy)GameFacade.Instance.RetrieveProxy(ProxyName.CfgEternityProxy)).GetGamingConfig(1).Value;
			m_CM_Character_AxisSetting = new CMAxisSetting(new CMAxisSetting.CM(new CMAxisSetting.Axis(gamingConfig.CmCharacter.Value.XaxisMaxSpeedMouse)
					, new CMAxisSetting.Axis(gamingConfig.CmCharacter.Value.YaxisMaxSpeedMouse))
				, new CMAxisSetting.CM(new CMAxisSetting.Axis(gamingConfig.CmCharacter.Value.XaxisMaxSpeedGamepad)
					, new CMAxisSetting.Axis(gamingConfig.CmCharacter.Value.YaxisMaxSpeedGamepad)));
			InputManager.Instance.OnInputDeviceChanged += OnInputDeviceChanged;
		}

		m_ChangingCMs = new Queue<CMType>();
		m_ChangingCMBlendReaminTime = 0;

		m_CameraRenderer = new CRenderer.MainCameraRenderer();
		m_CameraRenderer.Initialize(GetCameraName(), m_Camera, m_CameraRendererResource);

		_ForDebug.GUIEnable = new bool[(int)GUIType.Count];

		TrackCaptureManager.GetInstance().InitializeCapturer("Main Camera", this).StartCapture();
	}

	public override string GetCameraName()
	{
		return "MainCamera";
	}

	public PostProcessLayer GetPostProcessLayer()
	{
		return m_PostProcessLayer;
	}

	public CMType GetLastCMType()
	{
		return m_LastCMType;
	}

	/// <summary>
	/// cmType是否再请求切换的队列里
	/// </summary>
	public bool HasInChanngingCMs(CMType cmType)
	{
		return m_ChangingCMs.Contains(cmType);
	}

	/// <summary>
	/// 不是及时生效的，目的是：
	///		避免出现同时三个相机在插值
	///		让上一次插值完成后再切换下一个相机
	/// </summary>
	public void RequestChangeCM(CMType cmType)
	{
		Leyoutech.Utility.DebugUtility.Log(CameraManager.LOG_TAG
			, string.Format("Request Change CM {0}"
				, cmType));

		m_ChangingCMs.Enqueue(cmType);
	}

	/// <summary>
	/// 强制切换机位
	/// 某些特殊情况下使用，可能会出现莫名其妙的问题
	/// </summary>
	public void ForceChangeCM(CMType cmType)
	{
		Leyoutech.Utility.DebugUtility.Log(CameraManager.LOG_TAG
			, string.Format("Force Change CM {0}"
				, cmType));
		m_ChangingCMs.Clear();
		m_ChangingCMs.Enqueue(cmType);
		m_ChangingCMBlendReaminTime = 0.0f;
		DoLateUpdate_ChangeCM(1.0f / 60.0f);
	}

	public CinemachineVirtualCameraBase GetCM_VirtualCameraBase(CMType cmType)
	{
		return m_CMs[(int)cmType].GetCM_VirtualCameraBase();
	}

	public CinemachineVirtualCamera GetCM_VirtualCamera(CMType cmType)
	{
		return m_CMs[(int)cmType].GetCM_VirtualCamera();
	}

	public CinemachineFreeLook GetCM_FreeLook(CMType cmType)
	{
		return m_CMs[(int)cmType].GetCM_FreeLook();
	}

	/// <summary>
	/// 只有这个<see cref="CinemachineFreeLook"/>能用
	/// </summary>
	public void SetFollowAndLookAtCMFreeLookAxisValue(CMType cmType, Transform follow, Transform lookAt, float valueX, float valueY)
	{
		Leyoutech.Utility.DebugUtility.Log(CameraManager.LOG_TAG
			, string.Format("Set CM({0}) Follow:({1}) LookAt:({2}) AxisValue:({3:F2}, {4:F2})"
				, cmType
				, follow != null
					? follow.name
					: "None"
				, lookAt != null
					? lookAt.name
					: "None"
				, valueX
				, valueY));

		CinemachineFreeLook cm = GetCM_FreeLook(cmType);
		cm.Follow = follow;
		cm.LookAt = lookAt;
		cm.m_XAxis.Value = valueX;
		cm.m_YAxis.Value = valueY;
	}

	public void SetFollowAndLookAt(CMType cmType, Transform follow, Transform lookAt)
	{
		Leyoutech.Utility.DebugUtility.Log(CameraManager.LOG_TAG
			, string.Format("Set CM({0}) Follow:({1}) LookAt:({2})"
				, cmType
				, follow != null
					? follow.name
					: "None"
				, lookAt != null
					? lookAt.name
					: "None"));

		CinemachineVirtualCameraBase cm = GetCM_VirtualCameraBase(cmType);
		cm.Follow = follow;
		cm.LookAt = lookAt;
	}

	public void SetLookAt(CMType cmType, Transform lookAt)
	{
		Leyoutech.Utility.DebugUtility.Log(CameraManager.LOG_TAG
			, string.Format("Set CM({0}) LookAt:({1})"
				, cmType
				, lookAt != null
					? lookAt.name
					: "None"));

		CinemachineVirtualCameraBase cm = GetCM_VirtualCameraBase(cmType);
		cm.LookAt = lookAt;
	}

	/// <summary>
	/// 只有这个<see cref="CinemachineFreeLook"/>能用
	/// </summary>
	public void SetCMFreeLookAxisValue(CMType cmType, float valueX, float valueY)
	{
		Leyoutech.Utility.DebugUtility.Log(CameraManager.LOG_TAG
			, string.Format("Set CM({0}) AxisValue:({1:F2}, {2:F2})"
				, cmType
				, valueX
				, valueY));

		CinemachineFreeLook cm = GetCM_FreeLook(cmType);
		cm.m_XAxis.Value = valueX;
		cm.m_YAxis.Value = valueY;
	}

	public void SetNearClipPlane(CMType cmType, float near)
	{
		CMData cm = m_CMs[(int)cmType];
		switch (cm.GetCMCameraType())
		{
			case CMCameraType.FreeLook:
				cm.GetCM_FreeLook().m_Lens.NearClipPlane = near;
				break;
			case CMCameraType.VirtualCamera:
				cm.GetCM_VirtualCamera().m_Lens.NearClipPlane = near;
				break;
			default:
				Leyoutech.Utility.DebugUtility.Assert(false, CameraManager.LOG_TAG, $"Not support CameraType({cm.GetCMCameraType()})");
				break;
		}
	}

	public void SetFarClipPlane(CMType cmType, float far)
	{
		CMData cm = m_CMs[(int)cmType];
		switch (cm.GetCMCameraType())
		{
			case CMCameraType.FreeLook:
				cm.GetCM_FreeLook().m_Lens.FarClipPlane = far;
				break;
			case CMCameraType.VirtualCamera:
				cm.GetCM_VirtualCamera().m_Lens.FarClipPlane = far;
				break;
			default:
				Leyoutech.Utility.DebugUtility.Assert(false, CameraManager.LOG_TAG, $"Not support CameraType({cm.GetCMCameraType()})");
				break;
		}
	}

    public void SetTrackedObjectOffset(CMType cmType, Vector3 trackedObjectOffset)
    {
        CinemachineVirtualCamera cm = GetCM_VirtualCamera(cmType);
        CinemachineComposer aimComponent = cm.GetCinemachineComponent(CinemachineCore.Stage.Aim) as CinemachineComposer;
        aimComponent.m_TrackedObjectOffset = trackedObjectOffset;
    }

    /// <summary>
    /// 只有这个<see cref="CinemachineFreeLook"/>能用
    /// </summary>
    public Vector2 GetCMFreeLookAxisMaxSpeed(CMType cmType)
	{
		CinemachineFreeLook cm = GetCM_FreeLook(cmType);
		return new Vector2(cm.m_XAxis.m_MaxSpeed, cm.m_YAxis.m_MaxSpeed);
	}

	/// <summary>
	/// 只有这个<see cref="CinemachineFreeLook"/>能用
	/// </summary>
	public void SetCMFreeLookAxisMaxSpeed(CMType cmType, float maxSpeedX, float maxSpeedY)
	{
		Leyoutech.Utility.DebugUtility.Log(CameraManager.LOG_TAG
			, string.Format("Set CM({0}) AxisMaxSpeed:({1:F2}, {2:F2})"
				, cmType
				, maxSpeedX
				, maxSpeedY));

		CinemachineFreeLook cm = GetCM_FreeLook(cmType);
		cm.m_XAxis.m_MaxSpeed = maxSpeedX;
		cm.m_YAxis.m_MaxSpeed = maxSpeedY;
	}

	public void ApplyCMDofParam(MotionType type, Vector3 followOffset, Vector3 trackedObjectOffset, float cmFieldOfView, float pitchDamping, float yawDamping, float rollDamping, float screenY)
	{
		CMType cmType = CMType.Notset;
		if (type == MotionType.Dof4)
		{
			cmType = CMType.Jet;
		}
		else if (type == MotionType.Dof6)
		{
			cmType = CMType.JetSpeedUp;
		}

		if (cmType != CMType.Notset)
		{
			CinemachineVirtualCamera cm = GetCM_VirtualCamera(cmType);
			CinemachineTransposer bodyComponent = cm.GetCinemachineComponent(CinemachineCore.Stage.Body) as CinemachineTransposer;
			bodyComponent.m_FollowOffset = followOffset;
			bodyComponent.m_PitchDamping = pitchDamping;
			bodyComponent.m_YawDamping = yawDamping;
			bodyComponent.m_RollDamping = rollDamping;
			cm.m_Lens.FieldOfView = cmFieldOfView;

			CinemachineComposer aimComponent = cm.GetCinemachineComponent(CinemachineCore.Stage.Aim) as CinemachineComposer;
			aimComponent.m_TrackedObjectOffset = trackedObjectOffset;
			aimComponent.m_ScreenY = screenY;
		}
	}

	public void ApplyCMParam(WarshipCamera cameraParam)
	{
		ApplyCMParame_FreeLook(CMType.Spacecraft
			, cameraParam.CMSpacecraftTopRigHeight
			, cameraParam.CMSpacecraftTopRigRadius
			, cameraParam.CMSpacecraftMiddleRigHeight
			, cameraParam.CMSpacecraftMiddleRigRadius
			, cameraParam.CMSpacecraftBottomRigHeight
			, cameraParam.CMSpacecraftBottomRigRadius
			, cameraParam.CMSpacecraftFieldOfView);
		m_CM_Spacecraft_AxisSetting = new CMAxisSetting(new CMAxisSetting.CM(new CMAxisSetting.Axis(cameraParam.CMSpacecraftXaxisMaxSpeedMouse)
				, new CMAxisSetting.Axis(cameraParam.CMSpacecraftYaxisMaxSpeedMouse))
			, new CMAxisSetting.CM(new CMAxisSetting.Axis(cameraParam.CMSpacecraftXaxisMaxSpeedGamepad)
				, new CMAxisSetting.Axis(cameraParam.CMSpacecraftYaxisMaxSpeedGamepad)));

		ApplyCMParame_VirtualCamera(CMType.LeapPrepare
			, cameraParam.CMLeapPrepareBodyFollowOffsetX
			, cameraParam.CMLeapPrepareBodyFollowOffsetY
			, cameraParam.CMLeapPrepareBodyFollowOffsetZ
			, cameraParam.CMLeapPrepareFieldOfView);

		ApplyCMParame_VirtualCamera(CMType.Leaping
			, cameraParam.CMLeapingBodyFollowOffsetX
			, cameraParam.CMLeapingBodyFollowOffsetY
			, cameraParam.CMLeapingBodyFollowOffsetZ
			, cameraParam.CMLeapingFieldOfView);

		ApplyCMParame_VirtualCamera(CMType.LeapFinish
			, cameraParam.CMLeapFinishBodyFollowOffsetX
			, cameraParam.CMLeapFinishBodyFollowOffsetY
			, cameraParam.CMLeapFinishBodyFollowOffsetZ
			, cameraParam.CMLeapFinishFieldOfView);

		//ApplyCMParame_VirtualCamera(CMType.Jet
		//	, cameraParam.CMLeapPrepareBodyFollowOffsetX
		//	, cameraParam.CMLeapPrepareBodyFollowOffsetY
		//	, cameraParam.CMLeapPrepareBodyFollowOffsetZ
		//	, cameraParam.CMLeapPrepareFieldOfView);

		OnInputDeviceChanged(InputManager.Instance.CurrentInputDevice);
	}

	/// <summary>
	/// <see cref="CinemachineVirtualCameraBase.OnTargetObjectWarped(Transform, Vector3)"/>
	/// 扭曲所有Enable的CM
	/// </summary>
	public void TargetObjectWarpedAllCM(Vector3 positionDelta)
	{
		Leyoutech.Utility.DebugUtility.Log(CameraManager.LOG_TAG
			, "Target Object Warped All CM positionDelta: " + positionDelta);
		for (int iCM = 0; iCM < m_CMs.Length; iCM++)
		{
			m_CMs[iCM].TargetObjectWarped(positionDelta);
		}
	}

	protected override void DoGUIOverride(Config config)
	{
		config.BeginToolbarHorizontal();
		for (int iGUIType = 0; iGUIType < (int)GUIType.Count; iGUIType++)
		{
			_ForDebug.GUIEnable[iGUIType] = config.ToolbarToggle(_ForDebug.GUIEnable[iGUIType], ((GUIType)iGUIType).ToString());
		}
		_ForDebug.DisplayDetail = config.ToolbarToggle(_ForDebug.DisplayDetail, "Display Detail");
#if UNITY_EDITOR
		if (config.IsEditor
			&& config.ToolbarButton(false, "Save Prefab"))
		{
			_SaveSelfToPrefab();
		}
#endif
		config.EndHorizontal();

		if (_ForDebug.GUIEnable[(int)GUIType.Camera])
		{
			GUILayout.Label("Camera:", config.ImportantLabelStyle);
			DoGUICamera(config);
		}
		if (_ForDebug.GUIEnable[(int)GUIType.Renderer])
		{
			GUILayout.Label("Renderer:", config.ImportantLabelStyle);
			m_CameraRenderer.DoGUI(config, _ForDebug.DisplayDetail);
		}
		if (_ForDebug.GUIEnable[(int)GUIType.CM])
		{
			GUILayout.Label("CM:", config.ImportantLabelStyle);
			DoGUICM(config);
		}
	}

	protected override void DoLateUpdate(float deltaTime)
	{
		DoLateUpdate_ChangeCM(deltaTime);

		if (UIManager.Instance.Aspect > 0)
		{
			SetCameraAspectAndRect(UIManager.Instance.Aspect, UIManager.Instance.ViewportRect);
		}
	}

	protected void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		m_CameraRenderer.OnRenderImage(source, destination);
	}

	private void DoGUICamera(Config config)
	{
		#region Load CameraLens
		if (_ForDebug.CameraNearClipPlanes == null)
		{
			_ForDebug.CameraNearClipPlaneIndex = 0;
			_ForDebug.CameraFarClipPlaneIndex = 0;
			if (SettingINI.Setting.TryGetValue(SettingINI.Setting.CombineKey(SettingINI.Constants.GROUP_DEBUG
				, SettingINI.Constants.KEY_MAIN_CAMERA_LENS), out string cameraLenString))
			{
				string[] lens = cameraLenString.Split('|');
				List<float> temp = new List<float>();
				if (lens.Length > 0)
				{
					Leyoutech.Utility.StringUtility.SplitToFloatArray(temp, lens[0], ':');
					if (temp.Count > 0)
					{
						_ForDebug.CameraNearClipPlanes = temp.ToArray();
					}
				}
				temp.Clear();
				if (lens.Length > 1)
				{
					Leyoutech.Utility.StringUtility.SplitToFloatArray(temp, lens[1], ':');
					if (temp.Count > 0)
					{
						_ForDebug.CameraFarClipPlanes = temp.ToArray();
					}
				}
			}

			if (_ForDebug.CameraNearClipPlanes == null)
			{
				_ForDebug.CameraNearClipPlanes = new float[] { GetCamera().nearClipPlane };
			}
			if (_ForDebug.CameraFarClipPlanes == null)
			{
				_ForDebug.CameraFarClipPlanes = new float[] { GetCamera().farClipPlane };
			}
		}
		#endregion

		#region Camera
		config.BeginHorizontal();
		m_PostProcessLayer.enabled = config.ToolbarToggle(m_PostProcessLayer.enabled, "PostProcess");
		if (m_HXVCamera)
		{
			m_HXVCamera.enabled = config.ToolbarToggle(m_HXVCamera.enabled, "HXVCamera");
		}
		if (GUILayout.Button($"Near:{GetCamera().nearClipPlane}", config.ButtonStyle))
		{
			_ForDebug.CameraNearClipPlaneIndex = _ForDebug.CameraNearClipPlaneIndex + 1 >= _ForDebug.CameraNearClipPlanes.Length ? 0 : _ForDebug.CameraNearClipPlaneIndex + 1;

			for (int iCM = 0; iCM < m_CMs.Length; iCM++)
			{
				SetNearClipPlane((CMType)iCM, _ForDebug.CameraNearClipPlanes[_ForDebug.CameraNearClipPlaneIndex]);
			}
		}
		if (GUILayout.Button($"Far:{GetCamera().farClipPlane}", config.ButtonStyle))
		{
			_ForDebug.CameraFarClipPlaneIndex = _ForDebug.CameraFarClipPlaneIndex + 1 >= _ForDebug.CameraFarClipPlanes.Length ? 0 : _ForDebug.CameraFarClipPlaneIndex + 1;

			for (int iCM = 0; iCM < m_CMs.Length; iCM++)
			{
				SetFarClipPlane((CMType)iCM, _ForDebug.CameraFarClipPlanes[_ForDebug.CameraFarClipPlaneIndex]);
			}
		}
		GetCamera().fieldOfView = config.Slider("FOV: {0}", GetCamera().fieldOfView, 10f, 180f, 2.5f, true);
		config.EndHorizontal();
		#endregion

		#region PostProcessLayer
		config.BeginToolbarHorizontal();
		m_PostProcessLayer.antialiasingMode = config.EnumPopup(m_PostProcessLayer.antialiasingMode, "Anti", true);
		m_PostProcessLayer.fog.enabled = config.ToolbarToggle(m_PostProcessLayer.fog.enabled, "Fog enabled");
		m_PostProcessLayer.fog.excludeSkybox = config.ToolbarToggle(m_PostProcessLayer.fog.enabled, "Fog skybox");
		config.EndHorizontal();

		if (_ForDebug.DisplayDetail)
		{
			config.BeginToolbarHorizontal();
			GUILayout.Label("Anti:", config.ImportantLabelStyle);
			switch (m_PostProcessLayer.antialiasingMode)
			{
				case PostProcessLayer.Antialiasing.None:
					break;
				case PostProcessLayer.Antialiasing.FastApproximateAntialiasing:
					m_PostProcessLayer.fastApproximateAntialiasing.fastMode = config.ToolbarToggle(m_PostProcessLayer.fastApproximateAntialiasing.fastMode, "FastMode");
					m_PostProcessLayer.fastApproximateAntialiasing.keepAlpha = config.ToolbarToggle(m_PostProcessLayer.fastApproximateAntialiasing.keepAlpha, "KeepAlpha");
					break;
				case PostProcessLayer.Antialiasing.SubpixelMorphologicalAntialiasing:
					if (GUILayout.Button($"Quality:{m_PostProcessLayer.subpixelMorphologicalAntialiasing.quality}", config.ButtonStyle))
					{
						switch (m_PostProcessLayer.subpixelMorphologicalAntialiasing.quality)
						{
							case SubpixelMorphologicalAntialiasing.Quality.Low:
								m_PostProcessLayer.subpixelMorphologicalAntialiasing.quality = SubpixelMorphologicalAntialiasing.Quality.Medium;
								break;
							case SubpixelMorphologicalAntialiasing.Quality.Medium:
								m_PostProcessLayer.subpixelMorphologicalAntialiasing.quality = SubpixelMorphologicalAntialiasing.Quality.High;
								break;
							case SubpixelMorphologicalAntialiasing.Quality.High:
								m_PostProcessLayer.subpixelMorphologicalAntialiasing.quality = SubpixelMorphologicalAntialiasing.Quality.Low;
								break;
							default:
								Leyoutech.Utility.DebugUtility.Assert(false, CameraManager.LOG_TAG, $"Not support Quality({m_PostProcessLayer.subpixelMorphologicalAntialiasing.quality})");
								break;
						}
					}
					break;
				case PostProcessLayer.Antialiasing.TemporalAntialiasing:
					m_PostProcessLayer.temporalAntialiasing.jitterSpread = config.Slider("JitterSpread:{0:F2}", m_PostProcessLayer.temporalAntialiasing.jitterSpread, 0f, 1f, 0.05f, true);
					m_PostProcessLayer.temporalAntialiasing.stationaryBlending = config.Slider("StationaryBlending:{0:F2}", m_PostProcessLayer.temporalAntialiasing.stationaryBlending, 0f, 1f, 0.05f, true);
					m_PostProcessLayer.temporalAntialiasing.motionBlending = config.Slider("MotionBlending:{0:F2}", m_PostProcessLayer.temporalAntialiasing.motionBlending, 0f, 1f, 0.05f, true);
					m_PostProcessLayer.temporalAntialiasing.sharpness = config.Slider("Sharpness:{0:F2}", m_PostProcessLayer.temporalAntialiasing.sharpness, 0f, 3f, 0.05f, true);
					break;
				default:
					Leyoutech.Utility.DebugUtility.Assert(false, CameraManager.LOG_TAG, $"Not support Antialiasing({m_PostProcessLayer.antialiasingMode})");
					break;
			}
			config.EndHorizontal();
		}
		#endregion
	}

	private void DoGUICM(Config config)
	{
		config.BeginToolbarHorizontal();
		if (config.ToolbarButton(m_MainCMBrain.enabled, "Brain"))
		{
			m_MainCMBrain.enabled = !m_MainCMBrain.enabled;
		}
		for (int iCM = 0; iCM < m_CMs.Length; iCM++)
		{
			CMType cmType = (CMType)iCM;
			if (config.ToolbarButton(m_CMs[iCM].IsLive(), cmType.ToString()))
			{
				RequestChangeCM(cmType);
			}
		}
		config.EndHorizontal();

		if (!_ForDebug.DisplayDetail)
		{
			return;
		}

		for (int iCM = 0; iCM < m_CMs.Length; iCM++)
		{
			if (!m_CMs[iCM].IsLive())
			{
				continue;
			}

			CinemachineVirtualCameraBase iterCM = m_CMs[iCM].GetCM_VirtualCameraBase();
			GUILayout.Label(((CMType)iCM).ToString()
				, (int)m_LastCMType == iCM
					? config.ImportantLabelStyle
					: config.LabelStyle);

			GUILayout.Box(string.Format("Priority:{0} Follow:({1}) LookAt:({2})"
					, iterCM.Priority
					, iterCM.Follow != null
						? iterCM.Follow.name
						: "None"
					, iterCM.LookAt != null
						? iterCM.LookAt.name
						: "None")
				, config.BoxStyle);

			TransitionBlendDefinition transitionBlendDefinition = m_CMs[iCM].GetTransitionBlendDefinition();
			GUILayout.Box(string.Format("Transition: BlendHint({0}) InheritPosition({1})"
					, transitionBlendDefinition.BlendHint
					, transitionBlendDefinition.InheritPosition)
				, config.BoxStyle);

			if (iterCM.Follow)
			{
				Vector3 cameraToFollow = iterCM.Follow.position - GetCamera().transform.position;
				GUILayout.Box(string.Format("To Follow Distance:{0:F2} - Height:{1:F2} - Horizontal:{2:F2}"
						, cameraToFollow.magnitude
						, cameraToFollow.y
						, new Vector2(cameraToFollow.x, cameraToFollow.z).magnitude)
					, config.BoxStyle);
			}

			if (iterCM is CinemachineFreeLook)
			{
				CinemachineFreeLook iterFreeLookCM = iterCM as CinemachineFreeLook;
				GUILayout.Box(string.Format("Axis: Value({0}) MaxSpeed({1})"
						, new Vector2(iterFreeLookCM.m_XAxis.Value, iterFreeLookCM.m_YAxis.Value)
						, new Vector2(iterFreeLookCM.m_XAxis.m_MaxSpeed, iterFreeLookCM.m_YAxis.m_MaxSpeed))
					, config.BoxStyle);
			}
		}
	}

	/// <summary>
	/// <see cref="RequestChangeCM(CMType)"/>
	/// </summary>
	private void DoLateUpdate_ChangeCM(float deltaTime)
	{
		m_ChangingCMBlendReaminTime = Mathf.Max(0.0f, m_ChangingCMBlendReaminTime - deltaTime);
		if (m_ChangingCMBlendReaminTime > 0
			|| m_ChangingCMs.Count == 0
			|| CinemachineCore.Instance.BrainCount > 1)
		{
			return;
		}

		CMType cmType = m_ChangingCMs.Dequeue();
		if (m_LastCMType == cmType)
		{
			Leyoutech.Utility.DebugUtility.LogWarning(CameraManager.LOG_TAG, "Repeat change CM to " + cmType);
			return;
		}

		if (!m_MainCMBrain.enabled)
		{
			m_MainCMBrain.enabled = true;
		}

		CinemachineBlendDefinition cinemachineBlendDefinition = m_MainCMBrain.m_CustomBlends.GetBlendForVirtualCameras(m_LastCMType.ToString(), cmType.ToString(), m_MainCMBrain.m_DefaultBlend);
		Leyoutech.Utility.DebugUtility.Log(CameraManager.LOG_TAG
			, string.Format("Change CM form {0} to {1} use blend definition Style:{2} Time:{3}"
				, m_LastCMType
				, cmType
				, cinemachineBlendDefinition.m_Style
				, cinemachineBlendDefinition.m_Time));

		try
		{
			Leyoutech.Utility.DebugUtility.Log(CameraManager.LOG_TAG, "Begin invoke CM Changed");
			_OnCMChanged?.Invoke(m_LastCMType, cmType);
			Leyoutech.Utility.DebugUtility.Log(CameraManager.LOG_TAG, "End invoke CM Changed");
		}
		catch (Exception e)
		{
			Leyoutech.Utility.DebugUtility.LogError(CameraManager.LOG_TAG, "Invoke CM Changed Exception:\n" + e.ToString());
		}

		m_ChangingCMBlendReaminTime = cinemachineBlendDefinition.m_Time;

		TransitionBlendDefinition transitionBlendDefinition = m_MainCameraTransitionBlenderSettings.FindBlend(m_LastCMType.ToString(), cmType.ToString());
		m_CMs[(int)cmType].ApplyTransitionBlendDefinition(transitionBlendDefinition);
		m_CMs[(int)cmType].SetEnable(true);

		if (m_LastCMType != CMType.Notset)
		{
			m_CMs[(int)m_LastCMType].SetEnable(false);
		}

		m_LastCMType = cmType;
	}

	/// <summary>
	/// 应用VirtualCamera的相机参数
	/// TODO 目前所有<see cref="CinemachineVirtualCamera"/>的<see cref="CinemachineCore.Stage.Body"/>都是<see cref="CinemachineTransposer"/>。如果以后不是这样，就要修改这里的逻辑
	/// </summary>
	private void ApplyCMParame_VirtualCamera(CMType cmType
		, float cmBodyFollowOffsetX
		, float cmBodyFollowOffsetY
		, float cmBodyFollowOffsetZ
		, float cmFieldOfView)
	{
		CinemachineVirtualCamera cm = GetCM_VirtualCamera(cmType);
		CinemachineTransposer bodyComponent = cm.GetCinemachineComponent(CinemachineCore.Stage.Body) as CinemachineTransposer;
		bodyComponent.m_FollowOffset = new Vector3(cmBodyFollowOffsetX
			, cmBodyFollowOffsetY
			, cmBodyFollowOffsetZ);
		cm.m_Lens.FieldOfView = cmFieldOfView;
	}

	/// <summary>
	/// 应用FreeLook的相机参数
	/// </summary>
	private void ApplyCMParame_FreeLook(CMType cmType
		, float cmTopRigHeight
		, float cmTopRigRadius
		, float cmMiddleRigHeight
		, float cmMiddleRigRadius
		, float cmBottomRigHeight
		, float cmBottomRigRadius
		, float cmFieldOfView)
	{
		CinemachineFreeLook cm = GetCM_FreeLook(cmType);
		cm.m_Orbits[0] = new CinemachineFreeLook.Orbit(cmTopRigHeight, cmTopRigRadius);
		cm.m_Orbits[1] = new CinemachineFreeLook.Orbit(cmMiddleRigHeight, cmMiddleRigRadius);
		cm.m_Orbits[2] = new CinemachineFreeLook.Orbit(cmBottomRigHeight, cmBottomRigRadius);
		cm.m_Lens.FieldOfView = cmFieldOfView;
	}

	/// <summary>
	/// 输入设备切换的回调，用于处理：
	///		切换手柄和鼠标对相机的参数
	/// </summary>
	private void OnInputDeviceChanged(GameInputDevice inputDevice)
	{
		if (inputDevice == GameInputDevice.KeyboardAndMouse)
		{
			ApplyCMAxisSetting(CMType.Character, m_CM_Character_AxisSetting.Mouse);
			ApplyCMAxisSetting(CMType.Spacecraft, m_CM_Spacecraft_AxisSetting.Mouse);
		}
		else
		{
			ApplyCMAxisSetting(CMType.Character, m_CM_Character_AxisSetting.Gamepad);
			ApplyCMAxisSetting(CMType.Spacecraft, m_CM_Spacecraft_AxisSetting.Gamepad);
		}
	}

	private void ApplyCMAxisSetting(CMType cmType
		, CMAxisSetting.CM axisSetting)
	{
		CinemachineFreeLook cm = GetCM_FreeLook(cmType);
		cm.m_XAxis.m_MaxSpeed = axisSetting.X.MaxSpeed;
		cm.m_YAxis.m_MaxSpeed = axisSetting.Y.MaxSpeed;
	}

	string ITrackObject.GetUserData()
	{
		return string.Empty;
	}

#if UNITY_EDITOR
	private void _SaveSelfToPrefab()
	{
		AssetAddressConfig config = UnityEditor.AssetDatabase.LoadAssetAtPath<AssetAddressConfig>(AssetAddressConfig.CONFIG_PATH);
		if (config == null)
		{
			UnityEditor.EditorUtility.DisplayDialog("Save Camera Main Prefab", string.Format("Not found address({0})", AssetAddressConfig.CONFIG_PATH), "Ok");
			return;
		}
		string assetPath = "";
		foreach (var data in config.AddressDatas)
		{
			if (data.AssetAddress == AssetAddressKey.CAMERA_CAMERA_MAIN)
			{
				assetPath = data.AssetPath;
				break;
			}
		}
		if (string.IsNullOrEmpty(assetPath))
		{
			UnityEditor.EditorUtility.DisplayDialog("Save Camera Main Prefab", string.Format("Not found address({0})", AssetAddressKey.CAMERA_CAMERA_MAIN), "Ok");
			return;
		}
		UnityEditor.PrefabUtility.SaveAsPrefabAsset(m_Transform.parent.gameObject, assetPath);

		UnityEditor.EditorUtility.DisplayDialog("Save Camera Main Prefab", "Camera main prefab saved", "Ok");
	}
#endif

	public enum CMType : byte
	{
		Character,
		Spacecraft,
		Jet,
		JetSpeedUp,
		LeapPrepare,
		Leaping,
		LeapFinish,
		TransferIn,
		/// <summary>
		/// 必须放在最后
		/// </summary>
		Count,

		Notset,
	}

	public enum CMCameraType : byte
	{
		FreeLook,
		VirtualCamera,

		Unknown,
	}

	private enum GUIType : byte
	{
		Camera = 0,
		Renderer,
		CM,
		Count,
	}

	[StructLayout(LayoutKind.Explicit)]
	private struct CMData
	{
		[FieldOffset(0)]
		private readonly CinemachineVirtualCameraBase m_CM_VirtualCameraBase;

		[FieldOffset(8)]
		private readonly CinemachineVirtualCamera m_CM_VirtualCamera;
		[FieldOffset(8)]
		private readonly CinemachineFreeLook m_CM_FreeLook;

		[FieldOffset(16)]
		private readonly CMType m_CMType;
		[FieldOffset(17)]
		private readonly CMCameraType m_CMCameraType;

		public void SetEnable(bool enable, bool moveToTopOfPrioritySubqueue = true)
		{
			m_CM_VirtualCameraBase.enabled = enable;

			if (enable
				&& moveToTopOfPrioritySubqueue)
			{
				m_CM_VirtualCameraBase.MoveToTopOfPrioritySubqueue();
			}
		}

		public CMData(CinemachineVirtualCameraBase cm, CMType cmType)
		{
			m_CM_VirtualCameraBase = cm;
			m_CMType = cmType;

			m_CM_VirtualCamera = null;
			m_CM_FreeLook = null;
			if (m_CM_VirtualCameraBase is CinemachineVirtualCamera)
			{
				m_CMCameraType = CMCameraType.VirtualCamera;
				m_CM_VirtualCamera = m_CM_VirtualCameraBase as CinemachineVirtualCamera;
			}
			else if (m_CM_VirtualCameraBase is CinemachineFreeLook)
			{
				m_CMCameraType = CMCameraType.FreeLook;
				m_CM_FreeLook = m_CM_VirtualCameraBase as CinemachineFreeLook;
			}
			else
			{
				m_CMCameraType = CMCameraType.Unknown;
				Leyoutech.Utility.DebugUtility.Assert(false
					, CameraManager.LOG_TAG
					, string.Format("Not support CMCameraType({0}) in CMType({1})"
						, m_CM_VirtualCameraBase.GetType()
						, m_CMType)
					, cm);
			}

			Leyoutech.Utility.DebugUtility.Log(CameraManager.LOG_TAG
					, string.Format("Initialized CM CMType({0}) CMCameraType({1})"
						, m_CMType
						, m_CMCameraType)
					, cm);

			SetEnable(false);
		}

		public CinemachineVirtualCameraBase GetCM_VirtualCameraBase()
		{
			return m_CM_VirtualCameraBase;
		}

		public CinemachineVirtualCamera GetCM_VirtualCamera()
		{
			Leyoutech.Utility.DebugUtility.Assert(m_CMCameraType == CMCameraType.VirtualCamera
				, CameraManager.LOG_TAG
				, "m_CMCameraType == CMCameraType.VirtualCamera");

			return m_CM_VirtualCamera;
		}

		public CinemachineFreeLook GetCM_FreeLook()
		{
			Leyoutech.Utility.DebugUtility.Assert(m_CMCameraType == CMCameraType.FreeLook
				, CameraManager.LOG_TAG
				, "m_CMCameraType == CMCameraType.FreeLook");

			return m_CM_FreeLook;
		}

		public CMType GetCMType()
		{
			return m_CMType;
		}

		public CMCameraType GetCMCameraType()
		{
			return m_CMCameraType;
		}

		public bool IsLive()
		{
			return CinemachineCore.Instance.IsLive(m_CM_VirtualCameraBase);
		}

		/// <summary>
		/// <see cref="CinemachineVirtualCameraBase.OnTargetObjectWarped(Transform, Vector3)"/>
		/// </summary>
		public void TargetObjectWarped(Vector3 positionDelta)
		{
			if (!m_CM_VirtualCameraBase.enabled)
			{
				return;
			}

			Transform target = m_CM_VirtualCameraBase.LookAt
				? m_CM_VirtualCameraBase.LookAt
				: m_CM_VirtualCameraBase.Follow
					? m_CM_VirtualCameraBase.Follow
					: null;

			if (target)
			{
				m_CM_VirtualCameraBase.OnTargetObjectWarped(target, positionDelta);
			}
		}

		public void ApplyTransitionBlendDefinition(TransitionBlendDefinition transitionBlendDefinition)
		{
			Leyoutech.Utility.DebugUtility.Log(CameraManager.LOG_TAG
				, string.Format("CM({0}) apply transitionBlend BlendHint:{1} InheritPosition:{2}"
					, m_CMType
					, transitionBlendDefinition.BlendHint
					, transitionBlendDefinition.InheritPosition));

			switch (m_CMCameraType)
			{
				case CMCameraType.FreeLook:
					m_CM_FreeLook.m_Transitions.m_BlendHint = transitionBlendDefinition.BlendHint;
					m_CM_FreeLook.m_Transitions.m_InheritPosition = transitionBlendDefinition.InheritPosition;
					break;
				case CMCameraType.VirtualCamera:
					m_CM_VirtualCamera.m_Transitions.m_BlendHint = transitionBlendDefinition.BlendHint;
					m_CM_VirtualCamera.m_Transitions.m_InheritPosition = transitionBlendDefinition.InheritPosition;
					break;
				default:
					Leyoutech.Utility.DebugUtility.Assert(false
						, CameraManager.LOG_TAG
						, "Not support CMCameraType: " + m_CMCameraType);
					break;
			}
		}

		public TransitionBlendDefinition GetTransitionBlendDefinition()
		{
			switch (m_CMCameraType)
			{
				case CMCameraType.FreeLook:
					return new TransitionBlendDefinition(m_CM_FreeLook.m_Transitions.m_BlendHint
						, m_CM_FreeLook.m_Transitions.m_InheritPosition);
				case CMCameraType.VirtualCamera:
					return new TransitionBlendDefinition(m_CM_VirtualCamera.m_Transitions.m_BlendHint
						, m_CM_VirtualCamera.m_Transitions.m_InheritPosition);
				default:
					Leyoutech.Utility.DebugUtility.Assert(false
						, CameraManager.LOG_TAG
						, "Not support CMCameraType: " + m_CMCameraType);
					return new TransitionBlendDefinition(CinemachineVirtualCameraBase.BlendHint.None, false);
			}
		}
	}

	public struct ForDebug
	{
		public bool DisplayDetail;

		public int CameraNearClipPlaneIndex;
		public float[] CameraNearClipPlanes;
		public int CameraFarClipPlaneIndex;
		public float[] CameraFarClipPlanes;
		public bool[] GUIEnable;
	}

	private struct CMAxisSetting
	{
		public CM Mouse;
		public CM Gamepad;

		public CMAxisSetting(CM mouse, CM gamepad)
		{
			Mouse = mouse;
			Gamepad = gamepad;
		}

		public struct CM
		{
			public Axis X;
			public Axis Y;

			public CM(Axis x, Axis y)
			{
				X = x;
				Y = y;
			}
		}

		public struct Axis
		{
			public float MaxSpeed;

			public Axis(float maxSpeed)
			{
				MaxSpeed = maxSpeed;
			}
		}
	}
}