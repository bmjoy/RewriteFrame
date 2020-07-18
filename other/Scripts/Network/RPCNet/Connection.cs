using DotNetty.Buffers;
using DotNetty.Codecs.Protobuf;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Crucis.Protocol
{
    public class Connection : ChannelHandlerAdapter, IDisposable
    {
        static public readonly SingleThreadEventLoop Group = new SingleThreadEventLoop();

        public long Id { get; private set; }

        public NetWork netWork { get; private set; }

        public string netAddress { get; set; }

        private static uint SequenceId { get; set; }

        private IChannel connect_channel;

        private IChannelHandlerContext channel_handler;

        private BaseStream oldPLStream;

        private Dictionary<uint, BaseStream> Streams = new Dictionary<uint, BaseStream>();

        private object SyncObject => Streams;

        private object SyncMessage => SequenceId;

        // This means that the pool has disposed us, but there may still be
        // requests in flight that will continue to be processed.
        private bool disposed_;

        // This will be set when:
        // (1) We receive GOAWAY -- will be set to the value sent in the GOAWAY frame
        // (2) A connection IO error occurs -- will be set to int.MaxValue
        //     (meaning we must assume all streams have been processed by the server)
        private int last_stream_id_ = -1;

        // This will be set when a connection IO error occurs
        private Exception abort_exception_;

        public Connection(NetWork netWrokparam)
        {
            this.Id = IdGenerater.GenerateInstanceId();
            this.netWork = netWrokparam;

            CreateOldPLStream();
        }

        #region Connect Controll 相关函数
        public async Task<IChannel> ConnectAsync(string address)
        {
            try
            {
                netAddress = address;
                IPEndPoint remote_address = NetworkHelper.ToIPEndPoint(netAddress);
                var bootstrap = new Bootstrap();
				bootstrap
					.Group(Connection.Group)
					.ChannelFactory(() => 
						{
							return new TcpSocketChannel(remote_address.AddressFamily);
						})
                    .Option(ChannelOption.TcpNodelay, true)
                    .Handler(new ActionChannelInitializer<ISocketChannel>(channel =>
                    {
                        IChannelPipeline pipeline = channel.Pipeline;
                        pipeline.AddLast(new ProtobufVarint32FrameDecoder());
                        pipeline.AddLast(new ProtobufVarint32LengthFieldPrepender());
                        pipeline.AddLast(this);
                    }));

                connect_channel = await bootstrap.ConnectAsync(remote_address);
                return connect_channel;
            }
            catch (Exception e)
            {
                Debug.Fail($"connect error: {e.Message}");
                return null;
            }
        }

        public Task CloseAsync()
        {
            return CloseAsync(channel_handler);
        }

        public void Dispose()
        {
            lock (SyncObject)
            {
                if (!disposed_)
                {
                    disposed_ = true;
                    // 删除ClientNode中的Connection
                    CheckForShutdown();
                }
            }
        }

        private void CheckForShutdown()
        {
            Debug.Assert(disposed_ || last_stream_id_ != -1);
            Debug.Assert(Monitor.IsEntered(SyncObject));

            netWork.Remove(this);
            foreach (BaseStream stream in this.Streams.Values.ToArray())
            {
                stream.StreamRead(new Head { Type = ContentType.ErrorType });
            }

            // Do shutdown.
            CloseAsync();
            this.Streams.Clear();
        }

        private RequestException GetShutdownException()
        {
            // Throw a retryable exception that will allow this unprocessed request to be processed on a new connection.
            // In rare cases, such as receiving GOAWAY immediately after connection establishment, we will not
            // actually retry the request, so we must give a useful exception here for these cases.
            Exception innerException;
            if (abort_exception_ != null)
            {
                innerException = abort_exception_;
            }
            else if (last_stream_id_ != -1)
            {
                // We must have received a GOAWAY.
                innerException = new IOException(ErrorCode.ERR_MyErrorCode.ToString());
            }
            else
            {
                // We must either be disposed or out of stream IDs.
                // Note that in this case, the exception should never be visible to the user (it should be retried).
                innerException = new ObjectDisposedException(nameof(Connection));
            }

            return new RequestException(ErrorCode.ERR_MyErrorCode.ToString(), innerException, allowRetry: RequestRetryType.RetryOnSameOrNextProxy);
        }
        #endregion

        #region ChannelHandlerAdapter 相关函数
        public override void ChannelActive(IChannelHandlerContext context)
        {
            channel_handler = context;
        }

        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            try
            {
                if (message is IByteBuffer byteBuffer)
                {
                    var bytes = new byte[byteBuffer.ReadableBytes];
                    byteBuffer.ReadBytes(bytes);
                    Head headPackage = Head.Parser.ParseFrom(bytes);
                    BaseStream stream;

                    if (headPackage.Type == ContentType.OldProto)
                    {
                        stream = oldPLStream;
                    }
                    else if(!this.Streams.TryGetValue(headPackage.StreamID, out stream))
                    {
                        Debug.Fail($"not found stream, Service id: {headPackage.ServiceID}, Function id: {headPackage.FunctionID}");
                        return;
                    }
                    stream.StreamRead(headPackage);
                }
            }
            catch (Exception e)
            {
                Debug.Fail($"read message error: {e.Message}");
            }
        }

        public override void ChannelReadComplete(IChannelHandlerContext context) => context.Flush();

        public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
        {
            abort_exception_ = exception;
            CheckForShutdown();
        }
        #endregion


        #region 消息发送相关函数
        // 有返回值RPC CALL
        public async Task<IMessage> CallAsync(Head request)
        {
            CallRWStream stream = new CallRWStream();
            Streams.Add(stream.StreamId, stream);

            return await stream.StreamWrightAsync(request, this);
        }

        // 有返回值RPC CALL，可以取消
        public async Task<IMessage> CallAsync(Head request, CancellationToken cancellationToken)
        {
            CallRWStream stream = new CallRWStream();
            Streams.Add(stream.StreamId, stream);

            cancellationToken.Register(() => this.Streams.Remove(stream.StreamId));
            Send(request);

            return await stream.StreamWrightAsync(request, this);
        }

        // 无返回值RPC CALL
        public void Send(Head headpackage)
        {
            SendWStream stream = new SendWStream();
            Streams.Add(stream.StreamId, stream);
            stream.StreamWright(headpackage, this);
        }

        // OldPL，建立连接通信
        private void CreateOldPLStream()
        {
            if (oldPLStream != null)
                Streams.Remove(oldPLStream.StreamId);

            oldPLStream = new OldPLStream();
            Streams.Add(oldPLStream.StreamId, oldPLStream);
            oldPLStream.StreamCreate(null, this);
        }

        private void CloseOldPLStream()
        {
            oldPLStream?.StreamRead(new Head { Type = ContentType.CloseStreamType });

            if (oldPLStream != null)
                Streams.Remove(oldPLStream.StreamId);
            oldPLStream = null;
        }

        // OldPL，发送消息
        public void OldPLSend(Head headpackage)
        {
            if(oldPLStream != null)
            {
                oldPLStream.StreamWright(headpackage, this);
            }
        }

        // STREAM流，建立连接通信
        public BaseStream CreateStream(Head create, StreamType st)
        {
            BaseStream stream;
            switch (st)
            {
                case StreamType.Call_RW:
                    stream = new CallRWStream();
                    break;
                case StreamType.Send_W:
                    stream = new SendWStream();
                    break;
                case StreamType.Stream_RW:
                    stream = new StreamReadWrightStream();
                    break;
                case StreamType.Stream_R:
                    stream = new StreamReadStream();
                    break;
                case StreamType.Stream_W:
                    stream = new StreamWrightStream();
                    break;
                default:
                    stream = new OldPLStream();
                    break;
            }
            Streams.Add(stream.StreamId, stream);
            stream.StreamCreate(create, this);

            return stream;
        }

        // STREAM流，关闭连接通信
        public void CloseStream(uint streamid, Head close)
        {
            if (Streams.ContainsKey(streamid))
            {
                // 使用对应的流发送消息
                Streams[streamid].StreamWright(close, this);
                Streams[streamid].StreamClose();
                Streams.Remove(streamid);
            }
        }

        // STREAM流，发送消息
        public void StreamSend(uint streamid, Head headpackage)
        {
            if (Streams.ContainsKey(streamid))
            {
                // 使用对应的流发送消息
                Streams[streamid].StreamWright(headpackage, this);
                return;
            }
            Debug.Fail($"stream message error; opcode: {(ushort)headpackage.FunctionID}");
            throw new Exception();
        }

        // STREAM刷新消息
        public void FlushMessageAsync(Head headpackage)
        {
            // 序列，防止重发
            lock (SyncMessage)
            {
                headpackage.SeqID = ++SequenceId;
                IByteBuffer iniMessage = Unpooled.Buffer(headpackage.CalculateSize());
                iniMessage.WriteBytes(headpackage.ToByteArray());

                try
                {
                    connect_channel.WriteAndFlushAsync(iniMessage);
                }
                catch (Exception e)
                {
                    Debug.Fail($"connect_channel error:{e.Message}");
                }
            }
        }

        public void ClearStream(uint streamid)
        {
            Streams.Remove(streamid);
        }

        #endregion

    }
}
