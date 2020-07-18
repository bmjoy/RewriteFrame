/*===============================
 * Author: [Allen]
 * Purpose: 散弹枪，目标区域
 * Time: 2020/3/6 11:21:35
================================*/
using EditorExtend;
using Leyoutech.Utility;
using System.Collections.Generic;
using UnityEngine;
using Target = CCrossSightLoic.Target;


public partial class WeaponAndCrossSight_ShotGun : WeaponAndCrossSight
{
    /// <summary>
    /// 区域
    /// </summary>
    public class AimArea
    {
        /// <summary>
        ///中心区域 false,边上子区域  true
        /// </summary>
        public bool IsSon = false;

       /// <summary>
       /// 锥体中轴线向量
       /// </summary>
        private Vector3 m_CentralRay;

        #region ========-以下变量都是为了获取中轴线==========
        //中轴线，跟主摄像机正方向，偏移最大角度
        private float m_MaxSpreadAngle=0;

        /// <summary>
        /// 当前中轴线跟摄像机正方向的偏移角度
        /// </summary>
        private float m_SpreadAngle=0;

        /// <summary>
        /// 中轴线，在摄像机椎体圆面，等分角度
        /// </summary>
        private float m_RollAngle=0;

        /// <summary>
        /// 总共有多少个子区域
        /// </summary>
        private int m_SonCount = 0;

        #endregion === end===



        #region ========-计算得到区域内检测射线变量==========
        /// <summary>
        /// 锥体检测范围角度
        /// </summary>
        private float m_CoverAngle=0;

        /// <summary>
        /// 检测目标所用的射线数量
        /// </summary>
        private float m_RayCount=0;

        #endregion === end===


        /// <summary>
        /// 检测逻辑集合容器
        /// </summary>
        private List<CCrossSightLoic> m_CCrossSightLoicList = new List<CCrossSightLoic>();

        /// <summary>
        /// 准星目标列表
        /// </summary>
        protected List<Target> m_Targets = new List<Target>();


        private List<Vector3> reyList = new List<Vector3>();

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="coverAngle">锥体检测范围角度</param>
        /// <param name="rayCount">检测目标所用的射线数量</param>
        /// <param name="skillMaxDistance">技能的最大检测距离</param>
        public void Init(bool isSon, float maxSpreadAngle, float coverAngle, float rayCount, float skillMaxDistance,int SonCount)
        {
            IsSon = isSon;
            m_MaxSpreadAngle = maxSpreadAngle;
            m_CoverAngle = coverAngle;
            m_RayCount = rayCount;
            m_SonCount = SonCount;

            m_CCrossSightLoicList.Clear();
            for (int i = 0; i < m_RayCount; i++)
            {
                CrossSightInfo crossSightInfo = new CrossSightInfo();
                crossSightInfo.m_MaxRayDistance = skillMaxDistance;
                crossSightInfo.m_CrossSightShape = CrossSightShape.CrossLine;
                crossSightInfo.m_LineInfo.detectionMode = DetectionMode.All;
                CCrossSightLoic crossSightLoic = new CCrossSightLoic();
                crossSightLoic = new CrossSightLoic_Improve_line(crossSightLoic);
                crossSightLoic.SetCrossSightInfo(crossSightInfo);
                m_CCrossSightLoicList.Add(crossSightLoic);
            }
        }

        /// <summary>
        /// 获取覆盖角度
        /// </summary>
        /// <returns></returns>
        public float GetCoverAngle()
        {
            return m_CoverAngle;
        }

        /// <summary>
        /// 计算中轴线，跟 区域检测的射线向量，并给准星检测逻辑设置检测基准方向
        /// </summary>
        public void CalculateTheCentralRay(Vector3 camerForward,int index, float rollAngleOffset)
        {
            reyList.Clear();
            //开枪前计算中轴线，此时当前中轴线偏移度设置到最大偏移度上
            m_SpreadAngle = m_MaxSpreadAngle;

            if(!IsSon)
            {
                m_CentralRay = camerForward;
            }
            else
            {
                m_RollAngle = m_SonCount == 0 ? 0 : (360 / m_SonCount * index + rollAngleOffset) % 360;
                m_CentralRay = MathUtility.PitchAndRoll(camerForward, m_SpreadAngle / 2, m_RollAngle);
            }


            //射线计算
            float rayRollAngleOffset = Random.Range(0, 360);

            /// 每条射线的pitch角为最大spread角的 1/4 - 3/4
            /// 每条射线的roll角为 随机偏移角度 + 360 / 射线数量 * 当前射线索引
            for (int i = 0; i < m_RayCount; i++)
            {
                float raySpreadAngle = Random.Range(0.25f, 0.75f) * m_CoverAngle / 2;
                float rayRollAngle = (360 / m_RayCount * i + rayRollAngleOffset) % 360;

                Vector3 rayDirection = MathUtility.PitchAndRoll(m_CentralRay, raySpreadAngle, rayRollAngle);

                //检测逻辑设置检测方向
                CCrossSightLoic crossSightLoic = m_CCrossSightLoicList[i];
                crossSightLoic.SetCheckDirection(rayDirection);
                reyList.Add(rayDirection);
            }
        }

        /// <summary>
        /// 锥体中轴线向量
        /// </summary>
        /// <returns></returns>
        public Vector3 GetCentralRay()
        {
            return m_CentralRay;
        }

        /// <summary>
        /// 释放数据
        /// </summary>
        public void OnRelease()
        {
            for (int i = 0; i < m_CCrossSightLoicList.Count; i++)
            {
                CCrossSightLoic crossSightLoic = m_CCrossSightLoicList[i];
                if(crossSightLoic != null)
                    crossSightLoic.Release();
            }
            m_Targets.Clear();
        }

        //获取目标列表
        public  List<Target> GetTargets()
        {
            m_Targets.Clear();

            for (int i = 0; i < m_CCrossSightLoicList.Count; i++)
            {
                CCrossSightLoic crossSightLoic = m_CCrossSightLoicList[i];
                if (crossSightLoic != null)
                    m_Targets.AddRange(crossSightLoic.GetTarget());
            }
            return m_Targets;
        }

        public void DrawRay()
        {
            Vector3 pos = CameraManager.GetInstance().GetMainCamereComponent().GetPosition();

            for (int i = 0; i < reyList.Count; i++)
            {
                Vector3 hitpos = pos + reyList[i] * 10;
                GizmosHelper.GetInstance().DrawLine(pos, hitpos, Color.gray);
            }
        }
    }
}
