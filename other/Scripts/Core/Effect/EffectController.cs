using Leyoutech.Core.Pool;
using Leyoutech.Core.Timer;
using Game.VFXController;
using Leyoutech.Utility;
using UnityEngine;
using SystemObject = System.Object;
using UnityObject = UnityEngine.Object;
using UnityEditor;
using Leyoutech.Core.Loader;
using System.Threading;
using System.Collections.Generic;

namespace Leyoutech.Core.Effect
{
	public enum EffectStatus
	{
		None,
		/// <summary>
		/// 正在播放
		/// </summary>
		Playing,
		/// <summary>
		/// 已经调用了停止操作. 可能还有残余粒子
		/// </summary>
		Stop,
		/// <summary>
		/// 已经回收, 放入池子中
		/// </summary>
		Recycled,
	}

	public delegate void OnEffectFinish(EffectController effect);
	public delegate void OnEffectLoaded(EffectController effect,System.Object usedata);


	/// <summary>
	/// 代码中操作特效的类. 隐藏了加载特效的异步操作, 可以使用同步的方式操作特效
	/// 对特效的操作只有: PlayFX, StopFX, RecycleFX. (都加上FX俩字是为了搜索的时候好搜)
	/// 不处理Enable和Disable. 所有的播放和停止都由代码控制, 没有任何隐藏行为.
	/// </summary>
	public class EffectController : GameObjectPoolItem
	{
		public static Thread s_MainThread;

		public static void InitMainThread()
		{
			s_MainThread = System.Threading.Thread.CurrentThread;
		}

		public static bool IsMainThread()
		{
			return s_MainThread.Equals(System.Threading.Thread.CurrentThread);
		}

		private const string LOG_TAG = "EffectController";

		/// <summary>
		/// 资源加载操作的句柄
		/// </summary>
		AssetLoaderHandle m_AssetLoadHandler;
		// TODO, 谁会关心一个特效是不是已经播完了呢? 在停止的时候做可以吗?
		public event OnEffectFinish OnEffectFinished = delegate (EffectController e) { };

		private event OnEffectLoaded m_OnEffectLoaded = null;

		private EffectStatus m_Status = EffectStatus.None;
		private TimerTaskInfo m_Timer = null;

		/// <summary>
		/// 特效停止播放后是否自动销毁
		/// 两种情况下认为已经可以自动销毁了:
		/// 1. 没有活跃的粒子
		/// 2. 特效停止后经过的时间大于VFXController中设置的DestroyDelay
		/// </summary>
		private bool m_AutoDestroyWhenNotExistLivingParticles;

		/// <summary>
		/// 是不是为主角创建的特效. 震屏和后处理组件有 "只为主角播放这个效果" 的逻辑
		/// </summary>
		private bool m_ForMainPlayer = false;

		/// <summary>
		/// 开始播放的时间
		/// </summary>
		private float m_StartTime;

		/// <summary>
		/// 更新间隔. 单位: 帧
		/// 不用非得每帧去检查是不是要销毁特效, 省一点
		/// </summary>
		private const uint UPDATE_INTERVAL = 10;
		private uint m_UpdateIntervalCounter;

		/// <summary>
		/// 生命周期的倍率
		/// 比如使用激光模拟的机枪子弹特效, 根据目标距离自己的远近, 子弹需要飞行的时间不一样
		/// </summary>
		private float m_LifetimeScale;

		/// <summary>
		/// 真正的特效Object
		/// </summary>
		private VFXController m_VFXController;

		private string m_AssetName;

		// 特效实体未加载完成前对特效进行的操作, 需要先缓存, 在特效加载完成后重新进行这些操作
		// OpCache: 操作缓存
		#region VFXBeam.SetBeamTarget 操作缓存

		/// <summary>
		/// 是否缓存了 SetBeamTarget 的操作
		/// </summary>
		private bool m_OpCache_SetBeamTarget;
		private Transform m_OpCache_BeamStartTransform;
		private Transform m_OpCache_BeamTargetTransform;
		private Vector3 m_OpCache_BeamTargetOffset;
		private bool m_OpCache_UpdateHitPoint;
		private List<Collider> m_OpCache_TargetColliderList;
		private Vector3 m_OpCache_TargetPoint;

		#endregion

		public string GetVFXAssetName()
		{
			return m_AssetName;
		}

		/// <summary>
		/// 逻辑代码不允许调用这个接口
		/// </summary>
		/// <returns></returns>
		internal VFXController GetEffectObject()
		{
			return m_VFXController;
		}

		public EffectStatus GetStatus()
		{
			return m_Status;
		}

		/// <summary>
		/// 设置特效在播放完以后是否会自动回收
		/// </summary>
		/// <param name="autoDestroy">true: 播放完自动回收. false: 播放完特效还在</param>
		public void SetAutoRecycleWhenNotExistLivingParticles(bool autoDestroy)
		{
			m_AutoDestroyWhenNotExistLivingParticles = autoDestroy;
		}

		public void SetCreateForMainPlayer(bool forMainPlayer)
		{
			m_ForMainPlayer = forMainPlayer;
		}

		public void SetLifetimeScale(float scale)
		{
			m_LifetimeScale = scale;
		}

		public int m_EffectID;

		private static int m_GlobalEffectID;
		private static int EffectID()
		{
			return m_GlobalEffectID++;
		}

		class LoadData
		{
			public int effectID;
			public string assetName;
		}

		public string MyName;

        private System.Object usedata = null;

        /// <summary>
        /// 特效的初始化
        /// </summary>
        /// <param name="effectPath"></param>
        /// <param name="poolGroup"></param>
        public void Initialize(string effectPath, string poolGroup = "", OnEffectLoaded OnEffectLoaded = null , System.Object usedata = null)
		{
			m_OnEffectLoaded += OnEffectLoaded;
            this.usedata = usedata;
			m_AssetName = effectPath;
			m_Status = EffectStatus.Playing;
			m_EffectID = EffectID();

			MyName = "EffectController_" + m_AssetName + "_" + m_EffectID;
			name = MyName;

			LoadData d = new LoadData();
			d.effectID = m_EffectID;
			d.assetName = m_AssetName;
			if (!string.IsNullOrEmpty(m_AssetName) && effectPath != "None")
			{
				m_AssetLoadHandler = AssetUtil.LoadAssetAsync(effectPath, OnEffectLoadComplete, Loader.AssetLoaderPriority.Default, null, d);
			}
		}

		/// <summary>
		/// 使用实例化Prefab的方式创建特效. 用于特效浏览器
		/// </summary>
		/// <param name="effectPath"></param>
		public void InitializeByInstantiateAsset(string effectPath)
		{
#if UNITY_EDITOR
			m_AssetName = effectPath;
			m_Status = EffectStatus.Playing;

			if (!string.IsNullOrEmpty(m_AssetName) && effectPath != "None")
			{
				GameObject vfxAsset = AssetDatabase.LoadAssetAtPath<GameObject>(effectPath);
				OnEffectLoadComplete(effectPath, vfxAsset, m_AssetName);
			}
#endif
		}

		public void PlayFX()
		{
			if (m_Status == EffectStatus.Stop || m_Status == EffectStatus.None)
			{
				m_Status = EffectStatus.Playing;
				m_StartTime = Time.time;
				ClearTimer();

				if (m_VFXController != null)
				{
					m_VFXController.PlayFX(m_ForMainPlayer);
				}
			}
		}

		/// <summary>
		/// 停止播放特效, 但是特效不会自动销毁, 在手动调用 RecycleFX 或者 StopAndRecycleFX 之前, 特效会一直存在, 只是不播放了
		/// 这个函数的调用者, 应该负责特效的回收(RecycleFX 或者 StopAndRecycleFX)
		/// </summary>
		/// <param name="immediately"></param>
		public void StopFX(bool immediately = false)
		{
			if (m_Status == EffectStatus.Playing || m_Status == EffectStatus.None)
			{
				m_AutoDestroyWhenNotExistLivingParticles = false;

				m_Status = EffectStatus.Stop;

				if (m_VFXController != null)
				{
					m_VFXController.StopFX(immediately);
				}
			}
		}

		/// <summary>
		/// 停止播放特效并在播放停止后回收
		/// </summary>
		/// <param name="immediately"></param>
		public void StopAndRecycleFX(bool immediately = true)
		{
			StopFX(immediately);

			ClearTimer();

			m_AutoDestroyWhenNotExistLivingParticles = true;

			if (m_VFXController != null && m_VFXController.DestroyDelay > 0)
			{
				m_Timer = TimerManager.GetInstance().AddEndTimer(m_VFXController.DestroyDelay, TimeToAutoDestroy);
			}
			else
			{
				RecycleFX();
			}
		}

		/// <summary>
		/// 直接回收特效
		/// 会立即停止当前特效的播放， 并把特效回收，放入池子中。
		/// 调用这个方法后，外部对于特效的引用（EffectController）将不再可用。所以这个方法调用后需要将特效的引用赋空
		/// </summary>
		public void RecycleFX()
		{
			if (m_DestroyedUnsafe)
			{
				return;
			}

			if (m_Status == EffectStatus.Recycled)
				return;

			if (m_AssetLoadHandler != null && m_VFXController == null)
				AssetUtil.UnloadAssetLoader(m_AssetLoadHandler);

			StopFX(true);

			OnEffectFinished?.Invoke(this);
			OnEffectFinished = delegate (EffectController e)
			{
			};

			ClearTimer();
			m_Status = EffectStatus.Recycled;
			
			// 暂时使用老的缓存池。 等李玉斌移植完毕
			//effect.GetEffectObject()?.ReleaseItem();
			if (m_VFXController != null)
			{
				m_VFXController.RecycleFX();
				m_VFXController.transform.SetParent(null, false);
				m_VFXController.Recycle();
				m_VFXController = null;
			}
			else
			{
				DebugUtility.LogFormat("EffectBug", "null VFXController. name: ", GetVFXAssetName());
			}

			ReleaseItem();
		}

		/// <summary>
		/// 如果此特效是一个射线特效, 设置射线的目标
		/// 如果特效不是射线, 则不会有任何效果
		/// </summary>
		/// <param name="targetTransform">目标Transform, 射线以此为结束点</param>
		/// <param name="offset">射线结束点相对于targetTransform的偏移</param>
		/// <param name="updateHitPoint">是否根据目标Collider的形状每帧更新射线弹着点</param>
		/// <param name="targetColliderList">只有需要每帧更新弹着点的时候才需要设置这个参数。 目标拥有的所有Collider</param>
		public void SetBeamTarget(Transform startTransform, Transform targetTransform, Vector3 offset, bool updateHitPoint = false, List<Collider> targetColliderList = null)
		{
			if (m_VFXController == null)
			{
				m_OpCache_SetBeamTarget = true;
				m_OpCache_BeamStartTransform = startTransform;
				m_OpCache_BeamTargetTransform = targetTransform;
				m_OpCache_BeamTargetOffset = offset;
				m_OpCache_UpdateHitPoint = updateHitPoint;
				m_OpCache_TargetColliderList = targetColliderList;
				m_OpCache_TargetPoint = targetTransform.position + offset;
			}
			else
			{
				m_OpCache_SetBeamTarget = false;

				VFXBeam beam = m_VFXController.GetComponent<VFXBeam>();
				if (beam != null)
				{
					beam.SetBeamTarget(transform, targetTransform
										, offset, updateHitPoint, m_OpCache_TargetColliderList);
				}
			}
		}

		/// <summary>
		/// 如果此特效是一个射线特效, 设置射线的目标点
		/// 如果特效不是射线, 则不会有任何效果
		/// </summary>
		/// <param name="targetPoint"></param>
		public void SetBeamTarget(Transform startTransform, Vector3 targetPoint)
		{
			if (m_VFXController == null)
			{
				m_OpCache_SetBeamTarget = true;
				m_OpCache_BeamStartTransform = startTransform;
				m_OpCache_TargetPoint = targetPoint;
			}
			else
			{
				m_OpCache_SetBeamTarget = false;

				VFXBeam beam = m_VFXController.GetComponent<VFXBeam>();
				if (beam != null)
				{
					beam.SetBeamTarget(transform, targetPoint);
				}
			}
		}

		public override void DoSpawned()
		{
			base.DoSpawned();
			_Reset();
		}

		/// <summary>
		/// 重播缓存的操作
		/// </summary>
		private void ReplayOperationsCached()
		{
			if (m_VFXController == null)
				return;

			if (m_OpCache_SetBeamTarget)
			{
				VFXBeam beam = m_VFXController.GetComponent<VFXBeam>();
				if (beam != null)
				{
					if (m_OpCache_BeamTargetTransform)
					{
						beam.SetBeamTarget(m_OpCache_BeamStartTransform, m_OpCache_BeamTargetTransform
							, m_OpCache_BeamTargetOffset, m_OpCache_UpdateHitPoint, m_OpCache_TargetColliderList);
					}
					else
					{
						beam.SetBeamTarget(m_OpCache_BeamStartTransform, m_OpCache_TargetPoint);
					}
				}
			}
		}

		private void Update()
		{
			if (string.IsNullOrEmpty(m_AssetName) || m_AssetName == "None")
			{
				RecycleFX();
				return;
			}

			if (m_VFXController != null)
			{
				m_VFXController.OnUpdate(Time.deltaTime);

				if (m_Status == EffectStatus.Playing)
				{
					if (m_VFXController.AutoStop && Time.time > m_StartTime + m_VFXController.StopDelay * m_LifetimeScale)
					{
						StopAndRecycleFX();
					}
				}

				if (m_Status == EffectStatus.Stop)
				{
					if (++m_UpdateIntervalCounter % UPDATE_INTERVAL == 0)
					{
						if (m_AutoDestroyWhenNotExistLivingParticles
							&& !m_VFXController.HaveLivingParticles())
						{
							RecycleFX();
						}
					}
				}
			}
		}

		private void LateUpdate()
		{
			if (m_VFXController != null)
			{
				m_VFXController.OnLateUpdate(Time.deltaTime);
			}
		}

		private bool m_DestroyedUnsafe;

		private void OnDestroy()
		{
			if (!SafeDestroy)
			{
				m_DestroyedUnsafe = true;
				//Debug.LogErrorFormat("EffectController 不该被直接 Destroy. {0}", name);
			}

            if (m_AssetLoadHandler != null && m_VFXController == null)
			{
                AssetUtil.UnloadAssetLoader(m_AssetLoadHandler);
				m_AssetLoadHandler = null;
			}

            ClearTimer();
			EffectManager.GetInstance().OnDestroyEffect(this);
		}

		//private void OnEffectLoadComplete(string effectPath, UnityObject unityObject, SystemObject userData)
		//{
		//	effectAssetHandle = null;

		//	GameObject effectGO = (GameObject)unityObject;
		//	string poolGroup = userData as string;

		//	// 读取完的时候, 特效已经销毁了
		//	if (m_Status == EffectStatus.Recycled)
		//	{
		//		RecycleFX();
		//		return;
		//	}
		//	// 读取资源错误
		//	if (unityObject == null)
		//	{
		//		DebugLogger.LogError($"EffectController::OnEffectLoadComplete. 读取资源失败: {effectPath}");
		//		RecycleFX();
		//		return;
		//	}
		//	// 指定了池子组的名字, 但是找不到池子组
		//	if (!string.IsNullOrEmpty(poolGroup) && !PoolManager.GetInstance().HasSpawnPool(poolGroup))
		//	{
		//		DebugLogger.LogError($"EffectController::OnEffectLoadComplete. 找不到SpawnPool. SpawnPoolName: {poolGroup}. effectPath: {effectPath}");
		//		RecycleFX();
		//		return;
		//	}

		//	// 没挂脚本, 就不自动挂了, 省的老是忽略这个错误
		//	VFXController vfx = effectGO.GetComponent<VFXController>();
		//	if (vfx == null)
		//	{
		//		DebugLogger.LogError($"EffectController::OnEffectLoadComplete. 特效没有正确地挂脚本: {effectPath}");
		//		RecycleFX();
		//		return;
		//	}

		//	if (string.IsNullOrEmpty(poolGroup))
		//	{
		//		_SetEffectAndUpdateEffectStatus(vfx);
		//	}
		//	else
		//	{
		//		SpawnPool spawnPool = PoolManager.GetInstance().GetSpawnPool(poolGroup);
		//		GameObjectPool objPool = spawnPool.GetGameObjectPool(effectPath);
		//		if (objPool == null)
		//		{
		//			GameObject template = Instantiate(effectGO);
		//			objPool = spawnPool.CreateGameObjectPool(effectPath, template);
		//		}

		//		_SetEffectAndUpdateEffectStatus(objPool.GetComponentItem<VFXController>());
		//	}
		//}

		void OnEffectLoadComplete(string pathOrAddress, UnityObject returnObject, SystemObject userData)
		{
            LoadData d = userData as LoadData;

			if (!m_AssetName.Equals(d.assetName))
			{
				// 可能有这种情况:
				// A特效 请求加载. 没加载完的时候A特效就回收了. B 特效使用了同一个EffectController, 也请求加载. 这时候A刚加载完
				return;
			}

			if (!m_EffectID.Equals(d.effectID))
			{
				return;
			}

			if (returnObject == null)
			{
				return;
			}

			m_AssetLoadHandler = null;

			GameObject effectGO = (GameObject)returnObject;

			// 读取完的时候, 特效已经销毁了
			if (m_Status == EffectStatus.Recycled)
			{
				RecycleFX();
				return;
			}

			// 读取资源错误
			if (effectGO == null)
			{
				DebugUtility.LogError(LOG_TAG, $"EffectController::OnEffectLoadComplete. 读取资源失败: {m_AssetName}");
				RecycleFX();
				return;
			}

			// 没挂脚本, 就不自动挂了, 省的老是忽略这个错误
			VFXController vfx = effectGO.GetComponent<VFXController>();
			if (vfx == null)
			{
				DebugUtility.LogError(LOG_TAG, $"EffectController::OnEffectLoadComplete. 特效没有正确地挂脚本: {m_AssetName}");
				RecycleFX();
				return;
			}

			if (!effectGO.IsPooled())
			{
				effectGO.CreatePool(1, pathOrAddress);
			}

			VFXController vfxInstance = vfx.Spawn();
			vfxInstance.DoSpawned();
			_SetEffectAndUpdateEffectStatus(vfxInstance);
		}

		/// <summary>
		/// 播放停止后自动强制销毁的计时器回调
		/// </summary>
		/// <param name="data"></param>
		private void TimeToAutoDestroy(SystemObject data)
		{
			RecycleFX();
		}

		/// <summary>
		/// 加载完成后设置特效Object, 并把当前的特效状态更新给特效Object
		/// 这个函数设置为public是为了给特效浏览器使用 (SkillEffectBrowser). 
		/// 除此之外, 外部不应该调用这个方法
		/// </summary>
		/// <param name="effect"></param>
		public void _SetEffectAndUpdateEffectStatus(VFXController effect)
		{
			m_VFXController = effect;
			m_VFXController.CachedTransform.SetParent(CachedTransform, false);
			m_VFXController.SetEffectController(this);

			ReplayOperationsCached();

			if (m_Status == EffectStatus.Playing)
			{
				m_StartTime = Time.time;
				m_VFXController.PlayFX(m_ForMainPlayer);

				LayerUtil.SetGameObjectToLayer(gameObject, GameConstant.LayerTypeID.TransparentFX, true);
			}
			else if (m_Status == EffectStatus.Stop)
			{
				ClearTimer();
				m_VFXController.StopFX(true);
			}
			else if (m_Status == EffectStatus.Recycled)
			{
                // OnEffectLoadComplete里已经处理了
                //	m_VFXController.StopFX(true);
                //	m_VFXController.ReleaseItem();
            }

            m_OnEffectLoaded?.Invoke(this, usedata);
        }

		private void ClearTimer()
		{
			if (m_Timer != null)
			{
				TimerManager.GetInstance().RemoveTimer(m_Timer);
				m_Timer = null;
			}
		}

		/// <summary>
		/// 这个函数设置为public是为了给特效浏览器使用 (SkillEffectBrowser). 
		/// 除此之外, 外部不应该调用这个方法
		/// </summary>
		public void _Reset()
		{
			ClearTimer();

			m_AssetName = "";
			m_Status = EffectStatus.None;
			m_VFXController = null;
			OnEffectFinished = null;
			m_OnEffectLoaded = null;
			m_LifetimeScale = 1;
			m_AutoDestroyWhenNotExistLivingParticles = true;
			m_ForMainPlayer = false;
			m_StartTime = 0f;
			m_UpdateIntervalCounter = 0;
			m_VFXController = null;

			m_OpCache_SetBeamTarget = false;
			m_OpCache_BeamTargetTransform = null;
			m_OpCache_BeamTargetOffset = Vector3.zero;
			m_OpCache_UpdateHitPoint = false;
			m_OpCache_TargetColliderList = null;
			m_OpCache_TargetPoint = Vector3.zero;

            usedata = null;
		}
	}
}
