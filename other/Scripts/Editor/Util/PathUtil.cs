using System.IO;
using UnityEditor;
using UnityEngine;

namespace LeyoutechEditor.Core.Util
{
    public static class PathUtil
    {
        /// <summary>
        /// 获得工程内资源路径
        /// </summary>
        /// <param name="dirPath"></param>
        /// <returns></returns>
        public static string GetAssetPath(string dirPath)
        {
            dirPath = dirPath.Replace("\\", "/");
            if (dirPath.StartsWith(Application.dataPath))
            {
                return "Assets" + dirPath.Replace(Application.dataPath, "");
            }
            return string.Empty;
        }

        /// <summary>
        /// 获取磁盘路径
        /// </summary>
        /// <param name="assetPath"></param>
        /// <returns></returns>
        public static string GetDiskPath(string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath))
            {
                return string.Empty;
            }
            assetPath = assetPath.Replace("\\", "/");
            if (!assetPath.StartsWith("Assets"))
            {
                return string.Empty;
            }
            return Application.dataPath + assetPath.Substring(assetPath.IndexOf("Assets") + 6);
        }


        /// <summary>
        /// 获取选择的文件夹路径
        /// </summary>
        /// <returns></returns>
        public static string GetSelectionAssetDirPath()
        {
            string path = "Assets";
            foreach (UnityEngine.Object obj in Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.Assets))
            {
                if(obj == null)
                {
                    continue;
                }
                path = AssetDatabase.GetAssetPath(obj);
                if(Path.HasExtension(path))//确定路径是否包括文件扩展名
                {
                    path = Path.GetDirectoryName(path);//返回指定路径字符串的目录信息
                }
                break;
            }
            return path;
        }
    }
}


