#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Map
{
	[CustomEditor(typeof(BindTeleport))]
	public class BindTeleportInspector : Editor
	{
		private BindTeleport m_Target;

		private void OnEnable()
		{
			m_Target = target as BindTeleport;
		}

		public override void OnInspectorGUI()
		{
			if(m_Target.m_TeleportNames != null)
			{
				m_Target.m_SelectTeleportIndex = EditorGUILayout.Popup("Teleport ID", m_Target.m_SelectTeleportIndex, m_Target.m_TeleportNames);
				if(m_Target.teleportList != null && m_Target.teleportList.Count>m_Target.m_SelectTeleportIndex)
				{
					m_Target.m_TeleportId = m_Target.teleportList[m_Target.m_SelectTeleportIndex].ID;
				}
			}
		}
	}
}
#endif