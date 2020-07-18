using Eternity.FlatBuffer;
using Gameplay.Battle.Skill.Tokens;

namespace Gameplay.Battle.Skill.States
{
    public class SkillBreakReleaseState : SkillStateBase
    {
        public SkillBreakReleaseState() : base(SkillStateToken.BreakRelease)
        {
        }

        protected override TrackGroup GetTrackGroup(out float timeScale)
        {
            timeScale = 1.0f;
            SkillData skillData = Context.GetObject<SkillData>();
            SkillRunningData runningData = Context.GetObject<SkillRunningData>();
            int stageIndex = runningData.ReleaseStageIndex;

            return skillData.ReleaseStageData.Value.Childs(stageIndex).Value.BreakGroup.Value;
        }

        protected override void OnTrackGroupComplete()
        {
            Machine.PerformAction(SkillActionToken.FinalizeAction);
        }
    }
}
