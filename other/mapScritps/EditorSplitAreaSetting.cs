using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Map
{
    /// <summary>
    /// 切分区域配置(编辑状态，保存在本地)
    /// </summary>
    public class EditorSplitAreaSetting : ScriptableObject
    {
        public List<EditorSplitAreaLayerSetting> m_EditorSplitAreaLayerSettings;
        /// <summary>
        /// 屏幕占比
        /// </summary>
        public float m_Rate = 0.03f;
    }

    [System.Serializable]
    public class EditorSplitAreaLayerSetting
    {
        /// <summary>
        /// 层名称
        /// </summary>
        public string m_LayerName;

        /// <summary>
        /// 优先级
        /// </summary>
        public int m_Priority;

        /// <summary>
        /// 格子大小
        /// </summary>
        public int m_GridSize;

        /// <summary>
        /// 最小物件AABB盒大小
        /// </summary>
        public float m_MinAABBSize;
        /// <summary>
        /// 最大物件AABB盒大小
        /// </summary>
        public float m_MaxAABBSize;

        /// <summary>
        /// 27宫格偏移
        /// </summary>
        public int m_Offest;
        /// <summary>
        /// 最小物件AABB盒大小
        /// </summary>
        //public Vector3 m_MinAABBSize;

        /// <summary>
        /// 最大物件AABB盒大小
        /// </summary>
        //public Vector3 m_MaxAABBSize;
    }

    /// <summary>
    /// 层设置
    /// </summary>
    [System.Serializable]
    public class SplitAreaLayerSetting
    {
        /// <summary>
        /// 层名称
        /// </summary>
        public string m_LayerName;

        /// <summary>
        /// 优先级
        /// </summary>
        public int m_Priority;

        /// <summary>
        /// 格子大小
        /// </summary>
        public int m_GridSize;

        /// <summary>
        /// 虚拟格子配置信息
        /// </summary>
        public List<VirtualGridSetting> m_VirtualGridSettings = new List<VirtualGridSetting>();
    }

    /// <summary>
    /// 虚拟格子信息
    /// </summary>
    [System.Serializable]
    public class VirtualGridSetting
    {
        /// <summary>
        /// 虚拟格子名称
        /// </summary>
        public string m_Name;
        /// <summary>
        /// 虚拟格子内的单元信息
        /// </summary>
        public List<CellUnitInfo> m_Units = new List<CellUnitInfo>();

        /// <summary>
        /// 虚拟格子所处的位置
        /// </summary>
        public Vector3 m_Position;

        /// <summary>
        /// 虚拟格子x轴的序列
        /// </summary>
        public int m_IndexX;

        /// <summary>
        /// 虚拟格子y轴的序列
        /// </summary>
        public int m_IndexY;

        /// <summary>
        /// 虚拟格子z轴的序列
        /// </summary>
        public int m_IndexZ;
    }

    /// <summary>
    /// 虚拟格子内的单元信息(参考unit的数据)
    /// </summary>
    [System.Serializable]
    public class CellUnitInfo
    {

    }
}

