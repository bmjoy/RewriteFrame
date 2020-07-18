using LeyoutechEditor.Core.Packer;
using LeyoutechEditor.Core.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.U2D;

namespace LeyoutechEditor.Core.BundleDepend
{
    public class AssetDependFinder
    {
        public Action<string, float> ProgressCallback { get; set; }//处理进度的回调<当前处理的文件，总的进度>

        private int m_FindIndex = 0;//当前处理的资源的索引
        
        private List<string> m_BundleAssetPathList = null;//所有指定的资源的List
        private Dictionary<string, string> m_SpriteInAtlasDic = new Dictionary<string, string>();//由于需要将Sprite与Atlas一起打包，需要特别处理Sprite
        private Dictionary<string, List<string>> m_BundleDependAssetDic = new Dictionary<string, List<string>>();//对应的资源引用的其它资源
        private Dictionary<string, int> m_AssetUsedCountDic = new Dictionary<string, int>();//统计到的资源的使用次数

        /// <summary>
        /// 根据配置文件查找资源的依赖情况
        /// </summary>
        /// <param name="tagConfig"></param>
        public void Find(AssetBundleTagConfig tagConfig)
        {
            m_BundleAssetPathList = (from groupData in tagConfig.GroupDatas
                                   from detailData in groupData.AssetDatas
                                   select detailData.AssetPath).ToList();
            m_FindIndex = 0;
            DealWithAtlas();

            //处理非Atlas的资源，查找其引用的资源
            m_BundleAssetPathList.ForEach((path) =>
            {
                if(!(Path.GetExtension(path).ToLower() == ".spriteatlas"))
                {

                    ProgressCallback?.Invoke(path, m_FindIndex / (float)m_BundleAssetPathList.Count);
                    m_FindIndex++;

                    List<string> depends = new List<string>();
                    FindAssetDependExcludeBundle(path, depends, new string[] { ".cs" , ".shader"});
                    m_BundleDependAssetDic.Add(path,depends);
                }
            });

            //统计所有资源的使用次数
            foreach(var kvp in m_BundleDependAssetDic)
            {
                foreach(var path in kvp.Value)
                {
                    if(m_AssetUsedCountDic.ContainsKey(path))
                    {
                        m_AssetUsedCountDic[path]++;
                    }else
                    {
                        m_AssetUsedCountDic.Add(path, 1);
                    }
                }
            }
        }

        /// <summary>
        /// 获取所有被重复引用的资源
        /// </summary>
        /// <returns></returns>
        public Dictionary<string,int> GetRepeatUsedAssets()
        {
            Dictionary<string, int> result = new Dictionary<string, int>();
            foreach(var kvp in m_AssetUsedCountDic)
            {
                if(kvp.Value>1)
                {
                    result.Add(kvp.Key, kvp.Value);
                }
            }
            return result;
        }

        /// <summary>
        /// 通过资源的路径，查找被使用的AB
        /// </summary>
        /// <param name="assetPath"></param>
        /// <returns></returns>
        public string[] GetBundleByUsedAsset(string assetPath)
        {
            List<string> result = new List<string>();
            foreach (var kvp in m_BundleDependAssetDic)
            {
                if(kvp.Value.IndexOf(assetPath)>0)
                {
                    result.Add(kvp.Key);
                }
            }
            return result.ToArray();
        }
        /// <summary>
        /// 查找依赖的资源
        /// </summary>
        /// <param name="assetPath">资源路径</param>
        /// <param name="depends">依赖的资源的List</param>
        /// <param name="excludeExtension">忽略的引用资源</param>
        private void FindAssetDependExcludeBundle(string assetPath,List<string> depends, string[] excludeExtension)
        {
            string[] directDepends = AssetDatabase.GetDependencies(assetPath, false);
            foreach(var path in directDepends)
            {
                if(path.StartsWith("Packages/"))
                {
                    continue;
                }
                string ext = Path.GetExtension(path).ToLower();
                //对于指定需要忽略的资源、本身是AB的资源以及Sprite位于Atlas中的依赖的资源需要直接Pass掉
                if(path!=assetPath && Array.IndexOf(excludeExtension,ext) < 0 && m_BundleAssetPathList.IndexOf(path)<0 && 
                    depends.IndexOf(path)<0 && !m_SpriteInAtlasDic.ContainsKey(path))
                {
                    depends.Add(path);

                    FindAssetDependExcludeBundle(path,depends,excludeExtension);
                }
            }
        }
        /// <summary>
        /// 处理Atlas，将Sprite与Atlas一起处理
        /// </summary>
        private void DealWithAtlas()
        {
            List<string> atlasPaths = (from path in m_BundleAssetPathList
                                       where Path.GetExtension(path).ToLower() == ".spriteatlas"
                                       select path).ToList();
            atlasPaths.ForEach((atlasPath) =>
            {
                ProgressCallback?.Invoke(atlasPath, m_FindIndex / (float)m_BundleAssetPathList.Count);
                m_FindIndex++;

                SpriteAtlas atlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(atlasPath);
                if (atlas == null)
                {
                    Debug.LogError("AssetDependFinder::Find->atlas is null. path = " + atlasPath);
                }
                else
                {
                    string[] spriteInAtlas = SpriteAtlasUtil.GetDependAssets(atlas);

                    m_BundleDependAssetDic.Add(atlasPath, new List<string>(spriteInAtlas));

                    foreach (var spritePath in spriteInAtlas)
                    {
                        m_SpriteInAtlasDic.Add(spritePath, atlasPath);
                    }
                }
            });
        }
        
    }
}
