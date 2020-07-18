/*===============================
 * Author: [Allen]
 * Purpose: 准星逻辑，改善类,方锥体
 * Time: 2020/2/21 3:06:05
================================*/
using EditorExtend;
using System.Collections.Generic;
using UnityEngine;
using Target = CCrossSightLoic.Target;

public class CrossSightLoic_Improve_ConeSquare : CrossSightLoic_Improve
{
    private Camera m_virtualCamera; //虚拟相机

    public CrossSightLoic_Improve_ConeSquare(CCrossSightLoic baseCCrossSightLoic) : base(baseCCrossSightLoic)
    {
    }

    public override void SetCrossSightInfo(CrossSightInfo crosssightinfo)
    {
        base.SetCrossSightInfo(crosssightinfo);

        //构建虚拟相机
        if(m_virtualCamera == null)
        {
            m_virtualCamera = new GameObject("CrossSight VirtualCamera").AddComponent<Camera>();
            m_virtualCamera.enabled = false;
            GameObject.DontDestroyOnLoad(m_virtualCamera.gameObject);
        }
    }

    /// <summary>
    /// 修改虚拟摄像机属性
    /// </summary>
    public override void ChangeVirtualCameraAttribute(float fieldOfView , float aspect)
    {
        if (m_virtualCamera != null)
        {
            m_virtualCamera.fieldOfView = fieldOfView;
            m_virtualCamera.aspect = aspect;
        }
    }


    public override void Release()
    {
        base.Release();
        if (m_virtualCamera != null)
            GameObject.Destroy(m_virtualCamera.gameObject);
        m_virtualCamera = null;
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

        Plane[] planes = null;

        // 更新虚拟相机的方向和位置
        if (m_virtualCamera != null)
        {
            m_virtualCamera.transform.position = pos;
            m_virtualCamera.transform.rotation = CameraManager.GetInstance().GetMainCamereComponent().GetRotation();

            planes = GeometryUtility.CalculateFrustumPlanes(m_virtualCamera);
        }

        for (int i = 0; i < colliders.Length; i++)
        {
            if (planes == null)
                break;

            if (GeometryUtility.TestPlanesAABB(planes, colliders[i].bounds))
            {
                BaseEntity entity = colliders[i].attachedRigidbody?.GetComponent<BaseEntity>();
                if (entity !=null && CCrossSightLoic.IsAdd(targets, entity.EntityId()))
                {
                    Target target = new Target();
                    target.target_entityId = entity.EntityId();
                    target.target_pos = entity.transform.position;
                    targets.Add(target);

                    GizmosHelper.GetInstance().DrawLine(pos, target.target_pos, Color.red);
                }
            }
        }

        return targets;
    }


    /// <summary>
    ///  获取弹道上的目标，并返回弹道方向
    /// </summary>
    public override void GetBallisticTargetAndOutDirection(out List<Target> targets, out Vector3 direction, params object[] args)
    {
        Vector3 CamerPos = CameraManager.GetInstance().GetMainCamereComponent().GetPosition();// Camera.main.transform.position;
        base.GetBallisticTargetAndOutDirection(out targets, out direction, args);

        if (m_virtualCamera != null)
        {
            float offx = 0.0f;  //范围 0--0.5
            float offy = 0.0f;
            m_ballisticOffsetFunc?.Invoke(out offx, out offy, m_ballisticOffsetFunc_args);

            float offz =500; //到摄像机近视窗平面距离
            Vector3 viewPos = new Vector3(offx,offy,offz);//(0.5f + offx, 0.5f + offy, offz);
            Vector3 worldPos = m_virtualCamera.ViewportToWorldPoint(viewPos);
            direction = (worldPos - CamerPos).normalized;

            //验证
            //             GameObject ga = new GameObject("ddddd");
            //             ga.transform.position = worldPos;
            //             ga.AddComponent<BoxCollider>().size = Vector3.one;

            if (m_crosssightinfo.m_ConeSquareInfo.detectionMode == DetectionMode.One)
            {
                //射线检测目标
                RaycastHit hit = new RaycastHit();
                int layermask = LayerUtil.GetLayersIntersectWithSkillProjectile(true);// LayerMask.GetMask("Enemy"/*, "Obstacles"*/); //指定射线碰撞的对象
                Physics.Raycast(CamerPos, worldPos - CamerPos, out hit, offz, layermask);//检测障碍

                if (hit.rigidbody != null)
                {
                    BaseEntity entity = hit.rigidbody?.GetComponent<BaseEntity>();
                    if (entity != null && CCrossSightLoic.IsAdd(targets, entity.EntityId()))
                    {
                        Target target = new Target();
                        target.target_entityId = entity.EntityId();
                        target.target_pos = hit.point;
                        targets.Add(target);
                    }
                }
            }
            else if (m_crosssightinfo.m_ConeSquareInfo.detectionMode == DetectionMode.All)
            {
                int layermask = LayerUtil.GetLayersIntersectWithSkillProjectile(true);// LayerMask.GetMask("Enemy"/*, "Obstacles"*/); //指定射线碰撞的对象
                RaycastHit[] hits = Physics.RaycastAll(CamerPos,              //起始点
                    worldPos - CamerPos,                                                     //方向
                    m_crosssightinfo.m_MaxRayDistance,                //最大距离
                    layermask);

                for (int tt = 0; tt < hits.Length; tt++)
                {
                    RaycastHit hitt = hits[tt];
                    if (hitt.rigidbody != null)
                    {
                        BaseEntity entity = hitt.rigidbody?.GetComponent<BaseEntity>();
                        if (entity != null && CCrossSightLoic.IsAdd(targets, entity.EntityId()))
                        {
                            Target target = new Target();
                            target.target_entityId = entity.EntityId();
                            target.target_pos = hitt.point;
                            targets.Add(target);

                            GizmosHelper.GetInstance().DrawLine(CamerPos, hitt.point, Color.red);                           
                        }
                    }
                }
            }
        }
    }
}