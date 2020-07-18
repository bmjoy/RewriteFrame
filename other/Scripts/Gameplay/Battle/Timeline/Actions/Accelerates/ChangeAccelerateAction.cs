using Eternity.FlatBuffer;
using Leyoutech.Core.Context;
using Leyoutech.Core.Timeline;

namespace Gameplay.Battle.Timeline.Actions.Accelerates
{
    public class ChangeAccelerateAction : ADurationActionItem
    {
        private IMoveActionProperty m_MoveActionProperty = null;
        private ChangeAccelerateData m_AccData;

        public override void SetEnv(IContext context, ActionData data,float timeScale)
        {
            base.SetEnv(context, data,timeScale);

            m_AccData = GetData<ChangeAccelerateData>();
            Duration = m_AccData.Duration * timeScale;
        }

        public override void DoEnter()
        {
            m_MoveActionProperty = m_Context.GetObject<IMoveActionProperty>();
        }

        public override void DoExit()
        {
        }

        public override void DoUpdate(float deltaTime)
        {
            float speed = m_MoveActionProperty.GetSpeed() + m_AccData.Accelerate * SceneController.SKILL_PRECISION * deltaTime;
            m_MoveActionProperty.SetSpeed(speed);
        }
    }
}
