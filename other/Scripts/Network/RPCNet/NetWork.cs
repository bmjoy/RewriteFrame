using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Reflection;
using Google.Protobuf;
using System;
using DotNetty.Buffers;

namespace Crucis.Protocol
{
    public class NetWork
    {
        public static NetWork Instance { get; private set; }
        
        private readonly Dictionary<string, Connection> connections = new Dictionary<string, Connection>();

        private readonly List<BaseService> gameServices = new List<BaseService>();

        private static AttributeSystem attributeSystem;

        public static AttributeSystem AttributeSystem
        {
            get
            {
                return attributeSystem ?? (attributeSystem = new AttributeSystem());
            }
        }

        public static MessageDispatcherComponent messageDispatcherComponent;

        public OpcodeTypeComponent opcodeTypeComponent = new OpcodeTypeComponent();

        public IMessagePacker MessagePacker { get; set; }

        public IMessageDispatcher MessageDispatcher { get; set; }

        public static void InitNetWorkStstem(Assembly assembly)
        {
            NetWork.AttributeSystem.Add(assembly);
            NetWork.Instance = new NetWork();
        }

        public int Count
        {
            get { return this.connections.Count; }
        }

        public NetWork()
        {
            MessagePacker = new ProtobufPacker();
            MessageDispatcher = new OuterMessageDispatcher();

            NetWork.messageDispatcherComponent = new MessageDispatcherComponent(this);
        }

        public void AddService(BaseService gameService)
        {
            gameServices.Add(gameService);
        }

        public void RemoveService(BaseService gameService)
        {
            gameServices.Remove(gameService);
        }

        public void UpdateServicesState(string uri, ConnectState type)
        {
            foreach (var s in gameServices)
            {
                if (s.uri_.Equals(uri))
                    s.SetConnectState(type);
            }
        }

        public async Task<Connection> ConnectAsync(string remote_address)
        {
            if (connections.ContainsKey(remote_address))
            {
                return this.connections[remote_address];
            }

            Connection c = new Connection(this);
            var channel = await c.ConnectAsync(remote_address);
            if (channel == null)
            {
                UpdateServicesState(remote_address, ConnectState.ConnectFailed);
                return null;
            }

            this.connections.Add(remote_address, c);
            UpdateServicesState(remote_address, ConnectState.Connected);
            return c;
        }
        
        public virtual void Remove(string remote_address, int i)
        {
            Connection c;
            if (!this.connections.TryGetValue(remote_address, out c))
            {
                return;
            }
            this.connections.Remove(remote_address);
            c.Dispose();
        }

        public virtual void Remove(Connection c)
        {
            while (this.connections.ContainsValue(c))
            {
                foreach (var pair_kv in this.connections)
                {
                    if (pair_kv.Value == c)
                    {
                        this.connections.Remove(pair_kv.Key);
                        UpdateServicesState(pair_kv.Key, ConnectState.Disconnected);
                        break;
                    }
                }
            }
            c.Dispose();
        }

        public Connection GetConnectionAsync(string remote_address)
        {
            Connection c = null;
            this.connections.TryGetValue(remote_address, out c);
            //if (c == null)
            //    c = await ConnectAsync(remote_address);
            return c;
        }

        public void Dispose()
        {
            foreach (Connection session in this.connections.Values.ToArray())
            {
                session.Dispose();
            }
            connections.Clear();

            Instance = null;
        }

        #region 消息发送相关函数

        public async Task<IMessage> CallAsync(string uri, uint serviceid, uint functionid, IMessage args)
        {
            Head package = new Head() { ServiceID = serviceid, FunctionID = functionid, Type = ContentType.RequestType, Body = args.ToByteString() };
            Connection c = GetConnectionAsync(uri);
            if(c != null)
                return await c.CallAsync(package);
            else
                throw new Exception($"No Connection, URL: {uri}");
        }

        public void Send(string uri, uint serviceid, uint functionid, IMessage args)
        {
            Head package = new Head() { ServiceID = serviceid, FunctionID = functionid, Type = ContentType.RequestType, Body = args.ToByteString() };
            Connection c = GetConnectionAsync(uri);
            if (c != null)
                 c.Send(package);
            else
                throw new Exception($"No Connection, URL: {uri}");
        }

        public BaseStream CreateStreamAsync(string uri, uint serviceid, uint functionid, IMessage args, StreamType st)
        {
            Head package = new Head() { ServiceID = serviceid, FunctionID = functionid, Type = ContentType.CreateStreamType, Body = args.ToByteString() };
            Connection c = GetConnectionAsync(uri);
            if (c != null)
                return c.CreateStream(package, st);
            else
            {
                UnityEngine.Debug.LogError($"No Connection, URL: {uri}");
                return null;
            }
        }

        public void CloseStreamAsync(uint streamid, string uri, uint serviceid, uint functionid)
        {
            Head package = new Head() { ServiceID = serviceid, FunctionID = functionid, Type = ContentType.CloseStreamType };
            Connection c = GetConnectionAsync(uri);
            if (c != null)
                c.CloseStream(streamid, package);
            else
                throw new Exception($"No Connection, URL: {uri}");
        }

        public void StreamSend(uint streamid, string uri, uint serviceid, uint functionid, IMessage args)
        {
            Head package = new Head() { ServiceID = serviceid, FunctionID = functionid, Type = ContentType.RequestType, Body = args.ToByteString() };
            Connection c = GetConnectionAsync(uri);
            if (c != null)
                c.StreamSend(streamid, package);
            else
                throw new Exception($"No Connection, URL: {uri}");
        }

        public void OldPLSend(string uri, uint serviceid, uint functionid, IMessage args)
        {
            Head package = new Head() { ServiceID = serviceid, FunctionID = functionid, Type = ContentType.OldProto, Body = args.ToByteString() };
            Connection c = GetConnectionAsync(uri);
            if (c != null)
                c.OldPLSend(package);
            else
                throw new Exception($"No Connection, URL: {uri}");
        }
        #endregion
    }
}
