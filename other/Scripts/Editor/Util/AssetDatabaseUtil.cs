using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace LeyoutechEditor.Core.Util
{
    public static class AssetDatabaseUtil
    {
        /// <summary>
        /// 查找所有的场景资源
        /// </summary>
        /// <returns></returns>
        public static string[] FindScenes()
        {
            return GetAssetPathByGUID(AssetDatabase.FindAssets("t:scene"));
        }

        /// <summary>
        /// 查找所有设置过BundleName的所有资源
        /// </summary>
        /// <returns></returns>
        public static string[] FindAssetWithBundleName()
        {
            return GetAssetPathByGUID(AssetDatabase.FindAssets("b:"));
        }

        /// <summary>
        /// 在给定的目录(folderPath)中查找指定类型的资源
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="folderPath"></param>
        /// <returns></returns>
        public static string[] FindAssetInFolder<T>(string folderPath)
        {
            return GetAssetPathByGUID(AssetDatabase.FindAssets($"t:{typeof(T).Name}", new string[] { folderPath }));
        }

        /// <summary>
        /// 查找指定类型的资源
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static string[] FindAssets<T>() where T : UnityEngine.Object
        {
            return GetAssetPathByGUID(AssetDatabase.FindAssets($"t:{typeof(T).Name} "));
        }

        /// <summary>
        /// 查找指定标签的资源(label)
        /// </summary>
        /// <param name="label"></param>
        /// <returns></returns>
        public static string[] FindAssets(string label)
        {
            return GetAssetPathByGUID(AssetDatabase.FindAssets($"l:{label} "));
        }

        /// <summary>
        /// 根据资源的T及设置的标签label查找符合的资源
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="label"></param>
        /// <returns>符合条件的资源地址的数组</returns>
        public static string[] FindAssets<T>(string label) where T : UnityEngine.Object
        {
            return GetAssetPathByGUID(AssetDatabase.FindAssets($"t:{typeof(T).Name} l:{label}"));
        }

        /// <summary>
        /// 将guid转换成资源路径
        /// 由于使用AssetDatabase.FindAssets得到的资源为guid,为方便使用需要转换一下
        /// </summary>
        /// <param name="guids"></param>
        /// <returns></returns>
        private static string[] GetAssetPathByGUID(string[] guids)
        {
            if (guids == null)
            {
                return null;
            }

            string[] paths = new string[guids.Length];
            for (int i = 0; i < guids.Length; i++)
            {
                paths[i] = AssetDatabase.GUIDToAssetPath(guids[i]);
            }
            return paths;
        }

        /// <summary>
        /// 在Project中创建指定类型的资源
        /// 只能创建ScriptableObject类型的资源，对于给定的名称，如果目录中已经存在相同名称的资源，会进行重命名
        /// 创建资源时可以指定资源存储的目录 (assetFolder基于Unity的Assets目录)
        /// </summary>
        /// <typeparam name="T">资源的类型</typeparam>
        /// <param name="fileName">资源名称</param>
        /// <param name="assetFolder">资源存储目录，默认为空</param>
        /// <returns></returns>
        public static T CreateAsset<T>(string fileName, string assetFolder = "") where T : ScriptableObject
        {
            if (fileName.Contains("/"))
                throw new ArgumentException("Base name should not contain slashes");
            if (!string.IsNullOrEmpty(assetFolder) && !assetFolder.StartsWith("Assets"))
            {
                throw new ArgumentException("Asset Folder should be start with Assets");
            }

            string folderPath = string.Empty;
            if (!string.IsNullOrEmpty(assetFolder))
            {
                string diskFolderPath = PathUtil.GetDiskPath(assetFolder);
                if (!Directory.Exists(diskFolderPath))
                {
                    Directory.CreateDirectory(diskFolderPath);
                }
                folderPath = assetFolder;
            } else
            {
                folderPath = AssetDatabase.GetAssetPath(Selection.activeObject);

                if (string.IsNullOrEmpty(folderPath))
                {
                    folderPath = "Assets";
                }
                else if (Path.GetExtension(folderPath) != string.Empty)
                {
                    folderPath = folderPath.Replace(Path.GetFileName(folderPath), string.Empty);
                }
            }

            var asset = ScriptableObject.CreateInstance<T>();
            var assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(folderPath + "/" + fileName + ".asset");

            AssetDatabase.CreateAsset(asset, assetPathAndName);
            AssetDatabase.SaveAssets();
            return asset;
        }
        /// <summary>
        /// 查找指定的资源依赖的所有资源
        /// 通过设定ignoreExt的值可以忽略掉指定文件后缀的资源
        /// 默认情况下后缀的检查，以小写字母进行。如string[] ignoreExt = new string[]{".cs"}
        /// </summary>
        /// <param name="assetPath"></param>
        /// <param name="ingoreExt"></param>
        /// <returns></returns>
        public static string[] GetDependencies(string assetPath, string[] ignoreExt = null)
        {
            return GetAssetDependencies(assetPath, true, ignoreExt);
        }

        /// <summary>
        /// 查找指定的资源直接依赖的资源
        /// 通过设定ignoreExt的值可以忽略掉指定文件后缀的资源
        /// 默认情况下后缀的检查，以小写字母进行。如string[] ignoreExt = new string[]{".cs"}
        /// </summary>
        /// <param name="assetPath"></param>
        /// <param name="ingoreExt"></param>
        /// <returns></returns>
        public static string[] GetDirectlyDependencies(string assetPath, string[] ignoreExt = null)
        {
            return GetAssetDependencies(assetPath, false, ignoreExt);
        }
       /// <summary>
       /// 查找资源依赖
       /// </summary>
       /// <param name="assetPath">要查找的资源的路径</param>
       /// <param name="isRecursive">是否递归查找</param>
       /// <param name="ignoreExt">忽略的文件的后缀</param>
       /// <returns></returns>
        private static string[] GetAssetDependencies(string assetPath,bool isRecursive,string[] ignoreExt)
        {
            string[] assetPaths = AssetDatabase.GetDependencies(assetPath, isRecursive);
            if (ignoreExt == null || ignoreExt.Length == 0)
            {
                return assetPaths;
            }

            return (from path in assetPaths
                    let ext = Path.GetExtension(path).ToLower()
                    where Array.IndexOf(ignoreExt, ext) < 0
                    select path).ToArray();
        }
    }
}
