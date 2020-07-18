using System;

public class HeroState
{
    /// <summary>
    /// 基础状态最多占位
    /// </summary>
    private const ushort BaseStateBit = 4;

    /// <summary>
    /// 状态总位数
    /// </summary>
    private const ushort StateTotalBit = sizeof(ulong) * 8;

    /// <summary>
    /// 子状态最多占位
    /// </summary>
    private const ushort SubStateBit = StateTotalBit - BaseStateBit;

    /// <summary>
    /// 服务器子状态占位
    /// </summary>
    private const ushort ServerSubStateBit = 49;

    /// <summary>
    /// 客户端子状态占位
    /// </summary>
    private const ushort ClientSubStateBit = SubStateBit - ServerSubStateBit;

    /// <summary>
    /// 过滤客户端特有子状态
    /// </summary>
    private const ulong OnlyServerStateMask = (0xFFFFFFFFFFFFFFFF >> (StateTotalBit - ServerSubStateBit - 1)) | 0xFUL << (StateTotalBit - BaseStateBit);

    /// <summary>
    /// 角色状态：高4位基础状态，低60位组合子状态
    /// </summary>
    private ulong m_HeroState;

    public HeroState()
    {

    }

    public HeroState(ulong state)
    {
        SetState(state);
    }

    /// <summary>
    /// 设置状态
    /// </summary>
    public void SetState(ulong state)
    {
        m_HeroState = state;
    }

    /// <summary>
    /// 获取当前状态
    /// </summary>
    /// <returns></returns>
    public ulong GetState()
    {
        return m_HeroState;
    }

    /// <summary>
    /// 设置基础状态
    /// </summary>
    /// <param name="baseState"></param>
    public void SetMainState(EnumMainState baseState)
    {
        ulong state = (ulong)baseState;
        m_HeroState = state << SubStateBit;
    }

    /// <summary>
    /// 设置子状态
    /// </summary>
    /// <param name="subState"></param>
    public void AddSubState(EnumSubState subState)
    {
        int offset = (int)subState;
        if (offset > SubStateBit)
        {
            return;
        }

        ulong state = 1UL << offset;
        m_HeroState |= state;
    }

    /// <summary>
    /// 删除子状态
    /// </summary>
    /// <param name="subState"></param>
    public void RemoveSubState(EnumSubState subState)
    {
        int offset = (int)subState;
        if (offset > SubStateBit)
        {
            return;
        }

        ulong state = 1UL << offset;
        state = ~state;
        m_HeroState &= state;
    }

    /// <summary>
    /// 获取主状态
    /// </summary>
    /// <returns></returns>
    public EnumMainState GetMainState()
    {
        return GetMainState(m_HeroState);
    }

    /// <summary>
    /// 获取主状态
    /// </summary>
    public static EnumMainState GetMainState(ulong state)
    {
        ulong mainState = state >> SubStateBit;

        return (EnumMainState)mainState;
    }

    /// <summary>
    /// 是否包含子状态
    /// </summary>
    /// <param name="subState"></param>
    /// <returns></returns>
    public bool IsHasSubState(EnumSubState subState)
    {
        return IsHasSubState(m_HeroState, subState);
    }

    /// <summary>
    /// 获取清除客户端特有子状态的状态
    /// </summary>
    public ulong GetOnlyServerState()
    {
        return m_HeroState & OnlyServerStateMask;
    }

    /// <summary>
    /// 是否包含子状态
    /// </summary>
    /// <param name="subState"></param>
    /// <returns></returns>
    public static bool IsHasSubState(ulong state, EnumSubState subState)
    {
        state &= 0xFFFFFFFFFFFFFFF;
        int offset = (int)subState;
        if (offset > SubStateBit)
        {
            return false;
        }

        ulong ss = 1UL << offset;

        return (state & ss) != 0;
    }

    /// <summary>
    /// 获取二进制格式字符串
    /// </summary>
    /// <returns></returns>
    public static string GetString(ulong state)
    {
        state &= 0xFFFFFFFFFFFFFFF;
        string str = Convert.ToString((long)state, 2).PadLeft(SubStateBit, '0');
        int index = 0;
        string newStr = "";
        foreach (var item in str)
        {
            newStr += item;
            if (index == 3 && index > 0)
            {
                newStr += " ";
                index = 0;
            }
            else
            {
                index++;
            }
        }

        return newStr;
    }
}
