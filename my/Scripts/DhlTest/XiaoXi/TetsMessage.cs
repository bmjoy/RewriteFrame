using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TetsMessage : MonoBehaviour
{
    // Start is called before the first frame update
    void Awake()
    {
        Debug.Log("fasong");
        EventDispatcher.Global.Regist(EventEnum.TestA, TestACallBack);
    }

    // Update is called once per frame
    void Update()
    {

    }
    public void TestACallBack(params object[] objs)
    {
        Debug.LogError(objs[0]);

    }
}
