using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Text;
using UnityEditor.ProjectWindowCallback;
using System.Text.RegularExpressions;
 
public class CreateLua {
    [MenuItem("Assets/Create/Lua Sctipt",false,80)]
    static void CreateLuaFile()
    {
        CreateFile("lua");
    }

    [MenuItem("Assets/Create/Text File",false,81)]
    static void CreateTextFile()
    {
        CreateFile("txt");
    }

    /// <summary>
    /// 创建文件类的文件
    /// </summary>
    /// <param name="fileEx"></param>
    static void CreateFile(string fileEx)
    {
        //获取当前所选择的目录（相对于Assets的路径）
        var selectPath = AssetDatabase.GetAssetPath(Selection.activeObject);
        var path = Application.dataPath.Replace("Assets", "") + "/";
        var newFileName = "new_" + fileEx + "." + fileEx;
        var newFilePath = selectPath + "/" + newFileName;
        var fullPath = path + newFilePath;

        //简单的重名处理
        if (File.Exists(fullPath))
        {
            var newName = "new_" + fileEx + "-" + UnityEngine.Random.Range(0, 100) + "." + fileEx;
            newFilePath = selectPath + "/" + newName;
            fullPath = fullPath.Replace(newFileName, newName);
        }

        //如果是空白文件，编码并没有设成UTF-8
        File.WriteAllText(fullPath, "--  please create lue file", Encoding.UTF8);

        AssetDatabase.Refresh();

        //选中新创建的文件
        var asset = AssetDatabase.LoadAssetAtPath(newFilePath, typeof(UnityEngine.Object));
        Selection.activeObject = asset;
    }
}