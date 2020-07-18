using Leyoutech.Core.Timeline;

namespace Gameplay.Battle.Timeline
{
    public interface IActionPool
    {
        AActionItem GetAction();
        void ReleaseAction(AActionItem actionItem);
        void Clear();
    }
}
