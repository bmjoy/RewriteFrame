using Leyoutech.Core.Loader.Config;
using System;
using System.Collections.Generic;
using UnityEngine.Serialization;

namespace LeyoutechEditor.Core.Packer
{
    [Serializable]
    public class AssetBundleTagConfig
    {
        [FormerlySerializedAs("groupDatas")]
        public List<AssetBundleGroupData> GroupDatas = new List<AssetBundleGroupData>();
    }

    [Serializable]
    public class AssetBundleGroupData
    {
        [FormerlySerializedAs("groupName")]
        public string GroupName;
        [FormerlySerializedAs("isGenAddress")]
        public bool IsGenAddress = false;
        [FormerlySerializedAs("isMain")]
        public bool IsMain = true;
        [FormerlySerializedAs("isPreload")]
        public bool IsPreload = false;
        [FormerlySerializedAs("assetDatas")]
        public List<AssetAddressData> AssetDatas = new List<AssetAddressData>();
    }
}
