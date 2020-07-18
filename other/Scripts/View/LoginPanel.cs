using PureMVC.Interfaces;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoginPanel : UIPanelBase
{
    /// <summary>
    /// PlayerPrefs key
    /// </summary>
    private const string LAST_LOGIN_USER = "LAST_LOGIN_USER";

    /// <summary>
    /// asset
    /// </summary>
    private const string ASSET_ADDRESS = "Assets/Artwork/UI/Prefabs/LoginPanel.prefab";

    /// <summary>
    /// 验证proxy
    /// 处理验证登陆所有业务逻辑
    /// </summary>
    private LoginProxy m_LoginProxy;

    /// <summary>
    /// canvasGroup
    /// </summary>
    private CanvasGroup m_CanvasGroup;

    /// <summary>
    /// 用户名、密码输入组件组合节点
    /// </summary>
    private GameObject m_CenterContent;

    /// <summary>
    /// nameInput
    /// </summary>
    private TMP_InputField m_NameInput;

    /// <summary>
    /// passwordInput
    /// </summary>
    private TMP_InputField m_PassInput;

	/// <summary>
	/// 版本号
	/// </summary>
	private TextMeshProUGUI m_VersionText;

	/// <summary>
	/// 登陆按钮
	/// </summary>
	private Button m_BtnLogin;

    /// <summary>
    /// 服务器列表按钮
    /// </summary>
    private Button m_BtnServerList;

    /// <summary>
    /// Loading根节点物体
    /// </summary>
    private GameObject m_LoadingObj;
    /// <summary>
    /// focus 索引
    /// </summary>
    private int m_IndexFocus;
    public LoginPanel() : base(UIPanel.LoginPanel, ASSET_ADDRESS, PanelType.Normal)
    {

    }

    public override void Initialize()
    {
        m_CanvasGroup = FindComponent<CanvasGroup>("Content");
        m_CenterContent = FindComponent<RectTransform>("Content/CenterPanel").gameObject;
        m_NameInput = FindComponent<TMP_InputField>("Content/CenterPanel/Input_Name");
        m_PassInput = FindComponent<TMP_InputField>("Content/CenterPanel/Input_Password");
        m_BtnLogin = FindComponent<Button>("Content/CenterPanel/Btn_Enter");
        m_BtnServerList = FindComponent<Button>("Content/CenterPanel/Btn_ServerList");
        m_LoadingObj = FindComponent<Transform>("Content/SeverWait").gameObject;
        m_LoadingObj.SetActive(false);
		m_VersionText = FindComponent<TextMeshProUGUI>("Content/VersionNumber");

		UIManager.Instance.OpenPanel(UIPanel.CommonNoticePanel);
    }

    public override void OnShow(object msg)
    {
        base.OnShow(msg);
        m_IndexFocus = 0;
        m_BtnLogin.onClick.AddListener(OnClickBtnLogin);
        m_BtnServerList.onClick.AddListener(OnClickBtnServerList);
        m_BtnLogin.enabled = m_NameInput.text != string.Empty ? true : false;
        m_NameInput.onValueChanged.AddListener((str) =>
        {
            m_BtnLogin.enabled = m_NameInput.text != string.Empty ? true : false;
        });
        m_LoginProxy = Facade.RetrieveProxy(ProxyName.LoginProxy) as LoginProxy;
        if (SettingINI.Setting.TryGetValue(SettingINI.Setting.CombineKey(SettingINI.Constants.GROUP_LOGIN
            , SettingINI.Constants.KEY_DEFAULT_LAST_LOGIN_SERVER), out string defaultLastLoginServer))
        {
            m_LoginProxy.SetLastLoginServer(defaultLastLoginServer);
        }
        m_LoginProxy.LoadServerList();

        if (SettingINI.Setting.GetBoolValue(SettingINI.Setting.CombineKey(SettingINI.Constants.GROUP_LOGIN
                    , SettingINI.Constants.KEY_DISPLAY_AGREEMENT_PANEL)
                , true)
            && PlayerPrefs.GetString(GameConstant.FIRSTLOGIN) != "1")
        {
            GetGameObject().SetActive(false);
            UIManager.Instance.OpenPanel(UIPanel.AgreementPanel);
        }

        if (SettingINI.Setting.GetBoolValue(SettingINI.Setting.CombineKey(SettingINI.Constants.GROUP_LOGIN
                , SettingINI.Constants.KEY_AUTO_LOGIN)
            , false))
        {
			CoroutineHelper.GetInstance().StartCoroutine(AutoLogin());
        }

		m_VersionText.text = "The version number " + Application.version;

    }

	public override void OnHide(object msg)
    {
        m_NameInput.onValueChanged.RemoveAllListeners();
        base.OnHide(msg);
    }

    public override void OnGotFocus()
    {
        base.OnGotFocus();
        AddHotKey("tab",HotKeyID.FuncLT,OnSwichTab);
        AddHotKey("x", HotKeyID.FuncEnter, OnLoginHotKey);
    }

    private void OnLoginHotKey(HotkeyCallback callback)
    {
        if (callback.started)
        {
            if (InputManager.Instance.CurrentInputDevice == InputManager.GameInputDevice.KeyboardAndMouse)
            {
                OnClickBtnLogin();
            }
        }
    }

    private void OnSwichTab(HotkeyCallback callback)
    {
        if (callback.started)
        {
            if (InputManager.Instance.CurrentInputDevice == InputManager.GameInputDevice.KeyboardAndMouse)
            {
                m_IndexFocus++;
                m_IndexFocus %= 2;
                if (m_IndexFocus == 0)
                {
                    EventSystemUtils.SetFocus(m_NameInput);
                    m_NameInput.caretPosition = m_NameInput.text.Length;
                }
                else if (m_IndexFocus == 1)
                {
                    EventSystemUtils.SetFocus(m_PassInput);
                    m_PassInput.selectionFocusPosition = m_PassInput.text.Length;
                }
                
            }
           
        }
    }

    public override void OnLostFocus()
    {
        base.OnLostFocus();
    }

    public override void OnRefresh(object msg)
    {
    }

    public override NotificationName[] ListNotificationInterests()
    {
		return new NotificationName[]
		{
			NotificationName.MSG_GRPC_SERVERLIST_BACK,
			NotificationName.MSG_CHARACTER_LIST_GETED,
			NotificationName.MSG_LOGINPANEL_ACTIVE,
			NotificationName.SOCKETCONNECTFAIL,
			NotificationName.MSG_LOGINWAITSHOW,
		};
    }

    public override void HandleNotification(INotification notification)
    {
        switch (notification.Name)
        {
            case NotificationName.MSG_GRPC_SERVERLIST_BACK:
                SetBaseData();
                if (!m_LoginProxy.HasLastLoginServer() && PlayerPrefs.GetString(GameConstant.FIRSTLOGIN) == "1")
                {
                    GetGameObject().SetActive(false);
                    UIManager.Instance.OpenPanel(UIPanel.ServerListPanel);
                }
                break;
            case NotificationName.MSG_CHARACTER_LIST_GETED:
                UIManager.Instance.ClosePanel(this);
                UIManager.Instance.OpenPanel(UIPanel.CharacterPanel);
                break;
            case NotificationName.MSG_LOGINPANEL_ACTIVE:
                GetGameObject().SetActive(true);
                break;
			case NotificationName.SOCKETCONNECTFAIL:
				m_CanvasGroup.interactable = true;
				UIManager.Instance.StartCoroutine(Excute(Time.deltaTime, () =>
				{
					m_BtnLogin.GetComponent<Animator>().SetTrigger("Normal");
				}));
				break;
			case NotificationName.MSG_LOGINWAITSHOW:
				m_LoadingObj.SetActive((bool)notification.Body);
				break;
			default:
                break;
        }
    }

    /// <summary>
    /// 设置基本数据
    /// </summary>
    private void SetBaseData()
    {
        m_NameInput.transform.Find("Text Area/Placeholder").GetComponent<TMP_Text>().text = TableUtil.GetLanguageString("login_title_1001");//帐户名称
        m_PassInput.transform.Find("Text Area/Placeholder").GetComponent<TMP_Text>().text = TableUtil.GetLanguageString("login_title_1002");//输入密码
        m_BtnLogin.transform.Find("Label").GetComponent<TMP_Text>().text = TableUtil.GetLanguageString("login_btn_1003");//登录

        m_NameInput.text = PlayerPrefs.GetString(LAST_LOGIN_USER);
        if (SettingINI.Setting.GetBoolValue(SettingINI.Setting.CombineKey(SettingINI.Constants.GROUP_LOGIN
                    , SettingINI.Constants.KEY_ENABLE_DEFAULT_USERNAME)
                , false)
            && SettingINI.Setting.TryGetValue(SettingINI.Setting.CombineKey(SettingINI.Constants.GROUP_LOGIN
                    , SettingINI.Constants.KEY_DEFAULT_USERNAME)
                , out string defaultUsername))
        {
            m_NameInput.text = defaultUsername;
        }

		if (SettingINI.Setting.GetBoolValue(SettingINI.Setting.CombineKey(SettingINI.Constants.GROUP_LOGIN
				, SettingINI.Constants.KEY_RANDOM_USERNAME)
			, false))
		{
			m_NameInput.text = Guid.NewGuid().ToString().Substring(4, 16);
		}

        EventSystemUtils.SetFocus(m_NameInput);
        m_NameInput.caretPosition = m_NameInput.text.Length;
    }

	#region BtnOnClickHandler
	/// <summary>
	/// 点击登陆按钮
	/// </summary>
	private void OnClickBtnLogin()
    {
        NetworkManager.Instance.GetLoginController().Account = m_NameInput.text;
        NetworkManager.Instance.GetLoginController().Password = m_PassInput.text;
        m_LoginProxy.Login();
        PlayerPrefs.SetString(LAST_LOGIN_USER, m_NameInput.text);
        EventSystemUtils.SetFocus(null);
		m_CanvasGroup.interactable = false;
		UIManager.Instance.StartCoroutine(Excute(0.5f,()=> {
			m_LoadingObj.SetActive(true);
		}));
		
	}

	/// <summary>
	/// 找回密码按钮点击
	/// </summary>
	private void OnRetrieveButtonClick()
    {
        Application.OpenURL("https://www.baidu.com/");
    }

    /// <summary>
    /// 注册按钮点击
    /// </summary>
    private void OnRegisterButtonClick()
    {
        Application.OpenURL("https://www.baidu.com/");
    }

    /// <summary>
    /// 协议按钮点击
    /// </summary>
    private void OnAgreementButtonClick()
    {
        UIManager.Instance.OpenPanel(UIPanel.AgreementPanel);
    }

    /// <summary>
    /// 退出按钮点击时
    /// </summary>
    private void OnExitButtonClick(HotkeyCallback callbackContext)
    {
        if (callbackContext.performed)
        {
            OpenExitPanel();
        }
    }

    /// <summary>
    /// 服务器列表点击时
    /// </summary>
    private void OnClickBtnServerList()
    {
        UIManager.Instance.OpenPanel(UIPanel.ServerListPanel);
        EventSystemUtils.SetFocus(null);
    }
	#endregion
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
	#region 退出确认框
	/// <summary>
	/// 打开退出确认框
	/// </summary>
	private void OpenExitPanel()
    {
        SendNotification(NotificationName.MSG_QUIT);
    }

	#endregion

	private IEnumerator AutoLogin()
	{
		yield return new WaitForSeconds(3.0f);
		OnClickBtnLogin();
	}
}