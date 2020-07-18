///*===============================
// * Author: [dinghuilin]
// * Purpose: SetDropDownCard.cs
// * Time: 2019/03/22  14:17
//================================*/
//using TMPro;
//using UnityEngine;
///// <summary>
///// 设置界面 下拉框
///// </summary>
//public class SetDropDownElement : SetElementBase/*,ISubmitHandler,ICancelHandler*/
//{
//	/// <summary>
//	/// 设置本地Proxy
//	/// </summary>
//	private StoreHelperProxy m_StoreHelperProxy;
//	/// <summary>
//	/// 下拉框
//	/// </summary>
//	private TMP_Dropdown m_Dropdown;
//	public TMP_Dropdown GetDropdown()
//	{
//		return m_Dropdown;
//	}
//	/// <summary>
//	/// 设置数据
//	/// </summary>
//	private SetVO m_SetVO;
//	/// <summary>
//	/// 设置面板
//	/// </summary>
//	private GameSettingPanel m_SetPanel;
//	/// <summary>
//	/// 下拉组件动画
//	/// </summary>
//	private TMP_Text m_DropdownText;
//	/// <summary>
//	/// card名字
//	/// </summary>
//	private TMP_Text m_CardName;
//	/// <summary>
//	/// HACK 为了修复最后一个item不显示
//	/// </summary>
//	private Transform m_LastItem;
//	/// <summary>
//	/// 下拉箭头
//	/// </summary>
//	private GameObject m_DropArrow;
//	/// <summary>
//	/// 最后数值
//	/// </summary>
//	private int m_LastValue;
//	/// <summary>
//	/// 展示数值
//	/// </summary>
//	private int m_ShowValue;


//	private void Awake()
//	{
//		m_Dropdown = transform.Find("Content/FilterList").GetComponent<TMP_Dropdown>();
//		m_DropArrow = transform.Find("Content/FilterList/Arrow").gameObject;
//		m_DropdownText = transform.Find("Content/FilterList/Label").GetComponent<TMP_Text>();
//		m_CardName = transform.Find("Content/Name").GetComponent<TMP_Text>();
//		m_Dropdown.onValueChanged.AddListener(OnValueChanged);
//	}

//	/// <summary>
//	/// 初始化数据
//	/// </summary>
//	/// <param name="setVO">设置VO</param>
//	/// <param name="proxy">设置Proxy</param>
//	/// <param name="setPanel">设置面板</param>
//	public void Init(SetVO setVO, CfgSettingProxy proxy, GameSettingPanel setPanel,Vector2Int ver ,GroupScrollerView view)
//	{
//		this.m_StoreHelperProxy = (StoreHelperProxy)GameFacade.Instance.RetrieveProxy(ProxyName.StoreHelperProxy);
//		this.m_SetVO = setVO;
//		m_GroupScrollerView = view;
//		SetIsDropDown(true);
//		this.m_SetPanel = setPanel;
//		m_CardName.text = proxy.GetLocalization(SystemLanguage.English, setVO.Name);
//		m_Dropdown.ClearOptions();
//		UIEventListener.UIEventListener.AttachListener(m_Dropdown.gameObject, ver).onClick = OnClick;

//		if (setVO.Id == (int)VideoSettingType.ScreenResolution)
//		{
//			foreach (Resolution res in m_StoreHelperProxy.GetResolutions())
//			{
//				TMP_Dropdown.OptionData iterDropItem = new TMP_Dropdown.OptionData();
//				iterDropItem.text = res.width + " x " + res.height + " , " + res.refreshRate + "Hz";
//				m_Dropdown.options.Add(iterDropItem);
//			}
//		}
//		else
//		{
//			for (int iItem = 0; iItem < setVO.OptionLength; iItem++)
//			{
//				TMP_Dropdown.OptionData iterDropItem = new TMP_Dropdown.OptionData();
//				iterDropItem.text = proxy.GetLocalization(SystemLanguage.English, setVO.OptionDsc(setVO.Option(iItem)));
//				m_Dropdown.options.Add(iterDropItem);
//			}
//		}
//		transform.localScale = Vector3.one;
//		m_Dropdown.options.Add(new TMP_Dropdown.OptionData("")); // HACK 为了修复最后一个item不显示
//		m_Dropdown.RefreshShownValue();
//		Refresh(true);
		
//	}

//	/// <summary>
//	/// video quality自定义
//	/// </summary>
//	/// <param name="type">视频设置类型</param>
//	/// <param name="value">参数</param>
//	public void Custom(VideoSettingType type, int value)
//	{
//		CfgSettingProxy proxy = (CfgSettingProxy)GameFacade.Instance.RetrieveProxy(ProxyName.CfgSettingProxy);
//		if (value != proxy.GetSetVOByKey((int)type).Defaults(m_StoreHelperProxy.GetVideoQuality()))
//		{
//			m_StoreHelperProxy.SetVideoQuality(6);
//		}
//	}
//	/// <summary>
//	/// 刷新
//	/// </summary>
//	/// <param name="isRestore">是否刷新</param>
//	public void Refresh(bool isRestore = false)
//	{
//		// Note: 实际Unity是可以改分辨率的, 这个分辨率是渲染用的分辨率，渲染后把渲染结果拉伸到全屏大小
//		//      这里缺少设计，所以先保留原有功能
//		bool isResolutionSettingAndFullscrren = m_SetVO.Id == (int)VideoSettingType.ScreenResolution
//				&& m_StoreHelperProxy.GetDisplayMode() == 0;
//		isResolutionSettingAndFullscrren = false; // HACK 策划决定去掉全屏模式
//		bool isEnable = !isResolutionSettingAndFullscrren;
//		if (isRestore)
//		{
//			m_ShowValue = m_StoreHelperProxy.GetValue(m_SetVO.Id);
//			m_LastValue = m_StoreHelperProxy.GetInt(((VideoSettingType)m_SetVO.Id).ToString());
//		}

//		if (m_SetVO.Id == (int)VideoSettingType.ScreenResolution)
//		{
//			if (isResolutionSettingAndFullscrren)
//			{
//				// HACK 现在不会进入这个分支
//				Resolution currentResolution = m_StoreHelperProxy.GetResolutions()[isEnable ? m_StoreHelperProxy.GetScreenResolution() : m_StoreHelperProxy.GetResolutions().Length - 1];
//				m_Dropdown.captionText.text = currentResolution.width + " x " + currentResolution.height + " , " + currentResolution.refreshRate + "Hz";
//			}
//			else
//			{
//				m_Dropdown.value = m_ShowValue;
//				Resolution currentResolution = (m_ShowValue >= 0 && m_ShowValue < m_StoreHelperProxy.GetResolutions().Length)
//					? m_StoreHelperProxy.GetResolutions()[m_ShowValue]
//					: m_StoreHelperProxy.GetResolutions()[m_StoreHelperProxy.GetResolutions().Length - 1];
//				m_Dropdown.captionText.text = currentResolution.width + " x " + currentResolution.height + " , " + currentResolution.refreshRate + "Hz";
//			}
//		}
//		else
//		{
//			m_Dropdown.value = m_ShowValue;
//			m_Dropdown.captionText.text = m_Dropdown.options[m_ShowValue].text;
//		}
//		m_DropArrow.SetActive(isEnable);
//		//m_Dropdown.gameObject.GetComponent<Animator>().enabled = isEnable;
//		m_Dropdown.interactable = isEnable;
//		m_DropdownText.color = m_ShowValue != m_LastValue ? new Color(191 / 255f, 242 / 255f, 252 / 255f, 1f) : new Color(189 / 255f, 189 / 255f, 189 / 255f, 1f);
//	}
	
//	/// <summary>
//	/// 数值改变
//	/// </summary>
//	/// <param name="value">参数</param>
//	private void OnValueChanged(int value)
//	{
//		if (value >= m_Dropdown.options.Count)
//		{
//			m_Dropdown.value = m_Dropdown.options.Count - 1;
//			return;
//		}

//		switch (m_SetVO.Id)
//		{
//			case (int)VideoSettingType.TextureQuality:
//			case (int)VideoSettingType.LightQuality:
//			case (int)VideoSettingType.AnisotropicTextures:
//				if (m_StoreHelperProxy.GetVideoQuality() != 6)
//				{
//					m_Dropdown.captionText.text = m_Dropdown.captionText.text;
//					Custom((VideoSettingType)m_SetVO.Id, value);
//				}
//				break;
//			case (int)VideoSettingType.ShadowsQuality:
//				m_Dropdown.captionText.text = m_Dropdown.captionText.text;
//				Custom(VideoSettingType.ShadowsQuality, value);
//				break;
//			case (int)VideoSettingType.ScreenResolution:
//			case (int)VideoSettingType.AntiAliasing:
//			case (int)VideoSettingType.DisplayMode:
//				m_Dropdown.captionText.text = m_Dropdown.captionText.text;
//				break;
//		}

//		m_ShowValue = value;
//		m_StoreHelperProxy.SetValue(m_SetVO.Id, value);

//		m_SetPanel.RefreshView();
//	}
//	Vector2Int vector;
//	protected override void OnClick(GameObject go, object[] objs)
//	{
//		vector = (Vector2Int)objs[0];
//		m_GroupScrollerView.SetSelection(vector);
//		m_GroupScrollerView.ScrollToSelection();
//		if (go.transform.Find("Dropdown List") != null)
//		{
//			go.transform.Find("Dropdown List").GetComponent<CanvasGroup>().alpha = 1;
//		}
//	}

//	//public virtual void OnSubmit(BaseEventData eventData)
//	//{
//	//	StartCoroutine(DelayOneFrameReselect());
//	//}

//	//public virtual void OnCancel(BaseEventData eventData)
//	//{
//	//	StartCoroutine(DelayOneFrameReselect());
//	//}

//	//private IEnumerator DelayOneFrameReselect()
//	//{
//	//	yield return new WaitForEndOfFrame();
//	//	m_GroupScrollerView.SetSelection(vector);
//	//	m_GroupScrollerView.ScrollToSelection();
//	//	//...
//	//}
//}