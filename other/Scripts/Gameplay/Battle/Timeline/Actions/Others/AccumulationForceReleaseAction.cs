using Leyoutech.Core.Timeline;

namespace Gameplay.Battle.Timeline.Actions.Others
{
    public class AccumulationForceReleaseAction : AEventActionItem
    {
        public override void Trigger()
        {
            //AccumulationForceReleaseData data = GetData<AccumulationForceReleaseData>();

            //蓄力技能强势释放
            IBaseActionProperty baseActionProperty = m_Context.GetObject<IBaseActionProperty>();

            if (!baseActionProperty.IsMain())
                return;

            //通知准星Size变化
            baseActionProperty.SendEvent(ComponentEventName.CoerceSkillButtonUp, null);

            TimelineTrackGroup trackGroup = m_Context.GetObject<TimelineTrackGroup>();
            trackGroup.Pause();
        }
    }
}
