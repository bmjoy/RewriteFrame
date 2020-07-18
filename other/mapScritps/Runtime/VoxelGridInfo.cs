using Leyoutech.Utility;
using System.IO;
using UnityEngine;

namespace Map
{
	[System.Serializable]
	public class VoxelGridInfo
	{
		/// <summary>
		/// Voxel的大小
		/// </summary>
		public int VoxelSize;
		/// <summary>
		/// xyz三个轴上Voxel的数量
		/// </summary>
		public Vector3 VoxelCounts;
		/// <summary>
		/// l:Left,-x d:Down,-y b:Back,-z
		/// </summary>
		public Vector3 LDBVoxelPosition;
		/// <summary>
		/// 这个Area下的所有Voxel
		/// </summary>
		public VoxelInfo[] VoxelInfos;

		/// <summary>
		/// IndexVector是3D空间中的这个Voxel的位置，Index是这个Voxel在<see cref="VoxelInfos"/>中的索引
		/// </summary>
		public int ConvertIndexVectorToIndex(int voxelX, int voxelY, int voxelZ)
		{
			return (int)(voxelX * VoxelCounts.y * VoxelCounts.z + voxelY * VoxelCounts.z + voxelZ);
		}

		/// <summary>
		/// <see cref="ConvertIndexToIndexVector(int)"/>
		/// </summary>
		public Vector3 ConvertIndexToIndexVector(int index)
		{
			int voxelX = Mathf.FloorToInt(index / (VoxelCounts.y * VoxelCounts.z));
			index -= (int)(voxelX * VoxelCounts.y * VoxelCounts.z);
			int voxelY = Mathf.FloorToInt(index / VoxelCounts.z);
			index -= (int)(voxelY * VoxelCounts.z);
			int voxelZ = index;
			return new Vector3(voxelX, voxelY, voxelZ);
		}

		/// <summary>
		/// <see cref="ConvertIndexVectorToIndex(float, float, float)"/>
		/// </summary>
		public int ConvertIndexVectorToIndex(Vector3 index)
		{
			return ConvertIndexVectorToIndex((int)index.x, (int)index.y, (int)index.z);
		}

		/// <summary>
		/// <see cref="CaculateVoxelIndexVector"/>
		/// </summary>
		public int CaculateVoxelIndex(Vector3 position)
		{
			return ConvertIndexVectorToIndex(CaculateVoxelIndexVector(position));
		}

		/// <summary>
		/// 计算一个坐标在哪个Voxel中
		/// </summary>
		public Vector3 CaculateVoxelIndexVector(Vector3 position)
		{
			Vector3 indexVector = (position - LDBVoxelPosition) / VoxelSize;
			indexVector = MathUtility.Clamp(indexVector, Vector3.zero, VoxelCounts - Vector3.one);
			return indexVector;
		}

		/// <summary>
		/// <see cref="CaculateVoxelCenter(int, int, int)"/>
		/// </summary>
		public Vector3 CaculateVoxelCenter(Vector3 index)
		{
			return CaculateVoxelCenter((int)index.x, (int)index.y, (int)index.z);
		}

		/// <summary>
		/// 计算一个Vocel的中心点
		/// </summary>
		public Vector3 CaculateVoxelCenter(int voxelX, int voxelY, int voxelZ)
		{
			return LDBVoxelPosition + new Vector3(voxelX, voxelY, voxelZ) * VoxelSize; ;
		}

        #region 二进制序列化和反序列化
        public void Serialize(BinaryWriter writer)
        {
            writer.Write(VoxelSize);
            writer.Write(VoxelCounts.x);
            writer.Write(VoxelCounts.y);
            writer.Write(VoxelCounts.z);

            writer.Write(LDBVoxelPosition.x);
            writer.Write(LDBVoxelPosition.y);
            writer.Write(LDBVoxelPosition.z);

            VoxelInfo[] voxelInfos = VoxelInfos;
            if (voxelInfos == null || voxelInfos.Length <= 0)
            {
                writer.Write(0);
            }
            else
            {
                writer.Write(voxelInfos.Length);
                for (int iVoxel = 0; iVoxel < voxelInfos.Length; iVoxel++)
                {
                    VoxelInfo voxelInfo = voxelInfos[iVoxel];
                    voxelInfo.Serialize(writer);
                }
            }
        }

        public void Deserialize(BinaryReader reader)
        {
            VoxelSize = reader.ReadInt32();

            Vector3 voxelCounts = default(Vector3);
            voxelCounts.x = reader.ReadSingle();
            voxelCounts.y = reader.ReadSingle();
            voxelCounts.z = reader.ReadSingle();
            VoxelCounts = voxelCounts;

            Vector3 lDBVoxelPosition = default(Vector3);
            lDBVoxelPosition.x = reader.ReadSingle();
            lDBVoxelPosition.y = reader.ReadSingle();
            lDBVoxelPosition.z = reader.ReadSingle();
            LDBVoxelPosition = lDBVoxelPosition;

            int voxelLength = reader.ReadInt32();
            if (voxelLength > 0)
            {
                VoxelInfos = new VoxelInfo[voxelLength];
                for (int iVoxel = 0; iVoxel < voxelLength; iVoxel++)
                {
                    VoxelInfo voxelInfo = new VoxelInfo();
                    voxelInfo.DeSerialize(reader);
                    VoxelInfos[iVoxel] = voxelInfo;
                }
            }
        }
        #endregion
    }
}