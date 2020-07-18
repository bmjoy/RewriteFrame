using Leyoutech.Core.Loader;
using Leyoutech.Core.Loader.Config;
using Leyoutech.Core.Util;
using LeyoutechEditor.Core.EGUI;
using ReflectionMagic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using DotAssetManager = Leyoutech.Core.Loader.AssetManager;
using UnityObject = UnityEngine.Object;

namespace LeyoutechEditor.Core.BundleView
{
    /// <summary>
    /// 用于显示AB资源的加载过程，及各个资源在内存中存在情况
    /// </summary>
    public class BundleViewWindow : EditorWindow
    {
        [MenuItem("Custom/Asset Bundle/Bundle View Window")]
        public static void ShowWin()
        {
            BundleViewWindow win = EditorWindow.GetWindow<BundleViewWindow>();
            win.titleContent = new GUIContent("Bundle Packer");
            win.autoRepaintOnSceneChange = true;
            win.Show();
        }

        private Dictionary<string, bool> m_AssetNodeFoldoutDic = new Dictionary<string, bool>();//各个AssetNode的折叠情况
        private Dictionary<string, bool> m_BundleNodeFoldoutDic = new Dictionary<string, bool>();//BundleNode的折叠情况

        //从AssetBundleLoader中得到需要的字段
        private AssetBundleLoader m_BundleLoader = null;
        private Dictionary<string, AssetNode> m_AssetNodeDic = null;
        private Dictionary<string, BundleNode> m_BundleNodeDic = null;
        private AssetAddressConfig m_AssetAddressConfig = null;
        private AssetBundleManifest m_AssetBundleManifest = null;
        
        private Color32 m_NodeBGColor = new Color32(95, 158, 160, 255);
        private bool m_IsShowAssetNodeMainBundle = true;//是否显示资源所在的主Bundle
        private bool m_IsShowAssetNodeDependBundle = true;//是否显示资源所在的主Bundle依赖的其它Bundle
        private bool m_IsShowLoadingAssetNode = true;//是否显示正在加载中的资源

        private bool m_IsShowBundleNodeDirect = true;//是否显示Bundle的直接依赖的其它Bundle
        private bool m_IsShowLoadingBundleNode = true;//是否显示正在加载中的Bundle

        private List<AssetNode> m_AssetNodes = new List<AssetNode>();//过滤后的AssetNode
        private List<BundleNode> m_BundleNodes = new List<BundleNode>();//过滤后
        private bool m_IsChanged = true;
        private string m_SearchText = "";//文本搜索

        private string[] m_ToolbarTitle = new string[] {
            "Asset Node",
            "Bundle Node",
        };

        private int m_ToolbarSelectIndex = 0;
        private SearchField m_SearchField = null;
        
        private Vector2 m_ScrollPos = Vector2.zero;
        private void OnGUI()
        {
            if(!InitLoader())
            {
                return;
            }
            if(m_SearchField == null)
            {
                m_SearchField = new SearchField();
                m_SearchField.autoSetFocusOnFindCommand = true;
            }
            EditorGUILayout.BeginHorizontal("toolbar", GUILayout.ExpandWidth(true));
            {
                int selectedIndex = GUILayout.Toolbar(m_ToolbarSelectIndex, m_ToolbarTitle, EditorStyles.toolbarButton,GUILayout.MaxWidth(200));
                if(selectedIndex!=m_ToolbarSelectIndex)
                {
                    m_SearchText = "";
                    m_ToolbarSelectIndex = selectedIndex;
                    m_IsChanged = true;
                    m_ScrollPos = Vector2.zero;
                }
                GUILayout.FlexibleSpace();
                //强制进行资源的清理回收
                if (GUILayout.Button("GC", EditorStyles.toolbarButton, GUILayout.Width(40)))
                {
                    m_BundleLoader.UnloadUnusedAssets(null);
                }
                //将当前显示的内容存储到磁盘中
                if (GUILayout.Button("Export", EditorStyles.toolbarButton, GUILayout.Width(60)))
                {
                    ExportToDisk(false);
                }
                //将所在的内容存储到磁盘中
                if (GUILayout.Button("Export All",EditorStyles.toolbarButton,GUILayout.Width(60)))
                {
                    ExportToDisk(true);
                }

                string tempSearchText = m_SearchField.OnToolbarGUI(m_SearchText);
                if(tempSearchText!=m_SearchText)
                {
                    m_IsChanged = true;
                    m_SearchText = tempSearchText;
                }
            }
            EditorGUILayout.EndHorizontal();
            if (m_IsChanged)
            {
                if(m_ToolbarSelectIndex == 0)//显示AssetNode的标签页
                {
                    m_AssetNodes.Clear();
                    foreach(var nodeKVP in m_AssetNodeDic)
                    {
                        AssetNode node = nodeKVP.Value;
                        if(!m_IsShowLoadingAssetNode)
                        {
                            bool isDone = node.AsDynamic().IsDone;
                            if(!isDone)
                            {
                                continue;
                            }
                        }
                        if(string.IsNullOrEmpty(m_SearchText) || nodeKVP.Key.ToLower().Contains(m_SearchText.ToLower()))
                        {
                            m_AssetNodes.Add(node);
                        }
                    }
                    m_AssetNodes.Sort((item1, item2) =>
                    {
                        string path1 = item1.AsDynamic().m_AssetPath;
                        string path2 = item2.AsDynamic().m_AssetPath;
                        return path1.CompareTo(path2);
                    });
                }
                else if(m_ToolbarSelectIndex == 1)//显示BundleNode的标签页
                {
                    m_BundleNodes.Clear();
                    foreach (var nodeKVP in m_BundleNodeDic)
                    {
                        BundleNode node = nodeKVP.Value;
                        if (!m_IsShowLoadingBundleNode)
                        {
                            bool isDone = node.AsDynamic().IsDone;
                            if (!isDone)
                            {
                                continue;
                            }
                        }
                        if (string.IsNullOrEmpty(m_SearchText) || nodeKVP.Key.ToLower().Contains(m_SearchText.ToLower()))
                        {
                            m_BundleNodes.Add(node);
                        }
                    }
                    m_BundleNodes.Sort((item1, item2) =>
                    {
                        string path1 = item1.AsDynamic().m_BundlePath;
                        string path2 = item2.AsDynamic().m_BundlePath;
                        return path1.CompareTo(path2);
                    });
                }
            }

            if(m_ToolbarSelectIndex == 0)
            {
                bool isShowLoading = EditorGUILayout.Toggle("Show Loading Node:", m_IsShowLoadingAssetNode);
                if(isShowLoading!= m_IsShowLoadingAssetNode)
                {
                    m_IsChanged = true;
                    m_IsShowLoadingAssetNode = isShowLoading;
                }
                m_IsShowAssetNodeMainBundle = EditorGUILayout.Toggle("Show Main Bundle:", m_IsShowAssetNodeMainBundle);
                m_IsShowAssetNodeDependBundle = EditorGUILayout.Toggle("Show Depend Bundle:", m_IsShowAssetNodeDependBundle);
            }
            else if(m_ToolbarSelectIndex == 1)
            {
                bool isShowLoading = EditorGUILayout.Toggle("Show Loading Node:", m_IsShowLoadingBundleNode);
                if (isShowLoading != m_IsShowLoadingBundleNode)
                {
                    m_IsChanged = true;
                    m_IsShowLoadingBundleNode = isShowLoading;
                }
                m_IsShowBundleNodeDirect = EditorGUILayout.Toggle("Show Depend Bundle:", m_IsShowBundleNodeDirect);
            }

            m_ScrollPos = EditorGUILayout.BeginScrollView(m_ScrollPos,EditorStyles.helpBox);
            {
                if(m_ToolbarSelectIndex == 0)
                {
                    DrawAssetNodes();
                }else if(m_ToolbarSelectIndex == 1)
                {
                    DrawBundleNodes();
                }
            }
            EditorGUILayout.EndScrollView();
        }

        /// <summary>
        /// 初始化Window需要的数据
        /// </summary>
        /// <returns></returns>
        private bool InitLoader()
        {
            if (!EditorApplication.isPlaying)
            {
                return false;
            }
            if (m_BundleLoader != null)
            {
                return true;
            }
            //获取AssetManager
            bool isInitSuccess = (bool)DotAssetManager.GetInstance().AsDynamic().m_IsInit;
            if (!isInitSuccess)
            {
                return false;
            }

            DotAssetManager assetManager = DotAssetManager.GetInstance();
            dynamic loader = (AssetBundleLoader)assetManager.AsDynamic().m_AssetLoader;
            if (!(loader is AssetBundleLoader))
            {
                return false;
            }
            m_BundleLoader = loader;
            dynamic loaderDynamic = m_BundleLoader.AsDynamic();
            m_AssetAddressConfig = loaderDynamic.m_AssetAddressConfig;
            m_AssetBundleManifest = loaderDynamic.m_AssetBundleManifest;
            m_AssetNodeDic = loaderDynamic.m_AssetNodeDic;
            m_BundleNodeDic = loaderDynamic.m_BundleNodeDic;
            return true;
        }
        /// <summary>
        /// 绘制AssetNode结点
        /// </summary>
        private void DrawAssetNodes()
        {
            foreach (var node in m_AssetNodes)
            {
                string assetPath = node.AsDynamic().m_AssetPath;
                if (!m_AssetNodeFoldoutDic.TryGetValue(assetPath, out bool isFoldout))
                {
                    isFoldout = false;
                    m_AssetNodeFoldoutDic.Add(assetPath, isFoldout);
                }

                bool isDone = node.AsDynamic().IsDone;
                if(!isDone)
                {
                    assetPath += "(Loading)";
                }

                m_AssetNodeFoldoutDic[assetPath] = EditorGUILayout.Foldout(isFoldout, assetPath);
                if (isFoldout)
                {
                    DrawAssetNode(node, m_IsShowAssetNodeMainBundle, m_IsShowAssetNodeDependBundle);
                }
            }
        }

        /// <summary>
        /// 绘制AssetNode的详细信息
        /// </summary>
        /// <param name="assetNode">AssetNode数据</param>
        /// <param name="showMainBundle">是否显示其所在的Bundle</param>
        /// <param name="showDependBundle">是否显示依赖的Bundle</param>
        private void DrawAssetNode(AssetNode assetNode,bool showMainBundle = false,bool showDependBundle = false)
        {
            dynamic assetNodeDynamic = assetNode.AsDynamic();
            string assetPath = assetNodeDynamic.m_AssetPath;
            bool isDone = assetNodeDynamic.IsDone;
            bool isAlive = assetNodeDynamic.IsAlive();
            int loadCount = assetNodeDynamic.m_LoadCount;
            List<WeakReference> weakAssets = assetNodeDynamic.m_WeakAssets;

            EditorGUIUtil.BeginGUIBackgroundColor(m_NodeBGColor);
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                {
                    EditorGUIUtil.BeginGUIColor(Color.white);
                    {
                        EditorGUIUtil.BeginIndent();
                        {
                            EditorGUILayout.LabelField("Asset Node:");
                            EditorGUIUtil.BeginIndent();
                            {
                                EditorGUILayout.LabelField($"Asset Path:{assetPath}{(isDone?"":"  (Loading)")}");
                                EditorGUILayout.LabelField($"Is Alive:{isAlive}");
                                EditorGUILayout.LabelField($"Load Count:{loadCount}");
                                if (weakAssets.Count > 0)
                                {
                                    EditorGUILayout.LabelField("Weak Ref:");
                                    EditorGUIUtil.BeginIndent();
                                    {
                                        foreach (var weakRef in weakAssets)
                                        {
                                            if (!UnityObjectExtension.IsNull(weakRef.Target))
                                            {
                                                UnityObject uObj = weakRef.Target as UnityObject;
                                                EditorGUILayout.LabelField($"Name:{uObj.name}");
                                            }
                                        }
                                    }
                                    EditorGUIUtil.EndIndent();
                                }
                            }
                            EditorGUIUtil.EndIndent();

                            if (showMainBundle || showDependBundle)
                            {
                                BundleNode mainBundleNode = assetNodeDynamic.m_BundleNode;
                                string mainBundlePath = mainBundleNode.AsDynamic().m_BundlePath;
                                EditorGUILayout.LabelField("Bundle Node:");
                                EditorGUIUtil.BeginIndent();
                                {
                                    if(showMainBundle)
                                    {
                                        EditorGUILayout.LabelField("Main Bundle:");
                                        EditorGUIUtil.BeginIndent();
                                        {
                                            DrawBundleNode(mainBundleNode);
                                        }
                                        EditorGUIUtil.EndIndent();
                                    }
                                    if(showDependBundle)
                                    {
                                        EditorGUILayout.LabelField("Depend Bundle:");
                                        string[] depends = m_AssetBundleManifest.GetAllDependencies(mainBundlePath);
                                        EditorGUIUtil.BeginIndent();
                                        {
                                            foreach(var depend in depends)
                                            {
                                                BundleNode dependNode = m_BundleNodeDic[depend];
                                                DrawBundleNode(dependNode);
                                                EditorGUILayout.Space();
                                            }
                                        }
                                        EditorGUIUtil.EndIndent();
                                    }
                                }
                                EditorGUIUtil.EndIndent();

                            }
                        }
                        EditorGUIUtil.EndIndent();
                    }
                    EditorGUIUtil.EndGUIColor();

                }
                EditorGUILayout.EndVertical();
            }
            EditorGUIUtil.EndGUIBackgroundColor();
        }

        /// <summary>
        /// 导出文本到磁盘中
        /// </summary>
        /// <param name="fileDiskPath"></param>
        /// <param name="isAll"></param>
        private void ExportAssetNodes(string fileDiskPath,bool isAll)
        {
            StringBuilder sb = new StringBuilder();
            List<AssetNode> exportNodes = new List<AssetNode>();
            if(isAll)
            {
                exportNodes.AddRange(m_AssetNodeDic.Values.ToArray());
            }else
            {
                exportNodes.AddRange(m_AssetNodes);
            }
            foreach(var node in exportNodes)
            {
                dynamic nodeDynamic = node.AsDynamic();
                string assetPath = nodeDynamic.m_AssetPath;
                BundleNode mainBundleNode = nodeDynamic.m_BundleNode;
                int loadCount = nodeDynamic.m_LoadCount;

                sb.AppendLine($"Asset Path:{assetPath}");
                sb.AppendLine($"Load Count:{loadCount}");
                if(m_IsShowAssetNodeMainBundle)
                {
                    sb.AppendLine("Main Bundle:");
                    sb.AppendLine(GetBundleNodeDesc(mainBundleNode, "    "));
                }
                if(m_IsShowAssetNodeDependBundle)
                {
                    string mainBundlePath = mainBundleNode.AsDynamic().m_BundlePath;
                    string[] depends = m_AssetBundleManifest.GetAllDependencies(mainBundlePath);
                    sb.AppendLine("Depend Bundle:");
                    foreach (var depend in depends)
                    {
                        BundleNode dependNode = m_BundleNodeDic[depend];
                        sb.AppendLine(GetBundleNodeDesc(dependNode, "    "));
                    }
                }
                sb.AppendLine();
            }
            File.WriteAllText(fileDiskPath, sb.ToString());
        }
        /// <summary>
        /// 绘制当前使用的BundleNode的信息
        /// </summary>
        private void DrawBundleNodes()
        {
            foreach (var node in m_BundleNodes)
            {
                string bundlePath = node.AsDynamic().m_BundlePath;
                if (!m_BundleNodeFoldoutDic.TryGetValue(bundlePath, out bool isFoldout))
                {
                    isFoldout = false;
                    m_BundleNodeFoldoutDic.Add(bundlePath, isFoldout);
                }

                bool isDone = node.AsDynamic().IsDone;
                if(!isDone)
                {
                    bundlePath += "(Loading)";
                }

                m_BundleNodeFoldoutDic[bundlePath] = EditorGUILayout.Foldout(isFoldout, bundlePath);
                if (isFoldout)
                {
                    EditorGUIUtil.BeginGUIBackgroundColor(m_NodeBGColor);
                    {
                        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                        {
                            EditorGUIUtil.BeginIndent();
                            {
                                EditorGUILayout.LabelField("Bundle Node:");
                                EditorGUIUtil.BeginIndent();
                                {
                                    DrawBundleNode(node);
                                }
                                EditorGUIUtil.EndIndent();

                                if (m_IsShowBundleNodeDirect)
                                {
                                    EditorGUILayout.LabelField("Direct Bundle:");
                                    string[] depends = m_AssetBundleManifest.GetDirectDependencies(bundlePath);
                                    EditorGUIUtil.BeginIndent();
                                    {
                                        foreach (var depend in depends)
                                        {
                                            BundleNode dependNode = m_BundleNodeDic[depend];
                                            DrawBundleNode(dependNode);
                                            EditorGUILayout.Space();
                                        }
                                    }
                                    EditorGUIUtil.EndIndent();
                                }
                            }
                            EditorGUIUtil.EndIndent();
                        }
                        EditorGUILayout.EndVertical();
                    }
                    EditorGUIUtil.EndGUIBackgroundColor();
                }
            }
        }

        /// <summary>
        /// 绘制BundleNode的详细信息
        /// </summary>
        /// <param name="bundleNode"></param>
        private void DrawBundleNode(BundleNode bundleNode)
        {
            dynamic bundleNodeDynamic = bundleNode.AsDynamic();
            string bundlePath = bundleNodeDynamic.m_BundlePath;
            bool isDone = bundleNodeDynamic.IsDone;
            int refCount = bundleNodeDynamic.m_RefCount;
            
            EditorGUILayout.LabelField($"Bundle Path:{bundlePath}{(isDone?"":"   (Loading)")}");
            if(isDone)
            {
                bool isScene = bundleNodeDynamic.IsScene;
                EditorGUILayout.LabelField($"IsScene:{isScene}");
            }
            EditorGUILayout.LabelField($"Ref Count:{refCount}");
        }

        /// <summary>
        /// 导出Bundle的使用情况到磁盘中
        /// </summary>
        /// <param name="fileDiskPath"></param>
        /// <param name="isAll"></param>
        private void ExportBundleNodes(string fileDiskPath, bool isAll)
        {
            StringBuilder sb = new StringBuilder();
            List<BundleNode> exportNodes = new List<BundleNode>();
            if (isAll)
            {
                exportNodes.AddRange(m_BundleNodeDic.Values.ToArray());
            }
            else
            {
                exportNodes.AddRange(m_BundleNodes);
            }
            foreach (var node in exportNodes)
            {
                sb.AppendLine(GetBundleNodeDesc(node));
                if(m_IsShowBundleNodeDirect)
                {
                    string bundlePath = node.AsDynamic().m_BundlePath;
                    string[] depends = m_AssetBundleManifest.GetAllDependencies(bundlePath);
                    sb.AppendLine("Depend Bundle:");
                    foreach (var depend in depends)
                    {
                        sb.AppendLine("    " + depend);
                    }
                }
                sb.AppendLine();
            }
            File.WriteAllText(fileDiskPath, sb.ToString());
        }

        private string GetBundleNodeDesc(BundleNode bundleNode,string prefix = "")
        {
            dynamic bundleNodeDynamic = bundleNode.AsDynamic();
            string bundlePath = bundleNodeDynamic.m_BundlePath;
            bool isScene = bundleNodeDynamic.IsScene();
            int refCount = bundleNodeDynamic.m_RefCount;

            return $"{prefix}Bundle Path:{bundlePath}\n{prefix}Ref Count:{refCount}\n{prefix}IsScene:{isScene}";
        }

        private void ExportToDisk(bool isAll)
        {
            string defaultFileName = "";
            if (m_ToolbarSelectIndex == 0)
            {
                defaultFileName = "asset_nodes";
            }
            else if (m_ToolbarSelectIndex == 1)
            {
                defaultFileName = "bundle_nodes";
            };

            string fileDiskPath = EditorUtility.SaveFilePanel("Save Nodes", "D:\\", defaultFileName, "txt");
            if (!string.IsNullOrEmpty(fileDiskPath))
            {
                if (m_ToolbarSelectIndex == 0)
                {
                    ExportAssetNodes(fileDiskPath, isAll);
                }
                else if (m_ToolbarSelectIndex == 1)
                {
                    ExportBundleNodes(fileDiskPath, isAll);
                }
            }
        }
    }
}
