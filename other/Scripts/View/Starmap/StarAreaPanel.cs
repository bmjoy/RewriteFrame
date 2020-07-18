using UnityEngine;

public class StarAreaPanel : CompositeView
{
	public GameObject PointPrefab { get; private set; }
	public GameObject LinePrefab { get; private set; }
	public StarAreaPanel() : base(UIPanel.StarAreaPanel, PanelType.Normal)
	{
	}

	public override void Initialize()
	{
		base.Initialize();
		UIManager.Instance.GetUIElement(Leyoutech.Core.Loader.Config.AssetAddressKey.PRELOADUIELEMENT_STARMAPSPECIALELEMENT,
			(cell) =>
			{
				PointPrefab = cell;
			}
		);
		UIManager.Instance.GetUIElement(Leyoutech.Core.Loader.Config.AssetAddressKey.PRELOADUIELEMENT_STARMAPLINEELEMENT,
			(cell) =>
			{
				LinePrefab = cell;
			}
		);
    }
    protected override void OnEscCallback(HotkeyCallback callback)
    {
        //base.OnEscCallback(callback);
    }
}