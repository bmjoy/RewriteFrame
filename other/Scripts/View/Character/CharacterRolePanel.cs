using PureMVC.Interfaces;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static ServerListProxy;
using static ServerInfoVO;
using static ConfirmPanel;
using Assets.Scripts.Define;
using UnityEngine.InputSystem;
/// <summary>
/// 选角 角色列表页面
/// </summary>
public class CharacterRolePanel : UIPanelBase
{
	private const string ASSET_ADDRESS = "Assets/Artwork/UI/Prefabs/CharacterRolePanel.prefab";

	/// <summary>
	/// 保存最后登录角色
	/// </summary>
	public const string LAST_LOGIN_ROLE = "LAST_LOGIN_ROLE";

	/// <summary>
	/// 角色列表预设
	/// </summary>
	private GameObject m_CharacterItemPrefab;

	/// <summary>
	/// 角色列表根节点
	/// </summary>
	private Transform m_CharacterItemRoot;

	/// <summary>
	/// 角色列表 toggle list
	/// </summary>
	private List<Toggle> m_CharacterToggleList;

	/// <summary>
	/// 当前角色索引
	/// </summary>
	private int m_CurrentCharacterIndex;

	/// <summary>
	/// 服务器列表Proxy
	/// </summary>
	private ServerListProxy m_ServerListProxy;

	/// <summary>
	/// hotkey节点
	/// </summary>
	private Transform m_HotKeyRoot;

	/// <summary>
	/// 上次按钮索引
	/// </summary>
	private int m_LastSelectIndex;

	/// <summary>
	/// 按住触发事件的时长
	/// </summary>
	private float HOLD_TIME;

	/// <summary>
	/// 标题
	/// </summary>
	private TextMeshProUGUI m_Title;

	/// <summary>
	/// canvasGroup
	/// </summary>
	private CanvasGroup m_CanvasGroup;

	/// <summary>
	/// 内容根节点
	/// </summary>
	private Transform m_ContentBox;

	/// <summary>
	/// 选角列表缓存
	/// </summary>
	private Dictionary<int, Eletment> m_EletmentDic = new Dictionary<int, Eletment>();

	public CharacterRolePanel() : base(UIPanel.CharacterRolePanel, ASSET_ADDRESS, PanelType.Normal) { }

	public override void Initialize()
	{
		m_ServerListProxy = (ServerListProxy)Facade.RetrieveProxy(ProxyName.ServerListProxy);
		m_CharacterItemPrefab = FindComponent<Transform>("Content/ContentBox/CharacterItem").gameObject;
		m_CharacterItemRoot = FindComponent<Transform>("Content/ContentBox/CharacterItemRoot");
        m_HotKeyRoot = FindComponent<Transform>("Control/GameViewFooter/List");
		m_Title = FindComponent<TextMeshProUGUI>("Content/ContentBox/Title/Label_Name");
		m_ContentBox = FindComponent<Transform>("Content/ContentBox");
		m_CanvasGroup = m_ContentBox.GetOrAddComponent<CanvasGroup>();
		m_CharacterToggleList = new List<Toggle>();
		m_Title.transform.parent.gameObject.SetActive(true);
		HOLD_TIME = ((CfgEternityProxy)Facade.RetrieveProxy(ProxyName.CfgEternityProxy)).GetGamingConfig(1).Value.Reading.Value.TimeGeneral;
    }

    public override void OnRefresh(object msg)
    {
        List<CharacterVO> datas = m_ServerListProxy.GetLastLoginServer().CharacterList;
        if (datas != null)
        {
            int index = 0;

            if (PlayerPrefs.HasKey(LAST_LOGIN_ROLE))
            {
                for (int i = 0; i < datas.Count; i++)
                {
                    if (datas[i].UId.ToString() == PlayerPrefs.GetString(LAST_LOGIN_ROLE))
                    {
                        index = i;
                        break;
                    }
                }
            }

            UpdateCharacterList(index);
        }
    }

    public override void OnHide(object msg)
	{
		base.OnHide(msg);

		for (int i = 0; i < m_CharacterItemRoot.childCount; i++)
		{
            Toggle toggle = m_CharacterItemRoot.GetChild(i).GetComponent<Toggle>();
            toggle.onValueChanged.RemoveAllListeners();
            toggle.isOn = false;
		}
		m_CanvasGroup.interactable = true;
	}

	public override NotificationName[] ListNotificationInterests()
	{
		return new NotificationName[]
		{
			NotificationName.MSG_CHARACTER_DEL_SUCC
		};
	}

	public override void HandleNotification(INotification notification)
	{
		switch (notification.Name)
		{
			case NotificationName.MSG_CHARACTER_DEL_SUCC:
				OnCharacterDeleted();
				break;
		}
    }


    /// <summary>
    /// 删除角色回调
    /// </summary>
    private void OnCharacterDeleted()
    {
        List<CharacterVO> datas = m_ServerListProxy.GetLastLoginServer().CharacterList;        
        if (datas.Count == 0)
        {
            UIManager.Instance.ClosePanel(this);

            m_ServerListProxy.SetCurrentState(CharacterPanelState.CreatRole);
        }
        else
        {
            UpdateCharacterList(0);
        }
    }


    #region 热键回调处理

    /// <summary>
    /// 注册热键
    /// </summary>
    public override void OnGotFocus()
    {
        base.OnGotFocus();

		AddHotKey(HotKeyID.FuncA, OnLoginHotkeyCallback, m_HotKeyRoot, TableUtil.GetLanguageString("common_hotkey_id_1004"));
		AddHotKey("f", HotKeyID.FuncY, OnCreateHotkeyCallback, m_HotKeyRoot, TableUtil.GetLanguageString("character_hotkey_1001"));
        AddHotKey(HotKeyID.FuncR3, OnDeleteHotkeyCallback, HOLD_TIME, m_HotKeyRoot, TableUtil.GetLanguageString("character_hotkey_1002"));
	}

	/// <summary>
	/// 处理创建角色热键回调
	/// </summary>
	/// <param name="callbackContext"></param>
	private void OnBackHotkeyCallback(HotkeyCallback callbackContext)
	{
		if (callbackContext.performed)
		{
			NetworkManager.Instance.GetLoginController().ExitCurrentServer(2);
			UIManager.Instance.ClosePanel(UIPanel.CharacterPanel);
			UIManager.Instance.OpenPanel(UIPanel.LoginPanel);
		}
	}

	/// <summary>
	/// 处理登陆热键回调
	/// </summary>
	/// <param name="callbackContext"></param>
	private void OnLoginHotkeyCallback(HotkeyCallback callbackContext)
	{
		if (callbackContext.performed)
		{
			if (callbackContext.isFromKeyboardMouse || callbackContext.isFromUI)
			{
				List<CharacterVO> datas = m_ServerListProxy.GetLastLoginServer().CharacterList;
				if (datas.Count> m_CurrentCharacterIndex)
				{
					PlayerPrefs.SetString(LAST_LOGIN_ROLE, datas[m_CurrentCharacterIndex].UId.ToString());
					m_ServerListProxy.CharacterLogin();
					m_CanvasGroup.interactable = false;
				}
			}
		}
	}

	/// <summary>
	/// 处理创建角色热键回调
	/// </summary>
	/// <param name="callbackContext"></param>
	private void OnCreateHotkeyCallback(HotkeyCallback callbackContext)
    {
        if (callbackContext.performed)
        {
            UIManager.Instance.ClosePanel(this);

            m_ServerListProxy.SetCurrentState(CharacterPanelState.CreatRole);
        }
    } 

    /// <summary>
    /// 处理删除角色热键回调
    /// </summary>
    /// <param name="callbackContext"></param>
    private void OnDeleteHotkeyCallback(HotkeyCallback callbackContext)
    {
        if (callbackContext.performed)
        {
			WwiseUtil.PlaySound((int)WwiseMusic.Music_deleteRole_Over, false, null);
			OpenParameter openParameter = new OpenParameter();
            openParameter.Title = TableUtil.GetLanguageString("common_text_id_1018");
            openParameter.Content = string.Format(TableUtil.GetLanguageString("common_text_id_1017"),
            m_ServerListProxy.GetCurrentCharacterVO().Name, m_ServerListProxy.GetCurrentCharacterVO().Level);
            openParameter.backgroundColor = BackgroundColor.Error;

            HotKeyButton HotKeyQuit = new HotKeyButton();
			HotKeyQuit.actionName = HotKeyID.FuncX;
            HotKeyQuit.showText = TableUtil.GetLanguageString("common_hotkey_id_1001");
            HotKeyQuit.onEvent = OnSubmitDeleteCharacter;

            HotKeyButton HotKeyCancel = new HotKeyButton();
            HotKeyCancel.actionName = HotKeyID.FuncB;
            HotKeyCancel.showText = TableUtil.GetLanguageString("common_hotkey_id_1002");
            HotKeyCancel.onEvent = OnCancelDeleteCharacter;

            openParameter.HotkeyArray = new HotKeyButton[] { HotKeyQuit, HotKeyCancel };
            UIManager.Instance.OpenPanel(UIPanel.ConfirmPanel, openParameter);
			WwiseUtil.PlaySound((int)WwiseMusic.Music_Panel_PopupPanel_Open, false, null);
		}
    }

	/// <summary>
	/// 确认要删除角色
	/// </summary>
	/// <param name="callbackContext"></param>
	private void OnSubmitDeleteCharacter(HotkeyCallback callbackContext)
	{
		if (callbackContext.performed)
		{
			UIManager.Instance.ClosePanel((ConfirmPanel)Facade.RetrieveMediator(UIPanel.ConfirmPanel));
			WwiseUtil.PlaySound((int)WwiseMusic.Music_Panel_PopupPanel_Close, false, null);
			m_ServerListProxy.DelCharacter();
		}
    }

    /// <summary>
    /// 取消删除角色
    /// </summary>
    /// <param name="callbackContext"></param>
    private void OnCancelDeleteCharacter(HotkeyCallback callbackContext)
    {
        if (callbackContext.performed)
        {
			WwiseUtil.PlaySound((int)WwiseMusic.Music_Panel_PopupPanel_Close, false, null);
			UIManager.Instance.ClosePanel((ConfirmPanel)Facade.RetrieveMediator(UIPanel.ConfirmPanel));
        }
    }

    #endregion

    #region 角色列表处理

    /// <summary>
    /// 更新角色列表
    /// </summary>
    private void UpdateCharacterList(int selectIndex)
    {
        List<CharacterVO> datas = m_ServerListProxy.GetLastLoginServer().CharacterList;

        m_CharacterToggleList.Clear();
		//  m_CharacterItemRoot.GetComponent<ToggleGroup>().allowSwitchOff = true;
        //int index = 0;
        int count = datas != null ? datas.Count : 0;
		SetHotKeyEnabled("f", datas.Count < 5);
		m_Title.text = string.Format(TableUtil.GetLanguageString("mailbox_title_1004"),datas.Count,5);
		for (int i = 0; i < count; i++)
        {
            int sign = i;

            CharacterVO data = datas[i];

            Transform item =
				sign < m_CharacterItemRoot.childCount ? 
                m_CharacterItemRoot.GetChild(sign) : 
                GameObject.Instantiate(m_CharacterItemPrefab, m_CharacterItemRoot).transform;
            item.gameObject.SetActive(true);
			
            Toggle toggle = null;
			if (m_EletmentDic.ContainsKey(sign))
			{
				toggle = m_EletmentDic[sign].m_Toggle;
				m_EletmentDic[sign].InitData(data);
			}
			else
			{
				Eletment eletment = new Eletment();
				eletment.InitEletment(item);
				eletment.InitData(data);
				m_EletmentDic.Add(sign,eletment);
				toggle = eletment.m_Toggle;
			}
			toggle.onValueChanged.RemoveAllListeners();
            toggle.group = m_CharacterItemRoot.GetComponent<ToggleGroup>();
            toggle.group.allowSwitchOff = true;
			m_CharacterToggleList.Add(toggle);
			//  toggle.isOn = false;
			m_LastSelectIndex = -1;
			toggle.onValueChanged.AddListener((select) => {
                toggle.group.allowSwitchOff = false;
                OnToggleClick(select, item.gameObject, sign);
            });
			toggle.isOn = false;
        }
		for (int i = count; i < m_CharacterItemRoot.childCount; i++)
		{
			m_CharacterItemRoot.GetChild(i).GetComponent<Toggle>().onValueChanged.RemoveAllListeners();
			m_CharacterItemRoot.GetChild(i).gameObject.SetActive(false);
		}
        m_CharacterToggleList[selectIndex].isOn = true;

        FocusTo(m_CharacterToggleList[selectIndex]);
    }

	/// <summary>
	/// toggle 点击事件
	/// </summary>
	/// <param name="isOn">是否选中</param>
	private void OnToggleClick(bool isOn, GameObject go, int index)
    {
        Animator animator = go.GetComponent<Animator>();
		if (isOn)
		{
            
            animator.SetBool("IsOn", true);
			m_CurrentCharacterIndex = index;
			if (m_LastSelectIndex == index)
			{
				List<CharacterVO> datas = m_ServerListProxy.GetLastLoginServer().CharacterList;
				PlayerPrefs.SetString(LAST_LOGIN_ROLE, datas[index].UId.ToString());
				m_ServerListProxy.CharacterLogin();
				m_CanvasGroup.interactable = false;
			}
			else
			{
				List<CharacterVO> datas = m_ServerListProxy.GetLastLoginServer().CharacterList;
				m_ServerListProxy.SetCurrentCharacterVO(datas[index]);
				m_CharacterToggleList[index].isOn = true;
			}

			m_LastSelectIndex = index;
            //m_CharacterItemRoot.GetComponent<ToggleGroup>().allowSwitchOff = false;
        }
        else
		{
			animator.SetBool("IsOn", false);
            animator.SetTrigger("Normal");
		}
	}

	#endregion

	/// <summary>
	/// 选角元素
	/// </summary>
	public class Eletment
	{
		/// <summary>
		/// 名字Text
		/// </summary>
		private TMP_Text m_NameText;
		/// <summary>
		/// 等级Text
		/// </summary>
		private TMP_Text m_LevelText;
		/// <summary>
		/// 创角图标
		/// </summary>
		private GameObject m_ImageCreateIconObj;
		/// <summary>
		/// 按钮
		/// </summary>
		public Toggle m_Toggle;
		/// <summary>
		/// 获取组件
		/// </summary>
		/// <param name="item">item元素</param>
		public void InitEletment(Transform item)
		{
			m_Toggle = item.GetComponent<Toggle>();
			m_NameText = item.transform.Find("Label_NameField").GetComponent<TMP_Text>();
			m_LevelText = item.transform.Find("Label_LeverField").GetComponent<TMP_Text>();
			m_ImageCreateIconObj = item.transform.Find("Image_CreateIcon").gameObject;
			m_ImageCreateIconObj.SetActive(false);
		}

		/// <summary>
		/// 初始化数据
		/// </summary>
		/// <param name="data">数据</param>
		public void InitData(CharacterVO data)
		{
			m_NameText.text = data.Name;
			m_LevelText.text = string.Format(TableUtil.GetLanguageString("character_text_1019"), data.Level);
		}
	}
}
