using Leyoutech.Core.Loader;
using System;
using UnityEditor;
using UnityEngine.Serialization;

namespace LeyoutechEditor.Core.Packer
{
    /// <summary>
    /// AB可使用的压缩方式
    /// </summary>
    public enum CompressOptions
    {
        Uncompressed = 0,
        StandardCompression,
        ChunkBasedCompression,
    }
    /// <summary>
    /// 目标平台 
    /// </summary>
    public enum ValidBuildTarget
    {
        PS3 = 10,
        XBOX360 = 11,
        StandaloneWindows64 = 19,
        PSP2 = 30,
        PS4 = 31,
        PSM = 32,
        XboxOne = 33,
    }

    [Serializable] 
    public class BundlePackConfig
    {
        [FormerlySerializedAs("outputDirPath")]
        public string OutputDirPath = "";//输出AB的位置
        [FormerlySerializedAs("cleanupBeforeBuild")]
        public bool CleanupBeforeBuild = false;//打AB前是否需要将原有目录删除
        [FormerlySerializedAs("buildTarget")]
        public ValidBuildTarget BuildTarget = ValidBuildTarget.StandaloneWindows64;//平台
        [FormerlySerializedAs("compression")]
        public CompressOptions Compression = CompressOptions.StandardCompression;//压缩方式
        [FormerlySerializedAs("bundleOptions")]
        public BuildAssetBundleOptions BundleOptions = BuildAssetBundleOptions.DeterministicAssetBundle;

        public BundlePackConfig()
        {
            
        }

        internal BuildTarget GetBuildTarget()
        {
            return (BuildTarget)BuildTarget;
        }
        /// <summary>
        /// 获取打AB需要设置的选项
        /// </summary>
        /// <returns></returns>
        internal BuildAssetBundleOptions GetBundleOptions()
        {
            if(Compression == CompressOptions.Uncompressed)
            {
                return BundleOptions | BuildAssetBundleOptions.UncompressedAssetBundle;
            }else if(Compression == CompressOptions.ChunkBasedCompression)
            {
                return BundleOptions | BuildAssetBundleOptions.ChunkBasedCompression;
            }else
            {
                return BundleOptions;
            }
        }
    }
}
