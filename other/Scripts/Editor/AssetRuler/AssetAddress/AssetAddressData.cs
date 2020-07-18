using Leyoutech.Core.Loader.Config;
using System;
using System.Collections.Generic;

namespace LeyoutechEditor.Core.AssetRuler.AssetAddress
{
    /// <summary>
    /// AB 包模式
    /// </summary>
    public enum AssetBundlePackMode
    {
        [EnumLabel("文件夹为一个包")]
        Together,

        [EnumLabel("单文件为一个包")]
        Separate,

        [EnumLabel("文件夹规定指定数量为一个包")]
        GroupByCount,

        [EnumLabel("文件夹为一个包，并指定别名")] 
        TogetherWithFolderSuperaddAssignName,

        [EnumLabel("多文件夹为一个包，并指定别名")]
        TogetherWithLotOfFolderAssignName,
    }

    /// <summary>
    /// 资源地址模式
    /// </summary>
    public enum AssetAddressMode
    {
        [EnumLabel("全路径")]
        FullPath,                                           //全路径

        [EnumLabel("文件名,没有扩展名")]
        FileNameWithoutExtension,        //没有扩展名的文件名  D:\11.text ----> 11

        [EnumLabel("文件名有扩展名")]
        FileName,                                        //文件名   D:\11.text ----> 11 .text

        [EnumLabel("按照给定格式的文件名")]
        FileFormatName,                           //按照格式的文件名
    }

    /// <summary>
    /// AB 包，set Name 的模式
    /// </summary>
    public enum AssetBundleNameMode
    {
        [EnumLabel("使用path 字符串")]
        PathString,
        [EnumLabel("使用path 的Md5")]
        PathMd5,
    }



    /// <summary>
    /// 资源地址集合返回类型
    /// </summary>
    public class AssetAddressAssemblyResult : AssetAssemblyResult
    {

    }

    /// <summary>
    /// 资源地址组返回结果类型
    /// </summary>
    public class AssetAddressGroupResult : AssetGroupResult
    {
        public bool m_IsGenAddress = false;
        public bool m_IsMain = true;
        public bool m_IsPreload = false;
    }

    /// <summary>
    /// 资源操作结果类型
    /// </summary>
    public class AssetAddressOperationResult : AssetOperationResult
    {
        public Dictionary<string, AssetAddressData> m_AddressDataDic = new Dictionary<string, AssetAddressData>();
    }

   
}
