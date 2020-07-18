#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Map
{
	[CustomEditor(typeof(CreatureRoot))]
	public class CreatureRootInspector : Editor
	{
		private CreatureRoot m_Target;
		private bool m_ShowCreature;
		private void OnEnable()
		{
			m_Target = target as CreatureRoot;
		}

		public override void OnInspectorGUI()
		{
			EditorGUI.indentLevel = 0;
			m_ShowCreature = EditorGUILayout.Foldout(m_ShowCreature, "Creature列表");
			if (m_ShowCreature)
			{
				EditorGUI.indentLevel = 1;
				if (m_Target.m_CreatureCache != null && m_Target.m_CreatureCache.Length > 0)
				{
					for (int iCreature = 0; iCreature < m_Target.m_CreatureCache.Length; iCreature++)
					{
						EditorGUILayout.ObjectField(m_Target.m_CreatureCache[iCreature], typeof(Creature), true);
					}
				}

			}
			EditorGUI.indentLevel = 0;
		}
	}
}
#endif
