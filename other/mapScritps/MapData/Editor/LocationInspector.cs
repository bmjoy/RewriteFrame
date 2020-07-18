#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Map
{
	[CustomEditor(typeof(Location))]
	public class LocationInspector : Editor
	{
		private Location m_Target;
        private bool m_IsForceLand;
        private bool m_lastLand;
        /// <summary>
        /// 选中的类型
        /// </summary>
        private int m_SelectTypeIndex;
		private void OnEnable()
		{
			m_Target = target as Location;
			m_SelectTypeIndex = (int)m_Target.m_LocationType;
		}

		public override void OnInspectorGUI()
		{
			EditorGUILayout.LabelField("Location Id", m_Target.m_Uid.ToString());
			m_Target.m_LocationName = EditorGUILayout.TextField("名称", m_Target.m_LocationName);
			m_SelectTypeIndex = EditorGUILayout.Popup(new GUIContent("类型"), m_SelectTypeIndex, EditorGamingMapData.LOCATIONTYPE_NAME);
			OnSelectLocationType();
            if (m_Target.IsShowLand())
            {
                m_IsForceLand = EditorGUILayout.Toggle("强制落地", m_IsForceLand);
                if (m_lastLand != m_IsForceLand)
                {
                    m_Target.ForceLand();
                    m_lastLand = m_IsForceLand;
                }
            }
        }

		private void OnSelectLocationType()
		{
			m_Target.m_LocationType = (LocationType)m_SelectTypeIndex;
		}
	}
}
#endif
