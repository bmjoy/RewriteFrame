/*===============================
 * Author: [Allen]
 * Purpose: AssetBundle选择某些文件打包窗口
 * Time: 2019/11/29 12:01:17
================================*/

using Leyoutech.Core.Loader;
using Leyoutech.Core.Loader.Config;
using LeyoutechEditor.Core.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.U2D;
using UnityObject = UnityEngine.Object;

namespace LeyoutechEditor.Core.Packer
{
    public class AssetBundleSelectBulidWindow : EditorWindow
    {
        /// <summary>
        /// 真正的输出路径
        /// </summary>
        private string outPath_true = string.Empty;

        /// <summary>
        /// 临时的输出路径
        /// </summary>
        private string outPath_temp = string.Empty;

        /// <summary>
        /// Bundle的选项
        /// </summary>
        private BuildAssetBundleOptions options;

        /// <summary>
        /// 目标平台
        /// </summary>
        private BuildTarget buildTarget;

        private bool bInit = false;

        AssetBundleManifest m_AssetBundleManifest;

        private List<string> AbNameList = new List<string>() { "" };


        private AssetAddressData[] Alldatas;

        AssetBundleSelectBulidWindow()
        {
            this.titleContent = new GUIContent("AssetBundle 指定AB替换打包");
        }

        //添加菜单栏用于打开窗口
        [MenuItem("Custom/Asset Bundle/AssetBundle 指定AB替换打包")]
        static void showWindow()
        {
            EditorWindow.GetWindow(typeof(AssetBundleSelectBulidWindow));
        }

        void OnInspectorUpdate()
        {
            //Debug.Log("窗口面板的更新");
            //这里开启窗口的重绘，不然窗口信息不会刷新
            this.Repaint();
        }

        void OnDestroy()
        {
            bInit = false;
        }


        void OnGUI()
        {
            GUILayout.BeginVertical();

            //绘制标题
            GUILayout.Space(10);
            GUI.skin.label.fontSize = 24;
            GUI.skin.label.alignment = TextAnchor.MiddleCenter;
            GUILayout.Label("AssetBundle 指定AB替换打包");


            #region 列表按钮
            GUILayout.BeginHorizontal();
            //添加
            if (GUILayout.Button(EditorGUIUtility.FindTexture("d_Toolbar Plus"), GUILayout.Width(50)))
            {
                AbNameList.Add(string.Empty);
            }

            //清除
            if (GUILayout.Button("Clear", GUILayout.Width(50), GUILayout.Height(22)))
            {
                AbNameList.Clear();
                AbNameList.Add(string.Empty);
            }
            GUILayout.EndHorizontal();
            #endregion


            for (int i = 0; i < AbNameList.Count; i++)
            {
                GUILayout.Space(10);
                AbNameList[i] = EditorGUILayout.TextField("指定替换的AB名字: ", AbNameList[i]);
            }

            GUILayout.Space(30);

            if (GUILayout.Button(" 替换打包", GUILayout.Height(30)))
            {
                if (!bInit)
                    ToInit();
                ToFindDirectDependencies();

                if (!string.IsNullOrEmpty(outPath_temp) && Directory.Exists(outPath_temp))
                {
                    Directory.Delete(outPath_temp, true);
                }
            }
            GUILayout.EndVertical();
        }

        private void ToInit()
        {
            string AssetRootDir = UnityEditor.EditorPrefs.GetString("BT_ASSET_BUNDLE_PATH_PREFS", string.Empty) + "/" + EditorUserBuildSettings.activeBuildTarget + "/assetbundles";
            string manifestPath = $"{AssetRootDir}/{AssetBundleConst.ASSETBUNDLE_MAINFEST_NAME}";
            AssetBundle manifestAB = AssetBundle.LoadFromFile(manifestPath);
            m_AssetBundleManifest = manifestAB.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
            // AB释放掉
            manifestAB.Unload(false);

            AssetBundleTagConfig tagConfig = Util.FileUtil.ReadFromBinary<AssetBundleTagConfig>(BundlePackUtil.GetTagConfigPath());
            Alldatas = (from groupData in tagConfig.GroupDatas
                        from assetData in groupData.AssetDatas
                        select assetData).ToArray();

            bInit = true;
        }


        /// <summary>
        /// 查找依赖bundle
        /// </summary>
        private void ToFindDirectDependencies()
        {
            List<string> AbDependenciesNameList = new List<string>();
            AbDependenciesNameList.Clear();

            for (int i = 0; i < AbNameList.Count; i++)
            {
                string mainbundName = AbNameList[i];
                if (!string.IsNullOrEmpty(mainbundName))
                {
                    ToFindDirectDependencies(mainbundName, ref AbDependenciesNameList);
                }
            }

            ToDoCreatNewBundle(AbDependenciesNameList);
        }

        /// <summary>
        /// 递归
        /// </summary>
        /// <param name="mainbundName"></param>
        /// <param name="AbDependenciesNameList"></param>
        void ToFindDirectDependencies(string mainbundName, ref List<string> AbDependenciesNameList)
        {
            if (!AbDependenciesNameList.Contains(mainbundName))
            {
                AbDependenciesNameList.Add(mainbundName);
            }
            string[] dependBundlePaths = m_AssetBundleManifest.GetDirectDependencies(mainbundName);
            if (dependBundlePaths != null && dependBundlePaths.Length > 0)
            {
                foreach (var path in dependBundlePaths)
                {
                    if (!AbDependenciesNameList.Contains(path))
                    {
                        AbDependenciesNameList.Add(path);
                        ToFindDirectDependencies(path, ref AbDependenciesNameList);
                    }
                }
            }
        }

        /// <summary>
        /// 指定主Bundl,根据它的关联bund，执行替换
        /// </summary>
        /// <param name="mainBundName"></param>
        void ToDoCreatNewBundle(List<string> AbDependenciesNameList)
        {
            outPath_true = GetTrueOutPath();
            outPath_temp = GetOutPathTemp(true);
            if (string.IsNullOrEmpty(outPath_true))
                return;

            //某个bund ,跟打进它的所有资源关系容器
            Dictionary<string, List<AssetAddressData>> bundAndAssetDic = new Dictionary<string, List<AssetAddressData>>();

            //查找，所有 bund ，跟bund对应的所有 data结构
            foreach (var assetData in Alldatas)
            {
                if (AbDependenciesNameList.Contains(assetData.BundlePath))
                {
                    List<AssetAddressData> findList = null;
                    if (!bundAndAssetDic.TryGetValue(assetData.BundlePath, out findList))
                    {
                        findList = new List<AssetAddressData>();
                        bundAndAssetDic.Add(assetData.BundlePath, findList);
                    }

                    findList = bundAndAssetDic[assetData.BundlePath];
                    findList.Add(assetData);
                }
            }

            AssetBundleBuild[] buildMap = new AssetBundleBuild[bundAndAssetDic.Count];
            int index_bund = 0;
            foreach (var bund in bundAndAssetDic)
            {
                string[] itemAssets = new string[bund.Value.Count]; //List<AssetAddressData>
                int index_data = 0;

                List<string> spriteAssetPathList = new List<string>();  //特别给SpriteAtlas 使用的
                foreach (var hansAssetData in bund.Value) //hansAssetData = AssetAddressData
                {
                    itemAssets[index_data] = hansAssetData.AssetPath;
                    index_data++;

                    //由于UGUI中SpriteAtlas的特殊性，为了防止UI的Prefab打包无法与Atlas关联，
                    // 从而设定将SpriteAtlas所使用的Sprite一起打包
                    if (Path.GetExtension(hansAssetData.AssetPath).ToLower() == ".spriteatlas")
                    {
                        GetSpriteAssetitem(hansAssetData.AssetPath, ref spriteAssetPathList);
                    }
                }

                if (spriteAssetPathList.Count > 0)
                {
                    //这个bund 是图集spriteatlas
                    string[] spriteatlas = spriteAssetPathList.ToArray();
                    string[] result = new string[spriteatlas.Length + itemAssets.Length];
                    Array.Copy(itemAssets, result, itemAssets.Length);
                    Array.Copy(spriteatlas, 0, result, itemAssets.Length, spriteatlas.Length);
                    itemAssets = result;
                }


                buildMap[index_bund].assetBundleName = bund.Key;
                buildMap[index_bund].assetNames = itemAssets;
                index_bund++;
            }

            AssetBundleManifest manifest = BuildPipeline.BuildAssetBundles(outPath_temp,
                buildMap, options, buildTarget);

            AssetDatabase.Refresh(); //刷新
            if (manifest == null)
                Debug.LogError(string.Format("打包失败!"));
            else
            {
                Debug.Log(string.Format("打包OK!"));
            }

            string[] outbundls = manifest.GetAllAssetBundles();
            for (int t = 0; t < outbundls.Length; t++)
            {
                string path = outbundls[t];
                Debug.Log("----> out bund name :" + path);
                MoveFile(path);
            }
        }

        void GetSpriteAssetitem(string atlasAssetPath, ref List<string> spriteAssetPathList)
        {
            SpriteAtlas atlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(atlasAssetPath);
            if (atlas != null)
            {
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
            }
        }

        /// <summary>
        /// 获取文件临时输出路径
        /// </summary>
        /// <returns></returns>
        string GetOutPathTemp(bool isClear)
        {
            string outputTargetDir = GetTrueOutPath() + "_temp";
            if (isClear && Directory.Exists(outputTargetDir))
            {
                Directory.Delete(outputTargetDir, true);
            }
            if (!Directory.Exists(outputTargetDir))
            {
                Directory.CreateDirectory(outputTargetDir);
            }
            return outputTargetDir;
        }

        /// <summary>
        /// 获得正在的输出路径
        /// </summary>
        /// <returns></returns>
        string GetTrueOutPath()
        {
            BundlePackConfig m_PackConfig = Util.FileUtil.ReadFromBinary<BundlePackConfig>(BundlePackUtil.GetPackConfigPath());

            options = m_PackConfig.GetBundleOptions();
            buildTarget = m_PackConfig.GetBuildTarget();

            string outputTargetDir = m_PackConfig.OutputDirPath + "/" + buildTarget.ToString() + "/" + AssetBundleConst.ASSETBUNDLE_MAINFEST_NAME;

            if (!Directory.Exists(outputTargetDir))
            {
                Debug.LogError("eternity_assetbunles/StandaloneWindows64/assetbundles  -------> AB 存放路径不存在");
                return string.Empty;
            }
            return outputTargetDir;
        }

        /// <summary>
        /// 移动文件
        /// </summary>
        void MoveFile(string bundlname)
        {
            BundlePackConfig m_PackConfig = Util.FileUtil.ReadFromBinary<BundlePackConfig>(BundlePackUtil.GetPackConfigPath());
            string outputTargetDir = m_PackConfig.OutputDirPath + "/" + m_PackConfig.GetBuildTarget().ToString() + "/" + AssetBundleConst.ASSETBUNDLE_MAINFEST_NAME + "_temp";

            //bund 文件
            string path = string.Format("{0}/{1}", outPath_temp, bundlname);
            string path1 = string.Format("{0}/{1}", outPath_true, bundlname);
            if (File.Exists(path))
            {
                File.Delete(path1);
                File.Move(path, path1);
            }

            //bund manifest文件
            string path2 = string.Format("{0}/{1}.{2}", outPath_temp, bundlname, "manifest");
            string path3 = string.Format("{0}/{1}.{2}", outPath_true, bundlname, "manifest");
            if (File.Exists(path2))
            {
                File.Delete(path3);
                File.Move(path2, path3);
            }
        }
    }
}
