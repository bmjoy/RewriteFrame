#if UNITY_EDITOR
using EditorExtend;
using Map;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Map
{
	/// <summary>
	/// npc根节点
	/// </summary>
	[ExecuteInEditMode]
	public class CreatureRoot : MapEntityRoot
	{
		#region 私有属性
		/// <summary>
		/// 最大的CreatureId
		/// </summary>
		private int m_MaxCreatureId;

		/// <summary>
		/// Json数据
		/// </summary>
		private EditorCreature[] m_EditorCreature;

		private EditorTeleport[] m_EditorTeleport;
		/// <summary>
		/// 绑定Teleport组件
		/// </summary>
		private BindTeleport[] m_BindTeleports;
		/// <summary>
		/// 创建npc的携程
		/// </summary>
		private IEnumerator m_CreateCreatureEnumerator;
		#endregion

		#region 公开属性

		#endregion

		#region 挂载
		/// <summary>
		/// 节点之下所包含的Creature
		/// </summary>
		public Creature[] m_CreatureCache;
		#endregion

		#region 私有方法
		private void OnEnable()
		{
			if (Application.isPlaying)
			{
				return;
			}
		}

		private void Reset()
		{
			if (m_CreatureCache != null && m_CreatureCache.Length > 0)
			{
				for (int iCreature = 0; iCreature < m_CreatureCache.Length; iCreature++)
				{
					m_CreatureCache[iCreature].DestroySelf();
				}
			}
			m_CreatureCache = null;
		}

		private void OnDestroy()
		{
			Reset();
		}
		/// <summary>
		/// 计算最大的CreatureId
		/// </summary>
		private void CalcuateMaxCreatureId()
		{
			if (m_CreatureCache != null && m_CreatureCache.Length > 0)
			{
				for (int iCreature = 0; iCreature < m_CreatureCache.Length; iCreature++)
				{
					Creature creature = m_CreatureCache[iCreature];
					if (creature != null)
					{
						if (creature.m_CreatureId > m_MaxCreatureId)
						{
							m_MaxCreatureId = creature.m_CreatureId;
						}
					}
				}
			}
		}
		/// <summary>
		/// 创建npc携程
		/// </summary>
		/// <returns></returns>
		private IEnumerator CreatureCreatures()
		{
			if (m_EditorCreature != null && m_EditorCreature.Length > 0)
			{
				for (int iCreature = 0; iCreature < m_EditorCreature.Length; iCreature++)
				{
					GameObject creatureObj = new GameObject();
					creatureObj.transform.SetParent(transform);
					Creature creature = creatureObj.AddComponent<Creature>();
					creature.Init(m_EditorCreature[iCreature]);
					EditorTeleport teleport = GetTeleport(creature.m_Uid);
					if (teleport != null)
					{
						BindTeleport bindTeleport = creatureObj.AddComponent<BindTeleport>();
						if (bindTeleport != null)
						{
							bindTeleport.m_TeleportId = teleport.teleportId;
						}
					}
					if (iCreature % 5 == 0)
					{
						yield return null;
					}
				}
			}
			yield return null;
		}

		/// <summary>
		/// 获取传送门数据
		/// </summary>
		/// <param name="creatureId"></param>
		/// <returns></returns>
		private EditorTeleport GetTeleport(ulong creatureId)
		{
			if (m_EditorTeleport != null && m_EditorTeleport.Length > 0)
			{
				for (int iTeleport = 0; iTeleport < m_EditorTeleport.Length; iTeleport++)
				{
					if (m_EditorTeleport[iTeleport].creaureId == creatureId)
					{
						return m_EditorTeleport[iTeleport];
					}
				}
			}
			return null;
		}
		#endregion

		#region 公开方法
		/// <summary>
		/// 获取所绑定的传送门
		/// </summary>
		/// <returns></returns>
		public BindTeleport[] GetBindTeleports()
		{
			return m_BindTeleports;
		}

		/// <summary>
		/// 初始化
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="teleports"></param>
		public void Init(EditorCreature[] creature, EditorTeleport[] teleports)
		{
			m_EditorCreature = creature;
			m_EditorTeleport = teleports;
			m_CreateCreatureEnumerator = CreatureCreatures();
		}

		/// <summary>
		/// 创建NPC
		/// </summary>
		public void CreateCreature()
		{
			GameObject creatureObj = new GameObject();
			creatureObj.transform.SetParent(transform);
			Creature creature = creatureObj.AddComponent<Creature>();
			if (creature != null)
			{
                m_CreatureCache = gameObject.GetComponentsInChildren<Creature>();
                CalcuateMaxCreatureId();
				int nextCreatureId = (int)(m_MaxCreatureId + 1);
				string creatureUidStr = string.Format("{0}{1}{2}", m_GamingMapArea.GetGamingMapId(), m_GamingMapArea.m_AreaId, MapEditorUtility.CalcuateNumber(nextCreatureId, 3));
                ulong creatureUid = ulong.Parse(creatureUidStr);
                creature.Init(creatureUid, nextCreatureId, this);
			}
			Selection.activeGameObject = creatureObj;
		}

		public IEnumerator OnUpdate(GamingMapArea mapArea)
		{
			m_GamingMapArea = mapArea;
			if (m_CreateCreatureEnumerator != null)
			{
				while (m_CreateCreatureEnumerator.MoveNext())
				{
					yield return null;
				}
				m_CreateCreatureEnumerator = null;
				m_EditorCreature = null;
			}
			yield return null;
			m_BindTeleports = gameObject.GetComponentsInChildren<BindTeleport>();
			if (m_BindTeleports != null && m_BindTeleports.Length > 0)
			{
				for (int iBind = 0; iBind < m_BindTeleports.Length; iBind++)
				{
                    if(m_BindTeleports[iBind] == null || m_BindTeleports[iBind].gameObject == null)
                    {
                        continue;
                    }
					IEnumerator bindTeleportEnumerator = m_BindTeleports[iBind].DoUpdate(m_GamingMapArea);
					if (bindTeleportEnumerator != null)
					{
						while (bindTeleportEnumerator.MoveNext())
						{
							yield return null;
						}
					}
				}
			}
			yield return null;
			m_CreatureCache = gameObject.GetComponentsInChildren<Creature>();
			CalcuateMaxCreatureId();
			if (m_CreatureCache != null && m_CreatureCache.Length > 0)
			{
				for (int iCreatureCache = 0; m_CreatureCache != null && iCreatureCache < m_CreatureCache.Length; iCreatureCache++)
				{
					IEnumerator creatureEnumrator = m_CreatureCache[iCreatureCache].OnUpdate(this, m_ShowModel);
					if (creatureEnumrator != null)
					{
						while (m_CreatureCache != null && m_CreatureCache[iCreatureCache] != null && creatureEnumrator != null && creatureEnumrator.MoveNext())
						{
							yield return null;
						}
					}
				}
			}
			yield return null;
		}

		#endregion

		#region 父类方法
		public override void BeginExport()
		{
			base.BeginExport();
			if (m_CreatureCache != null && m_CreatureCache.Length > 0)
			{
				for (int iLocation = 0; iLocation < m_CreatureCache.Length; iLocation++)
				{
					m_CreatureCache[iLocation].RefreshPosition(true);
				}
			}
		}

		public override void Clear(bool needDestroy = true)
		{
			Reset();
			base.Clear(needDestroy);
		}
		
		#endregion
		
	}
}
#endif
