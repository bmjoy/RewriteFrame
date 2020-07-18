using UnityEngine;

public class UIViewPage
{
    /// <summary>
    /// 名字
    /// </summary>
    public string Name;
    /// <summary>
    /// 图标的ID
    /// </summary>
    public uint Icon;
    /// <summary>
    /// 标签
    /// </summary>
    public string Label = "";

    /// <summary>
    /// 是否可见
    /// </summary>
    public bool Visible = true;

    /// <summary>
    /// 布局模式
    /// </summary>
    public UIViewListLayout ListLayoutMode = UIViewListLayout.Row;
    /// <summary>
    /// 布局模式是否允许改变
    /// </summary>
    public bool ListLayoutAllowChange = false;

    /// <summary>
    /// 排序列表
    /// </summary>
    public UIViewSortItem[] Sorters;
    /// <summary>
    /// 排序索引
    /// </summary>
    public int SortIndex = 0;

    /// <summary>
    /// 分类列表
    /// </summary>
    public UIViewCategory[] Categorys;
    /// <summary>
    /// 分类索引
    /// </summary>
    public int CategoryIndex = 0;
    /// <summary>
    /// 分类模式
    /// </summary>
    public bool CategoryIsFilterMode;

    private Vector2Int m_selection = new Vector2Int(0, 0);
    /// <summary>
    /// 列表选中项
    /// </summary>
    public Vector2Int ListSelection
    {
        get { return m_selection; }
        set { m_selection = value; }
    }
    /// <summary>
    /// 列表选中项偏移
    /// </summary>
    public Vector2 ListSelectionOffset = new Vector2(0, 0);
}
