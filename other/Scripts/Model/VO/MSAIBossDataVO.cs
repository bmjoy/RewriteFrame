using System.Collections.Generic;

public enum PlayerAIState
{
    ForeWarnState = 0x10000,
    InBattleState = 0x100000,
    DyingState = 0x110000,
    DeathState = 0x1000000,
    OutOfSceneState = 0x1010000,
    EscapeFromAI = 0x1100000
}

public enum AICombinnationState
{
    InBattleAndAIIdel = 0x100001,
    InBattleAndAIBattle = 0x100010,
    InBattleAndAILostPl = 0x100011,
    InBattleAndAIMRetre = 0x100100,
    InBattleAndAIMRFail = 0x100101,
    InBattleAndAIBCBoss = 0x100110,
    InBattleAndAICBossS = 0x100111,
    InBattleAndAICBossF = 0x101000,
    InBattleAndAIBBossT = 0x101001,
    InBattleAndAIBossTO = 0x101010,
    InBattleAndAIDfBoss = 0x101011,
    InBattleAndPOutofSc =  0x101100,

    DyingAndAIIdel = 0x110001,
    DyingAndAIBattle = 0x110010,
    DyingAndAILostPl = 0x110011,
    DyingAndAIMRetre = 0x110100,
    DyingAndAIMRFail = 0x110101,
    DyingAndAIBCBoss = 0x110110,
    DyingAndAICBossS = 0x110111,
    DyingAndAICBossF = 0x111000,
    DyingAndAIBBossT = 0x111001,
    DyingAndAIBossTO = 0x111010,
    DyingAndAIDfBoss = 0x111011,
    DyingAndPOutofSc = 0x111100,

    DeathAndAIIdel = 0x1000001,
    DeathAndAIBattle = 0x1000010,
    DeathAndAILostPl = 0x1000011,
    DeathAndAIMRetre = 0x1000100,
    DeathAndAIMRFail = 0x1000101,
    DeathAndAIBCBoss = 0x1000110,
    DeathAndAICBossS = 0x1000111,
    DeathAndAICBossF = 0x1001000,
    DeathAndAIBBossT = 0x1001001,
    DeathAndAIBossTO = 0x1001010,
    DeathAndAIDfBoss = 0x1001011,
    DeathAndPOutofSc = 0x1001100,


    OutOfSAndAIIdel = 0x1010001,
    OutOfSAndAIBattle = 0x1010010,
    OutOfSAndAILostPl = 0x1010011,
    OutOfSAndAIMRetre = 0x1010100,
    OutOfSAndAIMRFail = 0x1010101,
    OutOfSAndAIBCBoss = 0x1010110,
    OutOfSAndAICBossS = 0x1010111,
    OutOfSAndAICBossF = 0x1011000,
    OutOfSAndAIBBossT = 0x1011001,
    OutOfSAndAIBossTO = 0x1011010,
    OutOfSAndAIDfBoss = 0x1011011,
    OutOfSAndPOutofSc = 0x1011100,
}


public class MonsterList
{
    public List<ulong> m_MonsterList;
}

public class MSG_MSAIBoss_Info
{
    /// <summary>
    /// 战场状态
    /// </summary>
    public Assets.Scripts.Define.AIPlotState aiState;
    /// <summary>
    /// 玩家状态
    /// </summary>
    public PlayerAIState playerState;
}

public class MSG_TimeInfo
{
    /// <summary>
    /// 时间
    /// </summary>
    public uint time;
    /// <summary>
    /// 时间到了的回掉
    /// </summary>
    public System.Action CallbackAction = null;
}

public class AI_Distance_Info
{
    public int times;   //次数
    public ulong serverTime;  //上一次播放的时间
    public bool isStayInAI;   //是否一直在靠近AI区域内
    public AI_Distance_Info(){}
    public AI_Distance_Info(int times, ulong time, bool isStay)
    {
        this.times = times;
        this.serverTime = time;
        this.isStayInAI = isStay;
    }
}

public class MSG_WarningHUDInfo
{
	/// <summary>
	/// languageId
	/// </summary>
	public string languageId;
	/// <summary>
	/// 时间
	/// </summary>
	public float time = 0;
}