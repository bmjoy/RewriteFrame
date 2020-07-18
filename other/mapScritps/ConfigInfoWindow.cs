#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Map
{
    /// <summary>
    /// 配置信息显示窗口
    /// </summary>
    [ExecuteInEditMode]
    public class ConfigInfoWindow : EditorWindow
    {
        private Func<int> m_OnInitCallBack;
        private Func<int,string> m_OnRefreshCallBack;
        private Action<int> m_OnConfirmCallBack;

        private int m_ConfigCount;
        public static void OpenWindow(Func<int> onInit, Func<int, string> onRefresh, Action<int> onConfirm)
        {
            ConfigInfoWindow win =  GetWindow<ConfigInfoWindow>();
            win.position = new Rect(600, 300, 400, 400);
            win.maxSize = new Vector2(300, 500);
            win.Show();
            win.m_OnInitCallBack = onInit;
            win.m_OnRefreshCallBack = onRefresh;
            win.m_OnConfirmCallBack = onConfirm;
            win.Init();
        }

        private Vector2 m_CreatureScrollPos;
        //每一页就显示30个
        private const int PAGE_MAX_COUNT = 30;
        private int m_CurPage = 1;
        public void Init()
        {
            if(m_OnInitCallBack != null)
            {
                m_ConfigCount = m_OnInitCallBack();
            }
        }

        private void OnGUI()
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Id", GUILayout.Width(100)))
            {

            }
            GUILayout.EndHorizontal();
            if (m_ConfigCount > 0)
            {
                GUILayout.BeginVertical();
                m_CreatureScrollPos = GUILayout.BeginScrollView(m_CreatureScrollPos);
                int minCount = (m_CurPage - 1) * PAGE_MAX_COUNT;
                int maxCount = m_CurPage * PAGE_MAX_COUNT;
                int lastCell = m_ConfigCount % PAGE_MAX_COUNT;
                int totalPage = m_ConfigCount / PAGE_MAX_COUNT;
                if (lastCell > 0)
                {
                    totalPage++;
                }

                for (int iNpc = 0; iNpc < m_ConfigCount; iNpc++)
                {
                    if (iNpc < minCount || iNpc > maxCount)
                    {
                        continue;
                    }
                    GUILayout.BeginHorizontal("box");
                    GUI.skin.button.alignment = TextAnchor.MiddleLeft;
                    if (GUILayout.Button(m_OnRefreshCallBack(iNpc), GUILayout.Width(200)))
                    {

                    }

                    GUI.skin.button.alignment = TextAnchor.MiddleCenter;
                    GUILayout.Space(10);

                    if (GUILayout.Button("选中",GUILayout.Width(50)))
                    {
                        if(m_OnConfirmCallBack != null)
                        {
                            m_OnConfirmCallBack(iNpc);
                        }
                    }

                    GUILayout.EndHorizontal();
                }
                GUILayout.EndScrollView();
                GUILayout.EndVertical();
                GUILayout.BeginHorizontal();
                if (m_CurPage > 1)
                {
                    if (GUILayout.Button("上一页"))
                    {
                        m_CurPage--;
                    }
                }
                GUILayout.Label("当前页:" + m_CurPage);
                if (GUILayout.Button("刷新"))
                {
                    Init();
                }
                if (m_CurPage < totalPage)
                {
                    if (GUILayout.Button("下一页"))
                    {
                        m_CurPage++;
                    }
                }
                GUILayout.EndHorizontal();
            }
        }

        private void OnEnable()
        {

        }
    }

}
#endif