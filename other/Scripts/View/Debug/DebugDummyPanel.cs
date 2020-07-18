/// <summary>
/// 空界面，目的是让<see cref="DebugPanel"/>在不打开遮挡画面的界面的情况下使用虚拟光标
/// </summary>
public class DebugDummyPanel : UIPanelBase
{
	public const string ASSET_ADDRESS = "FPJDSFJHIDSOFDISOFHSDOFUDSFDISFUEIWNFDSNFOISNFOUEWBNIDSNFKl";

	internal static bool _s_IsShowing;

	public DebugDummyPanel() : base(UIPanel.DebugDummyPanel, ASSET_ADDRESS, PanelType.Normal)
	{
		_s_IsShowing = false;
	}

	public override void OnShow(object msg)
	{
		base.OnShow(msg);
		_s_IsShowing = true;
		StartUpdate();
	}

	public override void OnHide(object msg)
	{
		StopUpdate();
		_s_IsShowing = false;
		base.OnHide(msg);
	}

	protected override void Update()
	{
		base.Update();

		InputManager.Instance.UIInputMap = HotKeyMapID.UI;
	}
}