using UnityEngine;

namespace Map
{
	public class LODSwitchItemGroup : MonoBehaviour, ILODItem
	{
		public LODGroup LODGroup;
		public SwitchItem[] Switchs;

		private int m_LastLODIndex;

		public void DoUpdateLOD(int lodIndex)
		{
			if (lodIndex != m_LastLODIndex)
			{
				bool lastLODIndexValid = IsValidIndex(m_LastLODIndex);
				bool lodIndexValid = IsValidIndex(lodIndex);
				//Leyoutech.Utility.DebugUtility.Log(Constants.LOD_LOG_TAG
				//	, string.Format("LODSwitchItem({0}) changed from {1}({2}) to {3}({4})"
				//		, gameObject.name
				//		, m_LastLODIndex
				//		, lastLODIndexValid && Switchs[m_LastLODIndex].Item
				//			? Switchs[m_LastLODIndex].Item.name
				//			: "None"
				//		, lodIndex
				//		, lodIndexValid && Switchs[lodIndex].Item
				//			? Switchs[lodIndex].Item.name
				//			: "None"));

				if (lastLODIndexValid)
				{					
					Switchs[m_LastLODIndex].SetActive(false);
				}
				if (lodIndexValid)
				{
					Switchs[lodIndex].SetActive(true);
				}

				m_LastLODIndex = lodIndex;
			}
		}

		public LODGroup GetLODGroup()
		{
			return LODGroup;
		}

		public int GetMaxDisplayLODIndex()
		{
			return Constants.NOTSET_LOD_INDEX;
		}

		public bool IsAlive()
		{
			return this != null;
		}

		public void SetLODItemActive(bool active)
		{
		}

		protected void OnEnable()
		{
			if (LODGroup)
			{
				LODManager.GetInstance().AddLODItem(this);
			}

			m_LastLODIndex = Constants.NOTSET_LOD_INDEX;
			for (int iSwitch = 0; iSwitch < Switchs.Length; iSwitch++)
			{
				Switchs[iSwitch].SetActive(false);
			}
		}

		protected void OnDisable()
		{
			LODManager.GetInstance().RemoveLODItem(this);
		}

		private bool IsValidIndex(int lodIndex)
		{
			return lodIndex >= 0
				&& lodIndex < Switchs.Length;
		}

		[System.Serializable]
		public struct SwitchItem
		{
			public GameObject[] Items;

			public void SetActive(bool active)
			{
				if (Items != null)
				{
                    for (int i = 0; i < Items.Length; i++)
                    {
                        Items[i].SetActive(active);
                    }
				}
			}
		}
	}
}