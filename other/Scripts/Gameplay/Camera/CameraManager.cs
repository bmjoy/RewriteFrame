using DebugPanel;
using Leyoutech.Core.Loader;
using Leyoutech.Core.Loader.Config;
using Leyoutech.Utility;
using System;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
	public const string LOG_TAG = "Camera";
#if UNITY_EDITOR
	/// <summary>
	/// 当前是场景漫游
	/// </summary>
	internal static bool _s_IsRoaming = false;
#endif
	private const int CAMERA_COMPONENT_COUNT = 3;

	private static CameraManager ms_Instance;

	private MainCameraComponent m_MainCameraComponent;
	private UICameraComponent m_UICameraComponent;
	private UI3DCameraComponent m_UI3DCameraComponent;

	private AsyncOperation m_InitializeAsyncOperation;
	private int m_InitializedCameraComponentCount;

	/// <summary>
	/// 屏幕的宽高比 Screen.width / Screen.height
	/// </summary>
	private float m_AspectRatio;
	/// <summary>
	/// 高宽比 1.0f / <see cref="m_AspectRatio"/>
	/// </summary>
	private float m_AspectRatioInvert;

	internal ForDebug _ForDebug;

	public static CameraManager GetInstance()
	{
		if (ms_Instance == null)
		{
			GameObject gameObject = new GameObject("CameraManager");
			DontDestroyOnLoad(gameObject);
			ms_Instance = gameObject.AddComponent<CameraManager>();
		}
		return ms_Instance;
	}

	public AsyncOperation Initialize()
	{
		DebugUtility.Log(LOG_TAG, "Initialize CameraManager");
		m_InitializedCameraComponentCount = 0;
		m_InitializeAsyncOperation = new AsyncOperation();

		AssetManager.GetInstance().InstanceAssetAsync(AssetAddressKey.CAMERA_CAMERA_MAIN, OnInstantiateMainCameraComplete);
		AssetManager.GetInstance().InstanceAssetAsync(AssetAddressKey.CAMERA_CAMERA_UI, OnInstantiateUICameraComplete);
		AssetManager.GetInstance().InstanceAssetAsync(AssetAddressKey.CAMERA_CAMERA_UI3D, OnInstantiateUI3DCameraComplete);
		return m_InitializeAsyncOperation;
	}

	public MainCameraComponent GetMainCamereComponent()
	{
		return m_MainCameraComponent;
	}

	public UICameraComponent GetUICameraComponent()
	{
		return m_UICameraComponent;
	}

	public UI3DCameraComponent GetUI3DCameraComponent()
	{
		return m_UI3DCameraComponent;
	}

	public Camera GetCurrentCamera_ForBothRuntimeAndEditor()
	{
		MainCameraComponent mainCamComponent = CameraManager.GetInstance().GetMainCamereComponent();
		if (mainCamComponent != null)
		{
			return mainCamComponent.GetCamera();
		}
		else
		{
			return Camera.main;
		}
	}

	/// <summary>
	/// <see cref="m_AspectRatio"/>
	/// </summary>
	/// <returns></returns>
	public float GetAspectRatio()
	{
		return m_AspectRatio;
	}

	/// <summary>
	/// <see cref="m_AspectRatioInvert"/>
	/// </summary>
	public float GetAspectRatioInvert()
	{
		return m_AspectRatioInvert;
	}

	protected void LateUpdate()
	{
		m_AspectRatio = (float)Screen.width / Screen.height;
		m_AspectRatioInvert = 1.0f / m_AspectRatio;
	}

	private void OnInstantiateMainCameraComplete(string pathOrAddress, UnityEngine.Object obj, object userData)
	{
		m_MainCameraComponent = OnInstantiateCameraComplete<MainCameraComponent>(obj, "MainCamera");
		if (m_InitializeAsyncOperation.IsDone)
		{
			OnInitialized();
		}
	}

	private void OnInstantiateUICameraComplete(string pathOrAddress, UnityEngine.Object obj, object userData)
	{
		m_UICameraComponent = OnInstantiateCameraComplete<UICameraComponent>(obj, "UICamera");
		if (m_InitializeAsyncOperation.IsDone)
		{
			OnInitialized();
		}
	}

	private void OnInstantiateUI3DCameraComplete(string pathOrAddress, UnityEngine.Object obj, object userData)
	{
		m_UI3DCameraComponent = OnInstantiateCameraComplete<UI3DCameraComponent>(obj, "UI3DCamera");
        m_UI3DCameraComponent.GetCamera().gameObject.SetActive(false);
        if (m_InitializeAsyncOperation.IsDone)
		{
			OnInitialized();
		}
	}

	private T OnInstantiateCameraComplete<T>(UnityEngine.Object obj, string cameraName) where T : BaseCameraComponent
	{
		DebugUtility.Assert(obj != null, LOG_TAG, "Instantiate " + cameraName);

		DebugUtility.Log(LOG_TAG, "Instantiated " + cameraName);
		GameObject cameraGameObject = obj as GameObject;
		m_InitializeAsyncOperation.IsDone = (++m_InitializedCameraComponentCount) == CAMERA_COMPONENT_COUNT;
		cameraGameObject.transform.SetParent(transform, false);
		T cameraComponent = cameraGameObject.GetComponentInChildren<T>();
		cameraComponent.Initialize();
		DebugUtility.Log(LOG_TAG, "Initialized " + cameraName);
		return cameraComponent;
	}

	private void OnInitialized()
	{
		DebugUtility.Log(LOG_TAG, "Begin invoke camera manager initialized callback");
		try
		{
			m_InitializeAsyncOperation.Complete?.Invoke();
			DebugUtility.Log(LOG_TAG, "Invoke camera manager initialized callback success");
		}
		catch (Exception e)
		{
			DebugUtility.LogError(LOG_TAG, string.Format("Invoke camera manager initiazlied callback failed, Exception:\n{0}", e.ToString()));
		}
		finally
		{
			m_InitializeAsyncOperation = null;
		}

		DebugPanelInstance.GetInstance().RegistGUI(TabName.Camera, DoGUI, true);
		DebugUtility.Log(LOG_TAG, "Initialized CameraManager");
	}

	private void DoGUI(Config config)
	{
		config.BeginToolbarHorizontal();
		DoGUICameraButton(config, "Main", m_MainCameraComponent);
		DoGUICameraButton(config, "UI", m_UICameraComponent);
		DoGUICameraButton(config, "UI3D", m_UI3DCameraComponent);
		config.EndHorizontal();

		_ForDebug.SelectedCameraComponent?.DoGUI(config);
	}

	private void DoGUICameraButton(Config config
		, string text
		, BaseCameraComponent camera)
	{
		if (config.ToolbarButton(camera == _ForDebug.SelectedCameraComponent, text))
		{
#if UNITY_EDITOR
			if (_ForDebug.SelectedCameraComponent == camera)
			{
				UnityEditor.Selection.activeGameObject = camera.gameObject;
			}
#endif
			_ForDebug.SelectedCameraComponent = camera;
		}
	}

	public class AsyncOperation
	{
		public bool IsDone = false;
		public Action Complete;
	}

	public struct ForDebug
	{
		public BaseCameraComponent SelectedCameraComponent;
	}
}