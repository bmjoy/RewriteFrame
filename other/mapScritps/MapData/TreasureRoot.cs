#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Map
{
    /// <summary>
    /// 寻宝根节点
    /// </summary>
    [ExecuteInEditMode]
    public class TreasureRoot : MapEntityRoot
    {
        #region 私有属性
        /// <summary>
        /// 寻宝数据
        /// </summary>
        private EditorTreasure[] m_EditorTreasure;
        private IEnumerator m_CreateTreasureEnumerator;
        #endregion

        #region 公开属性
        #endregion

        #region 挂载
        public List<Treasure> m_TreasureCache = new List<Treasure>();
        #endregion

        #region 私有方法
        private void Reset()
        {
            if (m_TreasureCache != null && m_TreasureCache.Count > 0)
            {
                for (int iTreasure = 0; iTreasure < m_TreasureCache.Count; iTreasure++)
                {
                    m_TreasureCache[iTreasure].DestroySelf();
                }
            }
            m_TreasureCache.Clear();
        }
        #endregion

        #region 公开方法

        /// <summary>
        /// 同步信息
        /// </summary>
        public void SyncInfo()
        {
            if(m_TreasureCache == null || m_TreasureCache.Count<=0)
            {
                return;
            }
            
            if (m_GamingMapArea != null)
            {
                TreasureRootMark treasureRoot = null;
                Area[] areas = FindObjectsOfType<Area>();
                if (areas != null && areas.Length > 0)
                {
                    for (int iArea = 0; iArea < areas.Length; iArea++)
                    {
                        Area area = areas[iArea];
                        if (area.Uid == m_GamingMapArea.m_AreaId)
                        {
                            treasureRoot = area.m_TreasureRoot;
                        }
                    }
                }

                if(treasureRoot == null)
                {
                    EditorUtility.DisplayDialog("提示", string.Format("同步失败!未找到{0}", m_GamingMapArea.m_AreaId), "OK");
                    return;
                }

                List<SemaphoreMark> semaphores = treasureRoot.m_SemaphorMarkCache;
                if (m_TreasureCache != null && m_TreasureCache.Count > 0)
                {
                    List<Treasure> needRelease = new List<Treasure>();
                    for (int iTreasure = m_TreasureCache.Count - 1; iTreasure >= 0; iTreasure--)
                    {
                        Treasure treasure = m_TreasureCache[iTreasure];
                        bool hasAlive = false;
                        for (int iSem = 0;iSem< semaphores.Count;iSem++)
                        {
                            SemaphoreMark mark = semaphores[iSem];
                            if (treasure.name == mark.name && treasure.m_TreasureNpcId == mark.m_NpcId
                            && treasure.m_TreasureGroupId == mark.m_GroupId)
                            {
                                treasure.Sync(mark);
                                hasAlive = true;
                                break;
                            }
                        }
                        if(!hasAlive)
                        {
                            needRelease.Add(treasure);
                        }
                    }
                    if(needRelease != null && needRelease.Count>0)
                    {
                        for(int iNeed = 0;iNeed<needRelease.Count;iNeed++)
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
            RefreshTreasureCache();
            if (Exists(mark))
            {
                return;
            }
            m_TreasureCache.Add(CreateTreasure(mark));
        }

        private Treasure CreateTreasure(SemaphoreMark mark)
        {
            GameObject treasureObj = new GameObject();
            treasureObj.transform.SetParent(transform);
            treasureObj.transform.localRotation = Quaternion.identity;
            treasureObj.transform.position = mark.transform.position;
            treasureObj.name = mark.name;
            Treasure treasure = treasureObj.AddComponent<Treasure>();
            treasure.Init(mark.m_NpcId,mark.m_GroupId);
            return treasure;
        }

        public void Remove(SemaphoreMark mark)
        {
            RefreshTreasureCache();
            if (Exists(mark))
            {
                if(m_TreasureCache != null && m_TreasureCache.Count>0)
                {
                    for(int iTreasure = m_TreasureCache.Count-1;iTreasure>=0;iTreasure--)
                    {
                        Treasure treasure = m_TreasureCache[iTreasure];
                        if(treasure.name == mark.name && treasure.m_TreasureNpcId == mark.m_NpcId
                            && treasure.m_TreasureGroupId == mark.m_GroupId)
                        {
                            treasure.Release(true);
                            m_TreasureCache.Remove(treasure);
                        }
                    }
                }
            }
        }

        public bool Exists(SemaphoreMark mark)
        {
            if(mark == null)
            {
                return false;
            }
            return Exists(mark.m_GroupId,mark.m_NpcId, mark.name);
        }

        public bool Exists(uint groupId,uint npcId,string name)
        {
            if(m_TreasureCache == null || m_TreasureCache.Count <= 0)
            {
                return false;
            }

            for(int iTreasure = 0;iTreasure<m_TreasureCache.Count;iTreasure++)
            {
                Treasure treasure = m_TreasureCache[iTreasure];
                if(treasure!= null && treasure.m_TreasureNpcId == npcId 
                    && treasure.m_TreasureGroupId == groupId&& treasure.name.Equals(name))
                {
                    return true;
                }
            }
            return false;
        }

        public void Init(EditorTreasure[] treasures)
        {
            m_EditorTreasure = treasures;
            m_CreateTreasureEnumerator = CreateTreasures();
        }

        private IEnumerator CreateTreasures()
        {
            if (m_EditorTreasure != null && m_EditorTreasure.Length > 0)
            {
                for (int iTreasure = 0; iTreasure < m_EditorTreasure.Length; iTreasure++)
                {
                    CreateTreasure(m_EditorTreasure[iTreasure]);
                    if (iTreasure % 5 == 0)
                    {
                        yield return null;
                    }
                }
            }
            yield return null;
        }

        private void CreateTreasure(EditorTreasure editorTreasure)
        {
            GameObject treasureObj = new GameObject();
            treasureObj.transform.SetParent(transform);
            treasureObj.transform.localRotation = Quaternion.identity;
            treasureObj.transform.position = editorTreasure.tresurePos.ToVector3();
            treasureObj.name = editorTreasure.name;
            Treasure treasure = treasureObj.AddComponent<Treasure>();
            treasure.Init(editorTreasure.treasureNpcId, editorTreasure.treasureGroupId);
        }

        private void RefreshTreasureCache()
        {
            m_TreasureCache.Clear();
            Treasure[] treasures = GetComponentsInChildren<Treasure>();
            if (treasures != null && treasures.Length > 0)
            {
                m_TreasureCache.AddRange(treasures);
            }
        }

        public IEnumerator OnUpdate(GamingMapArea area)
        {
            m_GamingMapArea = area;
            if (m_CreateTreasureEnumerator != null)
            {
                while (m_CreateTreasureEnumerator.MoveNext())
                {
                    yield return null;
                }
                m_CreateTreasureEnumerator = null;
                m_EditorTreasure = null;
            }
            m_TreasureCache.Clear();
            Treasure[] treasures = GetComponentsInChildren<Treasure>();
            if(treasures != null && treasures.Length>0)
            {
                m_TreasureCache.AddRange(treasures);
                for (int iTreasure = 0;iTreasure< m_TreasureCache.Count;iTreasure++)
                {
                    IEnumerator treasureEnumerator = m_TreasureCache[iTreasure].DoUpdate(this);
                    if (treasureEnumerator != null)
                    {
                        while (treasureEnumerator.MoveNext())
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
            if (m_TreasureCache != null && m_TreasureCache.Count > 0)
            {
                for (int iTreasure = 0; iTreasure < m_TreasureCache.Count; iTreasure++)
                {
                    m_TreasureCache[iTreasure].RefreshPosition(true);
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
