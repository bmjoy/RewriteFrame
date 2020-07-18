/*===============================
 * Author: [Allen]
 * Purpose: 音效枚举
 * Time: 2019/5/18 11:48:39
================================*/

// 注：此处枚举 int 值对应 表 game_music 表对应的ID 值

/// <summary>
/// 音效bank库
/// </summary>
public enum WwiseSoundBank
{
    /// <summary>
    /// 背景音乐
    /// </summary>
    SoundBank_Music = 1,

    /// <summary>
    /// 所有事件（初始加载）
    /// </summary>
    SoundBank_int_EntireEvents = 2,

    /// <summary>
    /// 所有语音事件（初始加载）
    /// </summary>
    SoundBank_int_VoxEvents = 3,

    /// <summary>
    /// UI以及HUD
    /// </summary>
    SoundBank_SFX_UI = 4,

    /// <summary>
    ///2D环境(InDoor)
    /// </summary>
    SoundBank_Str2D_BG_InD = 5,

    /// <summary>
    /// 2D氛围（DeepSpace）
    /// </summary>
    SoundBank_Str2D_Atmos_DsP = 6,

    /// <summary>
    /// 语音
    /// </summary>
    SoundBank_Vox = 7,

}

/// <summary>
/// 指定音效
/// </summary>
public enum WwiseMusic
{
    #region 背景音乐

    /// <summary>
    /// 主要的背景音乐
    /// </summary>
    Music_MainBackgroundMusic = 1,

    /// <summary>
    /// 登陆游戏音乐
    /// </summary>
    Music_BGM_LoginGame = 1000,

    /// <summary>
    /// 创角完毕，进入游戏音乐
    /// </summary>
    Music_BGM_ToEnterGame = 1001,
    #endregion

    #region  面板
    /// <summary>
    /// UI-面板-手表界面开启
    /// </summary>
    Music_Panel_Watch_Open = 2,

    /// <summary>
    ///  UI-面板-手表界面关闭
    /// </summary>
    Music_Panel_Watch_Close = 3,

    /// <summary>
    /// UI-面板-手表界面按钮选中/切换
    /// </summary>
    Music_Panel_Watch_ButtonChange = 4,

    /// <summary>
    /// UI-面板-创角界面开启
    /// </summary>
    Music_Panel_CreatRole_Open = 5,

    /// <summary>
    /// UI-面板-创角界面关闭
    /// </summary>
    Music_Panel_CreatRole_Close = 6,

    /// <summary>
    /// UI-面板-NPC入口界面通用开启
    /// </summary>
    Music_Panel_NpcPanel_Open = 7,

    /// <summary>
    /// UI-面板-NPC入口界面通用关闭
    /// </summary>
    Music_Panel_NpcPanel_Close = 8,

    /// <summary>
    ///UI-面板-弹出面板开启（删除人物、角色取名）
    /// </summary>
    Music_Panel_PopupPanel_Open = 9,

    /// <summary>
    /// UI-面板-弹出面板关闭（删除人物、角色取名）
    /// </summary>
    Music_Panel_PopupPanel_Close = 10,

    /// <summary>
    ///UI-面板-任务界面开启
    /// </summary>
    Music_Panel_TaskPanel_Open = 11,

    /// <summary>
    /// UI-面板-任务界面关闭
    /// </summary>
    Music_Panel_TaskPanel_Close = 12,

    /// <summary>
    ///UI-面板-战舰被毁选择界面
    /// </summary>
    Music_Panel_BattleShipDestroyed = 13,

    /// <summary>
    ///UI-面板-二级界面通用开启
    /// </summary>
    Music_Panel_secondarylevelPanel_Open = 14,

    /// <summary>
    ///UI-面板-二级界面通用关闭
    /// </summary>
    Music_Panel_secondarylevelPanel_Close = 15,

    #endregion

    #region  按钮

    /// <summary>
    ///UI-点击-聚合按钮、容器上方标签、容器左侧标签通用
    /// </summary>
    Music_Button_Click_1 = 16,

    /// <summary>
    ///UI-点击-容器列表按钮通用
    /// </summary>
    Music_Button_Click_2 = 17,

    /// <summary>
    ///UI-光标-光标有效移动
    /// </summary>
    Music_Button_SelectMove_valid = 18,

    /// <summary>
    ///UI-光标-光标无效 or 越界 移动
    /// </summary>
    Music_Button_SelectMove_invalid = 19,

    /// <summary>
    ///UI-热键-点击&持续开始(loop)
    /// </summary>
    Music_Hotkey_Down_loop = 20,

    /// <summary>
    ///UI-热键-中途取消- 结束
    /// </summary>
    Music_Hotkey_Up = 21,

    /// <summary>
    ///UI-热键-点击&持续开始（one shot）
    /// </summary>
    Music_Hotkey_Down_oneShot = 54,

    #endregion

    #region  特殊UI 操作

    /// <summary>
    /// UI-热键-（分解）结束
    /// </summary>
    Music_Resolve_Over = 22,

    /// <summary>
    ///UI-热键-（社交解除）结束
    /// </summary>
    Music_SocialLift_Over = 23,

    /// <summary>
    ///UI-热键-（删除信息）结束
    /// </summary>
    Music_deleteMessages_Over = 24,

    /// <summary>
    ///UI-热键-（删除人物）结束
    /// </summary>
    Music_deleteRole_Over = 25,

    /// <summary>
    ///UI-热键-切换对比目标（战舰装配&背包）
    /// </summary>
    Music_changeCompare = 26,

    /// <summary>
    ///UI-热键-点击&持续开始（生产界面）
    /// </summary>
    Music_Production_began = 27,

    /// <summary>
    ///UI-热键-中途取消（生产界面）
    /// </summary>
    Music_Production_ToCancel = 28,

    /// <summary>
    ///UI-热键-结束（生产界面）
    /// </summary>
    Music_Production_end = 29,

    /// <summary>
    /// UI-特殊点击-安装武器
    /// </summary>
    Music_WeaponParts_Setup = 30,

    /// <summary>
    /// UI-特殊点击-卸下武器
    /// </summary>
    Music_WeaponParts_Disboard = 31,

    /// <summary>
    /// UI-特殊点击-安装转化炉
    /// </summary>
    Music_reborner_Setup = 32,

    /// <summary>
    ///UI-特殊点击-卸下转化炉
    /// </summary>
    Music_reborner_Disboard = 33,

    /// <summary>
    ///UI-特殊点击-安装反应堆
    /// </summary>
    Music_reactor_Setup = 34,

    /// <summary>
    ///UI-特殊点击-卸下反应堆
    /// </summary>
    Music_reactor_Disboard = 35,

    /// <summary>
    ///UI-特殊点击-安装纳米机器人
    /// </summary>
    Music_robot_Setup = 36,

    /// <summary>
    ///UI-特殊点击-卸下纳米机器人
    /// </summary>
    Music_robot_Disboard = 37,

    /// <summary>
    ///UI-特殊点击-安装装甲涂层
    /// </summary>
    Music_ArmorCoating_Setup = 38,

    /// <summary>
    ///UI-特殊点击-卸下装甲涂层
    /// </summary>
    Music_ArmorCoating_Disboard = 39,

    /// <summary>
    ///UI-特殊点击-安装辅助单元
    /// </summary>
    Music_auxiliary_Setup = 40,

    /// <summary>
    ///UI-特殊点击-卸下辅助单元
    /// </summary>
    Music_auxiliary_Disboard = 41,

    /// <summary>
    ///UI-特殊点击-安装处理器
    /// </summary>
    Music_processor_Setup = 42,

    /// <summary>
    ///UI-特殊点击-卸下处理器
    /// </summary>
    Music_processor_Disboard = 43,

    /// <summary>
    ///UI-特殊点击-安装放大器
    /// </summary>
    Music_amplifier_Setup = 44,

    /// <summary>
    ///UI-特殊点击-卸下放大器
    /// </summary>
    Music_amplifier_Disboard = 45,

    /// <summary>
    ///UI-特殊点击-安装芯片
    /// </summary>
    Music_chip_Setup = 46,

    /// <summary>
    ///UI-特殊点击-卸下芯片
    /// </summary>
    Music_chip_Disboard = 47,

    /// <summary>
    ///UI-特殊点击-开启对比（装配/背包/生产）
    /// </summary>
    Music_Compare_Open = 48,

    /// <summary>
    ///UI-特殊点击-关闭对比（装配/背包/生产）
    /// </summary>
    Music_Compare_Close = 49,

    #endregion

    #region     特殊事件

    /// <summary>
    /// UI-事件-创角&选角界面进入游戏
    /// </summary>
    Music_CreateRoleToGame = 50,

    /// <summary>
    ///UI-事件-死亡倒计时（毫秒）
    /// </summary>
    Music_CountDownTime_1 = 51,

    /// <summary>
    ///UI-事件-死亡倒计时（秒）
    /// </summary>
    Music_CountDownTime_2 = 52,

    /// <summary>
    ///UI-事件-停止死亡倒计时（毫秒）
    /// </summary>
    Music_CountDownTime_over = 53,

    /// <summary>
    ///UI-事件-以战舰形态复活按钮
    /// </summary>
    Music_Ship_rebirth_button = 55,

    /// <summary>
    ///UI-事件-以人形态复活按钮
    /// </summary>
    Music_human_rebirth_button = 56,

    #endregion

    #region  Hud 音效
    /// <summary>
    ///  HUD-战舰状态战斗中间屏幕提示语出现
    /// </summary>
    Music_Hud_Reminder_Open = 301,

    /// <summary>
    /// HUD-战舰状态战斗中间屏幕提示语消失
    /// </summary>
    Music_Hud_Reminder_Close = 302,

    /// <summary>
    /// HUD-战舰状态武器栏弹出
    /// </summary>
    Music_Hud_WeaponsColumn_Open = 303,

    /// <summary>
    ///HUD-战舰状态武器栏收起
    /// </summary>
    Music_Hud_WeaponsColumn_Close = 304,

    /// <summary>
    /// HUD-战舰状态技能键/切换
    /// </summary>
    Music_Hud_SkillKey_Switch = 305,

    /// <summary>
    /// HUD-战舰状态武器键/切换
    /// </summary>
    Music_Hud_WeaponKey_Switch = 306,

    /// <summary>
    /// HUD-附近发现相关资源开启--通用
    /// </summary>
    Music_Hud_Resource_Open = 307,

    /// <summary>
    /// HUD-附近发现相关资源关闭--通用
    /// </summary>
    Music_Hud_Resource_Close = 308,

    #endregion

    #region  临时 特殊 ID
    /// <summary>
    /// npc死亡开始
    /// </summary>
    Music_Npc_Dead_Begin = 8001,

    /// <summary>
    /// npc死亡结束
    /// </summary>
    Music_Npc_Dead_End = 8002,

    /// <summary>
    /// 怪巢Boss-爆炸
    /// </summary>
    Weird_Boss_Dead_End = 8020,

	/// <summary>
	/// 掉落物音效
	/// </summary>
	DropItem_Bron = 10010,

	/// <summary>
	/// 干扰屏幕特效
	/// </summary>
	Interference_FX_Sound = 9001,

	/// <summary>
	/// 进入宝藏区域的特效音效
	/// </summary>
	InTreasure_FX_Sound = 9004,

	/// <summary>
	/// 进入宝藏阶段掉落音效
	/// </summary>
	Treasure_Drop_FX_Sound = 10010,

	/// <summary>
	/// 干扰器连线音效
	/// </summary>
	Treasure_Disturbor_Sound1 = 10015,	/// 寻宝-干扰器禁锢能量（6个都未被摧毁）
	Treasure_Disturbor_Sound2 = 10016,	/// 寻宝-干扰器禁锢能量（还剩 4 & 5 个）
	Treasure_Disturbor_Sound3 = 10017,	/// 寻宝-干扰器禁锢能量（还剩 2 & 3 个）
	Treasure_Disturbor_Sound4 = 10018,	/// 寻宝-干扰器禁锢能量（还剩 1 个）
	Treasure_Disturbor_Sound5 = 10019	/// 寻宝-停止干扰器禁锢能量
	#endregion
}

/// <summary>
/// 音效参数
/// </summary>
public enum WwiseRtpc
{
    /// <summary>
    /// 主音量控制
    /// </summary>
    Rtpc_MasterVolume = 1,

    /// <summary>
    /// 音乐音量控制
    /// </summary>
    Rtpc_MusicVolume = 2,

    /// <summary>
    /// 音效量控制
    /// </summary>
    Rtpc_SFXVolume = 3,

    /// <summary>
    /// 语音音量控制
    /// </summary>
    Rtpc_VoiceVolume = 4,

    /// <summary>
    /// 船形态尾焰速度
    /// </summary>
    Rtpc_ShipVelocity = 5,

    /// <summary>
    /// 船形态尾焰速度
    /// </summary>
    Rtpc_ShipRotation = 6,

    /// <summary>
    /// 热键长按
    /// </summary>
    Rtpc_UI_Hotkey = 7,

    /// <summary>
    /// NPC交互界面
    /// </summary>
    Rtpc_UI_Inf_NPC = 8,
}

/// <summary>
/// 特殊功能枚举
/// 跟组合共同使用
/// </summary>
public enum WwiseMusicSpecialType
{
    #region 角色装备材质
    SpecialType_RoleEquipMaterial_1 = 1,     //角色衣饰材质（甲A）
    SpecialType_RoleEquipMaterial_2 = 2,     //角色衣饰材质（布A）
    SpecialType_RoleEquipMaterial_3 = 3,     //角色衣饰材质（布B）
    SpecialType_RoleEquipMaterial_4 = 4,     //角色衣饰材质（布C）
    SpecialType_RoleEquipMaterial_5 = 5,     //角色衣饰材质（布D）
    SpecialType_RoleEquipMaterial_6 = 6,     //角色衣饰材质（皮A）
    SpecialType_RoleEquipMaterial_7 = 7,     //角色衣饰材质（皮B）
    SpecialType_RoleEquipMaterial_8 = 8,     //角色衣饰材质（皮C）
    SpecialType_RoleEquipMaterial_9 = 9,     //角色衣饰材质（皮D）
    SpecialType_RoleEquipMaterial_10 = 10,     //角色衣饰材质（皮E）

    #endregion

    #region 角色脚步材质
    SpecialType_RoleFootStop = 100,                //角色停止脚步
    SpecialType_RoleFootMaterial_1 = 101,     //脚步材质：男性/女性  地毯
    SpecialType_RoleFootMaterial_2 = 102,     //脚步材质：男性/女性  玻璃
    SpecialType_RoleFootMaterial_3 = 103,     //脚步材质：男性/女性  大理石
    SpecialType_RoleFootMaterial_4 = 104,     // 脚步材质：男性/女性  金属
    #endregion

    #region 战舰引擎
    SpecialType_WarShipEngine_1 = 201,     //战舰引擎功率（尾焰喷射
    SpecialType_WarShipEngine_2 = 202,     //战舰引擎功率（侧边喷射
    SpecialType_WarShipEngine_3 = 203,     // 战舰引擎功率（停止侧边喷射
    #endregion


    #region  战舰跃迁复活 
    SpecialType_JumpPreparationBegin = 300,               //玩家跃迁准备开始
    SpecialType_JumpPreparationCancel = 301,             //玩家跃迁准备取消
    SpecialType_JumpBegin = 302,                                    //玩家跃迁开始
    SpecialType_JumpEnd = 303,                                       //玩家跃迁结束
    SpecialType_Die_Begin = 304,                                    //玩家死亡开始
    SpecialType_Die_End = 305,                                      //玩家死亡结束
    SpecialType_Rebirth = 306,                                         //玩家复活
    #endregion

    #region  语音部分
    SpecialType_Voice_Enter_Battle = 401,							//系统语音：战舰进入战斗状态
    SpecialType_Voice_Enter_Cruise = 402,							//系统语音：战舰进入巡航状态
    SpecialType_Voice_Unable_Switch_State = 403,					//系统语音：战舰正处于开火倒计时（无法切换状态）
    SpecialType_Voice_Attack_Intensified = 404,						//系统语音：战舰进入过载状态 - 攻击被强化
    SpecialType_Voice_Defense_Intensified = 405,					//系统语音：战舰进入过载状态 - 防御被强化
    SpecialType_Voice_Speed_Intensified = 406,						//系统语音：战舰进入过载状态 - 速度被强化
    SpecialType_Voice_Power_Failure = 407,							//系统语音：电力不足（过载状态结束）
    SpecialType_Voice_ConverterPower_Full = 408,					//系统语音：转化炉能量全满
    SpecialType_Voice_Converter_Explosion = 409,					//系统语音：转化炉爆发
    SpecialType_Voice_Converter_Explosion_StateOver = 410,			//系统语音：转化炉爆发状态结束
    SpecialType_Voice_Hp_Low = 411,									//系统语音：战舰生命低于一定百分比
    SpecialType_Voice_Shield_Low = 412,								//系统语音：战舰护盾低于一定百分比
    SpecialType_Voice_Ship_Destroyed = 413,							//系统语音：战舰被击毁
    SpecialType_Voice_Ship_Reborn = 414,							//系统语音：战舰已重生
    SpecialType_Voice_Start_Transition = 415,						//系统语音：跃迁引擎启动（开始准备跃迁）
    SpecialType_Voice_Off_Transition = 416,							//系统语音：跃迁中止（主动或被动）
    SpecialType_Voice_Arrive_Transition = 417,						//系统语音：抵达跃迁终点（跃迁引擎关闭）
    SpecialType_Voice_From_DeepSpaceToSpaceStation = 418,			//系统语音：从深空进入空间站
    SpecialType_Voice_From_SpaceStationToDeepSpace = 419,			//系统语音：从空间站进入深空
    SpecialType_Voice_GetTheTask = 420,								//系统语音：领取任务
    SpecialType_Voice_TaskTargetUpdate = 421,						//系统语音：任务目标更新
    SpecialType_Voice_FinishTheTask = 422,							//系统语音：完成任务
    SpecialType_Voice_StartCrossingTheGate = 423,					//系统语音：开始穿越星门
    SpecialType_Voice_EndCrossingTheGate = 424,						//系统语音：穿越星门结束
    SpecialType_Voice_Be_Attacked = 425,                            //系统语音：被攻击

    //导演区
    SpecialType_Voice_director_monster_killed = 426,                            //怪巢导演区：小怪被击杀随机语音
    SpecialType_Voice_director_monster_Boss1 = 427,                           //怪巢导演区：怪巢区域（通讯被入侵/警告）-BOSS发声
    SpecialType_Voice_director_monster_Boss2 = 428,                            //怪巢导演区：全歼小怪后-BOSS发声
    SpecialType_Voice_director_monster_EnterArea = 429,                    //怪巢导演区（语音助手）：进入区域时触发
    SpecialType_Voice_director_monster_retreat = 430,                          //怪巢导演区（语音助手）：小怪撤退时触发
    SpecialType_Voice_director_monster_Boss3 = 431,                            //怪巢导演区（语音助手）：进入BOSS战时触发
    SpecialType_Voice_director_monster_countDownTime = 432,         //怪巢导演区（语音助手）：进入10秒倒计时时触发
    SpecialType_Voice_director_monster_bomb = 433,                            //怪巢导演区（语音助手）：倒计时爆炸后触发（玩家没死）
    SpecialType_Voice_director_monster_NearNest1= 434,                     //怪巢导演区（语音助手）：靠近怪巢（第一次警告）
    SpecialType_Voice_director_monster_NearNest2 = 435,                    //怪巢导演区（语音助手）：靠近怪巢（第二次警告）
    SpecialType_Voice_director_monster_NearNest3 = 436,                    //怪巢导演区（语音助手）：靠近怪巢（第三次警告）

	/// <summary>
	/// 采集&寻宝
	/// </summary>
	SpecialType_Voice_minera_event1 = 437,                    //系统语音：附近发现矿物资源
	SpecialType_Voice_minera_event2 = 438,                    //系统语音：目标矿物资源已枯竭
	SpecialType_Voice_minera_event3 = 439,                    //系统语音：稀有矿物资源出现
	SpecialType_Voice_minera_event4 = 440,                    //系统语音：矿石资源待收集
	SpecialType_Voice_treasure_event1 = 441,                    //系统语音：已进入宝藏区域
	SpecialType_Voice_treasure_event2 = 442,                    //系统语音：宝藏已出现
	SpecialType_Voice_treasure_event3 = 443,                    //系统语音：宝藏已被回收

	SpecialType_Voice_minera_weak = 444,                       //系统语音：采矿攻击弱点时语音表现

	SpecialType_Voice_treasure_event4 = 445,                    //系统语音：提示切回巡航状态，开启带锁宝箱
	SpecialType_Voice_treasure_event5 = 446,                    //领航员：宝藏干扰器出现后，提示玩家尝试攻击
	SpecialType_Voice_treasure_event6 = 447,                    //领航员：宝藏从透明开始渐变出来的开始的时候
	SpecialType_Voice_treasure_event7 = 448,                    //领航员：刷出或者掉落常规宝箱时
	SpecialType_Voice_treasure_event8 = 449,                    //领航员：刷出或者掉落带锁宝箱时
	SpecialType_Voice_treasure_event9 = 450,                    //领航员：刷出宝藏守卫时
	SpecialType_Voice_treasure_event10 = 451,                    //领航员：干掉宝藏守卫时
	SpecialType_Voice_treasure_event11 = 452,                    //领航员：刷出精英宝藏守卫时
	SpecialType_Voice_treasure_event12 = 453,                    //领航员：干掉精英宝藏守卫时

	#endregion

}

/// <summary>
/// 主角or其他
/// 玩家跟主角音效不同,目前只有船有需求
/// </summary>
public enum WwiseMusicPalce
{
    Palce_1st = 1,     //第一人称
    Palce_3st = 3,     //第三人称
}
