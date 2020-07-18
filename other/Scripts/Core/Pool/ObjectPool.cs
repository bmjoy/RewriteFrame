using System.Collections;
using System.Collections.Generic;

namespace Leyoutech.Core.Pool
{

    /// <summary>
    /// 对象池
    /// 元素对象必须继承 IObjectPoolItem。初始容量0
    /// </summary>
    public class ObjectPool
    {
        private const string LOG_TAG = "ObjectPool";

        /// <summary>
        /// 栈，保存不活跃元素
        /// </summary>
        private Stack m_Stack = new Stack();

        /// <summary>
        /// 总容量
        /// </summary>
        public int Count { get; private set; }

        /// <summary>
        /// 活跃数量
        /// </summary>
        public int ActiveCount { get { return Count - InactiveCount; } }

        /// <summary>
        /// 不激活数量
        /// </summary>
        public int InactiveCount { get { return m_Stack.Count; } }


        public ObjectPool()
        {
        }

        /// <summary>
        /// 获取一个活跃元素
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Get<T>() where T:IObjectPoolItem,new()
        {
            T element = default;
            if (m_Stack.Count == 0)
            {
                element = new T();
                ++Count;
                element.OnNew();
            }
            else
            {
                element = (T)m_Stack.Pop();
            }
            return element;
        }


        /// <summary>
        /// 回收一个元素，到不激活容器内
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="element"></param>
        public void Release<T>(T element) where T:IObjectPoolItem
        {
            element.OnRelease();

            m_Stack.Push(element);
        }

        /// <summary>
        /// 清除
        /// </summary>
        public void Clear()
        {
            m_Stack.Clear();
            m_Stack = null;
        }
    }

    /// <summary>
    /// 对象池
    /// 元素对象必须继承 IObjectPoolItem。可设置初始容量
    /// </summary>
    /// <typeparam name="T"></typeparam>

    public class ObjectPool<T> where T : class,IObjectPoolItem,new()
    {
        /// <summary>
        /// 栈，保存不活跃元素
        /// </summary>
        private Stack<T> m_Stack = new Stack<T>();

        /// <summary>
        /// 总容量
        /// </summary>
        public int Count { get; private set; }

        /// <summary>
        /// 活跃数量
        /// </summary>
        public int ActiveCount { get { return Count - InactiveCount; } }

        /// <summary>
        /// 不激活数量
        /// </summary>
        public int InactiveCount { get { return m_Stack.Count; } }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="preloadCount">初始容量</param>

        public ObjectPool(int preloadCount=0)
        {
            if(preloadCount>0)
            {
                for (int i = 0; i < preloadCount; i++)
                {
                    T element = new T();
                    element.OnNew();
                    m_Stack.Push(element);

                    ++Count;
                }
            }
        }

        /// <summary>
        /// 获取一个激活元素
        /// </summary>
        /// <returns></returns>
        public T Get()
        {
            T element = default;
            if (m_Stack.Count == 0)
            {
                element = new T();
                ++Count;
                element.OnNew();
            }
            else
            {
                element = m_Stack.Pop();
            }
            return element;
        }


        /// <summary>
        /// 回收一个元素，到不激活容器
        /// </summary>
        /// <param name="element"></param>
        public void Release(T element)
        {
            element.OnRelease();

            m_Stack.Push(element);
        }

        /// <summary>
        /// 清除
        /// </summary>
        public void Clear()
        {
            m_Stack.Clear();
            m_Stack = null;
        }
    }
}
