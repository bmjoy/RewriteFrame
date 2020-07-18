using Assets.Scripts.Define;
using Eternity.FlatBuffer;
using Eternity.Runtime.Item;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static BattleWeapon_Missile;
using static SpacecraftEntity;
using SystemObject = System.Object;

public enum NotificationName
{

	MSG_MAP_NPC_CHANGED,

	StartupPreload,
	StartupInitialize,
	StartupPreloadComplete,

	ChatMessageChanged,
	ChatTalkToFriend,
	ChatInsertItemLink,

	PlayerExpChanged,
	PlayerLevelChanged,
	PlayerShipExpChanged,
	PlayerShipLevelChanged,
	PlayerDanExpChanged,
	PlayerWeaponToggleBegin,
	PlayerWeaponToggleEnd,
	PlayerWeaponPowerChanged,
    PlayerWeaponFireFail,
    /// <summary>
    /// 武器射击, 通知UI本次射击弹道的屏幕坐标
    /// </summary>
    PlayerWeaponShot,
	/// <summary>
	/// 武器准星扩散变化, 通知UI准星的屏幕占比
	/// </summary>
	PlayerWeaponCrosshairScale,

	/// <summary>
	/// 武器当前选中的目标. 当前仅用于导弹武器
	/// </summary>
	PlayerMissileWeaponTargetSelection,

    /// <summary>
    /// 散弹枪区域情况通知Hud 
    /// </summary>
    PlayerWeaponCrosshairScale_ShotGun,


    /// <summary>
    /// 根据当前武器的特性, 有可攻击目标被准星拾取到了
    /// </summary>
    PlayerWeaponSelectedTarget,

	PlayerPoseChanged,

	PlayerPressSkillKey,
	PlayerSkillRequest,
	PlayerSkillReleased,

	MainHeroDeath,
	MainHeroRevive,


	MSG_SMITH_UPGRADE_BACK,

	ItemGetting,

	TeamErrorInfo,

	MSG_INPUT_TO_UI,
	MSG_INPUT_TO_CHARACTER,
	MSG_SUBMIT_MISSION,
	MSG_DROP_MISSION,
	MSG_PK_MODE_CHANGED,
	MSG_TOGGLE_HUD_TAG,
	MSG_INPUT_TO_SPACECRAFT,

	/// <summary>
	/// 技能伤害
	/// </summary>
	SkillHurt,
	/// <summary>
	/// BUFF伤害
	/// </summary>
	BuffHurt,
	/// <summary>
	/// 伤害免疫
	/// </summary>
	HurtImmuno,

	MSG_HUD_NOTICE,
	MSG_CAMERA_RESET,
	MSG_INPUT_DISABLE,
	/// <summary>
	/// HUD模式改变
	/// </summary>
	MSG_HUD_MODE_CHANGED,

	/// <summary>
	/// HUD上显示大事件提示
	/// </summary>
	MSG_HUD_BIG_EVENT,

	/// <summary>
	/// HUD上显示任务完成提示
	/// </summary>
	MSG_HUD_FINISH_MISSION,

	/// <summary>
	/// 切换场景结束
	/// </summary>
	MSG_SWITCH_SCENE_END,

	/// <summary>
	/// 飞船切换武器完成
	/// </summary>
	MSG_SKILL_TOGGLE_WEAPON_END,

	/// <summary>
	/// 飞船武器能量变化
	/// </summary>
	MSG_CHARACTER_WEAPON_POWER_CHANGED,

	/// <summary>
	/// 飞船的技能发生变化
	/// </summary>
	MSG_SHIP_SKILL_CHANGED,
    /// <summary>
    /// 飞船的武器技能CD发生变化
    /// </summary>
    MSG_SHIP_SKILL_CD_CHANGED,

    /// <summary>
    /// 切换武器键按下
    /// </summary>
    MSG_SKILL_KEY,

	/// <summary>
	/// 技能1键按下
	/// </summary>
	MSG_SKILL_KEY_1,

	/// <summary>
	/// 技能2键按下
	/// </summary>
	MSG_SKILL_KEY_2,

	/// <summary>
	/// 技能3键按下
	/// </summary>
	MSG_SKILL_KEY_3,

	/// <summary>
	/// 技能4键按下
	/// </summary>
	MSG_SKILL_KEY_4,

	/// <summary>
	/// 成功击杀敌方
	/// </summary>
	EntityDeath,
	/// <summary>
	/// 通讯窗变化
	/// </summary>
	VideoPhoneChange,

	Voice,
	/// <summary>
	/// 切换区域
	/// </summary>
	ChangeArea,


	MSG_FRIEND_STATUS_CHANGED,
	MSG_FRIEND_INVITE_LIST_CHANGED,
	MSG_PVE_SCHEDULE_UPDATE,
	MSG_PVE_SCHEDULE_CHANGE,
	MSG_PVE_SCHEDULE_ADD,
	MSG_SCENE_EVENT_CHANGED,

	MSG_PVE_Animator_Victory,
	MSG_PVE_Animator_defeat,
	MSG_PVE_Settlement_InjuryStatistics,

	MSG_AIRCRAFT_LEAP_IN_SCENE_READY,
	MSG_LEAP_COMPLETED,
    LeapNavigationBegin,
    LeapNavigationEnd,

    /// <summary>
    /// 天赋信息改变
    /// </summary>
    MSG_TALENT_CHANGEINFOS,
    /// <summary>
    /// 天赋操作
    /// </summary>
    MSG_TALENT_OPERATION,
    /// <summary>
    /// 武器tips对比
    /// </summary>
    MSG_TIPSWEAPON_COMPAER,
	/// <summary>
	/// 武器tips取消对比
	/// </summary>
	MSG_TIPSWEAPON_CANCELCOMPAER,
	/// <summary>
	/// 更新武器Tips面板
	/// </summary>
	MSG_UPDATEWEAPONTIPS_VIEW,
	/// <summary>
	/// 打开NPC交互提示
	/// </summary>
	MSG_NPC_TALK_TIP_OPEN,

	/// <summary>
	/// 关闭NPC交互提示
	/// </summary>
	MSG_NPC_TALK_TIP_CLOSE,
	/// <summary>
	/// NPC交互中
	/// </summary>
	MSG_ACTIVE_NPC_INTERACTIVING,
	/// <summary>
	/// NPC交互取消
	/// </summary>
	MSG_ACTIVE_NPC_CANCEL_INTERACTIVE,

	/// </summary>
	MSG_SWITCH_SCENE,

	/// </summary>
	MSG_REMOVE_SCENE_ENTITY,

	/// </summary>




	/// <summary>
	/// 任务追踪状态改变时
	/// </summary>
	MissionTrackStateChanged,

	/// <summary>
	/// 钱币改变
	/// </summary>
	MSG_PLAYER_MONEY_CHANGE,
	/// <summary>
	/// 背包报错
	/// </summary>
	MSG_PACK_ERROR,
	/// <summary>
	/// 邮件列表渲染
	/// </summary>
	MSG_EMAIL_RENDERLIST,
	/// <summary>
	/// 邮件渲染内容
	/// </summary>
	MSG_EMAIL_RENDERCONTENT,
	/// <summary>
	/// 郵件列表改变
	/// </summary>
	MSG_EMAIL_RENDERLISTONEICON,
	/// <summary>
	/// 删除邮件
	/// </summary>
	MSG_EMAIL_DELETE,
	/// <summary>
	/// 领取附件时背包格子不足
	/// </summary>
	MSG_PACKAGE_NOTENOUGH,
	/// <summary>
	/// 日志列表渲染
	/// </summary>
	MSG_LOG_RENDERLIST,
	/// <summary>
	/// 好友列表改变
	/// </summary>
	MSG_FRIEND_LIST_CHANGED,
	/// <summary>
	/// 组队成员改变
	/// </summary>
	MSG_TEAM_MEMBER_UPDATE,
    /// <summary>
    /// 组队成员改变区域
    /// </summary>
    MSG_TEAM_MEMBER_UPDATEAREA,
    /// <summary>
    /// 组队战斗信息改变
    /// </summary>
    MSG_TEAM_BATTLE_UPDATE,
	/// <summary>
	/// 组队成员添加
	/// </summary>
	MSG_TEAM_MEMBER_ADDED,
	/// <summary>
	/// 队友信息（船）改变
	/// </summary>
	MSG_FRIEND_SHIPDATA_CHANGE,
	/// <summary>
	/// 组队成员离开
	/// </summary>
	MSG_TEAM_MEMBER_LEAVE,
	CMD_TEAM_INVITE_REPLY,
	CMD_TEAM_INVITE,
	MSG_TEAM_INVITE_REPLY,
	/// <summary>
	/// 场景中玩家改变
	/// </summary>
	MSG_PLAYER_CHANGED,
	#region CharacterList
	/// <summary>
	/// 从服务器获得到角色列表
	/// </summary>
	MSG_CHARACTER_LIST_GETED,
	MSG_LOGINPANEL_ACTIVE,
	#endregion

	#region CreateCharacter
	MSG_CHARACTER_ROLLNAME_SUCC,
	MSG_CHARACTER_ROLLNAME_FAIL,
	MSG_CHARACTER_CREATE_FAIL,
	MSG_CHARACTER_CREATE_SUCCESS,
	MSG_CHARACTER_DEL_SUCC,
	MSG_CHARACTER_DEL_FAIL,
	#endregion

	MSG_CHILDPANEL_HOTKEY,
	MSG_CLOSE_MAIN_PANEL,
	MSG_CHILD_TIPS_CHANGE,
	MSG_MESSAGE_BOSS_SHOW,

	#region Socket
	SocketConnectStart,
	SocketConnectRestart,
	SocketConnected,
	SOCKETCONNECTFAIL,
	SocketConnectBreak,


	MSG_SOCKET_CONN_START,
	MSG_SOCKET_CONN_SUCCESS,
	MSG_SOCKET_CONN_FAIL,
	MSG_SOCKET_CONN_BREAK,
	#endregion
	MSG_SWITCH_SCENE_START,
	MSG_CAMERA_FREE,

	/// <summary>
	/// 退出游戏
	/// </summary>
	MSG_QUIT,
	MSG_LOGINWAITSHOW,
	#region 生产相关
	MsgProduthotkeyUpdate,
	MSG_FOUNDRY_UPDATE,
	MSG_FOUNDRY_BUILD,
	MSG_FOUNDRY_GET_INFO,
	MSG_FOUNDRY_CANCEL,
	MSG_FOUNDRY_SPEED,
	MSG_FOUNDRY_RECEIVE,
	MSG_PRODUT_UPDATE,
    MSG_PRODUCE_ORDER,
    MSG_PRODUCE_ORDE_RRETRIEVE,
    MSG_PRODUCE_ORDE_SHOW,
    #endregion

    #region NewItem
    MSG_PACKAGE_ITEM_ADD,
	MSG_PACKAGE_ITEM_DESTORY,
	MSG_PACKAGE_ITEM_CONSUME,
	MSG_PACKAGE_ITEM_MOVE,

	MSG_PACKAGE_ITEMSYNC_BEGIN,
	MSG_PACKAGE_ITEMSYNC_END,
	MSG_PACKAGE_ITEM_CHANGE,
	#endregion

	#region 货币
	MSG_CURRENCY_CHANGED,
	#endregion

	#region 战舰数据变化

	MSG_SHIP_DATA_CHANGED,

	#endregion
	/// <summary>
	/// 开宝箱收到回复
	/// </summary>
	MSG_S2C_OPEN_CEHST_BY_KEY_RESULT,
	MSG_CLIENT_MAP_NPC_CHANGED,

	/// <summary>
	/// 打开确认框
	/// </summary>
	MSG_CONFIRM_OPEN,

	/// <summary>
	/// 组队平台--取消匹配
	/// </summary>
	MSG_TEAM_PLATFORM_CancelMatching,

	/// <summary>
	/// 组队平台--所有成员都接受了邀请
	/// </summary>
	MSG_TEAM_PLATFORM_Allaccept,

	/// <summary>
	/// 组队平台--开启准备倒计时
	/// </summary>
	MSG_TEAM_PLATFORM_ReadyCountDowntime,

	/// <summary>
	/// 血量变化
	/// </summary>
	MSG_HP_CHANGED,

    /// <summary>
    /// 商店交易
    /// </summary>
    MSG_SHOP_EXCHANGE,
    /// <summary>
    /// 商店数据变化
    /// </summary>
    MSG_SHOP_CHANGE,

	#region playerController 用到的
	MSG_PLAYER_PROP_UPDATE,
	MSG_PLAYER_MODULE_UPDATE,
	MSG_PLAYER_MODULE_ADD,
	MSG_PLAYER_SHIP_UPDATE,
	MSG_PLAYER_SHIP_ADD,
	MSG_PLAYER_MODULE_DEL,
	MSG_PLAYER_SHIP_DEL,
	MSG_PLAYER_POWER_CHOOSE,
	MSG_PLAYER_SHIP_USE,
	#endregion

	MSG_MISSION_EXPLORE,
	MSG_MISSION_DIALOG,
	MSG_FRIEND_DEL_BLACK_REQ,
	MSG_GRPC_SERVERLIST_BACK,
	MSG_GRPC_SERVERLIST_ERR,
	MSG_GRPC_AUTH_BACK,
	MSG_GRPC_AUTH_ERR,
	MSG_GRPC_GETROLES_BACK,
	MSG_GRPC_GETROLES_ERR,

	MSG_CHARACTER_CREATE_GENDER_CHANGED,
	MSG_CHARACTER_CREATE_SKIN_CHANGED,
	MSG_CHARACTER_CREATE_STATE_CHANGE,
	MSG_CHARACTER_CREATE_CURRENT_CHARACTERVO_CHANGE,
    MSG_CHARACTER_MODEL_ROTATE,

    /// <summary>
    /// ESC动画播放结束
    /// </summary>
    MSG_ESC_ANIMATOR_END,
    #region     ////////////////////////////////////////////////////////////////////////tips 相关/////////////////////////////////////////////////////////////////////////////////////////////////////

    /// <summary>
    ///tips对比
    /// </summary>
    MSG_TIPS_COMPAER_OPEN,

	/// <summary>
	///tips取消对比
	/// </summary>
	MSG_TIPS_COMPAER_CANCEL,

	/// <summary>
	/// tips 对比内容改变了
	/// </summary>
	MSG_TIPS_COMPAER_CONTENT_CHANGE,

	/// <summary>
	/// tips 对比切换
	/// </summary>
	MSG_TIPS_COMPAER_CONTENT_SWITCH,

	/// <summary>
	/// 好友tips 内容改变了
	/// </summary>
	MSG_TIPS_FRIEND_CONTENT_CHANGE,
	/// <summary>
	/// 功能说明tips  查看内容改变
	/// </summary>
	MSG_LabelTipsContentChange,

	/// <summary>
	/// 钱币tips  查看内容改变
	/// </summary>
	MSG_CoinTipsContentChange,

	/// <summary>
	/// 开格子tips  查看内容改变
	/// </summary>
	MSG_FunctionBoxTipsContentChange,


	/// <summary>
	/// 选角/创角，查看索引改变
	/// </summary>
	MSG_CharacterTipsIndeChange,

	/// <summary>
	/// 邮件tips，查看索引改变
	/// </summary>
	MSG_MailTipsIDChange,

	/// <summary>
	///战舰MOD入口tips，查看改变
	/// </summary>
	MSG_FunctionModTipsContentChange,

	/// <summary>
	///材料tips，查看改变
	/// </summary>
	MSG_MaterialTipsContentChange,

	/// <summary>
	/// 零件内容改变
	/// </summary>
	MSG_PartTipsContentChange,

	/// <summary>
	/// 船数据改变
	/// </summary>
	MSG_ShipTipsContentChange,

	/// <summary>
	/// 零件蓝图内容改变
	/// </summary>
	MSG_PartBlueTipsContentChange,

	/// <summary>
	/// Mod内容改变
	/// </summary>
	MSG_ModTipsContentChange,

	/// <summary>
	/// Mod蓝图内容改变
	/// </summary>
	MSG_ModBlueTipsContentChange,

	/// <summary>
	/// 任务tips，查看任务ID改变
	/// </summary>
	MSG_MissionTipsIDChange,

	/// <summary>
	/// 任务tips，任务数据发生了变化
	/// </summary>
	MSG_MissionTipsDataChange,

	#endregion end tips 相关


	#region  ////////////////////////////////////////////////////////////////音效 相关/////////////////////////////////////////////////////////////////////////////////////////////////////

	/// <summary>
	/// 设置Listener 的目标
	/// </summary>
	MSG_SOUND_SET_LISTENER_TARGET,

	/// <summary>
	/// 加载音效组合
	/// </summary>
	MSG_SOUND_LOAD_COMBO,

	/// <summary>
	/// 卸载 加载音效组合
	/// </summary>
	MSG_SOUND_UNLOAD_COMBO,

	/// <summary>
	/// 播放
	/// </summary>
	MSG_SOUND_PLAY,

	#endregion  end音效 相关



	MissionBigEvents,

	MSG_SKILL_RELEASE,
	MSG_SKILL_RESET_CARTRIDGE_BOX,

	MSG_TEAM_ERROR_INFO,


	/// <summary>
	/// 死亡复活
	/// </summary>
	MSG_REQUEST_RELIFE,
	/// <summary>
	/// 发送通用提示消息
	/// </summary>
	MSG_SENDCOMMONNOTICE,
	/// <summary>
	/// 向GameServer发送协议
	/// </summary>
	SendToGameServer,

	MSG_INTERACTIVE_SHOWFLAG,
	MSG_INTERACTIVE_HIDEFLAG,
	MSG_INTERACTIVE_SHOWTIP,
	MSG_INTERACTIVE_HIDETIP,
	MSG_INTERACTIVE_MISSIONESCORT,
	MSG_HUMAN_ENTITY_ON_ADDED,
	MSG_WARSHIP_PANEL_CHANGE,
	MSG_PACKAGE_ITEM_OPERATE,
	MSG_3DVIEWER_CHANGE,
	MSG_ENTITY_ON_REMOVE,

	MSG_SKILL_START_CHANTING,
	MSG_SKILL_STOP_CHANTING,
	MSG_SKILL_START_CHARGING,
	MSG_SKILL_STOP_CHARGING,
	MSG_SKILL_START_CHANNELLING,
	MSG_SKILL_STOP_CHANNELLING,

	MSG_CHARACTER_BUFF_ADD,
    MSG_DETECTOR_SHOW,

    MSG_HOLD_WATCH_KEY,

	/// <summary>
	/// 飞船形态, 改变战斗模式. 现在有巡航模式和战斗模式
	/// </summary>
	MSG_CHANGE_BATTLE_STATE,
    /// <summary>
    /// 切换战斗模式失败
    /// </summary>
    MSG_CHANGE_BATTLE_STATE_FAIL,

    /// <summary>
    /// 开启/结束过载模式
    /// </summary>
    MSG_ACTIVATE_OVERLOAD,

	/// <summary>
	/// 开启/关闭 转化炉
	/// </summary>
	MSG_ACTIVATE_BURST,

	MSG_DIALOGUE_SHOW,
	MSG_DIALOGUE_HIDE,
	MSG_DIALOGUE_COMPLETED,
    /// <summary>
    /// 进入6Dof模式
    /// </summary>
    Enter6DofMode,
    /// <summary>
    /// 退出6Dof模式
    /// </summary>
    Exit6DofMode,
    /// <summary>
    /// bufferIcon 改变
    /// </summary>
    MSG_BUFFERICON_CHANGE,
    /// <summary>
    /// 显示玩家复活界面
    /// </summary>
    MSG_SHOW_RELVIE_PANEL,
	/// <summary>
	/// 接受任务
	/// </summary>
	MSG_MISSION_ACCEPT,
	/// <summary>
	/// 完成任务
	/// </summary>
	MSG_MISSION_COMMIT,
	/// <summary>
	/// 放弃任务
	/// </summary>
	MSG_MISSION_ABANDON,
	/// <summary>
	/// 进度改变
	/// </summary>
	MSG_MISSION_CHANGE,
	/// <summary>
	/// 任务失败
	/// </summary>
	MSG_MISSION_FAIL,
	/// <summary>
	/// 组 或 节点 更新
	/// </summary>
	MSG_MISSION_STATE_CHANGE,

    /// <summary>
    /// 玩家等级提升
    /// </summary>
    MSG_PLAYER_LEVEL_UP,
    /// <summary>
    /// 玩家飞船的等级提升
    /// </summary>
    MSG_PLAYER_SHIP_LEVEL_UP,
    /// <summary>
    /// 玩家手表等级提升
    /// </summary>
    MSG_PLAYER_WATCH_LEVEL_UP,
    /// <summary>
    /// 玩家经验值提升
    /// </summary>
    MSG_PLAYER_WATCH_EXP_UP,
    /// <summary>
    /// 手表升级奖励
    /// </summary>
    MSG_LEVELUP_REWARD_LIST,
    AIPlotState_Change,


	/// <summary>
	/// 星图切换面板
	/// </summary>
	MSG_STARMAP_PANEL_CHANGE,


	#region ================================-技能==================================
	/// <summary>
	/// 显示技能进度条
	/// </summary>
	ShowSkillProgressBar,

    /// <summary>
    /// 显示技能进度条
    /// </summary>
    HidSkillProgressBar,

    #endregion ==============================end==-技能============================


    Total,

}

public class MsgOpenPanelBase
{
	public UIPanel m_PanelName;
}

public class MsgClosePanelBase
{
	public UIPanelBase.PanelType m_PanelType;
}

public class MsgChangeHUD
{
	public UIPanel[] m_HudPanels;
}

public class MsgMissionBigEvents
{
	public string MainTitle;
	public string SecondaryTitle;
	public MissionGenre MissionType;
	public int MissionQuality;
}

public class MsgShowFinishMission
{
	/// <summary>
	/// 标题
	/// </summary>
	public string m_Title;

	/// <summary>
	/// 图标的bundleName
	/// </summary>
	public string m_Bundle;

	/// <summary>
	/// 图标的文件名
	/// </summary>
	public string m_Name;
}

public class MsgNpcInteractive
{
	/// <summary>
	/// npc的ID
	/// </summary>
	public uint m_NpcID;
}

public class MsgMissionChange
{
	public NotificationName m_ChangeType;
	public int m_MissionId;
}

public class MsgChatChannelMessageChanged
{
	public ChatChannel m_channel;
}

public class MsgChatTalkToFriend
{
	public ulong m_ID;
	public string m_Name;
}

public class MsgChatInsertItemLink
{
	public int m_ItemType;
	public int m_ItemID;
	public string m_ItemName;
}

public class MsgPlayerExpChanged
{
	public double m_Exp;
}

public class MsgPlayerLevelChanged
{
	public bool m_IsSelf;
	public uint m_OldLevel;
	public uint m_NewLevel;
	public string m_PlayerName;
}

public class MsgShipLevelUp
{
    /// <summary>
    /// 等级
    /// </summary>
    public int m_level;
    /// <summary>
    /// tid
    /// </summary>
    public uint m_Tid;
}
public class MsgPlayerShipExpChanged
{
	public double m_Exp;
}

public class MsgPlayerShipLevelChanged
{
	public bool m_IsSelf;
	public uint m_OldLevel;
	public uint m_NewLevel;
	public string m_PlayerName;
}

public class MsgPlayerDanExpChanged
{
	public double m_Exp;
}

public class MsgItemGetting
{
	public string m_Name;
	public double m_Count;
	public string m_IconBundle;
	public string m_IconName;
}

public class MsgPlayerWeaponPowerChanged
{
	/// <summary>
	/// 武器在包里, 这个应该是武器在包里的格子的索引
	/// </summary>
	public int WeaponIndex;
	public int OldValue;
	public int NewValue;
}
public class MsgTeamErrorInfo
{
	public string m_Error;
}
public class MsgPlayerPressSkillKey
{
	public int KeyID;
}

public class MsgPlayerSkillCooldown
{
	public int SkillID;
}

public class MsgPlayerSkillRequest
{
	public int SkillID;
}

public class MsgMapId
{
	public int MapId;
}

public class MsgShipUID
{
	public ulong ShipUID;
}

public class MsgShipUIDWeaponType : MsgShipUID
{
	public int WeaponType;
}

public class MsgShipUIDWeaponTypeIndex : MsgShipUIDWeaponType
{
	public int Index;
}

public class MsgIndex
{
	public int Index;
}
public class MsgProduct
{
	public NotificationName msg;
	public int itemTID; //Tid

}

public class MsgError
{
	public int LanguageIndex;
}

public class MsgChildHotkey
{
	public string id;
	public string ActionName;
	public string Text;
	public Action<HotkeyCallback> Callback;
	public bool? IsHold;
	public float? HoldTime;
	public bool? IsRegister;
	public bool? IsShow;
	public bool? ChangeText;
}

public class MsgSkillHurt
{
	/// <summary>
	/// 目标ID
	/// </summary>
	public uint TargetID;
	/// <summary>
	/// 伤害量
	/// </summary>
	public int Damage;
	/// <summary>
	/// 暴击类型
	/// </summary>
	public int CritType;
	/// <summary>
	/// 穿透伤害
	/// </summary>
	public int PenetrationDamage;
	/// <summary>
	/// 是否闪避
	/// </summary>
	public bool IsDoge;
	/// <summary>
	/// 效果ID
	/// </summary>
	public int EffectID;
}

public class MsgBuffHurt
{
	/// <summary>
	/// 目标ID
	/// </summary>
	public uint TargetID;
	/// <summary>
	/// 效果类型
	/// </summary>
	public EffectType EffectType;
	/// <summary>
	/// 血值变化
	/// </summary>
	public int HpAmount;
}

public class MsgHurtImmuno
{
	public uint TargetID;
	public int Type;
	public int Value;
}

public class MsgCharacterPanelState
{
	public ServerListProxy.CharacterPanelState State;
}

public class MsgWarshipPanelState
{
	public WarshipPanelState BeforeState;
	public WarshipPanelState State;

	public IShip CurrentShip;
	public DataBase<IWeapon> CurrentWeaponData;
	public DataBase<IEquipment> CurrentEquipmentData;
	public DataBase<IReformer> CurrentReformerData;
	public ModData CurrentModData;

	public class ModData : DataBase<IMod>
	{
		public EquipmentModL1 ModType1;
		public EquipmentModL2 ModType2;

		public ModData(IMod data, ulong uid, int pos) : base(data, uid, pos) { }
	}

	public class DataBase<T>
	{
		public T Data;
		public ulong ContainerUID;
		public int ContainerPOS;

		public DataBase(T data, ulong uid, int pos)
		{
			Data = data;
			ContainerUID = uid;
			ContainerPOS = pos;
		}
	}
}

public class Msg3DViewerInfo
{
	public Model Model;
	public bool IsShip;
    public Vector3 position;
    public Vector2 size;
}

/// <summary>
/// 打开通讯窗参数
/// </summary>
public struct PlayParameter
{
    /// <summary>
    /// 组Id
    /// </summary>
    public int groupId;
    /// <summary>
    /// 回调
    /// </summary>
    public Action action;
    /// <summary>
    /// 怪物名字
    /// </summary>
    public uint npcId;
}


#region ------------ 音效相关参数结构---------

/// <summary>
/// 加载音效组合
/// </summary>
public class MsgLoadSoundCombo
{
	/// <summary>
	/// 组合ID
	/// </summary>
	public int SoundComboId;
	/// <summary>
	/// 是否是需要执行 PrepareEvent
	/// </summary>
	public bool todoPrepareEvent;
}

/// <summary>
/// 播放特殊枚举标注的音效
/// </summary>
public class MsgPlaySpecialTypeMusicOrSound
{
	public int ComboId;                                                                                                                //组合ID
	public WwiseMusicSpecialType type;                                                                                 //类型
	public WwiseMusicPalce palce = WwiseMusicPalce.Palce_1st;                                    //第几人称
	public bool alreadyPrepare;                                                                                                 //是否已经 Prepare 加载了
	public bool UseSoundParent;                                                                                              //使用SoundParent 话，标记以下 好对Null 作处理
	public Transform SoundParent;                                                                                           //挂点
	public Vector3 point;                                                                                                             //位置
	public Action<SystemObject> endAction = null;                                                               //非循环音效，播放完毕回调（需要的赋值就可以了）
	public SystemObject userEndData = null;                                                                          //回调参数
}

/// <summary>
/// 播放音效
/// </summary>
public class MsgPlayMusicOrSound
{
	public int musicId;                                                                                                                  //音效ID
	public bool alreadyPrepare;                                                                                                 //是否已经 Prepare 加载了
	public bool UseSoundParent;                                                                                              //使用SoundParent 话，标记以下 好对Null 作处理
	public Transform SoundParent;                                                                                           //挂点
	public Vector3 point;                                                                                                             //位置
	public Action<SystemObject> endAction = null;                                                                             //非循环音效，播放完毕回调（需要的赋值就可以了）
	public SystemObject userEndData = null;                                                                          //回调参数
}

/// <summary>
/// toggle越界声效
/// </summary>
public class MsgPlayMusicOrSound_outLine
{
	public GameObject OldSelectionObj;                      //越界前选择的对象
}


#endregion ------ 音效相关参数结构---end---



public class MsgInteractiveInfo
{
	public uint Tid;
	public string Describe;
	public bool MustUseHumanFBox;
}

public class MsgInteractiveTip
{
	public uint TemplateID;
}

public class MsgEntityInfo
{
	public uint Uid;
	public uint Tid;
	public bool IsMain;
}

public class MsgPlayerWeaponShot
{
	public Vector2 screenPoint;
}

public class MsgChangeCrosshairType
{
	public CrosshairType Type;
}

public class MsgPlayerWeaponCrosshairOffset
{
	/// <summary>
	/// 水平方向屏幕占比
	/// </summary>
	public float HorizontalRelativeHeight;
	/// <summary>
	/// 竖直方向屏幕占比
	/// </summary>
	public float VerticalRelativeHeight;
}

public class MsgChildShowTipsInfo
{
    public object TipData;
}

public class MsgShipDataChanged
{
	public enum Type
	{
		Add, Remove
	}
	public ulong ShipUid;
	public Type ChangeType;
	public Category ItemType;
	public ulong ContainerUid;
	public ulong ItemUid;
}

public class MsgShipDataUpdata
{
	public enum Type
	{
		Add, Remove, Change
	}
	public Category Category;
}

public class MessageSingleton
{

	private static Dictionary<Type, object> m_ObjectCache = new Dictionary<Type, object>();

	public static T Get<T>()
	{
		Type objectType = typeof(T);
		object objectInstance = null;
		if (!m_ObjectCache.TryGetValue(objectType, out objectInstance))
		{
			objectInstance = Activator.CreateInstance(objectType);
			m_ObjectCache.Add(objectType, objectInstance);
		}
		return (T)objectInstance;
	}

}

public class SkillStartPeriodNotify
{
	public SkillPeriodType PeriodType;
	public int SkillID;
	/// <summary>
	/// 秒
	/// </summary>
	public float Duration;
	/// <summary>
	/// 开始的时间
	/// </summary>
	public float BeginTime;
}

public class SkillFinishPeriodNotify
{
	public bool Success;
}

public class ChangeBattleStateNotify
{
    public bool IsSelf;
    public EnumMainState NewMainState;
    public EnumMainState OldMainState;

    public HeroState NewState;
    public HeroState OldStata;
}

public class ActivateOverloadNotify
{
	public bool Active;
}

public class ActivateBurstNotify
{
	public bool Active;
}

public class SkillHurtInfo
{
	/// <summary>
	/// 目标ID
	/// </summary>
	public uint TargetID;
	/// <summary>
	/// 伤害量
	/// </summary>
	public int Damage;
	/// <summary>
	/// 穿透伤害量
	/// </summary>
	public int PenetrationDamage;
	/// <summary>
	/// 是否暴击
	/// </summary>
	public bool IsCrit;
	/// <summary>
	/// 是否闪避
	/// </summary>
	public bool IsDodge;
	/// <summary>
	/// 效果ID
	/// </summary>
	public int EffectID;
	/// <summary>
	/// 是不是弱点攻击 
	/// </summary>
	public bool IsWeak;
}
public class BuffHurtInfo
{
	/// <summary>
	/// 目标ID
	/// </summary>
	public uint targetID;
	/// <summary>
	/// 效果类型
	/// </summary>
	public EffectType type;
	/// <summary>
	/// 效果值
	/// </summary>
	public int value;
}
public class HurtImmuno
{
	/// <summary>
	/// 目标ID
	/// </summary>
	public uint targetID;
	/// <summary>
	/// 类型
	/// </summary>
	public int type;
	/// <summary>
	/// 值
	/// </summary>
	public int value;
}

public class LoadingPanelParamere
{
	/// <summary>
	/// 显示完成回调
	/// </summary>
	public Action OnShown;
}

public class DialogueInfo
{
	public uint DialogueTid;

	public Vector3? SoundPoint;

	public Transform SoundParent;

	public uint NpcTid;

	public bool NeedSendToServer;
}

public class ShowRelviePanelNotify
{
	/// <summary>
	/// 是否有空间站复活
	/// </summary>
	public bool IsShowHallRelive;
	/// <summary>
	/// 击杀者名字
	/// </summary>
	public string KillerName;
	/// <summary>
	/// 倒计时时间
	/// </summary>
	public uint Countdown;
}

public class MsgPlayerInfo
{
	/// <summary>
	/// 角色Tid
	/// </summary>
	public int Tid;
	/// <summary>
	/// 玩家名称
	/// </summary>
	public string Name;
	/// <summary>
	/// 战舰
	/// </summary>
	public IShip Ship;

	/// <summary>
	///UID
	/// </summary>
	public ulong UID;

}

public class MsgPlayerWeaponCrosshair
{
	/// <summary>
	/// 水平方向屏幕占比
	/// </summary>
	public float HorizontalRelativeHeight;
	/// <summary>
	/// 竖直方向屏幕占比
	/// </summary>
	public float VerticalRelativeHeight;


    // 导弹
    /// <summary>
    /// 导弹的锁定一层的时间
    /// </summary>
    public float MissileLockTime = 0;
    /// <summary>
    /// 一次发射中的导弹数量上限
    /// </summary>
    public int MissileCountInOneShot;
}

/// <summary>
/// 散弹枪，区域情况通知UI
/// </summary>
public class MsgPlayerWeaponCrosshair_ShotGun
{
    // 霰弹枪
    /// <summary>
    /// 子区域占屏幕比例
    /// </summary>
    public List<Vector2> SubAimAreaRelativeHeight = new List<Vector2>();
    /// <summary>
    /// 子区域屏幕坐标
    /// </summary>
    public List<Vector3> SubAimAreaScreenPosition = new List<Vector3>();

    /// <summary>
    /// 经过这么长时间后, 子区域恢复至最中心的位置
    /// </summary>
    public float RemainingRestoreDuration;
}





public class ItemOperateInfoTemp
{
	public enum OperateType : int
	{
		Add,
		Remove,
		Replace
	}
	public Category Category = 0;
	public OperateType Type = 0;
	public int Pos = 0;
	public ulong UID = 0;
}

public class ItemChangeInfo
{
	public enum Type
	{
		Add, Del, AttrChange, CountChange
	}
	public Category Category = 0;
	public Type ChangeType;
	public ulong UID;
	public uint TID;
	public ulong ParentUID;
	public int ItemPos;
	public long CountChangeDelta;
}

public class EntityDeathInfo
{
	public uint heroID;
	public uint killerID;
}

public class PlayerMissileWeaponTargetSelectionInfo
{
	public Dictionary<ulong, WeaponAndCrossSight_Missile.TargetLockInfo> TargeList;
}

public class WeaponSelectedTargetInfo
{
	public SpacecraftEntity selectedTarget;
}

public class TaskVoiceInfo
{
	public int audioID;
	public string text;
	public Transform soundParent;
	public WwiseMusicPalce place;   //播放的是语音才需要设置这个
	public bool isVoice;            //true 语音 false 音效
	public bool isReplaceOther;     //true 顶替掉前一个
	public Action CallbackAction = null;
}

public class MsgStarmapPanelState
{
	public object Data;
	public ulong BeforeID;
}

public class MsgMissionInfo
{
	/// <summary>
	/// 任务表id
	/// </summary>
	public uint MissionTid { get; }
	/// <summary>
	/// 任务uid
	/// </summary>
	public ulong MissionUid { get; }

	public MsgMissionInfo(uint tid, ulong uid)
	{
		MissionTid = tid;
		MissionUid = uid;
	}
}

public class MissionOpenMsgInfo
{
	/// <summary>
	/// 任务数据
	/// </summary>
	public MissionVO MissionVO;
	/// <summary>
	///npc transform
	/// </summary>
	public Transform NpcTransform;
}

public class MsgProduceConfim
{
    /// <summary>
    /// 生产指令
    /// </summary>
    public ProduceOrder OrderType;
    /// <summary>
    /// Tid
    /// </summary>
    public int Tid;
    /// <summary>
    /// 消耗数量
    /// </summary>
    public long ExpendNum;

}
public class MsgOpenProduce
{
    /// <summary>
    /// 入口类型
    /// </summary>
    public ProduceDialogType MProduceDialogType;

    /// <summary>
	/// 当前生产类型
	/// </summary>
	public ProduceType CurrentProduceType;
}
public class MsgDetectorShow
{
    /// <summary>
    /// 是否显示
    /// </summary>
    public bool Show;

    /// <summary>
    /// 高度
    /// </summary>
    public float Height;

    /// <summary>
    /// 最大高度
    /// </summary>
    public float MaxHeight;
}

public class MsgTalentOperation
{
    /// <summary>
    /// 操作指令
    /// </summary>
    public TalentCode M_TalentCode;
    /// <summary>
    /// 操作的tid
    /// </summary>
    public uint Tid;
}

#region ================================-技能==================================

/// <summary>
/// 显示技能时间型进度条
/// </summary>
public class MsgShowSkillTimeProgressBar
{
    public int StyleIndex;                                                          //进度条样式索引
    public float Duration;                                                         //进度条最大维持时间
}

/// <summary>
/// 隐藏技能时间型进度条
/// </summary>
public class MsgHideSkillTimeProgressBar
{
    public int StyleIndex;                                                          //进度条样式索引
}

#endregion ==============================end==-技能============================


