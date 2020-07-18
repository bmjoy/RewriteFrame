using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Leyoutech.Core.Loader.Config
{
    public class AssetAddressConfig : ScriptableObject, ISerializationCallbackReceiver
    {
        /// <summary>
        /// 配置文件
        /// </summary>
        public static readonly string CONFIG_PATH = "Assets/Tools/AssetAddress/asset_address_config.asset";

        /// <summary>
        /// 配置文件保存的 AB 文件
        /// </summary>
        public static readonly string CONFIG_ASSET_BUNDLE_NAME = "assetaddressconfig";

        /// <summary>
        /// 资源地址数据，数组容器
        /// </summary>
        [FormerlySerializedAs("addressDatas")]
        public AssetAddressData[] AddressDatas = new AssetAddressData[0];


        /// <summary>
        /// 资源地址数据容器 《路径，资源地址数据》
        /// </summary>
        private Dictionary<string, AssetAddressData> m_PathToAssetDic = new Dictionary<string, AssetAddressData>();

        /// <summary>
        /// 《地址，路径》对应关系容器
        /// </summary>
        private Dictionary<string, string> m_AddressToPathDic = new Dictionary<string, string>();

        /// <summary>
        /// 《label ,路径列表》对应关系容器
        /// </summary>
        private Dictionary<string, List<string>> m_LabelToPathDic = new Dictionary<string, List<string>>();

        /// <summary>
        /// 《label ,地址列表》对应关系容器
        /// </summary>
        private Dictionary<string, List<string>> m_LabelToAddressDic = new Dictionary<string, List<string>>();


        /// <summary>
        /// 地址，转换一组路径
        /// </summary>
        /// <param name="addresses">地址数组</param>
        /// <returns></returns>
        public string[] GetAssetPathByAddress(string[] addresses)
        {
            List<string> paths = new List<string>();
            for(int i =0;i<addresses.Length;++i)
            {
                string assetPath = GetAssetPathByAddress(addresses[i]);
                if(assetPath == null)
                {
                    Debug.LogError($"AssetAddressConfig::GetAssetPathByAddress->Not found Asset by Address.address = {addresses[i]}");
                    return null;
                }else
                {
                    paths.Add(assetPath);
                }
            }
            return paths.ToArray() ;
        }

        /// <summary>
        /// 地址转换路径
        /// </summary>
        /// <param name="address">地址</param>
        /// <returns></returns>
        public string GetAssetPathByAddress(string address)
        {
            if(m_AddressToPathDic.TryGetValue(address,out string path))
            {
                return path;
            }
            return null;
        }

        /// <summary>
        /// 根据label 获取对应的所有 路径数组
        /// </summary>
        /// <param name="label"></param>
        /// <returns></returns>
        public string[] GetAssetPathByLabel(string label)
        {
            if(m_LabelToPathDic.TryGetValue(label,out List<string> paths))
            {
                return paths.ToArray();
            }
            return null;
        }


        /// <summary>
        /// 根据label 获取对应的所有 地址数组
        /// </summary>
        /// <param name="label"></param>
        /// <returns></returns>
        public string[] GetAssetAddressByLabel(string label)
        {
            if (m_LabelToAddressDic.TryGetValue(label, out List<string> addresses))
            {
                return addresses.ToArray();
            }
            return null;
        }


        /// <summary>
        /// 获取bundl 路径
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public string GetBundlePathByPath(string path)
        {
            if(m_PathToAssetDic.TryGetValue(path,out AssetAddressData data))
            {
                return data.BundlePath;
            }
            return null;
        }


        /// <summary>
        /// 获取一组bundl 路径数组
        /// </summary>
        /// <param name="paths"></param>
        /// <returns></returns>
        public string[] GetBundlePathByPath(string[] paths)
        {
            string[] bundlePaths = new string[paths.Length];
            for(int i =0;i<paths.Length;++i)
            {
                bundlePaths[i] = GetBundlePathByPath(paths[i]);
            }
            return bundlePaths;
        }

        /// <summary>
        /// 序列化后（ISerializationCallbackReceiver 接口）
        /// </summary>
        public void OnAfterDeserialize()
        {
            foreach(var data in AddressDatas)
            {
                if(m_AddressToPathDic.ContainsKey(data.AssetAddress))
                {
                    Debug.LogError("AssetAddressConfig::OnAfterDeserialize->address repeat.address = "+data.AssetAddress);
                    continue;
                }
                m_AddressToPathDic.Add(data.AssetAddress, data.AssetPath);
                m_PathToAssetDic.Add(data.AssetPath, data);

                if(data.Labels!=null && data.Labels.Length>0)
                {
                    foreach(var label in data.Labels)
                    {
                        if(!m_LabelToPathDic.TryGetValue(label,out List<string> paths))
                        {
                            paths = new List<string>();
                            m_LabelToPathDic.Add(label, paths);
                        }

                        if(!m_LabelToAddressDic.TryGetValue(label,out List<string> addresses))
                        {
                            addresses = new List<string>();
                            m_LabelToAddressDic.Add(label, addresses);
                        }
                        paths.Add(data.AssetPath);
                        addresses.Add(data.AssetAddress);
                    }
                }
            }
        }

        /// <summary>
        /// 序列化前（ISerializationCallbackReceiver 接口）
        /// </summary>
        public void OnBeforeSerialize()
        {
            
        }
    }
}
