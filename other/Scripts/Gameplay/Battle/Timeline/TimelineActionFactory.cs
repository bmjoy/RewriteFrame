using Eternity.FlatBuffer;
using Eternity.FlatBuffer.Enums;
using Leyoutech.Core.Timeline;
using System.Collections.Generic;

namespace Gameplay.Battle.Timeline
{
    public class TimelineActionFactory : IActionFactory
    {
        private Dictionary<ActionID, IActionPool> m_ActionPoolDic = new Dictionary<ActionID, IActionPool>();

        private static TimelineActionFactory sm_Factory = null;
        public static TimelineActionFactory Factory
        {
            get
            {
                if(sm_Factory == null)
                {
                    sm_Factory = new TimelineActionFactory();
                }
                return sm_Factory;
            }
        }

        public void ReleaseAction(AActionItem actionItem)
        {
            ActionData actionData = actionItem.GetData();
            if (m_ActionPoolDic.TryGetValue(actionData.Id, out IActionPool pool))
            {
                pool.ReleaseAction(actionItem);
            }
        }

        public AActionItem RetainAction(ActionData actionData)
        {
            if(m_ActionPoolDic.TryGetValue(actionData.Id,out IActionPool pool))
            {
                return pool.GetAction();
            }
            else
            {
                pool = ActionPool.Creater(actionData.Id);
                if(pool!=null)
                {
                    m_ActionPoolDic.Add(actionData.Id, pool);
                    return pool.GetAction();
                }
            }

            return null;
        }

        /// <summary>
        /// 清除
        /// </summary>
        public void Clear()
        {
            foreach (var poolitem in m_ActionPoolDic)
            {
                poolitem.Value.Clear();
            }
            m_ActionPoolDic.Clear();
        }
    }
}
