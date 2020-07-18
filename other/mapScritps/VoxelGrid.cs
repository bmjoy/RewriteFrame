#if UNITY_EDITOR
using EditorExtend;
using Leyoutech.Utility;
using UnityEngine;

namespace Map
{
	[System.Serializable]
	public class VoxelGrid
	{
		/// <summary>
		/// 每个格子的大小
		/// </summary>
		[Range(Constants.VOXEL_SIZE_MIN, Constants.VOXEL_SIZE_MAX)]
		[Tooltip("格子大小")]
		public int VoxelSize = 1000;
		/// <summary>
		/// 会有多少个Voxel
		/// </summary>
		[ReadOnly]
		public Vector3 m_VoxelCounts;
		/// <summary>
		/// l:Left,-x d:Down,-y b:Back,-z		
		/// </summary>
		private Vector3 m_LDBVoxelPosition;
		/// <summary>
		/// r:Right,x u:Up,y f:Front,z		
		/// </summary>
		private Vector3 m_RUFVoxelPosition;

		public void DoUpdate(Bounds aabb)
		{
			m_VoxelCounts = MathUtility.EachCeilToInt(aabb.size / VoxelSize);
            m_VoxelCounts = MathUtility.Clamp(m_VoxelCounts, Vector3.one, m_VoxelCounts);
            m_LDBVoxelPosition = aabb.center - VoxelSize * (m_VoxelCounts * 0.5f);
			m_RUFVoxelPosition = m_LDBVoxelPosition + VoxelSize * m_VoxelCounts;
		}

		public Vector3 GetVoxelCounts()
		{
			return m_VoxelCounts;
		}

		public Vector3 GetLDBVoxelPosition()
		{
			return m_LDBVoxelPosition;
		}

		public Vector3 GetRUFVoxelPosition()
		{
			return m_RUFVoxelPosition;
		}
	}
}
#endif