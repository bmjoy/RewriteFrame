/*===============================
 * Author: [Allen]
 * Purpose: BulletTriggerCollisionEnter
 * Time: 2019/12/26 16:10:51
================================*/
using Eternity.FlatBuffer.Enums;
using System;
using System.Collections.Generic;
using UnityEngine;

public class BulletTriggerCollisionEnter : MonoBehaviour
{
    //所属的人entity
    private BaseEntity m_OwnerEntity = null;

    //所属的人entity
    private BaseEntity m_BulletEntity = null;


    /// <summary>
    /// 碰撞后单位不是目标单位是否继续飞
    /// </summary>
    private bool m_IsContinueToFly = false;


    //目标单位
    private BaseEntity targetEntity;

    //目标点
    private Vector3 targetPoint;

    //是否是指定目标碰撞
    private bool targetIsEntity = false;

    //碰撞后的回调函数
    private Action<BaseEntity> m_TriggerEnterAction;

    private GameplayProxy gameplayProxy;

    /// <summary>
    /// 碰撞阵营
    /// </summary>
    private FactionType m_TriggerFactionType;

    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="bulletOwerEntity">子弹拥有者Enity</param>
    /// <param name="bulletEntity">子弹Enity</param>
    /// <param name="IsRigibody">碰撞后单位是否继续飞 ，刚体爆炸，非刚体继续飞</param>
    /// <param name="targetIsentity">是否指定了目标</param>
    /// <param name="targetentity">指定的飞向的目标</param>
    /// <param name="targetpoint">指定的飞向的点</param>
    /// <param name="factionType">碰撞后的阵营判断</param>
    /// <param name="toEndFunc">碰撞后的回调函数</param>
    public void Init(BaseEntity bulletOwerEntity, BaseEntity bulletEntity, bool IsRigibody, bool targetIsentity, BaseEntity targetentity, Vector3 targetpoint, FactionType factionType, Action<BaseEntity> toEndFunc)
    {
        gameplayProxy = GameFacade.Instance.RetrieveProxy(ProxyName.GameplayProxy) as GameplayProxy;

        m_OwnerEntity = bulletOwerEntity;
        m_BulletEntity = bulletEntity;
        m_IsContinueToFly = !IsRigibody;
        targetIsEntity = targetIsentity;
        targetEntity = targetentity;
        targetPoint = targetpoint;
        m_TriggerFactionType = factionType;

        m_TriggerEnterAction = toEndFunc;

        Leyoutech.Utility.DebugUtility.LogWarning("子弹碰撞检测 ",string.Format("Init --->子弹拥有者entity = {0}, 子弹自己entiy = {1}, 目标是单位吗：{2} , 目标单位 = {3} , 目标点 = {4} ，撞到单位是否继续飞:{5}  ",
            m_OwnerEntity.EntityId(),
            m_BulletEntity.EntityId(),
            targetIsEntity,
            targetIsEntity? targetEntity.EntityId().ToString() : "Null" ,
            targetPoint,
            m_IsContinueToFly
            ));
    }


    private void OnCollisionEnter(UnityEngine.Collision other)
    {
    }

    private void OnTriggerEnter(UnityEngine.Collider other)
    {
        BaseEntity otherEntity = other?.attachedRigidbody?.GetComponent<BaseEntity>();
        if (otherEntity == null)
            return;
        if (otherEntity == m_OwnerEntity)
            return;

        if (targetIsEntity) //向指定目标飞
        {
            if (m_IsContinueToFly) //刚体子弹继续飞，不到目标不停
            {
                if (otherEntity != targetEntity) //不是目标忽略
                    return;

                ToLog(otherEntity);
                m_TriggerEnterAction?.Invoke(otherEntity);
            }
            else //撞到任意单位了
            {
                //阵营判断
                bool enemy = gameplayProxy.IsBelongToThisFactionType(m_TriggerFactionType, m_OwnerEntity, otherEntity);
                if (enemy)
                {
                    ToLog(otherEntity);
                    m_TriggerEnterAction?.Invoke(otherEntity);
                }
            }
        }
        else//向指定点飞
        {
            if (m_IsContinueToFly) //继续飞，不到目标不停
            {
            }
            else //撞到任意单位了
            {
                //阵营判断
                bool enemy = gameplayProxy.IsBelongToThisFactionType(m_TriggerFactionType, m_OwnerEntity, otherEntity);
                if (enemy)
                {
                    ToLog(otherEntity);
                    m_TriggerEnterAction?.Invoke(otherEntity);
                }
            }
        }
    }

    private void ToLog(BaseEntity otherEntity)
    {
        string str = string.Format("撞到单位了！！ --->子弹拥有者entity = {0}, 子弹自己entiy = {1}, 撞到entiy = {2}", m_OwnerEntity.EntityId(), m_BulletEntity.EntityId(), otherEntity.EntityId());
        Leyoutech.Utility.DebugUtility.LogWarning("子弹碰撞检测 ", str);
    }
}
