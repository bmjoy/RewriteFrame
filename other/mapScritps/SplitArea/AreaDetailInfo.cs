#define USE_SPLITAREA
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Profiling;

namespace Map
{
	/// <summary>
	/// Area的详细信息
	/// 把它从<see cref="AreaInfo"/>中拆出来是为了让Area的基本信息和Map存在一个文件中
	/// 这样加载MapInfo后就能知道所有Area的位置，用于判断玩家在哪个区域
	/// 当玩家位于某个区域的时候再加载这个区域的详细信息文件
	/// </summary>
	[System.Serializable]
	public class AreaDetailInfo : ScriptableObject
	{
		/// <summary>
		/// 这个Area下包含的Asset
		/// </summary>
		public AssetInfo[] AssetInfos;

        /// <summary>
        /// 这个Area下包含的层信息
        /// </summary>
        public AreaLayerInfo[] AreaLayerInfos;

        #region 二进制序列化反序列化

        public void Serialize(string path)
        {
            BinaryFile file = new BinaryFile(System.Text.Encoding.Unicode);
            if (!file.OpenWrite(path))
            {
                file.Close();
                return;
            }
            WriteAreaDetailInfoBinary(file);
            file.Close();
        }
        /// <summary>
        /// AreaDetialInfo写入二进制文件
        /// </summary>
        /// <param name="binaryFile"></param>
        /// <param name="areaDetailInfo"></param>
        private void WriteAreaDetailInfoBinary(BinaryFile binaryFile)
        {
            BinaryWriter writer = binaryFile.m_Writer;
            AssetInfo[] assetInfos = this.AssetInfos;
            if (assetInfos == null || assetInfos.Length <= 0)
            {
                writer.Write(0);
            }
            else
            {
                writer.Write(assetInfos.Length);
                for (int iAsset = 0; iAsset < assetInfos.Length; iAsset++)
                {
                    AssetInfo assetInfo = assetInfos[iAsset];
                    assetInfo.Serialize(writer);
                }
            }
            AreaLayerInfo[] areaLayerInfos = this.AreaLayerInfos;
            if (areaLayerInfos == null || areaLayerInfos.Length <= 0)
            {
                writer.Write(0);
            }
            else
            {
                writer.Write(areaLayerInfos.Length);
                for (int iLayer = 0; iLayer < areaLayerInfos.Length; iLayer++)
                {
                    AreaLayerInfo layerInfo = areaLayerInfos[iLayer];
                    layerInfo.Serialize(writer);
                    
                }
            }
        }
        public void Deserialize(byte[] bytes)
        {
            if (bytes == null)
            {
                return;
            }
            
            BinaryFile file = new BinaryFile(System.Text.Encoding.Unicode);
            if (!file.OpenRead(bytes))
            {
                file.Close();
                return;
            }
            BinaryReader reader = file.m_Reader;
            int assetInfoCount = reader.ReadInt32();
            if(assetInfoCount>0)
            {
                AssetInfos = new AssetInfo[assetInfoCount];
                for(int iAsset=0;iAsset<assetInfoCount;iAsset++)
                {
                    AssetInfo assetInfo = new AssetInfo();
                    assetInfo.DeSerialize(reader);
                    AssetInfos[iAsset] = assetInfo;
                }
            }

            int areaLayerCount = reader.ReadInt32();
            if(areaLayerCount>0)
            {
                AreaLayerInfos = new AreaLayerInfo[areaLayerCount];
                for(int iLayer=0;iLayer<areaLayerCount;iLayer++)
                {
                    AreaLayerInfo areaLayerInfo = new AreaLayerInfo();
                    areaLayerInfo.DeSerialize(reader);
                    AreaLayerInfos[iLayer] = areaLayerInfo;
                }
            }

            file.Close();
        }
        #endregion

    }

    /// <summary>
    /// 区域层信息
    /// </summary>
    [System.Serializable]
    public class AreaLayerInfo
    {
        /// <summary>
        /// 优先级
        /// </summary>
        public int m_Priority;

        /// <summary>
        /// 格子大小
        /// </summary>
        public int m_GridSize;

        /// <summary>
        /// 27宫格偏移量
        /// </summary>
        public int m_Offest;
        /// <summary>
        /// 根据格子的x y z存储索引，找到对应的格子
        /// </summary>
        public List<long> AreaVirtualGridIndexs = new List<long>();
#if UNITY_EDITOR
        public HashSet<long> AreaVirtualGridIndexCache = new HashSet<long>();
        /// <summary>
        /// 为了导出快捷
        /// </summary>
        public Dictionary<long, AreaVirtualGridInfo> AreaVirtualGridInfoCache = new Dictionary<long, AreaVirtualGridInfo>();
#endif
        /// <summary>
        /// 虚拟格子信息
        /// </summary>
        public List<AreaVirtualGridInfo> AreaVirtualGridInfos = new List<AreaVirtualGridInfo>();
        /// <summary>
        /// 层的单元信息
        /// </summary>
        public List<SceneUnitInfo> m_Units = new List<SceneUnitInfo>();

        public int m_MinIndexX;
        public int m_MaxIndexX;
        public int m_MinIndexY;
        public int m_MaxIndexY;
        public int m_MinIndexZ;
        public int m_MaxIndexZ;

        public static long GetHashCode(Vector3Int value, int code=4)
        {
            return GetHashCode(value.x,value.y,value.z,code); 
        }
        /// <summary>
        /// 自定义获取hash值 code代表的是最大位数包含符号位
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        public static long GetHashCode(int x,int y,int z,int code=4)
        {
            //code必须>=2
            if(code<=1)
            {
                Debug.LogError("参数值不对");
                return -1;
            }

            long result = 0;
            if(z>0)
            {
                result += z;
            }
            else
            {
                long num = (long)Math.Pow(10, code-1);
                result+=(Mathf.Abs(z)+ num);
            }

            if (y > 0)
            {
                long num = (long)Math.Pow(10, code);
                result += (num * y);
            }
            else
            {
                long num = (long)Math.Pow(10, code*2-1);
                result += (Mathf.Abs(y) * (long)Math.Pow(10, code) + num);
            }

            if (x > 0)
            {
                long num = (long)Math.Pow(10, 2*code);
                result += (num * x);
            }
            else
            {
                long num = (long)Math.Pow(10, code * 3 - 1);
                result += (Mathf.Abs(x) * (long)Math.Pow(10, code*2) + num);
            }
            return result;
        }

        #region 二进制序列化和反序列化
        public void Serialize(BinaryWriter writer)
        {
            writer.Write(m_Priority);
            writer.Write(m_GridSize);
            writer.Write(m_Offest);

            List<long> areaVirtualGridIndexs = AreaVirtualGridIndexs;
            if (areaVirtualGridIndexs == null || areaVirtualGridIndexs.Count <= 0)
            {
                writer.Write(0);
            }
            else
            {
                writer.Write(areaVirtualGridIndexs.Count);
                for (int iIndex = 0; iIndex < areaVirtualGridIndexs.Count; iIndex++)
                {
                    writer.Write(areaVirtualGridIndexs[iIndex]);
                }
            }

            List<AreaVirtualGridInfo> areaVirtualGridInfos = AreaVirtualGridInfos;
            if (areaVirtualGridInfos == null || areaVirtualGridInfos.Count <= 0)
            {
                writer.Write(0);
            }
            else
            {
                writer.Write(areaVirtualGridInfos.Count);
                for (int iGrid = 0; iGrid < areaVirtualGridInfos.Count; iGrid++)
                {
                    AreaVirtualGridInfo gridInfo = areaVirtualGridInfos[iGrid];
                    gridInfo.Serialize(writer);
                }
            }

            List<SceneUnitInfo> units = m_Units;
            if (units == null || units.Count <= 0)
            {
                writer.Write(0);
            }
            else
            {
                writer.Write(units.Count);
                for (int iUnit = 0; iUnit < units.Count; iUnit++)
                {
                    SceneUnitInfo sceneUnitInfo = units[iUnit];
                    sceneUnitInfo.Serialize(writer);
                }
            }
        }

        public void DeSerialize(BinaryReader reader)
        {
            m_Priority = reader.ReadInt32();
            m_GridSize = reader.ReadInt32();
            m_Offest = reader.ReadInt32();

            int indexCount = reader.ReadInt32();
            if (indexCount > 0)
            {
                AreaVirtualGridIndexs = new List<long>();
                for (int iIndex = 0; iIndex < indexCount; iIndex++)
                {
                    AreaVirtualGridIndexs.Add(reader.ReadInt64());
                }
            }

            int gridCount = reader.ReadInt32();
            if (gridCount > 0)
            {
                AreaVirtualGridInfos = new List<AreaVirtualGridInfo>();
                for (int iGrid = 0; iGrid < gridCount; iGrid++)
                {
                    AreaVirtualGridInfo gridInfo = new AreaVirtualGridInfo();
                    gridInfo.DeSerialize(reader);
                    
                    m_MinIndexX = Mathf.Min(gridInfo.m_IndexX, m_MinIndexX);
                    m_MaxIndexX = Mathf.Max(gridInfo.m_IndexX, m_MaxIndexX);

                    m_MinIndexY = Mathf.Min(gridInfo.m_IndexY, m_MinIndexY);
                    m_MaxIndexY = Mathf.Max(gridInfo.m_IndexY, m_MaxIndexY);

                    m_MinIndexZ = Mathf.Min(gridInfo.m_IndexZ, m_MinIndexZ);
                    m_MaxIndexZ = Mathf.Max(gridInfo.m_IndexZ, m_MaxIndexZ);
                    AreaVirtualGridInfos.Add(gridInfo);
                }
            }


            int unitCount = reader.ReadInt32();
            if (unitCount > 0)
            {
                m_Units = new List<SceneUnitInfo>();
                for (int iUnit = 0; iUnit < unitCount; iUnit++)
                {
                    SceneUnitInfo sceneUnitInfo = new SceneUnitInfo();
                    sceneUnitInfo.DeSerialize(reader);
                    m_Units.Add(sceneUnitInfo);
                }

            }
        }
#endregion
    }


    /// <summary>
    /// 区域虚拟格子信息
    /// </summary>
    [System.Serializable]
    public struct AreaVirtualGridInfo
    {
        /// <summary>
        /// 虚拟格子包含的unit索引
        /// </summary>
        public List<int> m_UnitIndexs;
#if UNITY_EDITOR
        public HashSet<int> m_UnitIndexCache;
#endif
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

#region 二进制序列化和反序列化
        public void Serialize(BinaryWriter writer)
        {
            List<int> unitIndexs = m_UnitIndexs;
            if (unitIndexs == null || unitIndexs.Count <= 0)
            {
                writer.Write(0);
            }
            else
            {
                writer.Write(unitIndexs.Count);
                for (int iUnit = 0; iUnit < unitIndexs.Count; iUnit++)
                {
                    writer.Write(unitIndexs[iUnit]);
                }
            }

            Vector3 pos = m_Position;
            writer.Write(pos.x);
            writer.Write(pos.y);
            writer.Write(pos.z);

            writer.Write(m_IndexX);
            writer.Write(m_IndexY);
            writer.Write(m_IndexZ);
        }

        public void DeSerialize(BinaryReader reader)
        {
            int unitIndexCount = reader.ReadInt32();
            if (unitIndexCount > 0)
            {
                m_UnitIndexs = new List<int>();
                for (int iUnit = 0; iUnit < unitIndexCount; iUnit++)
                {
                    m_UnitIndexs.Add(reader.ReadInt32());
                }
            }

            Vector3 pos = default(Vector3);
            pos.x = reader.ReadSingle();
            pos.y = reader.ReadSingle();
            pos.z = reader.ReadSingle();

            m_Position = pos;
            m_IndexX = reader.ReadInt32();
            m_IndexY = reader.ReadInt32();
            m_IndexZ = reader.ReadInt32();
        }
#endregion
    }

}