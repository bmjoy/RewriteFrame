using System;
using System.Diagnostics;

namespace Crucis.Protocol
{
	public abstract class AMHandler<Message> : IMHandler where Message: class
	{
		protected abstract void Run(Connection session, Message message);

		public void Handle(Connection session, object msg)
		{
			Message message = msg as Message;
			if (message == null)
			{
                Debug.Fail($"消息类型转换错误: {msg.GetType().Name} to {typeof(Message).Name}");
                return;
			}
			this.Run(session, message);
		}

		public Type GetMessageType()
		{
			return typeof(Message);
		}
	}
}