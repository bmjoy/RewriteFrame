/*===============================
 * Author: [Allen]
 * Purpose: 子弹外形组件
 * Time: 2019/12/23 16:27:18
================================*/

using Eternity.FlatBuffer;
using Eternity.FlatBuffer.Enums;
using Leyoutech.Core.Timeline;
using Leyoutech.Utility;
using System.Collections.Generic;
using UnityEngine;

public interface IBulletAppearanceProperty
{
    /// <summary>
    /// 获取飞行物数据ID
    /// </summary>
    /// <returns></returns>
    int GetFlyerDataId();

    Transform GetTransform();

    void SetRigidbody(Rigidbody rigidbody);

    /// <summary>
    /// 获取方向
    /// </summary>
    Vector3 GetDirection();

    /// <summary>
    /// 获取子弹entity
    /// </summary>
    /// <returns></returns>
    BaseEntity GetOwner();

    /// <summary>
    /// 子弹拥有者Entity ID
    /// </summary>
    uint GetOwnerSpacecaftEntityId();

    /// <summary>
    /// 目标是否是entity
    /// </summary>
    bool IsHaveTarget();

    /// <summary>
    /// 子弹要飞去的目标单位ID
    /// </summary>
    uint GetTargetEntityID();

    /// <summary>
    ///子弹要飞去的目标位置
    /// </summary>
    Vector3 GetTargetPoint();


}



public class BulletAppearanceComponent : EntityComponent<IBulletAppearanceProperty>
{
    private IBulletAppearanceProperty m_Property;

    private FlyerData m_FlyerData;//飞行数据

    private Transform m_BulletTf; //子弹Transform

    private BulletTriggerCollisionEnter m_TriggerCollisionEnter; //碰撞检测器

    public override void OnInitialize(IBulletAppearanceProperty property)
    {
        m_Property = property;
        m_BulletTf = m_Property.GetTransform();

        CfgEternityProxy cfgEternity = GameFacade.Instance.RetrieveProxy(ProxyName.CfgEternityProxy) as CfgEternityProxy;
        int flyerdataId = m_Property.GetFlyerDataId();
        m_FlyerData = cfgEternity.GetFlyerData(flyerdataId);

        BaseEntity bulletOwerEntity = GameplayManager.Instance.GetEntityManager().GetEntityById<BaseEntity>(m_Property.GetOwnerSpacecaftEntityId());
        BaseEntity targetEnity = m_Property.IsHaveTarget() ? GameplayManager.Instance.GetEntityManager().GetEntityById<BaseEntity>(m_Property.GetTargetEntityID()) : null;
        Vector3 targetPoint = m_Property.GetTargetPoint();

        m_TriggerCollisionEnter = m_BulletTf.GetOrAddComponent<BulletTriggerCollisionEnter>();

        m_TriggerCollisionEnter.Init(bulletOwerEntity,          //子弹拥有者Enity
            m_Property.GetOwner(),                                             //子弹Enity
            m_FlyerData.BaseData.Value.IsRigibody,                 //碰撞后单位是否继续飞 ，刚体爆炸，非刚体继续飞
            m_Property.IsHaveTarget(),                                        //是否指定了目标
           targetEnity,                                                                     //指定的飞向的目标
            targetPoint,                                                                    //指定的飞向的点
            m_FlyerData.BaseData.Value.FactionType,              //碰撞后的阵营判断
            TriggerToEnitity);                                                           //碰撞后的回调函数


        string assetPath = m_FlyerData.BaseData.Value.ApperanceAddress;
        if (!string.IsNullOrEmpty(assetPath))
        {
            AssetUtil.InstanceAssetAsync(assetPath,
              (pathOrAddress, returnObject, userData) =>
              {
                  if (returnObject != null)
                  {
                      GameObject pObj = (GameObject)returnObject;
                      OnLoadModel(pObj);
                  }
                  else
                  {
                      Debug.LogError(string.Format("资源加载成功，但返回null,  pathOrAddress = {0}", pathOrAddress));
                  }
              });
        }
    }


    private void OnLoadModel(GameObject pObj)
    {
        m_BulletTf.gameObject.name = string.Format("{0}_{1}", m_BulletTf.gameObject.name, m_FlyerData.BaseData.Value.ApperanceAddress);

        LayerUtil.SetGameObjectToLayer(m_BulletTf.gameObject, GameConstant.LayerTypeID.SkillProjectile, true);
        pObj.transform.SetParent(m_BulletTf, false);
        pObj.transform.localPosition = Vector3.zero;
        pObj.transform.localRotation = Quaternion.identity;

        //刚体，碰撞盒都挂载 ，子弹transfrom ，子弹预制体只作显示
        //刚体

        Rigidbody rigidbody = m_BulletTf.GetOrAddComponent<Rigidbody>();
        rigidbody.useGravity = false;
        rigidbody.drag = 0;
        rigidbody.angularDrag = 0;
        rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rigidbody.freezeRotation = true;
        rigidbody.velocity = Vector3.zero;

        m_Property.SetRigidbody(rigidbody);

        FlyerColliderData colliderData = m_FlyerData.BaseData.Value.Collider.Value;

        //碰撞盒
        switch (colliderData.ColliderType)
        {
            case FlyerColliderType.Capsule: //胶囊体
                {
                    // 0->Capsule height is along the x - axis. 1->Capsule height is along the y - axis. 2->Capsule height is along the z - axis
                    CapsuleCollider collider = m_BulletTf.GetOrAddComponent<CapsuleCollider>();
                    collider.direction = 2;
                    collider.radius = colliderData.Radius * SceneController.SKILL_PRECISION;
                    collider.height = colliderData.Height * SceneController.SKILL_PRECISION;
                    collider.isTrigger = true;
                }
                break;
            case FlyerColliderType.Sphere://球体
                {
                    SphereCollider collider = m_BulletTf.GetOrAddComponent<SphereCollider>();
                    collider.radius = colliderData.Radius * SceneController.SKILL_PRECISION;
                    collider.isTrigger = true;
                }
                break;
            case FlyerColliderType.Box://盒子
                {
                    BoxCollider collider = m_BulletTf.GetOrAddComponent<BoxCollider>();
                    collider.size = colliderData.Size.Value.ToVector3() * SceneController.SKILL_PRECISION;
                    collider.isTrigger = true;
                }
                break;
            default:
                break;
        }

        Quaternion rot = Quaternion.identity;
        rot.eulerAngles = m_Property.GetDirection();
        m_BulletTf.rotation = rot;
    }



    /// <summary>
    /// 碰撞到单位了
    /// </summary>
    /// <param name="otherEntity"></param>
    private void TriggerToEnitity(BaseEntity otherEntity)
    {
        Leyoutech.Utility.DebugUtility.LogWarning("子弹", string.Format("碰撞到单位了 who = {0}", otherEntity));

        FlyerTriggerToEnitity trigger = new FlyerTriggerToEnitity();
        trigger.targetEntity = otherEntity;
        SendEvent(ComponentEventName.FlyerTriggerToEnitity, trigger);
    }
}

