using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XLua;
using System;
[LuaCallCSharp]
public class TestCS
{
    [CSharpCallLua]
    public delegate void EventHandle();
    public event EventHandle eventHandle;

    #region lua 调用C#
    LuaEnv luaEnv = new LuaEnv();
    //静态属性 和方法
    public static int index = 1;
    public static int Add(int a, int b)
    {
        return a + b;
    }

    //非静态方法 和属性
    public int id = 1;
    public static void Log(int id = 1, string name = "hello")
    {
        Debug.LogError("id加姓名：" + id + name);
    }

    #endregion

    // Update is called once per frame
    void Update()
    {
        if (luaEnv != null)
        {
            //清除Lua未手动释放的Luabase对象
            luaEnv.Tick();
        }
    }
    void OnDestroy()
    {
        //释放
        luaEnv.Dispose();
    }

    public static void LoadAssetAsync3(Action callBack)
    {
        Debug.LogError("fgggggggg===" + (callBack != null));
        if (callBack != null) callBack();
        //  callBack();
    }

}
