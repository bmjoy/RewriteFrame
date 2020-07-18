using System;
using UnityEngine.Serialization;

namespace Leyoutech.Core.Loader.Config
{
    /// <summary>
    /// 资源地址数据
    /// </summary>
    [Serializable]
    public class AssetAddressData
    {
        [FieldLabel("资源地址")]
        [FormerlySerializedAs("assetAddress")]
        public string AssetAddress;

        [FieldLabel("资源路径")]
        [FormerlySerializedAs("assetPath")]
        public string AssetPath;

        [FieldLabel("bundle路径")]
        [FormerlySerializedAs("bundlePath")]
        public string BundlePath;

        [FormerlySerializedAs("labels")]
        public string[] Labels = new string[0];
    }
}
