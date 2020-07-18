using Eternity.FlatBuffer;
using Eternity.FlatBuffer.Enums;
using PureMVC.Patterns.Proxy;
using System.Collections.Generic;
using UnityEngine.Assertions;

public partial class CfgEternityProxy : Proxy
{

    /// <summary>
    /// 获取player总数据长度
    /// </summary>
    /// <returns></returns>
    public int GetPlayerDataLength()
    {
        return m_Config.PlayersLength;
    }

    /// <summary>
    /// 获取人形态数据
    /// </summary>
    /// <param name="tid"></param>
    /// <returns></returns>
    public Player? GetPlayerByKey(uint tid)
    {
        Player? player = m_Config.PlayersByKey(tid);
        Assert.IsTrue(player.HasValue, "CfgEternityProxy => GetEquipByKey not exist tid " + tid);
        return player;
    }

    /// <summary>
    /// 根据数据表index获取player信息
    /// </summary>
    /// <param name="index">index</param>
    /// <returns></returns>
    public Player? GetPlayerByIndex(int index)
    {
        return m_Config.Players(index);
    }

	/// <summary>
	/// 女孩子们
	/// </summary>
	private List<Player> m_FemalePlayers;

	/// <summary>
	/// 男孩子们
	/// </summary>
	private List<Player> m_MalePlayers;
	
	/// <summary>
	/// 根据mapid动态加载npc列表
	/// </summary>
	public void CheckData()
	{
		if (m_FemalePlayers == null && m_MalePlayers == null)
		{
			m_FemalePlayers = new List<Player>();
			m_MalePlayers = new List<Player>();
			int PlayerTmpLength = GetPlayerDataLength();
			for (int i = 0; i < PlayerTmpLength; i++)
			{
				Player vo = GetPlayerByIndex(i).Value;
				if (vo.Gender == (uint)Gender.Male)
				{
					m_MalePlayers.Add(vo);
				}
				else
				{
					m_FemalePlayers.Add(vo);
				}
			}
		}
	}

	/// <summary>
	/// 获取男孩子通过index
	/// </summary>
	/// <param name="index">index</param>
	/// <returns></returns>
	public Player GetMalePlayerByIndex(int index)
	{
		return m_MalePlayers[index];
	}

	/// <summary>
	/// 获取男孩子们个数
	/// </summary>
	public int GetMalePlayerLength()
	{
		return m_MalePlayers.Count;
	}

	/// <summary>
	/// 获取女孩子通过index
	/// </summary>
	/// <param name="index">index</param>
	/// <returns></returns>
	public Player GetFamalePlayerByIndex(int index)
	{
		return m_FemalePlayers[index];
	}

	/// <summary>
	/// 获取女孩子个数
	/// </summary>
	public int GetFamalePlayerLength()
	{
		return m_FemalePlayers.Count;
	}

    /// <summary>
    ///  获取Player
    /// </summary>
    /// <param name="itemTId">道具tId</param>
    /// <returns></returns>
    public Player? GetPlayerByItemTId(int itemTId)
    {
		int length = GetPlayerDataLength();
        for (int i = 0; i < length; i++)
        {
            Player vo = GetPlayerByIndex(i).Value;
            if (vo.Id == itemTId)
            {
                return vo;
            }
        }
		UnityEngine.Debug.Log("没有找到"+itemTId);
		return null;
    }

	/// <summary>
	/// 通过tid获取性别
	/// </summary>
	/// <param name="tid">tid</param>
	/// <returns></returns>
	public int GetGenderByTid(int tid)
	{
		return (int)GetPlayerByKey((uint)tid).Value.Gender;
	}

}