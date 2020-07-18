using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace LeyoutechEditor.Core.AssetRuler
{
    public class AssetAssembly : ScriptableObject
    {
        /// <summary>
        /// 资源集合类型
        /// </summary>
        [EnumLabel("资源集合类型")]
        [FormerlySerializedAs("assetAssemblyType")]
        public AssetAssemblyType m_AssetAssemblyType = AssetAssemblyType.AssetAddress;

        /// <summary>
        /// 资源组列表
        /// </summary>
        [FormerlySerializedAs("assetGroups")]
        public List<AssetGroup> m_AssetGroups = new List<AssetGroup>();


        /// <summary>
        /// 执行
        /// </summary>
        /// <returns></returns>
        public virtual AssetAssemblyResult Execute()
        {
            return null;
        }
    }
}
