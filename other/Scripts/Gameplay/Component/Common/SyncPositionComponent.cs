using Assets.Scripts.Define;
using UnityEngine;

public interface ISyncMapPositionProperty
{
    Transform GetRootTransform();
}

public class SyncMapPositionComponent : EntityComponent<ISyncMapPositionProperty>
{
    private ISyncMapPositionProperty m_Property;
    private GameplayProxy m_GameplayProxy;

    public override void OnInitialize(ISyncMapPositionProperty property)
    {
        m_Property = property;
        m_GameplayProxy = GameFacade.Instance.RetrieveProxy(ProxyName.GameplayProxy) as GameplayProxy;
    }

    public override void OnUpdate(float delta)
    {
        Vector3 playerRealWorldPosition;
        if (m_GameplayProxy.ServerAreaOffsetPositionToWorldPosition(out playerRealWorldPosition, m_GameplayProxy.ClientToServerAreaOffset(m_Property.GetRootTransform().position)))
        {
            Map.MapManager.GetInstance().SetPlayerPosition(playerRealWorldPosition, m_Property.GetRootTransform().position);
        }
    }
}
