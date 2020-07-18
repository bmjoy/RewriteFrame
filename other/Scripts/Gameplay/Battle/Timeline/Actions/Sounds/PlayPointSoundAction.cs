using Eternity.FlatBuffer;
using Gameplay.Battle.Timeline;
using Leyoutech.Core.Timeline;

namespace Battle.Timeline.Actions.Sounds
{
    public class PlayPointSoundAction : AEventActionItem
    {
        public override void Trigger()
        {
            PlayPointSoundData data = GetData<PlayPointSoundData>();

            IBaseActionProperty baseActionProperty = m_Context.GetObject<IBaseActionProperty>();
            if (baseActionProperty.IsMain())
            {
                WwiseUtil.PlaySound(data.MainSoundID, false, data.Position.Value.ToVector3());
            }
            else
            {
                WwiseUtil.PlaySound(data.ThirdPartSoundID, false, data.Position.Value.ToVector3());
            }
        }
    }
}
