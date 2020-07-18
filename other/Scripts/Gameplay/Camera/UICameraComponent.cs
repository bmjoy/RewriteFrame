using DebugPanel;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class UICameraComponent : BaseCameraComponent
{
	private PostProcessLayer m_PostProcessLayer;
	private CRenderer.UICameraRenderer m_CameraRenderer;

	public override void Initialize()
	{
		base.Initialize();

		m_PostProcessLayer = gameObject.GetComponent<PostProcessLayer>();

		m_CameraRenderer = new CRenderer.UICameraRenderer();
	}

	public override string GetCameraName()
	{
		return "UICamera";
	}

	protected override void DoGUIOverride(Config config)
	{
		config.BeginToolbarHorizontal();
		m_PostProcessLayer.enabled = config.ToolbarToggle(m_PostProcessLayer.enabled, "PostProcess");
		config.EndHorizontal();
	}

	protected override void DoLateUpdate(float deltaTime)
	{
	}

	protected void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		m_CameraRenderer.OnRenderImage(source, destination);
	}
}