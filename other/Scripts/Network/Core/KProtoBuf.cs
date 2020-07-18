using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using Assets.Scripts;
using System.Runtime.InteropServices;
using Assets.Scripts.Proto;
using Assets.Scripts.Global;
using UnityEngine;

namespace Assets.Scripts.Lib.Net
{

    class KConstants
    {
		public const string LOG_TAG = "Net";

		public static int SERVER_FPS = 10;
        //public static string IP_HOST = "172.18.69.211";
        public static readonly int SIZE_OF_BYTE = 1;
        public static readonly int SIZE_OF_WORD = 2;
        public static string SKILL_ICON_DEF = "1";
        public static float PULL_BAKC_MAX_DISTANCE = 8.0f;
        public static float MOVE_IGNORE_DISTANCE = 0.1f;
        public static float MINI_FLOAT = 0.01f;
    }

    public class KProtoBuf : ICloneable
	{
        static public int GetUTF8BytesLength(byte[] byteArray)
        {
            //处理UTF8的Encoding.UTF8.GetBytes 返回数据里面带有过多的0数组.
            for (int index = 0; index < byteArray.Length; index++)
            {
                if (byteArray[index] == 0)
                    return index;
            }
            return byteArray.Length;
        }

        public KProtoBuf()
        {
        }

        public static string MemoryStreamToString(MemoryStream valueStream)
        {
            long currentPos = valueStream.Seek(0, SeekOrigin.Current);
            valueStream.Seek(0, SeekOrigin.Begin);
            byte[] abyValue = new byte[valueStream.Length];
            valueStream.Read(abyValue, 0, abyValue.Length);
            string sValue = System.Text.Encoding.ASCII.GetString(abyValue);
            valueStream.Seek(currentPos, SeekOrigin.Begin);
            return sValue;
        }

        public static string MemoryStreamToDebugString(MemoryStream valueStream)
        {
            long currentPos = valueStream.Seek(0, SeekOrigin.Current);
            valueStream.Seek(0, SeekOrigin.Begin);
            byte[] abyValue = new byte[valueStream.Length];
            valueStream.Read(abyValue, 0, abyValue.Length);
            valueStream.Seek(currentPos, SeekOrigin.Begin);

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < abyValue.Length; ++i)
                sb.Append((int)abyValue[i]).Append(' ');
            return sb.ToString();
        }

        public virtual void Pack(BinaryWriter writer)
		{			
		}

        public virtual void UnPack(BinaryReader reader)
		{
		}

		private static void WriteBytes(BinaryWriter bytes, byte[] src, int length)
		{
            bytes.Write(length);
            if(length > 0)
            {
                 bytes.Write(src, 0, length);
            }
		}
		
		private static byte[] ReadBytes(BinaryReader bytes, int length)
		{	
		    length = bytes.ReadInt32();
			if (length > 0)
			{
                return bytes.ReadBytes(length);
			}
            return null;
		}
		
		public static void WriteString(BinaryWriter bytes, string src, int length)
		{
            byte[] srcByByte = System.Text.Encoding.UTF8.GetBytes(src);
            int strLen = GetUTF8BytesLength(srcByByte);
			long begin = 0;
			long end = 0;
			if (length == 0)//没有指定发送长度,把整个字符串发过去
			{
				bytes.Write((short)0);
				begin = bytes.Seek(0, SeekOrigin.Current);
                bytes.Write(srcByByte, 0, strLen);
				bytes.Write((byte)0);
				end = bytes.Seek(0, SeekOrigin.Current);
				
				length = (int)(end - begin);
                bytes.Seek(-length - KConstants.SIZE_OF_WORD, SeekOrigin.Current);
				bytes.Write((short)length);
				bytes.Seek(length, SeekOrigin.Current);
			}
			else//指定发送长度,把整个字符串的前length长度的字节发送过去
			{
				begin = bytes.Seek(0, SeekOrigin.Current);
                if (strLen < length)
                {
                    bytes.Write(srcByByte, 0, strLen);
                    bytes.Write((byte)0);
                }
                else
                {
                    Debug.LogError("KProtoBuf Write String Error,string:"+src);
                    bytes.Write(srcByByte, 0, length);
                }
                end = bytes.Seek(0, SeekOrigin.Current);
				if (end - begin < length)
				{
                    bytes.Seek((int)(length - end + begin), SeekOrigin.Current);
				}
			}
		}

        public static string ReadString(BinaryReader bytes, int length)
		{
			if (length == 0)
				length = bytes.ReadInt16();
            byte[] b = bytes.ReadBytes(length);
            for (int i = 0; i < b.Length; i++)
            {
                if (b[i] == 0)
                {
                    length = i;
                    break;
                }
            }
			return Encoding.UTF8.GetString(b, 0, length);
		}

        public static void WriteByteArray(BinaryWriter writer, byte[] bytes, int byteCount)
        {
            writer.Write(bytes, 0, byteCount);
        }

        public static byte[] ReadByteArray(BinaryReader reader, int length)
		{
			if (length == 0)
                length = reader.ReadUInt16();

            byte[] bytes = reader.ReadBytes(length);
            return bytes;
		}

        public static T CreateProtoBufAndUnPack<T>(BinaryReader reader) where T : KProtoBuf
        {
            T obj = Activator.CreateInstance(typeof(T)) as T;
            obj.UnPack(reader);
            return obj;
        }

		object ICloneable.Clone() {
			return this.Clone();
		}
		
		public KProtoBuf Clone() {
			return (KProtoBuf) this.MemberwiseClone();
		}
	}
}
