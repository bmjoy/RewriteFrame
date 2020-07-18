using Assets.Scripts.Define;
using Assets.Scripts.Proto;
using Eternity.FlatBuffer.Enums;
using Eternity.Runtime.Item;
using Google.Protobuf.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static SpacecraftEntity;

public enum ComponentEventName
{
    #region Input
    ChangeHumanInputState,
    ChangeSpacecraftInputState,
    ChangeMotionType,
    InputSample,
    #endregion

    AvatarLoadFinish,
    HumanAnimationChangeState,
	OnGetMeshRenderer,

	SpacecraftAnimationChangeState,
	SpacecraftChangeState,
	PlayStateAnima,

	/// <summary>
	/// 刷新飞船拟态数据
	/// </summary>
	RefreshSpacecraftMimesisData,

    MoveSceneRoot,
    SetOffset,

    RespondMobnsterRoadPoint,
    RespondForceChangePos,

    MSG_SKILL_HIT,

    MSG_SKILL_START_CHANTING,
    MSG_SKILL_STOP_CHANTING,
    MSG_SKILL_UPDATE_CHANTING_TIME,

    MSG_SKILL_START_CHARGING,
    MSG_SKILL_STOP_CHARGING,
    MSG_SKILL_UPDATE_CHARGING_TIME,

    MSG_SKILL_START_CHANNELLING,
    MSG_SKILL_STOP_CHANNELLING,
    MSG_SKILL_UPDATE_CHANNELLING_TIME,

    MSG_SKILL_START_MULTISHOT,
    MSG_SKILL_STOP_MULTISHOT,
    MSG_SKILL_UPDATE_MULTISHOT_TIME,

    ChangeTarget,
    LockTarget,

    Event_s2c_cast_skill,
    Event_s2c_sing_skill,
    Event_s2c_stop_skill,
    Event_s2c_cast_skill_fail_notify,
    Event_s2c_skill_effect,
	Event_s2c_heal_effect,
	Event_s2c_effect_immuno,

    ShipJumpResponse,

    /// <summary>
    /// 飞船形态, 改变战斗模式. 现在有巡航模式和战斗模式
    /// </summary>
    ChangeBattleState,

	/// <summary>
	/// 开启/关闭 转化炉
	/// </summary>
	ActivateBurst,

	/// <summary>
	/// 开始/结束 启动转化炉的聚气
	/// </summary>
	BurstPressed,

	/// <summary>
	/// 添加Buff
	/// </summary>
	BuffAdd,

	/// <summary>
	/// 移除Buff
	/// </summary>
	BuffRemove,

	/// <summary>
	/// 设置飞船可见性. 改变SkinRootTransform即它的子节点的所有Render的 active 属性
	/// </summary>
	SetSpacecraftVisibility,

	/// <summary>
	/// 是否激活飞船的Collider
	/// </summary>
	ActivateSpacecraftCollider,

	/// <summary>
	/// 创建了新的飞船
	/// </summary>
	NewSpacecraftEntity,

    /// <summary>
    /// 显示死亡特效
    /// </summary>
    ShowDeadExplosionFx,

    /// <summary>
    /// 显示死亡滑行特效
    /// </summary>
    ShowDeadSlidingFx,

    /// <summary>
    /// 显示死亡尸体特效
    /// </summary>
    ShowDeadCorpseFx,

    /// <summary>
    /// 显示复活特效
    /// </summary>
    ShowReliveFx,

    /// <summary>
    /// 角色死亡
    /// </summary>
    Dead,

    /// <summary>
    /// 角色复活
    /// </summary>
    Relive,

    /// <summary>
    /// 强制改变飞船运动
    /// </summary>
    ForceChangeSpacecraftMotion,

	/// <summary>
	/// 无双值满了
	/// </summary>
	MaxPeerlessReached,

	#region 技能和武器
	/// <summary>
	/// 按下或者松开释放技能的快捷键
	/// </summary>
	CastSkill,
	/// <summary>
	/// 技能释放失败, 客户端检查没过
	/// </summary>
	UnsuccessfulReleaseOfSkill,
	/// <summary>
	/// 请求武器开火
	/// </summary>
	WeaponOperation,
	/// <summary>
	/// 武器开火后
	/// </summary>
	PostWeaponFire,
	/// <summary>
	/// 武器弹药信息改变
	/// </summary>
	WeaponPowerChanged,
	/// <summary>
	/// 武器切换完毕
	/// </summary>
	PlayerWeaponToggleEnd,
	#endregion

	/// <summary>
	/// 飞船更新速度
	/// </summary>
	SpacecraftUpdateSpeed,

	/// <summary>
	/// 人形态移动   
	/// </summary>
	HumanMoveRespond,

	/// <summary>
	/// 飞船被选中为主角的目标
	/// </summary>
	SpacecraftIsSelectedAsTarget,
	
	/// <summary>
	/// 有包里的物品被 增删改 了
	/// </summary>
	ItemInPackageChanged,
	/// <summary>
	/// 物品同步结束
	/// </summary>
	ItemSyncEnd,
	/// <summary>
	/// 武器技能结束了
	/// </summary>
	WeaponSkillFinished,

	/// <summary>
	/// 播放设备的死亡特效
	/// </summary>
	ShowDeviceDeadFX,
	/// <summary>
	/// 播放设备死亡动画
	/// </summary>
	PlayDeviceDeadAnimation,

    /// <summary>
    /// 飞船跃迁开始
    /// </summary>
    SpacecraftLeapStart,
    /// <summary>
    /// 飞船跃迁结束
    /// </summary>
    SpacecraftLeapEnd,
    /// <summary>
    /// 对矿产生伤害
    /// </summary>
    MineDamage,
	/// <summary>
	/// 连线结束
	/// </summary>
	LineEffectEnd,
	/// <summary>
	/// 封印结束
	/// </summary>
	SealEnd,
	/// <summary>
	/// 播放音效
	/// </summary>
	PlaySound,
	/// <summary>
	/// 播放窗口语音
	/// </summary>
	PlayVideoSound,
	/// <summary>
	/// 播放系统语音
	/// </summary>
	PlaySystemSound,
	/// <summary>
	/// 播放矿石碎裂音效
	/// </summary>
	PlayFragmentationSound,
	/// <summary>
	/// 播放矿石阶段奖励音效
	/// </summary>
	PlayDropSound,
	/// <summary>
	/// 检查死亡掉落
	/// </summary>
	CheckDeadDrop,
    #region ================================-技能==================================

    /// <summary>
    /// 技能按键响应 
    /// </summary>
    SkillButtonResponse,

    /// <summary>
    /// 请求瞬发预演释放的技能,返回结果
    /// </summary>
    CaseSkillResult,

    /// <summary>
    /// 停止当前技能
    /// </summary>
    ToStopSkill,

    /// <summary>
    ///结束技能释放
    /// </summary>
    ToEndSkill,


    /// <summary>
    /// 暂停释放技能
    /// </summary>
    PauseReleaseSkill,

    /// <summary>
    /// 恢复释放技能
    /// </summary>
    RecoveryReleaseSkill,


    /// <summary>
    /// 子弹碰撞到单位了
    /// </summary>
    FlyerTriggerToEnitity,

    /// <summary>
    /// 蓄力索引结果
    /// </summary>
    AccumulationIndex,

    /// <summary>
    /// 随机发射点
    /// </summary>
    RandomEmitNode,

    /// <summary>
    /// 武器的技能释放结束
    /// </summary>
    WeaponSkillFinish,

    /// <summary>
    /// 强制技能按键抬起
    /// </summary>
    CoerceSkillButtonUp,


    /// <summary>
    /// 广播释放技能
    /// </summary>
    BroadCastSkill_ReleaseSkill,

    /// <summary>
    /// 广播,技能释放，目标列表
    /// </summary>
    BroadCastSkill_BeginTargets,


    /// <summary>
    /// 广播蓄力索引，目标列表等
    /// </summary>
    BroadCastSkill_Accumulation,


    /// <summary>
    /// 广播引导，目标列表变化
    /// </summary>
    BroadCastSkill_ChangeTargets,

    #endregion ============================end====-技能============================
    /// <summary>
    /// 4/6dof切换重置
    /// </summary>
    ResetRotation,

	#region 新协议相关
	G2C_ChangeState,
    G2C_HeroMove,
    #endregion

    #region 新消息
    /// <summary>
    /// 玩家状态改变
    /// </summary>
    ChangHeroState,
    /// <summary>
    /// 改变属性
    /// </summary>
    ChangeProperty,
    #endregion
}

public interface IComponentEvent
{
}

public class ChangeProperty : IComponentEvent
{
    public SpacecraftMotionInfo SpacecraftMotionInfo;
}

public class ChangHeroState : IComponentEvent
{

} 

public class G2C_ChangeState : IComponentEvent
{
    /// <summary>
    /// 实例id
    /// </summary>
    public ulong EntityId;
    /// <summary>
    /// 当前状态
    /// </summary>
    public ulong CurrentState;
    /// <summary>
    /// 上一个状态
    /// </summary>
    public ulong PreviousState;
    /// <summary>
    /// 扩展数据
    /// </summary>
    public Crucis.Protocol.StateExMessage ExData;
}

public class G2C_HeroMove : IComponentEvent
{
    public Crucis.Protocol.HeroMoveParams Respond;
}

public class HumanMoveRespond : IComponentEvent
{
    public bool IsRun;
    public Vector3 Position;
    public Quaternion Rotation;
    public Vector3 TargetPosition;
}

public class ChangeHumanInputStateEvent : IComponentEvent
{
    public Vector3 EngineAxis;
}

public class HumanAnimationChangeStateEvent : IComponentEvent
{
    public Vector3 EngineAxis;
}

public class ChangeInputModeEvent : IComponentEvent
{
    public InputMode InputMode;
}

public class SetOffsetEvent : IComponentEvent
{
    public Vector3 Offset;
}

public class ChangeSpacecraftInputStateEvent : IComponentEvent
{
    public Vector3 EngineAxis;
    public Vector3 RotateAxis;
}

public class SpacecraftAnimationChangeStateEvent : IComponentEvent
{
    public Vector3 EngineAxis;
    public Vector3 RotateAxis;
}

public class SpacecraftChangeState : IComponentEvent
{
	public EnumMainState OldMainState;
	public EnumMainState NewMainState;
}

public class PlayStateAnima : IComponentEvent
{
	public String Name;
}

public class AvatarLoadFinishEvent : IComponentEvent
{
    public SpacecraftPresentation SpacecraftPresentation;
    public Animator Animator;
    public Animator[] Animators;
}

public class GetMeshRendererEvent : IComponentEvent
{
	public MeshRenderer MeshRenderer;
	public Transform Transform;
}

//=================================================
public class RespondMoveEvent : IComponentEvent
{
    public bool IsMonster;

    public uint behaviorType;
    public Vector3 Offset;
    public uint bindEntiryId;
    public bool bindBeRefresh;
    public bool bindBeImme;

    public Vector3 Position;
    public Quaternion Rotation;

    public Vector3 LineVelocity;
    public Vector3 AngularVelocity;

    public Vector3 EngineAxis;
    public Vector3 RotateAxis;

    public Vector3 PredictPoint;

    public List<Vectors> ServerPoints;

	public bool syncState;
    public ulong State;
    public bool IsHitWall;
}

public class RespondForceChangePos : IComponentEvent
{
	public bool Forced;
    public uint EntiryId;
    public ulong AreaUid;
    public Vector3 Position;
    public Quaternion Rotation;
    public Vector3 LineVelocity;
    public Vector3 AngularVelocity;
    public Vector3 EngineAxis;
    public Vector3 RotateAxis;
    public bool IsResetAll;
}

public class ChangeTargetEvent : IComponentEvent
{
    public SpacecraftEntity newTarget;
}




public class SkillStartPeriod : IComponentEvent
{
    public SkillPeriodType PeriodType;
    public int SkillID;
    /// <summary>
    /// 秒
    /// </summary>
    public float Time;
}

public class SkillFinishPeriod : IComponentEvent
{
    public bool Success;
}

/// <summary>
/// 开始吟唱
/// </summary>
public class SkillStartChanting : IComponentEvent
{
    public SkillPeriodType PeriodType;
    public int SkillID;
    public float Time;
}

/// <summary>
/// 开始吟唱
/// </summary>
public class SkillStopChanting : IComponentEvent
{
    bool Success;
}

/// <summary>
/// 开始吟唱
/// </summary>
public class SkillUpdateChantingTime : IComponentEvent
{
    public float Time;
}

/// <summary>
/// 开始吟唱
/// </summary>
public class SkillStartCharging : IComponentEvent
{
    public int SkillID;
    public float Time;
}

/// <summary>
/// 开始吟唱
/// </summary>
public class SkillStopCharging : IComponentEvent
{
    bool Success;
}

/// <summary>
/// 更新蓄力的当前经过时间
/// </summary>
public class SkillUpdateChargingTime : IComponentEvent
{
    public float Time;
}

/// <summary>
/// 开始引导
/// </summary>
public class SkillStartChannelling : IComponentEvent
{
    public int SkillID;
    public float Time;
}

/// <summary>
/// 停止引导
/// </summary>
public class SkillStopChannelling : IComponentEvent
{
    bool Success;
}

/// <summary>
/// 更新引导的当前经过时间
/// </summary>
public class SkillUpdateChannellingTime : IComponentEvent
{
    public float Time;
}

/// <summary>
/// 开始多轮发射
/// </summary>
public class SkillStartMultiShot : IComponentEvent
{
    public int SkillID;
    public float Time;
}

/// <summary>
/// 停止多轮发射
/// </summary>
public class SkillStopMultiShot : IComponentEvent
{
    bool Success;
}

/// <summary>
/// 更新多轮发射的当前经过时间
/// </summary>
public class SkillUpdateMultiShotTime : IComponentEvent
{
    public float Time;
}

/// <summary>
/// 技能击中目标时的通知
/// </summary>
public class SkillHitNotification : IComponentEvent
{
    /// <summary>
    /// 光束类技能是瞬间命中的, 在创建技能Object的时候就命中了. 
    /// 非光束类技能有自己的Collider, 与目标碰撞时触发命中.
    /// IsBeam为True时, TargetEntity可用
    /// IsBeam为False时, TargetCollider可用
    /// </summary>
    public bool IsBeam;
    public SpacecraftEntity TargetEntity;
    public Vector3 HitPoint;
}


public class ReceiveSkillHurtNotification : IComponentEvent
{
    public uint TargetID;
    public int Damage;
    public int CritType;
    public int PenetrationDamage;
    public bool IsDodge;
    public int EffectID;
}

public class S2C_CAST_SKILL_Event : IComponentEvent
{
	public S2C_CAST_SKILL msg;
}

public class S2C_SING_SKILL_Event : IComponentEvent
{
	public S2C_SING_SKILL msg;
}

public class S2C_STOP_SKILL_Event : IComponentEvent
{
	public S2C_STOP_SKILL msg;
}

public class S2C_CAST_SKILL_FAIL_NOTIFY_Event : IComponentEvent
{
	public S2C_CAST_SKILL_FAIL_NOTIFY msg;
}

public class S2C_SKILL_EFFECT_Event : IComponentEvent
{
	public S2C_SKILL_EFFECT msg;
}

public class S2C_HEAL_EFFECT_Event : IComponentEvent
{
	public S2C_HEAL_EFFECT msg;
}

public class S2C_EFFECT_IMMUNO_Event : IComponentEvent
{
	public S2C_EFFECT_IMMUNO msg;
}

public class CastSkillEvent : IComponentEvent
{
    public int SkillID;
}

public class ActivateBurstPressedEvent : IComponentEvent
{
	public bool Ready;
}

public class ActivateBurstEvent : IComponentEvent
{
	public bool Active;
}

public class ShipJumpResponseEvent : IComponentEvent
{
    public S2C_SHIP_JUMP_RESPONSE Data;
}

public class AddBuffEvent : IComponentEvent
{
	public BuffVO buff;
}

public class RemoveBuffEvent : IComponentEvent
{
	public uint buffID;
}

public class ChangeSpacecraftVisibilityEvent : IComponentEvent
{
	public enum VisibilityType
	{
		MainBody,
		VFX,
		MainBodyAndVFX,
		EntireSpacecraft,
		Count,
	}

	public VisibilityType PartType;
	public bool Visible;
}

public class ActivateSpacecraftColliderEvent : IComponentEvent
{
	public bool active;
}

public class NewSpacecraftEvent : IComponentEvent
{
	public KHeroType HeroType;
}

#region 技能
public class SkillHotkey : IComponentEvent
{
	public string Hotkey;
	public bool IsWeaponSkill;
	public int SkillIndex;
	public HotkeyPhase ActionPhase;
}

public class SkillCastEvent : IComponentEvent
{
	public enum Operation
	{
		Stop = 0,
		Cast = 1,
	}

	/// <summary>
	/// 是不是当前武器对应的技能
	/// </summary>
	public bool IsWeaponSkill;
	/// <summary>
	/// 非武器 表示第几个技能，武器则表示技能ID
	/// </summary>
	public int SkillIndex;
	public bool KeyPressed;
}
#endregion
public class ResetPositionEvent : IComponentEvent
{
    public Vector3 Position;
}

public class ResetRotationEvent : IComponentEvent
{
    public Quaternion Roation;
}

public class DeadEvent : IComponentEvent
{
    public uint Uid;
    public int CD;
    public uint KillerNpctemplateID;
    public string KillerPlayerName;
    public List<short> ReliveOptions;
    public List<DropInfo> DropList;
}

public class ForceChangeSpacecraftMotionEvent : IComponentEvent
{
    /// <summary>
    /// 运动改变类型
    /// </summary>
    public ForceActionType ForceActionType;
    /// <summary>
    /// 运动模型类型
    /// </summary>
    public PhysxMotionType PhysxMotionType;
    /// <summary>
    /// 以下参数是否有效
    /// </summary>
    public bool IsAvailableParams;
    public Vector3 Position;
    public Quaternion Rotation;
    public Vector3 LineVelocity;
    public Vector3 AngularVelocity;
    public Vector3 EngineAxis;
    public Vector3 RotateAxis;
    public List<Vector3> Targets;
}

public class RequestTransferEvent : IComponentEvent
{
	public Vector3 EndPoint;
	public uint TeleportId;
}

public class BeSelectedAsTarget : IComponentEvent
{
	/// <summary>
	/// 是不是被选中了
	/// 用来表示这次通知是选中, 还是取消选中
	/// </summary>
	public bool isSelected;
}


public class ItemOperateEvent: IComponentEvent
{
	public enum OperateType : int
	{
		Add,
		Remove,
		Replace
	}
	public Eternity.Runtime.Item.Category Category = 0;
	public OperateType Type = 0;
	public int Pos = 0;
	public ulong UID = 0;
}

public class RefreshSpacecraftMimesisDataEvent : IComponentEvent
{
    public SpacecraftMotionInfo.MimesisInfo MimesisData;
}

public class WeaponSkillFinishedEvent : IComponentEvent
{
	public bool Success;
}

public class UnsuccessfulReleaseOfSkillEvent : IComponentEvent
{
	public int SkillID;
}

public class MineDamage : IComponentEvent
{
    public Collider HitCollider;
    public uint Damage;
}

public class PlaySound : IComponentEvent
{
	public int SoundID;
	public Transform Transform;
}

public class PlayVideoSound : IComponentEvent
{
	public int GroupID;
	public Action Action;
	public uint NpcId;
}

public class PlaySystemSound : IComponentEvent
{
	public WwiseMusicSpecialType SoundID;
	public Action<object> EndAction;
}

public class ResetRotation : IComponentEvent
{
	public MotionType Type;
}

#region ================================-技能==================================
/// <summary>
/// 请求瞬发预演释放的技能返回结果
/// </summary>
public class CaseSkillResult : IComponentEvent
{
    public int skillId;         //技能ID
    public bool succeed; //成功与否
}

/// <summary>
/// 停止技能
/// </summary>
public class StopSkillResult : IComponentEvent
{
    public int skillId;         //技能ID
}


/// <summary>
/// 结束技能
/// </summary>
public class EndSkillResult:IComponentEvent
{
    public int skillId;         //技能ID
}


/// <summary>
/// 子弹碰撞到单位了
/// </summary>
public class FlyerTriggerToEnitity: IComponentEvent
{
    public BaseEntity targetEntity; //子弹碰到的单位
    public Vector3 targetPosition;//打击的位置
}


/// <summary>
/// 服务器蓄力索引结果
/// </summary>
public class AccumulationIndexResult : IComponentEvent
{
    public int skillId;                                 //技能ID
    public int accumulationIndex;         //蓄力索引
}

/// <summary>
/// 随机发射点
/// </summary>
public class RandomEmitNodeResult : IComponentEvent
{
    public global::Crucis.Protocol.EmitNode emitNode;
}

/// <summary>
/// 武器的技能释放结束
/// </summary>
public class WeaponSkillFinish : IComponentEvent
{
    public bool IsMain;                             //是否是主角
    public int skillId;                                 //技能ID
}

/// <summary>
/// 广播释放技能
/// </summary>
public class BroadCastSkill_ReleaseSkill : IComponentEvent
{
    public int skillId;                                 //技能ID
}

/// <summary>
///广播，技能释放，下发目标列表，跟最远处方向
/// </summary>
public class BroadCastSkill_BeginTargets : IComponentEvent
{
    public int skillId;                                                                                                     //技能ID
    public RepeatedField<global::Crucis.Protocol.TargetInfo> targets;             //目标列表
    public Crucis.Protocol.Vec3 direction;                                                               //目标列表为空时，最远处朝向上的一个“点”
}


/// <summary>
/// 广播蓄力索引，目标列表等
/// </summary>
public class BroadCastSkill_Accumulation : IComponentEvent
{
    public int skillId;                                                                                                     //技能ID
    public RepeatedField<global::Crucis.Protocol.TargetInfo> targets;             //目标列表
    public Crucis.Protocol.Vec3 direction;                                                               //目标列表为空时，最远处朝向上的一个“点”
    public int groupIndex;                                                                                         //蓄力选择的groupIndex
}


/// <summary>
///  广播引导，目标列表变化
/// </summary>
public class BroadCastSkill_ChangeTargets : IComponentEvent
{
    public int skillId;                                                                                                     //技能ID
    public RepeatedField<global::Crucis.Protocol.TargetInfo> targets;             //目标列表
    public Crucis.Protocol.Vec3 direction;                                                               //目标列表为空时，最远处朝向上的一个“点”
}

#endregion ===========================end=====-技能============================

