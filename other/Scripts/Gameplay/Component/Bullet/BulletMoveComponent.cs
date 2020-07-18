/*===============================
 * Author: [Allen]
 * Purpose: BulletMoveComponent
 * Time: 2019/12/24 17:07:46
================================*/
using UnityEngine;

public interface IBulletMoveProperty:IBulletTargetProperty
{
    Transform GetTransform();

    /// <summary>
    /// 获取方向
    /// </summary>
    Vector3 GetDirection();

    /// <summary>
    /// 获取速度
    /// </summary>
    float GetSpeed();


    void SetSpeed(float speed);
}

public class BulletMoveComponent : EntityComponent<IBulletMoveProperty>
{
    private IBulletMoveProperty m_Property;
    Transform m_Transform;

    public override void OnInitialize(IBulletMoveProperty property)
    {
        m_Property = property;
        m_Transform = m_Property.GetTransform();
    }

    public override void OnFixedUpdate()
    {
        //位置
        float currSpeed = m_Property.GetSpeed();
        Quaternion velRot = Quaternion.Euler(m_Property.GetDirection());
        Vector3 direction = velRot * Vector3.forward;

        Vector3 deltaPosion = direction * currSpeed * Time.fixedDeltaTime;
        m_Transform.position += deltaPosion;

        //旋转
        if (direction != Vector3.zero)
        {  
            m_Transform.rotation = Quaternion.Lerp(m_Transform.rotation, Quaternion.LookRotation(direction), 0.3f);
        }

        Vector3 targetPostion = m_Property.GetCurrentTargetPoint();
        float dis = Vector3.Distance(m_Transform.position, targetPostion);

        string log = string.Format("目标点 = {0} , 当前速度 v = {1} , 当前方向 dir = {2} ,还剩多远 dis = {3}  ",
           targetPostion,
           currSpeed,
           direction,
           dis
           );

        if (Vector3.Distance(m_Transform.position, targetPostion) < SceneController.BULLET_OVER_DISTANCE)
        {
            //Debug.Log("-----子弹移动- 到达目标点");

            FlyerTriggerToEnitity trigger = new FlyerTriggerToEnitity();
            trigger.targetEntity = null;
            trigger.targetPosition = targetPostion;
            SendEvent(ComponentEventName.FlyerTriggerToEnitity, trigger);
        }
    }
}

