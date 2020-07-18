using Gameplay.Battle.Flyer.States;
using Leyoutech.AI.FSM;
using Leyoutech.Core.Context;

namespace Gameplay.Battle.Flyer
{
    public class FlyerStateMachine : StateMachine
    {
        public FlyerStateMachine(IContext context) : base(context)
        {
            RegisterState(new FlyerFlyingState());
            RegisterState(new FlyerEndState());
        }
    }
}
