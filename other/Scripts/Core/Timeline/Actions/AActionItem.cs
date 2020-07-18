using Eternity.FlatBuffer;
using FlatBuffers;
using Leyoutech.Core.Context;
using Leyoutech.Core.Pool;
using System;
using UnityEngine.Assertions;

namespace Leyoutech.Core.Timeline
{
    public abstract class AActionItem: IComparable<AActionItem>, IObjectPoolItem
    {
        protected IContext m_Context;
        protected ActionData m_Data;

        public AActionItem()
        {
        }

        public float FireTime { get; set; }

        public virtual void SetEnv(IContext context, ActionData data,float timeScale)
        {
            m_Context = context;
            m_Data = data;
            FireTime = m_Data.FireTime * timeScale;
        }

        public T GetContext<T>() where T:IContext
        {
            return (T)m_Context;
        }

        public ActionData GetData()
        {
            return m_Data;
        }

        public T GetData<T>() where T:struct, IFlatbufferObject
        {
            T? data = m_Data.ActiondataUnion<T>();

            Assert.IsTrue(data.HasValue, $"AActionItem::GetData<{typeof(T).Name}>->data not exist");

            return data.Value;
        }

        public int CompareTo(AActionItem other)
        {
            if (other == null)
                return -1;

            if (FireTime > other.FireTime)
            {
                return 1;
            }
            else if (FireTime < other.FireTime)
            {
                return -1;
            }
            else
            {
                return 0;
            }
        }

        public virtual void OnNew()
        {
        }

        public virtual void OnRelease()
        {
        }
    }
}
