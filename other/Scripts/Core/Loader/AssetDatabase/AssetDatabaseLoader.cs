using Leyoutech.Core.Loader.Config;
using Leyoutech.Core.Pool;
using System.Collections.Generic;
using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace Leyoutech.Core.Loader
{
    public class AssetDatabaseLoader : AAssetLoader
    {
       /// <summary>
       /// 加载操作容器 《唯一ID ，操作列表》
       /// </summary>
        private Dictionary<long, List<AssetDatabaseAsyncOperation>> m_AsyncOperationDic = new Dictionary<long, List<AssetDatabaseAsyncOperation>>();


        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="assetRootDir"></param>
        protected override void InnerInitialize(string assetRootDir)
        {
#if UNITY_EDITOR
            if(m_PathMode == AssetPathMode.Address)
            {
                m_AssetAddressConfig = UnityEditor.AssetDatabase.LoadAssetAtPath<AssetAddressConfig>(AssetAddressConfig.CONFIG_PATH);
            }
#else
            Debug.LogError("");
#endif
        }

        /// <summary>
        /// updat 刷新初始化结果
        /// </summary>
        /// <param name="isSuccess">初始化成功/失败结构</param>
        /// <returns></returns>
        protected override bool UpdateInitialize(out bool isSuccess)
        {
            isSuccess = true;
            if(m_PathMode == AssetPathMode.Address && m_AssetAddressConfig == null)
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
            List<AssetDatabaseAsyncOperation> operationList = new List<AssetDatabaseAsyncOperation>();
            m_AsyncOperationDic.Add(loaderData.m_UniqueID, operationList);
            for (int i = 0; i < loaderData.m_AssetPaths.Length; ++i)
            {
                AssetDatabaseAsyncOperation operation = new AssetDatabaseAsyncOperation(loaderData.m_AssetPaths[i]);
                m_LoadingAsyncOperationList.Add(operation);
                operationList.Add(operation);
            }
        }
        
        /// <summary>
        /// 更新加载状态，进度等，并反馈此次加载任务的完成结果
        /// </summary>
        /// <param name="loaderData">加载任务数据</param>
        /// <returns></returns>
        protected override bool UpdateLoadingLoaderData(AssetLoaderData loaderData)
        {
            List<AssetDatabaseAsyncOperation> operationList = m_AsyncOperationDic[loaderData.m_UniqueID];
            bool isComplete = true;

            AssetLoaderHandle loaderHandle = null;
            if (m_LoaderHandleDic.ContainsKey(loaderData.m_UniqueID))
            {
                loaderHandle = m_LoaderHandleDic[loaderData.m_UniqueID];
            }

            for (int i = 0; i < loaderData.m_AssetPaths.Length; ++i)
            {
                if(loaderData.GetLoadState(i)) //加载完成了跳过
                {
                    continue;
                }
                string assetPath = loaderData.m_AssetPaths[i];
                AssetDatabaseAsyncOperation operation = operationList[i];

                if (operation.Status == AssetAsyncOperationStatus.Loaded) //操作状态为完成了
                {
                    UnityObject uObj = operation.GetAsset();

                    if(uObj == null)
                    {
                        //Debug.LogError($"AssetDatabaseLoader::UpdateLoadingLoaderData->asset is null.path = {assetPath}");
                        Debug.LogError($"加载完成，但资源UnityObject == null,   path = {assetPath}");
                    }

                    //实例化
                    if (uObj != null && loaderData.m_IsInstance)
                    {
                        uObj = UnityObject.Instantiate(uObj);
                    }

                    //保存Obj ，进度，状态，执行单资源回调
                    loaderHandle.SetObject(i, uObj);
                    loaderHandle.SetProgress(i, 1.0f);

                    loaderData.SetLoadState(i);
                    loaderData.InvokeComplete(i, uObj);
                }
                else if (operation.Status == AssetAsyncOperationStatus.Loading)   //加载中，未完成
                {
                    //跟新进度
                    float oldProgress = loaderHandle.GetProgress(i);
                    float curProgress = operation.Progress();
                    if (oldProgress != curProgress)
                    {
                        loaderHandle.SetProgress(i, curProgress);
                        loaderData.InvokeProgress(i, curProgress);
                    }
                    isComplete = false;
                }
                else
                {
                    isComplete = false;
                }
            }
            //更新全部进度
            loaderData.InvokeBatchProgress(loaderHandle.AssetProgresses);

            //全部完成
            if (isComplete)
            {
                loaderHandle.State = AssetLoaderState.Complete;
                loaderData.InvokeBatchComplete(loaderHandle.AssetObjects);
                m_AsyncOperationDic.Remove(loaderData.m_UniqueID);
            }
            return isComplete;
        }

        /// <summary>
        /// 停止指定资源的加载
        /// </summary>
        /// <param name="loaderData"></param>

        protected override void UnloadLoadingAssetLoader(AssetLoaderData loaderData)
        {
            List<AssetDatabaseAsyncOperation> operationList = m_AsyncOperationDic[loaderData.m_UniqueID];
            operationList.ForEach((operation) =>
            {
                m_LoadingAsyncOperationList.Remove(operation);//全局加载实施操作列表 ，移除本次加载任务的 所有操作Operation
            });
            m_AsyncOperationDic.Remove(loaderData.m_UniqueID);

            m_LoaderDataLoadingList.Remove(loaderData);
            m_LoaderDataPool.Release(loaderData);
        }
    }
}
