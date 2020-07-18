using System.IO;
using UnityEngine;

namespace Map
{
	/// <summary>
	/// 在游戏中使用的Area的信息
	/// </summary>
	[System.Serializable]
	public class AreaInfo
	{
		/// <summary>
		/// <see cref="Area.Uid"/>
		/// </summary>
		public ulong Uid;
		/// <summary>
		/// 在<see cref="MapInfo.AreaInfos"/>中的Index
		/// </summary>
		public int Index;
		/// <summary>
		/// 世界空间的坐标
		/// </summary>
		[Tooltip("Area世界坐标")]
		public Vector3 Position;
		/// <summary>
		/// 世界空间的旋转
		/// </summary>
		[Tooltip("Area的旋转")]
		public Quaternion Rotation;
		/// <summary>
		/// 这个Area的AABB
		/// </summary>
		[Tooltip("Area 包围盒")]
		public Bounds AABB;
		/// <summary>
		/// 这个Area的直径
		/// </summary>
		[Tooltip("Area直径")]
		public float Diameter;
		/// <summary>
		/// <see cref="AreaDetailInfo"/>
		/// </summary>
		[Tooltip("资源路径")]
		public string DetailInfoAddressableKey;

        #region 二进制序列化和反序列化

        public void Serialize(BinaryWriter writer)
        {
            writer.Write(Uid);
            writer.Write(Index);

            writer.Write(Position.x);
            writer.Write(Position.y);
            writer.Write(Position.z);

            writer.Write(Rotation.x);
            writer.Write(Rotation.y);
            writer.Write(Rotation.z);
            writer.Write(Rotation.w);

            writer.Write(AABB.center.x);
            writer.Write(AABB.center.y);
            writer.Write(AABB.center.z);

            writer.Write(AABB.size.x);
            writer.Write(AABB.size.y);
            writer.Write(AABB.size.z);

            writer.Write(Diameter);
            writer.Write(DetailInfoAddressableKey);
        }

        public void Deserialize(BinaryReader reader)
        {
            Uid = reader.ReadUInt64();
            Index = reader.ReadInt32();
            Position.x = reader.ReadSingle();
            Position.y = reader.ReadSingle();
            Position.z = reader.ReadSingle();

            Rotation.x = reader.ReadSingle();
            Rotation.y = reader.ReadSingle();
            Rotation.z = reader.ReadSingle();
            Rotation.w = reader.ReadSingle();

            Vector3 aabbCenter = default(Vector3);
            aabbCenter.x = reader.ReadSingle();
            aabbCenter.y = reader.ReadSingle();
            aabbCenter.z = reader.ReadSingle();

            Vector3 aabbSize = default(Vector3);
            aabbSize.x = reader.ReadSingle();
            aabbSize.y = reader.ReadSingle();
            aabbSize.z = reader.ReadSingle();

            AABB = new Bounds(aabbCenter, aabbSize);

            Diameter = reader.ReadSingle();
            DetailInfoAddressableKey = reader.ReadString();
        }
        #endregion


    }
}