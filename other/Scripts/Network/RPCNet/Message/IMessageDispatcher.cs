namespace Crucis.Protocol
{
	public interface IMessageDispatcher
	{
		void Dispatch(Connection session, uint opcode, object message);
	}
}
