using UnityEngine.EventSystems;
using UnityEngine.UI;

public static class EventSystemUtils
{
    public static void SetFocus(Selectable value)
    {
        if (value)
        {
            EventSystem.current.SetSelectedGameObject(value.gameObject);
        }
        else
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
    }
}
