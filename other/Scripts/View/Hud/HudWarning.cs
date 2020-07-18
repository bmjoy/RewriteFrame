using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class HudWarning : HudBase
{
	private const string ASSET_ADDRESS = Leyoutech.Core.Loader.Config.AssetAddressKey.PRELOADUI_HUD_INSTANCEAMBUSHPANEL;

	/// <summary>
	/// 提示内容
	/// </summary>
	private TMP_Text m_Text;

	public HudWarning() : base(UIPanel.HudWarning, ASSET_ADDRESS, PanelType.Hud) { }

	public override void Initialize()
	{
		m_Text = FindComponent<TMP_Text>("BossTip/Base/Label/Label_D3");
	}

	public override void OnShow(object msg)
	{
		base.OnShow(msg);
		MSG_WarningHUDInfo info = msg as MSG_WarningHUDInfo;
		m_Text.text = TableUtil.GetLanguageString(info.languageId);
		CfgEternityProxy cfgEternityProxy = GameFacade.Instance.RetrieveProxy(ProxyName.CfgEternityProxy) as CfgEternityProxy;
		if (info.time > 0)
		{
			UIManager.Instance.StartCoroutine(Excute(info.time, () =>
			{
				UIManager.Instance.ClosePanel(UIPanel.HudWarning);
			}));
		}
	}

	/// <summary>
	/// 延迟调用
	/// </summary>
	/// <param name="seconds">秒数</param>
	/// <param name="callBack">回调函数</param>
	/// <returns></returns>
	public static IEnumerator Excute(float seconds, Action callBack)
	{
		yield return new WaitForSeconds(seconds);
		callBack();
	}
}
