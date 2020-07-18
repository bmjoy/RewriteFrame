﻿using Eternity.FlatBuffer;
using Gameplay.Battle.Emit;
using Gameplay.Battle.Skill;
using Gameplay.Battle.Timeline.Utils;
using Leyoutech.Core.Timeline;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Battle.Timeline.Actions.Effects
{
    public class AddEmitLinkTargetEffectAction : AEventActionItem
    {
        public override void Trigger()
        {
            EmitSelectionData emitSelectionData = m_Context.GetObject<SkillRunningData>().EmitSelectoin;
            IBaseActionProperty property = m_Context.GetObject<IBaseActionProperty>();
            IBindNodeActionProperty bindNodeProperty = m_Context.GetObject<IBindNodeActionProperty>();

            AddEmitLinkTargetEffectData data = GetData<AddEmitLinkTargetEffectData>();


            ActionTarget actionTarget = SelectionTargetUtil.GetActionTarget(m_Context, data.TargetSelection.Value);
            if(actionTarget==null || actionTarget.TargetType == ActionTargetType.None)
            {
                Leyoutech.Utility.DebugUtility.LogWarning("AddEmitLinkTargetEffectAction", "AddEmitLinkTargetEffectAction::Trigger->target not found");
                return;
            }

            EmitData[] emitDatas = emitSelectionData.GetEmits(data.EmitIndex);
            if (emitDatas != null && emitDatas.Length > 0)
            {
                for (int i = 0; emitDatas != null && i < emitDatas.Length; i++)
                {
                    EmitData emitData = emitDatas[i];

                    List<Transform> bindTransforms = null;
                    if (bindNodeProperty.GetPresentation() == null)
                    {
                        Leyoutech.Utility.DebugUtility.LogWarning("查找挂点", string.Format("bindNodeProperty.GetPresentation() == null ,Entity = {0} ,ItemId = {1} " +
                            "模型未加载完毕，挂点脚本未被赋值!, 此时挂点作 root 处理",
                            property.EntityId(),
                            property.GetItemID()
                            ));

                        bindTransforms = new List<Transform>();
                        bindTransforms.Add(property.GetRootTransform());
                    }
                    else
                    {
                        bindTransforms = bindNodeProperty.GetPresentation().GetBindNode(emitData.NodeType.ToLaunchPoint(), emitData.NodeIndex);
                    }


                    foreach (var tran in bindTransforms)
                    {
                        if(actionTarget.TargetType == ActionTargetType.Entity)
                        {
                            if (data.TargetSelection.Value.TargetSecondType == Eternity.FlatBuffer.Enums.TargetSecondType.Entity)
                            {
                                EffectActionUtil.CreateEffect(data.Address, tran, property.IsMain(), actionTarget.TargetEntity, Vector3.zero);
                            }
                            else
                            {
                                EffectActionUtil.CreateEffect(data.Address, tran, property.IsMain(), actionTarget.TargetEntity, actionTarget.TargetEntityHitPositionOffet);
                            }
                        }
                        else
                        {
                            EffectActionUtil.CreateEffect(data.Address, tran, property.IsMain(), actionTarget.TargetPosition);
                        }
                    }
                }
            }
        }
    }
}
