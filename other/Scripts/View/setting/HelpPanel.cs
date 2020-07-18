using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HelpPanel : UIPanelBase
{
	/// <summary>
	/// 资源地址
	/// </summary>
	private const string ASSET_ADDRESS = "Assets/Artwork/UI/Prefabs/HelpPanel.prefab";
	/// <summary>
	/// 热键挂点
	/// </summary>
	private Transform m_HotKeyRoot;
	public HelpPanel() : base(UIPanel.HelpPanel, ASSET_ADDRESS, PanelType.Normal)
	{

	}
	/// <summary>
	/// <see cref="UIPanelBase.Initialize()"/>
	/// </summary>
	public override void Initialize()
	{
	}
	/// <summary>
	/// <see cref="UIPanelBase.OnRefresh()"/>
	/// </summary>
	public override void OnRefresh(object msg)
	{
		
	}
    public override void OnGotFocus()
    {
        base.OnGotFocus();
        AddHotKey(HotKeyID.FuncB, ClosePanel);
    }
	/// <summary>
	/// 关闭面板
	/// </summary>
	public void ClosePanel(HotkeyCallback callbackContext)
	{
		if (callbackContext.performed)
		{
			UIManager.Instance.ClosePanel(this);
		}
	}
}
