//using PureMVC.Interfaces;
//using System.Collections.Generic;
//using TMPro;
//using UnityEngine;
//using UnityEngine.UI;

/////设置接入GroupScrollerView 后，dropDown 上下键，鼠标进入动画没离开，
///// <summary>
///// 设置面板
///// </summary>
//public class GameSettingPanel : UIPanelBase
//{
//	/// <summary>
//	/// 资源地址
//	/// </summary>
//	private const string ASSET_ADDRESS = "Assets/Artwork/UI/Prefabs/GameSettingPanel.prefab";

//	/// <summary>
//	/// 左边toggle集合
//	/// </summary>
//	private Toggle[] m_LeftToggles;

//	/// <summary>
//	/// 视频list
//	/// </summary>
//	private Transform m_VideoList;

//	/// <summary>
//	/// 声音List
//	/// </summary>
//	private Transform m_SoundList;

//	/// <summary>
//	/// 下拉item
//	/// </summary>
//	private GameObject m_SetDropDownItem;

//	/// <summary>
//	/// 设置Item
//	/// </summary>
//	private GameObject m_SetItem;

//	/// <summary>
//	/// 单个toggle Item
//	/// </summary>
//	private GameObject m_SetToggleItem;

//	/// <summary>
//	/// 设置文本
//	/// </summary>
//	private GameObject m_SetTextItem;

//	/// <summary>
//	/// settingProxy
//	/// </summary>
//	private CfgSettingProxy m_SettingProxy;

//	/// <summary>
//	/// StoreHelperProxy帮助存档
//	/// </summary>
//	private StoreHelperProxy m_StoreHelperProxy;

//	/// <summary>
//	/// 下拉组件集合
//	/// </summary>
//	private List<SetDropDownElement> m_SetDropDownList;

//	/// <summary>
//	/// 设置Slider List
//	/// </summary>
//	private List<SetSliderElement> m_SetSliderCardList;

//	/// <summary>
//	/// 设置setToggle List
//	/// </summary>
//	private List<SetToggleElement> m_SetToggleList;

//	/// <summary>
//	/// 进度条
//	/// </summary>
//	private Image m_Slider;

//	/// <summary>
//	/// 进度条背景
//	/// </summary>
//	private Transform m_SliderBg;

//	/// <summary>
//	/// 点击提示
//	/// </summary>
//	private TMP_Text m_HitMsg;

//	/// <summary>
//	/// 恢复状态
//	/// </summary>
//	private RestoreState m_RestoreState;

//	/// <summary>
//	/// 点击Msg自动隐藏CD
//	/// </summary>
//	private float m_HitMsgAutoHideCD;

//	/// <summary>
//	/// 持续时间
//	/// </summary>
//	private float m_HoldTime;

//	/// <summary>
//	/// 恢复热键
//	/// </summary>
//	private HotKeyState m_RestoreHotKeyState;

//	/// <summary>
//	/// 设置类型
//	/// </summary>
//	private SetType m_SetType;

//	/// <summary>
//	/// 最后改变
//	/// </summary>
//	private bool m_LastHasChanged;

//	/// <summary>
//	/// 左边toggleIndex 
//	/// </summary>
//	private int m_LeftToggleIndex = 0;

//	/// <summary>
//	/// tips顶部描述文本
//	/// </summary>
//	private string[] m_TopTipsTitles;

//	/// <summary>
//	/// tips左侧描述文本
//	/// </summary>
//	private string[] m_LeftTipsDescribe;

//	/// <summary>
//	/// 热键挂点
//	/// </summary>
//	private Transform m_HotKeyRoot;

//	/// <summary>
//	/// 下边toggleIndex 
//	/// </summary>
//	private int m_DownToggleIndex = 0;

//	/// <summary>
//	/// 当前激活的DropDown
//	/// </summary>
//	private TMP_Dropdown m_CurrentDropDown;

//	/// <summary>
//	/// GroupScrollerView滑动组件
//	/// </summary>
//	private GroupScrollerView m_GroupScrollerView;

//	/// <summary>
//	/// 当前被选的toggle
//	/// </summary>
//	private GameObject m_CurrentToggle;

//	/// <summary>
//	/// 页面上ToggleGroup
//	/// </summary>
//	private ToggleGroup m_ToggleGroup;

//	/// <summary>
//	///  账号页面root
//	/// </summary>
//	private Transform m_AccountsRoot;

//	public GameSettingPanel() : base(PanelName.GameSettingPanel, ASSET_ADDRESS, PanelType.Normal)
//	{

//	}

//	/// <summary>
//	/// <see cref="UIPanelBase.Initialize()"/>
//	/// </summary>
//	public override void Initialize()
//	{
//		m_SettingProxy = (CfgSettingProxy)Facade.RetrieveProxy(ProxyName.CfgSettingProxy);
//		m_StoreHelperProxy = (StoreHelperProxy)Facade.RetrieveProxy(ProxyName.StoreHelperProxy);
//		m_VideoList = FindComponent<Transform>("Content/ShowPlace/Container/VideoList/Viewport/Content");
//		m_SoundList = FindComponent<Transform>("Content/ShowPlace/Container/SoundList/Viewport/Content");
//		m_ToggleGroup = FindComponent<ToggleGroup>("Content/ShowPlace/Container/VideoList/Viewport/Content");
//		m_AccountsRoot = FindComponent<Transform>("Content/ShowPlace/Container/AccessibilityList");
//		m_LeftToggles = GetTransform().Find("Control/TopButton/ToggleBar").GetComponentsInChildren<Toggle>(true);//查询子物体组件数组
//		m_Slider = FindComponent<Image>("Content/ShowPlace/Slider/Slider");
//		m_SliderBg = FindComponent<Transform>("Content/ShowPlace/Slider");
//		m_HitMsg = FindComponent<TMP_Text>("Content/ShowPlace/Slider/Label_ShowMsg");
//		m_HotKeyRoot = FindComponent<Transform>("Control/View_Bottom/List");
//		m_GroupScrollerView = FindComponent<GroupScrollerView>("Content/ShowPlace/Container/VideoList");
//		LoadElement();
//		m_ToggleGroup.allowSwitchOff = true;
//		m_SetDropDownList = new List<SetDropDownElement>();
//		m_SetSliderCardList = new List<SetSliderElement>();
//		m_SetToggleList = new List<SetToggleElement>();
//		//to  do 新的设置界面
//		//m_LeftToggles[0].transform.Find("Label_Name").GetComponent<TMP_Text>().text = TableUtil.GetLanguageString(300023);
//		//m_LeftToggles[1].transform.Find("Label_Name").GetComponent<TMP_Text>().text = TableUtil.GetLanguageString(300025);
//		//m_LeftToggles[2].transform.Find("Label_Name").GetComponent<TMP_Text>().text = TableUtil.GetLanguageString(300027);
//		//m_TopTipsTitles = new string[] { TableUtil.GetLanguageString(300023), TableUtil.GetLanguageString(300025), TableUtil.GetLanguageString(300027) };
//		//m_LeftTipsDescribe = new string[] { TableUtil.GetLanguageString(300024), TableUtil.GetLanguageString(300026), TableUtil.GetLanguageString(300028) };
//		m_GroupScrollerView.RenderItem = OnItemRenderer;
//		m_GroupScrollerView.RenderHead = OnHeadRenderer;
//		m_GroupScrollerView.ItemTemplate = m_SetItem.GetComponent<RectTransform>();
//		m_GroupScrollerView.HeadTemplate = m_SetItem.GetComponent<RectTransform>();
//    }

//	/// <summary>
//	/// <see cref="UIPanelBase.OnRefresh(object)"/>
//	/// </summary>
//	public override void OnRefresh(object msg)
//	{
//		GetTransform().parent.GetComponent<Canvas>().pixelPerfect = true; // HAKE 为了解决：右侧横线在低分辨率，transform改变时会闪烁。不确定这样做有没有性能问题
//		m_SliderBg.gameObject.SetActive(false);
//		SetAnimator(true);
//		SelectPage(0, true);
//		m_HitMsgAutoHideCD = 0;
//		m_RestoreState = RestoreState.None;
//		m_RestoreHotKeyState = HotKeyState.None;
//		RefreshView(true);
//	}

//	/// <summary>
//	/// <see cref="UIPanelBase.OnShow(object)"/>
//	/// </summary>
//	public override void OnShow(object objs)
//	{
//		base.OnShow(objs);
//		for (int iToggle = 0; iToggle < m_LeftToggles.Length; iToggle++)
//		{
//			Toggle iterToggle = m_LeftToggles[iToggle];
//			int pageIndex = iToggle;
//			iterToggle.onValueChanged.AddListener((selected) =>
//			{
//				if (selected)
//				{
//					SetAnimator(selected);
//					SelectPage(pageIndex);
//				}
//			});
//		}
//	}

//	/// <summary>
//	/// <see cref="UIPanelBase.OnHide(object)"/>
//	/// </summary>
//	public override void OnHide(object objs)
//	{
//		base.OnHide(objs);
//		for (int iToggle = 0; iToggle < m_LeftToggles.Length; iToggle++)
//		{
//			Toggle iterToggle = m_LeftToggles[iToggle];
//			iterToggle.onValueChanged.RemoveAllListeners();
//		}
//		m_SetDropDownList.Clear();
//		m_SetSliderCardList.Clear();
//		m_SetToggleList.Clear();

//	}

//	public override void OnGotFocus()
//	{
//		base.OnGotFocus();
//		AddHotKey(HotKeyID.NavNegative, Q_Page);
//		AddHotKey(HotKeyID.NavPositive, E_Page);
//		AddHotKey(HotKeyID.NavLeft, Left_Select);
//		AddHotKey(HotKeyID.NavRight, Right_Select);
//		//to  do 新的设置界面
//		//AddHotKey(GameConstant.F, OnClickApply, m_HotKeyRoot, TableUtil.GetLanguageString(122));
//		//AddHotKey(GameConstant.G, RestoreDefault, m_HotKeyRoot, TableUtil.GetLanguageString(202005));
//		//AddHotKey(GameConstant.ENTER, OnClickEnter, m_HotKeyRoot, TableUtil.GetLanguageString(202005));
//		AddHotKey(HotKeyID.FuncB, ClosePanel, m_HotKeyRoot, TableUtil.GetLanguageString("common_hotkey_id_1003"));
//	}
    
//	/// <summary>
//	/// <see cref="UIPanelBase.ListNotificationInterests()"/>
//	/// </summary>
//	public override NotificationName[] ListNotificationInterests()
//	{
//		return base.ListNotificationInterests();
//	}

//	/// <summary>
//	/// <see cref="UIPanelBase.HandleNotification(INotification)"/>
//	/// </summary>
//	public override void HandleNotification(INotification notification)
//	{

//	}

//	/// <summary>
//	/// 加载Element
//	/// </summary>
//	private void LoadElement()
//	{
//		UIManager.Instance.GetUIElement("Assets/Artwork/UI/Prefabs/Element/SetItemElement.prefab", (GameObject prefab) =>
//		{
//			m_SetItem = prefab;
//		});
//	}

//	/// <summary>
//	/// 刷新界面
//	/// </summary>
//	/// <param name="isRestore">是否恢复</param>
//	public void RefreshView(bool isRestore = false)
//	{
//		foreach (SetDropDownElement dropdown in m_SetDropDownList)
//		{
//			dropdown.Refresh(isRestore);
//		}
//		foreach (SetSliderElement slider in m_SetSliderCardList)
//		{
//			slider.Refresh(isRestore);
//		}
//		foreach (var toggle in m_SetToggleList)
//		{
//			toggle.Refresh(isRestore);
//		}
//	}

//	#region 按键
//	/// <summary>
//	/// Q翻页
//	/// </summary>
//	/// <param name="callbackContext"></param>
//	private void Q_Page(HotkeyCallback callbackContext)
//	{
//		if (callbackContext.performed)
//		{
//			Toggle btn = null;
//			m_LeftToggleIndex--;
//			if (m_LeftToggleIndex < 0)
//			{
//				m_LeftToggleIndex = 0;
//			}

//			btn = m_LeftToggles[m_LeftToggleIndex];
//			btn.isOn = true;
//		}
//	}

//	/// <summary>
//	/// E键翻页
//	/// </summary>
//	/// <param name="callbackContext"></param>
//	private void E_Page(HotkeyCallback callbackContext)
//	{
//		if (callbackContext.performed)
//		{
//			Toggle btn = null;
//			m_LeftToggleIndex++;
//			if (m_LeftToggleIndex == m_LeftToggles.Length)
//			{
//				m_LeftToggleIndex = m_LeftToggles.Length - 1;
//			}
//			btn = m_LeftToggles[m_LeftToggleIndex];
//			btn.isOn = true;
//		}
//	}

//	/// <summary>
//	/// 左键选择
//	/// </summary>
//	/// <param name="callbackContext"></param>
//	private void Left_Select(HotkeyCallback callbackContext)
//	{
//		if (callbackContext.performed)
//		{
//			SetElementBase m_CurrentSetActive = m_CurrentToggle.GetComponent<SetElementBase>();
//			if (m_CurrentSetActive != null)
//			{
//				if (m_CurrentSetActive.GetOptionType() == OptionType.Toggle || m_CurrentSetActive.GetOptionType() == OptionType.Slider)
//				{
//					m_CurrentSetActive.LeftButtonClick();
//				}
//			}
//		}
//	}

//	/// <summary>
//	/// 右键选择
//	/// </summary>
//	/// <param name="callbackContext"></param>
//	private void Right_Select(HotkeyCallback callbackContext)
//	{
//		if (callbackContext.performed)
//		{
//			SetElementBase m_CurrentSetActive = m_CurrentToggle.GetComponent<SetElementBase>();
//			if (m_CurrentSetActive != null)
//			{
//				if (m_CurrentSetActive.GetOptionType() == OptionType.Toggle || m_CurrentSetActive.GetOptionType() == OptionType.Slider)
//				{
//					m_CurrentSetActive.RightButtonClick();
//				}
//			}

//		}
//	}

//	#endregion

//	/// <summary>
//	/// dropDown 是否展开
//	/// </summary>
//	/// <returns></returns>
//	public bool DropDownIsExpanded()
//	{
//		for (int i = 0; i < m_SetDropDownList.Count; i++)
//		{
//			if (m_SetDropDownList[i].GetDropdown() != null)
//			{
//				if (m_SetDropDownList[i].GetDropdown().IsExpanded)
//				{
//					m_CurrentDropDown = m_SetDropDownList[i].GetDropdown();
//					return true;
//				}
//			}
//		}
//		return false;
//	}

//	/// <summary>
//	/// 关闭面板
//	/// </summary>
//	public void ClosePanel(HotkeyCallback callbackContext)
//	{
//		if (callbackContext.cancelled)
//		{
//			if (!DropDownIsExpanded())
//			{
//				m_StoreHelperProxy.ReloadData(SetType.Audio);
//				m_StoreHelperProxy.ReloadData(SetType.Video);
//				UIManager.Instance.ClosePanel(this);
//				UIManager.Instance.OpenPanel(PanelName.GameEscPanel);
//			}
//			else
//			{
//				m_CurrentDropDown.Hide();
//			}
//		}
//	}
//	public void OnClickEnter(HotkeyCallback callbackContext)
//	{
//		//if (callbackContext.cancelled)
//		//{
//		//	if (m_CurrentDropDown!=null&& !DropDownIsExpanded())
//		//	{
//		//		//m_CurrentDropDown.Show();
//		//	}
//		//}
//	}

//	/// <summary>
//	/// 点击恢复
//	/// </summary>
//	private void OnClickRestore()
//	{
//		if (m_RestoreHotKeyState == HotKeyState.None)
//		{
//			m_RestoreHotKeyState = HotKeyState.Hold;
//		}
//		else if (m_RestoreHotKeyState == HotKeyState.Up)
//		{
//			m_RestoreHotKeyState = HotKeyState.UpHold;
//		}
//	}

//	/// <summary>
//	/// 恢复默认
//	/// </summary>
//	private void RestoreDefault(HotkeyCallback callbackContext)
//	{
//		if (callbackContext.performed)
//		{
//			OnClickRestore();
//			m_RestoreState = RestoreState.Restored;
//			m_HitMsg.text = "The settings have been restored.";
//			m_RestoreHotKeyState = HotKeyState.UpHold;
//			m_HitMsgAutoHideCD = 5.0f;
//			m_StoreHelperProxy.RestoreToDefault(m_SetType);
//			RefreshView(true);
//		}

//	}

//	/// <summary>
//	/// 点击应用
//	/// </summary>
//	private void OnClickApply(HotkeyCallback callbackContext)
//	{
//		if (!callbackContext.performed)
//		{
//			return;
//		}
//		m_StoreHelperProxy.ApplySetting(m_SetType);
//		RefreshView(true);
//	}

//	/// <summary>
//	/// 渲染分栏标题
//	/// </summary>
//	/// <param name="data"></param>
//	/// <param name="view"></param>
//	private void OnHeadRenderer(object data, Transform view)
//	{
//		/*
//		SetVO setVO = (SetVO)data;
//		SetTextElement setTextCard = view.GetChild(1).GetComponent<SetTextElement>();
//		view.name = "OptionType.Text";
//		setTextCard.SetOptionType(OptionType.Text);
//		setTextCard.Init(setVO, m_SettingProxy);
//		view.GetChild(1).gameObject.SetActive(true);
//		*/
//	}

//	/// <summary>
//	/// 渲染单个格子内容
//	/// </summary>
//	/// <param name="currentIndex">当前index</param>
//	/// <param name="data">数据</param>
//	/// <param name="view">单条面板</param>
//	/// <param name="isSelected">是否选中</param>
//	public void OnItemRenderer(Vector2Int currentIndex, object data, Transform view, bool isSelected)
//	{
//		/*
//		int page = currentIndex.x;
//		int index = currentIndex.y;
//		SetVO setVO = (SetVO)data;
//		Toggle toggle = view.GetComponent<Toggle>();
//		GameObject son = null;
//		for (int i = 0; i < view.childCount; i++)
//		{
//			view.GetChild(i).gameObject.SetActive(false);
//		}
//		switch ((OptionType)setVO.OptionType)
//		{
//			case OptionType.DropDown:
//				SetDropDownElement setDropDownCard = view.GetChild(0).GetComponent<SetDropDownElement>();
//				view.name = index + "|OptionType.DropDown";
//				if (!m_SetDropDownList.Contains(setDropDownCard))
//				{
//					m_SetDropDownList.Add(setDropDownCard);
//				}
//				setDropDownCard.SetOptionType(OptionType.DropDown);
//				son = view.GetChild(0).gameObject;
//				son.SetActive(true);
//				setDropDownCard.Init(setVO, m_SettingProxy, this, currentIndex, m_GroupScrollerView);
//				break;
//			case OptionType.Toggle:
//				SetToggleElement setToggleCard = view.GetChild(2).GetComponent<SetToggleElement>();
//				view.name = index + "|OptionType.Toggle";
//				m_SetToggleList.Add(setToggleCard);
//				setToggleCard.SetOptionType(OptionType.Toggle);
//				son = view.GetChild(2).gameObject;
//				son.SetActive(true);
//				setToggleCard.Init(setVO, m_SettingProxy, this, currentIndex, m_GroupScrollerView);
//				break;
//			case OptionType.Slider:
//				SetSliderElement setCard = view.GetChild(3).GetComponent<SetSliderElement>();
//				view.name = index + "|OptionType.Slider";
//				m_SetSliderCardList.Add(setCard);
//				setCard.SetOptionType(OptionType.Slider);
//				son = view.GetChild(3).gameObject;
//				son.SetActive(true);
//				setCard.Init(setVO, m_SettingProxy, this, currentIndex, m_GroupScrollerView);
//				break;
//			default:
//				break;
//		}
//		if (toggle != null)
//		{
//			toggle.group = m_ToggleGroup;
//			toggle.onValueChanged.RemoveAllListeners();
//			toggle.isOn = isSelected;
//			if (!isSelected)
//			{
//				view.GetComponent<Animator>().SetTrigger("Normal");
//			}
//			toggle.onValueChanged.AddListener((select) =>
//			{
//				if (select)
//				{
//					m_GroupScrollerView.SetSelection(new Vector2Int(page, index));
//				}
//			});
//			SetToggleClick(isSelected, toggle, son, index, (OptionType)setVO.OptionType);
//		}
//		*/
//	}
//	/*
//	/// <summary>
//	/// 通过标签刷新数据
//	/// </summary>
//	/// <param name="list">数据</param>
//	public void RefreshViewByLabel(List<SetVO> list)
//	{
//		m_GroupScrollerView.ClearData();
//		SetVO setVO = new SetVO ();
//		List<int> titleList = new List<int>();
//		for (int i = 0; i < list.Count; i++)
//		{
//			if ((OptionType)list[i].OptionType == OptionType.Text)
//			{
//				titleList.Add(i);
//			}
//		}
//		for (int i = 0; i < titleList.Count; i++)
//		{
//			List<object> datas = new List<object>();
//			for (int j = 0; j < list.Count; j++)
//			{
//				if (i + 1 < titleList.Count)
//				{
//					if (j > titleList[i] && j < titleList[i + 1])
//					{
//						setVO = list[j];
//						datas.Add(setVO);
//					}
//				}
//				else
//				{
//					if (j > titleList[i])
//					{
//						setVO = list[j];
//						datas.Add(setVO);
//					}
//				}
//			}
//			m_GroupScrollerView.AddDatas(list[titleList[i]], datas);
//		}
//		m_GroupScrollerView.SetSelection(new Vector2Int(0, 0));
//		m_GroupScrollerView.ScrollToSelection();
//		m_GroupScrollerView.CellSize = new Vector2(1000, 45);
//		m_GroupScrollerView.CellSpace = new Vector2(0, 5);
//		m_GroupScrollerView.ColumnCount = 1;
//	}
//	*/

//	/// <summary>
//	/// 切换选择页音频，视频，steam
//	/// </summary>
//	/// <param name="index">索引index</param>
//	/// <param name="isInit">是否初始化</param>
//	private void SelectPage(int index, bool isInit = false)
//	{
//		/*
//		m_VideoList.transform.localPosition = Vector3.zero;
//		m_SoundList.transform.localPosition = Vector3.zero;

//		for (int iToggle = 0; iToggle < m_LeftToggles.Length; iToggle++)
//		{
//			var toggle = m_LeftToggles[iToggle];
//			Image backGroud = toggle.transform.Find("Image_Choose").GetComponent<Image>();
//			TMP_Text label = toggle.transform.Find("Label_Name").GetComponent<TMP_Text>();
//			bool isOn = isInit ? iToggle == index : toggle.isOn;
//			if (isOn)
//			{
//				toggle.isOn = true;
//				backGroud.gameObject.SetActive(true);
//				label.color = new Color(255f / 255f, 255f / 255f, 255f / 255f, 255f / 255f);
//			}
//			else
//			{
//				toggle.isOn = false;
//				backGroud.gameObject.SetActive(false);
//				label.color = new Color(125f / 255f, 120f / 255f, 120f / 255f, 255f / 255f);
//			}
//		}
//		switch (index)
//		{
//			case 0:
//				m_SetType = SetType.Video;
//				RefreshViewByLabel(m_SettingProxy.GetOrderSetVOList(SetType.Video));
//				m_AccountsRoot.gameObject.SetActive(false);
//				break;
//			case 1:
//				m_SetType = SetType.Audio;
//				RefreshViewByLabel(m_SettingProxy.GetOrderSetVOList(SetType.Audio));
//				m_AccountsRoot.gameObject.SetActive(false);
//				break;
//			case 2:
//				m_SetType = SetType.Game;
//				m_GroupScrollerView.ClearData();
//				m_AccountsRoot.gameObject.SetActive(true);
//				break;
//		}
//		if (!isInit)
//		{
//			m_StoreHelperProxy.ReloadData(m_SetType);
//			RefreshView(true);
//		}
//		*/
//	}

//	/// <summary>
//	/// 设置动画
//	/// </summary>
//	/// <param name="selected">是否选中</param>
//	private void SetAnimator(bool selected)
//	{
//		if (selected)
//		{
//			foreach (var toggle in m_LeftToggles)
//			{
//				toggle.GetComponent<Animator>().SetBool("IsOn", toggle.isOn);
//			}
//		}
//	}

//	/// <summary>
//	/// 设置toggle点击事件
//	/// </summary>
//	/// <param name="selected">是否选中</param>
//	/// <param name="toggle">toggle</param>
//	/// <param name="index">索引</param>
//	private void SetToggleClick(bool selected, Toggle toggle, GameObject son, int index, OptionType type)
//	{
//		Image backGroud = son.transform.Find("Background/Image_Choose").GetComponent<Image>();
//		m_DownToggleIndex = index;
//		if (selected)
//		{
//			m_CurrentToggle = son;
//			var a = toggle.navigation;
//			a.mode = Navigation.Mode.Automatic;
//			toggle.navigation = a;
//			backGroud.gameObject.SetActive(true);
//			if (type == OptionType.DropDown)
//			{
//				m_CurrentDropDown = m_CurrentToggle.GetComponentInChildren<TMP_Dropdown>(true);
//			}
//			else
//			{
//				m_CurrentDropDown = null;
//			}
//		}
//		else
//		{
//			var a = toggle.navigation;
//			a.mode = Navigation.Mode.None;
//			toggle.navigation = a;
//			backGroud.gameObject.SetActive(false);
//		}
//	}

//	#region 枚举
//	/// <summary>
//	/// 恢复设置状态
//	/// </summary>
//	private enum RestoreState
//	{
//		/// <summary>
//		/// 没触发重置
//		/// </summary>
//		None,

//		/// <summary>
//		/// 将要重置
//		/// </summary>
//		WillRestore,

//		/// <summary>
//		/// 重置后
//		/// </summary>
//		Restored,
//	}

//	/// <summary>
//	/// 热键状态
//	/// </summary>
//	private enum HotKeyState
//	{
//		/// <summary>
//		/// 没按下
//		/// </summary>
//		None,

//		/// <summary>
//		/// 按下
//		/// </summary>
//		Hold,

//		/// <summary>
//		/// 抬起
//		/// </summary>
//		Up,

//		/// <summary>
//		/// 抬起后按住
//		/// </summary>
//		UpHold,
//	}
//	#endregion

//}

