using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class BehaviorController : MonoBehaviour
{
    public bool IsMain = false;

    public Action<string> Callback = delegate { };

    // 行为类型
    public enum BehaviorType
    {
        BT_Begin,
        BT_Idle,                // 待机状态

        // 飞船形态移动行为
        BT_FourcesLocal,        // 本地加力行为（本地坐标系）,仅针对玩家行为的接口
        BT_Torque,              // 施加扭矩，仅有本地坐标系,仅针对玩家行为的接口
        BT_ForwardCtlMove,      // MMO模型，移动与方向不绑定，独立控制模型
        BT_WorldForceCtlMove,   // MMO模型，移动决定飞船方向模型，方向绑定移动模型
        BT_4DOFForwardMove,     // 4DOF移动模型，飞船朝向摄像头目标旋转，UP朝上，本地坐标系力移动
        BT_6DOFForwardMove,     // 6DOF移动模型，飞船朝向摄像机目标旋转，本地坐标系力移动


        // 扩展行为
        BT_StopRotate,          // 停止指定方向旋转行为
        BT_StopMove,            // 停止指定方向移动行为
        BT_TurnToTarget,        // 朝向目标点行为
        BT_UpTurnToTarget,      // Z轴朝上转向目标

        BT_TurningMove,         // 转向状态下的移动行为
        BT_StopThenMoveToTarget,// 停止并直接移动到目标点行为
        BT_SmoothMoveToTarget,  // 平滑移动到目标点行为
        BT_SmoothMoveTargets,   // 平滑移动一系列的点

        // 扩展行为-特殊行为
        BT_Leap,                // 跃迁行为
        BT_LeapBreak,			// 跃迁打断

        BT_BindMove,            // 绑定移动

        BT_Slied,               // 滑行

        BT_End                  // 结束
    }

    /// <summary>
    /// 当前关联对象的行为状态
    /// </summary>
    private System.Int32 m_behaviortype;

    private void SetBehaviorType(int bt)
    {
        m_behaviortype = bt;
    }

    private void AddBehaviorType(BehaviorType bt)
    {
        m_behaviortype |= 1 << (int)(bt);
    }

    private void RemoveBehaviorType(BehaviorType bt)
    {
        m_behaviortype &= ~(1 << (int)(bt));
    }

    private BaseBehavior GetBehaviorEntity(BehaviorType bt)
    {
        if (!behaviorUnions.ContainsKey(bt))
        {
            BaseBehavior bbh = null;
            switch (bt)
            {
                case BehaviorType.BT_Idle:
                    bbh = new IdleBehavior();
                    break;
                case BehaviorType.BT_FourcesLocal:
                    bbh = new FourcesLocalBehavior();
                    break;
                case BehaviorType.BT_Torque:
                    bbh = new TorqueBehavior();
                    break;
                case BehaviorType.BT_ForwardCtlMove:
                    bbh = new ForwardControlMoveBehavior();
                    break;
                case BehaviorType.BT_WorldForceCtlMove:
                    bbh = new WorldForceControlMoveBehavior();
                    break;
                case BehaviorType.BT_4DOFForwardMove:
                    bbh = new DOF4ForwardMoveBehavior();
                    break;
                case BehaviorType.BT_6DOFForwardMove:
                    bbh = new DOF6ForwardMoveBehavior();
                    break;
                case BehaviorType.BT_StopRotate:
                    bbh = new StopRotateBehavior();
                    break;
                case BehaviorType.BT_StopMove:
                    bbh = new StopMoveBehavior();
                    break;
                case BehaviorType.BT_TurnToTarget:
                    bbh = new TurnToTargetBehavior();
                    break;
                case BehaviorType.BT_UpTurnToTarget:
                    bbh = new UpTurnToTargetBehavior();
                    break;
                case BehaviorType.BT_TurningMove:
                    bbh = new TurningMoveBehavior();
                    break;
                case BehaviorType.BT_StopThenMoveToTarget:
                    bbh = new StopThenMoveToTargetBehavior();
                    break;
                case BehaviorType.BT_SmoothMoveToTarget:
                    bbh = new SmoothMoveToTargetBehavior();
                    break;
                case BehaviorType.BT_SmoothMoveTargets:
                    bbh = new SmoothMoveTargetsBehavior();
                    break;
                case BehaviorType.BT_Leap:
                    bbh = new LeapBehavior();
                    break;
                case BehaviorType.BT_LeapBreak:
                    bbh = new LeapBreakBehavior();
                    break;
                case BehaviorType.BT_BindMove:
                    bbh = new BindMoveBehavior();
                    break;
                case BehaviorType.BT_Slied:
                    bbh = new SliedBehavior();
                    break;
                default:
                    bbh = new IdleBehavior();
                    break;
            }
            behaviorUnions.Add(bt, bbh);
        }

        return behaviorUnions[bt];
    }

    public bool HasBehaviorType(BehaviorType bt)
    {
        return (m_behaviortype & 1 << (int)bt) > 0;
    }

    private Dictionary<BehaviorType, BaseBehavior> behaviorUnions = new Dictionary<BehaviorType, BaseBehavior>();

    public static int[,] behavior_condition_matrix = new int[,]
        {
            //                                  S
            //                                  t
            //                                  o  S
            //                W                       p  m  S
            //                o                       T  o  m
            //                r  4  6                 h  o  o
            //             F  l  D  D           U     e  t  o
            //             o  d  O  O           p     n  h  t
            //       F     r  F  F  F        T  T     M  M  h
            //       o     w  o  F  F        u  t  T  o  o  M
            //       u     a  r  o  o  S     r  r  u  v  v  o
            //       r     r  c  r  r  t     n  n  r  e  e  v     L  B 
            //       c     d  e  w  w  o  S  T  T  n  T  T  e     e  i 
            //       e     C  C  a  a  p  t  o  o  i  o  o  T     a  n 
            //       s  T  t  t  r  r  R  o  T  T  n  T  T  a     p  d 
            // B     L  o  l  l  d  d  o  p  a  a  g  a  a  r     B  M  S
            // e  I  o  r  M  M  M  M  t  M  r  r  M  r  r  g  L  r  o  l
            // g  d  c  q  o  o  o  o  a  o  g  g  o  g  g  e  e  e  v  i
            // i  l  a  u  v  v  v  v  t  v  e  e  v  e  e  t  a  a  e  e
            // n  e  l  e  e  e  e  e  e  e  t  t  e  t  t  s  p  k, 0, d
              {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0},  // Begin
              {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 0},  // Idle
              {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 0},  // FourcesLocal
              {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 0},  // Torque
              {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 0},  // ForwardCtlMove
              {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 0, 0},  // WorldForceCtlMove
              {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 0, 0},  // 4DOFForwardMove
              {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 0, 0},  // 6DOFForwardMove
              {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 0},  // StopRotate
              {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 0},  // StopMove
              {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 0},  // TurnToTarget
              {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 0},  // UpTurnToTarget
              {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 0},  // TurningMove
              {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 0},  // StopThMoveToTarget
              {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 0},  // SmoothMoveToTarget
              {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 0, 0},  // SmoothMoveTargets
              {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1},  // Leap
              {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1},  // LeapBreak
              {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 0, 0},  // BindMove
              {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 0, 0},  // Slied
        };

    public static int[,] behavior_result_matrix = new int[,]
        {
            //                                  S
            //                                  t
            //                                  o  S
            //                W                       p  m  S
            //                o                       T  o  m
            //                r  4  6                 h  o  o
            //             F  l  D  D           U     e  t  o
            //             o  d  O  O           p     n  h  t
            //       F     r  F  F  F        T  T     M  M  h
            //       o     w  o  F  F        u  t  T  o  o  M
            //       u     a  r  o  o  S     r  r  u  v  v  o
            //       r     r  c  r  r  t     n  n  r  e  e  v     L  B  B
            //       c     d  e  w  w  o  S  T  T  n  T  T  e     e  i  i
            //       e     C  C  a  a  p  t  o  o  i  o  o  T     a  n  n
            //       s  T  t  t  r  r  R  o  T  T  n  T  T  a     p  d  d
            // B     L  o  l  l  d  d  o  p  a  a  g  a  a  r     B  M  M
            // e  I  o  r  M  M  M  M  t  M  r  r  M  r  r  g  L  r  o  o
            // g  d  c  q  o  o  o  o  a  o  g  g  o  g  g  e  e  e  v  v
            // i  l  a  u  v  v  v  v  t  v  e  e  v  e  e  t  a  a  e  e
            // n  e  l  e  e  e  e  e  e  e  t  t  e  t  t  s  p  k, 0, 0
              {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0},  // Begin
              {0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1},  // Idle
              {0, 1, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1},  // FourcesLocal
              {0, 1, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1},  // Torque
              {0, 1, 1, 1, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1},  // ForwardCtlMove
              {0, 1, 1, 1, 1, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1},  // WorldForceCtlMove
              {0, 1, 1, 1, 1, 1, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1},  // 4DOFForwardMove
              {0, 1, 1, 1, 1, 1, 1, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1},  // 6DOFForwardMove
              {0, 1, 1, 1, 1, 1, 1, 1, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1},  // StopRotate
              {0, 1, 1, 1, 1, 1, 1, 1, 1, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1},  // StopMove
              {0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1},  // TurnToTarget
              {0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 1, 1, 1, 1, 1, 1, 1, 1},  // UpTurnToTarget
              {0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 1, 1, 1, 1, 1, 1, 1},  // TurningMove
              {0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 1, 1, 1, 1, 1, 1},  // StopThMoveToTarget
              {0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 1, 1, 1, 1, 1},  // SmoothMoveToTarget
              {0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 1, 1, 1, 1},  // SmoothMoveTargets
              {0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 1, 1, 1},  // Leap
              {0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 1, 1},  // LeapBreak
              {0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 1},  // BindMove
              {0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0},  // BindMove
        };

    private bool AddBehavior(BehaviorType bt)
    {
        // 检查条件
        for (int i = 0; i < (int)(BehaviorType.BT_End); i++)
        {
            if (behavior_condition_matrix[(int)bt, i] == 1)
            {
                if (HasBehaviorType((BehaviorType)i)) return false;
            }
        }

        // 执行行为
        AddBehaviorType(bt);

        // 产生结果
        for (int i = 0; i < (int)(BehaviorType.BT_End); i++)
        {
            if (behavior_result_matrix[(int)bt, i] == 1)
            {
                RemoveBehaviorType((BehaviorType)i);
            }
        }
        AddBehaviorType(bt);

        return true;
    }

    public void RemoveBehavior(BehaviorType bt)
    {
        RemoveBehaviorType(bt);
    }

    public void OnBehaviorComplete(BehaviorType bt)
    {
        RemoveBehavior(bt);
    }

    private BaseActionExecuter m_baseactionexecuter = new BaseActionExecuter();

    public void AddAction(BaseAction action)
    {
        m_baseactionexecuter.AddAction(action);
    }

    private Rigidbody m_rigidbody;  // 移动行为针对对象

    private void UpdateBehavior()
    {
        BehaviorType cur_type = BehaviorType.BT_Idle;
        while (cur_type != BehaviorType.BT_End)
        {
            if (HasBehaviorType(cur_type) && behaviorUnions.ContainsKey(cur_type))
            {
                behaviorUnions[cur_type].Execute();
            }
            else if (behaviorUnions.ContainsKey(cur_type))
                behaviorUnions.Remove(cur_type);

            cur_type++;
        }

        if (m_rigidbody)
            m_baseactionexecuter.Execute();

#if ENABLE_SYNCHRONOUS_SELF_LOG
        if (IsMain)
        {
            FightLogToFile.Instance.Write("clinetTime:" + Clock.Instance.GetMillisecond() + " ");
            FightLogToFile.Instance.WriteToJson("position", m_rigidbody.position);
            FightLogToFile.Instance.WriteToJson("rotation", m_rigidbody.rotation);
            FightLogToFile.Instance.WriteToJson("velocity", m_rigidbody.velocity);
            FightLogToFile.Instance.WriteToJson("angularVelocity", m_rigidbody.angularVelocity);
            FightLogToFile.Instance.Write("\n");
        }
#endif
#if ENABLE_SYNCHRONOUS_OTHER_LOG
        if (!IsMain && GetId(gameObject.name) > 100)
        {
            FightLogToFile.Instance.Write("clinetTime:" + Clock.Instance.GetMillisecond() + " ");
            FightLogToFile.Instance.Write("name:" + gameObject.name + " ");
            FightLogToFile.Instance.WriteToJson("position", m_rigidbody.position);
            FightLogToFile.Instance.WriteToJson("rotation", m_rigidbody.rotation);
            FightLogToFile.Instance.WriteToJson("velocity", m_rigidbody.velocity);
            FightLogToFile.Instance.WriteToJson("angularVelocity", m_rigidbody.angularVelocity);
            FightLogToFile.Instance.Write("\n");
        }
#endif
    }

    private int GetId(string name)
    {
        int s = name.IndexOf('_') + 1;
        int l = name.LastIndexOf('_') - s;

        return int.Parse(name.Substring(s, l));
    }

    private Vector3 m_force = Vector3.zero;            // 坐标系正向最大加速度
    private Vector3 m_invforce = Vector3.zero;         // 坐标系负向最大加速度
    private Vector3 m_stopforce = Vector3.zero;        // 停止最大减速度
    private Vector3 m_maxsp = Vector3.zero;            // 坐标系正向最大速度
    private Vector3 m_invmaxsp = Vector3.zero;         // 坐标系负向最大速度

    private Vector3 m_torque = Vector3.zero;           // 坐标系最大角加速度（扭矩）
    private Vector3 m_stoptourque = Vector3.zero;      // 停止最大角减速度
    private Vector3 m_maxanglespeed = Vector3.zero;    // 角最大速度

    private float m_leapforce = 0;                     // 跃迁加速度
    private float m_leapstopforce = 0;			       // 跃迁减速度

    /// <summary>
    /// Use this for initialization
    /// </summary>
    void Awake()
    {
        m_rigidbody = GetComponent<Rigidbody>();    //获取组件
    }

    /// <summary>
    /// Update is called once per frame
    /// </summary>
    void Update()
    {

    }

    /// <summary>
    /// Update is called once per frame
    /// </summary>
    void FixedUpdate()
    {
        // 更新行为对应数据
        UpdateBehavior();
    }

    /// <summary>
    /// 设置刚体初始值信息
    /// </summary>
    /// <param name="position"></param>
    /// <param name="rotation"></param>
    /// <param name="velocity"></param>
    /// <param name="angularVelocity"></param>
    public void SetRigidInfo(Vector3 position, Quaternion rotation, Vector3 velocity, Vector3 angularVelocity)
    {
        // 设置初始状态
        transform.rotation = rotation;
        transform.position = position;
        m_rigidbody.velocity = velocity;
        m_rigidbody.angularVelocity = angularVelocity;
    }

    /// <summary>
    /// 初始化行为参数
    /// </summary>
    /// <param name="force"></param>
    /// <param name="invforce"></param>
    /// <param name="stopforce"></param>
    /// <param name="maxsp"></param>
    /// <param name="invmaxsp"></param>
    /// <param name="torque"></param>
    /// <param name="stoptourque"></param>
    /// <param name="maxanglespeed"></param>
    public void InitBehaviorAbility(Vector3 force, Vector3 invforce, Vector3 stopforce, Vector3 maxsp, Vector3 invmaxsp,
                            Vector3 torque, Vector3 stoptourque, Vector3 maxanglespeed, float leapforce = 5000, float leapstopforce = 5000)
    {
#if ENABLE_SYNCHRONOUS_SELF_LOG
        if (IsMain)
        {
            FightLogToFile.Instance.Write("InitBehaviorAbility=================================\n");
            FightLogToFile.Instance.Write("clinetTime:" + Clock.Instance.GetMillisecond() + "\n");
            FightLogToFile.Instance.WriteToJsonEnter("force", force);
            FightLogToFile.Instance.WriteToJsonEnter("invforce", invforce);
            FightLogToFile.Instance.WriteToJsonEnter("stopforce", stopforce);
            FightLogToFile.Instance.WriteToJsonEnter("maxsp", maxsp);
            FightLogToFile.Instance.WriteToJsonEnter("invmaxsp", invmaxsp);
            FightLogToFile.Instance.WriteToJsonEnter("torque", torque);
            FightLogToFile.Instance.WriteToJsonEnter("stoptourque", stoptourque);
            FightLogToFile.Instance.WriteToJsonEnter("maxanglespeed", maxanglespeed);
            FightLogToFile.Instance.Write("InitBehaviorAbility=================================\n");
        }
#endif

#if ENABLE_SYNCHRONOUS_NPC_LOG
		FightLogToFile.Instance.Write("InitBehaviorAbility=================================\n");
        FightLogToFile.Instance.Write("clinetTime:" + Clock.Instance.GetMillisecond() + "\n");
        FightLogToFile.Instance.WriteToJsonEnter("force", force);
        FightLogToFile.Instance.WriteToJsonEnter("invforce", invforce);
        FightLogToFile.Instance.WriteToJsonEnter("stopforce", stopforce);
        FightLogToFile.Instance.WriteToJsonEnter("maxsp", maxsp);
        FightLogToFile.Instance.WriteToJsonEnter("invmaxsp", invmaxsp);
        FightLogToFile.Instance.WriteToJsonEnter("torque", torque);
        FightLogToFile.Instance.WriteToJsonEnter("stoptourque", stoptourque);
        FightLogToFile.Instance.WriteToJsonEnter("maxanglespeed", maxanglespeed);
        FightLogToFile.Instance.Write("InitBehaviorAbility=================================\n");
#endif

        m_force = force;
        m_invforce = invforce;
        m_stopforce = stopforce;
        m_maxsp = maxsp;
        m_invmaxsp = invmaxsp;

        m_torque = torque * ((float)Math.PI / 180f);
        m_stoptourque = stoptourque * ((float)Math.PI / 180f);
        m_maxanglespeed = maxanglespeed * ((float)Math.PI / 180f);

        m_leapforce = leapforce;
        m_leapstopforce = leapstopforce;


        if (m_maxsp.x < 0 || m_maxsp.y < 0 || m_maxsp.z < 0)
            m_maxsp = Vector3.zero;
        if (m_invmaxsp.x < 0 || m_invmaxsp.y < 0 || m_invmaxsp.z < 0)
            m_invmaxsp = Vector3.zero;
    }

    /// <summary>
    /// 行为模型是否可用
    /// </summary>
    /// <returns></returns>
    public bool IsBehaviorControllerEnable()
    {
        if (m_force.Equals(Vector3.zero) && m_invforce.Equals(Vector3.zero) &&
            m_stopforce.Equals(Vector3.zero) && m_maxsp.Equals(Vector3.zero) &&
            m_invmaxsp.Equals(Vector3.zero) && m_torque.Equals(Vector3.zero) &&
            m_stoptourque.Equals(Vector3.zero) &&
            m_maxanglespeed.Equals(Vector3.zero))
        {
            return false;
        }
        return true;

    }

    /// <summary>
    /// 客户端本地对场景物体
    /// 因为移动超出范围进行位置矫正拖拽的时候调用
    /// 谨慎调用
    /// </summary>
    /// <param name="drag_distance"></param>
    public void OnDragWorldPosition(Vector3 drag_distance)
    {
        BehaviorType curtp = BehaviorType.BT_Idle;
        while (curtp != BehaviorType.BT_End)
        {
            if (HasBehaviorType(curtp) && behaviorUnions.ContainsKey(curtp))
            {
                behaviorUnions[curtp].OnDragWorldPosition(drag_distance);
            }
            curtp++;
        }
    }

    // 行为系列接口
    ///////////////////////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// // 设置IDLE状态，终止目前做的其他所有行为，不再对飞船作用任何力
    /// </summary>
    public void SetToIdle()
    {
        if (AddBehavior(BehaviorType.BT_Idle))
            ((IdleBehavior)GetBehaviorEntity(BehaviorType.BT_Idle)).Init(
                                                        this,
                                                        Time.fixedTime,
                                                        -1);
    }

    /// <summary>
    /// //BT_FourcesLocal，本地坐标系下，对飞船施加Force的力
    /// </summary>
    /// <param name="force"></param>
    public void FourcesLocal(Vector3 force)
    {
        if (!IsBehaviorControllerEnable()) return;

        if (AddBehavior(BehaviorType.BT_FourcesLocal))
            ((FourcesLocalBehavior)GetBehaviorEntity(BehaviorType.BT_FourcesLocal)).Init(
                                                            this,
                                                            Time.fixedTime,
                                                            -1,
                                                            m_rigidbody,

                                                            force,
                                                            m_stopforce,
                                                            m_maxsp,
                                                            m_invmaxsp,

                                                            ForceMode.Acceleration,
                                                            true);
    }

    /// <summary>
    /// //BT_Torque，本地坐标系下，对飞船施加torque的扭矩
    /// </summary>
    /// <param name="torque"></param>
    public void Torque(Vector3 torque)
    {
        if (!IsBehaviorControllerEnable()) return;

        if (AddBehavior(BehaviorType.BT_Torque))
            ((TorqueBehavior)GetBehaviorEntity(BehaviorType.BT_Torque)).Init(
                                                            this,
                                                            Time.fixedTime,
                                                            -1,
                                                            m_rigidbody,

                                                            torque,
                                                            m_stoptourque,
                                                            m_maxanglespeed,

                                                            ForceMode.Acceleration,
                                                            true);
    }

    /// <summary>
    /// 转向目标，如果当前有角度都，先停止角速度，再旋转
    /// </summary>
    /// <param name="target"></param>
    /// <param name="motion"></param>
    public void TurnToTarget(Vector3 target, PhysxMotionType motion = PhysxMotionType.Jet)
    {
        if (!IsBehaviorControllerEnable()) return;

        if (AddBehavior(BehaviorType.BT_TurnToTarget))
            ((TurnToTargetBehavior)GetBehaviorEntity(BehaviorType.BT_TurnToTarget)).Init(
                                                            this,
                                                            Time.fixedTime,
                                                            -1,
                                                            m_rigidbody,
                                                            motion,
                                                            m_torque,
                                                            m_stoptourque,
                                                            m_maxanglespeed,
                                                            target,
                                                            true);
    }

    /// <summary>
    /// BT_StopRotate，停止旋转
    /// </summary>
    public void StopRotate()
    {
        if (!IsBehaviorControllerEnable()) return;

        if (AddBehavior(BehaviorType.BT_StopRotate))
            ((StopRotateBehavior)GetBehaviorEntity(BehaviorType.BT_StopRotate)).Init(
                                                            this,
                                                            Time.fixedTime,
                                                            -1,
                                                            m_rigidbody,
                                                            m_stoptourque,
                                                            true);
    }

    /// <summary>
    /// BT_StopMove，停止移动
    /// </summary>
    /// <param name="motion"></param>
    public void StopMove(PhysxMotionType motion = PhysxMotionType.Jet)
    {
        if (!IsBehaviorControllerEnable()) return;

        if (AddBehavior(BehaviorType.BT_StopMove))
            ((StopMoveBehavior)GetBehaviorEntity(BehaviorType.BT_StopMove)).Init(
                                                            this,
                                                            Time.fixedTime,
                                                            -1,
                                                            m_rigidbody,
                                                            motion,
                                                            m_stopforce,
                                                            true);
    }

	/// <summary>
	/// BT_StopMove，停止移动 停止旋转
	/// </summary>
	public void SetStop()
	{
		m_rigidbody.velocity = Vector3.zero;
		m_rigidbody.angularVelocity = Vector3.zero;
	}

	/// <summary>
	/// BT_StopThenMoveToTarget, 停止，再直接移动到目标点行为
	/// </summary>
	/// <param name="target"></param>
	/// <param name="motion"></param>
	public void StopThenMoveToTarget(Vector3 target, PhysxMotionType motion = PhysxMotionType.Jet)
    {
        if (!IsBehaviorControllerEnable()) return;

        if (AddBehavior(BehaviorType.BT_StopThenMoveToTarget))
            ((StopThenMoveToTargetBehavior)GetBehaviorEntity(BehaviorType.BT_StopThenMoveToTarget)).Init(
                                                            this,
                                                            Time.fixedTime,
                                                            -1,
                                                            m_rigidbody,
                                                            motion,
                                                            m_force,
                                                            m_invforce,
                                                            m_stopforce,
                                                            m_stoptourque,
                                                            m_maxsp,
                                                            m_invmaxsp,
                                                            target,
                                                            true);
    }

    /// <summary>
    /// BT_SmoothMoveToTarget，平滑移动到目标
    /// </summary>
    /// <param name="target"></param>
    /// <param name="motion"></param>
    public void SmoothMoveToTarget(Vector3 target, PhysxMotionType motion = PhysxMotionType.Jet)
    {
        if (!IsBehaviorControllerEnable()) return;

        if (AddBehavior(BehaviorType.BT_SmoothMoveToTarget))
            ((SmoothMoveToTargetBehavior)GetBehaviorEntity(BehaviorType.BT_SmoothMoveToTarget)).Init(
                                                            this,
                                                            Time.fixedTime,
                                                            -1,
                                                            m_rigidbody,
                                                            motion,

                                                            m_force,
                                                            m_invforce,
                                                            m_stopforce,

                                                            m_torque,
                                                            m_stoptourque,

                                                            m_maxsp,
                                                            m_invmaxsp,

                                                            m_maxanglespeed,
                                                            target,
                                                            true);
    }

    /// <summary>
    /// BT_SmoothMoveTargets，按照路点移动，直到到达最后一个目标
    /// </summary>
    /// <param name="targets"></param>
    /// <param name="motion"></param>
    public void SmoothMoveTargets(List<Vector3> targets, PhysxMotionType motion = PhysxMotionType.Jet)
    {
        if (!IsBehaviorControllerEnable()) return;

        if (AddBehavior(BehaviorType.BT_SmoothMoveTargets))
            ((SmoothMoveTargetsBehavior)GetBehaviorEntity(BehaviorType.BT_SmoothMoveTargets)).Init(
                                                            this,
                                                            Time.fixedTime,
                                                            -1,
                                                            m_rigidbody,
                                                            motion,

                                                            m_force,
                                                            m_invforce,
                                                            m_stopforce,

                                                            m_torque,
                                                            m_stoptourque,

                                                            m_maxsp,
                                                            m_invmaxsp,

                                                            m_maxanglespeed,
                                                            targets,
                                                            true);
    }

    /// <summary>
    /// BT_Leap，跃迁
    /// </summary>
    /// <param name="targets"></param>
    /// <param name="motion"></param>
    public void Leap(List<Vector3> targets, float waittime = 3.0f, PhysxMotionType motion = PhysxMotionType.Jet)
    {
        if (!IsBehaviorControllerEnable()) return;

        if (AddBehavior(BehaviorType.BT_Leap))
            ((LeapBehavior)GetBehaviorEntity(BehaviorType.BT_Leap)).Init(
                                                            this,
                                                            Time.fixedTime,
                                                            -1,
                                                            m_rigidbody,
                                                            motion,

                                                            m_force,
                                                            m_invforce,
                                                            m_stopforce,
                                                            m_torque,
                                                            m_stoptourque,
                                                            m_maxsp,
                                                            m_invmaxsp,
                                                            m_maxanglespeed,
                                                            m_leapforce,
                                                            m_leapstopforce,
                                                            targets,
                                                            waittime,
                                                            true);
    }

    /// <summary>
    /// BT_LeapBreak，打断跃迁，恢复姿态
    /// </summary>
    /// <param name="motion"></param>
    public void LeapBreak(PhysxMotionType motion = PhysxMotionType.Jet)
    {
        if (!IsBehaviorControllerEnable()) return;

        if (AddBehavior(BehaviorType.BT_LeapBreak))
            ((LeapBreakBehavior)GetBehaviorEntity(BehaviorType.BT_LeapBreak)).Init(
                                                            this,
                                                            Time.fixedTime,
                                                            -1,
                                                            m_rigidbody,
                                                            motion,

                                                            m_force,
                                                            m_invforce,
                                                            m_stopforce,
                                                            m_torque,
                                                            m_stoptourque,
                                                            m_maxsp,
                                                            m_invmaxsp,
                                                            m_maxanglespeed,
                                                            true);
    }

    /// <summary>
    /// // BT_ForwordCtlMove-玩家
    /// </summary>
    /// <param name="force"></param>
    /// <param name="orientations"></param>
    /// <param name="motion"></param>
    public void ForwardControlMove(Vector3 force, Vector3 orientations, PhysxMotionType motion = PhysxMotionType.Spacecraft)
    {
        if (!IsBehaviorControllerEnable()) return;

        if (AddBehavior(BehaviorType.BT_ForwardCtlMove))
            ((ForwardControlMoveBehavior)GetBehaviorEntity(BehaviorType.BT_ForwardCtlMove)).Init(
                                                            this,
                                                            Time.fixedTime,
                                                            -1,
                                                            m_rigidbody,
                                                            motion,

                                                            m_force,
                                                            m_invforce,
                                                            m_stopforce,
                                                            m_torque,
                                                            m_stoptourque,
                                                            m_maxsp,
                                                            m_invmaxsp,
                                                            m_maxanglespeed,

                                                            force,
                                                            orientations,
                                                            true);
    }

    /// <summary>
    /// WorldForceControlMove
    /// </summary>
    /// <param name="worldforce"></param>
    /// <param name="orientations"></param>
    /// <param name="motion"></param>
    public void WorldForceControlMove(Vector3 worldforce, Vector3 orientations, PhysxMotionType motion = PhysxMotionType.Spacecraft)
    {
        if (!IsBehaviorControllerEnable()) return;

        if (AddBehavior(BehaviorType.BT_WorldForceCtlMove))
            ((WorldForceControlMoveBehavior)GetBehaviorEntity(BehaviorType.BT_WorldForceCtlMove)).Init(
                                                            this,
                                                            Time.fixedTime,
                                                            -1,
                                                            m_rigidbody,
                                                            motion,

                                                            m_force,
                                                            m_invforce,
                                                            m_stopforce,
                                                            m_torque,
                                                            m_stoptourque,
                                                            m_maxsp,
                                                            m_invmaxsp,
                                                            m_maxanglespeed,

                                                            worldforce,
                                                            orientations,
                                                            true);
    }

    /// <summary>
    /// DOF4ForwardMove
    /// </summary>
    /// <param name="worldforce"></param>       前后左右给的力，本地坐标系
    /// <param name="orientations"></param>     飞船的LOOK AT，世界坐标系
    public void DOF4ForwardMove(Vector3 localforce, Vector3 orientations)
    {
        if (!IsBehaviorControllerEnable()) return;

        if (AddBehavior(BehaviorType.BT_4DOFForwardMove))
            ((DOF4ForwardMoveBehavior)GetBehaviorEntity(BehaviorType.BT_4DOFForwardMove)).Init(
                                                            this,
                                                            Time.fixedTime,
                                                            -1,
                                                            m_rigidbody,
                                                            PhysxMotionType.Spacecraft,

                                                            m_force,
                                                            m_invforce,
                                                            m_stopforce,
                                                            m_torque,
                                                            m_stoptourque,
                                                            m_maxsp,
                                                            m_invmaxsp,
                                                            m_maxanglespeed,

                                                            localforce,
                                                            orientations,
                                                            true);
    }

    /// <summary>
    /// DOF6ForwardMove
    /// </summary>
    /// <param name="forcelocal"></param>
    /// <param name="orientations"></param>
    public void DOF6ForwardMove(Vector3 localforce, Vector3 localtorque)
    {
        if (!IsBehaviorControllerEnable()) return;

        if (AddBehavior(BehaviorType.BT_6DOFForwardMove))
            ((DOF6ForwardMoveBehavior)GetBehaviorEntity(BehaviorType.BT_6DOFForwardMove)).Init(
                                                            this,
                                                            Time.fixedTime,
                                                            -1,
                                                            m_rigidbody,
                                                            PhysxMotionType.Jet,

                                                            m_force,
                                                            m_invforce,
                                                            m_stopforce,
                                                            m_torque,
                                                            m_stoptourque,
                                                            m_maxsp,
                                                            m_invmaxsp,
                                                            m_maxanglespeed,

                                                            localforce,
                                                            localtorque,

                                                            true);
    }

    /// <summary>
    /// BindMove
    /// </summary>
    /// <param name="scene"></param>
    /// <param name="bind_uid"></param>
    /// <param name="offset"></param>
    public void BindMove(Crucis.Protocol.BindMove bindmove)
    //1 public void BindMove(RespondMoveEvent respondMove)
    //2 public void BindMove(Rigidbody bindrigid, Vector3 offset, bool berefresh, bool beimme)
    {
        if (AddBehavior(BehaviorType.BT_BindMove))
        {
            ((BindMoveBehavior)GetBehaviorEntity(BehaviorType.BT_BindMove)).Init(
                                                            this,
                                                            Time.fixedTime,
                                                            -1,
                                                            m_rigidbody,

                                                            m_force,
                                                            m_invforce,
                                                            m_stopforce,
                                                            m_torque,
                                                            m_stoptourque,
                                                            m_maxsp,
                                                            m_invmaxsp,
                                                            m_maxanglespeed,

                                                            bindmove,

                                                            //1 respondMove,

                                                            //2 bindrigid,
                                                            //2 offset,
                                                            //2 berefresh,
                                                            //2 beimme,

                                                            true);
        }
    }

    /// <summary>
    /// Slied
    /// </summary>
    /// <param name="damping"></param>
    public void Slide(float damping)
    {
        if (!IsBehaviorControllerEnable()) return;

        if (AddBehavior(BehaviorType.BT_Slied))
            ((SliedBehavior)GetBehaviorEntity(BehaviorType.BT_Slied)).Init(
                                                            this,
                                                            Time.fixedTime,
                                                            -1,
                                                            m_rigidbody,
                                                            PhysxMotionType.Jet,

                                                            m_stopforce,
                                                            m_stoptourque,

                                                            damping,

                                                            true);
    }
}

