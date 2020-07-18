#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class DebugLightMap : MonoBehaviour
{
    public int m_BakeLightMapIndex;
    public Vector4 m_BakeLightMapScale;

    public int m_RealLightMapIndex;
    public Vector4 m_RealLightMapScale;

    /// <summary>
    /// 实际的reallightmapindex
    /// </summary>
    public int m_ActRealLightMapIndex;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void Set()
    {
        Renderer rener = GetComponent<Renderer>();
        if (rener)
        {
            rener.realtimeLightmapIndex = m_ActRealLightMapIndex;
        }
    }
    private void OnEnable()
    {
        Renderer rener = GetComponent<Renderer>();
        if(rener)
        {
            m_BakeLightMapIndex = rener.lightmapIndex;
            m_BakeLightMapScale = rener.lightmapScaleOffset;

            m_RealLightMapIndex = rener.realtimeLightmapIndex;
            m_RealLightMapScale = rener.realtimeLightmapScaleOffset;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
#endif