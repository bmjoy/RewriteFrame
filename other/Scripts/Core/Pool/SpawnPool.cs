using Leyoutech.Utility;
using System.Collections.Generic;
using UnityEngine;

namespace Leyoutech.Core.Pool
{
    /// <summary>
    /// 主要用于GameObjectPool的分组，可以根据使用场景进行添加和删除分组，不同的分组中可以有相同GameObject的缓存池
    /// </summary>
    public class SpawnPool
    {
        private const string LOG_TAG = "SpawnPool";

        private Dictionary<string, GameObjectPool> goPools = new Dictionary<string, GameObjectPool>();//存储缓存池
        internal Transform CachedTransform { get; private set; } = null;//为了方便管理，对于分组添加了Transform
        internal string PoolName { get; private set; } = string.Empty;//分组名称

        internal SpawnPool()
        {
        }
        /// <summary>
        /// 初始化分组
        /// </summary>
        /// <param name="pName">分组名称</param>
        /// <param name="parentTran">父结点</param>
        internal void InitSpawn(string pName, Transform parentTran)
        {
            PoolName = pName;

            CachedTransform = new GameObject($"Spawn_{PoolName}").transform;
            CachedTransform.SetParent(parentTran, false);
        }

        /// <summary>
        /// 判断分组中是否存在指定的缓存池
        /// </summary>
        /// <param name="assetPath"></param>
        /// <returns></returns>
        public bool HasGameObjectPool(string assetPath)
        {
            return goPools.ContainsKey(assetPath);
        }

        /// <summary>
        /// 缓存池中将默认以资源的路径为唯一标识，通过资源唯一标识可以获致到对应的缓存池
        /// </summary>
        /// <param name="assetPath"></param>
        /// <returns></returns>
        public GameObjectPool GetGameObjectPool(string assetPath)
        {
            if(goPools.TryGetValue(assetPath,out GameObjectPool goPool))
            {
                return goPool;
            }
            return null;
        }
        /// <summary>
        /// 使用给定的GameObject创建缓存池
        /// </summary>
        /// <param name="assetPath">资源唯一标签，一般使用资源路径</param>
        /// <param name="template">模板GameObject</param>
        /// <returns></returns>
        public GameObjectPool CreateGameObjectPool(string assetPath,GameObject template, PoolTemplateType templateType = PoolTemplateType.Prefab)
        {
            if(template == null)
            {
                DebugUtility.LogWarning(LOG_TAG, "SpawnPool::CreateGameObjectPool->Template Item is Null");
                return null;
            }
            if (goPools.TryGetValue(assetPath, out GameObjectPool goPool))
            {
                DebugUtility.LogWarning(LOG_TAG, "SpawnPool::CreateGameObjectPool->the asset pool has been created.assetPath = " + assetPath);
            }
            else
            {
                goPool = new GameObjectPool();
                goPool.InitPool(this, assetPath, template, templateType);

                goPools.Add(assetPath, goPool);
            }
           return goPool;
        }
        /// <summary>
        /// 删除指定的缓存池
        /// </summary>
        /// <param name="assetPath">资源唯一标签，一般使用资源路径</param>
        public void DeleteGameObjectPool(string assetPath)
        {
            GameObjectPool gObjPool = GetGameObjectPool(assetPath);
            if(gObjPool!=null)
            {
                gObjPool.DestroyPool();
                goPools.Remove(assetPath);
            }
        }
        /// <summary>
        /// 通过分组裁剪缓存池
        /// </summary>
        /// <param name="deltaTime"></param>
        internal void CullSpawn(float deltaTime)
        {
            foreach(var kvp in goPools)
            {
                if(kvp.Value.IsCull)
                {
                    kvp.Value.CullPool(deltaTime);
                }
            }
        }
        /// <summary>
        /// 销毁分组
        /// </summary>
        internal void DestroySpawn()
        {
            foreach(var kvp in goPools)
            {
                kvp.Value.DestroyPool();
            }
            goPools.Clear();
            Object.Destroy(CachedTransform.gameObject);
        }
    }
}
