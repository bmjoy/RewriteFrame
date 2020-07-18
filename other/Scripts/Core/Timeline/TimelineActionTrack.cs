using Assets.Scripts.Core.Timeline;
using Eternity.FlatBuffer;
using Eternity.FlatBuffer.Enums;
using Leyoutech.Core.Context;
using Leyoutech.Utility;
using System.Collections.Generic;

namespace Leyoutech.Core.Timeline
{
    /// <summary>
    /// 单轨道Action时间轴
    /// </summary>
    public class TimelineActionTrack : ATimelineEnv
    {
        private const string LOG_TAG = "TimelineActionTrack";

        /// <summary>
        /// 包含的所有待触发的action
        /// </summary>
        private List<AActionItem> m_ActionItems = new List<AActionItem>(); 

        /// <summary>
        /// 持续型已触发action 暂存容器
        /// </summary>
        private List<ADurationActionItem> m_RunningActionItems = new List<ADurationActionItem>();

        /// <summary>
        /// 这个轨道运行了多久
        /// </summary>
        private float m_ElapsedTime = 0.0f;


        /// <summary>
        /// action 工厂
        /// </summary>
        private IActionFactory m_ActionFactroy = null;


        /// <summary>
        /// 构造函数
        /// 根据轨道 action 数据，构建包含的所有action存入容器
        /// </summary>
        /// <param name="context"></param>
        /// <param name="actionTrack"></param>
        public TimelineActionTrack(IContext context,ActionTrack actionTrack,float timeScale)
        {
            SetEnv(context);

            m_ActionFactroy = context.GetObject<IActionFactory>();

            for(int i =0;i<actionTrack.ActionsLength;i++)
            {
                ActionData actionData = actionTrack.Actions(i).Value;
                if(actionData.Platform == ActionPlatform.All || actionData.Platform == ActionPlatform.Client)
                {
                    AActionItem actionItem = m_ActionFactroy.RetainAction(actionData);
                    if(actionItem == null)
                    {
                        DebugUtility.LogError(LOG_TAG, "TimelineActionTrack::Init->item is null");
                        continue;
                    }
                    actionItem.SetEnv(context, actionData,timeScale);
                    m_ActionItems.Add(actionItem);
                }
            }
        }

        /// <summary>
        /// 循环
        /// </summary>
        /// <param name="deltaTime"></param>
        public override void DoUpdate(float deltaTime)
        {
            if(m_ActionItems.Count == 0 && m_RunningActionItems.Count == 0)
            {
                return;
            }

            m_ElapsedTime += deltaTime; //这个轨道运行了多久

            //循环所有action,触发时间到达，分组触发，
            //持续类型进入m_RunningActionItems 并触发DoEnter
            //一次类型触发Trigger
            while(m_ActionItems.Count>0)
            {
                var item = m_ActionItems[0];
                if (item.FireTime <= m_ElapsedTime)
                {
                    if (item is ADurationActionItem durationActionItem)
                    {
                        durationActionItem.DoEnter();
                        m_RunningActionItems.Add(durationActionItem);
                    }
                    else if (item is AEventActionItem eventActionItem)
                    {
                        eventActionItem.Trigger();
                        m_ActionFactroy.ReleaseAction(eventActionItem);
                    }
                    m_ActionItems.RemoveAt(0);
                }
                else
                {
                    break;
                }
            }
            
            //持续一段时间的action,触发updata,并且到达结束移除
            for (int i = 0; i < m_RunningActionItems.Count;)
            {
                ADurationActionItem item = m_RunningActionItems[i];
                item.DoUpdate(deltaTime);
                if (item.EndTime <= m_ElapsedTime)
                {
                    item.DoExit();
                    m_RunningActionItems.RemoveAt(i);

                    m_ActionFactroy.ReleaseAction(item);
                }
                else
                {
                    ++i;
                }
            }
        }
        
        public override void DoDestroy()
        {
            for(int i = m_RunningActionItems.Count-1;i>=0;--i)
            {
                var item = m_RunningActionItems[i];
                item.DoExit();
                m_ActionFactroy.ReleaseAction(item);
            }
            m_RunningActionItems.Clear();
            for(int i = m_ActionItems.Count-1;i>=0;--i)
            {
                var item = m_ActionItems[i];
                m_ActionFactroy.ReleaseAction(item);
            }
            m_ActionItems.Clear();

            m_ElapsedTime = 0.0f;
            m_ActionFactroy = null;

            base.DoDestroy();
        }

        public void DoPause()
        {
            foreach(var item in m_RunningActionItems)
            {
                item.DoPause();
            }
        }

        public void DoResume()
        {
            foreach (var item in m_RunningActionItems)
            {
                item.DoResume();
            }
        }
        
    }
}
