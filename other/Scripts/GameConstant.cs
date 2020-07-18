/*===============================
 * Purpose: 游戏全局常量脚本
 * Time: 2019/4/16 12:06:59
================================*/


using System.Collections.Generic;
/// <summary>
/// 游戏全局常量
/// </summary>
public class GameConstant
{
	public static class CurrencyConst
	{
		public const int GAME_CURRENCY_ITEM_TID = 1100004;
		public const int RECHARGE_CURRENCY_ITEM_TID = 1000001;
        public const int TALENTCURRENCY = 1100101;
    }

	/// <summary>
	/// Unity的单位对应的米数
	/// </summary>
	public const float METRE_PER_UNIT = 1000.0f;

	/// <summary>
	/// 船形默认可见距离（单位：米）
	/// </summary>
	public const float DEFAULT_VISIBILITY_METRE_FOR_SHIP = 15000.0f;

	/// <summary>
	/// 人形默认可见距离（单位：米）
	/// </summary>
	public const float DEFAULT_VISIBILITY_METRE_FOR_HUMAN = 12;


	/// <summary>
	/// LayerType
	/// </summary>
	public static class LayerTypeID
	{
		/// <summary>
		/// 默认层级. 当做场景中的静态物体
		/// 本来跟美术约定了场景中静态物体一定得是SceneOnly, 但是因为历史原因(很多场景中有一大堆Default层级的东西, 实在改不过来), 就把Default也当做场景静态物体了
		/// </summary>
		public const int Default = 0;
		/// <summary>
		/// 特效
		/// </summary>
		public const int TransparentFX = 1;
		/// <summary>
		/// 主角的人物(室内) / 飞船(深空)
		/// </summary>
		public const int MainPlayer = 8;
		/// <summary>
		/// 技能投射物
		/// </summary>
		public const int SkillProjectile = 9;
		/// <summary>
		/// 室内NPC
		/// </summary>
		public const int HumanNPC = 10;
		/// <summary>
		/// 深空Npc的飞船. 包括敌人和可交互NPC
		/// </summary>
		public const int SpacecraftNPC = 11;
		/// <summary>
		/// 深空里其他玩家的飞船.
		/// </summary>
		public const int SpacecraftOtherPlayer = 12;
		/// <summary>
		/// 场景中的静态物体
		/// </summary>
		public const int SceneOnly = 15;
		/// <summary>
		/// 室内其他玩家的人物
		/// </summary>
		public const int HumanOtherPlayer = 16;
		/// <summary>
		/// 不可选中的飞船. 目前用于隐身飞船
		/// </summary>
		public const int UnselectableSpacecraft = 17;
		/// <summary>
		/// 用于客户端与服务器的碰撞同步
		/// </summary>
		public const int ServerSynchronization = 18;
        /// <summary>
        /// 技能可以穿过战舰不能穿过
        /// </summary>
        public const int SkillCrossSpacecraftBlock = 19;
        /// <summary>
        /// 不可见的功能性NPC, 比如传送点
        /// </summary>
        public const int InvisibleFunctionalNPC = 23;
	}


	/// <summary>
	/// 预制体后缀
	/// </summary>
	public const string ICON_POSTFIX = ".prefab";


	/// <summary>
	/// Icon 预制体存放路径
	/// </summary>
	public const string ICON_ASSETADDRESS_ROOTPATH = "Assets/Artwork/UI/Prefabs/Element/{0}{1}";
	/// <summary>
	/// 是否第一次登陆显示协议
	/// </summary>
	public const string FIRSTLOGIN = "FirstLogin";

	////////////////////////////////////////////////////////////////////////分隔符/////////////////////////////////////////////////////////////////////////////////////////////////////
	#region  图集地址
	//图集名字读表
	//图集名字读表
	//图集名字读表
	/// <summary>
	/// 热键Icon图集资源地址
	/// </summary>
	public const string ICON_ATLAS_ASSETADDRESS = "HotkeyAtlas";

	/// <summary>
	/// 普通Icon图集资源地址
	/// </summary>
	public const string COMMON_ICON_ATLAS_ASSETADDRESS = "IconAtlas";

	/// <summary>
	/// 功能类Icon图集资源地址
	/// </summary>
	public const string FUNCTION_ICON_ATLAS_ASSETADDRESS = "FunctionIconAtlas";

	/// <summary>
	/// 公共图片图集资源地址
	/// </summary>
	public const string COMMON_ATLAS_ASSETADDRESS = "CommonAtlas";

	/// <summary>
	/// 技能图标图集资源地址
	/// </summary>
	public const string ATLAS_SKILL = "IconSkillAtlas";

	#endregion
	////////////////////////////////////////////////////////////////////////分隔符/////////////////////////////////////////////////////////////////////////////////////////////////////

	public static class ItemConst
	{
		/// <summary>
		///通用排序规则：
		///品质降序＞T级升序＞强化等级降序＞使用等级降序＞名称A-Z＞出售价格降序＞获得时间降序＞order降序＞ID升序
		///如果道具缺少这些字段，比如芯片没有T级，则这类道具排序时略过这条规则；
		///
		///切换一套排序规则即将某一个排序指标提到最高；
		///目前有7套排序规则，按顺序切换：
		///1、  Sorted by: Default
		///2、  Sorted by: Tonnage
		///3、  Sorted by: Enchant Level
		///4、  Sorted by: Role Level
		///5、  Sorted by: Alphabetical
		///6、  Sorted by: Selling Price
		///7、  Sorted by: Newest
		///如果道具缺少这些字段，比如蓝图没有强化等级，则这类道具排序时略过这条规则；
		/// </summary>
		public enum ItemSortType : int
		{
			Default, Tonnage, Enchant, Role, Alphabetical, Selling, Newest,Order, Tid
		}

		/// <summary>
		/// item排序默认优先顺序
		/// </summary>
		public static readonly List<ItemSortType> ItemDefSort = new List<ItemSortType>()
		{
			ItemSortType.Default,
			ItemSortType.Tonnage,
			ItemSortType.Enchant,
			ItemSortType.Role,
			ItemSortType.Alphabetical,
			ItemSortType.Selling,
			ItemSortType.Newest,
            ItemSortType.Order,
			ItemSortType.Tid
		};
        /// <summary>
        /// 船排序默认优先顺序
        /// </summary>
        public static readonly List<ItemSortType> ShipDefSort = new List<ItemSortType>()
        {
            ItemSortType.Tonnage,
            ItemSortType.Enchant,
            ItemSortType.Alphabetical,
            ItemSortType.Newest,
            ItemSortType.Order,
            ItemSortType.Tid
        };
    }

}