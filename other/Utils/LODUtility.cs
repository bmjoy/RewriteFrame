using UnityEngine;

namespace Leyoutech.Utility
{
	public static class LODUtility
	{
		/// <summary>
		/// 获取当前的LOD
		/// </summary>
		public static int CaculateCurrentLOGIndex(LODGroup lodGroup, Camera camera)
		{
			return CaculateCurrentLOGIndex(lodGroup
				, CaculateLODGroupRelativeHeightInScreen(lodGroup, camera));
		}

		/// <summary>
		/// 获取当前的LOD
		/// </summary>
		public static int CaculateCurrentLOGIndex(LODGroup lodGroup, Vector3 cameraPosition, float halfTanFov)
		{
			return CaculateCurrentLOGIndex(lodGroup
				, CaculateLODGroupRelativeHeightInScreen(lodGroup, cameraPosition, halfTanFov));
		}

		/// <summary>
		/// 获取当前的LOD
		/// </summary>
		public static int CaculateCurrentLOGIndex(LODGroup lodGroup, float relativeHeight)
		{
			LOD[] lods = lodGroup.GetLODs();
			for (int iLod = 0; iLod < lods.Length; iLod++)
			{
				if (relativeHeight >= lods[iLod].screenRelativeTransitionHeight)
				{
					return iLod;
				}
			}

			return lodGroup.lodCount;
		}

		/// <summary>
		/// LODGroup在屏幕上占的高度 0~1
		/// </summary>
		public static float CaculateLODGroupRelativeHeightInScreen(LODGroup lodGroup, Vector3 cameraPosition, float halfTanFov)
		{
			float distance = (lodGroup.transform.TransformPoint(lodGroup.localReferencePoint)
					- cameraPosition)
				.magnitude;
			return RendererUtility.CaculateRelativeHeightInScreen(CaculateLODGroupSize_WorldSpace(lodGroup) * 0.5f, distance, halfTanFov);
		}

		/// <summary>
		/// LODGroup在屏幕上占的高度 0~1
		/// </summary>
		public static float CaculateLODGroupRelativeHeightInScreen(LODGroup lodGroup, Camera camera)
		{
			float distance = (lodGroup.transform.TransformPoint(lodGroup.localReferencePoint)
					- camera.transform.position)
				.magnitude;
			return RendererUtility.CaculateRelativeHeightInScreen(CaculateLODGroupSize_WorldSpace(lodGroup) * 0.5f, distance, camera);
		}

		/// <summary>
		/// LODGroup在世界空间的大小
		/// </summary>
		public static float CaculateLODGroupSize_WorldSpace(LODGroup lodGroup)
		{
			Vector3 lossyScale = lodGroup.transform.lossyScale;
			float largestAxis = Mathf.Abs(lossyScale.x);
			largestAxis = Mathf.Max(largestAxis, Mathf.Abs(lossyScale.y));
			largestAxis = Mathf.Max(largestAxis, Mathf.Abs(lossyScale.z));
			return largestAxis * lodGroup.size;
		}
	}
}