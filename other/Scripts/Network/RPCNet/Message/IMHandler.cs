using System;

namespace Crucis.Protocol
{
	public interface IMHandler
	{
		void Handle(Connection session, object message);
		Type GetMessageType();
	}
}