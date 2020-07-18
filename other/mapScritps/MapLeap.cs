#if UNITY_EDITOR
using EditorExtend;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Map
{
    [ExecuteInEditMode]
    public class MapLeap : MonoBehaviour
    {
        /// <summary>
        /// 跃迁名字
        /// </summary>
        public string m_LeapName;

        [ReadOnly]
        public ulong m_AreaId;

        public Vector3 GetPosition()
        {
            return transform.position;
        }

        public void DoUpdate(ulong areaId)
        {
            m_AreaId = areaId;
        }
    }

}
#endif