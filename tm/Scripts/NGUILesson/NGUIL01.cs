using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class NGUIL01 : MonoBehaviour
{
    public UIButton MyUIButton;
    public Button button;
    void Start()
    {
       // button.onClick.AddListener(ONClick);
        UIEventListener.Get(MyUIButton.gameObject).onClick = OnCL;
    }

    public void ONClick()
    {
        Debug.LogError("dayin");
    }
    public void OnCL(GameObject go)
    {
        Debug.LogError("dayin");
    }
}
