using Leyoutech.Core.Loader;
using Leyoutech.Core.Pool;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityObject = UnityEngine.Object;
using SystemObject = System.Object;

namespace Map
{
    /// <summary>
    /// 区域内的单元管理（因为同一层中的不同格子有可能含有同一个unit，所以得单独管理）
    /// </summary>
    public class UnitController
    {
        #region 属性
        /// <summary>
        /// unit数据
        /// </summary>
        private List<SceneUnitInfo> m_Units;

        /// <summary>
        /// unit资源数据
        /// </summary>
        private AssetInfo[] m_AssetInfos;
        /// <summary>
        /// 运行时unit状态
        /// </summary>
        private UnitData[] m_UnitDatas;
        /// <summary>
        /// unit操作命令
        /// </summary>
        private Queue<UnitCommand> m_UnitCommands;
        /// <summary>
        /// 当前在执行的<see cref="UnitCommand"/>的数量
        /// </summary>
        private int m_RuningUnitCommandCount;

        /// <summary>
        /// 是否在Release
        /// </summary>
        private bool m_IsReleasing;
        /// <summary>
        /// 是否已经Release
        /// </summary>
        private bool m_IsReleased;
        /// <summary>
        /// 当前实例化出来的Unit的数量
        /// Debug用的
        /// </summary>
        private int m_InstantiatedUnitCount;

        private AreaController m_AreaController;
        /// <summary>
        /// 上一次Release的Unit的Index
        /// </summary>
        private int m_LastReleaseUnitIndex;

        /// <summary>
        /// 延迟卸载的时间
        /// </summary>
        public static float sm_DelayUnLoadTime = 5f;

        /// <summary>
        /// 需要卸载的unit索引
        /// </summary>
        private List<int> m_NeedUnloadIndex;
        
        #endregion
        

        #region 公开方法

        /// <summary>
        /// 切换完区域后判断当前是否已经把周围的27宫格加载完毕
        /// </summary>
        /// <returns></returns>
        public bool IsInitializedPlayNeedUnit()
        {
            return m_RuningUnitCommandCount <= 0;
        }

        public bool IsReleased()
        {
            return m_IsReleased;
        }

        /// <summary>
        /// 初始化
        /// </summary>
        public void Initialize(List<SceneUnitInfo> unitInfos, AssetInfo[] assetInfos,AreaController areaController)
        {
            m_AreaController = areaController;
            m_Units = unitInfos;
            m_AssetInfos = assetInfos;
            m_UnitCommands = new Queue<UnitCommand>();
            m_UnitDatas = new UnitData[m_Units.Count];
            m_NeedUnloadIndex = new List<int>();
            for (int iUnit = 0; iUnit < m_UnitDatas.Length; iUnit++)
            {
                m_UnitDatas[iUnit].SetState(UnitState.Released);
                m_UnitDatas[iUnit].ClearRef();
            }
        }

        /// <summary>
        /// 添加unit
        /// </summary>
        /// <param name="unitIndex"></param>
        public void AddUnit(int unitIndex)
        {
            //Debug.LogError("AddUnit1:"+unitIndex);
            m_UnitDatas[unitIndex].RemoveState(UnitState.WaitRelease);
            m_UnitDatas[unitIndex].AddRef();
            if (!m_UnitDatas[unitIndex].HasState(UnitState.Instantiated)
                && !m_UnitDatas[unitIndex].HasState(UnitState.Instantiating)
                && !m_UnitDatas[unitIndex].HasState(UnitState.WaitInstantiate))
            {
                //Debug.LogError("AddUnit2:" + unitIndex);
                
                m_UnitDatas[unitIndex].AddState(UnitState.WaitInstantiate);
                m_UnitCommands.Enqueue(new UnitCommand(unitIndex, CommandType.Instantiate));
            }
        }

        /// <summary>
        /// 移除unit
        /// </summary>
        /// <param name="unitIndex"></param>
        public void RemoveUnit(int unitIndex)
        {
            bool canRemove = m_UnitDatas[unitIndex].RemoveRef();
            if (m_UnitDatas[unitIndex].HasState(UnitState.Instantiated)
                        || m_UnitDatas[unitIndex].HasState(UnitState.Instantiating))
            {
                if(canRemove)
                {
                    if(!m_NeedUnloadIndex.Contains(unitIndex))
                    {
                        m_NeedUnloadIndex.Add(unitIndex);
                    }
                }
                
            }
        }

        private void DoUnloadUnit()
        {
            if(m_NeedUnloadIndex == null || m_NeedUnloadIndex.Count<=0)
            {
                return;
            }

            for(int iIndex = m_NeedUnloadIndex.Count-1; iIndex>=0;iIndex--)
            {
                int unitIndex = m_NeedUnloadIndex[iIndex];
                UnitData unitData = m_UnitDatas[unitIndex];
                //Debug.LogError("unloadtime:"+unitData.m_UnLoadTime+" refCount:"+unitData.m_RefCount + " name:" + unitData.GameObject.name);
                if(unitData.m_UnLoadTime<=Time.time && unitData.m_RefCount<=0)
                {
                    unitData.AddState(UnitState.WaitRelease);
                    //Debug.LogError("RemoveUnit:" + unitIndex+" state:"+unitData.m_State+ " name:"+ unitData.GameObject.name);
                    m_UnitCommands.Enqueue(new UnitCommand(unitIndex, CommandType.Release));
                    m_NeedUnloadIndex.Remove(unitIndex);
                    m_UnitDatas[unitIndex] = unitData;
                }
            }
        }

        public void RequestRelease()
        {
            m_LastReleaseUnitIndex = -1;
            if (m_IsReleasing || m_IsReleased)
            {
                return;
            }
            m_IsReleasing = true;
            m_LastReleaseUnitIndex = -1;
        }

        public void DoUpdate()
        {
            if (m_IsReleasing)
            {
                DoUpdateReleasing();
            }
            else if(!m_IsReleased)
            {
                DoUnloadUnit(); 
                DoUpdateUnitCommand();
            }
            
        }

        /// <summary>
        /// 执行删除和生成命令
        /// </summary>
        private void DoUpdateUnitCommand()
        {
            int releaseCommandCount = 0;
            int enqueueCommandCount = 0;
            while (m_RuningUnitCommandCount + releaseCommandCount
                    < Constants.MAX_RUNING_UNITCOMMAND_COUNT
                && m_UnitCommands.Count > enqueueCommandCount)
            {
                UnitCommand iterUnitCommand = m_UnitCommands.Dequeue();
                UnitData iterUnitData = m_UnitDatas[iterUnitCommand.UnitIndex];
                switch (iterUnitCommand.Command)
                {
                    case CommandType.Instantiate:
                        if (iterUnitData.HasState(UnitState.WaitRelease))
                        {
                            continue;
                        }
                        // 防止重复实例化
                        else if (!iterUnitData.HasState(UnitState.Instantiated)
                            && !iterUnitData.HasState(UnitState.Instantiating)
                            && iterUnitData.HasState(UnitState.WaitInstantiate))
                        {
                            m_RuningUnitCommandCount++;
                            

                            SceneUnitInfo unitInfo = m_Units[iterUnitCommand.UnitIndex];
                            AssetUtil.InstanceAssetAsync(m_AssetInfos[unitInfo.AssetIndex].AddressableKey
                                , OnInstantiateUnitCompleted, AssetLoaderPriority.Default, null
                                , new LoadUnitData(iterUnitCommand.UnitIndex, iterUnitCommand.CommandId));

                            iterUnitData.RemoveState(UnitState.WaitInstantiate);
                            iterUnitData.AddState(UnitState.Instantiating);
                        }
                        break;
                    case CommandType.Release:
                        if (iterUnitData.HasState(UnitState.WaitRelease))
                        {
                            if (iterUnitData.HasState(UnitState.Instantiated))
                            {
                                //Debug.LogError("release:" + iterUnitData.GameObject.name);
                                UnityObject.DestroyImmediate(iterUnitData.GameObject);
                                iterUnitData.GameObject = null;
                                iterUnitData.SetState(UnitState.Released);
                                iterUnitData.ClearRef();
                                m_InstantiatedUnitCount--;
                                releaseCommandCount++;
                            }
                            else
                            {
                                //Debug.LogError("未释放waitRelease:" + iterUnitData.GameObject.name);
                                m_UnitCommands.Enqueue(iterUnitCommand);
                                enqueueCommandCount++;
                            }
                        }
                        break;
                    default:
                        break;
                }

                m_UnitDatas[iterUnitCommand.UnitIndex] = iterUnitData;
            }
        }

        private void OnInstantiateUnitCompleted(string pathOrAddress, UnityObject obj, SystemObject userData)
        {
            m_RuningUnitCommandCount--;

            LoadUnitData loadData = userData as LoadUnitData;
            int unitIndex = loadData.m_UnitIndex;
            int unitCommandId = loadData.m_CommandId;

            
            UnitData unitData = m_UnitDatas[unitIndex];
            if (m_IsReleasing)
            {
                UnityObject.DestroyImmediate(obj);
                unitData.RemoveState(UnitState.Instantiating);
                unitData.AddState(UnitState.Released);
                unitData.GameObject = null;
                unitData.ClearRef();
            }
            else
            {
                GameObject resultGameObject = obj as GameObject;

                m_InstantiatedUnitCount++;

                unitData.GameObject = resultGameObject;
                unitData.RemoveState(UnitState.Instantiating);
                unitData.AddState(UnitState.Instantiated);

                SceneUnitInfo unitInfo = m_Units[unitIndex];
                Transform unitTransform = resultGameObject.transform;
                unitTransform.SetParent(m_AreaController.GetRoot(), false);
                unitTransform.localPosition = unitInfo.LocalPosition;
                unitTransform.localRotation = unitInfo.LocalRotation;
                unitTransform.localScale = unitInfo.LocalScale;
                 
                RendererInfo[] rendererInfos = unitInfo.RendererInfos;
                if(rendererInfos != null && rendererInfos.Length>0)
                {
                    for (int iRenderer = 0; iRenderer < rendererInfos.Length; iRenderer++)
                    {
                        RendererInfo iterRendererInfo = rendererInfos[iRenderer];
                          
                        Transform iterRendererTranfsorm = unitTransform;
                        if(iterRendererInfo.TransformIndexs != null)
                        {
                            for (int iTransformIndex = 0; iTransformIndex < iterRendererInfo.TransformIndexs.Length; iTransformIndex++)
                            {
                                iterRendererTranfsorm = iterRendererTranfsorm.GetChild(iterRendererInfo.TransformIndexs[iTransformIndex]);
                            }
                        }

                        Renderer iterRenderer = iterRendererTranfsorm.GetComponent<Renderer>();
                        if (iterRenderer && iterRendererInfo.LightmapIndex>=0)
                        {
                          /*  for (int iMaterial = 0; iMaterial < iterRenderer.materials.Length; iMaterial++)
                            {
                                Material iterMaterial = iterRenderer.materials[iMaterial];
                                if (CRenderer.Shaders.GetInstance().TryFindShader(iterMaterial.shader.name, out Shader shader))
                                {
                                    iterMaterial.shader = shader;
                                }
                            }*/
                            iterRenderer.lightmapIndex = iterRendererInfo.LightmapIndex;
                            iterRenderer.lightmapScaleOffset = iterRendererInfo.LightmapScaleOffset;

                            iterRenderer.realtimeLightmapIndex = iterRendererInfo.RealLightmapIndex;
                            iterRenderer.realtimeLightmapScaleOffset = iterRendererInfo.RealLightmapScaleOffset;
                        }
                    }

                }
                
            }
            m_UnitDatas[unitIndex] = unitData;
        }

        private void DoUpdateReleasing()
        {
            // 等实例中化的Unit实例化完
            if (m_RuningUnitCommandCount > 0)
            {
                return;
            }

            // 释放已经实例化的Unit
            int releaseCount = 0;
            while (releaseCount < Constants.MAX_RELEASE_UNIT_COUNT_EACHFRAME_WHEN_RELEASEAREA
                && ++m_LastReleaseUnitIndex < m_UnitDatas.Length)
            {
                if (m_UnitDatas[m_LastReleaseUnitIndex].HasState(UnitState.Instantiated))
                {
                    GameObject iterUnitGameObject = m_UnitDatas[m_LastReleaseUnitIndex].GameObject;
                    UnityObject.DestroyImmediate(iterUnitGameObject);//底层有释放
                    iterUnitGameObject = null;
                    m_UnitDatas[m_LastReleaseUnitIndex].GameObject = null;
                    m_UnitDatas[m_LastReleaseUnitIndex].SetState(UnitState.Released);
                    m_UnitDatas[m_LastReleaseUnitIndex].ClearRef();
                    m_InstantiatedUnitCount--;
                    releaseCount++;
                }
            }

            if (releaseCount == 0)
            {
                m_Units = null;
                m_UnitDatas = null;
                m_UnitCommands.Clear();
                m_UnitCommands = null;
                m_IsReleasing = false;
                m_IsReleased = true;
                m_NeedUnloadIndex.Clear();
            }
        }

#endregion

#region 数据结构

        [System.Flags]
        private enum UnitState
        {
            /// <summary>
            /// 这个Unit还未实例化，或者已经被Release
            /// 且这个Unit不需要被实例化
            /// </summary>
            Released = 1 << 0,
            /// <summary>
            /// 已经实例化出来了
            /// </summary>
            Instantiated = 1 << 1,
            /// <summary>
            /// 在实例化中
            /// </summary>
            Instantiating = 1 << 2,
            /// <summary>
            /// 等待被实例化
            /// 如果已经是<see cref="WaitRelease"/>状态，那么就不会实力化这个Unit
            /// </summary>
            WaitInstantiate = 1 << 3,
            /// <summary>
            /// 等待被Reslease
            /// </summary>
            WaitRelease = 1 << 4,
        }

        private struct UnitData
        {
            /// <summary>
            /// 这个Unit的GameObject
            /// </summary>
            public GameObject GameObject;
            /// <summary>
            /// 这个Unit的状态
            /// </summary>
            public UnitState m_State;

            /// <summary>
            /// 被引用的次数
            /// </summary>
            public int m_RefCount;
            /// <summary>
            /// 卸载时间
            /// </summary>
            public float m_UnLoadTime;

            public void AddRef()
            {
                m_RefCount++;
                if(m_UnLoadTime>=0)
                {
                    m_UnLoadTime = -1;
                }
            }

            public void ClearRef()
            {
                m_RefCount = 0;
                m_UnLoadTime = -1;
            }

            public bool RemoveRef()
            {
                if((--m_RefCount)<=0)
                {
                    m_UnLoadTime = Time.time + UnitController.sm_DelayUnLoadTime;
                    return true;
                }
                return false;
            }
            /// <summary>
            /// 添加一种状态
            /// </summary>
            public void AddState(UnitState state)
            {
                m_State = m_State | state;
            }

            /// <summary>
            /// 移除一种状态
            /// </summary>
            public void RemoveState(UnitState state)
            {
                m_State = m_State & ~state;
            }

            /// <summary>
            /// 设置状态
            /// </summary>
            public void SetState(UnitState state)
            {
                m_State = state;
            }

            /// <summary>
            /// 当前是否包含这个State
            /// </summary>
            public bool HasState(UnitState state)
            {
                return (m_State & state) != 0;
            }
        }

        private class LoadUnitData
        {
            public int m_CommandId;
            public int m_UnitIndex;

            public LoadUnitData(int unitIndex, int commandId)
            {
                m_CommandId = commandId;
                m_UnitIndex = unitIndex;
            }
        }

        private struct UnitCommand
        {
            public const int NOTSET_COMMAND_ID = -1;
            private static int ms_LastCommandId = NOTSET_COMMAND_ID;

            public readonly int CommandId;
            public readonly int UnitIndex;
            public readonly CommandType Command;

            public static int GetLastCommandId()
            {
                return ms_LastCommandId;
            }

            public UnitCommand(int unitIndex, CommandType command)
            {
                CommandId = ++ms_LastCommandId;
                UnitIndex = unitIndex;
                Command = command;
            }
        }

        private enum CommandType
        {
            Instantiate,
            Release,
        }

#endregion
    }
}

