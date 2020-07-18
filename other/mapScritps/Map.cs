#if UNITY_EDITOR
using EditorExtend;
using Leyoutech.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Map
{
	/// <summary>
	/// <see cref="http://wiki.leyoutech.com/pages/viewpage.action?pageId=17761761"/>
	/// </summary>
	[ExecuteInEditMode]
	public class Map : MonoBehaviour
	{
		/// <summary>
		/// 这个Map的唯一ID
		/// 用于策划填表
		/// </summary>
		//[Range(Constants.MAP_UID_MIN, Constants.MAP_UID_MAX),Tooltip("地图ID")]
		[Tooltip("地图ID")]
		[ReadOnly]
		public uint Uid;
		/// <summary>
		/// 导出地图时是否开启烘焙 默认不开启烘焙
		/// </summary>
		public bool OpenLightBake = false;
		/// <summary> 
		/// 预期的FOV
		/// </summary>
		[Range(10, 180),Tooltip("预期的FOV")]
		public float ExpectedFov = 60;
		/// <summary>
		/// 自动计算VoxelSize
		/// </summary>
		[Tooltip("是否开启计算格子大小")]
		public bool AutoVoxelSize = true;
		/// <summary>
		/// 这个Map下的Voxel
		/// </summary>
		public VoxelGrid VoxelGrid;
		/// <summary>
		/// 导出时的设置
		/// </summary>
		[Tooltip("导出设置")]
		public ExportSetting ExportSetting;
		/// <summary>
		/// Debug的信息
		/// </summary>
		[Header("DEBUG"),Tooltip("调试信息")]
		public ForDebug _Debug;
		/// <summary>
		/// 是否显示地图的AB
		/// </summary>
		public bool _DebugShowMapAB;
		/// <summary>
		/// 地图AB颜色显示
		/// </summary>
		public Color _DebugMapABColor;
		
		/// <summary>
		/// 用于每帧更新一部分内容
		/// </summary>
		private IEnumerator m_DoUpdateEnumerator;

        /// <summary>
        /// 导出collider的携程
        /// </summary>
        private IEnumerator m_ExportColliderEnumerator;
		/// <summary>
		/// 已经分配出去的Uid，用于检测重复的Uid
		/// </summary>
		private HashSet<uint> m_AllocatedAreaUids;
        
		/// <summary>
		/// 收集<see cref="m_Areas"/>时的中间变量，Cache下来减少GC
		/// </summary>
		private List<Area> m_AreasCache;
        
		/// <summary>
		/// 用于：
		///		<see cref="SceneUnit.CaculatePrefabOverrideModification"/>
		/// </summary>
		private List<string> m_StringsCache;
		/// <summary>
		/// 更新了几次
		/// </summary>
		private int m_DoUpdateTimes;
		/// <summary>
		/// 自动分配的AreaUid
		/// </summary>
		private uint m_LastAutoAllocateAreaUid;
		/// <summary>
		/// 这个Map的AABB
		/// </summary>
		public Bounds m_AABB;

		/// <summary>
		/// 星图比例
		/// </summary>
		[Range(0, 1)]
		public float m_StarMapRate = 1;

		/// <summary>
		/// 是否显示星图
		/// </summary>
		public bool debugShowStarMap;

		[EditorExtend.Button("合并AreaSpawner(用于兼容之前的)", "MerageAreaSpawner", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance,
			"ShowMerageAreaSpawner", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance
			)]
	    public bool MergeAreaSpawnerDataTag;

        public Bounds GetAABB()
		{
			return m_AABB;
		}

		public List<Area> GetAreaCache()
		{
			return m_AreasCache;
		}
		

		public float GetStarMapRate()
		{
			return m_StarMapRate;
		}
		

#region AreaSpawner相关
		/// <summary>
		/// 当前Scene
		/// </summary>
		private Scene m_OwnerScene;

        /// <summary>
        /// 用于随机分布的shader
        /// </summary>
        [ReadOnly]
        public ComputeShader m_RandomDisperseMesh;
        /// <summary>
        /// AreaSpawner模板路径
        /// </summary>
        private string m_AreaSpawnerTempletePath;

		public List<AreaSpawner> m_AreaSpawnerCache = null;
		/// <summary>
		/// 已经分配出去的Uid，用于检测重复的Uid
		/// </summary>
		private HashSet<ulong> m_AllocatedAreaSpawnerUids;

		/// <summary>
		/// 用于每帧更新一部分内容
		/// </summary>
		private IEnumerator m_LoadAllAreaEnumerator;

        /// <summary>
        /// 导出3D空间寻路
        /// </summary>
        private IEnumerator m_Export3DNavMeshEnumerator;
		/// <summary>
		/// 对应scene的路径 用于生成Area所在的路径
		/// </summary>
		private string m_OwnerAreaPath;

		/// <summary>
		/// 获取当前地图存储区域的路径
		/// </summary>
		/// <returns></returns>
		public string GetOwnerAreaPath()
		{
			return m_OwnerAreaPath;
		}

        private void InitAreaScenePath()
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
                Selection.activeObject = gameObject;
            }
        }
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
						Map map = rootObjs[iRoot].GetComponent<Map>();
						if (map != null && map == this)
						{
							m_OwnerScene = scene;
							break;
						}
					}
				}
			}
		}

		/// <summary>
		/// 检查一个AreaSpawner的Uid是否已经被分配了
		/// </summary>
		public bool CheckAllocateAreaSpawnerUid(ulong uid)
		{
			if (!m_AllocatedAreaSpawnerUids.Add(uid))
			{
				return false;
			}
			else
			{
				return true;
			}
		}

		public Scene GetOwnerScene()
		{
			if(m_OwnerScene.name == null)
			{
				InitOwnerScene();
			}
            if(m_OwnerScene.name == null || m_OwnerScene.path == null)
            {
                Debug.LogError("获取的当前Scene不对");
            }
			return m_OwnerScene;
		}
		
		private bool ShowMerageAreaSpawner()
		{
			AreaSpawner[] spawnerArray = UnityEngine.Object.FindObjectsOfType<AreaSpawner>();
			if(spawnerArray != null && spawnerArray.Length>0)
			{
				return false;
			}
			return true;
		}
		public void CreateAutoAreaSpawner()
		{
			Map map = GameObject.FindObjectOfType<Map>();
			if (map == null)
			{
				EditorUtility.DisplayDialog("提示", "请先创建Map", "确定");
				return;
			}
			
			//创建AreaSpawner
			ulong areaSpawnerId = TimeUtility.GetTimeStamp();
			bool existSpawner = CheckAllocateAreaSpawnerUid(areaSpawnerId);
			if(!existSpawner)
			{
				EditorUtility.DisplayDialog("提示", "已存在该AreaSpawnerId", "确定");
				return;
			}
			SaveOtherArea();
			AddSpawnerArea(areaSpawnerId);
		}
		
		public void SaveOtherArea()
		{
			AreaSpawner[] areaSpawnerArray = UnityEngine.Object.FindObjectsOfType<AreaSpawner>();
			if(areaSpawnerArray != null && areaSpawnerArray.Length>0)
			{
				for(int iArea =0;iArea<areaSpawnerArray.Length;iArea++)
				{
					AreaSpawner areaSpawner = areaSpawnerArray[iArea];
					if(areaSpawner != null)
					{
						Area area = areaSpawner.GetArea();
						if(area != null && area.gameObject != null)
						{
							SaveArea(areaSpawner);
						}
					}
				}
			}
		}

		private void InitAreaSpawner()
		{
            string[] resAssets = AssetDatabase.FindAssets(string.Format("{0} t:ComputeShader", typeof(BatchRendering.RandomDisperseMesh).Name));
            if (resAssets != null && resAssets.Length > 0)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(resAssets[0]);
                m_RandomDisperseMesh = AssetDatabase.LoadAssetAtPath<ComputeShader>(assetPath);
            }
            m_AreaSpawnerCache = new List<AreaSpawner>();
			m_AllocatedAreaSpawnerUids = new HashSet<ulong>();
			MapEditorSetting mapSetting = MapEditorUtility.GetMapEditorSetting();
			if(mapSetting != null)
			{
				m_AreaSpawnerTempletePath = mapSetting.m_AreaSpawnerTempletePath;
			}
			AreaSpawner[] areaSpawnerArray = gameObject.GetComponentsInChildren<AreaSpawner>();
			if(areaSpawnerArray != null && areaSpawnerArray.Length>0)
			{
				for(int iArea =0; iArea < areaSpawnerArray.Length; iArea++)
				{
					AreaSpawner area = areaSpawnerArray[iArea];
					m_AreaSpawnerCache.Add(area);
				}
			}
			EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyGUI;
		}

		/// <summary>
		/// 初始化AreaSpanwerInfo 为了与之前的合并
		/// </summary>
		private void MerageAreaSpawner()
		{
			bool isMerge = EditorUtility.DisplayDialog("提示", "此功能只是为了合并老编辑器编出来的地图，是否合并?", "确定", "取消");
			if(!isMerge)
			{
				return;
			}
			Map map = UnityEngine.Object.FindObjectOfType<Map>();
			if(map == null)
			{
				return;
			}

			Area[] areaArray = UnityEngine.Object.FindObjectsOfType<Area>();
			//计算场景中存在的Area
			if (areaArray != null && areaArray.Length > 0)
			{
				for (int iArea = 0; iArea < areaArray.Length; iArea++)
				{
					Area area = areaArray[iArea];
					if (area == null)
					{
						continue;
					}

					IEnumerator areaEnumerator = area.DoUpdate_Co(map, -1, false);
					while (areaEnumerator.MoveNext())
					{

					}

					CreateAreaSpawner(area);
				}
			}
		}

		private void CreateAreaSpawner(Area area = null, ulong areaUid = 0)
		{
			Map map = GameObject.FindObjectOfType<Map>();
			if (map == null)
			{
				EditorUtility.DisplayDialog("提示", "请先创建Map", "确定");
				return;
			}

			if(string.IsNullOrEmpty(m_AreaSpawnerTempletePath))
			{
				EditorUtility.DisplayDialog("提示", "请先设置AreaSpawner模板路径", "确定");
				return;
			}
			GameObject areaSpawnerObj = AssetDatabase.LoadAssetAtPath<GameObject>(m_AreaSpawnerTempletePath);
			if(areaSpawnerObj != null)
			{
				GameObject spawnerObj = GameObject.Instantiate(areaSpawnerObj);
				spawnerObj.transform.SetParent(transform);
				AreaSpawner spawner = spawnerObj.AddComponent<AreaSpawner>();
				spawner.Create(this, area, areaUid);
				m_AreaSpawnerCache.Add(spawner);
				Selection.activeObject = spawnerObj;
			}
			AssetDatabase.Refresh();
		}
		

		public void SwitchSpawnerArea(AreaSpawner areaSpawner,bool closeOther = true,bool loadRecast = false)
		{
			if(areaSpawner == null)
			{
				return;
			}
			Area[] areaArray = UnityEngine.Object.FindObjectsOfType<Area>();
			if(areaArray != null && areaArray.Length>0)
			{
				for(int iArea = 0;iArea<areaArray.Length;iArea++)
				{
					Area area = areaArray[iArea];
					if(area != null && area.Uid == areaSpawner.GetAreaId())
					{
						//Debug.LogError(string.Format("{0}Area已导入",area.Uid));
						return;
					}
				}
			}
			if(closeOther)
			{
				SaveOtherArea();
			}
			string areaName = areaSpawner.GetAreaScenePath();
			if (string.IsNullOrEmpty(areaName))
			{
                if(!Application.isBatchMode)
                {
                    EditorUtility.DisplayDialog("提示", "不存在Area场景", "确定");
                }
				return;
			}

			Scene areaScene = EditorSceneManager.OpenScene(areaName, OpenSceneMode.Additive);
			if (areaScene != null)
			{
				SceneManager.SetActiveScene(areaScene);
				GameObject[] rootObjArray = areaScene.GetRootGameObjects();
				if (rootObjArray != null && rootObjArray.Length > 0)
				{
					for (int index = 0; index < rootObjArray.Length; index++)
					{
						SceneManager.MoveGameObjectToScene(rootObjArray[index], this.GetOwnerScene());
						if (index == 0)
						{
							Selection.activeObject = rootObjArray[index];
						}
						Area area = rootObjArray[index].GetComponent<Area>();
						if (area != null)
						{
							areaSpawner.SetArea(area);
                            if(loadRecast)
                            {
                                LoadRecastRoot(area);
                            }
						}
					}

				}
				EditorSceneManager.CloseScene(areaScene, true);
			}

			//TODO:把对应GamingMap的Area也加载进来
			GamingMap[] gamingMaps = UnityEngine.Object.FindObjectsOfType<GamingMap>();
			if(gamingMaps != null && gamingMaps.Length>0)
			{
				for(int iGaming = 0;iGaming<gamingMaps.Length;iGaming++)
				{
					GamingMap map = gamingMaps[iGaming];
					if(map.m_MapId == Uid)
					{
						map.LoadGamingMapArea(areaSpawner);
					}
				}
			}
		}

		public void SaveArea(AreaSpawner areaSpawner)
		{
			if(areaSpawner == null)
			{
				return;
			}
			if (areaSpawner == null)
			{
				EditorUtility.DisplayDialog("提示", "不存在Area", "确定");
				return;
			}
            SaveExtenInfo(areaSpawner.m_AreaUid);
			Area area = areaSpawner.GetArea();
			if(area == null || area.gameObject == null)
			{
				EditorUtility.DisplayDialog("提示", "未加载对应的Area", "确定");
				return;
			}
			string areaName = areaSpawner.GetAreaScenePath();
			if (string.IsNullOrEmpty(areaName))
			{
                if (!Application.isBatchMode)
                {
                    EditorUtility.DisplayDialog("提示", "不存在Area场景", "确定");
                }
                return;
			}
			Scene areaScene = EditorSceneManager.OpenScene(areaName, OpenSceneMode.Additive);
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
				SceneManager.MoveGameObjectToScene(areaSpawner.m_Area.gameObject, areaScene);
				SceneManager.SetActiveScene(areaScene);
			}
			EditorSceneManager.SaveScene(areaScene);
			EditorSceneManager.CloseScene(areaScene, true);
			if(areaSpawner.m_Area != null && areaSpawner.m_Area.gameObject != null)
			{
				GameObject.DestroyImmediate(areaSpawner.m_Area.gameObject);
			}
            //TODO:修改scene名字
            AssetDatabase.RenameAsset(areaName, areaSpawner.gameObject.name);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
		}

		private void AddSpawnerArea(ulong areaId)
		{
			if(m_AreaSpawnerCache == null)
			{
				m_AreaSpawnerCache = new List<AreaSpawner>();
			}
			CreateAreaSpawner(null,areaId);
		}

		public void RemoveSpawnerArea(AreaSpawner areaSpawner)
		{
			AreaSpawner spawner = areaSpawner;
			if (spawner != null)
			{
				if(EditorUtility.DisplayDialog("提示", "是否删除该Area对应的Scene", "确定", "取消"))
				{
					FileUtility.DeleteFile(spawner.GetAreaScenePath());
					AssetDatabase.Refresh();
				}
				m_AreaSpawnerCache.Remove(spawner);
				spawner.Clear();
				GameObject.DestroyImmediate(spawner.gameObject);
				spawner = null;
			}
			

		}

		public void SaveMap()
		{
			SaveOtherArea();
			if (m_OwnerScene != null)
			{
				EditorSceneManager.SaveScene(m_OwnerScene);
			}
		}

		public List<AreaSpawner> GetAreaSpawnerList()
		{
			return m_AreaSpawnerCache;
		}

		public bool CheckCanExport()
		{
			Vector3 voxelCounts = VoxelGrid.GetVoxelCounts();
			int voxelCount = (int)(voxelCounts.x
					* voxelCounts.y
					* voxelCounts.z);
			if (voxelCount <= 0)
			{
				return false;
			}
			return true;
		}

		/// <summary>
		/// 刷新星图
		/// </summary>
		private void RefreshStarMap()
		{
			if(m_AreaSpawnerCache != null && m_AreaSpawnerCache.Count>0)
			{
				for(int iArea = 0;iArea< m_AreaSpawnerCache.Count;iArea++)
				{
					Vector3 relativePos = m_AreaSpawnerCache[iArea].GetAreaPosition() - GetAABB().center;
					m_AreaSpawnerCache[iArea].transform.position = relativePos * m_StarMapRate + GetAABB().center;
				}
			}
		}

        private List<GUIContent> contentList = new List<GUIContent>();
        /// <summary>
        /// 右键响应
        /// </summary>
        /// <param name="instanceID"></param>
        /// <param name="selectionRect"></param>
        private void OnHierarchyGUI(int instanceID,Rect selectionRect)
		{
			if (Event.current != null && selectionRect.Contains(Event.current.mousePosition)
				&& Event.current.button == 1 && Event.current.type<=EventType.MouseUp)
			{
				GameObject selectedGameObject = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
				if(selectedGameObject)
				{
					
					AreaSpawner areaSpawner = selectedGameObject.GetComponent<AreaSpawner>();
					if( areaSpawner != null)
					{

                        //EditorUtility.DisplayPopupMenu(new Rect(mousePosition.x, mousePosition.y, 0, 0), "GameObject/Map/AreaSpawner", null);
                        contentList.Clear();
                        contentList.Add(new GUIContent("导出区域"));
						contentList.Add(new GUIContent("删除区域"));
						if (areaSpawner.GetArea() == null)
						{
							contentList.Add(new GUIContent("导入Area"));
						}
						else
						{
							contentList.Add(new GUIContent("保存Area"));
						}
						RefreshAreaSpawnerUI(contentList, areaSpawner);

					}
					else
					{
						Area area = selectedGameObject.GetComponent<Area>();
						if(area != null)
						{
                            contentList.Clear();
                            contentList.Add(new GUIContent("导出区域"));
							contentList.Add(new GUIContent("删除区域"));
							contentList.Add(new GUIContent("保存Area"));
							contentList.Add(new GUIContent("定位AreaScene"));
							contentList.Add(new GUIContent("创建/RandomDisperseMesh"));
                            contentList.Add(new GUIContent("创建/跃迁点"));
                            contentList.Add(new GUIContent("寻宝/创建寻宝节点"));
                            contentList.Add(new GUIContent("寻宝/导入寻宝节点"));
                            contentList.Add(new GUIContent("寻宝/保存寻宝节点"));
                            contentList.Add(new GUIContent("寻宝/删除寻宝节点"));
                            contentList.Add(new GUIContent("挖矿/创建挖矿节点"));
                            contentList.Add(new GUIContent("挖矿/导入挖矿节点"));
                            contentList.Add(new GUIContent("挖矿/保存挖矿节点"));
                            contentList.Add(new GUIContent("挖矿/删除挖矿节点"));
                            contentList.Add(new GUIContent("Recast区域/创建"));
                            contentList.Add(new GUIContent("Recast区域/导入"));
                            contentList.Add(new GUIContent("Recast区域/保存"));
                            contentList.Add(new GUIContent("Recast区域/删除"));
                            RefreshAreaSpawnerUI(contentList, area.GetAreaSpawner());
						}
						Map map = selectedGameObject.GetComponent<Map>();
						if(map != null)
						{
                            contentList.Clear();
                            contentList.Add(new GUIContent("导出所有区域"));
							contentList.Add(new GUIContent("创建AreaSpawner"));
							contentList.Add(new GUIContent("保存Map"));
							contentList.Add(new GUIContent("导入所有区域"));
							contentList.Add(new GUIContent("加载对应的GamingMap"));
                            contentList.Add(new GUIContent("导出碰撞信息"));
                            contentList.Add(new GUIContent("清除无用unit"));
                            string []sceneSizes = System.Enum.GetNames(typeof(RootScenceRecast.ROOT_SCENCE_CUT_SIZE));
                            if(sceneSizes != null && sceneSizes.Length>0)
                            {
                                for(int iScene = 0;iScene<sceneSizes.Length;iScene++)
                                {
                                    contentList.Add(new GUIContent(string.Format("导出空间寻路/{0}", sceneSizes[iScene])));
                                }
                            }
                            
                            RefreshMapUI(contentList, map);
						}

                        ///挖矿节点
                        MineralRootMark mineralMark = selectedGameObject.GetComponent<MineralRootMark>();
                        if(mineralMark != null)
                        {
                            contentList.Clear();
                            contentList.Add(new GUIContent("保存"));
                            RefreshMineralRoot(contentList, mineralMark);
                        }

                        ///寻宝节点
                        TreasureRootMark treasureMark = selectedGameObject.GetComponent<TreasureRootMark>();
                        if(treasureMark != null)
                        {
                            contentList.Clear();
                            contentList.Add(new GUIContent("保存"));
                            RefreshTreasureRoot(contentList, treasureMark);
                        }

                        //Recast区域节点
                        RecastRootMark recastMark = selectedGameObject.GetComponent<RecastRootMark>();
                        if (recastMark != null)
                        {
                            contentList.Clear();
                            contentList.Add(new GUIContent("保存"));
                            contentList.Add(new GUIContent("创建Recast"));
                            RefreshRecastRoot(contentList, recastMark);
                        }
                    }
				}
			}
		}

#region 挖矿/寻宝相关

        private string GetMapPath()
        {
            return "Assets\\Packages\\Map\\Edit";
            //Scene scene = gameObject.scene;
            //if(!scene.IsValid())
            //{
            //    return "";
            //}
            //return scene.path.Replace(".unity","");
        }
        /// <summary>
        /// 载入挖矿节点
        /// </summary>
        private void LoadMineralRoot(Area area)
        {
            LoadRoot<MineralRootMark>(area);
        }

        private void LoadRoot<T>(Area area) where T : IRootMark
        {
            if (area == null)
            {
                return;
            }
            T[] childs = area.GetComponentsInChildren<T>();
            if(childs != null && childs.Length>0)
            {
                return;
            } 
            string mapPath = GetMapPath();
            if (string.IsNullOrEmpty(mapPath))
            {
                return;
            }
            string[] prefabs = AssetDatabase.FindAssets("t: Prefab", new string[] { mapPath });
            if (prefabs != null && prefabs.Length > 0)
            {
                for (int iPrefab = 0; iPrefab < prefabs.Length; iPrefab++)
                {
                    string assetPath = AssetDatabase.GUIDToAssetPath(prefabs[iPrefab]);
                    GameObject obj = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                    if (obj != null)
                    {
                        T mark = obj.GetComponent<T>();
                        if (mark != null && mark.m_AreaId == area.Uid)
                        {
                            GameObject markObj = UnityEditor.PrefabUtility.InstantiatePrefab(obj) as GameObject;
                            markObj.transform.SetParent(area.transform);
                            markObj.transform.localPosition = mark.m_RelativeAreaPos;
                            markObj.transform.localRotation = mark.m_RelativeAreaRot;
                            markObj.transform.localScale = mark.m_RelativeAreaScale;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 删除节点
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="areaId"></param>
        /// <param name="root"></param>
        private void DeleteMarkRoot<T>(Area area) where T:IRootMark
        {
            T[] roots = area.GetComponentsInChildren<T>();
            if(roots != null && roots.Length>0)
            {
                for(int iRoot = roots.Length-1;iRoot>=0;iRoot--)
                {
                    GameObject.DestroyImmediate(roots[iRoot].gameObject);
                }
            }
            string mapPath = GetMapPath();
            if (string.IsNullOrEmpty(mapPath))
            {
                return;
            }
            string rootName = "";
            if (typeof(T) == typeof(MineralRootMark))
            {
                rootName = string.Format("{0}_挖矿节点", area.name);
            }
            else if (typeof(T) == typeof(TreasureRootMark))
            {
                rootName = string.Format("{0}_寻宝节点", area.name);
            }
            else if (typeof(T) == typeof(RecastRootMark))
            {
                rootName = string.Format("{0}_Recast区域", area.name);
            }
            string prefabPath = string.Format("{0}/{1}.prefab", mapPath, rootName);
            AssetDatabase.DeleteAsset(prefabPath);
        }

        private void SaveMarkRoot<T>(ulong areaId, GameObject root = null) where T:IRootMark
        {
            Area[] areas = FindObjectsOfType<Area>();
            if(areas == null || areas.Length<=0)
            {
                return;
            }
            Area area = null;
            for(int iArea =0;iArea<areas.Length;iArea++)
            {
                if(areas[iArea].Uid == areaId)
                {
                    area = areas[iArea];
                    break;
                }
            }
            if (area == null)
            {
                return;
            }
            if(root == null)
            {
                T markRoot = area.GetComponentInChildren<T>();
                if(markRoot != null)
                {
                    root = markRoot.gameObject;
                }
            }
            if(root == null)
            {
                return;
            }
            string mapPath = GetMapPath();
            if (string.IsNullOrEmpty(mapPath))
            {
                return;
            }
            if (typeof(T)== typeof(MineralRootMark))
            {
                root.name = string.Format("{0}_挖矿节点", area.name);
            }
            else if(typeof(T) == typeof(TreasureRootMark))
            {
                root.name = string.Format("{0}_寻宝节点", area.name);
            }
            else if(typeof(T) == typeof(RecastRootMark))
            {
                root.name = string.Format("{0}_Recast区域", area.name);
            }
            string prefabPath = string.Format("{0}/{1}.prefab", mapPath, root.name);
            GameObject rootObj = UnityEditor.PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            if(rootObj != null)
            {
                GameObject.DestroyImmediate(root);
            }
            
        }

        /// <summary>
        /// 保存挖矿节点
        /// </summary>
        private void SaveMineralRoot(ulong areaId,GameObject root = null)
        {
            SaveMarkRoot<MineralRootMark>(areaId,root);
        }

        private void DeleteMineralRoot(Area area)
        {
            DeleteMarkRoot<MineralRootMark>(area);
        }
        

        /// <summary>
        /// 载入寻宝节点
        /// </summary>
        private void LoadTreasureRoot(Area area)
        {
            LoadRoot<TreasureRootMark>(area);
        }

        /// <summary>
        /// 保存寻宝节点
        /// </summary>
        private void SaveTreasureRoot(ulong areaId,GameObject root = null)
        {
            SaveMarkRoot<TreasureRootMark>(areaId, root);
        }

        private void DeleteTresureRoot(Area area)
        {
            DeleteMarkRoot<TreasureRootMark>(area);
        }
        /// <summary>
        /// 刷新挖矿节点
        /// </summary>
        /// <param name="contentList"></param>
        /// <param name="mineralMark"></param>
        private void RefreshMineralRoot(List<GUIContent> contentList, MineralRootMark mineralMark)
        {
            if (mineralMark == null)
            {
                return;
            }

            Vector2 mousePosition = Event.current.mousePosition;
            GameObject userData = Selection.activeGameObject;
            int selected = -1;
            EditorUtility.DisplayCustomMenu(new Rect(mousePosition.x, mousePosition.y, 0, 0), contentList.ToArray(), selected,
                delegate (object data, string[] opt, int select)
                {
                    switch (select)
                    {
                        case 0://保存
                            SaveMineralRoot(mineralMark.m_AreaId, mineralMark.gameObject);
                            break;
                    }
                }, userData);
            Event.current.Use();
        }

        /// <summary>
        /// 刷新Recast节点 
        /// </summary>
        /// <param name="contentList"></param>
        /// <param name="treasureMark"></param>
        private void RefreshRecastRoot(List<GUIContent> contentList, RecastRootMark recastMark)
        {
            if (recastMark == null)
            {
                return;
            }

            Vector2 mousePosition = Event.current.mousePosition;
            GameObject userData = Selection.activeGameObject;
            int selected = -1;
            EditorUtility.DisplayCustomMenu(new Rect(mousePosition.x, mousePosition.y, 0, 0), contentList.ToArray(), selected,
                delegate (object data, string[] opt, int select)
                {
                    switch (select)
                    {
                        case 0://保存
                            SaveRecastRoot(recastMark.m_AreaId, recastMark.gameObject);
                            break;
                        case 1://创建Recast
                            GameObject region = RecastRegionInfo.CreateRegion();
                            if(region != userData)
                            {
                                region.transform.SetParent(recastMark.transform);
                            }
                            break;
                    }
                }, userData);
            Event.current.Use();
        }

        /// <summary>
        /// 刷新寻宝节点 
        /// </summary>
        /// <param name="contentList"></param>
        /// <param name="treasureMark"></param>
        private void RefreshTreasureRoot(List<GUIContent> contentList, TreasureRootMark treasureMark)
        {
            if (treasureMark == null)
            {
                return;
            }

            Vector2 mousePosition = Event.current.mousePosition;
            GameObject userData = Selection.activeGameObject;
            int selected = -1;
            EditorUtility.DisplayCustomMenu(new Rect(mousePosition.x, mousePosition.y, 0, 0), contentList.ToArray(), selected,
                delegate (object data, string[] opt, int select)
                {
                    switch (select)
                    {
                        case 0://保存
                            SaveTreasureRoot(treasureMark.m_AreaId, treasureMark.gameObject);
                            break;
                    }
                }, userData);
            Event.current.Use();
        }
#endregion

        /// <summary>
        /// 导入所有Area
        /// </summary>
        public IEnumerator LoadAllArea(bool loadRecast = false)
		{
			yield return null;
			if(m_AreaSpawnerCache != null && m_AreaSpawnerCache.Count>0)
			{
				for(int iArea = 0;iArea<m_AreaSpawnerCache.Count;iArea++)
				{
					m_AreaSpawnerCache[iArea].LoadArea(false, loadRecast);
					yield return null;
				}
			}
		}

        /// <summary>
        /// 清除无用unit
        /// </summary>
        private void CleanUnUsedUnit()
        {
            IEnumerator clearEnumerator = new Exporter().ClearUnUsedUnit(this.Uid);
            while (clearEnumerator.MoveNext())
            {

            }
        }

		private void RefreshMapUI(List<GUIContent> contentList, Map map)
		{
			if(map == null)
			{
				return;
			}

			Vector2 mousePosition = Event.current.mousePosition;
			GameObject userData = Selection.activeGameObject;
			int selected = -1;
			EditorUtility.DisplayCustomMenu(new Rect(mousePosition.x, mousePosition.y, 0, 0), contentList.ToArray(), selected,
				delegate (object data, string[] opt, int select)
				{
					switch (select)
					{
						case 0:
							map.ExportMap();
							break;
						case 1:
							map.CreateAutoAreaSpawner();
							break;
						case 2:
							map.SaveMap();
							break;
						case 3:
						    m_LoadAllAreaEnumerator = LoadAllArea();
							break;
						case 4://加载对应的GamingMap
							map.LoadGamingMap();
                            break;
                        case 5://导出collider
                            //map.BeginExportColliders();
                            new ExportMapColliders().BeginExport(map);
							break;
                        case 6://清除对应map无用unit
                            CleanUnUsedUnit();
                            break;
                        default:
                            if(select>=7)
                            {
                                int[] sceneSizes = (int[])System.Enum.GetValues(typeof(RootScenceRecast.ROOT_SCENCE_CUT_SIZE));
                                int sceneSelect = select - 7;
                                if (sceneSizes != null && sceneSizes.Length > sceneSelect && sceneSelect>=0)
                                {
                                    // Export3DNavMesh((RootScenceRecast.ROOT_SCENCE_CUT_SIZE)sceneSizes[sceneSelect], map);
                                    new Export3DNavMesh().BeginExport(map, (RootScenceRecast.ROOT_SCENCE_CUT_SIZE)sceneSizes[sceneSelect]);
                                }
                            }
                            break;
					}
				}, userData);
			Event.current.Use();
		}
        
        /// <summary>
        /// 加载与之对应的GamingMap
        /// </summary>
        public void LoadGamingMap()
		{
			List<EditorGamingMap> needLoadList = new List<EditorGamingMap>();
			List<EditorGamingMap> gamingMapList = EditorGamingMapData.LoadAllGamingMapJson();
			if(gamingMapList != null && gamingMapList.Count>0)
			{
				for(int iGaming = 0;iGaming<gamingMapList.Count;iGaming++)
				{
					EditorGamingMap gamingMap = gamingMapList[iGaming];
					if (gamingMap.mapId == this.Uid)
					{
						needLoadList.Add(gamingMap);
					}
				}
			}
			
			string jsonPath = "";
			GamingMapEditorSetting gamingSetting = GamingMapEditorUtility.GetGamingMapEditorSetting();
			if (gamingSetting != null)
			{
				jsonPath = gamingSetting.m_GamingMapPath;
			}

			if (needLoadList != null && needLoadList.Count>0)
			{
				for(int iNeed = 0;iNeed<needLoadList.Count;iNeed++)
				{
					string gamingMapPath = string.Format("{0}/GamingMap_{1}.unity", jsonPath, needLoadList[iNeed].gamingmapId);
					EditorSceneManager.OpenScene(gamingMapPath, OpenSceneMode.Additive);
				}
			}
			else
			{
				EditorUtility.DisplayDialog("提示", "无关联的GamingMap", "确定");
			}
		}

        public void LoadExtenInfo(Area area)
        {
            LoadMineralRoot(area);
            LoadTreasureRoot(area);
            LoadRecastRoot(area);
        }

        public void SaveExtenInfo(ulong areaid)
        {
            SaveMineralRoot(areaid);
            SaveTreasureRoot(areaid);
            SaveRecastRoot(areaid);
        }

		private void RefreshAreaSpawnerUI(List<GUIContent> contentList,AreaSpawner areaSpawner)
		{
			if(areaSpawner == null)
			{
				return;
			}
			Vector2 mousePosition = Event.current.mousePosition;
			GameObject userData = Selection.activeGameObject;
			int selected = -1;
			EditorUtility.DisplayCustomMenu(new Rect(mousePosition.x, mousePosition.y, 0, 0), contentList.ToArray(), selected,
				delegate (object data, string[] opt, int select)
				{
					switch (select)
					{
						case 0:
							areaSpawner.ExportArea();
							break;
						case 1:
							areaSpawner.DelArea();
							break;
						case 2:
							if(opt[select].Equals("保存Area"))
							{
								areaSpawner.SaveArea();
							}
							else if(opt[select].Equals("导入Area"))
							{
								areaSpawner.LoadArea();
							}
							break;
						case 3://定位AreaScene
							SelectAreaScene(areaSpawner);
							break;
                        case 4://创建RandomDisperseMesh
                            CreateRandomDisperseMesh(areaSpawner.GetArea());
                            break;
                        case 5://创建跃迁点
                            CreateLeap(areaSpawner.GetArea());
                            break;
                        case 6://创建寻宝节点
                            CreateTreasureRoot(areaSpawner.GetArea());
                            break;
                        case 7://导入寻宝节点
                            LoadTreasureRoot(areaSpawner.GetArea());
                            break;
                        case 8://保存寻宝节点
                            SaveTreasureRoot(areaSpawner.m_AreaUid);
                            break;
                        case 9://删除寻宝节点
                            DeleteTresureRoot(areaSpawner.GetArea());
                            break;
                        case 10://创建挖矿节点
                            CreateMineralRoot(areaSpawner.GetArea());
                            break;
                        case 11://导入挖矿节点
                            LoadMineralRoot(areaSpawner.GetArea());
                            break;
                        case 12://保存挖矿节点
                            SaveMineralRoot(areaSpawner.m_AreaUid);
                            break;
                        case 13://删除挖矿节点
                            DeleteMineralRoot(areaSpawner.GetArea());
                            break;
                        case 14://创建Recast区域
                            CreateRecastRoot(areaSpawner.GetArea());
                            break;
                        case 15://导入Recast区域
                            LoadRecastRoot(areaSpawner.GetArea());
                            break;
                        case 16://保存Recast区域
                            SaveRecastRoot(areaSpawner.m_AreaUid);
                            break;
                        case 17://删除Recast区域
                            DeleteRecastRoot(areaSpawner.GetArea());
                            break;
                    }
				}, userData);
			Event.current.Use();
		}

        /// <summary>
        /// 创建Recast区域节点
        /// </summary>
        private void CreateRecastRoot(Area area)
        {
            if(area == null)
            {
                return;
            }
            LoadRecastRoot(area);
            RecastRootMark mark = area.gameObject.GetComponentInChildren<RecastRootMark>();
            if (mark == null)
            {
                GameObject recastRoot = new GameObject();
                recastRoot.AddComponent<RecastRootMark>();
                recastRoot.transform.SetParent(area.transform);
                recastRoot.transform.localPosition = Vector3.zero;
                recastRoot.transform.localScale = Vector3.one;
                recastRoot.transform.localRotation = Quaternion.identity;
                recastRoot.name = string.Format("{0}_Recast区域", area.name);
            }
                
        }
        /// <summary>
        /// 导入Recast区域节点
        /// </summary>
        private void LoadRecastRoot(Area area)
        {
            LoadRoot<RecastRootMark>(area);
        }
        /// <summary>
        /// 保存Recast区域节点
        /// </summary>
        private void SaveRecastRoot(ulong areaId, GameObject root = null)
        {
            SaveMarkRoot<RecastRootMark>(areaId, root);
        }

        private void DeleteRecastRoot(Area area)
        {
            DeleteMarkRoot<RecastRootMark>(area);
        }

        private void CreateMineralRoot(Area area)
        {
            LoadMineralRoot(area);
            if (area == null || area.gameObject == null)
            {
                return;
            }
            MineralRootMark mark = area.gameObject.GetComponentInChildren<MineralRootMark>();
            if(mark == null)
            {
                GameObject rootObj = new GameObject(typeof(MineralRootMark).Name);
                rootObj.transform.SetParent(area.transform);
                rootObj.transform.localPosition = Vector3.zero;
                rootObj.transform.localRotation = Quaternion.identity;
                rootObj.name = "挖矿节点";
                mark = rootObj.AddComponent<MineralRootMark>();
            }
            Selection.activeObject = mark.gameObject;
        }

        private void CreateTreasureRoot(Area area)
        {
            if (area == null || area.gameObject == null)
            {
                return;
            }
            LoadTreasureRoot(area);
            TreasureRootMark mark = area.gameObject.GetComponentInChildren<TreasureRootMark>();
            if (mark == null)
            {
                GameObject rootObj = new GameObject(typeof(TreasureRootMark).Name);
                rootObj.transform.SetParent(area.transform);
                rootObj.transform.localPosition = Vector3.zero;
                rootObj.transform.localRotation = Quaternion.identity;
                rootObj.name = "寻宝节点";
                mark = rootObj.AddComponent<TreasureRootMark>();
            }
            Selection.activeObject = mark.gameObject;
        }

        /// <summary>
        /// 创建随机分布
        /// </summary>
        /// <param name="areaSpawner"></param>
        private void CreateRandomDisperseMesh(Area area)
        {
            if(area == null || area.gameObject == null)
            {
                return;
            }
            GameObject randomObj = new GameObject(typeof(BatchRendering.RandomDisperseMesh).Name);
            randomObj.transform.SetParent(area.transform);
            randomObj.transform.localPosition = Vector3.zero;
            randomObj.transform.localRotation = Quaternion.identity;
            BatchRendering.RandomDisperseMesh randomMesh = randomObj.AddComponent<BatchRendering.RandomDisperseMesh>();
            randomMesh.ComputeShader = m_RandomDisperseMesh;
            Selection.activeObject = randomObj;
        }

        /// <summary>
        /// 创建跃迁点
        /// </summary>
        private void CreateLeap(Area area)
        {
            if (area == null || area.gameObject == null)
            {
                return;
            }
            if(area.m_Leap != null)
            {
                Selection.activeObject = area.m_Leap;
                return;
            }
            GameObject leapObj = new GameObject(typeof(MapLeap).Name);
            leapObj.transform.SetParent(area.transform);
            leapObj.transform.localPosition = Vector3.zero;
            leapObj.transform.localRotation = Quaternion.identity;
            MapLeap mapLeap = leapObj.AddComponent<MapLeap>();
            Selection.activeObject = mapLeap;
        }

		private void SelectAreaScene(AreaSpawner areaSpawner)
		{
			if(areaSpawner == null)
			{
				return;
			}
			string areaScenePath = areaSpawner.GetAreaScenePath();
            SceneAsset sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(areaScenePath);
            if(sceneAsset != null)
            {
                EditorGUIUtility.PingObject(sceneAsset);
                Selection.activeObject = sceneAsset;
            }
		}
#endregion
        

		/// <summary>
		/// 检查一个Area的Uid是否已经被分配了
		/// </summary>
		public bool CheckAllocateAreaUid(uint uid, GameObject context)
		{
			if (!m_AllocatedAreaUids.Add(uid))
			{
				Debug.LogError(string.Format("({0})的Uid({1})重复", ObjectUtility.CalculateTransformPath(context.transform), uid), context);
				return false;
			}
			else
			{
				return true;
			}
		}
        

		public List<string> GetStringsCache()
		{
			DebugUtility.Assert(m_StringsCache.Count == 0, "m_StringsCache.Count == 0");
			return m_StringsCache;
		}

		public int GetDoUpdateTimes()
		{
			return m_DoUpdateTimes;
		}

		public void BeginExportMap()
		{
			EditorApplication.update -= OnUpdate;

			m_AllocatedAreaUids.Clear();
			m_AreasCache.Clear();

			m_AllocatedAreaSpawnerUids.Clear();
		}

		public void EndExportMap()
		{
			EditorApplication.update += OnUpdate;
			m_DoUpdateEnumerator = _DoUpdate_Co();
		}

		/// <summary>
		/// 自动分配一个Area的Uid
		/// </summary>
		public uint AutoAllocateAreaUid()
		{
			return m_LastAutoAllocateAreaUid++;
		}

		protected void OnEnable()
		{
			// 这个脚本只在编辑器下使用
			if (Application.isPlaying)
			{
				Destroy(this);
				return;
			}
			m_AreasCache = new List<Area>();
			m_AllocatedAreaUids = new HashSet<uint>();
			m_StringsCache = new List<string>();
			
			m_DoUpdateTimes = 0;
			m_DoUpdateEnumerator = _DoUpdate_Co();
			EditorApplication.update += OnUpdate;
			
			InitAreaSpawner();
		}

		

		protected void OnDisable()
		{
			EditorApplication.update -= OnUpdate;

			if (m_AllocatedAreaUids != null)
			{
				m_AllocatedAreaUids.Clear();
				m_AllocatedAreaUids = null;
			}

			if (m_AreasCache != null)
			{
				m_AreasCache.Clear();
				m_AreasCache = null;
			}

			if (m_StringsCache != null)
			{
				m_StringsCache.Clear();
				m_StringsCache = null;
			}
			m_DoUpdateEnumerator = null;

			if(m_AllocatedAreaSpawnerUids != null)
			{
				m_AllocatedAreaSpawnerUids.Clear();
				m_AllocatedAreaSpawnerUids = null;
			}
			EditorApplication.hierarchyWindowItemOnGUI -= OnHierarchyGUI;

		}

		protected void OnDrawGizmosSelected()
		{
			// HACK 为了能实时在Inspector面板上的Debug中看到Voxel信息，所以在这里也Update
			VoxelGrid.DoUpdate(m_AABB);
			_Debug.DoUpdate(this);
			if(_DebugShowMapAB)
			{
				GizmosUtility.DrawBounds(GetAABB(), _DebugMapABColor);
			}
			
		}

		/// <summary>
		/// 导出Map
		/// </summary>
		public ExporterHandle ExportMap()
		{
            ExporterHandle exporterHandle = null;
            if (m_AreaSpawnerCache != null && m_AreaSpawnerCache.Count > 0)
            {
                exporterHandle = new Exporter().BeginExport(this, ExportSetting, MapEditorUtility.CreateExportParameter(), m_AreaSpawnerCache, true);
            }
            return exporterHandle;
        }

		private void OnUpdate()
		{
			if(m_DoUpdateEnumerator != null)
			{
				m_DoUpdateEnumerator.MoveNext();
			}

			if(m_LoadAllAreaEnumerator != null)
			{
				if(!m_LoadAllAreaEnumerator.MoveNext())
				{
					m_LoadAllAreaEnumerator = null;
				}
			}

            if(m_ExportColliderEnumerator != null)
            {
                if(!m_ExportColliderEnumerator.MoveNext())
                {
                    m_ExportColliderEnumerator = null;
                }
            }

            if(m_Export3DNavMeshEnumerator != null)
            {
                if(!m_Export3DNavMeshEnumerator.MoveNext())
                {
                    m_Export3DNavMeshEnumerator = null;
                }
            }
		}

		private IEnumerator _DoUpdate_AreaSpawners(bool isExporting)
		{
			Bounds mapAABB = new Bounds();
			float minAreaDiameter = float.MaxValue;

			if (m_AreaSpawnerCache != null && m_AreaSpawnerCache.Count > 0)
			{
				for(int iArea = 0;iArea<m_AreaSpawnerCache.Count;iArea++)
				{
					AreaSpawner iterAreaSpawner = m_AreaSpawnerCache[iArea];
					if(iterAreaSpawner != null)
					{
						IEnumerator areaSpawnerEnumerator = iterAreaSpawner.DoUpdate(this, isExporting);
						while (iterAreaSpawner != null && iterAreaSpawner.gameObject != null && areaSpawnerEnumerator.MoveNext())
						{
							if (!isExporting)
							{
								yield return null;
							}
						}

						Bounds areaAABB = iterAreaSpawner.GetAABB();
						if (mapAABB.min == Vector3.zero && mapAABB.max == Vector3.zero)
						{
							mapAABB.SetMinMax(areaAABB.min, areaAABB.max);
						}
						else
						{
							mapAABB.Encapsulate(areaAABB);
						}

						float diameter = MathUtility.CaculateLongestSide(areaAABB);
						minAreaDiameter = minAreaDiameter < diameter ? minAreaDiameter : diameter;
					}
				}
				
				m_AABB = mapAABB;

				if (AutoVoxelSize)
				{
					int autoVoxelSize = (int)(RendererUtility.CacluateToCameraDistance(minAreaDiameter
							, Constants.AREA_MIN_DISPLAY_RELATIVE_SIZE
							, RendererUtility.CaculateHalfTanCameraFov(ExpectedFov))
						* Constants.AUTO_MAP_VOXELSIZE_COEFFICIENT);
					Vector3 voxelCounts = MathUtility.EachCeilToInt(m_AABB.size / autoVoxelSize);
					if (voxelCounts.x * voxelCounts.y * voxelCounts.z > Constants.MAX_MAP_VOXELGRID_AUTO_VOXEL_COUNT)
					{
						autoVoxelSize = (int)Mathf.Pow(m_AABB.size.x * m_AABB.size.y * m_AABB.size.z / Constants.MAX_MAP_VOXELGRID_AUTO_VOXEL_COUNT, 0.3334f);
					}
					VoxelGrid.VoxelSize = autoVoxelSize;
				}
			}

		}

		private IEnumerator _DoUpdate_Areas(bool isExporting)
		{
			Bounds mapAABB = new Bounds();
			float minAreaDiameter = float.MaxValue;
            yield return null;
            m_AABB = mapAABB;

			if (AutoVoxelSize)
			{
				int autoVoxelSize = (int)(RendererUtility.CacluateToCameraDistance(minAreaDiameter
						, Constants.AREA_MIN_DISPLAY_RELATIVE_SIZE
						, RendererUtility.CaculateHalfTanCameraFov(ExpectedFov))
					* Constants.AUTO_MAP_VOXELSIZE_COEFFICIENT);
				Vector3 voxelCounts = MathUtility.EachCeilToInt(m_AABB.size / autoVoxelSize);
				if (voxelCounts.x * voxelCounts.y * voxelCounts.z > Constants.MAX_MAP_VOXELGRID_AUTO_VOXEL_COUNT)
				{
					autoVoxelSize = (int)Mathf.Pow(m_AABB.size.x * m_AABB.size.y * m_AABB.size.z / Constants.MAX_MAP_VOXELGRID_AUTO_VOXEL_COUNT, 0.3334f);
				}
				VoxelGrid.VoxelSize = autoVoxelSize;
			}
		}

		internal IEnumerator _DoUpdate_Co(bool isExporting = false)
		{
            yield return null;
            //不放在OnEnable里初始化是因为 OnEnable时 当前Scene还未加载完毕
            InitAreaScenePath();
            
            while (true)
			{
				if (transform.parent != null)
				{
					Debug.LogError(string.Format("Map({0})必须是Scene的根节点", name));
				}

				m_AllocatedAreaUids.Clear();
				

				gameObject.name = Constants.MAP_GAMEOBJECT_NAME_STARTWITHS + Uid;
				transform.position = Vector3.zero;
				transform.rotation = Quaternion.identity;
				transform.localScale = Vector3.one;
				m_LastAutoAllocateAreaUid = Constants.AUTO_ALLOCATE_AREA_UID_BEGIN;
                yield return null;
                m_AllocatedAreaSpawnerUids.Clear();
                IEnumerator areaSpawnersEnumerator = _DoUpdate_AreaSpawners(isExporting);
                while (areaSpawnersEnumerator.MoveNext())
                {
                    yield return null;
                }
                RefreshStarMap();

				if (VoxelGrid != null)
				{
					VoxelGrid.DoUpdate(m_AABB);
				}
				

				if (!isExporting) // 导出时不需要Debug
				{
					_Debug.DoUpdate(this);
				}

				m_DoUpdateTimes++;
			}
		}

		/// <summary>
		/// Debug的信息
		/// </summary>
		[System.Serializable]
		public struct ForDebug
		{
			/// <summary>
			/// Map的直径
			/// </summary>
			[Tooltip("map直径")]
			public float Diameter;
			/// <summary>
			/// 1米的Unit在多远的距离能被看到
			/// </summary>
			[Tooltip("1米的Area能被看见的距离")]
			public float AreaOneMeterDisplayDistance;
			/// <summary>
			/// 1米的Unit在多远的距离能被看到
			/// </summary>
			[Tooltip("1米的Unit能被看见的距离")]
			public float UnitOneMeterDisplayDistance;
			[Tooltip("格子调试")]
			public VoxelDebug VoxelDebug;
			/// <summary>
			/// 在编辑器中看的备注，不是给玩家看的
			/// </summary>
			[TextArea]
			public string Remark;

			public void DoUpdate(Map owner)
			{
				Diameter = MathUtility.CaculateDiagonal(owner.GetAABB());
				float halfTanCameraFov = RendererUtility.CaculateHalfTanCameraFov(owner.ExpectedFov);
				AreaOneMeterDisplayDistance = RendererUtility.CacluateToCameraDistance(1, Constants.AREA_MIN_DISPLAY_RELATIVE_SIZE, halfTanCameraFov);
				UnitOneMeterDisplayDistance = RendererUtility.CacluateToCameraDistance(1, Constants.UNIT_MIN_DISPLAY_RELATIVE_SIZE, halfTanCameraFov);
				if(VoxelDebug != null)
				{
					VoxelDebug.DoUpdate(owner.VoxelGrid, owner.gameObject);
				}
				
			}
		}
	}
}
#endif