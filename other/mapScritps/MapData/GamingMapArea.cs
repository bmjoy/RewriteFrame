#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Map
{
    [ExecuteInEditMode]
	public class GamingMapArea : MonoBehaviour
	{
		#region 私有属性
		/// <summary>
		/// json数据
		/// </summary>
		private EditorArea m_EditorArea;
		/// <summary>
		/// 当前区域的传送门列表
		/// </summary>
		private List<TeleportVO> m_TeleportList;

		/// <summary>
		/// Area场景名称
		/// </summary>
		private string m_AreaScenePath;
		#endregion

		#region 公开属性
		/// <summary>
		/// 区域Id
		/// </summary>
		public ulong m_AreaId;
		/// <summary>
		/// 区域名称
		/// </summary>
		public string m_AreaName;

        /// <summary>
        /// 最大战舰碰撞盒高度
        /// </summary>
        private float m_MaxWarShipHeight;

        /// <summary>
        /// 区域类型
        /// </summary>
        public AreaType m_AreaType;

        /// <summary>
        /// 当为副跃迁点时 返回主跃迁点id
        /// </summary>
        public ulong m_FatherArea;

        /// <summary>
        /// 当为主跃迁点时 返回对应的子跃迁点列表
        /// </summary>
        public List<ulong> m_ChildAreas;

        /// <summary>
        /// 任务发布列表
        /// </summary>
        public List<int> m_MissionList;

        public ulong m_RelieveCreatue;

        [System.NonSerialized]
        public List<string> m_RelieveCache;
        [System.NonSerialized]
        public List<ulong> m_RelieveIdCache;
		#endregion

		#region 挂载
		/// <summary>
		/// NPC根节点
		/// </summary>
		public CreatureRoot m_CreatureRoot;
		/// <summary>
		/// Location根节点
		/// </summary>
		public LocationRoot m_LocationRoot;
		/// <summary>
		/// GamingMap节点
		/// </summary>
		public GamingMap m_GamingMap;
		/// <summary>
		/// Teleport根节点
		/// </summary>
		public TeleportRoot m_TeleportRoot;

        /// <summary>
        /// Leap根节点
        /// </summary>
        public LeapRoot m_LeapRoot;

        /// <summary>
        /// 触发器根节点
        /// </summary>
        public TriggerRoot m_TriggerRoot;

        /// <summary>
        /// 寻宝根节点
        /// </summary>
        public TreasureRoot m_TreasureRoot;

        /// <summary>
        /// 采矿节点
        /// </summary>
        public MineralRoot m_MineralRoot;
		#endregion

		#region 私有方法

		private void OnEnable()
		{
			if (Application.isPlaying)
			{
				return;
			}
			m_CreatureRoot = GetComponentInChildren<CreatureRoot>();
			m_LocationRoot = GetComponentInChildren<LocationRoot>();
			m_TeleportRoot = GetComponentInChildren<TeleportRoot>();
            m_LeapRoot = GetComponentInChildren<LeapRoot>();
            m_TriggerRoot = GetComponentInChildren<TriggerRoot>();
            m_TreasureRoot = GetComponentInChildren<TreasureRoot>();
            m_MineralRoot = GetComponentInChildren<MineralRoot>();
            if (m_LeapRoot == null)
            {
                CreateLeapRoot();
            }
            if(m_TriggerRoot == null)
            {
                CreateTriggerRoot();
            }
            if(m_TreasureRoot == null)
            {
                CreateTreasureRoot();
            }
            if(m_MineralRoot == null)
            {
                CreateMineralRoot();
            }
        }
        
        private void CreateTreasureRoot()
        {
            GameObject treasureObj = new GameObject(typeof(TreasureRoot).Name);
            treasureObj.transform.SetParent(transform);
            treasureObj.transform.localPosition = Vector3.zero;
            treasureObj.transform.localScale = Vector3.one;
            m_TreasureRoot = treasureObj.AddComponent<TreasureRoot>();
        }

        private void CreateMineralRoot()
        {
            GameObject mineralObj = new GameObject(typeof(MineralRoot).Name);
            mineralObj.transform.SetParent(transform);
            mineralObj.transform.localPosition = Vector3.zero;
            mineralObj.transform.localScale = Vector3.one;
            m_MineralRoot = mineralObj.AddComponent<MineralRoot>();
        }

        private void CreateLeapRoot()
        {
            string leapTempletePath = GamingMapEditorUtility.GetLeapTempletePath();
            if (string.IsNullOrEmpty(leapTempletePath))
            {
                return;
            }
            GameObject leapTemplete = AssetDatabase.LoadAssetAtPath<GameObject>(leapTempletePath);
            if(leapTemplete != null)
            {
                GameObject leapRootObj = GameObject.Instantiate(leapTemplete);
                leapRootObj.name = "LeapRoot";
                leapRootObj.transform.SetParent(transform);
                leapRootObj.transform.localPosition = Vector3.zero;
                leapRootObj.transform.localRotation = Quaternion.identity;
                m_LeapRoot = leapRootObj.AddComponent<LeapRoot>();
            }
        }
		/// <summary>
		/// 调整Transform
		/// </summary>
		/// <param name="areaSpawner"></param>
		public void AdjustTransform(AreaSpawner areaSpawner)
		{
			if (areaSpawner != null)
			{
				transform.position = areaSpawner.GetAreaPosition();
				transform.rotation = areaSpawner.GetAreaRotation();
			}
		}

		private void Reset(bool needDestroy = true)
		{
			if (m_CreatureRoot != null)
			{
				m_CreatureRoot.Clear(needDestroy);
			}
			if (m_LocationRoot != null)
			{
				m_LocationRoot.Clear(needDestroy);
			}
			if (m_TeleportRoot != null)
			{
				m_TeleportRoot.Clear(needDestroy);
			}
            if(m_LeapRoot != null)
            {
                m_LeapRoot.Clear(needDestroy);
            }
            if(m_TriggerRoot != null)
            {
                m_TriggerRoot.Clear(needDestroy);
            }
            if(m_TreasureRoot != null)
            {
                m_TreasureRoot.Clear(needDestroy);
            }
            if(m_MineralRoot != null)
            {
                m_MineralRoot.Clear(needDestroy);
            }
		}
		#endregion

		#region 公开方法

        public void LoadRelieveCreature()
        {
            if(m_CreatureRoot == null || m_CreatureRoot.m_CreatureCache == null || m_CreatureRoot.m_CreatureCache.Length<=0)
            {
                m_RelieveCreatue = 0;
                return;
            }

            if (m_RelieveCache == null)
            {
                m_RelieveCache = new List<string>();
            }

            if (m_RelieveIdCache == null)
            {
                m_RelieveIdCache = new List<ulong>();
            }

            m_RelieveCache.Clear();
            m_RelieveIdCache.Clear();


            Creature[] creatures = m_CreatureRoot.m_CreatureCache;
            if(creatures != null && creatures.Length>0)
            {
                for(int iCreature = 0;iCreature<creatures.Length;iCreature++)
                {
                    Creature creature = creatures[iCreature];
                    if(creature.IsRelieveCreature())
                    {
                        m_RelieveIdCache.Add(creature.m_Uid);
                        m_RelieveCache.Add(creature.gameObject.name);
                    }
                }
            }

            if(!m_RelieveIdCache.Contains(m_RelieveCreatue))
            {
                m_RelieveCreatue = 0;
            }

        }

        public void SetMaxWarShipHeight(float height)
        {
            m_MaxWarShipHeight = height;
        }

        public float GetMaxWarShipHeight()
        {
            if(m_MaxWarShipHeight<=0)
            {
                if(m_LocationRoot != null)
                {
                    CapsuleCollider collider =  m_LocationRoot.GetComponentInChildren<CapsuleCollider>();
                    if(collider != null)
                    {
                        m_MaxWarShipHeight = collider.height;
                    }
                }
            }
            return m_MaxWarShipHeight;
        }
		/// <summary>
		/// 初始化
		/// </summary>
		/// <param name="areaId"></param>
		public void Init(AreaSpawner areaSpawner,GamingMap map)
		{
            m_GamingMap = map;
			m_AreaId = areaSpawner.GetAreaId();
            //gameObject.name = string.Format("Area_{0}", m_AreaId);
            //gameObject.name = string.Format("Area_{0}", m_AreaName);
            gameObject.name = string.Format("{0}_{1}", m_AreaName, m_AreaId);
			AdjustTransform(areaSpawner);
            RefreshAreaInfo();
        }

        public void RefreshAreaInfo()
        {
            if(m_LeapRoot == null)
            {
                m_FatherArea = 0;
                m_ChildAreas = null;
                return;
            }

            if(m_LeapRoot.m_LeapType == LeapType.Child)
            {
                m_FatherArea = m_LeapRoot.m_MainLeapId;
                m_ChildAreas = null;
            }
            else if(m_LeapRoot.m_LeapType == LeapType.Main)
            {
                m_FatherArea = 0;
                if(m_ChildAreas == null)
                {
                    m_ChildAreas = new List<ulong>();
                }
                m_ChildAreas.Clear();

                List<Leap> leapList = m_GamingMap.m_LeapOverview.m_LeapList;
                if (leapList != null && leapList.Count > 0)
                {
                    for (int iLeap = 0; iLeap < leapList.Count; iLeap++)
                    {
                        Leap leap = leapList[iLeap];
                        if (leap.m_MainLeapId>0 && leap.m_MainLeapId == m_LeapRoot.m_LeapId)
                        {
                            m_ChildAreas.Add(leap.m_LeapId);
                        }
                    }
                }
            }
           
        }

		/// <summary>
		/// 获取绑定的BindTeleport
		/// </summary>
		/// <param name="teleportId"></param>
		/// <returns></returns>
		public ulong GetBindTeleport(int teleportId)
		{
			if (m_CreatureRoot == null)
			{
                return 0;
			}
			BindTeleport[] teleports = m_CreatureRoot.GetBindTeleports();
			if (teleports != null && teleports.Length > 0)
			{
				for (int iTeleport = 0; iTeleport < teleports.Length; iTeleport++)
				{
					if (teleports[iTeleport].m_TeleportId == teleportId)
					{
						return teleports[iTeleport].m_CreatureId;
					}
				}
			}
            return 0;
		}
		/// <summary>
		/// 获取当前区域拥有的传送门
		/// </summary>
		/// <returns></returns>
		public List<TeleportVO> GetTeleportList()
		{
			if (m_TeleportList == null)
			{
				m_TeleportList = new List<TeleportVO>();
				List<TeleportVO> teleportList = ConfigVO<TeleportVO>.Instance.GetList();
				if (teleportList != null && teleportList.Count > 0)
				{
					List<string> teleportIdList = new List<string>();
					for (int iTeleport = 0; iTeleport < teleportList.Count; iTeleport++)
					{
						TeleportVO vo = teleportList[iTeleport];
						if (vo != null && vo.StartGamingMap == m_GamingMap.m_Uid && vo.StartGamingMapArea == m_AreaId)
						{
							m_TeleportList.Add(vo);
						}
					}
				}
			}
			return m_TeleportList;
		}

	    /// <summary>
		/// 初始化
		/// </summary>
		/// <param name="area"></param>
		/// <param name="map"></param>
		public void Init(EditorArea area, GamingMap map)
		{
			m_GamingMap = map;
			m_AreaId = area.areaId;
			m_EditorArea = area;
			m_AreaName = area.areaName;
            m_AreaType = (AreaType)area.areaType;
            m_FatherArea = area.fatherArea;
            m_RelieveCreatue = area.relieveCreature;
            if (area.childrenAreaList != null && area.childrenAreaList.Length>0)
            {
                m_ChildAreas = new List<ulong>();
                for(int iChild = 0;iChild<area.childrenAreaList.Length;iChild++)
                {
                    m_ChildAreas.Add(area.childrenAreaList[iChild]);
                }
            }
            else
            {
                m_ChildAreas = null;
            }
            //gameObject.name = string.Format("Area_{0}", m_AreaName);
            gameObject.name = string.Format("{0}_{1}", m_AreaName, m_AreaId);
            AreaSpawner areaSpawner = map.GetAreaSpawner(m_AreaId);
			AdjustTransform(areaSpawner);

			if (m_EditorArea != null)
			{
				CreateRoots(m_EditorArea);
				m_EditorArea = null;
			}

            if(area.sceneMissionReleaseId != null && area.sceneMissionReleaseId.Length>0)
            {
                m_MissionList = new List<int>(area.sceneMissionReleaseId);
            }

		}

		/// <summary>
		/// 开始导出
		/// </summary>
		public void BeginExport()
		{
			if (m_CreatureRoot != null)
			{
				m_CreatureRoot.BeginExport();
			}
			if (m_LocationRoot != null)
			{
				m_LocationRoot.BeginExport();
			}
			if (m_TeleportRoot != null)
			{
				m_TeleportRoot.BeginExport();
			}
            if(m_LeapRoot != null)
            {
                m_LeapRoot.BeginExport();
            }

            if(m_TriggerRoot != null)
            {
                m_TriggerRoot.BeginExport();
            }
            if(m_TreasureRoot != null)
            {
                m_TreasureRoot.BeginExport();
            }
            if(m_MineralRoot != null)
            {
                m_MineralRoot.BeginExport();
            }
        }

		/// <summary>
		/// 创建根节点
		/// </summary>
		private void CreateRoots(EditorArea area)
		{
            if(m_TreasureRoot == null || m_TreasureRoot.gameObject == null)
            {
                CreateTreasureRoot();
            }
            else
            {
                m_TreasureRoot.Clear(false);
            }
            m_TreasureRoot.Init(area.treasureList);

            if (m_MineralRoot == null || m_MineralRoot.gameObject == null)
            {
                CreateMineralRoot();
            }
            else
            {
                m_MineralRoot.Clear(false);
            }
            m_MineralRoot.Init(area.mineralList);

            //创建CreatureRoot
            if (m_CreatureRoot == null || m_CreatureRoot.gameObject == null)
			{
				GameObject creatureRootObj = new GameObject("CreatureRoot");
				creatureRootObj.transform.SetParent(transform);
				creatureRootObj.transform.localPosition = Vector3.zero;
				creatureRootObj.transform.localRotation = Quaternion.identity;
				m_CreatureRoot = creatureRootObj.AddComponent<CreatureRoot>();
			}
			else
			{
				m_CreatureRoot.Clear(false);
			}
			m_CreatureRoot.Init(area.creatureList, area.teleportList);

			//创建TeleportRoot
			if (m_TeleportRoot == null || m_TeleportRoot.gameObject == null)
			{
				GameObject teleportRootObj = new GameObject("TeleportRoot");
				teleportRootObj.transform.SetParent(transform);
				teleportRootObj.transform.localPosition = Vector3.zero;
				teleportRootObj.transform.localRotation = Quaternion.identity;
				m_TeleportRoot = teleportRootObj.AddComponent<TeleportRoot>();
			}
			else
			{
				m_TeleportRoot.Clear(false);
			}

			m_TeleportRoot.Init(area.teleportList, this);

			//创建LocationRoot
			if (m_LocationRoot == null || m_LocationRoot.gameObject == null)
			{
                GameObject locationRootObj = new GameObject("LocationRoot");
                locationRootObj.transform.SetParent(transform);
                locationRootObj.transform.localPosition = Vector3.zero;
                locationRootObj.transform.localRotation = Quaternion.identity;
                m_LocationRoot = locationRootObj.AddComponent<LocationRoot>();
            }
			else
			{
				m_LocationRoot.Clear(false);
			}
			m_LocationRoot.Init(area.locationList);

            //创建LeapRoot
            if(m_LeapRoot == null || m_LeapRoot.gameObject == null)
            {
                CreateLeapRoot();
            }
            else
            {
                m_LeapRoot.Clear(false);
            }
            m_LeapRoot.Init(area.leapList);

            if(m_TriggerRoot == null || m_TriggerRoot.gameObject ==null)
            {
                CreateTriggerRoot();
            }
            else
            {
                m_TriggerRoot.Clear(false);
            }
            m_TriggerRoot.Init(area.triggerList);
        }

        private void CreateTriggerRoot()
        {
            GameObject triggerRootObj = new GameObject("TriggerRoot");
            triggerRootObj.transform.SetParent(transform);
            triggerRootObj.transform.localPosition = Vector3.zero;
            triggerRootObj.transform.localRotation = Quaternion.identity;
            m_TriggerRoot = triggerRootObj.AddComponent<TriggerRoot>();
        }
		/// <summary>
		/// 获取Area所存放的路径
		/// </summary>
		/// <returns></returns>
		public string GetAreaScenePath()
		{
            if(string.IsNullOrEmpty(m_AreaScenePath))
            {
                m_AreaScenePath = string.Format("{0}/GamingMapArea_{1}_{2}.unity", m_GamingMap.GetOwnerAreaPath(), m_GamingMap.m_Uid, m_AreaId);
            }
			return m_AreaScenePath;
		}

		public void Clear(bool needDestroy = true)
		{
			Reset(needDestroy);
			if (needDestroy)
			{
				GameObject.DestroyImmediate(gameObject);
			}
		}

		/// <summary>
		/// 获取mapid
		/// </summary>
		/// <returns></returns>
		public uint GetMapId()
		{
			return m_GamingMap.m_MapId;
		}
		/// <summary>
		/// 获取GamingMap id
		/// </summary>
		/// <returns></returns>
		public uint GetGamingMapId()
		{
			return m_GamingMap.m_Uid;
		}

        public GamingMapType GetGamingMapType()
        {
            return m_GamingMap.m_Type;
        }

        public KMapPathType GetGamingMapPathType()
        {
            return m_GamingMap.m_PathType;
        }
		/// <summary>
		/// 获取npc
		/// </summary>
		/// <returns></returns>
		public Creature[] GetCreature()
		{
			return m_CreatureRoot.m_CreatureCache;
		}

		/// <summary>
		/// 获取location列表
		/// </summary>
		/// <returns></returns>
		public Location[] GetLocation()
		{
			return m_LocationRoot.m_LocationCache;
		}
        
        /// <summary>
        /// 获取触发器列表
        /// </summary>
        /// <returns></returns>
        public Trigger[] GetTrigger()
        {
            return m_TriggerRoot.m_TriggerCache;
        }

        /// <summary>
        /// 获取信号列表
        /// </summary>
        /// <returns></returns>
        public List<Treasure> GetTreasure()
        {
            if(m_TreasureRoot == null)
            {
                return null;
            }
            return m_TreasureRoot.m_TreasureCache;
        }

        public List<Mineral> GetMineral()
        {
            if(m_MineralRoot == null)
            {
                return null;
            }
            return m_MineralRoot.m_MineralCache;
        }

		public IEnumerator OnUpdate(GamingMap map, bool isExport = false)
		{
			m_GamingMap = map;
			m_AreaScenePath = string.Format("{0}/GamingMapArea_{1}_{2}.unity", m_GamingMap.GetOwnerAreaPath(), m_GamingMap.m_Uid, m_AreaId);
            gameObject.name = string.Format("{0}_{1}", m_AreaName,m_AreaId);
            if (m_CreatureRoot == null && m_LocationRoot == null&&m_TriggerRoot == null&&m_TreasureRoot == null && m_MineralRoot ==null)
			{
				yield return null;
			}
			else
			{
				if (m_CreatureRoot != null)
				{
					IEnumerator creatureRootEnumerot = m_CreatureRoot.OnUpdate(this);
					if (creatureRootEnumerot != null)
					{
						while (m_CreatureRoot != null && creatureRootEnumerot.MoveNext())
						{
							yield return null;
						}
					}
				}
				if (m_LocationRoot != null)
				{
					IEnumerator locationRootEnumerot = m_LocationRoot.OnUpdate(this);
					if (locationRootEnumerot != null)
					{
						while (m_LocationRoot != null && m_LocationRoot.gameObject != null && locationRootEnumerot.MoveNext())
						{
							yield return null;
						}
					}
				}

				if (m_TeleportRoot != null)
				{
					IEnumerator teleportRootEnumerot = m_TeleportRoot.OnUpdate(this);
					if (teleportRootEnumerot != null)
					{
						while (m_TeleportRoot != null && teleportRootEnumerot.MoveNext())
						{
							yield return null;
						}
					}
				}

                if(m_LeapRoot != null)
                {
                    IEnumerator leapRootEnumerot = m_LeapRoot.OnUpdate(this);
                    if(leapRootEnumerot != null)
                    {
                        while(m_LeapRoot != null && leapRootEnumerot.MoveNext())
                        {
                            yield return null;
                        }
                    }
                }

                if(m_TriggerRoot != null)
                {
                    IEnumerator triggerRootEnumerot = m_TriggerRoot.OnUpdate(this);
                    if(triggerRootEnumerot != null)
                    {
                        while ( m_TriggerRoot != null&& m_TriggerRoot.gameObject != null && triggerRootEnumerot != null  && triggerRootEnumerot.MoveNext())
                        {
                            yield return null;
                        }
                    }
                }

                if(m_TreasureRoot != null && m_TreasureRoot.gameObject != null)
                {
                    IEnumerator treasureRootEnumerot = m_TreasureRoot.OnUpdate(this);
                    if(treasureRootEnumerot != null)
                    {
                        while(m_TreasureRoot != null && m_TreasureRoot.gameObject != null
                            && treasureRootEnumerot !=null && treasureRootEnumerot.MoveNext())
                        {
                            yield return null;
                        }
                    }

                }

                if (m_MineralRoot != null && m_MineralRoot.gameObject != null)
                {
                    IEnumerator mineralRootEnumerot = m_MineralRoot.OnUpdate(this);
                    if (mineralRootEnumerot != null)
                    {
                        while (m_MineralRoot != null && m_MineralRoot.gameObject != null
                            && mineralRootEnumerot != null && mineralRootEnumerot.MoveNext())
                        {
                            yield return null;
                        }
                    }

                }
            }
            if(isExport)
            {
                RefreshAreaInfo();
            }
		}
		#endregion
	}
}
#endif
