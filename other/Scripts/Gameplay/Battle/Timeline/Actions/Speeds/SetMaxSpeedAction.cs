using Eternity.FlatBuffer;
using Leyoutech.Core.Timeline;

namespace Gameplay.Battle.Timeline.Actions.Speeds
{
    public class SetMaxSpeedAction : AEventActionItem
    {
        public override void Trigger()
        {
            IMoveActionProperty moveActionProperty = m_Context.GetObject<IMoveActionProperty>();
            SetMaxSpeedData data = GetData<SetMaxSpeedData>();

            moveActionProperty.SetMaxSpeed(data.MaxSpeed);
        }
    }
}
