using Crucis.Protocol;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public interface ISpacecraftCollisionProperty
{
    bool IsMain();
    Rigidbody GetRigidbody();
    HeroState GetCurrentState();
    void AddOnCollisionCallback(Action<Collision> enter, Action<Collision> stay, Action<Collision> exit);
    void RemoveCollisionCallback(Action<Collision> enter, Action<Collision> stay, Action<Collision> exit);
}

public class SpacecraftCollisionComponent : EntityComponent<ISpacecraftCollisionProperty>
{
    private ISpacecraftCollisionProperty m_Property;
    private readonly HashSet<Collision> m_CollisionSet = new HashSet<Collision>();
    private readonly HashSet<Collision> m_WillRemoveCollisionSet = new HashSet<Collision>();
    /// <summary>
    /// 是否处于碰撞状态
    /// </summary>
    private bool m_IsCollision = false;

    public override void OnInitialize(ISpacecraftCollisionProperty property)
    {
        m_Property = property;

        m_Property.AddOnCollisionCallback(OnCollisionEnter, OnCollisionStay, OnCollisionExit);
    }

    public override void OnDestroy()
    {
        base.OnDestroy();

        m_Property.RemoveCollisionCallback(OnCollisionEnter, OnCollisionStay, OnCollisionExit);
    }

    public override void OnUpdate(float delta)
    {
        if (m_IsCollision)
        {
            m_WillRemoveCollisionSet.Clear();
            foreach (var item in m_CollisionSet)
            {
                if (item.collider == null)
                {
                    m_WillRemoveCollisionSet.Add(item);
                }
            }

            m_CollisionSet.ExceptWith(m_WillRemoveCollisionSet);

            CheckIsCollision();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!m_IsCollision)
        {
            Rigidbody rigidbody = m_Property.GetRigidbody();
            Assert.IsFalse(rigidbody == null, "rigidbody is null");

            GameplayProxy gameplayProxy = GameFacade.Instance.RetrieveProxy(ProxyName.GameplayProxy) as GameplayProxy;
            Assert.IsFalse(gameplayProxy == null, "gameplayProxy is null");

            m_IsCollision = true;

            rigidbody.velocity = Vector3.zero;
            rigidbody.angularVelocity = Vector3.zero;
            rigidbody.freezeRotation = true;

            m_CollisionSet.Add(collision);
            if (m_Property.IsMain())
            {
                HeroMoveHandler.SyncHitWall(
                   true,
                   m_Property.GetCurrentState().GetOnlyServerState(),
                   gameplayProxy.GetCurrentAreaUid(),
                   gameplayProxy.ClientToServerAreaOffset(rigidbody.position),
                   rigidbody.rotation,
                   rigidbody.velocity,
                   rigidbody.angularVelocity
                 );
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision == null)
        {
            Debug.LogWarning("OnCollisionExit obj is null");
            return;
        }

        if (collision.gameObject == null)
        {
            Debug.LogWarning("OnCollisionExit obj.gameObject is null");
            return;
        }

        //Debug.LogError("OnCollisionExit " + collision.transform.name);

        if (m_CollisionSet.Contains(collision))
        {
            m_CollisionSet.Remove(collision);
        }

        CheckIsCollision();
    }

    private void OnCollisionStay(Collision collision)
    {
    }

    private void CheckIsCollision()
    {
        if (m_CollisionSet.Count == 0)
        {
            Rigidbody rigidbody = m_Property.GetRigidbody();
            Assert.IsFalse(rigidbody == null, "rigidbody is null");

            GameplayProxy gameplayProxy = GameFacade.Instance.RetrieveProxy(ProxyName.GameplayProxy) as GameplayProxy;
            Assert.IsFalse(gameplayProxy == null, "gameplayProxy is null");

            m_IsCollision = false;

            rigidbody.velocity = Vector3.zero;
            rigidbody.angularVelocity = Vector3.zero;
            rigidbody.freezeRotation = false;

            if (m_Property.IsMain())
            {
                HeroMoveHandler.SyncHitWall(
                     false,
                     m_Property.GetCurrentState().GetOnlyServerState(),
                     gameplayProxy.GetCurrentAreaUid(),
                     gameplayProxy.ClientToServerAreaOffset(rigidbody.position),
                     rigidbody.rotation,
                     rigidbody.velocity,
                     rigidbody.angularVelocity
                );
            }
        }
    }
}
