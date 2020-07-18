using Gameplay.Battle.Emit;

namespace Gameplay.Battle.Skill
{
    public class SkillRunningData
    {
        public EmitSelectionData EmitSelectoin { get; }

        public float TimeScaleRate { get; set; } = 1.0f;
        public int ReleaseStageIndex { get; set; } = 0;
        public int ReleaseLoopCount { get; set; } = 1;

        public SkillRunningData()
        {
            EmitSelectoin = new EmitSelectionData();
        }

        public void Reset()
        {
            EmitSelectoin.Clear();
            TimeScaleRate = 1.0f;
            ReleaseStageIndex = 0;
            ReleaseLoopCount = 1;
        }
    }
}
