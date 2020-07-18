/*===============================
 * Author: [Allen]
 * Purpose: 准星逻辑，改善类,圆形
 * Time: 2020/2/19 23:20:38
================================*/

using EditorExtend;
using System.Collections.Generic;
using UnityEngine;
using Target = CCrossSightLoic.Target;


public class CrossSightLoic_Improve_Round : CrossSightLoic_Improve
{
    public CrossSightLoic_Improve_Round(CCrossSightLoic baseCCrossSightLoic) : base(baseCCrossSightLoic)
    {
    }


    /// <summary>
    /// 改善的，获取目标列表
    /// </summary>
    /// <returns></returns>
    protected override List<Target> ImproveGetTarget()
    {
        List<Target> targets = base.ImproveGetTarget();

        Vector3 pos = CameraManager.GetInstance().GetMainCamereComponent().GetPosition();
        Vector3 direction = IsChangCheckDirection ? m_CheckDirection : CameraManager.GetInstance().GetMainCamereComponent().GetForward();


        if (m_crosssightinfo.m_RoundInfo.detectionMode == DetectionMode.One)
        {
            RaycastHit hitInfo = new RaycastHit();
            if (Physics.SphereCast(pos,                                     //起始点
                m_crosssightinfo.m_RoundInfo.radius,            //半径
                direction,                                                                //方向
                out hitInfo,
                m_crosssightinfo.m_MaxRayDistance,              //最大距离
                //LayerMask.GetMask("Enemy")))                        //层
                LayerUtil.GetLayersIntersectWithSkillProjectile(true)))
            {
                if (hitInfo.rigidbody != null)
                {
                    BaseEntity entity = hitInfo.rigidbody?.GetComponent<BaseEntity>();
                    if (entity != null && CCrossSightLoic.IsAdd(targets, entity.EntityId()))
                    {
                        Target target = new Target();
                        target.target_entityId = entity.EntityId();
                        target.target_pos = hitInfo.point;
                        targets.Add(target);

                        GizmosHelper.GetInstance().DrawLine(pos, hitInfo.point, Color.red);
                        GizmosHelper.GetInstance().DrawSphere(hitInfo.point, m_crosssightinfo.m_RoundInfo.radius, Color.red);
                    }
                }
            }
        }
        else if (m_crosssightinfo.m_RoundInfo.detectionMode == DetectionMode.All)
        {
            RaycastHit[] hits = Physics.SphereCastAll(pos,   //起始点
                m_crosssightinfo.m_RoundInfo.radius,            //半径
                direction,                                                                //方向
                m_crosssightinfo.m_MaxRayDistance,              //最大距离
               // LayerMask.GetMask("Enemy"));                       //层
               LayerUtil.GetLayersIntersectWithSkillProjectile(true));


            for (int i = 0; i < hits.Length; i++)
            {
                RaycastHit hit = hits[i];
                if (hit.rigidbody != null)
                {
                    BaseEntity entity = hit.rigidbody?.GetComponent<BaseEntity>();
                    if (entity != null && CCrossSightLoic.IsAdd(targets, entity.EntityId()))
                    {
                        Target target = new Target();
                        target.target_entityId = entity.EntityId();
                        target.target_pos = hit.point;
                        targets.Add(target);

                        GizmosHelper.GetInstance().DrawLine(pos, hit.point, Color.red);
                    }
                }
            }
        }

            return targets;
    }
}