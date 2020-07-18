using System;
using System.Collections.Generic;
using Eternity.FlatBuffer;
using Gameplay.Battle.Emit;
using Gameplay.Battle.Skill;
using Gameplay.Battle.Timeline.Utils;
using Leyoutech.Core.Timeline;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Gameplay.Battle.Timeline.Actions.Emit
{
    public class AutoRangeEmitNodeAction : AEventActionItem
    {
        public override void Trigger()
        {
            AutoRangeEmitNodeData data = GetData<AutoRangeEmitNodeData>();

            IBaseActionProperty property = m_Context.GetObject<IBaseActionProperty>();
            IBindNodeActionProperty bindNodeProperty = m_Context.GetObject<IBindNodeActionProperty>();
            EmitSelectionData emitSelectionData = m_Context.GetObject<SkillRunningData>().EmitSelectoin;

            List<Transform> bindTransforms = null;
            if (bindNodeProperty.GetPresentation() == null)
            {
                Leyoutech.Utility.DebugUtility.LogWarning("查找挂点", string.Format("bindNodeProperty.GetPresentation() == null ,Entity = {0} ,ItemId = {1}" +
                    "模型未加载完毕，挂点脚本未被赋值!, 此时挂点作 root 处理",
                    property.EntityId(),
                    property.GetItemID()
                    ));

                bindTransforms = new List<Transform>();
                bindTransforms.Add(property.GetRootTransform());
            }
            else
            {
               bindTransforms = bindNodeProperty.GetPresentation().GetBindNode(data.NodeType.ToLaunchPoint(), -1);
            }


            if (bindTransforms == null || bindTransforms.Count==0)
            {
                Debug.LogError("AutoRangeEmitNodeAction::Trigger->bind node is null");
                return;
            }
            List<int> orgIndex = new List<int>();
            for(int i =0;i<bindTransforms.Count;++i)
            {
                orgIndex.Add(i);
            }
            if(data.Count>orgIndex.Count)
            {
                for(int i =0;i<data.Count - orgIndex.Count;++i)
                {
                    Random.InitState((int)DateTime.Now.Ticks);
                    int index = Random.Range(0, orgIndex.Count);
                    orgIndex.Add(index);
                }
            }

            List<EmitData> emitDatas = new List<EmitData>();
            for(int i =0;i< data.Count;++i)
            {
                Random.InitState((int)DateTime.Now.Ticks);
                int index = Random.Range(0, orgIndex.Count);
                orgIndex.RemoveAt(orgIndex.IndexOf(index));

                EmitData emitData = new EmitData()
                {
                    NodeType = data.NodeType,
                    NodeIndex = index,
                };

                emitDatas.Add(emitData);
            }

            emitSelectionData.AddOrUpdateEmit(data.AssignIndex, emitDatas.ToArray());
        }
    }
}

