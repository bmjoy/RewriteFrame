using Eternity.FlatBuffer;
using Gameplay.Battle.Skill.Tokens;
using Leyoutech.AI.FSM;
using Leyoutech.Core.Context;
using UnityEngine;

namespace Gameplay.Battle.Skill.States
{
    public class SkillBeginState : SkillStateBase
    {
        public SkillBeginState() : base(SkillStateToken.Begin)
        {
        }

        protected override void OnStageChanged(ActionToken action, object data)
        {
            SkillStageType stageType = (SkillStageType)data;
            if(stageType == SkillStageType.BreakBegin)
            {
                ChangeState(SkillStateToken.BreakBegin);
            }else if(stageType == SkillStageType.Release)
            {
                ChangeState(SkillStateToken.Release);
            }else if(stageType == SkillStageType.End)
            {
                ChangeState(SkillStateToken.End);
            }else
            {
                Debug.LogError($"SkillBeginState::OnStageChange->Stage type is {stageType}");
            }
        }

        protected override TrackGroup GetTrackGroup(out float timeScale)
        {
            SkillData skillData = Context.GetObject<SkillData>();
            SkillBeginStageData stageData = skillData.BeginStageData.Value;
            if(stageData.IsScaleByTime)
            {
                SkillRunningData runningData = Context.GetObject<SkillRunningData>();
                timeScale = runningData.TimeScaleRate;
            }else
            {
                timeScale = 1.0f;
            }

            return stageData.Group.Value;
        }

        protected override void OnTrackGroupComplete()
        {
            SetStageType(SkillStageType.Release);
        }
    }
}
