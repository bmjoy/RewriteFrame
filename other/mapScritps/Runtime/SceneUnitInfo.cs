using System.IO;
using UnityEngine;

namespace Map
{
	[System.Serializable]
	public struct SceneUnitInfo
	{
		/// <summary>
		/// 在<see cref="AreaInfo.SceneUnitInfos"/>中的Index
		/// </summary>
		public int Index;
		/// <summary>
		/// 相对于Area的坐标
		/// </summary>
		public Vector3 LocalPosition;
		/// <summary>
		/// 相对于Area的旋转
		/// </summary>
		public Quaternion LocalRotation;
		/// <summary>
		/// 相对于Area的缩放
		/// </summary>
		public Vector3 LocalScale;
		/// <summary>
		/// 这个Unit的Asset，<see cref="AreaDetailInfo.AssetInfos"/>
		/// </summary>
		public int AssetIndex;
		/// <summary>
		/// 这个Unit下的Renderer
		/// </summary>
		public RendererInfo[] RendererInfos;
#if UNITY_EDITOR
		/// <summary>
		/// 导出过程中用的临时变量
		/// </summary>
		internal Bounds _AABB;
		/// <summary>
		/// 导出过程中用的临时变量
		/// </summary>
		internal float _Diameter;
#endif

        #region 二进制序列化和反序列化
        public void Serialize(BinaryWriter writer)
        {
            writer.Write(Index);
            Vector3 localPosition = LocalPosition;
            writer.Write(localPosition.x);
            writer.Write(localPosition.y);
            writer.Write(localPosition.z);

            Quaternion localRotation = LocalRotation;
            writer.Write(localRotation.x);
            writer.Write(localRotation.y);
            writer.Write(localRotation.z);
            writer.Write(localRotation.w);

            Vector3 localScale = LocalScale;
            writer.Write(localScale.x);
            writer.Write(localScale.y);
            writer.Write(localScale.z);

            writer.Write(AssetIndex);

            RendererInfo[] rendererInfos = RendererInfos;
            if (rendererInfos == null || rendererInfos.Length <= 0)
            {
                writer.Write(0);
            }
            else
            {
                writer.Write(rendererInfos.Length);
                for (int iRender = 0; iRender < rendererInfos.Length; iRender++)
                {
                    RendererInfo rendererInfo = rendererInfos[iRender];
                    rendererInfo.Serialize(writer);
                }
            }

            //正式时，这个数据不用导出
#if false
            Bounds unitAABB = _AABB;
            Vector3 aabbCenter = unitAABB.center;
            writer.Write(aabbCenter.x);
            writer.Write(aabbCenter.y);
            writer.Write(aabbCenter.z);

            Vector3 size = unitAABB.size;
            writer.Write(size.x);
            writer.Write(size.y);
            writer.Write(size.z);
#endif
        }

        public void DeSerialize(BinaryReader reader)
        {
            Index = reader.ReadInt32();
            Vector3 localPosition = default(Vector3);
            localPosition.x = reader.ReadSingle();
            localPosition.y = reader.ReadSingle();
            localPosition.z = reader.ReadSingle();
            LocalPosition = localPosition;

            Quaternion localRotation = default(Quaternion);
            localRotation.x = reader.ReadSingle();
            localRotation.y = reader.ReadSingle();
            localRotation.z = reader.ReadSingle();
            localRotation.w = reader.ReadSingle();
            LocalRotation = localRotation;

            Vector3 localScale = default(Vector3);
            localScale.x = reader.ReadSingle();
            localScale.y = reader.ReadSingle();
            localScale.z = reader.ReadSingle();
            LocalScale = localScale;

            AssetIndex = reader.ReadInt32();

            int renderInfoCount = reader.ReadInt32();
            if (renderInfoCount > 0)
            {
                RendererInfos = new RendererInfo[renderInfoCount];
                for (int iRender = 0; iRender < renderInfoCount; iRender++)
                {
                    RendererInfo rendererInfo = new RendererInfo();
                    rendererInfo.DeSerialize(reader);
                    RendererInfos[iRender] = rendererInfo;
                }
            }




            //正式不导出 只是为了测试
#if false
                    Vector3 aabbCenter = default(Vector3);
                    aabbCenter.x = reader.ReadSingle();
                    aabbCenter.y = reader.ReadSingle();
                    aabbCenter.z = reader.ReadSingle();

                    Vector3 aabbSize = default(Vector3);
                    aabbSize.x = reader.ReadSingle();
                    aabbSize.y = reader.ReadSingle();
                    aabbSize.z = reader.ReadSingle();

                    _AABB = new Bounds(aabbCenter, aabbSize);
#endif
        }
        #endregion
    }
}