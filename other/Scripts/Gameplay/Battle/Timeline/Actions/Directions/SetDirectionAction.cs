using Eternity.FlatBuffer;
using Leyoutech.Core.Timeline;

namespace Gameplay.Battle.Timeline.Actions.Directions
{
    public class SetDirectionAction : AEventActionItem
    {
        public override void Trigger()
        {
            IMoveActionProperty moveActionProperty = m_Context.GetObject<IMoveActionProperty>();
            SetDirectionData data = GetData<SetDirectionData>();

            moveActionProperty.SetDirection(data.Direction.Value.ToVector3());
        }
    }
}
