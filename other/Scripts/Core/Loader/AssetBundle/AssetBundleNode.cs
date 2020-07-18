using Leyoutech.Core.Pool;
using System;
using System.Collections.Generic;
using UnityEngine;
using SystemObject = System.Object;
using UnityObject = UnityEngine.Object;

namespace Leyoutech.Core.Loader
{
    /// <summary>
    /// 主资源节点
    /// 从AB中Get到的资源，将会存储在此Node中
    /// </summary>
    public class AssetNode : IObjectPoolItem
    {
        private string m_AssetPath = null;                                                                                                //源路径
        private BundleNode m_BundleNode = null;                                                                               //主资源对应 BundleNode
        private List<WeakReference> m_WeakAssets = new List<WeakReference>();                     //弱引用列表，通过弱引用的方式用于侦测资源是否被释放

        private int m_LoadCount = 0;                                                                                                          //被引用的数量，由于同一个AB中可以存储多个资源，此值表示正在加载的资源列表中对此资源的引用次数
        public void RetainLoadCount() => ++m_LoadCount;                                                                   //引用的数量 +1
        public void ReleaseLoadCount() => --m_LoadCount;                                                                  //引用的数量 -1

        /// <summary>
        /// 构造函数
        /// </summary>
        public AssetNode() { }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <param name="node">主BundleNode</param>
        public void InitNode(string path,BundleNode node)
        {
            m_AssetPath = path;
            m_BundleNode = node;
            m_BundleNode.RetainRefCount();   //BundleNode 引用数量+1
        }

        /// <summary>
        /// 是否都加载完成
        /// </summary>
        public bool IsDone
        {
            get
            {
                return m_BundleNode.IsDone;
            }
        }

        /// <summary>
        /// 依赖于弱引用，判断资源是否依然存在
        /// </summary>
        /// <returns></returns>
        public bool IsAlive()
        {
            if(!IsDone || m_LoadCount>0 || m_BundleNode.IsScene)
            {
                return true;
            }

            foreach(var weakAsset in m_WeakAssets)
            {
                if (!IsNull(weakAsset.Target))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 从AB中获取指定的资源，如果是场景的话，将会直接返回AB
        /// </summary>
        /// <returns></returns>
        public UnityObject GetAsset()
        {
            UnityObject asset = m_BundleNode.GetAsset(m_AssetPath);
            if(m_BundleNode.IsScene)
            {
                return asset;
            }
            m_WeakAssets.Add(new WeakReference(asset, false));
            return asset;
        }

        /// <summary>
        /// 可以通过此函数得到对对象Instantiate的实例化对象
        /// </summary>
        /// <returns></returns>
        public UnityObject GetInstance()
        {
            UnityObject asset = m_BundleNode.GetAsset(m_AssetPath);
            if(asset ==null)
            {
                return null;
            }
            if (m_BundleNode.IsScene)
            {
                Debug.LogError("AssetNode::GetInstance-> 场景不必执行实例化！");
                return asset;
            }

            UnityObject instance = UnityObject.Instantiate(asset);
            AddInstance(instance);
            return instance;
        }


        /// <summary>
        /// 添加对Instance对象的管理
        /// </summary>
        /// <param name="uObj"></param>
        public void AddInstance(UnityObject uObj)
        {
            //检查是否弱引用列表，是否已经有吧引用释放掉的位置，重新赋值
            bool isSet = false;
            for (int i = 0; i < m_WeakAssets.Count; ++i)
            {
                if (IsNull(m_WeakAssets[i].Target))
                {
                    m_WeakAssets[i].Target = uObj;
                    isSet = true;
                    break;
                }
            }

            //没有则新添一个
            if(!isSet)
            {
                m_WeakAssets.Add(new WeakReference(uObj,false));
            }
        }


        /// <summary>
        /// 判空
        /// </summary>
        /// <param name="sysObj"></param>
        /// <returns></returns>
        private bool IsNull(SystemObject sysObj)
        {
            if (sysObj == null || sysObj.Equals(null))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 新元素被创建时调用
        /// </summary>
        public void OnNew() { }

        /// <summary>
        /// 元素回收时被调用
        /// </summary>
        public void OnRelease()
        {
            m_AssetPath = null;
            m_BundleNode.ReleaseRefCount();
            m_BundleNode = null;
            m_WeakAssets.Clear();
            m_LoadCount = 0;
        }
    }


    /// <summary>
    /// 加载到的AB，将会存储到此数据中
    /// </summary>
    public class BundleNode : IObjectPoolItem
    {
        private string m_BundlePath;                                                                                                  //bundle 路径
        private int m_RefCount;                                                                                                            //被引用数量
        private bool m_IsDone = false;                                                                                                //是否加载完毕                                                                                         
        private bool m_IsSetAssetBundle = false;                                                                              //是否已经AB加载完毕，并赋值了
        private AssetBundle m_AssetBundle = null;                                                                          //AB
        private List<BundleNode> m_DirectDependNodes = new List<BundleNode>();            //关联BundleNode

        /// <summary>
        /// 构造
        /// </summary>
        public BundleNode() { }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="path">bundle 路径</param>
        public void InitNode(string path)
        {
            m_BundlePath = path;
        }


        /// <summary>
        /// 加载完毕，保存AB 
        /// </summary>
        /// <param name="bundle">AB</param>
        public void SetAssetBundle(AssetBundle bundle)
        {
            m_AssetBundle = bundle;
            m_IsSetAssetBundle = true;
        }


        /// <summary>
        /// 保存它的依赖 BundleNode
        /// </summary>
        /// <param name="node"></param>
        public void AddDependNode(BundleNode node)
        {
            m_DirectDependNodes.Add(node);
            node.RetainRefCount();
        }

        /// <summary>
        /// 引用数量
        /// </summary>
        public int RefCount { get => m_RefCount;}

        /// <summary>
        /// 引用计数+1
        /// </summary>
        public void RetainRefCount() => ++m_RefCount;

        /// <summary>
        /// 引用计数-1，并对它关联的AB 一同 -1
        /// </summary>
        public void ReleaseRefCount()
        {
            --m_RefCount;
            if(m_RefCount == 0)
            {
                for(int i =0;i<m_DirectDependNodes.Count;++i)
                {
                    m_DirectDependNodes[i].ReleaseRefCount();
                }
            }
        }

        /// <summary>
        /// 是否加载完毕
        /// </summary>
        public bool IsDone
        {
            get
            {
                if(m_IsDone)
                {
                    return true;
                }

                if (!m_IsSetAssetBundle)
                {
                    return false;
                }
                for (int i = 0; i < m_DirectDependNodes.Count; ++i)
                {
                    if(!m_DirectDependNodes[i].IsDone)
                    {
                        return false;
                    }
                }
                m_IsDone = true;
                return m_IsDone;
            }
        }

        /// <summary>
        /// 是否是 场景 AB 
        /// </summary>
        public bool IsScene
        {
            get
            {
                if(m_AssetBundle!=null)
                {
                    return m_AssetBundle.isStreamedSceneAssetBundle;
                }
                else if(!m_IsSetAssetBundle)
                {
                    Debug.LogError("BundleNode::IsScene->AssetBundle has not been set,you should call IsDone at first");
                }
                else
                {
                    Debug.LogError("BundleNode::IsScene->AssetBundle Load failed");
                }
                
                return false;
            }
        }

        /// <summary>
        /// 从AB中获取资源
        /// </summary>
        /// <param name="assetPath">资源路径</param>
        /// <returns></returns>
        public UnityObject GetAsset(string assetPath)
        {
            return IsScene ? m_AssetBundle : m_AssetBundle?.LoadAsset(assetPath);
        }


        /// <summary>
        /// 新元素被创建时调用
        /// </summary>
        public void OnNew() { }

        /// <summary>
        /// 元素回收时被调用
        /// </summary>
        public void OnRelease()
        {
            m_BundlePath = null;
            m_IsSetAssetBundle = false;
            m_IsDone = false;
            m_AssetBundle?.Unload(true);
            m_AssetBundle = null;
            m_DirectDependNodes.Clear();
            m_RefCount = 0;
        }
    }
}
