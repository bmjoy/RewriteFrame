using System.IO;

namespace Map
{
	/// <summary>
	/// <see cref="http://wiki.leyoutech.com/pages/viewpage.action?pageId=17761761"/>
	/// </summary>
	[System.Serializable]
	public struct VoxelInfo
	{
		/// <summary>
		/// 如果是UnitVoxel：
		///		当玩家位于这个Voxel下需要显示的Unit
		///		按相机位于Voxel的中心时，这个物体的屏占比降序
		///		
		///	如果是AreaVoxel：
		///		玩家位于这个Voxel时可能所处的Area
		///		具体位于哪个Area，按玩家到Area的距离/Area的直径来算
		/// </summary>
		public int[] Indexs;

        #region 二进制序列化和反序列化
        public void Serialize(BinaryWriter writer)
        {
            if (Indexs == null || Indexs.Length <= 0)
            {
                writer.Write(0);
            }
            else
            {
                writer.Write(Indexs.Length);
                for (int iIndex = 0; iIndex < Indexs.Length; iIndex++)
                {
                    writer.Write(Indexs[iIndex]);
                }
            }
        }

        public void DeSerialize(BinaryReader reader)
        {
            int voxelIndexCount = reader.ReadInt32();
            if (voxelIndexCount > 0)
            {
                Indexs = new int[voxelIndexCount];
                for (int iVoxelInfo = 0; iVoxelInfo < voxelIndexCount; iVoxelInfo++)
                {
                    Indexs[iVoxelInfo] = reader.ReadInt32();
                }
            }
        }
        #endregion
    }
}