#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Map
{
    public class DebugAreaLayer : MonoBehaviour
    {
        /// <summary>
        /// 层信息
        /// </summary>
        //[HideInInspector]
        public AreaLayerInfo[] m_AreaLayerInfos;
        //[HideInInspector]
        public AreaDetailInfo m_AreaDetailInfo;
        public bool[] m_ShowLayer;
        public bool[] m_ShowUnitAB;
        public Color[] m_Colors = { Color.red,Color.yellow,Color.white,Color.blue,Color.green};
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public void ShowSceneUI()
        {
            return;
            if (m_AreaLayerInfos != null && m_AreaLayerInfos.Length > 0)
            {
                for (int iLayer = 0; iLayer < m_AreaLayerInfos.Length; iLayer++)
                {
                    if (m_ShowLayer[iLayer])
                    {
                        AreaLayerInfo layerInfo = m_AreaLayerInfos[iLayer];
                        if (layerInfo != null)
                        {
                            List<AreaVirtualGridInfo> areaVirtualGridInfos = layerInfo.AreaVirtualGridInfos;
                            if (areaVirtualGridInfos != null && areaVirtualGridInfos.Count > 0)
                            {
                                for (int iArea = 0; iArea < areaVirtualGridInfos.Count; iArea++)
                                {
                                    AreaVirtualGridInfo gridInfo = areaVirtualGridInfos[iArea];
                                    List<int> unitIndexs = gridInfo.m_UnitIndexs;
                                    if (unitIndexs != null && unitIndexs.Count > 0)
                                    {
                                        for (int iUnit = 0; iUnit < unitIndexs.Count; iUnit++)
                                        {
                                            SceneUnitInfo unitInfo = layerInfo.m_Units[unitIndexs[iUnit]];
                                            AssetInfo assetInfo = m_AreaDetailInfo.AssetInfos[unitInfo.AssetIndex];
                                            Handles.Label(unitInfo.LocalPosition,assetInfo.AddressableKey);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            if(m_AreaLayerInfos != null && m_AreaLayerInfos.Length>0)
            {
                for(int iLayer =0;iLayer<m_AreaLayerInfos.Length;iLayer++)
                {
                    //if(m_ShowLayer[iLayer])
                    {
                        AreaLayerInfo layerInfo = m_AreaLayerInfos[iLayer];
                        if(layerInfo != null)
                        { 
                            List<AreaVirtualGridInfo> areaVirtualGridInfos = layerInfo.AreaVirtualGridInfos;
                            if(areaVirtualGridInfos != null && areaVirtualGridInfos.Count>0)
                            {
                                for(int iArea =0;iArea<areaVirtualGridInfos.Count;iArea++)
                                {
                                    AreaVirtualGridInfo gridInfo = areaVirtualGridInfos[iArea];
                                    if (m_ShowLayer[iLayer])
                                    {
                                        Gizmos.color = m_Colors[iLayer];
                                        Gizmos.DrawWireCube(gridInfo.m_Position, new Vector3(layerInfo.m_GridSize, layerInfo.m_GridSize, layerInfo.m_GridSize));
                                    }
                                       
                                    if(m_ShowUnitAB[iLayer])
                                    {
                                        List<int> unitIndexs = gridInfo.m_UnitIndexs;
                                        if (unitIndexs != null && unitIndexs.Count > 0)
                                        {
                                            for (int iUnit = 0; iUnit < unitIndexs.Count; iUnit++)
                                            {
                                                SceneUnitInfo unitInfo = layerInfo.m_Units[unitIndexs[iUnit]];
                                                Gizmos.color = new Color(1, 1, 1, 0.2f);
                                                Gizmos.DrawWireCube(unitInfo._AABB.center, unitInfo._AABB.size);
                                            }
                                        }
                                    }
                                    
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}

#endif