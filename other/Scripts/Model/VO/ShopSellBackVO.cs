using Eternity.FlatBuffer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///商店回购
/// </summary>
public class ShopSellBackVO 
{
    /// <summary>
    /// 商店Id
    /// </summary>
    public uint ShopId;
    /// <summary>
    /// 物品Uid
    /// </summary>
    public ulong Uid;
    /// <summary>
    /// 物品Tid
    /// </summary>
    public ulong Tid;
    /// <summary>
    /// 物品数量
    /// </summary>
    public uint Num;
    /// <summary>
    /// 到期时间
    /// </summary>
    public ulong ExpireTime;
    /// <summary>
    /// 表数据
    /// </summary>
    public Item ItemConfig;
}
