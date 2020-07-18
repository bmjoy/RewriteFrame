using Leyoutech.Utility;
using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Map
{
    public class MapManager : MonoBehaviour
    {
        private static MapManager ms_MapManager;

        /// <summary>
        /// map id
        /// </summary>
        internal Action<uint> _OnChangedMap;
        /// <summary>
        /// area id
        /// </summary>
        internal Action<ulong> _OnChangedArea;
        /// <summary>
        /// area id
        /// </summary>
        internal Action<ulong> _OnReleasedArea;
        /// <summary>
        /// map id
        /// </summary>
        internal Action<uint> _OnClosedMap;

        /// <summary>
        /// 当前的状态
        /// </summary>
        private State m_State;
        /// <summary>
        /// 当前的Map
        /// </summary>
        private MapController m_CurrentMapController;

        /// <summary>
        /// 将要切换到的Map的Uid
        /// </summary>
        private uint m_WaitChangeMapUid;
        /// <summary>
        /// Map加载好后的回调
        /// </summary>
        private Action m_OnReadyToPlayCallback;
        /// <summary>
        /// 玩家的信息
        /// </summary>
        internal PlayerInfo _PlayerInfo;
        /// <summary>
        /// Debug用的
        /// </summary>
        internal ForDebug _ForDebug;

        public static MapManager GetInstance()
        {
            if (ms_MapManager == null)
            {
                GameObject go = new GameObject("MapManager");
                DontDestroyOnLoad(go);
                ms_MapManager = go.AddComponent<MapManager>();
                ms_MapManager.Initialize();
            }
            return ms_MapManager;
        }

        public AreaInfo FindAreaInfoByUidFromCurrentMap(ulong areaUid)
        {
            return m_CurrentMapController == null
                ? null
                : m_CurrentMapController.FindAreaInfoByUid(areaUid);
        }

        /// <summary>
        /// 如果当前没有Map，则返回<see cref="Constants.NOTSET_MAP_UID"/>
        /// </summary>
        public uint GetCurrentMapUid()
        {
            return m_CurrentMapController != null
                ? m_CurrentMapController.GetMapUid()
                : Constants.NOTSET_MAP_UID;
        }

        /// <summary>
        /// 如果当前没有Area，则返回<see cref="Constants.NOTSET_AREA_UID"/>
        /// </summary>
        public ulong GetCurrentAreaUid()
        {
            return m_CurrentMapController != null
                ? m_CurrentMapController.GetCurrentAreaUid()
                : Constants.NOTSET_AREA_UID;
        }

        /// <summary>
        /// <see cref="m_CurrentMapController"/>
        /// </summary>
        public MapController GetCurrentMapController()
        {
            return m_CurrentMapController;
        }

        public void TryCloseMap()
        {
            DebugUtility.Log(Constants.LOG_TAG, "Try Close Map");
            DebugUtility.Assert(m_State == State.Normal, Constants.LOG_TAG, "m_State == State.Normal");

            m_State = State.ClosingMap;
        }

        /// <summary>
        /// 尝试切换Map
        /// </summary>
        /// <param name="onReadyToPlayCallback"><see cref="MapController.ForReadyToPlayCallback.OnReadyToPlayCallback"/></param>
        public void TryChangeMap(uint mapUid, Action onReadyToPlayCallback = null)
        {
            DebugUtility.Log(Constants.LOG_TAG, string.Format("Try Change Map({0})", mapUid));
            DebugUtility.Assert(m_State == State.Normal || m_State == State.Notset
                , Constants.LOG_TAG, "m_State == State.Normal || m_State == State.Notset");

            m_State = State.WaitChangeMap;
            m_WaitChangeMapUid = mapUid;
            m_OnReadyToPlayCallback = onReadyToPlayCallback;
        }

        public void SetPlayerPosition(Vector3 playerRealWorldPosition, Vector3 playerGameWorldPosition)
        {
            _PlayerInfo.IsRealWorldChange = playerRealWorldPosition != _PlayerInfo.RealWorldPosition;
            _PlayerInfo.RealWorldPosition = playerRealWorldPosition;

            _PlayerInfo.IsGameWorldChange = playerGameWorldPosition != _PlayerInfo.GameWorldPosition;
            _PlayerInfo.GameWorldPosition = playerGameWorldPosition;
            //Debug.LogError("playerGameWorldPosition:"+ playerGameWorldPosition);
            Vector3 realWorld2GameWorld = playerGameWorldPosition - playerRealWorldPosition;
            _PlayerInfo.IsRealWorld2GameWorldChange = (realWorld2GameWorld - _PlayerInfo.RealWorld2GameWorld).sqrMagnitude > Mathf.Epsilon;
            _PlayerInfo.RealWorld2GameWorld = realWorld2GameWorld;

            DebugUtility.LogVerbose(Constants.LOG_TAG, $"PlayerPosition R({_PlayerInfo.IsRealWorldChange}) G({_PlayerInfo.IsGameWorldChange}) R2G({_PlayerInfo.IsRealWorld2GameWorldChange}) R({StringUtility.ConvertToDisplay(_PlayerInfo.RealWorldPosition)}) G({StringUtility.ConvertToDisplay(_PlayerInfo.GameWorldPosition)}) R2G({StringUtility.ConvertToDisplay(_PlayerInfo.RealWorld2GameWorld)})");
        }

        public void ForceUpdate()
        {
            LateUpdate();
        }

        protected void LateUpdate()
        {
            switch (m_State)
            {
                case State.Normal:
                    m_CurrentMapController.DoUpdate();
                    break;
                case State.ChangingMap:
                    if (m_CurrentMapController.GetState() == MapController.State.Normal)
                    {
                        m_State = State.Normal;
                    }
                    else if (m_CurrentMapController.GetState() == MapController.State.LoadingMap)
                    {
                        m_CurrentMapController.DoUpdate();
                    }
                    break;
                case State.WaitChangeMap:
                    bool readyToChangeMap = false;
                    if (m_CurrentMapController != null)
                    {
                        if (m_CurrentMapController.GetState() == MapController.State.Released)
                        {
                            readyToChangeMap = true;
                            SafeInvokeClosedMap(m_CurrentMapController.GetMapUid());
                            m_CurrentMapController = null;
                        }
                        else if (!m_CurrentMapController.IsReleasing())
                        {
                            m_CurrentMapController.RequestRelease();
                        }
                        else
                        {
                            m_CurrentMapController.DoUpdate();
                        }
                    }
                    else
                    {
                        readyToChangeMap = true;
                    }

                    if (readyToChangeMap)
                    {
                        m_State = State.ChangingMap;
                        m_CurrentMapController = new MapController();
                        m_CurrentMapController.RequestInitialize(m_WaitChangeMapUid, m_OnReadyToPlayCallback);
                        m_OnReadyToPlayCallback = null;
                    }
                    break;
                case State.ClosingMap:
                    if (m_CurrentMapController.GetState() == MapController.State.Released)
                    {
                        SafeInvokeClosedMap(m_CurrentMapController.GetMapUid());
                        m_CurrentMapController = null;
                        m_State = State.Notset;
                    }
                    else if (!m_CurrentMapController.IsReleasing())
                    {
                        m_CurrentMapController.RequestRelease();
                    }
                    else
                    {
                        m_CurrentMapController.DoUpdate();
                    }
                    break;
                case State.Notset:
                    // dont need handle
                    break;
                default:
                    DebugUtility.Assert(false, Constants.LOG_TAG, "意外的State: " + m_State);
                    break;
            }

            _PlayerInfo.IsGameWorldChange = false;
            _PlayerInfo.IsRealWorldChange = false;
        }

        private void Initialize()
        {
            m_State = State.Notset;
            _PlayerInfo = new PlayerInfo();
            _PlayerInfo.Reset();

            DebugPanel.DebugPanelInstance.GetInstance().RegistGUI(DebugPanel.TabName.Map, DoGUI, true);
        }

        /// <summary>
        /// <see cref="_OnClosedMap"/>
        /// </summary>
        private void SafeInvokeClosedMap(uint mapUid)
        {
            try
            {
                DebugUtility.Log(Constants.LOG_TAG, $"Begin invoke closed map({mapUid})");
                _OnClosedMap?.Invoke(mapUid);
                DebugUtility.Log(Constants.LOG_TAG, "End invoke closed map callback");
            }
            catch (Exception e)
            {
                DebugUtility.LogError(Constants.LOG_TAG, string.Format("Invoke closed map callback failed, Exception:\n{0}", e.ToString()));
            }
        }

        private void DoGUI(DebugPanel.Config config)
        {
            if (!config.IsEditor)
            {
                GUILayout.BeginHorizontal();
                GUILayout.BeginVertical();
            }

            // 左侧
            GUILayout.Box($"MapManager State:{m_State} PlayerRealWorldPosition:({_PlayerInfo.RealWorldPosition.x:F1}, {_PlayerInfo.RealWorldPosition.y:F1}, {_PlayerInfo.RealWorldPosition.z:F1}) PlayerGameWorldPosition:({_PlayerInfo.GameWorldPosition.x:F1}, {_PlayerInfo.GameWorldPosition.y:F1}, {_PlayerInfo.GameWorldPosition.z:F1})"
                , config.BoxStyle);

#if UNITY_EDITOR
            if (PreviewHelper.GetInstance() != null)
            {
                if (_ForDebug._MapUids == null)
                {
                    _ForDebug.InitMapUids();
                }

                _ForDebug._PreviewHelperFoldout = UnityEditor.EditorGUILayout.Foldout(_ForDebug._PreviewHelperFoldout, "Preview Helper");
                if (_ForDebug._PreviewHelperFoldout)
                {
                    UnityEditor.EditorGUILayout.LabelField("Map:");
                    config.BeginToolbarHorizontal();
                    const float BUTTON_MAX_WIDTH = 120.0f;
                    int buttonCountPreRow = Mathf.CeilToInt(config.PanelWidth / BUTTON_MAX_WIDTH);
                    for (int iMap = 0; iMap < _ForDebug._MapUids.Count; iMap++)
                    {
                        if (iMap > 0
                            && iMap % buttonCountPreRow == 0)
                        {
                            UnityEditor.EditorGUILayout.EndHorizontal();
                            config.BeginToolbarHorizontal();
                        }

                        if (config.ToolbarButton(_ForDebug._MapUids[iMap] == GetCurrentMapUid(), _ForDebug._MapUids[iMap].ToString()))
                        {
                            TryChangeMap(_ForDebug._MapUids[iMap]);
                        }
                    }
                    UnityEditor.EditorGUILayout.EndHorizontal();
                    UnityEditor.EditorGUILayout.Space();

                    if (m_CurrentMapController != null)
                    {
                        MapInfo mapInfo = m_CurrentMapController._GetMapInfo();
                        if (mapInfo != null)
                        {
                            UnityEditor.EditorGUILayout.LabelField("Area:");
                            config.BeginToolbarHorizontal();
                            for (int iArea = 0; iArea < mapInfo.AreaInfos.Length; iArea++)
                            {
                                if (iArea > 0
                                    && iArea % buttonCountPreRow == 0)
                                {
                                    UnityEditor.EditorGUILayout.EndHorizontal();
                                    config.BeginToolbarHorizontal();
                                }

                                if (config.ToolbarButton(mapInfo.AreaInfos[iArea].Uid == GetCurrentAreaUid(), mapInfo.AreaInfos[iArea].Uid.ToString()))
                                {
                                    PreviewHelper.GetInstance().SetRealPosition(mapInfo.AreaInfos[iArea].Position);
                                }
                            }
                            UnityEditor.EditorGUILayout.EndHorizontal();
                        }
                    }
                    UnityEditor.EditorGUILayout.Space();
                }
            }
#endif

            if (m_CurrentMapController != null)
            {
                GUILayout.Label("Current Map", config.LabelStyle);
                m_CurrentMapController._DoGUI(config);
            }

            if (!config.IsEditor)
            {
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
            }
        }

        public enum State
        {
            Notset,
            /// <summary>
            /// 正常游戏
            /// </summary>
            Normal,
            /// <summary>
            /// 正在切换Map
            /// </summary>
            ChangingMap,
            /// <summary>
            /// 等待切换Map
            /// </summary>
            WaitChangeMap,
            /// <summary>
            /// 关闭Map
            /// </summary>
            ClosingMap,
        }

        public struct ForDebug
        {
            public uint TargetMapUid;
#if UNITY_EDITOR
            internal List<uint> _MapUids;
            internal bool _PreviewHelperFoldout;

            public void InitMapUids()
            {
                _MapUids = new List<uint>();

                MapEditorSetting mapEditorSetting = MapEditorUtility.GetMapEditorSetting();
                string[] mapPathArray = AssetDatabase.FindAssets("t:Scene", new string[] { mapEditorSetting != null ? mapEditorSetting.m_MapSavePath : "" });
                if (mapPathArray == null)
                {
                    return;
                }

                for (int iMap = 0; iMap < mapPathArray.Length; iMap++)
                {
                    string assetPath = AssetDatabase.GUIDToAssetPath(mapPathArray[iMap]);
                    string[] pathSplitArray = assetPath.Split('/');
                    if (pathSplitArray.Length == 0)
                    {
                        continue;
                    }

                    string pathSplit = pathSplitArray[pathSplitArray.Length - 1];
                    if (!pathSplit.Contains("Map_"))
                    {
                        continue;
                    }
                    pathSplit = pathSplit.Replace(".unity", "");

                    string[] splitPath = pathSplit.Split('/');
                    if (splitPath.Length == 0)
                    {
                        continue;
                    }

                    string showPath = splitPath[splitPath.Length - 1];
                    if (string.IsNullOrEmpty(showPath))
                    {
                        continue;
                    }

                    string[] showPathArray = showPath.Split('_');
                    if (showPathArray.Length != 2)
                    {
                        continue;
                    }

                    if (uint.TryParse(showPathArray[1], out uint result)
                        && result > 0)
                    {
                        _MapUids.Add(result);
                    }
                }
            }
#endif
        }
    }
}