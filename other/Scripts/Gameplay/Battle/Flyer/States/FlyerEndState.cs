using Eternity.FlatBuffer;
using Gameplay.Battle.Flyer.Tokens;
using Leyoutech.AI.FSM;
using Leyoutech.Core.Context;
using System;

namespace Gameplay.Battle.Flyer.States
{
    public class FlyerEndState : FlyerStateBase
    {
        public FlyerEndState() : base(FlyerStateToken.End)
        {
        }

        protected override void OnInitialized()
        {
            RegisterAction(FlyerActionToken.FinalizeAction, OnStageFinalize);
            base.OnInitialized();
        }

        protected override TrackGroup GetTrackGroup(out float timeScale)
        {
            timeScale = 1.0f;
            FlyerData flyerData = Context.GetObject<FlyerData>();
            return flyerData.EndData.Value.Group.Value;
        }

        protected override void OnTrackGroupComplete()
        {
            ChangeState(null);
        }

        private void OnStageFinalize(ActionToken action, object data)
        {
            ChangeState(null, data);
        }
    }
}
