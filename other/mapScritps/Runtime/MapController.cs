using System;
using System.Collections.Generic;
using Leyoutech.Utility;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Map
{
    [System.Serializable]
    public class MapController
    {
        /// <summary>
        /// map的Uid
        /// </summary>
        private uint m_MapUid;
        /// <summary>
        /// 当前的状态
        /// </summary>
        private State m_State;
        /// <summary>
        /// 当前的Map
        /// </summary>
        private MapInfo m_MapInfo;
        /// <summary>
        /// 当前Map用的Scene
        /// </summary>
        private Scene m_MapScene;
        /// <summary>
        /// 玩家当前所在的区域
        /// </summary>
        private AreaController m_CurrentAreaController;
        /// <summary>
        /// 过期的Area
        /// </summary>
        private BetterList<AreaController> m_ExpiredAreaControllers;
        /// <summary>
        /// <see cref="ForReadyToPlayCallback"/>
        /// </summary>
        private ForReadyToPlayCallback m_ForReadyToPlayCallback;
        /// <summary>
        /// <see cref="EnableOriginPointOffset"/>
        /// </summary>
        private List<EnableOriginPointOffset> m_NeedOriginPointOffsets;
        /// <summary>
        /// <see cref="ForcedLockingDistanceToPlayer"/>
        /// </summary>
        private List<ForcedLockingDistanceToPlayer> m_NeedForcedLockingDistanceToPlayers;
        /// <summary>
        /// 限制只能切换到区域
        /// <see cref="Constants.NOT_LIMIT_AREA_UID"/>不限制
        /// </summary>
        private ulong m_LimitChangeToAreaUid = Constants.NOT_LIMIT_AREA_UID;

        public void RequestInitialize(uint mapUid, System.Action OnReadyToPlayCallback = null)
        {
            DebugUtility.Log(Constants.LOG_TAG, string.Format("Map({0}) Request Initialize", mapUid));

            m_MapUid = mapUid;
            m_ForReadyToPlayCallback = new ForReadyToPlayCallback();
            m_ForReadyToPlayCallback.OnReadyToPlayCallback = OnReadyToPlayCallback;

            m_State = State.LoadingMap;

            m_ExpiredAreaControllers = new BetterList<AreaController>(4);

            // 加载新Map
            AssetUtil.LoadAssetAsync(Constants.MAP_INFO_ADDRESSKEY_STARTWITHS + mapUid, OnLoadMapInfoCompleted);
        }

        /// <summary>
        /// 如果当前没有Area，则返回<see cref="Constants.NOTSET_AREA_UID"/>
        /// </summary>
        public ulong GetCurrentAreaUid()
        {
            return m_CurrentAreaController != null
                ? m_CurrentAreaController.GetAreaUid()
                : Constants.NOTSET_AREA_UID;
        }

        /// <summary>
        /// Release current area
        /// </summary>
        public void RequestReleaseCurrentArea()
        {
            if (m_CurrentAreaController != null)
            {
                m_ExpiredAreaControllers.Add(m_CurrentAreaController);
                m_CurrentAreaController.RequestRelease();
                m_CurrentAreaController = null;
            }
        }

        public AreaInfo FindAreaInfoByUid(ulong areaUid)
        {
            return m_MapInfo == null
                ? null
                : m_MapInfo.FindAreaInfoByUid(areaUid);
        }

        /// <summary>
        /// Release current map
        /// </summary>
        public void RequestRelease()
        {
            DebugUtility.Log(Constants.LOG_TAG, string.Format("Map({0}) Request Release", m_MapUid));

            m_State = State.ReleasingAreas;
            RequestReleaseCurrentArea();
            m_ForReadyToPlayCallback = null;
        }

        public State GetState()
        {
            return m_State;
        }

        public uint GetMapUid()
        {
            return m_MapUid;
        }

        public void DoUpdate()
        {
            DebugUtility.LogVerbose(Constants.LOG_TAG, $"Map({m_MapUid}) DoUpdate State({m_State})");

            switch (m_State)
            {
                case State.Normal:
                    // 要放在DoUpdate_ExpiredArea前面，因为当前的Area可能是Expired
                    DoUpdate_Area();
                    DoUpdate_ExpiredArea();
                    DoUpdate_ReadyToPlayCallback();
                    DoUpdate_NeedOriginPointOffset();
                    DoUpdate_ForcedLockingDistanceToPlayer();
                    break;
                case State.LoadingMap:
                    if (!m_MapScene.IsValid())
                    {
                        break;
                    }

					DoUpdate_ExpiredArea();
					DoUpdate_NeedOriginPointOffset();
					DoUpdate_ForcedLockingDistanceToPlayer();

					if (m_CurrentAreaController == null // 玩家当前不在任何区域
						|| m_CurrentAreaController.IsInitialized())// 当前所在区域已经初始化完成
					{
						m_State = State.Normal;
					}
					else
					{
						m_CurrentAreaController.DoUpdate();
					}
					break;
                case State.ChangingArea:
                    DoUpdate_Area();
					DoUpdate_ExpiredArea();
					DoUpdate_ReadyToPlayCallback();
                    DoUpdate_NeedOriginPointOffset();
                    DoUpdate_ForcedLockingDistanceToPlayer();

					if (m_CurrentAreaController != null
						&& m_CurrentAreaController.IsInitialized())
					{
						m_State = State.Normal;
					}
					break;
                case State.ReleasingAreas:
                    DoUpdate_ExpiredArea();
                    DoUpdate_NeedOriginPointOffset();
                    DoUpdate_ForcedLockingDistanceToPlayer();

                    if (m_ExpiredAreaControllers.Count == 0)
                    {
                        m_State = State.ReleasingMap;

                        m_ExpiredAreaControllers.Clear();
                        m_ExpiredAreaControllers = null;

                        if (m_MapScene.IsValid())
                        {
                            m_NeedOriginPointOffsets.Clear();
                            m_NeedOriginPointOffsets = null;
                            m_NeedForcedLockingDistanceToPlayers.Clear();
                            m_NeedForcedLockingDistanceToPlayers = null;
                            AssetUtil.UnloadSceneAsync(m_MapInfo.SceneAddressableKey, OnUnloadMapSceneCompleted, null);
                        }
                    }
                    break;
                case State.ReleasingMap:
                case State.Released:
                    // 不需要处理
                    break;
                default:
                    DebugUtility.Assert(false, "意外的State: " + m_State);
                    break;
            }
        }

        public bool IsReleasing()
        {
            return m_State == State.ReleasingAreas || m_State == State.ReleasingMap;
        }

        /// <summary>
        /// <see cref="m_LimitChangeToAreaUid"/>
        /// </summary>
        public void SetLimitChangeToAreaUid(ulong areaUid = Constants.NOT_LIMIT_AREA_UID)
        {
            DebugUtility.Log(Constants.LOG_TAG, $"Set limit change to area uid({areaUid})");
            m_LimitChangeToAreaUid = areaUid;
        }

        private void OnUnloadMapSceneCompleted(string pathOrAddress, object userData)
        {
            DebugUtility.Log(Constants.LOG_TAG, string.Format("Map({0}) Unloaded Map Scene", m_MapUid));
            m_State = State.Released;
        }

        /// <summary>
        /// <see cref="ForReadyToPlayCallback"/>
        /// </summary>
        private void DoUpdate_ReadyToPlayCallback()
        {
            if (m_ForReadyToPlayCallback != null
                && m_ForReadyToPlayCallback.StartArea!= null &&
                (m_ForReadyToPlayCallback.StartArea != m_CurrentAreaController
                    || m_ForReadyToPlayCallback.StartArea.IsInitializedPlayNeedUnit()))
            {
                
                InvokeReadyToPlayCallback();
            }
        }

        private void InvokeReadyToPlayCallback()
        {
            DebugUtility.Log(Constants.LOG_TAG, string.Format("Map({0}) Ready To Play", m_MapUid));
            //Debug.LogError("InvokeReadyToPlayCallback");
            try
            {
                DebugUtility.Log(Constants.LOG_TAG, "Begin Invoke OnReadyToPlayCallback");
                m_ForReadyToPlayCallback.OnReadyToPlayCallback?.Invoke();
                DebugUtility.Log(Constants.LOG_TAG, "End Invoke OnReadyToPlayCallback");
            }
            catch (System.Exception e)
            {
                DebugUtility.LogError(Constants.LOG_TAG, string.Format("Invoke OnReadyToPlayCallback Exception:\n{0}", e.ToString()));
            }
            finally
            {
                m_ForReadyToPlayCallback = null;
            }

            try
            {
                DebugUtility.Log(Constants.LOG_TAG, "Begin Invoke _OnChangedMap");
                MapManager.GetInstance()._OnChangedMap?.Invoke(m_MapUid);
                DebugUtility.Log(Constants.LOG_TAG, "End Invoke _OnChangedMap");
            }
            catch (System.Exception e)
            {
                DebugUtility.LogError(Constants.LOG_TAG, string.Format("Invoke _OnChangedMap Exception:\n{0}", e.ToString()));
            }
        }

        /// <summary>
        /// 更新已经过期的Area，流程
        ///		等待已经实例化过程中的Unit实例化完
        ///		这些Area内的Unit会每帧销毁一些
        ///		等到所有Unit销毁完成后，<see cref="AreaController"/>的引用会被释放
        /// </summary>
        private void DoUpdate_ExpiredArea()
        {
            for (int iArea = m_ExpiredAreaControllers.Count - 1; iArea >= 0; iArea--)
            {
                AreaController iterAreaController = m_ExpiredAreaControllers[iArea];
                iterAreaController.DoUpdate();
                if (iterAreaController.IsReleased())
                {
                    try
                    {
                        DebugUtility.Log(Constants.LOG_TAG, $"Begin Invoke OnReleasedArea({iterAreaController.GetAreaUid()})");
                        MapManager.GetInstance()._OnReleasedArea?.Invoke(iterAreaController.GetAreaUid());
                        DebugUtility.Log(Constants.LOG_TAG, $"End Invoke OnReleasedArea({iterAreaController.GetAreaUid()})");
                    }
                    catch (Exception e)
                    {
                        DebugUtility.LogError(Constants.LOG_TAG, string.Format("Invoke OnReleasedArea Exception:\n{0}", e.ToString()));
                    }

                    m_ExpiredAreaControllers.RemoveAt(iArea);
                }
            }
        }

        private void DoUpdate_ForcedLockingDistanceToPlayer(bool force = false)
        {
            if (m_NeedForcedLockingDistanceToPlayers == null)
            {
                return;
            }

            if (force
                || MapManager.GetInstance()._PlayerInfo.IsGameWorldChange)
            {
                DebugUtility.LogVerbose(Constants.LOG_TAG, $"Map({m_MapUid}) Update ForcedLockingDistanceToPlayer");
                Vector3 playerGameWorldPosition = MapManager.GetInstance()._PlayerInfo.GameWorldPosition;
                for (int iGameObject = 0; iGameObject < m_NeedForcedLockingDistanceToPlayers.Count; iGameObject++)
                {
                    m_NeedForcedLockingDistanceToPlayers[iGameObject].transform.localPosition = playerGameWorldPosition
                        + m_NeedForcedLockingDistanceToPlayers[iGameObject]._DistanceToPlayer;
                }
            }
        }

        private void DoUpdate_NeedOriginPointOffset(bool force = false) 
        {
            if (m_NeedOriginPointOffsets == null)
            {
                return;
            }

            if (force
                || MapManager.GetInstance()._PlayerInfo.IsRealWorld2GameWorldChange)
            {
                DebugUtility.LogVerbose(Constants.LOG_TAG, $"Map({m_MapUid}) Update NeedOriginPointOffset");

                Vector3 realWorld2GameWorld = MapManager.GetInstance()._PlayerInfo.RealWorld2GameWorld;

                for (int iGameObject = 0; iGameObject < m_NeedOriginPointOffsets.Count; iGameObject++)
                {
                    Vector3 newPosition = m_NeedOriginPointOffsets[iGameObject]._RealWorldPostion
                        + realWorld2GameWorld;

                    m_NeedOriginPointOffsets[iGameObject].transform.localPosition = newPosition;
                }
            }
        }

        /// <summary>
        /// 检测是否需要切换区域
        ///		true：切换区域
        ///		false：更新当前区域
        /// </summary>
        /// <param name="firstUpdate">true: 加载Map后第一次更新Area</param>
        private void DoUpdate_Area(bool firstUpdate = false)
        {
            // 玩家当前所在区域
            int areaIndex = m_MapInfo.CaculateAreaIndex(MapManager.GetInstance()._PlayerInfo.RealWorldPosition);

            // 玩家切换区域了
            if (m_CurrentAreaController == null
                || m_CurrentAreaController.GetAreaIndex() != areaIndex)
            {
                // UNDOEN 切换Voxel需要加个延迟，以防玩家在Voxel的边界来回摩擦时频繁切换Voxel
                TryChangeArea(areaIndex);
            }
            else
            { 
                m_CurrentAreaController.DoUpdate();
            }

            if (firstUpdate)
            {
                if (m_CurrentAreaController != null)
                {
                    m_ForReadyToPlayCallback.StartArea = m_CurrentAreaController;
                }
                else
                {
                    InvokeReadyToPlayCallback();
                }
            }
        }

        /// <summary>
        /// 切换Area
        /// </summary>
        private void TryChangeArea(int areaIndex)
        {
            if (m_CurrentAreaController != null)
            {
                ulong lastAreaUid = GetCurrentAreaUid();
                ulong newAreaUid = areaIndex != Constants.NOTSET_AREA_INDEX
                    ? m_MapInfo.AreaInfos[areaIndex].Uid
                    : Constants.NOTSET_AREA_UID;
                if (lastAreaUid != newAreaUid)
                {
                    DebugUtility.Log(Constants.LOG_TAG, $"Begin change area from {lastAreaUid} to {newAreaUid}, {MapManager.GetInstance()._PlayerInfo}");
                }
            }

            if (m_CurrentAreaController != null)
            {
                m_CurrentAreaController.RequestRelease();
                m_ExpiredAreaControllers.Add(m_CurrentAreaController);
                m_CurrentAreaController = null;
            }

            if (areaIndex != Constants.NOTSET_AREA_INDEX)
            {
                AreaInfo targetAraeInfo = m_MapInfo.AreaInfos[areaIndex];
                if (m_LimitChangeToAreaUid == Constants.NOT_LIMIT_AREA_UID
                    || m_LimitChangeToAreaUid == targetAraeInfo.Uid)
                {
                    m_State = State.ChangingArea;

                    m_CurrentAreaController = new AreaController();
                    m_CurrentAreaController.RequestInitialize(this, m_MapInfo.AreaInfos[areaIndex]);

                    DebugUtility.Log(Constants.LOG_TAG, "Begin invoke on changed area");
                    try
                    {
                        MapManager.GetInstance()._OnChangedArea?.Invoke(m_MapInfo.AreaInfos[areaIndex].Uid);
                        DebugUtility.Log(Constants.LOG_TAG, "End invoke on changed area");
                    }
                    catch (Exception e)
                    {
                        DebugUtility.LogError(Constants.LOG_TAG, string.Format("Invoke on changed area failed, Exception:\n{0}", e.ToString()));
                    }
                }
                else
                {
                    DebugUtility.LogVerbose(Constants.LOG_TAG, $"Cant change to area({targetAraeInfo.Uid}), because limit change to uid({m_LimitChangeToAreaUid})");
                }
            }
        }

        /// <summary>
        /// 加载MapInfo成功
        /// </summary>
        private void OnLoadMapInfoCompleted(string pathOrAddress, UnityEngine.Object obj, object userData)
        {
            DebugUtility.Assert(obj != null, Constants.LOG_TAG, "Load Map Info");

            DebugUtility.Log(Constants.LOG_TAG, string.Format("Map({0}) Loaded Map Info", m_MapUid));
            //m_MapInfo = obj as MapInfo;
            TextAsset text = obj as TextAsset;
            m_MapInfo = new MapInfo();
            m_MapInfo.Deserialize(text.bytes);
            m_MapInfo.Initialize();
            
            if(GameFacade.Instance != null)
            {
                GameplayProxy gameplayProxy = GameFacade.Instance.RetrieveProxy(ProxyName.GameplayProxy) as GameplayProxy;
                if (gameplayProxy != null)
                {
                    Vector3 realWorldPos = MapManager.GetInstance()._PlayerInfo.RealWorldPosition;
                    Vector3 gameWorldPos = gameplayProxy.WorldPositionToServerAreaOffsetPosition(realWorldPos);
                    MapManager.GetInstance().SetPlayerPosition(realWorldPos, gameWorldPos);
                }
            }
            AssetUtil.LoadSceneAsync(m_MapInfo.SceneAddressableKey, OnLoadMapSceneCompleted, null, LoadSceneMode.Additive);
        }

        /// <summary>
        /// 加载Map对应的Scene成功
        /// </summary>
        private void OnLoadMapSceneCompleted(string pathOrAddress, Scene scene, object userData)
        {
            DebugUtility.Assert(scene.IsValid(), Constants.LOG_TAG, "Load Map Scene");

            DebugUtility.Log(Constants.LOG_TAG, string.Format("Map({0}) Loaded Map Scene", m_MapUid));

            m_MapScene = scene;
            SceneManager.SetActiveScene(m_MapScene);
            GameObject[] rootGameObjects = m_MapScene.GetRootGameObjects();
            m_NeedOriginPointOffsets = new List<EnableOriginPointOffset>(rootGameObjects.Length);
            m_NeedForcedLockingDistanceToPlayers = new List<ForcedLockingDistanceToPlayer>(rootGameObjects.Length);
            for (int iGameObject = 0; iGameObject < rootGameObjects.Length; iGameObject++)
            {
                GameObject iterGameObject = rootGameObjects[iGameObject];
                EnableOriginPointOffset iterEnableOriginPointOffset = iterGameObject.GetComponent<EnableOriginPointOffset>();
                if (iterEnableOriginPointOffset)
                {
                    iterEnableOriginPointOffset._RealWorldPostion = iterEnableOriginPointOffset.transform.localPosition;
                    m_NeedOriginPointOffsets.Add(iterEnableOriginPointOffset);
                }

                ForcedLockingDistanceToPlayer iterForcedLockingDistanceToPlayer = iterGameObject.GetComponent<ForcedLockingDistanceToPlayer>();
                if (iterForcedLockingDistanceToPlayer)
                {
                    iterForcedLockingDistanceToPlayer._DistanceToPlayer = iterForcedLockingDistanceToPlayer.transform.localPosition;
                    m_NeedForcedLockingDistanceToPlayers.Add(iterForcedLockingDistanceToPlayer);
                }
            }

            DoUpdate_NeedOriginPointOffset(true);
            DoUpdate_ForcedLockingDistanceToPlayer(true);

            // 加载当前Area
            DoUpdate_Area(true);
            // HACK 因为DoUpdate_Area会把State改为ChangingArea
            m_State = State.LoadingMap;
        }

        internal void _DoGUI(DebugPanel.Config config)
        {
            int activeAreaCount = 0;
            int instantiatedUnitCount = 0;

            if (m_ExpiredAreaControllers != null)
            {
                activeAreaCount += m_ExpiredAreaControllers.Count;
                for (int iArea = 0; iArea < m_ExpiredAreaControllers.Count; iArea++)
                {
                    instantiatedUnitCount += m_ExpiredAreaControllers[iArea].GetInstantiatedUnitCount();
                }
            }

            if (m_CurrentAreaController != null)
            {
                activeAreaCount++;
                instantiatedUnitCount += m_CurrentAreaController.GetInstantiatedUnitCount();
            }

            GUILayout.Box(string.Format("Uid:{0} State:{1} Loaded {2} Areas, {3} Units"
                    , m_MapUid
                    , m_State
                    , activeAreaCount
                    , instantiatedUnitCount)
                , config.BoxStyle);

            if (m_CurrentAreaController != null)
            {
                GUILayout.Label("Current Area", config.ImportantLabelStyle);
                m_CurrentAreaController._DoGUI(config);
            }

            if (m_ExpiredAreaControllers != null
                && m_ExpiredAreaControllers.Count > 0)
            {
                GUILayout.Space(config.SpacePixels * 2.0f);
                GUILayout.Label("Expired Areas", config.ImportantLabelStyle);
                for (int iArea = 0; iArea < m_ExpiredAreaControllers.Count; iArea++)
                {
                    m_ExpiredAreaControllers[iArea]._DoGUI(config);
                    GUILayout.Space(config.SpacePixels);
                }
            }

            if (!config.IsEditor)
            {
                GUILayout.FlexibleSpace();
                GUILayout.EndVertical();
                GUILayout.BeginVertical();
            }

            // 右侧
        }

#if UNITY_EDITOR
        /// <summary>
        /// 测试用的代码，不要使用
        /// </summary>
        internal MapInfo _GetMapInfo()
        {
            return m_MapInfo;
        }
#endif

        public enum State
        {
            /// <summary>
            /// 正常游戏
            /// </summary>
            Normal = 1 << 0,
            /// <summary>
            /// 加载Map中
            /// </summary>
            LoadingMap = 1 << 1,
            /// <summary>
            /// 正在切换Area
            /// </summary>
            ChangingArea = 1 << 3,
            /// <summary>
            /// 这个Map在Releasing
            /// </summary>
            ReleasingAreas = 1 << 4,
            /// <summary>
            /// 这个Map在Releasing
            /// </summary>
            ReleasingMap = 1 << 5,
            /// <summary>
            /// 这个Map已经被Released
            /// </summary>
            Released = 1 << 6,
        }

        /// <summary>
        /// 实现<see cref="ForReadyToPlayCallback.OnReadyToPlayCallback"/>需要用到的变量
        /// 封装这个类是因为这段逻辑有点绕，和<see cref="AreaController"/>写在一起会导致逻辑有点混乱
        /// </summary>
        private class ForReadyToPlayCallback
        {
            /// <summary>
            /// Map已经准备让玩家玩的回调
            /// </summary>
            public System.Action OnReadyToPlayCallback;
            /// <summary>
            /// 玩家刚进这个Map时所在的Area
            /// </summary>
            public AreaController StartArea;
        }
    }
}