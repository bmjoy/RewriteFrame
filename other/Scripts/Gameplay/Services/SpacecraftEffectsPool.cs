using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class SpacecraftEffectsPool
{
    private static SpacecraftEffectsPool ms_SpacecraftEffectsPool;

    private Dictionary<string, GameObject> m_EffectsPoolTable = new Dictionary<string, GameObject>();

    private SpacecraftEffectsPool()
    {

    }

    public static SpacecraftEffectsPool GetInstance()
    {
        if (ms_SpacecraftEffectsPool == null)
        {
            ms_SpacecraftEffectsPool = new SpacecraftEffectsPool();
        }

        return ms_SpacecraftEffectsPool;
    }

    public void CreateEffectsPools(string effectPrefabAddress)
    {
        if (effectPrefabAddress.Length == 0)
        {
            return;
        }

        if (m_EffectsPoolTable.ContainsKey(effectPrefabAddress))
        {
            return;
        }

        AssetUtil.LoadAssetAsync(effectPrefabAddress,
            (pathOrAddress, returnObject, userData) =>
            {
                if (returnObject != null)
                {
                    GameObject go = (UnityEngine.GameObject)returnObject;
                    Assert.IsNotNull(go, effectPrefabAddress);
                    if (m_EffectsPoolTable.ContainsKey(effectPrefabAddress))
                    {
                        return;
                    }
                    go.CreatePool(10, effectPrefabAddress);
                    go.name = effectPrefabAddress;
                    if (!m_EffectsPoolTable.ContainsKey(effectPrefabAddress))
                        m_EffectsPoolTable.Add(effectPrefabAddress, go);
                }
                else
                {
                    Debug.LogError(string.Format("资源加载成功，但返回null,  pathOrAddress = {0}", pathOrAddress));
                }
            });
    }

    public GameObject SpawnEffect(string effectPrefabAddress, Transform parent)
    {
        if (effectPrefabAddress.Length == 0)
        {
            return null;
        }

        GameObject effect;

        if (m_EffectsPoolTable.TryGetValue(effectPrefabAddress, out effect))
        {
            GameObject ret = effect.Spawn(parent);
            ret.transform.localScale = Vector3.one;
            return ret;
        }

        return null;
    }
}