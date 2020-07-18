using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIScrollRect : ScrollRect
{
    /// <summary>
    /// 默认模板工厂
    /// </summary>
    private static TempalteFactory DefaultTempateFactory = new TempalteFactory();

    /// <summary>
    /// 是否需要标题
    /// </summary>
    [SerializeField]
    private bool m_NeedHead = true;
    private bool m_NeedHeadPrev;
    /// <summary>
    /// 数据组的内补白
    /// </summary>
    [SerializeField]
    private RectOffset m_ContentPadding = new RectOffset();
    private RectOffset m_ContentPaddingPrev = new RectOffset();
    /// <summary>
    /// 数据组之间的间隔
    /// </summary>
    [SerializeField]
    private float m_ContentSpacing;
    private float m_ContentSpacingPrev;
    /// <summary>
    /// 标题高度
    /// </summary>
    [SerializeField]
    private float m_HeadHeight;
    private float m_HeadHeightPrev;
    /// <summary>
    /// 列表列数
    /// </summary>
    [SerializeField]
    private int m_ListColumns;
    private int m_ListColumnsPrev;
    /// <summary>
    /// 列表的内补白
    /// </summary>
    [SerializeField]
    private RectOffset m_ListPadding = new RectOffset();
    private RectOffset m_ListPaddingPrev = new RectOffset();
    /// <summary>
    /// 格子大小
    /// </summary>
    [SerializeField]
    private Vector2 m_CellSize;
    private Vector2 m_CellSizePrev;
    /// <summary>
    /// 格子间的间隔
    /// </summary>
    [SerializeField]
    private Vector2 m_CellSpace;
    private Vector2 m_CellSpacePrev;

    /// <summary>
    /// 布局已经改变
    /// </summary>
    private bool m_LayoutChanged = false;
    /// <summary>
    /// 动画属性已改变
    /// </summary>
    private bool m_AnimationPropChanged = false;

    /// <summary>
    /// 数据内容
    /// </summary>
    private List<Data> m_Datas = new List<Data>();
    /// <summary>
    /// 数据排序器
    /// </summary>
    private IComparer<object> m_DataSorter;
    /// <summary>
    /// 数据内容是否已改变
    /// </summary>
    private bool m_DataChanged = false;

    /// <summary>
    /// 模板工厂
    /// </summary>
    private TempalteFactory m_TemplateFactory;
    /// <summary>
    /// 模板工厂是否已改变
    /// </summary>
    private bool m_TemplateFactoryChanged = false;

    /// <summary>
    /// 是否需要先调用一下LeateUpdate
    /// </summary>
    private bool m_NeedFirstCallLateUpdate = false;

    /// <summary>
    /// 内容矩形
    /// </summary>
    private Vector4 m_ContentRect = new Vector4();
    /// <summary>
    /// 所有标题对应的矩形列表
    /// </summary>
    private Dictionary<int, Vector4> m_HeadRectList = new Dictionary<int, Vector4>();
    /// <summary>
    /// 所有列表对应的矩形列表
    /// </summary>
    private Dictionary<int, Vector4> m_ListRectList = new Dictionary<int, Vector4>();
    /// <summary>
    /// 所有已实例化的标题
    /// </summary>
    private Dictionary<int, RectTransform> m_Heads = new Dictionary<int, RectTransform>();
    /// <summary>
    /// 所有已实例化的格子
    /// </summary>
    private Dictionary<int, Dictionary<int, RectTransform>> m_Cells = new Dictionary<int, Dictionary<int, RectTransform>>();
    /// <summary>
    /// 所有已实例化的格子占位符
    /// </summary>
    private Dictionary<int, Dictionary<int, RectTransform>> m_CellPlaceholders = new Dictionary<int, Dictionary<int, RectTransform>>();
    /// <summary>
    /// 回收实例时使用的缓存
    /// </summary>
    private HashSet<int> m_Caches = new HashSet<int>();

    /// <summary>
    /// 上次的选中索引
    /// </summary>
    private Vector2Int m_SelectionOld = new Vector2Int(-1, -1);
    /// <summary>
    /// 当前的选中索引
    /// </summary>
    private Vector2Int m_Selection = new Vector2Int(-1, -1);
    /// <summary>
    /// 当前的选中数据
    /// </summary>
    private object m_SelectionData = null;
    /// <summary>
    /// 需要的选中偏移
    /// </summary>
    private Vector2 m_SelectionOffset;
    /// <summary>
    /// 需要的选中偏移是否有效
    /// </summary>
    private int m_SelectionOffsetFlag = 0;
    /// <summary>
    /// 需要的选中索引
    /// </summary>
    private Vector2Int m_SelectionNew = new Vector2Int(-1, -1);
    /// <summary>
    /// 需要的选中索引是否有效
    /// </summary>
    private bool m_SelectionNewFlag = false;
    /// <summary>
    /// 需要的选中数据
    /// </summary>
    private object m_SelectionDataNew = null;
    /// <summary>
    /// 选中项是否已改变
    /// </summary>
    private bool m_SelectionChanged = false;

    /// <summary>
    /// 标题渲染函数
    /// </summary>
    public UnityAction<int, object, RectTransform> OnHeadRenderer;
    /// <summary>
    /// 格子渲染函数
    /// </summary>
    public UnityAction<Vector2Int, object, RectTransform, bool> OnCellRenderer;
    /// <summary>
    /// 格子占位符渲染函数
    /// </summary>
    public UnityAction<Vector2Int, RectTransform, bool> OnCellPlaceholderRenderer;
    /// <summary>
    /// 选择变化
    /// </summary>
    public UnityAction<int, int, object> OnSelectionChanged;
    /// <summary>
    /// 导航回调
    /// </summary>
    public UnityAction<bool> OnNavigateCallback;
    /// <summary>
    /// 点击回调
    /// </summary>
    public UnityAction<bool, int, int> OnClickCallback;
    /// <summary>
    /// 悬浮回调
    /// </summary>
    public UnityAction<bool, int, int> OnOverCallback;
    /// <summary>
    /// 双击回调
    /// </summary>
    public UnityAction<int, int, object> OnDoubleClickCallback;

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
    /// 是否需要标题
    /// </summary>
    public bool NeedHead
    {
        get { return m_NeedHead; }
        set { SetProperty(ref m_NeedHead, value); }
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
        get { return Mathf.Max(1,m_ListColumns); }
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
    /// 设置属性值
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="currentValue"></param>
    /// <param name="newValue"></param>
    protected bool SetProperty<T>(ref T currentValue, T newValue)
    {
        if ((currentValue != null || newValue != null) && (currentValue == null || !currentValue.Equals(newValue)))
        {
            m_LayoutChanged = true;
            currentValue = newValue;
            SetDirty();
            return true;
        }
        return false;
    }

#if UNITY_EDITOR
	protected override void OnValidate()
    {
        m_LayoutChanged = true;
        base.OnValidate();
    }
#endif

	protected override void OnRectTransformDimensionsChange()
    {
        m_LayoutChanged = true;
        base.OnRectTransformDimensionsChange();
    }

    protected override void OnDidApplyAnimationProperties()
    {
        m_AnimationPropChanged = true;

        base.OnDidApplyAnimationProperties();
    }

    #endregion

    #region 数据接口

    /// <summary>
    /// 清空数据
    /// </summary>
    public void ClearData()
    {
        m_Datas.Clear();
        m_DataChanged = true;

        RemoveAllCells();
        SetDirty();
    }

    /// <summary>
    /// 添加数据
    /// </summary>
    /// <param name="headData">标题数据</param>
    /// <param name="itemDatas">列表数据</param>
    public void AddDatas(object headData, object[] itemDatas)
    {
        AddDatas(headData, new List<object>(itemDatas));
    }
    /// <summary>
    /// 添加数据
    /// </summary>
    /// <param name="headData">标题数据</param>
    /// <param name="itemDatas">列表数据</param>
    public void AddDatas(object headData, List<object> itemDatas)
    {
        Data data = new Data();
        data.TitleData = headData;
        data.ItemDataList = itemDatas;
        m_Datas.Add(data);
        m_DataChanged = true;

        RemoveAllCells();
        SetDirty();
    }

    /// <summary>
    /// 数据排序
    /// </summary>
    public IComparer<object> DataSorter
    {
        get
        {
            return m_DataSorter;
        }
        set
        {
            m_DataSorter = value;
            m_DataChanged = true;
            SetDirty();
        }
    }

    /// <summary>
    /// 排序数据
    /// </summary>
    public void SortDatas(bool keepSelection = true)
    {
        m_DataChanged = true;
        SetDirty();

        if (keepSelection && m_SelectionData != null)
            SetSelectionBy(m_SelectionData);
    }

    /// <summary>
    /// 获取数据组数
    /// </summary>
    public int GetDataGroupCount()
    {
        return m_Datas != null ? m_Datas.Count : 0;
    }

    #endregion

    #region 模板接口

    /// <summary>
    /// 模板工厂
    /// </summary>
    public TempalteFactory GetTemplateFactory()
    {
        if (m_TemplateFactory != null)
            return m_TemplateFactory;

        return DefaultTempateFactory;
    }

    /// <summary>
    /// 设置模板工厂
    /// </summary>
    /// <param name="factory"></param>
    public void SetTemplateFactory(TempalteFactory factory)
    {
        if (m_TemplateFactory != factory)
        {
            m_TemplateFactory = factory;
            m_TemplateFactoryChanged = true;
        }
    }

    /// <summary>
    /// 模板工厂基类
    /// </summary>
    public class TempalteFactory
    {
        /// <summary>
        /// 创建标题
        /// </summary>
        /// <returns>标题</returns>
        public virtual RectTransform CreateHead() { return null; }
        /// <summary>
        /// 回收标题
        /// </summary>
        /// <param name="head">标题</param>
        public virtual void RecycleHead(RectTransform head) { }
        /// <summary>
        /// 创建单元格
        /// </summary>
        /// <returns>单元格</returns>
        public virtual RectTransform CreateCell() { return null; }
        /// <summary>
        /// 回收单元格
        /// </summary>
        /// <param name="item">单元格</param>
        public virtual void RecycleCell(RectTransform item) { }
        /// <summary>
        /// 创建单元格占位符
        /// </summary>
        /// <returns>单元格占位符</returns>
        public virtual RectTransform CreateCellPlaceholder() { return null; }
        /// <summary>
        /// 回收单元格占位符
        /// </summary>
        /// <param name="cellPlaceHolder">单元格占位符</param>
        public virtual void RecycleCellPlacehoder(RectTransform cellPlaceHolder) { }
    }

    #endregion

    #region 列表内容

    /// <summary>
    /// OnEnabled
    /// </summary>
    protected override void OnEnable()
    {
        m_NeedFirstCallLateUpdate = true;

        base.OnEnable();

        onValueChanged.RemoveListener(OnValueChanged);
        onValueChanged.AddListener(OnValueChanged);
    }

    /// <summary>
    /// OnDisabled
    /// </summary>
    protected override void OnDisable()
    {
        onValueChanged.RemoveListener(OnValueChanged);

        base.OnDisable();
    }

    /// <summary>
    /// LateUpdate
    /// </summary>
    protected override void LateUpdate()
    {
        if (m_AnimationPropChanged)
        {
            if (m_ContentPaddingPrev != ContentPadding ||
               m_ContentSpacingPrev != ContentSpacing ||
               m_NeedHead != m_NeedHeadPrev ||
               m_HeadHeightPrev != HeadHeight ||
               m_ListColumnsPrev != ColumnCount ||
               m_ListPaddingPrev != CellListPadding ||
               m_CellSizePrev != CellSize ||
               m_CellSpacePrev != CellSpace)
            {
                m_ContentPaddingPrev = ContentPadding;
                m_ContentSpacingPrev = ContentSpacing;
                m_NeedHeadPrev = m_NeedHead;
                m_HeadHeightPrev = HeadHeight;
                m_ListColumnsPrev = ColumnCount;
                m_ListPaddingPrev = CellListPadding;
                m_CellSizePrev = CellSize;
                m_CellSpacePrev = CellSpace;

                m_LayoutChanged = true;

                if (m_SelectionOffsetFlag == 2)
                    m_SelectionOffsetFlag = 1;
            }

            m_AnimationPropChanged = false;
        }

        if (m_DataChanged || m_LayoutChanged || m_TemplateFactoryChanged || m_SelectionChanged)
        {
            if (m_NeedFirstCallLateUpdate)
            {
                base.LateUpdate();
                m_NeedFirstCallLateUpdate = false;
            }

            if (m_TemplateFactoryChanged)
                RemoveAllCells();

            if (m_DataChanged && m_Datas != null && m_DataSorter != null)
            {
                for (int i = 0; i < m_Datas.Count; i++)
                {
                    if (m_Datas[i].ItemDataList != null)
                        m_Datas[i].ItemDataList.Sort(m_DataSorter);
                }
            }

            Vector2Int selectionNext = m_Selection;

            if (m_SelectionNewFlag)
                selectionNext = m_SelectionNew;

            if (m_SelectionDataNew != null)
            {
                for (int i = 0; i < m_Datas.Count; i++)
                {
                    bool finded = false;
                    for (int j = 0; j < m_Datas[i].ItemDataList.Count; j++)
                    {
                        if (m_Datas[i].ItemDataList[j] == m_SelectionDataNew)
                        {
                            selectionNext = new Vector2Int(i, j);
                            finded = true;
                            break;
                        }
                    }
                    if (finded)
                        break;
                }
            }

            Vector2Int clampIndex = ClampSelectionIndex(selectionNext);
            if (m_Selection != clampIndex)
            {
                m_SelectionOld = m_Selection;
                m_Selection = clampIndex;
            }

            MeasureContentSize();
            RelayoutContent(m_LayoutChanged || m_DataChanged);

            m_DataChanged = false;
            m_LayoutChanged = false;
            m_TemplateFactoryChanged = false;
            m_SelectionChanged = false;

            SetFocusTo(m_Selection.x, m_Selection.y);
            CheckSelectionDataChanged();
        }

        base.LateUpdate();

        if (m_SelectionOffsetFlag == 1)
        {
            int groupIndex = m_Selection.x;
            int listIndex = m_Selection.y;

            if (m_ListRectList.ContainsKey(groupIndex))
            {
                int col = listIndex % ColumnCount;
                int row = Mathf.FloorToInt(listIndex / ColumnCount);

                float x = col * (CellSize.x + CellSpace.x);
                float y = row * (CellSize.y + CellSpace.y);

                Vector4 headRect = m_HeadRectList[groupIndex];
                Vector4 listRect = m_ListRectList[groupIndex];

                Vector4 cellRect = new Vector4(listRect.x + x, listRect.y + y, CellSize.x, CellSize.y);
                float viewportX = cellRect.x - m_SelectionOffset.x;
                float viewportY = cellRect.y - m_SelectionOffset.y;
                if (m_SelectionOffset.y == 0 && row == 0)
                    viewportY -= headRect.w + m_ListPadding.top;

                Vector4 viewportRect = new Vector4(viewportX, viewportY, viewport.rect.width, viewport.rect.height);
                Vector4 contentRect = m_ContentRect;

                float scrollRangeX = contentRect.z - viewportRect.z;
                float scrollRangeY = contentRect.w - viewportRect.w;

                verticalNormalizedPosition = 1 - Mathf.Clamp01(viewportRect.y / scrollRangeY);
                horizontalNormalizedPosition = Mathf.Clamp01(viewportRect.x / scrollRangeX);

                if (vertical && verticalScrollbar)
                {
                    verticalScrollbar.size = viewportRect.w / contentRect.w;
                    verticalScrollbar.value = verticalNormalizedPosition;
                }

                if (horizontal && horizontalScrollbar)
                {
                    horizontalScrollbar.size = viewportRect.z / contentRect.z;
                    horizontalScrollbar.value = horizontalNormalizedPosition;
                }
                else
                {
                    horizontalScrollbar.value = 0;
                    horizontalNormalizedPosition = 0;
                }
            }

            m_SelectionOffsetFlag = 2;
        }
    }

    /// <summary>
    /// 面板滚动时
    /// </summary>
    /// <param name="position">滚动量</param>
    private void OnValueChanged(Vector2 position)
    {
        RelayoutContent();
    }

    /// <summary>
    /// 重新布局内容
    /// </summary>
    private void RelayoutContent(bool layoutChanged = false)
    {
        RemoveAllInvisbleCells();
        AddAndLayoutCells(layoutChanged);
    }

    /// <summary>
    /// 刷新当前的所有单元格
    /// </summary>
    public void RefreshCurrentAllCells()
    {
        RelayoutContent(true);
    }

    /// <summary>
    /// 计算内容大小
    /// </summary>
    private void MeasureContentSize()
    {
        m_HeadRectList.Clear();
        m_ListRectList.Clear();

        float w = 0;
        float h = 0;

        Vector2 viewrectSize = viewRect.rect.size;
        if (m_Datas != null)
        {
            //计算最大宽度
            w += ContentPadding.left;
            w += CellListPadding.left;
            w += CellSize.x * ColumnCount + (ColumnCount > 0 ? CellSpace.x * (ColumnCount - 1) : 0);
            w += CellListPadding.right;
            w += ContentPadding.right;
            w = Mathf.Max(w, viewrectSize.x);

            //计算所有标题和列表的矩形
            h += ContentPadding.top;
            for (int i = 0; i < m_Datas.Count; i++)
            {
                Data data = m_Datas[i];

                object head = m_Datas[i] != null ? m_Datas[i].TitleData : null;
                List<object> array = m_Datas[i] != null ? data.ItemDataList : null;

                int rowCount = array != null ? Mathf.CeilToInt((float)array.Count / (float)ColumnCount) : 0;

                //如果列表为空，添加一行占位
                rowCount = Mathf.Max(1, rowCount);

                float headW = w;
                float headH = (NeedHead && head != null) ? HeadHeight : 0;
                float listW = CellSize.x * ColumnCount + (ColumnCount > 0 ? CellSpace.y * (ColumnCount - 1) : 0);
                float listH = CellSize.y * rowCount + (rowCount > 0 ? CellSpace.y * (rowCount - 1) : 0);

                m_HeadRectList.Add(i, new Vector4(ContentPadding.left, h, headW, headH));
                h += headH;

                h += CellListPadding.top;
                m_ListRectList.Add(i, new Vector4(ContentPadding.left + CellListPadding.left, h, listW, listH));
                h += listH;
                h += CellListPadding.bottom;

                if (i < m_Datas.Count - 1)
                    h += ContentSpacing;
            }
            h += ContentPadding.bottom;

            h = Mathf.Max(h, viewrectSize.y);
        }

        if (h <= 0)
            w = 0;

        content.pivot = new Vector2(0, 1);
        content.anchorMin = new Vector2(0, 1);
        content.anchorMax = new Vector2(0, 1);
        content.sizeDelta = new Vector2(w, h);

        m_ContentRect = new Vector4(0, 0, w, h);
    }

    /// <summary>
    /// 删除所有格子
    /// </summary>
    public void RemoveAllCells()
    {
        TempalteFactory factory = GetTemplateFactory();

        //回收所有标题
        foreach (int key in m_Heads.Keys)
        {
            factory.RecycleHead(m_Heads[key]);
        }
        m_Heads.Clear();

        //回收所有单元格
        foreach (int i in m_Cells.Keys)
        {
            foreach (int j in m_Cells[i].Keys)
            {
                RectTransform cell = m_Cells[i][j];

                NavigateItem focus = cell.GetComponent<NavigateItem>();
                if (focus)
                    focus.enabled = false;

                factory.RecycleCell(cell);
            }
            m_Cells[i].Clear();
        }
        //回收所有单元格占位符
        foreach (int i in m_CellPlaceholders.Keys)
        {
            foreach (int j in m_CellPlaceholders[i].Keys)
            {
                RectTransform cell = m_CellPlaceholders[i][j];

                NavigateItem focus = cell.GetComponent<NavigateItem>();
                if (focus)
                    focus.enabled = false;

                factory.RecycleCellPlacehoder(cell);
            }
            m_CellPlaceholders[i].Clear();
        }

        //删除格子时，如果指针是否在列表内，那么强制处理下事件
        //否则，被清掉的格子被重用时，选中状态会不正常。
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas && canvas.worldCamera)
        {
            Vector2 point = Vector2.zero;
            Vector2 poniterPosition = InputManager.Instance.GetCurrentVirtualCursorPos();
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(viewport, poniterPosition, canvas.worldCamera, out point))
            {
                if (viewport.rect.Contains(point))
                {
                    EventSystem.current.currentInputModule.Process();
                }
            }
        }
    }

    /// <summary>
    /// 删除所有不可见格子
    /// </summary>
    private void RemoveAllInvisbleCells()
    {
        TempalteFactory factory = GetTemplateFactory();

        Vector4 viewportRect = new Vector4(-content.anchoredPosition.x, content.anchoredPosition.y, viewport.rect.width, viewport.rect.height);

        //回收无效标题
        m_Caches.Clear();
        foreach (int key in m_Heads.Keys)
        {
            if (!NeedHead || !Overlaps(m_HeadRectList[key], viewportRect))
                m_Caches.Add(key);
        }
        foreach (int key in m_Caches)
        {
            if (m_Heads.ContainsKey(key))
                factory.RecycleHead(m_Heads[key]);

            m_Heads.Remove(key);
        }
        m_Caches.Clear();

        //回收无效单元格
        foreach (int i in m_Cells.Keys)
        {
            if (!m_ListRectList.ContainsKey(i)) continue;

            Vector4 listRect = m_ListRectList[i];
            bool listOverlaps = Overlaps(listRect, viewportRect);

            m_Caches.Clear();
            foreach (int j in m_Cells[i].Keys)
            {
                if (m_Selection.x == i && m_Selection.y == j)
                    continue;

                if (listOverlaps)
                {
                    int col = j % ColumnCount;
                    int row = Mathf.FloorToInt(j / ColumnCount);

                    float x = col * (CellSize.x + CellSpace.x);
                    float y = row * (CellSize.y + CellSpace.y);

                    Vector4 cellRect = new Vector4(listRect.x + x, listRect.y + y, CellSize.x, CellSize.y);

                    if (!Overlaps(cellRect, viewportRect))
                        m_Caches.Add(j);
                }
                else
                {
                    m_Caches.Add(j);
                }
            }

            foreach (int key in m_Caches)
            {
                if (m_Cells[i].ContainsKey(key))
                {
                    RectTransform cell = m_Cells[i][key];

                    NavigateItem focus = cell.GetComponent<NavigateItem>();
                    if (focus)
                        focus.enabled = false;

                    factory.RecycleCell(cell);
                }

                m_Cells[i].Remove(key);
            }
            m_Caches.Clear();
        }
        //回收无效单元格占位符
        foreach (int i in m_CellPlaceholders.Keys)
        {
            if (!m_ListRectList.ContainsKey(i)) continue;

            Vector4 listRect = m_ListRectList[i];
            bool listOverlaps = Overlaps(listRect, viewportRect);

            int dataCount = GetGroupArrayLength(i);

            m_Caches.Clear();
            foreach (int j in m_CellPlaceholders[i].Keys)
            {
                if (m_Selection.x == i && m_Selection.y == j)
                    continue;

                if (j < dataCount && listOverlaps)
                {
                    int col = j % ColumnCount;
                    int row = Mathf.FloorToInt(j / ColumnCount);

                    float x = col * (CellSize.x + CellSpace.x);
                    float y = row * (CellSize.y + CellSpace.y);

                    Vector4 cellRect = new Vector4(listRect.x + x, listRect.y + y, CellSize.x, CellSize.y);

                    if (!Overlaps(cellRect, viewportRect))
                        m_Caches.Add(j);
                }
                else
                {
                    m_Caches.Add(j);
                }
            }
            foreach (int key in m_Caches)
            {
                if (m_CellPlaceholders[i].ContainsKey(key))
                {
                    RectTransform cell = m_CellPlaceholders[i][key];

                    NavigateItem focus = cell.GetComponent<NavigateItem>();
                    if (focus)
                        focus.enabled = false;

                    factory.RecycleCellPlacehoder(cell);
                }

                m_CellPlaceholders[i].Remove(key);
            }
            m_Caches.Clear();
        }
    }

    /// <summary>
    /// 添加并布局格子
    /// </summary>
    /// <param name="layoutChanged">布局是否失效</param>
    private void AddAndLayoutCells(bool layoutChanged)
    {
        Vector2 selfSize = GetComponent<RectTransform>().rect.size;
        Vector2 viewportSize = viewport.rect.size;
        Vector4 viewportRect = new Vector4(-content.anchoredPosition.x, content.anchoredPosition.y, viewport.rect.width, viewport.rect.height);
        
        int index = 0;
        bool isSelectionChanged = m_Selection.x != m_SelectionOld.x || m_Selection.y != m_SelectionOld.y;

        if (m_Datas != null)
        {
            TempalteFactory factory = GetTemplateFactory();

            for (int i = 0; i < m_Datas.Count; i++)
            {
                Data groupData = m_Datas[i];

                //标题
                Vector4 headRect = m_HeadRectList[i];
                if (NeedHead && groupData.TitleData != null && Overlaps(headRect, viewportRect))
                {
                    bool isNew = false;
                    RectTransform head = m_Heads.ContainsKey(i) ? m_Heads[i] : null;
                    if (!head && factory != null)
                    {
                        head = factory.CreateHead();
                        if (head)
                        {
                            head.SetParent(content);
                            head.localScale = Vector3.one;
                            head.localEulerAngles = Vector3.zero;
                            head.localPosition = Vector3.zero;
                            head.anchorMin = Vector2.up;
                            head.anchorMax = Vector2.up;
                            head.pivot = Vector2.up;

                            isNew = true;
                        }
                    }

                    m_Heads[i] = head;

                    if (head)
                    {
                        head.SetSiblingIndex(index);
                        head.anchoredPosition = new Vector2(headRect.x, -headRect.y);
                        head.sizeDelta = new Vector2(headRect.z, headRect.w);
                        index++;

                        object title = groupData != null ? groupData.TitleData : null;

                        if (isNew)
                            OnHeadRenderer?.Invoke(i, title, head);
                        else if (layoutChanged)
                            OnHeadRenderer?.Invoke(i, title, head);
                    }
                }

                //列表
                Vector4 listRect = m_ListRectList[i];
                bool listOverlaps = Overlaps(listRect, viewportRect);
                if (listOverlaps || m_Selection.x == i)
                {
                    List<object> cellDatas = groupData != null ? groupData.ItemDataList : null;

                    if (!m_Cells.ContainsKey(i))
                        m_Cells.Add(i, new Dictionary<int, RectTransform>());
                    if (!m_CellPlaceholders.ContainsKey(i))
                        m_CellPlaceholders.Add(i, new Dictionary<int, RectTransform>());

                    int dataCount = GetGroupArrayLength(i);
                    for (int j = 0; j < dataCount; j++)
                    {
                        //对于和视口不重叠的组，只渲染当前选中的单元格。
                        if (!listOverlaps && (m_Selection.x != i || m_Selection.y != j))
                            continue;

                        //计算单元格位置
                        int col = j % ColumnCount;
                        int row = Mathf.FloorToInt(j / ColumnCount);

                        float cellX = listRect.x + col * (CellSize.x + CellSpace.x);
                        float cellY = listRect.y + row * (CellSize.y + CellSpace.y);

                        Vector4 cellRect = new Vector4(cellX, cellY, m_CellSize.x, m_CellSize.y);

                        //忽略的所有和视口不重叠的单元格（当前选中的单元格除外）。
                        if (!Overlaps(cellRect, viewportRect) && (m_Selection.x != i || m_Selection.y != j))
                            continue;

                        bool isNew = false;
                        bool isCellPlaceholder = cellDatas != null && j < cellDatas.Count ? false : true;

                        RectTransform cell = null;
                        if (isCellPlaceholder)
                            cell = m_CellPlaceholders[i].ContainsKey(j) ? m_CellPlaceholders[i][j] : null;
                        else
                            cell = m_Cells[i].ContainsKey(j) ? m_Cells[i][j] : null;

                        if (!cell && factory != null)
                        {
                            cell = isCellPlaceholder ? factory.CreateCellPlaceholder() : factory.CreateCell();
                            if (cell)
                            {
                                cell.SetParent(content);
                                cell.localScale = Vector3.one;
                                cell.localEulerAngles = Vector3.zero;
                                cell.localPosition = Vector3.zero;
                                cell.anchorMin = Vector2.up;
                                cell.anchorMax = Vector2.up;
                                cell.pivot = Vector2.up;

                                isNew = true;
                            }
                        }

                        if (cell)
                        {
                            if (isCellPlaceholder)
                                m_CellPlaceholders[i][j] = cell;
                            else
                                m_Cells[i][j] = cell;

                            cell.SetSiblingIndex(index);
                            cell.anchoredPosition = new Vector2(cellRect.x, -cellRect.y);
                            cell.sizeDelta = new Vector2(cellRect.z, cellRect.w);
                            index++;

                            NavigateItem focus = cell.GetComponent<NavigateItem>() ?? cell.gameObject.AddComponent<NavigateItem>();
                            focus.enabled = true;
                            focus.Viewer = this;
                            focus.groupIndex = i;
                            focus.listIndex = j;

                            bool isCurrSelection = m_Selection.x == i && m_Selection.y == j;
                            bool needRenderer = isNew || layoutChanged;
                            if (!needRenderer && isSelectionChanged)
                            {
                                bool isPrevSelection = m_SelectionOld.x == i && m_SelectionOld.y == j;
                                if (isPrevSelection || isCurrSelection)
                                    needRenderer = true;
                            }

                            if (needRenderer)
                            {
                                if (isCellPlaceholder)
                                    OnCellPlaceholderRenderer?.Invoke(new Vector2Int(i, j), cell, isCurrSelection);
                                else
                                    OnCellRenderer?.Invoke(new Vector2Int(i, j), cellDatas[j], cell, isCurrSelection);
                            }
                        }
                    }
                }
            }
        }

        m_SelectionOld = m_Selection;
    }

    /// <summary>
    /// 相交检测
    /// </summary>
    /// <param name="rectA">矩形A</param>
    /// <param name="rectB">矩形B</param>
    /// <returns>是否相交</returns>
    private bool Overlaps(Vector4 rectA, Vector4 rectB)
    {
        float halfW1 = rectA.z / 2;
        float halfH1 = rectA.w / 2;
        float centerX1 = rectA.x + halfW1;
        float centerY1 = rectA.y + halfH1;

        float halfW2 = rectB.z / 2;
        float halfH2 = rectB.w / 2;
        float centerX2 = rectB.x + halfW2;
        float centerY2 = rectB.y + halfH2;

        return Mathf.Abs(centerX2 - centerX1) < (halfW2 + halfW1) && Mathf.Abs(centerY2 - centerY1) < (halfH1 + halfH2);
    }


    #endregion


    #region 焦点导航

    /// <summary>
    /// 导航项目
    /// </summary>
    private class NavigateItem : UIPanelFocus.Focusable,IPointerDownHandler,IPointerUpHandler,IPointerEnterHandler,IPointerExitHandler
    {
        [HideInInspector]
        public UIScrollRect Viewer;
        [HideInInspector]
        public int groupIndex;
        [HideInInspector]
        public int listIndex;
        /// <summary>
        /// 最事一次点击时间
        /// </summary>
        private float m_LastClickTime;

        /// <summary>
        /// 导航时
        /// </summary>
        /// <param name="dir">方向</param>
        /// <returns>新焦点</returns>
        public override GameObject OnMove(MoveDirection dir)
        {
            GameObject next = Viewer.FindNext(new Vector2Int(groupIndex, listIndex), dir);
            Viewer.OnNavigateCallback?.Invoke(next != null);
            return next;
        }
        /// <summary>
        /// 鼠标按下时
        /// </summary>
        /// <param name="eventData">事件数据</param>
        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                if (isActiveAndEnabled)
                {
                    Viewer.OnClickCallback?.Invoke(true, groupIndex, listIndex);
                }
            }
        }
        /// <summary>
        /// 鼠标松开时
        /// </summary>
        /// <param name="eventData"></param>
        public void OnPointerUp(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                if (isActiveAndEnabled)
                {
                    Viewer.OnClickCallback?.Invoke(false, groupIndex, listIndex);
                }
            }

            if (Time.time - m_LastClickTime < 0.3f)
            {
                m_LastClickTime = 0;

                Viewer.OnDoubleClickCallback?.Invoke(groupIndex, listIndex, Viewer.m_SelectionData);
            }
            else
            {
                m_LastClickTime = Time.time;
            }
        }
        /// <summary>
        /// 鼠标进入时
        /// </summary>
        /// <param name="eventData"></param>
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (isActiveAndEnabled)
            {
                Viewer.OnOverCallback?.Invoke(true, groupIndex, listIndex);
            }
        }
        /// <summary>
        /// 鼠标退出时
        /// </summary>
        /// <param name="eventData"></param>
        public void OnPointerExit(PointerEventData eventData)
        {
            if (isActiveAndEnabled)
            {
                Viewer.OnOverCallback?.Invoke(false, groupIndex, listIndex);
            }
        }
    }

    /// <summary>
    /// 查找下一个
    /// </summary>
    /// <param name="current">当前格子</param>
    /// <param name="dir">查找方向</param>
    /// <returns>查找结果</returns>
    private GameObject FindNext(Vector2Int current, MoveDirection dir)
    {
        Vector2Int next;
        if(FindNext(current, dir,out next))
        {
            SetSelection(next.x, next.y, true);

            int listCount = 0;
            if (m_Datas != null && next.x >= 0 && next.x < m_Datas.Count && m_Datas[next.x] != null && m_Datas[next.x].ItemDataList != null)
                listCount = m_Datas[next.x].ItemDataList.Count;

            if (next.y < listCount)
            {
                if (m_Cells.ContainsKey(next.x) && m_Cells[next.x].ContainsKey(next.y))
                    return m_Cells[next.x][next.y].gameObject;
            }
            else
            {
                if (m_CellPlaceholders.ContainsKey(next.x) && m_CellPlaceholders[next.x].ContainsKey(next.y))
                    return m_CellPlaceholders[next.x][next.y].gameObject;
            }
        }

        return null;
    }
    /// <summary>
    /// 查找下一个
    /// </summary>
    /// <param name="current">当前格子</param>
    /// <param name="dir">查找方向</param>
    /// <param name="fined">查找结果</param>
    /// <returns>是否找到</returns>
    private bool FindNext(Vector2Int current, MoveDirection dir, out Vector2Int fined)
    {
        switch (dir)
        {
            case MoveDirection.Left: return FindNextByLeft(current, out fined);
            case MoveDirection.Right: return FindNextByRight(current, out fined);
            case MoveDirection.Up: return FindNextByUp(current, out fined);
            case MoveDirection.Down: return FindNextByDown(current, out fined);
        }

        fined = current;
        return false;
    }
    /// <summary>
    /// 往左查找索引
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
            int currCol = arrayIndex % ColumnCount;
            if (currCol > 0)
            {
                newIndex = new Vector2Int(tableIndex, arrayIndex - 1);
                return true;
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
            int currCol = arrayIndex % ColumnCount;
            if (currCol < ColumnCount - 1)
            {
                newIndex = new Vector2Int(tableIndex, arrayIndex + 1);
                return true;
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
            int currRow = arrayIndex / ColumnCount;
            int currCol = arrayIndex % ColumnCount;
            if (currRow > 0)
            {
                newIndex = new Vector2Int(tableIndex, arrayIndex - ColumnCount);
                return true;
            }
            else if(tableIndex>0)
            {
                tableIndex--;
                newIndex = new Vector2Int(tableIndex, GetGroupArrayLength(tableIndex) - ColumnCount + currCol);
                return true;
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
    /// <returns>是否找到</returns>
    private bool FindNextByDown(Vector2Int selection, out Vector2Int newIndex)
    {
        int tableIndex = selection.x;
        int arrayIndex = selection.y;

        if (tableIndex != -1 && arrayIndex != -1)
        {
            int arrayCount = GetGroupArrayLength(tableIndex);
            if (arrayIndex + ColumnCount < arrayCount)
            {
                newIndex = new Vector2Int(tableIndex, arrayIndex + ColumnCount);
                return true;
            }
            else if (tableIndex < m_Datas.Count - 1)
            {
                newIndex = new Vector2Int(tableIndex + 1, arrayIndex % ColumnCount);
                return true;
            }
        }
        newIndex = selection;
        return false;
    }

    #endregion

    #region 选择滚动

    /// <summary>
    /// 获取当前选择
    /// </summary>
    /// <returns>索引</returns>
    public Vector2Int GetSelection()
    {
        return m_Selection;
    }

    /// <summary>
    /// 设置当前选择
    /// </summary>
    /// <param name="index">索引</param>
    public void SetSelection(Vector2Int index)
    {
        //SetSelection(index.x, index.y, true);
        m_SelectionNew = index;
        m_SelectionNewFlag = true;
        m_SelectionDataNew = null;
        m_SelectionChanged = true;

        StopMovement();
        SetDirty();
    }
    
    /// <summary>
    /// 根据数据设置当前选择
    /// </summary>
    /// <param name="data"></param>
    public void SetSelectionBy(object data)
    {
        m_SelectionNew = new Vector2Int(-1, -1);
        m_SelectionNewFlag = false;
        m_SelectionDataNew = data;
        m_SelectionChanged = true;

        StopMovement();
        SetDirty();
    }

    /// <summary>
    /// 获取当前选择相对于视口的偏移量
    /// </summary>
    /// <returns>偏移量</returns>
    public Vector2 GetSelectionOffset()
    {
        int groupIndex = m_Selection.x;
        int listIndex = m_Selection.y;

        if (m_ListRectList.ContainsKey(groupIndex))
        {
            int col = listIndex % ColumnCount;
            int row = Mathf.FloorToInt(listIndex / ColumnCount);

            float x = col * (CellSize.x + CellSpace.x);
            float y = row * (CellSize.y + CellSpace.y);

            Vector4 listRect = m_ListRectList[groupIndex];

            Vector4 cellRect = new Vector4(listRect.x + x, listRect.y + y, CellSize.x, CellSize.y);
            Vector4 viewportRect = new Vector4(-content.anchoredPosition.x, content.anchoredPosition.y, viewport.rect.width, viewport.rect.height);

            float offsetX = cellRect.x - viewportRect.x;
            float offsetY = cellRect.y - viewportRect.y;

            return new Vector2(offsetX, offsetY);
        }

        return Vector2.zero;
    }

    /// <summary>
    /// 设置选择的偏移量
    /// </summary>
    /// <param name="offset"></param>
    public void SetSelectionOffset(Vector2 offset)
    {
        m_SelectionOffset = offset;
        m_SelectionOffsetFlag = 1;

        StopMovement();
        SetDirty();
    }

    /// <summary>
    /// 设置当前选择
    /// </summary>
    /// <param name="groupIndex">组索引</param>
    /// <param name="listIndex">列表索引</param>
    /// <param name="focus">是否需要聚焦</param>
    private void SetSelection(int groupIndex, int listIndex, bool focus)
    {
        m_SelectionNew = new Vector2Int(-1, -1);
        m_SelectionNewFlag = false;
        m_SelectionDataNew = null;
        m_SelectionOffsetFlag = 0;

        m_SelectionOld = m_Selection;
        m_Selection = ClampSelectionIndex(new Vector2Int(groupIndex, listIndex));

        ScrollTo(groupIndex, listIndex, focus);

        RelayoutContent(false);

        SetFocusTo(groupIndex, listIndex);

        CheckSelectionDataChanged();
    }

    /// <summary>
    /// 获取指定组的数据长度
    /// </summary>
    /// <param name="index">组索引</param>
    /// <returns>长度</returns>
    private int GetGroupArrayLength(int index)
    {
        if (m_Datas != null && index < m_Datas.Count)
        {
            if (m_Datas[index] == null || m_Datas[index].ItemDataList == null || m_Datas[index].ItemDataList.Count <= 0)
                return ColumnCount;
            else
                return Mathf.CeilToInt((float)m_Datas[index].ItemDataList.Count / (float)ColumnCount) * ColumnCount;
        }
        return 0;
    }
    
    /// <summary>
    /// 移动焦点到指定单元格
    /// </summary>
    /// <param name="groupIndex">组索引</param>
    /// <param name="listIndex">列表索引</param>
    private void SetFocusTo(int groupIndex,int listIndex)
    {
        int listCount = 0;
        if (m_Datas != null && groupIndex >= 0 && groupIndex < m_Datas.Count && m_Datas[groupIndex] != null && m_Datas[groupIndex].ItemDataList != null)
            listCount = m_Datas[groupIndex].ItemDataList.Count;

        if (listIndex < listCount)
        {
            if (m_Cells.ContainsKey(groupIndex) && m_Cells[groupIndex].ContainsKey(listIndex))
            {
                RectTransform cell = m_Cells[groupIndex][listIndex];

                if (cell && UIPanelFocus.Current!=null)
                    UIPanelFocus.Current.SetSelection(cell.gameObject);
            }
        }
        else
        {
            if (m_CellPlaceholders.ContainsKey(groupIndex) && m_CellPlaceholders[groupIndex].ContainsKey(listIndex))
            {
                RectTransform cell = m_CellPlaceholders[groupIndex][listIndex];
                if (cell && UIPanelFocus.Current != null)
                    UIPanelFocus.Current.SetSelection(cell.gameObject);
            }
        }
    }

    /// <summary>
    /// 限制选择索引
    /// </summary>
    /// <param name="selection">选择索引</param>
    /// <returns>检查后的选择索引</returns>
    private Vector2Int ClampSelectionIndex(Vector2Int selection)
    {
        int groupIndex = selection.x;
        int listIndex = selection.y;

        if (m_Datas == null || m_Datas.Count <= 0 || groupIndex < 0)
        {
            groupIndex = -1;
            listIndex = -1;
        }
        else
        {
            groupIndex = Mathf.Min(groupIndex, m_Datas.Count - 1);
            listIndex = Mathf.Clamp(listIndex, 0, GetGroupArrayLength(groupIndex) - 1);
        }

        return new Vector2Int(groupIndex, listIndex);
    }

    /// <summary>
    /// 检查择中数据的变化
    /// </summary>
    private void CheckSelectionDataChanged()
    {
        int groupIndex = m_Selection.x;
        int listIndex = m_Selection.y;

        //选中数据改变了
        object data = null;
        if (groupIndex >= 0 && listIndex >= 0 && groupIndex < m_Datas.Count &&
            m_Datas[groupIndex] != null && m_Datas[groupIndex].ItemDataList != null &&
            listIndex < m_Datas[groupIndex].ItemDataList.Count
            )
        {
            data = m_Datas[groupIndex].ItemDataList[listIndex];
        }

        if (m_SelectionData != data || (m_SelectionOld.x != m_Selection.x || m_SelectionOld.y != m_Selection.y))
        {
            m_SelectionData = data;
            OnSelectionChanged?.Invoke(groupIndex, listIndex, data);
        }
    }

    /// <summary>
    /// 滚动到指定索引
    /// </summary>
    /// <param name="groupIndex">组索引</param>
    /// <param name="listIndex">列表索引</param>
    private void ScrollTo(int groupIndex, int listIndex, bool keepOneCellOffset)
    {
        if (!m_ListRectList.ContainsKey(groupIndex)) return;

        int col = listIndex % ColumnCount;
        int row = Mathf.FloorToInt(listIndex / ColumnCount);

        Vector4 listRect = m_ListRectList[groupIndex];

        float x = col * (CellSize.x + CellSpace.x);
        float y = row * (CellSize.y + CellSpace.y);

        Vector4 cellRect = new Vector4(listRect.x + x, listRect.y + y, CellSize.x, CellSize.y);
        Vector4 viewportRect = new Vector4(-content.anchoredPosition.x, content.anchoredPosition.y, viewport.rect.width, viewport.rect.height);
        Vector4 contentRect = m_ContentRect;

        bool scrollChanged = false;
        float scrollRangeX = contentRect.z - viewportRect.z;
        float scrollRangeY = contentRect.w - viewportRect.w;

        float cellL = cellRect.x;
        float cellR = cellRect.x + cellRect.z;
        float cellT = cellRect.y;
        float cellB = cellRect.y + cellRect.w;

        float edgeMulti = keepOneCellOffset ? 1 : 0;

        if (cellL <= viewportRect.x + (CellSize.x * edgeMulti))
        {
            //在视口左方
            if (keepOneCellOffset)
            {
                if (col > 0)
                    cellL -= CellSpace.x + CellSize.x;
            }
            else
            {
                if (col > 0)
                    cellL -= CellSpace.x;
            }

            horizontalNormalizedPosition = horizontal ? Mathf.Clamp01(cellL / scrollRangeX) : 0;
            scrollChanged = true;
        }
        else if (cellR >= viewportRect.x + viewportRect.z - (CellSize.x * edgeMulti))
        {
            //在视口右方
            if (keepOneCellOffset)
            {
                if (col < ColumnCount - 1)
                    cellR += CellSpace.x + CellSize.x;
            }
            else
            {
                if (col < ColumnCount - 1)
                    cellR += CellSpace.x;
            }

            horizontalNormalizedPosition = horizontal ? Mathf.Clamp01((cellR - viewportRect.z) / scrollRangeX) : 0;
            scrollChanged = true;
        }
        if (cellT <= viewportRect.y + (CellSize.y * edgeMulti))
        {
            //在视口上方
            if (keepOneCellOffset)
            {
                if (row > 0)
                {
                    cellT -= CellSpace.y + CellSize.y;
                }
                else
                {
                    cellT -= CellListPadding.top;
                    cellT -= m_HeadRectList.ContainsKey(groupIndex) ? m_HeadRectList[groupIndex].w : 0;
                    cellT -= CellListPadding.bottom;

                    if (groupIndex > 0)
                    {
                        cellT -= ContentSpacing;
                        cellT -= CellSize.y;
                    }
                }
            }
            else
            {
                if (row > 0)
                {
                    cellT -= CellSpace.y;
                }
                else
                {
                    float headHeight = m_HeadRectList.ContainsKey(groupIndex) ? m_HeadRectList[groupIndex].w : 0;

                    cellT -= CellListPadding.top;
                    cellT -= headHeight > 0 ? headHeight : ContentSpacing;
                }
            }

            verticalNormalizedPosition = 1 - Mathf.Clamp01(cellT / scrollRangeY);
            scrollChanged = true;
        }
        else if (cellB >= viewportRect.y + viewportRect.w - (CellSize.y * edgeMulti))
        {
            //在视口下方
            if (keepOneCellOffset)
            {
                if (cellB < listRect.y + listRect.w)
                {
                    cellB += CellSize.y + CellSpace.y;
                }
                else
                {
                    cellB += CellListPadding.bottom;
                    cellB += ContentSpacing;
                    cellB += m_HeadRectList.ContainsKey(groupIndex + 1) ? m_HeadRectList[groupIndex + 1].w : 0;
                    cellB += CellListPadding.top;
                    cellB += CellSize.y;
                }
            }
            else
            {
                if (cellB < listRect.y + listRect.w)
                {
                    cellB += CellSpace.y;
                }
                else
                {
                    cellB += CellListPadding.bottom > 0 ? CellListPadding.bottom : ContentSpacing;
                }
            }

            verticalNormalizedPosition = 1 - Mathf.Clamp01((cellB - viewportRect.w) / scrollRangeY);
            scrollChanged = true;
        }

        if (scrollChanged)
            StopMovement();
    }

    #endregion
}
