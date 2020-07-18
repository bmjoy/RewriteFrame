using Eternity.FlatBuffer;
using PureMVC.Interfaces;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using static UIViewState;

public class CompositeView : UIPanelBase
{
    /// <summary>
    /// 根节点
    /// </summary>
    protected Transform Root;
    /// <summary>
    /// 动画
    /// </summary>
    public UIAnimationEvent AnimatorController;
    /// <summary>
    /// 背景容器
    /// </summary>
    public Transform BackBox;
    /// <summary>
    /// 热键容器
    /// </summary>
    public Transform HotkeyBox;
    /// <summary>
    /// 标题容器
    /// </summary>
    public Transform TitleBox;
    /// <summary>
    /// 列表容器
    /// </summary>
    public Transform ListBox;
    /// <summary>
    /// Tip容器
    /// </summary>
    public Transform TipBox;
    /// <summary>
    /// 分页容器
    /// </summary>
    public Transform PageBox;
    /// <summary>
    /// 排序容器
    /// </summary>
    public Transform SortBox;
    /// <summary>
    /// 模型容器
    /// </summary>
    public Transform ModelBox;
    /// <summary>
    /// 其它容器
    /// </summary>
    public Transform OtherBox;

    /// <summary>
    /// 视图部件列表
    /// </summary>
    private List<BaseViewPart> m_ViewPartList = new List<BaseViewPart>();
    /// <summary>
    /// 视图部件消息表
    /// </summary>
    private Dictionary<NotificationName, List<BaseViewPart>> m_ViewPartMessages = new Dictionary<NotificationName, List<BaseViewPart>>();

    /// <summary>
    /// 视图状态
    /// </summary>
    private static Dictionary<UIPanel, UIViewState> m_ViewStates = new Dictionary<UIPanel, UIViewState>();
    /// <summary>
    /// 视图状态恢复表
    /// </summary>
    private static Dictionary<UIPanel, UIViewStateRestore> m_ViewStateRestore = new Dictionary<UIPanel, UIViewStateRestore>();

    public CompositeView(UIPanel name, PanelType type) : base(name, null, type)
    {
        UiConfig? uiConfig = (Facade.RetrieveProxy(ProxyName.CfgEternityProxy) as CfgEternityProxy).GetUIConfig((uint)Name);
        if (uiConfig.HasValue)
            m_AssetAddress = uiConfig.Value.RootPrefab;
    }

    /// <summary>
    /// 视图状态
    /// </summary>
    public UIViewState State
    {
        get { return m_ViewStates[Name]; }
    }

    /// <summary>
    /// 获取所有挂点
    /// </summary>
    /// <returns>挂点列表</returns>
    public List<Transform> GetOrderMountPoints()
    {
        UICompositeViewMounts mounts = GetTransform().GetComponent<UICompositeViewMounts>();
        if (mounts)
            return mounts.GetOrderMountPoints();
        else
            return new List<Transform>();
    }

    /// <summary>
    /// 初始化
    /// </summary>
    public override void Initialize()
    {
        UICompositeViewMounts mounts = GetTransform().GetComponent<UICompositeViewMounts>();
        if (mounts)
        {
            BackBox = mounts.BackBox;
            TitleBox = mounts.TitleBox;
            ListBox = mounts.ListBox;
            TipBox = mounts.TipBox;
            PageBox = mounts.PageBox;
            SortBox = mounts.SortBox;
            OtherBox = mounts.OtherBox;
            ModelBox = mounts.ModelBox;
            HotkeyBox = mounts.HotkeyBox;
        }

        Root = FindComponent<Transform>("Content");
        AnimatorController = GetTransform().GetComponent<UIAnimationEvent>();

        //初始化页面状态
        CfgEternityProxy cfg = Facade.RetrieveProxy(ProxyName.CfgEternityProxy) as CfgEternityProxy;
        UiConfig? uiConfig = cfg.GetUIConfig((uint)Name);
        if (uiConfig.HasValue && !m_ViewStates.ContainsKey(Name))
        {
            UIViewState state = new UIViewState();
            state.UIConfig = uiConfig;
            state.Initialize();

            m_ViewStates.Add(Name, state);
        }
    }

    /// <summary>
    /// 打开时
    /// </summary>
    /// <param name="msg"></param>
    public override void OnShow(object msg)
    {
        m_ViewPartMessages.Clear();

        if (State != null)
        {
            //重置状态
            State.ResetPageDatas();

            //恢复状态
            if (m_ViewStateRestore.ContainsKey(Name))
            {
                m_ViewStateRestore[Name].ApplyTo(State);
                m_ViewStateRestore.Remove(Name);
            }

            //实例化部件
            if (State.PartIDList != null)
            {
                foreach (int id in State.PartIDList)
                {
                    BaseViewPart part = UIPartFactory.CreateUIPart((UIPartID)id);
                    if (part != null)
                    {
                        m_ViewPartList.Add(part);

                        NotificationName[] messages = part.ListNotificationInterests();
                        if (messages != null)
                        {
                            foreach (NotificationName message in messages)
                            {
                                if (!m_ViewPartMessages.ContainsKey(message))
                                    m_ViewPartMessages.Add(message, new List<BaseViewPart>());
                                if (!m_ViewPartMessages[message].Contains(part))
                                    m_ViewPartMessages[message].Add(part);
                            }
                        }
                    }
                }
            }

            //打开音效
            PlaySound(State.OpenSound);
        }

        base.OnShow(msg);

        //初始化部件
        foreach (BaseViewPart part in m_ViewPartList)
        {
            part.OwnerView = this;
            part.Opened = true;
            part.OnShow(msg);
        }

        //esc
        State.GetAction(UIAction.Common_Back).Callback -= OnEscCallback;
        State.GetAction(UIAction.Common_Back).Callback += OnEscCallback;
    }

    /// <summary>
    /// 关闭时
    /// </summary>
    /// <param name="msg"></param>
    public override void OnHide(object msg)
    {
        //关闭音效
        if (State!=null)
            PlaySound(State.CloseSound);

        //清理部件
        foreach (BaseViewPart part in m_ViewPartList)
        {
            part.OnHide();
            part.Opened = false;
            part.OwnerView = null;
        }

        //清理状态事件
        if (State != null)
            State.ClearAllEventListener();

        //
        base.OnHide(msg);

        m_ViewPartList.Clear();
        m_ViewPartMessages.Clear();
    }

    /// <summary>
    /// 获得焦点时
    /// </summary>
    public override void OnGotFocus()
    {
        base.OnGotFocus();

        foreach (BaseViewPart part in m_ViewPartList)
        {
            part.OnGotFocus();
        }
    }

    /// <summary>
    /// 焦点丢失时
    /// </summary>
    public override void OnLostFocus()
    {
        foreach (BaseViewPart part in m_ViewPartList)
        {
            part.OnLostFocus();
        }

        base.OnLostFocus();
    }

    /// <summary>
    /// 收集各个部件需要的消息
    /// </summary>
    /// <returns>消息列表</returns>
    public override NotificationName[] ListNotificationInterests()
    {
        List<NotificationName> messages = new List<NotificationName>();
        foreach(NotificationName key in m_ViewPartMessages.Keys)
        {
            messages.Add(key);
         }

        return messages.ToArray();
    }

    /// <summary>
    /// 分发消息到各个部件
    /// </summary>
    /// <param name="notification">消息</param>
    public override void HandleNotification(INotification notification)
    {
        if(m_ViewPartMessages.ContainsKey(notification.Name))
        {
            foreach(BaseViewPart part in m_ViewPartMessages[notification.Name])
            {
                part.HandleNotification(notification);
            }
        }
    }

    /// <summary>
    /// ESC键按下时
    /// </summary>
    /// <param name="callback">热键状态</param>
    protected virtual void OnEscCallback(HotkeyCallback callback)
    {
        UIManager.Instance.ClosePanel(this);
    }

    /// <summary>
    /// 播放音效
    /// </summary>
    /// <param name="soundID">音效ID</param>
    protected void PlaySound(int soundID)
    {
        if (soundID != 0)
            WwiseUtil.PlaySound(soundID, false, null);
    }

    /// <summary>
    /// 打开子面板
    /// </summary>
    public void OpenChildPanel(UIPanel panelName, object msg = null)
    {
        UIPanel myName = Name;

        UIViewStateRestore info = new UIViewStateRestore();
        info.PageIndex = State.GetPageIndex();
        info.CategoryIndex = State.GetPageCategoryIndex(info.PageIndex);
        info.SortIndex = State.GetPage().SortIndex;
        info.CompareMode = State.IsCompareMode();
        info.CompareIndex = State.GetCompareIndex();
        info.LayoutMode = State.GetPage().ListLayoutMode;
        info.SelectionIndex = State.GetPage().ListSelection;
        info.SelectionOffset = State.GetPage().ListSelectionOffset;

        UIManager.Instance.ClosePanel(this);
        UIManager.Instance.OpenPanel(panelName, msg);

        UIPanelBase panel = UIManager.Instance.GetPanel(panelName);
        if (panel != null)
        {
            if (m_ViewStateRestore.ContainsKey(myName))
                m_ViewStateRestore.Remove(myName);

            m_ViewStateRestore.Add(myName, info);

            panel.OnClosed += () =>
            {
                UIManager.Instance.OpenPanel(myName);
            };
        }
    }

    /// <summary>
    /// 视图状态恢复信息
    /// </summary>
    private class UIViewStateRestore
    {
        /// <summary>
        /// 页索引
        /// </summary>
        public int PageIndex;
        /// <summary>
        /// 分类索引
        /// </summary>
        public int CategoryIndex;
        /// <summary>
        /// 排序索引
        /// </summary>
        public int SortIndex;
        /// <summary>
        /// 比较模式
        /// </summary>
        public bool CompareMode;
        /// <summary>
        /// 比较索引
        /// </summary>
        public int CompareIndex;
        /// <summary>
        /// 布局模式
        /// </summary>
        public UIViewListLayout LayoutMode;
        /// <summary>
        /// 选择索引
        /// </summary>
        public Vector2Int SelectionIndex;
        /// <summary>
        /// 选择偏移
        /// </summary>
        public Vector2 SelectionOffset;

        /// <summary>
        /// 应用到指定状态
        /// </summary>
        /// <param name="State"></param>
        public void ApplyTo(UIViewState State)
        {
            State.SetPageIndex(PageIndex);
            State.SetPageCategoryIndex(PageIndex, CategoryIndex);
            State.SetPageSortIndex(PageIndex, SortIndex);
            State.GetPage(PageIndex).ListLayoutMode = LayoutMode;
            State.GetPage(PageIndex).ListSelection = SelectionIndex;
            State.GetPage(PageIndex).ListSelectionOffset = SelectionOffset;
            State.SetCompareMode(CompareMode);
            State.SetCompareIndex(CompareIndex);
        }
    }
}
