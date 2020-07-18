
using Eternity.FlatBuffer;

namespace Leyoutech.Core.Timeline
{
    public interface IActionFactory
    {
        /// <summary>
        /// 由action 数据 得到具体 Action
        /// </summary>
        /// <param name="actionData">action 数据</param>
        /// <returns></returns>
        AActionItem RetainAction(ActionData actionData);


        /// <summary>
        /// 释放具体Action
        /// </summary>
        /// <param name="actionItem"></param>
        void ReleaseAction(AActionItem actionItem);


        /// <summary>
        /// 清除
        /// </summary>
        void Clear();
    }
}
