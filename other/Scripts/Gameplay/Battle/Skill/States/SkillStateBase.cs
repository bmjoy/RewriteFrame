using Gameplay.Battle.Skill.Tokens;
using Gameplay.Battle.Timeline.States;
using Leyoutech.AI.FSM;
using System;

namespace Gameplay.Battle.Skill.States
{
    public abstract class SkillStateBase : AActionState
    {
        protected SkillStateBase(StateToken token) : base(token)
        {
        }

        protected override void OnInitialized()
        {
            RegisterAction(SkillActionToken.StageChangedAction, OnStageChanged);
            RegisterAction(SkillActionToken.PausedTimeline, OnPausedTimeline);
            RegisterAction(SkillActionToken.ResumeTimeline, OnResumeTimeline);
            RegisterAction(SkillActionToken.FinalizeAction, OnSkillFinalize);

            base.OnInitialized();
        }

        protected virtual void OnStageChanged(ActionToken action, object data)
        {
            throw new NotImplementedException($"SkillStateBase::OnStageChanged->the method is not implemented.action = {action}");
        }

        private void OnPausedTimeline(ActionToken action, object data)
        {
            m_TrackGroup?.Pause();
        }

        private void OnResumeTimeline(ActionToken action, object data)
        {
            m_TrackGroup?.Resume();
        }

        private void OnSkillFinalize(ActionToken action, object data)
        {
            ChangeState(null, data);
        }

        protected void SetStageType(SkillStageType stageType)
        {
            SpacecraftSkillComponent skillComponent = Context.GetObject<SpacecraftSkillComponent>();
            skillComponent.StageType = stageType;
        }
    }
}
