using Eternity.FlatBuffer;
using Leyoutech.Core.Context;
using Leyoutech.Core.Timeline;
using UnityEngine;

namespace Gameplay.Battle.Timeline.Actions.FlyerPaths
{
    public class FlyerAngularTracePathAction : ADurationActionItem
    {
        private IMoveActionProperty m_MoveActionProperty = null;
        private IBaseActionProperty m_BaseActionProperty = null;
        private float m_RemainingTime = 0.0f;

        private IBulletTargetProperty m_IBulletTargetProperty = null;
        public override void SetEnv(IContext context, ActionData data,float timeScale)
        {
            base.SetEnv(context, data,timeScale);

            var pathData = GetData<FlyerAngularTracePathData>();
            Duration = pathData.Duration * timeScale;
        }

        public override void DoEnter()
        {
            m_MoveActionProperty = m_Context.GetObject<IMoveActionProperty>();
            m_BaseActionProperty = m_Context.GetObject<IBaseActionProperty>();
            m_IBulletTargetProperty = m_Context.GetObject<IBulletTargetProperty>();

            m_RemainingTime = Duration;
        }

        public override void DoExit()
        {

        }

        public override void DoUpdate(float deltaTime)
        {
            m_RemainingTime -= deltaTime;
            if (m_RemainingTime > 0)
            {

                Vector3 targetPostion = m_IBulletTargetProperty.GetCurrentTargetPoint();
                Vector3 flyerPosition = m_BaseActionProperty.GetRootTransform().position;
                Vector3 targetDirection = (targetPostion - flyerPosition).normalized;
                Vector3 flyerDirection = m_BaseActionProperty.GetRootTransform().forward;

                float targetAngle = Vector3.Angle(flyerDirection, targetDirection);
                float angularSpeed = targetAngle / m_RemainingTime;

                if (Mathf.Abs(Vector3.Dot(flyerDirection, targetDirection)) > 1 - Mathf.Epsilon)
                {
                    flyerDirection = targetDirection;
                }
                else
                {
                    Vector3 rotationAxis = Vector3.Cross(flyerDirection, targetDirection).normalized;
                    float rotationDelta = deltaTime < m_RemainingTime ? deltaTime : m_RemainingTime;
                    Quaternion rotation = Quaternion.AngleAxis(angularSpeed * rotationDelta, rotationAxis);
                    flyerDirection = rotation * flyerDirection.normalized;
                }

                Vector3 eulerAngles = Quaternion.FromToRotation(Vector3.forward, flyerDirection.normalized).eulerAngles;
                m_MoveActionProperty.SetDirection(eulerAngles);


                Quaternion velRot = Quaternion.Euler(eulerAngles);
                Vector3 direction = velRot * Vector3.forward;
               // Leyoutech.Utility.DebugUtility.LogWarning("角速度", "角速度修改后的方向" + direction);
            }
        }
    }
}
