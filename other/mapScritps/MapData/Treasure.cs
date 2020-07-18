#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Map
{
    /// <summary>
    /// 寻宝节点
    /// </summary>
    [ExecuteInEditMode]
    public class Treasure : MapEntity<TreasureRoot>
    {
        /// <summary>
        /// 配置中的npc id
        /// </summary>
        public uint m_TreasureNpcId;

        /// <summary>
        /// 属于哪个组
        /// </summary>
        public uint m_TreasureGroupId;
        

        public void Init(uint npcId,uint groupId)
        {
            this.m_TreasureNpcId = npcId;
            this.m_TreasureGroupId = groupId;
        }

        public IEnumerator DoUpdate(TreasureRoot root)
        {
            m_Root = root;
            yield return null;
        }

        /// <summary>
        /// 同步位置
        /// </summary>
        /// <param name="mark"></param>
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
            m_TreasureNpcId = 0;
            m_TreasureGroupId = 0;
            if(needDestroy)
            {
                UnityEngine.Object.DestroyImmediate(gameObject);
            }
        }
    }

}
#endif