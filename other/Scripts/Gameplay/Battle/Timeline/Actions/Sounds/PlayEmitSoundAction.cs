using Eternity.FlatBuffer;
using Gameplay.Battle.Emit;
using Gameplay.Battle.Skill;
using Gameplay.Battle.Timeline.Utils;
using Leyoutech.Core.Timeline;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Battle.Timeline.Actions.Sounds
{
    public class PlayEmitSoundAction : AEventActionItem
    {
        public override void Trigger()
        {
            EmitSelectionData emitSelectionData = m_Context.GetObject<SkillRunningData>().EmitSelectoin;
            IBindNodeActionProperty bindNodeProperty = m_Context.GetObject<IBindNodeActionProperty>();
            IBaseActionProperty baseActionProperty = m_Context.GetObject<IBaseActionProperty>();
            PlayEmitSoundData nodeData = GetData<PlayEmitSoundData>();

            EmitData[] emitDatas = emitSelectionData.GetEmits(nodeData.EmitIndex);
            if(emitDatas!=null && emitDatas.Length>0)
            {
                foreach (var emitData in emitDatas)
                {
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
                        if (baseActionProperty.IsMain())
                        {
                            WwiseUtil.PlaySound(nodeData.MainSoundID, false, tran);
                        }
                        else
                        {
                            WwiseUtil.PlaySound(nodeData.ThirdPartSoundID, false, tran);
                        }
                    }
                }
            }else
            {

            }
        }
    }
}
