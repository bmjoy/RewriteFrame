using Assets.Scripts.Lib.Net;
using EditorExtend;
using Eternity.FlatBuffer;
using Eternity.FlatBuffer.Enums;
using Gameplay.Battle.Emit;
using Gameplay.Battle.Skill;
using Gameplay.Battle.Timeline.Utils;
using Leyoutech.Core.Timeline;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine; 

namespace Gameplay.Battle.Timeline.Actions.Flyer
{
    /// <summary>
    /// 指定发射点释放飞行物
    /// </summary>
    public class FlyerEmitAction : AEventActionItem
    {
        public override void Trigger()
        {
            IBaseActionProperty baseActionProperty = m_Context.GetObject<IBaseActionProperty>();
            Leyoutech.Utility.DebugUtility.LogWarning("释放飞行物", string.Format("触发释放飞行物Action，释放单位自己entity = {0} ", baseActionProperty.EntityId()));

            FlyerEmitData flyerEmitData = GetData<FlyerEmitData>();
            SkillData skillData = m_Context.GetObject<SkillData>();
            EmitSelectionData emitSelectionData = m_Context.GetObject<SkillRunningData>().EmitSelectoin;

            EmitData[] emitDatas = emitSelectionData.GetEmits(flyerEmitData.EmitIndex);
            IBindNodeActionProperty bindNodeProperty = m_Context.GetObject<IBindNodeActionProperty>();

            for (int i = 0; emitDatas != null && i < emitDatas.Length; i++)
            {
                EmitData emitData = emitDatas[i];


                List<Transform> bindTransforms = null;
                if (bindNodeProperty.GetPresentation() == null)
                {
                    Leyoutech.Utility.DebugUtility.LogWarning("查找挂点", string.Format("bindNodeProperty.GetPresentation() == null ,Entity = {0} ,ItemId = {1} " +
                        "模型未加载完毕，挂点脚本未被赋值!, 此时挂点作 root 处理",
                        baseActionProperty.EntityId(),
                        baseActionProperty.GetItemID()));

                    bindTransforms = new List<Transform>();
                    bindTransforms.Add(baseActionProperty.GetRootTransform());
                }
                else
                {
                    bindTransforms = bindNodeProperty.GetPresentation().GetBindNode(emitData.NodeType.ToLaunchPoint(), emitData.NodeIndex);
                }


                foreach (var tran in bindTransforms)
                {
                    //发射位置
                    Vector3 flyerPosition = Vector3.zero;
                    if (flyerEmitData.PositionType == FlyerPositionType.Emit) //与发射点的位置重合 
                    {
                        flyerPosition = tran.position;
                    }
                    else if (flyerEmitData.PositionType == FlyerPositionType.EmitOffset)//与发射点位置偏移 
                    {
                        flyerPosition = tran.position + flyerEmitData.PositionOrOffset.Value.ToVector3();
                    }

                    //发射方向
                    Vector3 flyerDirection = Vector3.zero;
                    Quaternion qq = Quaternion.identity;

                    //给子弹的目标信息
                    bool ishaveTarget = false;
                    BaseEntity targetEntity = null;
                    uint targetEnityId = 0;
                    Vector3 targetPoint = Vector3.zero;
                    Vector3 calculateDirection = Vector3.zero; //用于计算没有目标时，求最远点的方向

                    //=========================目标设置==============================

                    ActionTarget actionTarget = SelectionTargetUtil.GetActionTarget(m_Context, flyerEmitData.TargetSelection.Value);
                    if (actionTarget == null || actionTarget.TargetType == ActionTargetType.None)
                    {
                        Leyoutech.Utility.DebugUtility.LogWarning("AddBindLinkTargetEffectAction", "AddBindLinkTargetEffectAction::Trigger->target not found");
                        return;
                    }

                    if(actionTarget.TargetType == ActionTargetType.Entity)
                    {//有单位目标
                        targetEntity = actionTarget.Entity;
                        ishaveTarget = targetEntity != null;
                        if (ishaveTarget)
                            targetEnityId = (uint)targetEntity.EntityId();

                        Leyoutech.Utility.DebugUtility.LogWarning("释放飞行物", string.Format("-----1-----有目标，释放单位自己entity = {0},目标单位 entiy = {1}",
                            baseActionProperty.EntityId(),
                            targetEnityId));
                    }
                    else
                    {//没有单位，点
                        targetPoint = actionTarget.TargetPosition;

                        Leyoutech.Utility.DebugUtility.LogWarning("释放飞行物", string.Format("--------2---没目标，释放单位自己entity = {0},最远点为发射方向  targetPoint = {1}",
                            baseActionProperty.EntityId(),
                            targetPoint));
                    }


                    //======================end===目标设置==============================


                    if (flyerEmitData.DirectionType == FlyerDirectionType.Emit)//与发射点的方向相同 
                    {
                        flyerDirection = Quaternion.FromToRotation(Vector3.forward, tran.up).eulerAngles;
                        qq = Quaternion.Euler(flyerDirection);
                    }
                    else if (flyerEmitData.DirectionType == FlyerDirectionType.Entity)//与实体的方向相同 
                    {
                        flyerDirection = Quaternion.FromToRotation(Vector3.forward, baseActionProperty.GetRootTransform().forward).eulerAngles;
                        qq = Quaternion.Euler(flyerDirection);
                    }
                    else if (flyerEmitData.DirectionType == FlyerDirectionType.Target)///与目标的方向相同 
                    {
                        if (ishaveTarget && targetEntity != null) //有目标，目标单位为发射方向
                        {
                            Vector3 vector = (targetEntity.GetRootTransform().position - tran.position).normalized;
                            flyerDirection = Quaternion.FromToRotation(Vector3.forward, vector).eulerAngles;
                            qq = Quaternion.Euler(flyerDirection);
                        }
                        else
                        {//没目标，最远点为发射方向
                            Vector3 vector = (targetPoint - tran.position).normalized;
                            flyerDirection = Quaternion.FromToRotation(Vector3.forward, vector).eulerAngles;
                            qq = Quaternion.Euler(flyerDirection);

                            GizmosHelper.GetInstance().DrawLine(tran.position, targetPoint, Color.green);
                        }
                    }

                    //创建子弹
                    SpacecraftSkillComponent skillComponent = m_Context.GetObject<SpacecraftSkillComponent>();
                    CreatBulletData creatBulletData = (CreatBulletData)Activator.CreateInstance(typeof(CreatBulletData));
                    creatBulletData.flyerDataID = flyerEmitData.FlyerID;                                       //飞行数据ID
                    creatBulletData.OwnerEntityId = (uint)baseActionProperty.EntityId();        //哪个单位ID
                    creatBulletData.owerSkillId = skillComponent.SkillID;                                    //哪个技能ID

                    //位置
                    creatBulletData.posX = flyerPosition.x;
                    creatBulletData.posY = flyerPosition.y;
                    creatBulletData.posZ = flyerPosition.z;

                    //旋转
                    creatBulletData.rotationX = flyerDirection.x;
                    creatBulletData.rotationY = flyerDirection.y;
                    creatBulletData.rotationZ = flyerDirection.z;

                    //子弹要飞去的目标单位ID
                    creatBulletData.isHaveTarget = ishaveTarget;
                    creatBulletData.targetEntityId = targetEnityId;

                    //子弹的要飞去的目标位置
                    creatBulletData.target_posX = targetPoint.x;
                    creatBulletData.target_posY = targetPoint.y;
                    creatBulletData.target_posZ = targetPoint.z;


                    if (skillData.BaseData.Value.IsNeedTarget && !ishaveTarget)
                    {//需要目标，但目标值为null 则不执行
                        return;
                    }


                    if (flyerPosition == Vector3.zero)
                    {
                        Leyoutech.Utility.DebugUtility.LogWarning("释放飞行物", string.Format("子弹发射初始位置==Vector3.zero 确定正确吗？ SkillID = {0} ", skillData.Id));
                    }

                    Leyoutech.Utility.DebugUtility.LogWarning("释放飞行物", string.Format("创建子弹! ------> 释放单位自己entity = {0}, 子弹位置：{1}  , 朝向：{2} , 有目标ID吗?{3}  ,  目标EntityID: {4},  目标点: {5} ",
                        baseActionProperty.EntityId(),
                        flyerPosition,
                        qq * Vector3.forward,
                        ishaveTarget,
                        targetEnityId,
                        targetPoint));

                    GameplayManager.Instance.GetEntityManager().CreateBulletEntity<BulletEntity, CreatBulletData>(creatBulletData);
                }
            }
        }
    }
}
