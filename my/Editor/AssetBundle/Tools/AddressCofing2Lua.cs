using System.Collections.Generic;
using UnityEngine;
using System.Text;
using UnityEditor;
public class AddressCofing2Lua : Singleton<AddressCofing2Lua>
{
    /// <summary>
    /// 单利类
    /// </summary>

    public override void Dispose() { }
    public List<string> addressList = new List<string>();
    private string AssetsaddressConfig = "Assets/LuaScripts/AssetaddressConfig.lua";
    /// <summary>
    /// 写入地址
    /// </summary>
    /// <param name="outFile"></param>
    /// <param name="outLines"></param>
    public void WriteAssetsAddress()
    {

        StringBuilder classSB = new StringBuilder();
        addressList.Sort();

        classSB.AppendLine("AssetaddressConfig = {}");

        addressList.ForEach((value) =>
        {
            if (value.Contains(".prefab") || value.Contains(".spriteatlas"))
            {
                string root = value.Replace("Assets/AssetsPackage/", "");
                string[] str = root.Split(new char[] { '.' });
                string[] str2 = str[0].Split(new char[] { '/' });
                classSB.AppendLine("AssetaddressConfig." + str2[str2.Length - 1] + "  =  " + "\"" + str[0] + "." + str[1] + "\"");
            }
        });

        classSB.AppendLine("return " + "AssetaddressConfig");
        string assetsaddress = AssetsaddressConfig;

        GameUtility.SafeWriteAllText(assetsaddress, classSB.ToString());

        addressList.Clear();
        // AssetDatabase.Refresh();
        //AssetBundleEditorHelper.CreateAssetbundleForCurrent(assetsaddress);
        Debug.Log("AssetsaddressConfig.Lua success...");


        AssetDatabase.Refresh();
    }


}
