using Leyoutech.Core.Pool;
using Leyoutech.Utility;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Profiling;

namespace Map
{
    /// <summary>
    /// 区域内的层管理
    /// </summary>
    public class AreaLayerController
    {
        #region 属性
        private AreaController m_AreaController;

        /// <summary>
        /// 该层中虚拟格子的索引
        /// </summary>
        private Dictionary<long, int> m_GridIndexs;
        /// <summary>
        /// 区域层数据
        /// </summary>
        private AreaLayerInfo m_AreaLayerInfo;

        /// <summary>
        /// 单元控制器
        /// </summary>
        private UnitController m_UnitController;

        /// <summary>
        /// 已加载的格子
        /// </summary>
        private HashSet<Vector3Int> m_LoadedGrids;
        /// <summary>
        /// 需要加载的格子
        /// </summary>
        private List<Vector3Int> m_NeedLoadGrids;

        /// <summary>
        /// 需要卸载的格子
        /// </summary>
        private List<Vector3Int> m_NeedUnLoadGrids;
        
        /// <summary>
        /// 当前位置X序号
        /// </summary>
        private int m_CurrentPosX;
        /// <summary>
        /// 当前位置Y序号
        /// </summary>
        private int m_CurrentPosY;
        /// <summary>
        /// 当前位置Z序号
        /// </summary>
        private int m_CurrentPosZ;

        private bool m_IsReleasing = false;
        /// <summary>
        /// 卸载范围
        /// </summary>
        private int m_UnloadRange=0;

        /// <summary>
        /// 需要移除已加载的格子
        /// </summary>
        private List<Vector3Int> m_RemoveLoaded = new List<Vector3Int>();

        /// <summary>
        /// 优先级 主要用于加载时
        /// </summary>
        public int Priority
        {
            get
            {
                if (m_AreaLayerInfo != null)
                {
                    return m_AreaLayerInfo.m_Priority;
                }
                return -1;
            }
        }

        /// <summary>
        /// 虚拟格子的大小
        /// </summary>
        public int GridSize
        {
            get
            {
                if (m_AreaLayerInfo != null)
                {
                    return m_AreaLayerInfo.m_GridSize;
                }
                return -1;
            }
        }
        
        #endregion

        #region 公开方法

        public bool IsReleased()
        {
            return m_UnitController.IsReleased();
        }

        public bool IsInitializedPlayNeedUnit()
        {
            return m_UnitController.IsInitializedPlayNeedUnit();
        }

        public void RequestRelease()
        {
            m_UnitController.RequestRelease();
            m_IsReleasing = true;
        }

        /// <summary>
        /// 检查玩家位置
        /// </summary>
        /// <param name="playerPos"></param>
        public void DoCheckPos(Vector3 playerPos,bool force = false)
        {
            if(m_IsReleasing)
            {
                return;
            }
            int posX = (GridSize != 0) ? Mathf.FloorToInt(playerPos.x / GridSize) : 0;
            int posY = (GridSize != 0) ? Mathf.FloorToInt(playerPos.y / GridSize) : 0;
            int posZ = (GridSize != 0) ? Mathf.FloorToInt(playerPos.z / GridSize) : 0;
            if( force || posX != m_CurrentPosX || posY != m_CurrentPosY || posZ != m_CurrentPosZ)
            {
                m_CurrentPosX = posX;
                m_CurrentPosY = posY;
                m_CurrentPosZ = posZ;
                //TODO:卸载超出范围的Grid和加载在范围内的Grid
                CheckLoadOrUnLoadGrid();
            }
        }

        
        /// <summary>
        /// 检查能加载的Grid和卸载的grid
        /// </summary>
        private void CheckLoadOrUnLoadGrid()
        {
            //检查可卸载的Grid
            if (m_LoadedGrids != null && m_LoadedGrids.Count > 0)
            {
                m_RemoveLoaded.Clear();
                HashSet<Vector3Int>.Enumerator iter = m_LoadedGrids.GetEnumerator();
                while (iter.MoveNext())
                {
                    Vector3Int gridIndex = iter.Current;
                    int offestX = Mathf.Abs(gridIndex.x - m_CurrentPosX);
                    int offestY = Mathf.Abs(gridIndex.y - m_CurrentPosY);
                    int offestZ = Mathf.Abs(gridIndex.z - m_CurrentPosZ);
                    if (offestX > m_UnloadRange || offestY > m_UnloadRange || offestZ > m_UnloadRange)
                    {
                        m_NeedUnLoadGrids.Add(gridIndex);
                        m_RemoveLoaded.Add(gridIndex);
                        if (m_NeedLoadGrids.Contains(gridIndex))
                        {
                            m_NeedLoadGrids.Remove(gridIndex);
                        }
                    }
                }

                if(m_RemoveLoaded != null && m_RemoveLoaded.Count>0)
                {
                    for(int iLoad =0;iLoad<m_RemoveLoaded.Count;iLoad++)
                    {
                        m_LoadedGrids.Remove(m_RemoveLoaded[iLoad]);
                    }
                    m_RemoveLoaded.Clear();
                }
            }
            //检查可加载的Grid
            int minIndexX = -m_UnloadRange + m_CurrentPosX;
            int maxIndexX = m_UnloadRange + m_CurrentPosX;
            int minIndexY = -m_UnloadRange + m_CurrentPosY;
            int maxIndexY = m_UnloadRange + m_CurrentPosY;
            int minIndexZ = -m_UnloadRange + m_CurrentPosZ;
            int maxIndexZ = m_UnloadRange + m_CurrentPosZ;
            minIndexX = Mathf.Max(minIndexX, m_AreaLayerInfo.m_MinIndexX);
            maxIndexX = Mathf.Min(maxIndexX, m_AreaLayerInfo.m_MaxIndexX);

            minIndexY = Mathf.Max(minIndexY, m_AreaLayerInfo.m_MinIndexY);
            maxIndexY = Mathf.Min(maxIndexY, m_AreaLayerInfo.m_MaxIndexY);

            minIndexZ = Mathf.Max(minIndexZ, m_AreaLayerInfo.m_MinIndexZ);
            maxIndexZ = Mathf.Min(maxIndexZ, m_AreaLayerInfo.m_MaxIndexZ);
            for (int iX = minIndexX; iX<= maxIndexX; iX++)
            {
                for(int iY = minIndexY; iY<= maxIndexY; iY++)
                {
                    for(int iZ = minIndexZ; iZ<= maxIndexZ; iZ++)
                    {
                        Vector3Int gridIndex = new Vector3Int(iX,iY,iZ);
                        long hashCode = AreaLayerInfo.GetHashCode(gridIndex);
                        bool hasGridIndex = m_GridIndexs.ContainsKey(hashCode);
                        if (hasGridIndex)
                        {
                            if (m_LoadedGrids.Add(gridIndex))
                            {
                                m_NeedLoadGrids.Add(gridIndex);
                                if (m_NeedUnLoadGrids.Contains(gridIndex))
                                {
                                    m_NeedUnLoadGrids.Remove(gridIndex);
                                }
                            }
                        }
                    }
                }
            }
        }
        
        /// <summary>
        /// 单元创建和卸载
        /// </summary>
        public void DoUnitUpdate()
        {
            m_UnitController.DoUpdate();
        }

        public void DoUpdate(Vector3 playerPos, bool forceCheck = false)
        {
            DoCheckPos(playerPos, forceCheck);
            DoLoadGrid();
            DoUnloadGrid();
        }

        /// <summary>
        /// 载入格子
        /// </summary>
        private void DoLoadGrid()
        {
            if(m_NeedLoadGrids != null && m_NeedLoadGrids.Count>0)
            {
                for(int iGrid =0;iGrid<m_NeedLoadGrids.Count;iGrid++)
                {
                    Vector3Int v3Int = m_NeedLoadGrids[iGrid];
                    long gridHashId = AreaLayerInfo.GetHashCode(v3Int);
                    if (m_GridIndexs.ContainsKey(gridHashId))
                    {
                        int index = m_GridIndexs[gridHashId];
                        if(m_AreaLayerInfo.AreaVirtualGridInfos.Count>index)
                        {
                            AreaVirtualGridInfo gridInfo = m_AreaLayerInfo.AreaVirtualGridInfos[index];
                            List<int> unitIndexs = gridInfo.m_UnitIndexs;
                            if(unitIndexs != null && unitIndexs.Count>0)
                            {
                                for(int iUnit =0;iUnit<unitIndexs.Count;iUnit++)
                                {
                                    m_UnitController.AddUnit(unitIndexs[iUnit]);
                                }
                            }
                        }
                    }
                    else
                    {
                        Debug.LogError("不存在gridHashId:" + gridHashId);
                    }
                }
                m_NeedLoadGrids.Clear();
            }
        }
        
        /// <summary>
        /// 卸载格子
        /// </summary>
        private void DoUnloadGrid()
        {
            if (m_NeedUnLoadGrids != null && m_NeedUnLoadGrids.Count > 0)
            {
                for (int iGrid = 0; iGrid < m_NeedUnLoadGrids.Count; iGrid++)
                {
                    Vector3Int v3Int = m_NeedUnLoadGrids[iGrid];
                    long gridHashId = AreaLayerInfo.GetHashCode(v3Int);
                    if (m_GridIndexs.ContainsKey(gridHashId))
                    {
                        int index = m_GridIndexs[gridHashId];
                        
                        if (m_AreaLayerInfo.AreaVirtualGridInfos.Count > index)
                        {
                            AreaVirtualGridInfo gridInfo = m_AreaLayerInfo.AreaVirtualGridInfos[index];
                            List<int> unitIndexs = gridInfo.m_UnitIndexs;
                            if (unitIndexs != null && unitIndexs.Count > 0)
                            {
                                for (int iUnit = 0; iUnit < unitIndexs.Count; iUnit++)
                                {
                                    m_UnitController.RemoveUnit(unitIndexs[iUnit]);
                                }
                            }
                        }
                    }
                    else
                    {
                        Debug.LogError("不存在gridHashId:" + gridHashId);
                    }
                }
                m_NeedUnLoadGrids.Clear();
            }
        }

        /// <summary>
        /// 初始化
        /// </summary>
        public void Initialize(AreaLayerInfo areaLayerInfo, AssetInfo[] assetInfos,AreaController areaController, int unloadRange = 3)
        {
            m_AreaController = areaController;
            m_UnloadRange = unloadRange;
            m_AreaLayerInfo = areaLayerInfo;
            m_GridIndexs = new Dictionary<long, int>();
            m_LoadedGrids = new HashSet<Vector3Int>();
            m_NeedLoadGrids = new List<Vector3Int>();
            m_NeedUnLoadGrids = new List<Vector3Int>();
            m_UnitController = new UnitController();
            m_UnitController.Initialize(areaLayerInfo.m_Units, assetInfos, m_AreaController);
            //初始化虚拟格子
            if (m_AreaLayerInfo != null)
            {
                List<long> gridIndexs = m_AreaLayerInfo.AreaVirtualGridIndexs;
                if(gridIndexs != null && gridIndexs.Count>0)
                {
                    for(int iGrid =0;iGrid<gridIndexs.Count;iGrid++)
                    {
                        //导出索引时 要有个检查过程 判断是否有重复
                        m_GridIndexs.Add(gridIndexs[iGrid], iGrid);
                    }
                }
            }
        }
        #endregion
    }
}

