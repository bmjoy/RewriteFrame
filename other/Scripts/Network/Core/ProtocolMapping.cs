using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Scripts.Lib.Net;
using Assets.Scripts.Proto;

namespace Assets.Scripts.Lib.Net
{
    /// <summary>
    /// 协议处理器
    /// </summary>
    /// <param name="ar"></param>
    public delegate void ProtocolHandler(KProtoBuf ar);

    /// <summary>
    /// 协议处理器列表
    /// </summary>
    public class ProcessInfo
    {
        /// <summary>
        /// 协议ID
        /// </summary>
        public int protocolID;

        /// <summary>
        /// 协议内容
        /// </summary>
        public KProtoBuf protocolData = null;

        /// <summary>
        /// 处理事件
        /// </summary>
        public event ProtocolHandler OnProtocolHandler;

        /// <summary>
        /// 处理器列表
        /// </summary>
        public IList<ProtocolHandler> handlerList = new List<ProtocolHandler>();

        /// <summary>
        /// 构造器
        /// </summary>
        /// <param name="protoID"></param>
        /// <param name="type"></param>
        public ProcessInfo(int protoID, Type type)
        {
            protocolID = protoID;
            protocolData = (KProtoBuf)Activator.CreateInstance(type);
        }

        /// <summary>
        /// 添加处理器
        /// </summary>
        /// <param name="handler"></param>
        public void AddHandler(ProtocolHandler handler)
        {
            if (handler != null && !handlerList.Contains(handler))
            {
                handlerList.Add(handler);
                OnProtocolHandler += handler;
            }
        }

        /// <summary>
        /// 处理协议
        /// </summary>
        public void HandleProtocol()
        {
            try
            {
                OnProtocolHandler(protocolData);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError("HandleProtocol " + e);
                throw;
            }
        }
    }

    /// <summary>
    /// 协议映射表
    /// </summary>
    public class ProtocolMapping
    {
        private Dictionary<int, ProcessInfo> processInfoDict = new Dictionary<int, ProcessInfo>();

        /// <summary>
        /// 添加协议处理器
        /// </summary>
        /// <param name="protocolId"></param>
        /// <param name="handler"></param>
        /// <param name="type"></param>
        public void AddProtocolHandler(int protocolId, ProtocolHandler handler, Type type)
        {
            ProcessInfo info;
            processInfoDict.TryGetValue(protocolId, out info);
            if (info == null)
            {
                info = new ProcessInfo(protocolId, type);
                processInfoDict.Add(protocolId, info);
            }
            info.AddHandler(handler);
        }

        /// <summary>
        /// 查找协议处理器
        /// </summary>
        /// <param name="protocolId"></param>
        /// <returns></returns>
        public ProcessInfo GetProcessInfo(int protocolId)
        {
            ProcessInfo info;
            processInfoDict.TryGetValue(protocolId, out info);
            return info;
        }

    }
}
