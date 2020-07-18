using Assets.Scripts.Define;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HudFurnacePanel : HudBase
{
    private const string ASSET_ADDRESS = Leyoutech.Core.Loader.Config.AssetAddressKey.PRELOADUI_HUD_SPECIALSKILL;

    /// <summary>
    /// 状态动画
    /// </summary>
    private Animator m_StateAnimator;
    /// <summary>
    /// 效果动画
    /// </summary>
    private Animator m_EffectAnimator;

    /// <summary>
    /// 聚气条
    /// </summary>
    private Image m_PowerSlider;

    /// <summary>
    /// 倒讲时容器
    /// </summary>
    private RectTransform m_CountdownBox;
    /// <summary>
    /// 倒计时(秒)
    /// </summary>
    private TMP_Text m_CountdownText1;
    /// <summary>
    /// 倒计时(毫秒)
    /// </summary>
    private TMP_Text m_CountdownText2;

    /// <summary>
    /// 热键容器
    /// </summary>
    private RectTransform m_HotkeyBox;
    /// <summary>
    /// 热键A
    /// </summary>
    private RectTransform m_Hotkey1;
    /// <summary>
    /// 热键B
    /// </summary>
    private RectTransform m_Hotkey2;

    /// <summary>
    /// 是否为战斗状态
    /// </summary>
    private bool m_BattleState;
    /// <summary>
    /// 主动画上次状态
    /// </summary>
    private string m_EffectState;


    public HudFurnacePanel() : base(UIPanel.HudFurnacePanel, ASSET_ADDRESS, PanelType.Hud) { }

    public override void Initialize()
    {
        m_StateAnimator = GetTransform().GetComponent<Animator>();
        m_EffectAnimator = FindComponent<Animator>("Content/SkillButton");

        m_PowerSlider = FindComponent<Image>("Content/SkillButton/Loading/Image_Loading");

        m_CountdownBox = FindComponent<RectTransform>("Content/SkillButton/CD");
        m_CountdownText1 = FindComponent<TMP_Text>("Content/SkillButton/CD/CDTime/Label_CDTimes_second");
        m_CountdownText2 = FindComponent<TMP_Text>("Content/SkillButton/CD/CDTime/Label_CDTimes_milisecond");
        
        m_HotkeyBox = FindComponent<RectTransform>("Content/SkillButton/HotKey");
        m_Hotkey1 = FindComponent<RectTransform>("Content/SkillButton/HotKey/HotKey_L");
        m_Hotkey2 = FindComponent<RectTransform>("Content/SkillButton/HotKey/HotKey_R");
    }

    public override void OnShow(object msg)
    {
        base.OnShow(msg);

        AddHotKey(HotKeyMapID.SHIP, HotKeyID.ShipReadyBurst, OnHotKey1, m_Hotkey1, "", HotkeyManager.HotkeyStyle.UI_SIMPLE);
        AddHotKey(HotKeyMapID.SHIP, HotKeyID.WeaponFire, OnHotKey2, m_Hotkey2, "", HotkeyManager.HotkeyStyle.UI_SIMPLE);

        m_BattleState = false;
        m_EffectState = string.Empty;

        StartUpdate();
    }

    /// <summary>
    /// 更新
    /// </summary>
    protected override void Update()
    {
        SpacecraftEntity main = GetMainEntity();
        if (main == null)
            return;

        //当前状态
        bool isBattle = IsBattling();
        bool isBurstReady = main.GetBurstReady();
        bool isBurstActivated = IsPeerless();

        //能量属性
        float peerless = (float)main.GetAttribute(AttributeName.kConverterValue);
        float peerlessMax = (float)main.GetAttribute(AttributeName.kConverterMax);

        //能量条
        m_PowerSlider.gameObject.SetActive(true);
        if (peerlessMax > 0 && peerless > 0)
            m_PowerSlider.fillAmount = Mathf.Lerp(m_PowerSlider.fillAmount, peerless / peerlessMax, Time.deltaTime * 2);
        else
            m_PowerSlider.fillAmount = Mathf.Lerp(m_PowerSlider.fillAmount, 0, Time.deltaTime * 2);

        //倒计时
        //m_CountdownBox.gameObject.SetActive(isBurstActivated);
        //if(isBurstActivated)
        //{
        //peerless
        //double consume = main.GetAttribute(AttributeName.kConverterCostEfficiency);
        //Debug.LogError(consume);
        //}

        //热键显示
        //m_HotkeyBox.gameObject.SetActive(isBattle);
        m_Hotkey2.gameObject.SetActive(isBurstReady && !isBurstActivated);
        m_Hotkey1.gameObject.SetActive(Mathf.Approximately(peerless, peerlessMax) && !isBurstReady && !isBurstActivated);

        //状态动画
        if (isBattle != m_BattleState)
        {
            m_BattleState = isBattle;

            m_StateAnimator.ResetTrigger("Cruise");
            m_StateAnimator.ResetTrigger("Battle");
            m_StateAnimator.SetTrigger(isBattle ? "Battle" : "Cruise");
        }

        //特效动画
        string newState = "Normal";
        if (isBattle)
        {
            if (peerless >= peerlessMax && peerlessMax > 0)
                newState = isBurstReady ? "Hold" : "Already";
            if (isBurstActivated)
                newState = "Release";
        }
        if (!newState.Equals(m_EffectState))
        {
            m_EffectState = newState;

            m_EffectAnimator.ResetTrigger("Normal");
            m_EffectAnimator.ResetTrigger("Already");
            m_EffectAnimator.ResetTrigger("Hold");
            m_EffectAnimator.ResetTrigger("Release");
            m_EffectAnimator.SetTrigger(newState);
        }
    }

    /// <summary>
    /// 热键1按下时
    /// </summary>
    /// <param name="callback">热键参数</param>
    private void OnHotKey1(HotkeyCallback callback)
    {
    }

    /// <summary>
    /// 热键2按下时
    /// </summary>
    /// <param name="callback">热键参数</param>
    private void OnHotKey2(HotkeyCallback callback)
    {
    }
}
