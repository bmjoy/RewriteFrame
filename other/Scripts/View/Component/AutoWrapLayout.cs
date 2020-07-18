using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AutoWrapLayout : LayoutGroup
{
    public Vector2 spacing = new Vector2(10, 10);

    public override void SetLayoutHorizontal()
    {

    }

    public override void CalculateLayoutInputVertical()
    {
    }

    public override void SetLayoutVertical()
    {
        if (rectChildren.Count <= 0)
            return;

        List<Vector2> rowSizes = new List<Vector2>();

        float width = rectTransform.rect.width - padding.horizontal;
        float height = rectTransform.rect.height - padding.vertical;

        float x = 0;
        float y = 0;

        for (int i = 0; i < rectChildren.Count; i++)
        {
            RectTransform curr = rectChildren[i];
            RectTransform next = i < rectChildren.Count - 1 ? rectChildren[i + 1] : null;

            x += curr.rect.width + spacing.x;
            y = Mathf.Max(y, curr.rect.height + spacing.y);

            float nextWidth = next != null ? next.rect.width : 0;
            if (x + nextWidth >= width || i == rectChildren.Count - 1)
            {
                rowSizes.Add(new Vector2(x - spacing.x, y));
                x = 0;
                y = 0;
            }
        }

        float contentHeight = 0;
        for(int i=0;i<rowSizes.Count;i++)
        {
            contentHeight += rowSizes[i].y;
        }
        contentHeight -= spacing.y;

        float alignH = ((int)((int)childAlignment) % 3) * 0.5f;
        float alignV = ((int)((int)childAlignment) / 3) * 0.5f;

        x = 0;
        y = 0;
        int row = 0;
        int col = 0;
        float offsetH = (width - rowSizes[row].x) * alignH;
        float offsetV = (height - contentHeight) * alignV;
        for (int i = 0; i < rectChildren.Count; i++)
        {
            RectTransform child = rectChildren[i];
            RectTransform next = i < rectChildren.Count - 1 ? rectChildren[i + 1] : null;

            SetChildAlongAxis(child, 0, x + offsetH + padding.left);
            SetChildAlongAxis(child, 1, y + offsetV + padding.top);

            x += child.rect.width + spacing.x;

            float nextWidth = next != null ? next.rect.width : 0;
            if (x - spacing.x + nextWidth >= rowSizes[row].x || i == rectChildren.Count - 1)
            {
                x = 0;
                y += rowSizes[row].y;

                row++;
                col = 0;
                if (row < rowSizes.Count)
                {
                    offsetH = (width - rowSizes[row].x) * alignH;
                }
            }
            else
            {
                col++;
            }
        }
    }
}
