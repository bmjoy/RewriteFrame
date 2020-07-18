using System;
using System.Collections.Generic;

namespace Game.Frame.Net
{
	/// <summary>
	/// 每个类型对应一给单例
	/// </summary>
	public class SingleInstanceCache
	{
		private static Dictionary<Type, object> ms_ObjectCache = new Dictionary<Type, object>();

		public static T GetInstanceByType<T>()
		{
			Type objectType = typeof(T);
			if (!ms_ObjectCache.TryGetValue(objectType, out object objectInstance))
			{
				objectInstance = Activator.CreateInstance(objectType);
				ms_ObjectCache.Add(objectType, objectInstance);
			}
			return (T)objectInstance;
		}
	}
}