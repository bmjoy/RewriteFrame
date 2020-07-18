using Eternity.FlatBuffer;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class UIViewState
{
    /// <summary>
    /// 图标
    /// </summary>
    public int Icon { get; private set; }
    /// <summary>
    /// 主标题
    /// </summary>
    public string MainTitle { get; private set; }
    /// <summary>
    /// 副标题
    /// </summary>
    public string SecondaryTitle { get; private set; }
    /// <summary>
    /// 部件ID列表
    /// </summary>
    public int[] PartIDList { get; private set; }
    /// <summary>
    /// 开启音效
    /// </summary>
    public int OpenSound { get; private set; }
    /// <summary>
    /// 关闭音效
    /// </summary>
    public int CloseSound { get; private set; }
    /// <summary>
    /// 背景路径
    /// </summary>
    public string BackgroundPath { get; private set; }
    /// <summary>
    /// 主资源路径
    /// </summary>
    public string RootPerfabPath { get; private set; }

    /// <summary>
    /// 分页表
    /// </summary>
    private Dictionary<int, UIViewPage> m_Pages = new Dictionary<int, UIViewPage>();
    /// <summary>
    /// 标签页索引
    /// </summary>
    private int m_PageIndex = 0;


    /// <summary>
    /// 标签页改变之前
    /// </summary>
    public event UnityAction<int, int> OnPageIndexChangePre;
    /// <summary>
    /// 标签页改变事件
    /// </summary>
    public event UnityAction<int, int> OnPageIndexChanged;
    /// <summary>
    /// 排序索引改变事件
    /// </summary>
    public event UnityAction<int, int, int> OnSortIndexChanged;
    /// <summary>
    /// 过滤索引改变事件
    /// </summary>
    public event UnityAction<int, int, int> OnCategoryIndexChanged;
    /// <summary>
    /// 页面标签改变事件
    /// </summary>
    public event UnityAction<int, string> OnPageLabelChanged;
    /// <summary>
    /// 页面可见性改变事件
    /// </summary>
    public event UnityAction<int, bool> OnPageVisibleChanged;
    /// <summary>
    /// 样式索引改变事件
    /// </summary>
    public event UnityAction<int, UIViewListLayout, UIViewListLayout> OnLayoutStyleChanged;

    /// <summary>
    /// UI配置
    /// </summary>
    public UiConfig? UIConfig;

    /// <summary>
    /// 初始化状态
    /// </summary>
    public void Initialize()
    {
        if (!UIConfig.HasValue)
            return;

        CfgEternityProxy cfg = GameFacade.Instance.RetrieveProxy(ProxyName.CfgEternityProxy) as CfgEternityProxy;
        GameLocalizationProxy localization = GameFacade.Instance.RetrieveProxy(ProxyName.GameLocalizationProxy) as GameLocalizationProxy;

        //主面板资源
        RootPerfabPath = UIConfig.Value.RootPrefab;
        //图标
        Icon = 0;
        if (UIConfig.Value.TitleLength > 0)
        {
            int icon = 0;
            if (int.TryParse(UIConfig.Value.Title(0), out icon))
                Icon = icon;
        }
        //主标题
        MainTitle = string.Empty;
        if (UIConfig.Value.TitleLength > 1)
            MainTitle = localization.GetString(UIConfig.Value.Title(1));
        //副标题
        SecondaryTitle = string.Empty;
        if (UIConfig.Value.TitleLength > 2)
            SecondaryTitle = localization.GetString(UIConfig.Value.Title(2));
        //背景资源
        BackgroundPath = UIConfig.Value.BgPic;
        //部件ID列表
        PartIDList = UIConfig.Value.GetUiModuleArray();
        //打开音效
        OpenSound = UIConfig.Value.OpenSound;
        //关闭音效
        CloseSound = UIConfig.Value.CloseSound;

        //所有页
        int count = UIConfig.Value.LabelIdLength;
        for (int i = 0; i < count; i++)
        {
            UiLabelConfig? pageCfg = cfg.GetUIPage((uint)(UIConfig.Value.LabelId(i)));
            if (!pageCfg.HasValue)
                continue;

            UIViewPage page = GetPage(i);

            //名称
            page.Name = !string.IsNullOrEmpty(pageCfg.Value.Name) ? localization.GetString(pageCfg.Value.Name) : string.Empty;
            //图标
            page.Icon = pageCfg.Value.Id;
            //布局
            page.ListLayoutMode = pageCfg.Value.ListType == 0 ? UIViewListLayout.Row : UIViewListLayout.Grid;
            //布局切换
            page.ListLayoutAllowChange = pageCfg.Value.ListExchange != 0;

            //排序方式列表
            List<UIViewSortItem> sorters = new List<UIViewSortItem>();
            int sortCount = pageCfg.Value.SortListLength;
            for (int j = 0; j < sortCount; j++)
            {
                UIViewSortKind kind = (UIViewSortKind)pageCfg.Value.SortList(j);
                string label = string.Empty;
                if (kind != UIViewSortKind.None)
                    label = localization.GetString("package_title_" + (kind + 1022));

                sorters.Add(new UIViewSortItem() { Label = label, Kind = kind });
            }
            page.Sorters = sorters.ToArray();
            page.SortIndex = 0;

            //分类列表
            bool isFilterMode = pageCfg.Value.AssistFunc == 1;
            List<UIViewCategory> categorys = new List<UIViewCategory>();
            List<ItemType> allItemTypes = new List<ItemType>();
            int length = pageCfg.Value.CategoryLength;
            for (int j = 0; j < length; j++)
            {
                UiCategoryConfig? categoryCfg = cfg.GetUICategory((uint)pageCfg.Value.Category(j));
                if (categoryCfg.HasValue)
                {
                    UIViewCategory category = new UIViewCategory();

                    if (categoryCfg.Value.Type == 1)
                    {
                        category.Label = localization.GetString(categoryCfg.Value.LabelName);
                        category.ItemType = new ItemType[] { };
                    }
                    else
                    {
                        ItemType itemType = ItemTypeUtil.GetItemType(categoryCfg.Value.Args);

                        category.Label = TableUtil.GetLanguageString(itemType.EnumList[itemType.EnumList.Length - 1] as System.Enum);
                        category.ItemType = new ItemType[] { itemType };

                        allItemTypes.Add(itemType);
                    }

                    category.Arguments = categoryCfg.Value.GetOtherArgsArray();

                    categorys.Add(category);
                }
            }

            //附加All
            if (isFilterMode && !string.IsNullOrEmpty(pageCfg.Value.AllLabel))
            {
                UIViewCategory category = new UIViewCategory();
                category.IsAll = true;
                category.Label = localization.GetString(pageCfg.Value.AllLabel);
                category.ItemType = allItemTypes.ToArray();
                category.Arguments = new int[] { };
                categorys.Insert(0, category);
            }

            //
            page.CategoryIndex = 0;
            page.CategoryIsFilterMode = isFilterMode;
            page.Categorys = categorys.ToArray();
        }
    }

    /// <summary>
    /// 重置页面数据
    /// </summary>
    public void ResetPageDatas()
    {
        CfgEternityProxy cfg = GameFacade.Instance.RetrieveProxy(ProxyName.CfgEternityProxy) as CfgEternityProxy;

        //当前页
        m_PageIndex = 0;

        //比较模式
        m_CompareMode = false;
        m_CompareIndex = -1;

        //所有页
        int count = UIConfig.Value.LabelIdLength;
        for (int i = 0; i < count; i++)
        {
            ResetPage(i);
        }

        //重置热键
        ResetAllActionState();

        ClearAllEventListener();
    }

    /// <summary>
    /// 重置指定页数据
    /// </summary>
    private void ResetPage(int index)
    {
        if (index < 0)
            return;

        CfgEternityProxy cfg = GameFacade.Instance.RetrieveProxy(ProxyName.CfgEternityProxy) as CfgEternityProxy;
        UiLabelConfig? pageCfg = cfg.GetUIPage((uint)(UIConfig.Value.LabelId(index)));
        if (pageCfg.HasValue)
        {
            UIViewPage page = GetPage(index);
            page.CategoryIndex = 0;
            page.SortIndex = 0;
            page.Visible = true;
            page.ListLayoutMode = pageCfg.Value.ListType == 0 ? UIViewListLayout.Row : UIViewListLayout.Grid;
            page.ListSelection = Vector2Int.zero;
            page.ListSelectionOffset = Vector2.zero;
        }
    }

    /// <summary>
    /// 添除所有事件监视器
    /// </summary>
    public void ClearAllEventListener()
    {
        OnPageIndexChangePre = null;
        OnPageIndexChanged = null;
        OnPageVisibleChanged = null;
        OnPageLabelChanged = null;
        OnCategoryIndexChanged = null;
        OnSortIndexChanged = null;
        OnLayoutStyleChanged = null;

        OnCompareModeChanged = null;
        OnSelectionChanged = null;
        OnModelInfoChanged = null;

        OnActionBoxChanged = null;
        OnActionEnableChanged = null;
        OnActionStateChanged = null;
        OnActionVisibleChanged = null;

        foreach (UIViewAction hotkey in m_ActionTable.Values)
        {
            hotkey.Callback = null;
        }
    }

    /// <summary>
    /// 获取当前页数据
    /// </summary>
    /// <returns>UIViewPage</returns>
    public UIViewPage GetPage()
    {
        return GetPage(GetPageIndex());
    }
    /// <summary>
    /// 获取指定页数据
    /// </summary>
    /// <param name="index">索引</param>
    /// <returns>页</returns>
    public UIViewPage GetPage(int index)
    {
        if (!m_Pages.ContainsKey(index))
            m_Pages.Add(index, new UIViewPage());

        return m_Pages[index];
    }

    /// <summary>
    /// 获取页索引
    /// </summary>
    /// <returns>索引</returns>
    public int GetPageIndex()
    {
        return m_PageIndex;
    }
    /// <summary>
    /// 设置页索引
    /// </summary>
    /// <param name="index">索引</param>
    public void SetPageIndex(int index)
    {
        if (m_PageIndex != index)
        {
            ResetPage(m_PageIndex);

            int old = m_PageIndex;
            m_PageIndex = index;

            ResetAllActionState();

            OnPageIndexChangePre?.Invoke(old, m_PageIndex);
            OnPageIndexChanged?.Invoke(old, m_PageIndex);
        }
    }

    /// <summary>
    /// 获取页面可见性
    /// </summary>
    /// <param name="pageIndex">页面索引</param>
    /// <returns>是否可见</returns>
    public bool GetPageVisible(int pageIndex)
    {
        return GetPage(pageIndex).Visible;
    }
    /// <summary>
    /// 设置页面可见性
    /// </summary>
    /// <param name="pageIndex">页面索引</param>
    /// <param name="visible">是否可见</param>
    public void SetPageVisible(int pageIndex, bool visible)
    {
        bool oldValue = GetPageVisible(pageIndex);
        if (oldValue != visible)
        {
            GetPage(pageIndex).Visible = visible;
            OnPageVisibleChanged?.Invoke(pageIndex, visible);
        }
    }

    /// <summary>
    /// 获取页标签
    /// </summary>
    /// <param name="pageIndex">页索引</param>
    /// <returns>文本</returns>
    public string GetPageLabel(int pageIndex)
    {
        return GetPage(pageIndex).Label;
    }
    /// <summary>
    /// 设置页标签
    /// </summary>
    /// <param name="pageIndex">页索引</param>
    /// <param name="text">文本</param>
    public void SetPageLabel(int pageIndex, string text)
    {
        string oldValue = GetPageLabel(pageIndex);
        if (!string.Equals(oldValue, text))
        {
            GetPage(pageIndex).Label = text;
            OnPageLabelChanged?.Invoke(pageIndex, text);
        }
    }

    /// <summary>
    /// 获取分类索引
    /// </summary>
    /// <param name="pageIndex">页索引</param>
    /// <returns>过滤索引</returns>
    public int GetPageCategoryIndex(int pageIndex)
    {
        return GetPage(pageIndex).CategoryIndex;
    }
    /// <summary>
    /// 设置分类索引
    /// </summary>
    /// <param name="pageIndex">页索引</param>
    /// <param name="newValue">过滤索引</param>
    public void SetPageCategoryIndex(int pageIndex, int newValue)
    {
        int oldValue = GetPageCategoryIndex(pageIndex);
        if (oldValue != newValue)
        {
            GetPage(pageIndex).CategoryIndex = newValue;
            OnCategoryIndexChanged?.Invoke(pageIndex, oldValue, newValue);
        }
    }
    /// <summary>
    /// 获取分类数据
    /// </summary>
    /// <returns></returns>
    public UIViewCategory GetPageCategoryData()
    {
        UIViewPage page = GetPage();
        if (page.Categorys != null)
        {
            int categoryIndex = GetPageCategoryIndex(GetPageIndex());
            if (categoryIndex >= 0 && categoryIndex < page.Categorys.Length)
            {
                return page.Categorys[categoryIndex];
            }
        }
        return null;
    }

    /// <summary>
    /// 获取布局样式
    /// </summary>
    /// <param name="index"></param>
    /// <returns>UIViewListLayout</returns>
    public UIViewListLayout GetPageLayoutStyle(int index)
    {
        return GetPage(index).ListLayoutMode;
    }
    /// <summary>
    /// 设置布局样式
    /// </summary>
    /// <param name="index">页面索引</param>
    /// <param name="style">布局样式</param>
    public void SetPageLayoutStyle(int index, UIViewListLayout newValue)
    {
        UIViewListLayout oldValue = GetPage(index).ListLayoutMode;
        if (oldValue != newValue)
        {
            GetPage(index).ListLayoutMode = newValue;
            OnLayoutStyleChanged?.Invoke(index, oldValue, newValue);
        }
    }

    /// <summary>
    /// 设置排序索引
    /// </summary>
    /// <param name="pageindex">索引</param>
    public void SetPageSortIndex(int pageIndex, int newValue)
    {
        int oldValue = GetPage(pageIndex).SortIndex;
        if (oldValue != newValue)
        {
            GetPage(pageIndex).SortIndex = newValue;
            OnSortIndexChanged?.Invoke(pageIndex, oldValue, newValue);
        }
    }



    #region Tip相关

    /// <summary>
    /// 当前选中的数据
    /// </summary>
    private object m_SelectionData;
    /// <summary>
    /// 当前选中的数据改变事件
    /// </summary>
    public UnityAction<object> OnSelectionChanged;

    /// <summary>
    /// 是否为比较模式
    /// </summary>
    private bool m_CompareMode;
    /// <summary>
    /// 比较索引
    /// </summary>
    private int m_CompareIndex;
    /// <summary>
    /// 布局模式改变事件
    /// </summary>
    public UnityAction OnCompareModeChanged;

    /// <summary>
    /// 获取Tip数据
    /// </summary>
    /// <returns>Tip数据</returns>
    public object GetTipData()
    {
        return m_SelectionData;
    }
    /// <summary>
    /// 设置Tip数据
    /// </summary>
    /// <param name="data">Tip数据</param>
    public void SetTipData(object data)
    {
        //if(m_SelectionData!=data)
        //{
        m_SelectionData = data;
        OnSelectionChanged?.Invoke(m_SelectionData);
        //}
    }

    /// <summary>
    /// 获取比较模式
    /// </summary>
    /// <returns>布局模式</returns>
    public bool IsCompareMode()
    {
        return m_CompareMode;
    }
    /// <summary>
    /// 设置比较模式
    /// </summary>
    /// <param name="compareMode">比较模式</param>
    public void SetCompareMode(bool compareMode)
    {
        if (m_CompareMode != compareMode)
        {
            m_CompareMode = compareMode;
            OnCompareModeChanged?.Invoke();
        }
    }

    /// <summary>
    /// 获取比较索引
    /// </summary>
    /// <returns>索引</returns>
    public int GetCompareIndex()
    {
        return m_CompareIndex;
    }
    /// <summary>
    /// 设置比较索引
    /// </summary>
    /// <param name="index">索引</param>
    public void SetCompareIndex(int index)
    {
        m_CompareIndex = index;
    }

    #endregion

    #region 3D模型

    /// <summary>
    /// 模型环境的路径
    /// </summary>
    private string m_ModelEnvironmentPath;
    /// <summary>
    /// 模型的路径列表
    /// </summary>
    private Effect3DViewer.ModelInfo[] m_ModelPathList;
    /// <summary>
    /// 特效的路径
    /// </summary>
    private string m_ModelEffectPath;
    /// <summary>
    /// 模型信息改变时
    /// </summary>
    public UnityAction OnModelInfoChanged;

    /// <summary>
    /// 获取3D模型信息
    /// </summary>
    /// <param name="environment">环境Path</param>
    /// <param name="models">模型Path</param>
    /// <param name="effect">效果Path</param>
    public void Get3DModelInfo(out string environment, out Effect3DViewer.ModelInfo[] models, out string effect)
    {
        environment = m_ModelEnvironmentPath;
        models = m_ModelPathList;
        effect = m_ModelEffectPath;
    }

    /// <summary>
    /// 设置3D模型信息
    /// </summary>
    /// <param name="environment">环境Path</param>
    /// <param name="models">模型Path</param>
    /// <param name="effect">效果Path</param>
    public void Set3DModelInfo(string environment, Effect3DViewer.ModelInfo[] models, string effect)
    {
        m_ModelEnvironmentPath = environment;
        m_ModelPathList = models;
        m_ModelEffectPath = effect;

        OnModelInfoChanged?.Invoke();
    }
    /// <summary>
    /// 设置3D模型信息
    /// </summary>
    /// <param name="environment">环境Path</param>
    /// <param name="models">模型Path</param>
    /// <param name="effect">效果路径</param>
    public void Set3DModelInfo(string environment, string model, string effect)
    {
        Set3DModelInfo(environment, new Effect3DViewer.ModelInfo[] { new Effect3DViewer.ModelInfo() { perfab = model, scale = Vector3.one } }, effect);
    }
    /// <summary>
    /// 设置3D模型信息
    /// </summary>
    /// <param name="environment">环境Path</param>
    /// <param name="models">模型Path</param>
    public void Set3DModelInfo(string environment, string model)
    {
        Set3DModelInfo(environment, model, null);
    }

    #endregion

    #region 动作相关

    /// <summary>
    /// 热键挂点
    /// </summary>
    private Transform m_ActionBox = null;
    /// <summary>
    /// 热键表
    /// </summary>
    private Dictionary<string, UIViewAction> m_ActionTable = new Dictionary<string, UIViewAction>();

    /// <summary>
    /// 热键挂点改变时
    /// </summary>
    public UnityAction OnActionBoxChanged;
    /// <summary>
    /// 热键可见性改变时
    /// </summary>
    public UnityAction<string, bool> OnActionVisibleChanged;
    /// <summary>
    /// 热键可用性改变时
    /// </summary>
    public UnityAction<string, bool> OnActionEnableChanged;
    /// <summary>
    /// 热键状态改变时
    /// </summary>
    public UnityAction<string> OnActionStateChanged;

    /// <summary>
    /// 
    /// </summary>
    private bool m_ActionCompareEnabled = true;
    /// <summary>
    /// 
    /// </summary>
    public UnityAction ActionCompareEnableChanged;

    /// <summary>
    /// 获取热键容器
    /// </summary>
    /// <returns>容器</returns>
    public Transform GetActionBox()
    {
        return m_ActionBox;
    }
    public void SetActionBox(Transform box)
    {
        if (m_ActionBox != box)
        {
            m_ActionBox = box;
            OnActionBoxChanged?.Invoke();
        }
    }

    /// <summary>
    /// 获取动作
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public UIViewAction GetAction(string id)
    {
        if (!m_ActionTable.ContainsKey(id))
            m_ActionTable.Add(id, new UIViewAction(id, this));

        return m_ActionTable[id];
    }

    /// <summary>
    /// 获取动作比较模式启用
    /// </summary>
    /// <returns>bool</returns>
    public bool GetActionCompareEnabled()
    {
        return m_ActionCompareEnabled;
    }
    /// <summary>
    /// 设置动作比较模式启用
    /// </summary>
    /// <param name="value">bool</param>
    public void SetActionCompareEnabled(bool value)
    {
        m_ActionCompareEnabled = value;
        ActionCompareEnableChanged?.Invoke();
    }

    /// <summary>
    /// 重置所有动作状态
    /// </summary>
    private void ResetAllActionState()
    {
        int pageIndex = GetPageIndex();
        if (pageIndex < 0 || pageIndex >= UIConfig.Value.LabelIdLength)
            return;

        CfgEternityProxy cfg = GameFacade.Instance.RetrieveProxy(ProxyName.CfgEternityProxy) as CfgEternityProxy;
        GameLocalizationProxy localization = GameFacade.Instance.RetrieveProxy(ProxyName.GameLocalizationProxy) as GameLocalizationProxy;

        UiLabelConfig? pageCfg = cfg.GetUIPage((uint)UIConfig.Value.LabelId(pageIndex));
        if (!pageCfg.HasValue)
            return;

        int count = pageCfg.Value.HotkeyLength;
        for (int i = 0; i < count; i++)
        {
            UiHotkeyConfig? hotkeyCfg = cfg.GetUIHotkey(pageCfg.Value.Hotkey(i));
            if (!hotkeyCfg.HasValue)
                continue;

            UIViewAction key = GetAction(hotkeyCfg.Value.Id);

            string hotkey = "";
            string text = "";
            float time = 0;
            int arg = 0;

            key.StateList.Clear();
            if (key.ID == UIAction.Common_Sort)
            {
                var sortItems = GetPage().Sorters;
                for (int j = 0; j < sortItems.Length; j++)
                {
                    if (j < hotkeyCfg.Value.HotKeyLength)
                        hotkey = hotkeyCfg.Value.HotKey(j);
                    if (j < hotkeyCfg.Value.TimeLength)
                        time = hotkeyCfg.Value.Time(j);
                    if (j < hotkeyCfg.Value.ArgsLength)
                        arg = hotkeyCfg.Value.Args(j);

                    text = sortItems[j].Label;

                    key.StateList.Add(new UIViewActionState() { Hotkey = hotkey, Text = text, Time = time, Arg = arg });
                }
            }
            else
            {
                int stateCount = Mathf.Max(hotkeyCfg.Value.HotKeyLength, hotkeyCfg.Value.TimeLength, hotkeyCfg.Value.TextLength);
                for (int j = 0; j < stateCount; j++)
                {
                    if (j < hotkeyCfg.Value.HotKeyLength)
                        hotkey = hotkeyCfg.Value.HotKey(j);
                    if (j < hotkeyCfg.Value.TextLength)
                        text = localization.GetString(hotkeyCfg.Value.Text(j));
                    if (j < hotkeyCfg.Value.TimeLength)
                        time = hotkeyCfg.Value.Time(j);
                    if (j < hotkeyCfg.Value.ArgsLength)
                        arg = hotkeyCfg.Value.Args(j);

                    key.StateList.Add(new UIViewActionState() { Hotkey = hotkey, Text = text, Time = time, Arg = arg });
                }
            }
            key.State = Mathf.Min(Mathf.Max(0, hotkeyCfg.Value.NormalState), key.StateList.Count - 1);
        }
    }

    #endregion
}
