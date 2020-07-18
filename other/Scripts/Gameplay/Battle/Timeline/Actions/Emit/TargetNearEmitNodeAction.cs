using Eternity.FlatBuffer;
using Gameplay.Battle.Emit;
using Gameplay.Battle.Skill;
using Gameplay.Battle.Timeline.Utils;
using Leyoutech.Core.Timeline;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Battle.Timeline.Actions.Emit
{
    public class BindTransformDistanceData
    {
        public int Index { get; set; }
        public float Distance { get; set; }
    }

    public class TargetNearEmitNodeAction : AEventActionItem
    {
        public override void Trigger()
        {
            EmitSelectionData emitSelectionData = m_Context.GetObject<SkillRunningData>().EmitSelectoin;
            TargetNearEmitNodeData nodeData = GetData<TargetNearEmitNodeData>();
            IBindNodeActionProperty bindNodeProperty = m_Context.GetObject<IBindNodeActionProperty>();
            IBaseActionProperty property = m_Context.GetObject<IBaseActionProperty>();


            List<Transform> bindTransforms = null;
            if (bindNodeProperty.GetPresentation() == null)
            {
                Leyoutech.Utility.DebugUtility.LogWarning("查找挂点", string.Format("bindNodeProperty.GetPresentation() == null ,Entity = {0} ,ItemId = {1} " +
                    "模型未加载完毕，挂点脚本未被赋值!, 此时挂点作 root 处理",
                    property.EntityId(),
                    property.GetItemID()));

                bindTransforms = new List<Transform>();
                bindTransforms.Add(property.GetRootTransform());
            }
            else
            {
              bindTransforms = bindNodeProperty.GetPresentation().GetBindNode(nodeData.NodeType.ToLaunchPoint(), -1);
            }



            ActionTarget actionTarget = SelectionTargetUtil.GetActionTarget(m_Context, nodeData.TargetSelection.Value);
            if (actionTarget == null || actionTarget.TargetType == ActionTargetType.None)
            {
                Leyoutech.Utility.DebugUtility.LogWarning("TargetNearEmitNodeAction", "TargetNearEmitNodeAction::Trigger->target not found");
                return;
            }

            Vector3 targetPosition = Vector3.zero;
            if(actionTarget.TargetType == ActionTargetType.Entity)
            {
                targetPosition = actionTarget.TargetEntity.position;
            }else
            {
                targetPosition = actionTarget.TargetPosition;
            }
            List<BindTransformDistanceData> distances = new List<BindTransformDistanceData>();
            for(int i =0;i<bindTransforms.Count;++i)
            {
                float distance = Vector3.Distance(bindTransforms[i].position, targetPosition);
                distances.Add(new BindTransformDistanceData()
                {
                    Index = i,
                    Distance = distance,
                });
            }

            distances.Sort((item1, item2) =>
            {
                return item1.Distance.CompareTo(item2.Distance);
            });
            int emitDataCount = nodeData.Count;
            if(emitDataCount>distances.Count)
            {
                Leyoutech.Utility.DebugUtility.LogError("Action", "TargetNearEmitNodeAction::Trigger->the number of emit in config is less than the count of the node.");
                emitDataCount = distances.Count;
            }

            EmitData[] emitDatas = new EmitData[emitDataCount];
            for(int i =0;i<nodeData.Count && i< distances.Count;++i)
            {
                emitDatas[i] = new EmitData()
                {
                    NodeType = nodeData.NodeType,
                    NodeIndex = distances[i].Index,
                };
            }

            emitSelectionData.AddOrUpdateEmit(nodeData.AssignIndex,emitDatas);
        }
    }
}
