using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
/// <summary>
/// 1,怎么根据配置文件打包的
/// 2，怎么根据资源名加载预设的和图片的，需要提供ab名吗
/// 3，打包的压缩方式
/// 4，加载是根据ResourceWebRequester里面的api 
/// 5，每个加载都有一个加载器
/// 6，卸载的时候是怎么卸载的资源
/// </summary>
public class AssetBulidPoolDHL : Editor
{
    #region 打包

    [MenuItem("AssetBundle/Build AssetBundles")]

    /// <summary>
    /// 根据SetName 后自动打包
    /// </summary>
    static void BuildAllAssetBundles()//进行打包
    {
        string dir = Application.dataPath + "/.." + "/TestAssetBundles";
        //判断该目录是否存在
        if (Directory.Exists(dir) == false)
        {
            Directory.CreateDirectory(dir);//在工程下创建AssetBundles目录
        }
        //相关参数自行百度
        BuildPipeline.BuildAssetBundles(dir, BuildAssetBundleOptions.DeterministicAssetBundle
         | BuildAssetBundleOptions.ChunkBasedCompression, BuildTarget.Android);
        Debug.Log("build Secuss");
    }

    [MenuItem("AssetBundle/Build Array AssetBundles")]
    public static void BuildArrayAssetBundles()
    {
        string filePath = "Assets/AssetsPackage/UI/Prefabs/View/UIBoardNPC.prefab";
        string dir = Application.dataPath + "/.." + "/TestAssetBundles";
        //判断该目录是否存在
        if (Directory.Exists(dir) == false)
        {
            Directory.CreateDirectory(dir);//在工程下创建AssetBundles目录
        }
        AssetBundleBuild[] ss = new AssetBundleBuild[1];
        AssetBundleBuild build = new AssetBundleBuild();
        build.assetBundleName = "uitest.ab";
        build.assetBundleVariant = string.Empty;
        build.assetNames = new string[] { filePath.Replace("\\", "/") };
        ss[0] = build;
        //参数一为打包到哪个路径，参数二压缩选项  参数三 平台的目标
        //只要setname 的资源都会按照设计的名字打包

        BuildPipeline.BuildAssetBundles(dir, ss, BuildAssetBundleOptions.DeterministicAssetBundle
        | BuildAssetBundleOptions.ChunkBasedCompression, BuildTarget.Android);
        Debug.Log("build Secuss");
    }

    #endregion


    #region 设置AB名字
    [MenuItem("AssetBundle/Set Selection AssetBundle Name")]
    /// <summary>
    /// 1,对于选中的文件SetName
    /// </summary>
    public static void SetAssetbundleName()
    {
        Object[] selects = Selection.objects;
        foreach (Object selected in selects)
        {

            string path = AssetDatabase.GetAssetPath(selected);
            AssetImporter assetImporter = AssetImporter.GetAtPath(path);
            assetImporter.assetBundleName = selected.name;
            assetImporter.assetBundleVariant = "unity3d";//后缀名
        }
        Debug.Log("SetName Secuss");
    }

    [MenuItem("AssetBundle/Set Folder AssetBundle Name")]
    /// <summary>
    /// 2,指定整个文件夹打包 也可以指定文件Assets路径，比如Assets/AssetsPackage/TestAB/asas.prefab
    /// </summary>
    public static void SetFolderAssetbundleName()
    {
        AssetImporter assetImporter = AssetImporter.GetAtPath("Assets/AssetsPackage/TestAB");
        assetImporter.assetBundleName = "testababab";
        assetImporter.assetBundleVariant = "unity3d";//后缀名
        Debug.Log("SetName Secuss");
    }

    //3，手动设置setName  4,根据自写打包规则setName  此处省略

    #endregion

    #region 清除SetName
    [MenuItem("AssetBundle/Clear AssetBundle Name")]
    /// <summary>
    /// 清除SetName
    /// 清除之前设置过的AssetBundleName，避免产生不必要的资源也打包
    /// 工程中只要设置了AssetBundleName的，都会进行打包
    /// </summary>
    public static void ClearAssetbundleName()
    {
        int length = AssetDatabase.GetAllAssetBundleNames().Length;
        string[] oldAssetBundleNames = new string[length];
        for (int i = 0; i < length; i++)
        {
            oldAssetBundleNames[i] = AssetDatabase.GetAllAssetBundleNames()[i];
        }

        for (int j = 0; j < oldAssetBundleNames.Length; j++)
        {
            AssetDatabase.RemoveAssetBundleName(oldAssetBundleNames[j], true);
        }
        Debug.Log("ClearName Secuss");

    }
    #endregion


}
