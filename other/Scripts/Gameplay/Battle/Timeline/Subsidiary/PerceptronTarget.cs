/*===============================
 * Author: [Allen]
 * Purpose: 感知目标类
 * Time: 2019/12/12 12:09:37
================================*/
using Eternity.FlatBuffer;
using Eternity.FlatBuffer.Enums;
using Google.Protobuf.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using NetHelper = Crucis.Protocol.NetHelper;
using TargetInfo = Crucis.Protocol.TargetInfo;
using Vec3 = Crucis.Protocol.Vec3;

namespace Gameplay.Battle.Timeline
{
    public class PerceptronTarget
    {
        private PlayerSkillProxy m_SkillProxy;
        private CfgEternityProxy m_CfgEternityProxy;
        private GameplayProxy m_GameplayProxy;

        /// <summary>
        /// 所属于的单位
        /// </summary>
        private BaseEntity m_OwnerEntity = null;

        /// <summary>
        /// 目标链表
        /// </summary>
        private LinkedList<CCrossSightLoic.Target> targetsLinkedList = new LinkedList<CCrossSightLoic.Target>();

        /// <summary>
        /// 计算目标列表时的依据方向
        /// </summary>
        private Vector3 m_calculateDirection = Vector3.zero;


        /// <summary>
        /// 队列是否变化了
        /// </summary>
        private bool targetsQueueIsChange = false;


        /// <summary>
        /// 获取当前正在释放中的技能ID
        /// </summary>
        private Func<int> m_GetCurrSkillId;

        /// <summary>
        /// 获取当前释放技能，技能键是否被按下
        /// </summary>
        private Func<bool> m_GetSkillBttonIsDown;

        private Action m_GetChangeGuideSkillTargetAction;


#region  第三方用
        /// <summary>
        /// 广播的目标列表
        /// </summary>
        private List<ulong> m_BroadCastTargets = new List<ulong>();

        /// <summary>
        /// 广播的最远处的方向,上的一个“点”
        /// </summary>
        private Vector3 m_BroadCastDirection;
#endregion




        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="entity">感知所属于的单位</param>
        public PerceptronTarget(BaseEntity entity, Func<int> GetCurrSkillId, Func<bool> GetSkillBttonIsDown, Action GetChangeGuideSkillTargetAction)
        {
            m_OwnerEntity = entity;
            m_GetCurrSkillId = GetCurrSkillId;
            m_GetSkillBttonIsDown = GetSkillBttonIsDown;
            m_GetChangeGuideSkillTargetAction = GetChangeGuideSkillTargetAction;

            m_CfgEternityProxy = GameFacade.Instance.RetrieveProxy(ProxyName.CfgEternityProxy) as CfgEternityProxy;
            m_SkillProxy = GameFacade.Instance.RetrieveProxy(ProxyName.PlayerSkillProxy) as PlayerSkillProxy;
            m_GameplayProxy = GameFacade.Instance.RetrieveProxy(ProxyName.GameplayProxy) as GameplayProxy;
        }


        /// <summary>
        /// 获取当前，武器&准星
        /// </summary>
        /// <returns></returns>
        public WeaponAndCrossSight GetCurrWeaponAndCrossSight()
        {
            return m_SkillProxy.GetCurrentWeaponAndCrossSight();
        }


        /// <summary>
        /// 获取当前感知到的所有单位
        /// </summary>
        /// <param name="direction"> 返回目标计算方式的，依据方向</param> //准星镜头正方向（主相机正方向）or 抖动的随机方向
        /// <returns></returns>
        private List<CCrossSightLoic.Target> GetCurrentEntity(out Vector3 direction, SkillTagercalculationType TagercalculationType = SkillTagercalculationType.CrossSightLens)
        {
            List<CCrossSightLoic.Target> result = new List<CCrossSightLoic.Target>();
            direction = CameraManager.GetInstance().GetMainCamereComponent().GetForward();

            if (m_OwnerEntity.IsMain())
            {
                //武器&准星
                WeaponAndCrossSight m_weaponCS = GetCurrWeaponAndCrossSight();

                if (m_weaponCS != null)
                {
                    List<CCrossSightLoic.Target> targets = null;
                    if (TagercalculationType == SkillTagercalculationType.CrossSightLens) //准星镜头内的目标
                    {
                        targets = m_weaponCS.GetTargets();
                    }
                    else if (TagercalculationType == SkillTagercalculationType.Formula) //公式计算的目标，比如抖动
                    {
                        m_weaponCS.GetBallisticTargetAndOutDirection(out targets, out direction, null);
                    }

                    if(targets !=null)
                        result.AddRange(targets);
                }
            }
            else
            {

                for (int i = 0;  i < m_BroadCastTargets.Count; i++)
                {
                    BaseEntity entity = GameplayManager.Instance.GetEntityManager().GetEntityById<BaseEntity>((uint)m_BroadCastTargets[i]);
                    if (entity == null)
                        continue;

                    CCrossSightLoic.Target cct = new CCrossSightLoic.Target();
                    cct.target_entityId = m_BroadCastTargets[i];
                    cct.target_pos = entity.GetRootTransform().position;
                    result.Add(cct);
                }


                Vector3 worldPosition = m_GameplayProxy.ServerAreaOffsetToClientPosition(m_BroadCastDirection); //服务器坐标，转 Unity 坐标
                direction = (worldPosition - m_OwnerEntity.GetRootTransform().position).normalized;

                //Leyoutech.Utility.DebugUtility.LogError("第三方最远点方向", string.Format("计算方向 -----> ,self = {0} , m_BroadCastDirection = {1}, worldPosition = {2} , PlayPos = {3}  , direction = {4}",
                //   m_OwnerEntity.EntityId(),
                //   m_BroadCastDirection,
                //   worldPosition,
                //   m_OwnerEntity.GetRootTransform().position,
                //   direction
                //   ));
            }

            // Leyoutech.Utility.DebugUtility.LogWarning("目标队列", "准星 List: " + string.Join(",", result));
            return result;
        }

        /// <summary>
        /// 获取当前目标队列
        /// </summary>
        /// <param name="direction"> 返回目标计算方式的，依据方向</param> //准星镜头正方向（主相机正方向）or 抖动的随机方向
        /// <param name="isNew"></param>
        /// <returns></returns>
        public LinkedList<CCrossSightLoic.Target> GetCurrQueue(out Vector3 direction, bool isNew = true, SkillTagercalculationType TagercalculationType = SkillTagercalculationType.CrossSightLens)
        {
            if (isNew)
            {
                List<CCrossSightLoic.Target> lists = GetCurrentEntity(out direction, TagercalculationType);
                m_calculateDirection = direction;


                List<ulong> bLists = new List<ulong>();
                List<Vector3> vLists = new List<Vector3>();

                for (int i = 0; i < lists.Count; i++)
                {
                    bLists.Add(lists[i].target_entityId);
                    vLists.Add(lists[i].target_pos);
                }


                bool bclear = false;//目标变了
                bool vclear = false; //接触点变化
                LinkedListNode<CCrossSightLoic.Target> pVertor = targetsLinkedList.First;
                CCrossSightLoic.Target pb = null;
                while (pVertor != null)
                {
                    pb = pVertor.Value;
                    pVertor = pVertor.Next;
                    if (!bLists.Contains(pb.target_entityId))
                    {
                       // Leyoutech.Utility.DebugUtility.LogWarning("目标队列", "=============1======获取当前目标队列 不同了=================");
                        bclear = true;
                        break;
                    }

                    if (vclear)
                        continue;

                    if (!vLists.Contains(pb.target_pos))
                        vclear = true;                   
                }

                bclear = bclear || (targetsLinkedList.Count != bLists.Count);

                if (bclear || vclear)
                {
                    targetsLinkedList.Clear();
                    for (int i = 0; i < lists.Count; i++)
                    {
                        targetsLinkedList.AddLast(lists[i]);
                    }

                    if (bclear)
                    {
                        targetsQueueIsChange = true;
                       // Leyoutech.Utility.DebugUtility.LogWarning("目标队列", "=============444======获取当前目标队列 不同了================targetsQueueIsChange=" + targetsQueueIsChange);
                    }
                    else
                    {
                        targetsQueueIsChange = false;
                    }
                }
            }

            direction = m_calculateDirection;
            return targetsLinkedList;
        }

        /// <summary>
        /// 获取当前目标列表数量
        /// </summary>
        /// <returns></returns>
        public int GetCurrentTargetsCount()
        {
            return targetsLinkedList.Count;
        }


        /// <summary>
        /// 获取当前感知到的目标的 TargetInfo信息
        /// </summary>
        /// <param name="superposition">给服务的目标，同单位是否叠加</param>
        /// <param name="direction"> 返回目标计算方式的，依据方向</param> //准星镜头正方向（主相机正方向）or 抖动的随机方向
        /// <param name="isNew">是否需要全新的列表</param> 引导技能由于updata 内重新获取了，这里isNew给 false
        /// <param name="TagercalculationType">准星框or 公式另外计算</param>
        /// <returns></returns>

        public List<Crucis.Protocol.TargetInfo> GetCurrentTargetInfos(bool superposition,
            out Vector3 direction,
            bool isNew = true,
            SkillTagercalculationType TagercalculationType = SkillTagercalculationType.CrossSightLens)
        {

            List<Crucis.Protocol.TargetInfo> targets = new List<Crucis.Protocol.TargetInfo>();
            LinkedList<CCrossSightLoic.Target> entitys = GetCurrQueue(out direction, isNew, TagercalculationType);

            Dictionary<uint, uint> targetDic = new Dictionary<uint, uint>();

            LinkedListNode<CCrossSightLoic.Target> pVertor = entitys.First;
            ulong pb ;


            while (pVertor != null)
            {
                pb = pVertor.Value.target_entityId;
                pVertor = pVertor.Next;

                BaseEntity entity = GameplayManager.Instance.GetEntityManager().GetEntityById<BaseEntity>((uint)pb);
                if (entity == null)
                    continue;

                if (!targetDic.ContainsKey(entity.UId()))
                {
                    targetDic.Add(entity.UId(), 1);
                }
                else
                {
                    targetDic[entity.UId()] += 1;
                }
            }

            foreach (var item in targetDic)
            {
                if (superposition) //数量叠加
                {
                    Crucis.Protocol.TargetInfo targetInfo = new Crucis.Protocol.TargetInfo();
                    targetInfo.TargetId = item.Key;
                    targetInfo.Count = item.Value;
                    targets.Add(targetInfo);
                }
                else
                {//数量不叠加
                    for (int i = 0; i < item.Value; i++)
                    {
                        Crucis.Protocol.TargetInfo targetInfo = new Crucis.Protocol.TargetInfo();
                        targetInfo.TargetId = item.Key;
                        targetInfo.Count = 1;
                        targets.Add(targetInfo);
                    }
                }
            }

            return targets;
        }

        public void ClearSkilltargetEntitys()
        {
            targetsLinkedList.Clear();
            m_BroadCastTargets.Clear();
        }

        /// <summary>
        /// 清除
        /// </summary>
        public void Clear()
        {
            ClearSkilltargetEntitys();
            m_OwnerEntity = null;
        }


        public void OnUpdata()
        {
            if (!m_OwnerEntity.IsMain())
                return;         


            int currSkillID = -1;
            if (m_GetCurrSkillId != null)
            {
                currSkillID = m_GetCurrSkillId.Invoke();
                if (currSkillID <= 0)
                    return;

                SkillData skillData = m_CfgEternityProxy.GetSkillData(currSkillID);
                if (!skillData.BaseData.Value.CanChangeTarget)
                    return;

                if (m_GetSkillBttonIsDown == null || !m_GetSkillBttonIsDown.Invoke())
                    return;

                GetCurrQueue(out m_calculateDirection, true, skillData.BaseData.Value.TagercalculationType);

                if (targetsQueueIsChange)
                {
                    //通知服务器目标更换了
                    m_GetChangeGuideSkillTargetAction.Invoke();
                    targetsQueueIsChange = false;
                }
            }
        }


        /// <summary>
        /// 后移entity 
        /// </summary>
        /// <param name="entity"></param>
        public void RetropositionEntity(CCrossSightLoic.Target target)
        {
            if (target == null)
                return;
            if (targetsLinkedList.Contains(target))
            {
                targetsLinkedList.Remove(target);
                targetsLinkedList.AddLast(target);
            }
        }

        /// <summary>
        /// 设置广播的目标列表，跟最远处方向
        /// </summary>
        /// <param name="target_list">目标列表</param>
        /// <param name="direction">最远处的方向</param>
        /// <param name="isbegin">是否释放技能那一时刻的</param>

        public void SetBroadCastTargets(RepeatedField<Crucis.Protocol.TargetInfo> target_list, Vec3 direction,bool isbegin)
        {
            m_BroadCastDirection = NetHelper.Vec3ToPxVec3(direction);

            List<ulong> targets = new List<ulong>();
            for (int i = 0; i < target_list.Count; i++)
            {
              Crucis.Protocol.TargetInfo targetInfo = target_list[i];
                for (int t = 0; t < targetInfo.Count; t++)
                {
                    targets.Add(targetInfo.TargetId);
                }
            }

            m_BroadCastTargets.Clear();
            m_BroadCastTargets.AddRange(targets);

            //技能释放那一时刻的，设置一下新目标列表
            if (isbegin)
            {
                targetsLinkedList.Clear();
                for (int i = 0; i < m_BroadCastTargets.Count; i++)
                {
                    BaseEntity entity = GameplayManager.Instance.GetEntityManager().GetEntityById<BaseEntity>((uint)m_BroadCastTargets[i]);
                    if (entity == null)
                        continue;

                    CCrossSightLoic.Target cctarget = new CCrossSightLoic.Target();
                    cctarget.target_entityId = m_BroadCastTargets[i];
                    cctarget.target_pos = entity.GetRootTransform().position;
                    targetsLinkedList.AddLast(cctarget);
                }

                Vector3 worldPosition = m_GameplayProxy.ServerAreaOffsetToClientPosition(m_BroadCastDirection); //服务器坐标，转 Unity 坐标
                m_calculateDirection = (worldPosition - m_OwnerEntity.GetRootTransform().position).normalized;
            }
        }


        ///// <summary>
        ///// 执行筛选器
        ///// </summary>
        ///// <param name="SelectionData"></param>
        ///// <param name="isNew"></param>
        ///// <returns></returns>
        //private LinkedList<BaseEntity> ToDoSkillTargetFilter(SkillSelectionData SelectionData, bool isNew = true)
        //{
        //    List<BaseEntity> lists = GetCurrentEntity();
        //    LinkedList<BaseEntity> baseLinkedList = new LinkedList<BaseEntity>();
        //    for (int i = 0; i < lists.Count; i++)
        //    {
        //        baseLinkedList.AddLast(lists[i]);
        //    }
        //    SkillTargetFilter skillTargetFilter = new SkillTargetFilter();
        //    skillTargetFilter.SetTargetEntitys(m_OwnerEntity, baseLinkedList);

        //    //阵营摘选
        //    FactionTargetFilter factionTargetFilter = new FactionTargetFilter(skillTargetFilter, SelectionData.FactionType);
        //    return factionTargetFilter.GetTargetEntitys();
        //}
    }
}
