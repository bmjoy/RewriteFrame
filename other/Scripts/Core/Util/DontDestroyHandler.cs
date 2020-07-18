using UnityEngine;

namespace Leyoutech.Core.Util
{
    /// <summary>
    /// 辅助类，用于添加不可删除的Mono对象
    /// </summary>
    public static class DontDestroyHandler
    {
        private static readonly string SINGLETON_ROOT_NAME = "Singleton Root";
        private static Transform sm_RootTran = null;
        //根结点的Transform
        private static Transform RootTransform
        {
            get
            {
                if(sm_RootTran == null)
                {
                    CreateRootTransform();
                }

                return sm_RootTran;
            }
        }
        //创建根结点,并标记为DontDestroyOnLoad
        private static void CreateRootTransform()
        {
            GameObject rootGO = GameObject.Find(SINGLETON_ROOT_NAME);
            if (rootGO == null)
            {
                rootGO = new GameObject(SINGLETON_ROOT_NAME);
            }
            Object.DontDestroyOnLoad(rootGO);
            sm_RootTran = rootGO.transform;
        }
        /// <summary>
        /// 根据名称创建一个结点
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static Transform CreateTransform(string name)
        {
            GameObject behGO = new GameObject(name);
            Transform tran = behGO.transform;
            AddTransform(tran);
            return tran;
        }

        /// <summary>
        /// 添加Transfom到Root结点下
        /// </summary>
        /// <param name="tran"></param>
        public static void AddTransform(Transform tran)
        {
            tran.parent = RootTransform;
            tran.localPosition = Vector3.zero;
            tran.localScale = Vector3.one;
            tran.localEulerAngles = Vector3.zero;
        }
        /// <summary>
        /// 根据指定的类型查找或创建结点
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T CreateComponent<T>() where T : MonoBehaviour
        {
            T component = RootTransform.GetComponentInChildren<T>();
            if(component == null)
            {
                Transform tran = CreateTransform(typeof(T).Name);
                component = tran.gameObject.AddComponent<T>();
            }

            return component;
        }
        /// <summary>
        /// 删除根结点
        /// </summary>
        public static void Destroy()
        {
            if(sm_RootTran!=null)
            {
                Object.Destroy(sm_RootTran.gameObject);
            }
            sm_RootTran = null;
        }
    }
}
