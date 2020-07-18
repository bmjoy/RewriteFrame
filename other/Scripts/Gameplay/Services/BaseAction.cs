using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseAction {
    // 执行行为，无状态
    abstract public void Execute();
}

// !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
// 以下为具体BaseAction封装，操作为原子行为，且不互相耦合
// !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

/// <summary>
/// Force和Torque的基类
/// </summary>
public abstract class ForcesTorqueAction : BaseAction
{
    public Rigidbody    m_rigidbody;
    public Vector3      m_efficacy;

    public BaseAction Init(Rigidbody rigidbody, Vector3 efficacy)
    {
        m_rigidbody = rigidbody;
        m_efficacy = efficacy;

        return this;
    }
}

/// <summary>
/// Force类型的力行为
/// </summary>
public class ForcesForce : ForcesTorqueAction
{
    public override void Execute()
    {
        if (m_rigidbody)
            m_rigidbody.AddForce(m_efficacy, ForceMode.Force);
        else
            throw new System.NotImplementedException();
    }
}

/// <summary>
/// Impulse类型的力行为
/// </summary>
public class ForcesImpulse : ForcesTorqueAction
{
    public override void Execute()
    {
        if (m_rigidbody)
            m_rigidbody.AddForce(m_efficacy, ForceMode.Impulse);
        else
            throw new System.NotImplementedException();
    }
}

/// <summary>
/// VelocityChange类型的力行为
/// </summary>
public class ForcesVelocityChange : ForcesTorqueAction
{
    public override void Execute()
    {
        if (m_rigidbody)
            m_rigidbody.AddForce(m_efficacy, ForceMode.VelocityChange);
        else
            throw new System.NotImplementedException();
    }
}

/// <summary>
/// Acceleration类型的力行为
/// </summary>
public class ForcesAcceleration : ForcesTorqueAction
{
    public override void Execute()
    {
        if (m_rigidbody)
            m_rigidbody.AddForce(m_efficacy, ForceMode.Acceleration);
        else
            throw new System.NotImplementedException();
    }
}

/// <summary>
/// Force类型的扭矩行为
/// </summary>
public class TorqueForce : ForcesTorqueAction
{
    public override void Execute()
    {
        if (m_rigidbody)
            m_rigidbody.AddTorque(m_efficacy, ForceMode.Force);
        else
            throw new System.NotImplementedException();
    }
}

/// <summary>
/// Impulse类型的扭矩行为
/// </summary>
public class TorqueImpulse : ForcesTorqueAction
{
    public override void Execute()
    {
        if (m_rigidbody)
            m_rigidbody.AddTorque(m_efficacy, ForceMode.Impulse);
        else
            throw new System.NotImplementedException();
    }
}

/// <summary>
/// VelocityChange类型的扭矩行为
/// </summary>
public class TorqueVelocityChange : ForcesTorqueAction
{
    public override void Execute()
    {
        if (m_rigidbody)
            m_rigidbody.AddTorque(m_efficacy, ForceMode.VelocityChange);
        else
            throw new System.NotImplementedException();
    }
}

/// <summary>
/// Acceleration类型的扭矩行为
/// </summary>
public class TorqueAcceleration : ForcesTorqueAction
{
    public override void Execute()
    {
        if (m_rigidbody)
            m_rigidbody.AddTorque(m_efficacy, ForceMode.Acceleration);
        else
            throw new System.NotImplementedException();
    }
}

public class BaseAnimation : BaseAction
{
    public BaseAnimation()
    {
    }
    public override void Execute()
    {
    }
}