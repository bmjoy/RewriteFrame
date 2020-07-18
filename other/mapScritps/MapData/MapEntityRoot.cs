#if UNITY_EDITOR
using Map;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Map
{
	[ExecuteInEditMode]
	public class MapEntityRoot : MonoBehaviour
	{
		/// <summary>
		/// GamingMap区域
		/// </summary>
		public GamingMapArea m_GamingMapArea;

		/// <summary>
		/// 是否显示模型
		/// </summary>
		public bool m_ShowModel = true;

		/// <summary>
		/// 获取GamingMap
		/// </summary>
		/// <returns></returns>
		public GamingMap GetGamingMap()
		{
			return m_GamingMapArea.m_GamingMap;
		}

        public GamingMapType GetGamingMapType()
        {
            return m_GamingMapArea.GetGamingMapType();
        }

        public KMapPathType GetGamingMapPathType()
        {
            return m_GamingMapArea.GetGamingMapPathType();
        }
        /// <summary>
        /// 获取地图Id
        /// </summary>
        /// <returns></returns>
        public uint GetGamingMapId()
		{
			return m_GamingMapArea.m_GamingMap.m_MapId;
		}
		/// <summary>
		/// 获取AreaId
		/// </summary>
		/// <returns></returns>
		public ulong GetAreaId()
		{
			return m_GamingMapArea.m_AreaId;
		}

		/// <summary>
		/// 清除所有
		/// </summary>
		public virtual void Clear(bool needDestroy = true)
		{
			if (needDestroy)
			{
				GameObject.DestroyImmediate(gameObject);
			}
		}

		public virtual void BeginExport()
		{

		}
    }
}
#endif
