#if UNITY_EDITOR
using EditorExtend;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Map
{
    [ExecuteInEditMode]
    public class TreasureInfoMark : MonoBehaviour
    {
        public uint m_NpcId;
        [ReadOnly]
        public uint m_GroupId;

        /// <summary>
		/// 当前选中的npc列表中的哪个
		/// </summary>
        [System.NonSerialized]
        public int m_SelectNpcIndex = -1;
        [System.NonSerialized]
        public string[] m_NpcIdArray;

        private void OnEnable()
        {
            if (Application.isPlaying)
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

        public void DoUpdate(uint groupId)
        {
            m_GroupId = groupId;
        }
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}

#endif