using Leyoutech.Core.Effect;
using UnityEngine;

namespace Gameplay.Battle.Timeline.Actions.Effects
{
    internal static class EffectActionUtil
    {
        internal static EffectController CreateEffect(string effectAddress,Vector3 position,bool isMain)
        {
            EffectController effect = EffectManager.GetInstance().CreateEffect(effectAddress, EffectManager.GetEffectGroupNameInSpace(isMain));
            effect.transform.position = position;
            effect.SetCreateForMainPlayer(isMain);

            return effect;
        }

        internal static EffectController CreateEffect(string effectAddress,Transform parentTransfrom,bool isMain)
        {
            EffectController effect = EffectManager.GetInstance().CreateEffect(effectAddress, EffectManager.GetEffectGroupNameInSpace(isMain));
            effect.transform.SetParent(parentTransfrom, false);
            effect.transform.localPosition = Vector3.zero;
            effect.SetCreateForMainPlayer(isMain);

            return effect;
        }

        internal static EffectController CreateEffect(string effectAddress,Transform parentTransfrom, bool isMain,Transform targetTransfrom,Vector3 offset)
        {
            EffectController effect = CreateEffect(effectAddress, parentTransfrom, isMain);
            effect.SetBeamTarget(parentTransfrom, targetTransfrom, offset /*Vector3.zero*/);

            return effect;
        }

        internal static EffectController CreateEffect(string effectAddress, Transform parentTransfrom, bool isMain,Vector3 targetPos)
        {
            EffectController effect = CreateEffect(effectAddress, parentTransfrom, isMain);
            effect.SetBeamTarget(parentTransfrom, targetPos);

            return effect;
        }



        /// <summary>
        /// 修改连接 特效的 目标位置
        /// </summary>
        /// <param name="targetTransfrom">特效连接单位</param>
        /// <param name="offset">于特效连接单位中心位置的偏移</param>
        internal static void ChangeLinkEffectTarget(EffectController effect, Transform targetTransfrom, Vector3 offset)
        {
            if (effect == null)
                return;
            effect.SetBeamTarget(effect.transform.parent, targetTransfrom, offset /*Vector3.zero*/);
        }

        /// <summary>
        /// 修改连接 特效的 目标位置
        /// </summary>
        /// <param name="targetPos">特效连接点</param>
        internal static void ChangeLinkEffectTarget(EffectController effect, Vector3 targetPos)
        {
            if (effect == null)
                return;
            effect.SetBeamTarget(effect.transform.parent, targetPos );
        }
    }
}
