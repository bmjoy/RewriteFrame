using System.IO;
using UnityEngine;

namespace Map
{
	/// <summary>
	/// Renderer信息，用于
	///		在游戏中还原Lightmap
	/// </summary>
	[System.Serializable]
	public struct RendererInfo
	{
		/// <summary>
		/// 这个Renderer Component相对于Unit的Index，<see cref="Transform.GetChild(int)"/>
		/// </summary>
		public int[] TransformIndexs;
		/// <summary>
		/// <see cref="Renderer.lightmapIndex"/>
		/// </summary>
		public int LightmapIndex;
		/// <summary>
		/// <see cref="Renderer.lightmapScaleOffset"/>
		/// </summary>
		public Vector4 LightmapScaleOffset;

        public int RealLightmapIndex;

        public Vector4 RealLightmapScaleOffset;

        #region 二进制序列化和反序列化
        public void Serialize(BinaryWriter writer)
        {
            int[] transformIndexs = TransformIndexs;
            if (transformIndexs == null || transformIndexs.Length <= 0)
            {
                writer.Write(0);
            }
            else
            {
                writer.Write(transformIndexs.Length);
                for (int iIndex = 0; iIndex < transformIndexs.Length; iIndex++)
                {
                    writer.Write(transformIndexs[iIndex]);
                }
            }
            writer.Write(LightmapIndex);

            Vector4 lightmapScaleOffset = LightmapScaleOffset;
            writer.Write(lightmapScaleOffset.x);
            writer.Write(lightmapScaleOffset.y);
            writer.Write(lightmapScaleOffset.z);
            writer.Write(lightmapScaleOffset.w);

            writer.Write(RealLightmapIndex);

            Vector4 realLightmapScaleOffset = RealLightmapScaleOffset;
            writer.Write(realLightmapScaleOffset.x);
            writer.Write(realLightmapScaleOffset.y);
            writer.Write(realLightmapScaleOffset.z);
            writer.Write(realLightmapScaleOffset.w);
        }

        public void DeSerialize(BinaryReader reader)
        {
            int tranIndexCount = reader.ReadInt32();
            if (tranIndexCount > 0)
            {
                TransformIndexs = new int[tranIndexCount];
                for (int iTrans = 0; iTrans < tranIndexCount; iTrans++)
                {
                    TransformIndexs[iTrans] = reader.ReadInt32();
                }
            }
            LightmapIndex = reader.ReadInt32();
            Vector4 lightMapScalOffest = default(Vector4);
            lightMapScalOffest.x = reader.ReadSingle();
            lightMapScalOffest.y = reader.ReadSingle();
            lightMapScalOffest.z = reader.ReadSingle();
            lightMapScalOffest.w = reader.ReadSingle();
            LightmapScaleOffset = lightMapScalOffest;

            RealLightmapIndex = reader.ReadInt32();
            Vector4 realLightMapScalOffest = default(Vector4);
            realLightMapScalOffest.x = reader.ReadSingle();
            realLightMapScalOffest.y = reader.ReadSingle();
            realLightMapScalOffest.z = reader.ReadSingle();
            realLightMapScalOffest.w = reader.ReadSingle();
            RealLightmapScaleOffset = realLightMapScalOffest;
        }
        #endregion
    }
}