using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Assertions;

public interface IHumanCorrectionPositionYProperty
{
    void SetLocalPosition(Vector3 position);
    Vector3 GetLocalPosition();
}

public class HumanCorrectionPositionYComponent : EntityComponent<IHumanCorrectionPositionYProperty>
{
    public override void OnInitialize(IHumanCorrectionPositionYProperty property)
    {
        GameplayProxy gameplayProxy = GameFacade.Instance.RetrieveProxy(ProxyName.GameplayProxy) as GameplayProxy;
        NavMeshPath navMeshPath = gameplayProxy.GetNavMeshPath();

        Assert.IsNotNull(navMeshPath, "navMeshPath is null");

        //Vector3 position = property.GetLocalPosition();
        //NavMeshHit hit;
        //if (NavMesh.SamplePosition(position, out hit, 100, NavMesh.AllAreas))
        //{
        //    position.y = hit.position.y;
        //    property.SetLocalPosition(position);
        //}
    }
}
