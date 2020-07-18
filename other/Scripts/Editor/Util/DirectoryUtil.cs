using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace LeyoutechEditor.Core.Util
{
    public static class DirectoryUtil
    {
        /// <summary>
        /// 获取目录中所有文件资源路径
        /// </summary>
        /// <param name="assetDir">磁盘目录</param>
        /// <param name="includeSubdir">是否包括子目录</param>
        /// <returns></returns>
        public static string[] GetAsset(string assetDir, bool includeSubdir)
        {
            string diskDir = PathUtil.GetDiskPath(assetDir);
            string[] files = Directory.GetFiles(diskDir, "*.*", includeSubdir ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
            if(files!=null && files.Length>0)
            {
                for(int i =0;i<files.Length;i++)
                {
                    files[i] = PathUtil.GetAssetPath(files[i].Replace("\\", "/"));
                }
            }
            return files;
        }

        /// <summary>
        /// 获取目录中所有文件资源路径，同时验证合法性
        /// </summary>
        /// <param name="assetDir">目录</param>
        /// <param name="includeSubdir">是否包括子目录</param>
        /// <param name="filter">正则表达式规则</param>
        /// <returns></returns>
        public static string[] GetAssetsByFileNameFilter(string assetDir,bool includeSubdir,string filter)
        {
            return GetAssetsByFileNameFilter(assetDir, includeSubdir, filter, null);
        }

        /// <summary>
        /// 获取目录中所有文件资源路径，同时执行后缀过滤并验证合法性，
        /// </summary>
        /// <param name="assetDir">目录</param>
        /// <param name="includeSubdir">是否包括子目录</param>
        /// <param name="filter">正则表达式规则</param>
        /// <param name="ignoreExtersion">需要过滤的后缀</param>
        /// <returns></returns>
        public static string[] GetAssetsByFileNameFilter(string assetDir,bool includeSubdir, string filter,string[] ignoreExtersion)
        {
            string[] files = GetAsset(assetDir, includeSubdir);
            List<string> assetPathList = new List<string>();
            foreach(var file in files)
            {
                string fileName = Path.GetFileName(file);//返回指定路径字符串的文件名和扩展名 GetFileName('C:\mydir\myfile.ext') returns 'myfile.ext'
                bool isValid = true;
                if(!string.IsNullOrEmpty(filter))
                {
                    isValid = Regex.IsMatch(fileName, filter);//正则表达式验证，用于验证字符串或以确保符合特定模式的一个字符串
                }
                if(isValid && ignoreExtersion!=null && ignoreExtersion.Length>0)
                {
                    string fileExt = Path.GetExtension(file).ToLower();
                    if(Array.IndexOf(ignoreExtersion,fileExt)>=0)
                    {
                        isValid = false;
                    }
                }
                if(isValid)
                {
                    assetPathList.Add(file);
                }
            }
            return assetPathList.ToArray();
        }

        /// <summary>
        /// 从sourceDirName目录中复制所有的目录及文件到指定的destDirName目录中
        /// 可以通过ignoreFileExt参数指定忽略的文件后缀。默认采用小写进行比对，忽略大小写
        /// string[] ignoreFileExt = new string[]{".meta"}
        /// </summary>
        /// <param name="sourceDirName">源目录</param>
        /// <param name="destDirName">目标目录</param>
        /// <param name="ignoreFileExt">忽略文件后缀</param>
        private static void Copy(string sourceDirName, string destDirName, string[] ignoreFileExt = null)
        {
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            foreach (string folderPath in Directory.GetDirectories(sourceDirName, "*", SearchOption.AllDirectories))
            {
                if (!Directory.Exists(folderPath.Replace(sourceDirName, destDirName)))
                    Directory.CreateDirectory(folderPath.Replace(sourceDirName, destDirName));
            }

            foreach (string filePath in Directory.GetFiles(sourceDirName, "*.*", SearchOption.AllDirectories))
            {
                var fileDirName = Path.GetDirectoryName(filePath).Replace("\\", "/");

                var fileExt = Path.GetExtension(filePath).ToLower();
                if(ignoreFileExt != null && Array.IndexOf(ignoreFileExt,fileExt)>=0)
                {
                    continue;
                }

                var fileName = Path.GetFileName(filePath);
                string newFilePath = Path.Combine(fileDirName.Replace(sourceDirName, destDirName), fileName);

                File.Copy(filePath, newFilePath, true);
            }
        }
    }
}
