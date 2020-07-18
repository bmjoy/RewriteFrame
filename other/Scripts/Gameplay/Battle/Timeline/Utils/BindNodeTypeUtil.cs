using Eternity.FlatBuffer.Enums;

namespace Gameplay.Battle.Timeline.Utils
{
    public static class BindNodeTypeUtil
    {
        public static SkillLaunchPoint ToLaunchPoint(this BindNodeType nodeType)
        {
            return (SkillLaunchPoint)((int)nodeType);
        }
    }
}
