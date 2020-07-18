
namespace Assets.Scripts.Define
{
    public enum E_GamePlatform
    {
        egpDefault = 0,//默认,自动识别.
        egpTX = 1,//腾讯平台.
        egpOther = 2,//其它.
    }

    public enum E_TitleType
    {
        ZhiZhun = 1,
        MingRen = 2,
        TeXv    = 3,
    }

    public enum E_ItemTitle
    {
        None         = 0,
        OutOfPrint   = 1, //绝版..
        NotSell      = 2, //非卖..
    }

    public enum E_ShuiJingBaoKuCostType
    {
        HonorPoints = 1,
        Gold        = 2,
    }

    public enum E_ExamResult
    {
        erNULL = 0, //没有选择答案...
        erRight = 1,//正确答案...
        erError = 2,//错误答案...
    }

    public enum E_AchievementType
    {
        atZongLan = 0,  //成就总览...
        atJueSe = 1,    //角色成就...
        atYueLi = 2,    //阅历...
        atXiuXing = 3,  //修行...
        atTiaoZhan = 4, //挑战...
        atSheHui = 5,   //社会...
        atRongYao = 6,  //荣耀...
    }

    public enum E_InspireType
    {
	    itLife = 1,   //生命鼓舞...
	    itAttack = 2, //攻击鼓舞...
	    itCount = 2,  //鼓舞总数量...
    }
    public enum E_PVEInspireType
    {
        itYinbi = 1,   //银币鼓舞...
        itGold = 2, //元宝鼓舞...
        itCount = 2,  //鼓舞总数量...
    }
    //对话类型
    public enum E_NarratorType
    {
        ntDialogLeft = 1,//对画框...
        ntDialogRight = 2,//
        ntText = 3,//插画文字...
        ntCaption = 4,//字幕...
        ntPopScreen = 5,//弹幕...
        ntDialogCenterBottom = 6
    }

    //法宝开启条件
    public enum KMagicWeaponOpenType
    {
        mwotDefault = 1, //默认开启
        mwotActivityBuy = 2,//活动购买
        mwotMaterial = 3,//材料合成
    }

    public enum KCommonItemTabType
    {
        cittDefault = 0,
        cittEquip = 1,//装备
        cittProp = 2,//道具
        cittConsumables = 3,//消耗品
    }

    public enum KFunStepOperateType
    {
        fstInvalide = 0,
        fstUpHorse = 1,//上坐骑
        fstWearArtifact = 2,//穿戴神兵
        fstWearWing = 3,//穿戴翅膀
    }

    public enum KBuffShowType //Buff显示位置..
    {
        bstHeroMenuView = 1,
        bstButtonBarView = 2,
        bstHide = 3,
    }

    public enum KFriendType
    {
        ftInvalide = 0,
        ftFriend = 1,//好友关系
        ftEnemy = 2, // 仇人关系
        ftBlacklist = 3,//黑名单
        ftKiller = 4, //杀手(不属于好友关系，纯粹前端用来显示区分)
        ftRecentContact = 5,//最近联系人(不属于好友关系，纯粹前端用来显示区分)
    }

    //指引触发的类型
    public enum KGuideUIEventType
    {
        OnClick = 0,//点击
        OnHover = 1,//
        OnKeyDown = 2,// 按键
    }

    //互斥的UI类型
    public enum KViewMutexType
    {
        vmtYunBiao = 1,//运镖
        guildYunBiao = 2,
    }

    //运镖类型
    public enum YunBiaoType
    {
        YBT_PERSONAL = 0, //个人运镖
        YBT_GUILD = 1 //军团运镖
    }

    public enum DateTimeState
    {
        END = 0,
        START = 1,
    }
}