using Leyoutech.Core.Loader.Config;
using System.IO;
using UnityEngine;
using UnityEngine.Serialization;

namespace LeyoutechEditor.Core.AssetRuler.AssetAddress
{
    [CreateAssetMenu(fileName = "address_mode_operation", menuName = "Asset Ruler/Asset Address/Operation/Address Mode 地址模式")]
    public class AssetAddressAddressModeOperation : AssetOperation
    {

        [EnumLabel("地址模式")]
        [FormerlySerializedAs("addressMode")]
        public AssetAddressMode m_AddressMode = AssetAddressMode.FileNameWithoutExtension;

        [FieldLabel("制定Format格式")]
        [FormerlySerializedAs("fileNameFormat")]
        public string m_FileNameFormat = "{0}";

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

                addressData.AssetAddress = GetAssetAddress(assetPath);
            }

            return result;
        }

        /// <summary>
        /// 获取地址
        /// </summary>
        /// <param name="assetPath"></param>
        /// <returns></returns>
        private string GetAssetAddress(string assetPath)
        {
            if (m_AddressMode == AssetAddressMode.FullPath)
                return assetPath;
            else if (m_AddressMode == AssetAddressMode.FileName)
                return Path.GetFileName(assetPath);
            else if (m_AddressMode == AssetAddressMode.FileNameWithoutExtension)
                return Path.GetFileNameWithoutExtension(assetPath);
            else if (m_AddressMode == AssetAddressMode.FileFormatName)
                return string.Format(m_FileNameFormat, Path.GetFileNameWithoutExtension(assetPath));
            else
                return assetPath;
        }
    }
}
