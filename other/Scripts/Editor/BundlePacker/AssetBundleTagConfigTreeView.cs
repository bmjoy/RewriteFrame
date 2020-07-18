using Leyoutech.Core.Loader.Config;
using LeyoutechEditor.Core.EGUI;
using LeyoutechEditor.Core.EGUI.TreeGUI;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace LeyoutechEditor.Core.Packer
{
    public class AssetBundleGroupTreeData
    {
        public AssetBundleGroupData GroupData { get; set; }
        public bool IsGroup { get; set; } = false;
        public int DataIndex { get; set; } = -1;

        public bool IsAddressRepeat { get; set; } = false;

        public List<AssetAddressData> RepeatAddressList { get; set; } = new List<AssetAddressData>();

        public static AssetBundleGroupTreeData Root
        {
            get { return new AssetBundleGroupTreeData(); }
        }
    }
    /// <summary>
    /// AB资源的树型结构
    /// </summary>
    public class AssetBundleTagConfigTreeView : TreeViewWithTreeModel<TreeElementWithData<AssetBundleGroupTreeData>>
    {
        private GUIContent m_AddressRepeatContent;

        public AssetBundleTagConfigTreeView(TreeViewState state, TreeModel<TreeElementWithData<AssetBundleGroupTreeData>> model) : 
            base(state, model)
        {
            m_AddressRepeatContent = EditorGUIUtility.IconContent("console.erroricon.sml", "Address Repeat");
            showBorder = true;
            showAlternatingRowBackgrounds = true;
            Reload();
        }

        protected override float GetCustomRowHeight(int row, TreeViewItem item)
        {
            var viewItem = (TreeViewItem<TreeElementWithData<AssetBundleGroupTreeData>>)item;
            AssetBundleGroupTreeData groupTreeData = viewItem.data.Data;
            if(groupTreeData.IsGroup)
            {
                return EditorGUIUtility.singleLineHeight + 2;
            }else
            {
                return rowHeight = EditorGUIUtility.singleLineHeight * 3 + 4;
            }
        }

        protected override bool CanMultiSelect(TreeViewItem item)
        {
            return false;
        }

        protected override bool CanStartDrag(CanStartDragArgs args)
        {
            return false;
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            var item = (TreeViewItem<TreeElementWithData<AssetBundleGroupTreeData>>)args.item;
            AssetBundleGroupTreeData groupTreeData = item.data.Data;

            Rect contentRect = args.rowRect;
            contentRect.x += GetContentIndent(item);
            contentRect.width -= GetContentIndent(item);

            GUILayout.BeginArea(contentRect);
            {
                AssetBundleGroupData groupData = groupTreeData.GroupData;
                if(groupTreeData.IsGroup)
                {
                    EditorGUILayout.BeginHorizontal();
                    {
                        string groupName = groupData.GroupName;
                        if(groupData.IsMain)
                        { 
                            groupName += "(Main)";
                        }
                        groupName += "  " + groupData.AssetDatas.Count;
                        EditorGUILayout.LabelField(new GUIContent(groupName));
                        if(groupTreeData.IsAddressRepeat)
                        {
                            if(GUILayout.Button(m_AddressRepeatContent))
                            {
                                SetExpanded(args.item.id,true);
                            }
                        }
                        GUILayout.FlexibleSpace();
                    }
                    EditorGUILayout.EndHorizontal();
                }
                else
                {
                    GUILayout.BeginHorizontal();
                    {
                        if (groupTreeData.IsAddressRepeat)//如果资源有重复的添加提示
                        {
                            if (GUILayout.Button(m_AddressRepeatContent,GUILayout.Width(24)))
                            {
                                Vector2 pos = GUIUtility.GUIToScreenPoint(Input.mousePosition);
                                AssetAddressRepeatPopupWindow.GetWindow().ShowWithParam(groupTreeData.RepeatAddressList, pos);
                            }
                        }else
                        {
                            GUILayout.Label(GUIContent.none, GUILayout.Width(24));
                        }

                        EditorGUIUtil.BeginLabelWidth(60);
                        {
                            AssetAddressData assetData = groupData.AssetDatas[groupTreeData.DataIndex];
                            EditorGUILayout.LabelField(new GUIContent("" + groupTreeData.DataIndex), GUILayout.Width(20));
                            EditorGUILayout.TextField("address:", assetData.AssetAddress);
                            GUILayout.BeginVertical();
                            {
                                EditorGUILayout.TextField("path:", assetData.AssetPath);
                                EditorGUILayout.TextField("bundle:", assetData.BundlePath);
                                EditorGUILayout.TextField("labels:", string.Join(",", assetData.Labels));
                            }
                            GUILayout.EndVertical();
                        }
                        EditorGUIUtil.EndLableWidth();
                    }
                    GUILayout.EndHorizontal();
                }
            }
            GUILayout.EndArea();
            
        }
    }
}
