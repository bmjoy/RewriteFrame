/*===============================
 * Author: [Allen]
 * Purpose:  准星逻辑，改善类,圆锥体
 * Time: 2020/2/21 1:56:58
================================*/
using EditorExtend;
using System.Collections.Generic;
using UnityEngine;
using Target = CCrossSightLoic.Target;

public class CrossSightLoic_Improve_Cone : CrossSightLoic_Improve
{
    public CrossSightLoic_Improve_Cone(CCrossSightLoic baseCCrossSightLoic) : base(baseCCrossSightLoic)
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

        Collider[] colliders = Physics.OverlapSphere(pos, m_crosssightinfo.m_MaxRayDistance, LayerUtil.GetLayersIntersectWithSkillProjectile(true)/*LayerMask.GetMask("Enemy")*/);
        for (int i = 0; i < colliders.Length; i++)
        {
            Vector3 colliderPos = colliders[i].transform.position;  //检测单位位置
            if (Vector3.Angle(direction,colliderPos - pos) <= m_crosssightinfo.m_ConeInfo.angle *0.5f) //单位跟摄像机正方向夹角是否在 1/2角度内
            {
                RaycastHit hit = new RaycastHit();

                int layermask = LayerUtil.GetLayersIntersectWithSkillProjectile(true);// LayerMask.GetMask("Enemy"/*, "Obstacles"*/); //指定射线碰撞的对象
                Physics.Raycast(pos, colliderPos - pos, out hit, m_crosssightinfo.m_MaxRayDistance, layermask);//检测障碍
                if(hit.collider == colliders[i])//如果途中无其他障碍物，那么射线就会碰撞到敌人
                {
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
        }
        return targets;
    }
}

