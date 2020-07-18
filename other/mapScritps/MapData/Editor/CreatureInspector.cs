#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Map
{
	[CustomEditor(typeof(Creature))]
	public class CreatureInspector : Editor
	{
		private Creature m_Target;
		/// <summary>
		/// npc配置
		/// </summary>
		private List<NpcVO> m_NpcListTemplateList;

		/// <summary>
		/// 是否自动创建
		/// </summary>
		private bool m_IsAutoCreate;

        /// <summary>
        /// 是否强制落地 只针对主场景
        /// </summary>
        private bool m_IsForceLand;
        private bool m_lastLand;
		/// <summary>
		/// 上一次的npcid
		/// </summary>
		private int m_LastNpcId;
        

        private void OnEnable()
		{
			m_Target = target as Creature;
			m_IsAutoCreate = m_Target.m_AutoCreation;
			List<NpcVO> allNpcs = ConfigVO<NpcVO>.Instance.GetList();
            
            if (allNpcs != null && allNpcs.Count>0)
            {
                m_NpcListTemplateList = new List<NpcVO>();
                KMapPathType mapPathType = m_Target.m_Root.GetGamingMapPathType();
                for (int iAllNpc = 0;iAllNpc<allNpcs.Count;iAllNpc++)
                {
                    NpcVO vo = allNpcs[iAllNpc];
                    if((mapPathType == KMapPathType.KMapPath_Groud && vo.motionType == (int)MotionType.Human)||
                        (mapPathType == KMapPathType.KMapPath_Space && vo.motionType == (int)MotionType.Ship))
                    {
                        m_NpcListTemplateList.Add(vo);
                    }
                }
            }

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
			m_LastNpcId = m_Target.m_NpcId;
		}

		public override void OnInspectorGUI()
		{
			EditorGUILayout.LabelField("Uid", m_Target.m_Uid.ToString());
            //if (m_Target.m_NpcIdArray != null)
            //{
            //	m_Target.m_SelectNpcIndex = EditorGUILayout.Popup(new GUIContent("Npc ID"), m_Target.m_SelectNpcIndex, m_Target.m_NpcIdArray);
            //}

            GUILayout.BeginHorizontal();
            NpcVO npcVo = ConfigVO<NpcVO>.Instance.GetData(m_Target.m_NpcId);
            if(npcVo != null)
            {
                EditorGUILayout.LabelField("Npc ID", string.Format("{0}_{1}", npcVo.ID, npcVo.Name));
            }
            else
            {
                EditorGUILayout.LabelField("Npc ID");
            }

            if (GUILayout.Button("选择"))
            {
                OnSelect();
            }

            GUILayout.EndHorizontal();
            m_IsAutoCreate = EditorGUILayout.Toggle("自动创建", m_IsAutoCreate);
            m_Target.m_AutoCreation = m_IsAutoCreate;
            OnSelectNpc();
            if(m_Target.IsShowLand())
            {
                m_IsForceLand = EditorGUILayout.Toggle("强制落地", m_IsForceLand);
                if(m_lastLand != m_IsForceLand)
                {
                    m_Target.ForceLand();
                    m_lastLand = m_IsForceLand;
                }
            }

            NpcVO vo = ConfigVO<NpcVO>.Instance.GetData(m_Target.m_NpcId);
            if(vo != null && vo.NpcType == (int)KHeroType.reliveObj)
            {
                m_Target.m_DebugShowRange = EditorGUILayout.Toggle("是否显示复活半径", m_Target.m_DebugShowRange);
                m_Target.m_ReviveRange = EditorGUILayout.Slider("复活半径:", m_Target.m_ReviveRange, 0.1f, 100f);
            }
        }

        private void OnSelect()
        {
            List<NpcVO> npcListTemplateList = null;
            ConfigInfoWindow.OpenWindow(() =>
            {
                List<NpcVO> allNpcs = ConfigVO<NpcVO>.Instance.GetList();
                if (allNpcs != null && allNpcs.Count > 0)
                {
                    npcListTemplateList = new List<NpcVO>();
                    KMapPathType mapPathType = m_Target.m_Root.GetGamingMapPathType();
                    for (int iAllNpc = 0; iAllNpc < allNpcs.Count; iAllNpc++)
                    {
                        NpcVO vo = allNpcs[iAllNpc];
                        if ((mapPathType == KMapPathType.KMapPath_Groud && vo.motionType == (int)MotionType.Human) ||
                            (mapPathType == KMapPathType.KMapPath_Space && vo.motionType == (int)MotionType.Ship))
                        {
                            npcListTemplateList.Add(vo);
                        }
                    }
                }
                return npcListTemplateList != null ? npcListTemplateList.Count : 0;
            }, (index) =>
            {
                string str = "";
                if (npcListTemplateList != null && npcListTemplateList.Count > index)
                {
                    str = $"{npcListTemplateList[index].ID}_{npcListTemplateList[index].Name}";
                }
                return str;
            }, (index) =>
            {
                m_Target.m_SelectNpcIndex = index;
            });
            //NpcInfoWindow.OpenWindow(m_Target);
        }
		/// <summary>
		/// 选中的是哪个npcid
		/// </summary>
		private void OnSelectNpc()
		{
			if (m_Target.m_SelectNpcIndex >= 0)
			{
				if (m_NpcListTemplateList != null && m_NpcListTemplateList.Count > m_Target.m_SelectNpcIndex)
				{
					m_Target.m_NpcId = m_NpcListTemplateList[m_Target.m_SelectNpcIndex].ID;
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
