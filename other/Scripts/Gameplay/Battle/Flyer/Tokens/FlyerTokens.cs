using Leyoutech.AI.FSM;

namespace Gameplay.Battle.Flyer.Tokens
{
    public static class FlyerActionToken
    {
        public static readonly ActionToken EndAction = new ActionToken("EndAction");
        public static readonly ActionToken FinalizeAction = new ActionToken("FinalizeAction");
    }

    public static class FlyerStateToken
    {
        public static readonly StateToken Flying = new StateToken("Flying");
        public static readonly StateToken End = new StateToken("End");
    }
}
