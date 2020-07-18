using System.Collections.Generic;

namespace LeyoutechEditor.Core.AssetRuler
{
    /// <summary>
    /// 资源集合类型
    /// </summary>
    public enum AssetAssemblyType
    {
        [EnumLabel("资源寻址类型")]
        AssetAddress,
    }


    /// <summary>
    /// 资源二级晒选器允许条件类型
    /// </summary>
    public enum AssetComposeType
    {
        [EnumLabel("全部满足条件才通过")]
        All,
        [EnumLabel("任意一条满足就通过")]
        Any,
    }

    /// <summary>
    /// 集合返回结果类型
    /// </summary>
    public class AssetAssemblyResult
    {
        public List<AssetGroupResult> m_GroupResults = new List<AssetGroupResult>();
    }


    /// <summary>
    /// 文件路径搜索返回结果
    /// </summary>
    public class AssetSearcherResult
    {
        public List<string> m_AssetPaths = new List<string>();
    }

    /// <summary>
    /// 资源组返回结果类型
    /// </summary>
    public class AssetGroupResult
    {
        public string m_GroupName = "";
        public List<AssetOperationResult> m_OperationResults = new List<AssetOperationResult>();
    }
    

    /// <summary>
    /// 资源文件夹过滤返回结果类型
    /// </summary>
    public class AssetFilterResult
    {
        public List<string> m_AssetPaths = new List<string>();
    }


    /// <summary>
    /// 资源二次过滤返回结果类型
    /// </summary>
    public class AssetOperationResult
    {
    }

    
}
