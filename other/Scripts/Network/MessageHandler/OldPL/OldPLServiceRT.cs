using pb = global::Google.Protobuf;

namespace Crucis.Protocol
{
    [Message(OldPLServiceOpCode.OldPLServiceId, OldPLServiceOpCode.OldPl)]
    public partial class OldPl : IMessage { }
}

namespace Crucis.Protocol
{
    public static partial class OldPLServiceOpCode
    {
        // service id
        public const ushort OldPLServiceId = 0;
        // opcode
        public const ushort OldPl = 1;
    }
}

namespace Crucis.Protocol
{
    public class OldPLService : BaseService
    {
        public static OldPLService Service;

        public OldPLService(string uri)
        {
            uri_ = uri;
        }

        // RPC无返回值-模板
        public void OldPl_SendAsync(pb::ByteString body_)
        {
            OldPl body = new OldPl() { Body = body_ };
            NetWork.Instance?.OldPLSend(uri_, OldPLServiceOpCode.OldPLServiceId, OldPLServiceOpCode.OldPl, body);
        }


    }
}