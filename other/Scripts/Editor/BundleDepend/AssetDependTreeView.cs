using LeyoutechEditor.Core.EGUI.TreeGUI;
using LeyoutechEditor.Core.Util;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace LeyoutechEditor.Core.BundleDepend
{
    /// <summary>
    /// 依赖资源的TreeView实现
    /// </summary>
    public class AssetDependTreeView : TreeViewWithTreeModel<TreeElementWithData<DependTreeData>>
    {
        private GUIContent m_WarningIconContent;
        private int m_CurMaxID = 1;
        public int NextID
        {
            get
            {
                return m_CurMaxID++;
            }
        }

        internal AssetDependWindow DependWin { get; set; } = null;

        public AssetDependTreeView(TreeViewState state, TreeModel<TreeElementWithData<DependTreeData>> model) :
            base(state, model)
        {
            m_WarningIconContent = EditorGUIUtility.IconContent("console.warnicon.sml", "Repeat");
            showBorder = true;
            showAlternatingRowBackgrounds = true;
            rowHeight = EditorGUIUtility.singleLineHeight*2;
            Reload();
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
            var item = (TreeViewItem<TreeElementWithData<DependTreeData>>)args.item;
            DependTreeData assetData = item.data.Data;

            if (assetData == null)
            {
                return;
            }
            Rect contentRect = args.rowRect;
            contentRect.x += GetContentIndent(item);
            contentRect.width -= GetContentIndent(item);

            Rect rect = contentRect;
            rect.width -= 80;
            
            if(assetData.IsBundle)
            {
                EditorGUI.LabelField(rect, assetData.AssetPath);
            }else
            {
                EditorGUI.LabelField(rect, assetData.AssetPath+$"({assetData.RepeatCount})");
            }

            rect.x += rect.width+5;
            rect.width = 70;
            if(GUI.Button(rect,"selected"))
            {
                SelectionUtil.ActiveObject(assetData.AssetPath);
            }
        }
    }
}
