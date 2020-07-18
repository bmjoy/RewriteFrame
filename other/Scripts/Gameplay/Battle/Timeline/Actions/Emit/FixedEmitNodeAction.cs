using Eternity.FlatBuffer;
using Gameplay.Battle.Emit;
using Gameplay.Battle.Skill;
using Leyoutech.Core.Timeline;

namespace Gameplay.Battle.Timeline.Actions.Emit
{
    public class FixedEmitNodeAction : AEventActionItem
    {
        public override void Trigger()
        {
            EmitSelectionData emitSelectionData = m_Context.GetObject<SkillRunningData>().EmitSelectoin;

            FixedEmitNodeData nodeData = GetData<FixedEmitNodeData>();
            EmitData emitData = new EmitData()
            {
                NodeType = nodeData.NodeType,
                NodeIndex = nodeData.NodeIndex,
            };
            emitSelectionData.AddOrUpdateEmit(nodeData.AssignIndex, emitData);
        }
    }
}
