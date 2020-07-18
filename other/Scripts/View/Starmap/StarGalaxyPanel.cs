using UnityEngine;

public class StarGalaxyPanel : CompositeView
{
	public GameObject CellPrefab { get; private set; }

	public StarGalaxyPanel() : base(UIPanel.StarGalaxyPanel, PanelType.Normal)
	{
	}

	public override void Initialize()
	{
		base.Initialize();
		UIManager.Instance.GetUIElement(Leyoutech.Core.Loader.Config.AssetAddressKey.PRELOADUIELEMENT_STARMAPELEMENT,
			(cell) =>
			{
				CellPrefab = cell;
			}
		);
	}

    protected override void OnEscCallback(HotkeyCallback callback)
    {
        //base.OnEscCallback(callback);
    }
}