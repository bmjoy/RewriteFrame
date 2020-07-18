#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Map
{
	[CustomEditor(typeof(LocationRoot))]
	public class LocationRootInspector : Editor
	{
		private LocationRoot m_Target = null;
		private bool m_ShowLocation;
		private void OnEnable()
		{
			m_Target = target as LocationRoot;
		}

		public override void OnInspectorGUI()
		{
			EditorGUI.indentLevel = 0;
			m_ShowLocation = EditorGUILayout.Foldout(m_ShowLocation, "Location列表");
			if (m_ShowLocation)
			{
				EditorGUI.indentLevel = 1;
				if (m_Target.m_LocationCache != null && m_Target.m_LocationCache.Length > 0)
				{
					for (int iTeleport = 0; iTeleport < m_Target.m_LocationCache.Length; iTeleport++)
					{
						EditorGUILayout.ObjectField(m_Target.m_LocationCache[iTeleport], typeof(Location), true);
					}
				}

			}
			EditorGUI.indentLevel = 0;
		}
	}
}
#endif
