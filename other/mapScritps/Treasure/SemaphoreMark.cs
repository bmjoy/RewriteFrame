#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Map
{
    /// <summary>
    /// 用于标记是否为信号量
    /// </summary>
    [ExecuteInEditMode]
    public class SemaphoreMark : MonoBehaviour
    {
        public uint m_GroupId;
        public uint m_NpcId;
        public TreasureInfoMark[] m_TreasureInfoMarks;

        public TreasureInfoMark[] TreasureInfoMarks
        {
            get
            {
                m_TreasureInfoMarks = GetComponentsInChildren<TreasureInfoMark>();
                return m_TreasureInfoMarks;
            }
        }
        /// <summary>
		/// 当前选中的npc列表中的哪个
		/// </summary>
        [System.NonSerialized]
        public int m_SelectNpcIndex = -1;
        [System.NonSerialized]
        public string[] m_NpcIdArray;

        private void OnEnable()
        {
            if(Application.isPlaying)
            {
                return;
            }
            InitConfig();
        }

        private void InitConfig()
        {
            if (EditorGamingMapData.m_VoNameDic == null)
            {
                EditorGamingMapData.m_VoNameDic = new Dictionary<string, List<string>>();
            }
            if (!EditorGamingMapData.m_VoNameDic.ContainsKey(typeof(NpcVO).Name))
            {
                List<string> npcListInfo = new List<string>();
                npcListInfo.Add("npc.csv");
                npcListInfo.Add("npc");
                EditorGamingMapData.m_VoNameDic.Add(typeof(NpcVO).Name, npcListInfo);
            }
        }

        public void DoUpdate()
        {
            m_TreasureInfoMarks = GetComponentsInChildren<TreasureInfoMark>();
            if(m_TreasureInfoMarks != null && m_TreasureInfoMarks.Length>0)
            {
                for(int iTreasure =0;iTreasure<m_TreasureInfoMarks.Length;iTreasure++)
                {
                    m_TreasureInfoMarks[iTreasure].DoUpdate(m_GroupId);
                }
            }
        }
    }
}
#endif