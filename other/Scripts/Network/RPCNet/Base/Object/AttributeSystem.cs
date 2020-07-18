using System;
using System.Collections.Generic;
using System.Reflection;

namespace Crucis.Protocol
{
    public sealed class AttributeSystem
    {
        private Assembly assembly;
		private readonly UnOrderMultiMap<Type, Type> types = new UnOrderMultiMap<Type, Type>();
        
		public void Add(Assembly assembly)
		{
			this.assembly = assembly;
			this.types.Clear();

            foreach (Type type in assembly.GetTypes())
            {
                object[] objects = type.GetCustomAttributes(typeof(BaseAttribute), false);
                if (objects.Length == 0)
                {
                    continue;
                }

                BaseAttribute baseAttribute = (BaseAttribute)objects[0];
                this.types.Add(baseAttribute.AttributeType, type);
            }

        }

		public Assembly GetAssembly()
		{
			return assembly;
		}
		
		public List<Type> GetTypes(Type systemAttributeType)
		{
			if (!this.types.ContainsKey(systemAttributeType))
			{
				return new List<Type>();
			}
			return this.types[systemAttributeType];
		}

	}
}