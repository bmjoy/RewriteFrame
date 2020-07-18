using System.Collections.Generic;

namespace Leyoutech.Core.Generic
{
    public interface IORMData<K>
    {
        K GetKey();
    }
    /// <summary>
    /// 简易的数据结构，可以按Index与Key两种方式访问数据
    /// </summary>
    /// <typeparam name="K"></typeparam>
    /// <typeparam name="D"></typeparam>
    public class IndexMapORM<K, D> where D :class, IORMData<K>
    {
        private List<D> m_DataList = new List<D>();
        private Dictionary<K, D> m_DataDic = new Dictionary<K, D>();
        
        public int Count { get => m_DataList.Count; }

        public bool Contain(K key) => m_DataDic.ContainsKey(key);

        /// <summary>
        /// 通过Index访问数据
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public D GetDataByIndex(int index)
        {
            if (index >= 0 && index < m_DataList.Count)
            {
                return m_DataList[index];
            }
            return default;
        }
        /// <summary>
        /// 通过Key访问数据
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public D GetDataByKey(K key)
        {
            if (m_DataDic.TryGetValue(key, out D data))
            {
                return data;
            }
            return default;
        }
        /// <summary>
        /// 添加数据
        /// </summary>
        /// <param name="data"></param>
        public void PushData(D data)
        {
            m_DataList.Add(data);
            m_DataDic.Add(data.GetKey(), data);
        }
        /// <summary>
        /// 从顶部拿出一个数据
        /// </summary>
        /// <returns></returns>
        public D PopData()
        {
            if (m_DataList.Count == 0)
            {
                return default;
            }

            D data = m_DataList[0];
            m_DataList.RemoveAt(0);
            m_DataDic.Remove(data.GetKey());

            return data;
        }
        /// <summary>
        /// 删除指定的数据
        /// </summary>
        /// <param name="data"></param>
        public void DeleteByData(D data)
        {
            m_DataList.Remove(data);
            m_DataDic.Remove(data.GetKey());
        }
        /// <summary>
        /// 根据Key删除数据
        /// </summary>
        /// <param name="key"></param>
        public void DeleteByKey(K key)
        {
            if(m_DataDic.TryGetValue(key,out D data))
            {
                DeleteByData(data);
            }
        }
        /// <summary>
        /// 根据Index删除数据
        /// </summary>
        /// <param name="index"></param>
        public void DeleteByIndex(int index)
        {
            if (index >= 0 && index < m_DataList.Count)
            {
                D data = m_DataList[index];
                DeleteByData(data);
            }
        }

        public void Clear() 
        {
            m_DataList.Clear();
            m_DataDic.Clear();
        }
    }
}
