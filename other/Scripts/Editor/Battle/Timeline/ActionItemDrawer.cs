using Eternity.Share.Config.ActionDatas;
using Eternity.Share.Config.Attributes;
using Eternity.Share.Config.DurationActionDatas;
using Eternity.Share.Config.TimeLine.Datas.Attributes;
using LeyoutechEditor.EGUI.FieldDrawer;
using System;
using UnityEditor;
using UnityEngine;

namespace EternityEditor.Battle.Timeline
{
    public class ActionItemDrawer
    {
        private ActionData m_ActionData;
        private DurationActionData m_DurationActionData;

        public ActionTrackDrawer TrackDrawer { get; private set; }

        private bool m_IsSelected = false;
        public bool IsSelected
        {
            get
            {
                return m_IsSelected;
            }
            set
            {
                if(m_IsSelected != value)
                {
                    m_IsSelected = value;
                    if(m_IsSelected)
                    {
                        TrackDrawer.OnItemSelected(this);
                    }
                }
            }
        }

        private TimelineDrawerConfig m_DrawerConfig = null;
        public ActionItemDrawer(TimelineDrawerConfig config, ActionTrackDrawer trackDrawer)
        {
            m_DrawerConfig = config;
            TrackDrawer = trackDrawer;
        }

        private string m_ActionBriefName = string.Empty;
        public void SetData(ActionData actionData)
        {
            m_ActionData = actionData;
            if (m_ActionData.GetType().IsSubclassOf(typeof(DurationActionData)))
            {
                m_DurationActionData = (DurationActionData)m_ActionData;
            }
            m_DataDrawer = new ObjectDrawer("Action Data", m_ActionData);
            
            m_ActionBriefName = string.Empty;
            var nameAttrs = m_ActionData.GetType().GetCustomAttributes(typeof(ActionBriefName), false);
            if(nameAttrs!=null && nameAttrs.Length>0)
            {
                m_ActionBriefName = ((ActionBriefName)nameAttrs[0]).Name;
            }
        }

        private bool m_IsPressed = false;
        public void OnGUI(Rect rect)
        {
            Rect itemRect = Rect.zero;
            itemRect.x = m_ActionData.FireTime * m_DrawerConfig.WidthForSecond - m_DrawerConfig.ScrollPosX;
            itemRect.y = rect.y;
            itemRect.height = m_DrawerConfig.TrackLineHeight;


            if(m_DurationActionData!=null)
            {
                if(m_DurationActionData.Duration<=0)
                {
                    m_DurationActionData.Duration = m_DrawerConfig.TimeStep;
                }
                itemRect.width = Mathf.Max(m_DrawerConfig.TimeStepWidth, m_DurationActionData.Duration * m_DrawerConfig.WidthForSecond);
            }else
            {
                itemRect.width = m_DrawerConfig.TimeStepWidth;
            }

            GUI.Label(itemRect, m_ActionBriefName, IsSelected ? "flow node 6" : "flow node 5");

            int eventBtn = Event.current.button;
            EventType eventType = Event.current.type;
            bool isContains = itemRect.Contains(Event.current.mousePosition);

            if (eventBtn == 0 && eventType == EventType.MouseDown && isContains)
            {
                m_IsPressed = true;
                IsSelected = true;

                Event.current.Use();
            }else if(eventBtn == 0 && m_IsPressed && eventType == EventType.MouseUp)
            {
                m_IsPressed = false;
                Event.current.Use();
            }else if(eventBtn == 0 && m_IsPressed && IsSelected && eventType == EventType.MouseDrag)
            {
                Vector2 deltaPos = Event.current.delta;
                float deltaTime = deltaPos.x / m_DrawerConfig.WidthForSecond;

                m_ActionData.FireTime += deltaTime;
                if (m_ActionData.FireTime < 0)
                {
                    m_ActionData.FireTime = 0;
                }
                else
                {
                    float endTime = m_ActionData.FireTime;
                    if (m_DurationActionData != null)
                    {
                        endTime += m_DurationActionData.Duration;
                    }
                    if (endTime > m_DrawerConfig.TimeLength)
                    {
                        m_ActionData.FireTime = m_DrawerConfig.TimeLength - (endTime - m_ActionData.FireTime);
                    }
                }

                Event.current.Use();
            }else if(eventBtn == 1 && isContains && eventType == EventType.MouseUp)
            {
                if(!IsSelected)
                {
                    IsSelected = true;
                }

                GenericMenu menu = new GenericMenu();
                menu.AddItem(new GUIContent("Delete"), false, () =>
                {
                    TrackDrawer.OnItemDelete(this);
                });
                menu.AddItem(new GUIContent("Copy"), false, () =>
                {
                    TrackDrawer.OnActionCopy(m_ActionData);
                });
                menu.ShowAsContext();

                Event.current.Use();
            }
        }

        private ObjectDrawer m_DataDrawer = null;
        public void OnPropertyGUI(bool isShowDesc = false)
        {
            Type actionType = m_ActionData.GetType();
            EditorGUILayout.LabelField(actionType.Name);

            var attrs = actionType.GetCustomAttributes(typeof(MemberDesc), false);
            if (attrs != null && attrs.Length > 0)
            {
                EditorGUILayout.LabelField((attrs[0] as MemberDesc).Desc, EditorStyles.wordWrappedLabel);
            }

            m_DataDrawer.OnGUI(isShowDesc);
        }
    }
}
