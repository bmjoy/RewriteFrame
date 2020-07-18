using Eternity.FlatBuffer;
using Leyoutech.Core.Context;
using Leyoutech.Core.Timeline;
using UnityEngine;

namespace Gameplay.Battle.Timeline.Actions.FlyerPaths
{
    public class FlyerTracePathAction : ADurationActionItem
    {
        private IMoveActionProperty m_MoveActionProperty = null;
        private IBaseActionProperty m_BaseActionProperty = null;
        private IBulletTargetProperty m_IBulletTargetProperty = null;

        public override void SetEnv(IContext context, ActionData data,float timeScale)
        {
            base.SetEnv(context, data,timeScale);

            var pathData = GetData<FlyerTracePathData>();
            Duration = pathData.Duration * timeScale;
        }

        public override void DoEnter()
        {
            m_MoveActionProperty = m_Context.GetObject<IMoveActionProperty>();
            m_BaseActionProperty = m_Context.GetObject<IBaseActionProperty>();
            m_IBulletTargetProperty = m_Context.GetObject<IBulletTargetProperty>();
        }

        public override void DoExit()
        {
            
        }

        public override void DoUpdate(float deltaTime)
        {
            Vector3 targetPostion = m_IBulletTargetProperty.GetCurrentTargetPoint();
            Vector3 flyerPosition = m_BaseActionProperty.GetRootTransform().position;
            Vector3 eulerAngles = Quaternion.FromToRotation(Vector3.forward, (targetPostion - flyerPosition).normalized).eulerAngles;
            m_MoveActionProperty.SetDirection(eulerAngles);
        }
    }
}
