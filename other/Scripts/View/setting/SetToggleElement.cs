///*===============================
// * Author: [dinghuilin]
// * Purpose: SetToggleCard.cs
// * Time: 2019/03/22  14:16
//================================*/
//using Game.Data.Setting;
//using TMPro;
//using UnityEngine;
//using UnityEngine.UI;
///// <summary>
///// SetToggle
///// </summary>
//public class SetToggleElement : SetElementBase
//{
//	/// <summary>
//	/// toggle动画
//	/// </summary>
//	private TMP_Text m_ToggleText;
//	/// <summary>
//	/// card名字
//	/// </summary>
//	private TMP_Text m_CardName;
//	/// <summary>
//	/// 设置 toggle显示Text
//	/// </summary>
//	private TMP_Text m_DisplayValue;
//	/// <summary>
//	/// 左按钮
//	/// </summary>
//	private Button m_LeftButton;
//	/// <summary>
//	/// 右按钮
//	/// </summary>
//	private Button m_RightButton;
//	/// <summary>
//	/// 面板
//	/// </summary>
//	private GameSettingPanel m_SettingPanel;
//	/// <summary>
//	/// 设置 帮助Proxy
//	/// </summary>
//	private StoreHelperProxy m_StoreHelperProxy;
//	/// <summary>
//	/// 设置VO
//	/// </summary>
//	private SetVO m_SetVO;
//	/// <summary>
//	/// 最后值
//	/// </summary>
//	private int m_LastValue;
//	/// <summary>
//	/// 显示值
//	/// </summary>
//	private int m_ShowValue;
//	/// <summary>
//	/// 显示文本
//	/// </summary>
//	private string[] m_DisplayText;

//	private void Awake()
//	{
//		m_CardName = transform.Find("Content/Name").GetComponent<TMP_Text>();
//		m_DisplayValue = transform.Find("Content/Content/Value").GetComponent<TMP_Text>();
//		m_ToggleText = transform.Find("Content/Content/Value").GetComponent<TMP_Text>();
//		m_LeftButton = transform.Find("Content/Content/LeftArrow").GetComponent<Button>();
//		m_RightButton = transform.Find("Content/Content/RightArrow").GetComponent<Button>();
//		m_LeftButton.onClick.AddListener(LeftButtonClick);
//		m_RightButton.onClick.AddListener(RightButtonClick);
		
//	}
//	/// <summary>
//	/// 初始化
//	/// </summary>
//	/// <param name="setVO">设置VO</param>
//	/// <param name="proxy">设置proxy</param>
//	/// <param name="settingPanel">设置面板</param>
//	public void Init(SetVO setVO, CfgSettingProxy proxy, GameSettingPanel settingPanel, Vector2Int ver, GroupScrollerView view)
//	{
//		this.m_StoreHelperProxy = (StoreHelperProxy)GameFacade.Instance.RetrieveProxy(ProxyName.StoreHelperProxy);
//		this.m_SetVO = setVO;
//		m_SettingPanel = settingPanel;
//		m_GroupScrollerView = view;
//		UIEventListener.UIEventListener.AttachListener(m_LeftButton.gameObject, ver).onClick = OnClick;
//		UIEventListener.UIEventListener.AttachListener(m_RightButton.gameObject, ver).onClick = OnClick;
//		m_CardName.text = proxy.GetLocalization(SystemLanguage.English, setVO.Name);
//		m_DisplayText = new string[] { proxy.GetLocalization(SystemLanguage.English,setVO.OptionDsc(setVO.Option(0)))
//			, proxy.GetLocalization(SystemLanguage.English,setVO.OptionDsc(setVO.Option(1)))};
//		transform.localScale = Vector3.one;
//		Refresh(true);

//	}
//	/// <summary>
//	/// 左按钮点击
//	/// </summary>
//	public override void LeftButtonClick()
//	{
//		m_ShowValue = m_ShowValue == 0
//			? m_DisplayText.Length - 1
//			: m_ShowValue - 1;
//		m_StoreHelperProxy.SetValue(m_SetVO.Id, m_ShowValue);
//		Refresh();
//	}
//	/// <summary>
//	/// 右按钮点击
//	/// </summary>
//	public override void RightButtonClick()
//	{
//		m_ShowValue = (m_ShowValue + 1 >= m_DisplayText.Length)
//		  ? 0
//		  : m_ShowValue + 1;
//		m_StoreHelperProxy.SetValue(m_SetVO.Id, m_ShowValue);
//		Refresh();
//	}
//	/// <summary>
//	/// 刷新
//	/// </summary>
//	/// <param name="isRestore">是否刷新</param>
//	public void Refresh(bool isRestore = false)
//	{
//		if (isRestore)
//		{
//			m_ShowValue = m_StoreHelperProxy.GetValue(m_SetVO.Id);
//			m_ShowValue = Mathf.Clamp(m_ShowValue, 0, m_DisplayText.Length - 1);
//			m_LastValue = m_StoreHelperProxy.GetInt(((VideoSettingType)m_SetVO.Id).ToString());
//		}
//		m_ToggleText.color = m_ShowValue != m_LastValue ? new Color(191 / 255f, 242 / 255f, 252 / 255f, 1f) : new Color(189 / 255f, 189 / 255f, 189 / 255f, 1f);
//		m_DisplayValue.text = m_DisplayText[m_ShowValue];
//	}
//}
