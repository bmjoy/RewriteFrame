using Leyoutech.Core.Timeline;

namespace Battle.Timeline.Actions.States
{
    public class PauseAction : AEventActionItem

    {
        public override void Trigger()
        {
            TimelineTrackGroup trackGroup = m_Context.GetObject<TimelineTrackGroup>();
            trackGroup.Pause();
        }
    }
}