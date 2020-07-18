/// <summary>
/// 热键表ID
/// </summary>
public enum HotKeyMapID
{
	/// <summary>
	/// 空
	/// </summary>
	None,
	/// <summary>
	/// 道具
	/// </summary>
	Item,
	/// <summary>
	/// 聊天
	/// </summary>
	Chat,
	/// <summary>
	/// 手表
	/// </summary>
	Watch,
	/// <summary>
	/// 人形态
	/// </summary>
	HUMAN,
	/// <summary>
	/// 船形态
	/// </summary>
	SHIP,
	/// <summary>
	/// 跃迁
	/// </summary>
	LEAP,
	/// <summary>
	/// UI
	/// </summary>
	UI
}

/// <summary>
/// 热键ID
/// </summary>
public class HotKeyID
{
	#region UGUI

	/// <summary>
	/// 主摇杆
	/// </summary>
	public const string UGUI_Stick1 = "UGUI_Stick1";
	/// <summary>
	/// 副摇杆
	/// </summary>
	public const string UGUI_Stick2 = "UGUI_Stick2";
	/// <summary>
	/// 主按钮
	/// </summary>
	public const string UGUI_Button1 = "UGUI_Button1";
	/// <summary>
	/// 副按钮
	/// </summary>
	public const string UGUI_Button2 = "UGUI_Button2";
	/// <summary>
	/// 横轴
	/// </summary>
	public const string UGUI_Horizontal = "UGUI_Horizontal";
	/// <summary>
	/// 横轴
	/// </summary>
	public const string UGUI_Vertical = "UGUI_Vertical";
	/// <summary>
	/// 确认
	/// </summary>
	public const string UGUI_Submit = "UGUI_Submit";
	/// <summary>
	/// 取消
	/// </summary>
	public const string UGUI_Cancel = "UGUI_Cancel";

	#endregion

	#region HUD手表模式

	/// <summary>
	/// 手表打开
	/// </summary>
	public const string WatchOpen = "WatchOpen";
	/// <summary>
	/// 手表关闭
	/// </summary>
	public const string WatchClose = "WatchClose";
	/// <summary>
	/// 手表轴
	/// </summary>
	public const string WatchAxis = "WatchAxis";
	/// <summary>
	/// 手表方向
	/// </summary>
	public const string WatchDirection = "WatchDirection";

	#endregion

	#region HUD聊天模式

	/// <summary>
	/// 聊天打开
	/// </summary>
	public const string ChatOpen = "ChatOpen";
	/// <summary>
	/// 聊天关闭
	/// </summary>
	public const string ChatClose = "ChatClose";
	/// <summary>
	/// 聊天历史
	/// </summary>
	public const string ChatHistory = "ChatHistory";

	#endregion

	#region HUD道具模式

	/// <summary>
	/// 道具模式打开
	/// </summary>
	public const string ItemOpen = "ItemOpen";
	/// <summary>
	/// 道具模式关闭
	/// </summary>
	public const string ItemClose = "ItemClose";
	/// <summary>
	/// 道具模式轴
	/// </summary>
	public const string ItemAxis = "ItemAxis";

	#endregion

	#region UI模式

	/// <summary>
	/// enter键
	/// </summary>
	public const string FuncA = "FuncA";

	/// <summary>
	/// esc键
	/// </summary>
	public const string FuncB = "FuncB";

	/// <summary>
	/// 空格键
	/// </summary>
	public const string FuncX = "FuncX";

	/// <summary>
	/// F键
	/// </summary>
	public const string FuncY = "FuncY";

	/// <summary>
	/// tab键
	/// </summary>
	public const string FuncLT = "FuncLT";

	/// <summary>
	/// X键
	/// </summary>
	public const string FuncRT = "FuncRT";

	/// <summary>
	/// 上键
	/// </summary>
	public const string NavUp = "NavUp";

	/// <summary>
	/// 下键
	/// </summary>
	public const string NavDown = "NavDown";

	/// <summary>
	/// 左键
	/// </summary>
	public const string NavLeft = "NavLeft";

	/// <summary>
	/// 右键
	/// </summary>
	public const string NavRight = "NavRight";

	/// <summary>
	/// Q键
	/// </summary>
	public const string NavNegative = "NavNegative";

	/// <summary>
	/// E键
	/// </summary>
	public const string NavPositive = "NavPositive";

	/// <summary>
	/// 左摇杆按下
	/// </summary>
	public const string FuncL3 = "FuncL3";

	/// <summary>
	/// V键(右摇杆按下)
	/// </summary>
	public const string FuncR3 = "FuncR3";

    /// <summary>
    /// Esc界面关闭
    /// </summary>
    public const string FuncEscForUI = "EscForUI";

    /// <summary>
    /// 单独的enter
    /// </summary>
    public const string FuncEnter = "EnterForUI";
    #endregion

    #region 人形态

    /// <summary>
    /// 人形态ESC
    /// </summary>
    public const string EscForHuman = "EscForHuman";
	/// <summary>
	/// 人形态交互
	/// </summary>
	public const string InteractiveForHuman = "InteractiveForHuman";
	/// <summary>
	/// 人形态摇杆移动
	/// </summary>
	public const string HumanMoveStick = "HumanMoveStick";
	/// <summary>
	/// 人形态键盘移动
	/// </summary>
	public const string HumanMoveAxis = "HumanMoveAxis";
	/// <summary>
	/// 人形态操作相机
	/// </summary>
	public const string HumanCamera = "HumanCamera";

	#endregion

	#region 船形态

	/// <summary>
	/// 船形态ESC
	/// </summary>
	public const string EscForShip = "EscForShip";
	/// <summary>
	/// 船形态交互
	/// </summary>
	public const string InteractiveForShip = "InteractiveForShip";

    /// <summary>
    /// 跃迁开始
    /// </summary>
    public const string LeapStart = "LeapStart";
    /// <summary>
    /// 跃迁停止
    /// </summary>
    public const string LeapStop = "LeapStop";

	/// <summary>
	/// 飞船移动 (手柄左摇杆, WASD)
	/// </summary>
	public const string ShipMove = "ShipMove";
	/// <summary>
	/// 飞船上升
	/// </summary>
	public const string ShipAscend = "ShipAscend";
	/// <summary>
	/// 飞船下降
	/// </summary>
	public const string ShipDescend = "ShipDescend";
	/// <summary>
	/// 飞船自动驾驶
	/// </summary>
	public const string ShipAutoMove = "ShipAutoMove";
	/// <summary>
	/// 6dof加速
	/// </summary>
	public const string ShipSpeedUp = "ShipSpeedUp";
	/// <summary>
	/// 飞船相机
	/// </summary>
	public const string ShipCamera = "ShipCamera";
	/// <summary>
	/// <summary>
	/// 飞船切换模式(巡航/战斗)
	/// </summary>
	public const string ShipSwitchMode = "ShipSwitchMode";
	/// <summary>
	/// 飞船过载
	/// </summary>
	public const string ShipOverload = "ShipOverload";
	/// <summary>
	/// 技能按键1
	/// </summary>
	public const string ShipSkill1 = "ShipSkill1";
	/// <summary>
	/// 技能按键2
	/// </summary>
	public const string ShipSkill2 = "ShipSkill2";
	/// <summary>
	/// 技能按键3
	/// </summary>
	public const string ShipSkill3 = "ShipSkill3";
	/// <summary>
	/// 技能按键4
	/// </summary>
	public const string ShipSkill4 = "ShipSkill4";

    /// <summary>
    /// 技能按键5
    /// </summary>
    public const string ShipSkill5 = "ShipSkill5";

    /// <summary>
    /// 技能按键6
    /// </summary>
    public const string ShipSkill6 = "ShipSkill6";

    /// <summary>
    /// 技能按键7
    /// </summary>
    public const string ShipSkill7 = "ShipSkill7";



    /// <summary>
    /// Burst准备
    /// </summary>
    public const string ShipReadyBurst = "ShipReadyBurst";
	/// <summary>
	/// 武器开火
	/// </summary>
	public const string WeaponFire = "WeaponFire";
	/// <summary>
	/// 武器装弹
	/// </summary>
	public const string WeaponReload = "WeaponReload";
	///  自动瞄准
	/// </summary>
	public const string WeaponAutoAim = "WeaponAutoAim";
	/// <summary>
	/// 改变当前武器InputAsset把Control类型设为Axis会报错, 所以弄两个, 一左一右
	/// </summary>
	public const string WeaponToggleLeft = "WeaponToggleLeft";
	/// <summary>
	/// InputAsset把Control类型设为Axis会报错, 所以弄两个, 一左一右
	/// </summary>
	public const string WeaponToggleRight = "WeaponToggleRight";

    /// <summary>
    /// 开宝箱上键
    /// </summary>
    public const string OpenBoxUp = "OpenBoxUp";
    /// <summary>
    /// 开宝箱下键
    /// </summary>
    public const string OpenBoxDown = "OpenBoxDown";
    /// <summary>
    /// 开宝箱左键
    /// </summary>
    public const string OpenBoxLeft = "OpenBoxLeft";
    /// <summary>
    /// 开宝箱右键
    /// </summary>
    public const string OpenBoxRight = "OpenBoxRight";

    #endregion

    #region 跃迁形态

    /// <summary>
    /// 跃迁状态相机
    /// </summary>
    public const string LeapCamera = "LeapCamera";

	#endregion

	#region 星图
	public const string StarmapOpen = "StarmapOpen";

	#endregion

	#region 测试用

	/// <summary>
	/// Debug
	/// </summary>
	public const string DebugKey = "DebugKey";

	/// <summary>
	/// 鼠标右键
	/// </summary>
	public const string MouseRight = "MouseRight";

	#endregion
}
