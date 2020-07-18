using Eternity.FlatBuffer;
using Gameplay.Battle.Emit;
using Gameplay.Battle.Skill;
using Leyoutech.Core.Timeline;

namespace Gameplay.Battle.Timeline.Actions.Emit
{
    public class RangeEmitNodeAction : AEventActionItem
    {
        public override void Trigger()
        {
            EmitSelectionData emitSelectionData = m_Context.GetObject<SkillRunningData>().EmitSelectoin;

            RangeEmitNodeData nodeData = GetData<RangeEmitNodeData>();
            if (!emitSelectionData.ContainsEmit(nodeData.AssignIndex))
            {
                TimelineTrackGroup trackGroup = m_Context.GetObject<TimelineTrackGroup>();
                trackGroup.Pause();
            }
        }
    }
}
