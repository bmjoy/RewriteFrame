#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Map
{
    [ExecuteInEditMode]
    public class LeapRoot : MapEntityRoot
    {
        /// <summary>
        /// 跃迁id
        /// </summary>
        public ulong m_LeapId;
        /// <summary>
        /// 跃迁名字
        /// </summary>
        public string m_LeapName;
        /// <summary>
        /// 跃迁描述
        /// </summary>
        public string m_LeapDescription;

        /// <summary>
        /// 跃迁点类型
        /// </summary>
        public LeapType m_LeapType;

        /// <summary>
        /// 对应的主跃迁点
        /// </summary>
        public ulong m_MainLeapId;

        /// <summary>
        /// 是否直接可见
        /// </summary>
        public bool m_AutoVisible;

        /// <summary>
        /// 可见列表
        /// </summary>
        public ulong[] m_VisibleLeapList;
        /// <summary>
        /// 跃迁半径
        /// </summary>
        public float m_Range;
        
        /// <summary>
        /// icon资源
        /// </summary>
        //public Sprite m_Icon;
        /// <summary>
        /// 来自于icon配置表
        /// </summary>
        public int m_IconConfId;

        private LeapOverview m_LeapOverView;

        public List<Leap> m_MainLeapCache = new List<Leap>();

        /// <summary>
        /// 跃迁点的碰撞体
        /// </summary>
        public SphereCollider m_Collider;
        /// <summary>
        /// 碰撞盒的偏移大小
        /// </summary>
        public float m_Offest=0.2f;

        public void Init(EditorLeap[] leapList)
        {
            if(leapList == null || leapList.Length<=0)
            {
                return;
            }

            EditorLeap editorLeap = leapList[0];
            m_LeapId = editorLeap.leapId;
            m_LeapName = editorLeap.leapName;
            m_LeapDescription = editorLeap.description;
            m_LeapType = (LeapType)editorLeap.leapType;
            m_MainLeapId = editorLeap.mainLeapId;
            m_AutoVisible = editorLeap.autoVisible == 1 ? true : false;
            m_VisibleLeapList = editorLeap.visibleLeapList;
            m_Range = editorLeap.range;
            m_IconConfId = editorLeap.iconConfId;
            //string[] resAssets = AssetDatabase.FindAssets(string.Format("{0} t:Sprite", editorLeap.iconName));
            //string assetPath = resAssets[0];
            //for (int iRes = 0; iRes < resAssets.Length; iRes++)
            //{
            //    string resPath = AssetDatabase.GUIDToAssetPath(resAssets[iRes]);
            //    string[] resSplit = resPath.Split('/');
            //    if (resSplit != null && resSplit.Length > 0)
            //    {
            //        string lastName = resSplit[resSplit.Length - 1];
            //        string[] lastNameSplit = lastName.Split('_');
            //        if (lastNameSplit != null && lastNameSplit.Length > 0)
            //        {
            //            if (lastNameSplit[lastNameSplit.Length - 1].Equals(string.Format("{0}.png", editorLeap.iconName)))
            //            {
            //                assetPath = resAssets[iRes];
            //                break;
            //            }
            //        }
            //    }
            //}
            //string iconPath = AssetDatabase.GUIDToAssetPath(assetPath);
            //m_Icon = AssetDatabase.LoadAssetAtPath<Sprite>(iconPath);
        }

        public override void BeginExport()
        {
            base.BeginExport();
        }

        public List<Leap> GetLeap()
        {
            if(m_LeapOverView == null)
            {
                return null;
            }
            return m_LeapOverView.m_LeapList;
        }

        public IEnumerator OnUpdate(GamingMapArea mapArea)
        {
            MapLeap[] mapLeaps = FindObjectsOfType<MapLeap>();
            if (mapLeaps != null && mapLeaps.Length>0)
            {
                for(int iMap = 0; iMap < mapLeaps.Length; iMap++)
                {
                    if(mapLeaps[iMap].m_AreaId == mapArea.m_AreaId)
                    {
                        transform.position = mapLeaps[iMap].GetPosition();
                        break;
                    }
                }
            }
            yield return null;
            m_Collider = GetComponent<SphereCollider>();
            if(m_Collider == null)
            {
                m_Collider = gameObject.AddComponent<SphereCollider>();
            }

            m_Collider.radius = 0.5f + m_Offest+ mapArea.GetMaxWarShipHeight()/2;
            m_GamingMapArea = mapArea;
            m_LeapId = mapArea.m_AreaId;
            GamingMap gamingMap = m_GamingMapArea.m_GamingMap;
            m_LeapOverView = gamingMap.m_LeapOverview;
            yield return null;
            RefreshColliderState();
            yield return null;
            transform.localScale = Vector3.one * m_Range;

            //可见跃迁点每帧去算 是因为以防所有GamingMapArea加载进来后，策划手动去改了其他区域的跃迁点信息
            List<ulong> cacheList = gamingMap.m_LeapIdCache;
            cacheList.Clear();
            CalcuateVisibleList(ref cacheList);
            m_VisibleLeapList = cacheList.ToArray();
            yield return null;
        }
        
        /// <summary>
        /// 计算跃迁可见列表
        /// </summary>
        private void CalcuateVisibleList(ref List<ulong> cacheList)
        {
            if (m_LeapOverView == null)
            {
                return;
            }
            List<Leap> leapList = m_LeapOverView.m_LeapList;
            if (leapList == null || leapList.Count <= 0)
            {
                return;
            }

            for (int iLeap = 0; iLeap < leapList.Count; iLeap++)
            {
                Leap leap = leapList[iLeap];
                if (leap.m_LeapId == m_LeapId)
                {
                    continue;
                }
                if (m_LeapType == LeapType.Main)
                {
                    if (leap.m_LeapType == LeapType.Main || leap.m_MainLeapId == m_LeapId)
                    {
                        cacheList.Add(leap.m_LeapId);
                    }

                }
                else if (m_LeapType == LeapType.Child)
                {
                    if (leap.m_LeapType == LeapType.Main || leap.m_MainLeapId == m_MainLeapId)
                    {
                        cacheList.Add(leap.m_LeapId);
                    }
                }
            }
        }


        private void RefreshColliderState()
        {
            if (m_Collider == null || m_LeapType <= 0)
            {
                return;
            }
            m_Collider.enabled = false;
            Collider[] colliders = Physics.OverlapSphere(m_Collider.center, m_Collider.radius);
            if (colliders != null && colliders.Length > 0)
            {
                for (int iCollider = 0; iCollider < colliders.Length; iCollider++)
                {
                    Collider colliderTmp = colliders[iCollider];
                    //Debug.LogError("ActiveScene:"+SceneManager.GetActiveScene().name+"  gameObject scene:"+colliderTmp.gameObject.scene.name);
                    if(m_GamingMapArea != null && m_GamingMapArea.m_GamingMap != null&&
                        colliderTmp.gameObject.scene == m_GamingMapArea.m_GamingMap.GetOwnerScene())
                    {
                        continue;
                    }
                    Debug.LogError(string.Format("跃迁点位置错误:{0}与{1}相交", gameObject.name, EditorGamingMapData.GetParentName(colliders[iCollider].gameObject)));
                }
            }
        }
    }
} 
#endif