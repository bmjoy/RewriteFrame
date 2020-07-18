using Assets.Scripts.Define;
using TMPro;
using UnityEngine;

public class HudBattleStateCountdownPanel : HudBase
{
    private const string ASSET_ADDRESS = Leyoutech.Core.Loader.Config.AssetAddressKey.PRELOADUI_HUD_MODECOUTDOWNPANEL;

    /// <summary>
    /// 倒计时容器
    /// </summary>
    private RectTransform m_CooldownBox;
    /// <summary>
    /// 倒计时文字
    /// </summary>
    private TMP_Text m_CooldownText;
    /// <summary>
    /// GameplayProxy
    /// </summary>
    private GameplayProxy m_Gameplay;

    public HudBattleStateCountdownPanel() : base(UIPanel.HudBattleStateCountdownPanel, ASSET_ADDRESS, PanelType.Hud) { }

    public override void Initialize()
    {
        m_CooldownBox = FindComponent<RectTransform>("Content");
        m_CooldownText = FindComponent<TMP_Text>("Content/Label_Number");
    }

    public override void OnShow(object msg)
    {
        base.OnShow(msg);

        m_Gameplay = Facade.RetrieveProxy(ProxyName.GameplayProxy) as GameplayProxy;

        StartUpdate();
    }

    public override void OnHide(object msg)
    {
        m_Gameplay = null;

        base.OnHide(msg);
    }

    /// <summary>
    /// 更新
    /// </summary>
    protected override void Update()
    {
        if (m_Gameplay != null)
        {
            SpacecraftEntity main = GetMainEntity();
            if (main != null)
            {
                bool isFighting = IsBattling();

                float fireCD = main.GetFireCountdown();
                //float fireCDMax = 10.0f;// main.GetAttribute(AircraftAttributeType.peerlessTopLimit);

                m_CooldownBox.gameObject.SetActive(isFighting && fireCD > 0);
                if (m_CooldownBox.gameObject.activeSelf)
                    m_CooldownText.text = string.Format(TableUtil.GetLanguageString("hud_text_id_1014"), fireCD > 0 ? (int)fireCD : 0);
            }
        }
    }
}
