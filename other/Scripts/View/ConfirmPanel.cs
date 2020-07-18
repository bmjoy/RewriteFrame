/*===============================
 * Author: [Allen]
 * Purpose: 通知界面，可携带多组热键
 * Time: 2019/3/30 15:09:03
================================*/
using Assets.Scripts.Define;
using System;
using System.Runtime.InteropServices;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class ConfirmPanel : UIPanelBase
{
    private const string ASSET_ADDRESS = "Assets/Artwork/UI/Prefabs/ConfirmPanel.prefab";

	/// <summary>
	/// 颜色表
	/// </summary>
    private CfgEternityProxy m_CfgEternityProxy;

    /// <summary>
    /// 界面的hotkey 数组
    /// </summary>
    private HotKeyButton[] PanelHotkeyArray;

    /// <summary>
    /// 左边ICON
    /// </summary>
    private Image m_LeftIcon;

    /// 右边上背景条
    /// </summary>
    private Image m_TopBase;

    /// <summary>
    /// 标题
    /// </summary>
    private TMP_Text m_Title;

    /// <summary>
    /// 内容
    /// </summary>
    private TMP_Text m_Describe;

    /// <summary>
    /// 热键挂点
    /// </summary>
    private Transform m_HotKeyRoot;

	/// <summary>
	/// 打开面板传入的消息
	/// </summary>
	private object m_CurrentMsg;

	public ConfirmPanel() : base(UIPanel.ConfirmPanel, ASSET_ADDRESS, PanelType.Notice)
    {
    }
    public override void Initialize()
    {
        m_CfgEternityProxy = (CfgEternityProxy)GameFacade.Instance.RetrieveProxy(ProxyName.CfgEternityProxy);
        m_LeftIcon = FindComponent<Image>("Content/Left/Image_Icon");
        m_TopBase = FindComponent<Image>("Content/Right/TopBase");
        m_Title = FindComponent<TMP_Text>("Content/Right/Label_Title");
        m_Describe = FindComponent<TMP_Text>("Content/Right/Label_Desc");
        m_HotKeyRoot = FindComponent<Transform>("Control/Footer/Content");
    }

    public override void OnShow(object msg)
    {
        base.OnShow(msg);
		m_CurrentMsg = msg;
    }

    public void Close()
    {
        UIManager.Instance.ClosePanel(this);
    }

    public override void OnHide(object msg)
    {
        PanelHotkeyArray = null;
        base.OnHide(msg);
    }

    public override void OnRefresh(object msg)
    {
		if (msg == null)
		{
			return;
		}
		OpenParameter? openParameter = (OpenParameter?)msg;
		if (openParameter.HasValue)
		{
			m_Title.text = openParameter.Value.Title;
			m_Describe.text = openParameter.Value.Content;
			SetColor(openParameter.Value.backgroundColor);
		}
    }

	public override void OnGotFocus()
	{
		base.OnGotFocus();
		OpenParameter? openParameter = (OpenParameter?)m_CurrentMsg;
		if (openParameter.HasValue)
		{
			SetHotKeys(openParameter.Value.HotkeyArray);
		}
	}

	/// <summary>
	/// 设置颜色
	/// </summary>
	/// <param name="backgroundColor">背景颜色</param>
	private void SetColor(BackgroundColor backgroundColor)
    {
        switch (backgroundColor)
        {
            case BackgroundColor.Error:
                {
                    Color color = m_CfgEternityProxy.GetGlobalColor((int)KGlobalColorKey.ErrorNotice);
                    m_LeftIcon.color = color;
					m_TopBase.color = color;
                }
                break;
            default:
                {
                    Color color = m_CfgEternityProxy.GetGlobalColor((int)KGlobalColorKey.GeneralNotice);
                    m_LeftIcon.color = color;
					m_TopBase.color = color;
                }
                break;
        }
    }

    /// <summary>
    /// 设置hotKey
    /// </summary>
    /// <param name="hotkeyArray">热键组</param>
    private void SetHotKeys(HotKeyButton[] hotkeyArray)
    {
		if (hotkeyArray == null || hotkeyArray.Length == 0)
		{
			return;
		}
		for (int i = 0 ; i < hotkeyArray.Length; i++)
        {
            HotKeyButton hotBtn = hotkeyArray[i];
            AddHotKey(hotBtn.actionName, hotBtn.onEvent, m_HotKeyRoot, hotBtn.showText);
        }
        PanelHotkeyArray = hotkeyArray;
    }

	/// <summary>
	/// 打开时的传入参数结构体
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	public struct OpenParameter
	{
		/// <summary>
		/// 标题
		/// </summary>
		public string Title;

		/// <summary>
		/// 内容
		/// </summary>
		public string Content;

		/// <summary>
		/// 提示颜色类型  普通 or  错误
		/// 只定义了枚举类型变量，没有给枚举类型的变量赋值，则系统会自动默认赋值为0
		/// </summary>
		public BackgroundColor backgroundColor;

		/// <summary>
		/// 热键
		/// </summary>
		[MarshalAsAttribute(UnmanagedType.ByValArray/*, SizeConst = 1*/)]
		public HotKeyButton[] HotkeyArray;

	}

	/// <summary>
	/// 背景颜色
	/// </summary>
	public enum BackgroundColor
	{
		/// <summary>
		/// 普通
		/// </summary>
		Normal,

		/// <summary>
		/// 错误
		/// </summary>
		Error
	}

	public struct HotKeyButton
	{
		/// <summary>
		/// InputAction名称
		/// </summary>
		public string actionName;

		/// <summary>
		/// 显示的热键描述
		/// </summary>
		public string showText;

		/// <summary>
		/// 热键响应回调
		/// </summary>
		public Action<HotkeyCallback> onEvent;
	}
}

