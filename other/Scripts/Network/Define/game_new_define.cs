/*
compile by protobuf, please don't edit it manually. 
Any problem please contact tongxuehu@gmail.com, thx.
*/

using System;
using System.Collections.Generic;
using System.IO;
using Assets.Scripts.Lib.Net;

namespace Assets.Scripts.Define
{
    // 新属性枚举
    public enum AttributeName
    {
        kFrontSpeedMax = 10001,   // 向前最大线速度
        kBackSpeedMax = 10002,   // 向后最大线速度
        kRightSpeedMax = 10003,   // 向右最大线速度
        kLeftSpeedMax = 10004,   // 向左最大线速度
        kUpSpeedMax = 10005,   // 向上最大线速度
        kDownSpeedMax = 10006,   // 向下最大线速度
        kFrontSpeedAccelerate = 10007,   // 向前线加速度
        kBackSpeedAccelerate = 10008,   // 向后线加速度
        kRightSpeedAccelerate = 10009,   // 向右线加速度
        kLeftSpeedAccelerate = 10010,   // 向左线加速度
        kUpSpeedAccelerate = 10011,   // 向上线加速度
        kDownSpeedAccelerate = 10012,   // 向下线加速度
        kFrontBackSpeedDecelerate = 10013,   // 前后阻力减速度
        kLeftRightSpeedDecelerate = 10014,   // 左右阻力减速度
        kUpDownSpeedDecelerate = 10015,   // 上下阻力减速度
        kXAngleSpeedMax = 10016,   // X轴最大角速度
        kYAngleSpeedMax = 10017,   // Y轴最大角速度
        kZAngleSpeedMax = 10018,   // Z轴最大角速度
        kXAngleSpeedAccelerate = 10019,   // X轴角加速度
        kYAngleSpeedAccelerate = 10020,   // Y轴角加速度
        kZAngleSpeedAccelerate = 10021,   // Z轴角加速度
        kXAngleSpeedDecelerate = 10022,   // X轴阻力角减速度
        kYAngleSpeedDecelerate = 10023,   // Y轴阻力角减速度
        kZAngleSpeedDecelerate = 10024,   // Z轴阻力角减速度
        kJumpSpeedBegine = 10025,   // 跃迁起始速度
        kJumpSpeedAccelerate = 10026,   // 跃迁加速度
        kJumpSpeedDecelerate = 10027,   // 跃迁减速度
        kXMaxAngle = 10028,   // X轴最大倾角
        kYMaxAngle = 10029,   // Y轴最大倾角
        kZMaxAngle = 10030,   // Z轴最大倾角
        kXMimicryMaxAngleAccelerate = 10031,   // 拟态X轴最大角加速度
        kYMimicryMaxAngleAccelerate = 10032,   // 拟态Y轴最大角加速度
        kZMimicryMaxAngleAccelerate = 10033,   // 拟态Z轴最大角加速度
        kXMimicryMaxAngle = 10034,   // 拟态X轴最大角度
        kYMimicryMaxAngle = 10035,   // 拟态Y轴最大角度
        kZMimicryMaxAngle = 10036,   // 拟态Z轴最大角度
        kFightFrontSpeedMax = 10101,   // 战斗向前最大线速度
        kFightBackSpeedMax = 10102,   // 战斗向后最大线速度
        kFightRightSpeedMax = 10103,   // 战斗向右最大线速度
        kFightLeftSpeedMax = 10104,   // 战斗向左最大线速度
        kFightUpSpeedMax = 10105,   // 战斗向上最大线速度
        kFightDownSpeedMax = 10106,   // 战斗向下最大线速度
        kFightFrontSpeedAccelerate = 10107,   // 战斗向前线加速度
        kFightBackSpeedAccelerate = 10108,   // 战斗向后线加速度
        kFightRightSpeedAccelerate = 10109,   // 战斗向右线加速度
        kFightLeftSpeedAccelerate = 10110,   // 战斗向左线加速度
        kFightUpSpeedAccelerate = 10111,   // 战斗向上线加速度
        kFightDownSpeedAccelerate = 10112,   // 战斗向下线加速度
        kFightFrontBackSpeedDecelerate = 10113,   // 战斗前后阻力减速度
        kFightLeftRightSpeedDecelerate = 10114,   // 战斗左右阻力减速度
        kFightUpDownSpeedDecelerate = 10115,   // 战斗上下阻力减速度
        kFightXMaxAngleSpeed = 10116,   // 战斗X轴最大角速度
        kFightYMaxAngleSpeed = 10117,   // 战斗Y轴最大角速度
        kFightZMaxAngleSpeed = 10118,   // 战斗Z轴最大角速度
        kFightXAngleSpeedAccelerate = 10119,   // 战斗X轴角加速度
        kFightYAngleSpeedAccelerate = 10120,   // 战斗Y轴角加速度
        kFightZAngleSpeedAccelerate = 10121,   // 战斗Z轴角加速度
        kFightXAngleSpeedDecelerate = 10122,   // 战斗X轴阻力角减速度
        kFightYAngleSpeedDecelerate = 10123,   // 战斗Y轴阻力角减速度
        kFightZAngleSpeedDecelerate = 10124,   // 战斗Z轴阻力角减速度
        kFightXMaxAngle = 10125,   // 战斗X轴最大倾角
        kFightYMaxAngle = 10126,   // 战斗Y轴最大倾角
        kFightZMaxAngle = 10127,   // 战斗Z轴最大倾角
        kXFightMimicryAngleSpeedAccelerate = 10128,   // 战斗拟态X轴最大角加速度
        kYFightMimicryAngleSpeedAccelerate = 10129,   // 战斗拟态Y轴最大角加速度
        kZFightMimicryAngleSpeedAccelerate = 10130,   // 战斗拟态Z轴最大角加速度
        kXFightMimicryAngle = 10131,   // 战斗拟态X轴最大角度
        kYFightMimicryAngle = 10132,   // 战斗拟态Y轴最大角度
        kZFightMimicryAngle = 10133,   // 战斗拟态Z轴最大角度
        kOverloadFrontSpeedMax = 10201,   // 过载向前最大线速度
        kOverloadBackSpeedMax = 10202,   // 过载向后最大线速度
        kOverloadRightSpeedMax = 10203,   // 过载向右最大线速度
        kOverloadLeftSpeedMax = 10204,   // 过载向左最大线速度
        kOverloadUpSpeedMax = 10205,   // 过载向上最大线速度
        kOverloadDownSpeedMax = 10206,   // 过载向下最大线速度
        kOverloadFrontSpeedAccelerate = 10207,   // 过载向前线加速度
        kOverloadBackSpeedAccelerate = 10208,   // 过载向后线加速度
        kOverloadRightSpeedAccelerate = 10209,   // 过载向右线加速度
        kOverloadLeftSpeedAccelerate = 10210,   // 过载向左线加速度
        kOverloadUpSpeedAccelerate = 10211,   // 过载向上线加速度
        kOverloadDownSpeedAccelerate = 10212,   // 过载向下线加速度
        kOverloadFrontBackSpeedDecelerate = 10213,   // 过载前后阻力减速度
        kOverloadLeftRightSpeedDecelerate = 10214,   // 过载左右阻力减速度
        kOverloadUpDownSpeedDecelerate = 10215,   // 过载上下阻力减速度
        kXOverloadAngleSpeedMax = 10216,   // 过载X轴最大角速度
        kYOverloadAngleSpeedMax = 10217,   // 过载Y轴最大角速度
        kZOverloadAngleSpeedMax = 10218,   // 过载Z轴最大角速度
        kXOverloadAngleSpeedAccelerate = 10219,   // 过载X轴角加速度
        kYOverloadAngleSpeedAccelerate = 10220,   // 过载Y轴角加速度
        kZOverloadAngleSpeedAccelerate = 10221,   // 过载Z轴角加速度
        kXOverloadAngleSpeedDecelerate = 10222,   // 过载X轴阻力角减速度
        kYOverloadAngleSpeedDecelerate = 10223,   // 过载Y轴阻力角减速度
        kZOverloadAngleSpeedDecelerate = 10224,   // 过载Z轴阻力角减速度
        kXOverloadMaxAngle = 10225,   // 过载X轴最大倾角
        kYOverloadMaxAngle = 10226,   // 过载Y轴最大倾角
        kZOverloadMaxAngle = 10227,   // 过载Z轴最大倾角
        kXOverloadMimicryAngleSpeedAccelerate = 10228,   // 过载拟态X轴最大角加速度
        kYOverloadMimicryAngleSpeedAccelerate = 10229,   // 过载拟态Y轴最大角加速度
        kZOverloadMimicryAngleSpeedAccelerate = 10230,   // 过载拟态Z轴最大角加速度
        kXOverloadMimicryMaxAngle = 10231,   // 过载拟态X轴最大角度
        kYOverloadMimicryMaxAngle = 10232,   // 过载拟态Y轴最大角度
        kZOverloadMimicryMaxAngle = 10233,   // 过载拟态Z轴最大角度
        kOverloadPowerCostEfficiency = 10299,   // 过载电力消耗效率
        kHPMax = 101,   // 装甲最大值
        kHP = 102,   // 装甲当前值
        kShieldMax = 103,   // 护盾最大值
        kShieldValue = 104,   // 护盾当前值
        kPowerMax = 105,   // 电力最大值
        kPowerValue = 106,   // 电力当前值
        kCartridgeCapacity = 201,   // 子弹弹夹容量
        kCartridgeReloadTime = 202,   // 子弹弹夹装填时间
        kCartridgeCost = 203,   // 子弹单次消耗弹药数量
        kGuidanceCapacity = 204,   // 制导弹仓数量
        kGuidanceReloadTime = 205,   // 制导弹仓回复时间
        kGuidanceCost = 206,   // 制导单次消耗制导数量
        kLightHeatMax = 207,   // 光束热量上限
        kLightHeatCDTime = 208,   // 光束热量冷却时间
        kLightHeatCDEfficiency = 209,   // 光束热量冷却效率
        kLightHeatSafeValue = 210,   // 光束热量安全阀值
        kLightHeatIncrementOnce = 211,   // 光束单次热量增加
        kConverterMax = 212,   // 转化炉能量上限
        kConverterAccumulateEfficiency = 213,   // 转化炉能量积累效率
        kConverterCostEfficiency = 214,   // 转化炉能量消耗效率
        kConverterValue = 215,   // 转化炉能量当前值
		kLightHeatCodingTime = 216,   // 热量冷却时间间隔
		kLightHeatCastSpeed = 217,   // 武器射速
		kWeaponAccuracy = 250,   // 准确度
		kWeaponStability = 251,   // 稳定性
		kRoleLevel = 301,   // 角色等级
        kWarShipLevel = 302,   // 战舰等级
        kDuanLevel = 303,   // 段位等级
        kPickupDistance = 320,   // 拾取范围
        kSpeedWalk = 401,   // 行走速度
        kSpeedRun = 402,   // 跑步速度
        kSpeedTurn = 403,   // 转身速度
        kSpeedJump = 404,   // 起跳速度
        kWatchpointY = 405,   // 观察点偏移Y
        kWatchpointX = 406,   // 观察点偏移X
        kCameraNear = 407,   // 摄像机近距离
        kCameraFar = 408,   // 摄像机远距离
        kAttack = 501,   // 攻击
        kDefend = 502,   // 防御
        kInvisible = 601,   // 隐身
        kAttributeNameMax = 10299
    }
}