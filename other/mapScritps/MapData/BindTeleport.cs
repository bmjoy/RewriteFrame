#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Map
{
	/// <summary>
	/// 绑定传送门（这里是在npc上绑定传送门）
	/// </summary>
	[ExecuteInEditMode]
	public class BindTeleport : MonoBehaviour
	{
		#region 私有属性
		/// <summary>
		/// GamingMapArea
		/// </summary>
		private GamingMapArea m_MapArea;
		#endregion

		#region 公开属性
		/// <summary>
		/// 传送门id
		/// </summary>
		public int m_TeleportId;

		/// <summary>
		/// npc id
		/// </summary>
		public ulong m_CreatureId;

		/// <summary>
		/// 当前地图中的所有传送门数据
		/// </summary>
		public List<TeleportVO> teleportList;
		/// <summary>
		/// 当前选中的Teleport 索引
		/// </summary>
		public int m_SelectTeleportIndex;

		/// <summary>
		/// 显示的Teleport名字列表
		/// </summary>
		public string[] m_TeleportNames;
		#endregion

		#region 挂载

		#endregion

		#region 私有方法

		#endregion

		#region 公开方法
		public IEnumerator DoUpdate(GamingMapArea area)
		{
			Creature creature = gameObject.GetComponent<Creature>();
			if (creature != null)
			{
				m_CreatureId = creature.m_Uid;
			}
			yield return null;
			m_MapArea = area;
			teleportList = area.GetTeleportList();//因为策划是手动挂脚本，所以得去检测
			if (m_TeleportNames == null)
			{
				if (teleportList != null && teleportList.Count > 0)
				{
					m_TeleportNames = new string[teleportList.Count];
					for (int iTeleport = 0; iTeleport < teleportList.Count; iTeleport++)
					{
						TeleportVO vo = teleportList[iTeleport];
						if (vo == null)
						{
							continue;
						}
						m_TeleportNames[iTeleport] = vo.ID.ToString();
					}
				}
			}
			yield return null;
		}
		#endregion
		
	}
}
#endif
