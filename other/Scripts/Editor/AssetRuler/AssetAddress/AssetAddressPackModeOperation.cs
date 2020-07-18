using Leyoutech.Core.Loader.Config;
using System.IO;
using UnityEngine;
using Leyoutech.Core.HashTools;
using UnityEngine.Serialization;

namespace LeyoutechEditor.Core.AssetRuler.AssetAddress
{
    [CreateAssetMenu(fileName = "pack_mode_operation", menuName = "Asset Ruler/Asset Address/Operation/Pack Mode 打包规则")]
    public class AssetAddressPackModeOperation : AssetOperation
    {
        [EnumLabel("AB 包模式")]
        [FormerlySerializedAs("packMode")]
        public AssetBundlePackMode m_PackMode = AssetBundlePackMode.Together;

        [EnumLabel("AB 包setName 的模式")]
        [FormerlySerializedAs("bundNameMode")]
        public AssetBundleNameMode m_BundNameMode = AssetBundleNameMode.PathString;

        [EnumLabel("Has 算法公式")]
        [FormerlySerializedAs("hasType")]
        public HashHelper.Has_Type m_HasType = HashHelper.Has_Type.Hash_MD5_32;

        /// <summary>
        /// 限制数量包内包含文件数量
        /// </summary>
        [FormerlySerializedAs("packCount")]
        public int m_PackCount = 0;

        /// <summary>
        /// 指定包名字
        /// </summary>
       [FormerlySerializedAs("packName")]
        public string m_PackName = "";


        private int m_GroupCount = 0;   //包文件数量
        private int m_GroupIndex = 0;   //索引

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

                string rootFolder = Path.GetDirectoryName(assetPath).Replace("\\", "/");

                string bundPathStr = GetAssetBundle(rootFolder, assetPath).ToLower();
                //设置AB set Name 
                switch (m_BundNameMode)
                {
                    case AssetBundleNameMode.PathString:
                        addressData.BundlePath = bundPathStr;
                        break;
                    case AssetBundleNameMode.PathMd5:
                        addressData.BundlePath = GetHash( bundPathStr,false);
                        break;
                }
            }

            return result;
        }


        /// <summary>
        /// 获取bundle name
        /// </summary>
        /// <param name="rootFolder"></param>
        /// <param name="assetPath"></param>
        /// <returns></returns>
        private string GetAssetBundle(string rootFolder, string assetPath)
        {
            if (m_PackMode == AssetBundlePackMode.Separate)//单包
            {
                char[] replaceChars = new char[] { '.', ' ', '-','\t' };
                foreach (var c in replaceChars)
                {
                    assetPath = assetPath.Replace(c, '_');
                }
                return assetPath;
            }
            else if (m_PackMode == AssetBundlePackMode.Together)//文件夹为一个包
            {
                return rootFolder.Replace(' ', '_');
            }
            else if (m_PackMode == AssetBundlePackMode.GroupByCount)//文件夹规定指定数量为一个包
            {
                m_GroupCount++;
                if (m_GroupCount >= m_PackCount)
                {
                    m_GroupIndex++;
                    m_GroupCount = 0;
                }
                return rootFolder + "_" + m_GroupIndex;
            }
            else if(m_PackMode == AssetBundlePackMode.TogetherWithFolderSuperaddAssignName) //文件夹为一个包，并指定别名
            {
                return rootFolder.Replace(' ', '_')+"/"+m_PackName;
            }
            else if (m_PackMode == AssetBundlePackMode.TogetherWithLotOfFolderAssignName) //多文件夹为一个包，并指定别名
            {
                return  m_PackName;
            }

            return null;
        }


        /// <summary>
        /// 获得hash 值
        /// </summary>
        /// <param name="word"></param>
        /// <param name="toUpper"></param>
        /// <returns></returns>
        private string GetHash(string word, bool toUpper = true)
        {
            switch (m_HasType)
            {
                case HashHelper.Has_Type.Hash_MD5_32:
                    return HashHelper.Hash_MD5_32(word, toUpper);
                case HashHelper.Has_Type.Hash_MD5_16:
                    return HashHelper.Hash_MD5_16(word, toUpper);
                case HashHelper.Has_Type.Hash_2_MD5_32:
                    return HashHelper.Hash_2_MD5_32(word, toUpper);
                case HashHelper.Has_Type.Hash_2_MD5_16:
                    return HashHelper.Hash_2_MD5_16(word, toUpper);
                case HashHelper.Has_Type.Hash_SHA_1:
                    return HashHelper.Hash_SHA_1(word, toUpper); ;
                case HashHelper.Has_Type.Hash_SHA_256:
                    return HashHelper.Hash_SHA_256(word, toUpper);
                case HashHelper.Has_Type.Hash_SHA_384:
                    return HashHelper.Hash_SHA_384(word, toUpper);
                case HashHelper.Has_Type.Hash_SHA_512:
                    return HashHelper.Hash_SHA_512(word, toUpper);
                default:
                    break;
            }

            return word;
        }
    }
}
