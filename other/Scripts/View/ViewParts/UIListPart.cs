using Eternity.FlatBuffer;
using Leyoutech.Core.Loader.Config;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIListPart : BaseViewPart,IComparer<object>
{
    private const string ASSET_ADDRESS = AssetAddressKey.PRELOADUI_COMMONLISTPART;

    /// <summary>
    /// 模板列表
    /// </summary>
    private Dictionary<string, GameObject> m_Templates = new Dictionary<string, GameObject>();
    /// <summary>
    /// 模板加载状态列表
    /// </summary>
    private Dictionary<string, bool> m_TemplateStates = new Dictionary<string, bool>();
    /// <summary>
    /// 当前标题模板路径
    /// </summary>
    private string m_CurrentHeadPath;
    /// <summary>
    /// 当前格子模板路径
    /// </summary>
    private string m_CurrentCellPath;
    /// <summary>
    /// 当前格子占位符模板路径
    /// </summary>
    private string m_CurrentCellPlaceholderPath;
    /// <summary>
    /// 最后一次加载模板是否页变改变导致的。
    /// </summary>
    private bool m_LastLoadTemplateIsPageChanged;

    /// <summary>
    /// 画布的相机
    /// </summary>
    private Camera m_Camera;
    /// <summary>
    /// 列表
    /// </summary>
    private UIScrollRect m_Scroller;
    /// <summary>
    /// 列表的RectTransform
    /// </summary>
    private RectTransform m_ScrollerRectTransform;
    /// <summary>
    /// 滚动控制器
    /// </summary>
    private UIStickScrollController m_ScrollerController;
    /// <summary>
    /// 列表动画
    /// </summary>
    private Animator m_ScrollerAnimator;
    /// <summary>
    /// 模板工厂
    /// </summary>
    private Factory m_Factory = new Factory();
    /// <summary>
    /// 排序标签
    /// </summary>
    private TMP_Text m_SortLabel;
    /// <summary>
    /// 排序标记
    /// </summary>
    private UIViewSortKind m_SortKind;
    /// <summary>
    /// 左上角标签
    /// </summary>
    protected TMP_Text LeftLabel { get; private set; }

    /// <summary>
    /// 需要选中的数据
    /// </summary>
    private object m_GotoData;

    /// <summary>
    /// 打开时
    /// </summary>
    /// <param name="msg">消息</param>
    public override void OnShow(object msg)
    {
        base.OnShow(msg);

        InputManager.Instance.OnInputDeviceChanged -= OnInputDeviceChanged;
        InputManager.Instance.OnInputDeviceChanged += OnInputDeviceChanged;
        InputManager.Instance.OnNavigateModeChanged -= OnNavigateModeChanged;
        InputManager.Instance.OnNavigateModeChanged += OnNavigateModeChanged;

        LoadViewPart(ASSET_ADDRESS, OwnerView.ListBox);
    }

    /// <summary>
    /// 关闭时
    /// </summary>
    public override void OnHide()
    {
        InputManager.Instance.OnInputDeviceChanged -= OnInputDeviceChanged;
        InputManager.Instance.OnNavigateModeChanged -= OnNavigateModeChanged;

        m_Factory.HeadTemplate = null;
        m_Factory.CellTemplate = null;
        m_Factory.CellPlaceholderTemplate = null;

        m_Factory.HeadReset = null;
        m_Factory.CellReset = null;
        m_Factory.CellPlaceholderReset = null;

        m_Templates.Clear();
        m_TemplateStates.Clear();
        m_CurrentHeadPath = null;
        m_CurrentCellPath = null;
        m_LastLoadTemplateIsPageChanged = true;

        base.OnHide();
    }

    /// <summary>
    /// 部件加载时
    /// </summary>
    protected override void OnViewPartLoaded()
    {
        if (GetTransform().GetComponentInParent<Canvas>() != null)
            m_Camera = GetTransform().GetComponentInParent<Canvas>().worldCamera;
        else
            Debug.Log(GetTransform());

        m_Scroller = FindComponent<UIScrollRect>("Content/Scroller");
        m_ScrollerAnimator = FindComponent<Animator>("Content/Scroller");
        m_ScrollerController = m_Scroller.gameObject.GetComponent<UIStickScrollController>();
        m_ScrollerRectTransform = m_Scroller.GetComponent<RectTransform>();

        if (m_ScrollerController == null)
            m_ScrollerController = m_Scroller.gameObject.AddComponent<UIStickScrollController>();

        m_ScrollerController.SetFocused(OwnerView.Focused);

        LeftLabel = FindComponent<TMP_Text>("Content/Scroller/Label_Tltle1");
        m_SortLabel = FindComponent<TMP_Text>("Content/Scroller/Label_Tltle2");

        if (m_Scroller)
        {
            m_Scroller.DataSorter = this;
            m_Scroller.OnHeadRenderer = OnHeadRenderer;
            m_Scroller.OnCellRenderer = OnCellRenderer;
            m_Scroller.OnCellPlaceholderRenderer = OnCellPlaceholderRenderer;
            m_Scroller.OnNavigateCallback = OnCellNavigateCallback;
            m_Scroller.OnClickCallback = OnCellClickCallback;
            m_Scroller.OnDoubleClickCallback = OnItemDoubleClick;
            m_Scroller.OnOverCallback = OnCellOverCallback;
            m_Scroller.OnSelectionChanged = OnSelectionChanged;
            m_Scroller.SetTemplateFactory(null);

            State.OnPageIndexChanged -= OnPageChanged;
            State.OnPageIndexChanged += OnPageChanged;
            State.OnSortIndexChanged -= OnSortChanged;
            State.OnSortIndexChanged += OnSortChanged;
            State.OnCategoryIndexChanged -= OnFilterChanged;
            State.OnCategoryIndexChanged += OnFilterChanged;

            State.OnLayoutStyleChanged -= OnLayoutStyleChanged;
            State.OnLayoutStyleChanged += OnLayoutStyleChanged;

            State.OnCompareModeChanged -= OnCompareModeChanged;
            State.OnCompareModeChanged += OnCompareModeChanged;

            State.GetAction(UIAction.Common_Sort).Callback -= OnSortKeyPress;
            State.GetAction(UIAction.Common_Sort).Callback += OnSortKeyPress;

            InitializePage();

            OnInputDeviceChanged(InputManager.Instance.CurrentInputDevice);
        }
    }

    /// <summary>
    /// 部件卸载时
    /// </summary>
    protected override void OnViewPartUnload()
    {
        State.OnPageIndexChanged -= OnPageChanged;
        State.OnSortIndexChanged -= OnSortChanged;
        State.OnCategoryIndexChanged -= OnFilterChanged;

        State.OnLayoutStyleChanged -= OnLayoutStyleChanged;
        State.OnCompareModeChanged -= OnCompareModeChanged;

        State.GetAction(UIAction.Common_Sort).Callback -= OnSortKeyPress;
        State.GetAction(UIAction.Common_Grid_List).Callback -= OnToggleGridLayout;

        OwnerView.DeleteHotKey("LayoutMode");
        OwnerView.DeleteHotKey("CompareMode");

        m_Camera = null;

        if (m_Scroller)
        {
            m_Scroller.vertical = true;
            m_Scroller.ClearData();
            m_Scroller.DataSorter = null;
            m_Scroller.OnHeadRenderer = null;
            m_Scroller.OnCellRenderer = null;
            m_Scroller.OnCellPlaceholderRenderer = null;
            m_Scroller.OnNavigateCallback = null;
            m_Scroller.OnClickCallback = null;
            m_Scroller.OnDoubleClickCallback = null;
            m_Scroller.OnSelectionChanged -= null;
            m_Scroller.SetTemplateFactory(null);
            m_Scroller = null;
        }

        if (m_ScrollerController)
        {
            m_ScrollerController.SetFocused(false);
            m_ScrollerController = null;
        }

        m_ScrollerAnimator = null;
        m_ScrollerRectTransform = null;

        LeftLabel = null;
        m_SortLabel = null;
    }

    public override void OnGotFocus()
    {
        base.OnGotFocus();

        if (m_Scroller)
        {
            m_Scroller.vertical = true;
            m_Scroller.OnEndDrag(new PointerEventData(EventSystem.current) { button = PointerEventData.InputButton.Left });
        }

        if (m_ScrollerController)
            m_ScrollerController.SetFocused(OwnerView.Focused);
    }

    /// <summary>
    /// 丢失焦点时
    /// </summary>
    public override void OnLostFocus()
    {
        if (m_Scroller)
        {
            m_Scroller.vertical = false;
            m_Scroller.OnEndDrag(new PointerEventData(EventSystem.current) { button = PointerEventData.InputButton.Left });
        }

        if(m_ScrollerController)
            m_ScrollerController.SetFocused(OwnerView.Focused);

        base.OnLostFocus();
    }

    /// <summary>
    /// 输入设备改变时
    /// </summary>
    private void OnInputDeviceChanged(InputManager.GameInputDevice device)
    {
        if (!m_Scroller)
            return;

        if (!OwnerView.Focused)
            return;

        if (m_Scroller.verticalScrollbar)
            m_Scroller.verticalScrollbar.interactable = InputManager.Instance.CurrentInputDevice == InputManager.GameInputDevice.KeyboardAndMouse;

        if (m_Scroller.horizontalScrollbar)
            m_Scroller.horizontalScrollbar.interactable = InputManager.Instance.CurrentInputDevice == InputManager.GameInputDevice.KeyboardAndMouse;

        if (InputManager.Instance.CurrentInputDevice == InputManager.GameInputDevice.KeyboardAndMouse)
        {
            var selection = State.GetPage().ListSelection;
            if (selection.x == -1 && selection.y == -1)
                m_Scroller.SetSelection(new Vector2Int(0, 0));
            else
                m_Scroller.SetSelection(State.GetPage().ListSelection);
        }
        else
        {
            if (!InputManager.Instance.GetNavigateMode())
                m_Scroller.SetSelection(new Vector2Int(-1, -1));
        }
    }

    /// <summary>
    /// 切换到非导航模式时取选焦点目标
    /// </summary>
    /// <param name="device">是否为原始</param>
    private void OnNavigateModeChanged(bool isNavigate)
    {
        if (!m_Scroller)
            return;

        if (InputManager.Instance.CurrentInputDevice == InputManager.GameInputDevice.KeyboardAndMouse)
            return;

        if (OwnerView.Focused && !isNavigate)
            m_Scroller.SetSelection(new Vector2Int(-1, -1));
    }

    /// <summary>
    /// 初始化页面
    /// </summary>
    private void InitializePage()
    {
        UpdateSortView();

        LoadListTemplate(true);

        if (m_ScrollerAnimator)
        {
            m_ScrollerAnimator.SetBool("IsSimple", State.GetPageLayoutStyle(State.GetPageIndex()) == UIViewListLayout.Grid);
            m_ScrollerAnimator.SetBool("IsCompare", State.IsCompareMode());
        }

        ListenerGridLayoutToggle();
        //OwnerView.State.SetPageSelection(OwnerView.State.GetPageIndex(), new Vector2Int(0, 0));
        OnPageIndexChanged(State.GetPageIndex(), State.GetPageIndex());
    }

    /// <summary>
    /// 当前页面改变时
    /// </summary>
    /// <param name="oldIndex">变化前的索引号</param>
    /// <param name="newIndex">变化后的索引号</param>
    private void OnPageChanged(int oldIndex, int newIndex)
    {
        State.SetTipData(null);

        InitializePage();
    }

    /// <summary>
    /// 过滤索引改变时
    /// </summary>
    /// <param name="pageIndex">页索引</param>
    /// <param name="oldIndex">变化前的索引号</param>
    /// <param name="newIndex">变化后的索引号</param>
    private void OnFilterChanged(int pageIndex, int oldIndex, int newIndex)
    {
        if (pageIndex != State.GetPageIndex())
            return;

        if (!State.GetPage().CategoryIsFilterMode)
        {
            //定位模式
            Vector2Int selection = m_Scroller.GetSelection();
            Vector2 selectionOffset = m_Scroller.GetSelectionOffset();

            //State.GetPage().ListSelection = selection;
            //State.GetPage().ListSelectionOffset = selectionOffset;

            m_Scroller.SetSelection(new Vector2Int(newIndex, selection.y));
        }
        else
        {
            //过滤模式
            //State.SetPageSelection(pageIndex, m_Scroller.GetSelection());
            //State.SetPageSelectionOffset(pageIndex, m_Scroller.GetSelectionOffset());

            OnFilterIndexChanged(oldIndex, newIndex);

            m_Scroller.SetSelection(new Vector2Int(0, 0));
        }
    }


    #region 排序功能

    /// <summary>
    /// 排序键按下时
    /// </summary>
    /// <param name="callback">热键状态</param>
    private void OnSortKeyPress(HotkeyCallback callback)
    {
        if (!callback.performed)
            return;

        if (OwnerView == null)
            return;

        int index = State.GetPage().SortIndex;
        if (index >= State.GetPage().Sorters.Length - 1)
            index = 0;
        else
            index++;

        State.SetPageSortIndex(State.GetPageIndex(), index);
    }

    /// <summary>
    /// 排序索引改变时
    /// </summary>
    /// <param name="pageIndex">页索引</param>
    /// <param name="oldIndex">变化前的索引号</param>
    /// <param name="newIndex">变化后的索引号</param>
    private void OnSortChanged(int pageIndex, int oldIndex, int newIndex)
    {
        if (pageIndex == State.GetPageIndex())
        {
            UpdateSortView();

            m_Scroller.SortDatas(false);
        }
    }

    /// <summary>
    /// 排序方法
    /// </summary>
    /// <param name="a">A</param>
    /// <param name="b">B</param>
    /// <returns>排序结果</returns>
    public int Compare(object a, object b)
    {
        return Compare(m_SortKind, a, b);
    }

    /// <summary>
    /// 按指定方式排序
    /// </summary>
    /// <param name="kind">排序方式</param>
    /// <param name="a">数据项A</param>
    /// <param name="b">数据项B</param>
    /// <returns>排序结果</returns>
    protected virtual int Compare(UIViewSortKind kind, object a, object b)
    {
        int result = 0;
        if (a is ItemBase && b is ItemBase)
        {
            ItemBase left = a as ItemBase;
            ItemBase right = b as ItemBase;

            switch (kind)
            {
                case UIViewSortKind.Quality:
                    result = right.ItemConfig.Quality - left.ItemConfig.Quality;
                    break;
                case UIViewSortKind.TLevel:
                    result = left.ItemConfig.Grade - right.ItemConfig.Grade;
                    break;
                case UIViewSortKind.StrengthenLevel:
                    result = right.Lv - left.Lv;
                    break;
                case UIViewSortKind.UseLevel:
                    result = right.ItemConfig.PlayerLvLimit - left.ItemConfig.PlayerLvLimit;
                    break;
                case UIViewSortKind.Name:
                    result = string.Compare(TableUtil.GetItemName(left.TID), TableUtil.GetItemName(right.TID));
                    break;
                case UIViewSortKind.GetTime:
                    result = (int)(right.CreateTime - left.CreateTime);
                    break;
                case UIViewSortKind.SellPrice:
                    result = (int)(right.ItemConfig.MoneyPrice - left.ItemConfig.MoneyPrice);
                    break;
            }

            if (result == 0)
                result = right.ItemConfig.Order - left.ItemConfig.Order;
            if (result == 0)
                result = (int)(right.TID - left.TID);
        }
        else if(a is Item && b is Item)
        {
            Item left = (Item)a;
            Item right = (Item)b;
            switch (kind)
            {
                case UIViewSortKind.Quality:
                    result = right.Quality - left.Quality;
                    break;
                case UIViewSortKind.TLevel:
                    result = left.Grade - right.Grade;
                    break;
                case UIViewSortKind.UseLevel:
                    result = right.PlayerLvLimit - left.PlayerLvLimit;
                    break;
                case UIViewSortKind.Name:
                    result = string.Compare(TableUtil.GetItemName(left.Id), TableUtil.GetItemName(right.Id));
                    break;
                case UIViewSortKind.SellPrice:
                    result = (int)(right.MoneyPrice - left.MoneyPrice);
                    break;
            }

            if (result == 0)
                result = right.Order - left.Order;
            if (result == 0)
                result = (int)(right.Id - left.Id);
        }

        return result;
    }

    /// <summary>
    /// 更新排序视图
    /// </summary>
    private void UpdateSortView()
    {
        int sortIndex = State.GetPage().SortIndex;
        UIViewSortItem[]  sortItems = State.GetPage().Sorters;

        //记录当前排序
        m_SortKind = sortIndex >= 0 && sortIndex < sortItems.Length ? sortItems[sortIndex].Kind : UIViewSortKind.None;

        //排序热键显示
        State.GetAction(UIAction.Common_Sort).State = sortIndex + 1 < sortItems.Length ? sortIndex + 1 : 0;

        //当前排序显示
        if (m_SortLabel)
            m_SortLabel.text = sortIndex >= 0 && sortIndex < sortItems.Length ? sortItems[sortIndex].Label : string.Empty;
    }

    #endregion

    #region 列表模式/网格模式

    /// <summary>
    /// 当前布局样式改变时
    /// </summary>
    /// <param name="pageIndex">页索引</param>
    /// <param name="oldStyle">变化前的布局样式</param>
    /// <param name="newStyle">变化后的布局样式</param>
    private void OnLayoutStyleChanged(int pageIndex, UIViewListLayout oldStyle, UIViewListLayout newStyle)
    {
        if (pageIndex == State.GetPageIndex())
        {
            LoadListTemplate(false);
            if(State.GetPageLayoutStyle(State.GetPageIndex()) == UIViewListLayout.Grid)
            {
                if (m_ScrollerAnimator)
                {
                    m_ScrollerAnimator.SetBool("IsSimple", true);
                    m_ScrollerAnimator.SetBool("IsCompare", State.IsCompareMode());
                }
                State.GetAction(UIAction.Common_Grid_List).State = 1;
            }
            else
            {
                if (m_ScrollerAnimator)
                {
                    m_ScrollerAnimator.SetBool("IsSimple", false);
                    m_ScrollerAnimator.SetBool("IsCompare", State.IsCompareMode());
                }
                State.GetAction(UIAction.Common_Grid_List).State = 0;
            }
        }
    }

    /// <summary>
    /// 监视网格布局
    /// </summary>
    private void ListenerGridLayoutToggle()
    {
        State.GetAction(UIAction.Common_Grid_List).State = 0;
        State.GetAction(UIAction.Common_Grid_List).Callback -= OnToggleGridLayout;

        if (Config.HasValue)
        {
            int pageIndex = State.GetPageIndex();
            if (pageIndex < Config.Value.LabelIdLength)
            {
                uint pageCfgID = (uint)Config.Value.LabelId(pageIndex);

                CfgEternityProxy cfg = GameFacade.Instance.RetrieveProxy(ProxyName.CfgEternityProxy) as CfgEternityProxy;
                UiLabelConfig? pageConfig = cfg.GetUIPage(pageCfgID);
                if (pageConfig.HasValue)
                {
                    if (State.GetPageLayoutStyle(State.GetPageIndex()) == UIViewListLayout.Row)
                        State.GetAction(UIAction.Common_Grid_List).State = 0;
                    else
                        State.GetAction(UIAction.Common_Grid_List).State = 1;


                    if (pageConfig.Value.ListExchange != 0)
                    {
                        State.GetAction(UIAction.Common_Grid_List).Callback += OnToggleGridLayout;
                    }
                }
            }
        }
    }
    /// <summary>
    /// 切换布局
    /// </summary>
    /// <param name="callback">热键状态</param>
    private void OnToggleGridLayout(HotkeyCallback callback)
    {
        if (callback.performed)
        {
            State.GetPage().ListSelection = m_Scroller.GetSelection();
            State.GetPage().ListSelectionOffset = m_Scroller.GetSelectionOffset();

            switch (State.GetPageLayoutStyle(State.GetPageIndex()))
            {
                case UIViewListLayout.Row: State.SetPageLayoutStyle(State.GetPageIndex(), UIViewListLayout.Grid); break;
                case UIViewListLayout.Grid: State.SetPageLayoutStyle(State.GetPageIndex(), UIViewListLayout.Row); break;
            }
        }
    }

    #endregion

    /// <summary>
    /// 布局模式改变时
    /// </summary>
    private void OnCompareModeChanged()
    {
        State.GetPage().ListSelection = m_Scroller.GetSelection();
        State.GetPage().ListSelectionOffset = m_Scroller.GetSelectionOffset();

        LoadListTemplate(false);

        if (m_ScrollerAnimator)
        {
            m_ScrollerAnimator.SetBool("IsCompare", State.IsCompareMode());
        }
    }

    /// <summary>
    /// 处理导航回调
    /// </summary>
    /// <param name="success">是否成功</param>
    private void OnCellNavigateCallback(bool success)
    {
        if (success)
            PlaySound((int)WwiseMusic.Music_Button_SelectMove_valid);
        else
            PlaySound((int)WwiseMusic.Music_Button_SelectMove_invalid);
    }

    /// <summary>
    /// 处理点击回调
    /// </summary>
    /// <param name="pressed">是否按下</param>
    private void OnCellClickCallback(bool pressed, int groupIndex, int listIndex)
    {
        if (pressed)
            m_Scroller.SetSelection(new Vector2Int(groupIndex, listIndex));
        else
            PlaySound((int)WwiseMusic.Music_Button_Click_2);
    }
    /// <summary>
    /// 处理悬浮回调
    /// </summary>
    /// <param name="enter"></param>
    private void OnCellOverCallback(bool enter, int groupIndex, int listIndex)
    {
        if (InputManager.Instance.CurrentInputDevice == InputManager.GameInputDevice.KeyboardAndMouse)
            return;

        if (!InputManager.Instance.GetNavigateMode())
        {
            if (enter)
            {
                m_Scroller.SetSelection(new Vector2Int(groupIndex, listIndex));
            }
            else
            {
                m_Scroller.SetSelection(new Vector2Int(-1, -1));
                EventSystem.current.SetSelectedGameObject(null);
            }
        }
    }

    /// <summary>
    /// 选择改变时
    /// </summary>
    /// <param name="groupIndex">组索引</param>
    /// <param name="listIndex">列表索引</param>
    /// <param name="data">数据</param>
    private void OnSelectionChanged(int groupIndex, int listIndex, object data)
    {
        State.GetPage().ListSelection = m_Scroller.GetSelection();
        State.GetPage().ListSelectionOffset = m_Scroller.GetSelectionOffset();

        State.SetTipData(data);
    }

    #region 子类扩展方法

    /// <summary>
    /// 刷新一下当前的所有单元格
    /// </summary>
    protected void RefreshCurrentAllCells()
    {
        if(m_Scroller)
            m_Scroller.RefreshCurrentAllCells();

        object old = State.GetTipData();
        State.SetTipData(null);
        State.SetTipData(old);
    }

    /// <summary>
    /// 获取排序功能是否启用
    /// </summary>
    /// <returns></returns>
    protected bool GetSortEnabled()
    {
        return m_Scroller.DataSorter != null;
    }

    /// <summary>
    /// 设置排序功能是否启用
    /// </summary>
    /// <param name="enabled"></param>
    protected void SetSortEnabled(bool enabled)
    {
        if (enabled)
            m_Scroller.DataSorter = this;
        else
            m_Scroller.DataSorter = null;
    }

    /// <summary>
    /// 设置页码和选择
    /// </summary>
    /// <param name="index">页码</param>
    /// <param name="selection">选中项</param>
    protected void SetPageAndSelection(int index, object selection)
    {
        State.SetPageIndex(index);

        m_GotoData = selection;
    }

    /// <summary>
    /// 分页索引改变时
    /// </summary>
    /// <param name="oldIndex">变化前的索引号</param>
    /// <param name="newIndex">变化后的索引号</param>
    protected virtual void OnPageIndexChanged(int oldIndex, int newIndex)
    {
    }

    /// <summary>
    /// 过滤索引改变时
    /// </summary>
    /// <param name="oldIndex"></param>
    /// <param name="newIndex"></param>
    protected virtual void OnFilterIndexChanged(int oldIndex, int newIndex)
    {
    }

    /// <summary>
    /// 获取标题模板
    /// </summary>
    /// <returns>模板地址</returns>
    protected virtual string GetHeadTemplate()
    {
        return AssetAddressKey.PRELOADUIELEMENT_PACKAGETITLEELEMENT;
    }

    /// <summary>
    /// 获取单元格模板
    /// </summary>
    /// <returns>模板地址</returns>
    protected virtual string GetCellTemplate()
    {
        return null;
    }

    /// <summary>
    /// 获取单元格占位符模板
    /// </summary>
    /// <returns>模板地址</returns>
    protected virtual string GetCellPlaceholderTemplate()
    {
        return AssetAddressKey.PRELOADUIELEMENT_PACKAGEELEMENT_EMPTY;
    }

    /// <summary>
    /// 标题栏渲染时
    /// </summary>
    /// <param name="groupIndex">组索引</param>
    /// <param name="headData">数据</param>
    /// <param name="headView">视图</param>
    protected virtual void OnHeadRenderer(int groupIndex, object headData, RectTransform headView)
    {
        TMP_Text label = FindComponent<TMP_Text>(headView, "Label_Name");
        if (label)
            label.text = headData.ToString();
    }

    /// <summary>
    /// 标题栏回收时
    /// </summary>
    /// <param name="cellView">单元格视图</param>
    protected virtual void OnHeadRecycle(RectTransform headView)
    {
        var animator = headView.GetComponent<Animator>();
        if (animator)
        {
            animator.SetBool("IsOn", false);
            animator.SetTrigger("Normal");
            animator.Update(100.0f);
        }
    }

    /// <summary>
    /// 单元格渲染时
    /// </summary>
    /// <param name="index">索引</param>
    /// <param name="data">数据</param>
    /// <param name="view">视图</param>
    /// <param name="selected">是否选中</param>
    private void OnCellRenderer(Vector2Int index, object data, RectTransform view, bool selected)
    {
        OnCellRenderer(index.x, index.y, data, view, selected);
    }

    /// <summary>
    /// 单元格渲染时
    /// </summary>
    /// <param name="groupIndex">组索引</param>
    /// <param name="cellIndex">单元格索引</param>
    /// <param name="cellData">单元格数据</param>
    /// <param name="cellView">单元格视图</param>
    /// <param name="selected">是否选中</param>
    protected virtual void OnCellRenderer(int groupIndex, int cellIndex, object cellData, RectTransform cellView, bool selected)
    {
    }

    /// <summary>
    /// 单元格回收时
    /// </summary>
    /// <param name="cellView">单元格视图</param>
    protected virtual void OnCellRecycle(RectTransform cellView)
    {
        var animator = cellView.GetComponent<Animator>();
        if (animator)
        {
            animator.SetBool("IsOn", false);
            animator.SetTrigger("Normal");
            animator.Update(100.0f);
        }
    }

    /// <summary>
    /// 单元格占位符渲染时
    /// </summary>
    /// <param name="index">索引</param>
    /// <param name="view">视图</param>
    /// <param name="selected">是否选中</param>
    private void OnCellPlaceholderRenderer(Vector2Int index, RectTransform view, bool selected)
    {
        OnCellPlaceholderRenderer(index.x, index.y, view, selected);
    }

    /// <summary>
    /// 单元格占位符回收时
    /// </summary>
    /// <param name="cellView">单元格占位符</param>
    protected virtual void OnCellPlaceholderRecycle(RectTransform cellView)
    {
        var animator = cellView.GetComponent<Animator>();
        if (animator)
        {
            animator.SetBool("IsOn", false);
            animator.SetTrigger("Normal");
            animator.Update(100.0f);
        }
    }

    /// <summary>
    /// 单元格占位符渲染时
    /// </summary>
    /// <param name="groupIndex">组索引</param>
    /// <param name="cellIndex">单元格索引</param>
    /// <param name="cellView">单元格视图</param>
    /// <param name="selected">是否选中</param>
    protected virtual void OnCellPlaceholderRenderer(int groupIndex, int cellIndex, RectTransform cellView, bool selected)
    {
        Animator animator = cellView.GetComponent<Animator>();
        if (animator)
        {
            animator.SetBool("IsOn", selected);
            if (!selected)
                animator.SetTrigger("Normal");
        }
    }

    /// <summary>
    /// 清空数据
    /// </summary>
    protected void ClearData()
    {
        if (m_Scroller == null)
        {
            return;
        }
        m_Scroller.ClearData();
    }

    /// <summary>
    /// 添加带标题的数据列表
    /// </summary>
    /// <param name="headData">标题数据</param>
    /// <param name="itemDatas">列表数据</param>
    protected void AddDatas(object headData, object[] itemDatas)
    {
        AddDatas(headData, new List<object>(itemDatas));
    }
    /// <summary>
    /// 添加数据
    /// </summary>
    /// <param name="headData"></param>
    /// <param name="itemDatas"></param>
    protected void AddDatas(object headData, List<object> itemDatas)
    {
        m_Scroller.AddDatas(headData, itemDatas);
    }

    /// <summary>
    /// 双击数据项
    /// </summary>
    protected virtual void OnItemDoubleClick(int groupIndex, int listIndex, object data)
    {

    }
    #endregion


    /// <summary>
    /// 加载列表模板
    /// </summary>
    private void LoadListTemplate(bool isFromPageChanged)
    {
        m_LastLoadTemplateIsPageChanged = isFromPageChanged;

        string headPath = GetHeadTemplate();
        string cellPath = GetCellTemplate();
        string emptyPath = GetCellPlaceholderTemplate();

        bool needLoadHead = !string.IsNullOrEmpty(headPath) && !m_TemplateStates.ContainsKey(headPath);
        bool needLoadCell = !string.IsNullOrEmpty(cellPath) && !m_TemplateStates.ContainsKey(cellPath);
        bool needLoadEmpty = !string.IsNullOrEmpty(emptyPath) && !m_TemplateStates.ContainsKey(emptyPath);

        if (needLoadHead)
            LoadListTemplate(headPath);

        if (needLoadCell)
            LoadListTemplate(cellPath);

        if (needLoadEmpty)
            LoadListTemplate(emptyPath);

        if (needLoadHead || needLoadCell || needLoadEmpty)
        {
            m_Scroller.RemoveAllCells();
            m_Scroller.SetTemplateFactory(null);
        }

        if(!needLoadHead && !needLoadCell && !needLoadEmpty)
            ResetListTemplate();
    }

    /// <summary>
    /// 加载列表模板
    /// </summary>
    /// <param name="assetAddress">资源路径</param>
    private void LoadListTemplate(string assetAddress)
    {
        string path = assetAddress;

        if (m_TemplateStates.ContainsKey(path))
            return;

        m_TemplateStates[path] = false;

        AssetUtil.LoadAssetAsync(assetAddress,
            (pathOrAddress, returnObject, userData) =>
            {
                //忽略关闭之后的加载回调
                if (!m_Scroller) return;
                //忽略重复的加载
                if (m_Templates.ContainsKey(path)) return;

                if (returnObject != null)
                {
                    GameObject prefab = (GameObject)returnObject;
                    prefab.CreatePool(pathOrAddress);
                    m_Templates[path] = prefab;
                    m_TemplateStates[path] = true;
                }
                else
                {
                    if (m_Templates.ContainsKey(path))
                        m_Templates.Remove(path);

                    Debug.LogError(string.Format("资源加载成功，但返回null,  pathOrAddress = {0}", pathOrAddress));
                }

                ResetListTemplate();
            });
    }


    /// <summary>
    /// 重置列表模板
    /// </summary>
    private void ResetListTemplate()
    {
        if (!m_Scroller) return;
        if (OwnerView == null) return;

        string headPath = GetHeadTemplate();
        string cellPath = GetCellTemplate();
        string emptyPath = GetCellPlaceholderTemplate();

        bool needLoadHead = !string.IsNullOrEmpty(headPath);
        bool needLoadCell = !string.IsNullOrEmpty(cellPath);
        bool needLoadEmpty = !string.IsNullOrEmpty(emptyPath);

        bool allLoaded = true;
        if (needLoadHead && !m_Templates.ContainsKey(headPath))
            allLoaded = false;
        if (needLoadCell && !m_Templates.ContainsKey(cellPath))
            allLoaded = false;
        if (needLoadEmpty && !m_Templates.ContainsKey(emptyPath))
            allLoaded = false;

        if (allLoaded)
        {
            if (!string.Equals(headPath, m_CurrentHeadPath) || !string.Equals(cellPath, m_CurrentCellPath) || !string.Equals(emptyPath, m_CurrentCellPlaceholderPath))
            {
                m_Factory.HeadTemplate = !string.IsNullOrEmpty(headPath) && m_Templates.ContainsKey(headPath) ? m_Templates[headPath] : null;
                m_Factory.CellTemplate = !string.IsNullOrEmpty(cellPath) && m_Templates.ContainsKey(cellPath) ? m_Templates[cellPath] : null;
                m_Factory.CellPlaceholderTemplate = !string.IsNullOrEmpty(emptyPath) && m_Templates.ContainsKey(emptyPath) ? m_Templates[emptyPath] : null;

                m_Factory.HeadReset = null;
                if (m_Factory.HeadTemplate != null)
                    m_Factory.HeadReset = OnHeadRecycle;

                m_Factory.CellReset = null;
                if (m_Factory.CellTemplate != null)
                    m_Factory.CellReset = OnCellRecycle;

                m_Factory.CellPlaceholderReset = null;
                if (m_Factory.CellPlaceholderTemplate != null)
                    m_Factory.CellPlaceholderReset = OnCellPlaceholderRecycle;

                m_Scroller.RemoveAllCells();
                m_Scroller.SetTemplateFactory(m_Factory);

                m_CurrentHeadPath = headPath;
                m_CurrentCellPath = cellPath;
                m_CurrentCellPlaceholderPath = emptyPath;
            }

            if(InputManager.Instance.GetNavigateMode())
            {
                m_Scroller.SetSelection(State.GetPage().ListSelection);
                m_Scroller.SetSelectionOffset(State.GetPage().ListSelectionOffset);
            }
            else
            {
                if(InputManager.Instance.CurrentInputDevice == InputManager.GameInputDevice.KeyboardAndMouse)
                {
                    m_Scroller.SetSelection(State.GetPage().ListSelection);
                    m_Scroller.SetSelectionOffset(State.GetPage().ListSelectionOffset);
                }
                else
                {
                    if (m_LastLoadTemplateIsPageChanged)
                    {
                        m_Scroller.SetSelection(new Vector2Int(-1, -1));
                        m_Scroller.SetSelectionOffset(State.GetPage().ListSelectionOffset);
                    }
                    else
                    {
                        m_Scroller.SetSelection(State.GetPage().ListSelection);
                        m_Scroller.SetSelectionOffset(State.GetPage().ListSelectionOffset);
                    }
                }
            }

            if (m_GotoData != null)
            {
                if (InputManager.Instance.CurrentInputDevice == InputManager.GameInputDevice.KeyboardAndMouse || InputManager.Instance.GetNavigateMode())
                    m_Scroller.SetSelectionBy(m_GotoData);

                m_GotoData = null;
            }
        }
    }

    /// <summary>
    /// 模板工厂
    /// </summary>
    private class Factory : UIScrollRect.TempalteFactory
    {
        /// <summary>
        /// 标题模板
        /// </summary>
        public GameObject HeadTemplate;
        /// <summary>
        /// 标题重置
        /// </summary>
        public UnityAction<RectTransform> HeadReset;
        /// <summary>
        /// 单元格模板
        /// </summary>
        public GameObject CellTemplate;
        /// <summary>
        /// 单元格重置
        /// </summary>
        public UnityAction<RectTransform> CellReset;
        /// <summary>
        /// 单元格占位符模板
        /// </summary>
        public GameObject CellPlaceholderTemplate;
        /// <summary>
        /// 单元格占位符重置
        /// </summary>
        public UnityAction<RectTransform> CellPlaceholderReset;

        /// <summary>
        /// 创建标题
        /// </summary>
        /// <returns>标题实例</returns>
        public override RectTransform CreateHead()
        {
            if(HeadTemplate!=null)
                return HeadTemplate.Spawn().GetComponent<RectTransform>();
            else
                return null;
        }
        /// <summary>
        /// 回收标题
        /// </summary>
        /// <param name="head">标题实例</param>
        public override void RecycleHead(RectTransform head)
        {
            if (head)
            {
                HeadReset?.Invoke(head);
                head.gameObject.Recycle();
            }
        }

        /// <summary>
        /// 创建单元格
        /// </summary>
        /// <returns>单元格实例</returns>
        public override RectTransform CreateCell()
        {
            if (CellTemplate != null)
            {
                GameObject instance = CellTemplate.Spawn();

                ButtonWithSound sound = instance.GetComponent<ButtonWithSound>();
                if (sound)
                    Object.Destroy(sound);

                return instance.GetComponent<RectTransform>();
            }
            else
                return null;
        }
        /// <summary>
        /// 回收单元格
        /// </summary>
        /// <param name="item">单元格实例</param>
        public override void RecycleCell(RectTransform item)
        {
            if (item)
            {
                CellReset?.Invoke(item);
                item.gameObject.Recycle();
            }
        }
        /// <summary>
        /// 创建单元格占位符
        /// </summary>
        /// <returns>单元格占位符实例</returns>
        public override RectTransform CreateCellPlaceholder()
        {
            if(CellPlaceholderTemplate!=null)
            {
                GameObject instance = CellPlaceholderTemplate.Spawn();

                ButtonWithSound sound = instance.GetComponent<ButtonWithSound>();
                if (sound)
                    Object.Destroy(sound);

                return instance.GetComponent<RectTransform>();
            }
            return null;
        }
        /// <summary>
        /// 回收单元格占位符
        /// </summary>
        /// <param name="cellPlaceHolder">单元格占位符实例</param>
        public override void RecycleCellPlacehoder(RectTransform cellPlaceHolder)
        {
            if (cellPlaceHolder)
            {
                CellPlaceholderReset?.Invoke(cellPlaceHolder);
                cellPlaceHolder.Recycle();
            }
        }
    }
}
