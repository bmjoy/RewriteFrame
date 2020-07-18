using Eternity.FlatBuffer;
using Gameplay.Battle.Flyer.Tokens;
using Leyoutech.AI.FSM;
using Leyoutech.Core.Context;
using System;

namespace Gameplay.Battle.Flyer.States
{
    public class FlyerFlyingState : FlyerStateBase
    {
        public FlyerFlyingState() : base(FlyerStateToken.Flying)
        {
        }

        protected override void OnInitialized()
        {
            RegisterAction(FlyerActionToken.EndAction, OnStageEnd);
            base.OnInitialized();
        }

        protected override TrackGroup GetTrackGroup(out float timeScale)
        {
            timeScale = 1.0f;
            FlyerData flyerData = Context.GetObject<FlyerData>();
            return flyerData.FlyingData.Value.Group.Value;
        }

        protected override void OnTrackGroupComplete()
        {
            ChangeState(FlyerStateToken.End);
        }

        private void OnStageEnd(ActionToken action, object data)
        {
            ChangeState(FlyerStateToken.End);
        }
    }
}
