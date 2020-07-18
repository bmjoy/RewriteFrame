using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Selectable))]
public class ScrollRectAutoFocusItem : MonoBehaviour, ISelectHandler
{
    public static bool AutoFocus = true;

    private ScrollRect scroller;

    public void Start()
    {
        scroller = GetComponentInParent<ScrollRect>();
	}

    public void OnSelect(BaseEventData eventData)
    {
        if (!AutoFocus || scroller == null)
        {
            return;
        }

        float viewportWidth = scroller.viewport.rect.width;
        float viewportHeight = scroller.viewport.rect.height;
        float viewportLeft = scroller.content.anchoredPosition.x * -1;
        float viewportRight = viewportLeft + viewportWidth;
        float viewportTop = scroller.content.anchoredPosition.y;
        float viewportBottom = viewportTop + viewportHeight;

        float contentWidth = scroller.content.rect.width;
        float contentHeight = scroller.content.rect.height;

        RectTransform rectTransform = GetComponent<RectTransform>();
        Rect rect = GetComponent<RectTransform>().rect;

        float cellTop = contentHeight - (rectTransform.anchoredPosition.y + rect.yMin + rect.height);
        float cellBottom = cellTop + rect.height;
        float cellLeft = rect.x;
        float cellRight = cellLeft + rect.width;

        //计算纵向滚动
        bool needScrollY = false;
        float scrollToY = scroller.content.anchoredPosition.y;
        if (cellTop < viewportTop || cellBottom < viewportTop)
        {
            //在视口上面
            needScrollY = true;
            scrollToY = cellTop;
        }
        else if (cellTop > viewportBottom || cellBottom > viewportBottom)
        {
            //在视口下面
            needScrollY = true;
            scrollToY = cellBottom - viewportHeight;
        }

        //计算横向滚动
        bool needScrollX = false;
        float scrollToX = scroller.content.anchoredPosition.x * -1;
        if (cellLeft < viewportLeft || cellRight < viewportLeft)
        {
            //在视口左边
            needScrollX = true;
            scrollToX = cellLeft;
        }
        else if (cellLeft > viewportRight || cellRight > viewportRight)
        {
            //在视口右边
            needScrollX = true;
            scrollToX = cellRight - viewportWidth;
        }

        if (needScrollX || needScrollY)
        {
            scroller.StopMovement();
        }

        if (needScrollX)
        {
            float contentHiddenWidth = contentWidth - viewportWidth;
            if (scrollToX > contentHiddenWidth)
            {
                scrollToX = contentHiddenWidth;
            }
            scroller.horizontalNormalizedPosition = scrollToX / contentHiddenWidth;
        }
        if (needScrollY)
        {
            float contentHiddenHeight = contentHeight - viewportHeight;
            if (scrollToY > contentHiddenHeight)
            {
                scrollToY = contentHiddenHeight;
            }
            scroller.verticalNormalizedPosition = 1.0f - scrollToY / contentHiddenHeight;
        }
    }
}
