#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Map
{
    /// <summary>
    /// 矿根节点
    /// </summary>
    [ExecuteInEditMode]
    public class MineralRoot : MapEntityRoot
    {
        #region 私有属性
        /// <summary>
        /// 采矿数据
        /// </summary>
        public EditorMineral[] m_EditorMinerals;
        private IEnumerator m_CreateMineralEnumerator;
        #endregion

        #region 公开属性

        #endregion

        #region 挂载
        public List<Mineral> m_MineralCache = new List<Mineral>();
        #endregion

        #region 私有方法
        private void Reset()
        {
            if (m_MineralCache != null && m_MineralCache.Count > 0)
            {
                for (int iMineral = 0; iMineral < m_MineralCache.Count; iMineral++)
                {
                    m_MineralCache[iMineral].DestroySelf();
                }
            }
            m_MineralCache.Clear();
        }
        #endregion

        #region 公开方法
        /// <summary>
        /// 同步信息
        /// </summary>
        public void SyncInfo()
        {
            if (m_MineralCache == null || m_MineralCache.Count <= 0)
            {
                return;
            }

            if (m_GamingMapArea != null)
            {
                MineralRootMark treasureRoot = null;
                Area[] areas = FindObjectsOfType<Area>();
                if (areas != null && areas.Length > 0)
                {
                    for (int iArea = 0; iArea < areas.Length; iArea++)
                    {
                        Area area = areas[iArea];
                        if (area.Uid == m_GamingMapArea.m_AreaId)
                        {
                            treasureRoot = area.m_MineralRoot;
                        }
                    }
                }

                if (treasureRoot == null)
                {
                    EditorUtility.DisplayDialog("提示", string.Format("同步失败!未找到{0}", m_GamingMapArea.m_AreaId), "OK");
                    return;
                }

                List<SemaphoreMark> semaphores = treasureRoot.m_SemaphorMarkCache;
                if (m_MineralCache != null && m_MineralCache.Count > 0)
                {
                    List<Mineral> needRelease = new List<Mineral>();
                    for (int iMineral = m_MineralCache.Count - 1; iMineral >= 0; iMineral--)
                    {
                        Mineral mineral = m_MineralCache[iMineral];
                        bool hasAlive = false;
                        for (int iSem = 0; iSem < semaphores.Count; iSem++)
                        {
                            SemaphoreMark mark = semaphores[iSem];
                            if (mineral.name == mark.name && mineral.m_MineralNpcId == mark.m_NpcId
                            && mineral.m_MineralGroupId == mark.m_GroupId)
                            {
                                mineral.Sync(mark);
                                hasAlive = true;
                                break;
                            }
                        }
                        if (!hasAlive)
                        {
                            needRelease.Add(mineral);
                        }
                    }
                    if (needRelease != null && needRelease.Count > 0)
                    {
                        for (int iNeed = 0; iNeed < needRelease.Count; iNeed++)
                        {
                            needRelease[iNeed].Release(true);
                        }
                        needRelease.Clear();
                    }
                }
            }

        }

        public void Add(SemaphoreMark mark)
        {
            RefreshMineralCache();
            if (Exists(mark))
            {
                return;
            }
            CreateMineral(mark);
        }

        /// <summary>
        /// 创建矿节点
        /// </summary>
        /// <param name="mark"></param>
        private void CreateMineral(SemaphoreMark mark)
        {
            GameObject treasureObj = new GameObject();
            treasureObj.transform.SetParent(transform);
            treasureObj.transform.localRotation = Quaternion.identity;
            treasureObj.transform.position = mark.transform.position;
            treasureObj.name = mark.name;
            Mineral mineral = treasureObj.AddComponent<Mineral>();
            mineral.Init(mark.m_NpcId, mark.m_GroupId);
        }

        public void Remove(SemaphoreMark mark)
        {
            RefreshMineralCache();
            if (Exists(mark))
            {
                if (m_MineralCache != null && m_MineralCache.Count > 0)
                {
                    for (int iMineral = m_MineralCache.Count - 1; iMineral >= 0; iMineral--)
                    {
                        Mineral mineral = m_MineralCache[iMineral];
                        if (mineral.name == mark.name && mineral.m_MineralNpcId == mark.m_NpcId
                            && mineral.m_MineralGroupId == mark.m_GroupId)
                        {
                            mineral.Release(true);
                        }
                    }
                }
            }
        }

        public bool Exists(SemaphoreMark mark)
        {
            if (mark == null)
            {
                return false;
            }
            return Exists(mark.m_GroupId, mark.m_NpcId, mark.name);
        }

        public bool Exists(uint groupId, uint npcId, string name)
        {
            if (m_MineralCache == null || m_MineralCache.Count <= 0)
            {
                return false;
            }

            for (int iMineral = 0; iMineral < m_MineralCache.Count; iMineral++)
            {
                Mineral mineral = m_MineralCache[iMineral];
                if (mineral != null && mineral.m_MineralNpcId == npcId
                    && mineral.m_MineralGroupId == groupId && mineral.name.Equals(name))
                {
                    return true;
                }
            }
            return false;
        }

        public void Init(EditorMineral[] minerals)
        {
            m_EditorMinerals = minerals;
            m_CreateMineralEnumerator = CreateMinerals();
        }

        private IEnumerator CreateMinerals()
        {
            if (m_EditorMinerals != null && m_EditorMinerals.Length > 0)
            {
                for (int iMineral = 0; iMineral < m_EditorMinerals.Length; iMineral++)
                {
                    CreateMineral(m_EditorMinerals[iMineral]);
                    if (iMineral % 5 == 0)
                    {
                        yield return null;
                    }
                }
            }
            yield return null;
        }

        private void CreateMineral(EditorMineral editorMineral)
        {
            GameObject mineralObj = new GameObject();
            mineralObj.transform.SetParent(transform);
            mineralObj.transform.localRotation = Quaternion.identity;
            mineralObj.transform.position = editorMineral.mineralPos.ToVector3();
            mineralObj.name = editorMineral.name;
            Mineral mineral = mineralObj.AddComponent<Mineral>();
            mineral.Init(editorMineral.mineralNpcId, editorMineral.mineralGroupId);
        }

        private void RefreshMineralCache()
        {
            m_MineralCache.Clear();
            Mineral[] minerals = GetComponentsInChildren<Mineral>();
            if (minerals != null && minerals.Length > 0)
            {
                m_MineralCache.AddRange(minerals);
            }
        }

        public IEnumerator OnUpdate(GamingMapArea area)
        {
            m_GamingMapArea = area;
            if (m_CreateMineralEnumerator != null)
            {
                while (m_CreateMineralEnumerator.MoveNext())
                {
                    yield return null;
                }
                m_CreateMineralEnumerator = null;
                m_EditorMinerals = null;
            }

            m_MineralCache.Clear();
            Mineral[] minerals = GetComponentsInChildren<Mineral>();
            if (minerals != null && minerals.Length > 0)
            {
                m_MineralCache.AddRange(minerals);
                for (int iMineral = 0; iMineral < m_MineralCache.Count; iMineral++)
                {
                    IEnumerator mineralEnumerator = m_MineralCache[iMineral].DoUpdate();
                    if (mineralEnumerator != null)
                    {
                        while (mineralEnumerator.MoveNext())
                        {
                            yield return null;
                        }
                    }
                }

            }
            yield return null;
        }
        #endregion

        #region 父类方法
        public override void BeginExport()
        {
            base.BeginExport();
            if (m_MineralCache != null && m_MineralCache.Count > 0)
            {
                for (int iMineral = 0; iMineral < m_MineralCache.Count; iMineral++)
                {
                    m_MineralCache[iMineral].RefreshPosition(true);
                }
            }
        }

        public override void Clear(bool needDestroy = true)
        {
            Reset();
            base.Clear(needDestroy);
        }
        #endregion
    }
}

#endif