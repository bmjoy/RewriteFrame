using Eternity.FlatBuffer;
using Leyoutech.Core.Timeline;
using UnityEngine;

namespace Gameplay.Battle.Timeline.Actions.Sounds
{
    public class PlayEntityPositionSoundAction : AEventActionItem
    {
        public override void Trigger()
        {
            PlayEntityPositionSoundData data = GetData<PlayEntityPositionSoundData>();

            IBaseActionProperty baseActionProperty = m_Context.GetObject<IBaseActionProperty>();
            Vector3 entityPosition = baseActionProperty.GetRootTransform().position;
            if (baseActionProperty.IsMain())
            {
                WwiseUtil.PlaySound(data.MainSoundID, false, entityPosition);
            }
            else
            {
                WwiseUtil.PlaySound(data.ThirdPartSoundID, false, entityPosition);
            }
        }
    }
}
