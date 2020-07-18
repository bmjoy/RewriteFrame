#if true
using DebugPanel;
using Leyoutech.Core.Loader;
using Leyoutech.Core.Pool;
using Leyoutech.Utility;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;
using SystemObject = System.Object;
using UnityObject = UnityEngine.Object;

namespace Map
{
    public class AreaController
    {
        #region 属性
        /// <summary>
        /// 要根据优先级进行排序，优先加载优先级高的
        /// </summary>
        public List<AreaLayerController> m_AreaLayerControllers;
        /// <summary>
        /// 这个区域的根节点
        /// </summary>
        private Transform m_AreaRoot;
        /// <summary>
        /// 对应的MapController
        /// </summary>
        [HideInInspector]
        private MapController m_Owner;
        /// <summary>
        /// 对应的Area数据
        /// </summary>
        private AreaInfo m_AreaInfo;
        /// <summary>
        /// 加载AreaDetailInfo的回调
        /// </summary>
        private AssetLoaderHandle m_AreaDetailLoaderHandle;
        /// <summary>
        /// Area的详细信息
        /// </summary>
        private AreaDetailInfo m_AreaDetailInfo;
        /// <summary>
        /// 是否初始化完成
        /// </summary>
        private bool m_IsInitialized;

        /// <summary>
        /// 位置检查时间(没必要每帧都去检查)
        /// </summary>
        private const float m_PositionCheckTime=0.4f;
        /// <summary>
        /// 上一次的检查时间
        /// </summary>
        private float m_LastCheckTime;

        private bool m_IsReleasing = false;
        #endregion

        public Transform GetRoot()
        {
            return m_AreaRoot;
        }

        public ulong GetAreaUid()
        {
            return m_AreaInfo.Uid;
        }

        public void RequestRelease()
        {
            if(m_AreaLayerControllers != null && m_AreaLayerControllers.Count>0)
            {
                for(int iLayer =0;iLayer<m_AreaLayerControllers.Count;iLayer++)
                {
                    m_AreaLayerControllers[iLayer].RequestRelease();
                }
            }
            m_IsReleasing = true;
        }

        public bool IsReleased()
        {
            if(!m_IsInitialized)
            {
                return false;
            }
            bool isReleased = true;
            if (m_AreaLayerControllers != null && m_AreaLayerControllers.Count > 0)
            {
                for (int iLayer = 0; iLayer < m_AreaLayerControllers.Count; iLayer++)
                {
                    isReleased = isReleased && m_AreaLayerControllers[iLayer].IsReleased();
                }

                if (isReleased)
                {
                    UnityObject.DestroyImmediate(m_AreaRoot.gameObject);
                    m_AreaRoot = null;
                }
            }
            else
            {
                if(m_IsReleasing)
                {
                    isReleased = true;
                }
            }
            if(isReleased)
            {
                m_IsReleasing = false;
            }
			if(m_IsReleasing)
			{
				DoUpdate_AreaRoot(false);
			}
            return isReleased;
        }
        

        public int GetInstantiatedUnitCount()
        {
            return 0;
        }

        internal void _DoGUI(Config config)
        {

        }

        public int GetAreaIndex()
        {
            return m_AreaInfo.Index;
        }

        public void RequestInitialize(MapController owner, AreaInfo areaInfo)
        {
            m_AreaRoot = new GameObject(Constants.AREA_GAMEOBJECT_NAME_STARTWITHS + areaInfo.Uid).transform;
            m_AreaRoot.SetPositionAndRotation(Vector3.zero, areaInfo.Rotation);
			m_IsReleasing = false;
			m_Owner = owner;
            m_AreaInfo = areaInfo;
			DoUpdate_AreaRoot(true);
			string areaDetailInfoAddressableKey = string.Format(Constants.AREA_DETAIL_INFO_FILENAME_FORMAT
                , owner.GetMapUid()
                , areaInfo.Uid);

            m_AreaDetailLoaderHandle = AssetUtil.LoadAssetAsync(areaDetailInfoAddressableKey, OnLoadAreaDetailInfoCompleted);
            
        }

		private void DoUpdate_AreaRoot(bool force)
		{
			if (force
				|| MapManager.GetInstance()._PlayerInfo.IsRealWorld2GameWorldChange)
			{
				if(m_AreaInfo != null)
				{
					Vector3 areaPosition = m_AreaInfo.Position + MapManager.GetInstance()._PlayerInfo.RealWorld2GameWorld;
					DebugUtility.Log(Constants.LOG_TAG, $"Map({m_Owner.GetMapUid()}) Area({m_AreaInfo.Uid}) Update AreaRoot({StringUtility.ConvertToDisplay(areaPosition)})");
					m_AreaRoot.position = areaPosition;
				}
			}
		}

		private void OnLoadAreaDetailInfoCompleted(string pathOrAddress, UnityObject obj, SystemObject userData)
        {
            if(obj == null)
            {
                DebugUtility.LogError(Constants.LOG_TAG, "AreaDetailInfo is null");
            }
            m_IsInitialized = true;
            if (m_IsReleasing)
            {
                if(m_AreaRoot != null)
                {
                    UnityObject.DestroyImmediate(m_AreaRoot.gameObject);
                    m_AreaRoot = null;
                }
                return;
            }
            TextAsset text = obj as TextAsset;
            m_AreaDetailInfo = new AreaDetailInfo();
            byte[] areaBytes = text.bytes;
            m_AreaDetailInfo.Deserialize(areaBytes);
            InitAreaLayer();
            ///一初始化完 就得去刷新地图
            DoCheckPlayerPos(true);
#if UNITY_EDITOR
            DebugAreaLayer debugLayer = m_AreaRoot.GetOrAddComponent<DebugAreaLayer>();
            debugLayer.m_AreaLayerInfos = m_AreaDetailInfo.AreaLayerInfos;
            debugLayer.m_AreaDetailInfo = m_AreaDetailInfo;
#endif
            
        }

        /// <summary>
        /// 初始化区域层
        /// </summary>
        private void InitAreaLayer()
        {
            if (m_AreaDetailInfo == null)
            {
                return;
            }
            if(m_AreaLayerControllers == null)
            {
                m_AreaLayerControllers = new List<AreaLayerController>();
            }
            AreaLayerInfo[] areaLayerInfos = m_AreaDetailInfo.AreaLayerInfos;
            if(areaLayerInfos != null && areaLayerInfos.Length>0)
            {
                for (int iLayer = 0; iLayer < areaLayerInfos.Length;iLayer++)
                {
                    AreaLayerInfo areaLayerInfo = areaLayerInfos[iLayer];
                    if(areaLayerInfo == null)
                    {
                        continue;
                    }
                    AreaLayerController areaLayerController = new AreaLayerController();
                    areaLayerController.Initialize(areaLayerInfo, m_AreaDetailInfo.AssetInfos,this, areaLayerInfo.m_Offest);
                    m_AreaLayerControllers.Add(areaLayerController);
                }
            }
        }

        public bool IsInitialized()
        {
            return m_IsInitialized;
        }

        public bool IsInitializedPlayNeedUnit()
        {
            bool isInitied = true;
            if (m_AreaLayerControllers != null && m_AreaLayerControllers.Count > 0)
            {
                for (int iLayer = 0; iLayer < m_AreaLayerControllers.Count; iLayer++)
                {
                    isInitied = isInitied&& m_AreaLayerControllers[iLayer].IsInitializedPlayNeedUnit();
                }
            }
            return isInitied;
        }

        public void DoUpdate()
        {
            if(!m_IsInitialized)
            {
				DoUpdate_AreaRoot(false);
				return;
            }
            DoCheckPlayerPos();
		}

        /// <summary>
        /// 检查玩家位置
        /// </summary>
        private void DoCheckPlayerPos(bool forceCheck = false)
        {
            if(m_AreaLayerControllers == null || m_AreaLayerControllers.Count<=0)
            {
                return;
            }
			DoUpdate_AreaRoot(false);
			bool canCheckPos = false;
            if(Time.time-m_LastCheckTime>=m_PositionCheckTime)
            {
                canCheckPos = true;
                m_LastCheckTime = Time.time;
            }
            Vector3 playerGameWorldPosition = MapManager.GetInstance()._PlayerInfo.GameWorldPosition;
            for (int iLayer = 0; iLayer < m_AreaLayerControllers.Count; iLayer++)
            {
                AreaLayerController areaLayerController = m_AreaLayerControllers[iLayer];
                if (canCheckPos)
                {
                    areaLayerController.DoUpdate(playerGameWorldPosition, forceCheck);
                }
                areaLayerController.DoUnitUpdate();
            }
		}
        
    }
}
#endif
