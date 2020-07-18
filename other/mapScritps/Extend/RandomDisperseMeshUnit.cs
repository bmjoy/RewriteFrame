#if UNITY_EDITOR
using System;
using UnityEngine;
using BatchRendering;
using UnityEditor;
using Leyoutech.Utility;

namespace Map
{
    /// <summary>
    /// 随机分布unit
    /// </summary>
	public class RandomDisperseMeshUnit : SceneUnit
	{
		private RandomDisperseMesh m_RandomDisperseMesh;

		public override void DoUpdate(Map map, Area area, bool isExporting)
		{
			m_RandomDisperseMesh = gameObject.GetComponent<RandomDisperseMesh>();

			base.DoUpdate(map, area, isExporting);
		}

		public override bool CheckIsValidUnit(bool isExporting)
		{
			if (!m_RandomDisperseMesh)
			{
				Debug.LogError(string.Format("Unit({0})不是一个合法的RandomDisperseMeshUnit\nRandomDisperseMeshUnit节点上必须挂RandomDisperseMesh"
					, ObjectUtility.CalculateTransformPath(transform))
					, m_Prefab);
				return false;
			}
			return base.CheckIsValidUnit(isExporting);
		}

		public override void OnExportPrefab()
		{
			base.OnExportPrefab();

			m_RandomDisperseMesh = gameObject.GetComponent<RandomDisperseMesh>();
			if (m_RandomDisperseMesh)
			{
				m_RandomDisperseMesh.enabled = true;
				m_RandomDisperseMesh.MyRendererIn = RandomDisperseMesh.RendererIn.Game;
			}
		}

		protected override bool CaculateAABB(bool isExporting, ref Bounds aabb)
		{
			if (m_RandomDisperseMesh)
			{
				aabb = m_RandomDisperseMesh.CaculateLimitBounds();
				return true;
			}
			else
			{
				return false;
			}
		}

        /// <summary>
        /// 判断预设是否修改
        /// </summary>
        /// <param name="propertyModification"></param>
        /// <returns></returns>
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