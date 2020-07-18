using UnityEngine;
using UnityEngine.Serialization;

namespace LeyoutechEditor.Core.AssetRuler.AssetAddress
{
    [CreateAssetMenu(fileName = "assetaddress_group", menuName = "Asset Ruler/Asset Address/Group 资源组", order = 2)]
    public class AssetAddressGroup : AssetGroup
    {
        [FieldLabel("是否根路径")]
        [FormerlySerializedAs("isGenAddress")]
        public bool m_IsGenAddress = false;

        [FieldLabel("是否主资源")]
        [FormerlySerializedAs("isMain")]
        public bool m_IsMain = true;

        [FieldLabel("是否预加载")]
        [FormerlySerializedAs("isPreload")]
        public bool m_IsPreload = false;

        /// <summary>
        /// 创建group 返回结果数据结构
        /// </summary>
        /// <returns></returns>
        protected override AssetGroupResult CreateGroupResult()
        {
            return new AssetAddressGroupResult();
        }

        /// <summary>
        /// 执行
        /// </summary>
        /// <param name="assemblyResult"></param>
        /// <returns></returns>
        public override AssetGroupResult Execute(AssetAssemblyResult assemblyResult)
        {
            AssetAddressGroupResult result = base.Execute(assemblyResult) as AssetAddressGroupResult;
            if(result != null)
            {
                result.m_IsGenAddress = m_IsGenAddress;
                result.m_IsMain = m_IsMain;
                result.m_IsPreload = m_IsPreload;
            }
            return result;
        }
    }
}
