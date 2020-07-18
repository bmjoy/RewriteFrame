using System;
using System.Collections.Generic;

namespace Crucis.Protocol
{
	public class OpcodeTypeComponent
	{
        public static uint InnerOpCode(ushort sreviceid, ushort functionid)
        {
            return (uint)(sreviceid << 16) + functionid;
        }

		private readonly DoubleMap<uint, Type> opcodeTypes = new DoubleMap<uint, Type>();
		
		private readonly Dictionary<uint, object> typeMessages = new Dictionary<uint, object>();

        public OpcodeTypeComponent()
        {
            Load();
        }

        public void Load()
		{
			this.opcodeTypes.Clear();
			this.typeMessages.Clear();
			
			List<Type> types = NetWork.AttributeSystem.GetTypes(typeof(MessageAttribute));
			foreach (Type type in types)
			{
				object[] attrs = type.GetCustomAttributes(typeof(MessageAttribute), false);
				if (attrs.Length == 0)
				{
					continue;
				}
				
				MessageAttribute messageAttribute = attrs[0] as MessageAttribute;
				if (messageAttribute == null)
				{
					continue;
				}

				this.opcodeTypes.Add(InnerOpCode(messageAttribute.ServiceId, messageAttribute.Opcode), type);
				this.typeMessages.Add(InnerOpCode(messageAttribute.ServiceId, messageAttribute.Opcode), Activator.CreateInstance(type));
			}
		}

		public uint GetOpcode(Type type)
		{
			return this.opcodeTypes.GetKeyByValue(type);
		}

		public Type GetType(uint opcode)
		{
			return this.opcodeTypes.GetValueByKey(opcode);
		}
		
		// 客户端为了0GC需要消息池
		public object GetInstance(uint opcode)
		{
			return this.typeMessages[opcode];
		}

	}
}