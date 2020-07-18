using Leyoutech.Core.Loader.Config;
using LeyoutechEditor.Core.EGUI;
using LeyoutechEditor.Core.Window;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace LeyoutechEditor.Core.Packer
{
    /// <summary>
    /// 资源地址重复弹板提示框
    /// </summary>
    public class AssetAddressRepeatPopupWindow : DotPopupWindow
    {
        private static int WIN_WIDTH = 600;
        private static int WIN_Height = 300;

        private Vector2 m_ScrollPos = Vector2.zero;
        private List<AssetAddressData> m_RepeatAddressList;

        public static AssetAddressRepeatPopupWindow GetWindow()
        {
            return GetPopupWindow<AssetAddressRepeatPopupWindow>();
        }

        public void ShowWithParam(List<AssetAddressData> list,Vector2 position)
        {
            m_RepeatAddressList = list;
            Show<AssetAddressRepeatPopupWindow>(new Rect(position+new Vector2(10,20), new Vector2(WIN_WIDTH, WIN_Height)), true, true);
        }

        protected override void OnGUI()
        {
            base.OnGUI();

            GUIStyle boldCenterStyle = new GUIStyle(EditorStyles.label);
            boldCenterStyle.alignment = TextAnchor.MiddleCenter;
            boldCenterStyle.fontStyle = FontStyle.Bold;
            string address = "";
            if(m_RepeatAddressList.Count>0)
            {
                address = m_RepeatAddressList[0].AssetAddress;
            }
            EditorGUILayout.LabelField($"Repeat Address({address})", boldCenterStyle);

            m_ScrollPos = EditorGUILayout.BeginScrollView(m_ScrollPos,EditorStyles.helpBox);
            {
                int index = 0;
                foreach(var data in m_RepeatAddressList)//绘制重复的项
                {
                    EditorGUILayout.BeginHorizontal();
                    {
                        EditorGUILayout.LabelField("" + index, GUILayout.Width(20));
                        EditorGUIUtil.BeginLabelWidth(60);
                        {
                            EditorGUILayout.TextField("assetPath",data.AssetPath);
                        }
                        EditorGUIUtil.EndLableWidth();
                        UnityObject uObj = AssetDatabase.LoadAssetAtPath<UnityObject>(data.AssetPath);
                        EditorGUILayout.ObjectField(uObj, typeof(UnityObject), true,GUILayout.Width(120));
                    }
                    EditorGUILayout.EndHorizontal();

                    ++index;
                }
            }
            EditorGUILayout.EndScrollView();
        }
    }
}
