using Eternity.FlatBuffer;
using Gameplay.Battle.Skill.Tokens;

namespace Gameplay.Battle.Skill.States
{
    public class SkillBreakBeginState : SkillStateBase
    {
        public SkillBreakBeginState() : base(SkillStateToken.BreakBegin)
        {
        }

        protected override TrackGroup GetTrackGroup(out float timeScale)
        {
            timeScale = 1.0f;
            SkillData skillData = Context.GetObject<SkillData>();
            return skillData.BeginStageData.Value.BreakGroup.Value;
        }

        protected override void OnTrackGroupComplete()
        {
            Machine.PerformAction(SkillActionToken.FinalizeAction);
        }
    }
}
