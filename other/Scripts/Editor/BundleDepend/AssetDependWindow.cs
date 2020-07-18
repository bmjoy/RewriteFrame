using LeyoutechEditor.Core.EGUI;
using LeyoutechEditor.Core.EGUI.TreeGUI;
using LeyoutechEditor.Core.Packer;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace LeyoutechEditor.Core.BundleDepend
{
    public class DependTreeData
    {
        public string AssetPath;
        public int RepeatCount = 0;

        public bool IsBundle = false;

        public static DependTreeData Root
        {
            get { return new DependTreeData(); }
        }
    }

    public class AssetDependWindow : EditorWindow
    {
        [MenuItem("Custom/Asset Bundle/Bundle Depend Window")]
        public static void ShowWin()
        {
            AssetDependWindow win = EditorWindow.GetWindow<AssetDependWindow>();
            win.titleContent = new GUIContent("Bundle Depend");
            win.Show();
        }

        AssetDependFinder m_Finder = null;
        private void OnEnable()
        {
            AssetBundleTagConfig tagConfig = Util.FileUtil.ReadFromBinary<AssetBundleTagConfig>(BundlePackUtil.GetTagConfigPath());
            m_Finder = BundlePackUtil.CreateAssetDependFinder(tagConfig, true);
        }

        private GUIStyle m_TitleStyle = null;
        private Vector2 m_ScrollPos = Vector2.zero;
        private AssetDependTreeView m_DependTreeView;
        private TreeViewState m_DependTreeViewState;

        private void OnGUI()
        {
            if (m_TitleStyle == null)
            {
                m_TitleStyle = new GUIStyle(EditorStyles.label);
                m_TitleStyle.alignment = TextAnchor.MiddleCenter;
                m_TitleStyle.fontSize = 24;
                m_TitleStyle.fontStyle = FontStyle.Bold;
            }

            DrawToolbar();

            EditorGUILayout.LabelField(new GUIContent("Asset Dependencies"), m_TitleStyle, GUILayout.Height(24));
            EditorGUILayout.LabelField(GUIContent.none, GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));

            Rect areaRect = GUILayoutUtility.GetLastRect();
            areaRect.x += 1;
            areaRect.width -= 2;
            areaRect.y += 1;
            areaRect.height -= 2;

            EditorGUIUtil.DrawAreaLine(areaRect, Color.blue);

            Rect dependTreeViewRect = areaRect;
            dependTreeViewRect.x += 1;
            dependTreeViewRect.width -= 2;
            dependTreeViewRect.y += 1;
            dependTreeViewRect.height -= 2;
            if (m_DependTreeView == null)
            {
                InitDependTreeView();
                RefreshDependTreeView();
            }
            m_DependTreeView?.OnGUI(dependTreeViewRect);
        }
        /// <summary>
        /// 初始化TreeView
        /// </summary>
        private void InitDependTreeView()
        {
            m_DependTreeViewState = new TreeViewState();
            TreeModel<TreeElementWithData<DependTreeData>> data = new TreeModel<TreeElementWithData<DependTreeData>>(
               new List<TreeElementWithData<DependTreeData>>()
               {
                    new TreeElementWithData<DependTreeData>(DependTreeData.Root,"",-1,-1),
               });

            m_DependTreeView = new AssetDependTreeView(m_DependTreeViewState,data);
            m_DependTreeView.DependWin = this;
        }
        /// <summary>
        /// 刷新数据
        /// </summary>
        private void RefreshDependTreeView()
        {
            TreeModel<TreeElementWithData<DependTreeData>> treeModel = m_DependTreeView.treeModel;
            TreeElementWithData<DependTreeData> treeModelRoot = treeModel.root;
            treeModelRoot.children?.Clear();

            Dictionary<string, int> repeatAssetDic = m_Finder.GetRepeatUsedAssets();

            List<string> paths = new List<string>();
            paths.AddRange(repeatAssetDic.Keys);
            paths.Sort();

            foreach(var path in paths)
            {
                DependTreeData adData = new DependTreeData();
                adData.AssetPath = path;
                adData.RepeatCount = repeatAssetDic[path];
                adData.IsBundle = false;

                TreeElementWithData<DependTreeData> assetPathTreeData = new TreeElementWithData<DependTreeData>(adData, "", 0, m_DependTreeView.NextID);
                treeModel.AddElement(assetPathTreeData, treeModelRoot, treeModelRoot.hasChildren ? treeModelRoot.children.Count : 0);

                string[] usedBundles = m_Finder.GetBundleByUsedAsset(path);
                foreach(var bundle in usedBundles)
                {
                    DependTreeData bundleData = new DependTreeData();
                    bundleData.AssetPath = bundle;
                    bundleData.IsBundle = true;

                    TreeElementWithData<DependTreeData> dependTreeData = new TreeElementWithData<DependTreeData>(bundleData, "", 1, m_DependTreeView.NextID);
                    treeModel.AddElement(dependTreeData, assetPathTreeData, assetPathTreeData.hasChildren ? assetPathTreeData.children.Count : 0);
                }
            }
        }

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal("toolbar", GUILayout.ExpandWidth(true));
            {
                if (GUILayout.Button("Start Check", "toolbarbutton", GUILayout.Width(100)))
                {
                    EditorApplication.delayCall += RefreshDependTreeView;
                }
                GUILayout.FlexibleSpace();
            }
            EditorGUILayout.EndHorizontal();
        }
       
    }
}
