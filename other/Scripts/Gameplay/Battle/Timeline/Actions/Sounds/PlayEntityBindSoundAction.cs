using Eternity.FlatBuffer;
using Gameplay.Battle.Timeline;
using Gameplay.Battle.Timeline.Utils;
using Leyoutech.Core.Timeline;
using System.Collections.Generic;
using UnityEngine;

namespace Battle.Timeline.Actions.Sounds
{
    public class PlayEntityBindSoundAction : AEventActionItem
    {
        public override void Trigger()
        {
            PlayEntityBindSoundData data = GetData<PlayEntityBindSoundData>();
            IBindNodeActionProperty bindNodeProperty = m_Context.GetObject<IBindNodeActionProperty>();
            IBaseActionProperty baseActionProperty = m_Context.GetObject<IBaseActionProperty>();

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
                bindTransforms = bindNodeProperty.GetPresentation().GetBindNode(data.NodeType.ToLaunchPoint(), data.NodeIndex);
            }


            foreach (var tran in bindTransforms)
            {
                if (baseActionProperty.IsMain())
                {
                    WwiseUtil.PlaySound(data.MainSoundID, false, tran);
                }
                else
                {
                    WwiseUtil.PlaySound(data.ThirdPartSoundID, false, tran);
                }
            }
        }
    }
}


