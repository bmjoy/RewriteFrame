#if UNITY_EDITOR
using EditorExtend;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Map
{
    /// <summary>
    /// 跃迁
    /// </summary>
    [ExecuteInEditMode]
    public class Leap : MonoBehaviour
    {
        /// <summary>
        /// 是否导出到json
        /// </summary>
        public bool m_IsExportToJson = true;
        /// <summary>
        /// 跃迁id
        /// </summary>
        [ReadOnly]
        public ulong m_LeapId;
        /// <summary>
        /// 父跃迁id
        /// </summary>
        [ReadOnly]
        public ulong m_MainLeapId;
        /// <summary>
        /// 跃迁名字
        /// </summary>
        [ReadOnly]
        public string m_LeapName;
        /// <summary>
        /// 跃迁类型
        /// </summary>
        [ReadOnly]
        public LeapType m_LeapType;

        [ReadOnly]
        public MeshRenderer m_MeshRender;
        /// <summary>
        /// 跃迁预览
        /// </summary>
        private LeapOverview m_LeapOverview;

        public IEnumerator OnUpdate(LeapOverview leapOverview,GamingMapArea area,GamingMapArea showArea)
        {
            m_LeapOverview = leapOverview;
            m_MeshRender = GetComponent<MeshRenderer>();
            gameObject.SetActive(m_IsExportToJson);
            if (m_MeshRender != null)
            {
                if (showArea == null)
                {
                    m_MeshRender.enabled = false;
                }
                else
                {
                    LeapRoot root = showArea.m_LeapRoot;
                    bool show = false;
                    if (root != null && root.m_VisibleLeapList.Length > 0)
                    {
                        for (int iVisible = 0; iVisible < root.m_VisibleLeapList.Length; iVisible++)
                        {
                            if (m_LeapId == root.m_VisibleLeapList[iVisible])
                            {
                                show = true;
                                break;
                            }
                        }
                    }
                    m_MeshRender.enabled = show;
                }
            }
            yield return null;
            if (area != null && area.m_LeapRoot != null)
            {
                LeapRoot leapRoot = area.m_LeapRoot;
                m_LeapName = leapRoot.m_LeapName;
                m_LeapType = leapRoot.m_LeapType;
                m_MainLeapId = leapRoot.m_MainLeapId;
                transform.position = leapRoot.transform.position;
                gameObject.name = string.Format("{0}_{1}", m_LeapId, m_LeapName);
                transform.localScale = Vector3.one * leapRoot.m_Range;
            }
        }
    }
}

#endif