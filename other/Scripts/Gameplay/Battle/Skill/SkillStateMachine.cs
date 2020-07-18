using Gameplay.Battle.Skill.States;
using Leyoutech.AI.FSM;
using Leyoutech.Core.Context;

namespace Gameplay.Battle.Skill
{
    public class SkillStateMachine : StateMachine
    {
        public SkillStateMachine(IContext context) : base(context)
        {
            RegisterState(new SkillBeginState());
            RegisterState(new SkillBreakBeginState());
            RegisterState(new SkillReleaseState());
            RegisterState(new SkillBreakReleaseState());
            RegisterState(new SkillEndState());
            RegisterState(new SkillBreakEndState());
        }
    }
}
