#if UNITY_EDITOR
using EditorExtend;
using Map;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Map
{
    [ExecuteInEditMode]
    public class LocationRoot : MapEntityRoot
    {
        #region 私有属性
        /// <summary>
        /// json数据
        /// </summary>
        private EditorLocation[] m_EditorLocation;
        /// <summary>
        /// 创建Location携程
        /// </summary>
        private IEnumerator m_CreateTeleportEnumerator;

        private GamingMapType m_lastGamingMapType;
        #endregion

        #region 公开属性
        /// <summary>
        /// 节点之下所包含的Location
        /// </summary>
        public Location[] m_LocationCache;

        public int m_MaxLocationId;
        #endregion

        #region 组件

        #endregion

        #region 私有方法
        private IEnumerator CreateLocations()
        {
            if (m_EditorLocation != null && m_EditorLocation.Length > 0)
            {
                for (int iLocation = 0; iLocation < m_EditorLocation.Length; iLocation++)
                {
                    GameObject locationObj = new GameObject();
                    locationObj.transform.SetParent(transform);
                    Location location = locationObj.AddComponent<Location>();
                    location.Init(m_EditorLocation[iLocation], this);

                    if (iLocation % 5 == 0)
                    {
                        yield return null;
                    }
                }
            }
            yield return null;
        }

        /// <summary>
        /// 计算最大的LocationId
        /// </summary>
        private void CalcuateMaxLocationId()
        {
            if (m_LocationCache != null && m_LocationCache.Length > 0)
            {
                for (int iLocation = 0; iLocation < m_LocationCache.Length; iLocation++)
                {
                    Location creature = m_LocationCache[iLocation];
                    if (creature != null)
                    {
                        if (creature.m_LocationId > m_MaxLocationId)
                        {
                            m_MaxLocationId = creature.m_LocationId;
                        }
                    }
                }
            }
        }

        private void Reset()
        {
            if (m_LocationCache != null && m_LocationCache.Length > 0)
            {
                for (int iLocation = 0; iLocation < m_LocationCache.Length; iLocation++)
                {
                    m_LocationCache[iLocation].DestroySelf();
                }
            }
            m_LocationCache = null;
        }

        private void ResetModelPath()
        {
            if (m_LocationCache != null && m_LocationCache.Length > 0)
            {
                for (int iLocation = 0; iLocation < m_LocationCache.Length; iLocation++)
                {
                    m_LocationCache[iLocation].m_ModelPath = "";
                }
            }
        }

        private void OnDestroy()
        {
            Reset();
        }
        #endregion

        #region 公开方法
        public void Init(EditorLocation[] locations)
        {
            m_EditorLocation = locations;
            m_CreateTeleportEnumerator = CreateLocations();
        }
        /// <summary>
        /// 创建Location
        /// </summary>
        public void CreateLocation()
        {
            GameObject locationObj = new GameObject();
            locationObj.transform.SetParent(transform);
            Location location = locationObj.AddComponent<Location>();
            if (location != null)
            {
                m_LocationCache = gameObject.GetComponentsInChildren<Location>();
                CalcuateMaxLocationId();
                int nextLocationId = m_MaxLocationId + 1;
                string strLocationUid = string.Format("{0}{1}{2}", m_GamingMapArea.GetGamingMapId(), m_GamingMapArea.m_AreaId, MapEditorUtility.CalcuateNumber(nextLocationId, 2));
                ulong locationUid = ulong.Parse(strLocationUid);
                location.Init(locationUid, nextLocationId, this);
            }
            Selection.activeGameObject = locationObj;
        }

        public IEnumerator OnUpdate(GamingMapArea mapArea)
        {
            m_GamingMapArea = mapArea;
            GamingMapType mapType = m_GamingMapArea.GetGamingMapType();
            if (mapType != m_lastGamingMapType)
            {
                ResetModelPath();
            }
            m_lastGamingMapType = mapType;
            if (m_CreateTeleportEnumerator != null)
            {
                while (m_CreateTeleportEnumerator.MoveNext())
                {
                    yield return null;
                }
                m_CreateTeleportEnumerator = null;
                m_EditorLocation = null;
            }
            yield return null;
            m_LocationCache = gameObject.GetComponentsInChildren<Location>();
            CalcuateMaxLocationId();
            if (m_LocationCache != null && m_LocationCache.Length > 0)
            {
                for (int iLocation = 0; m_LocationCache != null && iLocation < m_LocationCache.Length; iLocation++)
                {
                    IEnumerator locationEnumerator = m_LocationCache[iLocation].OnUpdate(this, m_ShowModel);
                    if (locationEnumerator != null)
                    {
                        while (m_LocationCache != null && m_LocationCache[iLocation] != null && locationEnumerator.MoveNext())
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
            if (m_LocationCache != null && m_LocationCache.Length > 0)
            {
                for (int iLocation = 0; iLocation < m_LocationCache.Length; iLocation++)
                {
                    m_LocationCache[iLocation].RefreshPosition(true);
                }
            }
        }
        #endregion

    }
}

#endif