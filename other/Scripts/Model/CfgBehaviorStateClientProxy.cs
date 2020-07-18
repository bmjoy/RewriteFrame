using Eternity.FlatBuffer;
using PureMVC.Patterns.Proxy;
using UnityEngine;
using UnityEngine.Assertions;

public partial class CfgEternityProxy : Proxy
{
    public string GetBehaviorTreeRelativePath(int id, EnumMainState enumMainState)
    {
        BehaviorStateClient? behaviorStateClient = m_Config.BehaviorStateClientsByKey((uint)id);
        Assert.IsTrue(behaviorStateClient.HasValue, "CfgEternityProxy => GetBehaviorTreeRelativePath not exist Id " + id);

        string path = null;
        switch (enumMainState)
        {
            case EnumMainState.Born:
                path = behaviorStateClient.Value.BornBehavior;
                break;
            case EnumMainState.Cruise:
                path = behaviorStateClient.Value.CruiseBehavior;
                break;
            case EnumMainState.Fight:
                path = behaviorStateClient.Value.FightBehavior;
                break;
            case EnumMainState.Dead:
                path = behaviorStateClient.Value.DeadBehavior;
                break;
            default:
                Debug.LogErrorFormat($"GetBehaviorTreeRelativePath id={id} enumMainState={enumMainState}");
                break;
        }

        if (path == null)
        {
            Debug.LogErrorFormat($"path == null GetBehaviorTreeRelativePath id={id} enumMainState={enumMainState}");
        }

        return path;
    }
}