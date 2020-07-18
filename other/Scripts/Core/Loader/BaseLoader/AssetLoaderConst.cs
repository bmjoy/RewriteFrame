using UnityEngine.SceneManagement;
using SystemObject = System.Object;
using UnityObject = UnityEngine.Object;

namespace Leyoutech.Core.Loader
{
    /// <summary>
    /// 单个资源加载完成委托
    /// </summary>
    /// <param name="pathOrAddress">路径/地址</param>
    /// <param name="uObj">加载完毕返回的 UnityObject</param>
    /// <param name="userData">携带参数</param>
    public delegate void OnAssetLoadComplete(string pathOrAddress, UnityObject uObj, SystemObject userData);

    /// <summary>
    /// 单个资源加载进度委托
    /// </summary>
    /// <param name="pathOrAddress">路径/地址</param>
    /// <param name="progress">进度值</param>
    /// <param name="userData">携带参数</param>
    public delegate void OnAssetLoadProgress(string pathOrAddress, float progress, SystemObject userData);



    /// <summary>
    /// 一组资源加载完成委托
    /// </summary>
    /// <param name="pathOrAddresses">一组 路径/地址 </param>
    /// <param name="uObjs">加载完毕返回的一组 UnityObject</param>
    /// <param name="userData">携带参数</param>
    public delegate void OnBatchAssetLoadComplete(string[] pathOrAddresses, UnityObject[] uObjs, SystemObject userData);

    /// <summary>
    /// 一组资源加载进度委托
    /// </summary>
    /// <param name="pathOrAddresses">一组 路径/地址</param>
    /// <param name="progresses">加载完毕返回的一组 进度值</param>
    /// <param name="userData">携带参数</param>
    public delegate void OnBatchAssetsLoadProgress(string[] pathOrAddresses, float[] progresses, SystemObject userData);


    /// <summary>
    /// 场景加载完成委托
    /// </summary>
    /// <param name="pathOrAddress"> 路径/地址</param>
    /// <param name="scene">加载完成的场景</param>
    /// <param name="userData">携带参数</param>
    public delegate void OnSceneLoadComplete(string pathOrAddress,Scene scene,SystemObject userData);

    /// <summary>
    /// 场景加载进度委托
    /// </summary>
    /// <param name="pathOrAddress">路径/地址</param>
    /// <param name="progress">进度值</param>
    /// <param name="userData">携带参数</param>
    public delegate void OnSceneLoadProgress(string pathOrAddress, float progress, SystemObject userData);

    public delegate void OnSceneUnloadComplete(string pathOrAddress, SystemObject userData);
    public delegate void OnSceneUnloadProgress(string pathOrAddress, float progress, SystemObject userData);

    /// <summary>
    /// 资源加载的方式
    /// </summary>
    public enum AssetLoaderMode
    {
        /// <summary>
        /// 编辑器方式
        /// </summary>
        AssetDatabase,

        /// <summary>
        /// AB模式
        /// </summary>
        AssetBundle,
    }

    /// <summary>
    /// 资源路径方式
    /// </summary>
    public enum AssetPathMode
    {
        /// <summary>
        /// 地址
        /// </summary>
        Address,

        /// <summary>
        /// 路径
        /// </summary>
        Path,
    }

    /// <summary>
    /// 加载优先级
    /// </summary>
    public enum AssetLoaderPriority
    {
        /// <summary>
        /// 非常低
        /// </summary>
        VeryLow = 100,

        /// <summary>
        /// 低
        /// </summary>
        Low = 200,

        /// <summary>
        /// 默认
        /// </summary>
        Default = 300,

        /// <summary>
        /// 高
        /// </summary>
        High = 400,

        /// <summary>
        /// 非常高
        /// </summary>
        VeryHigh = 500,
    }


    /// <summary>
    /// 加载状态
    /// </summary>
    public enum AssetLoaderState
    {
        /// <summary>
        /// 无效
        /// </summary>
        None,                     

        /// <summary>
        /// 等待
        /// </summary>
        Waiting,                 

        /// <summary>
        /// 加载中
        /// </summary>
        Loading,                 

        /// <summary>
        /// 完成
        /// </summary>
        Complete,             

        /// <summary>
        /// 取消
        /// </summary>
        Cancel,                   
    }
}
