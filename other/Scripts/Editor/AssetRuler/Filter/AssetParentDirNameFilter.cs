using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

namespace LeyoutechEditor.Core.AssetRuler
{
    [CreateAssetMenu(fileName = "asset_parentdir_filter", menuName = "Asset Ruler/Filter/In Parent Dir", order = 3)]
    public class AssetParentDirNameFilter : AssetFilter
    {
        public bool m_IgnoreCase = true;
        public string M_DirNameRegex = "";

        public override bool IsMatch(string assetPath)
        {
            FileInfo fi = new FileInfo(assetPath);
            DirectoryInfo di = fi.Directory;

            return Regex.IsMatch(di.Name, M_DirNameRegex, m_IgnoreCase?RegexOptions.IgnoreCase:RegexOptions.None);
        }
    }
}
