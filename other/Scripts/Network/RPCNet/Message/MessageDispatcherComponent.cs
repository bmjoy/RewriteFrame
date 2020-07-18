using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Crucis.Protocol
{
	/// <summary>
	/// 消息分发组件
	/// </summary>
	public class MessageDispatcherComponent
	{
		private readonly Dictionary<uint, List<IMHandler>> handlers = new Dictionary<uint, List<IMHandler>>();

        private NetWork netWork { get; set; }

		public MessageDispatcherComponent(NetWork netWorkp0)
		{
            netWork = netWorkp0;
            this.Load();
		}

		public void Load()
		{
			this.handlers.Clear();

			List<Type> types = NetWork.AttributeSystem.GetTypes(typeof(MessageHandlerAttribute));

			foreach (Type type in types)
			{
				object[] attrs = type.GetCustomAttributes(typeof(MessageHandlerAttribute), false);
				if (attrs.Length == 0)
				{
					continue;
				}

				IMHandler iMHandler = Activator.CreateInstance(type) as IMHandler;
				if (iMHandler == null)
				{
                    Debug.Fail($"message handle {type.Name} 需要继承 IMHandler");
					continue;
				}

				Type messageType = iMHandler.GetMessageType();
                

                uint opcode = netWork.opcodeTypeComponent.GetOpcode(messageType);
				if (opcode == 0)
				{
                    Debug.Fail($"消息opcode为0: {messageType.Name}");
					continue;
				}
				this.RegisterHandler(opcode, iMHandler);
			}
		}

		public void RegisterHandler(uint opcode, IMHandler handler)
		{
			if (!this.handlers.ContainsKey(opcode))
			{
				this.handlers.Add(opcode, new List<IMHandler>());
			}
			this.handlers[opcode].Add(handler);
		}

		public void Handle(Connection session, MessageInfo messageInfo)
		{
			List<IMHandler> actions;
			if (!this.handlers.TryGetValue(messageInfo.Opcode, out actions))
			{
                Debug.Fail($"消息没有处理: {messageInfo.Opcode}");
				return;
			}
			
			foreach (IMHandler ev in actions)
			{
				try
				{
					ev.Handle(session, messageInfo.Message);
				}
				catch (Exception e)
				{
                    Debug.Fail(e.ToString());
				}
			}
		}
	}
}