using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[ExecuteAlways]
[DisallowMultipleComponent]
[RequireComponent(typeof(ScrollRect))]
public class GroupScrollerView : MonoBehaviour//, ICanvasElement
{
    /// <summary>
    /// 标题渲染器
    /// </summary>
    public UnityAction<object, RectTransform> RenderHead;

    /// <summary>
    /// 格子渲染器
    /// </summary>
    public UnityAction<Vector2Int, object, RectTransform, bool> RenderItem;

    /// <summary>
    /// 标题模板
    /// </summary>
    [Space]
    [SerializeField]
    private RectTransform m_HeadTemplate;
    /// <summary>
    /// 格子模板
    /// </summary>
    [SerializeField]
    private RectTransform m_ItemTemplate;
    /// <summary>
    /// 整体的内边距
    /// </summary>
    [Space]
    [SerializeField]
    private RectOffset m_ContentPadding = new RectOffset();
    /// <summary>
    /// 分组之间的间隔
    /// </summary>
    [SerializeField]
    private float m_ContentSpacing = 0;
    /// <summary>
    /// 标题高度
    /// </summary>
    [Space]
    [SerializeField]
    private float m_HeadHeight = 30;
    /// <summary>
    /// 格子列表的内边距
    /// </summary>
    [Space]
    [SerializeField]
    private RectOffset m_ListPadding = new RectOffset();
    /// <summary>
    /// 格子列表的总列数
    /// </summary>
    [Space]
    [SerializeField]
    private int m_ListColumns = 3;
    /// <summary>
    /// 单元格的大小
    /// </summary>
    [SerializeField]
    private Vector2 m_CellSize = new Vector2(96, 96);
    /// <summary>
    /// 单元格的间隔
    /// </summary>
    [SerializeField]
    private Vector2 m_CellSpace = new Vector2(22, 22);
    /// <summary>
    /// 获得焦点时自动选中
    /// </summary>
    [SerializeField]
    private bool m_AutoSelectOnGotFocus = true;

    /// <summary>
    /// 模板是否已改变
    /// </summary>
    private bool m_TemplateChanged;
    /// <summary>
    /// 是否需要清理缓存
    /// </summary>
    private bool m_DataChanged = false;
    /// <summary>
    /// 是否需要重新布局
    /// </summary>
    private bool m_IsDirty = false;

    /// <summary>
    /// 有标题数据
    /// </summary>
    private List<Data> m_GroupDatas = new List<Data>();
    /// <summary>
    /// 最后一次选中位置
    /// </summary>
    private Vector2Int m_LastSelect = new Vector2Int(-1, -1);
    /// <summary>
    /// 当前选中的项
    /// </summary>
    private Vector2Int m_CurrentSelect = new Vector2Int(0, 0);

    /// <summary>
    /// 当前可见的所有格子对应的项
    /// </summary>
    private Dictionary<string, RectTransform> m_Index2ItemA = new Dictionary<string, RectTransform>();
    private Dictionary<string, RectTransform> m_Index2ItemB = new Dictionary<string, RectTransform>();

    /// <summary>
    /// 渲染用缓存
    /// </summary>
    private static List<string> StringKeys = new List<string>();
    private static Dictionary<string, ItemInfo> VisibleItems = new Dictionary<string, ItemInfo>();

    /// <summary>
    /// 数据
    /// </summary>
    private class Data
    {
        /// <summary>
        /// 标题数据
        /// </summary>
        public object TitleData;
        /// <summary>
        /// 列表数据
        /// </summary>
        public List<object> ItemDataList;
    }

    #region 属性接口

    /// <summary>
    /// 标题模板
    /// </summary>
    public RectTransform HeadTemplate
    {
        get { return m_HeadTemplate; }
        set
        {
            if(SetProperty(ref m_HeadTemplate, value))
                m_TemplateChanged = true;
        }
    }

    /// <summary>
    /// 单元格子模板
    /// </summary>
    public RectTransform ItemTemplate
    {
        get { return m_ItemTemplate; }
        set
        {
            if(SetProperty(ref m_ItemTemplate, value))
                m_TemplateChanged = true;
        }
    }

    /// <summary>
    /// 整体的内边距
    /// </summary>
    public RectOffset ContentPadding
    {
        get { return m_ContentPadding; }
        set { SetProperty(ref m_ContentPadding, value); }
    }

    /// <summary>
    /// 分组之间的间隔
    /// </summary>
    public float ContentSpacing
    {
        get { return m_ContentSpacing; }
        set { SetProperty(ref m_ContentSpacing, value); }
    }

    /// <summary>
    /// 标题高度
    /// </summary>
    public float HeadHeight
    {
        get { return m_HeadHeight; }
        set { SetProperty(ref m_HeadHeight, value); }
    }

    /// <summary>
    /// 列数
    /// </summary>
    public int ColumnCount
    {
        get { return m_ListColumns; }
        set { SetProperty(ref m_ListColumns, Mathf.Max(1, value)); }
    }

    /// <summary>
    /// 格子列表内边距
    /// </summary>
    public RectOffset CellListPadding
    {
        get { return m_ListPadding; }
        set { SetProperty(ref m_ListPadding, value); }
    }

    /// <summary>
    /// 单元格大小
    /// </summary>
    public Vector2 CellSize
    {
        get { return m_CellSize; }
        set { SetProperty(ref m_CellSize, new Vector2(Mathf.Max(1, value.x), Mathf.Max(1, value.y))); }
    }

    /// <summary>
    /// 单元格间隔
    /// </summary>
    public Vector2 CellSpace
    {
        get { return m_CellSpace; }
        set { SetProperty(ref m_CellSpace, value); }
    }
    /// <summary>
    /// 获得焦点时自动选中
    /// </summary>
    public bool AutoSelectOnGotFocus
    {
        get { return m_AutoSelectOnGotFocus; }
        set { SetProperty(ref m_AutoSelectOnGotFocus, value); }
    }

    /// <summary>
    /// 设置属性值
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="currentValue"></param>
    /// <param name="newValue"></param>
    protected bool SetProperty<T>(ref T currentValue, T newValue)
    {
        if ((currentValue != null || newValue != null) && (currentValue == null || !currentValue.Equals(newValue)))
        {
            currentValue = newValue;
            SetDirty();
            return true;
        }
        return false;
    }

    #endregion

    #region 数据接口

    /// <summary>
    /// 清空数据
    /// </summary>
    public void ClearData()
    {
        m_GroupDatas.Clear();
        m_DataChanged = true;

        SetDirty();
    }
    
    /// <summary>
     /// 添加带标题的数据列表
     /// </summary>
     /// <param name="headData">标题数据</param>
     /// <param name="itemDatas">列表数据</param>
    public void AddDatas(object headData, object[] itemDatas)
    {
        AddDatas(headData, new List<object>(itemDatas));
    }
    public void AddDatas(object headData, List<object> itemDatas)
    {
        Data data = new Data();
        data.TitleData = headData;
        data.ItemDataList = itemDatas;
        m_GroupDatas.Add(data);
        m_DataChanged = true;
        m_LastSelect = new Vector2Int(-1, -1);

        SetDirty();
    }

    #endregion

    #region 事件处理

    /// <summary>
    /// 启用时
    /// </summary>
    protected void OnEnable()
    {
        ScrollRect scroller = GetComponent<ScrollRect>();
        if (scroller)
        {
            scroller.onValueChanged.RemoveListener(OnScroll);
            scroller.onValueChanged.AddListener(OnScroll);
        }
        m_LastSelect = new Vector2Int(-1, -1);

        SetDirty();
    }

    /// <summary>
    /// 禁用时
    /// </summary>
    protected void OnDisable()
    {
        ScrollRect scroller = GetComponent<ScrollRect>();
        if (scroller)
        {
            scroller.onValueChanged.RemoveListener(OnScroll);
        }
    }

    /// <summary>
    /// 处理验证
    /// </summary>
    private void OnValidate()
    {
        SetDirty();
    }

    /// <summary>
    /// 滚动时
    /// </summary>
    /// <param name="pos">滚动位置</param>
    private void OnScroll(Vector2 pos)
    {
        /*
        ScrollRect scroller = GetComponent<ScrollRect>();
        if (Mathf.Approximately(pos.x, scroller.horizontalNormalizedPosition) &&
            Mathf.Approximately(pos.y, scroller.verticalNormalizedPosition))
        {
            return;
        }
        */
        UpdateList();
    }

    #endregion


    #region 布局更新

    /// <summary>
    /// 标记UI需要更新
    /// </summary>
    protected void SetDirty()
    {
        if (isActiveAndEnabled && !m_IsDirty)
        {
            m_IsDirty = true;

            StartCoroutine(DelayUpdate());
        }
    }

    /// <summary>
    /// 延迟一帧更新
    /// </summary>
    /// <returns></returns>
    protected IEnumerator DelayUpdate()
    {
        yield return new WaitForEndOfFrame();

        if (m_IsDirty)
            UpdateList();
    }

    /// <summary>
    /// 更新列表
    /// </summary>
    public void UpdateList()
    {
        m_IsDirty = false;

        bool isFocusing = false;

        UIPanelFocus CurrentPanel = UIPanelFocus.Current;
        GameObject CurrentSelection = CurrentPanel != null ? CurrentPanel.GetSelection() : null;

        ScrollRect scroller = GetComponent<ScrollRect>();

        //如果模板改变了,销毁所有单元格
        if (m_TemplateChanged)
        {
            m_TemplateChanged = false;

            for (int i = 0; i < scroller.content.childCount; i += 2)
            {
                Transform titleBox = scroller.content.GetChild(i);
                for (int j = titleBox.childCount - 1; j >= 0; j--)
                {
                    RecycleTitle(titleBox.GetChild(j));
                }
                Transform itemBox = scroller.content.GetChild(i + 1);
                for (int j = itemBox.childCount - 1; j >= 0; j--)
                {
                    if (itemBox.GetChild(j).gameObject == CurrentSelection)
                        isFocusing = true;

                    RecycleItem(itemBox.GetChild(j));
                }
            }

            Transform titlePool = GetTitlePool();
            titlePool.SetParent(null);
            Object.Destroy(titlePool.gameObject);

            Transform itemPool = GetItemPool();
            itemPool.SetParent(null);
            Object.Destroy(itemPool.gameObject);

            m_Index2ItemA.Clear();
        }

        //如果数据改变了，回收所有单元格
        if (m_DataChanged)
        {
            m_DataChanged = false;
            foreach (RectTransform transform in m_Index2ItemA.Values)
            {
                if (transform.gameObject == CurrentSelection)
                    isFocusing = true;

                RecycleItem(transform);
            }
            m_Index2ItemA.Clear();
        }

        int selectedTableIndex = Mathf.Max(0, Mathf.Min(m_CurrentSelect.x, m_GroupDatas.Count - 1));
        int selectedArrayIndex = m_CurrentSelect.y;
        if (m_GroupDatas.Count > 0)
            selectedArrayIndex = Mathf.Max(0, Mathf.Min(selectedArrayIndex, m_GroupDatas[selectedTableIndex].ItemDataList.Count - 1));
        else
            selectedArrayIndex = 0;

        m_CurrentSelect = new Vector2Int(selectedTableIndex, selectedArrayIndex);

        VisibleItems.Clear();

        int index = 0;
        float beginY = 0;
        for (int i = 0; i < m_GroupDatas.Count; i++)
        {
            beginY += i > 0 ? m_ContentSpacing : 0;

            beginY += LayoutHead(i, m_GroupDatas[i], scroller, GetOrCreateBox(scroller.content, index), beginY);
            index++;

            beginY += m_ContentSpacing;

            beginY += LayoutList(i, m_GroupDatas[i], scroller, GetOrCreateBox(scroller.content, index), beginY, VisibleItems);
            index++;
        }

        //回收多余的标题
        for (int i = index; i < scroller.content.childCount; i++)
        {
            Transform box = scroller.content.GetChild(i);

            LayoutElement viewElement = box.GetComponent<LayoutElement>();
            viewElement.preferredWidth = viewElement.minWidth = 0;
            viewElement.preferredHeight = viewElement.minHeight = 0;

            if (i % 2 == 0)
            {
                for (int j = box.childCount - 1; j >= 0; j--)
                {
                    RecycleTitle(box.GetChild(j));
                }
            }
        }

        //回收不可见的单元格
        StringKeys.Clear();
        StringKeys.AddRange(m_Index2ItemA.Keys);
        foreach(string key in StringKeys)
        {
            if(!VisibleItems.ContainsKey(key))
            {
                RecycleItem(m_Index2ItemA[key]);
                m_Index2ItemA.Remove(key);
            }
        }
        StringKeys.Clear();

        //布局可见的单元格
        //bool selectChanged = !m_LastSelect.Equals(m_CurrentSelect);
        foreach (ItemInfo info in VisibleItems.Values)
        {
            string key = info.DataIndex.x + "_" + info.DataIndex.y;
            
            RectTransform item = m_Index2ItemA.ContainsKey(key) ? m_Index2ItemA[key] : CreateItem(info.ItemBox);
            item.anchoredPosition = new Vector2(info.ItemPosition.x, -info.ItemPosition.y);
            item.sizeDelta = m_CellSize;
            item.SetSiblingIndex(info.ItemDepth);

            if (isFocusing && info.DataIndex.Equals(m_CurrentSelect) && CurrentPanel != null)
                CurrentPanel.SetSelection(item.gameObject);

            if (m_Index2ItemA.ContainsKey(key))
            {
                m_Index2ItemA.Remove(key);
                if (/*selectChanged && (*/info.DataIndex.Equals(m_LastSelect) || info.DataIndex.Equals(m_CurrentSelect))//)
                {
                    RenderItem?.Invoke(info.DataIndex, info.Data, item, info.DataIndex.x == m_CurrentSelect.x && info.DataIndex.y == m_CurrentSelect.y);
                }
            }
            else
            {
                RenderItem?.Invoke(info.DataIndex, info.Data, item, info.DataIndex.x == m_CurrentSelect.x && info.DataIndex.y == m_CurrentSelect.y);
            }

            FocusItem focus = item.GetComponent<FocusItem>() ?? item.gameObject.AddComponent<FocusItem>();
            focus.Viewer = this;
            focus.Index = info.DataIndex;

            AutoSelectOnGotFocus autoFocus = item.GetComponent<AutoSelectOnGotFocus>() ?? item.gameObject.AddComponent<AutoSelectOnGotFocus>();
            autoFocus.autoSelect = AutoSelectOnGotFocus;

            if(!m_Index2ItemB.ContainsKey(key))
            {
                m_Index2ItemB.Add(key, item);
            }
        }
        VisibleItems.Clear();

        Dictionary<string, RectTransform> tmp = m_Index2ItemA;
        m_Index2ItemA = m_Index2ItemB;
        m_Index2ItemB = tmp;
        m_Index2ItemB.Clear();

        m_LastSelect = m_CurrentSelect;

        VerticalLayoutGroup scrollerLayout = scroller.content.GetComponent<VerticalLayoutGroup>();
        if (!scrollerLayout)
        {
            scrollerLayout = scroller.content.gameObject.AddComponent<VerticalLayoutGroup>();
        }
        scrollerLayout.childControlWidth = true;
        scrollerLayout.childControlHeight = true;
        scrollerLayout.childForceExpandWidth = true;
        scrollerLayout.childForceExpandHeight = true;

        RectOffset offset = scrollerLayout.padding;
        offset.left = m_ContentPadding.left;
        offset.right = m_ContentPadding.right;
        offset.top = m_ContentPadding.top;
        offset.bottom = m_ContentPadding.bottom;

        scrollerLayout.padding = m_ContentPadding;
        scrollerLayout.padding = offset;
        scrollerLayout.spacing = m_ContentSpacing;

        ContentSizeFitter scrollerSize = scroller.content.GetComponent<ContentSizeFitter>();
        if (!scrollerSize)
        {
            scrollerSize = scroller.content.gameObject.AddComponent<ContentSizeFitter>();
        }
        scrollerSize.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        scrollerSize.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        
        Canvas.ForceUpdateCanvases();

        //Debug.Log("Updateing");
    }

    private class FocusItem : UIPanelFocus.Focusable
    {
        [HideInInspector]
        public GroupScrollerView Viewer;
        [HideInInspector]
        public Vector2Int Index;

        public override GameObject OnMove(MoveDirection dir)
        {
            return Viewer.SelectBy(dir);
        }
    }

    /// <summary>
    /// 布局标题
    /// </summary>
    /// <param name="data">数据</param>
    /// <param name="scroller">滚动面板</param>
    /// <param name="depth">索引</param>
    private float LayoutHead(int tableIndex, Data data, ScrollRect scroller, RectTransform container, float layoutY)
    {
        bool needTitle = data.TitleData != null && HeadTemplate != null;

        float width = 0;
        float height = 0;

        if (needTitle)
        {
            width = m_ContentPadding.left + /*m_ListPadding.left + */(m_ListColumns * m_CellSize.x + (m_ListColumns - 1) * m_CellSpace.x)/* + m_ListPadding.right*/ + m_ContentPadding.right;
            height = m_HeadHeight;
        }

        LayoutElement viewElement = container.GetComponent<LayoutElement>();
        viewElement.preferredWidth = viewElement.minWidth = width;
        viewElement.preferredHeight = viewElement.minHeight = height;

        if (needTitle)
        {
            float scrollY = scroller.content.anchoredPosition.y - m_ContentPadding.top;
            float viewportH = scroller.viewport.rect.height;

            //列表顶部相对于视口上部的距离
            float topYOnViewport = layoutY - scrollY;
            //列表底部相对于视口上部的距离
            float bottomYOnViewport = topYOnViewport + m_HeadHeight;

            if ((0 < topYOnViewport && topYOnViewport < viewportH) || (0 < bottomYOnViewport && bottomYOnViewport < viewportH) || (topYOnViewport < 0 && bottomYOnViewport > viewportH))
            {
                RectTransform rect = GetOrCreateTitle(container, 0);
                RenderHead?.Invoke(data.TitleData, rect);

                for (int i = container.childCount - 1; i >= 1; i--)
                {
                    RecycleTitle(container.GetChild(i));
                }

                return height;
            }
        }

        for (int i = container.childCount - 1; i >= 0; i--)
        {
            RecycleTitle(container.GetChild(i));
        }

        return height;
    }

    /// <summary>
    /// 布局单元格
    /// </summary>
    /// <param name="data">数据</param>
    /// <param name="scroller">滚动面板</param>
    /// <param name="depth">索引</param>
    private float LayoutList(int tableIndex, Data data, ScrollRect scroller, RectTransform container, float layoutY, Dictionary<string,ItemInfo> visibleItems)
    {
        Vector2 listSize = GetListSize(data);

        LayoutElement listLayoutElement = container.GetComponent<LayoutElement>();
        listLayoutElement.preferredWidth = listLayoutElement.minWidth = listSize.x;
        listLayoutElement.preferredHeight = listLayoutElement.minHeight = listSize.y;

        //
        float scrollX = Mathf.Abs(scroller.content.anchoredPosition.x) - m_ContentPadding.left;
        float scrollY = Mathf.Abs(scroller.content.anchoredPosition.y) - m_ContentPadding.top;
        float viewportW = scroller.viewport.rect.width;
        float viewportH = scroller.viewport.rect.height;

        //列表顶部相对于视口上部的距离
        float topYOnViewport = layoutY + m_ListPadding.top - scrollY;
        //列表底部相对于视口上部的距离
        float bottomYOnViewport = layoutY + listSize.y - m_ListPadding.bottom - scrollY;
        //是否可见
        if((0 <= topYOnViewport && topYOnViewport <= viewportH) || (0 <= bottomYOnViewport && bottomYOnViewport <= viewportH) || (topYOnViewport <= 0 && bottomYOnViewport >= viewportH))
        {
            //计算超始索引和结束索引
            int beginIndex = 0;
            int endIndex = data.ItemDataList.Count;
            if (topYOnViewport < 0)
            {
                beginIndex = Mathf.FloorToInt(Mathf.Abs(topYOnViewport) / (m_CellSize.y + m_CellSpace.y)) * m_ListColumns;
            }
            if (bottomYOnViewport > viewportH)
            {
                endIndex = Mathf.CeilToInt((listSize.y - m_ListPadding.top - (bottomYOnViewport - viewportH)) / (m_CellSize.y + m_CellSpace.y)) * m_ListColumns;
                if (endIndex > data.ItemDataList.Count)
                {
                    endIndex = data.ItemDataList.Count;
                }
            }

            int beginRow = Mathf.FloorToInt(beginIndex / m_ListColumns);

            int column = 0;
            float cellX = m_ListPadding.left;
            float cellY = m_ListPadding.top + beginRow * (m_CellSize.y + m_CellSpace.y);

            int depth = 0;
            for (int i = beginIndex; i < endIndex; i++)
            {
                float cellLeftInViewport = cellX - scrollX;
                float cellRightInViewport = cellLeftInViewport + m_CellSize.x;

                if ((cellLeftInViewport <= 0 && cellRightInViewport >= 0) ||
                    (cellLeftInViewport <= viewportW && cellRightInViewport >= viewportW) ||
                    (cellLeftInViewport >= 0 && cellRightInViewport <= viewportW))
                {
                    ItemInfo info = new ItemInfo();
                    info.Data = data.ItemDataList[i];
                    info.DataIndex = new Vector2Int(tableIndex, i);
                    info.ItemBox = container;
                    info.ItemPosition = new Vector2(cellX, cellY);
                    info.ItemDepth = depth;

                    visibleItems.Add(tableIndex + "_" + i, info);
                    depth++;
                }

                column++;
                cellX += m_CellSize.x + m_CellSpace.x;

                if (column >= m_ListColumns)
                {
                    column = 0;

                    cellX = m_ListPadding.left;
                    cellY += m_CellSize.y + m_CellSpace.y;
                }
            }
        }

        return listSize.y;
    }

    /// <summary>
    /// 数据布局项
    /// </summary>
    private struct ItemInfo
    {
        public object Data;
        public Vector2Int DataIndex;
        public RectTransform ItemBox;
        public Vector2 ItemPosition;
        public int ItemDepth;
    }

    /// <summary>
    /// 计算列表的布局尺寸
    /// </summary>
    /// <param name="data">数据</param>
    /// <returns>计算后的宽高</returns>
    private Vector2 GetListSize(Data data)
    {
        int maxRow = Mathf.CeilToInt((float)data.ItemDataList.Count / (float)m_ListColumns);
        int maxCol = Mathf.Min(data.ItemDataList.Count, m_ListColumns);

        float width = (maxCol > 0 ? maxCol * m_CellSize.x + (maxCol - 1) * m_CellSpace.x : 0) + m_ListPadding.left + m_ListPadding.right;
        float height = (maxRow > 0 ? maxRow * m_CellSize.y + (maxRow - 1) * m_CellSpace.y : 0) + m_ListPadding.top + m_ListPadding.bottom;

        return new Vector2(width, height);
    }

    /// <summary>
    /// 获取获创建容器
    /// </summary>
    /// <param name="container">容器</param>
    /// <param name="index">索引</param>
    /// <returns>RectTransform</returns>
    private RectTransform GetOrCreateBox(RectTransform container, int index)
    {
        if (index < container.childCount)
        {
            return container.GetChild(index).GetComponent<RectTransform>();
        }
        else
        {
            RectTransform view = new GameObject(index % 2 == 0 ? "HeadBox" : "ItemBox", typeof(RectTransform), typeof(LayoutElement)).GetComponent<RectTransform>();
            view.SetParent(container, false);
            view.pivot = new Vector2(0, 1);
            view.anchorMin = new Vector2(0, 1);
            view.anchorMax = new Vector2(0, 1);
            view.anchoredPosition = Vector2.zero;
            return view;
        }
    }

    /// <summary>
    /// 获取或者创建标题行
    /// </summary>
    /// <param name="container">容器</param>
    /// <param name="index">索引</param>
    /// <returns>RectTransform</returns>
    private RectTransform GetOrCreateTitle(Transform container,int index)
    {
        if (index < container.childCount)
        {
            return container.GetChild(index).GetComponent<RectTransform>();
        }
        else
        {
            RectTransform rect = null;

            Transform pool = GetTitlePool();
            if (pool.childCount > 0)
            {
                rect = pool.GetChild(0).GetComponent<RectTransform>();
                rect.SetParent(container, false);
            }
            else
            {
                rect = Object.Instantiate(HeadTemplate, container, false);
            }

            rect.name = HeadTemplate.name;
            rect.pivot = new Vector2(0, 1);
            rect.anchorMin = new Vector2(0, 0);
            rect.anchorMax = new Vector2(1, 1);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = Vector2.zero;
            rect.gameObject.SetActive(true);

            return rect;
        }
    }

    /// <summary>
    /// 销毁标题行
    /// </summary>
    /// <param name="item">节点</param>
    private void RecycleTitle(Transform item)
    {
        //remove all event

        Transform pool = GetTitlePool();
        item.gameObject.SetActive(false);
        item.SetParent(pool, false);
    }

    /// <summary>
    /// 获取标题行的缓存池
    /// </summary>
    /// <returns></returns>
    private Transform GetTitlePool()
    {
        Transform pool = transform.Find("_TitlePool");
        if(!pool)
        {
            pool = new GameObject("_TitlePool", typeof(RectTransform)).GetComponent<Transform>();
            pool.SetParent(transform, false);
        }
        return pool;
    }

    /// <summary>
    /// 创建单元格
    /// </summary>
    /// <param name="container">容器</param>
    /// <returns>RectTransform</returns>
    private RectTransform CreateItem(Transform container)
    {
        RectTransform rect = null;

        Transform pool = GetItemPool();
        if(pool.childCount>0)
        {
            rect = pool.GetChild(0).GetComponent<RectTransform>();
            rect.SetParent(container, false);
        }
        else
        {
            rect = Object.Instantiate(ItemTemplate, container, false);
        }

        rect.name = ItemTemplate.name;
        rect.pivot = new Vector2(0, 1);
        rect.anchorMin = new Vector2(0, 1);
        rect.anchorMax = new Vector2(0, 1);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = Vector2.zero;
        rect.gameObject.SetActive(true);

        return rect;
    }

    /// <summary>
    /// 销毁单元格
    /// </summary>
    /// <param name="item">节点</param>
    private void RecycleItem(Transform item)
    {
        //remove all event

        Transform pool = GetItemPool();
        item.gameObject.SetActive(false);
        item.SetParent(pool, false);
    }

    /// <summary>
    /// 获取单元格的缓存池
    /// </summary>
    /// <returns></returns>
    private Transform GetItemPool()
    {
        Transform pool = transform.Find("_ItemPool");
        if (!pool)
        {
            pool = new GameObject("_ItemPool", typeof(RectTransform)).GetComponent<Transform>();
            pool.SetParent(transform, false);
        }
        return pool;
    }
    #endregion


    #region 选择

    /// <summary>
    /// 获取当前选择
    /// </summary>
    /// <returns>当前选择</returns>
    public Vector2Int GetSelection()
    {
        int tableIndex = Mathf.Max(0, Mathf.Min(m_CurrentSelect.x, m_GroupDatas.Count - 1));
        int arrayIndex = m_CurrentSelect.y;
        if (m_GroupDatas.Count > 0)
        {
            arrayIndex = Mathf.Max(0, Mathf.Min(arrayIndex, m_GroupDatas[tableIndex].ItemDataList.Count - 1));
        }
        else
        {
            arrayIndex = 0;
        }
        return new Vector2Int(tableIndex, arrayIndex);
    }

    /// <summary>
    /// 设置当前选择
    /// </summary>
    /// <param name="select">当前选择</param>
    public GameObject SetSelection(Vector2Int select)
    {
        m_CurrentSelect = select;

        if(m_IsDirty)
            UpdateList();

        ScrollTo(GetSelection());
        UpdateList();

        return GetSelectedItem(GetSelection());
    }

    /// <summary>
    /// 获取选中项
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    private GameObject GetSelectedItem(Vector2Int index)
    {
        string key = index.x + "_" + index.y;
        if(m_Index2ItemA.ContainsKey(key))
        {
            return m_Index2ItemA[key].gameObject;
        }
        return null;
    }

    /// <summary>
    /// 按方向选择
    /// </summary>
    /// <param name="dir">方向</param>
    /// <returns>新的选中项</returns>
    private GameObject SelectBy(MoveDirection dir)
    {
        Vector2Int selection = Vector2Int.zero;
        if (FindNext(GetSelection(), dir, out selection))
        {
            MoveDirection dirH = MoveDirection.None;
            MoveDirection dirV = MoveDirection.None;

            m_CurrentSelect = selection;

            ScrollTo(selection, out dirH, out dirV);

            MoveDirection dirFocus = MoveDirection.None;
            if (dir == MoveDirection.Up || dir == MoveDirection.Down)
            {
                if (dirV == MoveDirection.Up)
                    dirFocus = MoveDirection.Up;
                else if (dirV == MoveDirection.Down)
                    dirFocus = MoveDirection.Down;
                else if (dirV == MoveDirection.None)
                    dirFocus = dir;
            }
            else if(dir== MoveDirection.Left || dir == MoveDirection.Right)
            {
                if (dirH == MoveDirection.Left)
                    dirFocus = MoveDirection.Left;
                else if (dirH == MoveDirection.Right)
                    dirFocus = MoveDirection.Right;
                else if (dirH == MoveDirection.None)
                    dirFocus = dir;
            }

            if (dirFocus != MoveDirection.None)
            {
                Vector2Int focus = Vector2Int.zero;
                if (FindNext(selection, dirFocus, out focus))
                    ScrollTo(focus);
            }

            UpdateList();

            return GetSelectedItem(selection);
        }

        return null;
    }

    /// <summary>
    /// 查找下一个
    /// </summary>
    /// <param name="current"></param>
    /// <param name="dir"></param>
    /// <param name="fined"></param>
    /// <returns></returns>
    private bool FindNext(Vector2Int current, MoveDirection dir,out Vector2Int fined)
    {
        switch(dir)
        {
            case MoveDirection.Left : return FindNextByLeft(current,out fined);
            case MoveDirection.Right: return FindNextByRight(current, out fined);
            case MoveDirection.Up   : return FindNextByUp(current,out fined);
            case MoveDirection.Down : return FindNextByDown(current, out fined);
        }

        fined = current;
        return false;
    }

    /// <summary>
    /// 往右查找索引
    /// </summary>
    /// <param name="selection">开始点</param>
    /// <param name="newIndex">找到的点</param>
    /// <returns>是否找到</returns>
    private bool FindNextByLeft(Vector2Int selection, out Vector2Int newIndex)
    { 
        int tableIndex = selection.x;
        int arrayIndex = selection.y;

        if (tableIndex != -1 && arrayIndex != -1)
        {
            int nextIndex = Mathf.Max(0, arrayIndex - 1);
            if (nextIndex < arrayIndex)
            {
                int oldRow = Mathf.FloorToInt((float)arrayIndex / (float)m_ListColumns);
                int newRow = Mathf.FloorToInt((float)nextIndex / (float)m_ListColumns);
                if (oldRow == newRow)
                {
                    newIndex = new Vector2Int(tableIndex, nextIndex);
                    return true;
                }
            }
        }
        newIndex = selection;
        return false;
    }
    /// <summary>
    /// 往右查找索引
    /// </summary>
    /// <param name="selection">开始点</param>
    /// <param name="newIndex">找到的点</param>
    /// <returns>是否找到</returns>
    private bool FindNextByRight(Vector2Int selection, out Vector2Int newIndex)
    {
        int tableIndex = selection.x;
        int arrayIndex = selection.y;

        if (tableIndex != -1 && arrayIndex != -1)
        {
            int nextIndex = Mathf.Min(m_GroupDatas[tableIndex].ItemDataList.Count - 1, arrayIndex + 1);
            if (nextIndex > arrayIndex)
            {
                int oldRow = Mathf.FloorToInt((float)arrayIndex / (float)m_ListColumns);
                int newRow = Mathf.FloorToInt((float)nextIndex / (float)m_ListColumns);
                if (oldRow == newRow)
                {
                    newIndex = new Vector2Int(tableIndex, nextIndex);
                    return true;
                }
            }
        }
        newIndex = selection;
        return false;
    }
    /// <summary>
    /// 往上查找索引
    /// </summary>
    /// <param name="selection">开始点</param>
    /// <param name="newIndex">找到的点</param>
    /// <returns>是否找到</returns>
    private bool FindNextByUp(Vector2Int selection, out Vector2Int newIndex)
    {
        int tableIndex = selection.x;
        int arrayIndex = selection.y;

        if (tableIndex != -1 && arrayIndex != -1)
        {
            int currRow = arrayIndex / m_ListColumns;

            int nextIndex = Mathf.Max(0, arrayIndex - m_ListColumns);
            int nextRow = nextIndex / m_ListColumns;
            if (nextRow < currRow)
            {
                newIndex = new Vector2Int(tableIndex, nextIndex);
                return true;
            }
            else
            {
                int nextTableIndex = tableIndex - 1;
                while (nextTableIndex >= 0)
                {
                    Data nextData = m_GroupDatas[nextTableIndex];
                    if (nextData.ItemDataList.Count > 0)
                    {
                        int currCol = arrayIndex % m_ListColumns;

                        int count = nextData.ItemDataList.Count;
                        int row = Mathf.CeilToInt((float)count / (float)m_ListColumns);

                        arrayIndex = row * m_ListColumns - (m_ListColumns - currCol);
                        if (arrayIndex > nextData.ItemDataList.Count - 1)
                        {
                            arrayIndex = nextData.ItemDataList.Count - 1;
                        }

                        newIndex = new Vector2Int(nextTableIndex, arrayIndex);
                        return true;
                    }
                    nextTableIndex--;
                }
            }
        }
        newIndex = selection;
        return false;
    }

    /// <summary>
    /// 往下查找索引
    /// </summary>
    /// <param name="selection">开始点</param>
    /// <param name="newIndex">找到的点</param>
    /// <returns></returns>
    private bool FindNextByDown(Vector2Int selection,out Vector2Int newIndex)
    {
        int tableIndex = selection.x;
        int arrayIndex = selection.y;

        if (tableIndex != -1 && arrayIndex != -1)
        {
            Data currData = m_GroupDatas[tableIndex];
            int currRow = arrayIndex / m_ListColumns;

            int nextIndex = Mathf.Min(currData.ItemDataList.Count - 1, arrayIndex + m_ListColumns);
            int nextRow = nextIndex / m_ListColumns;
            if (nextRow > currRow)
            {
                newIndex = new Vector2Int(tableIndex, nextIndex);
                return true;
            }
            else
            {
                arrayIndex = arrayIndex % m_ListColumns;

                int nextTableIndex = tableIndex + 1;
                while (nextTableIndex < m_GroupDatas.Count)
                {
                    Data nextData = m_GroupDatas[nextTableIndex];
                    if (nextData.ItemDataList.Count > 0)
                    {
                        if (arrayIndex >= nextData.ItemDataList.Count)
                        {
                            arrayIndex = nextData.ItemDataList.Count - 1;
                        }

                        newIndex = new Vector2Int(nextTableIndex, arrayIndex);
                        return true;
                    }
                    nextTableIndex++;
                }
            }
        }
        newIndex = selection;
        return false;
    }

    #endregion

    #region 聚焦

    /// <summary>
    /// 滚动到当前选中项
    /// </summary>
    public void ScrollToSelection()
    {
        if (m_IsDirty)
            UpdateList();

        ScrollTo(GetSelection());
        UpdateList();
    }

    /// <summary>
    /// 滚动到指定的索引
    /// </summary>
    /// <param name="index">索引</param>
    private void ScrollTo(Vector2Int index)
    {
        ScrollTo(index, out MoveDirection h, out MoveDirection v);
    }

    /// <summary>
    /// 滚动到指定的索引
    /// </summary>
    /// <param name="index">索引</param>
    private void ScrollTo(Vector2Int index, out MoveDirection h, out MoveDirection v)
    {
        h = MoveDirection.None;
        v = MoveDirection.None;

        int tableIndex = index.x;
        int arrayIndex = index.y;

        bool finded = false;
        int findRow = 0;
        float findTitleHeight = 0;
        Vector2 findPosition = Vector2.zero;

        float currentLayoutX = m_ContentPadding.left;
        float currentLayoutY = m_ContentPadding.top;
        for (int i = 0; i < m_GroupDatas.Count; i++)
        {
            currentLayoutY += i > 0 ? m_ContentSpacing : 0;

            Data data = m_GroupDatas[i];

            Vector2 listSize = GetListSize(data);

            float titleH = data.TitleData != null ? m_HeadHeight : 0;

            currentLayoutY += m_ContentSpacing;

            if (i == tableIndex)
            {
                if (arrayIndex < data.ItemDataList.Count)
                {
                    int row = Mathf.FloorToInt((float)arrayIndex / (float)m_ListColumns);
                    int col = arrayIndex % m_ListColumns;

                    float x = currentLayoutX + m_ListPadding.left + col * (m_CellSize.x + m_CellSpace.x);
                    float y = currentLayoutY + titleH + m_ListPadding.top + row * (m_CellSize.y + m_CellSpace.y);

                    finded = true;
                    findRow = row;
                    findTitleHeight = titleH;
                    findPosition = new Vector2(x, y);
                }
            }
            currentLayoutY += titleH;
            currentLayoutY += listSize.y;
        }

        if (finded)
        {
            ScrollRect scroller = GetComponent<ScrollRect>();

            float viewportWidth = scroller.viewport.rect.width;
            float viewportHeight = scroller.viewport.rect.height;
            float viewportLeft = scroller.content.anchoredPosition.x * -1;
            float viewportRight = viewportLeft + viewportWidth;
            float viewportTop = scroller.content.anchoredPosition.y;
            float viewportBottom = viewportTop + viewportHeight;

            float contentWidth = m_ContentPadding.left + m_ListPadding.left + (m_ListColumns * m_CellSize.x + (m_ListColumns - 1) * m_CellSpace.x) + m_ListPadding.right + m_ContentPadding.right;
            float contentHeight = currentLayoutY + m_ContentPadding.bottom;

            float cellTop = findPosition.y;
            float cellBottom = findPosition.y + m_CellSize.y;
            float cellLeft = findPosition.x;
            float cellRight = cellLeft + m_CellSize.x;

            //如果是第一行，则把标题也加入
            if (findRow == 0)
            {
                cellTop -= findTitleHeight;
            }

            //计算纵向滚动
            bool needScrollY = false;
            float scrollToY = scroller.content.anchoredPosition.y;
            if (cellTop < viewportTop || cellBottom < viewportTop)
            {
                //在视口上面
                needScrollY = true;
                scrollToY = cellTop;
                v = MoveDirection.Up;
            }
            else if (cellTop > viewportBottom || cellBottom > viewportBottom)
            {
                //在视口下面
                needScrollY = true;
                scrollToY = cellBottom - viewportHeight;
                v = MoveDirection.Down;
            }

            //计算横向滚动
            bool needScrollX = false;
            float scrollToX = scroller.content.anchoredPosition.x * -1;
            if (cellLeft < viewportLeft || cellRight < viewportLeft)
            {
                //在视口左边
                needScrollX = true;
                scrollToX = cellLeft;
                h = MoveDirection.Left;
            }
            else if (cellLeft > viewportRight || cellRight > viewportRight)
            {
                //在视口右边
                needScrollX = true;
                scrollToX = cellRight - viewportWidth;
                h = MoveDirection.Right;
            }

            if (needScrollX || needScrollY)
            {
                scroller.StopMovement();
            }

            float horizontalNormalizedPosition = scroller.horizontalNormalizedPosition;
            float verticalNormalizedPosition = scroller.verticalNormalizedPosition;
            if (needScrollX)
            {
                float contentHiddenWidth = contentWidth - viewportWidth;
                if (scrollToX > contentHiddenWidth)
                {
                    scrollToX = contentHiddenWidth;
                }
                //scroller.horizontalNormalizedPosition = scrollToX / contentHiddenWidth;
                horizontalNormalizedPosition = scrollToX / contentHiddenWidth;
            }
            if (needScrollY)
            {
                float contentHiddenHeight = contentHeight - viewportHeight;
                if (scrollToY > contentHiddenHeight)
                {
                    scrollToY = contentHiddenHeight;
                }
                //scroller.verticalNormalizedPosition = 1.0f - scrollToY / contentHiddenHeight;
                verticalNormalizedPosition = 1.0f - scrollToY / contentHiddenHeight;
            }

            if (needScrollX || needScrollY)
            {
                //Debug.LogError(string.Format("  ScrollTo({0:N2} , {1:N2})", horizontalNormalizedPosition, verticalNormalizedPosition));
                scroller.horizontalNormalizedPosition = horizontalNormalizedPosition;
                scroller.verticalNormalizedPosition = verticalNormalizedPosition;

                //ScrollTo(horizontalNormalizedPosition, verticalNormalizedPosition, false);
            }
        }
    }

    private Coroutine m_ScrollAnimCoroutine;
    private bool m_ScrollAnimEnabled = false;

    private void ScrollTo(float horizontal, float vertical, bool anim)
    {
        if (m_ScrollAnimCoroutine != null)
        {
            StopCoroutine(m_ScrollAnimCoroutine);
            m_ScrollAnimCoroutine = null;
        }

        if (m_ScrollAnimEnabled && anim)
        {
            m_ScrollAnimCoroutine = StartCoroutine(ScrollAnimCoroutine(horizontal, vertical));
        }
        else
        {
            //Debug.LogError(string.Format("  ScrollTo({0:N2} , {1:N2})", horizontal, vertical));
            ScrollRect scroller = GetComponent<ScrollRect>();
            scroller.horizontalNormalizedPosition = horizontal;
            scroller.verticalNormalizedPosition = vertical;
        }
    }

    private IEnumerator ScrollAnimCoroutine(float horizontal, float vertical)
    {
        ScrollRect scroller = GetComponent<ScrollRect>();
        while(true)
        {
            float delta = Time.deltaTime*5;

            float h = Mathf.Lerp(scroller.horizontalNormalizedPosition, horizontal, delta);
            float v = Mathf.Lerp(scroller.verticalNormalizedPosition, vertical, delta);

            scroller.horizontalNormalizedPosition = h;
            scroller.verticalNormalizedPosition = v;

            if (Mathf.Abs(horizontal - h)<0.001f && Mathf.Abs(vertical - v)<0.001f)
            {
                scroller.horizontalNormalizedPosition = horizontal;
                scroller.verticalNormalizedPosition = vertical;
                yield break;
            }
            else
            {
                yield return new WaitForEndOfFrame();
            }
        }
    }

    #endregion
}
