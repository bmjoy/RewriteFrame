using Eternity.Share.Config.ActionDatas;
using Eternity.Share.Config.AoeTargetDamageDatas;
using Eternity.Share.Config.ChangeAccelerateDatas;
using Eternity.Share.Config.FixedEmitNodeDatas;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace EternityEditor.Battle.Timeline
{
    public class ActionTrackDrawer
    {
        private int m_Index;
        private ActionTrack m_ActionTrack;

        private List<ActionItemDrawer> m_ItemDrawers = new List<ActionItemDrawer>();

        private bool m_IsSelected = false;
        public bool IsSelected
        {
            get { return m_IsSelected; }
            set
            {
                if(m_IsSelected!=value)
                {
                    m_IsSelected = value;
                    if (m_IsSelected)
                    {
                        GroupDrawer.OnTrackSelected(this);
                    }
                    else
                    {
                        m_SelectedItemIndex = -1;
                        m_ItemDrawers.ForEach((itemDrawer) =>
                        {
                            itemDrawer.IsSelected = false;
                        });
                    }
                }
            }
        }

        public TrackGroupDrawer GroupDrawer { get; set; }
        private TimelineDrawerConfig m_DrawerConfig = null;

        public ActionTrackDrawer(TimelineDrawerConfig config,TrackGroupDrawer groupDrawer)
        {
            m_DrawerConfig = config;
            GroupDrawer = groupDrawer;
        }

        public void SetData(int index,ActionTrack actionTrack)
        {
            m_Index = index;
            m_ActionTrack = actionTrack;

            m_ItemDrawers.Clear();
            foreach (var d in m_ActionTrack.Actions)
            {
                ActionItemDrawer itemDrawer = new ActionItemDrawer(m_DrawerConfig,this);
                itemDrawer.SetData(d);

                m_ItemDrawers.Add(itemDrawer);
            }
        }

        public void OnGUI(Rect rect)
        {
            foreach(var itemDrawer in m_ItemDrawers)
            {
                itemDrawer.OnGUI(rect);
            }

            int eventBtn = Event.current.button;
            EventType eventType = Event.current.type;
            Vector2 mPos = Event.current.mousePosition;
            bool isContains = rect.Contains(Event.current.mousePosition);

            if (eventBtn == 0 && eventType == EventType.MouseUp&& isContains)
            {
                IsSelected = true;
            }
            if(eventBtn == 1 && eventType == EventType.MouseUp && isContains)
            {
                m_DrawerConfig.ShowMenu((actionData) =>
                {
                    float fireTime = (mPos.x + m_DrawerConfig.ScrollPosX) / m_DrawerConfig.WidthForSecond;
                    actionData.Index = GroupDrawer.GetActionIndex();
                    actionData.FireTime = fireTime;

                    m_ActionTrack.Actions.Add(actionData);
                    ActionItemDrawer itemDrawer = new ActionItemDrawer(m_DrawerConfig, this);
                    itemDrawer.SetData(actionData);

                    m_ItemDrawers.Add(itemDrawer);
                });
            }
        }

        public void OnPropertyGUI(bool isShowDesc =false)
        {
            m_ActionTrack.TrackName = EditorGUILayout.TextField("Track Name", m_ActionTrack.TrackName);

            if(IsSelected && m_SelectedItemIndex>=0)
            {
                m_ItemDrawers[m_SelectedItemIndex].OnPropertyGUI(isShowDesc);
            }
        }

        private int m_SelectedItemIndex = -1;
        internal void OnItemSelected(ActionItemDrawer itemDrawer)
        {
            if(!IsSelected)
            {
                IsSelected = true;
            }
            int newSelectedIndex = m_ItemDrawers.IndexOf(itemDrawer);
            if(newSelectedIndex!=m_SelectedItemIndex)
            {
                if(m_SelectedItemIndex>=0&&m_SelectedItemIndex<m_ItemDrawers.Count)
                {
                    m_ItemDrawers[m_SelectedItemIndex].IsSelected = false;
                }
            }
            m_SelectedItemIndex = newSelectedIndex;
            m_ItemDrawers[m_SelectedItemIndex].IsSelected = true;
        }

        internal void OnItemDelete(ActionItemDrawer itemDrawer)
        {
            m_SelectedItemIndex = -1;
            int index = m_ItemDrawers.IndexOf(itemDrawer);
            m_ItemDrawers.Remove(itemDrawer);
            m_ActionTrack.Actions.RemoveAt(index);
        }

        internal void OnActionCopy(ActionData actionData)
        {
            ActionData data = ActionUtil.CopyFromAction(actionData);
            data.Index = GroupDrawer.GetActionIndex();

            m_ActionTrack.Actions.Add(data);
            ActionItemDrawer itemDrawer = new ActionItemDrawer(m_DrawerConfig, this);
            itemDrawer.SetData(data);

            m_ItemDrawers.Add(itemDrawer);
        }
    }
}
