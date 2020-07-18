/*===============================
 * Author: [Allen]
 * Purpose: CrossSightLineLogic
 * Time: 2019/11/26 20:15:27
================================*/

using System;
using System.Collections.Generic;
using UnityEngine;
using Target = CCrossSightLoic.Target;
using EditorExtend;

/// <summary>
/// 准星逻辑接口
/// </summary>
interface CrossSightLoic
{
    /// <summary>
    /// 设置准星信息
    /// </summary>
    /// <param name="crosssightinfo"></param>
    void SetCrossSightInfo(CrossSightInfo crosssightinfo);

    /// <summary>
    /// 获取目标
    /// </summary>
    /// <returns></returns>
     List<Target> GetTarget();
}


/// <summary>
/// 准星逻辑基类
/// </summary>
public class CCrossSightLoic : CrossSightLoic
{

    /// <summary>
    /// 准星目标
    /// </summary>
    public class Target
    {
        /// <summary>
        /// 目标单位
        /// </summary>
        public ulong target_entityId;

        /// <summary>
        /// 射线碰撞位置
        /// </summary>
        public Vector3 target_pos;
    }


    /// <summary>
    /// 准星信息
    /// </summary>
    protected CrossSightInfo m_crosssightinfo;


    //弹道偏移函数委托
    public delegate void BallisticOffsetFunc(out float offx, out float offy, params object[] args);

    /// <summary>
    /// 弹道偏移函数委托
    /// </summary>
    protected BallisticOffsetFunc m_ballisticOffsetFunc = null;

    protected object[] m_ballisticOffsetFunc_args;


    /// <summary>
    /// 中轴方向，默认摄像机正方向
    /// </summary>
    protected Vector3 m_CheckDirection ;

    /// <summary>
    /// 是否更改检测中轴方向
    /// </summary>
    protected bool IsChangCheckDirection = false;

    /// <summary>
    /// 设置检测方向
    /// </summary>
    public void SetCheckDirection(Vector3 v3)
    {
        IsChangCheckDirection = true;
        m_CheckDirection = v3;
    }

    /// <summary>
    /// 准星信息，弹道偏移函数委托
    /// </summary>
    /// <param name="crosssightinfo"></param>
    /// <param name="ballisticOffsetFunc"></param>
    public virtual void SetCrossSightInfo(CrossSightInfo crosssightinfo)
    {
        m_crosssightinfo = crosssightinfo;
        IsChangCheckDirection = false;
        //m_CheckDirection = CameraManager.GetInstance().GetMainCamereComponent().GetForward();
    }

    /// <summary>
    /// 弹道偏移函数委托
    /// </summary>
    public virtual void SetBallisticOffsetFunc(BallisticOffsetFunc ballisticOffsetFunc, params object[] args)
    {
        m_ballisticOffsetFunc = ballisticOffsetFunc;
        m_ballisticOffsetFunc_args = args;
    }


    /// <summary>
    /// 准星框内目标
    /// </summary>
    /// <returns></returns>
    public virtual List<Target> GetTarget()
    {
        return new List<Target>();
    }

    public virtual void Release()
    {
        m_crosssightinfo = null;
        m_ballisticOffsetFunc = null;
        IsChangCheckDirection = false;
    }


    /// <summary>
    ///  获取弹道上的目标，并返回弹道方向
    /// </summary>
    public virtual void GetBallisticTargetAndOutDirection(out List<Target>  target, out Vector3 direction, params object[] args)
    {
        target = new List<Target>();
        direction = CameraManager.GetInstance().GetMainCamereComponent().GetForward();
    }


    /// <summary>
    /// 修改虚拟摄像机属性
    /// </summary>
    public virtual void ChangeVirtualCameraAttribute(float fieldOfView, float aspect)
    {
    }



    /// <summary>
    /// 去除重复
    /// </summary>
    /// <param name=""></param>
    /// <param name="list"></param>
    public static bool IsAdd( List<Target> list , ulong targetID)
    {
        if ( list.Count == 0)
            return true;

        for (int i = 0; i < list.Count; i++)
        {
            Target tt = list[i];

            if (tt.target_entityId == targetID)
                return false;
        }   
        return true;
    }
}

/// <summary>
///准星逻辑，改善类
/// </summary>
public class CrossSightLoic_Improve : CCrossSightLoic
{
    private CCrossSightLoic m_CCrossSightLoic;
    public CrossSightLoic_Improve(CCrossSightLoic baseCCrossSightLoic)
    {
        m_CCrossSightLoic = baseCCrossSightLoic;
    }

    /// <summary>
    /// 改善的，获取目标列表
    /// </summary>
    /// <returns></returns>
    protected  virtual List<Target> ImproveGetTarget()
    {    
        return new List<Target>();
    }

    /// <summary>
    /// 获取目标列表
    /// </summary>
    /// <returns></returns>
    public override List<Target> GetTarget()
    {
        List<Target> targets = m_CCrossSightLoic.GetTarget();
        List<Target> thistargets = ImproveGetTarget();

        targets.AddRange(thistargets);
        return targets;
    }
}


/// <summary>
///准星逻辑，改善类,十字线
/// </summary>
public class CrossSightLoic_Improve_line : CrossSightLoic_Improve
{
    public CrossSightLoic_Improve_line(CCrossSightLoic baseCCrossSightLoic) : base(baseCCrossSightLoic)
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
        Vector3 direction = IsChangCheckDirection? m_CheckDirection : CameraManager.GetInstance().GetMainCamereComponent().GetForward();

        if (m_crosssightinfo.m_LineInfo.detectionMode == DetectionMode.One)
        {
            RaycastHit hitInfo = new RaycastHit();
            if (Physics.Raycast(pos,                                  //起始点
                direction,                                                      //方向
                out hitInfo,
                m_crosssightinfo.m_MaxRayDistance,    //最大距离
               // LayerMask.GetMask("Enemy")))              //层
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
        else if (m_crosssightinfo.m_LineInfo.detectionMode == DetectionMode.All)
        {
            RaycastHit[] hits = Physics.RaycastAll(pos, //起始点
                direction,                                                      //方向
                m_crosssightinfo.m_MaxRayDistance,    //最大距离
              //  LayerMask.GetMask("Enemy"));              //层
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