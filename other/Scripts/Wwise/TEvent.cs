/*===============================
 * Author: [Allen]
 * Purpose: 事件
 * Time: 2019/05/07  17:30
================================*/
namespace AK.Wwise
{
    public class TEvent : Event
    {
        /// <summary>
        /// 事件名称
        /// </summary>
        private string TName;


        public TEvent(string name)
        {
            TName = name;
        }

        /// <summary>
        /// 是否可用
        /// </summary>
        /// <returns></returns>
        public override bool IsValid()
        {
            return true;
        }

        /// <summary>
        /// 事件名称
        /// </summary>
        public override string Name
        {
            get
            {
                if (string.IsNullOrEmpty(TName))
                    return base.Name;

                return TName;
            }
        }
    }
}

