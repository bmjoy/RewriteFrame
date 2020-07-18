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
    public class AddEmitLinkTargetDurationEffectAction : ADurationActionItem
    {
        private List<EffectController> effects = new List<EffectController>();

        private AddEmitLinkTargetDurationEffectData aeeData;

        private IBaseActionProperty property;

        private List<Transform> bindTransforms = null;

        private ActionTarget actionTarget = null;

        public override void SetEnv(IContext context, ActionData data, float timeScale)
        {
            base.SetEnv(context, data, timeScale);
            aeeData = GetData<AddEmitLinkTargetDurationEffectData>();
            property = m_Context.GetObject<IBaseActionProperty>();

            Duration = aeeData.Duration * timeScale;
        }

        public override void DoEnter()
        {
            EmitSelectionData emitSelectionData = m_Context.GetObject<SkillRunningData>().EmitSelectoin;
           // IBaseActionProperty property = m_Context.GetObject<IBaseActionProperty>();
           // AddEmitLinkTargetDurationEffectData data = GetData<AddEmitLinkTargetDurationEffectData>();
            EmitData[] emitDatas = emitSelectionData.GetEmits(aeeData.EmitIndex);
            if(emitDatas == null || emitDatas.Length == 0)
            {
                Leyoutech.Utility.DebugUtility.LogWarning("AddEmitLinkTargetDurationEffectAction", "AddEmitLinkTargetDurationEffectAction::DoEnter->emitData not found");
                return;
            }

            actionTarget = SelectionTargetUtil.GetActionTarget(m_Context, aeeData.TargetSelection.Value);
            if (actionTarget == null || actionTarget.TargetType == ActionTargetType.None)
            {
                Leyoutech.Utility.DebugUtility.LogWarning("AddEmitLinkTargetDurationEffectAction", "AddEmitLinkTargetDurationEffectAction::DoEnter->target not found");
                return;
            }

            IBindNodeActionProperty bindNodeProperty = m_Context.GetObject<IBindNodeActionProperty>();
            for (int i = 0; i < emitDatas.Length; ++i)
            {
                EmitData emitData = emitDatas[i];

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
                    bindTransforms = bindNodeProperty.GetPresentation().GetBindNode(emitData.NodeType.ToLaunchPoint(), emitData.NodeIndex);
                }

                foreach (var tran in bindTransforms)
                {
                    EffectController effect = null;
                    if (actionTarget.TargetType == ActionTargetType.Entity)
                    {
                        if(aeeData.TargetSelection.Value.TargetSecondType == Eternity.FlatBuffer.Enums.TargetSecondType.Entity)
                        {
                            effect = EffectActionUtil.CreateEffect(aeeData.Address, tran, property.IsMain(), actionTarget.TargetEntity, Vector3.zero);
                        }
                        else
                        {
                            effect = EffectActionUtil.CreateEffect(aeeData.Address, tran, property.IsMain(), actionTarget.TargetEntity, actionTarget.TargetEntityHitPositionOffet);
                        }
                    }
                    else
                    {
                        effect = EffectActionUtil.CreateEffect(aeeData.Address, tran, property.IsMain(), actionTarget.TargetPosition);
                    }
                    if (effect != null)
                    {
                        effects.Add(effect);
                    }
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

            if (bindTransforms != null)
                bindTransforms.Clear();
            bindTransforms = null;
            actionTarget = null;
    }

        public override void DoUpdate(float deltaTime)
        {
            if (bindTransforms == null || bindTransforms.Count == 0)
                return;

            actionTarget = SelectionTargetUtil.GetActionTarget(m_Context, aeeData.TargetSelection.Value);
            if (actionTarget == null || actionTarget.TargetType == ActionTargetType.None)
            {
                Leyoutech.Utility.DebugUtility.LogWarning("AddEmitLinkTargetDurationEffectAction", "AddEmitLinkTargetDurationEffectAction::DoEnter->target not found");
                return;
            }

            //Leyoutech.Utility.DebugUtility.LogWarning("连接特效", string.Format("目标，entity = {0},偏移 offset = {1}", actionTarget.TargetEntity, actionTarget.TargetEntityHitPositionOffet));

            foreach (var effect in effects)
            {
                if (actionTarget.TargetType == ActionTargetType.Entity)
                {
                    if (aeeData.TargetSelection.Value.TargetSecondType == Eternity.FlatBuffer.Enums.TargetSecondType.Entity)
                    {
                        EffectActionUtil.ChangeLinkEffectTarget(effect, actionTarget.TargetEntity, Vector3.zero);
                    }
                    else
                    {
                        EffectActionUtil.ChangeLinkEffectTarget(effect, actionTarget.TargetEntity, actionTarget.TargetEntityHitPositionOffet);
                    }
                }
                else
                {
                    EffectActionUtil.ChangeLinkEffectTarget(effect,  actionTarget.TargetPosition);
                }
            }
        }
    }
}
