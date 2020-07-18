using Assets.Scripts.Define;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static InputManager;

public class RevivePanel : CompositeView
{

    /// <summary>
    /// 玩家名称
    /// </summary>
    public TMP_Text m_KillerName;

    /// <summary>
    /// 等待倒计时界面容器
    /// </summary>
    public Transform m_CountdownBox;
    /// <summary>
    /// 秒
    /// </summary>
    public TMP_Text m_CountdownSecond;
    /// <summary>
    /// 毫秒
    /// </summary>
    public TMP_Text m_CountdownMillisecond;

    public RevivePanel() : base(UIPanel.RevivePanel, PanelType.Normal)
    {

    }

    public override void Initialize()
    {
        base.Initialize();
        m_KillerName = FindComponent<TMP_Text>("Content/Other/Wait/Label_Kill");
        m_CountdownBox = FindComponent<Transform>("Content/Other/Wait");
        m_CountdownSecond = FindComponent<TMP_Text>("Content/Other/Wait/Label_time");
        m_CountdownMillisecond = FindComponent<TMP_Text>("Content/Other/Wait/Label_time (2)");
    }
}
