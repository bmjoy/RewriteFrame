using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XLua;
public class UILogin : MonoBehaviour
{
    public string panelname = "panelName";
    private void Awake()
    {
        // panelname = string.Format(gameObject.name, "panel");
        Logger.Log(11);

    }

    void Start()
    {
        //初始化lua环境  
        #region   C#调用lua



        XLuaManager.Instance.SafeDoString("Test01 = require \"LuaDhl/Test01\"");
        var luaenv = XLuaManager.Instance.GetLuaEnv();
        var luaTable = luaenv.Global.Get<LuaTable>("Test01"); //表里有return

        ///获取定义变量   可以通过luaEnv.Global.Get<>()  就可以在C#中访问XLUA里面的声明的变量
        int a = luaenv.Global.Get<int>("a");
        //dayi
        Debug.Log("C# 调用lua里面定义的变量" + a);

        // 获取lua 无参方法
        LuaFunction funLua1 = luaenv.Global.Get<LuaFunction>("funLua1");
        //调用方法
        funLua1.Call();

        //调用lua  有参方法
        LuaFunction funLua2 = luaenv.Global.Get<LuaFunction>("funLua2");
        funLua2.Call(1, 2);

        #endregion

        luaenv.DoString("CS.UnityEngine.Debug.Log('C#  hello word hello word')");

    }
}
