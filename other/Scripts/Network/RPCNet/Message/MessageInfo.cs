namespace Crucis.Protocol
{
	public struct MessageInfo
	{
		public uint Opcode { get; }
		public object Message { get; }

		public MessageInfo(uint opcode, object message)
		{
			this.Opcode = opcode;
			this.Message = message;
		}
	}
}
