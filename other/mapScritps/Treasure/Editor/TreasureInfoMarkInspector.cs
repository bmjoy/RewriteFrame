#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Map
{
    [CustomEditor(typeof(TreasureInfoMark))]
    public class TreasureInfoMarkInspector : Editor
    {
        private TreasureInfoMark m_Target;
        private List<NpcVO> allNpcs;
        private uint m_LastNpcId;
        private void OnEnable()
        {
            m_Target = target as TreasureInfoMark;
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
        }

        public override void OnInspectorGUI()
        {
            //m_Target.m_GroupId = (uint)EditorGUILayout.IntField("GroupId:", (int)m_Target.m_GroupId);
            //EditorGUILayout.LabelField("GroupId:", m_Target.m_GroupId.ToString());
            if (m_Target.m_NpcIdArray != null)
            {
                m_Target.m_SelectNpcIndex = EditorGUILayout.Popup(new GUIContent("Npc ID"), m_Target.m_SelectNpcIndex, m_Target.m_NpcIdArray);
            }
            OnSelectNpc();
            if(m_LastNpcId != m_Target.m_NpcId)
            {
                EditorUtility.SetDirty(m_Target);
                m_LastNpcId = m_Target.m_NpcId;
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