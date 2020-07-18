using Eternity.Share.Config.ActionDatas;
using Eternity.Share.Config.TimeLine.Datas;
using Eternity.Share.Config.TimeLine.Datas.Attributes;
using LeyoutechEditor.Core.EGUI;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace EternityEditor.Battle.Timeline
{
    public class TrackGroupDrawer
    {
        private static readonly int RULER_HEIGHT = 20;
        private static readonly int OPERATION_WIDTH = 80;
        private static readonly int TITLE_HEIGHT = 30;
        private static readonly int PROPERTY_WIDTH = 500;

        private string m_TitleName = string.Empty;
        private TrackGroup m_GroupData = null;

        private List<ActionTrackDrawer> m_TrackDrawers = new List<ActionTrackDrawer>();

        private GUIStyle m_TitleSytle = null;
        private GUIStyle m_PropertyStyle = null;

        private int m_SelectTrackIndex = -1;
        public int SelectTrackIndex
        {
            get { return m_SelectTrackIndex; }
            set 
            { 
                if(m_SelectTrackIndex!=value)
                {
                    if(m_SelectTrackIndex>=0 && m_SelectTrackIndex<m_TrackDrawers.Count)
                    {
                        m_TrackDrawers[m_SelectTrackIndex].IsSelected = false;
                    }
                    m_SelectTrackIndex = value;
                    if(m_SelectTrackIndex<0 || m_SelectTrackIndex>=m_TrackDrawers.Count)
                    {
                        m_SelectTrackIndex = -1;
                    }else
                    {
                        m_TrackDrawers[m_SelectTrackIndex].IsSelected = true;
                    }
                }
            }
        }

        public TimelineDrawerConfig DrawerConfig { get; set; } = new TimelineDrawerConfig();

        private Action m_RepaintAction = null;
        private Func<int> m_GetActionIndex = null;
        private Func<bool> m_IsShowDesc = null;
        public TrackGroupDrawer(
            Func<int> getActionIndex,
            Action repaintAction, 
            ActionCategory category,
            Func<bool> isShowDesc)
        {
            m_GetActionIndex = getActionIndex;
            m_RepaintAction = repaintAction;
            DrawerConfig.Category = category;
            m_IsShowDesc = isShowDesc;
        }

        public void SetData(string titleName,TrackGroup groupData)
        {
            m_TitleName = titleName;

            m_GroupData = groupData;
            DrawerConfig.TimeLength = m_GroupData.Length;

            for(int i = 0;i<m_GroupData.Tracks.Count;i++)
            {
                ActionTrackDrawer trackDrawer = new ActionTrackDrawer(DrawerConfig,this);
                
                trackDrawer.SetData(i, m_GroupData.Tracks[i]);

                m_TrackDrawers.Add(trackDrawer);
            }
        }

        public void OnGUI(Rect rect)
        {
            if(m_TitleSytle == null)
            {
                m_TitleSytle = new GUIStyle(EditorStyles.boldLabel);
                m_TitleSytle.alignment = TextAnchor.MiddleCenter;
                m_TitleSytle.fontSize = 20;
            }
            if(m_PropertyStyle == null)
            {
                m_PropertyStyle = new GUIStyle(EditorStyles.boldLabel);
                m_PropertyStyle.alignment = TextAnchor.MiddleCenter;
            }

            float deltaHeight = 0.0f;

            Rect titileRect = new Rect(rect.x, rect.y, rect.width, TITLE_HEIGHT);
            EditorGUI.LabelField(titileRect, m_TitleName, m_TitleSytle);
            
            deltaHeight += TITLE_HEIGHT;

            Rect propertyRect = new Rect(rect.x + rect.width-PROPERTY_WIDTH, rect.y + deltaHeight, PROPERTY_WIDTH, rect.height - deltaHeight);
            EditorGUIUtil.DrawAreaLine(propertyRect, Color.grey);
            OnPropertyGUI(propertyRect);

            Rect operationRect = new Rect(rect.x, rect.y + deltaHeight, OPERATION_WIDTH, RULER_HEIGHT);
            EditorGUIUtil.DrawAreaLine(operationRect, Color.grey);
            DrawTrackOperation(operationRect);

            Rect rulerRect = new Rect(rect.x+OPERATION_WIDTH+1, rect.y + deltaHeight, rect.width - PROPERTY_WIDTH-OPERATION_WIDTH, RULER_HEIGHT);
            EditorGUIUtil.DrawAreaLine(rulerRect, Color.grey);
            DrawRuler(rulerRect);

            deltaHeight += RULER_HEIGHT;

            Rect trackIndexRect = new Rect(rect.x, rect.y + deltaHeight, OPERATION_WIDTH, rect.height - deltaHeight);
            EditorGUIUtil.DrawAreaLine(trackIndexRect, Color.grey);
            DrawTrackIndex(trackIndexRect);

            Rect trackRect = new Rect(rect.x+OPERATION_WIDTH, rect.y + deltaHeight, rect.width - PROPERTY_WIDTH-OPERATION_WIDTH, rect.height - deltaHeight);
            EditorGUIUtil.DrawAreaLine(trackRect, Color.blue);

            DrawTimeGrid(trackRect);
            DrawTrack(trackRect);
            DrawScroll(trackRect);

        }

        private void DrawTrackOperation(Rect rect)
        {
            using (new GUILayout.AreaScope(rect))
            {
                using (new GUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("+", "ButtonLeft"))
                    {
                        ActionTrack actionTrack = new ActionTrack();
                        m_GroupData.Tracks.Add(actionTrack);

                        ActionTrackDrawer trackDrawer = new ActionTrackDrawer(DrawerConfig,this);
                        trackDrawer.SetData(m_GroupData.Tracks.Count-1, actionTrack);
                        m_TrackDrawers.Add(trackDrawer);

                        SelectTrackIndex = m_TrackDrawers.Count - 1;
                    }
                    using (new EditorGUI.DisabledGroupScope(false))
                    {
                        if (GUILayout.Button("-", "ButtonRight"))
                        {
                            if(SelectTrackIndex>=0)
                            {
                                m_GroupData.Tracks.RemoveAt(SelectTrackIndex);
                                m_TrackDrawers.RemoveAt(SelectTrackIndex);
                            }
                            SelectTrackIndex--;
                        }
                    }
                    //using (new EditorGUI.DisabledGroupScope(false))
                    //{
                    //    if (GUILayout.Button("\u2191", "ButtonMid"))//move up
                    //    {
                    //    }
                    //}
                    //using (new EditorGUI.DisabledGroupScope(false))
                    //{
                    //    if (GUILayout.Button("\u2193", "ButtonRight"))//move down
                    //    {
                    //    }
                    //}
                }
            }
        }

        private void DrawRuler(Rect rect)
        {
            using (new GUI.ClipScope(new Rect(rect.x, rect.y, rect.width, rect.height)))
            {
                int start = Mathf.FloorToInt(DrawerConfig.ScrollPosX / DrawerConfig.WidthForSecond);
                int end = Mathf.CeilToInt((DrawerConfig.ScrollPosX + rect.width) / DrawerConfig.WidthForSecond);

                int startCount = Mathf.FloorToInt(start / DrawerConfig.TimeStep);
                int endCount = Mathf.FloorToInt(end / DrawerConfig.TimeStep);
                for(int i = startCount; i<= endCount; i++)
                {
                    var x = i* DrawerConfig.TimeStepWidth - DrawerConfig.ScrollPosX;

                    if (i % 10 == 0)
                    {
                        Handles.color = new Color(0, 0, 0, 0.8f);
                        Handles.DrawLine(new Vector3(x, 0, 0), new Vector3(x, rect.height * 0.8f, 0));
                        GUI.Label(new Rect(x, 5, 40, 40), (i * DrawerConfig.TimeStep).ToString("F1"));
                    }
                    else if (i % 5 == 0)
                    {
                        Handles.color = new Color(0, 0, 0, 0.5f);
                        Handles.DrawLine(new Vector3(x, 0, 0), new Vector3(x, rect.height * 0.5f, 0));
                    }
                    else
                    {
                        Handles.color = new Color(0, 0, 0, 0.5f);
                        Handles.DrawLine(new Vector3(x, 0, 0), new Vector3(x, rect.height * 0.3f, 0));
                    }
                }
            }
        }

        private void DrawTrackIndex(Rect rect)
        {
            using (new GUI.ClipScope(new Rect(rect.x, rect.y, rect.width, rect.height)))
            {
                int start = Mathf.FloorToInt(DrawerConfig.ScrollPosY / DrawerConfig.TrackLineHeight);
                int end = Mathf.CeilToInt((DrawerConfig.ScrollPosY + rect.height) / DrawerConfig.TrackLineHeight);

                for(int i =start;i<end;++i)
                {
                    float y = DrawerConfig.TrackLineHeight * i - DrawerConfig.GroupScrollPos.y;

                    if(i>=m_GroupData.Tracks.Count)
                    {
                        break;
                    }

                    Rect indexRect = new Rect(0, y, OPERATION_WIDTH, DrawerConfig.TrackLineHeight);
                    string trackName = m_GroupData.Tracks[i].TrackName??"";
                    GUI.Label(indexRect,$"{trackName} ({i.ToString()})", SelectTrackIndex == i ? "flow node 1" : "flow node 0");
                    if (indexRect.Contains(Event.current.mousePosition) && Event.current.type == EventType.MouseUp && Event.current.button == 0)
                    {
                        SelectTrackIndex = i;
                        Event.current.Use();
                    }
                }
            }
        }

        private void DrawTimeGrid(Rect rect)
        {
            using (new GUI.ClipScope(new Rect(rect.x, rect.y, rect.width, rect.height)))
            {
                int startX = Mathf.FloorToInt(DrawerConfig.ScrollPosX / DrawerConfig.WidthForSecond);
                int endX = Mathf.CeilToInt((DrawerConfig.ScrollPosX + rect.width) / DrawerConfig.WidthForSecond);

                int startXCount = Mathf.FloorToInt(startX / DrawerConfig.TimeStep);
                int endXCount = Mathf.FloorToInt(endX / DrawerConfig.TimeStep);
                for (int i = startXCount; i <= endXCount; i++)
                {
                    var x = i * DrawerConfig.TimeStepWidth - DrawerConfig.ScrollPosX;

                    Color handlesColor = new Color(0, 0, 0, 0.4f);
                    if(i%10 == 0)
                    {
                        handlesColor = new Color(0, 0, 0, 0.9f);
                    }
                    else if(i%5 == 0)
                    {
                        handlesColor = new Color(0, 0, 0, 0.6f);
                    }
                    Handles.color = handlesColor;
                    Handles.DrawLine(new Vector3(x, 0, 0), new Vector3(x, rect.height, 0));
                }

                float stopLineX = m_GroupData.Length * DrawerConfig.WidthForSecond - DrawerConfig.ScrollPosX;
                Handles.color = Color.red;
                Handles.DrawLine(new Vector3(stopLineX, 0, 0), new Vector3(stopLineX, rect.height, 0));


                int startY = Mathf.FloorToInt(DrawerConfig.ScrollPosY / DrawerConfig.TrackLineHeight);
                int endY = Mathf.CeilToInt((DrawerConfig.ScrollPosY + rect.height) / DrawerConfig.TrackLineHeight);
                for (int i = startY; i <= endY; i++)
                {
                    float y = DrawerConfig.TrackLineHeight * i - DrawerConfig.ScrollPosY;
                    Handles.color = new Color(0, 0, 0, 0.9f);
                    Handles.DrawLine(new Vector3(0, y, 0), new Vector3(rect.width, y, 0));
                }
            }
        }

        private void DrawTrack(Rect rect)
        {
            using (new GUI.ClipScope(new Rect(rect.x, rect.y, rect.width, rect.height)))
            {
                int startY = Mathf.FloorToInt(DrawerConfig.ScrollPosY / DrawerConfig.TrackLineHeight);
                int endY = Mathf.CeilToInt((DrawerConfig.ScrollPosY + rect.height) / DrawerConfig.TrackLineHeight);

                float maxWidth = m_GroupData.Length * DrawerConfig.WidthForSecond;

                for (int i = startY; i < endY; ++i)
                {
                    float y = DrawerConfig.TrackLineHeight * i - DrawerConfig.ScrollPosY;

                    if(i>=m_TrackDrawers.Count)
                    {
                        break;
                    }

                    Rect trackRect = new Rect(0, y, rect.width, DrawerConfig.TrackLineHeight);
                    trackRect.width = Mathf.Min(rect.width, maxWidth - DrawerConfig.ScrollPosX);
                    if(SelectTrackIndex == i)
                    {
                        EditorGUIUtil.DrawAreaLine(trackRect, Color.green);
                    }

                    m_TrackDrawers[i].OnGUI(trackRect);
                }
            }
        }

        private void DrawScroll(Rect rect)
        {
            using (new GUILayout.AreaScope(rect))
            {
                using (var scop = new UnityEditor.EditorGUILayout.ScrollViewScope(DrawerConfig.GroupScrollPos))
                {
                    float scrollWith = Mathf.Max(m_GroupData.Length * DrawerConfig.WidthForSecond, rect.width);
                    float scrollHeight = Mathf.Max(m_GroupData.Tracks.Count * DrawerConfig.TrackLineHeight, rect.height);

                    GUILayout.Label("", GUILayout.Width(scrollWith), GUILayout.Height(scrollHeight - 20));

                    DrawerConfig.GroupScrollPos = scop.scrollPosition;
                }
            }
        }

        private Vector2 m_PropertyScrollPos = Vector2.zero;
        private void OnPropertyGUI(Rect rect)
        {
            GUILayout.BeginArea(rect);
            {
                EditorGUILayout.LabelField("Property", m_PropertyStyle);
                m_PropertyScrollPos = EditorGUILayout.BeginScrollView(m_PropertyScrollPos,GUILayout.Width(rect.width -17));
                {
                    EditorGUIUtil.BeginLabelWidth(100);
                    {
                        float timeLen = EditorGUILayout.FloatField("Time Length", m_GroupData.Length);
                        if(timeLen != m_GroupData.Length)
                        {
                            m_GroupData.Length = timeLen;
                            DrawerConfig.TimeLength = timeLen;
                        }

                        EditorGUILayout.Space();

                        if (SelectTrackIndex >= 0 && SelectTrackIndex < m_TrackDrawers.Count)
                        {
                            m_TrackDrawers[SelectTrackIndex].OnPropertyGUI(m_IsShowDesc());
                        }
                    }
                    EditorGUIUtil.EndLableWidth();
                }
                EditorGUILayout.EndScrollView();
            }
            GUILayout.EndArea();
        }

        internal void OnTrackSelected(ActionTrackDrawer trackDrawer)
        {
            SelectTrackIndex = m_TrackDrawers.IndexOf(trackDrawer);

            OnRepaint();
        }

        internal void OnRepaint()
        {
            m_RepaintAction?.Invoke();
        }

        internal int GetActionIndex()
        {
            return m_GetActionIndex!=null?m_GetActionIndex():-1;
        }
    }

}
