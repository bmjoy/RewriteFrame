using Eternity.FlatBuffer;
using PureMVC.Patterns.Proxy;
using UnityEngine;
using UnityEngine.Assertions;

public partial class CfgEternityProxy : Proxy
{
    public Model GetModel(int ModelId)
    {
        Model? ModelVo = m_Config.ModelsByKey((uint)ModelId);
        Assert.IsTrue(ModelVo.HasValue, "CfgEternityProxy => GetModel not exist ModelId " + ModelId);
        return ModelVo.Value;
    }
    public UiModel GetUiModel(string ModelId)
    {
        UiModel? UiModelVo = m_Config.UiModelsByKey(ModelId);
        Assert.IsTrue(UiModelVo.HasValue, "CfgEternityProxy => GetUiModel not exist ModelId " + ModelId);
        return UiModelVo.Value;
    }
    /// <summary>
    /// 获取ui模型位置
    /// </summary>
    /// <param name="modelVO"></param>
    /// <returns></returns>
    public Vector3 GetUiModelPos(UiModel modelVO)
    {
        if (modelVO.ModelPositionLength == 3)
        {
            return new Vector3(modelVO.ModelPosition(0), modelVO.ModelPosition(1), modelVO.ModelPosition(2));
        }
        return Vector3.zero;
    }
    /// <summary>
    /// 获取ui模型旋转角度
    /// </summary>
    /// <param name="modelVO"></param>
    /// <returns></returns>
    public Vector3 GetUiModelRotation(UiModel modelVO)
    {
        if (modelVO.ModelRotationLength == 3)
        {
            return new Vector3(modelVO.ModelRotation(0), modelVO.ModelRotation(1), modelVO.ModelRotation(2));
        }
        return Vector3.zero;
    }
    /// <summary>
    /// 获取ui模型缩放大小
    /// </summary>
    /// <param name="modelVO"></param>
    /// <returns></returns>
    public Vector3 GetUiModelScale(UiModel modelVO)
    {
        if (modelVO.ModelScale > 0)
        {
            return modelVO.ModelScale * Vector3.one;
        }
        return Vector3.one;
    }
    public int GetEffectIdByEffectType(uint modelId, EnumEffectType effectType)
    {
        Model? modelVoTemp = m_Config.ModelsByKey(modelId);
        if (!modelVoTemp.HasValue)
        {
            return 0;
        }

        Model model = modelVoTemp.Value;
        switch (effectType)
        {
            case EnumEffectType.Cruise:
                return model.CruiseFx;
            case EnumEffectType.Fight:
                return model.FightFx;
            case EnumEffectType.OverloadLoop:
                return model.OverloadLoopFx;
            case EnumEffectType.OverloadEnd:
                return model.OverloadEndFx;
            case EnumEffectType.DeathGlideTail:
                return model.DeadTailFx;
            case EnumEffectType.DeadExplosion:
                return model.DeadExplosionFx;
            case EnumEffectType.Revive:
                return model.RebirthFx;
            case EnumEffectType.DeadBall:
                return model.DeadBallFx;
			case EnumEffectType.DeadLeap:
				return model.DeadLeapFx;
            case EnumEffectType.ReformerBeginFx:
                return model.ReformerBeginFx;
            case EnumEffectType.ReformerLoopFx:
                return model.ReformerLoopFx;
            case EnumEffectType.ReformerEndFx:
                return model.ReformerEndFx;
            case EnumEffectType.BornFx:
                return model.BornFx;
			case EnumEffectType.JumpOwn:
				return model.JumpOwn;
			case EnumEffectType.JumpThird:
				return model.JumpThird;
			case EnumEffectType.JumpEnd:
				return model.JumpEnd;
			default:
                return 0;
        }
    }
}