using Eternity.FlatBuffer;
using Eternity.FlatBuffer.Enums;
using Leyoutech.Core.Context;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Battle.Timeline.Actions
{
    internal enum ActionTargetType
    {
        None = 0,
        Entity ,
        Position,
    }

    internal class ActionTarget
    {
        internal ActionTargetType TargetType { get; set; } = ActionTargetType.None;
        internal Transform TargetEntity { get; set; } = null;
        internal BaseEntity Entity { get; set; } = null;
        internal Vector3 TargetEntityHitPositionOffet { get; set; } =  Vector3.zero;
        internal Vector3 TargetPosition { get; set; } = Vector3.zero;
    }

    internal static class SelectionTargetUtil
    {
        internal static ActionTarget GetActionTarget (IContext context, TargetSelectionData selectionData)
        {
            ActionTarget actionTarget = new ActionTarget();

            IBaseActionProperty baseActionProperty = context.GetObject<IBaseActionProperty>();
            SkillData skillData = context.GetObject<SkillData>();
            PerceptronTarget perceptron = context.GetObject<PerceptronTarget>();

            Vector3 calculateDirection = Vector3.zero;

            LinkedList<CCrossSightLoic.Target> entities = null;
            if (!skillData.BaseData.Value.CanChangeTarget)
            {   //不能切换
                entities = perceptron.GetCurrQueue(out calculateDirection, false, skillData.BaseData.Value.TagercalculationType);
            }
            else
            {
                //能切换
                entities = perceptron.GetCurrQueue(out calculateDirection, true, skillData.BaseData.Value.TagercalculationType);
            }

            //             //阵营筛选，已经在技能释放前过滤过了，下面不进行
            //             if (entities.Count > 0) //有目标，目标单位为发射方向
            //             {
            CCrossSightLoic.Target cctarget = GetTargetEntity(context, entities, baseActionProperty.GetOwner(), selectionData);
            if (cctarget == null)
            {
                //没目标，最远点为发射方向
                GetFarthestPostionActionTarget(ref actionTarget, context, calculateDirection);
            }
            else
            {
                BaseEntity entity = GameplayManager.Instance.GetEntityManager().GetEntityById<BaseEntity>((uint)cctarget.target_entityId);
                if (entity != null)
                {
                    actionTarget.Entity = entity;
                    actionTarget.TargetEntity = entity.GetRootTransform();
                    actionTarget.TargetEntityHitPositionOffet = cctarget.target_pos - entity.GetRootTransform().position;
                    actionTarget.TargetType = ActionTargetType.Entity;
                }
                else
                {
                    //没目标，最远点为发射方向
                    GetFarthestPostionActionTarget(ref actionTarget, context, calculateDirection);
                }
            }
            //             }
            //             else
            //             {//没目标，最远点为发射方向
            // 
            //                 GetFarthestPostionActionTarget(ref  actionTarget,  context,  calculateDirection);
            //             }

            if (actionTarget.TargetType == ActionTargetType.None)
            {
                return null;
            }else
            {
                return actionTarget;
            }
        }

        /// <summary>
        /// 获取最远点
        /// </summary>
        /// <param name="context"></param>
        /// <param name="selectionData"></param>
        /// <returns></returns>
       private static  void GetFarthestPostionActionTarget( ref ActionTarget actionTarget, IContext context, Vector3 calculateDirection )
        {
            IBaseActionProperty baseActionProperty = context.GetObject<IBaseActionProperty>();
            if(baseActionProperty.IsMain())
            {
                SkillData skillData = context.GetObject<SkillData>();
                MainCameraComponent mainCamComponent = context.GetObject<MainCameraComponent>();

                if (mainCamComponent == null)
                {
                    actionTarget = null;
                    return;
                }
                //最远点
                Vector3 distantPoint = Vector3.zero;


                Vector3 playerPosition = baseActionProperty.GetRootTransform().position;   //玩家位置
                Vector3 CamPosition = mainCamComponent.GetPosition(); //摄像机位置

                Vector3 cameDir = calculateDirection;/*mainCamComponent.GetForward()*/   //摄像机方向向量
                Vector3 camera2Player = playerPosition - CamPosition;//摄像机到玩家的向量
                Vector3 verticalPos = CamPosition + Vector3.Dot(camera2Player, cameDir)*cameDir; //垂线坐标 = 摄像机坐标+ camera2Player在cameDir投影距离 * cameDir向量
                Vector3 Play2VerticaN = (verticalPos - playerPosition).normalized; //玩家到垂线点 的单位向量
                float Play2VerticaD = Vector3.Distance(verticalPos, playerPosition);//玩家跟垂线点的距离

                float MaxDis = skillData.BaseData.Value.MaxDistance;
                if (MaxDis > Play2VerticaD)
                {
                    distantPoint = Mathf.Sqrt(MaxDis * MaxDis - Play2VerticaD * Play2VerticaD) * cameDir + verticalPos; //最远点 = 三角函数求得垂线点到最远点向量+ 垂线点坐标
                }
                else
                {
                    distantPoint = playerPosition + Play2VerticaN * MaxDis;//垂线上找到距离是 MaxDis 的坐标
                }
                actionTarget.TargetType = ActionTargetType.Position;
                actionTarget.TargetPosition = distantPoint;
            }
            else
            {
                SkillData skillData = context.GetObject<SkillData>();
                Vector3 playerPosition = baseActionProperty.GetRootTransform().position;
                float d = skillData.BaseData.Value.MaxDistance; 

                Vector3 distantPoint = playerPosition + calculateDirection * d;
                actionTarget.TargetType = ActionTargetType.Position;
                actionTarget.TargetPosition = distantPoint;

                //Leyoutech.Utility.DebugUtility.LogError("第三方最远点方向", string.Format("计算最远点 ---->self = {0} , calculateDirection = {1},  PlayPos = {2}  , distantPoint = {3}",
                //    baseActionProperty.EntityId(),
                //    calculateDirection,
                //    playerPosition,
                //    distantPoint
                //    ));
            }
        }



        private static CCrossSightLoic.Target GetTargetEntity(IContext context,LinkedList<CCrossSightLoic.Target> entities, BaseEntity ower, TargetSelectionData targetSelection)
        {
            if (entities == null )
                return null;

            //阵营过滤
            GameplayProxy gameplayProxy = GameFacade.Instance.RetrieveProxy(ProxyName.GameplayProxy) as GameplayProxy;

            LinkedList<CCrossSightLoic.Target> nLinkedList = new LinkedList<CCrossSightLoic.Target>();

            LinkedListNode<CCrossSightLoic.Target> pVertor = entities.First;
            CCrossSightLoic.Target pb = null;
            while (pVertor != null)
            {
                pb = pVertor.Value;
                pVertor = pVertor.Next;

                BaseEntity entity2 = GameplayManager.Instance.GetEntityManager().GetEntityById<BaseEntity>((uint)pb.target_entityId);
                if (entity2 == null)
                    continue;

                bool same = gameplayProxy.IsBelongToThisFactionType(targetSelection.FactionType, ower, entity2);
                if (same)
                {
                    nLinkedList.AddLast(pb);
                }
            }


            //目标选择
            PerceptronTarget perceptron = context.GetObject<PerceptronTarget>();
            IBaseActionProperty baseActionProperty = context.GetObject<IBaseActionProperty>();

            CCrossSightLoic.Target result = null;
            if (targetSelection.Target == TargetType.Target) //表示当前选中的目标（实体,点）
            {
                result = nLinkedList.First?.Value;
            }
            else if (targetSelection.Target == TargetType.TargetNext) ///选中当前的目标，下次选择选中下一个目标
            {
                result = nLinkedList.First?.Value;
                perceptron.RetropositionEntity(nLinkedList.First?.Value);
            }
            else if (targetSelection.Target == TargetType.Self) /// 1.选择自己   2.如果是飞行物的话 自己表示的飞行物本身
            {
                result = new CCrossSightLoic.Target();
                result.target_entityId = baseActionProperty.GetOwner().EntityId();
                result.target_pos = baseActionProperty.GetOwner().GetRootTransform().position;
            }
            else if (targetSelection.Target == TargetType.Owner) /// 1.选择自己的owner(或者说是caster)  2.如果是飞行物的话还是技能的caster 
            {
                BaseEntity fentity = GameplayManager.Instance.GetEntityManager().GetEntityById<BaseEntity>(baseActionProperty.GetEntityFatherOwnerID());
                if(fentity != null)
                {
                    result = new CCrossSightLoic.Target();
                    result.target_entityId = baseActionProperty.GetEntityFatherOwnerID();
                    result.target_pos = fentity.GetRootTransform().position;
                }
            }

            return result;
        }
    }
}
