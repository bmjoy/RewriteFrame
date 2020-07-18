using Leyoutech.Core.Loader.Config;
using Leyoutech.Core.Pool;
using Leyoutech.Core.Timer;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace Leyoutech.Core.Loader
{
    public static class AssetBundleConst
    {
        /// <summary>
        /// 主AB信息文件名
        /// </summary>
        public static readonly string ASSETBUNDLE_MAINFEST_NAME = "assetbundles";
    }
    
    public class AssetBundleLoader : AAssetLoader
    {
        /// <summary>
        /// AssetNode 对象池
        /// </summary>
        private  ObjectPool<AssetNode> m_AssetNodePool = new ObjectPool<AssetNode>(50);

        /// <summary>
        /// bundlNode 对象池
        /// </summary>
        private  ObjectPool<BundleNode> m_BundleNodePool = new ObjectPool<BundleNode>(50);

        /// <summary>
        /// AssetNode 容器，《主资源路径，AssetNode》
        /// </summary>
        private Dictionary<string, AssetNode> m_AssetNodeDic = new Dictionary<string, AssetNode>();

        /// <summary>
        /// bundlNode 容器，《bundle路径，bundlNode》
        /// </summary>
        private Dictionary<string, BundleNode> m_BundleNodeDic = new Dictionary<string, BundleNode>();

        /// <summary>
        /// 正在执行加载操作容器 《bundle 路径，操作》
        /// </summary>
        private Dictionary<string, AssetBundleAsyncOperation> m_LoadingAsyncOperationDic = new Dictionary<string, AssetBundleAsyncOperation>();


        /// <summary>
        /// 定时器间隔时长30秒
        /// </summary>
        private float m_AssetCleanInterval = 120;

        /// <summary>
        /// 定时器
        /// </summary>
        private TimerTaskInfo m_AssetCleanTimer = null;
        
        /// <summary>
        /// AB 根路径
        /// </summary>
        private string m_AssetRootDir = "";

        /// <summary>
        /// 全局manifest
        /// </summary>
        private AssetBundleManifest m_AssetBundleManifest = null;


        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="rootDir">根路径</param>
        protected override void InnerInitialize(string rootDir)
        {
            m_AssetRootDir = rootDir;
            if(!string.IsNullOrEmpty(m_AssetRootDir) && !m_AssetRootDir.EndsWith("/"))
            {
                m_AssetRootDir += "/";
            }

            //开辟清除无效资源定时器
           m_AssetCleanTimer = TimerManager.GetInstance().AddIntervalTimer(m_AssetCleanInterval, this.OnCleanAssetInterval);

            //加载全局manifest
            string manifestPath = $"{this.m_AssetRootDir}/{AssetBundleConst.ASSETBUNDLE_MAINFEST_NAME}";
            AssetBundle manifestAB = AssetBundle.LoadFromFile(manifestPath);
            m_AssetBundleManifest = manifestAB.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
            // AB释放掉
            manifestAB.Unload(false);

            //加载资源打包配置数据
            string assetAddressConfigPath = $"{this.m_AssetRootDir}/{AssetAddressConfig.CONFIG_ASSET_BUNDLE_NAME}";
            AssetBundle assteAddressConfigAB = AssetBundle.LoadFromFile(assetAddressConfigPath);
            m_AssetAddressConfig = assteAddressConfigAB.LoadAsset<AssetAddressConfig>(AssetAddressConfig.CONFIG_PATH);
            assteAddressConfigAB.Unload(false);
        }


        /// <summary>
        /// updat 刷新初始化结果
        /// </summary>
        /// <param name="isSuccess">初始化成功/失败结构</param>
        /// <returns></returns>
        protected override bool UpdateInitialize(out bool isSuccess)
        {
            isSuccess = true;
            if(m_AssetBundleManifest == null)
            {
                isSuccess = false;
            }
            if(isSuccess && m_PathMode == AssetPathMode.Address && m_AssetAddressConfig == null)
            {
                isSuccess = false;
            }

            return true;
        }


        /// <summary>
        /// 设置加载操作数据,创建 Operation
        /// </summary>
        /// <param name="loaderData">加载任务数据</param>

        protected override void StartLoaderDataLoading(AssetLoaderData loaderData)
        {
            for (int i = 0; i < loaderData.m_AssetPaths.Length; ++i)
            {
                string assetPath = loaderData.m_AssetPaths[i];

                if(m_AssetNodeDic.TryGetValue(assetPath,out AssetNode assetNode))
                {
                    //已经有过了，计数+1
                    assetNode.RetainLoadCount();
                    continue;
                }

                string mainBundlePath = m_AssetAddressConfig.GetBundlePathByPath(assetPath);
                if(!m_BundleNodeDic.TryGetValue(mainBundlePath,out BundleNode bundleNode))
                {
                    bundleNode = CreateBundleNode(mainBundlePath);
                }
                //对应一个AssetNode 
                assetNode = m_AssetNodePool.Get();
                assetNode.InitNode(assetPath, bundleNode);
                assetNode.RetainLoadCount();                                                                            //计数  = 1

                m_AssetNodeDic.Add(assetPath, assetNode);
            }
        }

        /// <summary>
        /// 根据主Bundle，查找其依赖，并创建加载
        /// </summary>
        /// <param name="mainBundlePath">bundle 路径</param>
        /// <returns></returns>
        private BundleNode CreateBundleNode(string mainBundlePath)
        {
            if(!m_BundleNodeDic.TryGetValue(mainBundlePath,out BundleNode mainBundleNode))
            {
                // 创建异步操作
                CreateAsyncOperaton(mainBundlePath);

                mainBundleNode = m_BundleNodePool.Get();
                mainBundleNode.InitNode(mainBundlePath);

                m_BundleNodeDic.Add(mainBundlePath, mainBundleNode);
            }

            //关联bundle
            string[] dependBundlePaths = m_AssetBundleManifest.GetDirectDependencies(mainBundlePath);
            if (dependBundlePaths != null && dependBundlePaths.Length > 0)
            {
                foreach (var path in dependBundlePaths)
                {
                    if (!m_BundleNodeDic.TryGetValue(path, out BundleNode dependBundleNode))
                    {
                        dependBundleNode = CreateBundleNode(path);                                                                      //递归
                    }
                    mainBundleNode.AddDependNode(dependBundleNode);                   
                }
            }

            return mainBundleNode;
        }

        /// <summary>
        /// 根据AB的路径，创建AB的加载操作
        /// </summary>
        /// <param name="bundlePath"></param>
        private void CreateAsyncOperaton(string bundlePath)
        {
            AssetBundleAsyncOperation operation = new AssetBundleAsyncOperation(bundlePath, m_AssetRootDir);

            m_LoadingAsyncOperationList.Add(operation);
            m_LoadingAsyncOperationDic.Add(bundlePath,operation);
        }


        /// <summary>
        /// 单个 AB加载完成的回调
        /// 保存AB到BundleNode
        /// </summary>
        /// <param name="operation"></param>
        protected override void OnAsyncOperationLoaded(AAssetAsyncOperation operation)
        {
            BundleNode bundleNode = m_BundleNodeDic[operation.AssetPath];
            bundleNode.SetAssetBundle((operation.GetAsset() as AssetBundle));

            m_LoadingAsyncOperationDic.Remove(operation.AssetPath);
        }


        /// <summary>
        ///根据资源的路径，获取加载进度
        /// 返回主bundle,跟关联所有的bundle 平均进度
        /// </summary>
        /// <param name="assetPath">主 资源路径</param>
        /// <returns></returns>
        private float GetAssetLoadingProgress(string assetPath)
        {
            float progress = 0.0f;      //总进度
            int totalCount = 0;          //总数量
            string mainBundlePath = m_AssetAddressConfig.GetBundlePathByPath(assetPath);

            if (m_LoadingAsyncOperationDic.TryGetValue(mainBundlePath,out AssetBundleAsyncOperation mainOperation))
            {
                progress += mainOperation.Progress();
            }
            else
            {
                progress += 1.0f;
            }

            ++totalCount;

            //处理关联资源的进度
            string[] dependBundlePaths = m_AssetBundleManifest.GetAllDependencies(mainBundlePath);
            if(dependBundlePaths!=null && dependBundlePaths.Length>0)
            {
                foreach(var path in dependBundlePaths)
                {
                    if (m_LoadingAsyncOperationDic.TryGetValue(path, out AssetBundleAsyncOperation operation))
                    {
                        progress += operation.Progress();
                    }
                    else
                    {
                        progress += 1.0f;
                    }
                }
                totalCount += dependBundlePaths.Length;
            }
            return progress / totalCount;
        }


        /// <summary>
	/// 对于正在加载的请求，根据加载的情况更新其进度或者完成资源的加载
        /// 更新加载状态，进度等，并反馈此次加载任务的完成结果
        /// </summary>
        /// <param name="loaderData">加载任务数据</param>
        /// <returns></returns>
        protected override bool UpdateLoadingLoaderData(AssetLoaderData loaderData)
        {
            AssetLoaderHandle loaderHandle = null;
            if (m_LoaderHandleDic.ContainsKey(loaderData.m_UniqueID))//如果查找不到表示此次资源加载操作被取消了
            {
                loaderHandle = m_LoaderHandleDic[loaderData.m_UniqueID];
            }

            bool isComplete = true;   //加载是否完成
            for (int i = 0; i < loaderData.m_AssetPaths.Length; ++i)
            {
                if(loaderData.GetLoadState(i))//加载完成了跳过
                {
                    continue;
                }
                string assetPath = loaderData.m_AssetPaths[i];
                AssetNode assetNode = m_AssetNodeDic[assetPath];
                if(assetNode == null)
                {
                    //assetNode 存储值为空，直接此assetNode当做完成,为上面GetLoadState 跳过过滤
                    loaderData.SetLoadState(i); 
                    loaderData.InvokeComplete(i, null);
                    continue;
                }

                if(loaderHandle == null)
                {
                    //loaderHandle 值为空，assetNode完成,为上面GetLoadState 跳过过滤
                    if (assetNode.IsDone)
                    {
                        assetNode.ReleaseLoadCount();
                        loaderData.SetLoadState(i);  
                    }
                    else
                    {
                        isComplete = false;
                    }
                    continue;
                }


                if(assetNode.IsDone)//判断是否完成，如果完成了加载则进行相关的回调
                {
                    assetNode.ReleaseLoadCount();//计数-1
                    UnityObject uObj = null;
                    if(loaderData.m_IsInstance)
                    {
                        uObj = assetNode.GetInstance();
                    }
                    else
                    {
                        uObj = assetNode.GetAsset();
                    }

                    if (uObj == null)
                    {
                        Debug.LogError($"AssetBundleLoader::AssetLoadComplete->加载成功，但Obj 是空，path = {assetPath}");
                    }

                    //保存Obj ，进度，状态，执行单资源回调
                    loaderHandle.SetObject(i, uObj);
                    loaderHandle.SetProgress(i, 1.0f);

                    loaderData.SetLoadState(i);
                    loaderData.InvokeComplete(i, uObj);
                }
                else
                {
                    //加载中，未完成, 跟新进度
                    float progress = GetAssetLoadingProgress(assetPath);
                    float oldProgress = loaderHandle.GetProgress(i);
                    if (oldProgress != progress)
                    {
                        loaderHandle.SetProgress(i, progress);
                        loaderData.InvokeProgress(i, progress);
                    }
                    isComplete = false;
                }
            }
            if(loaderHandle!=null)
            {
                //更新全部进度
                loaderData.InvokeBatchProgress(loaderHandle.AssetProgresses);

                //全部完成
                if (isComplete)
                {
                    loaderHandle.State = AssetLoaderState.Complete;
                    loaderData.InvokeBatchComplete(loaderHandle.AssetObjects);
                }
            }
            
            return isComplete;
        }
        

        /// <summary>
        /// 设置定时器，定时检查资源并清理。由于Unity对资源的使用的方式问题，并不一定会清理掉资源。
        /// 某些资源只有调用Resources.UnloadUnusedAsset后，才会被清理掉
        /// </summary>
        /// <param name="userData"></param>
        private void OnCleanAssetInterval(System.Object userData)
        {
            InnerUnloadUnusedAssets();
        }


        /// <summary>
        /// 立即卸载掉指定的资源，只有确定资源不再使用时才可以使用此函数
        /// </summary>
        /// <param name="pathOrAddress"></param>
        public override void UnloadAsset(string pathOrAddress)
        {
            string assetPath = GetAssetPath(pathOrAddress);
            if(m_AssetNodeDic.TryGetValue(assetPath,out AssetNode assetNode))
            {
                if(assetNode.IsDone)
                {
                    m_AssetNodeDic.Remove(assetPath);
                    m_AssetNodePool.Release(assetNode);

                    InnerUnloadUnusedAssets();
                }
            }
        }

        /// <summary>
        ///  卸载，加载中“加载数据”执行删除
        /// </summary>
        /// <param name="loaderData"></param>
        protected override void UnloadLoadingAssetLoader(AssetLoaderData loaderData)
        {
        }

        /// <summary>
        /// 内部卸载无用资源函数
	/// 根据资源的记数查找可以释放的资源
        /// </summary>
        protected override void InnerUnloadUnusedAssets()
        {
            string[] assetNodeKeys = (from nodeKVP in m_AssetNodeDic where !nodeKVP.Value.IsAlive() select nodeKVP.Key).ToArray();
            foreach (var key in assetNodeKeys)
            {
                AssetNode assetNode = m_AssetNodeDic[key];
                m_AssetNodeDic.Remove(key);
                m_AssetNodePool.Release(assetNode);
            }

            string[] bundleNodeKeys = (from nodeKVP in m_BundleNodeDic where nodeKVP.Value.RefCount == 0 select nodeKVP.Key).ToArray();
            foreach (var key in bundleNodeKeys)
            {
                BundleNode bundleNode = m_BundleNodeDic[key];
                m_BundleNodeDic.Remove(key);
                m_BundleNodePool.Release(bundleNode);
            }
        }

        /// <summary>
        /// 实例化一个资源
	///对于使用Loader加载的资源需要通过此函数Instance对象，否则会造成资源无法回收
        /// </summary>
        /// <param name="assetPath"></param>
        /// <param name="asset"></param>
        /// <returns></returns>
        public override UnityObject InstantiateAsset(string assetPath, UnityObject asset)
        {
            if(m_PathMode == AssetPathMode.Address)
            {
                assetPath = m_AssetAddressConfig.GetAssetPathByAddress(assetPath);
            }
            if(m_AssetNodeDic.TryGetValue(assetPath,out AssetNode assetNode))
            {
                if(assetNode.IsDone)
                {
                    UnityObject instance = base.InstantiateAsset(assetPath, asset);
                    assetNode.AddInstance(instance);
                    return instance;
                }
            }
            return null;
        }
    }
}
