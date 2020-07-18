#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Map
{
    /// <summary>
    /// 跃迁模拟
    /// </summary>
    [ExecuteInEditMode]
    public class LeapSimulation : MonoBehaviour
    {
        public MapLeap[] m_Leaps;

        public void RefreshMapLeaps()
        {
            m_Leaps = FindObjectsOfType<MapLeap>();
        }
    }
}

#endif