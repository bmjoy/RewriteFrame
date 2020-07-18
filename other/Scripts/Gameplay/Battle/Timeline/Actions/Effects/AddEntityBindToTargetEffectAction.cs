using Eternity.FlatBuffer;
using Eternity.FlatBuffer.Enums;
using Gameplay.Battle.Timeline.Utils;
using Leyoutech.Core.Timeline;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Battle.Timeline.Actions.Effects
{
    public class AddEntityBindToTargetEffectAction : AEventActionItem
    {
        public override void Trigger()
        {
            AddEntityBindToTargetEffectData data = GetData<AddEntityBindToTargetEffectData>();

            ActionTarget actionTarget = SelectionTargetUtil.GetActionTarget(m_Context, data.TargetSelection.Value);
            if (actionTarget == null || actionTarget.TargetType == ActionTargetType.None)
            {
                Leyoutech.Utility.DebugUtility.LogWarning("AddBindLinkTargetEffectAction", "AddBindLinkTargetEffectAction::Trigger->target not found");
                return;
            }

            bool isMain = data.TargetSelection.Value.Target == TargetType.Self;

            if (actionTarget.TargetType == ActionTargetType.Position)
            {
                EffectActionUtil.CreateEffect(data.Address, actionTarget.TargetPosition, isMain);
            }
            else if(actionTarget.TargetType == ActionTargetType.Entity)
            {
                if(actionTarget.Entity!=null && actionTarget.Entity is SpacecraftEntity shipEntity)
                {
                    List<Transform> bindTransforms = null;
                    if (shipEntity.GetPresentation() == null)
                    {
                        Leyoutech.Utility.DebugUtility.LogWarning("查找挂点", string.Format("bindNodeProperty.GetPresentation() == null ,Entity = {0} ,ItemId = {1} " +
                            "模型未加载完毕，挂点脚本未被赋值!, 此时挂点作 root 处理",
                            shipEntity.EntityId(),
                            shipEntity.GetItemID()));

                        bindTransforms = new List<Transform>();
                        bindTransforms.Add(shipEntity.GetRootTransform());
                    }
                    else
                    {
                      bindTransforms = shipEntity.GetPresentation().GetBindNode(data.NodeType.ToLaunchPoint(), data.NodeIndex);
                    }


                    foreach (var tran in bindTransforms)
                    {
                        EffectActionUtil.CreateEffect(data.Address, tran, isMain);
                    }
                }else
                {
                    Debug.LogError("AddEntityBindToTargetEffectAction::Trigger->Target is not a Ship");
                }
            }
        }
    }
}
