/*===============================
 * Author: [Allen]
 * Purpose:  环境声源
 * Time: 2019/05/07  17:26
================================*/

namespace AK.Wwise
{
    public class TAkAmbient : AkAmbient
    {
        #region 定义
        /// <summary>
        /// 环境声事件类型
        /// </summary>
        [EnumLabel("环境声事件类型")]
        public enum AmbientEventType
        {
            [EnumLabel("无效")]
            noll,

            /// <summary>
            /// 进入
            /// </summary>
            [EnumLabel("进入")] 
            enter,

            /// <summary>
            /// 离开
            /// </summary>
            [EnumLabel("离开")]
            leave,
        }    
        #endregion


        /// <summary>
        /// 环境声事件类型
        /// </summary>
        [EnumLabel("环境声事件类型")]
        public AmbientEventType m_AmbientEventType = AmbientEventType.noll;

        protected override void Awake()
        {
            if (m_AmbientEventType == AmbientEventType.enter)
                triggerList = new System.Collections.Generic.List<int> { CollisionEnter_TRIGGER_ID, TriggenEnter_TRIGGER_ID };
            else  if (m_AmbientEventType == AmbientEventType.leave)
                triggerList = new System.Collections.Generic.List<int> { CollisionExit_TRIGGER_ID, TriggenExit_TRIGGER_ID, DESTROY_TRIGGER_ID };

            base.Awake();
        }

        /// <summary>
        /// 设置even
        /// </summary>
        /// <param name="eventName"></param>
        public void SetEventName(string eventName)
        {
            if(!string.IsNullOrEmpty(eventName))
            {
                data = new TEvent(eventName);
                data.IsUseName = true;
            }
        }
    }
}

