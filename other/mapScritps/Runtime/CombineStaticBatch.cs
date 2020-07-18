using System.Collections.Generic;
using UnityEngine;

namespace Map
{ 
	public class CombineStaticBatch : MonoBehaviour
	{ 
		public CombineType MyCombineType = CombineType.AllChildren;
		public GameObject[] SpecifiedGameObjects;

		private GameObject[] m_StaticBatchGameObjects;
		private TransformRecord[] m_TransfromRecords;

		protected void Awake()
		{
			List<GameObject> staticBatchGameObjects = new List<GameObject>();
			switch (MyCombineType)
			{
				case CombineType.AllChildren:
					Leyoutech.Utility.ObjectUtility.CollectAllChildren(staticBatchGameObjects, transform, true);
					break;
				case CombineType.AllChildrenWithoutDeactivate:
					Leyoutech.Utility.ObjectUtility.CollectAllChildren(staticBatchGameObjects, transform, false);
					break;
				case CombineType.Specified:
					if (SpecifiedGameObjects != null)
					{
						for (int iSpecified = 0; iSpecified < SpecifiedGameObjects.Length; iSpecified++)
						{
							GameObject iterGameObject = SpecifiedGameObjects[iSpecified];
							if (iterGameObject == null)
							{
								Leyoutech.Utility.DebugUtility.LogError(Constants.LOG_TAG
									, string.Format("CombineStaticBatch.SpecifiedGameObjects index({0}) is null in ({1})"
										, iSpecified
										, Leyoutech.Utility.ObjectUtility.CalculateTransformPath(transform))
									, gameObject);
								continue;
							}

							if (!iterGameObject.transform.IsChildOf(transform))
							{
								Leyoutech.Utility.DebugUtility.LogError(Constants.LOG_TAG
									, string.Format("CombineStaticBatch.SpecifiedGameObjects index({0}) is not a child of ({1})"
										, iSpecified
										, Leyoutech.Utility.ObjectUtility.CalculateTransformPath(transform))
									, iterGameObject);
								continue;
							}

							staticBatchGameObjects.Add(iterGameObject);
						}
					}
					else
					{
						Leyoutech.Utility.DebugUtility.LogError(Constants.LOG_TAG
							, string.Format("CombineStaticBatch.SpecifiedGameObjects is null in ({0})"
								, Leyoutech.Utility.ObjectUtility.CalculateTransformPath(transform))
							, gameObject);
					}
					break;
				default:
					Leyoutech.Utility.DebugUtility.Assert(false, Constants.LOG_TAG, "Not support CombineType: " + MyCombineType);
					break;
			}

			if (staticBatchGameObjects.Count == 0)
			{
				Leyoutech.Utility.DebugUtility.LogWarning(Constants.LOG_TAG
					, string.Format("CombineStaticBatch.staticBatchGameObjects count is zero in ({0})"
						, Leyoutech.Utility.ObjectUtility.CalculateTransformPath(transform))
					, gameObject);
			}

			m_StaticBatchGameObjects = staticBatchGameObjects.ToArray();
#if UNITY_EDITOR
			if (Constants.ENABLE_COMBINE_STATIC_BATCH_COMBINE)
#endif
			{
#pragma warning disable 162
#if UNITY_EDITOR
				long startTicks = Leyoutech.Utility.DebugUtility.GetTicksSinceStartup();
#endif
				StaticBatchingUtility.Combine(m_StaticBatchGameObjects, gameObject);
#if UNITY_EDITOR
				long elapsedTicks = Leyoutech.Utility.DebugUtility.GetTicksSinceStartup() - startTicks;
#endif
				Leyoutech.Utility.DebugUtility.Log(Constants.LOG_TAG
				, string.Format("CombineStaticBatch.Combine staticBatch count:{0} in ({1})"
						, m_StaticBatchGameObjects.Length
						, gameObject.name)
#if UNITY_EDITOR
					+ string.Format(" Elapsed {0:F3} ms", (elapsedTicks / 10000.0))
#endif
				, gameObject);
#pragma warning restore 162
			}


#pragma warning disable 162
			if (Constants.DEBUG_COMBINE_STATIC_BATCH_TRANFORM)
			{
				m_TransfromRecords = new TransformRecord[m_StaticBatchGameObjects.Length];
				for (int iGameObject = 0; iGameObject < m_StaticBatchGameObjects.Length; iGameObject++)
				{
					m_TransfromRecords[iGameObject] = new TransformRecord(m_StaticBatchGameObjects[iGameObject].transform);
				}
			}
			else
			{
				Destroy(this);
			}
#pragma warning restore 162
		}

		protected void LateUpdate()
		{
#pragma warning disable 162
			if (Constants.DEBUG_COMBINE_STATIC_BATCH_TRANFORM)
			{
				for (int iGameObject = 0; iGameObject < m_StaticBatchGameObjects.Length; iGameObject++)
				{
					if (!TransformRecord.Equals(m_TransfromRecords[iGameObject], m_StaticBatchGameObjects[iGameObject].transform))
					{
						Leyoutech.Utility.DebugUtility.LogWarning(Constants.LOG_TAG
							, string.Format("CombineStaticBatch.m_StaticBatchGameObjects index({0}) transfrom has changed in ({0})"
								, iGameObject
								, Leyoutech.Utility.ObjectUtility.CalculateTransformPath(transform))
							, gameObject);
					}
				}
			}
#pragma warning restore 162
		}


		public enum CombineType
		{
			AllChildren = 0,
			AllChildrenWithoutDeactivate = 1,
			Specified = 2,
		}

		private struct TransformRecord
		{
			public Vector3 LocalPosition;
			public Vector3 LocalScale;
			public Quaternion LocalRotation;

			public static bool Equals(TransformRecord a, Transform b)
			{
				return Equals(a, new TransformRecord(b));
			}

			public static bool Equals(TransformRecord a, TransformRecord b)
			{
				return a.LocalPosition == b.LocalPosition
					&& a.LocalScale == b.LocalScale
					&& a.LocalRotation == b.LocalRotation;
			}

			public TransformRecord(Transform transform)
			{
				LocalPosition = transform.localPosition;
				LocalScale = transform.localScale;
				LocalRotation = transform.localRotation;
			}
		}
	}
}