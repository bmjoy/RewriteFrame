using System;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

namespace LeyoutechEditor.Core.AssetRuler
{
    [CreateAssetMenu(fileName = "asset_inanydir_filter", menuName = "Asset Ruler/Filter/In Any Dir", order = 2)]
    public class AssetInAnyDirNameFilter : AssetFilter
    {
        /// <summary>
        /// 是否忽略
        /// </summary>
        public bool m_IgnoreCase = true;

        /// <summary>
        /// 文件夹正则表达式
        /// </summary>
        public string M_DirNameRegex = "";

        public override bool IsMatch(string assetPath)
        {
            FileInfo fi = new FileInfo(assetPath);
            DirectoryInfo di = fi.Directory;

            string fullDirName = di.FullName;
            string[] dirs = fullDirName.Split(new char[] { '\\', '/' }, StringSplitOptions.RemoveEmptyEntries);
            foreach(var dir in dirs)
            {
                if(Regex.IsMatch(dir, M_DirNameRegex, m_IgnoreCase ? RegexOptions.IgnoreCase : RegexOptions.None))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
