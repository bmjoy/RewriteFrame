using Eternity.FlatBuffer;
using Gameplay.Battle.Emit;
using Gameplay.Battle.Skill;
using Gameplay.Battle.Timeline.Utils;
using Leyoutech.Core.Context;
using Leyoutech.Core.Effect;
using Leyoutech.Core.Timeline;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Battle.Timeline.Actions.Effects
{
    public class AddEmitDurationEffectAction : ADurationActionItem
    {
        private List<EffectController> effects = new List<EffectController>();

        public override void SetEnv(IContext context, ActionData data,float timeScale)
        {
            base.SetEnv(context, data,timeScale);

            AddEmitDurationEffectData aeeData = GetData<AddEmitDurationEffectData>();
            Duration = aeeData.Duration * timeScale;
        }

        public override void DoEnter()
        {
            EmitSelectionData emitSelectionData = m_Context.GetObject<SkillRunningData>().EmitSelectoin;
            IBaseActionProperty property = m_Context.GetObject<IBaseActionProperty>();

            AddEmitDurationEffectData data = GetData<AddEmitDurationEffectData>();

            EmitData[] emitDatas = emitSelectionData.GetEmits(data.EmitIndex);
            IBindNodeActionProperty bindNodeProperty = m_Context.GetObject<IBindNodeActionProperty>();

            for (int i = 0; emitDatas != null && i < emitDatas.Length; i++)
            {
                EmitData emitData = emitDatas[i];

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
                    bindTransforms = bindNodeProperty.GetPresentation().GetBindNode(emitData.NodeType.ToLaunchPoint(), emitData.NodeIndex);
                }


                foreach (var tran in bindTransforms)
                {
                    EffectController effect = EffectActionUtil.CreateEffect(data.Address, tran, property.IsMain());
                    effects.Add(effect);
                }
            }
        }

        public override void DoExit()
        {
            foreach (var effect in effects)
            {
                effect.RecycleFX();
            }
            effects.Clear();
        }

        public override void DoUpdate(float deltaTime)
        {
            
        }
    }
}
