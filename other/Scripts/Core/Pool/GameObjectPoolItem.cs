using UnityEngine;

namespace Leyoutech.Core.Pool
{
    /// <summary>
    /// GameObject类型缓存池辅助类，可以用侦听对象从池中获取及回收到池的过程
    /// </summary>
    public class GameObjectPoolItem : MonoBehaviour
    {
        public string AssetPath { get; set; } = string.Empty;//唯一标识
        public string SpawnName { get; set; } = string.Empty;//所在的分组的名称

        private Transform cachedTransform = null;//缓存下来的Transfrom
        public Transform CachedTransform
        {
            get {
                if (cachedTransform == null)
                {
                    cachedTransform = transform;
                }
                return cachedTransform;
            }
        }

        private GameObject cachedGameObject = null;//缓存的GameObject
        public GameObject CachedGameObject
        {
            get
            {
                if(cachedGameObject == null)
                {
                    cachedGameObject = gameObject;
                }
                return cachedGameObject;
            }
        }

		public bool SafeDestroy = false;

		/// <summary>
		/// 当从池中获取时会被调用
		/// </summary>
		public virtual void DoSpawned()
        {
			SafeDestroy = false;
		}

        /// <summary>
        /// 回收到池中时会被调用 
        /// </summary>
        public virtual void DoDespawned()
        {

        }

        /// <summary>
        /// 可以通过此函数完成资源回收到池中
        /// </summary>
        public void ReleaseItem()
        {
            if (string.IsNullOrEmpty(AssetPath) || string.IsNullOrEmpty(SpawnName))
            {
                Destroy(CachedGameObject);
                return;
            }
            if (!PoolManager.GetInstance().HasSpawnPool(SpawnName))
            {
				SafeDestroy = true;

				Destroy(CachedGameObject);
                return;
            }
            SpawnPool spawnPool = PoolManager.GetInstance().GetSpawnPool(SpawnName);
            GameObjectPool gObjPool = spawnPool.GetGameObjectPool(AssetPath);
            if (gObjPool == null)
			{
				SafeDestroy = true;
				Destroy(CachedGameObject);
                return;
            }
            gObjPool.ReleasePoolItem(CachedGameObject);
        }
    }
}
