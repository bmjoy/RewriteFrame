using Assets.Scripts.Lib;
using Assets.Scripts.Lib.Net;
using Assets.Scripts.Logic.RemoteCall;
using Assets.Scripts.Proto;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace Game.Frame.Net
{
    public class AbsRpcController : BaseNetController
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public AbsRpcController()
        {
            NetworkManager.Instance.ListenGameServer(KS2C_Protocol.s2c_call_script, OnCallScript, typeof(S2C_CALL_SCRIPT));
        }

        #region "公共接口"

        /// <summary>
        /// 调用GameServer接口
        /// </summary>
        /// <param name="strFunc"></param>
        /// <param name="values"></param>
        protected void CallGameServer(string strFunc, params object[] values)
        {
            CallServer(KC2S_Protocol.c2s_call_gs, strFunc, values);
        }

        /// <summary>
        /// 调用LogicServer接口
        /// </summary>
        /// <param name="strFunc"></param>
        /// <param name="values"></param>
        protected void CallLogicServer(string strFunc, params object[] values)
        {
            CallServer(KC2S_Protocol.c2s_call_ls, strFunc, values);
        }

        /// <summary>
        /// 调用ZoneServer接口
        /// </summary>
        /// <param name="strFunc"></param>
        /// <param name="values"></param>
        protected void CallZoneServer(string strFunc, params object[] values)
        {
            CallServer(KC2S_Protocol.c2s_call_zs, strFunc, values);
        }

        /// <summary>
        /// 注册对象的指定方法为本地函数
        /// </summary>
        /// <param name="methodInfo"></param>
        protected void RegisterMethod(object obj, string name)
        {
            MethodInfo method = obj.GetType().GetMethod(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            if (method != null)
            {
                RegisterMethod(method);
            }
        }

        /// <summary>
        /// 注册对象的所有方法为本地函数
        /// </summary>
        /// <param name="obj"></param>
        protected void RegisterAllMethod(object obj)
        {
            MethodInfo[] methodInfos = obj.GetType().GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            if (methodInfos.Length > 0)
            {
                for (int index = 0; index < methodInfos.Length; index++)
                {
                    RegisterMethod(methodInfos[index]);
                }
            }
        }

        #endregion

        #region "消息的接收逻辑"

        private Dictionary<uint, MethodInfo> hash2MethodInfo = new Dictionary<uint, MethodInfo>();

        /// <summary>
        /// 注册本地方法
        /// </summary>
        /// <param name="methodInfo"></param>
        private void RegisterMethod(MethodInfo methodInfo)
        {
            uint methodHash = HashFunName(methodInfo.Name);
            if (!hash2MethodInfo.ContainsKey(methodHash))
            {
                hash2MethodInfo.Add(methodHash, methodInfo);
            }
        }

        /// <summary>
        /// 处理服务器到客户端的调用
        /// </summary>
        /// <param name="buf"></param>
        private void OnCallScript(KProtoBuf buf)
        {
            S2C_CALL_SCRIPT respond = buf as S2C_CALL_SCRIPT;

            ArrayList remoteObjects = UnpackAll(new MemoryStream(respond.data));

            uint methodHash = (uint)remoteObjects[0];
            MethodInfo methodInfo = hash2MethodInfo.ContainsKey(methodHash) ? hash2MethodInfo[methodHash] : null;
            if (methodInfo == null)
            {
                //Debugger.LogErrorFormat("Remote call method error, no such function, protocolID = {0}, hashCode = {1}", respond.protocolID, methodHash);
                return;
            }

            //Debug.LogWarning(methodInfo.Name);

            if (Application.isEditor)
            {
                try
                {
                    methodInfo.Invoke(this, FixRemoteCallParams(remoteObjects, methodInfo.GetParameters()));
                }
                catch (System.Exception ex)
                {
                    Debug.LogError("Remote call method error in function hashCode = " + methodHash + " Methodinfo Name: " + methodInfo.Name);
                    Debug.LogException(ex);
                }
            }
            else
            {
                methodInfo.Invoke(this, FixRemoteCallParams(remoteObjects, methodInfo.GetParameters()));
            }

            //回收临时数据
            RemoteBool.ResetPosition();
            RemoteInt8.ResetPosition();
            RemoteInt16.ResetPosition();
            RemoteInt32.ResetPosition();
            RemoteUInt8.ResetPosition();
            RemoteUInt16.ResetPosition();
            RemoteUInt32.ResetPosition();
            RemoteFloat.ResetPosition();
            RemoteDouble.ResetPosition();
            RemoteString.ResetPosition();
            RemoteString.ResetPosition();
            RemoteUInt64.ResetPosition();
        }

        /// <summary>
        /// 根据调用函数的接受参数，进行参数修正(数据压缩可能会把lua的number转换成各种类型,这个操作是反向修正参数接入c#的函数).
        /// </summary>
        /// <param name="array"></param>
        /// <param name="paramsInfo"></param>
        /// <returns></returns>
        private object[] FixRemoteCallParams(ArrayList array, ParameterInfo[] paramsInfo)
        {
            object[] parameters = new object[array.Count - 1];
            ParameterInfo callFunParam = null;
            int callFunParamIndex = 0;
            for (int i = 1; i < array.Count; i++)
            {
                callFunParamIndex = i - 1;
                callFunParam = paramsInfo[callFunParamIndex];
                if (callFunParam.ParameterType == typeof(int))
                {
                    parameters[callFunParamIndex] = Convert.ToInt32(array[i]);
                }
                else if (callFunParam.ParameterType == typeof(uint))
                {
                    parameters[callFunParamIndex] = Convert.ToUInt32(array[i]);
                }
                else if (callFunParam.ParameterType == typeof(float))
                {
                    parameters[callFunParamIndex] = Convert.ToSingle(array[i]);
                }
                else if (callFunParam.ParameterType == typeof(double))
                {
                    parameters[callFunParamIndex] = Convert.ToDouble(array[i]);
                }
                else if (callFunParam.ParameterType == typeof(ulong) && array[i].GetType() == typeof(string))
                {
                    parameters[callFunParamIndex] = ulong.Parse((string)array[i]);
                }
                else if (callFunParam.ParameterType == typeof(long) || callFunParam.ParameterType == typeof(ulong))
                {
                    parameters[callFunParamIndex] = Convert.ToInt64(array[i]);
                }
                else if (callFunParam.ParameterType.IsSubclassOf(typeof(Enum)))
                {
                    parameters[callFunParamIndex] = Convert.ToInt32(array[i]);
                }
                else
                {
                    parameters[callFunParamIndex] = array[i];
                }
            }
            return parameters;
        }


        private BinaryReader reader;

        private ArrayList UnpackAll(MemoryStream stream)
        {
            ArrayList retArray = new ArrayList();
            reader = new BinaryReader(stream);

            while (stream.Position != stream.Length)
            {
                UnPackUp(stream, reader, retArray);
            }

            reader.Close();
            return retArray;
        }

        private KSDType UnPackUp(MemoryStream stream, BinaryReader reader, ArrayList retArray)
        {
            KSDType eRet = (KSDType)stream.ReadByte();
            switch (eRet)
            {
                case KSDType.kstBool:
                    UnPackupBoolean(stream, reader, retArray);
                    break;
                case KSDType.kstUInt8:
                    UnPackupUInt8(stream, reader, retArray);
                    break;
                case KSDType.kstUInt16:
                    UnPackupUInt16(stream, reader, retArray);
                    break;
                case KSDType.kstUInt32:
                    UnPackupUInt32(stream, reader, retArray);
                    break;
                case KSDType.kstInt8:
                    UnPackupInt8(stream, reader, retArray);
                    break;
                case KSDType.kstInt16:
                    UnPackupInt16(stream, reader, retArray);
                    break;
                case KSDType.kstInt32:
                    UnPackupInt32(stream, reader, retArray);
                    break;
                case KSDType.kstFloat:
                    UnPackupFloat(stream, reader, retArray);
                    break;
                case KSDType.kstDouble:
                    UnPackupDouble(stream, reader, retArray);
                    break;
                case KSDType.kstString:
                    UnPackupString(stream, reader, retArray);
                    break;
                case KSDType.kstBigString:
                    UnPackupBigString(stream, reader, retArray);
                    break;
                case KSDType.kstUInt64:
                    UnPackupUInt64(stream, reader, retArray);
                    break;
                case KSDType.kstDataStream:
                    UnPackTable(stream, reader, retArray);
                    break;
                case KSDType.kstNull:
                    UnPackupNull(stream, reader, retArray);
                    break;
                default:
                    Debug.LogError("RemoteCall UnPackUp Error, type  unsupported, type:" + eRet);
                    eRet = KSDType.kstInvalid;
                    break;
            }
            return eRet;
        }

        private uint UnPackupBoolean(MemoryStream stream, BinaryReader read, ArrayList retArray)
        {
            bool v = read.ReadBoolean();
            retArray.Add(v);
            return sizeof(bool);
        }

        private uint UnPackupUInt8(MemoryStream stream, BinaryReader read, ArrayList retArray)
        {
            byte v = read.ReadByte();
            retArray.Add(v);
            return sizeof(byte);
        }

        private uint UnPackupUInt16(MemoryStream stream, BinaryReader read, ArrayList retArray)
        {
            ushort v = read.ReadUInt16();
            retArray.Add(v);
            return sizeof(ushort);
        }

        private uint UnPackupUInt32(MemoryStream stream, BinaryReader read, ArrayList retArray)
        {
            uint v = read.ReadUInt32();
            retArray.Add(v);
            return sizeof(uint);
        }

        private uint UnPackupInt8(MemoryStream stream, BinaryReader read, ArrayList retArray)
        {
            sbyte v = read.ReadSByte();
            retArray.Add(v);
            return sizeof(sbyte);
        }

        private uint UnPackupInt16(MemoryStream stream, BinaryReader read, ArrayList retArray)
        {
            short v = read.ReadInt16();
            retArray.Add(v);
            return sizeof(short);
        }

        private uint UnPackupInt32(MemoryStream stream, BinaryReader read, ArrayList retArray)
        {
            int v = read.ReadInt32();
            retArray.Add(v);
            return sizeof(int);
        }

        private uint UnPackupFloat(MemoryStream stream, BinaryReader read, ArrayList retArray)
        {
            float v = read.ReadSingle();
            retArray.Add(v);
            return sizeof(float);
        }

        private uint UnPackupDouble(MemoryStream stream, BinaryReader read, ArrayList retArray)
        {
            double v = read.ReadDouble();
            retArray.Add(v);
            return sizeof(double);
        }

        private uint UnPackupString(MemoryStream stream, BinaryReader read, ArrayList retArray)
        {
            //读取长度描述为一个字符的string.
            long startPos = stream.Position;
            byte length = read.ReadByte();
            if (length > 0)
            {
                byte[] ret = read.ReadBytes(length);
                string v = Encoding.UTF8.GetString(ret, 0, length);
                retArray.Add(v);
            }
            else
            {
                retArray.Add("");
            }
            byte stringEnd = read.ReadByte();
            return (uint)(stream.Position - startPos);
        }

        private uint UnPackupBigString(MemoryStream stream, BinaryReader read, ArrayList retArray)
        {
            //读取长度描述为两个字符的string.
            long startPos = stream.Position;
            ushort length = read.ReadUInt16();
            if (length > 0)
            {
                byte[] ret = read.ReadBytes(length);
                string v = Encoding.UTF8.GetString(ret, 0, length);
                retArray.Add(v);
            }
            else
            {
                retArray.Add("");
            }
            byte stringEnd = read.ReadByte();
            return (uint)(stream.Position - startPos);
        }

        private uint UnPackupUInt64(MemoryStream stream, BinaryReader read, ArrayList retArray)
        {
            ulong v = read.ReadUInt64();
            retArray.Add(v);
            return sizeof(ulong);
        }

        private uint UnPackupNull(MemoryStream stream, BinaryReader read, ArrayList retArray)
        {
            retArray.Add(null);
            return 0;
        }

        private bool IsNumberType(KSDType ksdType)
        {
            if (ksdType == KSDType.kstUInt8 ||
                ksdType == KSDType.kstUInt16 ||
                ksdType == KSDType.kstUInt32 ||
                ksdType == KSDType.kstInt8 ||
                ksdType == KSDType.kstInt16 ||
                ksdType == KSDType.kstInt32 ||
                ksdType == KSDType.kstFloat ||
                ksdType == KSDType.kstDouble)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private ushort UnPackTable(MemoryStream stream, BinaryReader read, ArrayList retArray, ushort tabSize = 0)
        {
            if (tabSize == 0)
            {
                tabSize = read.ReadUInt16();
            }

            uint tabEnd = (uint)(stream.Position) + tabSize;
            RemoteTable luaTab = new RemoteTable();

            while (stream.Position < tabEnd)
            {
                ArrayList tabKV = new ArrayList();

                KSDType keyType = UnPackUp(stream, read, tabKV);
                KSDType valueType = UnPackUp(stream, read, tabKV);

                if (IsNumberType(keyType))
                {
                    int nIndex = Convert.ToInt32(tabKV[0]);
                    switch (valueType)
                    {
                        case KSDType.kstBool:
                            luaTab[nIndex] = RemoteBool.GetItem((bool)tabKV[1]);
                            break;
                        case KSDType.kstInt8:
                            luaTab[nIndex] = RemoteInt8.GetItem((sbyte)tabKV[1]);
                            break;
                        case KSDType.kstInt16:
                            luaTab[nIndex] = RemoteInt16.GetItem((short)tabKV[1]);
                            break;
                        case KSDType.kstInt32:
                            luaTab[nIndex] = RemoteInt32.GetItem((int)tabKV[1]);
                            break;
                        case KSDType.kstUInt8:
                            luaTab[nIndex] = RemoteUInt8.GetItem((byte)tabKV[1]);
                            break;
                        case KSDType.kstUInt16:
                            luaTab[nIndex] = RemoteUInt16.GetItem((ushort)tabKV[1]);
                            break;
                        case KSDType.kstUInt32:
                            luaTab[nIndex] = RemoteUInt32.GetItem((uint)tabKV[1]);
                            break;
                        case KSDType.kstFloat:
                            luaTab[nIndex] = RemoteFloat.GetItem((float)tabKV[1]);
                            break;
                        case KSDType.kstDouble:
                            luaTab[nIndex] = RemoteDouble.GetItem((double)tabKV[1]);
                            break;
                        case KSDType.kstString:
                            luaTab[nIndex] = RemoteString.GetItem((string)tabKV[1]);
                            break;
                        case KSDType.kstBigString:
                            luaTab[nIndex] = RemoteString.GetItem((string)tabKV[1]);
                            break;
                        case KSDType.kstUInt64:
                            luaTab[nIndex] = RemoteUInt64.GetItem((ulong)tabKV[1]);
                            break;
                        case KSDType.kstDataStream:
                            luaTab[nIndex] = tabKV[1] as RemoteTable;
                            break;
                        case KSDType.kstNull:
                            luaTab[nIndex] = null;
                            break;
                        default:
                            Debug.LogError("UnKnow ValueType");
                            break;
                    }
                }
                else if (keyType == KSDType.kstString)
                {
                    string name = tabKV[0] as string;
                    switch (valueType)
                    {
                        case KSDType.kstBool:
                            luaTab[name] = RemoteBool.GetItem((bool)tabKV[1]);
                            break;
                        case KSDType.kstInt8:
                            luaTab[name] = RemoteInt8.GetItem((sbyte)tabKV[1]);
                            break;
                        case KSDType.kstInt16:
                            luaTab[name] = RemoteInt16.GetItem((short)tabKV[1]);
                            break;
                        case KSDType.kstInt32:
                            luaTab[name] = RemoteInt32.GetItem((int)tabKV[1]);
                            break;
                        case KSDType.kstUInt8:
                            luaTab[name] = RemoteUInt8.GetItem((byte)tabKV[1]);
                            break;
                        case KSDType.kstUInt16:
                            luaTab[name] = RemoteUInt16.GetItem((ushort)tabKV[1]);
                            break;
                        case KSDType.kstUInt32:
                            luaTab[name] = RemoteUInt32.GetItem((uint)tabKV[1]);
                            break;
                        case KSDType.kstFloat:
                            luaTab[name] = RemoteFloat.GetItem((float)tabKV[1]);
                            break;
                        case KSDType.kstDouble:
                            luaTab[name] = RemoteDouble.GetItem((double)tabKV[1]);
                            break;
                        case KSDType.kstString:
                            luaTab[name] = RemoteString.GetItem((string)tabKV[1]);
                            break;
                        case KSDType.kstBigString:
                            luaTab[name] = RemoteString.GetItem((string)tabKV[1]);
                            break;
                        case KSDType.kstUInt64:
                            luaTab[name] = RemoteUInt64.GetItem((ulong)tabKV[1]);
                            break;
                        case KSDType.kstDataStream:
                            luaTab[name] = tabKV[1] as RemoteTable;
                            break;
                        case KSDType.kstNull:
                            luaTab[name] = null;
                            break;
                        default:
                            Debug.LogError("UnKnow ValueType");
                            break;
                    }
                }
                else
                {
                    Debug.LogError("RemoteCall UnPackTable error, key type error");
                    throw new NotImplementedException();
                }
            }
            retArray.Add(luaTab);
            return tabSize;
        }

        #endregion

        #region "消息的发送逻辑"

        private static BinaryWriter callServerBuffWriter = null;
        private static C2S_CALL_SERVER callServerProto = null;

        private static MemoryStream tempStrData = null;
        private static BinaryWriter tempStrWriter = null;


        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="protocol"></param>
        /// <param name="strFunc"></param>
        /// <param name="values"></param>
        private void CallServer(KC2S_Protocol protocol, string strFunc, params object[] values)
        {
            if (callServerProto == null)
            {
                callServerProto = SingleInstanceCache.GetInstanceByType<C2S_CALL_SERVER>();
                callServerProto.data = new byte[1024];//第一次创建，申请足够的大小..
                callServerBuffWriter = new BinaryWriter(new MemoryStream(callServerProto.data));
            }
            callServerBuffWriter.Seek(0, SeekOrigin.Begin);

            callServerProto.protocolID = (byte)protocol;
            callServerBuffWriter.Write((short)0);
            WriteParam(callServerBuffWriter, strFunc, values);

            callServerProto._dataLength_ = (int)callServerBuffWriter.Seek(0, SeekOrigin.Current);
            callServerBuffWriter.Seek(0, SeekOrigin.Begin);
            callServerBuffWriter.Write((short)(callServerProto._dataLength_ - 2));
            callServerBuffWriter.Seek(callServerProto._dataLength_, SeekOrigin.Begin);

            NetworkManager.Instance.SendToGameServer(callServerProto);
        }

        private bool WriteParam(BinaryWriter write, string strFunc, params object[] values)
        {
            //写入remotecall 包头
            uint hashCode = HashFunName(strFunc);
            write.Write((byte)(KSDType.kstUInt32));
            write.Write(hashCode);

            foreach (object o in values)
            {
                Type type = o.GetType();
                if (type == typeof(bool))
                {
                    write.Write((byte)(KSDType.kstBool));
                    write.Write((bool)o);
                }
                else if (type == typeof(byte))
                {
                    write.Write((byte)(KSDType.kstUInt8));
                    write.Write((byte)o);
                }
                else if (type == typeof(ushort))
                {
                    write.Write((byte)(KSDType.kstUInt16));
                    write.Write((ushort)o);
                }
                else if (type == typeof(uint))
                {
                    write.Write((byte)(KSDType.kstUInt32));
                    write.Write((uint)o);
                }
                else if (type == typeof(sbyte))
                {
                    write.Write((byte)(KSDType.kstInt8));
                    write.Write((sbyte)o);
                }
                else if (type == typeof(short))
                {
                    write.Write((byte)(KSDType.kstInt16));
                    write.Write((short)o);
                }
                else if (type == typeof(int))
                {
                    write.Write((byte)(KSDType.kstInt32));
                    write.Write((int)o);
                }
                else if (type == typeof(float))
                {
                    write.Write((byte)(KSDType.kstFloat));
                    write.Write((float)o);
                }
                else if (type == typeof(double))
                {
                    write.Write((byte)(KSDType.kstDouble));
                    write.Write((double)o);
                }
                else if (type == typeof(string))
                {
                    string tempStr = o as string;
                    WriteString(write, tempStr);
                }
                else if (type == typeof(ulong))
                {
                    write.Write((byte)(KSDType.kstUInt64));
                    write.Write((ulong)o);
                }
                else if (type == typeof(RemoteTable))
                {
                    RemoteTable dataTable = o as RemoteTable;
                    dataTable.WritePack(write);
                }
                else
                {
                    Debug.LogError("Call Script Has UnSuport Type");
                    return false;
                }
            }
            return true;
        }

        private bool WriteString(BinaryWriter write, string value)
        {
            if (tempStrData == null)
            {
                tempStrData = new MemoryStream();
                tempStrWriter = new BinaryWriter(tempStrData);
            }

            //利用一个临时的BinaryWriter来计算字符串的真正长度(byte数组).
            var str = value.ToCharArray();
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

        #endregion

        #region "工具函数"

        /// <summary>
        /// 计算函数名的Hash值
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private uint HashFunName(string str)
        {
            uint seed = 131; // 31 131 1313 13131 131313 etc..  
            uint hash = 0;
            int strlen = str.Length;
            for (int i = 0; i < strlen; ++i)
            {
                hash = hash * seed + (str[i]);
            }
            return (hash & 0x7FFFFFFF);
        }

        #endregion
    }
}