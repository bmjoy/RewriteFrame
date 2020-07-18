using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

namespace LeyoutechEditor.Core.AssetRuler
{
    [CreateAssetMenu(fileName = "asset_filename_filter", menuName = "Asset Ruler/Filter/File Name",order =1)]
    public class AssetFileNameFilter : AssetFilter
    {
        /// <summary>
        /// 是否忽略
        /// </summary>
        public bool m_IgnoreCase = true;

        /// <summary>
        /// 文件名正则表达式
        /// </summary>
        public string m_FileNameRegex = "";

        public override bool IsMatch(string assetPath)
        {
            string fileName = Path.GetFileName(assetPath);
            return Regex.IsMatch(fileName, m_FileNameRegex, m_IgnoreCase ? RegexOptions.IgnoreCase : RegexOptions.None);
        }
    }
}
