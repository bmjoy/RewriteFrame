#if UNITY_EDITOR
using EditorExtend;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Map
{
    /// <summary>
    /// 跃迁点预览
    /// </summary>
    [ExecuteInEditMode]
    public class LeapOverview : MonoBehaviour
    {
        /// <summary>
        /// 跃迁点列表
        /// </summary>
        [ReadOnly]
        public List<Leap> m_LeapList;
        
        [ReadOnly]
        private GamingMap m_GamingMap;
        
        public IEnumerator OnUpdate(GamingMap gamingMap)
        {
            m_GamingMap = gamingMap;
            if (m_LeapList == null)
            {
                m_LeapList = new List<Leap>();
            }
            m_LeapList.Clear();
            Leap[] leaps = GetComponentsInChildren<Leap>(true);
            if (leaps != null && leaps.Length > 0)
            {
                for (int iLeap = 0; iLeap < leaps.Length; iLeap++)
                {
                    m_LeapList.Add(leaps[iLeap]);
                }
            }
            yield return null;
            //为了确定有没有遗漏没有的跃迁点
            if (m_GamingMap != null)
            {
                List<GamingMapArea> areaList = m_GamingMap.m_GamingAreaList;
                if (areaList != null && areaList.Count > 0)
                {
                    for (int iArea = 0; iArea < areaList.Count; iArea++)
                    {
                        GamingMapArea area = areaList[iArea];
                        if (area != null)
                        {
                            LeapRoot leapRoot = area.m_LeapRoot;
                            if (leapRoot != null)
                            {
                                Leap leap = GetLeap(leapRoot.m_LeapId);
                                if (leap == null)
                                {
                                    leap = CreateLeap(leapRoot);
                                    m_LeapList.Add(leap);
                                }
                            }
                        }
                    }
                }
            }
            yield return null;
            //因为只能由一个区域在线
            GamingMapArea showArea = null;
            if (m_GamingMap.m_GamingAreaList != null && m_GamingMap.m_GamingAreaList.Count > 0)
            {
                showArea = m_GamingMap.m_GamingAreaList[0];
            }
            //刷新Leap
            if (m_LeapList != null && m_LeapList.Count > 0)
            {
                for (int iLeap = 0; iLeap < m_LeapList.Count; iLeap++)
                {
                    Leap leap = m_LeapList[iLeap];

                    IEnumerator leapUpdateEnum = leap.OnUpdate(this, GetArea(leap.m_LeapId), showArea);
                    while (leapUpdateEnum != null && leapUpdateEnum.MoveNext())
                    {
                        yield return null;
                    }
                }
            }
            yield return null;
        }

        private GamingMapArea GetArea(ulong areaId)
        {
            List<GamingMapArea> areaList = m_GamingMap.m_GamingAreaList;
            if (areaList != null && areaList.Count > 0)
            {
                for(int iArea = 0;iArea<areaList.Count;iArea++)
                {
                    if(areaList[iArea] != null && areaList[iArea].m_AreaId == areaId)
                    {
                        return areaList[iArea];
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// 创建跃迁点
        /// </summary>
        /// <param name="root"></param>
        /// <returns></returns>
        private Leap CreateLeap(LeapRoot root)
        {
            Leap leap = null;
            string leapTempletePath = GamingMapEditorUtility.GetLeapTempletePath();
            if (string.IsNullOrEmpty(leapTempletePath))
            {
                return leap;
            }
            
            GameObject leapTemplete = AssetDatabase.LoadAssetAtPath<GameObject>(leapTempletePath);
            if (leapTemplete != null)
            {
                GameObject leapRootObj = GameObject.Instantiate(leapTemplete);
                leapRootObj.name = root.m_LeapName;
                leapRootObj.transform.SetParent(transform);
                leapRootObj.transform.localPosition = Vector3.zero;
                leapRootObj.transform.localRotation = Quaternion.identity;
                leap = leapRootObj.AddComponent<Leap>();
                leap.m_LeapId = root.m_LeapId;
            }
            return leap;
        }
        /// <summary>
        /// 获取跃迁点
        /// </summary>
        /// <param name="leapId"></param>
        /// <returns></returns>
        private Leap GetLeap(ulong leapId)
        {
            if(m_LeapList == null)
            {
                return null;
            }
            for(int iLeap = 0;iLeap<m_LeapList.Count;iLeap++)
            {
                if(m_LeapList[iLeap].m_LeapId == leapId)
                {
                    return m_LeapList[iLeap];
                }
            }
            return null;
        }
    }
}
#endif