#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Map
{
    /// <summary>
    /// 矿标签
    /// </summary>
    [ExecuteInEditMode]
    public class Mineral : MapEntity<MineralRoot>
    {
        /// <summary>
        /// 标识index
        /// </summary>
        public ulong m_MineralIndex;
        /// <summary>
        /// 配置中的npc id
        /// </summary>
        public uint m_MineralNpcId;

        /// <summary>
        /// 属于哪个组
        /// </summary>
        public uint m_MineralGroupId;
        

        public void Init(uint npcId,uint groupId)
        {
            this.m_MineralNpcId = npcId;
            this.m_MineralGroupId = groupId;
        }

        public IEnumerator DoUpdate()
        {
            yield return null;
        }

        public void Sync(SemaphoreMark mark)
        {
            if(mark == null)
            {
                return;
            }
            transform.position = mark.transform.position;
        }

        public void Release(bool needDestroy = false)
        {
            m_MineralNpcId = 0;
            m_MineralGroupId = 0;
            if(needDestroy)
            {
                UnityEngine.Object.DestroyImmediate(gameObject);
            }
        }
    }

}
#endif