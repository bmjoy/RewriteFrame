#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Map
{
    /// <summary>
    /// 添加寻宝信息
    /// </summary>
    [ExecuteInEditMode]
    public class TreasureInfoWindow : EditorWindow
    {
        private static TreasureInfoWindow sm_TreasureWin;

        public static void OpenWindow(TreasureRoot root)
        {
            sm_TreasureWin = GetWindow<TreasureInfoWindow>();
            sm_TreasureWin.position = new Rect(600, 300, 400, 400);
            sm_TreasureWin.minSize = new Vector2(500, 500);
            sm_TreasureWin.Show();
            sm_TreasureWin.Init(root);
        }

        private Vector2 m_TreasureScrollPos;
        private TreasureRoot m_Root;
        private List<SemaphoreMark> m_Marks = null;
        private TreasureRootMark m_RootMark;
        //每一页就显示30个
        private const int PAGE_MAX_COUNT = 30;
        private int m_CurPage = 1;
        public void Init(TreasureRoot root)
        {
            if(root == null)
            {
                return;
            }

            m_Root = root;

            GamingMapArea gamingArea = root.m_GamingMapArea;

            if (gamingArea != null)
            {
                Area[] areas = FindObjectsOfType<Area>();
                if (areas != null && areas.Length > 0)
                {
                    for (int iArea = 0; iArea < areas.Length; iArea++)
                    {
                        Area area = areas[iArea];
                        if (area.Uid == gamingArea.m_AreaId)
                        {
                            m_Marks = area.GetSemaphoreMarks();
                            m_RootMark = area.m_TreasureRoot;
                        }
                    }
                }
            }
        }

        private void OnGUI()
        {
            GUILayout.BeginHorizontal();
            if(GUILayout.Button("Name",GUILayout.Width(200)))
            {

            }
            if (GUILayout.Button("GroupId", GUILayout.Width(100)))
            {

            }
            if (GUILayout.Button("NpcId", GUILayout.Width(100)))
            {

            }
            GUILayout.EndHorizontal();
            if(m_Marks != null && m_Marks.Count>0)
            {
                GUILayout.BeginVertical();
                m_TreasureScrollPos = GUILayout.BeginScrollView(m_TreasureScrollPos);
                int minCount = (m_CurPage - 1) * PAGE_MAX_COUNT;
                int maxCount = m_CurPage * PAGE_MAX_COUNT;
                int lastCell = m_Marks.Count % PAGE_MAX_COUNT;
                int totalPage = m_Marks.Count / PAGE_MAX_COUNT;
                if(lastCell>0)
                {
                    totalPage++;
                }

                for(int iMark = 0;iMark< m_Marks.Count; iMark++)
                {
                    if(iMark<minCount || iMark>maxCount)
                    {
                        continue;
                    }
                    SemaphoreMark markInfo = m_Marks[iMark];
                    GUILayout.BeginHorizontal("box");
                    GUI.skin.button.alignment = TextAnchor.MiddleLeft;
                    if(GUILayout.Button(markInfo.name,GUILayout.Width(200)))
                    {
                        Selection.activeObject = markInfo.gameObject;
                    }

                    GUI.skin.button.alignment = TextAnchor.MiddleCenter;
                    GUILayout.Space(10);

                    GUILayout.Label(markInfo.m_GroupId.ToString(), GUILayout.MaxWidth(100));
                    GUILayout.Label(markInfo.m_NpcId.ToString(), GUILayout.MaxWidth(100));
                    
                    if(m_Root.Exists(markInfo))
                    {
                        if(GUILayout.Button("移除"))
                        {
                            m_Root.Remove(markInfo);
                        }
                    }
                    else
                    {
                        if(GUILayout.Button("添加"))
                        {
                            m_Root.Add(markInfo);
                        }
                    }
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndScrollView();
                GUILayout.EndVertical();
                GUILayout.BeginHorizontal();
                if(m_CurPage>1)
                {
                    if(GUILayout.Button("上一页"))
                    {
                        m_CurPage--;
                    }
                }
                GUILayout.Label("当前页:"+m_CurPage);
                if(GUILayout.Button("刷新"))
                {
                    Init(m_Root);
                }
                if(m_CurPage<totalPage)
                {
                    if(GUILayout.Button("下一页"))
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
