using Eternity.FlatBuffer;
using Gameplay.Battle.Skill.Tokens;
using Leyoutech.AI.FSM;
using Leyoutech.Core.Context;
using System;
using UnityEngine;

namespace Gameplay.Battle.Skill.States
{
    public class SkillReleaseState : SkillStateBase
    {
        public SkillReleaseState() : base(SkillStateToken.Release)
        {
        }

        protected override void OnStageChanged(ActionToken action, object data)
        {
            SkillStageType stageType = (SkillStageType)data;
            if (stageType == SkillStageType.BreakRelease)
            {
                ChangeState(SkillStateToken.BreakRelease);
            }
            else if (stageType == SkillStageType.End)
            {
                ChangeState(SkillStateToken.End);
            }
            else
            {
                Debug.LogError($"SkillReleaseState::OnStageChange->Stage type is {stageType}");
            }
        }

        protected override TrackGroup GetTrackGroup(out float timeScale)
        {
            SkillRunningData runningData = Context.GetObject<SkillRunningData>();
            SkillData skillData = Context.GetObject<SkillData>();

            int stageIndex = runningData.ReleaseStageIndex;

            SkillReleaseStageData stageData = skillData.ReleaseStageData.Value;
            if (stageData.IsScaleByTime)
            {
                timeScale = runningData.TimeScaleRate;
            }
            else
            {
                timeScale = 1.0f;
            }
            return stageData.Childs(stageIndex).Value.Group.Value;
        }

        protected override void OnTrackGroupComplete()
        {
            SkillRunningData runningData = Context.GetObject<SkillRunningData>();
            --runningData.ReleaseLoopCount;
            if(runningData.ReleaseLoopCount>0)
            {
                InitTrackGroup();
            }else
            {
                SetStageType(SkillStageType.End);
            }
        }
    }
}
