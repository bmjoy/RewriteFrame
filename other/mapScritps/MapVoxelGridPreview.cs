#if UNITY_EDITOR
using UnityEngine;

namespace Map
{
	public class MapVoxelGridPreview : MonoBehaviour
	{
		private MapInfo m_MapInfo;
		private Transform m_AreaTransform;

		public void Initialize(MapInfo map)
		{
			m_MapInfo = map;
			gameObject.name = Constants.MAP_VOXELGRID_PREVIEW_GAMEOBJECT_NAME_STARTWITHS;
		}

		protected void OnDrawGizmosSelected()
		{
			if (m_MapInfo == null)
			{
				return;
			}

			if (m_AreaTransform == null)
			{
				m_AreaTransform = GameObject.CreatePrimitive(PrimitiveType.Sphere).transform;
				m_AreaTransform.SetParent(transform, false);
			}

			int areaIndex = m_MapInfo.CaculateAreaIndex(transform.localPosition);
			if (areaIndex == Constants.NOTSET_AREA_INDEX)
			{
				m_AreaTransform.gameObject.SetActive(false);
			}
			else
			{
				m_AreaTransform.gameObject.SetActive(true);

				AreaInfo areaInfo = m_MapInfo.AreaInfos[areaIndex];
				m_AreaTransform.localScale = Vector3.one * areaInfo.Diameter;
				m_AreaTransform.position = areaInfo.Position;
			}
		}
	}
}
#endif