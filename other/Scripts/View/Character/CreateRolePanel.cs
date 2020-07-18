using Assets.Scripts.Define;
using PureMVC.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static ServerListProxy;
/// <summary> 
/// 创建角色面板    集合 CharacterGenderPanel性别  CharacterNamePanel CharacterSkinPanel
/// </summary>
public class CreateRolePanel : UIPanelBase
{
	#region 变量
	private const string ASSET_ADDRESS = "Assets/Artwork/UI/Prefabs/CreateRolePanel.prefab";

	private const string ROLESKINELEMENTASSET_ADDRESS = "Assets/Artwork/UI/Prefabs/Element/CreateRoleElement.prefab"; 
	/// <summary>
	/// 名字最小长度
	/// </summary>
	private const int NAME_MIN_LENGTH = 2;

	/// <summary>
	/// 名字最大长度
	/// </summary>
	private const int NAME_MAX_LENGTH = 17;

	/// <summary>
	/// 随机起名最大失败次数
	/// </summary>
	private const int RANDOM_NAME_TRYCOUNT = 3;

	/// <summary>
	/// 性别个数
	/// </summary>
	private const int SEX_COUNT = 2;

	/// <summary>
	/// 创角Proxy
	/// </summary>
	private ServerListProxy m_ServerListProxy;

	/// <summary>
	/// 角色Proxy
	/// </summary>
	private CfgEternityProxy m_CfgEternityProxy;

	/// <summary>
	/// 当前随机起名失败次数
	/// </summary>
	private int m_RandomRetryCount;

	/// <summary>
	/// 顶部两个按钮组 性别和皮肤
	/// </summary>
	private Toggle[] m_TopToggles;

	/// <summary>
	/// 性别按钮组
	/// </summary>
	private Toggle[] m_GenderToggles;

	/// <summary>
	/// 皮肤按钮组
	/// </summary>
	private Toggle[] m_SkinToggles;

	/// <summary>
	/// 性别按钮根节点
	/// </summary>
	private Transform m_GenderToggleRoot;

	/// <summary>
	/// 皮肤按钮根节点
	/// </summary>
	private Transform m_SkinToggleRoot;

	/// <summary>
	/// 性别根节点
	/// </summary>
	private Transform m_GenderRoot;

	/// <summary>
	/// 皮肤根节点
	/// </summary>
	private Transform m_SkinRoot;

	/// <summary>
	/// 当前层级
	/// </summary>
	private int m_CurrentTier;

	/// <summary>
	/// 性别和皮肤按扭index
	/// </summary>
	private int m_TopTogglesIndex;

	/// <summary>
	/// 皮肤层级index
	/// </summary>
	private int m_SkinTierIndex;

	/// <summary>
	/// 性别层级index
	/// </summary>
	private int m_GenderTierIndex;

	/// <summary>
	/// 皮肤按钮数
	/// </summary>
	private int m_SkinToggleCount;

	/// <summary>
	/// 当前toggle组
	/// </summary>
	private Toggle[] m_CurrentToggles;

	/// <summary>
	/// 起名Transform
	/// </summary>
	private Transform m_NameTransform;

	/// <summary>
	/// 限制条件Text
	/// </summary>
	private TMP_Text m_LimitingText;

	/// <summary>
	/// 热键挂点
	/// </summary>
	private Transform m_HotKeyRoot;

	/// <summary>
	/// 起名面板警告文本
	/// </summary>
	private TMP_Text m_WarningNameText;

	/// <summary>
	/// 起名面板名字输入框
	/// </summary>
	private TMP_InputField m_NamePanelInput;

	/// <summary>
	/// 起名面板确认键root
	/// </summary>
	private Transform m_HotKeyRootName;

	/// <summary>
	/// 是否打开起名面板
	/// </summary>
	private bool m_IsOpenName;

	/// <summary>
	/// 面板toggle item 
	/// </summary>
	private GameObject m_ToggleItem;

	/// <summary>
	/// 性别颜色Image 避免重复获取
	/// </summary>
	private Image[] m_SexIconImage;

	/// <summary>
	/// 皮肤颜色Image 避免重复获取
	/// </summary>
	private Image[] m_SkinIconImage;

	/// <summary>
	/// 性别list组件
	/// </summary>
	private GroupScrollerView m_GroupScrollerViewSex;

	/// <summary>
	/// 皮肤list组件
	/// </summary>
	private GroupScrollerView m_GroupScrollerViewSkin;
	
	/// <summary>
	/// q键挂点
	/// </summary>
	private Transform m_HotKeyRootQ;

	/// <summary>
	/// e键挂点
	/// </summary>
	private Transform m_HotKeyRootE;

	/// <summary>
	/// 名字是否符合
	/// </summary>
	private bool m_NameConform;

	/// <summary>
	/// 是否创建角色
	/// </summary>
	private bool m_IsCreatRole;

	/// <summary>
	/// 按键是否按下
	/// </summary>
	//private bool m_HotKeyDown;

	/// <summary>
	/// 性别语言
	/// </summary>
	private string[] m_SexLanguage;

	/// <summary>
	/// 皮肤语言
	/// </summary>
	private string[] m_SkinLanguage;

	/// <summary>
	/// 按住触发事件的时长
	/// </summary>
	private float HOLD_TIME;

	private int [] m_MaleIcons;
	#endregion
	public CreateRolePanel() : base(UIPanel.CreateRolePanel, ASSET_ADDRESS, PanelType.Normal)
	{

	}
	public override void Initialize()
	{
		m_ServerListProxy = (ServerListProxy)Facade.RetrieveProxy(ProxyName.ServerListProxy);
		m_CfgEternityProxy = (CfgEternityProxy)Facade.RetrieveProxy(ProxyName.CfgEternityProxy);
        HOLD_TIME = m_CfgEternityProxy.GetGamingConfig(1).Value.Reading.Value.TimeGeneral;

        m_TopToggles = FindComponentsInChildren<Toggle>("Content/ToggleTitle");
		m_GenderToggleRoot = FindComponent<Transform>("Content/SexList/Viewport/Content");
		m_SkinToggleRoot = FindComponent<Transform>("Content/SkinList/Viewport/Content");
		m_GenderRoot = FindComponent<Transform>("Content/SexList");
		m_SkinRoot = FindComponent<Transform>("Content/SkinList");
		m_HotKeyRootQ = FindComponent<Transform>("Content/ToggleTitle/Hotkey");
		m_HotKeyRootE = FindComponent<Transform>("Content/ToggleTitle/Hotkey2");
		m_GroupScrollerViewSex = FindComponent<GroupScrollerView>("Content/SexList");
		m_GroupScrollerViewSkin = FindComponent<GroupScrollerView>("Content/SkinList");
        m_HotKeyRootQ.GetComponent<CanvasGroup>().ignoreParentGroups = true;
        m_HotKeyRootQ.GetComponent<CanvasGroup>().blocksRaycasts = true;
        m_HotKeyRootE.GetComponent<CanvasGroup>().ignoreParentGroups = true;
        m_HotKeyRootE.GetComponent<CanvasGroup>().blocksRaycasts = true;
        m_NameTransform = FindComponent<Transform>("Name");
		m_LimitingText = FindComponent<TMP_Text>("Name/Content/Name/Label_Des");
		m_WarningNameText = FindComponent<TMP_Text>("Name/Content/Waring/Label_Des");
		m_NamePanelInput = FindComponent<TMP_InputField>("Name/Content/NameInput");
		m_HotKeyRoot = FindComponent<Transform>("Control/GameViewFooter/List");
		m_HotKeyRootName= m_NameTransform.Find("Control/List");
        m_GenderRoot.gameObject.SetActive(true);
		m_LimitingText.text = TableUtil.GetLanguageString("character_text_1006");
		m_SexLanguage = new string[] { "character_title_1009", "character_title_1008" };
		m_SkinLanguage = new string[] { "character_text_1018", "character_text_1017", "character_text_1016" };
		m_MaleIcons = new int [] {33004, 33003 };
		UIManager.Instance.GetUIElement(ROLESKINELEMENTASSET_ADDRESS, (GameObject prefab) =>
		{
			m_ToggleItem = prefab;
			m_ToggleItem.CreatePool(1, ROLESKINELEMENTASSET_ADDRESS);
		});
		m_GroupScrollerViewSkin.RenderItem = OnItemRendererSkin;
		m_GroupScrollerViewSkin.ItemTemplate = m_ToggleItem.GetComponent<RectTransform>();
		m_GroupScrollerViewSex.RenderItem = OnItemRendererSex;
		m_GroupScrollerViewSex.ItemTemplate = m_ToggleItem.GetComponent<RectTransform>();
		m_GroupScrollerViewSkin.ColumnCount = 1;
		m_GroupScrollerViewSkin.CellSize = new Vector2(336,88);
        m_GroupScrollerViewSkin.CellSpace = new Vector2(0, 20);
		m_GroupScrollerViewSex.ColumnCount = 1;
		m_GroupScrollerViewSex.CellSize = new Vector2(336, 88);
        m_GroupScrollerViewSex.CellSpace = new Vector2(0, 20);
		LoadSexToggle(true);
		LoadSkinToggle(true);
    }

	public override void OnShow(object msg)
	{
		base.OnShow(msg);

		for (int i = 0; i < m_TopToggles.Length; i++)
		{
			int index = i;

            Toggle toggle = m_TopToggles[i];
            toggle.onValueChanged.RemoveAllListeners();
            toggle.isOn = false;
            toggle.onValueChanged.AddListener((select) => OnTopToggleClick(index, select));
		}

		if (SettingINI.Setting.GetBoolValue(SettingINI.Setting.CombineKey(SettingINI.Constants.GROUP_LOGIN
			   , SettingINI.Constants.KEY_AUTO_CREATE_ROLE)
		   , false))
		{
			CoroutineHelper.GetInstance().StartCoroutine(AutoCreateRole());
		}
	}

	private IEnumerator AutoCreateRole()
	{
		yield return new WaitForSeconds(1.0f);

		m_ServerListProxy.CreateCharacter(Guid.NewGuid().ToString().Replace("-", "").Substring(3, 10));
	}

	public override void OnRefresh(object msg)
    {
        m_TopTogglesIndex = 0;
        m_GenderTierIndex = 0;
        m_SkinTierIndex = 0;

        m_NamePanelInput.text = "";
        m_WarningNameText.gameObject.SetActive(false);

		UIManager.Instance.StartCoroutine(Excute(0.1f, () => {
			m_GenderToggles = m_GenderToggleRoot.GetComponentsInChildren<Toggle>();
			m_SkinToggles = m_SkinToggleRoot.GetComponentsInChildren<Toggle>();
			m_TopToggles[0].isOn = true;
			m_TopToggles[0].GetComponent<Animator>().SetBool("IsOn", true);
			m_GenderToggles[m_GenderTierIndex].isOn = false;
			m_GenderToggles[m_GenderTierIndex].isOn = true;
			m_GenderToggles[m_GenderTierIndex].GetComponent<Animator>().SetBool("IsOn", true);
		}));

		m_IsOpenName = false;
        m_NameTransform.gameObject.SetActive(false);
		m_IsCreatRole = false;
		m_NameConform = false;
    }

    public override void OnHide(object msg)
	{
		base.OnHide(msg);

		for (int i = 0; i < m_TopToggles.Length; i++)
		{
			m_TopToggles[i].onValueChanged.RemoveAllListeners();
		}
		m_NameTransform.gameObject.SetActive(false);
	}

	public override void OnGotFocus()
	{
		base.OnGotFocus();
		AddHotKey(HotKeyID.NavNegative, HotKeyQOnClick, m_HotKeyRootQ, "", HotkeyManager.HotkeyStyle.UI_SIMPLE);
		AddHotKey(HotKeyID.NavPositive, HotKeyEOnClick, m_HotKeyRootE, "", HotkeyManager.HotkeyStyle.UI_SIMPLE);
		AddHotKey(HotKeyID.FuncX, NameInputOnClick, m_HotKeyRoot, TableUtil.GetLanguageString("character_hotkey_1003"));
		AddHotKey("esc", HotKeyID.FuncB, BackOnClick, m_HotKeyRoot, TableUtil.GetLanguageString("gameset_hotkey_1002"));
		SetHotKeyEnabled("esc", HaveCharacter());
	}

	public override NotificationName[] ListNotificationInterests()
	{
		return new NotificationName[]
		{
			NotificationName.MSG_CHARACTER_ROLLNAME_SUCC,
			NotificationName.MSG_CHARACTER_ROLLNAME_FAIL,
			NotificationName.MSG_CHARACTER_CREATE_FAIL
		};
	}

	public override void HandleNotification(INotification notification)
	{
		switch (notification.Name)
		{
			case NotificationName.MSG_CHARACTER_ROLLNAME_SUCC:
				m_NamePanelInput.text = notification.Body as string;
				EventSystemUtils.SetFocus(m_NamePanelInput);
				m_NamePanelInput.stringPosition = m_NamePanelInput.text.Length;
				break;
			case NotificationName.MSG_CHARACTER_ROLLNAME_FAIL:
				if (--m_RandomRetryCount > 0)
				{
					m_ServerListProxy.RandomName();
				}
				break;
			case NotificationName.MSG_CHARACTER_CREATE_FAIL:
				OnCharacterCreateFail(notification.Body);
				break;
		}
	}

    /// <summary>
    /// 选择性别或者皮肤按钮
    /// </summary>
    /// <param name="index">toggle 索引</param>
    /// <param name="select">是否点击</param>
    private void OnTopToggleClick(int index, bool select)
    {
        Animator animator = m_TopToggles[index].GetComponent<Animator>();
        if (select)
        {
            m_TopTogglesIndex = index;
            animator.SetBool("IsOn", true);

            m_GenderRoot.gameObject.SetActive(index == 0);
            m_SkinRoot.gameObject.SetActive(index == 1);

            GroupScrollerView scroller = index == 0 ? m_GroupScrollerViewSex : m_GroupScrollerViewSkin;
            int selectedIndex = index == 0 ? m_GenderTierIndex : m_SkinTierIndex;
            GameObject scrollerRow = scroller.SetSelection(new Vector2Int(0, selectedIndex));
            if (scrollerRow)
            {
                scrollerRow.GetComponent<Toggle>().isOn = true;
                scrollerRow.GetComponent<Animator>().SetBool("IsOn", true);

                FocusTo(scrollerRow.GetComponent<Toggle>());
            }
        }
        else
        {
            animator.SetTrigger("Normal");
            animator.SetBool("IsOn", false);
        }
    }

    #region 性别列表

    /// <summary>
    /// 加载性别按钮
    /// </summary>
    /// <param name="select"></param>
    private void LoadSexToggle(bool select)
	{
		if (select)
		{
			m_GroupScrollerViewSex.ClearData();
			List<object> dataSex = new List<object>();
			for (int i = 0; i < SEX_COUNT; i++)
			{
				dataSex.Add(i);
			}
			m_GroupScrollerViewSex.AddDatas(null, dataSex);
			m_GroupScrollerViewSex.UpdateList();
		}
		else
		{
			m_GroupScrollerViewSex.ClearData();
		}
    }

    public void OnItemRendererSex(Vector2Int currentIndex, object data, Transform view, bool isSelected)
    {
        int i = (int)data;
        int page = currentIndex.x;
        int index = currentIndex.y;
        Toggle toggle = view.GetComponent<Toggle>();
        view.GetComponent<Toggle>().group = m_GenderToggleRoot.GetComponent<ToggleGroup>();
        view.Find("Label").GetComponent<TMP_Text>().text = TableUtil.GetLanguageString(m_SexLanguage[i]);
        UIUtil.SetIconImageSquare(view.Find("Image_Icon").GetComponent<Image>(), (uint)m_MaleIcons[i]);
        view.localScale = Vector3.one;
        toggle.onValueChanged.RemoveAllListeners();
        toggle.isOn = false;
        Animator animator = view.GetComponent<Animator>();
        toggle.onValueChanged.AddListener((select) => SelectGenderClick(toggle, index, select));
        //FocusTo(toggle);
    }

    /// <summary>
    /// 点击选择具体性别 
    /// </summary>
    /// <param name="index">toggle 索引</param>
    /// <param name="select">是否点击</param>
    private void SelectGenderClick(Toggle toggle, int index, bool select)
    {
        Animator animator = toggle.GetComponent<Animator>();
        if (select)
        {
            animator.SetBool("IsOn", true);
            m_GenderTierIndex = index;
            switch (index)
            {
                case 0:
                    m_ServerListProxy.SetChangeGender(true);
                    break;
                case 1:
                    m_ServerListProxy.SetChangeGender(false);
                    break;
                default:
                    break;
            }
        }
        else
        {
            animator.SetTrigger("Normal");
            animator.SetBool("IsOn", false);
        }
    }

    #endregion

    #region 皮肤列表

    /// <summary>
    /// 加载皮肤按钮
    /// </summary>
    /// <param name="select"></param>
    private void LoadSkinToggle(bool select)
	{
		if (select)
		{
			m_GroupScrollerViewSkin.ClearData();
			int count = m_ServerListProxy.PlayerListCount();
			List<object> dataSkin = new List<object>();
			for (int i = 0; i < count; i++)
			{
				dataSkin.Add(i);
			}
			m_GroupScrollerViewSkin.AddDatas(null, dataSkin);
			m_GroupScrollerViewSkin.UpdateList();
		}
		else
		{
			m_GroupScrollerViewSex.ClearData();
		}
    }
    public void OnItemRendererSkin(Vector2Int currentIndex, object data, Transform view, bool isSelected)
    {
        int i = (int)data;
        int page = currentIndex.x;
        int index = currentIndex.y;
        Toggle toggle = view.GetComponent<Toggle>();
        view.GetComponent<Toggle>().group = m_SkinToggleRoot.GetComponent<ToggleGroup>();
        view.localScale = Vector3.one;
        view.Find("Label").GetComponent<TMP_Text>().text = TableUtil.GetItemName("player_name_{0}", (int)m_CfgEternityProxy.GetMalePlayerByIndex(index).Id);
        UIUtil.SetIconImageSquare(view.Find("Image_Icon").GetComponent<Image>(),
        (uint)m_CfgEternityProxy.GetMalePlayerByIndex(index).Icon);
        toggle.onValueChanged.RemoveAllListeners();
        toggle.isOn = false;
        Animator animator = view.GetComponent<Animator>();
        toggle.onValueChanged.AddListener((select) => SelectSkinClick(toggle, index, select));
        //FocusTo(toggle);
    }

    /// <summary>
    /// 点击选择具体皮肤
    /// </summary>
    /// <param name="index">toggle 索引</param>
    /// <param name="select">是否点击</param>
    private void SelectSkinClick(Toggle toggle, int index, bool select)
    {
        Animator animator = toggle.GetComponent<Animator>();
        if (select)
        {
            //	Debug.LogError("按下+"+index);
            animator.SetBool("IsOn", true);
            m_SkinTierIndex = index;
            m_ServerListProxy.SetChangeSkinIndex(index);
        }
        else
        {
            animator.SetTrigger("Normal");
            animator.SetBool("IsOn", false);
        }
    }

    #endregion



    /// <summary>
    /// 起名文本框文字改变
    /// </summary>
    /// <param name="name">名字</param>
    private void NameInputOnChange(string name)
	{
		m_NamePanelInput.text = name.Replace("\n", "").Replace(" ", "").Replace("\t", "").Replace("\r", "");
		m_NameConform = m_NamePanelInput.text.Length > NAME_MIN_LENGTH && m_NamePanelInput.text.Length < NAME_MAX_LENGTH;
		bool s = (m_NamePanelInput.text.Length > NAME_MIN_LENGTH && m_NamePanelInput.text.Length < NAME_MAX_LENGTH) ? true : false;
		m_WarningNameText.gameObject.SetActive(!s);
		if (m_NamePanelInput.text.Length <= NAME_MIN_LENGTH)
		{
			m_WarningNameText.text = TableUtil.GetLanguageString("character_text_1014");
		}
		else if (m_NamePanelInput.text.Length >= NAME_MAX_LENGTH)
		{
			m_WarningNameText.text = TableUtil.GetLanguageString("character_text_1013");
		}
		else
		{
			m_WarningNameText.text = "";
		}
	}

	#region 按键操作
	/// <summary>
	/// 随机起名点击之后
	/// 如果失败 自动重试3次
	/// </summary>
	/// <param name="callbackContext"></param>
	private void RandomOnClick(HotkeyCallback callbackContext)
	{
		if (callbackContext.performed)
		{
			if (m_IsOpenName)
			{
				m_RandomRetryCount = RANDOM_NAME_TRYCOUNT;
				m_ServerListProxy.RandomName();
			}
		}
	}

	/// <summary>
	/// Q键按下 
	/// </summary>
	/// <param name="callbackContext"></param>
	private void HotKeyQOnClick(HotkeyCallback callbackContext)
	{
		if (callbackContext.performed)
		{
			if (m_TopTogglesIndex>0)
			{
				m_TopTogglesIndex--;
				InputManager.Instance.SetSpecialUISelected( m_TopToggles[m_TopTogglesIndex].gameObject);
				m_TopToggles[m_TopTogglesIndex].isOn = true;
			}
			else
			{
				ButtonWithSound.Msg_PlayOutLineSound(m_TopToggles[0].gameObject);
			}

			// 
		}
	}

	/// <summary>
	/// E键按下 
	/// </summary>
	/// <param name="callbackContext"></param>
	private void HotKeyEOnClick(HotkeyCallback callbackContext)
	{
		if (callbackContext.performed)
		{
			if (m_TopTogglesIndex < m_TopToggles.Length - 1)
			{
				m_TopTogglesIndex++;
				InputManager.Instance.SetSpecialUISelected( m_TopToggles[m_TopTogglesIndex].gameObject);
				m_TopToggles[m_TopTogglesIndex].isOn = true;
			}
			else
			{
				ButtonWithSound.Msg_PlayOutLineSound(m_TopToggles[m_TopToggles.Length - 1].gameObject);
			}
		}
	}


	/// <summary>
	/// 空格键按下 确认进入下层级选择
	/// </summary>
	/// <param name="callbackContext"></param>
	private void HotKeyXOnClick(HotkeyCallback callbackContext)
	{
		if (callbackContext.performed)
		{
            m_NameConform = m_NamePanelInput.text.Length > NAME_MIN_LENGTH && m_NamePanelInput.text.Length < NAME_MAX_LENGTH;
            if (m_IsOpenName && m_NameConform)
			{
				CreateRoleClick();
				m_NameConform = false;
				m_IsCreatRole = true;
				m_NamePanelInput.interactable = false;
				SetHotKeyEnabled("tab", false);
				SetHotKeyEnabled("input_x", false);
				SetHotKeyEnabled("input_esc", false);
			}
			else if (m_IsOpenName && !m_NameConform && !m_IsCreatRole)
			{
                if (m_NamePanelInput.text.Length <= NAME_MIN_LENGTH)
                {
                    m_WarningNameText.gameObject.SetActive(true);
                    m_WarningNameText.text = TableUtil.GetLanguageString("character_text_1014");
                    Debug.Log(m_NamePanelInput.text);
                }
                else if (m_NamePanelInput.text.Length >= NAME_MAX_LENGTH)
                {
                    m_WarningNameText.gameObject.SetActive(true);
                    m_WarningNameText.text = TableUtil.GetLanguageString("character_text_1013");
                    Debug.Log(m_NamePanelInput.text);
                }
                else
                {
                    Debug.Log(m_NamePanelInput.text);
                }
            }
			
		}
	}

	/// <summary>
	/// back 键按下 返回上级选择
	/// </summary>
	/// <param name="callbackContext"></param>
	private void BackOnClick(HotkeyCallback callbackContext)
	{
		if (callbackContext.performed)
		{
			if (m_IsOpenName)
			{
				CloseNamePanel();
			}
			else
			{
				UIManager.Instance.ClosePanel(this);
				m_ServerListProxy.SetCurrentState(CharacterPanelState.RoleList);
			}
		}
	}

	#endregion


	#region 无修改
	
	/// <summary>
	/// 创角失败提示
	/// </summary>
	/// <param name="values">参数</param>
	public void OnCharacterCreateFail(params object[] values)
	{
		int id = (int)values[0];
		string errTex = "";
		m_WarningNameText.gameObject.SetActive(true);
		if ((KCreateRoleRespondCode)id != KCreateRoleRespondCode.eCreateRoleSucceed)
		{
			m_NamePanelInput.interactable = true;
			EventSystemUtils.SetFocus(m_NamePanelInput);
			SetHotKeyEnabled("tab", true);
			SetHotKeyEnabled("input_x", true);
			SetHotKeyEnabled("input_esc", true);
		}
		switch ((KCreateRoleRespondCode)id)
		{
			case KCreateRoleRespondCode.eCreateRoleSucceed:
				m_WarningNameText.gameObject.SetActive(false);
				break;
			case KCreateRoleRespondCode.eCreateRoleNameAlreadyExist:
				errTex = TableUtil.GetLanguageString("character_text_1011");
				break;
			case KCreateRoleRespondCode.eCreateRoleInvalidRoleName:
				errTex = TableUtil.GetLanguageString("character_text_1012");
				break;
			case KCreateRoleRespondCode.eCreateRoleNameTooLong:
				errTex = TableUtil.GetLanguageString("character_text_1013");
				break;
			case KCreateRoleRespondCode.eCreateRoleNameTooShort:
				errTex = TableUtil.GetLanguageString("character_text_1014");
				break;
			default:
				errTex = TableUtil.GetLanguageString("character_text_1015");
				break;
		}
		m_WarningNameText.text = errTex;
	}

	/// <summary>
	/// 创建角色
	/// </summary>
	private void CreateRoleClick()
	{
		m_ServerListProxy.CreateCharacter(m_NamePanelInput.text);
	}

	/// <summary>
	/// 查找子物体组件集合
	/// </summary>
	/// <typeparam name="T">泛型类</typeparam>
	/// <param name="path">路径</param>
	/// <returns></returns>
	private T[] FindComponentsInChildren<T>(string path) where T : Component
	{
		Transform result = GetTransform().Find(path);
		return result.GetComponentsInChildren<T>();
	}

	/// <summary>
	/// 是否有角色
	/// </summary>
	/// <returns>返回Bool</returns>
	private bool HaveCharacter()
	{
		ServerInfoVO serverInfo = m_ServerListProxy.GetLastLoginServer();
		if (serverInfo.CharacterList?.Count > 0)
		{
			return true;
		}
		else
		{
			return false;
		}
	}

	/// <summary>
	/// 点击起名文本
	/// </summary>
	/// <param name="go">点击物体</param>
	/// <param name="args">参数</param>
	private void NameInputOnClick(HotkeyCallback callbackContext)
	{
		if (callbackContext.performed)
		{
			m_IsOpenName = true;
			OnLostFocus();
			AddHotKey("tab", HotKeyID.FuncLT, RandomOnClick, m_HotKeyRootName, TableUtil.GetLanguageString("character_hotkey_1004"));
			AddHotKey("input_x", HotKeyID.FuncX , HotKeyXOnClick, m_HotKeyRootName, TableUtil.GetLanguageString("common_hotkey_id_1001"));
			AddHotKey("input_esc", HotKeyID.FuncB, BackOnClick, m_HotKeyRootName, TableUtil.GetLanguageString("common_hotkey_id_1002"));
			m_NameTransform.gameObject.SetActive(true);
			SetHotKeyEnabled("tab",true);
			SetHotKeyEnabled("input_x", true);
			SetHotKeyEnabled("input_esc", true);
			EventSystemUtils.SetFocus(m_NamePanelInput);
			m_NamePanelInput.interactable = true;
			m_NamePanelInput.text = "";
			m_NamePanelInput.onValueChanged.AddListener(NameInputOnChange);
            GameFacade.Instance.SendNotification(NotificationName.MSG_CHARACTER_MODEL_ROTATE,false);
            //起名声音
            WwiseUtil.PlaySound((int)WwiseMusic.Music_Panel_PopupPanel_Open, false, null);
		}
	}

	/// <summary>
	/// 关闭起名面板
	/// </summary>
	public void CloseNamePanel()
	{
		EventSystemUtils.SetFocus(null);
		m_NamePanelInput.onValueChanged.RemoveListener(NameInputOnChange);
		m_IsOpenName = false;
		m_WarningNameText.text = "";
		//m_NamePanelInput.text = "";
        DeleteHotKey("input_x");
        DeleteHotKey("input_esc");//注销起名面板上的enter键和esc键
		DeleteHotKey("tab");
		m_NameTransform.gameObject.SetActive(false);
		OnGotFocus();
        GameFacade.Instance.SendNotification(NotificationName.MSG_CHARACTER_MODEL_ROTATE, true);
        //起名声音
        WwiseUtil.PlaySound((int)WwiseMusic.Music_Panel_PopupPanel_Close, false, null);

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
	#endregion
}
