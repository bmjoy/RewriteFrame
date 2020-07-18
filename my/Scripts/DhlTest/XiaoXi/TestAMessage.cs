using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestAMessage : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("jieshou");
        EventDispatcher.Global.DispatchEvent(EventEnum.TestA, "sdsd");
    }

    // Update is called once per frame
    void Update()
    {

    }
}
