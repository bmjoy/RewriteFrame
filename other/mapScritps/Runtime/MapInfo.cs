using System;
using System.IO;
using UnityEngine;

namespace Map
{
    /// <summary>
    /// 在游戏中使用的Map的信息
    /// </summary>
    public class MapInfo : ScriptableObject
    {
        /// <summary>
        /// <see cref="Map.Uid"/>
        /// </summary>
        public uint Uid;
        /// <summary>
        /// 这个Map对应的Scene
        /// </summary>
        public string SceneAddressableKey;
        /// <summary>
        /// 这个Map下的Area
        /// </summary>
        public AreaInfo[] AreaInfos;
        /// <summary>
        /// 这个Map下的Voxel
        /// </summary>
        public VoxelGridInfo VoxelGridInfo;
        
        private System.Collections.Generic.Dictionary<ulong, int> m_AreaUid2Index;

        public void Initialize()
        {
            m_AreaUid2Index = new System.Collections.Generic.Dictionary<ulong, int>();
            for (int iArea = 0; iArea < AreaInfos.Length; iArea++)
            {
                m_AreaUid2Index.Add(AreaInfos[iArea].Uid, iArea);
            }
        }

        public AreaInfo FindAreaInfoByUid(ulong areaUid)
        {
            if (!m_AreaUid2Index.ContainsKey(areaUid))
            {
                return null;
            }

            return AreaInfos[m_AreaUid2Index[areaUid]];
        }

        /// <summary>
        /// 计算玩家位于坐标时应该加载的Area
        /// </summary>
        public int CaculateAreaIndex(Vector3 position)
        {
            int voxelIndex = VoxelGridInfo.CaculateVoxelIndex(position);
            VoxelInfo voxel = VoxelGridInfo.VoxelInfos[voxelIndex];
            if(voxel.Indexs == null)
            {
                return Constants.NOTSET_AREA_INDEX;
            }
            if (voxel.Indexs.Length == 0)
            {
                return Constants.NOTSET_AREA_INDEX; 
            }
            else
            {
                int nearestAreaIndex = Constants.NOTSET_AREA_INDEX;
                float nearestAreaDistance = float.MaxValue;
                for (int iArea = 0; iArea < voxel.Indexs.Length; iArea++)
                {
                    int areaIndex = voxel.Indexs[iArea];
                    AreaInfo iterAreaInfo = AreaInfos[areaIndex];
                    float distance = (position - iterAreaInfo.AABB.center).sqrMagnitude / iterAreaInfo.Diameter / iterAreaInfo.Diameter;
                    if (distance < nearestAreaDistance)
                    {
                        nearestAreaDistance = distance;
                        nearestAreaIndex = areaIndex;
                    }
                }
                return nearestAreaIndex;
            }
        }

        /// <summary>
        /// 通过AreaId获取AreaInfo
        /// </summary>
        /// <returns></returns>
        public AreaInfo GetAreaInfoByAreaId(ulong areaId)
        {
            if (AreaInfos != null && AreaInfos.Length > 0)
            {
                for (int iArea = 0; iArea < AreaInfos.Length; iArea++)
                {
                    AreaInfo areaInfo = AreaInfos[iArea];
                    if (areaInfo != null && areaInfo.Uid == areaId)
                    {
                        return areaInfo;
                    }
                }
            }
            return null;
        }

        #region 二进制序列化反序列化
        /// <summary>
        /// 序列化
        /// </summary>
        /// <param name="path"></param>
        public void Serialize(string path)
        {
            BinaryFile file = new BinaryFile(System.Text.Encoding.Unicode);
            if (!file.OpenWrite(path))
            {
                file.Close();
                return;
            }
            WriteMapInfoBinary(file);
            file.Close();
        }

        private void WriteMapInfoBinary(BinaryFile file)
        {
            BinaryWriter writer = file.m_Writer;
            writer.Write(this.Uid);
            writer.Write(this.SceneAddressableKey);
            if (this.AreaInfos == null || this.AreaInfos.Length <= 0)
            {
                writer.Write(0);
            }
            else
            {
                writer.Write(this.AreaInfos.Length);
                for (int iArea = 0; iArea < this.AreaInfos.Length; iArea++)
                {
                    AreaInfo areaInfo = this.AreaInfos[iArea];
                    areaInfo.Serialize(writer);
                }
            }
            VoxelGridInfo voxelGridInfo = this.VoxelGridInfo;
            voxelGridInfo.Serialize(writer);
        }

        /// <summary>
        /// 反序列化
        /// </summary>
        /// <param name="bytes"></param>
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
            this.Uid = reader.ReadUInt32();
            this.SceneAddressableKey = reader.ReadString();
            int areaInfosLength = reader.ReadInt32();
            if(areaInfosLength>0)
            {
                this.AreaInfos = new AreaInfo[areaInfosLength];
                for (int iArea =0;iArea<areaInfosLength;iArea++)
                {
                    AreaInfo areaInfo = new AreaInfo();
                    areaInfo.Deserialize(reader);
                    this.AreaInfos[iArea] = areaInfo;
                }
            }
            this.VoxelGridInfo = new VoxelGridInfo();
            this.VoxelGridInfo.Deserialize(reader);
            file.Close();
        }
        #endregion
    }
}