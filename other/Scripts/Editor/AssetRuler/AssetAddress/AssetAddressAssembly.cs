using Leyoutech.Core.Loader.Config;
using LeyoutechEditor.Core.Packer;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static LeyoutechEditor.Core.Packer.AssetBundleTagConfig;

namespace LeyoutechEditor.Core.AssetRuler.AssetAddress
{
    [CreateAssetMenu(fileName = "assetaddress_assembly", menuName = "Asset Ruler/Asset Address/Assembly 资源集合 ", order = 1)]
    public class AssetAddressAssembly : AssetAssembly
	{
		/// <summary>
		/// 用于标识测试group名的前缀字符串
		/// </summary>
		private static string DEFAULT_TEST_GROUP_PREFIX = "_test";
		public string TestGroupPrefix = DEFAULT_TEST_GROUP_PREFIX;

		/// <summary>
		/// 自动查找 Goup 配置
		/// </summary>
		public void AutoFind()
        {
            (Editor.CreateEditor(this) as AssetAddressAssemblyEditor).AutoFindGroup();
        }

        /// <summary>
        /// 执行保存到二进制中
        /// </summary>
        /// <returns></returns>
        public override AssetAssemblyResult Execute()
        {
            AssetAddressAssemblyResult result = new AssetAddressAssemblyResult();
            foreach(var group in m_AssetGroups)
            {
                group.Execute(result);
            }

            //读取配置
            AssetBundleTagConfig tagConfig = Util.FileUtil.ReadFromBinary<AssetBundleTagConfig>(BundlePackUtil.GetTagConfigPath());
            if(tagConfig.GroupDatas == null)
            {
                tagConfig.GroupDatas = new List<AssetBundleGroupData>();
            }
            else
            {
                tagConfig.GroupDatas.Clear();
            }

            foreach (var groupResult in result.m_GroupResults)
            {
                AssetAddressGroupResult gResult = groupResult as AssetAddressGroupResult;

                AssetBundleGroupData groupData = new AssetBundleGroupData();
                groupData.GroupName = gResult.m_GroupName;
                groupData.IsGenAddress = gResult.m_IsGenAddress;
                groupData.IsMain = gResult.m_IsMain;
                groupData.IsPreload = gResult.m_IsPreload;

                tagConfig.GroupDatas.Add(groupData);

                foreach (var operationResult in gResult.m_OperationResults)
                {
                    AssetAddressOperationResult oResult = operationResult as AssetAddressOperationResult;
                    foreach (var kvp in oResult.m_AddressDataDic)
                    {
                        AssetAddressData aaData = new AssetAddressData();
                        AssetAddressData kvpValue = kvp.Value as AssetAddressData;

                        aaData.AssetAddress = kvpValue.AssetAddress;
                        aaData.AssetPath = kvpValue.AssetPath;
                        aaData.BundlePath = kvpValue.BundlePath;
                        aaData.Labels = new List<string>(kvpValue.Labels).ToArray();

                        groupData.AssetDatas.Add(aaData);
                    }
                }
            }

            //保存配置
            Util.FileUtil.SaveToBinary<AssetBundleTagConfig>(BundlePackUtil.GetTagConfigPath(), tagConfig);

            return result;
        }
    }
}
