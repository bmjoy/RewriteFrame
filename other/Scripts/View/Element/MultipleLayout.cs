using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MultipleLayout : LayoutGroup
{
    public Vector2 Spacing = Vector2.zero;

    public float MaxWidth = 1000;
    public float RowHeight = 20;

    private List<float> m_RowWidth = new List<float>();
    
    public override void CalculateLayoutInputHorizontal()
    {
        base.CalculateLayoutInputHorizontal();

        m_RowWidth.Clear();

        float width = 0;
        float height = 0;

        if (rectChildren.Count > 0)
        {
            int col = 0;

            float x = 0;
            for (int i = 0; i < rectChildren.Count; i++)
            {
                RectTransform child = rectChildren[i];

                float childW = child.sizeDelta.x;

                x += childW + Spacing.x;

                if (x > MaxWidth || i == rectChildren.Count - 1)
                {
                    m_RowWidth.Add(x);
                    width = Mathf.Max(x, width);

                    Debug.LogError(width + "/" + MaxWidth);

                    x = 0;
                    col = 0;
                }
            }
        }

        width = Mathf.Max(width, MaxWidth) + padding.horizontal;
        height = m_RowWidth.Count * (RowHeight + Spacing.y) + padding.vertical - (m_RowWidth.Count > 0 ? Spacing.y : 0);

        SetLayoutInputForAxis(width, width, 1, 0);
        SetLayoutInputForAxis(height, height, 1, 1);

        Debug.LogError("CalculateLayoutInputHorizontal()");
    }

    public override void SetLayoutHorizontal()
    {
    }

    public override void CalculateLayoutInputVertical()
    {
        if (rectChildren.Count > 0)
        {
            float hAlign = ((float)((int)childAlignment) % 3) * 0.5f;
            float vAlign = ((float)((int)childAlignment) / 3) * 0.5f;

            int row = 0;
            int col = 0;
            float xOffset = (MaxWidth - m_RowWidth[row]) * hAlign;
            float yOffset = 0;

            float x = 0;
            float y = 0;
            for (int i = 0; i < rectChildren.Count; i++)
            {
                RectTransform child = rectChildren[i];

                SetChildAlongAxis(child, 0, x + xOffset);
                SetChildAlongAxis(child, 1, y + yOffset);

                x += child.sizeDelta.x + Spacing.x;

                if (x >= MaxWidth)
                {
                    x = 0;
                    row++;
                    col = 0;

                    if (row < m_RowWidth.Count)
                    {
                        xOffset = (MaxWidth - m_RowWidth[row]) * hAlign;
                        yOffset += RowHeight + Spacing.y;
                    }
                    else
                    {
                        break;
                    }
                }
                else
                {
                    col++;
                }
            }
        }
    }

    public override void SetLayoutVertical()
    {
    }
}
