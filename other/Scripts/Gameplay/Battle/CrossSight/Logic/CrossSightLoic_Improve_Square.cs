/*===============================
 * Author: [Allen]
 * Purpose: 准星逻辑，改善类,方形
 * Time: 2020/2/20 0:38:18
================================*/

using EditorExtend;
using System.Collections.Generic;
using UnityEngine;
using Target = CCrossSightLoic.Target;


public class CrossSightLoic_Improve_Square : CrossSightLoic_Improve
{
    public CrossSightLoic_Improve_Square(CCrossSightLoic baseCCrossSightLoic) : base(baseCCrossSightLoic)
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


        if (m_crosssightinfo.m_SquareInfo.detectionMode == DetectionMode.One)
        {
            RaycastHit hitInfo = new RaycastHit();
            if (Physics.BoxCast(pos,                                            //起始点
                new Vector3(m_crosssightinfo.m_SquareInfo.boxX, m_crosssightinfo.m_SquareInfo.boxY, m_crosssightinfo.m_SquareInfo.boxZ),            //盒子大小
                direction,                                                                //方向
                out hitInfo,
               Quaternion.identity,                                               // 旋转
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
                    }
                }
            }
        }
        else if (m_crosssightinfo.m_SquareInfo.detectionMode == DetectionMode.All)
        {
            RaycastHit[] hits = Physics.BoxCastAll(pos,                                            //起始点
                new Vector3(m_crosssightinfo.m_SquareInfo.boxX, m_crosssightinfo.m_SquareInfo.boxY, m_crosssightinfo.m_SquareInfo.boxZ),            //盒子大小
                direction,                                                                //方向
               Quaternion.identity,                                               // 旋转
                m_crosssightinfo.m_MaxRayDistance,              //最大距离
              //  LayerMask.GetMask("Enemy"));                       //层
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
