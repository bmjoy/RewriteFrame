using Leyoutech.Core.Pool;
using System;
using System.Collections.Generic;

namespace Leyoutech.Core.Context
{
    public class ContextObjectData : IObjectPoolItem
    {
        public object Target { get; set; }
        public bool IsNeverClear { get; set; } = false;

        public void OnNew()
        {
        }

        public void OnRelease()
        {
            Target = null;
            IsNeverClear = false;
        }
    }

    public class DefaultContext : IContext
    {
        private ObjectPool<ContextObjectData> dataPool = new ObjectPool<ContextObjectData>();
        private Dictionary<Type, ContextObjectData> m_ObjectDic = new Dictionary<Type, ContextObjectData>();
        public DefaultContext() { }

        /// <summary>
        /// 查找存在
        /// </summary>
        public bool ContainsObject<T>()
        {
            return m_ObjectDic.ContainsKey(typeof(T));
        }


        /// <summary>
        /// 查找存在
        /// </summary>
        public bool ContainsObject(Type type)
        {
            return m_ObjectDic.ContainsKey(type);
        }

        /// <summary>
        /// 获取
        /// </summary>
        public T GetObject<T>()
        {
            object value = GetObject(typeof(T));
            if(value == null)
            {
                return default(T);
            }
            return (T)value;
        }

        /// <summary>
        /// 获取
        /// </summary>
        public object GetObject(Type type)
        {
            if (m_ObjectDic.TryGetValue(type, out ContextObjectData value))
            {
                return value.Target;
            }
            return null;
        }


        /// <summary>
        /// 删除
        /// </summary>
        public void DeleteObject<T>()
        {
            DeleteObject(typeof(T));
        }


        /// <summary>
        /// 删除
        /// </summary>
        public void DeleteObject(Type type)
        {
            if (m_ObjectDic.ContainsKey(type))
            {
                ContextObjectData data = m_ObjectDic[type];
                m_ObjectDic.Remove(type);
                dataPool.Release(data);
            }
        }

        public void AddObject<T>(T obj, bool isNeverClear = false)
        {
            if (obj == null)
                return;
            AddObject(typeof(T), obj, isNeverClear);
        }

        public void AddObject(Type type, object obj, bool isNeverClear = false)
        {
            if (obj == null)
                return;

            if(!m_ObjectDic.TryGetValue(type,out ContextObjectData data))
            {
                data = dataPool.Get();
                m_ObjectDic.Add(type, data);
            }
            data.Target = obj;
            data.IsNeverClear = isNeverClear;
        }

        public void Clear(bool isForce = false)
        {
            List<Type> keys = new List<Type>(m_ObjectDic.Keys);
            foreach(var key in keys)
            {
                if(!isForce && m_ObjectDic[key].IsNeverClear)
                {
                    continue;
                }
                m_ObjectDic.Remove(key);
            }
        }
    }
}
