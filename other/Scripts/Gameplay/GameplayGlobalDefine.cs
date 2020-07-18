using UnityEngine;

[System.Serializable]
public class pdPlaneTweakableProperties
{ 
    [Header("转向能力")]
    public SteeringProperties PitchSteering;
    public SteeringProperties YawSteering;
    public SteeringProperties RollSteering;

    [System.Serializable]
    public struct SteeringProperties
    {
        /// <summary>
        /// 假的转向的最大角加速度，假的包括：
        ///     战舰的Pitch、Roll
        ///     战机的Roll
        /// </summary>
        public float MaxDummyAcceleration;
        /// <summary>
        /// 假的转向的最大角度
        /// </summary>
        public float MaxDummyAngular;
    }
}

/// <summary>
/// Animator变量信息
/// </summary>
public struct AnimatorParameterInfo
{
    public string Name;

    public AnimatorControllerParameterType Type;
}

public static class GamePlayGlobalDefine
{
    /// <summary>
    /// 玩家输入采样间隔时间
    /// </summary>
    public const float PlayInputSampleInterval = 0.1f;

    /// <summary>
    /// 飞船死亡最大滑行时间
    /// </summary>
    public const float SpacecraftMaxDeadSlidingTime = 3f;

    /// <summary>
    /// 飞船尸体特效延迟时间
    /// </summary>
    public const float SpcaecraftCorpseFxDelayTime = 3f;
}

public enum InputMode
{
    Keyboard,
    Joystick
}

public enum EnumMotionMode
{
    Normal = 0,
    Dof6ReplaceOverload = 1,
}

public enum MotionType
{
    Mmo,

	Dof4,

	Dof6
}

public static class SpacecraftDeadReliveFxName
{
    /// <summary>
    /// 死亡时停留特效
    /// </summary>
    public const string SelfDeadStayFxName = "siwangtingliu_post";
    /// <summary>
    /// 复活瞬间特效
    /// </summary>
    public const string ReliveFxName = "wanjiafuhuo";
    /// <summary>
    /// 死亡瞬间特效
    /// </summary>
    public const string DeadFxName = "Z_Death_PS_AR_Aurora";
    /// <summary>
    /// 死亡拖尾特效
    /// </summary>
    public const string DeadSlidingName = "Z_Death_PS_AR_Aurora_Pre";
}
