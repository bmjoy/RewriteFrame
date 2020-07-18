using AsyncCollection;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Crucis.Protocol
{
    public enum StreamType
    {
        Call_RW,
        Send_W,
        Stream_RW,
        Stream_R,
        Stream_W,
        OldPL,
    }

    public abstract class BaseStream
    {
        protected static uint StreamId_Mark { get; set; }

        protected uint stream_id_;

        protected Connection connection_;

        protected AsyncCollection<Head> responses_ = new AsyncCollection<Head>();

        public AsyncCollection<object> messages_ = new AsyncCollection<object>();

        public uint StreamId => stream_id_;

        public Action OnClosed;

        public abstract uint StreamCreate(Head create, Connection connection);

        public void StreamClose()
        {
            // 管理生命周期，删除RPC流
            connection_.ClearStream(stream_id_);
            responses_.Add(null);
            messages_.Add(null);

            Leyoutech.Utility.DebugUtility.Log("StreamReadWrightStream close", $"  ID: {stream_id_}");
            // 流关闭回调
            SafeInvoke(OnClosed, "Stream Closed");
        }

        private void SafeInvoke(Action action, string log)
        {
            try
            {
                action?.Invoke();
            }
            catch (Exception e)
            {
                Debug.Fail($"SafeInvoke Error: {log}, ErrorMessage:{e.Message}");
            }
        }

        public void StreamRead(Head responce)
        {
            responses_.Add(responce);
        }

        public abstract void ReadActionAsync();

        protected abstract void UnPack(Head responce);

        public virtual Task<IMessage> StreamWrightAsync(Head request, Connection connection) { return null; }

        public virtual void StreamWright(Head request, Connection connection) { return; }
    }

    /// <summary>
    /// 单次普通读写流，一次读写后关闭
    /// </summary>
    public class CallRWStream : BaseStream
    {
        private Action<IMessage> response_action_;

        public CallRWStream()
        {
            stream_id_ = ++StreamId_Mark;
        }

        public override uint StreamCreate(Head create, Connection connection)
        {
            return stream_id_;
        }

        public override async void ReadActionAsync()
        {
            Head head = await responses_.TakeAsync();
            if (head != null)
            {
                head.FunctionID = head.FunctionID * 2 + 1;
                UnPack(head);
            }
        }

        protected override void UnPack(Head responce)
        {
            try
            {
                uint OpCode = OpcodeTypeComponent.InnerOpCode((ushort)responce.ServiceID, (ushort)responce.FunctionID);
                object message_data;
                try
                {
                    OpcodeTypeComponent opcodeTypeComponent = connection_.netWork.opcodeTypeComponent;
                    object instance = opcodeTypeComponent.GetInstance(OpCode);

                    message_data = connection_.netWork.MessagePacker
                        .DeserializeFrom(instance, responce.Body.ToByteArray(), 0, responce.Body.Length);
                }
                catch (Exception e)
                {
                    // 出现任何消息解析异常都要断开Session，防止客户端伪造消息
                    Debug.Fail($"package error; opcode: {(ushort)responce.FunctionID} {connection_.netWork.Count} {e} ");
                    connection_.netWork.Remove(connection_);
                    return;
                }

                if (responce.Type == ContentType.ResponseType)
                {
                    // PRC CALL信息返回，任务回调-正确消息或错误消息
                    IMessage imessage = message_data as IMessage;
                    response_action_(imessage);
                }
                else if(responce.Type == ContentType.ErrorType)
                {
                    StreamClose();
                    Debug.Fail($"Error; ServiceID: {responce.ServiceID}, FunctionID: {responce.FunctionID}");
                    throw new Exception($"Error; ServiceID: {responce.ServiceID}, FunctionID: {responce.FunctionID}");
                }
                else
                {
                    StreamClose();
                    Debug.Fail($"Error responce message, Type:{responce.Type}; OpCode: {(ushort)responce.FunctionID}");
                    throw new Exception($"Error responce message, Type:{responce.Type}; OpCode: {(ushort)responce.FunctionID}");
                }
            }
            finally
            {
                StreamClose();
            }
        }

        public override Task<IMessage> StreamWrightAsync(Head request, Connection connection)
        {
            request.StreamID = stream_id_;
            connection_ = connection;

            var tcs = new TaskCompletionSource<IMessage>();
            response_action_ = (response) =>
            {
                try
                {
                    tcs.SetResult(response);
                }
                catch (Exception e)
                {
                    tcs.SetException(new Exception($"Rpc Error: {request.FunctionID}", e));
                }
            };

            ReadActionAsync();
            connection.FlushMessageAsync(request);
            return tcs.Task;
        }
    }

    /// <summary>
    /// 单次普通只写流，写一次后关闭
    /// </summary>
    public class SendWStream : BaseStream
    {
        public SendWStream()
        {
            stream_id_ = ++StreamId_Mark;
        }

        public override uint StreamCreate(Head create, Connection connection)
        {
            return stream_id_;
        }

        public override async void ReadActionAsync()
        {
            Head head = await responses_.TakeAsync();
            if (head != null)
            {
                head.FunctionID = head.FunctionID * 2 + 1;
                UnPack(head);
            }
        }

        protected override void UnPack(Head responce)
        {
            try
            {
                // void返回类型的只处理错误类型的消息返回
                if (responce.Type == ContentType.ErrorType)
                {
                    StreamClose();
                    Debug.Fail($"Error responce message, Type:{responce.Type}; OpCode: {(ushort)responce.FunctionID}");
                    throw new Exception($"Error responce message, Type:{responce.Type}; OpCode: {(ushort)responce.FunctionID}");
                }
            }
            finally
            {
                StreamClose();
            }
        }

        public override void StreamWright(Head request, Connection connection)
        {
            request.StreamID = stream_id_;
            connection_ = connection;

            connection.FlushMessageAsync(request);
        }
    }

    /// <summary>
    /// STREAM长链读写流，初始化建立连接，由服务端关闭或客户端退出登陆后，关闭
    /// </summary>
    public class StreamReadWrightStream : BaseStream
    {
        public StreamReadWrightStream()
        {
            stream_id_ = ++StreamId_Mark;

            Leyoutech.Utility.DebugUtility.Log("StreamReadWrightStream create", $"  ID: {stream_id_}");
        }

        public override uint StreamCreate(Head create, Connection connection)
        {
            this.StreamWright(create, connection);

            ReadActionAsync();
            return stream_id_;
        }

        public override async void ReadActionAsync()
        {
            Head head;
            while ((head = await responses_.TakeAsync()) != null)
            {
                 head.FunctionID = head.FunctionID * 2 + 1;
                 UnPack(head);
            }
            responses_.Dispose();
        }

        protected override void UnPack(Head responce)
        {
            // TODO:如果是关闭类型，则关闭该流
            if (responce.Type == ContentType.CloseStreamType)
            {
                StreamClose();
                return;
            }

            // 正常读取流信息，分发处理
            uint OpCode = OpcodeTypeComponent.InnerOpCode((ushort)responce.ServiceID, (ushort)responce.FunctionID);
            object message_data;
            try
            {
                OpcodeTypeComponent opcodeTypeComponent = connection_.netWork.opcodeTypeComponent;
                object instance = opcodeTypeComponent.GetInstance(OpCode);

                message_data = connection_.netWork.MessagePacker
                    .DeserializeFrom(instance, responce.Body.ToByteArray(), 0, responce.Body.Length);
            }
            catch (Exception e)
            {
                // 出现任何消息解析异常都要断开Session，防止客户端伪造消息
                Debug.Fail($"package error; opcode: {(ushort)responce.FunctionID} {connection_.netWork.Count} {e} ");
                connection_.netWork.Remove(connection_);
                return;
            }

            messages_.Add(message_data);
            //connection_.netWork.MessageDispatcher
            //    .Dispatch(connection_, (uint)OpCode, message_data);
        }

        public override void StreamWright(Head request, Connection connection)
        {
            // 流正常的写，建立对应的流
            request.StreamID = stream_id_;
            connection_ = connection;
            connection.FlushMessageAsync(request);
        }
    }

    /// <summary>
    /// STREAM长链读流，初始化建立连接
    /// </summary>
    public class StreamReadStream : BaseStream
    {
        public StreamReadStream()
        {
            stream_id_ = ++StreamId_Mark;
        }

        public override uint StreamCreate(Head create, Connection connection)
        {
            this.StreamWright(create, connection);

            ReadActionAsync();
            return stream_id_;
        }

        public override async void ReadActionAsync()
        {
            Head head;
            while ((head = await responses_.TakeAsync()) != null)
            {
                head.FunctionID = head.FunctionID * 2 + 1;
                UnPack(head);
            }
            responses_.Dispose();
        }

        protected override void UnPack(Head responce)
        {
            // TODO:如果是关闭类型，则关闭该流
            if (responce.Type == ContentType.CloseStreamType)
            {
                StreamClose();
                return;
            }

            // 正常读取流信息，分发处理
            uint OpCode = OpcodeTypeComponent.InnerOpCode((ushort)responce.ServiceID, (ushort)responce.FunctionID);
            object message_data;
            try
            {
                OpcodeTypeComponent opcodeTypeComponent = connection_.netWork.opcodeTypeComponent;
                object instance = opcodeTypeComponent.GetInstance(OpCode);

                message_data = connection_.netWork.MessagePacker
                    .DeserializeFrom(instance, responce.Body.ToByteArray(), 0, responce.Body.Length);
            }
            catch (Exception e)
            {
                // 出现任何消息解析异常都要断开Session，防止客户端伪造消息
                Debug.Fail($"package error; opcode: {(ushort)responce.FunctionID} {connection_.netWork.Count} {e} ");
                connection_.netWork.Remove(connection_);
                return;
            }

            messages_.Add(message_data);
            //connection_.netWork.MessageDispatcher
            //    .Dispatch(connection_, (uint)OpCode, message_data);
        }

        public override void StreamWright(Head request, Connection connection)
        {
            // 流正常的写，建立对应的流
            request.StreamID = stream_id_;
            connection_ = connection;
            connection.FlushMessageAsync(request);
        }
    }

    /// <summary>
    /// STREAM长链写流
    /// </summary>
    public class StreamWrightStream : BaseStream
    {
        public StreamWrightStream()
        {
            stream_id_ = ++StreamId_Mark;
        }

        public override uint StreamCreate(Head create, Connection connection)
        {
            this.StreamWright(create, connection);

            ReadActionAsync();
            return stream_id_;
        }

        public override async void ReadActionAsync()
        {
            Head head;
            while ((head = await responses_.TakeAsync()) != null)
            {
                head.FunctionID = head.FunctionID * 2 + 1;
                UnPack(head);
            }
            responses_.Dispose();
        }

        protected override void UnPack(Head responce)
        {
            // TODO:如果是关闭类型，则关闭该流
            if (responce.Type == ContentType.CloseStreamType)
            {
                StreamClose();
                return;
            }
        }

        public override void StreamWright(Head request, Connection connection)
        {
            // 流正常的写，建立对应的流
            request.StreamID = stream_id_;
            connection_ = connection;
            connection.FlushMessageAsync(request);
        }
    }

    /// <summary>
    /// 特殊流，支持老协议的流，在整个客户端整个生命周期都存在
    /// </summary>
    public class OldPLStream : BaseStream
    {
        public OldPLStream()
        {
            stream_id_ = ++StreamId_Mark;
        }

        public override uint StreamCreate(Head create, Connection connection)
        {
            connection_ = connection;

            ReadActionAsync();
            return stream_id_;
        }

        public override async void ReadActionAsync()
        {
            Head head;
            while ((head = await responses_.TakeAsync()) != null)
            {
                UnPack(head);
            }
            responses_.Dispose();
        }

        protected override void UnPack(Head responce)
        {
            // TODO:如果是关闭类型，则关闭该流
            if (responce.Type == ContentType.CloseStreamType)
            {
                StreamClose();
                return;
            }

            // 正常读取流信息，分发处理
            uint OpCode = OpcodeTypeComponent.InnerOpCode((ushort)responce.ServiceID, (ushort)responce.FunctionID);
            object message_data;
            try
            {
                OpcodeTypeComponent opcodeTypeComponent = connection_.netWork.opcodeTypeComponent;
                object instance = opcodeTypeComponent.GetInstance(OpCode);

                message_data = connection_.netWork.MessagePacker
                    .DeserializeFrom(instance, responce.Body.ToByteArray(), 0, responce.Body.Length);
            }
            catch (Exception e)
            {
                // 出现任何消息解析异常都要断开Session，防止客户端伪造消息
                Debug.Fail($"package error; opcode: {(ushort)responce.FunctionID} {connection_.netWork.Count} {e} ");
                connection_.netWork.Remove(connection_);
                return;
            }

            connection_.netWork.MessageDispatcher
                .Dispatch(connection_, (uint)OpCode, message_data);
        }

        public override void StreamWright(Head request, Connection connection)
        {
            // 流正常的写，建立对应的流
            request.StreamID = stream_id_;
            connection_ = connection;
            connection.FlushMessageAsync(request);
        }
    }
}
