#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Map
{
    [CustomEditor(typeof(SemaphoreMark))]
    public class SemaphoreMarkInspector : Editor
    {
        private SemaphoreMark m_Target;
        private List<NpcVO> allNpcs;
        private uint m_LastNpcId;
        private uint m_LastGroupId;
        private void OnEnable()
        {
            m_Target = target as SemaphoreMark;
            allNpcs = ConfigVO<NpcVO>.Instance.GetList();

            if (allNpcs != null)
            {
                for (int iNpc = 0; iNpc < allNpcs.Count; iNpc++)
                {
                    if (allNpcs[iNpc].ID == m_Target.m_NpcId)
                    {
                        m_Target.m_SelectNpcIndex = iNpc;
                        break;
                    }
                }
            }

            if (m_Target.m_NpcIdArray == null || m_Target.m_NpcIdArray.Length <= 0)
            {
                if (allNpcs != null && allNpcs.Count > 0)
                {
                    m_Target.m_NpcIdArray = new string[allNpcs.Count];
                    for (int iNpc = 0; iNpc < allNpcs.Count; iNpc++)
                    {
                        NpcVO vo = allNpcs[iNpc];
                        m_Target.m_NpcIdArray[iNpc] = string.Format("{0}_{1}", vo.ID, vo.Name);
                    }
                }
            }
            m_LastNpcId = m_Target.m_NpcId;
            m_LastGroupId = m_Target.m_GroupId;
        }

        public override void OnInspectorGUI()
        {
            m_Target.m_GroupId = (uint)EditorGUILayout.IntField("GroupId:", (int)m_Target.m_GroupId);
            if (m_Target.m_NpcIdArray != null)
            {
                m_Target.m_SelectNpcIndex = EditorGUILayout.Popup(new GUIContent("Npc ID"), m_Target.m_SelectNpcIndex, m_Target.m_NpcIdArray);
            }
            OnSelectNpc();
            if(m_LastNpcId != m_Target.m_NpcId || m_LastGroupId != m_Target.m_GroupId)
            {
                EditorUtility.SetDirty(m_Target);
                m_LastNpcId = m_Target.m_NpcId;
                m_LastGroupId = m_Target.m_GroupId;
            }
            
        }

        /// <summary>
		/// 选中的是哪个npcid
		/// </summary>
		private void OnSelectNpc()
        {
            if (m_Target.m_SelectNpcIndex >= 0)
            {
                if (allNpcs != null && allNpcs.Count > m_Target.m_SelectNpcIndex)
                {
                    m_Target.m_NpcId = (uint)allNpcs[m_Target.m_SelectNpcIndex].ID;
                }
            }
        }
    }
}

#endif