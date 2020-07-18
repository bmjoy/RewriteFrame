//#define USE_SPILITAREA
#if UNITY_EDITOR
using BatchRendering;
using Leyoutech.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Map
{
	/// <summary>
	/// MapEditor的工具类
	/// </summary>
	public static class MapEditorUtility
	{
		/// <summary>
		/// Cache在这是避免每次<see cref="GetMapEditorSetting"/>去加载资源
		/// 不要直接用这个变量，通过<see cref="GetMapEditorSetting"/>来获取
		/// </summary>
		private static MapEditorSetting m_MapEditorSetting = null;

		/// <summary>
		/// 获取<see cref="MapEditorSetting"/>，如果不存在则创建
		/// </summary>
		/// <returns></returns>
		public static MapEditorSetting GetOrCreateMapEditorSetting()
		{
			if (GetMapEditorSetting() == null)
			{
				CreateMapEditorSetting();
			}
			return m_MapEditorSetting;
		}

		/// <summary>
		/// 会先从Prefs中存的路径加载<see cref="MapEditorSetting"/>，如果不存在就从整个项目中寻找
		/// 寻找的结果会Cache下来，所以每帧调用这个函数不会有性能问题
		/// 找不到时返回null
		/// </summary>
		public static MapEditorSetting GetMapEditorSetting()
		{
			if (m_MapEditorSetting)
			{
				return m_MapEditorSetting;
			}

			if (!LoadMapEditorSetting(EditorPrefs.GetString(Constants.MAP_EDITOR_SETTING_PATH_PREFSKEY)))
			{
				string[] assets = AssetDatabase.FindAssets("t:MapEditorSetting");
				if (assets.Length > 0
					&& LoadMapEditorSetting(AssetDatabase.GUIDToAssetPath(assets[0])))
				{
					EditorPrefs.SetString(Constants.MAP_EDITOR_SETTING_PATH_PREFSKEY
						, assets[0]);
				}
			}
			return m_MapEditorSetting;
		}

        public static List<T> FindChilds<T>(Transform trans,string name) where T : Component
        {
            List<T> childs = new List<T>();
            T[] childrens = trans.GetComponentsInChildren<T>();
            if(childrens != null && childrens.Length>0)
            {
                for(int iChild =0;iChild<childrens.Length;iChild++)
                {
                    T child = childrens[iChild];
                    if(child.name.Equals(name))
                    {
                        childs.Add(child);
                    }
                }
            }
            return childs;
        }

		/// <summary>
		/// 在指定路径创建<see cref="MapEditorSetting"/>并将结果存到<see cref="m_MapEditorSetting"/>
		/// </summary>
		public static MapEditorSetting CreateMapEditorSetting(string directory)
		{
			string path = string.Format("{0}/{1}.asset", directory, Constants.MAP_EDITOR_SETTING_DEFAULT_FILENAME);
			AssetDatabase.CreateAsset(new MapEditorSetting(), path);
			EditorPrefs.SetString(Constants.MAP_EDITOR_SETTING_PATH_PREFSKEY, path);
			return GetMapEditorSetting();
		}

		public static bool CreateAssetAtExportDirectory<T>(ref T obj, string folderName, string fileName) where T : UnityEngine.Object
		{
			return CreateAssetAtExportDirectory(ref obj, string.Format("{0}/{1}", folderName, fileName));
		}

        /// <summary>
        /// 粘贴组件
        /// </summary>
        /// <param name="com"></param>
        /// <param name="targetObj"></param>
        public static void DoPasteComponent(Component com, GameObject targetObj)
        {
            if (com == null || targetObj == null)
            {
                return;
            }
            UnityEditorInternal.ComponentUtility.CopyComponent(com);
            Component oldComponent = targetObj.GetComponent(com.GetType());
            if (oldComponent != null)
            {
                if (!UnityEditorInternal.ComponentUtility.PasteComponentValues(oldComponent))
                {
                    Debug.LogError("Paste Values " + com.GetType().ToString() + " Failed");
                }
            }
            else
            {
                if (!UnityEditorInternal.ComponentUtility.PasteComponentAsNew(targetObj))
                {
                    Debug.Log("Paste New Values " + com.GetType().ToString() + " Failed");
                }
            }
        }


        private static List<string> modificationStrings = new List<string>();

        private static bool IsPrefabOverrideModification(PropertyModification propertyModification,SceneUnit sceneUnit)
        {
            bool isOverrideModification;
            if (propertyModification.target == sceneUnit)
            {
                isOverrideModification = false;
            }
            else
            {
                isOverrideModification = true;
            }
            return isOverrideModification;
        }
        
        public static string GetPrefabOverrideModification(GameObject obj)
        {
            string modification = "";
            GameObject childPrefabObj = UnityEditor.PrefabUtility.GetCorrespondingObjectFromSource(obj);
            PropertyModification[] modifications = UnityEditor.PrefabUtility.GetPropertyModifications(obj);
            modificationStrings.Clear();
            for (int iModification = 0; iModification < modifications.Length; iModification++)
            {
                PropertyModification iterModification = modifications[iModification];
                bool isOverrideModification;
                if (iterModification.target == childPrefabObj)
                {
                    isOverrideModification = !Constants.PREFAB_ROOT_GAMEOBJECT_IGNORE_MODIFICATION_PROPERTY_PATHS.Contains(iterModification.propertyPath);
                }
                else if (iterModification.target == childPrefabObj.transform)
                {
                    isOverrideModification = !Constants.PREFAB_ROOT_TRANSFORM_IGNORE_MODIFICATION_PROPERTY_PATHS.Contains(iterModification.propertyPath);
                }
                else if(iterModification.target == null)
                {
                    isOverrideModification = false;
                }
                else if(childPrefabObj.GetComponent<SceneUnit>())
                {
                    isOverrideModification = IsPrefabOverrideModification(iterModification, childPrefabObj.GetComponent<SceneUnit>());
                }
                else
                {
                    isOverrideModification = true;
                }

                if (isOverrideModification)
                {
                    modificationStrings.Add(Leyoutech.Utility.PrefabUtility.ConvertPrefabPropertyModificationToString(childPrefabObj, iterModification));
                }
            }

            if (modificationStrings.Count > 0)
            {
                StringBuilder stringBuilder = StringUtility.AllocStringBuilderCache();
                modificationStrings.Sort();
                for (int iModification = 0; iModification < modificationStrings.Count; iModification++)
                {
                    stringBuilder.Append(modificationStrings[iModification]);
                }
                modificationStrings.Clear();
                modification = StringUtility.ReleaseStringBuilderCacheAndReturnString();
            }
            else
            {
                modification = string.Empty;
            }
            return modification;
        }

        /// <summary>
        /// 创建一个Asset
        /// </summary>
        /// <returns>是否成功</returns>
        public static bool CreateAssetAtExportDirectory<T>(ref T obj, string fileName) where T : UnityEngine.Object
		{
			MapEditorSetting mapEditorSetting = GetOrCreateMapEditorSetting();
			if (string.IsNullOrEmpty(mapEditorSetting.AssetExportDirectory))
			{
				return false;
			}

			string path = string.Format("{0}/{1}.asset"
				, mapEditorSetting.AssetExportDirectory
				, fileName);
            AssetDatabase.CreateAsset(obj, path);
            obj = AssetDatabase.LoadAssetAtPath<T>(path);
			AssetDatabase.SetLabels(obj, Constants.EXPORT_MAPASSET_LABELS);
			return obj != null;
		}

		/// <summary>
		/// 创建MapEditorSetting
		/// </summary>
		[MenuItem("Custom/Map/Create Map Editor Setting")]
		private static void CreateMapEditorSetting()
		{
			MapEditorSetting setting = GetMapEditorSetting();
			if (!setting)
			{
				string path = AssetDatabase.GetAssetOrScenePath(Selection.activeObject);
				string directory;
				if (Directory.Exists(path))
				{
					directory = path;
				}
				else
				{
					if (File.Exists(path))
					{
						directory = path.Substring(0, path.LastIndexOf('/'));
					}

					else
					{
						directory = "Assets";
					}
				}

				setting = CreateMapEditorSetting(directory);
			}

			if (setting)
			{
				Selection.activeObject = setting;
			}
		}

		/// <summary>
		/// 打开预览场景
		/// </summary>
		[MenuItem("Custom/Map/Open Preview Scene")]
		private static void OpenPreviewScene()
		{
			MapEditorSetting setting = GetOrCreateMapEditorSetting();
			EditorSceneManager.OpenScene(setting.PreviewScenePath, OpenSceneMode.Single);
		}

#if false
        /// <summary>
        /// 把所有选中的Object中的Prefab标记为SceneUnit
        /// </summary>
        [MenuItem("Assets/Custom/Map/Mark all selected Prefabs as SceneUnit")]
		private static void MarkAllSelectedPrefabsAsSceneUnit()
		{
			UnityEngine.Object[] objects = Selection.objects;
			if (!EditorUtility.DisplayDialog("MapEditor", string.Format("当前选中的{0}个Object，是否要把它们标记为Unit？", objects.Length), "继续", "终止"))
			{
				return;
			}

			int prefabCount = 0;
			int markSuccessCount = 0;
			int markIgnoreCount = 0;
			int markFailedCount = 0;

			try
			{
				for (int iObject = 0; iObject < objects.Length; iObject++)
				{
					UnityEngine.Object iterObject = objects[iObject];
					GameObject iterGameObject = iterObject as GameObject;
					if (iterGameObject
						&& PrefabUtility.IsPartOfPrefabAsset(iterGameObject))
					{
						prefabCount++;
						if (EditorUtility.DisplayCancelableProgressBar("MapEditor", "Mark Prefab as Unit", iObject / (float)objects.Length))
						{
							break;
						}
						if (!iterGameObject.GetComponent<SceneUnit>())
						{
							string assetPath = AssetDatabase.GetAssetPath(iterGameObject);
							GameObject iterPrefabContent = PrefabUtility.LoadPrefabContents(assetPath);
							iterPrefabContent.AddComponent<SceneUnit>();
							bool success;
							PrefabUtility.SaveAsPrefabAsset(iterPrefabContent, assetPath, out success);
							if (success)
							{
								markSuccessCount++;
							}
							else
							{
								markFailedCount++;
							}
						}
						else
						{
							markIgnoreCount++;
						}
					}
				}
			}
			catch (Exception)
			{
			}
			// 不确定会不会有Exception，但万一有的话需要把ProgressBar关掉
			finally
			{
				EditorUtility.ClearProgressBar();

				EditorUtility.DisplayDialog("MapEdiotr"
					, string.Format("一共选中了{0}个Prefab，其中：\n\t{1}个Prefab已经是Unit\n\t{2}个Prefab成功标记为Unit\n\t{3}个Prefab标记为Unit失败，请检查Log输出"
						, prefabCount
						, markIgnoreCount
						, markSuccessCount
						, markFailedCount)
					, "确定");
			}
		}
#endif

		[MenuItem("Custom/Map/Export All Map")]
		private static void ExportAllMap()
		{
			if (EditorUtility.DisplayDialog("Map Editor", "这个操作会很耗时。确定要执行这个操作？\n在执行前请先保存当前场景，在弹出导出完成的对话框前，不要碰这台电脑。", "是", "否"))
			{
				new ExporterAll().BeginExport();
			}
		}

		/// <summary>
		/// 从路径加载<see cref="MapEditorSetting"/>，加载的结果会存到<see cref="m_MapEditorSetting"/>
		/// </summary>
		/// <returns>是否加载成功</returns>
		private static bool LoadMapEditorSetting(string path)
		{
			UnityEngine.Object settingObject = AssetDatabase.LoadAssetAtPath(path, typeof(MapEditorSetting));
			if (settingObject
				&& settingObject is MapEditorSetting)
			{
				m_MapEditorSetting = settingObject as MapEditorSetting;
				return true;
			}
			else
			{
				return false;
			}
		}

		// Map、Area、MapPreview、AreaPreview

		//[MenuItem("GameObject/Map/创建Map",false,11)]
		private static void CreateMap()
		{
			Map map = GameObject.FindObjectOfType<Map>();
			if(map == null)
			{
				GameObject mapObj = new GameObject();
				map = mapObj.AddComponent<Map>();
			}
		}
        
        [MenuItem("GameObject/Map/创建MapPreview",false,11)]
		private static void CreateMapPreview()
		{
            UnityEngine.SceneManagement.Scene emptyScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
			MapPreview mapPreview = GameObject.FindObjectOfType<MapPreview>();
			if(mapPreview == null)
			{
				GameObject mapPreviewObj = new GameObject();
				mapPreview = mapPreviewObj.AddComponent<MapPreview>();
                UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(mapPreviewObj, emptyScene);

            }
		}

        /// <summary>
        /// 打开文件目录
        /// </summary>
        /// <param name="defaultFolder"></param>
        /// <param name="defautName"></param>
        /// <returns></returns>
        public static string OpenFilePanel(string defaultFolder, string defautName = "")
		{
			if (string.IsNullOrEmpty(defaultFolder))
			{
				defaultFolder = Application.dataPath;
			}
			string filePath = EditorUtility.OpenFilePanel("选择文件", defaultFolder, defautName);
			return filePath;
		}

		/// <summary>
		/// 打开文件夹目录
		/// </summary>
		/// <param name="defaultFolder"></param>
		/// <returns></returns>
		public static string OpenFloderPanel(string defaultFolder = "")
		{
			if (string.IsNullOrEmpty(defaultFolder))
			{
				defaultFolder = Application.dataPath;
			}
			string folderPath = EditorUtility.OpenFolderPanel("选择目录", defaultFolder, "");
			return folderPath;
		}


        public static ExportParameter CreateExportParameter()
        {
            ExportParameter exportParameter = new ExportParameter();
            exportParameter.ExportedUnitAddressableKeys = new HashSet<string>();
            MapEditorSetting mapEditorSetting = MapEditorUtility.GetOrCreateMapEditorSetting();

            string exportAssetPath = string.Format("{0}/Units"
                    , mapEditorSetting.AssetExportDirectory);

            string[] resAssets = AssetDatabase.FindAssets("t:Prefab", new string[] { exportAssetPath });
            if (resAssets != null && resAssets.Length > 0)
            {
                for (int iRes = 0; iRes < resAssets.Length; iRes++)
                {
                    string resPath = AssetDatabase.GUIDToAssetPath(resAssets[iRes]);
                    string[] resSplit = resPath.Split('/');
                    if (resSplit != null && resSplit.Length > 0)
                    {
                        string lastName = resSplit[resSplit.Length - 1];
                        string[] lastNameSplit = lastName.Split('_');
                        if (lastNameSplit != null && lastNameSplit.Length == 2)
                        {
                            lastName = lastName.Replace(".prefab", "");
                            exportParameter.ExportedUnitAddressableKeys.Add(lastName);
                        }
                    }
                }
            }
            return exportParameter;
        }
        /// <summary>
        /// 检测GameObject的static标签
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="targetFlag"></param>
        /// <returns></returns>
        public static bool CheckStaticEditorFlags(GameObject obj, StaticEditorFlags targetFlag)
        {
            int realFlag = (int)targetFlag;
            int curFlag = (int)GameObjectUtility.GetStaticEditorFlags(obj);

            return (curFlag & realFlag) == realFlag;
        }

        /// <summary>
        /// 获取某个节点下的所有预设
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="prefabList"></param>
        public static void GetAllPrefab(Transform parent,List<GameObject> prefabList)
        {
            if(parent == null)
            {
                return;
            }

            if(prefabList == null)
            {
                prefabList = new List<GameObject>();
            }

            GameObject sourcePrefab = UnityEditor.PrefabUtility.GetCorrespondingObjectFromSource(parent.gameObject);
            if(sourcePrefab == null)
            {
                int childCount = parent.childCount;
                if(childCount>0)
                {
                    for(int iChild =0;iChild<childCount;iChild++)
                    {
                        Transform child = parent.GetChild(iChild);
                        GetAllPrefab(child, prefabList);
                    }
                }
            }
            else
            {
                prefabList.Add(parent.gameObject);
            }
        }

        /// <summary>
        /// 对string拆分成int数组
        /// </summary>
        /// <param name="str"></param>
        /// <param name="split"></param>
        /// <returns></returns>
        public static int[] GetIntArray(string str,char split = ',')
		{
			int[] intArray = null;
			string[] strSplit = str.Split(split);
			if(strSplit != null && strSplit.Length>0)
			{
				intArray = new int[strSplit.Length];
				for(int iStr = 0;iStr<strSplit.Length;iStr++)
				{
					intArray[iStr] = int.Parse(strSplit[iStr]);
				}
			}
			return intArray;
		}

        private static List<Component> sm_Components = new List<Component>();
        /// <summary>
        /// 计算物体AABB
        /// </summary>
        /// <param name="trans"></param>
        /// <param name="aabb"></param>
        /// <returns></returns>
        public static bool CaculateAABB(Transform trans, ref Bounds aabb,bool simulParticle = true)
        {
            bool initializedAABB = false;
            sm_Components.Clear();
            Component[] coms = trans.GetComponents<Component>();
            if (coms != null && coms.Length > 0)
            {
                for (int iCom = 0; iCom < coms.Length; iCom++)
                {
                    Component com = coms[iCom];
                    if (com is Renderer || com is Collider || com is ParticleSystem)
                    {
                        sm_Components.Add(com);
                    }
                }
            }

            Component[] childcComs = trans.GetComponentsInChildren<Component>(false);
            if (childcComs != null && childcComs.Length > 0)
            {
                for (int iCom = 0; iCom < childcComs.Length; iCom++)
                {
                    Component com = childcComs[iCom];
                    if (com is Renderer || com is Collider || com is ParticleSystem)
                    {
                        sm_Components.Add(com);
                    }
                }
            }

            for (int iCom = 0; iCom < sm_Components.Count; iCom++)
            {
                Component iterComponent = sm_Components[iCom];

                Bounds bounds = default(Bounds);
                if (iterComponent is Collider)
                {
                    bounds = (iterComponent as Collider).bounds;
                }
                else if ((iterComponent is ParticleSystem) && simulParticle)
                {
                    ParticleSystemRenderer iterRenderer = iterComponent.GetComponent<ParticleSystemRenderer>();
                    if (iterRenderer)
                    {
                        bounds = iterRenderer.bounds;
                    }
                    else
                    {
                        continue;
                    }

                }
                else if (iterComponent is Renderer)
                {
                    bounds = (iterComponent as Renderer).bounds;
                }
                else if(iterComponent is RandomDisperseMulMesh)
                {
                    bounds = (iterComponent as RandomDisperseMulMesh).CaculateLimitBounds();
                }
                else if (iterComponent is RandomDisperseMesh)
                {
                    bounds = (iterComponent as RandomDisperseMesh).CaculateLimitBounds();
                }
                else
                {
                    continue;
                }

                if (initializedAABB)
                {
                    aabb.Encapsulate(bounds);
                }
                else
                {
                    initializedAABB = true;
                    aabb = bounds;
                }

            }
            return initializedAABB;
        }
        
        /// <summary>
        /// 选中子物体下的粒子效果
        /// </summary>
        public static void SelectChildParticleSystem(GameObject root)
        {
            ParticleSystem[] particleSystems = root.GetComponentsInChildren<ParticleSystem>();
            if (particleSystems != null && particleSystems.Length > 0)
            {
                ObjectUtility.SelectionComponent(particleSystems);

                for (int iParticle = 0; iParticle < particleSystems.Length; iParticle++)
                {
                    particleSystems[iParticle].Simulate(Constants.PARTICLE_SIMULATE_TIME_WHEN_EXPOTEING);
                }
            }

        }

        /// <summary>
        /// 字符串格式转换
        /// </summary>
        /// <param name="unicodeString"></param>
        /// <returns></returns>
        public static string ToUTF8(string unicodeString)
		{
			UTF8Encoding utf8 = new UTF8Encoding(false);
			Byte[] encodedBytes = utf8.GetBytes(unicodeString);
			String decodedString = utf8.GetString(encodedBytes);
			return decodedString;
		}

		public static string ToGB2312(string unicodeString)
		{
			Encoding gb2312 = Encoding.GetEncoding("gb2312");
			Byte[] encodedBytes = gb2312.GetBytes(unicodeString);
			String decodedString = gb2312.GetString(encodedBytes);
			return decodedString;
		}

		/// <summary>
		/// 获取相对于unity的相对路径
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public static string GetFullPath(string path)
		{
			/*string relativePath = path;
			if (!string.IsNullOrEmpty(relativePath))
			{
				int pathIndex = path.IndexOf("Assets");
				if (pathIndex > 0)
				{
					relativePath = relativePath.Substring(pathIndex, relativePath.Length - pathIndex);
				}
			}
			return relativePath;
			if(!path.Contains("Assets"))
			{
				path = string.Format("Assets/{0}", path);
			}*/
			string fullPath = Path.GetFullPath(path);
			fullPath = fullPath.Replace("\\", "/");
			return fullPath;
		}
		

		public static string GetRelativePath(string relativeTo,string absolutePath = "")
		{
			if(string.IsNullOrEmpty(absolutePath))
			{
				absolutePath = System.IO.Directory.GetParent(Application.dataPath).FullName;
				//absolutePath = Application.dataPath;
				absolutePath = absolutePath.Replace("\\", "/");
			}
			string[] absoluteDirectories = absolutePath.Split('/');
			string[] relativeDirectories = relativeTo.Split('/');
			int length = absoluteDirectories.Length < relativeDirectories.Length ? absoluteDirectories.Length : relativeDirectories.Length;
			
			int lastCommonRoot = -1;
			int index;
			
			for (index = 0; index < length; index++)
				if (absoluteDirectories[index] == relativeDirectories[index])
					lastCommonRoot = index;
				else
					break;
			
			if (lastCommonRoot == -1)
				throw new ArgumentException("Paths do not have a common base");
			
			StringBuilder relativePath = new StringBuilder();
			
			for (index = lastCommonRoot + 1; index < absoluteDirectories.Length; index++)
				if (absoluteDirectories[index].Length > 0)
					relativePath.Append("..\\");
			
			for (index = lastCommonRoot + 1; index < relativeDirectories.Length - 1; index++)
				relativePath.Append(relativeDirectories[index] + "\\");
			relativePath.Append(relativeDirectories[relativeDirectories.Length - 1]);

			return relativePath.ToString();
		}
		
		/// <summary>
		/// 计算数字以几位显示
		/// </summary>
		/// <param name="number"></param>
		/// <param name="needShow"></param>
		/// <returns></returns>
		public static string CalcuateNumber(int number, int needShow)
		{
			int baseNum = (int)Mathf.Pow(10, needShow - 1);
			int baseValue = number / baseNum;
			if (baseValue > 0)
			{
				return number.ToString();
			}
			else
			{
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                int insertNum = 0;
                if (baseNum != 0)
                {
                    while (number / baseNum <= 0)
                    {
                        insertNum++;
                        baseNum = baseNum / 10;
                    }
                }
                else
                {
                    insertNum = needShow;
                }

                for (int iInsert = 0; iInsert < insertNum; iInsert++)
                {
                    sb.Append("0");
                }
                sb.Append(number);
                return sb.ToString();
            }
		}

        /// <summary>
        /// 打开目录
        /// </summary>
        /// <param name="path"></param>
        public static void OpenFolder(string path)
        {
            DirectoryInfo dir = Directory.GetParent(path);
            int index = path.LastIndexOf("/");
            System.Diagnostics.Process.Start("explorer.exe", dir.FullName);
        }

    }
}
#endif