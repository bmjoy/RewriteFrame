using Leyoutech.Core.Loader;
using Leyoutech.Core.Loader.Config;
using LeyoutechEditor.Core.AssetRuler.AssetAddress;
using LeyoutechEditor.Core.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.U2D;
using UnityEditor.U2D;
using UnityObject = UnityEngine.Object;
using System.Text;
using LeyoutechEditor.Core.BundleDepend;
using Leyoutech.Core.HashTools;

namespace LeyoutechEditor.Core.Packer
{
    public static class BundlePackUtil
    {
        //根据使用次数查找到的默认分组
        private static string AUTO_REPEAT_GROUP_NAME = "Auto Group";
        //配置文件存储路径
        public static string ASSET_RULER_CONFIG_PATH = "Assets/Tools/AssetRuler";

        /// <summary>
        /// 打包配置文件所在的位置
        /// </summary>
        /// <returns></returns>
        internal static string GetPackConfigPath()
        {
            var dataPath = Path.GetFullPath(".");
            dataPath = dataPath.Replace("\\", "/");
            dataPath += "/Library/BundlePack/pack_config.data";
            return dataPath;
        }
       /// <summary>
       /// Bundle分组配置所在的位置
       /// </summary>
       /// <returns></returns>
        internal static string GetTagConfigPath()
        {
            var dataPath = Path.GetFullPath(".");
            dataPath = dataPath.Replace("\\", "/");
            dataPath += "/Library/BundlePack/tag_config.data";
            return dataPath;
        }

        /// <summary>
        /// 处理资源，生成资源地址及对应的Class
        /// </summary>
        /// <param name="forceReimportConfig">是否重新导入配置文件，默认为True</param>
        /// <param name="ingoreAddressRepeat">是否忽略地址重复的情况，默认为False</param>
        /// <param name="isCreateKeyClass">是否生成Class文件</param>
        /// <returns></returns>
        public static bool GenerateConfigs(bool forceReimportConfig = true,bool ingoreAddressRepeat = false,bool isCreateKeyClass = true)
        {
            if(forceReimportConfig)
            {
                AssetDatabase.ImportAsset(ASSET_RULER_CONFIG_PATH, ImportAssetOptions.ImportRecursive|ImportAssetOptions.ForceUpdate);
            }
            UpdateTagConfig();

            bool isRepeat = IsAddressRepeat();
            if(isRepeat)
            {
                if(!ingoreAddressRepeat)
                {
                    return !isRepeat;
                }
            }
            UpdateAddressConfig();
            if(isCreateKeyClass)
            {
                CreateAddressKeyClass();
            }
            return !isRepeat;
        }

        /// <summary>
        /// 根据配置的AssetAddressAssembly更新资源的分组
        /// </summary>
        public static void UpdateTagConfig()
        {
            string[] settingPaths = AssetDatabaseUtil.FindAssets<AssetAddressAssembly>();
            if (settingPaths == null || settingPaths.Length == 0)
            {
                Debug.LogError("AssetBundleSchemaUtil::UpdateTagConfigBySchema->Not found schema Setting;");
                return;
            }
            foreach(var assetPath in settingPaths)
            {
                AssetAddressAssembly aaAssembly = AssetDatabase.LoadAssetAtPath<AssetAddressAssembly>(assetPath);
                if(aaAssembly!=null)
                {
                    aaAssembly.AutoFind();
                    aaAssembly.Execute();
                }
            }
        }

        /// <summary>
        /// 通过资源的分组信息，生成地址的配置文件
        /// </summary>
        public static void UpdateAddressConfig()
        {
            AssetBundleTagConfig tagConfig = Util.FileUtil.ReadFromBinary<AssetBundleTagConfig>(BundlePackUtil.GetTagConfigPath());
            AssetAddressConfig config = AssetDatabase.LoadAssetAtPath<AssetAddressConfig>(AssetAddressConfig.CONFIG_PATH);
            if (config == null)
            {
                config = ScriptableObject.CreateInstance<AssetAddressConfig>();
                AssetDatabase.CreateAsset(config, AssetAddressConfig.CONFIG_PATH);
                AssetDatabase.ImportAsset(AssetAddressConfig.CONFIG_PATH);
            }

            AssetAddressData[] datas = (from groupData in tagConfig.GroupDatas
                                        where groupData.IsMain == true
                                        from assetData in groupData.AssetDatas
                                        select assetData).ToArray();

            List<AssetAddressData> addressDatas = new List<AssetAddressData>();
            foreach (var assetData in datas)
            {
                AssetAddressData addressData = new Leyoutech.Core.Loader.Config.AssetAddressData()
                {
                    AssetAddress = assetData.AssetAddress,
                    AssetPath = assetData.AssetPath,
                    BundlePath = assetData.BundlePath,
                };
                if (assetData.Labels != null && assetData.Labels.Length > 0)
                {
                    addressData.Labels = new string[assetData.Labels.Length];
                    Array.Copy(assetData.Labels, addressData.Labels, addressData.Labels.Length);
                }
                addressDatas.Add(addressData);
            }

            config.AddressDatas = addressDatas.ToArray();
            EditorUtility.SetDirty(config);

            AssetDatabase.SaveAssets();
        }

        /// <summary>
        /// 生成Class，用于标识Address
        /// </summary>
        public static void CreateAddressKeyClass()
        {
            AssetBundleTagConfig tagConfig = Util.FileUtil.ReadFromBinary<AssetBundleTagConfig>(BundlePackUtil.GetTagConfigPath());

            List<string> fieldNameAndValues = new List<string>();
            foreach (var group in tagConfig.GroupDatas)
            {
                if (!group.IsMain || !group.IsGenAddress)
                {
                    continue;
                }
                string prefix = group.GroupName.Replace(" ", "");

                foreach (var data in group.AssetDatas)
                {
                    string address = data.AssetAddress;

                    string tempName = address;
                    if (tempName.IndexOf('/') > 0)
                    {
                        tempName = Path.GetFileNameWithoutExtension(tempName);
                    }
                    tempName = tempName.Replace(" ", "_").Replace(".", "_").Replace("-","_");

                    string fieldName = (prefix + "_" + tempName).ToUpper();
                    string content = $"{fieldName} = @\"{address}\";";

                    if (fieldNameAndValues.IndexOf(content) < 0)
                    {
                        fieldNameAndValues.Add(content);
                    }
                }
            }

            StringBuilder classSB = new StringBuilder();
            classSB.AppendLine("namespace Leyoutech.Core.Loader.Config");
            classSB.AppendLine("{");
            classSB.AppendLine("\tpublic static class AssetAddressKey");
            classSB.AppendLine("\t{");

            fieldNameAndValues.ForEach((value) =>
            {
                classSB.AppendLine("\t\tpublic const string " + value);
            });

            classSB.AppendLine("\t}");
            classSB.AppendLine("}");

            string filePath = Application.dataPath + "/Scripts/Core/Loader/Config/AssetAddressKey.cs";
            File.WriteAllText(filePath, classSB.ToString());
        }

		public static Dictionary<string, string> CollectionAssetPathToBundleNames()
		{
			Dictionary<string, string> pathToNames = new Dictionary<string, string>();
			pathToNames[AssetAddressConfig.CONFIG_PATH] = AssetAddressConfig.CONFIG_ASSET_BUNDLE_NAME.ToLower();

			#region Collection
			AssetBundleTagConfig tagConfig = Util.FileUtil.ReadFromBinary<AssetBundleTagConfig>(BundlePackUtil.GetTagConfigPath());
			AssetAddressData[] datas = (from groupData in tagConfig.GroupDatas
										from detailData in groupData.AssetDatas
										select detailData).ToArray();

			long elapsedMS = Leyoutech.Utility.DebugUtility.GetMillisecondsSinceStartup();
			for (int iData = 0; iData < datas.Length; iData++)
			{
				AssetAddressData iterData = datas[iData];
				string assetPath = iterData.AssetPath;
				string bundleName = iterData.BundlePath.ToLower();
				pathToNames[assetPath] = bundleName;

				if (Path.GetExtension(assetPath).ToLower() != ".spriteatlas")
				{
					continue;
				}

				SpriteAtlas atlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(assetPath);
				if (atlas == null)
				{
					continue;
				}

				UnityObject[] packables = atlas.GetPackables();
				for (int iPackable = 0; iPackable < packables.Length; iPackable++)
				{
					UnityObject iterPackable = packables[iPackable];
					if (iterPackable.GetType() == typeof(Sprite))
					{
						pathToNames[AssetDatabase.GetAssetPath(iterPackable)] = bundleName;
					}
					else if (iterPackable.GetType() == typeof(DefaultAsset))
					{
						string folderPath = AssetDatabase.GetAssetPath(iterPackable);
						string[] assets = AssetDatabaseUtil.FindAssetInFolder<Sprite>(folderPath);
						for (int iAsset = 0; iAsset < assets.Length; iAsset++)
						{
							pathToNames[assets[iAsset]] = bundleName;
						}
					}
				}
			}
			elapsedMS = Leyoutech.Utility.DebugUtility.GetMillisecondsSinceStartup() - elapsedMS;
			Leyoutech.Utility.DebugUtility.Log(Leyoutech.BuildPipeline.BuildPipelineGraph.LOG_TAG, $"Collection all bundleName count ({pathToNames.Count}) elapsed {Leyoutech.Utility.DebugUtility.FormatMilliseconds(elapsedMS)}");
			#endregion
			return pathToNames;
		}

		public static bool ResetBundleNames()
		{
			Dictionary<string, string> pathToNames = CollectionAssetPathToBundleNames();

			#region Reset
			long elapsedMS = Leyoutech.Utility.DebugUtility.GetMillisecondsSinceStartup();
			string[] assetPaths = AssetDatabase.GetAllAssetPaths();
			int modifyAssetCount = 0;
			for (int iAsset = 0; iAsset < assetPaths.Length; iAsset++)
			{
				if (iAsset % 100 == 0
					&& EditorUtility.DisplayCancelableProgressBar(Leyoutech.BuildPipeline.BuildPipelineGraph.LOG_TAG, $"Reset bundleName {iAsset}/{assetPaths.Length}. Modified {modifyAssetCount}", iAsset / (float)assetPaths.Length))
				{
					return false;
				}

				string iterAssetPath = assetPaths[iAsset];
				// 没有meta文件的不需要设置bundleName
				if (!File.Exists(iterAssetPath + ".meta"))
				{
					continue;
				}

				AssetImporter iterAssetImporter = AssetImporter.GetAtPath(iterAssetPath);
				if (!pathToNames.TryGetValue(iterAssetPath, out string bundleName))
				{
					bundleName = string.Empty;
				}

				if (iterAssetImporter.assetBundleName != bundleName)
				{
					modifyAssetCount++;
					iterAssetImporter.assetBundleName = bundleName;
				}
			}

			EditorUtility.ClearProgressBar();
			AssetDatabase.SaveAssets();
			Resources.UnloadUnusedAssets();
			elapsedMS = Leyoutech.Utility.DebugUtility.GetMillisecondsSinceStartup() - elapsedMS;
			Leyoutech.Utility.DebugUtility.Log(Leyoutech.BuildPipeline.BuildPipelineGraph.LOG_TAG, $"Modify bundleName count ({modifyAssetCount}) elapsed {Leyoutech.Utility.DebugUtility.FormatMilliseconds(elapsedMS)}");
			#endregion
			return true;
		}

		/// <summary>
		/// 根据配置中的数据设置BundleName
		/// </summary>
		/// <param name="isShowProgressBar">是否显示进度</param>
		public static void SetAssetBundleNames(bool isShowProgressBar = false)
        {
            AssetBundleTagConfig tagConfig = Util.FileUtil.ReadFromBinary<AssetBundleTagConfig>(BundlePackUtil.GetTagConfigPath());

            AssetImporter assetImporter = AssetImporter.GetAtPath(AssetAddressConfig.CONFIG_PATH);
            assetImporter.assetBundleName = AssetAddressConfig.CONFIG_ASSET_BUNDLE_NAME;

            AssetAddressData[] datas = (from groupData in tagConfig.GroupDatas
                                        from detailData in groupData.AssetDatas
                                        select detailData).ToArray();

            if (isShowProgressBar)
            {
                EditorUtility.DisplayProgressBar("Set Bundle Names", "", 0f);
            }

            if (datas != null && datas.Length > 0)
            {
                for (int i = 0; i < datas.Length; i++)
                {
                    if (isShowProgressBar)
                    {
                        EditorUtility.DisplayProgressBar("Set Bundle Names", datas[i].AssetPath, i / (float)datas.Length);
                    }
                    string assetPath = datas[i].AssetPath;
                    string bundlePath = datas[i].BundlePath;
                    AssetImporter ai = AssetImporter.GetAtPath(assetPath);
                    ai.assetBundleName = bundlePath;

                    if(Path.GetExtension(assetPath).ToLower() == ".spriteatlas")
                    {
                        SetSpriteBundleNameByAtlas(assetPath, bundlePath);
                    }
                }
            }
            if (isShowProgressBar)
            {
                EditorUtility.ClearProgressBar();
            }

            AssetDatabase.SaveAssets();
        }

        /// <summary>
        /// 由于UGUI中SpriteAtlas的特殊性，为了防止UI的Prefab打包无法与Atlas关联，
        /// 从而设定将SpriteAtlas所使用的Sprite一起打包
        /// </summary>
        /// <param name="atlasAssetPath">SpriteAtlas所在的资源路径</param>
        /// <param name="bundlePath">需要设置的BundleName</param>
        private static void SetSpriteBundleNameByAtlas(string atlasAssetPath,string bundlePath)
        {
            SpriteAtlas atlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(atlasAssetPath);
            if (atlas != null)
            {
                List<string> spriteAssetPathList = new List<string>();
                UnityObject[] objs = atlas.GetPackables();
                foreach (var obj in objs)
                {
                    if (obj.GetType() == typeof(Sprite))
                    {
                        spriteAssetPathList.Add(AssetDatabase.GetAssetPath(obj));
                    }
                    else if (obj.GetType() == typeof(DefaultAsset))
                    {
                        string folderPath = AssetDatabase.GetAssetPath(obj);
                        string[] assets = AssetDatabaseUtil.FindAssetInFolder<Sprite>(folderPath);
                        spriteAssetPathList.AddRange(assets);
                    }
                }
                spriteAssetPathList.Distinct();
                foreach (var path in spriteAssetPathList)
                {
                    AssetImporter ai = AssetImporter.GetAtPath(path);
                    ai.assetBundleName = bundlePath;
                }
            }
        }
        
        /// <summary>
        /// 清除设置的BundleName的标签
        /// </summary>
        /// <param name="isShowProgressBar">是否显示清除进度</param>
        public static void ClearAssetBundleNames(bool isShowProgressBar = false)
        {
            string[] bundleNames = AssetDatabase.GetAllAssetBundleNames();
            if(isShowProgressBar)
            {
                EditorUtility.DisplayProgressBar("Clear Bundle Names", "", 0.0f);
            }
            for (int i = 0; i < bundleNames.Length; i++)
            {
                if (isShowProgressBar)
                {
                    EditorUtility.DisplayProgressBar("Clear Bundle Names", bundleNames[i], i / (float)bundleNames.Length);
                }
                AssetDatabase.RemoveAssetBundleName(bundleNames[i], true);
            }
            if (isShowProgressBar)
            {
                EditorUtility.ClearProgressBar();
            }

            AssetDatabase.SaveAssets();
        }
        /// <summary>
        /// 根据配置打包AB
        /// </summary>
        /// <param name="packConfig"></param>
        /// <returns></returns>
        public static AssetBundleManifest PackAssetBundle(BundlePackConfig packConfig)
        {
            return PackAssetBundle(packConfig.OutputDirPath, packConfig.CleanupBeforeBuild, packConfig.GetBundleOptions(), packConfig.GetBuildTarget());
        }
        /// <summary>
        /// 进行AB的打包
        /// </summary>
        /// <param name="outputDir">AB输出目录</param>
        /// <param name="isClean">是否清理后再打AB</param>
        /// <param name="options">Bundle的选项</param>
        /// <param name="buildTarget">目标平台</param>
        /// <returns></returns>
        public static AssetBundleManifest PackAssetBundle(string outputDir,bool isClean,BuildAssetBundleOptions options, BuildTarget buildTarget)
        {
            string outputTargetDir = outputDir + "/" + buildTarget.ToString() + "/" + AssetBundleConst.ASSETBUNDLE_MAINFEST_NAME;

            if (isClean && Directory.Exists(outputTargetDir))
            {
                Directory.Delete(outputTargetDir, true);
            }
            if (!Directory.Exists(outputTargetDir))
            {
                Directory.CreateDirectory(outputTargetDir);
            }
            
            return BuildPipeline.BuildAssetBundles(outputTargetDir, options, buildTarget);
        }
		/// <summary>
		/// 根据项目中的配置进行AB打包
		/// </summary>
		/// <param name="assetBundleManifest"></param>
		/// <param name="setting"><see cref="PackBundleSetting"/></param>		
		/// <returns></returns>
		public static bool PackBundle(out AssetBundleManifest assetBundleManifest, PackBundleSetting setting)
		{
			assetBundleManifest = null;

			if (setting.UpdateConfigs)
			{
				if (!GenerateConfigs())
				{
					return false;
				}

				if (setting.FindDepend)
				{
					FindAndAddAutoGroup(setting.IsShowProgressBar);
				}
				else
				{
					DeleteAutoGroup();
				}
			}

			if (setting.ResetBundleName)
			{
				ClearAssetBundleNames(setting.IsShowProgressBar);
				SetAssetBundleNames(setting.IsShowProgressBar);
			}

			BundlePackConfig packConfig = Util.FileUtil.ReadFromBinary<BundlePackConfig>(BundlePackUtil.GetPackConfigPath());
			assetBundleManifest = PackAssetBundle(packConfig);

			return true;
		}
		/// <summary>
		/// 检查资源Address是否有重复的
		/// </summary>
		/// <returns></returns>
		public static bool IsAddressRepeat()
        {
            AssetBundleTagConfig tagConfig = Util.FileUtil.ReadFromBinary<AssetBundleTagConfig>(BundlePackUtil.GetTagConfigPath());
            AssetAddressData[] datas = (from groupData in tagConfig.GroupDatas
                                        where groupData.IsMain
                                        from assetData in groupData.AssetDatas
                                        select assetData).ToArray();

            List<string> addressList = new List<string>();
            bool isRepeat = false;
            foreach(var data in datas)
            {
                if(addressList.IndexOf(data.AssetAddress) >= 0)
                {
                    isRepeat = true;
                    Debug.LogError($"BundlePackUtil::IsAddressRepeat->AssetAddress Repeat.address = {data.AssetAddress}");
                }else
                {
                    addressList.Add(data.AssetAddress);
                }
            }

            return isRepeat;
        }

        /// <summary>
        /// 创建资源依赖查找对象
        /// </summary>
        /// <param name="tagConfig"></param>
        /// <param name="isShowProgressBar"></param>
        /// <returns></returns>
        public static AssetDependFinder CreateAssetDependFinder(AssetBundleTagConfig tagConfig,bool isShowProgressBar = false)
        {
            AssetDependFinder finder = new AssetDependFinder();

            if (isShowProgressBar)
            {
                finder.ProgressCallback = (assetPath, progress) =>
                {
                    EditorUtility.DisplayProgressBar("Find Depend", assetPath, progress);
                };
            }

            finder.Find(tagConfig);

            if (isShowProgressBar)
            {
                EditorUtility.ClearProgressBar();
            }

            return finder;
        }

        /// <summary>
        /// 删除自动查找到的依赖的分组
        /// </summary>
        internal static void DeleteAutoGroup()
        {
            AssetBundleTagConfig tagConfig = Util.FileUtil.ReadFromBinary<AssetBundleTagConfig>(BundlePackUtil.GetTagConfigPath());
            for (int i = 0; i < tagConfig.GroupDatas.Count; ++i)
            {
                if (tagConfig.GroupDatas[i].GroupName == AUTO_REPEAT_GROUP_NAME)
                {
                    tagConfig.GroupDatas.RemoveAt(i);
                    break;
                }
            }
            Util.FileUtil.SaveToBinary<AssetBundleTagConfig>(BundlePackUtil.GetTagConfigPath(), tagConfig);
        }

        /// <summary>
        /// 查找资源的依赖，并生成默认的分组，将依赖的资源也单独打包处理
        /// </summary>
        /// <param name="isShowProgressBar"></param>
        internal static void FindAndAddAutoGroup(bool isShowProgressBar = false)
        {
            DeleteAutoGroup();

            AssetBundleTagConfig tagConfig = Util.FileUtil.ReadFromBinary<AssetBundleTagConfig>(BundlePackUtil.GetTagConfigPath());
            AssetDependFinder finder = CreateAssetDependFinder(tagConfig, isShowProgressBar);

            Dictionary<string, int> repeatAssetDic = finder.GetRepeatUsedAssets();

            AssetBundleGroupData gData = new AssetBundleGroupData();
            gData.GroupName = AUTO_REPEAT_GROUP_NAME;
            gData.IsMain = false;

            foreach (var kvp in repeatAssetDic)
            {
                AssetAddressData aaData = new AssetAddressData();
                aaData.AssetAddress = aaData.AssetPath = kvp.Key;
                aaData.BundlePath = HashHelper.Hash_MD5_32(kvp.Key, false);//AssetDatabase.AssetPathToGUID(kvp.Key);
                gData.AssetDatas.Add(aaData);
            }

            if (gData.AssetDatas.Count > 0)
            {
                tagConfig.GroupDatas.Add(gData);
            }

            Util.FileUtil.SaveToBinary<AssetBundleTagConfig>(BundlePackUtil.GetTagConfigPath(), tagConfig);
        }

		public struct PackBundleSetting
		{
			///	 <summary>
			/// 是否需要查找资源的依赖
			/// </summary>
			public bool FindDepend;
			/// <summary>
			/// 是否显示进度
			/// </summary>
			public bool IsShowProgressBar;
			/// <summary>
			/// 更新Config
			/// </summary>
			public bool UpdateConfigs;
			/// <summary>
			/// 重新设置BundleName
			/// </summary>
			public bool ResetBundleName;
		}
	}
}

