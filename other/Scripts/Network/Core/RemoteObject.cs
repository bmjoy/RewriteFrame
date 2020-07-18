using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Assets.Scripts.Logic.RemoteCall
{
    public enum KSDType
    {
        kstInvalid = 0, //无效的类型
        kstBool = 1, //bool类型
        kstInt8 = 2, //8位int
        kstInt16 = 3, //16位int
        kstInt32 = 4, //32位int
        kstUInt8 = 5, //8位uint
        kstUInt16 = 6, //16位uint
        kstUInt32 = 7, //32位uint
        kstFloat = 8, //32位float
        kstDouble = 9, //64位double
        kstString = 10, //8位描述长度的,变长string
        kstBigString = 11, //16位描述长度的,变长string(暂时没做)
        kstUInt64 = 12, //64位uint
        kstDataStream = 13, //嵌套stream类型
        kstNull = 14,//对应lua的nil
    };

    public class RemoteObject
    {
        static private MemoryStream tempStrData = null;
        static private BinaryWriter tempStrWriter = null;

        static public bool WriteString(BinaryWriter write, string strData)
        {
            if (tempStrWriter == null)
            {
                tempStrData = new MemoryStream();
                tempStrWriter = new BinaryWriter(tempStrData);
            }

            var str = strData.ToCharArray();
            long strStartPos = tempStrWriter.Seek(0, SeekOrigin.Begin);
            tempStrWriter.Write(str);
            long strEndPos = tempStrWriter.Seek(0, SeekOrigin.Current);
            long strLength = strEndPos - strStartPos;
            if (strLength < 256)
            {
                write.Write((byte)(KSDType.kstString));
                write.Write((byte)strLength);
                write.Write(str);
                write.Write((byte)0);
            }
            else if (strLength < 1024)
            {
                write.Write((byte)(KSDType.kstBigString));
                write.Write((ushort)strLength);
                write.Write(str);
                write.Write((byte)0);
            }
            else
            {
                return false;
            }
            return true;
        }

        public virtual bool GetBool()
        {
            throw new NotImplementedException();
        }

        public virtual byte GetUInt8()
        {
            throw new NotImplementedException();
        }

        public virtual ushort GetUInt16()
        {
            throw new NotImplementedException();
        }

        public virtual uint GetUInt32()
        {
            throw new NotImplementedException();
        }

        public virtual sbyte GetInt8()
        {
            throw new NotImplementedException();
        }

        public virtual short GetInt16()
        {
            throw new NotImplementedException();
        }

        public virtual int GetInt32()
        {
            throw new NotImplementedException();
        }

        public virtual string GetString()
        {
            throw new NotImplementedException();
        }

        public virtual ulong GetUInt64()
        {
            throw new NotImplementedException();
        }

        public virtual float GetFloat()
        {
            throw new NotImplementedException();
        }

        public virtual double GetDouble()
        {
            throw new NotImplementedException();
        }

        public static implicit operator sbyte(RemoteObject f)
        {
            return f.GetInt8();
        }

        public static implicit operator byte(RemoteObject f)
        {
            return f.GetUInt8();
        }

        public static implicit operator short(RemoteObject f)
        {
            return f.GetInt16();
        }

        public static implicit operator ushort(RemoteObject f)
        {
            return f.GetUInt16();
        }

        public static implicit operator int(RemoteObject f)
        {
            return f.GetInt32();
        }

        public static implicit operator uint(RemoteObject f)
        {
            return f.GetUInt32();
        }

        public static implicit operator string(RemoteObject f)
        {
            return f.GetString();
        }

        public static implicit operator bool(RemoteObject f)
        {
            return f.GetBool();
        }

        public static implicit operator ulong(RemoteObject f)
        {
            return f.GetUInt64();
        }

        public virtual RemoteObject this [int index]
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public virtual RemoteObject this [string name]
        {
            get{ throw new NotImplementedException(); }
            set{ throw new NotImplementedException(); }
        }

        public virtual KSDType GetRemoteObjType()
        {
            return KSDType.kstInvalid;
        }

        public virtual bool WritePack(BinaryWriter write)
        {
            return false;
        }
    }

    public class RemoteBool : RemoteObject
    {
        public bool Value;

        private RemoteBool(bool val)
        {
            Value = val;
        }

        public override bool GetBool()
        {
            return Value;
        }

        private static int useCount = 0;

        public static void ResetPosition()
        {
            useCount = 0;
        }

        private static List<RemoteBool> pool = new List<RemoteBool>();

        public static void ClearPool()
        {
            pool.Clear();
            ResetPosition();
        }

        public static RemoteBool GetItem(bool val)
        {
            RemoteBool item;

            if (useCount >= pool.Count)
            {
                item = new RemoteBool(val);
                pool.Add(item);

            }
            else
            {
                item = pool[useCount];
                item.Value = val;
            }
            useCount++;

            return item;
        }

        public override KSDType GetRemoteObjType()
        {
            return KSDType.kstBool;
        }

        public override bool WritePack(BinaryWriter write)
        {
            write.Write((byte)(KSDType.kstBool));
            write.Write(Value);
            return true;
        }
    }

    public class RemoteUInt8 : RemoteObject
    {
        public byte Value;

        private RemoteUInt8(byte val)
        {
            Value = val;
        }

        public override byte GetUInt8()
        {
            return Convert.ToByte(Value);
        }

        public override ushort GetUInt16()
        {
            return Convert.ToUInt16(Value);
        }

        public override uint GetUInt32()
        {
            return Convert.ToUInt32(Value);
        }

        public override sbyte GetInt8()
        {
            return Convert.ToSByte(Value);
        }

        public override short GetInt16()
        {
            return Convert.ToInt16(Value);
        }

        public override int GetInt32()
        {
            return Convert.ToInt32(Value);
        }

        public override float GetFloat()
        {
            return Convert.ToSingle(Value);
        }

        public override double GetDouble()
        {
            return Convert.ToDouble(Value);
        }

        public override string GetString()
        {
            return Value.ToString();
        }

        private static int useCount = 0;

        public static void ResetPosition()
        {
            useCount = 0;
        }

        private static List<RemoteUInt8> pool = new List<RemoteUInt8>();

        public static void ClearPool()
        {
            pool.Clear();
            ResetPosition();
        }

        public static RemoteUInt8 GetItem(byte val)
        {
            RemoteUInt8 item;
            
            if (useCount >= pool.Count)
            {
                item = new RemoteUInt8(val);
                pool.Add(item);
                
            }
            else
            {
                item = pool[useCount];
                item.Value = val;
            }
            useCount++;

            return item;
        }

        public override KSDType GetRemoteObjType()
        {
            return KSDType.kstUInt8;
        }

        public override bool WritePack(BinaryWriter write)
        {
            write.Write((byte)(KSDType.kstUInt8));
            write.Write(Value);
            return true;
        }
    }

    public class RemoteUInt16 : RemoteObject
    {
        public ushort Value;

        private RemoteUInt16(ushort val)
        {
            Value = val;
        }

        public override byte GetUInt8()
        {
            return Convert.ToByte(Value);
        }

        public override ushort GetUInt16()
        {
            return Convert.ToUInt16(Value);
        }

        public override uint GetUInt32()
        {
            return Convert.ToUInt32(Value);
        }

        public override sbyte GetInt8()
        {
            return Convert.ToSByte(Value);
        }

        public override short GetInt16()
        {
            return Convert.ToInt16(Value);
        }

        public override int GetInt32()
        {
            return Convert.ToInt32(Value);
        }

        public override float GetFloat()
        {
            return Convert.ToSingle(Value);
        }

        public override double GetDouble()
        {
            return Convert.ToDouble(Value);
        }

        public override string GetString()
        {
            return Value.ToString();
        }

        private static int useCount = 0;

        public static void ResetPosition()
        {
            useCount = 0;
        }

        private static List<RemoteUInt16> pool = new List<RemoteUInt16>();

        public static void ClearPool()
        {
            pool.Clear();
            ResetPosition();
        }

        public static RemoteUInt16 GetItem(ushort val)
        {
            RemoteUInt16 item;
            
            if (useCount >= pool.Count)
            {
                item = new RemoteUInt16(val);
                pool.Add(item);
                
            }
            else
            {
                item = pool[useCount];
                item.Value = val;
            }
            useCount++;

            return item;
        }

        public override KSDType GetRemoteObjType()
        {
            return KSDType.kstUInt16;
        }

        public override bool WritePack(BinaryWriter write)
        {
            write.Write((byte)(KSDType.kstUInt16));
            write.Write(Value);
            return true;
        }
    }

    public class RemoteUInt32 : RemoteObject
    {
        public uint Value;

        private RemoteUInt32(uint val)
        {
            Value = val;
        }

        public override byte GetUInt8()
        {
            return Convert.ToByte(Value);
        }

        public override ushort GetUInt16()
        {
            return Convert.ToUInt16(Value);
        }

        public override uint GetUInt32()
        {
            return Convert.ToUInt32(Value);
        }

        public override sbyte GetInt8()
        {
            return Convert.ToSByte(Value);
        }

        public override short GetInt16()
        {
            return Convert.ToInt16(Value);
        }

        public override int GetInt32()
        {
            return Convert.ToInt32(Value);
        }

        public override float GetFloat()
        {
            return Convert.ToSingle(Value);
        }

        public override double GetDouble()
        {
            return Convert.ToDouble(Value);
        }

        public override string GetString()
        {
            return Value.ToString();
        }

        private static int useCount = 0;

        public static void ResetPosition()
        {
            useCount = 0;
        }

        private static List<RemoteUInt32> pool = new List<RemoteUInt32>();

        public static void ClearPool()
        {
            pool.Clear();
            ResetPosition();
        }

        public static RemoteUInt32 GetItem(uint val)
        {
            RemoteUInt32 item;
            
            if (useCount >= pool.Count)
            {
                item = new RemoteUInt32(val);
                pool.Add(item);
                
            }
            else
            {
                item = pool[useCount];
                item.Value = val;
            }
            useCount++;

            return item;
        }

        public override KSDType GetRemoteObjType()
        {
            return KSDType.kstUInt32;
        }

        public override bool WritePack(BinaryWriter write)
        {
            write.Write((byte)(KSDType.kstUInt32));
            write.Write(Value);
            return true;
        }
    }

    public class RemoteInt8 : RemoteObject
    {
        public sbyte Value;

        private RemoteInt8(sbyte val)
        {
            Value = val;
        }

        public override byte GetUInt8()
        {
            return Convert.ToByte(Value);
        }

        public override ushort GetUInt16()
        {
            return Convert.ToUInt16(Value);
        }

        public override uint GetUInt32()
        {
            return Convert.ToUInt32(Value);
        }

        public override sbyte GetInt8()
        {
            return Convert.ToSByte(Value);
        }

        public override short GetInt16()
        {
            return Convert.ToInt16(Value);
        }

        public override int GetInt32()
        {
            return Convert.ToInt32(Value);
        }

        public override float GetFloat()
        {
            return Convert.ToSingle(Value);
        }

        public override double GetDouble()
        {
            return Convert.ToDouble(Value);
        }

        public override string GetString()
        {
            return Value.ToString();
        }

        private static int useCount = 0;

        public static void ResetPosition()
        {
            useCount = 0;
        }

        private static List<RemoteInt8> pool = new List<RemoteInt8>();

        public static void ClearPool()
        {
            pool.Clear();
            ResetPosition();
        }

        public static RemoteInt8 GetItem(sbyte val)
        {
            RemoteInt8 item;
            
            if (useCount >= pool.Count)
            {
                item = new RemoteInt8(val);
                pool.Add(item);
                
            }
            else
            {
                item = pool[useCount];
                item.Value = val;
            }
            useCount++;

            return item;
        }

        public override KSDType GetRemoteObjType()
        {
            return KSDType.kstInt8;
        }

        public override bool WritePack(BinaryWriter write)
        {
            write.Write((byte)(KSDType.kstInt8));
            write.Write(Value);
            return true;
        }
    }

    public class RemoteInt16 : RemoteObject
    {
        public short Value;

        private RemoteInt16(short val)
        {
            Value = val;
        }

        public override byte GetUInt8()
        {
            return Convert.ToByte(Value);
        }

        public override ushort GetUInt16()
        {
            return Convert.ToUInt16(Value);
        }

        public override uint GetUInt32()
        {
            return Convert.ToUInt32(Value);
        }

        public override sbyte GetInt8()
        {
            return Convert.ToSByte(Value);
        }

        public override short GetInt16()
        {
            return Convert.ToInt16(Value);
        }

        public override int GetInt32()
        {
            return Convert.ToInt32(Value);
        }

        public override float GetFloat()
        {
            return Convert.ToSingle(Value);
        }

        public override double GetDouble()
        {
            return Convert.ToDouble(Value);
        }

        public override string GetString()
        {
            return Value.ToString();
        }

        private static int useCount = 0;

        public static void ResetPosition()
        {
            useCount = 0;
        }

        private static List<RemoteInt16> pool = new List<RemoteInt16>();

        public static void ClearPool()
        {
            pool.Clear();
            ResetPosition();
        }

        public static RemoteInt16 GetItem(short val)
        {
            RemoteInt16 item;
            
            if (useCount >= pool.Count)
            {
                item = new RemoteInt16(val);
                pool.Add(item);
                
            }
            else
            {
                item = pool[useCount];
                item.Value = val;
            }
            useCount++;

            return item;
        }

        public override KSDType GetRemoteObjType()
        {
            return KSDType.kstInt16;
        }

        public override bool WritePack(BinaryWriter write)
        {
            write.Write((byte)(KSDType.kstInt16));
            write.Write(Value);
            return true;
        }
    }

    public class RemoteInt32 : RemoteObject
    {
        public int Value;

        private RemoteInt32(int val)
        {
            Value = val;
        }

        public override byte GetUInt8()
        {
            return Convert.ToByte(Value);
        }

        public override ushort GetUInt16()
        {
            return Convert.ToUInt16(Value);
        }

        public override uint GetUInt32()
        {
            return Convert.ToUInt32(Value);
        }

        public override sbyte GetInt8()
        {
            return Convert.ToSByte(Value);
        }

        public override short GetInt16()
        {
            return Convert.ToInt16(Value);
        }

        public override int GetInt32()
        {
            return Convert.ToInt32(Value);
        }

        public override float GetFloat()
        {
            return Convert.ToSingle(Value);
        }

        public override double GetDouble()
        {
            return Convert.ToDouble(Value);
        }

        public override string GetString()
        {
            return Value.ToString();
        }

        private static int useCount = 0;

        public static void ResetPosition()
        {
            useCount = 0;
        }

        private static List<RemoteInt32> pool = new List<RemoteInt32>();

        public static void ClearPool()
        {
            pool.Clear();
            ResetPosition();
        }

        public static RemoteInt32 GetItem(int val)
        {
            RemoteInt32 item;
            
            if (useCount >= pool.Count)
            {
                item = new RemoteInt32(val);
                pool.Add(item);
                
            }
            else
            {
                item = pool[useCount];
                item.Value = val;
            }
            useCount++;

            return item;
        }

        public override KSDType GetRemoteObjType()
        {
            return KSDType.kstInt32;
        }

        public override bool WritePack(BinaryWriter write)
        {
            write.Write((byte)(KSDType.kstInt32));
            write.Write(Value);
            return true;
        }
    }

    public class RemoteFloat : RemoteObject
    {
        public float Value;

        private RemoteFloat(float val)
        {
            Value = val;
        }

        public override byte GetUInt8()
        {
            return Convert.ToByte(Value);
        }

        public override ushort GetUInt16()
        {
            return Convert.ToUInt16(Value);
        }

        public override uint GetUInt32()
        {
            return Convert.ToUInt32(Value);
        }

        public override sbyte GetInt8()
        {
            return Convert.ToSByte(Value);
        }

        public override short GetInt16()
        {
            return Convert.ToInt16(Value);
        }

        public override int GetInt32()
        {
            return Convert.ToInt32(Value);
        }

        public override float GetFloat()
        {
            return Convert.ToSingle(Value);
        }

        public override double GetDouble()
        {
            return Convert.ToDouble(Value);
        }

        public override string GetString()
        {
            return Value.ToString();
        }

        private static int useCount = 0;

        public static void ResetPosition()
        {
            useCount = 0;
        }

        private static List<RemoteFloat> pool = new List<RemoteFloat>();

        public static void ClearPool()
        {
            pool.Clear();
            ResetPosition();
        }

        public static RemoteFloat GetItem(float val)
        {
            RemoteFloat item;
            
            if (useCount >= pool.Count)
            {
                item = new RemoteFloat(val);
                pool.Add(item);
                
            }
            else
            {
                item = pool[useCount];
                item.Value = val;
            }
            useCount++;

            return item;
        }

        public override KSDType GetRemoteObjType()
        {
            return KSDType.kstFloat;
        }

        public override bool WritePack(BinaryWriter write)
        {
            write.Write((byte)(KSDType.kstFloat));
            write.Write(Value);
            return true;
        }
    }

    public class RemoteDouble : RemoteObject
    {
        public double Value;

        private RemoteDouble(double val)
        {
            Value = val;
        }

        public override byte GetUInt8()
        {
            return Convert.ToByte(Value);
        }

        public override ushort GetUInt16()
        {
            return Convert.ToUInt16(Value);
        }

        public override uint GetUInt32()
        {
            return Convert.ToUInt32(Value);
        }

        public override sbyte GetInt8()
        {
            return Convert.ToSByte(Value);
        }

        public override short GetInt16()
        {
            return Convert.ToInt16(Value);
        }

        public override int GetInt32()
        {
            return Convert.ToInt32(Value);
        }

        public override float GetFloat()
        {
            return Convert.ToSingle(Value);
        }

        public override double GetDouble()
        {
            return Convert.ToDouble(Value);
        }

        public override string GetString()
        {
            return Value.ToString();
        }

        private static int useCount = 0;

        public static void ResetPosition()
        {
            useCount = 0;
        }

        private static List<RemoteDouble> pool = new List<RemoteDouble>();

        public static void ClearPool()
        {
            pool.Clear();
            ResetPosition();
        }

        public static RemoteDouble GetItem(double val)
        {
            RemoteDouble item;
            
            if (useCount >= pool.Count)
            {
                item = new RemoteDouble(val);
                pool.Add(item);
                
            }
            else
            {
                item = pool[useCount];
                item.Value = val;
            }
            useCount++;

            return item;
        }

        public override KSDType GetRemoteObjType()
        {
            return KSDType.kstDouble;
        }

        public override bool WritePack(BinaryWriter write)
        {
            write.Write((byte)(KSDType.kstDouble));
            write.Write(Value);
            return true;
        }
    }

    public class RemoteUInt64 : RemoteObject
    {
        public UInt64 Value;

        private RemoteUInt64(UInt64 val)
        {
            Value = val;
        }

        public override UInt64 GetUInt64()
        {
            return Value;
        }

        public override string GetString()
        {
            return Value.ToString();
        }

        private static int useCount = 0;

        public static void ResetPosition()
        {
            useCount = 0;
        }

        private static List<RemoteUInt64> pool = new List<RemoteUInt64>();

        public static void ClearPool()
        {
            pool.Clear();
            ResetPosition();
        }

        public static RemoteUInt64 GetItem(UInt64 val)
        {
            RemoteUInt64 item;
            
            if (useCount >= pool.Count)
            {
                item = new RemoteUInt64(val);
                pool.Add(item);
                
            }
            else
            {
                item = pool[useCount];
                item.Value = val;
            }
            useCount++;

            return item;
        }

        public override KSDType GetRemoteObjType()
        {
            return KSDType.kstUInt64;
        }

        public override bool WritePack(BinaryWriter write)
        {
            write.Write((byte)(KSDType.kstUInt64));
            write.Write(Value);
            return true;
        }
    }

    public class RemoteString : RemoteObject
    {
        public string Value;

        private RemoteString(string val)
        {
            Value = val;
        }

        public override string GetString()
        {
            return Value;
        }

        private static int useCount = 0;

        public static void ResetPosition()
        {
            useCount = 0;
        }

        private static List<RemoteString> pool = new List<RemoteString>();

        public static void ClearPool()
        {
            pool.Clear();
            ResetPosition();
        }

        public static RemoteString GetItem(string val)
        {
            RemoteString item;
            
            if (useCount >= pool.Count)
            {
                item = new RemoteString(val);
                pool.Add(item);
                
            }
            else
            {
                item = pool[useCount];
                item.Value = val;
            }
            useCount++;

            return item;
        }

        public override KSDType GetRemoteObjType()
        {
            return KSDType.kstString;
        }

        public override bool WritePack(BinaryWriter write)
        {
            return WriteString(write, Value);
        }

    }

    public class RemoteTable : RemoteObject
    {
        public Dictionary<object, object> dictKV = new Dictionary<object, object>();

        public bool ContainsKey(object _key)
        {
            return dictKV.ContainsKey(_key);
        }

        public int Count
        {
            get
            {
                return dictKV.Count;
            }
        }

        public RemoteObject GetItemForLua(string name)
        {
            if (!dictKV.ContainsKey(name))
            {
                return null;
            }
            return dictKV[name] as RemoteObject; 
        }

        public RemoteTable GetTableForLua(string name)
        {
            if (!dictKV.ContainsKey(name))
            {
                return null;
            }
            return dictKV[name] as RemoteTable; 
        }

        public override RemoteObject this [int index]
        {
            get
            {
                if (!dictKV.ContainsKey(index))
                {
                    return null;
                }
                return dictKV[index] as RemoteObject;
            }
            set
            {
                dictKV[index] = value;
            }
        }

        public override RemoteObject this [string name]
        {
            get
            {
                if (!dictKV.ContainsKey(name))
                {
                    return null;
                }
                return dictKV[name] as RemoteObject; 
            }
            set
            {
                dictKV[name] = value; 
            }
        }

        public override KSDType GetRemoteObjType()
        {
            return KSDType.kstDataStream;
        }

        public bool WriteKey(BinaryWriter write, object key)
        {
            Type keyType = key.GetType();
            if (keyType == typeof(int))
            {
                int tempKey = (int)key;
                write.Write((byte)(KSDType.kstInt32));
                write.Write(tempKey);
                return true;
            }
            else if (keyType == typeof(string))
            {
                string tempKey = (string)key;
                return WriteString(write, tempKey);
            }
            return false;
        }

        public override bool WritePack(BinaryWriter write)
        {
            ushort packSize = 0;
            write.Write((byte)(KSDType.kstDataStream));
            long beignPos = write.Seek(0, SeekOrigin.Current);
            write.Write(packSize);
            //write data
            foreach (KeyValuePair<object, object> pairData in dictKV)
            {
                if (!WriteKey(write, pairData.Key))
                {
                    Debug.LogError("RemoteTable WritePack error, WriteKey error");
                    return false;
                }
                RemoteObject valueData = pairData.Value as RemoteObject;
                if (valueData == null)
                {
                    Debug.LogError("RemoteTable WritePack error, Value Type error");
                    return false;
                }
                if (!valueData.WritePack(write))
                {
                    Debug.LogError("RemoteTable WritePack error, WritePack error");
                    return false;
                }
            }

            long endPos = write.Seek(0, SeekOrigin.Current);
            long writeLength = endPos - beignPos - sizeof(ushort);
            if (writeLength < 0 || writeLength >= 0xFFFF)
            {
                Debug.LogError("RemoteTable WritePack error, pack size error, length:"+ writeLength);
                return false;
            }
            packSize = (ushort)writeLength;
            write.Seek((int)beignPos, SeekOrigin.Begin);
            write.Write(packSize);
            write.Seek((int)endPos, SeekOrigin.Begin);
            return true;
        }
    }
}
