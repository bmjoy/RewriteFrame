//using Game.Data.Setting;
//using TMPro;
//using UnityEngine;
//using UnityEngine.UI;
///// <summary>
///// SetSlider
///// </summary>
//public class SetSliderElement : SetElementBase
//{
//	/// <summary>
//	/// card名字
//	/// </summary>
//	private TMP_Text m_CardName;
//	/// <summary>
//	/// 设置 slider显示Text
//	/// </summary>
//	private TMP_Text m_DisplayText;
//	/// <summary>
//	/// 左按钮
//	/// </summary>
//	private Button m_LeftButton;
//	/// <summary>
//	/// 右按钮
//	/// </summary>
//	private Button m_RightButton;
//	/// <summary>
//	/// 设置Proxy
//	/// </summary>
//	private StoreHelperProxy m_StoreHelperProxy;
//	/// <summary>
//	/// 设置VO
//	/// </summary>
//	private SetVO m_SetVO;
//	/// <summary>
//	/// 设置面板
//	/// </summary>
//	private GameSettingPanel m_SetPanel;
//	/// <summary>
//	/// Dropdown集合
//	/// </summary>
//	private Transform m_DropdownList;
//	/// <summary>
//	/// 面板Content 物体Transform
//	/// </summary>
//	private Transform m_ContentTransform;
//	/// <summary>
//	/// Slider动画
//	/// </summary>
//	private TMP_Text m_SliderText;
//	/// <summary>
//	/// 显示内容
//	/// </summary>
//	private int m_ShowValue;
//	/// <summary>
//	/// 最后值
//	/// </summary>
//	private int m_LastValue;
//	/// <summary>
//	/// 最小值
//	/// </summary>
//	private int m_MinValue;
//	/// <summary>
//	/// 最大值
//	/// </summary>
//	private int m_MaxValue;

//	private void Awake()
//	{
//		m_CardName = transform.Find("Content/Name").GetComponent<TMP_Text>();
//		m_DisplayText = transform.Find("Content/Content/Value").GetComponent<TMP_Text>();
//		m_LeftButton = transform.Find("Content/Content/LeftArrow").GetComponent<Button>();
//		m_RightButton = transform.Find("Content/Content/RightArrow").GetComponent<Button>();
//		m_SliderText = transform.Find("Content/Content/Value").GetComponent<TMP_Text>();
//		m_LeftButton.onClick.AddListener(LeftButtonClick);
//		UIEventListener.UIEventListener.AttachListener(m_LeftButton.gameObject, true).onLongPress = OnLeftValueChanged;
//		m_RightButton.onClick.AddListener(RightButtonClick);
//		UIEventListener.UIEventListener.AttachListener(m_RightButton.gameObject, true).onLongPress = OnRightValueChanged;
		
//	}
//	/// <summary>
//	/// 初始化面板
//	/// </summary>
//	/// <param name="setVO">设置VO</param>
//	/// <param name="proxy">设置proxy</param>
//	/// <param name="setPanel">设置面板</param>
//	public void Init(SetVO setVO, CfgSettingProxy proxy, GameSettingPanel setPanel, Vector2Int ver, GroupScrollerView view)
//	{
//		this.m_StoreHelperProxy = (StoreHelperProxy)GameFacade.Instance.RetrieveProxy(ProxyName.StoreHelperProxy);
//		this.m_SetVO = setVO;
//		this.m_SetPanel = setPanel;
//		m_GroupScrollerView = view;
//		UIEventListener.UIEventListener.AttachListener(m_LeftButton.gameObject, ver).onClick = OnClick;
//		UIEventListener.UIEventListener.AttachListener(m_RightButton.gameObject, ver).onClick = OnClick;
//		m_CardName.text = proxy.GetLocalization(SystemLanguage.English, setVO.Name);
//		m_MinValue = setVO.Option(0);
//		m_MaxValue = setVO.Option(1);
//		transform.localScale = Vector3.one;
//		Refresh(true);
//	}
//	/// <summary>
//	/// 刷新
//	/// </summary>
//	/// <param name="isRestore">是否恢复</param>
//	public void Refresh(bool isRestore = false)
//	{
//		if (isRestore)
//		{
//			m_ShowValue = m_StoreHelperProxy.GetValue(m_SetVO.Id);
//			m_LastValue = m_StoreHelperProxy.GetInt(((VideoSettingType)m_SetVO.Id).ToString());
//		}
//		m_SliderText.color = m_ShowValue != m_LastValue ? new Color(191 / 255f, 242 / 255f, 252 / 255f, 1f) : new Color(189 / 255f, 189 / 255f, 189 / 255f, 1f);
//		m_DisplayText.text = m_ShowValue.ToString();
//		m_RightButton.interactable = m_ShowValue < m_MaxValue;
//		m_LeftButton.interactable = m_ShowValue > m_MinValue;
//	}
//	/// <summary>
//	/// 左按钮改变事件
//	/// </summary>
//	/// <param name="go">点击物体</param>
//	/// <param name="objs">UIEventListener参数</param>
//	private void OnLeftValueChanged(GameObject go, object[] objs)
//	{
//		LeftButtonClick();
//	}
//	/// <summary>
//	/// 右按钮改变事件
//	/// </summary>
//	/// <param name="go">点击物体</param>
//	/// <param name="objs">UIEventListener参数</param>
//	private void OnRightValueChanged(GameObject go, object[] objs)
//	{
//		RightButtonClick();
//	}
//	/// <summary>
//	/// 左按钮点击
//	/// </summary>
//	public override void LeftButtonClick()
//	{
//		if (m_ShowValue > m_MinValue)
//		{
//			m_ShowValue--;
//		}

//		m_StoreHelperProxy.SetValue(m_SetVO.Id, m_ShowValue);
//		Refresh();
//	}
//	/// <summary>
//	/// 右按钮点击
//	/// </summary>
//	public override void RightButtonClick()
//	{
//		if (m_ShowValue < m_MaxValue)
//		{
//			m_ShowValue++;
//		}

//		m_StoreHelperProxy.SetValue(m_SetVO.Id, m_ShowValue);
//		Refresh();
//	}
//}
