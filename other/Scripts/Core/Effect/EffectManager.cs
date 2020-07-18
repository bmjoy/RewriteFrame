using Game.VFXController;
using Leyoutech.Core.Pool;
using Leyoutech.Core.Util;
using Leyoutech.Utility;
using System;
using UnityEngine;

namespace Leyoutech.Core.Effect
{
	/// <summary>
	/// 特效管理器. 提供特效的创建, 释放, 预加载
	/// </summary>
	public class EffectManager : Util.Singleton<EffectManager>
	{
		// 特效组. 对应特效池子的分组. 每一组池子可以设置最大活跃Object数量, 清理频率 等
		public readonly static string EFFECT_GROUP_UNDEFINED = "Effect Group Undefined";
		public readonly static string EFFECT_GROUP_UI = "Effect Group UI";
		public readonly static string EFFECT_GROUP_BATTLE_OTHER_PLAYER = "Effect Group Battle Other Player";
		public readonly static string EFFECT_GROUP_BATTLE_MAIN_PLAYER = "Effect Group Battle Main Player";
		public readonly static string EFFECT_GROUP_TIMELINE = "Effect Group Timeline";

		private readonly static string ROOT_NAME = "Effect Root";
		private readonly static string CONTROLLER_SPAWN_NAME = "EffectControllerSpawn";
		private readonly static string CONTROLLER_POOL_PATH = "effect_controller_virtual_path";

		private Transform rootTransform = null;
		private GameObjectPool effectControllerPool = null;

		public Action initFinishCallback;

		static public string GetEffectGroupNameInSpace(bool isMainPlayer)
		{
			return isMainPlayer ? EffectManager.EFFECT_GROUP_BATTLE_MAIN_PLAYER : EffectManager.EFFECT_GROUP_BATTLE_OTHER_PLAYER;
		}

		protected override void DoInit()
		{
			rootTransform = DontDestroyHandler.CreateTransform(ROOT_NAME);

			SpawnPool spawnPool = PoolManager.GetInstance().GetSpawnPool(CONTROLLER_SPAWN_NAME, true);

            effectControllerPool = spawnPool.CreateGameObjectPool(CONTROLLER_POOL_PATH, GetEffectControllerTemplate(),PoolTemplateType.RuntimeInstance);
            effectControllerPool.IsAutoClean = false;
            effectControllerPool.PreloadTotalAmount = 20;
            effectControllerPool.PreloadOnceAmount = 2;
            effectControllerPool.completeCallback = OnInitComplete;
        }

		/// <summary>
		/// 进行特效的预加载，并创建缓存池
		/// </summary>
		/// <param name="poolData"></param>
		//public void PreloadEffect(PoolData poolData)
		//{
		//	PoolManager.GetInstance().CreateGameObjectPool(poolData);
		//}

  //      public void PreloadEffect(string spawnName, string assetPath, int preloadCount, OnPoolPreloadComplete callback)
  //      {
  //          PoolData poolData = new PoolData()
  //          {
  //              spawnName = spawnName,
  //              assetPath = assetPath,
  //              preloadTotalAmount = preloadCount,
  //              preloadCompleteCallback = callback,
  //          };
  //          PreloadEffect(poolData);
  //      }

        /// <summary>
        /// 获取指定的特效，并创建其对应的缓存池, 特效释放后会被回收到池中
        /// </summary>
        /// <param name="spawnName"></param>
        /// <param name="assetPath"></param>
        /// <param name="isAutoRelease"></param>
        /// <returns></returns>
        public EffectController CreateEffect(string assetPath, string spawnName = "", OnEffectLoaded OnEffectLoaded = null, System.Object usedata = null)
		{
			EffectController effectController = effectControllerPool.GetComponentItem<EffectController>();
			effectController.CachedTransform.SetParent(rootTransform);
			effectController.CachedTransform.localPosition = Vector3.zero;
			effectController.CachedTransform.localRotation = Quaternion.identity;
			effectController.CachedTransform.localScale = Vector3.one;

			effectController.Initialize(assetPath, spawnName, OnEffectLoaded , usedata);

			return effectController;
		}

		public void OnDestroyEffect(EffectController effect)
		{
			effectControllerPool.RemoveItemFromUnusedList(effect.gameObject);
		}

		/// <summary>
		/// 获取指定的特效，并创建其对应的缓存池, 特效释放后会被回收到池中
		/// </summary>
		/// <param name="spawnName"></param>
		/// <param name="assetPath"></param>
		/// <param name="isAutoRelease"></param>
		/// <returns></returns>
		public EffectController CreateEffectByInstantiateAsset(string assetPath, 
			Transform parent, 
			Vector3 localPosition,
			Quaternion localRotation,
			Vector3 localScale,
			string spawnName = "")
		{
			EffectController effectController = effectControllerPool.GetComponentItem<EffectController>();
			effectController.CachedTransform.SetParent(parent, false);
			effectController.CachedTransform.localPosition = localPosition;
			effectController.CachedTransform.localRotation = localRotation;
			effectController.CachedTransform.localScale = localScale;

			effectController.InitializeByInstantiateAsset(assetPath);

			return effectController;
		}



		private void OnInitComplete(string spawnName, string assetPath)
		{
			initFinishCallback?.Invoke();
			initFinishCallback = null;
		}

		private GameObject GetEffectControllerTemplate()
		{
			GameObject templateGO = new GameObject("Effect Controller");
			templateGO.AddComponent<EffectController>();

			return templateGO;
		}
	}
}
