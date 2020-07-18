using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 有执行状态的行为单位
public abstract class BaseBehavior {
    // 行为开始时间戳
    protected float actionstamp;

    // 行为持续时间
    // 小于0，持久执行（循环直到被外界打断）；
    // 等于0，执行1次；
    // 大于0，执行actionretain时常
    protected float actionretain;

    protected BehaviorController behaviorcontroller;

    protected BehaviorController.BehaviorType behaviortype;
    public BehaviorController.BehaviorType BehaviorType
    {
        get { return behaviortype; }
        set { behaviortype = value; }
    }

    protected bool syntony;

    protected float sqrprecision = 0.001f;   // 精度的平方

    protected void Init(
                        BehaviorController controller, 
                        float stamp, 
                        float retain,
                        bool syn)
    {
        behaviorcontroller = controller;

        actionstamp = stamp;
        actionretain = retain;

        syntony = syn;
    }

    public abstract void Execute();

    protected void ExecuteImp(BaseAction action)
    {
        if (actionretain < 0)
        {
            // 小于0，持久执行（循环直到被外界打断）；
            behaviorcontroller.AddAction(action);
        }
        else if (actionretain == 0)
        {
            // 等于0，执行1次；
            behaviorcontroller.AddAction(action);
            behaviorcontroller.RemoveBehavior(behaviortype);
        }
        else
        {
            // 大于0，执行actionretain时常
            if (Time.fixedTime - actionstamp >= actionretain)
            {
                behaviorcontroller.RemoveBehavior(behaviortype);
            }
            else
            {
                behaviorcontroller.AddAction(action);
            }
        }
    }

    /// <summary>
    /// OnDragWorldPosition 只在客户端做支持（服务端没有位置推拽的需求）
    /// </summary>
    /// <param name="drag_distance"></param>
    public virtual void OnDragWorldPosition(Vector3 drag_distance){}
}

// !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
// 以下为具体BaseBehavior封装，操作为状态行为，不互相耦合
// !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
// 飞船工作轴锁定类型
public enum PhysxMotionType
{
    Spacecraft, // 4
    Jet,        // 6
}

public struct WorkUnion
{
    public Vector3      work_current; 
    public float        work_starttime;
    public float        work_worktime;
    public ForceMode    work_forcemode;

}

public abstract class WorkUnionBehavior : BaseBehavior
{
    protected List<WorkUnion> m_workunions = new List<WorkUnion>();    // 飞船运行数据

    protected Rigidbody rigidbody;    // 目标刚体

    protected PhysxMotionType motiontype;

    protected void InitWorkUnion()
    {
        m_workunions.Clear();
    }

    protected bool UpdateTorques()
    {
        bool res = false;
        foreach (var work in m_workunions)
        {
            if (Time.fixedTime >= work.work_starttime)
            {
                if (Time.fixedTime - work.work_starttime < work.work_worktime)
                {
                    // 工作
                    BaseAction action = new TorqueAcceleration().Init(rigidbody, work.work_current);
                    ExecuteImp(action);

                    res = true;
                }
            }
        }
        return res;
    }

    protected Vector3 GetEuler(Vector3 forward)
    {
        Vector3 euler = Vector3.zero;

        Vector3 tmp_forward = forward;
        tmp_forward.y = 0;
        tmp_forward = SafeNormalized(tmp_forward);

        // xz平面
        float angle = Vector3.Angle(Vector3.forward, tmp_forward); // 角度值
        if (tmp_forward.x >= 0)
            euler.y = angle;
        else
            euler.y = -angle;

        // pitch角度
        tmp_forward = forward;
        tmp_forward.z = Mathf.Sqrt(tmp_forward.x * tmp_forward.x + tmp_forward.z * tmp_forward.z);
        tmp_forward.x = 0;
        tmp_forward = SafeNormalized(tmp_forward);
        angle = Vector3.Angle(Vector3.forward, tmp_forward); // 角度值
        if (tmp_forward.y >= 0)
            euler.x = -angle;
        else
            euler.x = angle;

        return euler;
    }

    protected bool UpdateForces()
    {
        bool res = false;
        foreach (var work in m_workunions)
        {
            if (Time.fixedTime >= work.work_starttime)
            {
                if (Time.fixedTime - work.work_starttime < work.work_worktime)
                {
                    // 工作
                    BaseAction action = new ForcesAcceleration().Init(rigidbody, work.work_current);
                    ExecuteImp(action);

                    res = true;
                }
            }
        }
        return res;
    }

    protected Vector3 SafeNormalized(Vector3 vec)
    {
        int x = vec.x > 0 ? 1 : -1;
        int y = vec.y > 0 ? 1 : -1;
        int z = vec.z > 0 ? 1 : -1;
        Vector3 resultvec = vec.normalized;
        resultvec = new Vector3(
                            Mathf.Abs(resultvec.x) * x,
                            Mathf.Abs(resultvec.y) * y,
                            Mathf.Abs(resultvec.z) * z);
        return resultvec;
    }
    // 当前速度施加最大反向扭矩减到0，需要时间
    protected float stopRotateMinTime(Vector3 angVelocity, float modulus)
    {
        return (Mathf.Sqrt(angVelocity.sqrMagnitude)) / modulus;
    }

    protected List<WorkUnion> stopRotateVelocity(Vector3 angularvec, float modulus)
    {
        List<WorkUnion> workunions = new List<WorkUnion>();

        WorkUnion resWorkUnion = new WorkUnion();
        Vector3 tmpscale = SafeNormalized(angularvec);
        resWorkUnion.work_current = new Vector3(
                                        -tmpscale.x * modulus,
                                        -tmpscale.y * modulus,
                                        -tmpscale.z * modulus);

        resWorkUnion.work_worktime = stopRotateMinTime(angularvec, modulus);
        resWorkUnion.work_starttime = Time.fixedTime;
        resWorkUnion.work_forcemode = ForceMode.Acceleration;

        workunions.Add(resWorkUnion);
        return workunions;
    }

    // 转一定弧度，预测最小时间
    protected float rotateAngleMinTime(float angle, float modulus, float stopmodulus)
    {
        //return Mathf.Sqrt(angle / modulus) * 2;

        float t1 = Mathf.Sqrt(angle / ((modulus / 2) + ((modulus * modulus) / (2 * stopmodulus))));
        float t2 = modulus * t1 / stopmodulus;

        return t1 + t2;
    }

    protected float rotateUpTime(float angle, float modulus, float stopmodulus)
    {
        return Mathf.Sqrt(
            angle / ((modulus / 2) + ((modulus * modulus) / (2 * stopmodulus))));
    }

    protected float rotateDownTime(float angle, float modulus, float stopmodulus)
    {
        float t1 = Mathf.Sqrt(
            angle / ((modulus / 2) + ((modulus * modulus) / (2 * stopmodulus))));
        return modulus * t1 / stopmodulus;
    }

    protected List<WorkUnion> RotateAngular(Vector3 from, Vector3 to, float maxangspeed, float modulus, float stopmodulus)    // 本地坐标系
    {
        List<WorkUnion> workunions = new List<WorkUnion>();

        WorkUnion resWorkUnion1, resWorkUnion2, resWorkUnion3;
        // 计算弧度
        float angle = Vector3.Angle(SafeNormalized(from), SafeNormalized(to)) / 180 * Mathf.PI;
        float pretime = speedUpTime(angle, modulus, stopmodulus);

        if (modulus * pretime > maxangspeed)
        {
            float t1 = maxangspeed / modulus;
            float t3 = maxangspeed / stopmodulus;
            float moveang = maxangspeed * t1 / 2 + maxangspeed * t3 / 2;
            float t2 = (angle - moveang) / maxangspeed;

            resWorkUnion1.work_worktime = t1;
            resWorkUnion1.work_starttime = Time.fixedTime;
            resWorkUnion1.work_forcemode = ForceMode.Acceleration;

            Vector3 torquedir = Vector3.Cross(from, to);
            if (Mathf.Abs(angle - Mathf.PI) < 0.02f && from.Equals(Vector3.forward))
                torquedir = Vector3.up;
            Vector3 tmpscale = SafeNormalized(torquedir);
            if (tmpscale.sqrMagnitude < 0.1f)
                tmpscale.y = 1.0f;
            resWorkUnion1.work_current = rigidbody.transform.TransformDirection(
                new Vector3(tmpscale.x * modulus, tmpscale.y * modulus, tmpscale.z * modulus));



            resWorkUnion2.work_worktime = t2;
            resWorkUnion2.work_starttime =
                resWorkUnion1.work_starttime + resWorkUnion1.work_worktime;
            resWorkUnion2.work_forcemode = ForceMode.Acceleration;
            resWorkUnion2.work_current = Vector3.zero;

            resWorkUnion3.work_worktime = t3;
            resWorkUnion3.work_starttime =
                resWorkUnion2.work_starttime + resWorkUnion2.work_worktime;
            resWorkUnion3.work_forcemode = ForceMode.Acceleration;
            resWorkUnion3.work_current = rigidbody.transform.TransformDirection(new Vector3(
                -tmpscale.x * stopmodulus, -tmpscale.y * stopmodulus, -tmpscale.z * stopmodulus));

            workunions.Add(resWorkUnion1);
            workunions.Add(resWorkUnion2);
            workunions.Add(resWorkUnion3);
        }
        else
        {
            // 分两个阶段
            resWorkUnion1.work_worktime = pretime;
            resWorkUnion1.work_starttime = Time.fixedTime;
            resWorkUnion1.work_forcemode = ForceMode.Acceleration;

            Vector3 torquedir = Vector3.Cross(from, to);
            if (Mathf.Abs(angle - Mathf.PI) < 0.02f && from.Equals(Vector3.forward))
                torquedir = Vector3.up;
            Vector3 tmpscale = SafeNormalized(torquedir);
            if (tmpscale.sqrMagnitude < 0.1f)
                tmpscale.y = 1.0f;

            resWorkUnion1.work_current = rigidbody.transform.TransformDirection(
                new Vector3(tmpscale.x * modulus, tmpscale.y * modulus, tmpscale.z * modulus));

            resWorkUnion2.work_worktime = 0;
            resWorkUnion2.work_starttime =
                resWorkUnion1.work_starttime + resWorkUnion1.work_worktime;
            resWorkUnion2.work_forcemode = ForceMode.Acceleration;
            resWorkUnion2.work_current = Vector3.zero;

            resWorkUnion3.work_worktime = speedDownTime(angle, modulus, stopmodulus);
            resWorkUnion3.work_starttime =
                resWorkUnion2.work_starttime + resWorkUnion2.work_worktime;
            resWorkUnion3.work_forcemode = ForceMode.Acceleration;
            resWorkUnion3.work_current = rigidbody.transform.TransformDirection(new Vector3(
                -tmpscale.x * stopmodulus, -tmpscale.y * stopmodulus, -tmpscale.z * stopmodulus));

            workunions.Add(resWorkUnion1);
            workunions.Add(resWorkUnion2);
            workunions.Add(resWorkUnion3);
        }

        return workunions;
    }

    protected float stopMoveMinTime(Vector3 Velocity, float modulus)
    {
        return (Mathf.Sqrt(Velocity.sqrMagnitude)) / modulus;
    }

    protected List<WorkUnion> stopMoveVelocity(Vector3 vec, float modulus)
    {
        List<WorkUnion> workunions = new List<WorkUnion>();

        WorkUnion resWorkUnion;
        Vector3 localvec = rigidbody.transform.InverseTransformDirection(vec);    // 本地坐标系速度
        // 计算出最小减速度
        Vector3 tmpscale = SafeNormalized(localvec);
        resWorkUnion.work_current = rigidbody.transform.TransformDirection(new Vector3(
                                        -tmpscale.x * modulus,
                                        -tmpscale.y * modulus,
                                        -tmpscale.z * modulus));

        resWorkUnion.work_worktime = stopMoveMinTime(vec, modulus);
        resWorkUnion.work_starttime = Time.fixedTime;
        resWorkUnion.work_forcemode = ForceMode.Acceleration;

        workunions.Add(resWorkUnion);
        return workunions;
    }

    // 走一段距离，预测最小时间
    protected float MoveDistanceMinTime(float distance, float modulus, float stopmodulus)
    {
        //return Mathf.Sqrt(distance * rigidbody.mass / modulus) * 2;

        float t1 = Mathf.Sqrt(distance / ((modulus / 2) + ((modulus * modulus) / (2 * stopmodulus))));
        float t2 = modulus * t1 / stopmodulus;

        return t1 + t2;
    }

    protected float speedUpTime(float distance, float modulus, float stopmodulus)
    {
        return Mathf.Sqrt(distance / ((modulus / 2) + ((modulus * modulus) / (2 * stopmodulus))));
    }

    protected float speedDownTime(float distance, float modulus, float stopmodulus)
    {
        float t1 = Mathf.Sqrt(distance / ((modulus / 2) + ((modulus * modulus) / (2 * stopmodulus))));
        return modulus * t1 / stopmodulus;
    }

    protected List<WorkUnion> MoveDistance(Vector3 dis, Vector3 forces, Vector3 invforces, 
                                           Vector3 maxspd, Vector3 invmaxspd, Vector3 stopforces)
    {
        List<WorkUnion> workunions = new List<WorkUnion>();

        Vector3 localdis = rigidbody.transform.InverseTransformDirection(dis);
        Vector3 currentforces = GetMaxVector(localdis, forces, invforces);
        Vector3 maxspeed = GetMaxVector(localdis, maxspd, invmaxspd);
        currentforces = rigidbody.transform.TransformDirection(currentforces);    // 世界坐标系下的力

        float modu = Mathf.Sqrt(currentforces.sqrMagnitude);
        float distance = Mathf.Sqrt(dis.sqrMagnitude);
        float stopmodu = Mathf.Sqrt(stopforces.sqrMagnitude);
        float fmaxspeed = Mathf.Sqrt(maxspeed.sqrMagnitude);
        float pretime = speedUpTime(distance, modu, stopmodu);

        if (modu * pretime > fmaxspeed)
        {
            // 分三个阶段
            WorkUnion resWorkUnion1, resWorkUnion2, resWorkUnion3;

            float t1 = fmaxspeed / modu;
            float t3 = fmaxspeed / stopmodu;
            float movedis = fmaxspeed * t1 / 2 + fmaxspeed * t3 / 2;
            float t2 = (distance - movedis)/fmaxspeed;


            resWorkUnion1.work_worktime = t1;
            resWorkUnion1.work_starttime = Time.fixedTime;
            resWorkUnion1.work_forcemode = ForceMode.Acceleration;
            Vector3 tmpscale = SafeNormalized(dis);
            resWorkUnion1.work_current = new Vector3(
                                            tmpscale.x * modu,
                                            tmpscale.y * modu,
                                            tmpscale.z * modu);



            resWorkUnion2.work_worktime = t2;
            resWorkUnion2.work_starttime = resWorkUnion1.work_starttime + resWorkUnion1.work_worktime;
            resWorkUnion2.work_forcemode = ForceMode.Acceleration;
            resWorkUnion2.work_current = Vector3.zero;


            resWorkUnion3.work_worktime = t3;
            resWorkUnion3.work_starttime = resWorkUnion2.work_starttime + resWorkUnion2.work_worktime;
            resWorkUnion3.work_forcemode = ForceMode.Acceleration;
            resWorkUnion3.work_current = new Vector3(
                                            -tmpscale.x * stopmodu,
                                            -tmpscale.y * stopmodu,
                                            -tmpscale.z * stopmodu);

            workunions.Add(resWorkUnion1);
            workunions.Add(resWorkUnion2);
            workunions.Add(resWorkUnion3);
        }
        else
        {
            // 分两个阶段
            WorkUnion resWorkUnion1, resWorkUnion2;
            resWorkUnion1.work_worktime = pretime;
            resWorkUnion1.work_starttime = Time.fixedTime;
            resWorkUnion1.work_forcemode = ForceMode.Acceleration;
            Vector3 tmpscale = SafeNormalized(dis);
            resWorkUnion1.work_current = new Vector3(
                                            tmpscale.x * modu,
                                            tmpscale.y * modu,
                                            tmpscale.z * modu);

            resWorkUnion2.work_worktime = speedDownTime(distance, modu, stopmodu);
            resWorkUnion2.work_starttime = resWorkUnion1.work_starttime + resWorkUnion1.work_worktime;
            resWorkUnion2.work_forcemode = ForceMode.Acceleration;
            resWorkUnion2.work_current = new Vector3(
                                            -tmpscale.x * stopmodu,
                                            -tmpscale.y * stopmodu,
                                            -tmpscale.z * stopmodu);

            workunions.Add(resWorkUnion1);
            workunions.Add(resWorkUnion2);
        }
        return workunions;
    }

    // 功能型函数,输入和输出都为本地坐标系
    protected Vector3 GetMaxVector(Vector3 dir, Vector3 maxv, Vector3 invmaxv)
    {
        Vector3 result = SafeNormalized(dir);

        float x = float.MaxValue;
        float y = float.MaxValue;
        float z = float.MaxValue;

        if (result.x > 0)
            x = maxv.x / result.x;
        else if(result.x < 0)
            x = -invmaxv.x / result.x;

        if (result.y > 0)
            y = maxv.y / result.y;
        else if(result.y < 0)
            y = -invmaxv.y / result.y;

        if (result.z > 0)
            z = maxv.z / result.z;
        else if(result.z < 0)
            z = -invmaxv.z / result.z;

        float scale = Mathf.Min(x, y);
        scale = Mathf.Min(scale, z);

        result.Scale(new Vector3(scale, scale, scale));

        return result;
    }
    
}

//BT_Idle             // 待机状态
public class IdleBehavior : BaseBehavior
{
    // 执行信息的参数 执行对象，IDLE动画名
    public BaseBehavior Init(
                        BehaviorController controller, 
                        float stamp, 
                        float retain, 
                        int addparam = 0,
                        bool syn = false)
    {
        Init(controller, stamp, retain, syn);
        BehaviorType = BehaviorController.BehaviorType.BT_Idle;
        return this;
    }

    public override void Execute()
    {
    }
}

//BT_FourcesLocal,    // 本地加力状态（本地坐标系）
public class FourcesLocalBehavior : BaseBehavior
{
    Rigidbody rigidbody;

    Vector3 rigid_force;
    Vector3 rigid_stopforce;
    Vector3 rigid_maxsp;
    Vector3 rigid_invmaxsp;

    ForceMode forcemode;        

    public BaseBehavior Init(
                        BehaviorController controller, 
                        float stamp, 
                        float retain,
                        Rigidbody rigid,


                        Vector3 force, 
                        Vector3 stopforce, 
                        Vector3 maxsp, 
                        Vector3 invmaxsp,

                        ForceMode mode,
                        bool syn = false)
    {
        Init(controller, stamp, retain, syn);
        BehaviorType = BehaviorController.BehaviorType.BT_FourcesLocal;

        rigidbody   = rigid;
        forcemode   = mode;

        rigid_force = force;
        rigid_stopforce = stopforce;
        rigid_maxsp = maxsp;
        rigid_invmaxsp = invmaxsp;

        return this;
    }

    public override void Execute()
    {
        Vector3 forces = Vector3.zero;
        // 根据当前状态，计算出飞船运行最终本地坐标系的力
        Vector3 localvel = rigidbody.transform.InverseTransformDirection(rigidbody.velocity);   // 本地速度
        float tmpprecision = Mathf.Sqrt(rigid_stopforce.sqrMagnitude) * 0.02f;

        // X轴
        if (rigid_force.x == 0)  // X轴不施加力
        {
            if (localvel.x * localvel.x > tmpprecision)
            {
                // 阻尼自动减速，力为速度的反向
                forces.x = -localvel.x * Mathf.Sqrt(rigid_stopforce.sqrMagnitude) / Mathf.Abs(localvel.x);
            }
            else
                localvel.x = 0;
        }
        else
        {
            // 如果当前速度达到最大值，则提高该方向的力不生效
            if (!(localvel.x != Mathf.Clamp(localvel.x, -rigid_invmaxsp.x, rigid_maxsp.x) &&
                localvel.x / Mathf.Abs(localvel.x) == rigid_force.x / Mathf.Abs(rigid_force.x)
                ))
            {
                // 如果方向一致 或者速度为0
                if (localvel.x == 0.0f ||
                    (localvel.x / Mathf.Abs(localvel.x) == rigid_force.x / Mathf.Abs(rigid_force.x)))
                {
                    forces.x = rigid_force.x;
                }
                else
                {
                    forces.x = -localvel.x * Mathf.Sqrt(rigid_stopforce.sqrMagnitude) / Mathf.Abs(localvel.x);
                    forces.x += rigid_force.x;
                }
            }
            else
            {
                localvel.x = localvel.x > 0 ? rigid_maxsp.x : -rigid_invmaxsp.x;
            }
        }

        // Y轴
        if (rigid_force.y == 0)  // X轴不施加力
        {
            if (localvel.y * localvel.y > tmpprecision)
            {
                // 阻尼自动减速，力为速度的反向
                forces.y = -localvel.y * Mathf.Sqrt(rigid_stopforce.sqrMagnitude) / Mathf.Abs(localvel.y);
            }
            else
                localvel.y = 0;
        }
        else
        {
            // 如果当前速度达到最大值，则提高该方向的力不生效
            if (!(localvel.y != Mathf.Clamp(localvel.y, -rigid_invmaxsp.y, rigid_maxsp.y) &&
                localvel.y / Mathf.Abs(localvel.y) == rigid_force.y / Mathf.Abs(rigid_force.y)
                ))
            {
                // 如果方向一致 或者速度为0
                if (localvel.y == 0.0f ||
                    (localvel.y / Mathf.Abs(localvel.y) == rigid_force.y / Mathf.Abs(rigid_force.y)))
                {
                    forces.y = rigid_force.y;
                }
                else
                {
                    forces.y = -localvel.y * Mathf.Sqrt(rigid_stopforce.sqrMagnitude) / Mathf.Abs(localvel.y);
                    forces.y += rigid_force.y;
                }
            }
            else
            {
                localvel.y = localvel.y > 0 ? rigid_maxsp.y : -rigid_invmaxsp.y;
            }
        }

        // Z轴
        if (rigid_force.z == 0)  // X轴不施加力
        {
            if (localvel.z * localvel.z > tmpprecision)
            {
                // 阻尼自动减速，力为速度的反向
                forces.z = -localvel.z * Mathf.Sqrt(rigid_stopforce.sqrMagnitude) / Mathf.Abs(localvel.z);
            }
            else
                localvel.z = 0;
        }
        else
        {
            // 如果当前速度达到最大值，则提高该方向的力不生效
            if (!(localvel.z != Mathf.Clamp(localvel.z, -rigid_invmaxsp.z, rigid_maxsp.z) &&
                localvel.z / Mathf.Abs(localvel.z) == rigid_force.z / Mathf.Abs(rigid_force.z)
                ))
            {
                // 如果方向一致 或者速度为0
                if (localvel.z == 0.0f ||
                    (localvel.z / Mathf.Abs(localvel.z) == rigid_force.z / Mathf.Abs(rigid_force.z)))
                {
                    forces.z = rigid_force.z;
                }
                else
                {
                    forces.z = -localvel.z * Mathf.Sqrt(rigid_stopforce.sqrMagnitude) / Mathf.Abs(localvel.z);
                    forces.z += rigid_force.z;
                }
            }
            else
            {
                localvel.z = localvel.z > 0 ? rigid_maxsp.z : -rigid_invmaxsp.z;
            }
        }

        rigidbody.velocity = rigidbody.transform.TransformDirection(localvel);
        forces = rigidbody.transform.TransformDirection(forces);

        // 如果当前力停止，飞船速度为0，则停止该状态
        if (forces.sqrMagnitude == 0)
            return;

        BaseAction action;
        if (forcemode == ForceMode.Force)
            action = new ForcesForce().Init(rigidbody, forces);
        else if (forcemode == ForceMode.Impulse)
            action = new ForcesImpulse().Init(rigidbody, forces);
        else if (forcemode == ForceMode.VelocityChange)
            action = new ForcesVelocityChange().Init(rigidbody, forces);
        else 
            action = new ForcesAcceleration().Init(rigidbody, forces);

        ExecuteImp(action);
    }
}

//BT_Torque,          // 扭矩状态，只有本地坐标系
public class TorqueBehavior : BaseBehavior
{
    Rigidbody rigidbody;
    ForceMode forcemode;

    Vector3 rigid_torque;
    Vector3 rigid_stoptourque;
    Vector3 rigid_maxanglespeed;

    public BaseBehavior Init(
                        BehaviorController controller,
                        float stamp,
                        float retain,
                        Rigidbody rigid,

                        Vector3 torque,
                        Vector3 stoptourque,
                        Vector3 maxanglespeed,

                        ForceMode mode,
                        bool syn = false)


    {
        Init(controller, stamp, retain, syn);
        BehaviorType = BehaviorController.BehaviorType.BT_Torque;

        rigidbody = rigid;
        forcemode = mode;

        rigid_torque = torque;
        rigid_stoptourque = stoptourque;
        rigid_maxanglespeed = maxanglespeed;

        return this;
    }

    public override void Execute()
    {
        Vector3 torque = Vector3.zero;
        Vector3 localangvel = rigidbody.transform.InverseTransformDirection(rigidbody.angularVelocity);
        float tmpprecision = Mathf.Sqrt(rigid_stoptourque.sqrMagnitude) * 0.02f;

        // X轴
        if (rigid_torque.x == 0)  
        {
            if (localangvel.x * localangvel.x > tmpprecision)
            {
                // 阻尼自动减速，力为速度的反向
                torque.x = -localangvel.x * Mathf.Sqrt(rigid_stoptourque.sqrMagnitude) / Mathf.Abs(localangvel.x);
            }
            else
                localangvel.x = 0;
        }
        else
        {
            // 如果当前角速度达到最大值，则提高该方向的扭矩不生效
            if (!(localangvel.x != Mathf.Clamp(localangvel.x, -rigid_maxanglespeed.x, rigid_maxanglespeed.x) &&
                localangvel.x / Mathf.Abs(localangvel.x) == rigid_torque.x / Mathf.Abs(rigid_torque.x)
                ))
            {
                if (localangvel.x == 0.0f ||
                    (localangvel.x / Mathf.Abs(localangvel.x) == rigid_torque.x / Mathf.Abs(rigid_torque.x)))
                {
                    torque.x = rigid_torque.x;
                }
                else
                {
                    torque.x = -localangvel.x * Mathf.Sqrt(rigid_stoptourque.sqrMagnitude) / Mathf.Abs(localangvel.x);
                    torque.x += rigid_torque.x;
                }
            }
            else
            {
                localangvel.x = localangvel.x > 0 ? rigid_maxanglespeed.x : -rigid_maxanglespeed.x;
            }
        }

        // Y轴
        if (rigid_torque.y == 0)  
        {
            if (localangvel.y * localangvel.y > tmpprecision)
            {
                // 阻尼自动减速，力为速度的反向
                torque.y = -localangvel.y * Mathf.Sqrt(rigid_stoptourque.sqrMagnitude) / Mathf.Abs(localangvel.y);
            }
            else
                localangvel.y = 0;
        }
        else
        {
            // 如果当前速度达到最大值，则提高该方向的力不生效
            if (!(localangvel.y != Mathf.Clamp(localangvel.y, -rigid_maxanglespeed.y, rigid_maxanglespeed.y) &&
                localangvel.y / Mathf.Abs(localangvel.y) == rigid_torque.y / Mathf.Abs(rigid_torque.y)
                ))
            {
                if (localangvel.y == 0.0f ||
                    (localangvel.y / Mathf.Abs(localangvel.y) == rigid_torque.y / Mathf.Abs(rigid_torque.y)))
                {
                    torque.y = rigid_torque.y;
                }
                else
                {
                    torque.y = -localangvel.y * Mathf.Sqrt(rigid_stoptourque.sqrMagnitude) / Mathf.Abs(localangvel.y);
                    torque.y += rigid_torque.y;
                }
            }
            else
            {
                localangvel.y = localangvel.y > 0 ? rigid_maxanglespeed.y : -rigid_maxanglespeed.y;
            }
        }

        // Z轴
        if (rigid_torque.z == 0)  
        {
            if (localangvel.z * localangvel.z > tmpprecision)
            {
                // 阻尼自动减速，力为速度的反向
                torque.z = -localangvel.z * Mathf.Sqrt(rigid_stoptourque.sqrMagnitude) / Mathf.Abs(localangvel.z);
            }
            else
                localangvel.z = 0;
        }
        else
        {
            // 如果当前速度达到最大值，则提高该方向的力不生效
            if (!(localangvel.z != Mathf.Clamp(localangvel.z, -rigid_maxanglespeed.z, rigid_maxanglespeed.z) &&
                localangvel.z / Mathf.Abs(localangvel.z) == rigid_torque.z / Mathf.Abs(rigid_torque.z)
                ))
            {
                if (localangvel.z == 0.0f ||
                    (localangvel.z / Mathf.Abs(localangvel.z) == rigid_torque.z / Mathf.Abs(rigid_torque.z)))
                {
                    torque.z = rigid_torque.z;
                }
                else
                {
                    torque.z = -localangvel.z * Mathf.Sqrt(rigid_stoptourque.sqrMagnitude) / Mathf.Abs(localangvel.z);
                    torque.z += rigid_torque.z;
                }
            }
            else
            {
                localangvel.z = localangvel.z > 0 ? rigid_maxanglespeed.z : -rigid_maxanglespeed.z;
            }
        }

        rigidbody.angularVelocity = rigidbody.transform.TransformDirection(localangvel);
        torque = rigidbody.transform.TransformDirection(torque);

        // 如果当前力停止，飞船速度为0，则停止该状态
        if (torque.sqrMagnitude == 0)
            return;

        BaseAction action;
        if (forcemode == ForceMode.Force)
            action = new TorqueForce().Init(rigidbody, torque);
        else if (forcemode == ForceMode.Impulse)
            action = new TorqueImpulse().Init(rigidbody, torque);
        else if (forcemode == ForceMode.VelocityChange)
            action = new TorqueVelocityChange().Init(rigidbody, torque);
        else
            action = new TorqueAcceleration().Init(rigidbody, torque);

        ExecuteImp(action);
    }
}

// BT_StopRotate,            // 停止指定方向旋转行为
public class StopRotateBehavior : WorkUnionBehavior
{
    public enum RotateState
    {
        state_org,              // 初始态，有角速度或线速度
        state_org2stop,         // 初始态到停止角度
        state_stop,             // 角速度停止的状态
        state_end,
    }

    Vector3 stop_torque;        // 飞船停止扭矩

    public RotateState rotate_state;   // 飞船当前状态
    ForceMode forcemode = ForceMode.Acceleration;

    public BaseBehavior Init(
                        BehaviorController controller,
                        float stamp,
                        float retain,
                        Rigidbody rigid,
                        Vector3 stoptorque,
                        bool syn = false
                        )
    {
        Init(controller, stamp, retain, syn);
        BehaviorType = BehaviorController.BehaviorType.BT_StopRotate;

        rigidbody = rigid;
        stop_torque = stoptorque;

        rotate_state = RotateState.state_org;
        return this;
    }

    public override void Execute()
    {
        // 1 开始状态，操作停止当前旋转
        if (rotate_state == RotateState.state_org)
        {
            m_workunions = stopRotateVelocity(rigidbody.angularVelocity, Mathf.Sqrt(stop_torque.sqrMagnitude));
            rotate_state = RotateState.state_org2stop;
        }
        // 2 停止初始旋转状态
        else if (rotate_state == RotateState.state_org2stop)
        {
            if (!UpdateTorques())
            {
                InitWorkUnion();
                rotate_state = RotateState.state_stop;
            }
        }
        // 3 旋转静止
        else if (rotate_state == RotateState.state_stop)
        {
            rigidbody.angularVelocity = Vector3.zero;

            rotate_state = RotateState.state_end;
            if (syntony)
                behaviorcontroller.OnBehaviorComplete(behaviortype);
        }
    }
}

//BT_StopMove,            // 停止指定方向移动行为
public class StopMoveBehavior : WorkUnionBehavior
{
    public enum MoveState
    {
        state_org,              // 初始态，有线速度
        state_org2stop,         // 初始态到停止移动
        state_stop,             // 停止   
        state_end,
    }

    Vector3 stop_forces;        // 飞船减速阻力
    public MoveState move_state;       // 飞船当前状态

    ForceMode forcemode = ForceMode.Acceleration;

    public BaseBehavior Init(
                        BehaviorController controller,
                        float stamp,
                        float retain,
                        Rigidbody rigid,
                        PhysxMotionType motiontp,
                        Vector3 stopforce,
                        bool syn = false
                        )
    {
        Init(controller, stamp, retain, syn);
        BehaviorType = BehaviorController.BehaviorType.BT_StopMove;

        rigidbody = rigid;
        motiontype = motiontp;
        stop_forces = stopforce;

        move_state = MoveState.state_org;
        return this;
    }

    public override void Execute()
    {
        // 1 开始状态，操作停止运动
        if (move_state == MoveState.state_org)
        {
            m_workunions = stopMoveVelocity(rigidbody.velocity, Mathf.Sqrt(stop_forces.sqrMagnitude));
            move_state = MoveState.state_org2stop;
        }
        // 2 停止初始运动状态
        else if (move_state == MoveState.state_org2stop)
        {
            if (!UpdateForces())
            {
                InitWorkUnion();
                move_state = MoveState.state_stop;
            }
        }
        // 3 运动静止
        else if (move_state == MoveState.state_stop)
        {
            rigidbody.velocity = Vector3.zero;

            move_state = MoveState.state_end;
            if (syntony)
                behaviorcontroller.OnBehaviorComplete(behaviortype);
        }
    }
}

// BT_TurnToTarget,    // 朝向目标点
public class TurnToTargetBehavior : WorkUnionBehavior
{
    public enum RotateState
    {
        state_org,              // 初始态，有角速度或线速度
        state_stop,             // 角速度停止的状态
        state_stop2turntoTarget,// 停止态到转向目标点的状态
        state_turntoTarget,     // 朝向目标点的状态       
        state_turntoTarget2up,  // 矫正飞船姿态朝上的状态
        state_up,               // 飞船姿态矫正完成
        state_end,
    }
    
    Vector3 rigid_torque;       // 飞船扭矩
    Vector3 stop_torque;        // 飞船停止扭矩
    Vector3 final_target;       // 最终目标
    Vector3 maxangularspeed;    // 最大角速度

    public RotateState rotate_state;   // 飞船当前状态

    StopRotateBehavior stoprotateBehavior;

    public override void OnDragWorldPosition(Vector3 drag_distance)
    {
        final_target = final_target + drag_distance;
    }

    public BaseBehavior Init(
                        BehaviorController controller,
                        float stamp,
                        float retain,
                        Rigidbody rigid,
                        PhysxMotionType motiontp,

                        Vector3 torque,
                        Vector3 stoptorque,
                        Vector3 maxangspeed,

                        Vector3 target,
                        bool syn = false
                        )
    {
        Init(controller, stamp, retain, syn);
        BehaviorType = BehaviorController.BehaviorType.BT_TurnToTarget;

        rigidbody       = rigid;
        motiontype      = motiontp;

        rigid_torque    = torque;
        stop_torque     = stoptorque;
        maxangularspeed = maxangspeed;
        final_target    = target;

        rotate_state    = RotateState.state_org;

        stoprotateBehavior = new StopRotateBehavior();
        stoprotateBehavior.Init(behaviorcontroller, actionstamp, actionretain, rigidbody, stop_torque);

        return this;
    }

    public override void Execute()
    {
        // TurnToTargetBehavior行为的执行
        // 1 开始状态，操作停止当前旋转
        if (rotate_state == RotateState.state_org)
        {
            stoprotateBehavior.Execute();
            if (stoprotateBehavior.rotate_state == StopRotateBehavior.RotateState.state_end)
            {
                rotate_state = RotateState.state_stop;
            }
        }
        // 3 旋转静止
        else if (rotate_state == RotateState.state_stop)
        {
            Vector3 destDir = final_target - rigidbody.transform.position;
            if (motiontype == PhysxMotionType.Spacecraft)    // 如果是4
                destDir.y = 0;

            // 本地坐标系，转向目标
            Vector3 tmpDir = rigidbody.transform.InverseTransformDirection(destDir); 

            if (tmpDir.sqrMagnitude <= sqrprecision)
                rotate_state = RotateState.state_turntoTarget; 
            else
            {
                m_workunions = RotateAngular(Vector3.forward, tmpDir, maxangularspeed.magnitude, rigid_torque.magnitude,
                    stop_torque.magnitude);
                rotate_state = RotateState.state_stop2turntoTarget;
            }
        }
        // 4 直接转到目标方向
        else if (rotate_state == RotateState.state_stop2turntoTarget)
        {
            if (!UpdateTorques())
            {
                InitWorkUnion();
                rotate_state = RotateState.state_turntoTarget;
            }
        }
        // 开启绕Z轴旋转
        else if (rotate_state == RotateState.state_turntoTarget)
        {
            rigidbody.angularVelocity = Vector3.zero;

            Vector3 upDir = rigidbody.transform.InverseTransformDirection(Vector3.up);
            upDir = new Vector3(upDir.x, upDir.y, 0);

            if (upDir.sqrMagnitude <= sqrprecision)
                rotate_state = RotateState.state_up; 
            else
            {
                m_workunions = RotateAngular(Vector3.up, upDir, maxangularspeed.magnitude, rigid_torque.magnitude,
                    stop_torque.magnitude);
                rotate_state = RotateState.state_turntoTarget2up;
            }
        }
        // 绕Z轴旋转
        else if (rotate_state == RotateState.state_turntoTarget2up)
        {
            if (!UpdateTorques())
            {
                InitWorkUnion();
                rotate_state = RotateState.state_up;
            }
        }
        else if (rotate_state == RotateState.state_up)
        {
            rigidbody.angularVelocity = Vector3.zero;

            rotate_state = RotateState.state_end;
            if (syntony)
                behaviorcontroller.OnBehaviorComplete(behaviortype);
        }

    }
}

// z轴正向旋转
public class UpTurnToTargetBehavior :  WorkUnionBehavior
{
    public enum RotateState
    {
        state_org,                // 初始态，有角速度或线速度
        state_stop,               // 角速度停止的状态
        state_stop2turntoTarget,  // 停止态到转向目标点的状态
        state_turntoTarget,       // 朝向目标点的状态
        state_turntoTarget2up,    // 矫正飞船姿态朝上的状态
        state_up,                 // 飞船姿态矫正完成
        state_end,
    };

    Vector3 rigid_torque;     // 飞船扭矩
    Vector3 stop_torque;      // 飞船停止扭矩
    Vector3 final_target;     // 最终目标
    Vector3 maxangularspeed;  // 最大角速度


    StopRotateBehavior stoprotateBehavior = new StopRotateBehavior();
    Vector3 origition_euler;
    Vector3 dest_euler;
    List<WorkUnion> m_workunions1;
    List<WorkUnion> m_workunions2;

    public RotateState rotate_state;  // 飞船当前状态

    public override void OnDragWorldPosition(Vector3 drag_distance)
    {
        final_target = final_target + drag_distance;
    }

    public BaseBehavior Init(
                        BehaviorController controller,
                        float stamp,
                        float retain,
                        Rigidbody rigid,
                        PhysxMotionType motiontp,

                        Vector3 torque,
                        Vector3 stoptorque,
                        Vector3 maxangspeed,

                        Vector3 target,
                        bool syn = false)
    {
        Init(controller, stamp, retain, syn);
        BehaviorType = BehaviorController.BehaviorType.BT_UpTurnToTarget;

        rigidbody = rigid;
        motiontype = motiontp;

        rigid_torque = torque;
        stop_torque = stoptorque;
        maxangularspeed = maxangspeed;
        final_target = target;

        rotate_state = RotateState.state_org;
        
        stoprotateBehavior.Init(behaviorcontroller, actionstamp,
                                 actionretain, rigidbody, stop_torque);
        return this;
    }

    protected List<WorkUnion> RotateAngularUseTime(Vector3 from, Vector3 to, float needtime, float modulus, float stopmodulus)
    {
        // 计算弧度
        List<WorkUnion> workunions = new List<WorkUnion>();

        float angle = Vector3.Angle(SafeNormalized(from), SafeNormalized(to)) / 180 * Mathf.PI;
        float time = needtime;

        float delt =
            modulus * time * modulus * time -
            4 * (modulus / 2.0f + modulus * modulus / (2 * stopmodulus)) * angle;
        float t1 = 0, t2 = 0, t3 = 0;
        if (delt > 0)
        {
            // 可以加速到最大速度，过程分为三段
            t1 = (modulus * time + Mathf.Sqrt(delt)) /
                 (modulus + modulus * modulus / stopmodulus);
            t2 = time - (modulus / stopmodulus + 1) * t1;
            t3 = modulus * t1 / stopmodulus;

            if (t2 < 0)
            {
                t1 = (modulus * time - Mathf.Sqrt(delt)) /
                     (modulus + modulus * modulus / stopmodulus);
                t2 = time - (modulus / stopmodulus + 1) * t1;
                t3 = modulus * t1 / stopmodulus;
            }
        }
        else
        {
            // 不能加速到最大速度，过程分为两段
            t1 = Mathf.Sqrt(angle /
                            (modulus / 2 + modulus * modulus / (2 * stopmodulus)));
            t2 = 0;
            t3 = modulus * t1 / stopmodulus;
            time = t1 + t2 + t3;
        }

        WorkUnion resWorkUnion1, resWorkUnion2, resWorkUnion3;
        resWorkUnion1.work_worktime = t1;
        resWorkUnion1.work_starttime = Time.fixedTime;
        resWorkUnion1.work_forcemode = ForceMode.Acceleration;

        Vector3 torquedir = Vector3.Cross(from, to);
        Vector3 tmpscale = SafeNormalized(torquedir);
        if (tmpscale.sqrMagnitude < 0.1f) tmpscale.y = 1.0f;
        resWorkUnion1.work_current = rigidbody.transform.TransformDirection(
            new Vector3(tmpscale.x * modulus, tmpscale.y * modulus, tmpscale.z * modulus));

        resWorkUnion2.work_worktime = t2;
        resWorkUnion2.work_starttime =
            resWorkUnion1.work_starttime + resWorkUnion1.work_worktime;
        resWorkUnion2.work_forcemode = ForceMode.Acceleration;
        resWorkUnion2.work_current = Vector3.zero;

        resWorkUnion3.work_worktime = t3;
        resWorkUnion3.work_starttime =
            resWorkUnion2.work_starttime + resWorkUnion2.work_worktime;
        resWorkUnion3.work_forcemode = ForceMode.Acceleration;
        resWorkUnion3.work_current = rigidbody.transform.TransformDirection(
            new Vector3(-tmpscale.x * stopmodulus, -tmpscale.y * stopmodulus, -tmpscale.z * stopmodulus));

        workunions.Add(resWorkUnion1);
        workunions.Add(resWorkUnion2);
        workunions.Add(resWorkUnion3);

        return workunions;
    }

    public override void Execute()
    {
        if (rotate_state == RotateState.state_org)
        {
            stoprotateBehavior.Execute();
            if (stoprotateBehavior.rotate_state ==
                StopRotateBehavior.RotateState.state_end)
            {
                rotate_state = RotateState.state_stop;
            }
        }

        else if (rotate_state == RotateState.state_stop)
        {
            // 计算初始状态信息，飞船当前朝向
            Vector3 forward = rigidbody.transform.TransformDirection(Vector3.forward);
            origition_euler = GetEuler(forward);

            forward = final_target - rigidbody.transform.position;
            dest_euler = GetEuler(forward);

            // 初始状态
            Vector3 from1 = rigidbody.transform.TransformDirection(Vector3.forward);
            from1.y = 0;
            from1 = rigidbody.transform.InverseTransformDirection(from1);

            // 计算旋转信息 1 XZ平面
            Vector3 destDir1 = final_target - rigidbody.transform.position;
            destDir1.y = 0;
            destDir1 = rigidbody.transform.InverseTransformDirection(destDir1);
            
            m_workunions1 = RotateAngular(from1, destDir1, maxangularspeed.magnitude, rigid_torque.magnitude, stop_torque.magnitude);

            // 初始状态
            Vector3 from2 = rigidbody.transform.TransformDirection(Vector3.forward);
            from2.z = Mathf.Sqrt(from2.x * from2.x + from2.z * from2.z);
            from2.x = 0;
            from2 = rigidbody.transform.InverseTransformDirection(from2);

            // 计算旋转信息 2 pitch
            Vector3 destDir2 = final_target - rigidbody.transform.position;
            destDir2.z = Mathf.Sqrt(destDir2.x * destDir2.x + destDir2.z * destDir2.z);
            destDir2.x = 0;
            destDir2 = rigidbody.transform.InverseTransformDirection(destDir2);

            m_workunions2 = RotateAngular(from2, destDir2, maxangularspeed.magnitude, rigid_torque.magnitude, stop_torque.magnitude);

            float all_time1 = 0, all_time2 = 0;
            foreach(var work in m_workunions1)
                all_time1 += work.work_worktime;
            foreach (var work in m_workunions2)
                all_time2 += work.work_worktime;

            if (all_time1 > all_time2)
            {
                m_workunions2 = RotateAngularUseTime(from2, destDir2, all_time1, rigid_torque.magnitude, stop_torque.magnitude);
            }
            else if (all_time1 < all_time2)
            {
                m_workunions1 = RotateAngularUseTime(from1, destDir1, all_time2, rigid_torque.magnitude, stop_torque.magnitude);
            }

            rotate_state = RotateState.state_stop2turntoTarget;
        }
        else if (rotate_state == RotateState.state_stop2turntoTarget)
        {
            // 计算
            WorkUnion workunion1_0 = m_workunions1[0];
            WorkUnion workunion1_1 = m_workunions1[1];
            WorkUnion workunion1_2 = m_workunions1[2];

            WorkUnion workunion2_0 = m_workunions2[0];
            WorkUnion workunion2_1 = m_workunions2[1];
            WorkUnion workunion2_2 = m_workunions2[2];

            Vector3 tmp_dest_euler = new Vector3(origition_euler.x, origition_euler.y, origition_euler.z);
            if (Time.fixedTime > workunion1_2.work_starttime + workunion1_2.work_worktime)
            {
                // 完成转向
                tmp_dest_euler = new Vector3(dest_euler.x, dest_euler.y, dest_euler.z);
                rotate_state = RotateState.state_turntoTarget;
                rigidbody.angularVelocity = Vector3.zero;
            }
            else
            {
                // XZ平面
                float work_time = Time.fixedTime - workunion1_0.work_starttime;

                if (work_time < workunion1_0.work_worktime)
                {
                    // 第一阶段角加速
                    tmp_dest_euler.y +=
                        ((workunion1_0.work_current.y * work_time * work_time) /
                         (2 * Mathf.PI) * 180);
                    rigidbody.angularVelocity = new Vector3(0, workunion1_0.work_current.y * work_time, 0);
                }
                else
                {
                    // 第二阶段角匀速
                    tmp_dest_euler.y +=
                        ((workunion1_0.work_current.y * workunion1_0.work_worktime *
                          workunion1_0.work_worktime) /
                         (2 * Mathf.PI) * 180);
                    work_time = Time.fixedTime - workunion1_1.work_starttime;

                    if (work_time < workunion1_1.work_worktime)
                    {
                        tmp_dest_euler.y += ((workunion1_0.work_current.y *
                                               workunion1_0.work_worktime * work_time) /
                                              Mathf.PI * 180);
                        rigidbody.angularVelocity = new Vector3(0, workunion1_0.work_current.y * workunion1_0.work_worktime, 0);
                    }
                    else
                    {
                        // 第三阶段角减速
                        work_time = workunion1_2.work_worktime -
                                    (Time.fixedTime - workunion1_2.work_starttime);
                        tmp_dest_euler.y = dest_euler.y;
                        tmp_dest_euler.y +=
                            ((workunion1_2.work_current.y * work_time * work_time) /
                             (2 * Mathf.PI) * 180);

                        rigidbody.angularVelocity = new Vector3(0, workunion1_0.work_current.y * workunion1_0.work_worktime + workunion1_2.work_current.y * work_time, 0);
                    }
                }

                // pitch角度
                work_time = Time.fixedTime - workunion2_0.work_starttime;

                if (work_time < workunion2_0.work_worktime)
                {
                    // 第一阶段角加速
                    tmp_dest_euler.x +=
                        ((workunion2_0.work_current.x * work_time * work_time) /
                         (2 * Mathf.PI) * 180);
                }
                else
                {
                    // 第二阶段角匀速
                    tmp_dest_euler.x +=
                        ((workunion2_0.work_current.x * workunion2_0.work_worktime *
                          workunion2_0.work_worktime) /
                         (2 * Mathf.PI) * 180);
                    work_time = Time.fixedTime - workunion2_1.work_starttime;

                    if (work_time < workunion2_1.work_worktime)
                    {
                        tmp_dest_euler.x += ((workunion2_0.work_current.x *
                                               workunion2_0.work_worktime * work_time) /
                                              Mathf.PI * 180);
                    }
                    else
                    {
                        // 第三阶段角减速
                        work_time = workunion2_2.work_worktime -
                                    (Time.fixedTime - workunion2_2.work_starttime);
                        tmp_dest_euler.x = dest_euler.x;
                        tmp_dest_euler.x +=
                            ((workunion2_2.work_current.x * work_time * work_time) /
                             (2 * Mathf.PI) * 180);
                    }
                }
            }

            // 设置朝向
            Quaternion quat = new Quaternion();
            quat.eulerAngles = tmp_dest_euler;
            rigidbody.transform.rotation = quat;
        }
        // 开启绕Z轴旋转
        else if (rotate_state == RotateState.state_turntoTarget)
        {
            Vector3 upDir =
                rigidbody.transform.InverseTransformDirection(Vector3.up);
            upDir = new Vector3(upDir.x, upDir.y, 0);

            if (upDir.sqrMagnitude <= sqrprecision)
                rotate_state = RotateState.state_up;
            else
            {
                m_workunions = RotateAngular(Vector3.up, upDir,
                              maxangularspeed.magnitude, rigid_torque.magnitude,
                              stop_torque.magnitude);
                rotate_state = RotateState.state_turntoTarget2up;
            }
        }
        // 绕Z轴旋转
        else if (rotate_state == RotateState.state_turntoTarget2up)
        {
            if (!UpdateTorques())
            {
                InitWorkUnion();
                rotate_state = RotateState.state_up;
            }
        }
        else if (rotate_state == RotateState.state_up)
        {
            rigidbody.angularVelocity = Vector3.zero;

            rotate_state = RotateState.state_end;
            if (syntony)
            {
                behaviorcontroller.OnBehaviorComplete(behaviortype);
            }
        }
    }
};

// BT_TurningMove,   
public class TurningMoveBehavior : WorkUnionBehavior
{
    Vector3 rigid_forces;   // 飞船推力
    Vector3 stop_forces;    // 飞船减速推力
    Vector3 maxspeed;       // 转弯最大线速度

    public BaseBehavior Init(
                        BehaviorController controller,
                        float stamp,
                        float retain,
                        Rigidbody rigid,
                        PhysxMotionType motiontp,
                        Vector3 force,
                        Vector3 stopforce,
                        Vector3 maxspd,
                        bool syn = false
                        )
    {
        Init(controller, stamp, retain, syn);
        BehaviorType = BehaviorController.BehaviorType.BT_TurningMove;

        rigidbody = rigid;
        motiontype = motiontp;

        rigid_forces = force;
        stop_forces = stopforce;
        maxspeed = maxspd;

        return this;
    }

    public override void Execute()
    {
        // 飞船正在转向
        Vector3 forceunion = Vector3.zero;
        Vector3 localvec = rigidbody.transform.InverseTransformDirection(rigidbody.velocity);
        
        if (localvec.z < maxspeed.z)
        {
            Vector3 fc = new Vector3(0, 0, rigid_forces.z);
            forceunion += fc;
        }
        else
        {
            Vector3 fc = new Vector3(0, 0, -stop_forces.magnitude);
            forceunion += fc;
        }

        /* 方法一
        int x = localvec.x > 0 ? -1 : 1;
        int y = localvec.y > 0 ? -1 : 1;
        if (localvec.x != 0)
        {
            Vector3 fc = new Vector3(x * stop_forces.magnitude, 0, 0);
            forceunion += fc;
        }
        if (localvec.y != 0)
        {
            Vector3 fc = new Vector3(0, y * stop_forces.magnitude, 0);
            forceunion += fc;
        }
        */

        // 方法二
        localvec.x = 0; localvec.y = 0;
        rigidbody.velocity = rigidbody.transform.TransformDirection(localvec);

        BaseAction action = new ForcesAcceleration().Init(rigidbody, rigidbody.transform.TransformDirection(forceunion));
        ExecuteImp(action);
    }
}

// BT_StopThenMoveToTarget,    // 朝向目标点
public class StopThenMoveToTargetBehavior : WorkUnionBehavior
{
    public enum MoveState
    {
        state_org,              // 初始态，有线速度
        state_stop,             // 停止
        state_stop2DirectMoveToTarget,// 停止态到超目标点移动的状态
        state_DirectMoveToTargetToStop,
        state_DirectMoveToTarget,     // 朝目标点移动的状态       
        state_end,
    }

    Vector3 rigid_forces;       // 飞船推力
    Vector3 rigid_invforces;    // 飞船推力
    Vector3 stop_forces;          // 飞船减速阻力
    Vector3 stop_rotate;
    Vector3 final_target;       // 最终目标
    Vector3 maxspeed;           // 最大线速度
    Vector3 invmaxspeed;
    Vector3 destdirection;

    MoveState move_state;       // 飞船当前状态
    StopMoveBehavior stopmovebehavior;
    StopRotateBehavior stoprotatebehavior;

    float latestsaveddistance;


    public override void OnDragWorldPosition(Vector3 drag_distance)
    {
        final_target = final_target + drag_distance;
    }

    public BaseBehavior Init(
                        BehaviorController controller,
                        float stamp,
                        float retain,
                        Rigidbody rigid,
                        PhysxMotionType motiontp,
                        Vector3 force,
                        Vector3 invforce,
                        Vector3 stopforce,
                        Vector3 stoprotate,
                        Vector3 maxsp,
                        Vector3 invmaxsp,
                        Vector3 target,
                        bool syn = false
                        )
    {
        Init(controller, stamp, retain, syn);
        BehaviorType = BehaviorController.BehaviorType.BT_StopThenMoveToTarget;

        rigidbody = rigid;
        motiontype = motiontp;

        rigid_forces = force;
        rigid_invforces = invforce;
        stop_forces = stopforce;
        stop_rotate = stoprotate;

        maxspeed = maxsp;
        invmaxspeed = invmaxsp;
        final_target = target;

        move_state = MoveState.state_org;

        stopmovebehavior = new StopMoveBehavior();
        stopmovebehavior.Init(controller, stamp, retain, rigid, motiontp, stopforce);

        stoprotatebehavior = new StopRotateBehavior();
        stoprotatebehavior.Init(controller, stamp, retain, rigid, stoprotate);

        return this;
    }

    public override void Execute()
    {
        // DirectMoveToTargetBehavior行为的执行
        // 1 开始状态，操作停止运动
        if (move_state == MoveState.state_org)
        {
            stopmovebehavior.Execute();
            stoprotatebehavior.Execute();

            if (stopmovebehavior.move_state == StopMoveBehavior.MoveState.state_end &&
                stoprotatebehavior.rotate_state == StopRotateBehavior.RotateState.state_end)
            {
                move_state = MoveState.state_stop;
            }
        }
        // 3 运动静止
        else if (move_state == MoveState.state_stop)
        {
            destdirection = final_target - rigidbody.transform.position;
            latestsaveddistance = destdirection.magnitude + 0.0001f;

            if (destdirection.sqrMagnitude <= sqrprecision)
                move_state = MoveState.state_DirectMoveToTarget;
            else
            {
                move_state = MoveState.state_stop2DirectMoveToTarget;
            }
        }
        // 4 直接运行到减速
        else if (move_state == MoveState.state_stop2DirectMoveToTarget)
        {
            float dis = (final_target - rigidbody.transform.position).magnitude;

            float ti = stopMoveMinTime(rigidbody.velocity, Mathf.Sqrt(stop_forces.sqrMagnitude));
            float curv = Mathf.Sqrt(rigidbody.velocity.sqrMagnitude);
            float needdis = curv / 2 * ti;
            if (latestsaveddistance < dis || needdis >= dis) // 减速停止
            {
                m_workunions = stopMoveVelocity(rigidbody.velocity, Mathf.Sqrt(stop_forces.sqrMagnitude));
                move_state = MoveState.state_DirectMoveToTargetToStop;
            }
            else
            {
                latestsaveddistance = dis;
                Vector3 destDis = rigidbody.transform.InverseTransformDirection(destdirection);

                Vector3 localmaxspeed = GetMaxVector(destDis, maxspeed, invmaxspeed);
                if (rigidbody.velocity.sqrMagnitude <= localmaxspeed.sqrMagnitude)  // 没有到最大速度
                {
                    // 加力
                    Vector3 force = GetMaxVector(destDis, rigid_forces, rigid_invforces);
                    force = rigidbody.transform.TransformDirection(force);
                    BaseAction action = new ForcesAcceleration().Init(rigidbody, force);
                    ExecuteImp(action);
                }
            }
        }
        else if (move_state == MoveState.state_DirectMoveToTargetToStop)
        {
            if (!UpdateForces())
            {
                InitWorkUnion();
                move_state = MoveState.state_DirectMoveToTarget;
            }
        }
        // 到达目标点
        else if (move_state == MoveState.state_DirectMoveToTarget)
        {
            rigidbody.velocity = Vector3.zero;

            move_state = MoveState.state_end;
            if (syntony)
                behaviorcontroller.OnBehaviorComplete(behaviortype);
        }
    }
}

// BT_SmoothMoveToTarget,    // 平滑移动到目标点
public class SmoothMoveToTargetBehavior : WorkUnionBehavior
{
    public enum MoveState
    {
        state_org,                      // 初始态，有线速度和角速度
        state_parallel,                 // 到达转向平行态
        state_paralleltostop,           // 平行到垂直于平行方向无速度
        state_stop,                     // 达到无垂直速度
        state_stoptomoveline,           // 平行态无垂直速度到既定移动线
        state_moveline,                 // 到达移动线上 
        state_movelinetostop,           // 速度方向相反，则停止
        state_movelinetodecelerate,     // 从移动线往目标点走到减速态    
        state_deceleratetotarget,       // 从移动线往目标点走    
        state_target,                   // 到达目标点
        state_2flat,
        state_flat,
        state_end,              
    }

    UpTurnToTargetBehavior turntorargetBehavior = new UpTurnToTargetBehavior();
    TurningMoveBehavior turnMoveBehavior = new TurningMoveBehavior();
    StopMoveBehavior stopmoveBehavior = new StopMoveBehavior();

    //4DOF运动模型
    WorldForceControlMoveBehavior worldforcemoveBehavior = new WorldForceControlMoveBehavior();

    Vector3 rigid_forces;       // 飞船+X+Y+Z推力
    Vector3 rigid_invforces;    // 飞船-X-Y-Z推力
    Vector3 rigid_stopforces;   // 飞船减速阻力

    Vector3 rigid_torque;       // 飞船扭矩
    Vector3 rigid_stoptorque;   // 飞船扭矩

    Vector3 final_target;       // 最终目标
    Vector3 destDir;

    Vector3 maxspeed;             // 最大线速度
    Vector3 maxinvspeed;          // 最大线速度
    Vector3 maxanglespeed;        // 最大角速度

    public MoveState move_state;  // 飞船当前状态

	float latestsaveddistance;

    public override void OnDragWorldPosition(Vector3 drag_distance)
    {
        final_target = final_target + drag_distance;
        turntorargetBehavior.OnDragWorldPosition(drag_distance);
    }

    public BaseBehavior Init(
                        BehaviorController controller,
                        float stamp,
                        float retain,

                        Rigidbody rigid,
                        PhysxMotionType motiontp,   // 飞船控制状态

                        Vector3 force,
                        Vector3 invforces,
                        Vector3 stopforce,

                        Vector3 torque,
                        Vector3 stoptorque,

                        Vector3 maxsp,
                        Vector3 maxinvsp,

                        Vector3 maxangsp,
                        Vector3 target,
                        bool syn = false
                        )
    {
        Init(controller, stamp, retain, syn);
        BehaviorType = BehaviorController.BehaviorType.BT_SmoothMoveToTarget;

        rigidbody = rigid;
        motiontype = motiontp;

        rigid_forces = force;
        rigid_invforces = invforces;
        rigid_stopforces = stopforce;

        rigid_torque = torque;
        rigid_stoptorque = stoptorque;

        maxspeed = maxsp;
        maxinvspeed = maxinvsp;

        maxanglespeed = maxangsp;
        final_target = target;
        destDir = target - rigid.transform.position;

        move_state = MoveState.state_org;

        
        turntorargetBehavior.Init(controller, stamp, retain, rigid, motiontp, torque, stoptorque, maxangsp, target);

        // 根据飞船属性，计算出最大转向速度
        Vector3 localdir = rigidbody.transform.InverseTransformDirection(destDir);
        float angle = Vector3.Angle(Vector3.forward, SafeNormalized(localdir)) / 180 * Mathf.PI;
		if (angle == 0.0f)
			maxsp.z = maxsp.z * 0.5f;
		else
			maxsp.z = Mathf.Min(maxsp.z * 0.5f, destDir.magnitude * 0.2f * maxangsp.magnitude / angle);

        turnMoveBehavior.Init(controller, stamp, retain, rigid, motiontp, force, stopforce, maxsp);

        return this;
    }

    public override void Execute()
    {
        if (motiontype == PhysxMotionType.Jet)
        {
            Execute6DOF();
        }
        else
        {
            Execute4DOF();
        }
    }

    public void Execute4DOF()
    {
        // 计算当前时间节点力的方向
        if (move_state == MoveState.state_org)
        {
            float tmpprecision = rigid_stopforces.magnitude * 0.02f;
            destDir = final_target - rigidbody.transform.position;

            if (destDir.sqrMagnitude < tmpprecision)
            {
                rigidbody.velocity = Vector3.zero;
                move_state = MoveState.state_target;
            }
            else
            {
                destDir = SafeNormalized(destDir);
                Vector3 orientations = SafeNormalized(new Vector3(destDir.x, 0, destDir.z));

                worldforcemoveBehavior.Init(behaviorcontroller, actionstamp, actionretain,
                                            rigidbody, motiontype, rigid_forces,
                                            rigid_invforces, rigid_stopforces, rigid_torque,
                                            rigid_stoptorque, maxspeed, maxinvspeed,
                                            maxanglespeed, destDir, orientations);

                Quaternion look = Quaternion.LookRotation(SafeNormalized(destDir));
                Quaternion invlook = Quaternion.Inverse(look);
                Vector3 vel = invlook * rigidbody.velocity;

                bool beinline = false;
                if (vel.x * vel.x < tmpprecision) vel.x = 0;
                if (vel.y * vel.y < tmpprecision) vel.y = 0;
                if (vel.x == 0 && vel.y == 0) beinline = true;

                vel = look * vel;
                rigidbody.velocity = vel;

                if (beinline == true)
                {
                    move_state = MoveState.state_movelinetodecelerate;
                    latestsaveddistance = (final_target - rigidbody.transform.position).magnitude;
                }
                else
                    worldforcemoveBehavior.Execute();
            }
            
        }
        else if (move_state == MoveState.state_movelinetodecelerate)
        {
            Vector3 disVec = final_target - rigidbody.transform.position;
            float dis = disVec.magnitude;
            float ti = stopMoveMinTime(rigidbody.velocity,
                                       rigid_stopforces.magnitude);
            float curv = rigidbody.velocity.magnitude;

            float needdis = curv / 2 * ti;
            float ang = Vector3.Angle(SafeNormalized(disVec), SafeNormalized(destDir));
            if ((latestsaveddistance < dis && Mathf.Abs(ang) > Mathf.PI / 2.0f)
                    || needdis >= dis)
            {
                worldforcemoveBehavior.Init(
                    behaviorcontroller, actionstamp, actionretain, rigidbody, motiontype,
                    rigid_forces, rigid_invforces, rigid_stopforces, rigid_torque,
                    rigid_stoptorque, maxspeed, maxinvspeed, maxanglespeed,
                    Vector3.zero, Vector3.zero);

                move_state = MoveState.state_deceleratetotarget;
            }
            else
            {
                worldforcemoveBehavior.Execute();
                latestsaveddistance = dis;
            }
        }
        else if (move_state == MoveState.state_deceleratetotarget)
        {
            float curv = rigidbody.velocity.magnitude;
            float angcurv = rigidbody.angularVelocity.magnitude;
            if (curv == 0 && angcurv == 0)
            {
                move_state = MoveState.state_target;
            }
            else
                worldforcemoveBehavior.Execute();
        }
        else if (move_state == MoveState.state_target)
        {
            //rigidbody.position = final_target;
            rigidbody.velocity = Vector3.zero;
            rigidbody.angularVelocity = Vector3.zero;

            move_state = MoveState.state_end;
            if (syntony)
            {
                behaviorcontroller.OnBehaviorComplete(behaviortype);
            }
        }
    }

    public void Execute6DOF()
    {
        // 飞船移动
        // 1 开始状态，操作停止当前速度
        if (move_state == MoveState.state_org)
        {
            turntorargetBehavior.Execute();
            turnMoveBehavior.Execute();

            // 如果飞船已经朝向目标，进入平行态运行模式
            if (turntorargetBehavior.rotate_state == 
                UpTurnToTargetBehavior.RotateState.state_end)
            {
                move_state = MoveState.state_parallel;
            }
        }
        // 3 旋转到达停止态，满足移动条件
        else if (move_state == MoveState.state_parallel)
        {
            // 飞船完成转向，简化飞行过程
            destDir = final_target - rigidbody.transform.position;

            Quaternion look = Quaternion.LookRotation(SafeNormalized(destDir));
            Quaternion invlook = Quaternion.Inverse(look);

            Vector3 vel = rigidbody.velocity;           // 世界坐标系坐标
            vel = invlook * vel;                        // 飞船朝向本地坐标系速度
            vel.z = 0;

            vel = look * vel;   // 世界坐标系，要停止的速度

            if (vel.sqrMagnitude > sqrprecision)
            {
                m_workunions = stopMoveVelocity(vel, rigid_stopforces.magnitude);
                move_state = MoveState.state_paralleltostop;
            }
            else
                move_state = MoveState.state_stop;
        }
        else if (move_state == MoveState.state_paralleltostop)
        {
            if (!UpdateForces())
            {
                InitWorkUnion();
                move_state = MoveState.state_stop;
            }
        }
        else if (move_state == MoveState.state_stop)
        {
            // 飞船速度跟dir一致，移动到对应位置
            destDir = final_target - rigidbody.transform.position;

            Quaternion look = Quaternion.LookRotation(SafeNormalized(destDir));
            Quaternion invlook = Quaternion.Inverse(look);
            Vector3 vel = invlook * rigidbody.velocity;
            vel.x = 0; vel.y = 0;

            vel = look * vel;
            rigidbody.velocity = vel;

            turntorargetBehavior.Init(behaviorcontroller, actionstamp, actionretain, rigidbody,
                    motiontype, rigid_torque, rigid_stoptorque, maxanglespeed, final_target);

            // 判断是否走过
            float ang = Vector3.Angle(SafeNormalized(vel), SafeNormalized(destDir));
            if (Mathf.Abs(ang) > Mathf.PI / 2.0f)
            {
                // 方向相反
                move_state = MoveState.state_movelinetostop;

                stopmoveBehavior.Init(
                    behaviorcontroller,
                    actionstamp,
                    actionretain,
                    rigidbody,
                    motiontype,
                    rigid_stopforces);
            }
            else
            {
                move_state = MoveState.state_movelinetodecelerate;

				Vector3 curpos = rigidbody.transform.position;
				latestsaveddistance = (final_target - curpos).magnitude;
			}
        }
        else if (move_state == MoveState.state_movelinetostop)
        {
            stopmoveBehavior.Execute();
            if (stopmoveBehavior.move_state == StopMoveBehavior.MoveState.state_end)
            {
                move_state = MoveState.state_movelinetodecelerate;

				Vector3 curpos = rigidbody.transform.position;
                latestsaveddistance = (final_target - curpos).magnitude;
			}
        }
        else if (move_state == MoveState.state_movelinetodecelerate)
        {
            turntorargetBehavior.Execute();

            destDir = final_target - rigidbody.transform.position;

			Quaternion look = Quaternion.LookRotation(SafeNormalized(destDir));
			Quaternion invlook = Quaternion.Inverse(look);
			Vector3 vel = invlook * rigidbody.velocity;
			vel.x = 0;
			vel.y = 0;

			vel = look * vel;
			rigidbody.velocity = vel;


            Vector3 curpos = rigidbody.transform.position;
            float dis = (final_target - curpos).magnitude;

            float ti = stopMoveMinTime(rigidbody.velocity, rigid_stopforces.magnitude);
            float curv = rigidbody.velocity.magnitude;

            Vector3 localDir = rigidbody.transform.InverseTransformDirection(destDir);
            Vector3 curmaxspeed = GetMaxVector(localDir, maxspeed, maxinvspeed);


            float needdis = curv / 2 * ti;
            if (latestsaveddistance < dis || needdis >= dis)
            {
                m_workunions = stopMoveVelocity(rigidbody.velocity, rigid_stopforces.magnitude);
                move_state = MoveState.state_deceleratetotarget;
            }
            else if (curv <= curmaxspeed.magnitude)
            {
                Vector3 force = GetMaxVector(localDir, rigid_forces, rigid_invforces);
                force = rigidbody.transform.TransformDirection(force);

                BaseAction action = new ForcesAcceleration().Init(rigidbody, force);
                ExecuteImp(action);
            }
            else
            {
                rigidbody.velocity = rigidbody.transform.TransformDirection(curmaxspeed);
            }
            

			latestsaveddistance = dis;
		}
        else if (move_state == MoveState.state_deceleratetotarget)
        {
            turntorargetBehavior.Execute();

            if (!UpdateForces())
            {
                InitWorkUnion();
                move_state = MoveState.state_target;
            }
        }
        else if (move_state == MoveState.state_target)
        {
			rigidbody.position = final_target;
			rigidbody.velocity = Vector3.zero;

            move_state = MoveState.state_2flat;
        }
        else if (move_state == MoveState.state_2flat)
        {
            Vector3 wordforward = rigidbody.transform.TransformDirection(Vector3.forward);
            wordforward.y = 0;
            wordforward = SafeNormalized(wordforward);
            if (wordforward.sqrMagnitude < 0.01f) wordforward.z = 1;
            Vector3 target = rigidbody.transform.position + wordforward;

            turntorargetBehavior.Init(behaviorcontroller, actionstamp, actionretain,
                                      rigidbody, motiontype, rigid_torque,
                                      rigid_stoptorque, maxanglespeed, target);
            move_state = MoveState.state_flat;
        }
        else if (move_state == MoveState.state_flat)
        {
            turntorargetBehavior.Execute();
            if (turntorargetBehavior.rotate_state ==
                UpTurnToTargetBehavior.RotateState.state_end)
            {
                move_state = MoveState.state_end;

                if (syntony)
                    behaviorcontroller.OnBehaviorComplete(behaviortype);
            }
        }
    }
}

// BT_SmoothMoveTargets
public class SmoothMoveTargetsBehavior : WorkUnionBehavior
{
    public enum MoveState
    {
        state_org,
        state_moving,
        state_end,
    }

    SmoothMoveToTargetBehavior totargetbehavior = new SmoothMoveToTargetBehavior();

    Vector3 rigid_forces;       // 飞船+X+Y+Z推力
    Vector3 rigid_invforces;    // 飞船-X-Y-Z推力
    Vector3 rigid_stopforces;   // 飞船减速阻力

    Vector3 rigid_torque;       // 飞船扭矩
    Vector3 rigid_stoptorque;   // 飞船扭矩

    List<Vector3> final_targets; // 最终目标
    Vector3 destDir;

    Vector3 maxspeed;             // 最大线速度
    Vector3 maxinvspeed;          // 最大线速度
    Vector3 maxanglespeed;        // 最大角速度

    MoveState move_state;         // 飞船当前状态

    public override void OnDragWorldPosition(Vector3 drag_distance)
    {
        for (int i = 0; i < final_targets.Count; i++)
        {
            final_targets[i] = final_targets[i] + drag_distance;
        }
        if(totargetbehavior != null)
        {
            totargetbehavior.OnDragWorldPosition(drag_distance);
        }
    }

    public BaseBehavior Init(
                        BehaviorController controller,
                        float stamp,
                        float retain,

                        Rigidbody rigid,
                        PhysxMotionType motiontp,   // 飞船控制状态

                        Vector3 force,
                        Vector3 invforces,
                        Vector3 stopforce,

                        Vector3 torque,
                        Vector3 stoptorque,

                        Vector3 maxsp,
                        Vector3 maxinvsp,

                        Vector3 maxangsp,
                        List<Vector3> targets,
                        bool syn = false
                        )
    {
        Init(controller, stamp, retain, syn);
        BehaviorType = BehaviorController.BehaviorType.BT_SmoothMoveTargets;

        rigidbody = rigid;
        motiontype = motiontp;

        rigid_forces = force;
        rigid_invforces = invforces;
        rigid_stopforces = stopforce;

        rigid_torque = torque;
        rigid_stoptorque = stoptorque;

        maxspeed = maxsp;
        maxinvspeed = maxinvsp;

        maxanglespeed = maxangsp;
        final_targets = targets;

        move_state = MoveState.state_org;

        return this;
    }

    public override void Execute()
    {
        if (move_state == MoveState.state_org)
        {
            if (final_targets.Count == 0)
                move_state = MoveState.state_end;
            else
            {
                Vector3 target = final_targets[0];
                
                totargetbehavior.Init(behaviorcontroller,  actionstamp, actionretain, rigidbody, motiontype,
                                    rigid_forces, rigid_invforces, rigid_stopforces,
                                    rigid_torque, rigid_stoptorque,
                                    maxspeed, maxinvspeed,
                                    maxanglespeed, target);


                move_state = MoveState.state_moving;
                final_targets.RemoveAt(0);
            }
            
        }
        else if (move_state == MoveState.state_moving)
        {
            if (totargetbehavior.move_state >= SmoothMoveToTargetBehavior.MoveState.state_deceleratetotarget)
            {
                // 进入该目标点减速阶段,换目标点
                if (final_targets.Count != 0)
                {
                    Vector3 target = final_targets[0];
                    totargetbehavior.Init(behaviorcontroller, actionstamp, actionretain, rigidbody, motiontype,
                                        rigid_forces, rigid_invforces, rigid_stopforces,
                                        rigid_torque, rigid_stoptorque,
                                        maxspeed, maxinvspeed,
                                        maxanglespeed, target);

                    final_targets.RemoveAt(0);
                }
                else
                {
                    totargetbehavior.Execute();
                    if (totargetbehavior.move_state == SmoothMoveToTargetBehavior.MoveState.state_end)
                    {
                        move_state = MoveState.state_end;
                        if (syntony)
                            behaviorcontroller.OnBehaviorComplete(behaviortype);
                    }
                }
            }
            else
            {
                totargetbehavior.Execute();
            }
        }
    }
}

// BT_Leap
public class LeapBehavior : WorkUnionBehavior
{
    public enum LeapState
    {
        state_org,
        state_org2stop,
        state_stop,
        state_stop2target,
        state_target,
        state_targetwaitfinish,
        state_target2leap,
        state_leaped,
        state_leap2flat,
        state_flat,
        state_end,
    };

	StopRotateBehavior stop_rotate_behavior = new StopRotateBehavior();        // 停止转向行为
    StopMoveBehavior stop_move_behavior = new StopMoveBehavior();            // 停止移动行为
    TurnToTargetBehavior turn_to_target_behavior = new TurnToTargetBehavior();   // 转向跃迁点行为
    UpTurnToTargetBehavior up_turn_to_target_behavior = new UpTurnToTargetBehavior();

    Vector3 rigid_forces;               // 飞船+X+Y+Z推力
    Vector3 rigid_invforces;            // 飞船-X-Y-Z推力
    Vector3 rigid_stopforces;           // 飞船减速阻力

    Vector3 rigid_torque;               // 飞船扭矩
    Vector3 rigid_stoptorque;           // 飞船扭矩

    List<Vector3> final_targets;        // 最终目标

    float target_wait_time;
    float leap_prepare_over_time;

    Vector3 maxspeed;                   // 最大线速度
    Vector3 maxinvspeed;                // 最大线速度
    Vector3 maxanglespeed;              // 最大角速度

    float rigid_leapforce;              // 跃迁加速度
    float rigid_leapstopforce;          // 跃迁减速度

	LeapState leap_state;               // 飞船当前状态

    Vector3 dest_dir;
    Vector3 dest_dir_normal;
    Vector3 rigid_org_position;
    float distance1, distance2, distance3;
    float max_vel;
    Vector3 max_vel_vec3;

    public override void OnDragWorldPosition(Vector3 drag_distance)
    {
        rigid_org_position = rigid_org_position + drag_distance;

        for (int i = 0; i < final_targets.Count; i++)
        {
            final_targets[i] = final_targets[i] + drag_distance;
        }

        if (stop_rotate_behavior != null)
            stop_rotate_behavior.OnDragWorldPosition(drag_distance);
        if (stop_move_behavior != null)
            stop_move_behavior.OnDragWorldPosition(drag_distance);
        if (turn_to_target_behavior != null)
            turn_to_target_behavior.OnDragWorldPosition(drag_distance);
        if (turn_to_target_behavior != null)
            up_turn_to_target_behavior.OnDragWorldPosition(drag_distance);
    }

    public BaseBehavior Init(
                        BehaviorController controller,
                        float stamp,
                        float retain,

                        Rigidbody rigid,
                        PhysxMotionType motiontp,

                        Vector3 force,
                        Vector3 invforces,
                        Vector3 stopforce,

                        Vector3 torque,
                        Vector3 stoptorque,

                        Vector3 maxsp,
                        Vector3 maxinvsp,

                        Vector3 maxangsp,

                        float leapforce,
                        float leapstopforce,

                        List<Vector3> targets,

                        float waittime,

                        bool syn = false)
    {
        Init(controller, stamp, retain, syn);
        BehaviorType = BehaviorController.BehaviorType.BT_Leap;

        rigidbody = rigid;
        motiontype = motiontp;

        rigid_forces = force;
        rigid_invforces = invforces;
        rigid_stopforces = stopforce;

        rigid_torque = torque;
        rigid_stoptorque = stoptorque;

        maxspeed = maxsp;
        maxinvspeed = maxinvsp;

        maxanglespeed = maxangsp;
        final_targets = targets;

        target_wait_time = waittime;
        leap_prepare_over_time = 0;

        rigid_leapforce = leapforce;
        rigid_leapstopforce = leapstopforce;

        leap_state = LeapState.state_org;

        return this;
    }

    public override void Execute()
    {
        // 0 初始态
        if (leap_state == LeapState.state_org)
        {

            stop_rotate_behavior.Init(behaviorcontroller, actionstamp, actionretain, rigidbody, rigid_stoptorque);
            stop_move_behavior.Init(behaviorcontroller, actionstamp, actionretain, rigidbody, motiontype, rigid_stopforces);

            leap_state = LeapState.state_org2stop;
        }
        // 1 停止旋转&移动
        else if (leap_state == LeapState.state_org2stop)
        {
            stop_rotate_behavior.Execute();
            stop_move_behavior.Execute();

            if (stop_rotate_behavior.rotate_state == StopRotateBehavior.RotateState.state_end &&
                stop_move_behavior.move_state == StopMoveBehavior.MoveState.state_end)
            {
                // 完成停止
                leap_state = LeapState.state_stop;

                rigidbody.velocity = Vector3.zero;
                rigidbody.angularVelocity = Vector3.zero;
            }
        }
        // 2 完成停止
        else if (leap_state == LeapState.state_stop)
        {
            up_turn_to_target_behavior.Init(behaviorcontroller, actionstamp, actionretain, rigidbody, motiontype,
                rigid_torque, rigid_stoptorque, maxanglespeed, final_targets[final_targets.Count - 1]);

            leap_state = LeapState.state_stop2target;
        }
        // 3 转向目标
        else if (leap_state == LeapState.state_stop2target)
        {
            up_turn_to_target_behavior.Execute();
            if (up_turn_to_target_behavior.rotate_state == UpTurnToTargetBehavior.RotateState.state_end)
            {
                // 完成转向
                leap_state = LeapState.state_targetwaitfinish;
                behaviorcontroller.Callback.Invoke("forward_to_the_target");
                leap_prepare_over_time = Time.fixedTime;

                rigidbody.angularVelocity = Vector3.zero;
            }
        }
        // 3.1 等待阶段
        else if (leap_state == LeapState.state_targetwaitfinish)
        {
            if (Time.fixedTime - leap_prepare_over_time > target_wait_time)
            {
                behaviorcontroller.Callback.Invoke("target_wait_finish");
                leap_state = LeapState.state_target;
            }
        }
        // 4 完成转向，准备跃迁
        else if (leap_state == LeapState.state_target)
        {
            // 计算最大跃迁速度，预估时间
            dest_dir = final_targets[final_targets.Count - 1] - rigidbody.position;
            dest_dir_normal = SafeNormalized(dest_dir);

            float distance = dest_dir.magnitude;
            float time = Mathf.Atan(distance / 10000) * 5;

            float delt = rigid_leapforce * time * rigid_leapforce * time
                - 4 * (rigid_leapforce / 2.0f + rigid_leapforce * rigid_leapforce / (2 * rigid_leapstopforce)) * distance;
            float t1 = 0, t2 = 0, t3 = 0;
            if (delt > 0)
            {
                // 可以加速到最大速度，过程分为三段
                t1 = (rigid_leapforce * time + Mathf.Sqrt(delt))
                    / (rigid_leapforce + rigid_leapforce * rigid_leapforce / rigid_leapstopforce);
                t2 = time - (rigid_leapforce / rigid_leapstopforce + 1) * t1;
                t3 = rigid_leapforce * t1 / rigid_leapstopforce;

                if (t2 < 0)
                {
                    t1 = (rigid_leapforce * time - Mathf.Sqrt(delt))
                    / (rigid_leapforce + rigid_leapforce * rigid_leapforce / rigid_leapstopforce);
                    t2 = time - (rigid_leapforce / rigid_leapstopforce + 1) * t1;
                    t3 = rigid_leapforce * t1 / rigid_leapstopforce;
                }
            }
            else
            {
                // 不能加速到最大速度，过程分为两段
                t1 = Mathf.Sqrt(distance / (rigid_leapforce / 2 + rigid_leapforce * rigid_leapforce / (2 * rigid_leapstopforce)));
                t2 = 0;
                t3 = rigid_leapforce * t1 / rigid_leapstopforce;
                time = t1 + t2 + t3;
            }

            WorkUnion resWorkUnion1, resWorkUnion2, resWorkUnion3;
            resWorkUnion1.work_worktime = t1;
            resWorkUnion1.work_starttime = Time.fixedTime;
            resWorkUnion1.work_forcemode = ForceMode.Acceleration;
            resWorkUnion1.work_current = new Vector3(dest_dir_normal.x * rigid_leapforce, dest_dir_normal.y * rigid_leapforce, dest_dir_normal.z * rigid_leapforce);
            max_vel = rigid_leapforce * t1;
            distance1 = max_vel * t1 / 2;
            max_vel_vec3 = resWorkUnion1.work_current;
            max_vel_vec3.Scale(new Vector3(t1, t1, t1));



            resWorkUnion2.work_worktime = t2;
            resWorkUnion2.work_starttime = resWorkUnion1.work_starttime + resWorkUnion1.work_worktime;
            resWorkUnion2.work_forcemode = ForceMode.Acceleration;
            resWorkUnion2.work_current = Vector3.zero;
            distance2 = max_vel * t2;

            resWorkUnion3.work_worktime = t3;
            resWorkUnion3.work_starttime = resWorkUnion2.work_starttime + resWorkUnion2.work_worktime;
            resWorkUnion3.work_forcemode = ForceMode.Acceleration;
            resWorkUnion3.work_current = new Vector3(-dest_dir_normal.x * rigid_leapstopforce, -dest_dir_normal.y * rigid_leapstopforce, -dest_dir_normal.z * rigid_leapstopforce);
            distance3 = max_vel * t3 / 2;

            m_workunions.Add(resWorkUnion1);
            m_workunions.Add(resWorkUnion2);
            m_workunions.Add(resWorkUnion3);

            leap_state = LeapState.state_target2leap;

            rigidbody.rotation = Quaternion.LookRotation(dest_dir);
            rigid_org_position = rigidbody.position;
        }
        // 5 开启跃迁
        else if (leap_state == LeapState.state_target2leap)
        {
            // 直接计算position，赋值
            if (Time.fixedTime > m_workunions[2].work_starttime + m_workunions[2].work_worktime)
            {
                // 完成跃迁
                float dis = distance1 + distance2 + distance3;
                rigidbody.position = rigid_org_position
                    + new Vector3(dest_dir_normal.x * dis, dest_dir_normal.y * dis, dest_dir_normal.z * dis);
                rigidbody.rotation = Quaternion.LookRotation(dest_dir);
                rigidbody.angularVelocity = Vector3.zero;
                rigidbody.velocity = Vector3.zero;

                leap_state = LeapState.state_leaped;
            }
            else if (Time.fixedTime <= m_workunions[2].work_starttime + m_workunions[2].work_worktime
                && Time.fixedTime > m_workunions[1].work_starttime + m_workunions[1].work_worktime)
            {
                // 在第三阶段
                float dis_time = m_workunions[2].work_starttime + m_workunions[2].work_worktime - Time.fixedTime;
                float dis = distance1 + distance2 + distance3
                    - dis_time * dis_time * rigid_leapstopforce / 2;

                rigidbody.position = rigid_org_position
                    + new Vector3(dest_dir_normal.x * dis, dest_dir_normal.y * dis, dest_dir_normal.z * dis);
                rigidbody.rotation = Quaternion.LookRotation(dest_dir);
                rigidbody.angularVelocity = Vector3.zero;
                rigidbody.velocity = Vector3.zero;

                float atm = Time.fixedTime - m_workunions[2].work_starttime;
                Vector3 curvel = max_vel_vec3 - (new Vector3(m_workunions[2].work_current.x * atm
                                                , m_workunions[2].work_current.y * atm
                                                , m_workunions[2].work_current.z * atm));
                curvel.Scale(new Vector3(0.00001f, 0.00001f, 0.00001f));
                rigidbody.velocity = curvel;
            }
            else if (Time.fixedTime <= m_workunions[1].work_starttime + m_workunions[1].work_worktime
                && Time.fixedTime > m_workunions[0].work_starttime + m_workunions[0].work_worktime)
            {
                // 在第二阶段
                float dis = distance1 + distance2
                    - (m_workunions[1].work_starttime + m_workunions[1].work_worktime - Time.fixedTime) * max_vel;

                rigidbody.position = rigid_org_position
                    + new Vector3(dest_dir_normal.x * dis, dest_dir_normal.y * dis, dest_dir_normal.z * dis);
                rigidbody.rotation = Quaternion.LookRotation(dest_dir);
                rigidbody.angularVelocity = Vector3.zero;
                //rigidbody.velocity = Vector3.zero;
                Vector3 curvel = max_vel_vec3;
                curvel.Scale(new Vector3(0.00001f, 0.00001f, 0.00001f));
                rigidbody.velocity = curvel;
            }
            else
            {
                // 在第一阶段
                float dis = (Time.fixedTime - m_workunions[0].work_starttime)
                    * (Time.fixedTime - m_workunions[0].work_starttime)
                    * rigid_leapforce / 2;

                rigidbody.position = rigid_org_position
                    + new Vector3(dest_dir_normal.x * dis, dest_dir_normal.y * dis, dest_dir_normal.z * dis);
                rigidbody.rotation = Quaternion.LookRotation(dest_dir);
                rigidbody.angularVelocity = Vector3.zero;
                //rigidbody.velocity = Vector3.zero;

                float atm = Time.fixedTime - m_workunions[0].work_starttime;
                Vector3 curvel = new Vector3(m_workunions[0].work_current.x * atm
                                                , m_workunions[0].work_current.y * atm
                                                , m_workunions[0].work_current.z * atm);
                curvel.Scale(new Vector3(0.00001f, 0.00001f, 0.00001f));
                rigidbody.velocity = curvel;
            }

        }
        // 6 完成跃迁
        else if (leap_state == LeapState.state_leaped)
        {
            dest_dir.y = 0;
            Vector3 target = rigidbody.position + dest_dir;
            turn_to_target_behavior.Init(behaviorcontroller, actionstamp, actionretain, rigidbody, motiontype,
                rigid_torque, rigid_stoptorque, maxanglespeed, target);

            leap_state = LeapState.state_leap2flat;
            behaviorcontroller.Callback.Invoke("state_leap2flat");
        }
        // 7 开启飞船放平操作
        else if (leap_state == LeapState.state_leap2flat)
        {
            turn_to_target_behavior.Execute();

            if (turn_to_target_behavior.rotate_state == TurnToTargetBehavior.RotateState.state_end)
            {
                // 完成转向
                leap_state = LeapState.state_flat;
                rigidbody.angularVelocity = Vector3.zero;
            }
        }
        // 8 完成飞船方平
        else if (leap_state == LeapState.state_flat)
        {
            leap_state = LeapState.state_end;

            // 设置角度
            Vector3 forward = rigidbody.transform.TransformDirection(Vector3.forward);
            Vector3 euler_forward = GetEuler(forward);
            euler_forward.x = 0; euler_forward.z = 0;
            Quaternion quat = new Quaternion();
            quat.eulerAngles = euler_forward;
            rigidbody.transform.rotation = quat;

            if (syntony)
                behaviorcontroller.OnBehaviorComplete(behaviortype);
        }
    }
}

// BT_LeapBreak
public class LeapBreakBehavior : WorkUnionBehavior
{
    public enum LeapBreakState
    {
        state_org,
        state_org2stop,
        state_stop,
        state_stop2flat,
        state_flat,
        state_end,
    };

    StopRotateBehavior      stop_rotate_behavior = new StopRotateBehavior();        // 停止转向行为
    StopMoveBehavior        stop_move_behavior = new StopMoveBehavior();            // 停止移动行为
    TurnToTargetBehavior    turn_to_target_behavior = new TurnToTargetBehavior();   // 转向跃迁点行为

    Vector3 dest_dir;

    Vector3 rigid_forces;               // 飞船+X+Y+Z推力
    Vector3 rigid_invforces;            // 飞船-X-Y-Z推力
    Vector3 rigid_stopforces;           // 飞船减速阻力

    Vector3 rigid_torque;               // 飞船扭矩
    Vector3 rigid_stoptorque;           // 飞船扭矩

    Vector3 maxspeed;                   // 最大线速度
    Vector3 maxinvspeed;                // 最大线速度
    Vector3 maxanglespeed;              // 最大角速度

    LeapBreakState leapbreak_state;				// 飞船当前状态

    public override void OnDragWorldPosition(Vector3 drag_distance)
    {
        if (stop_rotate_behavior != null)
            stop_rotate_behavior.OnDragWorldPosition(drag_distance);
        if (stop_move_behavior != null)
            stop_move_behavior.OnDragWorldPosition(drag_distance);
        if (turn_to_target_behavior != null)
            turn_to_target_behavior.OnDragWorldPosition(drag_distance);
    }

    public BaseBehavior Init(
                        BehaviorController controller,
                        float stamp,
                        float retain,

                        Rigidbody rigid,
                        PhysxMotionType motiontp,

                        Vector3 force,
                        Vector3 invforces,
                        Vector3 stopforce,

                        Vector3 torque,
                        Vector3 stoptorque,

                        Vector3 maxsp,
                        Vector3 maxinvsp,

                        Vector3 maxangsp,

                        bool syn = false)
    {
        Init(controller, stamp, retain, syn);
        BehaviorType = BehaviorController.BehaviorType.BT_LeapBreak;

        rigidbody = rigid;
        motiontype = motiontp;

        rigid_forces = force;
        rigid_invforces = invforces;
        rigid_stopforces = stopforce;

        rigid_torque = torque;
        rigid_stoptorque = stoptorque;

        maxspeed = maxsp;
        maxinvspeed = maxinvsp;

        maxanglespeed = maxangsp;

        leapbreak_state = LeapBreakState.state_org;

        return this;
    }

    public override void Execute()
    {
        // 0 初始态
        if (leapbreak_state == LeapBreakState.state_org)
        {
            stop_rotate_behavior.Init(behaviorcontroller, actionstamp, actionretain, rigidbody, rigid_stoptorque);
            stop_move_behavior.Init(behaviorcontroller, actionstamp, actionretain, rigidbody, motiontype, rigid_stopforces);
            leapbreak_state = LeapBreakState.state_org2stop;
        }
        // 1 停止旋转&移动
        else if (leapbreak_state == LeapBreakState.state_org2stop)
        {
            stop_rotate_behavior.Execute();
            stop_move_behavior.Execute();

            if (stop_rotate_behavior.rotate_state == StopRotateBehavior.RotateState.state_end &&
                stop_move_behavior.move_state == StopMoveBehavior.MoveState.state_end)
            {
                // 完成停止
                leapbreak_state = LeapBreakState.state_stop;

                rigidbody.velocity = Vector3.zero;
                rigidbody.angularVelocity = Vector3.zero;
            }
        }
        // 2 完成停止
        else if (leapbreak_state == LeapBreakState.state_stop)
        {
            Vector3 forword = rigidbody.rotation * Vector3.forward;
            forword.y = 0;

            Vector3 target = rigidbody.position + forword;
            turn_to_target_behavior.Init(behaviorcontroller, actionstamp, actionretain, rigidbody, motiontype,
                rigid_torque, rigid_stoptorque, maxanglespeed, target);

            leapbreak_state = LeapBreakState.state_stop2flat;
        }
        // 3 开启飞船放平操作
        else if (leapbreak_state == LeapBreakState.state_stop2flat)
        {
            turn_to_target_behavior.Execute();

            if (turn_to_target_behavior.rotate_state == TurnToTargetBehavior.RotateState.state_end)
            {
                // 完成转向
                leapbreak_state = LeapBreakState.state_flat;
                rigidbody.angularVelocity = Vector3.zero;
            }
        }
        // 8 完成飞船方平
        else if (leapbreak_state == LeapBreakState.state_flat)
        {
            leapbreak_state = LeapBreakState.state_end;

            // 设置角度
            Vector3 forward = rigidbody.transform.TransformDirection(Vector3.forward);
            Vector3 euler_forward = GetEuler(forward);
            euler_forward.x = 0; euler_forward.z = 0;
            Quaternion quat = new Quaternion();
            quat.eulerAngles = euler_forward;
            rigidbody.transform.rotation = quat;

            if (syntony)
                behaviorcontroller.OnBehaviorComplete(behaviortype);
        }
    }
}

// BT_ForwardCtlMove
public class ForwardControlMoveBehavior : WorkUnionBehavior
{
    Vector3 rigid_forces;       // 飞船+X+Y+Z推力
    Vector3 rigid_invforces;    // 飞船-X-Y-Z推力
    Vector3 rigid_stopforces;   // 飞船减速阻力

    Vector3 rigid_torque;       // 飞船扭矩
    Vector3 rigid_stoptorque;   // 飞船扭矩

    Vector3 maxspeed;           // 最大线速度
    Vector3 maxinvspeed;        // 最大线速度
    Vector3 maxanglespeed;      // 最大角速度

    Vector3 rigid_forcelocal;
    Vector3 rigid_orientations;

    FourcesLocalBehavior fources_local_behavior = new FourcesLocalBehavior();

    public BaseBehavior Init(
                        BehaviorController controller,
                        float stamp,
                        float retain,

                        Rigidbody rigid,
                        PhysxMotionType motiontp,

                        Vector3 force,
                        Vector3 invforces,
                        Vector3 stopforce,

                        Vector3 torque,
                        Vector3 stoptorque,

                        Vector3 maxsp,
                        Vector3 maxinvsp,

                        Vector3 maxangsp,

                        Vector3 forcelocal,
                        Vector3 orientations,

                        bool syn = false)
    {
        Init(controller, stamp, retain, syn);
        BehaviorType = BehaviorController.BehaviorType.BT_ForwardCtlMove;

        rigidbody = rigid;
        motiontype = motiontp;

        rigid_forces = force;
        rigid_invforces = invforces;
        rigid_stopforces = stopforce;

        rigid_torque = torque;
        rigid_stoptorque = stoptorque;

        maxspeed = maxsp;
        maxinvspeed = maxinvsp;

        maxanglespeed = maxangsp;

        rigid_forcelocal = forcelocal;
        rigid_orientations = orientations;

        fources_local_behavior.Init(controller, stamp, retain, rigid,
            forcelocal, stopforce, maxsp, maxinvsp, ForceMode.Acceleration);

        return this;
    }

    public override void Execute()
    {
        // 1 运动信息
        fources_local_behavior.Execute();

        // 2 旋转信息
        // 本地坐标系，转向目标
        Vector3 cur_toqure = Vector3.zero;
        Vector3 tmpDir = rigidbody.transform.InverseTransformDirection(rigid_orientations);
        float angle = Vector3.Angle(Vector3.forward, SafeNormalized(tmpDir)) / 180 * Mathf.PI;
        Vector3 vel = rigidbody.transform.InverseTransformDirection(rigidbody.angularVelocity);

        if (angle * angle <= sqrprecision && vel.sqrMagnitude <= sqrprecision)
            rigidbody.angularVelocity = Vector3.zero;
        else
        {
            Vector3 from = Vector3.forward;
            Vector3 torquedir = Vector3.Cross(from, tmpDir);
            Vector3 tmpscale = SafeNormalized(torquedir);
            if (tmpscale.sqrMagnitude < 0.1f) tmpscale.y = 1.0f;

            // 方向不一致
            // 停车角度大于等于当前角度
            if (vel.y * torquedir.y < 0 ||
                vel.y * vel.y / (2 * rigid_stoptorque.magnitude) >= angle)
            {
                cur_toqure.y = -vel.y / Mathf.Abs(vel.y) * rigid_stoptorque.magnitude;
            }
            else if (vel.y * vel.y < maxanglespeed.y * maxanglespeed.y)
            {
                // 没有到最大角速度
                cur_toqure.y = tmpscale.y * rigid_torque.magnitude;
            }

            BaseAction action2 = new TorqueAcceleration().Init(rigidbody, rigidbody.transform.TransformDirection(cur_toqure));
            ExecuteImp(action2);

        }
    }
}

// BT_WorldForceCtlMove
public class WorldForceControlMoveBehavior : WorkUnionBehavior
{
    Vector3 rigid_forces;       // 飞船+X+Y+Z推力
    Vector3 rigid_invforces;    // 飞船-X-Y-Z推力
    Vector3 rigid_stopforces;   // 飞船减速阻力

    Vector3 rigid_torque;       // 飞船扭矩
    Vector3 rigid_stoptorque;   // 飞船扭矩

    Vector3 maxspeed;           // 最大线速度
    Vector3 maxinvspeed;        // 最大线速度
    Vector3 maxanglespeed;      // 最大角速度

    Vector3 rigid_forceworld;
    Vector3 rigid_orientations;

    bool rigid_update_angle;

    float rigid_scale;

    float rigid_precision;
    float rigid_force_maxspeed;

    public BaseBehavior Init(
                        BehaviorController controller,
                        float stamp,
                        float retain,

                        Rigidbody rigid,
                        PhysxMotionType motiontp,

                        Vector3 force,
                        Vector3 invforces,
                        Vector3 stopforce,

                        Vector3 torque,
                        Vector3 stoptorque,

                        Vector3 maxsp,
                        Vector3 maxinvsp,

                        Vector3 maxangsp,

                        Vector3 forceworld,
                        Vector3 orientations,

                        bool syn = false)
    {
        Init(controller, stamp, retain, syn);
        BehaviorType = BehaviorController.BehaviorType.BT_WorldForceCtlMove;

        rigidbody = rigid;
        motiontype = motiontp;

        rigid_forces = force;
        rigid_invforces = invforces;
        rigid_stopforces = stopforce;

        rigid_torque = torque;
        rigid_stoptorque = stoptorque;

        maxspeed = maxsp;
        maxinvspeed = maxinvsp;
        maxanglespeed = maxangsp;

        rigid_forceworld = forceworld;
        forceworld.y = 0;
        maxspeed.z *= forceworld.magnitude;
        maxspeed.x = maxspeed.z;

        maxinvspeed.z *= forceworld.magnitude;
        maxinvspeed.x = maxinvspeed.z;

        rigid_scale = rigid_forceworld.magnitude;
        rigid_orientations = orientations;
        rigid_orientations.y = 0;

        rigid_update_angle = true;

        rigid_precision = rigid_stopforces.magnitude * 0.02f;

        Vector3 localDir = rigid_forceworld;
        localDir.y = 0;
        localDir.z = localDir.magnitude;
        localDir.x = 0;
        localDir.y = rigid_forceworld.y;

        rigid_force_maxspeed = GetMaxVector(localDir, maxspeed, maxinvspeed).magnitude;
        rigid_force_maxspeed = ((int)(rigid_force_maxspeed * 100)) / 100.0f;

        return this;
    }

    public override void Execute()
    {
        // 1 运动信息
        // 如果杆量信息小于sqrprecision，则只有阻尼信息生效
        Vector3 worldforces = Vector3.zero;
        Vector3 cur_toqure = Vector3.zero;
        // 根据当前状态，计算出飞船运行最终本地坐标系的力
        Vector3 worldvel = rigidbody.velocity;
        

        if (rigid_forceworld.sqrMagnitude < sqrprecision)
        {
            if (worldvel.sqrMagnitude > rigid_precision)
            {
                // 阻尼自动减速，力为速度的反向
                worldforces = SafeNormalized(worldvel) * (-rigid_stopforces.magnitude);
            }
            else
                worldvel = Vector3.zero;
        }
        else
        {
            // 当前有力
            Vector3 localforces = Vector3.zero;
            Quaternion look = Quaternion.LookRotation(SafeNormalized(rigid_forceworld));
            Quaternion invlook = Quaternion.Inverse(look);
            Vector3 tmpvel = invlook * worldvel;

            if (tmpvel.x * tmpvel.x > rigid_precision)
            {
                // 阻尼自动减速，力为速度的反向
                localforces.x =
                    -tmpvel.x * rigid_stopforces.magnitude / Mathf.Abs(tmpvel.x);
            }
            else
                tmpvel.x = 0;

            if (tmpvel.y * tmpvel.y > rigid_precision)
            {
                // 阻尼自动减速，力为速度的反向
                localforces.y = -tmpvel.y * rigid_stopforces.magnitude / Mathf.Abs(tmpvel.y);
            }
            else
                tmpvel.y = 0;

            if (tmpvel.z < 0.0f)
            {
                // 阻尼自动减速，力为速度的反向
                localforces.z = rigid_stopforces.magnitude;
            }

            if (tmpvel.z >= rigid_force_maxspeed)
            {
                if (tmpvel.z > rigid_force_maxspeed + rigid_precision)
                    localforces.z = -rigid_stopforces.magnitude;
                else
                    tmpvel.z = rigid_force_maxspeed;
            }
            else
            {
                // 生效推进力
                Vector3 forcelocal =
                    rigidbody.transform.InverseTransformDirection(rigid_forceworld);
                float forwordz =
                    GetMaxVector(forcelocal, rigid_forces, rigid_invforces).magnitude *
                    rigid_scale;
                localforces.z += forwordz;
            }
            worldvel = look * tmpvel;
            worldforces = look * localforces;
        }

        rigidbody.velocity = worldvel;

        BaseAction action1 = new ForcesAcceleration().Init(rigidbody, worldforces);
        ExecuteImp(action1);

        // 2 旋转信息
        // 本地坐标系，转向目标
        
        Vector3 tmpDir = rigidbody.transform.InverseTransformDirection(rigid_orientations);
        float angle = Vector3.Angle(Vector3.forward, SafeNormalized(tmpDir)) / 180 * Mathf.PI;
        Vector3 vel = rigidbody.transform.InverseTransformDirection(rigidbody.angularVelocity);

        if (rigid_update_angle && !rigidbody.freezeRotation) // 是否更新角度信息        
        {
            if (angle * angle <= sqrprecision && vel.sqrMagnitude <= sqrprecision)
            {
                rigidbody.angularVelocity = Vector3.zero;
                rigid_update_angle = false;

                if (rigid_orientations.sqrMagnitude > 0.01f)
                {
                    // 设置朝向
                    Vector3 euler = GetEuler(rigid_orientations);
                    Quaternion quat = new Quaternion();
                    quat.eulerAngles = euler;
                    rigidbody.transform.rotation = quat;
                }
            }
            else
            {
                Vector3 from = Vector3.forward;
                Vector3 torquedir = Vector3.Cross(from, tmpDir);
                Vector3 tmpscale = SafeNormalized(torquedir);
                if (tmpscale.sqrMagnitude < 0.1f) tmpscale.y = 1.0f;

                // 方向不一致
                // 停车角度大于等于当前角度
                if (vel.y * torquedir.y < 0 ||
                    vel.y * vel.y / (2 * rigid_stoptorque.y) >= angle)
                {
                    cur_toqure.y = -vel.y / Mathf.Abs(vel.y) * rigid_stoptorque.y;
                }
                else if (vel.y * vel.y < maxanglespeed.y * maxanglespeed.y)
                {
                    // 没有到最大角速度
                    cur_toqure.y = tmpscale.y * rigid_torque.y;
                }

                BaseAction action2 = new TorqueAcceleration().Init(rigidbody, rigidbody.transform.TransformDirection(cur_toqure));
                ExecuteImp(action2);
            }
        }

        if (rigidbody.velocity.Equals(Vector3.zero) &&
            rigidbody.angularVelocity.Equals(Vector3.zero) &&
            worldforces.Equals(Vector3.zero) &&
            cur_toqure.Equals(Vector3.zero))
        {
            if (syntony)
            {
                // 触发运动停止事件
                behaviorcontroller.Callback.Invoke("rigidstop");
                behaviorcontroller.OnBehaviorComplete(behaviortype);
            }
        }
    }
}

//BT_BindMove           
public class BindMoveBehavior : WorkUnionBehavior
{
    Vector3 rigid_forces;       // 飞船+X+Y+Z推力
    Vector3 rigid_invforces;    // 飞船-X-Y-Z推力
    Vector3 rigid_stopforces;   // 飞船减速阻力

    Vector3 rigid_torque;       // 飞船扭矩
    Vector3 rigid_stoptorque;   // 飞船扭矩

    Vector3 maxspeed;           // 最大线速度
    Vector3 maxinvspeed;        // 最大线速度
    Vector3 maxanglespeed;      // 最大角速度

    Crucis.Protocol.BindMove bind_move;

    Rigidbody bind_rigidbody = null;
    float     bind_tmpprecision;

    Vector3   bind_offset;
    Vector3   cacu_target;
    bool      bind_berefresh;
    bool      bind_beimme;
    bool      bind_fire_catched;

    SmoothMoveToTargetBehavior smtt_behavior = new SmoothMoveToTargetBehavior();
    WorldForceControlMoveBehavior wfcm_behavior = new WorldForceControlMoveBehavior();

    public override void OnDragWorldPosition(Vector3 drag_distance)
    {
        if (!bind_beimme && !bind_berefresh)
        {
            cacu_target = cacu_target + drag_distance;
            if (smtt_behavior != null)
                smtt_behavior.OnDragWorldPosition(drag_distance);
        }
    }

    // 执行信息的参数 执行对象，IDLE动画名
    public BaseBehavior Init(
                        BehaviorController controller,
                        float stamp,
                        float retain,
                        Rigidbody rigid,

                        Vector3 force,
                        Vector3 invforces,
                        Vector3 stopforce,

                        Vector3 torque,
                        Vector3 stoptorque,

                        Vector3 maxsp,
                        Vector3 maxinvsp,
                        Vector3 maxangsp,

                        Crucis.Protocol.BindMove bindmove,

                        //RespondMoveEvent respondMove,

                        //Rigidbody bindrigid,
                        //Vector3 offset,
                        //bool berefresh, 
                        //bool beimme,

                        bool syn = false)
    {
        Init(controller, stamp, retain, syn);
        BehaviorType = BehaviorController.BehaviorType.BT_BindMove;

        rigidbody = rigid;
        bind_move = bindmove;
        
        motiontype = PhysxMotionType.Spacecraft;

        rigid_forces = force;
        rigid_invforces = invforces;
        rigid_stopforces = stopforce;

        rigid_torque = torque;
        rigid_stoptorque = stoptorque;

        maxspeed = maxsp;
        maxinvspeed = maxinvsp;
        maxanglespeed = maxangsp;
        cacu_target = Vector3.zero;

        bind_fire_catched = true;

        bind_tmpprecision = maxspeed.z * 0.02f;

        return this;
    }

    private bool UpdateBindInfo()
    {
        if (bind_rigidbody == null)
        {
            GameplayProxy gameplay = GameFacade.Instance.RetrieveProxy(ProxyName.GameplayProxy) as GameplayProxy;
            SpacecraftEntity player = gameplay.GetEntityById<SpacecraftEntity>((uint)(bind_move.BindHeroId));
            if (player != null)
            {
                bind_rigidbody = player.GetRigidbody();
                bind_offset = bind_move.BindHeroOffset;

                bind_berefresh = bind_move.BindRefresh;
                bind_beimme = bind_move.BindEffectImme;

                if (!bind_beimme && !bind_berefresh)
                {
                    if (bind_rigidbody != null)
                    {
                        cacu_target = bind_rigidbody.position + bind_offset;
                        smtt_behavior.Init(behaviorcontroller, actionstamp, actionretain, rigidbody, motiontype, rigid_forces,
                                           rigid_invforces, rigid_stopforces, rigid_torque, rigid_stoptorque, maxspeed,
                                           maxinvspeed, maxanglespeed, cacu_target);
                    }
                }

                return true;
            }
            return false;
        }
        return true;
    }

    public override void Execute()
    {
        if (!UpdateBindInfo()) return;

        // 绑定移动
        if (rigidbody && bind_rigidbody)
        {
            if (bind_beimme)  // 瞬时刷新位置
            {
                rigidbody.position = bind_rigidbody.position + bind_offset;
                rigidbody.rotation = bind_rigidbody.rotation;
            }
            else if (bind_berefresh)  // 实时更新目标点对象位置
            {
                Vector3 update_target = bind_rigidbody.position + bind_offset;
                Vector3 update_dir = update_target - rigidbody.position;

                if (update_dir.magnitude < bind_tmpprecision)
                {
                    if (bind_fire_catched)
                    {
                        bind_fire_catched = false;
                        rigidbody.velocity = Vector3.zero;
                    }
                }
                else if(bind_fire_catched)
                {
                    update_dir = SafeNormalized(update_dir);
                    Vector3 orientations =
                        SafeNormalized(new Vector3(update_dir.x, 0, update_dir.z));

                    wfcm_behavior.Init(behaviorcontroller, actionstamp, actionretain,
                                       rigidbody, motiontype, rigid_forces, rigid_invforces,
                                       rigid_stopforces, rigid_torque, rigid_stoptorque,
                                       maxspeed, maxinvspeed, maxanglespeed, update_dir,
                                       orientations);

                    Quaternion look = Quaternion.LookRotation(SafeNormalized(update_dir));
                    Quaternion invlook = Quaternion.Inverse(look);
                    Vector3 vel = invlook * rigidbody.velocity;

                    vel.x = 0; vel.y = 0; vel = look * vel;
                    rigidbody.velocity = vel;

                    wfcm_behavior.Execute();
                }
            }
            else  // 不实时更新
            {
                smtt_behavior.Execute();
                if (smtt_behavior.move_state == SmoothMoveToTargetBehavior.MoveState.state_end)
                {
                    if (syntony)
                        behaviorcontroller.OnBehaviorComplete(behaviortype);
                }
            }
        }
    }
}

//BT_4DOFForwardMove
public class DOF4ForwardMoveBehavior : WorkUnionBehavior
{
    public enum DOF4State
    {
        state_org,                // 初始态
        state_turntoTarget,       // 朝向目标点的状态
        state_turntoTarget2up,    // 矫正飞船姿态朝上的状态
        state_up,                 // 飞船姿态矫正完成
        state_end,
    };

    Vector3 rigid_forces;       // 飞船+X+Y+Z推力
    Vector3 rigid_invforces;    // 飞船-X-Y-Z推力
    Vector3 rigid_stopforces;   // 飞船减速阻力

    Vector3 rigid_torque;       // 飞船扭矩
    Vector3 rigid_stoptorque;   // 飞船扭矩

    Vector3 maxspeed;           // 最大线速度
    Vector3 maxinvspeed;        // 最大线速度
    Vector3 maxanglespeed;      // 最大角速度

    Vector3 rigid_forcelocal;
    Vector3 rigid_orientations;

    Vector3 dest_euler;

    DOF4State dof4_state = DOF4State.state_org;  // 飞船当前状态

    FourcesLocalBehavior fources_local_behavior = new FourcesLocalBehavior();

    public BaseBehavior Init(
                       BehaviorController controller,
                        float stamp,
                        float retain,

                        Rigidbody rigid,
                        PhysxMotionType motiontp,

                        Vector3 force,
                        Vector3 invforces,
                        Vector3 stopforce,

                        Vector3 torque,
                        Vector3 stoptorque,

                        Vector3 maxsp,
                        Vector3 maxinvsp,

                        Vector3 maxangsp,

                       Vector3 forcelocal,
                       Vector3 orientations,

                       bool syn = false
        )
    {
        Init(controller, stamp, retain, syn);
        BehaviorType = BehaviorController.BehaviorType.BT_4DOFForwardMove;

        rigidbody = rigid;
        motiontype = motiontp;


        rigid_forces = force;
        rigid_invforces = invforces;
        rigid_stopforces = stopforce;

        rigid_torque = torque;
        rigid_stoptorque = stoptorque;

        maxspeed = maxsp;
        maxinvspeed = maxinvsp;
        maxanglespeed = maxangsp;

        rigid_forcelocal = forcelocal;

        Vector3 engineforce = Vector3.zero;
        if (forcelocal.x > 0.01f)
            engineforce.x = rigid_forces.x * forcelocal.x;
        else if(forcelocal.x < -0.01f)
            engineforce.x = rigid_invforces.x * forcelocal.x;

        if (forcelocal.y > 0.01f)
            engineforce.y = rigid_forces.y * forcelocal.y;
        else if (forcelocal.y < -0.01f)
            engineforce.y = rigid_invforces.y * forcelocal.y;

        if (forcelocal.z > 0.01f)
            engineforce.z = rigid_forces.z * forcelocal.z;
        else if (forcelocal.z < -0.01f)
            engineforce.z = rigid_invforces.z * forcelocal.z;

        fources_local_behavior.Init(controller, stamp, retain, rigid,
            engineforce, stopforce, maxsp, maxinvsp, ForceMode.Acceleration);

        dest_euler = GetEuler(orientations);
        dof4_state = DOF4State.state_org;

        return this;
    }

    public override void Execute()
    {
        // 1 运动信息
        fources_local_behavior.Execute();

        // 2 角度信息
        if (dof4_state == DOF4State.state_org)
        {
            Quaternion quat = new Quaternion();
            quat.eulerAngles = dest_euler;
            rigidbody.transform.rotation = quat;

            dof4_state = DOF4State.state_turntoTarget;
            rigidbody.angularVelocity = Vector3.zero;
        }
        // 开启绕Z轴旋转
        else if (dof4_state == DOF4State.state_turntoTarget)
        {
            Vector3 upDir =
                rigidbody.transform.InverseTransformDirection(Vector3.up);
            upDir = new Vector3(upDir.x, upDir.y, 0);

            if (upDir.sqrMagnitude <= sqrprecision)
                dof4_state = DOF4State.state_up;
            else
            {
                m_workunions = RotateAngular(Vector3.up, upDir,
                              maxanglespeed.magnitude, rigid_torque.magnitude,
                              rigid_stoptorque.magnitude);
                dof4_state = DOF4State.state_turntoTarget2up;
            }
        }

        // 绕Z轴旋转
        else if (dof4_state == DOF4State.state_turntoTarget2up)
        {
            if (!UpdateTorques())
            {
                InitWorkUnion();
                dof4_state = DOF4State.state_up;
            }
        }
        else if (dof4_state == DOF4State.state_up)
        {
            rigidbody.angularVelocity = Vector3.zero;
            dof4_state = DOF4State.state_end;
        }
    }
}

//BT_6DOFForwardMove
public class DOF6ForwardMoveBehavior : WorkUnionBehavior
{
    public enum DOF6State
    {
        state_org,                // 初始态
        state_end,
    };

    Vector3 rigid_forces;       // 飞船+X+Y+Z推力
    Vector3 rigid_invforces;    // 飞船-X-Y-Z推力
    Vector3 rigid_stopforces;   // 飞船减速阻力

    Vector3 rigid_torque;       // 飞船扭矩
    Vector3 rigid_stoptorque;   // 飞船扭矩

    Vector3 maxspeed;           // 最大线速度
    Vector3 maxinvspeed;        // 最大线速度
    Vector3 maxanglespeed;      // 最大角速度

    Vector3 rigid_forcelocal;
    Vector3 rigid_torquelocal;

    DOF6State dof6_state = DOF6State.state_org;  // 飞船当前状态

    FourcesLocalBehavior fources_local_behavior = new FourcesLocalBehavior();

    TorqueBehavior torque_local_behavior = new TorqueBehavior();


    public BaseBehavior Init(
                       BehaviorController controller,
                        float stamp,
                        float retain,

                        Rigidbody rigid,
                        PhysxMotionType motiontp,

                        Vector3 force,
                        Vector3 invforces,
                        Vector3 stopforce,

                        Vector3 torque,
                        Vector3 stoptorque,

                        Vector3 maxsp,
                        Vector3 maxinvsp,

                        Vector3 maxangsp,

                       Vector3 forcelocal,
                       Vector3 torquelocal,

                       bool syn = false
        )
    {
        Init(controller, stamp, retain, syn);
        BehaviorType = BehaviorController.BehaviorType.BT_4DOFForwardMove;

        rigidbody = rigid;
        motiontype = motiontp;


        rigid_forces = force;
        rigid_invforces = invforces;
        rigid_stopforces = stopforce;

        rigid_torque = torque;
        rigid_stoptorque = stoptorque;

        maxspeed = maxsp;
        maxinvspeed = maxinvsp;
        maxanglespeed = maxangsp;

        rigid_forcelocal = forcelocal;
        rigid_torquelocal = torquelocal;

        Vector3 engineforce = Vector3.zero;
        if (forcelocal.x > 0.01f)
            engineforce.x = rigid_forces.x * forcelocal.x;
        else if (forcelocal.x < -0.01f)
            engineforce.x = rigid_invforces.x * forcelocal.x;

        if (forcelocal.y > 0.01f)
            engineforce.y = rigid_forces.y * forcelocal.y;
        else if (forcelocal.y < -0.01f)
            engineforce.y = rigid_invforces.y * forcelocal.y;

        if (forcelocal.z > 0.01f)
            engineforce.z = rigid_forces.z * forcelocal.z;
        else if (forcelocal.z < -0.01f)
            engineforce.z = rigid_invforces.z * forcelocal.z;

        fources_local_behavior.Init(controller, stamp, retain, rigid,
            engineforce, stopforce, maxsp, maxinvsp, ForceMode.Acceleration);

        Vector3 enginetorque = Vector3.zero;
        enginetorque.x = rigid_torque.x * torquelocal.x;
        enginetorque.y = rigid_torque.y * torquelocal.y;
        enginetorque.z = rigid_torque.z * torquelocal.z;

        Vector3 use_maxangsp = maxangsp;
        if (torquelocal.x != 0)
            use_maxangsp.x = maxangsp.x * Mathf.Abs(torquelocal.x);
        if (torquelocal.y != 0)
            use_maxangsp.y = maxangsp.y * Mathf.Abs(torquelocal.y);
        if (torquelocal.z != 0)
            use_maxangsp.z = maxangsp.z * Mathf.Abs(torquelocal.z);


        torque_local_behavior.Init(controller, stamp, retain, rigid,
            enginetorque, stoptorque, use_maxangsp, ForceMode.Acceleration);

        dof6_state = DOF6State.state_org;

        return this;
    }

    public override void Execute()
    {
        // 1 运动信息
        fources_local_behavior.Execute();

        // 2 角度信息
        torque_local_behavior.Execute();
    }
}

// Slied
public class SliedBehavior : WorkUnionBehavior
{
    public enum SliedState
    {
        state_org,
        state_finish,
        state_end,
    };

    Vector3 stop_forces;      // 飞船减速阻力
    Vector3 stop_rotate;

    float rigid_damping;

    SliedState slied_state = SliedState.state_org;  // 飞船当前状态
    StopMoveBehavior stopmovebehavior = new StopMoveBehavior();
    StopRotateBehavior stoprotatebehavior = new StopRotateBehavior();

    public BaseBehavior Init(
                        BehaviorController controller,
                        float stamp,
                        float retain,

                        Rigidbody rigid,
                        PhysxMotionType motiontp,

                        Vector3 stopforce,
                        Vector3 stoprotate,
                        float damping,
                        bool syn = false)
    {
        Init(controller, stamp, retain, syn);
        behaviortype = BehaviorController.BehaviorType.BT_Slied;

        rigidbody = rigid;
        motiontype = motiontp;

        stop_forces = stopforce * damping;
        stop_rotate = stoprotate * damping;
        rigid_damping = damping;
        
        stopmovebehavior.Init(controller, stamp, retain, rigid, motiontp, stop_forces);

        stoprotatebehavior.Init(controller, stamp, retain, rigid, stop_rotate);

        return this;
    }


    public override void Execute()
    {
        if (slied_state == SliedState.state_org)
        {
            stopmovebehavior.Execute();
            stoprotatebehavior.Execute();

            if (stopmovebehavior.move_state == StopMoveBehavior.MoveState.state_end &&
                stoprotatebehavior.rotate_state ==
                    StopRotateBehavior.RotateState.state_end)
            {
                slied_state = SliedState.state_finish;
            }
        }
        else if (slied_state == SliedState.state_finish)
        {
            rigidbody.angularVelocity = Vector3.zero;
            rigidbody.velocity = Vector3.zero;

            slied_state = SliedState.state_end;
            if (syntony)
            {
                behaviorcontroller.OnBehaviorComplete(behaviortype);
            }
        }
    }
}



 
