#if UNITY_EDITOR
using Leyoutech.Utility;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Map
{
    [ExecuteInEditMode]
	public class GamingMap : MonoBehaviour
	{
		#region 私有属性
		
		private IEnumerator m_OnUpdateEnumerator;

		private IEnumerator m_OnExportEnumerator;
		/// <summary>
		/// 对应scene的路径 用于生成Area所在的路径
		/// </summary>
		private string m_OwnerAreaPath;
		/// <summary>
		/// 当前Scene
		/// </summary>
		private Scene m_OwnerScene;

		/// <summary>
		/// 缓存
		/// </summary>
		private List<GUIContent> m_ContentCache;
		#endregion

		#region 公开属性
		/// <summary>
		/// Map对应的id
		/// </summary>
		public uint m_MapId;

		/// <summary>
		/// GamingMap对应的id
		/// </summary>
		public uint m_Uid;

		/// <summary>
		/// 类型
		/// </summary>
		public GamingMapType m_Type;
		/// <summary>
		/// 地图名称
		/// </summary>
		public string m_MapName;

        /// <summary>
        /// 最大人数
        /// </summary>
        public int m_MaxPlayerNum;
        /// <summary>
        /// 销毁时间
        /// </summary>
        public int m_RemoveSecond;

        /// <summary>
        /// 寻路类型
        /// </summary>
        public KMapPathType m_PathType;

        /// <summary>
        /// 跃迁id缓存
        /// </summary>
        public List<ulong> m_LeapIdCache;

        /// <summary>
        /// 人形态复活地图id
        /// </summary>
        public uint m_SpaceGamingMapId;

        /// <summary>
        /// 任务发布列表
        /// </summary>
        public List<int> m_MissionList;
        /// <summary>
        /// 人行地图id列表
        /// </summary>
        [System.NonSerialized]
        public List<uint> m_HumanSpaceIds;

        [System.NonSerialized]
        public List<string> m_HumanSpaceIdStrs;


        /// <summary>
        /// 所属恒星id
        /// </summary>
        public int m_FixedStarId;
        /// <summary>
        /// 恒星id列表
        /// </summary>
        [System.NonSerialized]
        public List<int> m_FixedStarIds;
        /// <summary>
        /// 显示的恒星名称
        /// </summary>
        [System.NonSerialized]
        public List<string> m_FixedStarIdStrs;
        #endregion

        #region 挂载
        /// <summary>
        /// 该地图所拥有的GamingMapArea列表
        /// </summary>
        public List<GamingMapArea> m_GamingAreaList;

       

        /// <summary>
        /// 跃迁预览
        /// </summary>
        public LeapOverview m_LeapOverview;
		#endregion

		#region 私有方法
		/// <summary>
		/// 获取当前打开的Scene
		/// </summary>
		private void InitOwnerScene()
		{
			for (int iScene = 0; iScene < SceneManager.sceneCount; iScene++)
			{
				Scene scene = SceneManager.GetSceneAt(iScene);
				if (!scene.isLoaded)
				{
					continue;
				}
				GameObject[] rootObjs = scene.GetRootGameObjects();
				if (rootObjs != null && rootObjs.Length > 0)
				{
					for (int iRoot = 0; iRoot < rootObjs.Length; iRoot++)
					{
						GamingMap map = rootObjs[iRoot].GetComponent<GamingMap>();
						if (map != null && map == this)
						{
							m_OwnerScene = scene;
							break;
						}
					}
				}
			}
		}

		private void OnEnable()
		{
			if (EditorApplication.isPlaying)
			{
				return;
			}
			Init();
		}

		/// <summary>
		/// 创建GameArea
		/// </summary>
		/// <param name="areaSpawner"></param>
		private void CreateGamingArea(AreaSpawner areaSpawner)
		{
			string areaTemplete = "";
			GamingMapEditorSetting gamingSetting = GamingMapEditorUtility.GetGamingMapEditorSetting();
			if(gamingSetting != null)
			{
				areaTemplete = gamingSetting.m_GamingAreaTemplete;
			}
			GameObject areaTempleteAsset = AssetDatabase.LoadAssetAtPath<GameObject>(areaTemplete);
			GameObject obj = Instantiate(areaTempleteAsset);
			obj.name = string.Format("Area_{0}", areaSpawner.GetAreaId());
			obj.transform.SetParent(transform);
			GamingMapArea area = obj.GetComponent<GamingMapArea>();
			if (area == null)
			{
				area = obj.AddComponent<GamingMapArea>();
			}
			area.Init(areaSpawner,this);
			m_GamingAreaList.Add(area);
		}

		/// <summary>
		/// 初始化
		/// </summary>
		private void Init()
		{
           // AssetManager.Instance.Initialize();
            m_ContentCache = new List<GUIContent>();
            EditorApplication.update += OnUpdate;
            m_OnUpdateEnumerator = UpdateGamingAreaList();
            EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyGUI;
            m_LeapIdCache = new List<ulong>();
        }
        
        /// <summary>
        /// 清理
        /// </summary>
        private void Clear()
		{
            EditorApplication.update -= OnUpdate;
			m_OnUpdateEnumerator = null;
			EditorApplication.hierarchyWindowItemOnGUI -= OnHierarchyGUI;
            EditorGamingMapData.Clear();
            m_LeapIdCache = null;
        }

		private void OnDisable()
		{
			Clear();
		}

		void OnUpdate()
		{
			if (m_OnUpdateEnumerator != null)
			{
				m_OnUpdateEnumerator.MoveNext();
			}
			if(m_OnExportEnumerator != null)
			{
				if(!m_OnExportEnumerator.MoveNext())
				{
					m_OnExportEnumerator = null;
				}
			}
		}

		/// <summary>
		/// 右键响应
		/// </summary>
		/// <param name="instanceID"></param>
		/// <param name="selectionRect"></param>
		private void OnHierarchyGUI(int instanceID, Rect selectionRect)
		{
			if (Event.current != null && selectionRect.Contains(Event.current.mousePosition)
				&& Event.current.button == 1 && Event.current.type <= EventType.MouseUp)
			{
				GameObject selectedGameObject = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
				if (selectedGameObject)
				{
					CreatureRoot creatureRoot = selectedGameObject.GetComponent<CreatureRoot>();
					if (creatureRoot != null)
					{
						RefreshCreatureRootUI(creatureRoot);
					}

					Creature creature = selectedGameObject.GetComponent<Creature>();
					if (creature != null)
					{
						RefreshCreatureUI(creature);
					}

					LocationRoot locationRoot = selectedGameObject.GetComponent<LocationRoot>();
					if (locationRoot != null)
					{
						RefreashLocationRootUI(locationRoot);
					}

					Location location = selectedGameObject.GetComponent<Location>();
					if (location != null)
					{
						RefreshLocationUI(location);
					}

                    TriggerRoot triggerRoot = selectedGameObject.GetComponent<TriggerRoot>();
                    if(triggerRoot != null)
                    {
                        RefreshTriggerRootUI(triggerRoot);
                    }
                    Trigger trigger = selectedGameObject.GetComponent<Trigger>();
                    if(trigger != null)
                    {
                        RefreshTriggerUI(trigger);
                    }

					TeleportRoot teleportRoot = selectedGameObject.GetComponent<TeleportRoot>();
					if (teleportRoot != null)
					{
						RefreashTeleportRootUI(teleportRoot);
					}

					GamingMapArea mapArea = selectedGameObject.GetComponent<GamingMapArea>();
					if (mapArea != null)
					{
						RefreshMapAreaUI(mapArea);
					}

					GamingMap map = selectedGameObject.GetComponent<GamingMap>();
					if (map != null)
					{
						RefreshMapUI(map);
					}

                    Leap leap = selectedGameObject.GetComponent<Leap>();
                    if(leap != null)
                    {
                        RefreshLeapUI(leap);
                    }
				}
			}
		}

		/// <summary>
		/// 开始导出
		/// </summary>
		public void BeginExport()
		{
			if (m_GamingAreaList != null && m_GamingAreaList.Count > 0)
			{
				for (int iGaming = 0; iGaming < m_GamingAreaList.Count; iGaming++)
				{
					m_GamingAreaList[iGaming].BeginExport();
				}
			}
            //if (m_Type != GamingMapType.mapDeepSpace)
            //{
            //    m_SpaceGamingMapId = 0;
            //}
        }

		/// <summary>
		/// 结束导出
		/// </summary>
		public void EndExport()
		{
			if (m_GamingAreaList != null && m_GamingAreaList.Count > 0)
			{
				for (int iGaming = 0; iGaming < m_GamingAreaList.Count; iGaming++)
				{
					GamingMapArea mapArea = m_GamingAreaList[iGaming];
					if (mapArea != null && mapArea.gameObject != null)
					{
						mapArea.Clear();
					}
				}
			}
			m_GamingAreaList = null;
		}

        /// <summary>
        /// 刷新Leap UI
        /// </summary>
        /// <param name="leap"></param>
        private void RefreshLeapUI(Leap leap)
        {
            if(leap == null)
            {
                return;
            }

            m_ContentCache.Clear();
            if(leap.m_IsExportToJson)
            {
                m_ContentCache.Add(new GUIContent("禁止导出"));
            }
            else
            {
                m_ContentCache.Add(new GUIContent("开启导出"));
            }

            Vector2 mousePosition = Event.current.mousePosition;
            GameObject userData = Selection.activeGameObject;
            int selected = -1;
            EditorUtility.DisplayCustomMenu(new Rect(mousePosition.x, mousePosition.y, 0, 0), m_ContentCache.ToArray(), selected,
                delegate (object data, string[] opt, int select)
                {
                    switch (select)
                    {
                        case 0://是否开启导出Area
                            leap.m_IsExportToJson = !leap.m_IsExportToJson;
                            break;
                    }
                }, userData);
            Event.current.Use();
        }

		/// <summary>
		/// 刷新GamingMap ui
		/// </summary>
		/// <param name="map"></param>
		private void RefreshMapUI(GamingMap gamingMap)
		{
			if (gamingMap == null)
			{
				return;
			}

			m_ContentCache.Clear();
			m_ContentCache.Add(new GUIContent("导出数据到json"));
			m_ContentCache.Add(new GUIContent("保存"));
			Scene scene = gamingMap.GetMapScene();
			if (!scene.isLoaded)
			{
				gamingMap.OpenMapScene();
			}

			Map map = null;
			GameObject[] rootObjs = scene.GetRootGameObjects();
			if (rootObjs != null && rootObjs.Length > 0)
			{
				for (int iRoot = 0; iRoot < rootObjs.Length; iRoot++)
				{
					GameObject obj = rootObjs[iRoot];
					map = obj.GetComponent<Map>();
					if (map != null)
					{
						break;
					}
				}
			}
			if (map != null)
			{
				List<AreaSpawner> areaSpawnerList = map.GetAreaSpawnerList();
				if (areaSpawnerList != null && areaSpawnerList.Count > 0)
				{
					for (int iArea = 0; iArea < areaSpawnerList.Count; iArea++)
					{
						AreaSpawner areaSpawner = areaSpawnerList[iArea];
						GUIContent content = new GUIContent(string.Format("区域/GamingArea_{0}", areaSpawner.GetAreaId()));
						m_ContentCache.Add(content);
					}
				}
			}

			Vector2 mousePosition = Event.current.mousePosition;
			GameObject userData = Selection.activeGameObject;
			int selected = -1;
			EditorUtility.DisplayCustomMenu(new Rect(mousePosition.x, mousePosition.y, 0, 0), m_ContentCache.ToArray(), selected,
				delegate (object data, string[] opt, int select)
				{
					switch (select)
					{
						case 0:
                            //BeginExportGamingMap(gamingMap,map);
                            List<ulong> areaIds = new List<ulong>();
                            List<Leap> allLeaps = m_LeapOverview.m_LeapList;
                            if(allLeaps !=null && allLeaps.Count>0)
                            {
                                for(int iLeap =0;iLeap<allLeaps.Count;iLeap++)
                                {
                                    if(allLeaps[iLeap].m_IsExportToJson)
                                    {
                                        areaIds.Add(allLeaps[iLeap].m_LeapId);
                                    }
                                }
                            }
                            new ExportGamingMapData().BeginExport(map, gamingMap, areaIds);
                            break;
						case 1://保存
							gamingMap.SaveScene();
							break;
						default:
							gamingMap.OnClickArea(select, map);
							break;
					}
				}, userData);
			Event.current.Use();
		}
        #region 导出分离(备份)
#if false
                 private ExporterHandle m_ExporterGamingMapHandle = new ExporterHandle();
                /// <summary>
                /// 开始导出collider
                /// </summary>
                public ExporterHandle BeginExportGamingMap(GamingMap gamingMap,Map map)
                {
                    m_ExporterGamingMapHandle.IsDone = false;
                    m_OnExportEnumerator = ExportToJson(gamingMap, map);
                    return m_ExporterGamingMapHandle;
                }

                private IEnumerator ExportToJson(GamingMap gamingMap,Map map)
		        {
			        List<AreaSpawner> areaSpawnerList = map.GetAreaSpawnerList();
			        if (areaSpawnerList != null && areaSpawnerList.Count > 0)
			        {
				        for (int iArea = 0; iArea < areaSpawnerList.Count; iArea++)
				        {
					        AreaSpawner areaSpawner = areaSpawnerList[iArea];
					        if (areaSpawner != null)
					        {
						        LoadGamingMapArea(areaSpawner, false);
						        yield return null;
					        }
				        }
			        }
			        yield return null;
			        InitGamingArea();
                    yield return null;
			        BeginExport();
                    yield return null;
                    IEnumerator updateAreaEnum = UpdataGamingAreas(true);
                    if (updateAreaEnum != null)
                    {
                        while (updateAreaEnum.MoveNext())
                        {
                            yield return null;
                        }
                    }
                    yield return null;
                    //刷新LeapOverview
                    if (m_LeapOverview != null)
                    {
                        IEnumerator leapOverviewUpdate = m_LeapOverview.OnUpdate(this);
                        if (leapOverviewUpdate != null)
                        {
                            while (leapOverviewUpdate != null && leapOverviewUpdate.MoveNext())
                            {
                                yield return null;
                            }
                        }
                    }
                    yield return null;
			        EditorGamingMapData.SaveGamingMapToJson(gamingMap);
			        yield return null;
			        gamingMap.EndExport();
			        yield return null;
                    m_ExporterGamingMapHandle.IsDone = true;
                }
#endif

        #endregion

        /// <summary>
        /// 点击区域响应
        /// </summary>
        /// <param name="selectIndex"></param>
        private void OnClickArea(int selectIndex, Map map)
		{
			if (selectIndex < 2 || map == null)
			{
				return;
			}
			List<AreaSpawner> areaSpawnerList = map.GetAreaSpawnerList();
			if (areaSpawnerList == null || areaSpawnerList.Count < selectIndex - 1)
			{
				return;
			}
			AreaSpawner areaSpawner = areaSpawnerList[selectIndex - 2];
			if (areaSpawner != null)
			{
				areaSpawner.LoadArea();
				//TODO:加载对应的GamingMapArea
				LoadGamingMapArea(areaSpawner);
			}
		}

		/// <summary>
		/// 加载所有的Area
		/// </summary>
		/// <param name="map"></param>
		private void LoadAllArea(Map map)
		{
			List<AreaSpawner> areaSpawnerList = map.GetAreaSpawnerList();
			if (areaSpawnerList == null || areaSpawnerList.Count <= 0)
			{
				return;
			}
			for (int iArea = 0; iArea < areaSpawnerList.Count; iArea++)
			{
				AreaSpawner areaSpawner = areaSpawnerList[iArea];
				if (areaSpawner != null)
				{
					LoadGamingMapArea(areaSpawner, false);
				}
			}
		}

		/// <summary>
		/// 保存其他GamingArea
		/// </summary>
		private void SaveOtherGamingArea()
		{
			if (m_GamingAreaList != null && m_GamingAreaList.Count > 0)
			{
				for (int iGame = 0; iGame < m_GamingAreaList.Count; iGame++)
				{
					GamingMapArea mapArea = m_GamingAreaList[iGame];
					SaveGamingArea(mapArea);
				}
			}
		}

		/// <summary>
		/// 删除所有GamingArea
		/// </summary>
		private void DestroyAllGamingArea()
		{
			if (m_GamingAreaList != null && m_GamingAreaList.Count > 0)
			{
				for (int iGame = 0; iGame < m_GamingAreaList.Count; iGame++)
				{
					GamingMapArea mapArea = m_GamingAreaList[iGame];
					mapArea.Clear(true);
				}
			}
		}

		private string GetMapScenePath()
		{
			string sceneName = "";
			MapEditorSetting mapSetting = MapEditorUtility.GetMapEditorSetting();
			if(mapSetting != null)
			{
				sceneName = mapSetting.m_MapSavePath;
			}
			string scenePath = string.Format("{0}\\Map_{1}.unity"
				, sceneName
				, this.m_MapId);
			scenePath = scenePath.Replace("\\", "/");
			return scenePath;
		}

		public Scene GetMapScene()
		{
			string scenePath = GetMapScenePath();
			Scene scene = EditorSceneManager.GetSceneByPath(scenePath);
			return scene;
		}

		/// <summary>
		/// 保存场景
		/// </summary>
		private void SaveScene()
		{
			SaveOtherGamingArea();
			EditorSceneManager.SaveScene(GetOwnerScene());
		}

		/// <summary>
		/// 刷新GamingMapArea ui
		/// </summary>
		/// <param name="area"></param>
		private void RefreshMapAreaUI(GamingMapArea area)
		{
			if (area == null)
			{
				return;
			}

			m_ContentCache.Clear();
			m_ContentCache.Add(new GUIContent("重置"));
			m_ContentCache.Add(new GUIContent("保存至Scene"));
			m_ContentCache.Add(new GUIContent("读取json数据"));
            m_ContentCache.Add(new GUIContent("同步Area"));
			Vector2 mousePosition = Event.current.mousePosition;
			GameObject userData = Selection.activeGameObject;
			int selected = -1;
			EditorUtility.DisplayCustomMenu(new Rect(mousePosition.x, mousePosition.y, 0, 0), m_ContentCache.ToArray(), selected,
				delegate (object data, string[] opt, int select)
				{
					switch (select)
					{
						case 0://重置
							ResetArea(area);
							break;
						case 1://保存至Scene
							SaveGamingArea(area);
							break;
						case 2://读取json数据
							ReadJsonData(area);
							break;
                        case 3:
                            SyncArea(area);
                            break;
					}
				}, userData);
			Event.current.Use();

		}

        private void SyncArea(GamingMapArea area)
        {
            AreaSpawner[] areaSpawners = FindObjectsOfType<AreaSpawner>();
            if(areaSpawners != null && areaSpawners.Length>0)
            {
                for(int iArea =0;iArea<areaSpawners.Length; iArea++)
                {
                    if(areaSpawners[iArea].m_AreaUid == area.m_AreaId)
                    {
                        area.AdjustTransform(areaSpawners[iArea]);
                        break;
                    }
                }
            }
           
        }

		/// <summary>
		/// 读取json数据
		/// </summary>
		/// <param name="mapArea"></param>
		private void ReadJsonData(GamingMapArea mapArea)
		{
			if (mapArea == null)
			{
				return;
			}

			EditorGamingMap gamingMap = EditorGamingMapData.LoadGamingMapFromJson(mapArea.GetGamingMapId());
			if (gamingMap != null)
			{
				EditorArea[] areas = gamingMap.areaList;
				if (areas != null && areas.Length > 0)
				{
					for (int iArea = 0; iArea < areas.Length; iArea++)
					{
						EditorArea area = areas[iArea];
						if (area.areaId == mapArea.m_AreaId)
						{
							mapArea.Init(area, this);
						}
					}
				}

			}
		}

		/// <summary>
		/// 刷新TeleportRootui
		/// </summary>
		/// <param name="root"></param>
		private void RefreashTeleportRootUI(TeleportRoot root)
		{
			if (root == null)
			{
				return;
			}
			m_ContentCache.Clear();
			if (root.m_ShowModel)
			{
				m_ContentCache.Add(new GUIContent("隐藏模型"));
			}
			else
			{
				m_ContentCache.Add(new GUIContent("显示模型"));
			}
			Vector2 mousePosition = Event.current.mousePosition;
			GameObject userData = Selection.activeGameObject;
			int selected = -1;
			EditorUtility.DisplayCustomMenu(new Rect(mousePosition.x, mousePosition.y, 0, 0), m_ContentCache.ToArray(), selected,
				delegate (object data, string[] opt, int select)
				{
					switch (select)
					{
						case 0://是否显示模型
							root.m_ShowModel = !root.m_ShowModel;
							break;

					}
				}, userData);
			Event.current.Use();
		}

        private void RefreshTriggerRootUI(TriggerRoot root)
        {
            if (root == null)
            {
                return;
            }

            m_ContentCache.Clear();
            m_ContentCache.Add(new GUIContent("创建Trigger"));
            if (root.m_ShowModel)
            {
                m_ContentCache.Add(new GUIContent("隐藏模型"));
            }
            else
            {
                m_ContentCache.Add(new GUIContent("显示模型"));
            }
            Vector2 mousePosition = Event.current.mousePosition;
            GameObject userData = Selection.activeGameObject;
            int selected = -1;
            EditorUtility.DisplayCustomMenu(new Rect(mousePosition.x, mousePosition.y, 0, 0), m_ContentCache.ToArray(), selected,
                delegate (object data, string[] opt, int select)
                {
                    switch (select)
                    {
                        case 0://创建Trigger
                            root.CreateTrigger();
                            break;
                        case 1:
                            root.m_ShowModel = !root.m_ShowModel;
                            break;

                    }
                }, userData);
            Event.current.Use();
        }

        private void RefreshTriggerUI(Trigger trigger)
        {
            if (trigger == null)
            {
                return;
            }

            m_ContentCache.Clear();
            m_ContentCache.Add(new GUIContent("删除"));

            Vector2 mousePosition = Event.current.mousePosition;
            GameObject userData = Selection.activeGameObject;
            int selected = -1;
            EditorUtility.DisplayCustomMenu(new Rect(mousePosition.x, mousePosition.y, 0, 0), m_ContentCache.ToArray(), selected,
                delegate (object data, string[] opt, int select)
                {
                    switch (select)
                    {
                        case 0://删除Trigger
                            trigger.DestroySelf();
                            break;
                        case 1:
                            trigger.m_ShowModel = !trigger.m_ShowModel;
                            break;
                    }
                }, userData);
            Event.current.Use();
        }

		/// <summary>
		/// 刷新LocationRoot ui
		/// </summary>
		private void RefreashLocationRootUI(LocationRoot root)
		{
			if (root == null)
			{
				return;
			}

			m_ContentCache.Clear();
			m_ContentCache.Add(new GUIContent("创建Location"));
			if (root.m_ShowModel)
			{
				m_ContentCache.Add(new GUIContent("隐藏模型"));
			}
			else
			{
				m_ContentCache.Add(new GUIContent("显示模型"));
			}
			Vector2 mousePosition = Event.current.mousePosition;
			GameObject userData = Selection.activeGameObject;
			int selected = -1;
			EditorUtility.DisplayCustomMenu(new Rect(mousePosition.x, mousePosition.y, 0, 0), m_ContentCache.ToArray(), selected,
				delegate (object data, string[] opt, int select)
				{
					switch (select)
					{
						case 0://创建Location
							root.CreateLocation();
							break;
						case 1:
							root.m_ShowModel = !root.m_ShowModel;
							break;

					}
				}, userData);
			Event.current.Use();
		}

		/// <summary>
		/// 刷新Location
		/// </summary>
		/// <param name="location"></param>
		private void RefreshLocationUI(Location location)
		{
			if (location == null)
			{
				return;
			}

			m_ContentCache.Clear();
			m_ContentCache.Add(new GUIContent("删除"));

			Vector2 mousePosition = Event.current.mousePosition;
			GameObject userData = Selection.activeGameObject;
			int selected = -1;
			EditorUtility.DisplayCustomMenu(new Rect(mousePosition.x, mousePosition.y, 0, 0), m_ContentCache.ToArray(), selected,
				delegate (object data, string[] opt, int select)
				{
					switch (select)
					{
						case 0://删除Location
							location.DestroySelf();
							break;
						case 1:
							location.m_ShowModel = !location.m_ShowModel;
							break;
					}
				}, userData);
			Event.current.Use();
		}
		/// <summary>
		/// 刷新Npcui
		/// </summary>
		/// <param name="creature"></param>
		private void RefreshCreatureUI(Creature creature)
		{
			if (creature == null)
			{
				return;
			}
			m_ContentCache.Clear();
			m_ContentCache.Add(new GUIContent("删除"));
			Vector2 mousePosition = Event.current.mousePosition;
			GameObject userData = Selection.activeGameObject;
			int selected = -1;
			EditorUtility.DisplayCustomMenu(new Rect(mousePosition.x, mousePosition.y, 0, 0), m_ContentCache.ToArray(), selected,
				delegate (object data, string[] opt, int select)
				{
					switch (select)
					{
						case 0://创建Creature
							creature.DestroySelf();
							break;
					}
				}, userData);
			Event.current.Use();
		}
        

		/// <summary>
		/// 刷新npc列表显示ui
		/// </summary>
		/// <param name="contentList"></param>
		/// <param name="areaSpawner"></param>
		private void RefreshCreatureRootUI(CreatureRoot creatureRoot)
		{
			if (creatureRoot == null)
			{
				return;
			}
			m_ContentCache.Clear();
			m_ContentCache.Add(new GUIContent("创建Creature"));
			if (creatureRoot.m_ShowModel)
			{
				m_ContentCache.Add(new GUIContent("隐藏模型"));
			}
			else
			{
				m_ContentCache.Add(new GUIContent("显示模型"));
			}
			Vector2 mousePosition = Event.current.mousePosition;
			GameObject userData = Selection.activeGameObject;
			int selected = -1;
			EditorUtility.DisplayCustomMenu(new Rect(mousePosition.x, mousePosition.y, 0, 0), m_ContentCache.ToArray(), selected,
				delegate (object data, string[] opt, int select)
				{
					switch (select)
					{
						case 0://创建Creature
							creatureRoot.CreateCreature();
							break;
						case 1:
							creatureRoot.m_ShowModel = !creatureRoot.m_ShowModel;
							break;

					}
				}, userData);
			Event.current.Use();
		}

		public void InitGamingArea()
		{
			if (m_GamingAreaList == null)
			{
				m_GamingAreaList = new List<GamingMapArea>();
			}
			m_GamingAreaList.Clear();
			GamingMapArea[] areas = GetComponentsInChildren<GamingMapArea>();
			if (areas != null && areas.Length > 0)
			{
				for (int iArea = 0; iArea < areas.Length; iArea++)
				{
					m_GamingAreaList.Add(areas[iArea]);
				}
			}
		}

		private IEnumerator UpdateGamingAreaList()
		{
			yield return null;
			//不放在OnEnable里初始化是因为 OnEnable时 当前Scene还未加载完毕
			Scene scene = GetOwnerScene();
			if (scene != null)
			{
				string[] paths = scene.path.Split('/');
				if (paths != null && paths.Length > 0)
				{
					string ownerSceneFloder = scene.path.Substring(0, scene.path.Length - paths[paths.Length - 1].Length);
					m_OwnerAreaPath = string.Format("{0}{1}", ownerSceneFloder, m_OwnerScene.name);
					FileUtility.ExistsDirectoryOrCreate(m_OwnerAreaPath);
					AssetDatabase.Refresh();
				}
				Selection.activeObject = gameObject;
			}
			yield return null;
            //初始化LeapPreview
            m_LeapOverview = GetComponentInChildren<LeapOverview>();
            if(m_LeapOverview == null)
            {
                GameObject leapObj = new GameObject("LeapOverview");
                leapObj.transform.SetParent(transform);
                leapObj.transform.localPosition = Vector3.zero;
                leapObj.transform.localRotation = Quaternion.identity;
                m_LeapOverview = leapObj.AddComponent<LeapOverview>();
            }
            yield return null;
            //加载对应的MapScene
            Scene mapScene = GetMapScene();
			if (!mapScene.isLoaded)
			{
				OpenMapScene();
			}
			yield return null;
			IEnumerator initEnumerator = EditorConfigData.Instance.InitExcelData();
			if (initEnumerator != null)
			{
				while (initEnumerator.MoveNext())
				{
					yield return null;
				}
			}
			while (true)
			{
				InitGamingArea();
				yield return null;
                IEnumerator updateAreaEnum = UpdataGamingAreas();
                if(updateAreaEnum != null)
                {
                    while(updateAreaEnum.MoveNext())
                    {
                        yield return null;
                    }
                }

                if (m_LeapOverview != null)
                {
                    IEnumerator leapUpdateEnum = m_LeapOverview.OnUpdate(this);
                    if (leapUpdateEnum != null)
                    {
                        while (leapUpdateEnum.MoveNext())
                        {
                            yield return null;
                        }
                    }
                }

                yield return null;
			}
		}
        

        public IEnumerator UpdataGamingAreas(bool isExport = false)
        {
            if (m_GamingAreaList != null && m_GamingAreaList.Count > 0)
            {
                for (int iGame = 0; m_GamingAreaList != null && iGame < m_GamingAreaList.Count; iGame++)
                {
                    if (m_GamingAreaList[iGame] == null || m_GamingAreaList[iGame].gameObject == null)
                    {
                        continue;
                    }
                    IEnumerator gameAreaEnumerator = m_GamingAreaList[iGame].OnUpdate(this, isExport);
                    if (gameAreaEnumerator != null)
                    {
                        while (gameAreaEnumerator.MoveNext())
                        {
                            yield return null;
                        }
                    }
                }
            }
        }

		/// <summary>
		/// 创建区域
		/// </summary>
		/// <param name="area"></param>
		private GamingMapArea CreateArea(EditorArea area)
		{
			GameObject areaObj = new GameObject();
			areaObj.transform.SetParent(transform);
			GamingMapArea mapArea = areaObj.AddComponent<GamingMapArea>();
			mapArea.Init(area, this);
			return mapArea;
		}
		
#endregion

#region 公开方法

        /// <summary>
        /// 初始化恒星id列表
        /// </summary>
        public void LoadFixedStarIds()
        {
            if(m_FixedStarIds != null && m_FixedStarIds.Count>0)
            {
                return;
            }
            if (m_Type != GamingMapType.mapDeepSpace)
            {
                return;
            }

            m_FixedStarIds = new List<int>();
            m_FixedStarIdStrs = new List<string>();
            EditorGamingMapData.LoadStarMapVO();
            List<StarMapVO> starVoList = ConfigVO<StarMapVO>.Instance.GetList();
            if(starVoList != null && starVoList.Count>0)
            {
                for(int iStar = 0;iStar<starVoList.Count;iStar++)
                {
                    StarMapVO starMapVo = starVoList[iStar];
                    if(starMapVo == null)
                    {
                        continue;
                    }
                    m_FixedStarIds.Add(starMapVo.FixedStarid);
                    m_FixedStarIdStrs.Add(string.Format("{0}_{1}", starMapVo.FixedStarid, starMapVo.Name));
                }
            }
        }

        /// <summary>
        /// 初始化人形地图id列表
        /// </summary>
        public void LoadHumanSpaceIds()
        {
            if (m_Type == GamingMapType.mapMainCity || m_Type == GamingMapType.mapSpaceStation)
            {
                m_SpaceGamingMapId = 0;
                return;
            }

            if (m_HumanSpaceIds != null && m_HumanSpaceIds.Count>0)
            {
                return;
            }
            
            m_HumanSpaceIds = new List<uint>();
            m_HumanSpaceIdStrs = new List<string>();
            List<EditorGamingMap> gamingMapList = EditorGamingMapData.LoadAllGamingMapJson();
            if(gamingMapList != null && gamingMapList.Count>0)
            {
                for(int iGaming = 0;iGaming<gamingMapList.Count;iGaming++)
                {
                    EditorGamingMap gamingMap = gamingMapList[iGaming];
                    if(gamingMap.gamingType == (int)GamingMapType.mapSpaceStation || gamingMap.gamingType == (int)GamingMapType.mapMainCity)
                    {
                        m_HumanSpaceIds.Add(gamingMap.gamingmapId);
                        m_HumanSpaceIdStrs.Add(string.Format("{0}_{1}",gamingMap.gamingmapId, gamingMap.gamingmapName));
                    }
                }
            }
        }

		/// <summary>
		/// 获取某个地图某个区域的Locations
		/// </summary>
		/// <param name="mapId"></param>
		/// <param name="areaId"></param>
		/// <returns></returns>
		public static EditorLocation[] GetEditorLocations(uint mapId, ulong areaId)
		{
			EditorGamingMap map = EditorGamingMapData.LoadGamingMapFromJson(mapId);
			if (map != null && map.areaList != null && map.areaList.Length > 0)
			{
				for (int iArea = 0; iArea < map.areaList.Length; iArea++)
				{
					EditorArea area = map.areaList[iArea];
					if (area.areaId == areaId)
					{
						return area.locationList;
					}
				}
			}
			return null;
		}
		

		/// <summary>
		/// 重置
		/// </summary>
		public void ResetArea(GamingMapArea area)
		{
			if (area != null)
			{
				area.Clear(false);
			}
		}

		/// <summary>
		/// 获取当前地图存储区域的路径
		/// </summary>
		/// <returns></returns>
		public string GetOwnerAreaPath()
		{
            if (string.IsNullOrEmpty(m_OwnerAreaPath))
            {
                Scene scene = GetOwnerScene();
                if (scene != null)
                {
                    string[] paths = scene.path.Split('/');
                    if (paths != null && paths.Length > 0)
                    {
                        string ownerSceneFloder = scene.path.Substring(0, scene.path.Length - paths[paths.Length - 1].Length);
                        m_OwnerAreaPath = string.Format("{0}{1}", ownerSceneFloder, m_OwnerScene.name);
                        FileUtility.ExistsDirectoryOrCreate(m_OwnerAreaPath);
                        AssetDatabase.Refresh();
                    }
                }
            }
            return m_OwnerAreaPath;
        }

		/// <summary>
		/// 获取当前Scene
		/// </summary>
		/// <returns></returns>
		public Scene GetOwnerScene()
		{
			if (m_OwnerScene.name == null)
			{
				InitOwnerScene();
			}
			DebugUtility.Assert(m_OwnerScene.name != null && m_OwnerScene.path != null, "获取的当前Scene不对");
			return m_OwnerScene;
		}

		/// <summary>
		/// 初始化
		/// </summary>
		/// <param name="gamingMapuid"></param>
		/// <param name="mapId"></param>
		/// <param name="mapName"></param>
		/// <param name="mapType"></param>
		/// <param name="areaSpawners"></param>
		public void Init(uint gamingMapuid, uint mapId, string mapName, GamingMapType mapType, List<AreaSpawner> areaSpawners)
		{
			m_Uid = gamingMapuid;
			m_MapId = mapId;
			m_MapName = mapName;
			m_Type = mapType;
			string areaTemplete = "";
			GamingMapEditorSetting gamingSetting = GamingMapEditorUtility.GetGamingMapEditorSetting();
			if (gamingSetting != null)
			{
				areaTemplete = gamingSetting.m_GamingAreaTemplete;
			}
			GameObject areaTempleteAsset = AssetDatabase.LoadAssetAtPath<GameObject>(areaTemplete);
			if (areaTempleteAsset == null)
			{
				Debug.LogError("GamingMap Area 模板未设置");
				return;
			}

			if (areaSpawners != null && areaSpawners.Count > 0)
			{
				m_GamingAreaList = new List<GamingMapArea>();
				for (int iArea = 0; iArea < areaSpawners.Count; iArea++)
				{
					AreaSpawner areaSpawner = areaSpawners[iArea];
					CreateGamingArea(areaSpawner);
				}
			}
            SaveScene();
        }

		public void LoadGamingMapArea(AreaSpawner areaSpawner, bool closeOtherArea = true)
		{
			if (areaSpawner == null)
			{
				return;
			}
			string m_AreaScenePath = string.Format("{0}/GamingMapArea_{1}_{2}.unity", GetOwnerAreaPath(), m_Uid, areaSpawner.GetAreaId());
			if (string.IsNullOrEmpty(m_AreaScenePath))
			{
				Debug.LogError("m_AreaScenePath == null");
				return;
			}

			GamingMapArea[] areaArray = UnityEngine.Object.FindObjectsOfType<GamingMapArea>();
			if (areaArray != null && areaArray.Length > 0)
			{
				for (int iArea = 0; iArea < areaArray.Length; iArea++)
				{
					GamingMapArea area = areaArray[iArea];
					if (area != null && area.m_AreaId == areaSpawner.GetAreaId())
					{
						//Debug.LogError(string.Format("{0}Area已导入", area.m_AreaId));
						return;
					}
				}
			}
			if (closeOtherArea)
			{
				SaveOtherGamingArea();
			}
			string areaScenePath = m_AreaScenePath.Replace("Assets", Application.dataPath);
			Scene areaScene;
			if (!File.Exists(areaScenePath))
			{
				string gamingAreaPath = "";
				GamingMapEditorSetting gamingSetting = GamingMapEditorUtility.GetGamingMapEditorSetting();
				if (gamingSetting != null)
				{
					gamingAreaPath = gamingSetting.m_GamingAreaTemplete;
				}
				areaScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
				GameObject areaTempleteAsset = AssetDatabase.LoadAssetAtPath<GameObject>(gamingAreaPath);
				GameObject gamingArea = GameObject.Instantiate(areaTempleteAsset);
				GamingMapArea mapArea = gamingArea.GetComponent<GamingMapArea>();
				mapArea.Init(areaSpawner,this);
				EditorSceneManager.MoveGameObjectToScene(gamingArea, areaScene);
				EditorSceneManager.SaveScene(areaScene, m_AreaScenePath);
				EditorSceneManager.CloseScene(areaScene, true);
				AssetDatabase.Refresh();
			}

			areaScene = EditorSceneManager.OpenScene(m_AreaScenePath, OpenSceneMode.Additive);
			if (areaScene != null)
			{
				GamingMapArea mapArea = null;
				GameObject[] rootObjs = areaScene.GetRootGameObjects();
				if (rootObjs != null && rootObjs.Length > 0)
				{
					for (int rIndex = 0; rIndex < rootObjs.Length; rIndex++)
					{
						mapArea = rootObjs[rIndex].GetComponent<GamingMapArea>();
						if (mapArea != null)
						{
							break;
						}
					}
				}
				if (mapArea != null)
				{
					EditorSceneManager.MoveGameObjectToScene(mapArea.gameObject, GetOwnerScene());
					mapArea.transform.SetParent(transform);
				}
				EditorSceneManager.CloseScene(areaScene, true);
			}
		}
		
		/// <summary>
		/// 打开MapScene
		/// </summary>
		/// <param name="map"></param>
		public void OpenMapScene()
		{
			Scene scene = GetMapScene();
			if (!scene.isLoaded)
			{
				string scenePath = GetMapScenePath();
				EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
			}
		}
		

		public AreaSpawner GetAreaSpawner(ulong areaId)
		{
			OpenMapScene();
			Map[] maps = FindObjectsOfType<Map>();
			if (maps != null && maps.Length > 0)
			{
				for (int iMap = 0; iMap < maps.Length; iMap++)
				{
					if (maps[iMap].Uid == this.m_MapId)
					{
						List<AreaSpawner> areaSpawners = maps[iMap].GetAreaSpawnerList();
						if (areaSpawners != null && areaSpawners.Count > 0)
						{
							for (int iArea = 0; iArea < areaSpawners.Count; iArea++)
							{
								if (areaSpawners[iArea].m_AreaUid == areaId)
								{
									return areaSpawners[iArea];
								}
							}
						}
					}
				}
			}
			return null;
		}

		/// <summary>
		/// 保存GamingMapArea
		/// </summary>
		/// <param name="area"></param>
		public void SaveGamingArea(GamingMapArea area)
		{
			if (area == null || area.gameObject == null)
			{
				return;
			}
			string areaName = area.GetAreaScenePath();
			if (string.IsNullOrEmpty(areaName))
			{
                if (!Application.isBatchMode)
                {
                    EditorUtility.DisplayDialog("提示", "不存在Area场景", "确定");
                }
                return;
			}
			string areaPath = areaName.Replace("Assets", Application.dataPath);
			Scene areaScene;
			if (!File.Exists(areaPath))
			{
				areaScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
			}
			else
			{
				areaScene = EditorSceneManager.OpenScene(areaName, OpenSceneMode.Additive);
			}

			if (areaScene != null)
			{
				GameObject[] rootObjs = areaScene.GetRootGameObjects();
				if (rootObjs != null && rootObjs.Length > 0)
				{
					for (int rIndex = 0; rIndex < rootObjs.Length; rIndex++)
					{
						GameObject.DestroyImmediate(rootObjs[rIndex]);
					}
				}
				area.transform.SetParent(null);
				EditorSceneManager.MoveGameObjectToScene(area.gameObject, areaScene);
				SceneManager.SetActiveScene(areaScene);
			}
			EditorSceneManager.SaveScene(areaScene, areaName);
			EditorSceneManager.CloseScene(areaScene, true);
			if (area != null && area.gameObject != null)
			{
				GameObject.DestroyImmediate(area.gameObject);
			}

		}
#endregion
		
	}

}
#endif