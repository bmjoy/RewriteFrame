using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

class GridScrollRect<T>
{
    private int cellColCount = 9;
    private int cellW = 96;
    private int cellH = 96;
    private int cellSpaceW = 22;
    private int cellSpaceH = 22;
    private int cellPadding = 10;

    private ScrollRect scroller;
    private RectTransform itemTemplate;
    private UnityAction preUpdateList;
    private UnityAction<int, RectTransform, T, bool> cellUpdateCallback;
    private UnityAction<RectTransform> cellRecycleCallback;
    private UnityAction postUpdateList;
    private UnityAction<Vector3> posChange;
    private Dictionary<int, RectTransform> position2cell = new Dictionary<int, RectTransform>();
    private Dictionary<int, T> position2data = new Dictionary<int, T>();

    private T[] datas;

    public GridScrollRect(ScrollRect scrollrect, RectTransform itemTemplate, UnityAction<int, RectTransform, T, bool> cellUpdateCallback, UnityAction<RectTransform> cellRecycleCallback = null, UnityAction preUpdateList = null, UnityAction postUpdateList = null,UnityAction<Vector3> posChange = null)
    {
        this.scroller = scrollrect;
        this.itemTemplate = itemTemplate;
        this.cellUpdateCallback = cellUpdateCallback;
        this.cellRecycleCallback = cellRecycleCallback;
        this.preUpdateList = preUpdateList;
        this.postUpdateList = postUpdateList;
        this.posChange = posChange;
        this.scroller.verticalScrollbar.onValueChanged.AddListener(OnScrollChanged);
    }

    public void Release()
    {
        scroller.verticalScrollbar.onValueChanged.RemoveListener(OnScrollChanged);

        foreach(var item in position2cell.Keys)
        {
            cellRecycleCallback?.Invoke(position2cell[item]);
        }
        itemTemplate.RecycleAll();

        scroller = null;
        itemTemplate = null;

        cellUpdateCallback = null;
        cellRecycleCallback = null;
        preUpdateList = null;
        postUpdateList = null;

        position2cell.Clear();
        position2cell = null;

        position2data.Clear();
        position2data = null;

        datas = null;
    }

    public void SetLayout(int colCount, int cellW, int cellH, int spaceW = 0, int spaceH = 0, int padding = 0)
    {
        this.cellColCount = colCount;
        this.cellW = cellW;
        this.cellH = cellH;
        this.cellSpaceW = spaceW;
        this.cellSpaceH = spaceH;
        this.cellPadding = padding;
    }

    public void SetData(T[] datas)
    {
        this.datas = datas;
        UpdatePackList(false);
    }

    private void OnScrollChanged(float value)
    {
        UpdatePackList(false);      
    }

    /// <summary>
    /// 更新背包列表
    /// </summary>
    private void UpdatePackList(bool force)
    {
        if (scroller == null || datas == null) { return; }

        preUpdateList?.Invoke();

        scroller.content.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, cellColCount * (cellW + cellSpaceW) + cellPadding);
        scroller.content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Mathf.CeilToInt((float)datas.Length / (float)cellColCount) * (cellH + cellSpaceH) + cellPadding);

        var contentY = scroller.content.anchoredPosition.y;

        var layoutCellW = cellW + cellSpaceW;
        var layoutCellH = cellH + cellSpaceH;
        var layoutCellX = cellPadding;
        var layoutCellY = cellPadding + Mathf.Max(Mathf.FloorToInt((contentY - cellPadding) / layoutCellH), 0) * layoutCellH;

        var layoutDataI = Mathf.FloorToInt((layoutCellY - cellPadding) / layoutCellH) * cellColCount;
        var layoutRowCount = Mathf.CeilToInt((scroller.viewport.rect.height - cellPadding) / layoutCellH);

        var dataIndexBegin = Mathf.FloorToInt((layoutCellY - cellPadding) / layoutCellH) * cellColCount;
        var dataIndexEnd = dataIndexBegin + Mathf.CeilToInt((contentY - layoutCellY + scroller.viewport.rect.height) / layoutCellH) * cellColCount;
        if (dataIndexEnd > datas.Length)
        {
            dataIndexEnd = datas.Length;
        }

        //回收
        foreach (var position in position2cell.Keys.ToArray())
        {
            if (position < dataIndexBegin || position >= dataIndexEnd)
            {
                if (position2cell.ContainsKey(position))
                {
                    cellRecycleCallback?.Invoke(position2cell[position]);
                    position2cell[position].gameObject.Recycle();

                    position2cell.Remove(position);
                }
                if (position2data.ContainsKey(position))
                {
                    position2data.Remove(position);
                }
            }
        }

        //
        int index = 0;
        int rowIndex = 0;
        int colIndex = 0;
        int cellX = layoutCellX;
        int cellY = layoutCellY;
        for (int position = dataIndexBegin; position < dataIndexEnd; position++)
        {
            T data = position < datas.Length ? datas[position] : default(T);

            T cellData = position2data.ContainsKey(position) ? position2data[position] : default(T);
            RectTransform cellView = position2cell.ContainsKey(position) ? position2cell[position] : null;

            var needUpdateCell = true;
            if (cellData != null && cellData.Equals(data) && cellView != null)
            {
                needUpdateCell = false;
            }
            else if (cellView == null)
            {
                if (itemTemplate.CountPooled() == 0) { itemTemplate.CreatePool(1,string.Empty); };
                cellView = itemTemplate.Spawn(scroller.content).GetComponent<RectTransform>();
                cellView.localScale = Vector3.one;
            }

            position2cell[position] = cellView;
            if (data != null)
            {
                position2data[position] = data;
            }
            else if(position2data.ContainsKey(position))
            {
                position2data.Remove(position);
            }

            //
            cellView.SetSiblingIndex(index);
            cellView.anchoredPosition = new Vector2(cellX + cellW / 2, 0 - cellY - cellH / 2);

            cellUpdateCallback?.Invoke(position, cellView, data, needUpdateCell);
            posChange?.Invoke(cellView.position); ;
            colIndex++;
            cellX += layoutCellW;
            if (colIndex > 0 && colIndex >= cellColCount)
            {
                colIndex = 0;
                cellX = cellPadding;

                rowIndex++;
                cellY += layoutCellH;
            }

            index++;
        }

        postUpdateList?.Invoke();
    }

}
