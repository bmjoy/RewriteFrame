/*===============================
 * Author: [Allen]
 * Purpose: 子弹物理相关信息
 * Time: 2019/12/24 17:46:34
================================*/
using Assets.Scripts.Define;
using Assets.Scripts.Lib.Net;
using System.IO;
using UnityEngine;

public class CreatBulletData : KProtoBuf
{
    //子弹属于哪个人
    public uint OwnerEntityId;

    //子弹属于哪个技能
    public int owerSkillId;

    //子弹属于循环的第几轮索引
    public int LoopIndex;


    //飞行物ID
    public int flyerDataID;

    //出生位置
    public float posX;
    public float posY;
    public float posZ;

    //出生方向欧拉角度
    public float rotationX;
    public float rotationY;
    public float rotationZ;



    //子弹要飞去的目标单位ID
    public bool isHaveTarget;
    public uint targetEntityId;

    //子弹的要飞去的目标位置
    public float target_posX;
    public float target_posY;
    public float target_posZ;



    public override void UnPack(BinaryReader reader)
    {
        base.UnPack(reader);
    }
}

public interface IBulletTargetProperty
{
    /// <summary>
    /// 目标是否是entity
    /// </summary>
    bool IsHaveTarget();

    /// <summary>
    ///子弹要飞去的目标位置
    /// </summary>
    Vector3 GetTargetPoint();

    /// <summary>
    /// 子弹要飞去的目标单位ID
    /// </summary>
    uint GetTargetEntityID();

    Vector3 GetCurrentTargetPoint();
}



public class BulletPhysics : GameEntity<CreatBulletData>,
    IBulletAppearanceProperty,
    IBulletMoveProperty
{
    /// <summary>
    ///子弹拥有者Entity ID
    /// </summary>
    protected uint m_OwnerEntityId;

    /// <summary>
    /// 飞行物数据ID
    /// </summary>
    protected int m_FlyerDataId;

    /// <summary>
    /// 移动速度
    /// </summary>
    private float m_Speed = 0;

    /// <summary>
    /// 子弹最大速度
    /// </summary>
    private float m_MaxSpeed = float.MaxValue;

    /// <summary>
    /// 加速度
    /// </summary>
    private float m_AcceleratedSpeed = 0;


    /// <summary>
    /// 刚体
    /// </summary>
    private Rigidbody m_Rigidbody;

    /// <summary>
    /// 目标是否是entity
    /// </summary>
    private bool m_IsHaveTarget = false;

    /// <summary>
    /// 子弹要飞去的目标单位ID
    /// </summary>
    private uint m_TargetEntityId = 0;

    /// <summary>
    ///子弹要飞去的目标位置
    /// </summary>
    private Vector3 m_TargetPoint = Vector3.zero;

    private Vector3 m_CurrentTargetPoint = Vector3.zero;

    /// <summary>
    /// 当前方向
    /// </summary>
    private Vector3 m_Direction= Quaternion.identity.eulerAngles;

    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="respond">服务器返回协议类型</param>
    public override void InitializeByRespond(CreatBulletData respond)
    {
        m_FlyerDataId = respond.flyerDataID;
        m_OwnerEntityId = respond.OwnerEntityId;
        m_IsHaveTarget = respond.isHaveTarget;
        m_TargetEntityId = respond.targetEntityId;
        m_CurrentTargetPoint = m_TargetPoint = new Vector3(respond.target_posX, respond.target_posY, respond.target_posZ);

        //位置，旋转
        SetPoint(new Vector3(respond.posX, respond.posY, respond.posZ));
        SetDirection(new Vector3(respond.rotationX, respond.rotationY, respond.rotationZ));
    }

    public override void InitializeComponents()
    {
        AddEntityComponent<BulletAppearanceComponent, IBulletAppearanceProperty>(this);             //外观
        AddEntityComponent<BulletMoveComponent, IBulletMoveProperty>(this);                                    //移动

    }


    public Transform GetTransform()
    {
        return transform;
    }

    /// <summary>
    /// 设置位置
    /// </summary>
    /// <param name="pos"></param>
    public void SetPoint(Vector3 pos)
    {
        GetTransform().position = pos;
    }

    /// <summary>
    /// 设置方向,此处参数是eulerAngles
    /// </summary>
    public void SetDirection(Vector3 direction)
    {
        m_Direction = direction;
    }

    /// <summary>
    /// 获取方向欧拉角
    /// </summary>
    /// <returns></returns>
    public Vector3 GetDirection()
    {
        return m_Direction;
    }

    /// <summary>
    /// 获取飞行物数据ID
    /// </summary>
    public int GetFlyerDataId()
    {
        return m_FlyerDataId;
    }

    /// <summary>
    /// 子弹拥有者Entity ID
    /// </summary>
    /// <returns></returns>
    public uint GetOwnerSpacecaftEntityId()
    {
        return m_OwnerEntityId;
    }


    public void SetMaxSpeed(float maxSpeed)
    {
        m_MaxSpeed = maxSpeed;
    }

    /// <summary>
    /// 设置速度
    /// </summary>
    /// <param name="speed"></param>
    public void SetSpeed(float speed)
    {
        m_Speed = speed;
    }

    /// <summary>
    /// 获取速度
    /// </summary>
    /// <returns></returns>
    public float GetSpeed()
    {
        if (m_Speed > m_MaxSpeed)
            m_Speed = m_MaxSpeed;

        return m_Speed;
    }

    /// <summary>
    /// 获取刚体
    /// </summary>
    /// <returns></returns>
    public Rigidbody GetRigidbody()
    {
        return m_Rigidbody;
    }

    /// <summary>
    /// 设置刚体
    /// </summary>
    /// <param name="rigidbody"></param>
    public void SetRigidbody(Rigidbody rigidbody)
    {
        m_Rigidbody = rigidbody;
    }

    /// <summary>
    /// 目标是否是entity
    /// </summary>
    public bool IsHaveTarget()
    {
        return m_IsHaveTarget;
    }

    /// <summary>
    /// 子弹要飞去的目标单位ID
    /// </summary>
    public uint GetTargetEntityID()
    {
        return m_TargetEntityId;
    }

    /// <summary>
    ///子弹要飞去的目标位置
    /// </summary>
    public Vector3 GetTargetPoint()
    {
        return m_TargetPoint;
    }

    public Vector3 GetCurrentTargetPoint()
    {
        if (IsHaveTarget())
        {
            BaseEntity targetEntity = GameplayManager.Instance.GetEntityManager().GetEntityById<BaseEntity>(GetTargetEntityID());
            if(targetEntity != null && !targetEntity.IsDead())
            {
                m_CurrentTargetPoint = targetEntity.GetRootTransform().position;
            }
            return m_CurrentTargetPoint;
        }
        else
        {
            return GetTargetPoint();
        }
    }
}