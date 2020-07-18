#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Map
{
	[CustomEditor(typeof(Trigger))]
	public class TriggerInspector : Editor
	{
		private Trigger m_Target;
        private List<NpcVO> m_NpcListTemplateList;
        private List<NpcTriggerVO> m_TriggerListTemplateList;
        /// <summary>
		/// 上一次的npcid
		/// </summary>
		private uint m_LastNpcId;
        private void OnEnable()
		{
			m_Target = target as Trigger;
            InitSelectNpcId();
            InitSelectTriggerId();
        }

        /// <summary>
        /// 初始化当前选择的TriggerId
        /// </summary>
        private void InitSelectTriggerId()
        {
            m_TriggerListTemplateList = ConfigVO<NpcTriggerVO>.Instance.GetList();

            if (m_TriggerListTemplateList != null)
            {
                for (int iTrigger = 0; iTrigger < m_TriggerListTemplateList.Count; iTrigger++)
                {
                    if (m_TriggerListTemplateList[iTrigger].ID == m_Target.m_TriggerId)
                    {
                        m_Target.m_SelectTriggerIndex = iTrigger;
                        break;
                    }
                }
            }


            if (m_Target.m_TriggerIdArray == null || m_Target.m_TriggerIdArray.Length <= 0)
            {
                if (m_TriggerListTemplateList != null && m_TriggerListTemplateList.Count > 0)
                {
                    m_Target.m_TriggerIdArray = new string[m_TriggerListTemplateList.Count];
                    for (int iTrigger = 0; iTrigger < m_TriggerListTemplateList.Count; iTrigger++)
                    {
                        NpcTriggerVO vo = m_TriggerListTemplateList[iTrigger];
                        m_Target.m_TriggerIdArray[iTrigger] = vo.ID.ToString();
                    }
                }
            }
            m_LastNpcId = m_Target.m_NpcId;
        }

        /// <summary>
        /// 初始化当前选择的npcid
        /// </summary>
        private void InitSelectNpcId()
        {
            m_NpcListTemplateList = ConfigVO<NpcVO>.Instance.GetList();

            if (m_NpcListTemplateList != null)
            {
                for (int iNpc = 0; iNpc < m_NpcListTemplateList.Count; iNpc++)
                {
                    if (m_NpcListTemplateList[iNpc].ID == m_Target.m_NpcId)
                    {
                        m_Target.m_SelectNpcIndex = iNpc;
                        break;
                    }
                }
            }


            if (m_Target.m_NpcIdArray == null || m_Target.m_NpcIdArray.Length <= 0)
            {
                if (m_NpcListTemplateList != null && m_NpcListTemplateList.Count > 0)
                {
                    m_Target.m_NpcIdArray = new string[m_NpcListTemplateList.Count];
                    for (int iNpc = 0; iNpc < m_NpcListTemplateList.Count; iNpc++)
                    {
                        NpcVO vo = m_NpcListTemplateList[iNpc];
                        m_Target.m_NpcIdArray[iNpc] = string.Format("{0}_{1}", vo.ID, vo.Name);
                    }
                }
            }
        }

		public override void OnInspectorGUI()
		{
			EditorGUILayout.LabelField("index", m_Target.m_Index.ToString());
            //m_Target.m_TriggerName = EditorGUILayout.TextField("名称", m_Target.m_TriggerName);
            if (m_Target.m_NpcIdArray != null)
            {
                m_Target.m_SelectNpcIndex = EditorGUILayout.Popup(new GUIContent("Npc ID"), m_Target.m_SelectNpcIndex, m_Target.m_NpcIdArray);
            }

            if (m_Target.m_TriggerIdArray != null)
            {
                m_Target.m_SelectTriggerIndex = EditorGUILayout.Popup(new GUIContent("Trigger ID"), m_Target.m_SelectTriggerIndex, m_Target.m_TriggerIdArray);
            }

            //m_Target.m_TriggerType = (TriggerType)EditorGUILayout.EnumPopup("类型", m_Target.m_TriggerType);
            m_Target.m_AutoCreation = EditorGUILayout.Toggle("是否自动创建", m_Target.m_AutoCreation);
            m_Target.m_Range = EditorGUILayout.Slider("半径",m_Target.m_Range, 0.1f, 100);
            OnSelectChange();
        }

        /// <summary>
		/// 选中的发生了变化
		/// </summary>
		private void OnSelectChange()
        {
            if (m_Target.m_SelectNpcIndex >= 0)
            {
                if (m_NpcListTemplateList != null && m_NpcListTemplateList.Count > m_Target.m_SelectNpcIndex)
                {
                    m_Target.m_NpcId = (uint)m_NpcListTemplateList[m_Target.m_SelectNpcIndex].ID;
                }
            }
            if (m_Target.m_SelectTriggerIndex >= 0)
            {
                if (m_TriggerListTemplateList != null && m_TriggerListTemplateList.Count > m_Target.m_SelectTriggerIndex)
                {
                    m_Target.m_TriggerId = (uint)m_TriggerListTemplateList[m_Target.m_SelectTriggerIndex].ID;
                }
            }
            if (m_LastNpcId != m_Target.m_NpcId)
            {
                m_Target.m_ModelPath = "";
                m_Target.ShowModel();
                m_LastNpcId = m_Target.m_NpcId;
            }
        }
    }
}
#endif
