using PureMVC.Interfaces;
using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class HudAreaNamePanel : HudBase
{
	private const string ASSET_ADDRESS = Leyoutech.Core.Loader.Config.AssetAddressKey.PRELOADUI_HUD_AREANAMEPANEL;

	/// <summary>
	/// 地图名
	/// </summary>
	private TMP_Text m_Text;

	private CfgEternityProxy m_CfgEternityProxy;
    /// <summary>
    /// 协程
    /// </summary>
    private Coroutine m_Coroutine;

	public HudAreaNamePanel() : base(UIPanel.HudAreaNamePanel, ASSET_ADDRESS, PanelType.Hud) { }

	public override void Initialize()
	{
		m_CfgEternityProxy = GameFacade.Instance.RetrieveProxy(ProxyName.CfgEternityProxy) as CfgEternityProxy;

		m_Text = FindComponent<TMP_Text>("Content/CommandBox/SceneName");
	}

	public override void OnShow(object msg)
	{
		base.OnShow(msg);

        SetAraeName();
    }

    public override void OnHide(object msg)
    {
        if (m_Coroutine != null)
        {
            UIManager.Instance.StopCoroutine(m_Coroutine);
            m_Coroutine = null;
        }

        base.OnHide(msg);
    }

    /// <summary>
    /// 设置区域名
    /// </summary>
    public void SetAraeName()
    {
        Eternity.FlatBuffer.Map mapData = m_CfgEternityProxy.GetCurrentMapData();
        ulong area = Map.MapManager.GetInstance().GetCurrentAreaUid();
        m_Text.text = TableUtil.GetLanguageString($"area_name_{mapData.GamingmapId}_{area}");

        m_Coroutine = UIManager.Instance.StartCoroutine(Excute(5.0f, () =>
        {
            UIManager.Instance.ClosePanel(this);
        }));

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
