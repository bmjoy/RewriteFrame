namespace Crucis.Protocol
{
	public class OuterMessageDispatcher: IMessageDispatcher
	{
        public void Dispatch(Connection session, uint opcode, object message)
		{
			// 普通消息或者是Rpc请求消息
			MessageInfo messageInfo = new MessageInfo(opcode, message);
            NetWork.messageDispatcherComponent.Handle(session, messageInfo);
        }
	}
}
