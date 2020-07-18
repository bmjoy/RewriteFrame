using Eternity.FlatBuffer;
using Gameplay.Battle.Skill.Tokens;
using Leyoutech.AI.FSM;
using Leyoutech.Core.Context;
using UnityEngine;

namespace Gameplay.Battle.Skill.States
{
    public class SkillEndState : SkillStateBase
    {
        public SkillEndState() : base(SkillStateToken.End)
        {
        }

        protected override void OnStageChanged(ActionToken action, object data)
        {
            SkillStageType stageType = (SkillStageType)data;
            if (stageType == SkillStageType.BreakEnd)
            {
                ChangeState(SkillStateToken.BreakEnd);
            }
            else
            {
                Debug.LogError($"SkillEndState::OnStageChange->Stage type is {stageType}");
            }
        }

        protected override TrackGroup GetTrackGroup(out float timeScale)
        {
            SkillData skillData = Context.GetObject<SkillData>();
            SkillEndStageData stageData = skillData.EndStageData.Value;
            if(stageData.IsScaleByTime)
            {
                SkillRunningData runningData = Context.GetObject<SkillRunningData>();
                timeScale = runningData.TimeScaleRate;
            }
            else
            {
                timeScale = 1.0f;
            }
            return stageData.Group.Value;
        }

        protected override void OnTrackGroupComplete()
        {
            Machine.PerformAction(SkillActionToken.FinalizeAction);
        }
    }
}
