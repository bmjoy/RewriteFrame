using System;

namespace Leyoutech.Core.Context
{
    public interface IContext
    {
        /// <summary>
        /// 是否包含
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        bool ContainsObject<T>();
        bool ContainsObject(Type type);

        /// <summary>
        /// 添加
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        void AddObject<T>(T obj,bool isNeverClear = false);
        void AddObject(Type type, object obj,bool isNeverClear = false);
        /// <summary>
        /// 获取
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T GetObject<T>();
        object GetObject(Type type);
        /// <summary>
        /// 删除
        /// </summary>
        /// <typeparam name="T"></typeparam>
        void DeleteObject<T>();
        void DeleteObject(Type type);

        /// <summary>
        /// 清除
        /// </summary>
        void Clear(bool isForce = false);
    }
}
