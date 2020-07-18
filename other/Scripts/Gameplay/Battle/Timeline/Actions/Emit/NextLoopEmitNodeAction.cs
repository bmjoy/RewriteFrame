using Eternity.FlatBuffer;
using Gameplay.Battle.Emit;
using Gameplay.Battle.Skill;
using Gameplay.Battle.Timeline.Utils;
using Leyoutech.Core.Timeline;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Battle.Timeline.Actions.Emit
{
    public class NextLoopEmitNodeAction : AEventActionItem
    {
        public override void Trigger()
        {
            EmitSelectionData emitSelectionData = m_Context.GetObject<SkillRunningData>().EmitSelectoin;

            int assignIndex = GetData<NextLoopEmitNodeData>().AssignIndex;
            EmitData emitData = emitSelectionData.GetEmit(assignIndex);
            if(emitData !=null)
            {
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
                    bindTransforms = bindNodeProperty.GetPresentation().GetBindNode(emitData.NodeType.ToLaunchPoint(), -1);
                }


                if (emitData.NodeIndex + 1 >= bindTransforms.Count)
                {
                    emitData.NodeIndex = 0;
                }
                else
                {
                    emitData.NodeIndex++;
                }
            }else
            {
                Debug.LogError($"NextLoopEmitNodeAction::Trigger->EmitData not found.assignIndex = {assignIndex}.");
            }
        }
    }
}
