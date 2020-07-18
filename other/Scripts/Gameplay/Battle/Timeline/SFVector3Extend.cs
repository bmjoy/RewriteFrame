using Eternity.FlatBuffer;
using System;
using System.Collections.Generic;

namespace Leyoutech.Core.Timeline
{
    public static class SkillExtend
    {
        /// <summary>
        /// 转换成untiy Vector3
        /// </summary>
        /// <param name="sfvector3"></param>
        /// <returns></returns>
        public static UnityEngine.Vector3 ToVector3(this SFVector3 sfvector3)
        {
            return new UnityEngine.Vector3(sfvector3.X , sfvector3.Y , sfvector3.Z);
        }


        /// <summary>
        /// 链表，获取从第几个，到第几个元素
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="linkedList"></param>
        /// <param name="begin"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public static LinkedList<T> GetRange<T>(this LinkedList<T> linkedList , int begin, int end)
        {
            LinkedList<T> result = new LinkedList<T>();
            if (linkedList == null)
                return result;
            if (begin < 0 || end < 0 || begin >= linkedList.Count)
                return result;

            if(begin <= end)
            {
                LinkedListNode<T> pVertor = linkedList.First;
                T pb ;

                int index = 0;
                bool save = false;

                while (pVertor != null)
                {
                    pb = pVertor.Value;
                    pVertor = pVertor.Next;

                    if (index >= begin && index <= end)
                        save = true;
                    else
                        save = false;

                    if (save)
                        result.AddLast(pb);
                    index++;
                }
            }
            return result;
        }


        /// <summary>
        /// 链表追加到后面
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="linkedList"></param>
        /// <param name="sonList"></param>
        /// <returns></returns>
        public static LinkedList<T> AddRange<T>(this LinkedList<T> linkedList, LinkedList<T> sonList)
        {
            if(linkedList == null)
                linkedList = new  LinkedList<T>();
            if(sonList != null && sonList.Count > 0)
            {
                LinkedListNode<T> pVertor = sonList.First;
                T pb;


                while (pVertor != null)
                {
                    pb = pVertor.Value;
                    pVertor = pVertor.Next;
                    linkedList.AddLast(pb);
                }
            }
            return linkedList;
        }

        /// <summary>
        /// 移除所有满足条件的元素
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="linkedList"></param>
        /// <param name="func"></param>
        /// <returns></returns>

        public static LinkedList<T> RemoveAll<T>(this LinkedList<T> linkedList, Func<T ,bool > func )
        {
            LinkedListNode<T> pVertor = linkedList.First;
            T pb;
            while (pVertor != null)
            {
                pb = pVertor.Value;
                pVertor = pVertor.Next;

                bool re = func.Invoke(pb);
                if(re)
                    linkedList.Remove(pb);
            }
            return linkedList;
        }
    }
}
