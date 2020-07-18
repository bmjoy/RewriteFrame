/*===============================
 * Author: [Allen]
 * Purpose: AssetUtil
 * Time: 2019/10/18 11:55:53
================================*/
using SystemObject = System.Object;
using UnityObject = UnityEngine.Object;
using AssetMgr = Leyoutech.Core.Loader.AssetManager;
using Leyoutech.Core.Loader;
using UnityEngine.SceneManagement;
using Leyoutech.Core.Loader.Config;

public static class AssetUtil
{
    /// <summary>
    /// 异步加载单个文件
    /// </summary>
    /// <param name="pathOrAddress">路径/地址</param>
    /// <param name="complete">单个资源加载完成委托</param>
    /// <param name="priority">加载优先级</param>
    /// <param name="progress">单个资源加载进度委托</param>
    /// <param name="userData">携带参数</param>
    /// <returns></returns>
    public static AssetLoaderHandle LoadAssetAsync(
        string pathOrAddress,
        OnAssetLoadComplete complete,
        AssetLoaderPriority priority = AssetLoaderPriority.Default,
        OnAssetLoadProgress progress = null,
        SystemObject userData = null)
    {
        return AssetMgr.GetInstance().LoadAssetAsync(pathOrAddress, complete, priority, progress, userData);
    }


    /// <summary>
    /// 一组资源加载
    /// </summary>
    /// <param name="pathOrAddresses">一组 路径/地址</param>
    /// <param name="complete">单个资源加载完成委托</param>
    /// <param name="batchComplete">一组资源加载完成委托</param>
    /// <param name="priority">加载优先级</param>
    /// <param name="progress">单个资源加载进度委托</param>
    /// <param name="batchProgress">一组资源加载进度委托</param>
    /// <param name="userData">携带参数</param>
    /// <returns></returns>
    public static AssetLoaderHandle LoadBatchAssetAsync(
        string[] pathOrAddresses,
        OnAssetLoadComplete complete,
        OnBatchAssetLoadComplete batchComplete,
        AssetLoaderPriority priority = AssetLoaderPriority.Default,
        OnAssetLoadProgress progress = null,
        OnBatchAssetsLoadProgress batchProgress = null,
        SystemObject userData = null)
    {
        return AssetMgr.GetInstance().LoadBatchAssetAsync(pathOrAddresses, complete, batchComplete, priority, progress, batchProgress, userData);
    }


    /// <summary>
    /// 实例化单个资源
    /// </summary>
    /// <param name="pathOrAddress">路径/地址</param>
    /// <param name="complete">单个资源加载完成委托</param>
    /// <param name="priority">加载优先级</param>
    /// <param name="progress">单个资源加载进度委托</param>
    /// <param name="userData">携带参数</param>
    /// <returns></returns>
    public static AssetLoaderHandle InstanceAssetAsync(
        string pathOrAddress,
        OnAssetLoadComplete complete,
        AssetLoaderPriority priority = AssetLoaderPriority.Default,
        OnAssetLoadProgress progress = null,
        SystemObject userData = null)
    {
        return AssetMgr.GetInstance().InstanceAssetAsync(pathOrAddress, complete, priority, progress, userData);
    }


    /// <summary>
    ///实例化一组资源
    /// </summary>
    /// <param name="pathOrAddresses">一组 路径/地址</param>
    /// <param name="complete">单个资源加载完成委托</param>
    /// <param name="batchComplete">一组资源加载完成委托</param>
    /// <param name="priority">加载优先级</param>
    /// <param name="progress">单个资源加载进度委托</param>
    /// <param name="batchProgress">一组资源加载进度委托</param>
    /// <param name="userData">携带参数</param>
    /// <returns></returns>
    public static AssetLoaderHandle InstanceBatchAssetAsync(
        string[] pathOrAddresses,
        OnAssetLoadComplete complete,
        OnBatchAssetLoadComplete batchComplete,
        AssetLoaderPriority priority = AssetLoaderPriority.Default,
        OnAssetLoadProgress progress = null,
        OnBatchAssetsLoadProgress batchProgress = null,
        SystemObject userData = null)
    {
        return AssetMgr.GetInstance().InstanceBatchAssetAsync(pathOrAddresses, complete, batchComplete, priority = AssetLoaderPriority.Default, progress, batchProgress, userData);
    }


    /// <summary>
    /// 实例化一个资源
    /// </summary>
    /// <param name="pathOrAddress">路径/地址</param>
    /// <param name="asset">资源/模板UnityObject</param>
    /// <returns></returns>
    public static UnityObject InstantiateAsset(string pathOrAddress, UnityObject asset)
    {
        return AssetMgr.GetInstance().InstantiateAsset( pathOrAddress,  asset);
    }




    /// <summary>
    /// 加载场景
    /// </summary>
    /// <param name="pathOrAddress">路径/地址</param>
    /// <param name="completeCallback">场景加载完成委托</param>
    /// <param name="progressCallback">场景加载进度委托</param>
    /// <param name="loadMode">加载方式</param>
    /// <param name="activateOnLoad">是否激活场景，（没有到这个参数呀）</param>
    /// <param name="userData">携带参数</param>asa
    /// <returns></returns>
    public static SceneLoaderHandle LoadSceneAsync(string pathOrAddress,
        OnSceneLoadComplete completeCallback,
        OnSceneLoadProgress progressCallback,
        LoadSceneMode loadMode = LoadSceneMode.Single,
        bool activateOnLoad = true,
        SystemObject userData = null)
    {
        return AssetMgr.GetInstance().LoadSceneAsync(pathOrAddress, completeCallback, progressCallback, loadMode, activateOnLoad, userData);
    }


    /// <summary>
    /// 卸载场景
    /// </summary>
    /// <param name="pathOrAddress">路径/地址</param>
    /// <param name="completeCallback">完成委托</param>
    /// <param name="progressCallback">进度委托</param>
    /// <param name="userData">携带参数</param>
    public static void UnloadSceneAsync(string pathOrAddress,
        OnSceneUnloadComplete completeCallback,
        OnSceneUnloadProgress progressCallback,
        SystemObject userData = null)
    {
        AssetMgr.GetInstance().UnloadSceneAsync(pathOrAddress, completeCallback, progressCallback, userData);
    }



    /// <summary>
    /// 卸载一个加载任务
    /// </summary>
    /// <param name="handle"></param>
    /// <param name="destroyIfLoaded"></param>
    public static void UnloadAssetLoader(AssetLoaderHandle handle, bool destroyIfLoaded = false)
    {
        AssetMgr.GetInstance().UnloadAssetLoader(handle, destroyIfLoaded);
    }

    /// <summary>
    /// 通过 label 标签，获取一组资源地址
    /// </summary>
    /// <param name="label"></param>
    /// <returns></returns>
    public static string[] GetAssetPathOrAddressByLabel(string label)
    {
        return AssetMgr.GetInstance().GetAssetPathOrAddressByLabel( label);
    }

#if UNITY_EDITOR
	/// <summary>
	/// 通过Address(就是资源的key)获取AssetPath(以"Assets/"开头的资源路径名)
	/// </summary>
	/// <param name="address"></param>
	/// <returns>如果不存在, 返回null</returns>
	public static string GetAssetPathByAddress(string address)
	{
		AssetAddressConfig config = UnityEditor.AssetDatabase.LoadAssetAtPath<AssetAddressConfig>(AssetAddressConfig.CONFIG_PATH);
		return config.GetAssetPathByAddress(address);
	}
#endif
}

