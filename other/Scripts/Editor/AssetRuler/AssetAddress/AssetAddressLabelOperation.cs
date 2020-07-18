using Leyoutech.Core.Loader.Config;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace LeyoutechEditor.Core.AssetRuler.AssetAddress
{
    [CreateAssetMenu(fileName = "address_label_operation", menuName = "Asset Ruler/Asset Address/Operation/Set Label 设置标签")]
    public class AssetAddressLabelOperation : AssetOperation
    {
        /// <summary>
        /// label 标签列表
        /// </summary>
        [FormerlySerializedAs("labels")]
        public List<string> m_labels = new List<string>();


        /// <summary>
        /// 执行
        /// </summary>
        /// <param name="filterResult"></param>
        /// <param name="operationResult"></param>
        /// <returns></returns>
        public override AssetOperationResult Execute(AssetFilterResult filterResult, AssetOperationResult operationResult)
        {
            if (operationResult == null)
            {
                operationResult = new AssetAddressOperationResult();
            }
            AssetAddressOperationResult result = operationResult as AssetAddressOperationResult;
            foreach (var assetPath in filterResult.m_AssetPaths)
            {
                if (!result.m_AddressDataDic.TryGetValue(assetPath, out AssetAddressData addressData))
                {
                    addressData = new AssetAddressData();
                    addressData.AssetPath = assetPath;
                    result.m_AddressDataDic.Add(assetPath, addressData);
                }

                addressData.Labels = m_labels.ToArray();
            }

            return result;
        }

    }
}
