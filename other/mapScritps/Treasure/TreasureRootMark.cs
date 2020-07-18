#if UNITY_EDITOR
using EditorExtend;
using Leyoutech.Utility;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Map
{
    /// <summary>
    /// 在Map层中标记属于寻宝信息 无需导出
    /// </summary>
    [ExecuteInEditMode]
    public class TreasureRootMark : IRootMark
    {
        //[ReadOnly]
        //public ulong m_AreaId;
        //[ReadOnly]
        //public Vector3 m_RelativeAreaPos;
        //[ReadOnly]
        //public Vector3 m_RelativeAreaScale;
        //[ReadOnly]
        //public Quaternion m_RelativeAreaRot;
        public List<SemaphoreMark> m_SemaphorMarkCache = new List<SemaphoreMark>();
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public IEnumerator DoUpdate()
        {
            Transform areaTrans = transform.parent;
            while(areaTrans != null && areaTrans.GetComponent<Area>() == null)
            {
                areaTrans = areaTrans.parent;
            }
            Area area = areaTrans.GetComponent<Area>();
            if(area != null)
            {
                m_AreaId = area.Uid;

                m_RelativeAreaPos = area.transform.InverseTransformPoint(transform.position);
                m_RelativeAreaRot = Quaternion.Inverse(areaTrans.rotation) * transform.rotation;
                m_RelativeAreaScale = ObjectUtility.CalculateLossyScale(transform, area.transform);
                gameObject.name = string.Format("{0}_寻宝节点", areaTrans.name);
            }
            
            m_SemaphorMarkCache.Clear();
            SemaphoreMark[] m_SemaphorMarks = GetComponentsInChildren<SemaphoreMark>();
            if(m_SemaphorMarks != null && m_SemaphorMarks.Length>0)
            {
                m_SemaphorMarkCache.AddRange(m_SemaphorMarks);
                for (int iSeam = 0;iSeam< m_SemaphorMarkCache.Count;iSeam++)
                {
                    SemaphoreMark mark = m_SemaphorMarks[iSeam];
                    if(mark != null && mark.gameObject != null)
                    {
                        mark.DoUpdate();
                    }
                    yield return null;
                }
            }
            yield return null;
        }
    }
}

#endif