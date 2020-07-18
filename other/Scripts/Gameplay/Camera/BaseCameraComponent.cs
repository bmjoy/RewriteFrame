using System;
using UnityEngine;

public abstract class BaseCameraComponent : MonoBehaviour
{
	protected Camera m_Camera;
	protected Transform m_Transform;

	/// <summary>
	/// 在子类的<see cref="Initialize"/>中初始化
	/// </summary>
	protected UpdatePropertyFlag m_UpdateProperties = 0;

	protected float m_HalfTanFov;
	protected Vector3 m_Position;
	protected Vector3 m_Forward;
	protected Quaternion m_Rotation;

	public virtual void Initialize()
	{
		Leyoutech.Utility.DebugUtility.Log(CameraManager.LOG_TAG, "Initialize " + GetCameraName());

		m_Camera = gameObject.GetComponent<Camera>();
		m_Transform = transform;
	}

	public abstract string GetCameraName();

	public Camera GetCamera()
	{
		return m_Camera;
	}

	public Transform GetTransform()
	{
		return m_Transform;
	}

	/// <summary>
	/// <see cref="UpdatePropertyFlag.HalfTanFov"/>
	/// </summary>
	public float GetHalfTanFov()
	{
		return m_HalfTanFov;
	}

	public Vector3 GetPosition()
	{
		return m_Position;
	}

	public Vector3 GetForward()
	{
		return m_Forward;
	}

	public Quaternion GetRotation()
	{
		return m_Rotation;
	}

	public void SetCameraEnable(bool enable)
	{
		m_Camera.enabled = enable;
	}

	public void DoGUI(DebugPanel.Config config)
	{
		config.BeginToolbarHorizontal();
		m_Camera.enabled = config.ToolbarToggle(m_Camera.enabled, "Enable");
		m_Camera.allowHDR = config.ToolbarToggle(m_Camera.allowHDR, "HDR");
		m_Camera.allowMSAA = config.ToolbarToggle(m_Camera.allowMSAA, "MASS");
		m_Camera.allowDynamicResolution = config.ToolbarToggle(m_Camera.allowDynamicResolution, "dRes");

		m_Camera.depthTextureMode = config.EnumPopup(m_Camera.depthTextureMode, "DTM", true);
#if UNITY_EDITOR
		m_UpdateProperties = (UpdatePropertyFlag)UnityEditor.EditorGUILayout.EnumFlagsField(m_UpdateProperties, UnityEditor.EditorStyles.toolbarPopup);
#endif
		config.EndHorizontal();

		DoGUIOverride(config);
	}

	public void SetCameraAspectAndRect(float aspect, Rect viewportRect)
	{
		m_Camera.aspect = aspect;
		m_Camera.rect = viewportRect;
	}

	protected abstract void DoGUIOverride(DebugPanel.Config config);

	protected void LateUpdate()
	{
		if ((m_UpdateProperties & UpdatePropertyFlag.HalfTanFov) > 0)
		{
			m_HalfTanFov = Leyoutech.Utility.RendererUtility.CaculateHalfTanCameraFov(m_Camera.fieldOfView);
		}
		if ((m_UpdateProperties & UpdatePropertyFlag.Transform) > 0)
		{
			m_Position = m_Transform.position;
			m_Forward = m_Transform.forward;
			m_Rotation = m_Transform.rotation;
		}

		DoLateUpdate(Time.deltaTime);
	}

	protected abstract void DoLateUpdate(float deltaTime);

	/// <summary>
	/// 需要每帧更新的相机属性
	/// </summary>
	[Flags]
	public enum UpdatePropertyFlag
	{
		/// <summary>
		/// <see cref="Leyoutech.Utility.RendererUtility.CaculateHalfTanCameraFov"/>
		/// </summary>
		HalfTanFov = 1 << 0,
		Transform = 1 << 1,
	}
}