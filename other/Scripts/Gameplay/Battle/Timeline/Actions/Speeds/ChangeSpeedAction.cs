using Eternity.FlatBuffer;
using Leyoutech.Core.Timeline;

namespace Gameplay.Battle.Timeline.Actions.Speeds
{
    public class ChangeSpeedAction : AEventActionItem
    {
        public override void Trigger()
        {
            IMoveActionProperty moveActionProperty = m_Context.GetObject<IMoveActionProperty>();
            ChangeSpeedData data = GetData<ChangeSpeedData>();

            moveActionProperty.SetSpeed(data.Speed * SceneController.SKILL_PRECISION);
        }
    }
}
