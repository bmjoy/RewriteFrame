#if UNITY_EDITOR
using Leyoutech.Utility;
using System;
using System.Collections;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Map
{
	/// <summary>
	/// 随机分布
	/// </summary>
	public class RandomDisperse : MonoBehaviour
	{
		private static readonly Color GIZMOS_COLOR = new Color(0.5f, 0f, 0f, 0.5f);
		/// <summary>
		/// 每帧实例化多少个Unit
		/// </summary>
		private const int INSTANTIATE_UNIT_COUNT_EVERFRAME = 1000;

		/// <summary>
		/// 散布的半径
		/// </summary>
		public float Radius;
		/// <summary>
		/// 散布的物体数量
		/// </summary>
		public int Count;
		/// <summary>
		/// 要散布的物体模板
		/// </summary>
		public UnitTemplate[] UnitTemplates;
		/// <summary>
		/// 在哪个节点下分布
		/// 如果为null，则在场景根目录下
		/// </summary>
		public Transform DisperseParent;
		public bool DisplayGizmos;

		private IEnumerator m_DisperseEnumerator;
		private bool m_IsDispersing = false;

		[ContextMenu("Disperse")]
		public void BeginDisperse()
		{
			if (m_IsDispersing)
			{
				Debug.LogError("上一次的Disperse还未完成，请稍后重试");
				return;
			}
			m_IsDispersing = true;
			m_DisperseEnumerator = Disperse_Co();
			EditorApplication.update += OnUpdate_Disperse;
		}

		[ContextMenu("Destroy All Childern")]
		public void DestroyAllChildern()
		{
			ObjectUtility.DestroyAllChildern(transform);
		}

		private void EndDisperse()
		{
			m_IsDispersing = false;
			EditorApplication.update -= OnUpdate_Disperse;
		}

		private void OnUpdate_Disperse()
		{
			m_DisperseEnumerator.MoveNext();
		}

		private IEnumerator Disperse_Co()
		{
			float totalWeight = 0;
			int[] unitCounts = new int[UnitTemplates.Length];
			for (int iTemplate = 0; iTemplate < UnitTemplates.Length; iTemplate++)
			{
				totalWeight += UnitTemplates[iTemplate].Weight;
				unitCounts[iTemplate] = 0;
			}

			yield return null;
			for (int iUnit = 0; iUnit < Count; iUnit++)
			{
				if (iUnit % INSTANTIATE_UNIT_COUNT_EVERFRAME == 0)
				{
					if (EditorUtility.DisplayCancelableProgressBar("Disperse"
						, string.Format("{0} / {1}", iUnit, Count)
						, iUnit / Count))
					{
						break;
					}
					yield return null;
				}

				float randomWeight = UnityEngine.Random.Range(0, totalWeight);
				int templateIndex = 0;
				for (int iTemplate = 0; iTemplate < UnitTemplates.Length; iTemplate++)
				{
					randomWeight -= UnitTemplates[iTemplate].Weight;
					if (randomWeight < 0)
					{
						templateIndex = iTemplate;
						break;
					}
				}

				UnitTemplate unitTemplate = UnitTemplates[templateIndex];
				GameObject iterUnit = unitTemplate.IsPrefab
					? UnityEditor.PrefabUtility.InstantiatePrefab(unitTemplate.Unit) as GameObject
					: Instantiate(unitTemplate.Unit);

				if (DisperseParent != null)
				{
					iterUnit.transform.SetParent(DisperseParent, false);
				}
				iterUnit.transform.localPosition = RandomUtility.RandomInSphere(Radius);
				iterUnit.transform.localEulerAngles = RandomUtility.RandomEulerAngles();
				unitCounts[templateIndex]++;
			}

			StringBuilder stringBuilder = StringUtility.AllocStringBuilderCache();
			for (int iTemplate = 0; iTemplate < unitCounts.Length; iTemplate++)
			{
				stringBuilder.Append("Generate Index: ").Append(iTemplate).Append(" Count: ").Append(unitCounts[iTemplate]).Append("\n");
			}
			Debug.Log(StringUtility.ReleaseStringBuilderCacheAndReturnString());


			yield return null;
			EditorUtility.ClearProgressBar();
			EndDisperse();
		}

		private void OnDrawGizmosSelected()
		{
			if (DisplayGizmos)
			{
				Gizmos.color = GIZMOS_COLOR;
				Gizmos.DrawSphere(transform.position, Radius);
			}
		}

		[Serializable]
		public struct UnitTemplate
		{
			public GameObject Unit;
			/// <summary>
			/// 这个值越高，这个物体出现的频率也就越高
			/// </summary>
			public float Weight;
			/// <summary>
			/// 是否是Prefab
			/// </summary>
			public bool IsPrefab;
		}
	}
}
#endif