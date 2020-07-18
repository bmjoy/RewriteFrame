using PureMVC.Interfaces;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HudChatPanel : HudBase
{
    private const string ASSET_ADDRESS = Leyoutech.Core.Loader.Config.AssetAddressKey.PRELOADUI_HUD_CHATPANEL;

    /// <summary>
    /// 单条消息字符数上限
    /// </summary>
	private const int CHATACTER_LIMIT = 180;
    /// <summary>
    /// 可输入字符数低于此值时出现提示
    /// </summary>
	private const int CHATACTER_LIMIT_WAINING = 20;
    /// <summary>
    /// 自动隐藏聊天前等待秒数
    /// </summary>
	private const float HIDDEN_WAIT_TIME = 1000.0f;//5.0f;
    /// <summary>
    /// 自动隐藏聊天时动画时间秒数
    /// </summary>
	private const float HIDDEN_ANIM_TIME = 1.0f;
    /// <summary>
    /// 输入频道的名称ID
    /// </summary>
	public static int[] IN_CHANNEL_NAME_IDS = new int[] { 3016, 3011, 3012, 3013, 3014, 3015 };
    /// <summary>
    /// 输出频道的名称ID
    /// </summary>
	public static int[] OUT_CHANNEL_NAME_IDS = new int[] { 3010, 3011, 3012, 3013, 3014, 3015, 3016, 3017 };

    /// <summary>
    /// 根结点
    /// </summary>
    private CanvasGroup m_Root;
    /// <summary>
    /// 根节点画布
    /// </summary>
    private Canvas m_RootCanvas;
    /// <summary>
    /// 面板主体
    /// </summary>
    private RectTransform m_ChatBox;
    /// <summary>
    /// 面板背景
    /// </summary>
    private RectTransform m_ChatBack;
    /// <summary>
    /// 面板主体挡板
    /// </summary>
    private Canvas m_ChatBlocker;
    /// <summary>
    /// 面板主体档板按钮
    /// </summary>
    private Button m_ChatBlockerButton;
    /// <summary>
    /// 菜单挡板
    /// </summary>
    private Canvas m_MenuBlocker;
    /// <summary>
    /// 菜单挡板按钮
    /// </summary>
    private Button m_MenuBlockerButton;


    /// <summary>
    /// 输出面板
    /// </summary>
    private TMP_Text m_OutputPanel;
    /// <summary>
    /// 输出面板的滚动区
    /// </summary>
    private ScrollRect m_OutputPanelScroller;
    /// <summary>
    /// 输出面板的滚动条
    /// </summary>
    private CanvasGroup m_OutputPanelScrollerBar;
    /// <summary>
    /// 输出频道的扩展按钮
    /// </summary>
    private Button m_OutputPanelExpandButton;
    /// <summary>
    /// 输出面板是否已扩展
    /// </summary>
    private bool m_OutputPanelExtended = false;
    /// <summary>
    /// 输出频道的超链接菜单
    /// </summary>
    private RectTransform m_OutputPanelLinkMenu;


    /// <summary>
    /// 输出频道按钮列表的容器
    /// </summary>
    private RectTransform m_OutputChannelLine;
    /// <summary>
    /// 输出频道按钮列表Tip
    /// </summary>
    private RectTransform m_OutputChannelTip;
    /// <summary>
    /// 输出频道按钮列表Tip的文本框
    /// </summary>
    private TMP_Text m_OutputChannelTipField;


    /// <summary>
    /// 输入行容器
    /// </summary>
    private Transform m_InputLine;
    /// <summary>
    /// 输入行占位符
    /// </summary>
    private Button m_InputPlaceholder;
    /// <summary>
    /// 对话目标文本框
    /// </summary>
    private TMP_Text m_TalkTargetField;
    /// <summary>
    /// 输入文本框
    /// </summary>
    private TMP_InputField m_InputField;
    /// <summary>
    /// 输入字符限制提示
    /// </summary>
    private RectTransform m_InputCharLimitTip;
    /// <summary>
    /// 输入字符限制提示文本框
    /// </summary>
    private TMP_Text m_InputCharLimitField;


    /// <summary>
    /// 输入频道切换按钮
    /// </summary>
    private Toggle m_InputChannelToggleButton;
    /// <summary>
    /// 输入频道切换按钮的图标
    /// </summary>
    private Image m_InputChannelToggleButtonIcon;
    /// <summary>
    /// 输入频道切换按钮的开关标记
    /// </summary>
    private Image m_InputChannelToggleButtonChecker;
    /// <summary>
    /// 输入频道列表
    /// </summary>
    private RectTransform m_InputChannelList;


    /// <summary>
    /// 表情面板
    /// </summary>
    private Transform m_EmoticonBox;
    /// <summary>
    /// 表情面板切换按钮
    /// </summary>
    private Toggle m_EmoticonButton;
    /// <summary>
    /// 表情容器
    /// </summary>
    private Transform m_EmoticonList;
    /// <summary>
    /// 表情模板
    /// </summary>
    private Transform m_EmoticonTemplate;


    /// <summary>
    /// 好友面板
    /// </summary>
    private Transform m_FriendBox;
    /// <summary>
    /// 好友面板切换按钮
    /// </summary>
    private Toggle m_FriendButton;
    /// <summary>
    /// 好友面板滚动区
    /// </summary>
    private ScrollRect m_FriendScroller;
    /// <summary>
    /// 好友容器
    /// </summary>
    private Transform m_FriendList;
    /// <summary>
    /// 好友模板
    /// </summary>
    private Transform m_FriendTemplate;


    /// <summary>
    /// 区段面板
    /// </summary>
    private Transform m_SectionBox;
    /// <summary>
    /// 区段面板切换按钮
    /// </summary>
    private Button m_SectionButton;
    /// <summary>
    /// 区段容器
    /// </summary>
    private Transform m_SectionList;
    /// <summary>
    /// 区段列表
    /// </summary>
    private Transform m_SectionTemplate;


    /// <summary>
    /// 检查协程
    /// </summary>
    private Coroutine m_CheckCoroutine;


    /// <summary>
    /// 当前输入频道
    /// </summary>
    private ChatChannel m_CurrentInputChannel = ChatChannel.World;
    /// <summary>
    /// 当前对话目标ID
    /// </summary>
    private ulong m_CurrentTalkTargetUID;
    /// <summary>
    /// 当前对话目标名称
    /// </summary>
    private string m_CurrentTalkTargetName;


    /// <summary>
    /// 当前输入标所在位置
    /// </summary>
    private int m_CurrentInputStringPosition = 0;
    /// <summary>
    /// 上次输入点所在的字符位置
    /// </summary>
    private int m_LastInputStringPosition = 0;
    /// <summary>
    /// 上次收到消息的时间
    /// </summary>
    private float m_LastReceiveTime = 0;

    /// <summary>
    /// 输入历史
    /// </summary>
    private List<string> m_Historys = new List<string>();
    /// <summary>
    /// 输入历史索引
    /// </summary>
    private int m_HistoryIndex = 0;

    //public float ChatOffsetY = 1500;


    public HudChatPanel() : base(UIPanel.HudChatPanel, ASSET_ADDRESS, PanelType.Hud) { }

    public override void Initialize()
    {
        CanvasGroup main = GetTransform().GetComponent<CanvasGroup>();
        if (main == null)
        {
            main = GetTransform().gameObject.AddComponent<CanvasGroup>();
        }
        main.interactable = true;
        main.blocksRaycasts = true;
        main.ignoreParentGroups = true;

        m_Root = FindComponent<CanvasGroup>("Content");
        m_RootCanvas = GetTransform().GetComponentInParent<Canvas>();
        m_ChatBox = FindComponent<RectTransform>("Content/ChatMain");
        m_ChatBack = FindComponent<RectTransform>("Content/ChatMain/Back");
        m_ChatBack.gameObject.SetActive(false);
        m_ChatBlocker = FindComponent<Canvas>("ChatBlocker");
        m_ChatBlockerButton = FindComponent<Button>("ChatBlocker");
        m_MenuBlocker = FindComponent<Canvas>("MenuBlocker");
        m_MenuBlockerButton = FindComponent<Button>("MenuBlocker");

        m_OutputPanel = FindComponent<TMP_Text>("Content/ChatMain/Scroll View/Viewport/Content");
        m_OutputPanelScroller = FindComponent<ScrollRect>("Content/ChatMain/Scroll View");
        m_OutputPanelScrollerBar = FindComponent<CanvasGroup>("Content/ChatMain/Scroll View/Scrollbar Vertical");
        m_OutputPanelScrollerBar.alpha = 0;
        m_OutputPanelExpandButton = FindComponent<Button>("Content/ChatMain/ChannelLine/ExpandButton");
        m_OutputPanelLinkMenu = FindComponent<RectTransform>("Content/ChatMain/LinkMenu");
        m_OutputPanelLinkMenu.gameObject.SetActive(false);

        m_OutputChannelLine = FindComponent<RectTransform>("Content/ChatMain/ChannelLine");
        m_OutputChannelLine.gameObject.SetActive(false);
        m_OutputChannelTip = FindComponent<RectTransform>("Content/ChatMain/ChannelTip");
        m_OutputChannelTip.gameObject.SetActive(false);
        m_OutputChannelTipField = FindComponent<TMP_Text>("Content/ChatMain/ChannelTip/Tip");

        m_InputChannelToggleButton = FindComponent<Toggle>("Content/ChatMain/InputLine/ChannelButton");
        m_InputChannelToggleButtonIcon = FindComponent<Image>("Content/ChatMain/InputLine/ChannelButton");
        m_InputChannelToggleButtonIcon.gameObject.SetActive(false);
        m_InputChannelToggleButtonChecker = FindComponent<Image>("Content/ChatMain/InputLine/ChannelButton/Checker");
        m_InputChannelToggleButtonChecker.gameObject.SetActive(false);
        m_InputChannelList = FindComponent<RectTransform>("Content/ChatMain/InputLine/ChannelMenu");
        m_InputChannelList.gameObject.SetActive(false);

        m_EmoticonBox = FindComponent<Transform>("Content/ChatEmoticon");
        m_EmoticonBox.gameObject.SetActive(false);
        m_EmoticonButton = FindComponent<Toggle>("Content/ChatMain/InputLine/EmoticonButton");
        m_EmoticonList = FindComponent<Transform>("Content/ChatEmoticon/Scroll View/Viewport/Content");
        m_EmoticonTemplate = FindComponent<Transform>("Template/Emoticon");

        m_FriendBox = FindComponent<Transform>("Content/ChatFriends");
        m_FriendBox.gameObject.SetActive(false);
        m_FriendButton = FindComponent<Toggle>("Content/ChatMain/InputLine/FriendButton");
        m_FriendScroller = FindComponent<ScrollRect>("Content/ChatFriends/Scroll View");
        m_FriendList = FindComponent<Transform>("Content/ChatFriends/Scroll View/Viewport/Content");
        m_FriendTemplate = FindComponent<Transform>("Template/FriendItem");

        m_SectionBox = FindComponent<Transform>("Content/FriendSections");
        m_SectionBox.gameObject.SetActive(false);
        m_SectionButton = FindComponent<Button>("Content/ChatFriends/SectionPanel/Button");
        m_SectionList = FindComponent<Transform>("Content/FriendSections/SectionToggles/Toggles");
        m_SectionTemplate = FindComponent<Transform>("Template/SectionItem");

        m_InputCharLimitTip = FindComponent<RectTransform>("Content/ChatMain/InputLine/NewMsgButton");
        m_InputCharLimitTip.gameObject.SetActive(false);
        m_InputCharLimitField = FindComponent<TMP_Text>("Content/ChatMain/InputLine/NewMsgButton/Text");

        m_InputLine = FindComponent<Transform>("Content/ChatMain/InputLine");
        m_InputLine.gameObject.SetActive(false);
        m_InputPlaceholder = FindComponent<Button>("Content/ChatMain/FocusLine");
        m_TalkTargetField = FindComponent<TMP_Text>("Content/ChatMain/InputLine/TargetField");
        m_TalkTargetField.gameObject.SetActive(false);
        m_InputField = FindComponent<TMP_InputField>("Content/ChatMain/InputLine/InputField");
        m_InputField.onFocusSelectAll = false;
        m_InputField.richText = true;
        m_InputField.isRichTextEditingAllowed = false;
    }

    public override void OnShow(object msg)
    {
        base.OnShow(msg);

        m_OutputPanelExpandButton.onClick.AddListener(OnOutputPanelExpandButtonClick);
        m_InputChannelToggleButton.onValueChanged.AddListener(OnInputChannelToggleButtonClick);
        m_EmoticonButton.onValueChanged.AddListener(OnEmoticonButtonClick);
        m_FriendButton.onValueChanged.AddListener(OnFriendButtonClick);
        m_SectionButton.onClick.AddListener(OnSectionButtonClick);
        m_InputField.onValidateInput = OnValidateInput;
        m_InputField.onSubmit.AddListener(OnInputSubmit);
        m_InputField.onValueChanged.AddListener(OnInputChanged);
        m_InputPlaceholder.onClick.AddListener(OnFocusClick);
        m_ChatBlockerButton.onClick.AddListener(OnChatBlockerButtonClick);
        m_MenuBlockerButton.onClick.AddListener(OnMenuBlockerClick);

        Toggle[] toggles = m_OutputChannelLine.GetComponentsInChildren<Toggle>();
        for (int i = 0; i < toggles.Length; i++)
        {
            Toggle toggle = toggles[i];

            TipComponent tipComponent = toggle.gameObject.GetComponent<TipComponent>();
            if (!tipComponent)
            {
                tipComponent = toggle.gameObject.AddComponent<TipComponent>();
            }

            tipComponent.ShowTip = ShowOutputChannelButtonTip;
            tipComponent.HideTip = HideOutputChannelButtonTip;
            tipComponent.index = i;

            toggle.onValueChanged.RemoveAllListeners();
            toggle.onValueChanged.AddListener((selected) => { UpdateCurrentOutputChannelContent(); });
        }


        m_CheckCoroutine = UIManager.Instance.StartCoroutine(InputCheckCoroutine());


        OnOutputPanelExpandButtonClick();
        OnOutputPanelExpandButtonClick();

        m_LastReceiveTime = Time.time;

        AddHotKey(HotKeyMapID.SHIP, HotKeyID.ChatOpen, OnChatOpenCallback);
        AddHotKey(HotKeyMapID.HUMAN, HotKeyID.ChatOpen, OnChatOpenCallback);

        AddHotKey(HotKeyMapID.Chat, HotKeyID.ChatHistory, OnChatHistoryCallback);
        AddHotKey(HotKeyMapID.Chat, HotKeyID.ChatClose, OnChatCloseCallback);

        /*
        GetTransform().GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        GetTransform().GetComponent<RectTransform>().sizeDelta = Vector2.zero;
        */

        RectTransform rect = m_Root.GetComponent<RectTransform>();
        rect.anchoredPosition = new Vector2(0, 0);
        if (IsInSpace())
            rect.sizeDelta = new Vector2(rect.sizeDelta.x, -500);
        else
            rect.sizeDelta = new Vector2(rect.sizeDelta.x, -20);
    }

    public override void OnHide(object msg)
    {
        UIManager.Instance.StopCoroutine(m_CheckCoroutine);

        for (int i = 0; i < m_EmoticonList.childCount; i++) { m_EmoticonList.GetChild(i).GetComponent<Button>().onClick.RemoveAllListeners(); }
        for (int i = 0; i < m_FriendList.childCount; i++) { m_FriendList.GetChild(i).GetComponent<Button>().onClick.RemoveAllListeners(); }
        for (int i = 0; i < m_SectionList.childCount; i++) { m_SectionList.GetChild(i).GetComponent<Toggle>().onValueChanged.RemoveAllListeners(); }

        foreach (Toggle toggle in m_OutputChannelLine.GetComponentsInChildren<Toggle>()) { toggle.onValueChanged.RemoveAllListeners(); }
        foreach (Button btn in m_InputChannelList.GetComponentsInChildren<Button>()) { btn.onClick.RemoveAllListeners(); }
        foreach (Button btn in m_OutputPanelLinkMenu.GetComponentsInChildren<Button>()) { btn.onClick.RemoveAllListeners(); }

        m_OutputPanelExpandButton.onClick.RemoveAllListeners();
        m_InputChannelToggleButton.onValueChanged.RemoveAllListeners();
        m_EmoticonButton.onValueChanged.RemoveAllListeners();
        m_FriendButton.onValueChanged.RemoveAllListeners();
        m_SectionButton.onClick.RemoveAllListeners();

        m_EmoticonTemplate.RecycleAll();
        m_FriendTemplate.RecycleAll();
        m_SectionTemplate.RecycleAll();

        m_InputField.onSubmit.RemoveAllListeners();
        m_InputField.onValueChanged.RemoveAllListeners();
        m_InputPlaceholder.onClick.RemoveAllListeners();
        m_ChatBlockerButton.onClick.RemoveAllListeners();
        m_MenuBlockerButton.onClick.RemoveAllListeners();

        base.OnHide(msg);
    }

    public override NotificationName[] ListNotificationInterests()
    {
        return new NotificationName[]
        {
            NotificationName.ChatMessageChanged,
            NotificationName.ChatTalkToFriend,
            NotificationName.ChatInsertItemLink,
            NotificationName.MSG_FRIEND_LIST_CHANGED
        };
    }

    public override void HandleNotification(INotification notification)
    {
        switch ((NotificationName)notification.Name)
        {
            case NotificationName.ChatMessageChanged:
                OnChatMessage(notification.Body as MsgChatChannelMessageChanged);
                break;
            case NotificationName.ChatTalkToFriend:
                OnTalkToFriend(notification.Body as MsgChatTalkToFriend);
                break;
            case NotificationName.ChatInsertItemLink:
                OnAppendItemLink(notification.Body as MsgChatInsertItemLink);
                break;
            case NotificationName.MSG_FRIEND_LIST_CHANGED:
                OnFriendListChanged();
                break;
        }
    }

    /// <summary>
    /// 处理输入状态切换
    /// </summary>
    protected override void OnInputMapChanged()
    {
        m_Root.gameObject.SetActive(!IsWatchOrUIInputMode());
    }

    /// <summary>
    /// 处理聊天开启
    /// </summary>
    /// <param name="callback">热键状态</param>
    private void OnChatOpenCallback(HotkeyCallback callback)
    {
        if (callback.performed)
        {
            OnFocusClick();
        }
    }

    /// <summary>
    /// 处理聊天关闭
    /// </summary>
    /// <param name="callback">热键状态</param>
    private void OnChatCloseCallback(HotkeyCallback callback)
    {
        return;
        if (callback.performed)
        {
            EventSystem.current.SetSelectedGameObject(null);
            InputManager.Instance.HudInputMap = HotKeyMapID.None;

            OnChatBlockerButtonClick(false);
        }
    }

    /// <summary>
    /// 处理聊天历史切换
    /// </summary>
    /// <param name="callback">热键状态</param>
    private void OnChatHistoryCallback(HotkeyCallback callback)
    {
        if (m_Historys.Count > 0)
        {
            float value = callback.ReadValue<float>();
            if (value != 0)
            {
                m_HistoryIndex = Mathf.Clamp(m_HistoryIndex + (value > 0 ? -1 : 1), 0, m_Historys.Count - 1);
                m_InputField.text = m_Historys[m_HistoryIndex];
                SetFocus(m_InputField);
                m_InputField.stringPosition = m_InputField.text.Length;
                m_InputField.caretPosition = GetCaretPositionFromStringIndex(m_CurrentInputStringPosition);
            }
        }
    }

    /// <summary>
    /// 收到新的聊天消息
    /// </summary>
    /// <param name="message">消息内容</param>
	private void OnChatMessage(MsgChatChannelMessageChanged message)
    {
        if (message.m_channel == GetCurrentOutputChannel())
        {
            m_Root.alpha = 1;
            m_Root.blocksRaycasts = true;
            m_LastReceiveTime = Time.time;

            UpdateCurrentOutputChannelContent();
        }
    }

    /// <summary>
    /// 与指定好友私聊
    /// </summary>
    /// <param name="msg">好友信息</param>
	private void OnTalkToFriend(MsgChatTalkToFriend msg)
    {
        OnFocusClick();

        ChangeInputChannel(0, msg.m_ID, msg.m_Name);

        InsertText("");
    }

    /// <summary>
    /// 插入一个道具连接
    /// </summary>
    /// <param name="msg">道具信息</param>
	private void OnAppendItemLink(MsgChatInsertItemLink msg)
    {
        OnFocusClick();

        InsertLink(string.Format("item_{0}_{1}", msg.m_ItemType, msg.m_ItemID), msg.m_ItemName);
    }

    #region 焦点状态切换

    /// <summary>
    /// 输入框占位按钮点击时
    /// </summary>
    private void OnFocusClick()
    {
        m_ChatBack.gameObject.SetActive(true);
        m_OutputChannelLine.gameObject.SetActive(true);
        m_InputLine.gameObject.SetActive(true);
        m_InputPlaceholder.gameObject.SetActive(false);
        m_ChatBlocker.gameObject.SetActive(true);
        m_ChatBlocker.sortingOrder = GetTransform().GetComponentInParent<Canvas>().sortingOrder - 1;

        RectTransform maskCanvasTransform = m_ChatBlocker.GetComponent<RectTransform>();
        maskCanvasTransform.anchorMin = Vector2.zero;
        maskCanvasTransform.anchorMax = Vector2.one;
        maskCanvasTransform.anchoredPosition = Vector2.zero;
        maskCanvasTransform.sizeDelta = Vector2.zero;

        SetFocus(m_InputField);
        m_InputField.stringPosition = m_LastInputStringPosition;
        m_OutputPanelScrollerBar.alpha = 1;

        InputManager.Instance.HudInputMap = HotKeyMapID.Chat;

        if (m_OutputPanelExtended)
            ExpandOutputPanel();
        else
            CollapseOutputPanel();
    }

    /// <summary>
    /// 聊天档板按钮被点击时
    /// </summary>
    private void OnChatBlockerButtonClick()
    {
        OnChatBlockerButtonClick(true);
    }

    /// <summary>
    /// 聊天档板按钮被点击时
    /// </summary>
    /// <param name="exitUiInputMode">是否要退出UI输入模式</param>
	private void OnChatBlockerButtonClick(bool exitUiInputMode)
    {
        CollapseOutputPanel();

        m_ChatBack.gameObject.SetActive(false);
        m_OutputChannelLine.gameObject.SetActive(false);
        m_InputLine.gameObject.SetActive(false);
        m_InputPlaceholder.gameObject.SetActive(true);
        m_ChatBlocker.gameObject.SetActive(false);

        m_EmoticonBox.gameObject.SetActive(false);
        m_FriendBox.gameObject.SetActive(false);
        m_SectionBox.gameObject.SetActive(false);
        m_InputChannelList.gameObject.SetActive(false);
        m_OutputPanelLinkMenu.gameObject.SetActive(false);
        m_EmoticonButton.isOn = false;
        m_FriendButton.isOn = false;

        m_OutputPanelScrollerBar.alpha = 0;

        SetFocus(null);

        if (exitUiInputMode)
        {
            EventSystem.current.SetSelectedGameObject(null);
            InputManager.Instance.HudInputMap = HotKeyMapID.None;
        }

        m_LastReceiveTime = Time.time;
    }

    #endregion




    /// <summary>
    /// 输出面板的扩展按钮点击时
    /// </summary>
    private void OnOutputPanelExpandButtonClick()
    {
        m_OutputPanelExtended = !m_OutputPanelExtended;
        if (m_OutputPanelExtended)
        {
            ExpandOutputPanel();
        }
        else
        {
            CollapseOutputPanel();
        }
    }

    /// <summary>
    /// 超链接菜单挡板点击时
    /// </summary>
    private void OnMenuBlockerClick()
    {
        bool retest = m_OutputPanelLinkMenu.gameObject.activeSelf;

        m_InputChannelToggleButton.isOn = false;

        m_MenuBlocker.gameObject.SetActive(false);
        m_InputChannelList.gameObject.SetActive(false);
        m_OutputPanelLinkMenu.gameObject.SetActive(false);

        if (retest)
        {
            OnPointerClick(null);
        }
    }

    /// <summary>
    /// 输入频道切换按钮点击时
    /// </summary>
    /// <param name="selected">是否选中</param>
    private void OnInputChannelToggleButtonClick(bool selected)
    {
        if (selected)
        {
            OpenInputChannelList();
        }
        else
        {
            CloseInputChannelList();
        }
    }

    /// <summary>
    /// 获取当前焦点
    /// </summary>
    /// <returns>UI上当前设置为焦点的对象</returns>
    public Selectable GetFocus()
    {
        //UNDONE 输入焦点 chw
        if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject)
        {
            return EventSystem.current.currentSelectedGameObject.GetComponent<Selectable>();
        }

        return null;
    }

    /// <summary>
    /// 设置当前焦点
    /// </summary>
    /// <param name="value">要设置成当前焦点的对象</param>
    public void SetFocus(Selectable value)
    {
        //UNDONE 输入焦点 chw
        if (EventSystem.current != null && value != null)
        {
            EventSystem.current.SetSelectedGameObject(value.gameObject);
        }
    }


    /// <summary>
    /// 编码消息
    /// </summary>
    /// <param name="text">原始文本</param>
    /// <returns>编码后的文本</returns>
    private string EncodeMessage(string text)
    {
        text = Regex.Replace(text, " <sprite=(.*?)> ", "<S$1>");
        text = Regex.Replace(text, " <link=\"item_(.*?)_(.*?)\".*?<u>(.*?)</u>.*?</link> ", "<I$1_$2>$3</I>");
        return text;
    }

    /// <summary>
    /// 解码消息
    /// </summary>
    /// <param name="text">编码后的文本</param>
    /// <returns>原始文本</returns>
    private string DecodeMessage(string text)
    {
        text = Regex.Replace(text, "<S(.*?)>", "<sprite=$1>");
        text = Regex.Replace(text, "<I(.*?)_(.*?)>(.*?)</I>", " <link=\"item_$1_$2\"><color=#49ecff><u>$3</u></color></link> ");
        return text;
    }


    #region 输出面板

    /// <summary>
    /// 扩展输出面板
    /// </summary>
    private void ExpandOutputPanel()
    {
        m_Root.alpha = 1;
        m_Root.blocksRaycasts = true;
        m_ChatBox.sizeDelta = new Vector2(m_ChatBox.sizeDelta.x, 699);
        m_OutputPanelExpandButton.transform.localScale = new Vector3(1, 1, 1);

        bool a = m_OutputPanelScroller.verticalScrollbar.gameObject.activeSelf == false || m_OutputPanelScroller.verticalNormalizedPosition <= 0;
        if (AdjustOutputPanelMarginToAlignBottom() || a)
        {
            ScrollOutputPanelToBottom();
        }
    }

    /// <summary>
    /// 收缩输出面板
    /// </summary>
    private void CollapseOutputPanel()
    {
        m_Root.alpha = 1;
        m_Root.blocksRaycasts = true;
        m_ChatBox.sizeDelta = new Vector2(m_ChatBox.sizeDelta.x, 287);
        m_OutputPanelExpandButton.transform.localScale = new Vector3(1, -1, 1);

        bool a = m_OutputPanelScroller.verticalScrollbar.gameObject.activeSelf == false || m_OutputPanelScroller.verticalNormalizedPosition <= 0;
        if (AdjustOutputPanelMarginToAlignBottom() || a)
        {
            ScrollOutputPanelToBottom();
        }
    }

    /// <summary>
    /// 显示输出频道按钮Tip
    /// </summary>
    /// <param name="toggle">输出频道对应的按钮</param>
    /// <param name="index">输出频道列表索引</param>
    private void ShowOutputChannelButtonTip(Toggle toggle, int index)
    {
        CfgLanguageProxy languageProxy = Facade.RetrieveProxy(ProxyName.CfgLanguageProxy) as CfgLanguageProxy;

        m_OutputChannelTip.gameObject.SetActive(true);
        m_OutputChannelTipField.text = languageProxy.GetLocalization(OUT_CHANNEL_NAME_IDS[index]);

        Rect rect = toggle.GetComponent<RectTransform>().rect;
        m_OutputChannelTip.anchoredPosition = new Vector2(rect.width * (index + 1), -rect.height);
    }

    /// <summary>
    /// 关闭输出频道按钮Tip
    /// </summary>
    /// <param name="toggle">输出频道对应的按钮</param>
    /// <param name="index">输出频道列表索引</param>
    private void HideOutputChannelButtonTip(Toggle toggle, int index)
    {
        m_OutputChannelTip.gameObject.SetActive(false);
    }

    /// <summary>
    /// 刷新当前频道内容
    /// </summary>
    private void UpdateCurrentOutputChannelContent()
    {
        float contentH = m_OutputPanel.rectTransform.rect.height;
        float viewportH = m_OutputPanelScroller.viewport.rect.height;
        bool scrollbarOnBottom = contentH <= viewportH;
        if (!scrollbarOnBottom)
        {
            scrollbarOnBottom = Mathf.Abs(m_OutputPanelScroller.viewport.InverseTransformPoint(m_OutputPanel.rectTransform.TransformPoint(Vector3.zero)).y + viewportH - contentH) < 1.0f;
        }

        ChatProxy proxy = Facade.RetrieveProxy(ProxyName.ChatProxy) as ChatProxy;

        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        foreach (ChatMessageInfoVO value in proxy.GetMessages(GetCurrentOutputChannel()))
        {
            if (sb.Length > 0)
            {
                sb.Append("\n");
            }
            sb.Append(FormatMessage(value));
        }
        m_OutputPanel.text = sb.ToString();

        AdjustOutputPanelMarginToAlignBottom();

        if (scrollbarOnBottom)
        {
            ScrollOutputPanelToBottom();
        }

        proxy.CurrentChannel = GetCurrentOutputChannel();
    }

    /// <summary>
    /// 获取当前输出频道
    /// </summary>
    /// <returns>聊天频道</returns>
    private ChatChannel GetCurrentOutputChannel()
    {
        Toggle[] toggles = m_OutputChannelLine.GetComponentsInChildren<Toggle>();
        for (int i = 0; i < toggles.Length; i++)
        {
            if (toggles[i].isOn)
            {
                return (ChatChannel)i;
            }
        }
        return ChatChannel.All;
    }

    /// <summary>
    /// 格式化消息
    /// </summary>
    /// <param name="info">聊天数据VO</param>
    /// <returns>格式化后的消息内容</returns>
    private string FormatMessage(ChatMessageInfoVO info)
    {
        CfgLanguageProxy languageProxy = Facade.RetrieveProxy(ProxyName.CfgLanguageProxy) as CfgLanguageProxy;

        string time = string.Format("{0:D2}:{1:D2}:{2:D2}", info.Time.Hour, info.Time.Minute, info.Time.Second);
        ulong fromID = info.FromID;
        string fromName = info.FromName;
        string fromChannel = languageProxy.GetLocalization(OUT_CHANNEL_NAME_IDS[(int)info.Channel]);

        if (!info.MessageDecoded)
        {
            info.Message = DecodeMessage(info.Message);
            info.MessageDecoded = true;
        }

        if (info.FromID == 0 && info.FromName == null)
        {
            return string.Format("<color=#4aa6a6>[{0}]</color><color=#f7b94d>[{1}]</color><color=#fff82b>{2}</color>", time, languageProxy.GetLocalization(3018), info.Message);
        }
        else
        {
            if (info.Channel == ChatChannel.Whisper)
            {
                return string.Format("<color=#4aa6a6>[{0}]</color><color=#1870e7><u><link=\"role_{1}\">{2}</link></u>:</color><color=#8ec3ff>{3}</color>", time, fromID, fromName, info.Message);
            }
            else
            {
                return string.Format("<color=#4aa6a6>[{0}]</color><color=#f7b94d>[{1}]</color><color=#ccffff><u><link=\"role_{2}\">{3}</link></u>:</color>{4}", time, fromChannel, fromID, fromName, info.Message);
            }
        }
    }

    /// <summary>
    /// 调整输出文本的上边距以实现文本底部对齐。
    /// </summary>
    /// <returns>如果改变了边距则返回true, 否则返回false.</returns>
    private bool AdjustOutputPanelMarginToAlignBottom()
    {
        Vector4 margin = m_OutputPanel.margin;
        margin.y = 0;

        m_OutputPanel.margin = margin;

        Canvas.ForceUpdateCanvases();

        RectTransform scrollerTransform = m_OutputPanelScroller.GetComponent<RectTransform>();
        bool textNeedAlign = m_OutputPanel.preferredHeight <= scrollerTransform.rect.height;
        if (textNeedAlign)
        {
            margin.y = margin.y + (scrollerTransform.rect.height - m_OutputPanel.preferredHeight);

            m_OutputPanel.margin = margin;
        }

        return textNeedAlign;
    }

    /// <summary>
    /// 滚动输出面板到底部
    /// </summary>
    private void ScrollOutputPanelToBottom()
    {
        Canvas.ForceUpdateCanvases();
        m_OutputPanelScroller.verticalNormalizedPosition = 0;
        Canvas.ForceUpdateCanvases();
        //chatMainScroller.StopMovement();
    }

    #endregion

    #region 超链接菜单功能

    /// <summary>
    /// 打开链接相关的菜单
    /// </summary>
    /// <param name="playerID">玩家ID</param>
    /// <param name="playerName">玩家名称</param>
    private void OpenLinkMenu(ulong playerID, string playerName)
    {
        if (playerID == 0) { return; }

        Vector2 position;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(m_ChatBox, Input.mousePosition, m_RootCanvas.worldCamera, out position);
        m_OutputPanelLinkMenu.gameObject.SetActive(true);

        //Canvas.ForceUpdateCanvases();

        m_MenuBlocker.gameObject.SetActive(true);
        m_MenuBlocker.sortingOrder = m_RootCanvas.sortingOrder + 1;

        RectTransform maskCanvasTransform = m_MenuBlocker.GetComponent<RectTransform>();
        maskCanvasTransform.anchorMin = Vector2.zero;
        maskCanvasTransform.anchorMax = Vector2.one;
        maskCanvasTransform.anchoredPosition = Vector2.zero;
        maskCanvasTransform.sizeDelta = Vector2.zero;

        Canvas contextCanvas = m_OutputPanelLinkMenu.GetComponent<Canvas>();
        contextCanvas.enabled = true;
        contextCanvas.sortingOrder = m_MenuBlocker.sortingOrder + 1;

        FriendProxy friendProxy = Facade.RetrieveProxy(ProxyName.FriendProxy) as FriendProxy;
        bool isFriend = friendProxy.GetFriend(playerID) != null;
        bool isBlack = friendProxy.GetBlack(playerID) != null;

        Button[] buttons = m_OutputPanelLinkMenu.GetComponentsInChildren<Button>(true);
        for (int i = 0; i < buttons.Length; i++)
        {
            Button button = buttons[i];

            int menuID = i;
            ulong linkID = playerID;
            string linkText = playerName;

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => { ExecuteLinkMenu(menuID, linkID, linkText); });
        }

        buttons[0].gameObject.SetActive(playerID != 0);
        buttons[1].gameObject.SetActive(playerID != 0);
        buttons[2].gameObject.SetActive(!isFriend && playerID != 0);
        buttons[3].gameObject.SetActive(isFriend && playerID != 0);
        buttons[4].gameObject.SetActive(!isBlack && playerID != 0);
        buttons[5].gameObject.SetActive(isBlack && playerID != 0);

        Canvas.ForceUpdateCanvases();

        if (Input.mousePosition.y - m_OutputPanelLinkMenu.rect.height < 0)
        {
            position.Set(position.x, position.y + m_OutputPanelLinkMenu.rect.height);
        }
        m_OutputPanelLinkMenu.anchoredPosition = position + Vector2.right * 5;
    }

    /// <summary>
    /// 执行菜单项
    /// </summary>
    /// <param name="menuID">菜单ID</param>
    /// <param name="playerID">玩ID</param>
    /// <param name="playerName">玩家名称</param>
    private void ExecuteLinkMenu(int menuID, ulong playerID, string playerName)
    {
        m_MenuBlocker.gameObject.SetActive(false);
        m_OutputPanelLinkMenu.gameObject.SetActive(false);

        switch (menuID)
        {
            case 0:
                //talk
                ChangeInputChannel(0, playerID, playerName);
                break;
            case 1:
                //invite team
                NetworkManager.Instance.GetTeamController().Invite(playerID);
                break;
            case 2:
                //add Friend
                NetworkManager.Instance.GetFriendController().RequestAddToFriendList(playerID);
                break;
            case 3:
                //del Friend
                NetworkManager.Instance.GetFriendController().RequestDeleteFromFriendList(playerID);
                break;
            case 4:
                //add black
                NetworkManager.Instance.GetFriendController().RequestAddToBlackList(playerID);
                break;
            case 5:
                //del black
                NetworkManager.Instance.GetFriendController().RequestDeleteFromBlackList(playerID);
                break;
        }
    }

    #endregion

    #region 输入频道切换功能

    /// <summary>
    /// 打开输入频道列表
    /// </summary>
    private void OpenInputChannelList()
    {
        m_InputChannelList.gameObject.SetActive(true);

        m_MenuBlocker.gameObject.SetActive(true);
        m_MenuBlocker.sortingOrder = GetTransform().GetComponentInParent<Canvas>().sortingOrder + 1;

        RectTransform maskCanvasTransform = m_MenuBlocker.GetComponent<RectTransform>();
        maskCanvasTransform.anchorMin = Vector2.zero;
        maskCanvasTransform.anchorMax = Vector2.one;
        maskCanvasTransform.anchoredPosition = Vector2.zero;
        maskCanvasTransform.sizeDelta = Vector2.zero;

        Canvas channelCanvas = m_InputChannelList.GetComponent<Canvas>();
        channelCanvas.enabled = true;
        channelCanvas.sortingOrder = m_MenuBlocker.sortingOrder + 1;

        CfgLanguageProxy languageProxy = Facade.RetrieveProxy(ProxyName.CfgLanguageProxy) as CfgLanguageProxy;

        Button[] buttons = m_InputChannelList.GetComponentsInChildren<Button>(true);
        for (int i = 0; i < buttons.Length; i++)
        {
            Button button = buttons[i];

            if (i == 0)
            {
                button.gameObject.SetActive(false);
                continue;
            }

            TMP_Text text = button.transform.Find("Text").GetComponent<TMP_Text>();
            int channelIndex = i;

            text.text = languageProxy.GetLocalization(IN_CHANNEL_NAME_IDS[i]);

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => { ChangeInputChannel(channelIndex); });
        }
    }

    /// <summary>
    /// 关闭输入频道列表
    /// </summary>
    private void CloseInputChannelList()
    {
        m_InputChannelList.gameObject.SetActive(false);
        m_MenuBlocker.gameObject.SetActive(false);
    }

    /// <summary>
    /// 改变输入频道
    /// </summary>
    /// <param name="index">频道索引</param>
    /// <param name="playerID">玩家ID</param>
    /// <param name="playerName">玩家名称</param>
    private void ChangeInputChannel(int index, ulong playerID = 0, string playerName = null)
    {
        OnFocusClick();

        Button[] buttons = m_InputChannelList.GetComponentsInChildren<Button>(true);
        if (index >= 0 && index < buttons.Length)
        {
            Button button = buttons[index];
            Image icon = button.transform.Find("Icon").GetComponent<Image>();

            m_InputChannelToggleButtonIcon.sprite = icon.sprite;
            m_InputChannelToggleButtonIcon.gameObject.SetActive(true);
            m_InputChannelToggleButtonChecker.sprite = icon.sprite;
            m_InputChannelToggleButtonChecker.gameObject.SetActive(true);
        }

        m_MenuBlocker.gameObject.SetActive(false);
        m_InputChannelList.gameObject.SetActive(false);
        m_TalkTargetField.gameObject.SetActive(index == 0);
        m_TalkTargetField.text = "<link=\"" + playerID + "\">" + playerName + "</link>";

        m_InputChannelToggleButton.isOn = false;

        m_CurrentInputChannel = index == 0 ? ChatChannel.Whisper : (ChatChannel)index;
        m_CurrentTalkTargetUID = playerID;
        m_CurrentTalkTargetName = playerName;
    }

    #endregion

    #region 输入面板

    /// <summary>
    /// 插入表情
    /// </summary>
    /// <param name="emoticonID">表情ID</param>
    private void InsertEmoticon(int emoticonID)
    {
        if (!InsertText(" <sprite=" + emoticonID + "> "))
        {
            //Debugger.LogError("character limit " + CHATACTER_LIMIT + "!!");
        }
    }

    /// <summary>
    /// 插入链接
    /// </summary>
    /// <param name="id">链接ID</param>
    /// <param name="text">链接文字</param>
    private void InsertLink(string id, string text)
    {
        if (!InsertText(string.Format(" <link=\"{0}\"><color=#49ecff><u>{1}</u></color></link> ", id, text)))
        {
            //Debug.LogError("character limit " + CHATACTER_LIMIT + "!!");
        }
    }

    /// <summary>
    /// 插入文字
    /// </summary>
    /// <param name="insertText">文字内容</param>
    /// <param name="position">接入位置</param>
    /// <returns>是否成功</returns>
    private bool InsertText(string insertText, int position = -1)
    {
        if (position == -1) { position = m_LastInputStringPosition; }

        string text = m_InputField.text;
        if (MeasureCharCount(text) + MeasureCharCount(insertText) < CHATACTER_LIMIT + 1)
        {
            bool tmp = m_InputField.onFocusSelectAll;
            m_InputField.text = text.Substring(0, position) + insertText + text.Substring(position, text.Length - position);
            m_InputField.onFocusSelectAll = false;
            SetFocus(m_InputField);
            m_InputField.onFocusSelectAll = tmp;
            m_InputField.stringPosition = position + insertText.Length;
            return true;
        }
        return false;
    }

    /// <summary>
    /// 测量文本字符的数量
    /// </summary>
    /// <param name="text">带格式的文本</param>
    /// <returns>去格式后的字符数</returns>
    private int MeasureCharCount(string text)
    {
        return System.Text.RegularExpressions.Regex.Replace(text, "<.*?>", "").Length;
    }

    /// <summary>
    /// 验证输入字符的有效性
    /// </summary>
    /// <param name="text">当前文本</param>
    /// <param name="charIndex">输入位置</param>
    /// <param name="addedChar">新增字符</param>
    /// <returns>新增的字符</returns>
    private char OnValidateInput(string text, int charIndex, char addedChar)
    {
        return addedChar == '<' || addedChar == '>' || MeasureCharCount(text) >= CHATACTER_LIMIT + 1 ? '\0' : addedChar;
    }

    /// <summary>
    /// 输入框内容改变时
    /// </summary>
    /// <param name="text">改变后的文字</param>
    private void OnInputChanged(string text)
    {
        int position = m_InputField.stringPosition;

        string linkTagL = "<link=";
        string linkTagR = "</link>";
        string spriteTagL = "<sprite=";
        string spriteTagR = ">";
        if (position + linkTagL.Length < text.Length && text.Substring(position, linkTagL.Length).Equals(linkTagL))
        {
            int endTag = text.IndexOf("</link> ", position) + 8;
            m_InputField.text = text.Substring(0, position) + text.Substring(endTag, text.Length - endTag);
        }
        else if (position + spriteTagL.Length < text.Length && text.Substring(position, spriteTagL.Length).Equals(spriteTagL))
        {
            int endTag = text.IndexOf("> ", position) + 2;
            m_InputField.text = text.Substring(0, position) + text.Substring(endTag, text.Length - endTag);
        }
        else if (position > linkTagR.Length && position <= text.Length && linkTagR.Equals(text.Substring(position - linkTagR.Length, linkTagR.Length)))
        {
            int beginTag = text.LastIndexOf(" <link=", position);
            m_InputField.text = text.Substring(0, beginTag) + text.Substring(position, text.Length - position);
            m_InputField.stringPosition = beginTag;
            m_InputField.caretPosition = GetCaretPositionFromStringIndex(beginTag);
        }
        else if (position > spriteTagR.Length && position <= text.Length && spriteTagR.Equals(text.Substring(position - spriteTagR.Length, spriteTagR.Length)))
        {
            int beginTag = text.LastIndexOf(" <sprite=", position);
            m_InputField.text = text.Substring(0, beginTag) + text.Substring(position, text.Length - position);
            m_InputField.stringPosition = beginTag;
            m_InputField.caretPosition = GetCaretPositionFromStringIndex(beginTag);
        }

        if (text.Length == 0)
        {
            m_InputField.text = " ";
            m_InputField.stringPosition = 1;
            m_InputField.caretPosition = GetCaretPositionFromStringIndex(m_InputField.stringPosition);
        }
        else if (text.StartsWith(" <") || text.StartsWith("<") || !text.StartsWith(" "))
        {
            m_InputField.text = " " + m_InputField.text;
            m_InputField.stringPosition = m_InputField.stringPosition + 1;
            m_InputField.caretPosition = GetCaretPositionFromStringIndex(m_InputField.stringPosition);
        }
    }

    /// <summary>
    /// 输入框中按回车发送消息时
    /// </summary>
    /// <param name="text">输入的文本</param>
    private void OnInputSubmit(string text)
    {               
        if (string.IsNullOrEmpty(text.Trim()))
        {
            UIManager.Instance.StartCoroutine(WaitOneFrameAndDeactiveInput());
            return;
        }

        if (text.StartsWith("  <") || text.StartsWith(" ")) { text = text.Substring(1); }

        if (text.Length > 0)
        {
            int index = m_Historys.IndexOf(text);
            if (index != -1)
            {
                m_Historys.RemoveAt(index);
                m_Historys.Add(text);
                m_HistoryIndex = m_Historys.Count;
            }
            else
            {
                m_Historys.Add(text);
                while (m_Historys.Count >= 20)
                {
                    m_Historys.RemoveAt(0);
                }
                m_HistoryIndex = m_Historys.Count;
            }

            ChatProxy chatProxy = Facade.RetrieveProxy(ProxyName.ChatProxy) as ChatProxy;
            ServerListProxy serverListProxy = Facade.RetrieveProxy(ProxyName.ServerListProxy) as ServerListProxy;
            FriendProxy friendProxy = Facade.RetrieveProxy(ProxyName.FriendProxy) as FriendProxy;
            CfgLanguageProxy languageProxy = Facade.RetrieveProxy(ProxyName.CfgLanguageProxy) as CfgLanguageProxy;

            string playerName = serverListProxy.GetCurrentCharacterVO().Name;
            ChatChannel outputChannel = GetCurrentOutputChannel();

            if (m_CurrentInputChannel == ChatChannel.Whisper)
            {
                if (friendProxy.GetBlack(m_CurrentTalkTargetUID) != null)
                {
                    chatProxy.AddMessage(outputChannel, languageProxy.GetLocalization(3001));
                    UIManager.Instance.StartCoroutine(WaitOneFrameAndActiveInput());
                    return;
                }

                text = EncodeMessage(text);
                chatProxy.AddMessage(m_CurrentInputChannel, text, 0ul, playerName);
                NetworkManager.Instance.GetChatController().OnChatSend(m_CurrentInputChannel, text, m_CurrentTalkTargetUID);
            }
            else if (m_CurrentInputChannel == ChatChannel.Group)
            {
                chatProxy.AddMessage(outputChannel, languageProxy.GetLocalization(3002));
                UIManager.Instance.StartCoroutine(WaitOneFrameAndActiveInput());
                return;
            }
            else if (m_CurrentInputChannel == ChatChannel.Union)
            {
                chatProxy.AddMessage(outputChannel, languageProxy.GetLocalization(3003));
                UIManager.Instance.StartCoroutine(WaitOneFrameAndActiveInput());
                return;
            }
            else if (m_CurrentInputChannel == ChatChannel.Faction)
            {
                chatProxy.AddMessage(outputChannel, languageProxy.GetLocalization(3004));
                UIManager.Instance.StartCoroutine(WaitOneFrameAndActiveInput());
                return;
            }
            else
            {
                text = EncodeMessage(text);
                chatProxy.AddMessage(m_CurrentInputChannel, text, 0ul, playerName);

                NetworkManager.Instance.GetChatController().OnChatSend(m_CurrentInputChannel, text);
            }
        }

        m_InputField.ActivateInputField();

        m_InputField.text = " ";
        m_InputField.stringPosition = 1;
        m_InputField.caretPosition = GetCaretPositionFromStringIndex(m_InputField.stringPosition);
    }

    /// <summary>
    /// 延迟一帧退出输入模式
    /// </summary>
    /// <returns>IEnumerator</returns>
    private IEnumerator WaitOneFrameAndDeactiveInput()
    {
        yield return new WaitForSecondsRealtime(0.2f);
        //yield return new WaitForEndOfFrame();
        //yield return new WaitForEndOfFrame();

        OnChatBlockerButtonClick();
    }

    /// <summary>
    ///  延迟一帧重新激活输入
    /// </summary>
    /// <returns>IEnumerator</returns>
    private IEnumerator WaitOneFrameAndActiveInput()
    {
        int last = m_LastInputStringPosition;

        yield return new WaitForEndOfFrame();

        m_InputField.ActivateInputField();
        m_InputField.stringPosition = last;
        m_InputField.caretPosition = GetCaretPositionFromStringIndex(m_InputField.stringPosition);
    }

    /// <summary>
    /// 输入检查协程
    /// </summary>
    /// <returns>IEnumerator</returns>
    private IEnumerator InputCheckCoroutine()
    {
        while (true)
        {
            CheckInputContent();

            yield return new WaitForEndOfFrame();
        }
    }

    /// <summary>
    /// 每帧都检查，插入点位置是不是合适，绕开TMP_Input的bug
    /// </summary>
    private void CheckInputContent()
    {
        if (m_InputPlaceholder.gameObject.activeSelf && m_OutputPanelScrollerBar.alpha == 0)
        {
            float time = Time.time - m_LastReceiveTime;
            if (time >= HIDDEN_WAIT_TIME)
            {
                time = time - HIDDEN_WAIT_TIME;
                if (time > HIDDEN_ANIM_TIME)
                {
                    m_Root.alpha = 0;
                    m_Root.blocksRaycasts = false;

                    //UNDONE 老版事件相关
                    //EventDispatcher.Global.DispatchEvent(Notifications.MSG_CHAT_CLOSED);
                }
                else
                {
                    m_Root.alpha = 1 - time / HIDDEN_ANIM_TIME;
                    //canvasGroup.blocksRaycasts = false;
                }
            }
        }

        if (GetFocus() == m_InputField)
        {
            m_InputField.placeholder.enabled = string.IsNullOrEmpty(m_InputField.text.Trim());
        }

        int newStringPosition = m_InputField.stringPosition;
        if (newStringPosition != m_CurrentInputStringPosition)
        {
            string txt = m_InputField.text;
            if (string.IsNullOrEmpty(txt))
            {
                txt = m_InputField.text = " ";
                newStringPosition = m_InputField.stringPosition = 1;
            }

            if (newStringPosition < 0 || newStringPosition > txt.Length) { return; }

            //in link tag
            bool inLink = false;

            int linkOpenL1 = txt.LastIndexOf(" <link=", newStringPosition);
            int linkOpenL2 = txt.IndexOf(" <link=", newStringPosition + (newStringPosition > 0 && txt[newStringPosition - 1] == ' ' ? -1 : 0));
            for (int i = 0; i < 2; i++)
            {
                int linkOpenL = i == 0 ? linkOpenL1 : linkOpenL2;
                if (linkOpenL != -1)
                {
                    int linkOpenR = txt.IndexOf("<u>", linkOpenL) + 3;
                    int linkCloseL = txt.IndexOf("</u>", linkOpenL + 1);
                    int linkCloseR = txt.IndexOf("</link> ", linkCloseL) + 8;

                    if (newStringPosition > linkOpenL && newStringPosition <= linkCloseR)
                    {
                        if (m_CurrentInputStringPosition == linkCloseR && newStringPosition == linkCloseR - 1)
                        {
                            newStringPosition = linkOpenL;
                        }
                        else if (m_CurrentInputStringPosition == linkOpenL && (newStringPosition == linkOpenL + 1 || newStringPosition == linkOpenR))
                        {
                            newStringPosition = linkCloseR;
                        }
                        else
                        {
                            newStringPosition = newStringPosition - linkOpenR < linkCloseL - newStringPosition ? linkOpenL : linkCloseR;
                        }
                        inLink = true;
                        break;
                    }
                }
            }

            if (!inLink)
            {
                int spriteL1 = txt.LastIndexOf(" <sprite=", newStringPosition);
                int spriteL2 = txt.IndexOf(" <sprite=", newStringPosition + (newStringPosition > 0 && txt[newStringPosition - 1] == ' ' ? -1 : 0));
                for (int i = 0; i < 2; i++)
                {
                    int spriteL = i == 0 ? spriteL1 : spriteL2;
                    if (spriteL != -1)
                    {
                        int spriteR = txt.IndexOf("> ", spriteL) + 2;
                        if (newStringPosition > spriteL && newStringPosition < spriteR)
                        {
                            if (m_CurrentInputStringPosition == spriteL && newStringPosition > spriteL)
                            {
                                newStringPosition = spriteR;
                            }
                            else if (m_CurrentInputStringPosition == spriteR && newStringPosition < spriteR)
                            {
                                newStringPosition = spriteL;
                            }
                            else
                            {
                                newStringPosition = newStringPosition - spriteL == 1 ? spriteL : spriteR;
                            }
                        }
                        break;
                    }
                }
            }

            m_CurrentInputStringPosition = newStringPosition;
            if (txt.Length == 0)
            {
                m_InputField.text = " ";
                m_CurrentInputStringPosition = 1;
            }
            else if (GetFocus() == m_InputField && m_InputField.stringPosition == 0)
            {
                m_CurrentInputStringPosition = 1;
            }
            else if ((txt.StartsWith(" <") || txt.StartsWith("<") || !txt.StartsWith(" ")))
            {
                m_InputField.text = " " + txt;
                m_CurrentInputStringPosition += 1;
            }

            m_InputField.stringPosition = m_CurrentInputStringPosition;
            m_InputField.caretPosition = GetCaretPositionFromStringIndex(m_CurrentInputStringPosition);

            if (m_CurrentInputStringPosition > 0)
            {
                m_LastInputStringPosition = m_CurrentInputStringPosition;
            }

            int inputableCount = CHATACTER_LIMIT + 1 - MeasureCharCount(m_InputField.text);
            m_InputCharLimitTip.gameObject.SetActive(inputableCount < CHATACTER_LIMIT_WAINING);
            m_InputCharLimitField.text = inputableCount.ToString();
        }
    }

    /// <summary>
    /// 计算插入光标位置
    /// </summary>
    /// <param name="stringIndex">字符索引</param>
    /// <returns>插入位置</returns>
    private int GetCaretPositionFromStringIndex(int stringIndex)
    {
        int characterCount = m_InputField.textComponent.textInfo.characterCount;
        for (int i = 0; i < characterCount; i++)
        {
            if (m_InputField.textComponent.textInfo.characterInfo[i].index >= stringIndex)
            {
                return i;
            }
        }
        return characterCount;
    }

    #endregion

    #region 表情、好友、区段

    /// <summary>
    /// 表情按钮点击时
    /// </summary>
    /// <param name="selected">按钮是否选中</param>
    private void OnEmoticonButtonClick(bool selected)
    {
        m_EmoticonBox.gameObject.SetActive(selected);
        if (m_EmoticonBox.gameObject.activeSelf)
        {
            if (m_EmoticonList.childCount > 1) { return; }

            m_EmoticonTemplate.CreatePool(0, string.Empty);

            int index = 0;
            for (; index < 16; index++)
            {
                if (index >= m_EmoticonList.childCount) { m_EmoticonTemplate.Spawn(m_EmoticonList); }

                Transform child = m_EmoticonList.GetChild(index);
                Button button = child.GetComponent<Button>();
                TMP_Text field = child.transform.Find("Label").GetComponent<TMP_Text>();
                int emoticonID = index;

                field.text = "<sprite=" + index + "/>";
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => { InsertEmoticon(emoticonID); });
            }
            while (index < m_EmoticonList.childCount)
            {
                Transform emoticon = m_EmoticonList.GetChild(index);
                emoticon.GetComponent<Button>().onClick.RemoveAllListeners();
                emoticon.Recycle();
            }
        }
    }

    /// <summary>
    /// 好友按钮点击时
    /// </summary>
    /// <param name="selected">按钮是否选中</param>
    private void OnFriendButtonClick(bool selected)
    {
        m_FriendBox.gameObject.SetActive(selected);
        if (m_FriendBox.gameObject.activeSelf)
        {
            OnFriendListChanged();
        }
        else
        {
            m_SectionBox.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 好友数据改变时
    /// </summary>
    private void OnFriendListChanged()
    {
        if (m_FriendBox.gameObject.activeSelf)
        {
            FriendProxy proxy = Facade.RetrieveProxy(ProxyName.FriendProxy) as FriendProxy;
            FriendInfoVO[] friends = proxy.GetFriends();

            System.Array.Sort(friends, (a, b) => { return a.Name.CompareTo(b.Name); });

            m_FriendTemplate.CreatePool(0, string.Empty);

            int index = 0;
            for (; index < friends.Length; index++)
            {
                if (index >= m_FriendList.childCount) { m_FriendTemplate.Spawn(m_FriendList); }

                FriendInfoVO data = friends[index];

                Transform child = m_FriendList.GetChild(index);
                Button button = child.GetComponent<Button>();
                TMP_Text field = child.transform.Find("Label").GetComponent<TMP_Text>();

                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => { OpenLinkMenu(data.UID, !string.IsNullOrEmpty(data.Name) ? data.Name : data.SName); });
                field.text = string.IsNullOrEmpty(data.Name) ? data.SName : data.Name;
            }
            while (index < m_FriendList.childCount)
            {
                Transform row = m_FriendList.GetChild(index);
                row.GetComponent<Button>().onClick.RemoveAllListeners();
                row.Recycle();
            }
        }
    }

    /// <summary>
    /// 区段按钮点击时
    /// </summary>
    private void OnSectionButtonClick()
    {
        m_SectionBox.gameObject.SetActive(!m_SectionBox.gameObject.activeSelf);
        if (m_SectionBox.gameObject.activeSelf)
        {
            m_SectionTemplate.CreatePool(0, string.Empty);

            int index = 0;
            for (int i = 64; i <= 90; i++)
            {
                if (index >= m_SectionList.childCount) { m_SectionTemplate.Spawn(m_SectionList); }

                string txt = ((char)i).ToString();
                if (i == 64) { txt = "#"; }

                Transform child = m_SectionList.GetChild(index);
                Toggle button = child.GetComponent<Toggle>();
                TMP_Text field = child.transform.Find("Label").GetComponent<TMP_Text>();

                button.onValueChanged.RemoveAllListeners();
                button.onValueChanged.AddListener((selected) => { if (selected) { ScrollFriendListTo(txt); } });
                field.text = txt.ToString();

                index++;
            }
            while (index < m_SectionList.childCount)
            {
                Transform row = m_SectionList.GetChild(index);
                row.GetComponent<Toggle>().onValueChanged.RemoveAllListeners();
                row.Recycle();
            }
        }
    }

    /// <summary>
    /// 滚动好友列表到指事实上区段
    /// </summary>
    /// <param name="starts">起始字符</param>
    private void ScrollFriendListTo(string starts)
    {
        if (m_FriendList.childCount <= 0) { return; }

        bool isNumber = starts.Equals("#");

        int index = -1;
        for (int i = 0; i < m_FriendList.childCount; i++)
        {
            Transform row = m_FriendList.GetChild(i);

            string name = row.Find("Label").GetComponent<TMP_Text>().text;
            if (isNumber)
            {
                string first = name.Substring(0, 1);
                int number = 0;
                if (int.TryParse(first, out number))
                {
                    index = i;
                    break;
                }
            }
            else if (name.StartsWith(starts))
            {
                index = i;
                break;
            }
        }

        if (index == -1) { return; }

        float contentH = m_FriendScroller.content.rect.height;
        float viewportH = m_FriendScroller.viewport.rect.height;
        float rowH = contentH / m_FriendScroller.content.childCount;

        float val = (contentH - rowH * index - viewportH) / (contentH - viewportH);
        if (val < 0)
        {
            val = 0;
        }

        m_FriendScroller.verticalNormalizedPosition = val;
    }

    #endregion


    /// <summary>
    /// 检测超链接
    /// </summary>
    /// <param name="eventData">指针事件对象</param>
    private void OnPointerClick(PointerEventData eventData)
    {
        /*
		if (!m_OutputPanel) { return; }

		int linkIndex = TMP_TextUtilities.FindIntersectingLink(m_OutputPanel, Input.mousePosition, m_RootCanvas.worldCamera);

		if (linkIndex == -1) { return; }

		TMP_LinkInfo linkInfo = m_OutputPanel.textInfo.linkInfo[linkIndex];

		string linkID = linkInfo.GetLinkID();
        string linkText = linkInfo.GetLinkText();
		if (linkID.StartsWith("item_"))
		{
			string[] ids = linkID.Split('_');
			int itemType = 0;
			int itemID = 0;
			if (int.TryParse(ids[1], out itemType) && int.TryParse(ids[2], out itemID))
			{
				PackageProxy packageProxy = Facade.RetrieveProxy(ProxyName.PackageProxy) as PackageProxy;
				if (packageProxy.SetLastViewItem(itemType, itemID))
				{
					SendNotification(Notifications.MSG_OPEN_PANEL, "TipPanel", typeof(TipPanel));
				}
			}
		}
		else if (linkID.StartsWith("role_"))
		{
			ulong roleID = 0;
			if (ulong.TryParse(linkID.Substring(5), out roleID))
			{
				OpenLinkMenu(roleID, linkText);
			}
		}
        */
    }



    private class TipComponent : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        /// <summary>
        /// 索引
        /// </summary>
		public int index;
        /// <summary>
        /// 打开tips的回调
        /// </summary>
		public UnityAction<Toggle, int> ShowTip;
        /// <summary>
        /// 关闭tips的回调
        /// </summary>
		public UnityAction<Toggle, int> HideTip;

        /// <summary>
        /// 光标进入时
        /// </summary>
        /// <param name="eventData">事件数据</param>
		public void OnPointerEnter(PointerEventData eventData)
        {
            ShowTip?.Invoke(this.GetComponent<Toggle>(), index);
        }

        /// <summary>
        /// 光标退出时
        /// </summary>
        /// <param name="eventData">事件数据</param>
		public void OnPointerExit(PointerEventData eventData)
        {
            HideTip?.Invoke(this.GetComponent<Toggle>(), index);
        }
    }
}
