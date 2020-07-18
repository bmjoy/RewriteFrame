#if UNITY_EDITOR
using BatchRendering;
using EditorExtend;
using Leyoutech.Utility;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Map
{
	/// <summary>
	/// <see cref="http://wiki.leyoutech.com/pages/viewpage.action?pageId=17761761"/>
	/// </summary>
	[ExecuteInEditMode]
	public class Area : MonoBehaviour
	{
		/// <summary>
		/// 这个区域的唯一Id，用于策划填表
		/// </summary>
		//[Range(Constants.AREA_UID_MIN, Constants.AREA_UID_MAX), Tooltip("Area Id")]
		[ReadOnly]
		public ulong Uid;
		/// <summary>
		/// 自动计算Uid（自增长）
		/// </summary>
		[Tooltip("是否开启增长Area Id")]
		[HideInInspector]
		public bool AutoUid = false;
		/// <summary>
		/// 这个Area下的Voxel
		/// </summary>
		[Tooltip("Area下的格子信息")]
		public VoxelGrid VoxelGrid;
		/// <summary>
		/// Debug的信息
		/// </summary>
		[Header("DEBUG"), Tooltip("Area调试信息")]
		public ForDebug _Debug;
		/// <summary>
		/// 这个区域所属的Map
		/// </summary>
		private Map m_OwnerMap;
		/// <summary>
		/// 这个区域所属的AreaSpawner
		/// </summary>
		private AreaSpawner m_OwnerAreaSpawner;
		/// <summary>
		/// Area的AABB，用于计算这个Area需要多少Voxel
		/// </summary>
		public Bounds m_AABB;
		/// <summary>
		/// Area的直径
		/// </summary>
		private float m_Diameter;
        

        public MapLeap m_Leap;
        
        /// <summary>
        /// 寻宝节点
        /// </summary>
        public TreasureRootMark m_TreasureRoot;
        /// <summary>
        /// 采矿节点
        /// </summary>
        public MineralRootMark m_MineralRoot;

        public List<SemaphoreMark> GetSemaphoreMarks()
        {
            if(m_TreasureRoot == null)
            {
                return null;
            }
            return m_TreasureRoot.m_SemaphorMarkCache;
        }

        public List<SemaphoreMark> GetMineralSemaphoreMarks()
        {
            if (m_MineralRoot == null)
            {
                return null;
            }
            return m_MineralRoot.m_SemaphorMarkCache;
        }

        public Bounds GetAABB()
		{
			return m_AABB;
		}

		public float GetDiameter()
		{
			return m_Diameter;
		}

		public AreaSpawner GetAreaSpawner()
		{
			return m_OwnerAreaSpawner;
		}
        

        public RecastRegionInfo[] FindRegionInfos()
        {
            return GetComponentsInChildren<RecastRegionInfo>();
        }

		public IEnumerator DoUpdate_Co(Map map, int indexInMap, bool isExporting,AreaSpawner areaSpawner = null)
		{
            m_Leap = GetComponentInChildren<MapLeap>();
            m_TreasureRoot = GetComponentInChildren<TreasureRootMark>();
            if(m_TreasureRoot != null)
            {
                IEnumerator treasureEnumerator =  m_TreasureRoot.DoUpdate();
                if(treasureEnumerator != null)
                {
                    while(treasureEnumerator.MoveNext())
                    {
                        yield return null;
                    }
                }
            }
            yield return null;
            map.LoadExtenInfo(this);
            m_MineralRoot = GetComponentInChildren<MineralRootMark>();
            if (m_MineralRoot != null)
            {
                IEnumerator mineralEnumerator = m_MineralRoot.DoUpdate();
                if (mineralEnumerator != null)
                {
                    while (mineralEnumerator.MoveNext())
                    {
                        yield return null;
                    }
                }
            }
            yield return null;
            if (m_Leap != null)
            {
                m_Leap.DoUpdate(areaSpawner.m_AreaUid);
            }
            m_OwnerMap = map;
			m_OwnerAreaSpawner = areaSpawner;
			transform.localScale = Vector3.one;// 美术说Area不会有缩放
			if(m_OwnerAreaSpawner != null)
			{
				gameObject.name = m_OwnerAreaSpawner.GetAreaSpawnObjName();
			}
            Bounds areaAABB = new Bounds();
            MapEditorUtility.CaculateAABB(transform, ref areaAABB, isExporting);
            m_AABB = areaAABB;
			m_Diameter = MathUtility.CaculateDiagonal(m_AABB);
			if(VoxelGrid != null)
			{
				VoxelGrid.DoUpdate(m_AABB);
			}
			
			_Debug.DoUpdate(this);
		}
        
        protected void OnEnable()
		{
			// 这个脚本只在编辑器下使用
			if (Application.isPlaying)
			{
				Destroy(this);
				return;
			}
		}

		protected void OnDrawGizmosSelected()
		{
			// HACK 为了能实时在Inspector面板上的Debug中看到Voxel信息，所以在这里也Update
			if(VoxelGrid != null)
			{
				VoxelGrid.DoUpdate(m_AABB);
			}
			_Debug.DoUpdate(this);
			_Debug.DoDrawGizmos(this);
		}
        
		/// <summary>
		/// Debug的信息
		/// </summary>
		[System.Serializable]
		public struct ForDebug
		{
			/// <summary>
			/// Area的直径
			/// </summary>
			[Tooltip("Area直径")]
			public float Diameter;
			/// <summary>
			/// 是否显示Gizmos
			/// </summary>
			[Tooltip("是否显示Gizmos")]
			public bool DisplayGizmos;
			/// <summary>
			/// AABB的Gizmos颜色
			/// </summary>
			[Tooltip("区域显示颜色")]
			public Color AABBGizmosColor;
			[Tooltip("格子调试信息")]
			public VoxelDebug VoxelDebug;

			/// <summary>
			/// 在编辑器中看的备注，不是给玩家看的
			/// </summary>
			[TextArea]
			public string Remark;

			public void DoDrawGizmos(Area owner)
			{
				if (DisplayGizmos)
				{
					GizmosUtility.DrawBounds(owner.GetAABB(), AABBGizmosColor);
				}
				VoxelDebug.DoDrawGizmos(owner.VoxelGrid, owner.GetAABB());
			}

			public void DoUpdate(Area owner)
			{
				Diameter = MathUtility.CaculateDiagonal(owner.GetAABB());
				if(VoxelDebug != null && owner != null)
				{
					VoxelDebug.DoUpdate(owner.VoxelGrid, owner.gameObject);
				}
			}
		}
		
	}
}
#endif