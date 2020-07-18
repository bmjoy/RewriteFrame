#if UNITY_EDITOR
using System;
using UnityEngine;
using BatchRendering;
using UnityEditor;
using Leyoutech.Utility;

namespace Map
{
    /// <summary>
    /// 支持多个mesh的随机分布unit
    /// </summary>
	public class RandomDisperseMulMeshUnit : SceneUnit
	{
		private RandomDisperseMulMesh m_RandomDisperseMulMesh;

		public override void DoUpdate(Map map, Area area, bool isExporting)
		{
            m_RandomDisperseMulMesh = gameObject.GetComponent<RandomDisperseMulMesh>();

			base.DoUpdate(map, area, isExporting);
		}

		public override bool CheckIsValidUnit(bool isExporting)
		{
			if (!m_RandomDisperseMulMesh)
			{
				Debug.LogError(string.Format("Unit({0})不是一个合法的RandomDisperseMulMeshUnit\nRandomDisperseMulMeshUnit节点上必须挂RandomDisperseMulMesh"
					, ObjectUtility.CalculateTransformPath(transform))
					, m_Prefab);
				return false;
			}
			return base.CheckIsValidUnit(isExporting);
		}

		public override void OnExportPrefab()
		{
			base.OnExportPrefab();

            m_RandomDisperseMulMesh = gameObject.GetComponent<RandomDisperseMulMesh>();
			if (m_RandomDisperseMulMesh)
			{
                m_RandomDisperseMulMesh.enabled = true;
                m_RandomDisperseMulMesh.MyRendererIn = RandomDisperseMesh.RendererIn.Game;
			}
		}

		protected override bool CaculateAABB(bool isExporting, ref Bounds aabb)
		{
			if (m_RandomDisperseMulMesh)
			{
				aabb = m_RandomDisperseMulMesh.CaculateLimitBounds();
				return true;
			}
			else
			{
				return false;
			}
		}

		protected override bool IsPrefabOverrideModification(PropertyModification propertyModification)
		{
			bool isOverrideModification;
			if (propertyModification.target == m_Prefab.GetComponent<RandomDisperseMeshUnit>())
			{
				isOverrideModification = false;
			}
			else if (propertyModification.target is RandomDisperseMesh)
			{
				isOverrideModification = !Constants.PREFAB_RANDOM_DISPERSE_MESH_UNIT_IGNORE_MODIFICATION_PROPERTY_PATHS.Contains(propertyModification.propertyPath);
			}
			else
			{
				isOverrideModification = true;
			}
			return isOverrideModification;
		}
	}
}
#endif