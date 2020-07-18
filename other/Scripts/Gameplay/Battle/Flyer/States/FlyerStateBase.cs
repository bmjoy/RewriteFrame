using Gameplay.Battle.Skill;
using Gameplay.Battle.Timeline.States;
using Leyoutech.AI.FSM;

namespace Gameplay.Battle.Flyer.States
{
    public abstract class FlyerStateBase : AActionState
    {
        protected FlyerStateBase(StateToken token) : base(token)
        {
        }

        protected void SetStageType(FlyerStageType stageType)
        {
            FlyerActionComponent flyerComponent = Context.GetObject<FlyerActionComponent>();
            flyerComponent.StageType = stageType;
        }
    }
}
