using System.IO;

namespace Map
{
	[System.Serializable]
	public struct AssetInfo
	{
		public string AddressableKey;

        #region 二进制序列化和反序列化
        public void Serialize(BinaryWriter writer)
        {
            writer.Write(AddressableKey);
        }

        public void DeSerialize(BinaryReader reader)
        {
            AddressableKey = reader.ReadString();
        }
        #endregion
    }
}