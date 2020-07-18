namespace Crucis.Protocol
{
	public class MessageAttribute: BaseAttribute
	{
        public ushort ServiceId { get; }

        public ushort Opcode { get; }

		public MessageAttribute(ushort serviceid, ushort opcode)
		{
            this.ServiceId = serviceid;
			this.Opcode = opcode;
		}
	}
}