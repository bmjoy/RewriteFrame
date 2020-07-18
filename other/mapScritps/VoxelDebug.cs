#if UNITY_EDITOR
using Leyoutech.Utility;
using UnityEngine;

namespace Map
{
	/// <summary>
	/// Debug相关
	/// </summary>
	[System.Serializable]
	public class VoxelDebug
	{
		/// <summary>
		/// 这个Area会生成的Voxel数量
		/// </summary>
		[Tooltip("格子总数量")]
		public int VoxelCount;
		/// <summary>
		/// 这个Area会生成的Voxel数量
		/// </summary>
		[Tooltip("X Y Z三方向格子数量")]
		public Vector3 VoxelCounts;

		/// <summary>
		/// 是否显示Gizmos
		/// </summary>
		[Tooltip("是否显示Gizmos")]
		public bool DisplayGizmos;
		/// <summary>
		/// Voxle的Gizmos颜色
		/// </summary>
		[Tooltip("Gizmos颜色")]
		public Color VoxelGizmosColor = Color.gray;

		public void DoUpdate(VoxelGrid owner, GameObject content)
		{
			VoxelCounts = owner.GetVoxelCounts();
			VoxelCount = (int)(VoxelCounts.x * VoxelCounts.y * VoxelCounts.z);
			DebugUtility.Assert(VoxelCount > 0, "Voxel的数量应该大于0，是不是忘记修改Voxel的大小了", content, false);
		}

		public void DoDrawGizmos(VoxelGrid owner, Bounds aabb)
		{
			if (DisplayGizmos)
			{
				Vector3 voxelSize = owner.VoxelSize * Vector3.one;
				Bounds voxel = new Bounds(aabb.center, voxelSize);
				GizmosUtility.DrawBounds(voxel, VoxelGizmosColor);
				voxel.center = owner.GetLDBVoxelPosition();
				GizmosUtility.DrawBounds(voxel, VoxelGizmosColor);
				voxel.center = owner.GetRUFVoxelPosition();
				GizmosUtility.DrawBounds(voxel, VoxelGizmosColor);
			}
		}
	}
}
#endif