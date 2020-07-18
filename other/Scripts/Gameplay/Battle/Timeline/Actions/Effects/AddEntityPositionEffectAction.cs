using Eternity.FlatBuffer;
using Leyoutech.Core.Timeline;

namespace Gameplay.Battle.Timeline.Actions.Effects
{
    public class AddEntityPositionEffectAction : AEventActionItem
    {
        public override void Trigger()
        {
            IBaseActionProperty property = m_Context.GetObject<IBaseActionProperty>();
            AddEntityPositionEffectData data = GetData<AddEntityPositionEffectData>();
            EffectActionUtil.CreateEffect(data.Address, property.GetRootTransform().position, property.IsMain());
        }
    }
}
