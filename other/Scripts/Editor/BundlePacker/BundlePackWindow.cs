using Leyoutech.Core.Loader.Config;
using LeyoutechEditor.Core.BundleDepend;
using LeyoutechEditor.Core.EGUI;
using LeyoutechEditor.Core.EGUI.TreeGUI;
using LeyoutechEditor.Core.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace LeyoutechEditor.Core.Packer
{
    public class BundlePackWindow : EditorWindow
    {
        [MenuItem("Custom/Asset Bundle/Bundle Pack Window")]
        public static void ShowWin()
        {
            BundlePackWindow win = EditorWindow.GetWindow<BundlePackWindow>();
            win.titleContent = new GUIContent("Bundle Packer");
            win.Show();
        }

        private AssetBundleTagConfigTreeView m_DetailGroupTreeView;
        private TreeViewState m_DetailGroupTreeViewState;

        private AssetBundleTagConfig m_TagConfig = null;
        private BundlePackConfigGUI m_PackConfigGUI;

        private void OnEnable()
        {
            m_TagConfig = Util.FileUtil.ReadFromBinary<AssetBundleTagConfig>(BundlePackUtil.GetTagConfigPath());
            m_PackConfigGUI = new BundlePackConfigGUI();
        }
        /// <summary>
        /// 初始化资源查看的TreeView
        /// </summary>
        private void InitDetailGroupTreeView()
        {
            m_DetailGroupTreeViewState = new TreeViewState();
            TreeModel<TreeElementWithData<AssetBundleGroupTreeData>> data = new TreeModel<TreeElementWithData<AssetBundleGroupTreeData>>(
               new List<TreeElementWithData<AssetBundleGroupTreeData>>()
               {
                    new TreeElementWithData<AssetBundleGroupTreeData>(AssetBundleGroupTreeData.Root,"",-1,-1),
               });

            m_DetailGroupTreeView = new AssetBundleTagConfigTreeView(m_DetailGroupTreeViewState, data);
        }
        /// <summary>
        /// 生成TreeView需要的数据
        /// </summary>
        private void FilterTreeModel()
        {
            TreeModel<TreeElementWithData<AssetBundleGroupTreeData>> treeModel = m_DetailGroupTreeView.treeModel;
            TreeElementWithData<AssetBundleGroupTreeData> treeModelRoot = treeModel.root;
            treeModelRoot.children?.Clear();

            if(m_TagConfig.GroupDatas == null)
            {
                return;
            }

            List<AssetAddressData> dataList = (from groupData in m_TagConfig.GroupDatas where groupData.IsMain
                                        from detailData in groupData.AssetDatas
                                        select detailData).ToList();

            for (int i = 0; i < m_TagConfig.GroupDatas.Count; i++)
            {
                AssetBundleGroupData groupData = m_TagConfig.GroupDatas[i];
                TreeElementWithData<AssetBundleGroupTreeData> groupElementData = new TreeElementWithData<AssetBundleGroupTreeData>(
                    new AssetBundleGroupTreeData()
                    {
                        IsGroup = true,
                        GroupData = groupData,
                    }, "", 0, (i + 1) * 100);

                treeModel.AddElement(groupElementData, treeModelRoot, treeModelRoot.hasChildren ? treeModelRoot.children.Count : 0);

                bool isAddressRepeat = false;
                for (int j = 0; j < groupData.AssetDatas.Count; ++j)
                {
                    AssetAddressData detailData = groupData.AssetDatas[j];
                    List<AssetAddressData> repeatList = (from data in dataList
                                                         where data.AssetAddress == detailData.AssetAddress
                                                         select data).ToList();
                    
                    if (FilterAssetDetailData(detailData))
                    {
                        TreeElementWithData<AssetBundleGroupTreeData> elementData = new TreeElementWithData<AssetBundleGroupTreeData>(
                                new AssetBundleGroupTreeData()
                                {
                                    IsGroup = false,
                                    DataIndex = j,
                                    GroupData = groupData,
                                }, "", 1, (i + 1) * 100 + (j + 1));

                        if(repeatList.Count>1)
                        {
                            elementData.Data.IsAddressRepeat = true;
                            elementData.Data.RepeatAddressList = repeatList;
                            if(!isAddressRepeat)
                            {
                                isAddressRepeat = true;
                            }
                        }

                        treeModel.AddElement(elementData, groupElementData, groupElementData.hasChildren ? groupElementData.children.Count : 0);
                    }
                }
                groupElementData.Data.IsAddressRepeat = isAddressRepeat;
            }
        }
        /// <summary>
        /// 根据筛选条件筛选内容
        /// </summary>
        /// <param name="detailData"></param>
        /// <returns></returns>
        private bool FilterAssetDetailData(AssetAddressData detailData)
        {
            if(string.IsNullOrEmpty(m_SearchText))
            {
                return true;
            }

            bool isValid = false;
            if(m_SelecteddSearchParamIndex == 0 || m_SelecteddSearchParamIndex == 1)
            {
                if(!string.IsNullOrEmpty(detailData.AssetAddress))
                {
                    isValid = detailData.AssetAddress.IndexOf(m_SearchText, StringComparison.OrdinalIgnoreCase) >= 0;
                }
            }
            if(!isValid)
            {
                if(m_SelecteddSearchParamIndex == 0 || m_SelecteddSearchParamIndex == 2)
                {
                    if (!string.IsNullOrEmpty(detailData.AssetPath))
                    {
                        isValid = detailData.AssetPath.IndexOf(m_SearchText, StringComparison.OrdinalIgnoreCase) >= 0;
                    }
                }
            }
            if (!isValid)
            {
                if (m_SelecteddSearchParamIndex == 0 || m_SelecteddSearchParamIndex == 3)
                {
                    if (!string.IsNullOrEmpty(detailData.BundlePath))
                    {
                        isValid = detailData.BundlePath.IndexOf(m_SearchText, StringComparison.OrdinalIgnoreCase) >= 0;
                    }
                }
            }
            if(!isValid)
            {
                string label = string.Join(",", detailData.Labels);
                if (m_SelecteddSearchParamIndex == 0 || m_SelecteddSearchParamIndex == 4)
                {
                    if (!string.IsNullOrEmpty(label))
                    {
                        isValid = label.IndexOf(m_SearchText, StringComparison.OrdinalIgnoreCase) >= 0;
                    }
                }
            }
            return isValid;
        }

        private void OnGUI()
        {
            DrawToolbar();

            GUIStyle lableStyle = new GUIStyle(EditorStyles.label);
            lableStyle.alignment = TextAnchor.MiddleCenter;
            EditorGUILayout.LabelField("Asset Detail Group List", lableStyle, GUILayout.ExpandWidth(true));

            Rect lastRect = EditorGUILayout.GetControlRect(GUILayout.Height(600));
            if (m_DetailGroupTreeView == null)
            {
                InitDetailGroupTreeView();
                EditorApplication.delayCall += () =>
                {
                    FilterTreeModel();
                };
            }
            m_DetailGroupTreeView?.OnGUI(lastRect);

            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox,GUILayout.ExpandHeight(true),GUILayout.ExpandWidth(true));
                {
                    m_PackConfigGUI.LayoutGUI();
                }
                EditorGUILayout.EndVertical();
                EditorGUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.MaxWidth(180),GUILayout.ExpandHeight(true));
                {
                    DrawOperation();
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndHorizontal();
        }
        //搜索时检查的属性
        public string[] m_SearchParams = new string[]
        {
            "All",
            "Address",
            "Path",
            "Bundle",
            "Labels",
        };
        private int m_SelecteddSearchParamIndex = 0;
        private string m_SearchText = "";
        private bool m_IsExpandAll = false;

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal("toolbar", GUILayout.ExpandWidth(true));
            {
                if(GUILayout.Button(m_IsExpandAll? "\u25BC" : "\u25BA", "toolbarbutton",GUILayout.Width(60)))//展开或折叠TreeView的项
                {
                    m_IsExpandAll = !m_IsExpandAll;
                    if (m_IsExpandAll)
                    {
                        m_DetailGroupTreeView.ExpandAll();
                    }
                    else
                    {
                        m_DetailGroupTreeView.CollapseAll();
                    }
                }
                EditorGUILayout.Space();

                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Find Auto Group", "toolbarbutton", GUILayout.Width(120)))//开始查找依赖资源
                {
                    BundlePackUtil.FindAndAddAutoGroup(true);

                    m_TagConfig = Util.FileUtil.ReadFromBinary<AssetBundleTagConfig>(BundlePackUtil.GetTagConfigPath());
                    FilterTreeModel();
                }

                if(GUILayout.Button("Remove Auto Group", "toolbarbutton", GUILayout.Width(120)))//删除依赖资源分组
                {
                    BundlePackUtil.DeleteAutoGroup();
                    m_TagConfig = Util.FileUtil.ReadFromBinary<AssetBundleTagConfig>(BundlePackUtil.GetTagConfigPath());
                    FilterTreeModel();
                }

                if (GUILayout.Button("Open Depend Win","toolbarbutton",GUILayout.Width(160)))//打开依赖详细窗口
                {
                    AssetDependWindow.ShowWin();
                }

                int newSelectedIndex = EditorGUILayout.Popup(m_SelecteddSearchParamIndex, m_SearchParams, "ToolbarDropDown", GUILayout.Width(80));
                if(newSelectedIndex != m_SelecteddSearchParamIndex)
                {
                    m_SelecteddSearchParamIndex = newSelectedIndex;
                    FilterTreeModel();
                }

                EditorGUILayout.LabelField("", GUILayout.Width(200));
                Rect lastRect = GUILayoutUtility.GetLastRect();
                Rect searchFieldRect = new Rect(lastRect.x, lastRect.y, 180, 16);
                string newSearchText = EditorGUI.TextField(searchFieldRect, "", m_SearchText, "toolbarSeachTextField"); ;
                Rect searchCancelRect = new Rect(searchFieldRect.x + searchFieldRect.width, searchFieldRect.y, 16, 16);
                if (GUI.Button(searchCancelRect, "", "ToolbarSeachCancelButton"))
                {
                    newSearchText = "";
                    GUI.FocusControl("");
                }
                if(newSearchText != m_SearchText)
                {
                    m_SearchText = newSearchText;
                    FilterTreeModel();

                    m_IsExpandAll = true;
                    m_DetailGroupTreeView.ExpandAll();
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawOperation()
        {
            if (GUILayout.Button("Update Asset Group"))
            {
                BundlePackUtil.UpdateTagConfig();

                m_TagConfig = Util.FileUtil.ReadFromBinary<AssetBundleTagConfig>(BundlePackUtil.GetTagConfigPath());
                FilterTreeModel();
            }
            if(GUILayout.Button("Update Address Config"))
            {
                BundlePackUtil.UpdateAddressConfig();
            }
            if(GUILayout.Button("Create Key Class"))
            {
                BundlePackUtil.CreateAddressKeyClass();
            }
            if (GUILayout.Button("Clear Asset Bundle Names"))
            {
                BundlePackUtil.ClearAssetBundleNames(true);
            }
            if (GUILayout.Button("Set Asset Bundle Names"))
            {
                BundlePackUtil.SetAssetBundleNames(true);
            }

            GUILayout.FlexibleSpace();

            EditorGUIUtil.BeginGUIBackgroundColor(Color.red);
            {
                if(GUILayout.Button("Pack Without Depends",GUILayout.Height(30)))
                {
                    EditorApplication.delayCall += () =>
                    {
						BundlePackUtil.PackBundleSetting packBundleSetting = new BundlePackUtil.PackBundleSetting();
						packBundleSetting.FindDepend = false;
						packBundleSetting.IsShowProgressBar = true;
						packBundleSetting.ResetBundleName = true;
						packBundleSetting.UpdateConfigs = true;
						if (BundlePackUtil.PackBundle(out AssetBundleManifest assetBundleManifest, packBundleSetting))
                        {
                            EditorUtility.DisplayDialog("Success", "Pack AssetBundle Success", "OK");
                        }
                    };
                }
                if (GUILayout.Button("Pack With Depends", GUILayout.Height(30)))
                {
                    EditorApplication.delayCall += () =>
                    {
						BundlePackUtil.PackBundleSetting packBundleSetting = new BundlePackUtil.PackBundleSetting();
						packBundleSetting.FindDepend = true;
						packBundleSetting.IsShowProgressBar = true;
						packBundleSetting.ResetBundleName = true;
						packBundleSetting.UpdateConfigs = true;
						if (BundlePackUtil.PackBundle(out AssetBundleManifest assetBundleManifest, packBundleSetting))
                        {
                            EditorUtility.DisplayDialog("Success", "Pack AssetBundle Success", "OK");
                        }
                    };
                }
            }
            EditorGUIUtil.EndGUIBackgroundColor();
        }
    }
}
