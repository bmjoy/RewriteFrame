#if UNITY_EDITOR
using EditorExtend;
using Map;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Map
{
    /// <summary>
    /// 触发器根节点
    /// </summary>
    [ExecuteInEditMode]
    public class TriggerRoot : MapEntityRoot
    {
        #region 私有属性
        /// <summary>
        /// json数据
        /// </summary>
        private EditorTrigger[] m_EditorTrigger;
        /// <summary>
        /// 创建Trigger携程
        /// </summary>
        private IEnumerator m_CreateTriggerEnumerator;

        #endregion

        #region 公开属性
        /// <summary>
        /// 节点之下所包含的Location
        /// </summary>
        public Trigger[] m_TriggerCache;

        public ulong m_MaxTriggerId;
        #endregion

        #region 组件

        #endregion

        #region 私有方法
        private IEnumerator CreateTriggers()
        {
            if (m_EditorTrigger != null && m_EditorTrigger.Length > 0)
            {
                for (int iTrigger = 0; iTrigger < m_EditorTrigger.Length; iTrigger++)
                {
                    GameObject triggerObj = new GameObject();
                    triggerObj.transform.SetParent(transform);
                    Trigger trigger = triggerObj.AddComponent<Trigger>();
                    trigger.Init(m_EditorTrigger[iTrigger], this);

                    if (iTrigger % 5 == 0)
                    {
                        yield return null;
                    }
                }
            }
            yield return null;
        }

        /// <summary>
        /// 计算最大的TriggerId
        /// </summary>
        private void CalcuateMaxTriggerId()
        {
            if (m_TriggerCache != null && m_TriggerCache.Length > 0)
            {
                for (int iTrigger = 0; iTrigger < m_TriggerCache.Length; iTrigger++)
                {
                    Trigger trigger = m_TriggerCache[iTrigger];
                    if (trigger != null)
                    {
                        if (trigger.m_Index > m_MaxTriggerId)
                        {
                            m_MaxTriggerId = trigger.m_Index;
                        }
                    }
                }
            }
            else
            {
                m_MaxTriggerId = 0;
            }
        }

        private void Reset()
        {
            if (m_TriggerCache != null && m_TriggerCache.Length > 0)
            {
                for (int iTrigger = 0; iTrigger < m_TriggerCache.Length; iTrigger++)
                {
                    m_TriggerCache[iTrigger].DestroySelf();
                }
            }
            m_TriggerCache = null;
        }


        private void OnDestroy()
        {
            Reset();
        }
        #endregion

        #region 公开方法
        public void Init(EditorTrigger[] triggers)
        {
            m_EditorTrigger = triggers;
            m_CreateTriggerEnumerator = CreateTriggers();
        }
        /// <summary>
        /// 创建Trigger
        /// </summary>
        public void CreateTrigger()
        {
            GameObject triggerObj = new GameObject();
            triggerObj.transform.SetParent(transform);
            Trigger trigger = triggerObj.AddComponent<Trigger>();
            if (trigger != null)
            {
                ulong nextTriggerId = m_MaxTriggerId + 1;
                trigger.Init(nextTriggerId, this);
            }
            Selection.activeGameObject = triggerObj;
        }

        public IEnumerator OnUpdate(GamingMapArea mapArea)
        {
            m_GamingMapArea = mapArea;
            if (m_CreateTriggerEnumerator != null)
            {
                while (m_CreateTriggerEnumerator.MoveNext())
                {
                    yield return null;
                }
                m_CreateTriggerEnumerator = null;
                m_EditorTrigger = null;
            }
            yield return null;
            m_TriggerCache = gameObject.GetComponentsInChildren<Trigger>();
            CalcuateMaxTriggerId();
            if (m_TriggerCache != null && m_TriggerCache.Length > 0)
            {
                for (int iTrigger = 0; m_TriggerCache != null && iTrigger < m_TriggerCache.Length; iTrigger++)
                {
                    IEnumerator triggerEnumerator = m_TriggerCache[iTrigger].OnUpdate(this, m_ShowModel);
                    if (triggerEnumerator != null)
                    {
                        while (m_TriggerCache != null && m_TriggerCache[iTrigger] != null && triggerEnumerator.MoveNext())
                        {
                            yield return null;
                        }
                    }
                }
            }
            yield return null;
        }
        #endregion


        #region 基类方法
        public override void Clear(bool needDestroy = true)
        {
            Reset();
            base.Clear(needDestroy);
        }

        public override void BeginExport()
        {
            base.BeginExport();
            if (m_TriggerCache != null && m_TriggerCache.Length > 0)
            {
                for (int iTrigger = 0; iTrigger < m_TriggerCache.Length; iTrigger++)
                {
                    m_TriggerCache[iTrigger].RefreshPosition(true);
                }
            }
        }
        #endregion

    }
}

#endif