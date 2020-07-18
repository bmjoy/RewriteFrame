using Leyoutech.AI.FSM;

namespace Gameplay.Battle.Skill.Tokens
{
    public static class SkillActionToken
    {
        public static readonly ActionToken StageChangedAction = new ActionToken("StageChangedAction");
        public static readonly ActionToken ResumeTimeline = new ActionToken("ResumeTimeline");
        public static readonly ActionToken PausedTimeline = new ActionToken("PausedTimeline");
        public static readonly ActionToken FinalizeAction = new ActionToken("FinalizeAction");
    }

    public static class SkillStateToken
    {
        public static readonly StateToken Begin = new StateToken("Begin");
        public static readonly StateToken BreakBegin = new StateToken("BreakBegin");
        public static readonly StateToken Release = new StateToken("Release");
        public static readonly StateToken BreakRelease = new StateToken("BreakRelease");
        public static readonly StateToken End = new StateToken("End");
        public static readonly StateToken BreakEnd = new StateToken("BreakEnd");
    }
}
