using Eternity.FlatBuffer;
using Leyoutech.Core.Timeline;


namespace Gameplay.Battle.Timeline.Actions.UI
{
    /// <summary>
    /// 通知准星Size变化
    /// </summary>
    public class ChangeCrossSightSizeAction : AEventActionItem
    {
        public override void Trigger()
        {
            IBaseActionProperty property = m_Context.GetObject<IBaseActionProperty>();
            if (!property.IsMain())
                return;

            //通知准星Size变化
            property.SendEvent(ComponentEventName.PostWeaponFire, null);
        }
    }
}
